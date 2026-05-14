using System.Net;

namespace online_school_admin.Services;

public sealed class AuthApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public AuthApiException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
