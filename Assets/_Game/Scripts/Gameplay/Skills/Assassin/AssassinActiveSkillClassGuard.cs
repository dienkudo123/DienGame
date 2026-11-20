using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills.Marksman;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>
    /// Ensures the Assassin scene always uses Phi Anh Bat Tuong (Camille-like hook/dash)
    /// instead of any Marksman active skill. Runs on initial load, Lobby entry and restart.
    /// No changes to the Marksman scene or shared gameplay systems.
    /// </summary>
    public static class AssassinActiveSkillClassGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            // Idempotent even with Enter Play Mode Options / Domain Reload disabled.
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!scene.IsValid() || !scene.isLoaded) return;

            // Detect the class from the actual combat component, not Lobby state.
            // Therefore the Marksman scene cannot accidentally receive Assassin UI.
            AssassinMeleeAttack assassin = FindAssassinInScene(scene);
            if (assassin == null || !assassin.isActiveAndEnabled) return;

            GameObject player = assassin.gameObject;
            DisableIfPresent(player.GetComponent<PlayerAutoAttack>());
            DisableIfPresent(player.GetComponent<MarksmanSkillController>());
            DisableIfPresent(player.GetComponent<AirStrikeController>());
            DisableIfPresent(player.GetComponent<JudgementController>());

            // Remove a previously serialized / spawned Marksman Ultimate button.
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                JudgementUltimateButton[] wrongButtons =
                    roots[i].GetComponentsInChildren<JudgementUltimateButton>(true);
                for (int j = 0; j < wrongButtons.Length; j++)
                {
                    if (wrongButtons[j] != null)
                        DisableIfPresent(wrongButtons[j]);
                }

                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = transforms.Length - 1; j >= 0; j--)
                {
                    Transform t = transforms[j];
                    if (t != null && t.name == "JudgementUltimateButton" &&
                        t.GetComponentInParent<Canvas>(true) != null)
                    {
                        Object.Destroy(t.gameObject);
                    }
                }
            }

            // The approved Assassin active is an explicit 2-phase grapple:
            // button 1 hooks a solid wall / soul pillar, button 2 vaults outward.
            AssassinWallDive wallDive = player.GetComponent<AssassinWallDive>();
            if (wallDive == null) wallDive = player.AddComponent<AssassinWallDive>();
            wallDive.enabled = true;

            Canvas gameplayCanvas = FindGameplayCanvas(scene);
            if (gameplayCanvas == null)
            {
                Debug.LogError("Assassin active skill: GameplayCanvas not found in " +
                               scene.name + ". WallDive exists, but its button cannot be shown.");
                return;
            }

            AssassinWallDiveHud hud = gameplayCanvas.GetComponent<AssassinWallDiveHud>();
            if (hud == null) hud = gameplayCanvas.gameObject.AddComponent<AssassinWallDiveHud>();
            hud.SetWallDive(wallDive);
            hud.enabled = true;

            Debug.Log("DienGame: Assassin active is Phi Anh Bat Tuong (hook -> leap). " +
                      "Marksman active/HUD suppressed only in " + scene.name + ".");
        }

        private static AssassinMeleeAttack FindAssassinInScene(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                AssassinMeleeAttack[] attacks =
                    roots[i].GetComponentsInChildren<AssassinMeleeAttack>(true);
                for (int j = 0; j < attacks.Length; j++)
                {
                    AssassinMeleeAttack attack = attacks[j];
                    if (attack != null && attack.gameObject.scene == scene &&
                        attack.gameObject.activeInHierarchy && attack.enabled)
                        return attack;
                }
            }
            return null;
        }

        private static Canvas FindGameplayCanvas(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Canvas[] canvases = roots[i].GetComponentsInChildren<Canvas>(true);
                for (int j = 0; j < canvases.Length; j++)
                {
                    if (canvases[j] != null && canvases[j].name == "GameplayCanvas" &&
                        canvases[j].gameObject.scene == scene)
                        return canvases[j];
                }
            }
            return null;
        }

        private static void DisableIfPresent(Behaviour component)
        {
            if (component != null && component.enabled) component.enabled = false;
        }
    }
}
