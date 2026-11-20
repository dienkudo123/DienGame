namespace DienGame.Gameplay.Core.Pooling
{
    public interface IPoolLifecycle
    {
        void OnSpawnedFromPool();
        void OnReturnedToPool();
    }
}
