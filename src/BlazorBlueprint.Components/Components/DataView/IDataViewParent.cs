using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Internal interface allowing non-generic slot components (BbDataViewHeader, BbDataViewFooter)
/// to register their content with the parent BbDataView component without requiring
/// knowledge of the TItem type parameter.
/// </summary>
internal interface IDataViewParent
{
    public void SetHeaderContent(RenderFragment? content);
    public void SetFooterContent(RenderFragment? content);
}
