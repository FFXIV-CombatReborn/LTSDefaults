namespace DefaultRotations.Magical;

[Rotation("LTS's Default", CombatType.PvE, GameVersion = "6.58")]
[SourceCode(Path = "main/DefaultRotations/Magical/BLM_Default.cs")]
public class BLM_Default : BlackMageRotation
{
    private bool NeedToGoIce =>
        !(DespairPvE.EnoughLevel && CurrentMp >= DespairPvE.Info.MPNeed) &&
        !(FirePvE.EnoughLevel && CurrentMp >= FirePvE.Info.MPNeed);

    private bool NeedToTransposeGoIce(bool usedOne)
    {
        if (!NeedToGoIce || !ParadoxPvE.EnoughLevel) return false;

        var compare = usedOne ? -1 : 0;
        var count = PolyglotStacks;

        if (count == compare++ ||
            (count == compare++ && !EnchinaEndAfterGCD(2)) ||
            (!HasFire && !SwiftcastPvE.Cooldown.WillHaveOneChargeGCD(2) && !TriplecastPvE.CanUse(out _, gcdCountForAbility: 8)))
            return false;

        return count >= compare || (HasFire || SwiftcastPvE.Cooldown.WillHaveOneChargeGCD(2) || TriplecastPvE.Cooldown.WillHaveOneChargeGCD(2));
    }

