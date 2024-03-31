namespace DefaultRotations.Ranged;

[Rotation("LTS's Default", CombatType.PvE, GameVersion = "6.58")]
[SourceCode(Path = "main/DefaultRotations/Ranged/MCH_Default.cs")]
public sealed class MCH_Default : MachinistRotation
{
    // A configuration property to toggle the use of Reassemble with ChainSaw in the rotation.
    [RotationConfig(CombatType.PvE, Name = "Use Reassamble with ChainSaw")]
    public bool MCH_Reassemble { get; set; } = true;
    
    // Defines logic for actions to take during the countdown before combat starts.
    #region Countdown logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < 2 && UseBurstMedicine(out var act)) return act;
        return base.CountDownAction(remainTime);
    }
    #endregion

    // Defines the general logic for determining which global cooldown (GCD) action to take.
    #region GCD Logic
    protected override bool GeneralGCD(out IAction? act)
    {
        // Checks and executes AutoCrossbow or HeatBlast if conditions are met (overheated state).
        if (AutoCrossbowPvE.CanUse(out act)) return true;
        if (HeatBlastPvE.CanUse(out act)) return true;

        // Executes Bioblaster, and then checks for AirAnchor or HotShot, and Drill based on availability and conditions.
        if (BioblasterPvE.CanUse(out act)) return true;
        if (!SpreadShotPvE.CanUse(out _))
        {
            if (AirAnchorPvE.CanUse(out act)) return true;
            else if (!AirAnchorPvE.EnoughLevel && HotShotPvE.CanUse(out act)) return true;

            if (DrillPvE.CanUse(out act)) return true;
        }
        
        // Special condition for using ChainSaw outside of AoE checks if no action is chosen within 4 GCDs.
        if (!CombatElapsedLessGCD(4) && ChainSawPvE.CanUse(out act, skipAoeCheck: true)) return true;

        // AoE actions: ChainSaw and SpreadShot based on their usability.
        if (ChainSawPvE.CanUse(out act)) return true;
        if (SpreadShotPvE.CanUse(out act)) return true;

        // Single target actions: CleanShot, SlugShot, and SplitShot based on their usability.
        if (CleanShotPvE.CanUse(out act)) return true;
        if (SlugShotPvE.CanUse(out act)) return true;
        if (SplitShotPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }
    #endregion

    // Logic for using attack abilities outside of GCD, focusing on burst windows and cooldown management.
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
        
        // Skips further actions if the combat elapsed time is less than 8 seconds.
        if (CombatElapsedLess(8)) return false;
        
        // Prioritizes Ricochet and Gauss Round based on their current charges.
        if (GaussRoundPvE.Cooldown.CurrentCharges <= RicochetPvE.Cooldown.CurrentCharges)
        {
            if (RicochetPvE.CanUse(out act, skipClippingCheck:true, skipAoeCheck: true, usedUp: true)) return true;
        }
        if (GaussRoundPvE.CanUse(out act, skipClippingCheck: true, skipAoeCheck: true, usedUp: true)) return true;

        return base.AttackAbility(out act);
    }

    // Determines emergency actions to take based on the next planned GCD action.
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (MCH_Reassemble && ChainSawPvE.EnoughLevel && nextGCD.IsTheSameTo(true, ChainSawPvE))
        {
            if (ReassemblePvE.CanUse(out act, skipComboCheck: true)) return true;
        }
        
        // Attempts to use Ricochet and Gauss Round based on their conditions.
        if (RicochetPvE.CanUse(out act, skipClippingCheck: true, usedUp: true, skipAoeCheck: true)) return true;
        if (GaussRoundPvE.CanUse(out act, skipClippingCheck: true, usedUp: true, skipAoeCheck: true)) return true;
        
        // Uses Reassemble for if Drill is not of sufficient level.
        if (!DrillPvE.EnoughLevel && nextGCD.IsTheSameTo(true, CleanShotPvE)
            || nextGCD.IsTheSameTo(false, AirAnchorPvE, ChainSawPvE, DrillPvE))
        {
            if (ReassemblePvE.CanUse(out act, skipComboCheck: true)) return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }
    #endregion
    
    // Extra private helper methods for determining the usability of specific abilities under certain conditions.
    // These methods simplify the main logic by encapsulating specific checks related to abilities' cooldowns and prerequisites.
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
