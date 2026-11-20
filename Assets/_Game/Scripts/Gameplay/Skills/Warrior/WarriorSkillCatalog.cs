using System.Collections.Generic;
using DienGame.Gameplay.Skills;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Warrior
{
    /// <summary>
    /// Gameplay-only interpretations of the Quy Tang field/biomechanical setting;
    /// not a declaration of new canon. Max rank five for all seven passives.
    /// </summary>
    public static class WarriorSkillCatalog
    {
        public static readonly SkillId[] Passives =
        {
            SkillId.WarriorDrive, SkillId.WarriorBulwark,
            SkillId.WarriorShockwave, SkillId.WarriorHeat,
            SkillId.WarriorBreaker, SkillId.WarriorTempo,
            SkillId.WarriorRegeneration
        };
        public static string Name(SkillId id)
        {
            switch (id)
            {
                case SkillId.WarriorDrive: return "Trục Lực Cộng Hưởng";
                case SkillId.WarriorBulwark: return "Giáp Phản Xung";
                case SkillId.WarriorShockwave: return "Dư Chấn Trường X";
                case SkillId.WarriorHeat: return "Buồng Nhiệt Tăng Áp";
                case SkillId.WarriorBreaker: return "Phá Kết Cấu";
                case SkillId.WarriorTempo: return "Nhịp Kiếm Đồng Bộ";
                case SkillId.WarriorRegeneration: return "Tái Tạo Sinh Cơ";
                default: return "Kỹ năng Đấu sĩ";
            }
        }
        public static string Description(SkillId id, int level)
        {
            int lv = Mathf.Clamp(level, 1, 5);
            switch (id)
            {
                case SkillId.WarriorDrive: return $"Đòn thứ ba trong chuỗi tăng thêm {lv * 7}% lực. Không cộng dồn.";
                case SkillId.WarriorBulwark: return $"Giảm {4 + lv * 2}% sát thương nhận vào. Không tạo bất tử.";
                case SkillId.WarriorShockwave: return $"Khi kết thúc combo, dư chấn đánh 2 mục tiêu gần, mỗi mục tiêu {10 + lv * 5}% lực; hồi 4 giây.";
                case SkillId.WarriorHeat: return $"Mỗi 5 đòn trúng mục tiêu, giải phóng xung nhiệt {12 + lv * 5}% lực; hồi 5 giây.";
                case SkillId.WarriorBreaker: return $"Đòn thường tăng {lv * 3}% lực lên Boss, không tăng khống chế.";
                case SkillId.WarriorTempo: return $"Rút ngắn chu kỳ đòn thường {lv * 3}%, tối đa 15%.";
                case SkillId.WarriorRegeneration: return $"Mỗi 5 đòn trúng hồi {lv * .35f:0.##}% máu tối đa; hồi 12 giây.";
                default: return "Kỹ năng chiến đấu riêng của Đấu sĩ.";
            }
        }
    }
}
