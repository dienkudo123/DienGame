using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    [CreateAssetMenu(
        fileName = "EnemyCatalog",
        menuName = "DienGame/Enemies/Enemy Catalog")]
    public sealed class EnemyCatalog : ScriptableObject
    {
        [SerializeField] private List<EnemyDefinition> definitions = new();

        public IReadOnlyList<EnemyDefinition> Definitions => definitions;

        public bool TryGet(
            EnemyArchetype archetype,
            out EnemyDefinition definition)
        {
            for (int i = 0; i < definitions.Count; i++)
            {
                EnemyDefinition candidate = definitions[i];

                if (candidate != null && candidate.Archetype == archetype)
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public EnemyDefinition Get(EnemyArchetype archetype)
        {
            if (TryGet(archetype, out EnemyDefinition definition))
            {
                return definition;
            }

            Debug.LogError(
                $"EnemyCatalog: không có definition cho {archetype}."
            );

            return null;
        }

        public void ReplaceDefinitions(List<EnemyDefinition> newDefinitions)
        {
            definitions = newDefinitions ?? new List<EnemyDefinition>();
        }
    }
}
