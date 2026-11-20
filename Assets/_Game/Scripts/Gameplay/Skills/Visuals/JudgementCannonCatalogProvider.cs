using UnityEngine;

namespace DienGame.Gameplay.Skills.Visuals
{
    public static class JudgementCannonCatalogProvider
    {
        private const string ResourceName = "JudgementCannonCatalog";

        private static JudgementCannonCatalog _catalog;
        private static bool _loadAttempted;
        private static bool _warningShown;

        public static Sprite[] CannonFrames
        {
            get
            {
                EnsureLoaded();
                return _catalog != null
                    ? _catalog.CannonFrames
                    : System.Array.Empty<Sprite>();
            }
        }

        public static Sprite[] BeamFrames
        {
            get
            {
                EnsureLoaded();
                return _catalog != null
                    ? _catalog.BeamFrames
                    : System.Array.Empty<Sprite>();
            }
        }

        public static Sprite[] ShockwaveFrames
        {
            get
            {
                EnsureLoaded();
                return _catalog != null
                    ? _catalog.ShockwaveFrames
                    : System.Array.Empty<Sprite>();
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
            _catalog = Resources.Load<JudgementCannonCatalog>(ResourceName);

            if (_catalog == null && !_warningShown)
            {
                _warningShown = true;
                Debug.LogWarning(
                    "JudgementCannonCatalogProvider: Không tìm thấy Resources/JudgementCannonCatalog.asset. " +
                    "Trong Editor hãy chọn DienGame > Skills > Rebuild Judgement Cannon Catalog."
                );
            }
        }
    }
}
