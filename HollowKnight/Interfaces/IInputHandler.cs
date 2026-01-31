using UnityEngine;

namespace HollowKnight.Interfaces
{
    /// <summary>
    /// Abstraction layer for input handling.
    /// Provides a consistent interface for keyboard input across different mod frameworks.
    /// </summary>
    public interface IInputHandler
    {
        bool GetKeyDown(KeyCode key);
    }
}

