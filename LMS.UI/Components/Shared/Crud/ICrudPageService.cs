namespace WebUi.Components.Shared.Crud;

public interface ICrudPageService<TListItem, TFormModel, TKey>
{
    Task<IReadOnlyList<TListItem>> GetListAsync(CancellationToken cancellationToken = default);
    Task<TFormModel> CreateModelAsync(CancellationToken cancellationToken = default);
    Task<TFormModel> GetForEditAsync(TKey id, CancellationToken cancellationToken = default);
    Task SaveAsync(TFormModel model, CancellationToken cancellationToken = default);
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    Task SetActiveAsync(TKey id, bool isActive, CancellationToken cancellationToken = default);
}