using System.Collections.ObjectModel;
using System.Net.Http;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class FaqAdminViewModel : BaseViewModel
{
    private readonly AdminSettingsService _settings;

    public FaqAdminViewModel(AdminSettingsService settings)
    {
        _settings = settings;
        BackCommand = new RelayCommand(_ => BackRequested?.Invoke(), _ => !IsBusy);
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        AddCategoryCommand = new RelayCommand(async _ => await AddCategoryAsync(), _ => !IsBusy);
        DeleteCategoryCommand = new RelayCommand(async _ => await DeleteCategoryAsync(), _ => !IsBusy && SelectedCategory != null);
        SaveItemCommand = new RelayCommand(async _ => await SaveItemAsync(), _ => !IsBusy && SelectedCategory != null);
        DeleteItemCommand = new RelayCommand(async _ => await DeleteItemAsync(), _ => !IsBusy && SelectedItem != null);
        NewItemCommand = new RelayCommand(_ => BeginNewItem(), _ => !IsBusy && SelectedCategory != null);
    }

    public event Action? BackRequested;

    public RelayCommand BackCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand AddCategoryCommand { get; }
    public RelayCommand DeleteCategoryCommand { get; }
    public RelayCommand SaveItemCommand { get; }
    public RelayCommand DeleteItemCommand { get; }
    public RelayCommand NewItemCommand { get; }

    public ObservableCollection<AdminFaqCategoryDto> Categories { get; } = new();

    private AdminFaqCategoryDto? _selectedCategory;
    public AdminFaqCategoryDto? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                Items.Clear();
                SelectedItem = null;
                BeginNewItem();
                if (value?.Items != null)
                {
                    foreach (var i in value.Items)
                        Items.Add(i);
                }
                DeleteCategoryCommand.RaiseCanExecuteChanged();
                SaveItemCommand.RaiseCanExecuteChanged();
                NewItemCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ObservableCollection<AdminFaqItemDto> Items { get; } = new();

    private AdminFaqItemDto? _selectedItem;
    public AdminFaqItemDto? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                if (value != null)
                {
                    _editingFaqId = value.FaqId;
                    ItemQuestion = value.Question;
                    ItemAnswer = value.Answer;
                    ItemOrderText = value.ItemOrder?.ToString() ?? "";
                    ItemIsActive = value.IsActive ?? true;
                }
                else
                {
                    _editingFaqId = null;
                }
                DeleteItemCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string NewCategoryName { get => _newCategoryName; set => SetProperty(ref _newCategoryName, value); }
    private string _newCategoryName = "";

    public string ItemQuestion { get => _itemQuestion; set => SetProperty(ref _itemQuestion, value); }
    private string _itemQuestion = "";

    public string ItemAnswer { get => _itemAnswer; set => SetProperty(ref _itemAnswer, value); }
    private string _itemAnswer = "";

    public string ItemOrderText { get => _itemOrderText; set => SetProperty(ref _itemOrderText, value); }
    private string _itemOrderText = "";

    public bool ItemIsActive { get => _itemIsActive; set => SetProperty(ref _itemIsActive, value); }
    private bool _itemIsActive = true;

    private int? _editingFaqId;

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                BackCommand.RaiseCanExecuteChanged();
                RefreshCommand.RaiseCanExecuteChanged();
                AddCategoryCommand.RaiseCanExecuteChanged();
                DeleteCategoryCommand.RaiseCanExecuteChanged();
                SaveItemCommand.RaiseCanExecuteChanged();
                DeleteItemCommand.RaiseCanExecuteChanged();
                NewItemCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var tree = await _settings.GetFaqTreeAsync(cancellationToken);
            var selCatId = SelectedCategory?.CategoryId;
            var selFaqId = SelectedItem?.FaqId;
            Categories.Clear();
            foreach (var c in tree)
                Categories.Add(c);

            SelectedCategory = selCatId is > 0
                ? Categories.FirstOrDefault(c => c.CategoryId == selCatId)
                : Categories.FirstOrDefault();

            if (selFaqId is > 0)
                SelectedItem = Items.FirstOrDefault(i => i.FaqId == selFaqId);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task AddCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName)) return;
        Error = null;
        IsBusy = true;
        try
        {
            await _settings.CreateFaqCategoryAsync(new AdminFaqCategoryUpsertDto
            {
                CategoryName = NewCategoryName.Trim(),
                CategoryOrder = Categories.Count
            });
            NewCategoryName = "";
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task DeleteCategoryAsync()
    {
        if (SelectedCategory == null) return;
        if (!UserDialogs.Confirm($"Удалить категорию «{SelectedCategory.CategoryName}» и все вопросы в ней?", "FAQ"))
            return;
        Error = null;
        IsBusy = true;
        try
        {
            await _settings.DeleteFaqCategoryAsync(SelectedCategory.CategoryId);
            SelectedCategory = null;
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private void BeginNewItem()
    {
        SelectedItem = null;
        _editingFaqId = null;
        ItemQuestion = "";
        ItemAnswer = "";
        ItemOrderText = "";
        ItemIsActive = true;
    }

    private async Task SaveItemAsync()
    {
        if (SelectedCategory == null) return;
        if (string.IsNullOrWhiteSpace(ItemQuestion) || string.IsNullOrWhiteSpace(ItemAnswer))
        {
            Error = "Укажите вопрос и ответ.";
            return;
        }

        int? ord = null;
        if (!string.IsNullOrWhiteSpace(ItemOrderText) && int.TryParse(ItemOrderText.Trim(), out var o))
            ord = o;

        Error = null;
        IsBusy = true;
        try
        {
            var dto = new AdminFaqItemUpsertDto
            {
                CategoryId = SelectedCategory.CategoryId,
                Question = ItemQuestion.Trim(),
                Answer = ItemAnswer.Trim(),
                ItemOrder = ord,
                IsActive = ItemIsActive
            };

            if (_editingFaqId is > 0)
                await _settings.UpdateFaqItemAsync(_editingFaqId.Value, dto);
            else
                await _settings.CreateFaqItemAsync(dto);

            await LoadAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task DeleteItemAsync()
    {
        if (SelectedItem == null) return;
        if (!UserDialogs.Confirm("Удалить этот вопрос FAQ?", "FAQ"))
            return;
        Error = null;
        IsBusy = true;
        try
        {
            await _settings.DeleteFaqItemAsync(SelectedItem.FaqId);
            BeginNewItem();
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}
