using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    public static class EnemyCatalogProvider
    {
        private const string ResourceName = "EnemyCatalog";
        private static EnemyCatalog _catalog;

        public static EnemyCatalog Catalog
        {
            get
            {
                if (_catalog == null)
                {
                    _catalog = Resources.Load<EnemyCatalog>(ResourceName);
                }

                if (_catalog == null)
                {
                    Debug.LogError(
                        "EnemyCatalogProvider: không tìm thấy Assets/_Game/Resources/EnemyCatalog.asset. " +
                        "Trong Unity chọn DienGame > Enemies > Rebuild Enemy Catalog."
                    );
                }

                return _catalog;
            }
        }

        public static EnemyDefinition Get(EnemyArchetype archetype)
        {
            return Catalog != null
                ? Catalog.Get(archetype)
                : null;
        }

        public static void ClearCache()
        {
            _catalog = null;
        }
    }
}
