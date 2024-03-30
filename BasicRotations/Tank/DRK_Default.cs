namespace DefaultRotations.Tank;

[Rotation("LTS's Default", CombatType.PvE, GameVersion = "6.58")]
[SourceCode(Path = "main/DefaultRotations/Tank/DRK_Balance.cs")]
public sealed class DRK_Default : DarkKnightRotation
{
    // Indicates whether the Dark Knight can heal using a single ability.
    public override bool CanHealSingleAbility => false;
    
    // Determines if currently in a burst phase based on cooldowns of key abilities.
    private bool InTwoMIsBurst()
    {
        if ((BloodWeaponPvE.Cooldown.IsCoolingDown && DeliriumPvE.Cooldown.IsCoolingDown && ((LivingShadowPvE.Cooldown.IsCoolingDown && !(LivingShadowPvE.Cooldown.ElapsedAfter(15))) || !LivingShadowPvE.EnoughLevel))) return true;
        else return false;
    }
    
    // Checks if combat time is less than 3 seconds.
    private static bool CombatLess => CombatElapsedLess(3);

    // Manages DarkSide ability based on several conditions.
    private bool CheckDarkSide
    {
        get
        {
            if (DarkSideEndAfterGCD(3)) return true;

            if (CombatLess) return false;

            if ((InTwoMIsBurst() && HasDarkArts) || (HasDarkArts && Player.HasStatus(true, StatusID.BlackestNight)) || (HasDarkArts && DarkSideEndAfterGCD(3))) return true;

            if ((InTwoMIsBurst() && BloodWeaponPvE.Cooldown.IsCoolingDown && LivingShadowPvE.Cooldown.IsCoolingDown && SaltedEarthPvE.Cooldown.IsCoolingDown && ShadowbringerPvE.Cooldown.CurrentCharges == 0 && CarveAndSpitPvE.Cooldown.IsCoolingDown)) return true;

            if (TheBlackestNight && CurrentMp < 6000) return false;

            return CurrentMp >= 8500;
        }
    }
    
    // Logic to determine when to use blood-based abilities.
    private bool UseBlood
    {
        get
        {
            // Always prioritize Bloodspiller/Quietus during Delirium's active window.
            // Assuming Delirium provides a significant enough window to utilize its benefits fully.
            if (Player.HasStatus(true, StatusID.Delirium))
            {
                // Delirium is active, prioritize using Bloodspiller/Quietus.
                return true;
            }
    
            // Additional logic for when Delirium is not active or not applicable.
            // This includes checking if the Delirium cooldown is almost ready and Blood is nearing cap.
            if (Blood >= 90) return true;
    
            // Your existing conditions can remain as fallback logic when Delirium isn't a factor.
            if (!DeliriumPvE.EnoughLevel) return true;
            if (LivingShadowPvE.Cooldown.IsCoolingDown) return true;
            if (DeliriumPvE.Cooldown.WillHaveOneChargeGCD(3) && !LivingShadowPvE.Cooldown.WillHaveOneChargeGCD(3)) return true;
    
            return false;
        }
    }

    [RotationConfig(CombatType.PvE, Name = "Keep at least 3000 MP")]
    public bool TheBlackestNight { get; set; } = true;

    // Countdown logic to prepare for combat.
    // Includes logic for using Provoke, tank stances, and burst medicines.
    protected override IAction? CountDownAction(float remainTime)
    {
        //Provoke when has Shield.
        if (remainTime <= CountDownAhead)
        {
            if (HasTankStance)
            {
                if (ProvokePvE.CanUse(out _)) return ProvokePvE;
            }
            //else
            //{
            //    if (Unmend.CanUse(out var act1)) return act1;
            //}
        }
        if (remainTime <= 2 && UseBurstMedicine(out var act)) return act;
        if (remainTime <= 3 && TheBlackestNightPvE.CanUse(out act)) return act;
        if (remainTime <= 4 && BloodWeaponPvE.CanUse(out act)) return act;
        return base.CountDownAction(remainTime);
    }
    
    // Decision-making for emergency abilities, focusing on Blood Weapon usage.
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (base.EmergencyAbility(nextGCD, out act)) return true;

        //if ((InCombat && CombatElapsedLess(2) || DataCenter.TimeSinceLastAction.TotalSeconds >= 10) && nextGCD.IsTheSameTo(false, HardSlash, SyphonStrike, Souleater, BloodSpiller, Unmend))
        //if ((InCombat && CombatElapsedLess(2) || DataCenter.TimeSinceLastAction.TotalSeconds >= 10) && Target != null && Target.IsNPCEnemy() && NumberOfHostilesIn(25) == 1)
        if ((InCombat && CombatElapsedLess(2) || TimeSinceLastAction.TotalSeconds >= 10))
        {
            //int[] numbers = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            //foreach (int number in numbers)
            //{
            //    if (BloodWeapon.IsCoolingDown)
            //    {
            //        break;
            //    }

            //    BloodWeapon.CanUse(out act, CanUseOption.MustUse);
            //}
            if (BloodWeaponPvE.CanUse(out act, skipAoeCheck: true)) return true;

        }

