using DienGame.Gameplay.Enemy;
using UnityEngine;

namespace DienGame.Gameplay.World
{
    /// <summary>
    /// Phase 11: non-canonical encounter design grounded in the five existing islands.
    /// No replacements of original NPCs, maps, skills, enemy art or quest progress.
    /// Stage 1 retains the original tutorial encounter logic.
    /// </summary>
    public readonly struct Stage11EncounterProfile
    {
        public readonly int Stage;
        public readonly string BossTitle;
        public readonly float WaveIntervalMultiplier;
        public readonly int WaveSizeBonus;
        public readonly float BossHealthMultiplier;
        public readonly float BossSpecialInterval;
        public readonly float HazardInterval;
        public readonly float HazardWarning;
        public readonly float HazardRadius;
        public readonly float HazardHealthFraction;
        public readonly Color Color;
        private readonly float[] _weights;

        public Stage11EncounterProfile(int stage, string bossTitle, float waveIntervalMultiplier,
            int waveSizeBonus, float bossHealthMultiplier, float bossSpecialInterval,
            float hazardInterval, float hazardWarning, float hazardRadius, float hazardHealthFraction,
            Color color, float[] weights)
        {
            Stage = stage;
            BossTitle = bossTitle;
            WaveIntervalMultiplier = waveIntervalMultiplier;
            WaveSizeBonus = waveSizeBonus;
            BossHealthMultiplier = bossHealthMultiplier;
            BossSpecialInterval = bossSpecialInterval;
            HazardInterval = hazardInterval;
            HazardWarning = hazardWarning;
            HazardRadius = hazardRadius;
            HazardHealthFraction = hazardHealthFraction;
            Color = color;
            _weights = weights;
        }

        /// <summary>Weighted mix for Grunt, Runner, Tank, Ranged, Swarm.</summary>
        public EnemyArchetype RollArchetype(float progress)
        {
            if (_weights == null || _weights.Length != 5) return EnemyArchetype.Grunt;
            // Late waves favour heavies/ranged, but keep a mixed set on every island.
            float late = Mathf.Clamp01(progress);
            float total = 0f;
            for (int i = 0; i < 5; i++)
                total += Mathf.Max(0f, _weights[i] * (i == 2 || i == 3 ? 1f + .35f * late : 1f));
            if (total <= .0001f) return EnemyArchetype.Grunt;
            float roll = Random.value * total;
            for (int i = 0; i < 5; i++)
            {
                roll -= Mathf.Max(0f, _weights[i] * (i == 2 || i == 3 ? 1f + .35f * late : 1f));
                if (roll <= 0f) return IndexToArchetype(i);
            }
            return EnemyArchetype.Grunt;
        }

        private static EnemyArchetype IndexToArchetype(int index)
        {
            switch (index)
            {
                case 1: return EnemyArchetype.Runner;
                case 2: return EnemyArchetype.Tank;
                case 3: return EnemyArchetype.Ranged;
                case 4: return EnemyArchetype.Swarm;
                default: return EnemyArchetype.Grunt;
            }
        }
    }

    public static class Stage11EncounterProfiles
    {
        public static Stage11EncounterProfile Get(int stage)
        {
            stage = Mathf.Clamp(stage, 1, WorldStageThemes.TotalStages);
            Color accent = WorldStageThemes.Get(stage).Accent;
            switch (stage)
            {
                case 2: // Rừng Thiên Mộc: swarm + ambush, ecology/field-X contamination.
                    return new Stage11EncounterProfile(stage, "HỘ VỆ CƠ SINH RỪNG", 1.00f, 0,
                        1.10f, 23f, 38f, 1.55f, 1.15f, .045f, accent,
                        new[] { .24f, .23f, .09f, .14f, .30f });
                case 3: // Cổ Tự Cát Vàng: sentries and armored guardians.
                    return new Stage11EncounterProfile(stage, "HỘ VỆ TÀN TÍCH", .98f, 0,
                        1.18f, 22f, 36f, 1.60f, 1.10f, .050f, accent,
                        new[] { .20f, .12f, .24f, .29f, .15f });
                case 4: // Hỏa Thành Long Mạch: heat vents, melee rush and armor.
                    return new Stage11EncounterProfile(stage, "LÕI HỎA THÀNH", .97f, 1,
                        1.27f, 21f, 35f, 1.65f, 1.10f, .055f, accent,
                        new[] { .19f, .27f, .25f, .13f, .16f });
                case 5: // Cung Băng Vân Thiên: long-range pressure and crystal formations.
                    return new Stage11EncounterProfile(stage, "HỘ VỆ VÂN THIÊN", .96f, 1,
                        1.36f, 20f, 34f, 1.70f, 1.05f, .060f, accent,
                        new[] { .14f, .13f, .22f, .34f, .17f });
                default: // preserve Stage 1 tutorial / original boss.
                    return new Stage11EncounterProfile(1, "PHÁN QUYẾT", 1f, 0,
                        1f, 0f, 0f, 0f, 0f, 0f, accent,
                        new[] { .32f, .22f, .14f, .16f, .16f });
            }
        }
    }
}
