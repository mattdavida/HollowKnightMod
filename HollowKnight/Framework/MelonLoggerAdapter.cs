#if MELONLOADER
using MelonLoader;
using HollowKnight.Interfaces;

namespace HollowKnight.Framework
{
    /// <summary>
    /// MelonLoader-specific logger implementation.
    /// Wraps MelonLoader's LoggerInstance to match our IModLogger interface.
    /// </summary>
    public class MelonLoggerAdapter : IModLogger
    {
        public void Log(string message)
        {
            MelonLogger.Msg(message);
        }
    }
}
#endif

