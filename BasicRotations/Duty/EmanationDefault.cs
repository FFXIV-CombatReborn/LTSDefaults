﻿namespace DefaultRotations.Duty;

[Rotation("Emanation Default", CombatType.PvE)]
[DutyTerritory(263, 264)]
internal class EmanationDefault : DutyRotation
{
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        // 8521 8522 8523
        bool Lol1 = HostileTarget?.CastActionId == 8521
        bool Lol2 = HostileTarget?.CastActionId == 8522
        bool Lol3 = HostileTarget?.CastActionId == 8523

        if (Lol1 || Lol2 || Lol3)
        {
            if (VrilPvE.CanUse(out act)) return true; // Normal
            if (VrilPvE_9345.CanUse(out act)) return true; // Extreme
            return base.EmergencyAbility(nextGCD, out act);
        }
    }
}