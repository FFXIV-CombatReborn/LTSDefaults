namespace ExtraRotations.Magical;

[SourceCode(Path = "main/DefaultRotations/Magical/BLU_Extra.cs")]
public sealed class BLU_Extra : BLU_Base
{
    #region General rotation info
    public override string GameVersion => VERSION;
    public override string RotationName => $"{USERNAME}'s {ClassJob.Abbreviation} [{Type}]";
    public override CombatType Type => CombatType.PvE;
    #endregion General rotation info

    #region Rotation Configs

    #endregion

    #region Countdown logic

    #endregion

    #region GCD Logic

    #endregion

    #region oGCD Logic

    #endregion

    #region Extra Methods

    #endregion

    protected override bool AttackAbility(out IAction act)
    {
        act = null;
        return false;
    }

    protected override bool GeneralGCD(out IAction act)
    {
        if (ChocoMeteor.CanUse(out act)) return true;
        if (DrillCannons.CanUse(out act)) return true;

        if (TripleTrident.OnSlot && TripleTrident.RightType && TripleTrident.WillHaveOneChargeGCD(OnSlotCount(Whistle, Tingle), 0))
        {
            if ((TripleTrident.CanUse(out _, CanUseOption.MustUse) || !HasHostilesInRange) && Whistle.CanUse(out act)) return true;

            if (!Player.HasStatus(true, StatusID.Tingling)
                && Tingle.CanUse(out act, CanUseOption.MustUse)) return true;
            if (OffGuard.CanUse(out act)) return true;

            if (TripleTrident.CanUse(out act, CanUseOption.MustUse)) return true;
        }
        if (ChocoMeteor.CanUse(out act, HasCompanion ? CanUseOption.MustUse : CanUseOption.None)) return true;

        if (SonicBoom.CanUse(out act)) return true;
        if (DrillCannons.CanUse(out act, CanUseOption.MustUse)) return true;

        return false;
    }
}
