using Microsoft.AspNetCore.Components;

namespace pokedex_aspire.Web.Components.Shared;

public partial class TabStrip<TItem> where TItem : notnull
{
    [Parameter, EditorRequired]
    public string Id { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string AriaLabel { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public IReadOnlyList<TItem> Items { get; set; } = [];

    [Parameter, EditorRequired]
    public Func<TItem, string> ItemText { get; set; } = default!;

    [Parameter, EditorRequired]
    public TItem SelectedItem { get; set; } = default!;

    [Parameter]
    public EventCallback<TItem> SelectedItemChanged { get; set; }

    [Parameter, EditorRequired]
    public RenderFragment<TItem> ChildContent { get; set; } = default!;

    private TItem currentItem = default!;
    private bool hasCurrentItem;

    protected override void OnParametersSet()
    {
        if (!hasCurrentItem || !EqualityComparer<TItem>.Default.Equals(currentItem, SelectedItem))
        {
            currentItem = SelectedItem;
            hasCurrentItem = true;
        }
    }

    private bool IsSelected(TItem item) =>
        hasCurrentItem && EqualityComparer<TItem>.Default.Equals(currentItem, item);

    private async Task SelectItemAsync(TItem item)
    {
        currentItem = item;
        await SelectedItemChanged.InvokeAsync(item);
    }

    private string GetTabId(int index) => $"{Id}-tab-{index}";

    private string GetPanelId(int index) => $"{Id}-panel-{index}";
}