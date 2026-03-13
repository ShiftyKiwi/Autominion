using System;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace Autominion;

public sealed class HousingLocationMonitor : IDisposable
{
    private readonly Plugin plugin;
    private readonly MinionController controller;
    private DateTimeOffset lastPoll = DateTimeOffset.MinValue;
    private bool? lastWasOnPlot;

    public HousingLocation? LastLocation { get; private set; }
    public string? LastLocationKey { get; private set; }
    public ushort LastTerritoryId { get; private set; }
    public string CurrentTerritoryName { get; private set; } = "Unknown";
    public bool IsCurrentlyOnPlot { get; private set; }

    public HousingLocationMonitor(Plugin plugin, MinionController controller)
    {
        this.plugin = plugin;
        this.controller = controller;
    }

    public void Initialize()
    {
        Plugin.ClientState.TerritoryChanged += OnTerritoryChanged;
        Plugin.Framework.Update += OnFrameworkUpdate;

        if (Plugin.ClientState.IsLoggedIn)
        {
            EvaluateCurrentLocation(force: true);
        }
    }

    public void Dispose()
    {
        Plugin.ClientState.TerritoryChanged -= OnTerritoryChanged;
        Plugin.Framework.Update -= OnFrameworkUpdate;
    }

    public void ForceEvaluateCurrentLocation()
    {
        EvaluateCurrentLocation(force: true);
    }

    private void OnTerritoryChanged(ushort territory)
    {
        LastTerritoryId = territory;
        CurrentTerritoryName = GetTerritoryName(territory);
        EvaluateCurrentLocation(force: true);
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (DateTimeOffset.UtcNow - lastPoll < TimeSpan.FromSeconds(2))
        {
            return;
        }

        EvaluateCurrentLocation(force: false);
    }

    private void EvaluateCurrentLocation(bool force)
    {
        lastPoll = DateTimeOffset.UtcNow;

        var territory = Plugin.ClientState.TerritoryType;
        LastTerritoryId = territory;
        CurrentTerritoryName = GetTerritoryName(territory);

        var location = HousingLocation.FromCurrentLocation(territory);
        var locationKey = location?.GetLocationKey();
        var isOnPlot = location?.IsOnPlot == true;

        if (!force && plugin.Configuration.OnlyActOnLocationChanges && locationKey == LastLocationKey)
        {
            return;
        }

        LastLocation = location;
        LastLocationKey = locationKey;
        IsCurrentlyOnPlot = isOnPlot;

        var previousState = lastWasOnPlot;
        lastWasOnPlot = isOnPlot;

        if (previousState is null || location is null)
        {
            return;
        }

        if (!previousState.Value && isOnPlot && plugin.Configuration.DismissOnPlotEntry)
        {
            controller.RequestAction(MinionAction.Dismiss, location);
            return;
        }

        if (previousState.Value && !isOnPlot)
        {
            // Entering the interior from a plot should keep minions dismissed, never summon.
            if (location.IsInside == true || location.IsApartment)
            {
                if (plugin.Configuration.DismissOnPlotEntry)
                {
                    controller.RequestAction(MinionAction.Dismiss, location);
                }

                return;
            }

            if (plugin.Configuration.SummonOnPlotExit && location.IsInside == false)
            {
                controller.RequestAction(MinionAction.Summon, location);
            }
        }
    }

    private string GetTerritoryName(ushort territoryId)
    {
        return Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territory)
            ? territory.PlaceName.Value.Name.ToString()
            : $"Territory {territoryId}";
    }
}
