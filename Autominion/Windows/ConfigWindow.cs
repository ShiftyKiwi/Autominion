using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace Autominion.Windows;

public sealed class ConfigWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public ConfigWindow(Plugin plugin) : base("Autominion Settings###AutominionConfig")
    {
        this.plugin = plugin;
        Size = new Vector2(430, 280);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        var configuration = plugin.Configuration;

        DrawCheckbox("Enable plugin", configuration.PluginEnabled, value => configuration.PluginEnabled = value);
        DrawCheckbox("Summon while on housing exteriors", configuration.SummonInHousingExteriors, value => configuration.SummonInHousingExteriors = value);
        DrawCheckbox("Dismiss inside house interiors", configuration.DismissInHousingInteriors, value => configuration.DismissInHousingInteriors = value);
        DrawCheckbox("Dismiss inside apartments", configuration.DismissInApartments, value => configuration.DismissInApartments = value);
        DrawCheckbox("Use Minion Roulette to summon", configuration.UseMinionRouletteForSummon, value => configuration.UseMinionRouletteForSummon = value);
        DrawCheckbox("Only react to location changes", configuration.OnlyActOnLocationChanges, value => configuration.OnlyActOnLocationChanges = value);
        DrawCheckbox("Do not act while between areas", configuration.SuppressBetweenAreas, value => configuration.SuppressBetweenAreas = value);
        DrawCheckbox("Do not act while bound by duty", configuration.SuppressInDuty, value => configuration.SuppressInDuty = value);
        DrawCheckbox("Show debug details in main window", configuration.ShowDebugDetails, value => configuration.ShowDebugDetails = value);
    }

    private void DrawCheckbox(string label, bool currentValue, Action<bool> setValue)
    {
        var value = currentValue;
        if (ImGui.Checkbox(label, ref value))
        {
            setValue(value);
            plugin.Configuration.Save();
        }
    }
}
