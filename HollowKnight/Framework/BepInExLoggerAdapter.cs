#if BEPINEX
using BepInEx.Logging;
using HollowKnight.Interfaces;

namespace HollowKnight.Framework
{
    /// <summary>
    /// BepInEx-specific logger implementation.
    /// Wraps BepInEx's ManualLogSource to match our IModLogger interface.
    /// </summary>
    public class BepInExLoggerAdapter : IModLogger
    {
        private readonly ManualLogSource logger;

        public BepInExLoggerAdapter(ManualLogSource logger)
        {
            this.logger = logger;
        }

        public void Log(string message)
        {
            logger.LogInfo(message);
        }
    }
}
#endif

