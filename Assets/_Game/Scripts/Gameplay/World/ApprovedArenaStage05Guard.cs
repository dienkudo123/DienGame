using UnityEngine;

namespace DienGame.Gameplay.World
{
    /// <summary>
    /// The older Stage-5 premium map can auto-create additional terrain after
    /// sceneLoaded. Disable that legacy scene-wide overlay before its Start runs
    /// when the five approved maps are active; otherwise it duplicates colliders.
    /// No hard reference is required because older projects may not include it.
    /// </summary>
    [DefaultExecutionOrder(-30000)]
    public sealed class ApprovedArenaStage05Guard : MonoBehaviour
    {
        private void Start()
        {
            MonoBehaviour[] scripts = Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsSortMode.None);
            foreach (MonoBehaviour component in scripts)
            {
                if (component == null || component == this) continue;
                if (component.GetType().FullName !=
                    "DienGame.Gameplay.World.IceArenaPremiumRuntime") continue;
                component.StopAllCoroutines();
                component.enabled = false;
                Object.Destroy(component.gameObject);
                Debug.Log("ApprovedMap: legacy stage-5 premium overlay disabled.");
            }
        }
    }
}
