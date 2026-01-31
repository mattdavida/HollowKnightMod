using UnityEngine;
using HollowKnight.Interfaces;

namespace HollowKnight.Framework
{
    /// <summary>
    /// Unity input handling adapter.
    /// Wraps Unity's Input class to provide a testable interface.
    /// </summary>
    public class UnityInputAdapter : IInputHandler
    {
        public bool GetKeyDown(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }
    }
}

