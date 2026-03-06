using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace Autominion.Windows;

public sealed class ConfigWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private readonly List<(uint RowId, string Name)> ownedMinions = [];
    private string minionFilter = string.Empty;
    private bool needsRefresh = true;

    public ConfigWindow(Plugin plugin) : base("Autominion Settings###AutominionConfig")
    {
        this.plugin = plugin;
        Size = new Vector2(520, 400);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        var configuration = plugin.Configuration;

        DrawCheckbox("Enable plugin", configuration.PluginEnabled, value => configuration.PluginEnabled = value);
        DrawCheckbox("Dismiss when entering a housing plot", configuration.DismissOnPlotEntry, value => configuration.DismissOnPlotEntry = value);
        DrawCheckbox("Summon when leaving a housing plot", configuration.SummonOnPlotExit, value => configuration.SummonOnPlotExit = value);
        DrawCheckbox("Allow roulette fallback for summon", configuration.UseMinionRouletteForSummon, value => configuration.UseMinionRouletteForSummon = value);
        DrawCheckbox("Only react to location changes", configuration.OnlyActOnLocationChanges, value => configuration.OnlyActOnLocationChanges = value);
        DrawCheckbox("Do not act while between areas", configuration.SuppressBetweenAreas, value => configuration.SuppressBetweenAreas = value);
        DrawCheckbox("Do not act while bound by duty", configuration.SuppressInDuty, value => configuration.SuppressInDuty = value);
        DrawCheckbox("Show debug details in main window", configuration.ShowDebugDetails, value => configuration.ShowDebugDetails = value);

        ImGui.Separator();
        ImGui.Text("Preferred summon target after leaving plot");

        if (ImGui.Button("Refresh Owned Minions"))
        {
            needsRefresh = true;
        }

        ImGui.SetNextItemWidth(-1f);
        ImGui.InputTextWithHint("##minionFilter", "Filter owned minions", ref minionFilter, 128);

        RefreshOwnedMinionsIfNeeded();

        var selectedLabel = plugin.MinionController.GetConfiguredMinionLabel();
        if (ImGui.BeginCombo("Select Minion", selectedLabel))
        {
            DrawMinionOption("Minion Roulette", 0, configuration.SelectedMinionId == 0);

            foreach (var (rowId, name) in ownedMinions)
            {
                if (!string.IsNullOrWhiteSpace(minionFilter) && !name.Contains(minionFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                DrawMinionOption($"{name}##{rowId}", rowId, configuration.SelectedMinionId == rowId);
            }

            ImGui.EndCombo();
        }
    }

    private void RefreshOwnedMinionsIfNeeded()
    {
        if (!needsRefresh)
        {
            return;
        }

        ownedMinions.Clear();

        foreach (var companion in Plugin.DataManager.GetExcelSheet<Companion>()
                     .Where(c => c.RowId > 0 && !c.Singular.IsEmpty)
                     .OrderBy(c => c.Singular.ToString()))
        {
            if (!Plugin.UnlockState.IsCompanionUnlocked(companion))
            {
                continue;
            }

            ownedMinions.Add((companion.RowId, companion.Singular.ToString()));
        }

        needsRefresh = false;
    }

    private void DrawMinionOption(string label, uint rowId, bool isSelected)
    {
        if (ImGui.Selectable(label, isSelected))
        {
            plugin.Configuration.SelectedMinionId = rowId;
            plugin.Configuration.Save();
        }

        if (isSelected)
        {
            ImGui.SetItemDefaultFocus();
        }
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
