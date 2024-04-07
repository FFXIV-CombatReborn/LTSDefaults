using RotationSolver.Basic.Data;

namespace DefaultRotations.Ranged;

[Rotation("Testing Rotations", CombatType.PvE, GameVersion = "6.58", Description = "Additonal contributions to this rotation thanks to Toshi!")]
[SourceCode(Path = "main/DefaultRotations/Ranged/MCH_Beta.cs")]
public sealed class MCH_Beta : MachinistRotation
{
    #region Countdown logic
    // Defines logic for actions to take during the countdown before combat starts.
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < 2)
        {
            if (UseBurstMedicine(out var act)) return act;
        }
        if (remainTime < 5)
        {
            if (ReassemblePvE.CanUse(out var act)) return act;
        }
        return base.CountDownAction(remainTime);
    }
    #endregion

    #region Emergency Logic
    // Determines emergency actions to take based on the next planned GCD action.
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        // Reassemble Logic
        // Check next GCD action and conditions for Reassemble.
        bool isReassembleUsable =
            //Reassemble current # of charges and double proc protection
            ReassemblePvE.Cooldown.CurrentCharges > 0 && !Player.HasStatus(true, StatusID.Reassembled) &&
            //Chainsaw Level Check and NextGCD Check
            ((ChainSawPvE.EnoughLevel && nextGCD.IsTheSameTo(true, ChainSawPvE)) ||
            //AirAnchor Logic
            (AirAnchorPvE.EnoughLevel && nextGCD.IsTheSameTo(true, AirAnchorPvE)) ||
            //Drill Logic
            (DrillPvE.EnoughLevel && !ChainSawPvE.EnoughLevel && nextGCD.IsTheSameTo(true, DrillPvE)) ||
            //Cleanshot Logic
            (!DrillPvE.EnoughLevel && CleanShotPvE.EnoughLevel && nextGCD.IsTheSameTo(true, CleanShotPvE)) ||
            //HotShot Logic
            (!CleanShotPvE.EnoughLevel && nextGCD.IsTheSameTo(true, HotShotPvE)));

        // Attempt to use Reassemble if it's ready
        if (isReassembleUsable)
        {
            if (ReassemblePvE.CanUse(out act, onLastAbility: true, skipClippingCheck: true, skipComboCheck: true, usedUp: true)) return true;
        }

        // Prioritizes Ricochet and Gauss Round based on their current charges.
        if (GaussRoundPvE.Cooldown.CurrentCharges <= RicochetPvE.Cooldown.CurrentCharges && RicochetPvE.CanUse(out act, skipClippingCheck: true, skipAoeCheck: true, usedUp: true))
        {
            return true;
        }

        // Use GaussRound if it's available, regardless of Ricochet's status.
        else if (GaussRoundPvE.CanUse(out act, skipClippingCheck: true, skipAoeCheck: true, usedUp: true))
        {
            return true;
        }

        return base.EmergencyAbility(nextGCD, out act);

    }
    #endregion

    #region oGCD Logic
    // Logic for using attack abilities outside of GCD, focusing on burst windows and cooldown management.
    protected override bool AttackAbility(out IAction? act)
    {
        if (IsBurst)
        {
            if (UseBurstMedicine(out act)) return true;

            {
                if ((IsLastAbility(false, HyperchargePvE) || Heat >= 50) && !CombatElapsedLess(10) && CanUseHyperchargePvE(out _)
                && WildfirePvE.CanUse(out act, onLastAbility: true, skipClippingCheck: true, skipComboCheck: true)) return true;
            }
        }

        if (!CombatElapsedLess(12) && (!WildfirePvE.Cooldown.WillHaveOneCharge(30) || (Heat == 100)))
        {
            if (!CombatElapsedLess(12) && CanUseHyperchargePvE(out act)) return true;
        }
        if (CanUseRookAutoturretPvE(out act)) return true;

        if (BarrelStabilizerPvE.CanUse(out act)) return true;

        // Skips further actions if the combat elapsed time is less than 8 seconds.
        if (CombatElapsedLess(8)) return false;

        return base.AttackAbility(out act);
    }
    #endregion

    #region GCD Logic
    // Defines the general logic for determining which global cooldown (GCD) action to take.
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

    #region Extra Methods
    // Extra private helper methods for determining the usability of specific abilities under certain conditions.
    // These methods simplify the main logic by encapsulating specific checks related to abilities' cooldowns and prerequisites.
    private bool CanUseRookAutoturretPvE(out IAction? act)
    {
        act = null;

        // 
        if ((AirAnchorPvE.EnoughLevel && (!AirAnchorPvE.Cooldown.IsCoolingDown || AirAnchorPvE.Cooldown.ElapsedAfter(18))) ||
           (!AirAnchorPvE.EnoughLevel && (!HotShotPvE.Cooldown.IsCoolingDown || HotShotPvE.Cooldown.ElapsedAfter(18))))
        {
            return false;
        }

        // Use Rook Auto Turret
        return RookAutoturretPvE.CanUse(out act);
    }


    // Logic for Hypercharge
    private bool CanUseHyperchargePvE(out IAction? act)
    {
        float REST_TIME = 6f;
        if
                     //Cannot AOE
                     ((!SpreadShotPvE.CanUse(out _))
                     &&
                     ////Combat elapsed 12 seconds
                     //(!CombatElapsedLess(12))
                     //&&
                     // AirAnchor Enough Level % AirAnchor 
                     ((AirAnchorPvE.EnoughLevel && AirAnchorPvE.Cooldown.WillHaveOneCharge(REST_TIME))
                     ||
                     // HotShot Charge Detection
                     (!AirAnchorPvE.EnoughLevel && HotShotPvE.EnoughLevel && HotShotPvE.Cooldown.WillHaveOneCharge(REST_TIME))
                     ||
                     // Drill Charge Detection
                     (DrillPvE.EnoughLevel && DrillPvE.Cooldown.WillHaveOneCharge(REST_TIME))
                     ||
                     // Chainsaw Charge Detection
                     (ChainSawPvE.EnoughLevel && ChainSawPvE.Cooldown.WillHaveOneCharge(REST_TIME))))
        {
            act = null;
            return false;
        }
        else
        {
            // Use Hypercharge
            return HyperchargePvE.CanUse(out act);
        }
    }

    #endregion
}
