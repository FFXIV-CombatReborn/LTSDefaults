namespace DefaultRotations.Healer;

[RotationDesc(ActionID.Divination)]
[SourceCode(Path = "main/DefaultRotations/Healer/AST_Default.cs")]
public sealed class AST_Default : AST_Base
{
    #region General rotation info
    public override string GameVersion => VERSION;
    public override string RotationName => $"{USERNAME}'s {ClassJob.Abbreviation} [{Type}]";
    public override CombatType Type => CombatType.PvE;

    private const string
    UseEarthlyStarTime = "UseEarthlyStarTime",
    SpeedLightUsage = "SpeedLightUsage";

    #endregion General rotation info

    #region Rotation Configs
    protected override IRotationConfigSet CreateConfiguration()
        => base.CreateConfiguration()
            .SetFloat(RotationSolver.Basic.Configuration.ConfigUnitType.Seconds, CombatType.PvE, "UseEarthlyStarTime", 15, "Use Earthly Star during countdown timer.", 4, 20);
    #endregion

    #region Countdown logic

    #endregion

    #region GCD Logic

    #endregion

    #region oGCD Logic

    #endregion

    #region Extra Methods

    #endregion

    static IBaseAction AspectedBeneficDefense { get; } = new BaseAction(ActionID.AspectedBenefic, ActionOption.Hot)
    {
        ChoiceTarget = TargetFilter.FindAttackedTarget,
        ActionCheck = (b, m) => b.IsJobCategory(JobRole.Tank),
        TargetStatus = new StatusID[] { StatusID.AspectedBenefic },
    };

    protected override IAction CountDownAction(float remainTime)
    {
        if (remainTime < Malefic.CastTime + CountDownAhead
            && Malefic.CanUse(out var act, CanUseOption.IgnoreClippingCheck)) return act;
        if (remainTime < 3 && UseBurstMedicine(out act)) return act;
        if (remainTime < 4 && AspectedBeneficDefense.CanUse(out act, CanUseOption.IgnoreClippingCheck)) return act;
        if (remainTime < Configs.GetFloat(UseEarthlyStarTime)
            && EarthlyStar.CanUse(out act, CanUseOption.IgnoreClippingCheck)) return act;
        if (remainTime < 30 && Draw.CanUse(out act, CanUseOption.IgnoreClippingCheck)) return act;

        return base.CountDownAction(remainTime);
    }

    [RotationDesc(ActionID.CelestialIntersection, ActionID.Exaltation)]
    protected override bool DefenseSingleAbility(out IAction act)
    {
        if (CelestialIntersection.CanUse(out act, CanUseOption.EmptyOrSkipCombo)
            && CelestialIntersection.Target.WillStatusEnd(0, true, Exaltation.TargetStatus)) return true;
        if (Exaltation.CanUse(out act)
            && Exaltation.Target.WillStatusEnd(0, true, CelestialIntersection.TargetStatus)) return true;  
        return base.DefenseSingleAbility(out act);
    }

    [RotationDesc(ActionID.Macrocosmos)]
    protected override bool DefenseAreaGCD(out IAction act)
    {
        if (Player.WillStatusEnd(0, true, StatusID.CollectiveUnconscious)
            && Macrocosmos.CanUse(out act)) return true;
        return base.DefenseAreaGCD(out act);
    }

    [RotationDesc(ActionID.CollectiveUnconscious)]
    protected override bool DefenseAreaAbility(out IAction act)
    {
        if (Player.WillStatusEnd(0, true, StatusID.Macrocosmos)
            && CollectiveUnconscious.CanUse(out act)) return true;
        return base.DefenseAreaAbility(out act);
    }

    protected override bool GeneralGCD(out IAction act)
    {
        if (NotInCombatDelay && NumberOfAllHostilesInMaxRange > 0
            && AspectedBeneficDefense.CanUse(out act)
            && !AspectedBeneficDefense.Target.InCombat()) return true;

        if (Gravity.CanUse(out act)) return true;

        if (Combust.CanUse(out act)) return true;
        if (Malefic.CanUse(out act)) return true;
        if (Combust.CanUse(out act, CanUseOption.MustUse)) return true;

        return base.GeneralGCD(out act);
    }

