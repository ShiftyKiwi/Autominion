using Dalamud.Configuration;
using System;

namespace Autominion;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool PluginEnabled { get; set; } = true;
    public bool DismissOnPlotEntry { get; set; } = true;
    public bool SummonOnPlotExit { get; set; } = true;
    public bool UseMinionRouletteForSummon { get; set; } = true;
    public uint SelectedMinionId { get; set; } = 0;
    public bool OnlyActOnLocationChanges { get; set; } = true;
    public bool SuppressInDuty { get; set; } = true;
    public bool SuppressBetweenAreas { get; set; } = true;
    public bool ShowDebugDetails { get; set; }

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
