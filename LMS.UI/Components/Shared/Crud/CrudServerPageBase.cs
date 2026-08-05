using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WebUi.Components.Crud;

public abstract class CrudServerPageBase<TItem> : ComponentBase
{
    [Inject]
    protected IDialogService DialogService { get; set; } = default!;

    // برای سازگاری با صفحات قبلی
    protected MudTable<TItem>? CrudTable;

    // برای کامپوننت مشترک جدید CrudTable
    protected CrudTable<TItem>?
        CrudTableReference;

    protected async Task OpenDialogAndReloadAsync<TDialog>(
        string title,
        DialogParameters parameters,
        MaxWidth maxWidth)
        where TDialog : IComponent
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth = true,
            MaxWidth = maxWidth
        };

        var dialog = await DialogService.ShowAsync<TDialog>(
            title,
            parameters,
            options);

        var result = await dialog.Result;

        if (result is null || result.Canceled)
            return;

        await ReloadAsync();
    }

    protected async Task ReloadAsync()
    {
        if (CrudTableReference is not null)
        {
            await CrudTableReference.ReloadServerDataAsync();
            return;
        }

        if (CrudTable is not null)
            await CrudTable.ReloadServerData();
    }
}