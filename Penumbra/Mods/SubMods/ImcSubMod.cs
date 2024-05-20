using Newtonsoft.Json.Linq;
using Penumbra.GameData.Structs;
using Penumbra.Mods.Groups;

namespace Penumbra.Mods.SubMods;

public class ImcSubMod(ImcModGroup group) : IModOption
{
    public readonly ImcModGroup Group = group;

    public ImcSubMod(ImcModGroup group, JToken json)
        : this(group)
    {
        SubMod.LoadOptionData(json, this);
        AttributeMask = (ushort)((json[nameof(AttributeMask)]?.ToObject<ushort>() ?? 0) & ImcEntry.AttributesMask);
    }

    public Mod Mod
        => Group.Mod;

    public ushort AttributeMask;

    Mod IModOption.Mod
        => Mod;

    IModGroup IModOption.Group
        => Group;

    public string Name { get; set; } = "Part";

    public string FullName
        => $"{Group.Name}: {Name}";

    public string Description { get; set; } = string.Empty;

    public int GetIndex()
        => SubMod.GetIndex(this);
}