    [RotationConfig(CombatType.PvE, Name = "Use Transpose to Astral Fire before Paradox")]
    public bool UseTransposeForParadox { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Extend Astral Fire Time Safely")]
    public bool ExtendTimeSafely { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = @"Use ""Double Paradox"" rotation [N15]")]
    public bool UseN15 { get; set; } = false;

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < FireIiiPvE.Info.CastTime + CountDownAhead && FireIiiPvE.CanUse(out var act))
            return act;

        if (remainTime <= 12 && SharpcastPvE.CanUse(out act, usedUp: true))
            return act;

        return base.CountDownAction(remainTime);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        if (IsBurst && UseBurstMedicine(out act)) return true;
        if (InUmbralIce)
        {
            if (UmbralIceStacks == 2 && !HasFire
                && !IsLastGCD(ActionID.ParadoxPvE))
            {
                if (SwiftcastPvE.CanUse(out act)) return true;
                if (TriplecastPvE.CanUse(out act, usedUp: true)) return true;
            }

            if (UmbralIceStacks < 3 && LucidDreamingPvE.CanUse(out act)) return true;
            if (SharpcastPvE.CanUse(out act, usedUp: true)) return true;
        }
        if (InAstralFire)
        {
            if (!CombatElapsedLess(6) && CombatElapsedLess(9) && LeyLinesPvE.CanUse(out act)) return true;
            if (TriplecastPvE.CanUse(out act, gcdCountForAbility: 5)) return true;
        }
        if (AmplifierPvE.CanUse(out act)) return true;
        return base.AttackAbility(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        //To Fire
        if (CurrentMp >= 7200 && UmbralIceStacks == 2 && ParadoxPvE.EnoughLevel)
        {
            if ((HasFire || HasSwift) && TransposePvE.CanUse(out act, onLastAbility: true)) return true;
        }
        if (nextGCD.IsTheSameTo(false, FireIiiPvE) && HasFire)
        {
            if (TransposePvE.CanUse(out act)) return true;
        }

        //Using Manafont
        if (InAstralFire)
        {
            if (CurrentMp == 0 && ManafontPvE.CanUse(out act)) return true;
            //To Ice
            if (NeedToTransposeGoIce(true) && TransposePvE.CanUse(out act)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (InFireOrIce(out act, out var mustGo)) return true;
        if (mustGo) return false;
        //Triplecast for moving.
        if (IsMoving && HasHostilesInRange && TriplecastPvE.CanUse(out act, usedUp: true, skipClippingCheck: true)) return true;

        if (AddElementBase(out act)) return true;
        if (ScathePvE.CanUse(out act)) return true;
        if (MaintainStatus(out act)) return true;

        return base.GeneralGCD(out act);
    }

    private bool InFireOrIce(out IAction? act, out bool mustGo)
    {
        act = null;
        mustGo = false;

        if (InUmbralIce && (GoFire(out act) || MaintainIce(out act) || DoIce(out act)))
            return true;

        if (InAstralFire && (GoIce(out act) || MaintainFire(out act) || DoFire(out act)))
            return true;

        return false;
    }

    private bool GoIce(out IAction? act)
    {
        act = null;

        if (!NeedToGoIce) return false;

        if (BlizzardIiPvE.CanUse(out act) || BlizzardIiiPvE.CanUse(out act) || TransposePvE.CanUse(out act) || BlizzardPvE.CanUse(out act))
            return true;

        return false;
    }

    private bool MaintainIce(out IAction? act)
    {
        act = null;
        if (UmbralIceStacks == 1)
        {
            if (BlizzardIiPvE.CanUse(out act)) return true;

            if (Player.Level == 90 && BlizzardPvE.CanUse(out act)) return true;
            if (BlizzardIiiPvE.CanUse(out act)) return true;
        }
        if (UmbralIceStacks == 2 && Player.Level < 90)
        {
            if (BlizzardIiPvE.CanUse(out act)) return true;
            if (BlizzardPvE.CanUse(out act)) return true;
        }
        return false;
    }

    private bool DoIce(out IAction? act)
    {
        act = null; // Initialize act with null to handle cases where no action is determined

        if (IsLastAction(ActionID.UmbralSoulPvE, ActionID.TransposePvE) && IsParadoxActive && BlizzardPvE.CanUse(out act))
            return true;

        if (UmbralIceStacks == 3 && UsePolyglot(out act))
            return true;

        if (UmbralIceStacks == 3 && BlizzardIvPvE.EnoughLevel && UmbralHearts < 3 && !IsLastGCD(ActionID.BlizzardIvPvE, ActionID.FreezePvE))
        {
            if (FreezePvE.CanUse(out act) || BlizzardIvPvE.CanUse(out act))
                return true;
        }

        if (AddThunder(out act, 5))
            return true;

        if (UmbralIceStacks == 2 && UsePolyglot(out act, 0))
            return true;

        if (IsParadoxActive && BlizzardPvE.CanUse(out act))
            return true;

        if (BlizzardIiPvE.CanUse(out act) || BlizzardIvPvE.CanUse(out act) || BlizzardPvE.CanUse(out act))
            return true;

        return false;
    }

    private bool GoFire(out IAction? act)
    {
        act = null;

        //Transpose line
        if (UmbralIceStacks < 3) return false;

        //Need more MP
        if (CurrentMp < 9600) return false;

        if (IsParadoxActive)
        {
            if (BlizzardPvE.CanUse(out act)) return true;
        }

        //Go to Fire.
        if (FireIiPvE.CanUse(out act)) return true;
        if (FireIiiPvE.CanUse(out act)) return true;
        if (TransposePvE.CanUse(out act)) return true;
        if (FirePvE.CanUse(out act)) return true;

        return false;
    }

    private bool MaintainFire(out IAction? act)
    {
        switch (AstralFireStacks)
        {
            case 1:
                if (FireIiPvE.CanUse(out act)) return true;
                if (UseN15)
                {
                    if (HasFire && FireIiiPvE.CanUse(out act)) return true;
                    if (IsParadoxActive && FirePvE.CanUse(out act)) return true;
                }
                if (FireIiiPvE.CanUse(out act)) return true;
                break;
            case 2:
                if (FireIiPvE.CanUse(out act)) return true;
                if (FirePvE.CanUse(out act)) return true;
                break;
        }

        if (ElementTimeEndAfterGCD(ExtendTimeSafely ? 3u : 2u))
        {
            if (CurrentMp >= FirePvE.Info.MPNeed * 2 + 800 && FirePvE.CanUse(out act)) return true;
            if (FlarePvE.CanUse(out act)) return true;
            if (DespairPvE.CanUse(out act)) return true;
        }

        act = null;
        return false;
    }

    private bool DoFire(out IAction? act)
    {
        if (UsePolyglot(out act)) return true;

        // Add thunder only at combat start.
        if (CombatElapsedLess(5))
        {
            if (AddThunder(out act, 0)) return true;
        }

        if (TriplecastPvE.CanUse(out act, skipClippingCheck:true)) return true;

        if (AddThunder(out act, 0) && Player.WillStatusEndGCD(1, 0, true,
            StatusID.Thundercloud)) return true;

        if (UmbralHearts < 2 && FlarePvE.CanUse(out act)) return true;
        if (FireIiPvE.CanUse(out act)) return true;

        if (CurrentMp >= FirePvE.Info.MPNeed + 800)
        {
            if (FireIvPvE.EnoughLevel)
            {
                if (FireIvPvE.CanUse(out act)) return true;
            }
            else if (HasFire)
            {
                if (FireIiiPvE.CanUse(out act)) return true;
            }
            if (FirePvE.CanUse(out act)) return true;
        }

        if (DespairPvE.CanUse(out act)) return true;

        return false;
    }

    private bool UseInstanceSpell(out IAction? act)
    {
        act = null; // Ensure act is nullable and initialized with null

        if (UsePolyglot(out act) || (HasThunder && AddThunder(out act, 1)) || UsePolyglot(out act, 0))
            return true;

        return false;
    }

    private bool AddThunder(out IAction? act, uint gcdCount = 3)
    {
        act = null;

        if (IsLastGCD(ActionID.ThunderPvE, ActionID.ThunderIiPvE, ActionID.ThunderIiiPvE, ActionID.ThunderIvPvE))
            return false;

        if (ThunderIiPvE.CanUse(out act) || ThunderPvE.CanUse(out act))
            return true;

        return false;
    }

    private bool AddElementBase(out IAction act)
    {
        if (CurrentMp >= 7200)
        {
            if (FireIiPvE.CanUse(out act)) return true;
            if (FireIiiPvE.CanUse(out act)) return true;
            if (FirePvE.CanUse(out act)) return true;
        }
        else
        {
            if (BlizzardIiPvE.CanUse(out act)) return true;
            if (BlizzardIiiPvE.CanUse(out act)) return true;
            if (BlizzardPvE.CanUse(out act)) return true;
        }
        return false;
    }

    private bool UsePolyglot(out IAction? act, uint gcdCount = 3)
    {
        if (gcdCount == 0 || IsPolyglotStacksMaxed && EnchinaEndAfterGCD(gcdCount))
        {
            if (FoulPvE.CanUse(out act)) return true;
            if (XenoglossyPvE.CanUse(out act)) return true;
        }

        act = null;
        return false;
    }

    private bool MaintainStatus(out IAction? act)
    {
        act = null;

        if (CombatElapsedLess(6)) return false;

        if (UmbralSoulPvE.CanUse(out act) || (InAstralFire && TransposePvE.CanUse(out act)) || 
            (UseTransposeForParadox && InUmbralIce && !IsParadoxActive && UmbralIceStacks == 3 && TransposePvE.CanUse(out act)))
            return true;

        return false;
    }

    [RotationDesc(ActionID.BetweenTheLinesPvE, ActionID.LeyLinesPvE)]
    protected override bool HealSingleAbility(out IAction? act)
    {
        if (BetweenTheLinesPvE.CanUse(out act)) return true;
        if (LeyLinesPvE.CanUse(out act)) return true;

        return base.HealSingleAbility(out act);
    }
}
