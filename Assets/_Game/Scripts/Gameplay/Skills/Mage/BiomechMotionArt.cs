using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>
    /// Uses the four APPROVED painted 4x4 atlases from the user's latest art.
    /// The rows are Up/Right/Down/Left, the columns are four distinct hand-drawn poses.
    /// All sprite textures are normalized to 1024x1024; Runtime slicing requires Read/Write.
    /// No animator controller or Inspector setup needed. Never affects original Chimera.
    /// </summary>
    public static class BiomechMotionArt
    {
        private const string Root = "Equipment/BiomechApproved/";
        private static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();
        private static readonly HashSet<string> Missing = new HashSet<string>();

        public static bool IsSupported(string id)
        {
            return id == "mage_bio_icefox" || id == "mage_bio_firehawk" ||
                   id == "mage_bio_lizard" || id == "mage_bio_void";
        }

        public static Sprite GetFrame(string id, int facing, bool attacking, int frame)
        {
            string pet = FormName(id);
            if (pet == null) return null;
            // Normal attacks use windup/recoil poses, never the fully released signature frame.
            // Signature animations alone show the large third-frame projectile.
            int walkFrame = Mathf.Abs(frame) % 4;
            int normalAttackFrame = frame <= 0 ? 0 : frame <= 2 ? 1 : 3;
            Sprite approved;
            if (attacking)
                approved = GetApproved(pet, "Cast", "Cast4x4", facing, normalAttackFrame);
            else if (id == "mage_bio_firehawk")
            {
                // Keep the earlier corrected phoenix level-head flying atlas when present.
                approved = FireHawkFinalArt.Flight(facing, walkFrame);
                if (approved == null)
                    approved = GetApproved(pet, "Walk", "Walk4x4", facing, walkFrame);
            }
            else approved = GetApproved(pet, "Walk", "Walk4x4", facing, walkFrame);
            if (approved != null) return approved;
            return LegacyFrame(pet, facing, attacking, frame);
        }

        public static Sprite GetCast(string id, int facing, int frame, bool level5)
        {
            string pet = FormName(id);
            if (pet == null) return null;
            // Lv5 has separate composited accent frames from original painted cast poses;
            // the gameplay's Lv5 VFX also uses its own authored atlas.
            string category = level5 ? "CastLv5" : "Cast";
            string suffix = level5 ? "CastLv5_4x4" : "Cast4x4";
            Sprite result = GetApproved(pet, category, suffix, facing, frame);
            if (result != null) return result;
            if (id == "mage_bio_firehawk")
                return FireHawkFinalArt.Cast(facing, Mathf.Clamp(frame, 0, 4), level5);
            return LegacyFrame(pet, facing, true, frame);
        }

        public static bool HasPaintedArt(string id)
        {
            string pet = FormName(id);
            return pet != null && Resources.Load<Texture2D>(Root + "Walk/Bio_" + pet + "_Walk4x4") != null &&
                   Resources.Load<Texture2D>(Root + "Cast/Bio_" + pet + "_Cast4x4") != null;
        }

        private static Sprite GetApproved(string pet, string dir, string suffix, int direction, int frame)
        {
            string path = Root + dir + "/Bio_" + pet + "_" + suffix;
            if (!Cache.TryGetValue(path, out Sprite[] slices))
            {
                Texture2D tex = Resources.Load<Texture2D>(path);
                if (tex == null || tex.width != 1024 || tex.height != 1024)
                {
                    if (Missing.Add(path))
                        Debug.LogWarning("DienGame: missing painted biomech sheet or incorrect 1024x1024: Resources/" + path);
                    return null;
                }
                slices = new Sprite[16];
                for (int row = 0; row < 4; row++)
                for (int col = 0; col < 4; col++)
                {
                    Rect rect = new Rect(col * 256, (3 - row) * 256, 256, 256);
                    Sprite sprite = Sprite.Create(tex, rect, new Vector2(.5f, .5f), 185f,
                        0, SpriteMeshType.FullRect);
                    sprite.name = "Painted_" + pet + "_" + dir + "_D" + row + "_F" + col;
                    slices[row * 4 + col] = sprite;
                }
                Cache[path] = slices;
            }
            return slices[Mathf.Clamp(direction, 0, 3) * 4 + Mathf.Clamp(frame, 0, 3)];
        }

        private static Sprite LegacyFrame(string pet, int facing, bool attacking, int frame)
        {
            int columns = attacking ? 5 : 4;
            string dir = attacking ? "Attack" : "Walk";
            string path = "Equipment/BiomechMotionV2/" + dir + "/Bio_" + pet + "_" + dir + "4x" + columns;
            if (!Cache.TryGetValue(path, out Sprite[] clips))
            {
                Texture2D tex = Resources.Load<Texture2D>(path);
                if (tex == null || tex.width != columns * 256 || tex.height != 1024) return null;
                clips = new Sprite[columns * 4];
                for (int d = 0; d < 4; d++)
                for (int c = 0; c < columns; c++)
                    clips[d * columns + c] = Sprite.Create(tex,
                        new Rect(c * 256, (3 - d) * 256, 256, 256), new Vector2(.5f, .5f), 185f);
                Cache[path] = clips;
            }
            return clips[Mathf.Clamp(facing, 0, 3) * columns + Mathf.Clamp(frame, 0, columns - 1)];
        }

        private static string FormName(string id)
        {
            switch (id)
            {
                case "mage_bio_icefox": return "IceFox";
                case "mage_bio_firehawk": return "FireHawk";
                case "mage_bio_lizard": return "PoisonLizard";
                case "mage_bio_void": return "VoidOrb";
                default: return null;
            }
        }
    }
}
