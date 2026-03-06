using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace Autominion.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin) : base("Autominion###AutominionMain")
    {
        this.plugin = plugin;
        Size = new Vector2(520, 340);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        var configuration = plugin.Configuration;
        var location = plugin.LocationMonitor.LastLocation;

        ImGui.Text(configuration.PluginEnabled ? "Status: enabled" : "Status: disabled");
        ImGui.Text($"Current territory: {plugin.LocationMonitor.CurrentTerritoryName}");
        ImGui.Text($"Housing location: {(location is null ? "none" : location)}");
        ImGui.Text($"Configured summon target: {plugin.MinionController.GetConfiguredMinionLabel()}");
        ImGui.Text($"Current minion: {plugin.MinionController.CurrentMinionLabel}");
        ImGui.Text($"Pending action: {plugin.MinionController.PendingActionLabel}");
        ImGui.TextWrapped($"Last action: {plugin.MinionController.LastActionMessage}");

        ImGui.Spacing();

        if (ImGui.Button("Open Settings"))
        {
            plugin.ToggleConfigUi();
        }

        ImGui.SameLine();
        if (ImGui.Button("Rescan"))
        {
            plugin.LocationMonitor.ForceEvaluateCurrentLocation();
        }

        if (!configuration.ShowDebugDetails)
        {
            return;
        }

        ImGui.Separator();
        ImGui.Text($"Last location key: {plugin.LocationMonitor.LastLocationKey ?? "none"}");
        ImGui.Text($"Last evaluated territory: {plugin.LocationMonitor.LastTerritoryId}");
        ImGui.Text($"Last requested action: {plugin.MinionController.LastRequestedActionLabel}");
    }
}
