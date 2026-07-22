using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebUi.Services;

namespace WebUi.Components.Crud;

public abstract class CrudUpsertDialogBase<TModel> : ComponentBase
    where TModel : class
{
    [Inject]
    protected HttpClient Http { get; set; } = default!;

    [Inject]
    protected ISnackbar Snackbar { get; set; } = default!;

    [CascadingParameter]
    protected IMudDialogInstance MudDialog { get; set; } = default!;

    protected bool IsBusy { get; private set; }

    protected abstract string CreateEndpoint { get; }

    protected abstract string GetUpdateEndpoint(TModel model);

    protected abstract object CreateRequest(TModel model);

    protected abstract object UpdateRequest(TModel model);

    protected abstract string CreateSuccessMessage { get; }

    protected abstract string UpdateSuccessMessage { get; }

    protected async Task SaveAsync(TModel model, bool isEdit)
    {
        if (IsBusy)
            return;

        IsBusy = true;
        StateHasChanged();

        try
        {
            HttpResponseMessage response;

            if (isEdit)
            {
                response = await Http.PutAsJsonAsync(
                    GetUpdateEndpoint(model),
                    UpdateRequest(model));
            }
            else
            {
                response = await Http.PostAsJsonAsync(
                    CreateEndpoint,
                    CreateRequest(model));
            }

            if (!response.IsSuccessStatusCode)
            {
                Snackbar.Add(
                    await ApiErrorReader.ReadErrorAsync(response),
                    Severity.Error);

                return;
            }

            Snackbar.Add(
                isEdit ? UpdateSuccessMessage : CreateSuccessMessage,
                Severity.Success);

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