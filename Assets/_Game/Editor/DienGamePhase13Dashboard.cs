#if UNITY_EDITOR
using DienGame.Client;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Phase13;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    public sealed class DienGamePhase13Dashboard : EditorWindow
    {
        private float _lastRepaint;
        [MenuItem("DienGame/Phase13/Combat and Mobile QA")]
        public static void Open() => GetWindow<DienGamePhase13Dashboard>("Phase13 Combat QA");

        private void OnEnable() => EditorApplication.update += Refresh;
        private void OnDisable() => EditorApplication.update -= Refresh;
        private void Refresh()
        {
            if (!EditorApplication.isPlaying) return;
            float now = (float)EditorApplication.timeSinceStartup;
            if (now - _lastRepaint < .35f) return;
            _lastRepaint = now;
            Repaint();
        }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("PHASE 13 - SOUND, QUALITY AND BALANCE", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use the same island, stage, gear level and survival duration for each of the 4 classes. DPS includes pets and skills. Nothing below grants resources.", MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mobile quality", EditorStyles.boldLabel);
            int target = Phase13MobileProfile.RequestedFps;
            int wanted = EditorGUILayout.IntPopup("Mobile target FPS", target,
                new[] { "30 FPS (battery)", "60 FPS (smooth)" }, new[] { 30, 60 });
            if (wanted != target) Phase13MobileProfile.SetTargetFps(wanted);
            Phase13MobileProfile.SimulateMobileInEditor = EditorGUILayout.Toggle(
                "Simulate mobile VFX budget", Phase13MobileProfile.SimulateMobileInEditor);
            EditorGUILayout.LabelField("VFX skipped (hit/death)",
                Phase13MobileProfile.SkippedHitEffects + " / " + Phase13MobileProfile.SkippedDeathEffects);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sound", EditorStyles.boldLabel);
            float volume = EditorGUILayout.Slider("SFX volume", Phase13Audio.Volume, 0f, 1f);
            if (Mathf.Abs(volume - Phase13Audio.Volume) > .001f) Phase13Audio.SetVolume(volume);
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                if (GUILayout.Button("Preview level-up SFX")) Phase13Audio.Play(Phase13Cue.LevelUp);
                if (GUILayout.Button("Preview boss warning SFX")) Phase13Audio.Play(Phase13Cue.Boss);
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Combat telemetry (current run)", EditorStyles.boldLabel);
            if (!EditorApplication.isPlaying || !Phase13CombatMetrics.IsStarted)
            {
                EditorGUILayout.HelpBox("Enter a GameplayTest scene and press Play to measure DPS.", MessageType.None);
            }
            else
            {
                EditorGUILayout.LabelField("Class / Island", Phase13CombatMetrics.ClassName + " / " + Phase13CombatMetrics.Stage);
                EditorGUILayout.LabelField("Result / Duration", Phase13CombatMetrics.Result + " / " + Phase13CombatMetrics.ElapsedSeconds.ToString("F1") + "s");
                EditorGUILayout.LabelField("Damage dealt / DPS", Phase13CombatMetrics.TotalEnemyDamage.ToString("F0") + " / " + Phase13CombatMetrics.AverageDps.ToString("F1"));
                EditorGUILayout.LabelField("Damage taken / Healed", Phase13CombatMetrics.TotalDamageTaken.ToString("F0") + " / " + Phase13CombatMetrics.TotalHealing.ToString("F0"));
                EditorGUILayout.LabelField("Kills / XP", Phase13CombatMetrics.Kills + " / " + Phase13CombatMetrics.Experience);
                EditorGUILayout.LabelField("Enemies alive", EnemyHealth.Active.Count.ToString());
                if (GUILayout.Button("Copy current summary to clipboard"))
                    EditorGUIUtility.systemCopyBuffer = Phase13CombatMetrics.Summary();
                if (GUILayout.Button("Log current summary")) Debug.Log(Phase13CombatMetrics.Summary());
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Starter equipment multipliers (not measured DPS)", EditorStyles.boldLabel);
            foreach (PlayableClass cls in new[] { PlayableClass.Marksman, PlayableClass.Assassin, PlayableClass.Mage, PlayableClass.Warrior })
            {
                EquipmentDefinition starter = EquipmentCatalog.Starter(cls);
                if (starter != null)
                    EditorGUILayout.LabelField(cls.ToString(), starter.DamageMultiplier.ToString("F2") + "x - " + starter.Name);
            }
        }
    }
}
#endif