    [RotationDesc(ActionID.AspectedHelios, ActionID.Helios)]
    protected override bool HealAreaGCD(out IAction act)
    {
        if (AspectedHelios.CanUse(out act)) return true;
        if (Helios.CanUse(out act)) return true;
        return base.HealAreaGCD(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
    {
        if (base.EmergencyAbility(nextGCD, out act)) return true;

        if (PartyHealers.Count() == 1 && Player.HasStatus(false, StatusID.Silence)
            && HasHostilesInRange && EchoDrops.CanUse(out act)) return true;

        if (!InCombat) return false;

        if (nextGCD.IsTheSameTo(true, AspectedHelios, Helios))
        {
            if (Horoscope.CanUse(out act)) return true;
            if (NeutralSect.CanUse(out act)) return true;
        }

        if (nextGCD.IsTheSameTo(true, Benefic, Benefic2, AspectedBenefic))
        {
            if (Synastry.CanUse(out act)) return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool GeneralAbility(out IAction act)
    {
        if (Draw.CanUse(out act)) return true;
        if (Redraw.CanUse(out act)) return true;
        return base.GeneralAbility(out act);
    }

    [RotationDesc(ActionID.AspectedBenefic, ActionID.Benefic2, ActionID.Benefic)]
    protected override bool HealSingleGCD(out IAction act)
    {
        if (AspectedBenefic.CanUse(out act)
            && (IsMoving || AspectedBenefic.Target.GetHealthRatio() > 0.4)) return true;
        if (Benefic2.CanUse(out act)) return true;
        if (Benefic.CanUse(out act)) return true;
        return base.HealSingleGCD(out act);
    }

    protected override bool AttackAbility(out IAction act)
    {
        if (IsBurst && !IsMoving && Divination.CanUse(out act)) return true;

        if (Lightspeed.CanUse(out act))
        {
            switch (Configs.GetCombo(SpeedLightUsage))
            {
                case 0: // In Burst
                    if (!Player.WillStatusEndGCD(0, 0, true, StatusID.Divination)) return true;
                    break;
                case 1: // When Moving
                    if (IsMoving) return true;
                    break;
                case 2:// When cool down
                    return true;
            }
        }

        if (MinorArcana.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
        if (Draw.CanUse(out act, IsBurst ? CanUseOption.EmptyOrSkipCombo : CanUseOption.None)) return true;

        if (!IsMoving)
        {
            if (!Player.HasStatus(true, StatusID.EarthlyDominance, StatusID.GiantDominance))
            {
                if (EarthlyStar.CanUse(out act, CanUseOption.MustUse)) return true;
            }
            if (Astrodyne.CanUse(out act)) return true;
        }

        if (DrawnCrownCard == CardType.LORD || MinorArcana.WillHaveOneChargeGCD(1, 0))
        {
            if (MinorArcana.CanUse(out act, CanUseOption.MustUse)) return true;
        }

        if (Redraw.CanUse(out act)) return true;
        if (PlayCard(out act)) return true;

        return base.AttackAbility(out act);
    }

    [RotationDesc(ActionID.EssentialDignity, ActionID.CelestialIntersection, ActionID.CelestialOpposition,
        ActionID.EarthlyStar, ActionID.Horoscope)]
    protected override bool HealSingleAbility(out IAction act)
    {
        if (EssentialDignity.CanUse(out act)) return true;
        if (CelestialIntersection.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;

        if (DrawnCrownCard == CardType.LADY && MinorArcana.CanUse(out act, CanUseOption.MustUse)) return true;

        var tank = PartyTanks;
        var isBoss = Malefic.Target?.IsBossFromTTK() ?? false;
        if (EssentialDignity.IsCoolingDown && tank.Count() == 1 && tank.Any(t => t.GetHealthRatio() < 0.5) && !isBoss)
        {
            if (CelestialOpposition.CanUse(out act)) return true;

            if (Player.HasStatus(true, StatusID.GiantDominance))
            {
                act = EarthlyStar;
                return true;
            }

            if (!Player.HasStatus(true, StatusID.HoroscopeHelios, StatusID.Horoscope) && Horoscope.CanUse(out act)) return true;
            if (Player.HasStatus(true, StatusID.HoroscopeHelios) && Horoscope.CanUse(out act)) return true;
            if (tank.Any(t => t.GetHealthRatio() < 0.3) && Horoscope.CanUse(out act)) return true;
        }

        return base.HealSingleAbility(out act);
    }

    [RotationDesc(ActionID.CelestialOpposition, ActionID.EarthlyStar, ActionID.Horoscope)]
    protected override bool HealAreaAbility(out IAction act)
    {
        if (CelestialOpposition.CanUse(out act)) return true;

        if (Player.HasStatus(true, StatusID.GiantDominance))
        {
            act = EarthlyStar;
            return true;
        }
        if (Player.HasStatus(true, StatusID.HoroscopeHelios) && Horoscope.CanUse(out act)) return true;

        if (DrawnCrownCard == CardType.LADY && MinorArcana.CanUse(out act, CanUseOption.MustUse)) return true;

        return base.HealAreaAbility(out act);
    }
}
