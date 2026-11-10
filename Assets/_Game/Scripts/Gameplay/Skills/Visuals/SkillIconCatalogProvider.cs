using UnityEngine;

namespace DienGame.Gameplay.Skills.Visuals
{
    public static class SkillIconCatalogProvider
    {
        private const string ResourceName = "SkillIconCatalog";

        private static SkillIconCatalog _catalog;
        private static bool _loadAttempted;
        private static bool _warningShown;

        public static SkillIconCatalog Catalog
        {
            get
            {
                EnsureLoaded();
                return _catalog;
            }
        }

        public static Sprite GetIcon(SkillId skillId)
        {
            EnsureLoaded();
            return _catalog != null
                ? _catalog.GetIcon(skillId)
                : null;
        }

        public static Sprite JudgementIcon
        {
            get
            {
                EnsureLoaded();
                return _catalog != null
                    ? _catalog.JudgementIcon
                    : null;
            }
        }

        public static void Reload()
        {
            _catalog = null;
            _loadAttempted = false;
            _warningShown = false;
            EnsureLoaded();
        }

        private static void EnsureLoaded()
        {
            if (_loadAttempted)
            {
                return;
            }

            _loadAttempted = true;
            _catalog = Resources.Load<SkillIconCatalog>(ResourceName);

            if (_catalog == null && !_warningShown)
            {
                _warningShown = true;
                Debug.LogWarning(
                    "SkillIconCatalogProvider: Không tìm thấy Resources/SkillIconCatalog.asset. " +
                    "Trong Editor hãy chọn DienGame > Skills > Rebuild Skill Icon Catalog.");
            }
        }
    }
}
