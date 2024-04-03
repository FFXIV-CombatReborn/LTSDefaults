using RotationSolver.Basic.Helpers;
using System.ComponentModel;

namespace DefaultRotations.Magical;

[Rotation("LTS's Default", CombatType.PvE, GameVersion = "6.58")]
[SourceCode(Path = "main/DefaultRotations/Magical/SMN_Default.cs")]
public sealed class SMN_Default : SummonerRotation
{
    public enum SwiftType : byte
    {
        No,
        Emerald,
        Ruby,
        All,
    }

    public enum SummonOrderType : byte
    {
        [Description("Topaz-Emerald-Ruby")]
        TopazEmeraldRuby,

        [Description("Topaz-Ruby-Emerald")]
        TopazRubyEmerald,

        [Description("Emerald-Topaz-Ruby")]
        EmeraldTopazRuby,
    }

    [RotationConfig(CombatType.PvE, Name = "Order")]
    public SummonOrderType SummonOrder { get; set; } = SummonOrderType.EmeraldTopazRuby;

    [RotationConfig(CombatType.PvE, Name = "Use Swiftcast")]
    public SwiftType AddSwiftcast { get; set; } = SwiftType.No;

    [RotationConfig(CombatType.PvE, Name = "Use Crimson Cyclone")]
    public bool AddCrimsonCyclone { get; set; } = true;

    public override bool CanHealSingleSpell => false;

    [RotationDesc(ActionID.CrimsonCyclonePvE)]
    protected override bool MoveForwardGCD(out IAction? act)
    {
        return CrimsonCyclonePvE.CanUse(out act, skipAoeCheck: true) || base.MoveForwardGCD(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        act = null;

        if (SummonCarbunclePvE.CanUse(out act)) return true;
        if (SlipstreamPvE.CanUse(out act, true)) return true;
        if (CrimsonStrikePvE.CanUse(out act, true)) return true;
        if (PreciousBrilliancePvE.CanUse(out act)) return true;
        if (GemshinePvE.CanUse(out act)) return true;
        if (!IsMoving && AddCrimsonCyclone && CrimsonCyclonePvE.CanUse(out act, true)) return true;
        if ((Player.HasStatus(false, StatusID.SearingLight) || SearingLightPvE.Cooldown.IsCoolingDown) && SummonBahamutPvE.CanUse(out act)) return true;
        if (!SummonBahamutPvE.EnoughLevel && HasHostilesInRange && AetherchargePvE.CanUse(out act)) return true;
        if (IsMoving && (Player.HasStatus(true, StatusID.GarudasFavor) || InIfrit) && !Player.HasStatus(true, StatusID.Swiftcast) && !InBahamut && !InPhoenix && RuinIvPvE.CanUse(out act, true)) return true;
        if (SummonTimeEndAfterGCD() && AttunmentTimeEndAfterGCD() && !Player.HasStatus(true, StatusID.Swiftcast) && !InBahamut && !InPhoenix && RuinIvPvE.CanUse(out act, true)) return true;
        if (OutburstPvE.CanUse(out act)) return true;
        if (RuinPvE.CanUse(out act)) return true;

        // Summon order handling corrected
        switch (SummonOrder)
        {
            case SummonOrderType.TopazEmeraldRuby:
                if (SummonTopazPvE.CanUse(out act)) return true;
                if (SummonEmeraldPvE.CanUse(out act)) return true;
                if (SummonRubyPvE.CanUse(out act)) return true;
                break;

            case SummonOrderType.TopazRubyEmerald:
                if (SummonTopazPvE.CanUse(out act)) return true;
                if (SummonRubyPvE.CanUse(out act)) return true;
                if (SummonEmeraldPvE.CanUse(out act)) return true;
                break;

            case SummonOrderType.EmeraldTopazRuby:
                if (SummonEmeraldPvE.CanUse(out act)) return true;
                if (SummonTopazPvE.CanUse(out act)) return true;
                if (SummonRubyPvE.CanUse(out act)) return true;
                break;
        }

        return base.GeneralGCD(out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        act = null;
        var isTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        var isTargetDying = HostileTarget?.IsDying() ?? false;

        // Check for Burst phase without Searing Light status
        if (IsBurst && !Player.HasStatus(false, StatusID.SearingLight))
        {
            if (SearingLightPvE.CanUse(out act, true)) return true;
        }

        // Handling abilities in Bahamut phase
        if (InBahamut)
        {
            if (SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3))
            {
                if (EnkindleBahamutPvE.CanUse(out act, true)) return true;
            }
        }

        // Handling abilities in Phoenix phase
        if (InPhoenix)
        {
            if (DeathflarePvE.CanUse(out act, true) || RekindlePvE.CanUse(out act, true))
                return true;
        }

        // Handling when the target is a boss and is dying
        if (isTargetBoss && isTargetDying)
        {
            if (EnkindleBahamutPvE.CanUse(out act, true)) return true;
        }

        // Handling other abilities
        if (MountainBusterPvE.CanUse(out act, true) ||
            EnergySiphonPvE.CanUse(out act) ||
            EnergyDrainPvE.CanUse(out act))
            return true;

        return base.AttackAbility(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        act = null;

        if (AddSwiftcast != SwiftType.No && SwiftcastPvE.CanUse(out act) &&
            (AddSwiftcast == SwiftType.All ||
             AddSwiftcast == SwiftType.Emerald && (nextGCD.IsTheSameTo(true, SlipstreamPvE) || Attunement == 0 && Player.HasStatus(true, StatusID.GarudasFavor)) ||
             AddSwiftcast == SwiftType.Ruby && InIfrit && (nextGCD.IsTheSameTo(true, GemshinePvE, PreciousBrilliancePvE) || IsMoving)))
            return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override IAction? CountDownAction(float remainTime)
    {
        if (SummonCarbunclePvE.CanUse(out var act)) return act;

        if (remainTime <= RuinPvE.Info.CastTime + CountDownAhead && RuinPvE.CanUse(out act)) return act;

        return base.CountDownAction(remainTime);
    }
}