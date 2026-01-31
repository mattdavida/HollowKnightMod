using UnityEngine;
using HollowKnight.Interfaces;

namespace HollowKnight.Framework
{
    /// <summary>
    /// Unity time handling adapter.
    /// Wraps Unity's Time class to provide a testable interface.
    /// </summary>
    public class UnityTimeAdapter : ITimeProvider
    {
        public float DeltaTime => UnityEngine.Time.deltaTime;
        public float Time => UnityEngine.Time.time;
    }
}

