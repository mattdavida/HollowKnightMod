namespace HollowKnight.Interfaces
{
    /// <summary>
    /// Abstraction layer for time-based operations.
    /// Provides consistent time access across different mod frameworks.
    /// </summary>
    public interface ITimeProvider
    {
        float DeltaTime { get; }
        float Time { get; }
    }
}

