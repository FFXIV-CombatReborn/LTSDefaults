using System.Reflection.Metadata.Ecma335;

namespace DefaultRotations.Ranged;

[Rotation("LTS's PvP", CombatType.PvP, GameVersion = "6.58")]
[SourceCode(Path = "main/DefaultRotations/Ranged/MCH_Default.PvP.cs")]
public sealed class MCH_DefaultPvP : MachinistRotation
{

    public static IBaseAction MarksmansSpitePvP { get; } = new BaseAction((ActionID)29415);
    public static IBaseAction BishopAutoturretPvP2 { get; } = new BaseAction((ActionID)29412);

    [RotationConfig(CombatType.PvP, Name = "")]
    public bool LBInPvP { get; set; } = false;

    [Range(1, 100, ConfigUnitType.Percent, 1)]
    [RotationConfig(CombatType.PvP, Name = "The target HP%% required to perform LB:Marksman's Spite is")]
    public int MSValue { get; set; } = 45;

    [RotationConfig(CombatType.PvP, Name = "Sprint")]
    public bool UseSprintPvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Recuperate")]
    public bool UseRecuperatePvP { get; set; } = false;

    [Range(1, 100, ConfigUnitType.Percent, 1)]
    [RotationConfig(CombatType.PvP, Name = "RecuperateHP%%?")]
    public int RCValue { get; set; } = 75;

