﻿namespace DefaultRotations.Ranged;

[Rotation("Testing Rotations", CombatType.PvE, GameVersion = "6.58", Description = "Additonal contributions to this rotation thanks to Toshi!")]
[SourceCode(Path = "main/DefaultRotations/Ranged/DNC_Beta.cs")]
public sealed class DNC_Beta : DancerRotation
{
    // Override the method for actions to be taken during countdown phase of combat
    protected override IAction? CountDownAction(float remainTime)
    {
        // If there are 15 or fewer seconds remaining in the countdown
        if (remainTime <= 15)
        {
            // Attempt to use Standard Step if applicable
            if (StandardStepPvE.CanUse(out var act, skipAoeCheck: true)) return act;
            // Fallback to executing step GCD action if Standard Step is not used
            if (ExecuteStepGCD(out act)) return act;
        }
        // If none of the above conditions are met, fallback to the base class method
        return base.CountDownAction(remainTime);
    }

    // Override the method for handling emergency abilities
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        // Special handling if the last action was Quadruple Technical Finish and level requirement is met
        if (IsLastAction(ActionID.QuadrupleTechnicalFinishPvE) && TechnicalStepPvE.EnoughLevel)
        {
            // Attempt to use Devilment ignoring clipping checks
            if (DevilmentPvE.CanUse(out act, skipClippingCheck: true)) return true;
        }
        // Similar handling for Double Standard Finish when level requirement is not met
        else if (IsLastAction(ActionID.DoubleStandardFinishPvE) && !TechnicalStepPvE.EnoughLevel)
        {
            if (DevilmentPvE.CanUse(out act, skipClippingCheck: true)) return true;
        }

        // If currently dancing, defer to the base class emergency handling
        if (IsDancing)
        {
            return base.EmergencyAbility(nextGCD, out act);
        }

        // Use burst medicine if cooldown for Technical Step has elapsed sufficiently
        if (TechnicalStepPvE.Cooldown.ElapsedAfter(115)
            && UseBurstMedicine(out act)) return true;

        // Attempt to use Fan Dance III if available
        if (FanDanceIiiPvE.CanUse(out act, skipAoeCheck: true)) return true;

