using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Warrior
{
    /// <summary>
    /// Runtime eight-direction / four-frame sprite atlas loader. No Sprite Editor setup;
    /// source Warrior PNGs live in Resources/Warrior and are NOT the Assassin assets.
    /// Atlas rows top -> bottom: Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft.
    /// Gameplay direction indices are Down..DownRight and are rotated by four rows.
    /// </summary>
    public static class WarriorArt
    {
        private const int Columns = 4;
        private const int Rows = 8;
        private const float PixelsPerUnit = 40f;
        private static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();
        private static readonly HashSet<string> MissingWarnings = new HashSet<string>();

        public static Sprite Run(int direction, int frame) => Frame("Warrior/WarriorRun8x4", direction, frame);
        public static Sprite Slash(int direction, int frame) => Frame("Warrior/WarriorSlash8x4", direction, frame);

        private static Sprite Frame(string path, int direction, int frame)
        {
            if (!Cache.TryGetValue(path, out Sprite[] frames))
            {
                Texture2D texture = Resources.Load<Texture2D>(path);
                if (texture == null || texture.width % Columns != 0 || texture.height % Rows != 0)
                {
                    if (MissingWarnings.Add(path))
                        Debug.LogWarning("[DienGame WarriorArt] Missing/invalid 4x8 atlas: " + path);
                    Cache[path] = new Sprite[Columns * Rows];
                    return null;
                }
                frames = new Sprite[Columns * Rows];
                int cw = texture.width / Columns;
                int ch = texture.height / Rows;
                for (int y = 0; y < Rows; y++)
                for (int x = 0; x < Columns; x++)
                {
                    Rect rect = new Rect(x * cw, (Rows - 1 - y) * ch, cw, ch);
                    frames[y * Columns + x] = Sprite.Create(texture, rect, new Vector2(.5f, .5f),
                        PixelsPerUnit, 0, SpriteMeshType.FullRect);
                    frames[y * Columns + x].name = path + "_" + y + "_" + x;
                }
                Cache.Add(path, frames);
            }
            int atlasRow = (Mathf.Clamp(direction, 0, Rows - 1) + 4) % Rows;
            return frames[atlasRow * Columns + Mathf.Clamp(frame, 0, Columns - 1)];
        }
    }
}
