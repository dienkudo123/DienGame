#if UNITY_EDITOR
using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>Read-only diagnostics. Does not write PlayerPrefs or mutate gameplay.</summary>
    public sealed class DienGamePhase13BalanceAudit : EditorWindow
    {
        [MenuItem("DienGame/Phase13/Balance QA - Wave, Boss, HP")]
        public static void Open()
        {
            GetWindow<DienGamePhase13BalanceAudit>("Phase13 Balance QA").Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("DIENGAME - BEGINNER BALANCE", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Fixed monster HP within each island (no per-wave HP ramp). 2 to 5 enemies/wave, one species/wave, no more than 20 alive. Level 25 is the target for a 10-minute actively-played run, not a guaranteed level.", MessageType.Info);
            EditorGUILayout.LabelField("Default gunshot damage", GameBalance.PlayerDamage.ToString("0.0"));
            EditorGUILayout.LabelField("Default tutorial Grunt HP", (GameBalance.PlayerDamage * 1.5f).ToString("0.0"));
            EditorGUILayout.LabelField("Opening enemies per wave", GameBalance.BaseEnemiesPerWave.ToString());
            EditorGUILayout.LabelField("Opening wave delay", GameBalance.FirstWaveDelay.ToString("0.0") + " s");
            EditorGUILayout.LabelField("Enemy HP ramp within run", GameBalance.MaxEnemyHealthMultiplier.ToString("0.00") + "x");
            EditorGUILayout.LabelField("Enemy HP per island", "+" + (GameBalance.EnemyHealthBonusPerIsland * 100f).ToString("0") + "%");
            EditorGUILayout.LabelField("Wave range", GameBalance.BaseEnemiesPerWave + " - " + GameBalance.MaxEnemiesPerWave);
            EditorGUILayout.LabelField("Max alive enemies", GameBalance.MaxAliveEnemies.ToString());
            EditorGUILayout.LabelField("Target end-run level", GameBalance.TargetLevelAtEndOfRun.ToString());
            EditorGUILayout.LabelField("EXP needed Lv1 -> Lv25", GameBalance.CumulativeExperienceToReachLevel(GameBalance.TargetLevelAtEndOfRun).ToString());
            EditorGUILayout.LabelField("Boss base HP", GameBalance.BossMaxHealth.ToString("0"));
            EditorGUILayout.Space(10);
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Start Play Mode to inspect live wave species, actual HP and Boss state.", MessageType.None);
                return;
            }

            PlayerExperience playerExperience = FindFirstObjectByType<PlayerExperience>();
            if (playerExperience != null)
            {
                EditorGUILayout.LabelField("Current level", playerExperience.CurrentLevel.ToString());
                EditorGUILayout.LabelField("Current-level EXP", playerExperience.CurrentExperience.ToString() +
                    "/" + playerExperience.ExperienceToNextLevel);
                EditorGUILayout.LabelField("Collected EXP this run", DienGame.Gameplay.Phase13.Phase13CombatMetrics.Experience.ToString());
                if (DienGame.Gameplay.Phase13.Phase13CombatMetrics.ElapsedSeconds >= 120f)
                {
                    float expRate = DienGame.Gameplay.Phase13.Phase13CombatMetrics.Experience /
                        Mathf.Max(1f, DienGame.Gameplay.Phase13.Phase13CombatMetrics.ElapsedSeconds);
                    float projection = expRate * GameBalance.SurvivalDuration;
                    EditorGUILayout.LabelField("10-min EXP projection", projection.ToString("0") +
                        " (not guaranteed)");
                }
            }
            EnemyWaveSpawner spawner = FindFirstObjectByType<EnemyWaveSpawner>();
            PlayerAutoAttack marksman = FindFirstObjectByType<PlayerAutoAttack>();
            if (marksman != null)
                EditorGUILayout.LabelField("Live Marksman shot", marksman.AttackDamage.ToString("0.0"));
            if (spawner != null)
            {
                EditorGUILayout.LabelField("Waves spawned", spawner.WaveNumber.ToString());
                EditorGUILayout.LabelField("Last wave species", spawner.LastWaveArchetype.ToString());
                EditorGUILayout.LabelField("Alive enemies", spawner.AliveEnemyCount.ToString());
            }
            BossCombatController boss = FindFirstObjectByType<BossCombatController>();
            if (boss != null)
            {
                EnemyHealth hp = boss.GetComponent<EnemyHealth>();
                EditorGUILayout.LabelField("Boss telegraph/cast active", boss.IsCasting ? "YES" : "NO");
                if (hp != null) EditorGUILayout.LabelField("Boss HP", hp.CurrentHealth.ToString("0") + "/" + hp.MaxHealth.ToString("0"));
            }
            if (GUILayout.Button("Log live enemy HP / species"))
            {
                foreach (EnemyHealth hp in EnemyHealth.Active)
                {
                    if (hp == null) continue;
                    EnemyProfile profile = hp.GetComponent<EnemyProfile>();
                    Debug.Log("Balance QA | " + (profile != null ? profile.Archetype.ToString() : "Unknown") +
                        " | HP " + hp.CurrentHealth.ToString("0.0") + "/" + hp.MaxHealth.ToString("0.0"), hp);
                }
            }
        }

        private void Update() { if (EditorApplication.isPlaying) Repaint(); }
    }
}
#endif