        return base.EmergencyAbility(nextGCD, out act);
    }
    
    // Determines healing actions based on The Blackest Night ability.
    [RotationDesc(ActionID.TheBlackestNightPvE)]
    protected override bool HealSingleAbility(out IAction? act)
    {
        if (TheBlackestNightPvE.CanUse(out act)) return true;
        return base.HealSingleAbility(out act);
    }

    [RotationDesc(ActionID.DarkMissionaryPvE, ActionID.ReprisalPvE)]
    protected override bool DefenseAreaAbility(out IAction? act)
    {
        if (!InTwoMIsBurst() && DarkMissionaryPvE.CanUse(out act)) return true;
        if (!InTwoMIsBurst() && ReprisalPvE.CanUse(out act, skipAoeCheck: true)) return true;

        return base.DefenseAreaAbility(out act);
    }

    [RotationDesc(ActionID.TheBlackestNightPvE, ActionID.OblationPvE, ActionID.ReprisalPvE, ActionID.ShadowWallPvE, ActionID.RampartPvE, ActionID.DarkMindPvE)]
    protected override bool DefenseSingleAbility(out IAction? act)
    {
        act = null;

        if (Player.HasStatus(true, StatusID.BlackestNight)) return false;

        //10
        if (OblationPvE.CanUse(out act, usedUp: true, onLastAbility: true)) return true;

        if (ReprisalPvE.CanUse(out act, skipAoeCheck: true, onLastAbility: true)) return true;

        if (TheBlackestNightPvE.CanUse(out act, onLastAbility: true)) return true;
        //30
        if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && ShadowWallPvE.CanUse(out act)) return true;

        //20
        if (ShadowWallPvE.Cooldown.IsCoolingDown && ShadowWallPvE.Cooldown.ElapsedAfter(60) && RampartPvE.CanUse(out act)) return true;
        if (DarkMindPvE.CanUse(out act)) return true;

        return base.DefenseAreaAbility(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        // Prioritize Bloodspiller/Quietus if conditions meet
        if (UseBlood)
        {
            if (NumberOfHostilesInRange >= 3 && QuietusPvE.CanUse(out act))
            {
                return true; // Use Quietus for AoE
            }
            else if (BloodspillerPvE.CanUse(out act))
            {
                return true; // Use Bloodspiller for single-target
            }
        }

        //AOE
        if (StalwartSoulPvE.CanUse(out act)) return true;
        if (UnleashPvE.CanUse(out act)) return true;

        //单体
        if (SouleaterPvE.CanUse(out act)) return true;
        if (SyphonStrikePvE.CanUse(out act)) return true;
        if (HardSlashPvE.CanUse(out act)) return true;

        if (MergedStatus.HasFlag(AutoStatus.MoveForward) && MoveForwardAbility(out act)) return true;
        if (BloodWeaponPvE.Cooldown.IsCoolingDown && !Player.HasStatus(true, StatusID.BloodWeapon) && UnmendPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        //if (InCombat && CombatElapsedLess(2) && BloodWeapon.CanUse(out act)) return true;
        if (CheckDarkSide)
        {
            if (FloodOfDarknessPvE.CanUse(out act)) return true;
            if (EdgeOfDarknessPvE.CanUse(out act)) return true;
        }

        if (IsBurst)
        {
            if (UseBurstMedicine(out act)) return true;
            if (InCombat && DeliriumPvE.CanUse(out act)) return true;
            if (DeliriumPvE.Cooldown.ElapsedAfterGCD(1) && !DeliriumPvE.Cooldown.ElapsedAfterGCD(3) 
                && BloodWeaponPvE.CanUse(out act)) return true;
            if (LivingShadowPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (BloodspillerPvE.CanUse(out act, usedUp: true)) return true;
        }

        if (CombatLess)
        {
            act = null;
            return false;
        }

        if (!IsMoving && SaltedEarthPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (ShadowbringerPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (NumberOfHostilesInRange >= 3 && AbyssalDrainPvE.CanUse(out act)) return true;
        if (CarveAndSpitPvE.CanUse(out act)) return true;

        if (InTwoMIsBurst())
        {
            if (ShadowbringerPvE.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;

        }

        if (PlungePvE.CanUse(out act, skipAoeCheck: true) && !IsMoving) return true;

        if (SaltAndDarknessPvE.CanUse(out act)) return true;

        if (InTwoMIsBurst())
        {
            if (PlungePvE.CanUse(out act, usedUp: true, skipAoeCheck: true) && !IsMoving) return true;
        }

        return base.AttackAbility(out act);
    }
}
