using System;
using HollowKnight.Interfaces;
using HollowKnight.Core;

namespace HollowKnight.Services
{
    /// <summary>
    /// Service for managing player spells.
    /// Supports toggling spells on/off like Silksong's cheat system.
    /// </summary>
    public class SpellsService
    {
        private readonly IModLogger logger;
        private object heroController;

        public SpellsService(IModLogger logger)
        {
            this.logger = logger;
        }

        public void SetHeroController(object controller)
        {
            heroController = controller;
        }

        #region State Checking Methods

        public bool IsFireballUnlocked()
        {
            return GetSpellInt("fireballLevel", 0) > 0;
        }

        public bool IsQuakeUnlocked()
        {
            return GetSpellInt("quakeLevel", 0) > 0;
        }

        public bool IsScreamUnlocked()
        {
            return GetSpellInt("screamLevel", 0) > 0;
        }

        #endregion

        #region Toggle Methods

        public bool ToggleFireball(Action<string> onSuccess = null, Action<string> onError = null)
        {
            int currentLevel = GetSpellInt("fireballLevel", 0);
            int newLevel = (currentLevel + 1) % 3; // Cycle: 0 → 1 → 2 → 0
            
            // Ensure hasSpell is true if unlocking
            if (newLevel > 0)
            {
                SetSpellBool("hasSpell", true, null);
            }
            
            if (SetSpellInt("fireballLevel", newLevel, onError))
            {
                string message = newLevel == 0 ? "Fireball locked" : 
                                newLevel == 1 ? "Fireball [1/2]" : "Fireball [2/2] (Shade Soul)";
                onSuccess?.Invoke(message);
                return true;
            }
            return false;
        }

        public bool ToggleQuake(Action<string> onSuccess = null, Action<string> onError = null)
        {
            int currentLevel = GetSpellInt("quakeLevel", 0);
            int newLevel = (currentLevel + 1) % 3; // Cycle: 0 → 1 → 2 → 0
            
            if (newLevel > 0)
            {
                SetSpellBool("hasSpell", true, null);
            }
            
            if (SetSpellInt("quakeLevel", newLevel, onError))
            {
                string message = newLevel == 0 ? "Quake locked" : 
                                newLevel == 1 ? "Quake [1/2]" : "Quake [2/2] (Descending Dark)";
                onSuccess?.Invoke(message);
                return true;
            }
            return false;
        }

        public bool ToggleScream(Action<string> onSuccess = null, Action<string> onError = null)
        {
            int currentLevel = GetSpellInt("screamLevel", 0);
            int newLevel = (currentLevel + 1) % 3; // Cycle: 0 → 1 → 2 → 0
            
            if (newLevel > 0)
            {
                SetSpellBool("hasSpell", true, null);
            }
            
            if (SetSpellInt("screamLevel", newLevel, onError))
            {
                string message = newLevel == 0 ? "Scream locked" : 
                                newLevel == 1 ? "Scream [1/2]" : "Scream [2/2] (Abyss Shriek)";
                onSuccess?.Invoke(message);
                return true;
            }
            return false;
        }

        #endregion

        #region Spell Level Getters

        /// <summary>
        /// Gets the current Fireball spell level (0 = locked, 1 = Vengeful Spirit, 2 = Shade Soul).
        /// </summary>
        public int GetFireballLevel()
        {
            return GetSpellInt("fireballLevel", 0);
        }

        /// <summary>
        /// Gets the current Quake spell level (0 = locked, 1 = Desolate Dive, 2 = Descending Dark).
        /// </summary>
        public int GetQuakeLevel()
        {
            return GetSpellInt("quakeLevel", 0);
        }

        /// <summary>
        /// Gets the current Scream spell level (0 = locked, 1 = Howling Wraiths, 2 = Abyss Shriek).
        /// </summary>
        public int GetScreamLevel()
        {
            return GetSpellInt("screamLevel", 0);
        }

        #endregion

        #region Bulk Unlock Methods

        public bool UnlockAllSpells(Action<string> onSuccess = null, Action<string> onError = null)
        {
            try
            {
                SetSpellBool("hasSpell", true, null);
                SetSpellInt("fireballLevel", 2, null);
                SetSpellInt("quakeLevel", 2, null);
                SetSpellInt("screamLevel", 2, null);
                
                onSuccess?.Invoke("All spells unlocked at max level!");
                return true;
            }
            catch (Exception e)
            {
                onError?.Invoke($"Error unlocking all spells: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        private int GetSpellInt(string fieldName, int defaultValue)
        {
            if (heroController == null)
                return defaultValue;

            object playerData = PlayerDataHelper.GetPlayerData(heroController, (error) => logger.Log($"SpellsService.{fieldName}: {error}"));
            if (playerData == null)
                return defaultValue;

            return PlayerDataHelper.GetInt(playerData, fieldName, defaultValue, (error) => logger.Log($"SpellsService.{fieldName}: {error}"));
        }

        private bool SetSpellInt(string fieldName, int value, Action<string> onError)
        {
            if (heroController == null)
            {
                onError?.Invoke("Hero controller not available");
                return false;
            }

            object playerData = PlayerDataHelper.GetPlayerData(heroController, (error) => 
            {
                logger.Log($"SpellsService.{fieldName}: {error}");
                onError?.Invoke(error);
            });
            
            if (playerData == null)
                return false;

            return PlayerDataHelper.SetInt(playerData, fieldName, value, (error) => 
            {
                logger.Log($"SpellsService.{fieldName}: {error}");
                onError?.Invoke(error);
            });
        }

        private bool SetSpellBool(string fieldName, bool value, Action<string> onError)
        {
            if (heroController == null)
            {
                onError?.Invoke("Hero controller not available");
                return false;
            }

            object playerData = PlayerDataHelper.GetPlayerData(heroController, (error) => 
            {
                logger.Log($"SpellsService.{fieldName}: {error}");
                onError?.Invoke(error);
            });
            
            if (playerData == null)
                return false;

            return PlayerDataHelper.SetBool(playerData, fieldName, value, (error) => 
            {
                logger.Log($"SpellsService.{fieldName}: {error}");
                onError?.Invoke(error);
            });
        }

        #endregion
    }
}
