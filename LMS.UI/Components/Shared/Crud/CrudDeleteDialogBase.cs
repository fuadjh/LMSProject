using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebUi.Services;

namespace WebUi.Components.Crud;

public abstract class CrudDeleteDialogBase<TId> : ComponentBase
{
    [Inject]
    protected HttpClient Http { get; set; } = default!;

    [Inject]
    protected ISnackbar Snackbar { get; set; } = default!;

    [CascadingParameter]
    protected IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public TId Id { get; set; } = default!;

    [Parameter]
    public string ItemTitle { get; set; } = string.Empty;

    protected bool IsBusy { get; private set; }

    protected abstract string GetDeleteEndpoint(TId id);

    protected abstract string DeleteSuccessMessage { get; }

    protected async Task DeleteAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        StateHasChanged();

        try
        {
            var response = await Http.DeleteAsync(GetDeleteEndpoint(Id));

            if (!response.IsSuccessStatusCode)
            {
                Snackbar.Add(
                    await ApiErrorReader.ReadErrorAsync(response),
                    Severity.Error);

                return;
            }

            Snackbar.Add(DeleteSuccessMessage, Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
        }
        catch (HttpRequestException)
        {
            Snackbar.Add("ارتباط با سرور برقرار نشد.", Severity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected void CancelDialog()
    {
        if (!IsBusy)
            MudDialog.Cancel();
    }
}