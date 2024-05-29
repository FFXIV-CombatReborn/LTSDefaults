namespace DefaultRotations.Healer;

[Rotation("Default", CombatType.PvE, GameVersion = "6.58")]
[SourceCode(Path = "main/DefaultRotations/Healer/WHM_Default.cs")]
[Api(1)]
public sealed class WHM_Default :WhiteMageRotation
{
    #region Config Options
    [RotationConfig(CombatType.PvE, Name = "Use spells with cast times to heal. (Ignored if you are the only healer in party)")]
    public bool GCDHeal { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Use Lily at max stacks.")]
    public bool UseLilyWhenFull { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Regen on Tank at 5 seconds remaining on Prepull Countdown.")]
    public bool UsePreRegen { get; set; } = true;

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvE, Name = "Minimum health threshold party member needs to be to use Benediction")]
    public float BenedictionHeal { get; set; } = 0.3f;

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvE, Name = "If a party member's health drops below this percentage, the Regen healing ability will not be used on them")]
    public float RegenHeal { get; set; } = 0.3f;

    [Range(0, 10000, ConfigUnitType.None, 100)]
    [RotationConfig(CombatType.PvE, Name = "Casting cost requirement for Thin Air to be used")]
    public float ThinAirNeed { get; set; } = 1000;
    #endregion

    #region Countdown Logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < StonePvE.Info.CastTime + CountDownAhead
            && StonePvE.CanUse(out var act)) return act;

        if (UsePreRegen && remainTime <= 5 && remainTime > 3)
        {
            if (RegenPvE.CanUse(out act)) return act;
            if (DivineBenisonPvE.CanUse(out act)) return act;
        }
        return base.CountDownAction(remainTime);
    }
    #endregion

    #region oGCD Logic
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (nextGCD is IBaseAction action && action.Info.MPNeed >= ThinAirNeed &&
            ThinAirPvE.CanUse(out act)) return true;

        if (nextGCD.IsTheSameTo(true, AfflatusRapturePvE, MedicaPvE, MedicaIiPvE, CureIiiPvE)
            && (MergedStatus.HasFlag(AutoStatus.HealAreaSpell) || MergedStatus.HasFlag(AutoStatus.HealSingleSpell)))
        {
            if (PlenaryIndulgencePvE.CanUse(out act)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.TemperancePvE, ActionID.LiturgyOfTheBellPvE)]
    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (TemperancePvE.Cooldown.IsCoolingDown && !TemperancePvE.Cooldown.WillHaveOneCharge(100)
            || LiturgyOfTheBellPvE.Cooldown.IsCoolingDown && !LiturgyOfTheBellPvE.Cooldown.WillHaveOneCharge(160)) return false;

        if (TemperancePvE.CanUse(out act)) return true;

        if (LiturgyOfTheBellPvE.CanUse(out act, skipAoeCheck: true)) return true;
        return base.DefenseAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.DivineBenisonPvE, ActionID.AquaveilPvE)]
    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (DivineBenisonPvE.Cooldown.IsCoolingDown && !DivineBenisonPvE.Cooldown.WillHaveOneCharge(15)
            || AquaveilPvE.Cooldown.IsCoolingDown && !AquaveilPvE.Cooldown.WillHaveOneCharge(52)) return false;

        if (DivineBenisonPvE.CanUse(out act)) return true;

        if (AquaveilPvE.CanUse(out act)) return true;
        return base.DefenseSingleAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.AsylumPvE)]
    protected override bool HealAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (AsylumPvE.CanUse(out act)) return true;
        return base.HealAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.BenedictionPvE, ActionID.AsylumPvE, ActionID.DivineBenisonPvE, ActionID.TetragrammatonPvE)]
    protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
    {
        if (BenedictionPvE.CanUse(out act) &&
            RegenPvE.Target.Target?.GetHealthRatio() < BenedictionHeal) return true;

        if (!IsMoving && AsylumPvE.CanUse(out act)) return true;

        if (DivineBenisonPvE.CanUse(out act)) return true;

        if (TetragrammatonPvE.CanUse(out act)) return true;
        return base.HealSingleAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (InCombat)
        {
            if (PresenceOfMindPvE.CanUse(out act)) return true;
            if (AssizePvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic

    [RotationDesc(ActionID.AfflatusRapturePvE, ActionID.MedicaIiPvE, ActionID.CureIiiPvE, ActionID.MedicaPvE)]
    protected override bool HealAreaGCD(out IAction? act)
    {
        if (AfflatusRapturePvE.CanUse(out act)) return true;

        int hasMedica2 = PartyMembers.Count((n) => n.HasStatus(true, StatusID.MedicaIi));

        if (MedicaIiPvE.CanUse(out act) && hasMedica2 < PartyMembers.Count() / 2 && !IsLastAction(true, MedicaIiPvE)) return true;

        if (CureIiiPvE.CanUse(out act)) return true;

        if (MedicaPvE.CanUse(out act)) return true;

        return base.HealAreaGCD(out act);
    }

    [RotationDesc(ActionID.AfflatusSolacePvE, ActionID.RegenPvE, ActionID.CureIiPvE, ActionID.CurePvE)]
    protected override bool HealSingleGCD(out IAction? act)
    {
        if (AfflatusSolacePvE.CanUse(out act)) return true;

        if (RegenPvE.CanUse(out act) && (RegenPvE.Target.Target?.GetHealthRatio() > RegenHeal)) return true;

        if (CureIiPvE.CanUse(out act)) return true;

        if (CurePvE.CanUse(out act)) return true;

        return base.HealSingleGCD(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        //if (NotInCombatDelay && RegenDefense.CanUse(out act)) return true;

        if (AfflatusMiseryPvE.CanUse(out act, skipAoeCheck: true)) return true;

        bool liliesNearlyFull = Lily == 2 && LilyTime > 13;
        bool liliesFullNoBlood = Lily == 3;
        if (UseLilyWhenFull && (liliesNearlyFull || liliesFullNoBlood) && AfflatusMiseryPvE.EnoughLevel && BloodLily < 3)
        {
            if (UseLily(out act)) return true;
        }

        if (HolyPvE.CanUse(out act)) return true;

        if (AeroPvE.CanUse(out act)) return true;

        if (StonePvE.CanUse(out act)) return true;

        if (Lily >= 2)
        {
            if (UseLily(out act)) return true;
        }

        if (AeroPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;

        return base.GeneralGCD(out act);
    }
    #endregion

    #region Extra Methods
    public WHM_Default()
    {
        AfflatusRapturePvE.Setting.RotationCheck = () => BloodLily < 3;
        AfflatusSolacePvE.Setting.RotationCheck = () => BloodLily < 3;
    }
    public override bool CanHealSingleSpell => base.CanHealSingleSpell && (GCDHeal || PartyMembers.GetJobCategory(JobRole.Healer).Count() < 2);
    public override bool CanHealAreaSpell => base.CanHealAreaSpell && (GCDHeal || PartyMembers.GetJobCategory(JobRole.Healer).Count() < 2);

    private bool UseLily(out IAction? act)
    {
        if (AfflatusRapturePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (AfflatusSolacePvE.CanUse(out act)) return true;
        return false;
    }

    /*public override void DisplayStatus()
     {
         ImGui.Text(LilyTime.ToString());
     }*/
    #endregion
}
