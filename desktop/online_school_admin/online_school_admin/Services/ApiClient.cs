using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IO;
using System.Text.Json;
using online_school_admin.Infrastructure;

namespace online_school_admin.Services;

public sealed class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private readonly SessionService _session;

    public ApiClient(HttpClient http, SessionService session)
    {
        _http = http;
        _session = session;
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) };
        return await SendAsync<TResponse>(request, cancellationToken);
    }

    public async Task PostAsync<TRequest>(string path, TRequest body, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) };
        await SendNoContentAsync(request, cancellationToken);
    }

    public async Task PostAsync(string path, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        await SendNoContentAsync(request, cancellationToken);
    }

    public async Task<TResponse> PostAsync<TResponse>(string path, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        return await SendAsync<TResponse>(request, cancellationToken);
    }

    public async Task<TResponse> GetAsync<TResponse>(string path, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        return await SendAsync<TResponse>(request, cancellationToken);
    }

    public async Task<TResponse> PostMultipartAsync<TResponse>(
        string path,
        Stream fileStream,
        string formFieldName,
        string fileName,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        await using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms, cancellationToken);
        return await PostMultipartBytesAsync<TResponse>(path, ms.ToArray(), formFieldName, fileName, contentType, cancellationToken);
    }

    public async Task<TResponse> PostMultipartBytesAsync<TResponse>(
        string path,
        byte[] fileBytes,
        string formFieldName,
        string fileName,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        var bytesContent = new ByteArrayContent(fileBytes);
        if (!string.IsNullOrWhiteSpace(contentType))
            bytesContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(bytesContent, formFieldName, fileName);

        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
        return await SendAsync<TResponse>(request, cancellationToken);
    }

    public async Task PutAsync<TRequest>(string path, TRequest body, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, path) { Content = JsonContent.Create(body) };
        await SendNoContentAsync(request, cancellationToken);
    }

    public async Task PatchAsync(string path, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, path);
        await SendNoContentAsync(request, cancellationToken);
    }

    public async Task PatchAsync<TRequest>(string path, TRequest body, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, path) { Content = JsonContent.Create(body) };
        await SendNoContentAsync(request, cancellationToken);
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, path);
        await SendNoContentAsync(request, cancellationToken);
    }

    private bool AttachAuthHeader(HttpRequestMessage request)
    {
        var token = _session.AccessToken;
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return true;
        }
        return false;
    }

    private async Task<(HttpResponseMessage Response, bool HadAuth)> SendInnerAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var hadAuth = AttachAuthHeader(request);
            var resp = await _http.SendAsync(request, cancellationToken);
            return (resp, hadAuth);
        }
        catch (HttpRequestException ex)
        {
            AppLogger.Log(ex, "HTTP");
            throw new ApiException(HttpStatusCode.ServiceUnavailable, ApiErrorFormatter.Format(ex));
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            AppLogger.Log(ex, "Timeout");
            throw new ApiException(HttpStatusCode.ServiceUnavailable, ApiErrorFormatter.Format(ex));
        }
    }

    private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var (response, hadAuth) = await SendInnerAsync(request, cancellationToken);
        using (response)
        {
            return await ReadOrThrowAsync<T>(response, hadAuth, cancellationToken);
        }
    }

    private async Task SendNoContentAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var (response, hadAuth) = await SendInnerAsync(request, cancellationToken);
        using (response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Если запрос был без токена (например, login), показываем сообщение сервера.
                if (hadAuth)
                {
                    _session.Clear();
                    throw new ApiException(response.StatusCode, "Сессия истекла. Войдите заново.");
                }
                throw await ReadErrorAsync(response, cancellationToken);
            }
            if (response.StatusCode == HttpStatusCode.Forbidden)
                throw new ApiException(response.StatusCode, "Недостаточно прав для выполнения операции.");
            if (response.IsSuccessStatusCode)
                return;

            throw await ReadErrorAsync(response, cancellationToken);
        }
    }

    private async Task<T> ReadOrThrowAsync<T>(HttpResponseMessage response, bool hadAuth, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Если запрос был без токена (например, login), показываем сообщение сервера.
            if (hadAuth)
            {
                _session.Clear();
                throw new ApiException(response.StatusCode, "Сессия истекла. Войдите заново.");
            }
            throw await ReadErrorAsync(response, cancellationToken);
        }
        if (response.StatusCode == HttpStatusCode.Forbidden)
            throw new ApiException(response.StatusCode, "Недостаточно прав для выполнения операции.");

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
            if (data == null)
                throw new ApiException(response.StatusCode, "Пустой ответ сервера.");
            return data;
        }
        throw await ReadErrorAsync(response, cancellationToken);
    }

    private static async Task<ApiException> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var raw = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
        var text = raw;
        if (text.StartsWith('"') && text.EndsWith('"') && text.Length >= 2)
            text = text[1..^1].Replace("\\\"", "\"", StringComparison.Ordinal);
        var parsed = ApiErrorFormatter.TryParseServerDetail(raw);
        if (!string.IsNullOrWhiteSpace(parsed))
            text = parsed!;

        if (string.IsNullOrWhiteSpace(text))
            text = response.ReasonPhrase ?? response.StatusCode.ToString();

        return new ApiException(response.StatusCode, text);
    }
}

