using JetBrains.Annotations;

namespace Common
{
    public enum Scenes
    {
        HomeScene = 0,
        GameScene = 100,
        PreLvl1Scene = 1,
        [UsedImplicitly] PreLvl2Scene = 2,
        [UsedImplicitly] PreLvl3Scene = 3,
        EndScene = 4,
    }
}
