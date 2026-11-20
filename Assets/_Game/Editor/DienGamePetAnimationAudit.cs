#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using DienGame.Gameplay.Skills.Mage;

public static class DienGamePetAnimationAudit
{
    [MenuItem("DienGame/Animation/09 - Verify Pet Walk and Attack Sheets")]
    public static void Verify()
    {
        string[] pets = { "IceFox", "FireHawk", "PoisonLizard", "VoidOrb" };
        bool ok = true;
        int found = 0;
        foreach (string pet in pets)
        foreach (string action in new[] { "Walk", "Attack" })
        {
            int columns = action == "Walk" ? 4 : 5;
            string filename = "Bio_" + pet + "_" + action + "4x" + columns;
            string asset = "Assets/_Game/Resources/Equipment/BiomechMotionV2/" +
                action + "/" + filename + ".png";
            Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(asset);
            bool valid = sheet != null && sheet.width == 256 * columns && sheet.height == 1024;
            if (!valid)
            {
                ok = false;
                Debug.LogError("MISSING pet animation: " + asset);
            }
            else
            {
                found++;
                Debug.Log("PET CLIP OK: " + filename + " (" + sheet.width + "x" + sheet.height + ")");
            }
        }
        Debug.Log(ok ? "DIENGAME: 8/8 summon animation atlases found." :
            "DIENGAME: found " + found + "/8. Please re-extract the PATCH into the PROJECT ROOT.");
        EditorUtility.DisplayDialog("DienGame - Pet Animation", ok ?
            "Found all 8 pet animation atlases. Enter Play Mode to check moving and attacks." :
            "Missing animation atlases. Inspect the Console and project extraction path.", "OK");
    }

    [MenuItem("DienGame/Animation/09 - Inspect Live Pet (Play Mode)")]
    public static void Inspect()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Enter Play Mode and select Mage first.");
            return;
        }
        ChimeraCompanion companion = Object.FindFirstObjectByType<ChimeraCompanion>();
        if (companion == null)
        {
            Debug.LogWarning("No ChimeraCompanion found: enter the Mage gameplay scene.");
            return;
        }
        SpriteRenderer[] renderers = companion.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sprite in renderers)
            Debug.Log("PET renderer " + sprite.name + ": active=" + sprite.enabled +
                ", sprite=" + (sprite.sprite != null ? sprite.sprite.name : "NULL"));
        Debug.Log("Observe the pet continuously for at least 2 seconds, not a still screenshot.");
    }
}
#endif
