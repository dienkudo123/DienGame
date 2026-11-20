using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>
    /// Drawn Phoenix frames from the approved character illustration. Unity slices the
    /// four facing rows in their original order (Up, Right, Down, Left); no Animator
    /// or hierarchy setup is required. It never changes the sprites of other pets.
    /// </summary>
    public static class FireHawkFinalArt
    {
        private static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();
        private static readonly HashSet<string> Errors = new HashSet<string>();

        private const string Root = "Equipment/FireHawkFinal/";

        public static Sprite Flight(int facing, int frame)
        {
            return Get(Root + "Flight/FireHawk_Fly4x4", 4, 4, facing, frame, "Flight");
        }

        public static Sprite Cast(int facing, int frame, bool level5)
        {
            string name = level5 ? "FireHawk_CastLv5_4x5" : "FireHawk_CastLv1_4x5";
            return Get(Root + "Cast/" + name, 5, 4, facing, frame, level5 ? "CastLv5" : "CastLv1");
        }

        public static Sprite Flame(int frame)
        {
            return Get(Root + "Vfx/FireHawk_TwinFlame8x1", 8, 1, 0, frame, "Flame");
        }

        private static Sprite Get(string path, int columns, int rows, int facing, int frame, string title)
        {
            if (!Cache.TryGetValue(path, out Sprite[] sprites))
            {
                Texture2D texture = Resources.Load<Texture2D>(path);
                if (texture == null || texture.width != 256 * columns || texture.height != 256 * rows)
                {
                    if (Errors.Add(path)) Debug.LogError("DienGame: FireHawk sprite atlas missing/wrong dimensions: Resources/" + path);
                    return null;
                }
                sprites = new Sprite[columns * rows];
                for (int row = 0; row < rows; row++)
                for (int col = 0; col < columns; col++)
                {
                    Rect rect = new Rect(col * 256, (rows - 1 - row) * 256, 256, 256);
                    Sprite image = Sprite.Create(texture, rect, new Vector2(.5f, .5f), 185f, 0, SpriteMeshType.FullRect);
                    image.name = "FireHawkFinal_" + title + "_D" + row + "_F" + col;
                    sprites[row * columns + col] = image;
                }
                Cache[path] = sprites;
            }
            return sprites[Mathf.Clamp(facing, 0, rows - 1) * columns + Mathf.Clamp(frame, 0, columns - 1)];
        }
    }
}
