using VContainer;
using VContainer.Unity;
using UnityEngine;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private int _initialGold = 30;
    [SerializeField] private int _initialBaseHealth = 3;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<ObjectPool>().AsSelf();

        builder.RegisterComponentInHierarchy<EnemyMovementManager>()
            .As<IEnemyRegistry, IEnemyRouteTracker, IEnemyBaseArrivalSource>();

        builder.RegisterComponentInHierarchy<ProjectileManager>()
            .AsSelf();

        builder.Register<GoldManager>(Lifetime.Singleton)
            .WithParameter(_initialGold)
            .As<IGoldProvider, IGoldWallet>();

        builder.RegisterComponentInHierarchy<EnemySpawner>();
        builder.Register<BaseHealth>(Lifetime.Singleton)
            .WithParameter(_initialBaseHealth);
        builder.Register<GameModeManager>(Lifetime.Singleton);
        builder.RegisterComponentInHierarchy<BaseHealthCounterUI>();
        builder.RegisterComponentInHierarchy<GoldUI>();
        builder.RegisterComponentInHierarchy<TowerSelectionPanel>();
        builder.RegisterComponentInHierarchy<BuildingManager>();
        builder.RegisterComponentInHierarchy<PauseMenuUI>();
        builder.RegisterComponentInHierarchy<GameOverUI>();
    }
}
