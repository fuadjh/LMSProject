using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WebUi.Components.Crud;

public abstract class CrudServerPageBase<TItem> : ComponentBase
{
    [Inject]
    protected IDialogService DialogService { get; set; } = default!;

    protected MudTable<TItem>? CrudTable;

    protected async Task OpenDialogAndReloadAsync<TDialog>(
        string title,
        DialogParameters<TDialog> parameters,
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
        if (CrudTable is not null)
            await CrudTable.ReloadServerData();
    }
}