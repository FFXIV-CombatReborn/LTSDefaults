using RotationSolver.Basic.Rotations.Duties;

namespace DefaultRotations.Duty;

[Rotation("Emanation Default", CombatType.PvE)]
[DutyTerritory(263, 264)]
internal class EmanationDefault : DutyRotation
{
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (VrilPvE.CanUse(out act)) return true; // Normal
        if (VrilPvE_9345.CanUse(out act)) return true; // Extreme
        return base.EmergencyAbility(nextGCD, out act);
    }
}
