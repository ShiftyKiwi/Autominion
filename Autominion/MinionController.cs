using System;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace Autominion;

public enum MinionAction
{
    None = 0,
    Summon = 1,
    Dismiss = 2,
}

public sealed class MinionController : IDisposable
{
    private const uint MinionRouletteActionId = 10;
    private const int TickCooldown = 60;
    private const int TickTimeout = 2000;

    private readonly Plugin plugin;
    private MinionAction pendingAction = MinionAction.None;
    private HousingLocation? pendingLocation;
    private int frameTick;
    private int lastTick;
    private bool isSubscribed;

    public string LastActionMessage { get; private set; } = "No action yet.";
    public string PendingActionLabel => pendingAction == MinionAction.None ? "none" : pendingAction.ToString();
    public string LastRequestedActionLabel { get; private set; } = "none";
    public string CurrentMinionLabel => GetMinionName(GetLocalPlayer()?.CurrentMinion?.RowId ?? 0);

    public MinionController(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public void Dispose()
    {
        StopCycle();
    }

    public void RequestAction(MinionAction action, HousingLocation location)
    {
        if (!plugin.Configuration.PluginEnabled)
        {
            return;
        }

        pendingAction = action;
        pendingLocation = location;
        LastRequestedActionLabel = $"{action} @ {location}";
        StartCycle();
    }

    public string GetConfiguredMinionLabel()
    {
        return plugin.Configuration.SelectedMinionId == 0
            ? "Minion Roulette"
            : GetMinionName(plugin.Configuration.SelectedMinionId);
    }

    private void StartCycle()
    {
        if (isSubscribed)
        {
            return;
        }

        frameTick = 0;
        lastTick = 0;
        Plugin.Framework.Update += OnFrameworkUpdate;
        isSubscribed = true;
    }

    private void StopCycle()
    {
        if (isSubscribed)
        {
            Plugin.Framework.Update -= OnFrameworkUpdate;
            isSubscribed = false;
        }

        frameTick = 0;
        lastTick = 0;
        pendingAction = MinionAction.None;
        pendingLocation = null;
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        frameTick++;

        if (frameTick < lastTick + TickCooldown)
        {
            return;
        }

        if (frameTick > TickTimeout)
        {
            LastActionMessage = "Timed out waiting for a valid action window.";
            StopCycle();
            return;
        }

        lastTick = frameTick;

        var localPlayer = GetLocalPlayer();
        if (Plugin.PlayerState.ContentId == 0 || localPlayer == null)
        {
            return;
        }

        if (plugin.Configuration.SuppressInDuty && Plugin.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty])
        {
            return;
        }

        if (plugin.Configuration.SuppressBetweenAreas &&
            (Plugin.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas] || Plugin.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas51]))
        {
            return;
        }

        switch (pendingAction)
        {
            case MinionAction.Summon:
                TrySummon();
                break;
            case MinionAction.Dismiss:
                TryDismiss();
                break;
        }
    }

    private void TrySummon()
    {
        if (IsMinionSummoned())
        {
            LastActionMessage = "Summon skipped because a minion is already out.";
            StopCycle();
            return;
        }

        var selectedMinionId = plugin.Configuration.SelectedMinionId;
        if (selectedMinionId != 0)
        {
            if (ActionAvailable(ActionType.Companion, selectedMinionId) && CastAction(ActionType.Companion, selectedMinionId))
            {
                LastActionMessage = $"Summon requested for {GetMinionName(selectedMinionId)} at {pendingLocation}.";
                StopCycle();
                return;
            }

            if (!plugin.Configuration.UseMinionRouletteForSummon)
            {
                LastActionMessage = $"Specific minion summon failed for {GetMinionName(selectedMinionId)} and roulette fallback is disabled.";
                StopCycle();
                return;
            }
        }

        if (!plugin.Configuration.UseMinionRouletteForSummon)
        {
            LastActionMessage = "Summon skipped because roulette summon is disabled and no specific minion was configured.";
            StopCycle();
            return;
        }

        if (!ActionAvailable(ActionType.GeneralAction, MinionRouletteActionId))
        {
            return;
        }

        if (!CastAction(ActionType.GeneralAction, MinionRouletteActionId))
        {
            return;
        }

        LastActionMessage = $"Roulette summon requested for {pendingLocation}.";
        StopCycle();
    }

    private void TryDismiss()
    {
        var currentMinionId = GetLocalPlayer()?.CurrentMinion?.RowId ?? 0;
        if (currentMinionId == 0)
        {
            LastActionMessage = "Dismiss skipped because no minion is currently summoned.";
            StopCycle();
            return;
        }

        if (!ActionAvailable(ActionType.Companion, currentMinionId))
        {
            return;
        }

        if (!CastAction(ActionType.Companion, currentMinionId))
        {
            return;
        }

        LastActionMessage = $"Dismiss requested for {GetMinionName(currentMinionId)} at {pendingLocation}.";
        StopCycle();
    }

    private bool IsMinionSummoned()
    {
        return GetLocalPlayer()?.CurrentMinion?.RowId != 0;
    }

    private static IPlayerCharacter? GetLocalPlayer()
    {
        return Plugin.ObjectTable.LocalPlayer as IPlayerCharacter;
    }

    private static unsafe bool CastAction(ActionType actionType, uint id)
    {
        try
        {
            return ActionManager.Instance()->UseAction(actionType, id);
        }
        catch
        {
            return false;
        }
    }

    private static unsafe bool ActionAvailable(ActionType actionType, uint id)
    {
        try
        {
            return ActionManager.Instance()->GetActionStatus(actionType, id) == 0 &&
                   !ActionManager.Instance()->IsRecastTimerActive(actionType, id);
        }
        catch
        {
            return false;
        }
    }

    private static string GetMinionName(uint rowId)
    {
        if (rowId == 0)
        {
            return "none";
        }

        return Plugin.DataManager.GetExcelSheet<Companion>().TryGetRow(rowId, out var companion) && !companion.Singular.IsEmpty
            ? companion.Singular.ToString()
            : $"Row {rowId}";
    }
}
