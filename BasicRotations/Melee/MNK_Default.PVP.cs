namespace DefaultRotations.Melee;

[Rotation("LTS's Default", CombatType.PvP, GameVersion = "6.58", Description = "Beta Rotation")]
[SourceCode(Path = "main/DefaultRotations/Melee/MNK_Default.cs")]
[Api(1)]
public sealed class MNK_DefaultPvP : MonkRotation
{

    [RotationConfig(CombatType.PvE, Name = "Enable to do nothing when in Guard")]
    public bool GuardCancel { get; set; } = true;

    protected override bool GeneralGCD(out IAction? act)
    {
        act = null;

        #region PvP
        if (GuardCancel && Player.HasStatus(true, StatusID.Guard)) return false;

        if (RisingPhoenixPvP.CanUse(out act)) return true;
        if (EnlightenmentPvP.CanUse(out act)) return true;
        if (RisingPhoenixPvP.CanUse(out act)) return true;
        if (PhantomRushPvP.CanUse(out act)) return true;
        if (SixsidedStarPvP.CanUse(out act)) return true;
        if (EnlightenmentPvP.CanUse(out act, usedUp : true)) return true;

        if (InCombat)
        {
            if (RisingPhoenixPvP.CanUse(out act)) return true;
        }
        if (InCombat)
        {
            if (ThunderclapPvP.CanUse(out act)) return true;
        }
        if (InCombat)
        {
            if (RiddleOfEarthPvP.CanUse(out act)) return true;
        }

        if (Player.HasStatus(true, StatusID.EarthResonance))
        {
            if (EarthsReplyPvP.CanUse(out act)) return true;
        }

        if (PhantomRushPvP.CanUse(out act)) return true;
        if (DemolishPvP.CanUse(out act)) return true;
        if (TwinSnakesPvP.CanUse(out act)) return true;
        if (DragonKickPvP.CanUse(out act)) return true;
        if (SnapPunchPvP.CanUse(out act)) return true;
        if (TrueStrikePvP.CanUse(out act)) return true;
        if (BootshinePvP.CanUse(out act)) return true;


        return false;
        #endregion
    }
}