        // Fallback to base class method if none of the above conditions are met
        return base.EmergencyAbility(nextGCD, out act);
    }

    // Override the method for handling attack abilities
    protected override bool AttackAbility(out IAction? act)
    {
        act = null;

        // If currently in the middle of a dance, no attack ability should be executed
        if (IsDancing) return false;

        // Logic for using Fan Dance abilities based on certain conditions
        if ((Player.HasStatus(true, StatusID.Devilment) || Feathers > 3 || !TechnicalStepPvE.EnoughLevel) && !FanDanceIiiPvE.CanUse(out act, skipAoeCheck: true))
        {
            if (FanDancePvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (FanDanceIiPvE.CanUse(out act)) return true;
        }

        // Check for conditions to use Flourish
        if (((Player.HasStatus(true, StatusID.Devilment)) && (Player.HasStatus(true, StatusID.TechnicalFinish))) || ((!Player.HasStatus(true, StatusID.Devilment)) && (!Player.HasStatus(true, StatusID.TechnicalFinish))))
        {
            if (!Player.HasStatus(true, StatusID.ThreefoldFanDance) && FlourishPvE.CanUse(out act))
            {
                return true;
            }
        }

        // Attempt to use Fan Dance IV if available
        if (FanDanceIvPvE.CanUse(out act, skipAoeCheck: true)) return true;

        // Attempt to use Closed Position if applicable
        if (UseClosedPosition(out act)) return true;

        // Fallback to base class attack ability method if none of the above conditions are met
        return base.AttackAbility(out act);
    }

    // Override the method for handling general Global Cooldown (GCD) actions
    protected override bool GeneralGCD(out IAction? act)
    {
        act = null;
        // If not in combat and lacking the Closed Position status, attempt to use Closed Position
        if (!InCombat && !Player.HasStatus(true, StatusID.ClosedPosition) && ClosedPositionPvE.CanUse(out act)) return true;

        // Attempt to execute Dance Finish GCD or Step GCD actions
        if (FinishTheDance(out act)) return true;
        if (ExecuteStepGCD(out act)) return true;

        // Attempt to use Technical Step in burst mode and if in combat
        if (IsBurst && InCombat && TechnicalStepPvE.CanUse(out act, skipAoeCheck: true)) return true;

        // Delegate to AttackGCD method to handle attack actions during GCD
        if (AttackGCD(out act, Player.HasStatus(true, StatusID.Devilment))) return true;

        // Fallback to base class general GCD method if none of the above conditions are met
        return base.GeneralGCD(out act);
    }

    // Helper method to handle attack actions during GCD based on certain conditions
    private bool AttackGCD(out IAction? act, bool burst)
    {
        act = null;

        // Prevent action if currently dancing or holding too many feathers
        if (IsDancing || Feathers > 3) return false;

        // Logic for using Saber Dance and Starfall Dance based on burst mode or Esprit levels
        if ((burst || Esprit >= 85) && SaberDancePvE.CanUse(out act, skipAoeCheck: true)) return true;

        // Additional logic for using Tillana and Standard Step based on various checks
        if (!DevilmentPvE.CanUse(out act, skipComboCheck: true))
        {
            if (TillanaPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (StarfallDancePvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (UseStandardStep(out act)) return true;

        // Attempt to use various dance moves based on availability and conditions
        if (BloodshowerPvE.CanUse(out act)) return true;
        if (FountainfallPvE.CanUse(out act)) return true;
        if (RisingWindmillPvE.CanUse(out act)) return true;
        if (ReverseCascadePvE.CanUse(out act)) return true;
        if (BladeshowerPvE.CanUse(out act)) return true;
        if (WindmillPvE.CanUse(out act)) return true;
        if (FountainPvE.CanUse(out act)) return true;
        if (CascadePvE.CanUse(out act)) return true;

        // Return false if no action is determined to be taken
        return false;
    }

    private bool UseStandardStep(out IAction act)
    {
        // Attempt to use Standard Step if available and certain conditions are met
        if (!StandardStepPvE.CanUse(out act, skipAoeCheck: true)) return false;
        if (Player.WillStatusEndGCD(2, 0, true, StatusID.StandardFinish)) return true;

        // Check for hostiles in range and technical step conditions
        if (!HasHostilesInRange) return false;
        if (Player.HasStatus(true, StatusID.TechnicalFinish) && Player.WillStatusEndGCD(2, 0, true, StatusID.TechnicalFinish) || TechnicalStepPvE.Cooldown.IsCoolingDown && TechnicalStepPvE.Cooldown.WillHaveOneChargeGCD(2)) return false;

        return true;
    }

    // Helper method to decide usage of Closed Position based on specific conditions
    private bool UseClosedPosition(out IAction act)
    {
        // Attempt to use Closed Position if available and certain conditions are met
        if (!ClosedPositionPvE.CanUse(out act)) return false;

        if (InCombat && Player.HasStatus(true, StatusID.ClosedPosition))
        {
            // Check for party members with Closed Position status
            foreach (var friend in PartyMembers)
            {
                if (friend.HasStatus(true, StatusID.ClosedPosition_2026))
                {
                    // Use Closed Position if target is not the same as the friend with the status
                    if (ClosedPositionPvE.Target.Target != friend) return true;
                    break;
                }
            }
        }
        return false;
    }
    private bool FinishTheDance(out IAction? act)
    {
        bool areDanceTargetsInRange = AllHostileTargets.Any(hostile => hostile.DistanceToPlayer() < 14);

        // Check for Standard Step if targets are in range or status is about to end.
        if (Player.HasStatus(true, StatusID.StandardStep) && CompletedSteps == 2 &&
            (areDanceTargetsInRange || Player.WillStatusEnd(1f, true, StatusID.StandardStep)) &&
            DoubleStandardFinishPvE.CanUse(out act, skipAoeCheck: true))
        {
            return true;
        }

        // Check for Technical Step if targets are in range or status is about to end.
        if (Player.HasStatus(true, StatusID.TechnicalStep) && CompletedSteps == 4 &&
            (areDanceTargetsInRange || Player.WillStatusEnd(1f, true, StatusID.TechnicalStep)) &&
            QuadrupleTechnicalFinishPvE.CanUse(out act, skipAoeCheck: true))
        {
            return true;
        }

        act = null;
        return false;
    }

}
