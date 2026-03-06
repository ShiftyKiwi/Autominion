using Dalamud.Configuration;
using System;

namespace Autominion;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool PluginEnabled { get; set; } = true;
    public bool SummonInHousingExteriors { get; set; } = true;
    public bool DismissInHousingInteriors { get; set; } = true;
    public bool DismissInApartments { get; set; } = true;
    public bool UseMinionRouletteForSummon { get; set; } = true;
    public bool OnlyActOnLocationChanges { get; set; } = true;
    public bool SuppressInDuty { get; set; } = true;
    public bool SuppressBetweenAreas { get; set; } = true;
    public bool ShowDebugDetails { get; set; }

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
