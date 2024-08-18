using ImGuiNET;
using OtterGui;
using OtterGui.Classes;
using OtterGui.Raii;
using OtterGui.Services;
using OtterGui.Widgets;
using Penumbra.Api.Enums;
using Penumbra.Collections.Manager;
using Penumbra.GameData.Data;
using Penumbra.Mods;
using Penumbra.Mods.Editor;
using Penumbra.Services;
using Penumbra.UI.Classes;

namespace Penumbra.UI.Tabs;

public class ChangedItemsTab(
    CollectionManager collectionManager,
    CollectionSelectHeader collectionHeader,
    ChangedItemDrawer drawer,
    CommunicatorService communicator)
    : ITab, IUiService
{
    public ReadOnlySpan<byte> Label
        => "Changed Items"u8;

    private LowerString _changedItemFilter    = LowerString.Empty;
    private LowerString _changedItemModFilter = LowerString.Empty;

    public void DrawContent()
    {
        collectionHeader.Draw(true);
        drawer.DrawTypeFilter();
        var       varWidth = DrawFilters();
        using var child    = ImRaii.Child("##changedItemsChild", -Vector2.One);
        if (!child)
            return;

        var       height = ImGui.GetFrameHeight() + 2 * ImGui.GetStyle().CellPadding.Y;
        var       skips  = ImGuiClip.GetNecessarySkips(height);
        using var list   = ImRaii.Table("##changedItems", 3, ImGuiTableFlags.RowBg, -Vector2.One);
        if (!list)
            return;

        const ImGuiTableColumnFlags flags = ImGuiTableColumnFlags.NoResize | ImGuiTableColumnFlags.WidthFixed;
        ImGui.TableSetupColumn("items", flags, 450 * UiHelpers.Scale);
        ImGui.TableSetupColumn("mods",  flags, varWidth - 130 * UiHelpers.Scale);
        ImGui.TableSetupColumn("id",    flags, 130 * UiHelpers.Scale);

        var items = collectionManager.Active.Current.ChangedItems;
        var rest  = ImGuiClip.FilteredClippedDraw(items, skips, FilterChangedItem, DrawChangedItemColumn);
        ImGuiClip.DrawEndDummy(rest, height);
    }

    /// <summary> Draw a pair of filters and return the variable width of the flexible column. </summary>
    private float DrawFilters()
    {
        var varWidth = ImGui.GetContentRegionAvail().X
          - 450 * UiHelpers.Scale
          - ImGui.GetStyle().ItemSpacing.X;
        ImGui.SetNextItemWidth(450 * UiHelpers.Scale);
        LowerString.InputWithHint("##changedItemsFilter", "Filter Item...", ref _changedItemFilter, 128);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(varWidth);
        LowerString.InputWithHint("##changedItemsModFilter", "Filter Mods...", ref _changedItemModFilter, 128);
        return varWidth;
    }

    /// <summary> Apply the current filters. </summary>
    private bool FilterChangedItem(KeyValuePair<string, (SingleArray<IMod>, IIdentifiedObjectData?)> item)
        => drawer.FilterChangedItem(item.Key, item.Value.Item2, _changedItemFilter)
         && (_changedItemModFilter.IsEmpty || item.Value.Item1.Any(m => m.Name.Contains(_changedItemModFilter)));

    /// <summary> Draw a full column for a changed item. </summary>
    private void DrawChangedItemColumn(KeyValuePair<string, (SingleArray<IMod>, IIdentifiedObjectData?)> item)
    {
        ImGui.TableNextColumn();
        drawer.DrawCategoryIcon(item.Value.Item2);
        ImGui.SameLine();
        drawer.DrawChangedItem(item.Key, item.Value.Item2);
        ImGui.TableNextColumn();
        DrawModColumn(item.Value.Item1);

        ImGui.TableNextColumn();
        ChangedItemDrawer.DrawModelData(item.Value.Item2);
    }

    private void DrawModColumn(SingleArray<IMod> mods)
    {
        if (mods.Count <= 0)
            return;

        var       first = mods[0];
        using var style = ImRaii.PushStyle(ImGuiStyleVar.SelectableTextAlign, new Vector2(0, 0.5f));
        if (ImGui.Selectable(first.Name, false, ImGuiSelectableFlags.None, new Vector2(0, ImGui.GetFrameHeight()))
         && ImGui.GetIO().KeyCtrl
         && first is Mod mod)
            communicator.SelectTab.Invoke(TabType.Mods, mod);

        if (ImGui.IsItemHovered())
        {
            using var _ = ImRaii.Tooltip();
            ImGui.TextUnformatted("Hold Control and click to jump to mod.\n");
            if (mods.Count > 1)
                ImGui.TextUnformatted("Other mods affecting this item:\n" + string.Join("\n", mods.Skip(1).Select(m => m.Name)));
        }
    }
}
