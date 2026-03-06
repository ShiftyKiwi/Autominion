using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace Autominion.Windows;

public sealed class ConfigWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private string minionFilter = string.Empty;

    public ConfigWindow(Plugin plugin) : base("Autominion Settings###AutominionConfig")
    {
        this.plugin = plugin;
        Size = new Vector2(520, 420);
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
        DrawCheckbox("Allow roulette fallback for summon", configuration.UseMinionRouletteForSummon, value => configuration.UseMinionRouletteForSummon = value);
        DrawCheckbox("Only react to location changes", configuration.OnlyActOnLocationChanges, value => configuration.OnlyActOnLocationChanges = value);
        DrawCheckbox("Do not act while between areas", configuration.SuppressBetweenAreas, value => configuration.SuppressBetweenAreas = value);
        DrawCheckbox("Do not act while bound by duty", configuration.SuppressInDuty, value => configuration.SuppressInDuty = value);
        DrawCheckbox("Show debug details in main window", configuration.ShowDebugDetails, value => configuration.ShowDebugDetails = value);

        ImGui.Separator();
        ImGui.Text("Preferred summon target");
        ImGui.SetNextItemWidth(-1f);
        ImGui.InputTextWithHint("##minionFilter", "Filter minions", ref minionFilter, 128);

        var selectedLabel = plugin.MinionController.GetConfiguredMinionLabel();
        if (ImGui.BeginCombo("Select Minion", selectedLabel))
        {
            DrawMinionOption("Minion Roulette", 0, configuration.SelectedMinionId == 0);

            foreach (var companion in Plugin.DataManager.GetExcelSheet<Companion>()
                         .Where(c => c.RowId > 0 && !c.Singular.IsEmpty)
                         .OrderBy(c => c.Singular.ToString()))
            {
                var label = $"{companion.Singular}##{companion.RowId}";
                if (!string.IsNullOrWhiteSpace(minionFilter) &&
                    !companion.Singular.ToString().Contains(minionFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                DrawMinionOption(label, companion.RowId, configuration.SelectedMinionId == companion.RowId);
            }

            ImGui.EndCombo();
        }
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
