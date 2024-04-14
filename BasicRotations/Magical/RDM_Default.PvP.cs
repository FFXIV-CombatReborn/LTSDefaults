namespace DefaultRotations.Magical;

[Rotation("LTS's PvP", CombatType.PvP, GameVersion = "6.58")]
public class RDM_DefaultPvP : RedMageRotation
{
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

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (UseRecuperatePvP && Player.CurrentHp / Player.MaxHp * 100 < RCValue && RecuperatePvP.CanUse(out act)) return true;

        if (TryPurify(out act)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected bool DefenseAreaAbility(out IAction? act)
    {
        if (MagickBarrierPvP.CanUse(out act)) return true;
        if (FrazzlePvP.CanUse(out act)) return true;
        return base.DefenseAreaGCD(out act);
    }

    protected sealed override bool MoveBackAbility(IAction nextGCD, out IAction? act)
    {
        if (DisplacementPvP.CanUse(out act)) return true;
        return base.MoveBackAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        #region PvP
        act = null;
        if (GuardCancel && Player.HasStatus(true, StatusID.Guard)) return false;
        //if (BlackShiftPvP.CanUse(out act)) return true;
        //if (WhiteShiftPvP.CanUse(out act)) return true;

        #endregion
        return base.AttackAbility(nextGCD, out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        act = null;

        // Early exits for Guard status or Sprint usage
        if (GuardCancel && Player.HasStatus(true, StatusID.Guard)) return false;
        if (!Player.HasStatus(true, StatusID.Guard) && UseSprintPvP && !Player.HasStatus(true, StatusID.Sprint) && !InCombat && SprintPvP.CanUse(out act)) return true;

        //White Magic
        if (EnchantedRipostePvP.CanUse(out act)) return true;
        if (EnchantedZwerchhauPvP.CanUse(out act)) return true;
        if (EnchantedRedoublementPvP.CanUse(out act)) return true;
        if (VerholyPvP.CanUse(out act)) return true;
        if (ResolutionPvP.CanUse(out act)) return true;

        //Black Magic
        if (EnchantedRipostePvP_29692.CanUse(out act)) return true;
        if (EnchantedZwerchhauPvP_29693.CanUse(out act)) return true;
        if (EnchantedRedoublementPvP_29694.CanUse(out act)) return true;
        if (VerflarePvP.CanUse(out act)) return true;
        if (ResolutionPvP_29696.CanUse(out act)) return true;

        //White Magic
        if (VerstonePvP.CanUse(out act)) return true;
        if (VeraeroIiiPvP.CanUse(out act)) return true;

        //Black Magic
        if (VerfirePvP.CanUse(out act)) return true;
        if (VerthunderIiiPvP.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }
}
