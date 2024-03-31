namespace DefaultRotations.Ranged;

[Rotation("LTS's PvP", CombatType.PvP, GameVersion = "6.58")]
[SourceCode(Path = "main/DefaultRotations/Ranged/MCH_Default.PvP.cs")]
public sealed class MCH_DefaultPvP : MachinistRotation
{

    //public static IBaseAction MarksmansSpitePvP { get; } = new BaseAction((ActionID)29415);

    [RotationConfig(CombatType.PvP, Name = "Use Limit Break (Note: RSR cannot predict the future, and this has a cast time.")]
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

    [RotationConfig(CombatType.PvP, Name = "Use Purify")]
    public bool UsePurifyPvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Stun")]
    public bool Use1343PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on DeepFreeze")]
    public bool Use3219PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on HalfAsleep")]
    public bool Use3022PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Sleep")]
    public bool Use1348PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Bind")]
    public bool Use1345PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Heavy")]
    public bool Use1344PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Silence")]
    public bool Use1347PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Stop attacking while in Guard.")]
    public bool GuardCancel { get; set; } = false;

    private bool TryPurify(out IAction? action)
    {
        action = null;
        if (!UsePurifyPvP) return false;

        var purifyStatuses = new Dictionary<int, bool>
        {
            { 1343, Use1343PvP },
            { 3219, Use3219PvP },
            { 3022, Use3022PvP },
            { 1348, Use1348PvP },
            { 1345, Use1345PvP },
            { 1344, Use1344PvP },
            { 1347, Use1347PvP }
        };

        foreach (var status in purifyStatuses)
        {
            if (status.Value && Player.HasStatus(true, (StatusID)status.Key))
            {
                return PurifyPvP.CanUse(out action, skipClippingCheck: true);
            }
        }

        return false;
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        act = null;
        if (GuardCancel && Player.HasStatus(true, StatusID.Guard)) return false;

        if (Player.HasStatus(true, StatusID.Overheated_3149) &&
            HeatBlastPvP.CanUse(out act)) return true;

        if (!Player.HasStatus(true, StatusID.Overheated_3149) && !Target.HasStatus(true, StatusID.Guard))
        {
            act = null;
            if (Player.HasStatus(true, StatusID.Overheated_3149)) return false;


            if (LimitBreakLevel >= 1 && LBInPvP && Target.GetHealthRatio()*100 <= MSValue &&
                MarksmansSpitePvP.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;

            if (!Player.HasStatus(true, StatusID.Overheated_3149) && HostileTarget.DistanceToPlayer() <= 12 &&
                ScattergunPvP.CanUse(out act, skipAoeCheck: true)) return true;
            
            if (!Player.HasStatus(true, StatusID.Overheated_3149) && Player.HasStatus(true, StatusID.BioblasterPrimed) && HostileTarget.DistanceToPlayer() <= 12 &&
                BioblasterPvP.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;

            if (!Player.HasStatus(true, StatusID.Overheated_3149) && Player.HasStatus(true, StatusID.AirAnchorPrimed) && 
                AirAnchorPvP.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;

            if (!Player.HasStatus(true, StatusID.Overheated_3149) && Player.HasStatus(true,StatusID.ChainSawPrimed) &&  
                ChainSawPvP.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;
        }
        if (!Player.HasStatus(true, StatusID.Overheated_3149) && Player.HasStatus(true, StatusID.DrillPrimed) && DrillPvP.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;
        if (!Player.HasStatus(true, StatusID.Overheated_3149) && BlastChargePvP.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;
        if (Player.HasStatus(true, StatusID.Overheated_3149) &&
            HeatBlastPvP.CanUse(out act)) return true;

        if (!Player.HasStatus(true, StatusID.Guard) && UseSprintPvP && !Player.HasStatus(true, StatusID.Sprint) &&
            SprintPvP.CanUse(out act)) return true;
        #endregion

        return base.GeneralGCD(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (UseRecuperatePvP && Player.CurrentHp / Player.MaxHp * 100 < RCValue && RecuperatePvP.CanUse(out act)) return true;

        if (TryPurify(out act)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        #region PvP
        act = null;

        // Use WildfirePvP if Overheated
        if (Player.HasStatus(true, StatusID.Overheated_3149) && WildfirePvP.CanUse(out act, skipAoeCheck: true, skipComboCheck: true, skipClippingCheck: true)) return true;

        // Check if BioblasterPvP, AirAnchorPvP, or ChainSawPvP can be used
        if (InCombat &&
            (BioblasterPvP.CanUse(out act) || AirAnchorPvP.CanUse(out act) || ChainSawPvP.CanUse(out act)) &&
            AnalysisPvP.CanUse(out act)) return true;

        #endregion
        return base.AttackAbility(out act);
    }
}