    [RotationConfig(CombatType.PvP, Name = "Purify")]
    public bool UsePurifyPvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Stun")]
    public bool Use1343PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "DeepFreeze")]
    public bool Use3219PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "HalfAsleep")]
    public bool Use3022PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Sleep")]
    public bool Use1348PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Bind")]
    public bool Use1345PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Heavy")]
    public bool Use1344PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Silence")]
    public bool Use1347PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "While on the defensive, the attack is aborted.")]
    public bool GuardCancel { get; set; } = false;


    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        act = null;
        if (GuardCancel && Player.HasStatus(true, StatusID.Guard)) return false;

        if (!!Player.HasStatus(true, StatusID.Guard) && UseRecuperatePvP && ((Player.CurrentHp / Player.MaxHp) * 100) < RCValue &&
            RecuperatePvP.CanUse(out act)) return true;

        if (UsePurifyPvP && Use1343PvP && Player.HasStatus(true, (StatusID)1343) && PurifyPvP.CanUse(out act)) return true;
        if (UsePurifyPvP && Use3219PvP && Player.HasStatus(true, (StatusID)3219) && PurifyPvP.CanUse(out act)) return true;
        if (UsePurifyPvP && Use3022PvP && Player.HasStatus(true, (StatusID)3022) && PurifyPvP.CanUse(out act)) return true;
        if (UsePurifyPvP && Use1348PvP && Player.HasStatus(true, (StatusID)1348) && PurifyPvP.CanUse(out act)) return true;
        if (UsePurifyPvP && Use1345PvP && Player.HasStatus(true, (StatusID)1345) && PurifyPvP.CanUse(out act)) return true;
        if (UsePurifyPvP && Use1344PvP && Player.HasStatus(true, (StatusID)1344) && PurifyPvP.CanUse(out act)) return true;
        if (UsePurifyPvP && Use1347PvP && Player.HasStatus(true, (StatusID)1347) && PurifyPvP.CanUse(out act)) return true;

        if (GuardCancel && Player.HasStatus(true, StatusID.Guard)) return false;

        if (Player.HasStatus(true, StatusID.Overheated) &&
            HeatBlastPvP.CanUse(out act)) return true;


        if (!Player.HasStatus(true, StatusID.Overheated) && !Target.HasStatus(true, StatusID.Guard))
        {
            act = null;
            if (Player.HasStatus(true, StatusID.Overheated)) return false;


            if (LimitBreakLevel >= 1 && LBInPvP && Target.GetHealthRatio()*100 <= MSValue && 
                MarksmansSpitePvP.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;

            if (!Player.HasStatus(true, StatusID.Overheated) && HostileTarget.DistanceToPlayer() <= 12 &&
                ScattergunPvP.CanUse(out act, skipAoeCheck: true)) return true;
            
            if (!Player.HasStatus(true, StatusID.Overheated) && Player.HasStatus(true, StatusID.BioblasterPrimed) && HostileTarget.DistanceToPlayer() <= 12 &&
                BioblasterPvP.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;

            if (!Player.HasStatus(true, StatusID.Overheated) && Player.HasStatus(true, StatusID.AirAnchorPrimed) && 
                AirAnchorPvP.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;

            if (!Player.HasStatus(true, StatusID.Overheated) && Player.HasStatus(true,StatusID.ChainSawPrimed) &&  
                ChainSawPvP.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;
        }
        if (!Player.HasStatus(true, StatusID.Overheated) && Player.HasStatus(true, StatusID.DrillPrimed) && DrillPvP.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;
        if (!Player.HasStatus(true, StatusID.Overheated) && BlastChargePvP.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;
        if (Player.HasStatus(true, StatusID.Overheated) &&
            HeatBlastPvP.CanUse(out act)) return true;


        if (GuardCancel && Player.HasStatus(true, StatusID.Guard)) return false;

        if (!Player.HasStatus(true, StatusID.Guard) && UseSprintPvP && !Player.HasStatus(true, StatusID.Sprint) &&
            SprintPvP.CanUse(out act)) return true;
        #endregion

        return base.GeneralGCD(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {

        if (UsePurifyPvP && Use1343PvP && Player.HasStatus(true, (StatusID)1343) && PurifyPvP.CanUse(out act)) return true;
        if (UsePurifyPvP && Use3219PvP && Player.HasStatus(true, (StatusID)3219) && PurifyPvP.CanUse(out act)) return true;
        if (UsePurifyPvP && Use3022PvP && Player.HasStatus(true, (StatusID)3022) && PurifyPvP.CanUse(out act)) return true;
        if (UsePurifyPvP && Use1348PvP && Player.HasStatus(true, (StatusID)1348) && PurifyPvP.CanUse(out act)) return true;
        if (UsePurifyPvP && Use1345PvP && Player.HasStatus(true, (StatusID)1345) && PurifyPvP.CanUse(out act)) return true;
        if (UsePurifyPvP && Use1344PvP && Player.HasStatus(true, (StatusID)1344) && PurifyPvP.CanUse(out act)) return true;
        if (UsePurifyPvP && Use1347PvP && Player.HasStatus(true, (StatusID)1347) && PurifyPvP.CanUse(out act)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        #region PvP
        act = null;
        if (Player.HasStatus(true, StatusID.Overheated)) return false;

        if (BishopAutoturretPvP.CanUse(out act, skipClippingCheck: true, usedUp: true, skipAoeCheck: true)) return true;
        if (BishopAutoturretPvP2.CanUse(out act, skipClippingCheck: true, usedUp: true, skipAoeCheck: true)) return true;

        if (!Player.HasStatus(true, StatusID.Overheated) && !Target.HasStatus(true, StatusID.Guard))
        {
            if (!Player.HasStatus(true, StatusID.Overheated) && WildfirePvP.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;
            
            if (BishopAutoturret_PvP.CanUse(out act, usedUp: true, skipAoeCheck: true) && Target.DistanceToPlayer() <= 25) return true;
            if (!Player.HasStatus(true, StatusID.Overheated) && BishopAutoturretPvP2.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;

            if (!Player.HasStatus(true, StatusID.Overheated) && !Player.HasStatus(true, StatusID.Analysis) && !Player.HasStatus(true, StatusID.BioblasterPrimed) && InCombat &&
                AnalysisPvP.CanUse(out act,CanUseOption.UsedUp)) return true;
        }
        #endregion
        return base.AttackAbility(out act);
    }
}
