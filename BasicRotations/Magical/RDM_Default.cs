namespace DefaultRotations.Magical;

[Rotation("LTS's Default", CombatType.PvE, GameVersion = "6.58")]
[SourceCode(Path = "main/DefaultRotations/Magical/RDM_Default.cs")]
public sealed class RDM_Default : RedMageRotation
{
    private static BaseAction VerthunderStartUp { get; } = new BaseAction(ActionID.VerthunderPvE, false);

    private bool CanStartMeleeCombo
    {
        get
        {
            // Check for direct qualifications for starting melee combo
            if (Player.HasStatus(true, StatusID.Manafication, StatusID.Embolden) ||
                BlackMana == 100 || WhiteMana == 100) 
                return true;

            if (BlackMana != WhiteMana)
            {
                if ((WhiteMana < BlackMana && !Player.HasStatus(true, StatusID.VerstoneReady)) ||
                    (BlackMana < WhiteMana && !Player.HasStatus(true, StatusID.VerfireReady)))
                    return true;
            }

            // Prevent starting combo under specific conditions
            if (Player.HasStatus(true, VercurePvE.Setting.StatusProvide ?? new StatusID[0]) ||
                (EmboldenPvE.EnoughLevel && EmboldenPvE.Cooldown.WillHaveOneChargeGCD(5)))
                return false;

            return true;
        }
    }

    [RotationConfig(CombatType.PvE, Name = "Use Vercure for Dualcast when out of combat.")]
    public bool UseVercure { get; set; }

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < VerthunderStartUp.Info.CastTime + CountDownAhead &&
            VerthunderStartUp.CanUse(out var act))
            return act;

        // Remove Swift, Acceleration, and Dualcast statuses individually
        StatusHelper.StatusOff(StatusID.Dualcast);
        StatusHelper.StatusOff(StatusID.Acceleration);
        StatusHelper.StatusOff(StatusID.Swiftcast);

        return base.CountDownAction(remainTime);
    }


    protected override bool GeneralGCD(out IAction? act)
    {
        act = null;
        if (ManaStacks == 3) return false;

        // Prioritize actions based on conditions
        if (!VerthunderIiPvE.CanUse(out _))
        {
            if (VerfirePvE.CanUse(out act) || VerstonePvE.CanUse(out act)) return true;
        }

        if (ScatterPvE.CanUse(out act) ||
            (WhiteMana < BlackMana && 
             (VeraeroIiPvE.CanUse(out act) && BlackMana - WhiteMana != 5) ||
             (VeraeroPvE.CanUse(out act) && BlackMana - WhiteMana != 6)) ||
            VerthunderIiPvE.CanUse(out act) ||
            VerthunderPvE.CanUse(out act) ||
            JoltPvE.CanUse(out act) ||
            (UseVercure && NotInCombatDelay && VercurePvE.CanUse(out act)))
            return true;

        return base.GeneralGCD(out act);
    }

    protected override bool EmergencyGCD(out IAction? act)
    {
        act = null;
        if (ManaStacks == 3)
        {
            if (BlackMana > WhiteMana ? VerholyPvE.CanUse(out act, true) : VerflarePvE.CanUse(out act, true))
                return true;
        }

        if (ResolutionPvE.CanUse(out act, true) || ScorchPvE.CanUse(out act, true) ||
            (IsLastGCD(true, MoulinetPvE) && MoulinetPvE.CanUse(out act, true)) ||
            ZwerchhauPvE.CanUse(out act) || RedoublementPvE.CanUse(out act) ||
            (CanStartMeleeCombo && ((MoulinetPvE.CanUse(out act) && BlackMana >= 60 && WhiteMana >= 60) ||
                                    (BlackMana >= 50 && WhiteMana >= 50 && RipostePvE.CanUse(out act))) ||
            (ManaStacks > 0 && RipostePvE.CanUse(out act))))
            return true;

        return base.EmergencyGCD(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (CombatElapsedLess(4)) return false;

        if (IsBurst && HasHostilesInRange && EmboldenPvE.CanUse(out act, true)) return true;

        if ((Player.HasStatus(true, StatusID.Embolden) || IsLastAbility(ActionID.EmboldenPvE)) &&
            ManaficationPvE.CanUse(out act)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        act = null;
        // Swift action logic simplified
        if (ManaStacks == 0 && (BlackMana < 50 || WhiteMana < 50) &&
            (CombatElapsedLess(4) || !ManaficationPvE.EnoughLevel || !ManaficationPvE.Cooldown.WillHaveOneChargeGCD(0, 1)) &&
            InCombat && !Player.HasStatus(true, StatusID.VerfireReady, StatusID.VerstoneReady) &&
            (SwiftcastPvE.CanUse(out act) || AccelerationPvE.CanUse(out act, true)))
            return true;

        if (IsBurst && UseBurstMedicine(out act)) return true;

        // Attack abilities in a simplified manner
        if (ContreSixtePvE.CanUse(out act, true) || FlechePvE.CanUse(out act) ||
            EngagementPvE.CanUse(out act, true) || (CorpsacorpsPvE.CanUse(out act) && !IsMoving))
            return true;

        return base.AttackAbility(out act);
    }
}
