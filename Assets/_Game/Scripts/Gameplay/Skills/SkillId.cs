namespace DienGame.Gameplay.Skills
{
    public enum SkillId
    {
        None = 0,

        PiercingBullet = 100,

        ThunderBullet = 200,
        CombatDrone = 201,
        AirStrike = 202,
        RicochetBullet = 203,
        Overdrive = 204,
        TacticalShield = 205,

        // Assassin-only. Preserve Marksman IDs and saved progress keys.
        AssassinBloodSlash = 300,
        AssassinAfterimage = 301,
        AssassinFlyingDagger = 302,
        AssassinShadowClone = 303,
        AssassinDeathMark = 304,
        AssassinOrbitBlades = 305,
        AssassinExecution = 306,

        MageCrystalNeedles = 400,
        MageGravityWell = 401,
        MageMetalClaws = 402,
        MageBiocannon = 403,
        MageCrystalStrike = 404,
        MagePredatorResonance = 405,
        MageDualShield = 406,

        // Warrior-only. Kept out of other class pools and old saved IDs.
        WarriorDrive = 500,
        WarriorBulwark = 501,
        WarriorShockwave = 502,
        WarriorHeat = 503,
        WarriorBreaker = 504,
        WarriorTempo = 505,
        WarriorRegeneration = 506,
    }
}
