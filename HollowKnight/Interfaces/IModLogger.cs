namespace HollowKnight.Interfaces
{
    /// <summary>
    /// Abstraction layer for logging across different mod frameworks.
    /// Allows the same codebase to work with both BepInEx and MelonLoader logging systems.
    /// </summary>
    public interface IModLogger
    {
        void Log(string message);
    }
}

