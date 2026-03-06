using System;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace Autominion;

public sealed class HousingLocationMonitor : IDisposable
{
    private readonly Plugin plugin;
    private readonly MinionController controller;
    private DateTimeOffset lastPoll = DateTimeOffset.MinValue;

    public HousingLocation? LastLocation { get; private set; }
    public string? LastLocationKey { get; private set; }
    public ushort LastTerritoryId { get; private set; }
    public string CurrentTerritoryName { get; private set; } = "Unknown";

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

        if (!force && plugin.Configuration.OnlyActOnLocationChanges && locationKey == LastLocationKey)
        {
            return;
        }

        LastLocation = location;
        LastLocationKey = locationKey;

        if (location is null || !location.IsMeaningfulLocation)
        {
            return;
        }

        if (location.IsInteriorLike())
        {
            if (location.IsApartment && !plugin.Configuration.DismissInApartments)
            {
                return;
            }

            if (!location.IsApartment && !plugin.Configuration.DismissInHousingInteriors)
            {
                return;
            }

            controller.RequestAction(MinionAction.Dismiss, location);
            return;
        }

        if (location.IsExteriorWardLocation && plugin.Configuration.SummonInHousingExteriors)
        {
            controller.RequestAction(MinionAction.Summon, location);
        }
    }

    private string GetTerritoryName(ushort territoryId)
    {
        return Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territory)
            ? territory.PlaceName.Value.Name.ToString()
            : $"Territory {territoryId}";
    }
}
