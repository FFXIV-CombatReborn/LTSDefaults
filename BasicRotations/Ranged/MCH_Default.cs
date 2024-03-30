namespace DefaultRotations.Ranged;

[Rotation("LTS's Default", CombatType.PvE, GameVersion = "6.58")]
[SourceCode(Path = "main/DefaultRotations/Ranged/MCH_Default.cs")]
public sealed class MCH_Default : MachinistRotation
{
    [RotationConfig(CombatType.PvE, Name = "Use Reassamble with ChainSaw")]
    public bool MCH_Reassemble { get; set; } = true;

    #region Countdown logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < 2 && UseBurstMedicine(out var act)) return act;
        return base.CountDownAction(remainTime);
    }
    #endregion

    #region GCD Logic
    protected override bool GeneralGCD(out IAction? act)
    {
        //Overheated
        if (AutoCrossbowPvE.CanUse(out act)) return true;
        if (HeatBlastPvE.CanUse(out act)) return true;

        //Long Cds
        if (BioblasterPvE.CanUse(out act)) return true;
        if (!SpreadShotPvE.CanUse(out _))
        {
            if (AirAnchorPvE.CanUse(out act)) return true;
            else if (!AirAnchorPvE.EnoughLevel && HotShotPvE.CanUse(out act)) return true;

            if (DrillPvE.CanUse(out act)) return true;
        }

        if (!CombatElapsedLessGCD(4) && ChainSawPvE.CanUse(out act, skipAoeCheck: true)) return true;

        //Aoe
        if (ChainSawPvE.CanUse(out act)) return true;
        if (SpreadShotPvE.CanUse(out act)) return true;

        //Single
        if (CleanShotPvE.CanUse(out act)) return true;
        if (SlugShotPvE.CanUse(out act)) return true;
        if (SplitShotPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }
    #endregion

    #region oGCD Logic
    protected override bool AttackAbility(out IAction act)
    {
        if (IsBurst)
        {
            if (UseBurstMedicine(out act)) return true;
            if ((IsLastAbility(false, HyperchargePvE) || Heat >= 50) && !CombatElapsedLess(10)
                && WildfirePvE.CanUse(out act, CanUseOption.OnLastAbility)) return true;
        }

        if (!CombatElapsedLess(12) && CanUseHyperchargePvE(out act)) return true;
        if (CanUseRookAutoturretPvE(out act)) return true;

        if (BarrelStabilizerPvE.CanUse(out act)) return true;

        if (CombatElapsedLess(8)) return false;

        if (GaussRoundPvE.Cooldown.CurrentCharges <= RicochetPvE.Cooldown.CurrentCharges)
        {
            if (RicochetPvE.CanUse(out act, skipClippingCheck:true, skipAoeCheck: true, usedUp: true)) return true;
        }
        if (GaussRoundPvE.CanUse(out act, skipClippingCheck: true, skipAoeCheck: true, usedUp: true)) return true;

        return base.AttackAbility(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (MCH_Reassemble && ChainSawPvE.EnoughLevel && nextGCD.IsTheSameTo(true, ChainSawPvE))
        {
            if (ReassemblePvE.CanUse(out act, skipComboCheck: true)) return true;
        }
        if (RicochetPvE.CanUse(out act, skipClippingCheck: true, usedUp: true, skipAoeCheck: true)) return true;
        if (GaussRoundPvE.CanUse(out act, skipClippingCheck: true, usedUp: true, skipAoeCheck: true)) return true;

        if (!DrillPvE.EnoughLevel && nextGCD.IsTheSameTo(true, CleanShotPvE)
            || nextGCD.IsTheSameTo(false, AirAnchorPvE, ChainSawPvE, DrillPvE))
        {
            if (ReassemblePvE.CanUse(out act, skipComboCheck: true)) return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }
    #endregion

    #region Extra Methods
    private bool CanUseRookAutoturretPvE(out IAction? act)
    {
        act = null;
        if (AirAnchorPvE.EnoughLevel)
        {
            if (!AirAnchorPvE.Cooldown.IsCoolingDown || AirAnchorPvE.Cooldown.ElapsedAfter(18)) return false;
        }
        else
        {
            if (!HotShotPvE.Cooldown.IsCoolingDown || HotShotPvE.Cooldown.ElapsedAfter(18)) return false;
        }

        return RookAutoturretPvE.CanUse(out act);
    }

    const float REST_TIME = 6f;
    private bool CanUseHyperchargePvE(out IAction? act)
    {
        act = null;
        //Check recast.
        if (!SpreadShotPvE.CanUse(out _))
        {
            if (AirAnchorPvE.EnoughLevel)
            {
                if (AirAnchorPvE.Cooldown.WillHaveOneCharge(REST_TIME)) return false;
            }
            else
            {
                if (HotShotPvE.EnoughLevel && HotShotPvE.Cooldown.WillHaveOneCharge(REST_TIME)) return false;
            }
        }
        if (DrillPvE.EnoughLevel && DrillPvE.Cooldown.WillHaveOneCharge(REST_TIME)) return false;
        if (ChainSawPvE.EnoughLevel && ChainSawPvE.Cooldown.WillHaveOneCharge(REST_TIME)) return false;

        return HyperchargePvE.CanUse(out act);
    }

    #endregion
}