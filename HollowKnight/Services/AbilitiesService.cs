using System;
using HollowKnight.Interfaces;
using HollowKnight.Core;

namespace HollowKnight.Services
{
    /// <summary>
    /// Service for managing player abilities and movement upgrades.
    /// Supports toggling abilities on/off like Silksong's cheat system.
    /// </summary>
    public class AbilitiesService
    {
        private readonly IModLogger logger;
        private object heroController;

        public AbilitiesService(IModLogger logger)
        {
            this.logger = logger;
        }

        public void SetHeroController(object controller)
        {
            heroController = controller;
        }

        #region State Checking Methods

        public bool IsDoubleJumpUnlocked()
        {
            return GetAbilityBool("hasDoubleJump", false);
        }

        public bool IsDashUnlocked()
        {
            return GetAbilityBool("hasDash", false) && GetAbilityBool("canDash", false);
        }

        public bool IsShadowDashUnlocked()
        {
            return GetAbilityBool("hasShadowDash", false) && GetAbilityBool("canShadowDash", false);
        }

        public bool IsWallJumpUnlocked()
        {
            return GetAbilityBool("hasWalljump", false) && GetAbilityBool("canWallJump", false);
        }

        public bool IsSuperDashUnlocked()
        {
            return GetAbilityBool("hasSuperDash", false);
        }

        public bool IsAcidArmorUnlocked()
        {
            return GetAbilityBool("hasAcidArmour", false);
        }

        public bool IsDreamgateUnlocked()
        {
            return GetAbilityBool("hasDreamGate", false);
        }

        #endregion

        #region Toggle Methods

        public bool ToggleDoubleJump(Action<string> onSuccess = null, Action<string> onError = null)
        {
            bool currentState = IsDoubleJumpUnlocked();
            bool newState = !currentState;
            
            if (SetAbility("hasDoubleJump", newState, onError))
            {
                onSuccess?.Invoke($"Double Jump {(newState ? "unlocked" : "locked")}");
                return true;
            }
            return false;
        }

        public bool ToggleDash(Action<string> onSuccess = null, Action<string> onError = null)
        {
            bool currentState = IsDashUnlocked();
            bool newState = !currentState;
            
            bool result = SetAbility("hasDash", newState, onError) && SetAbility("canDash", newState, onError);
            if (result)
            {
                onSuccess?.Invoke($"Dash {(newState ? "unlocked" : "locked")}");
            }
            return result;
        }

        public bool ToggleShadowDash(Action<string> onSuccess = null, Action<string> onError = null)
        {
            bool currentState = IsShadowDashUnlocked();
            bool newState = !currentState;
            
            // If enabling, ensure regular dash is on
            if (newState)
            {
                SetAbility("hasDash", true, null);
                SetAbility("canDash", true, null);
            }
            
            bool result = SetAbility("hasShadowDash", newState, onError) && SetAbility("canShadowDash", newState, onError);
            if (result)
            {
                onSuccess?.Invoke($"Shadow Dash {(newState ? "unlocked" : "locked")}");
            }
            return result;
        }

        public bool ToggleWallJump(Action<string> onSuccess = null, Action<string> onError = null)
        {
            bool currentState = IsWallJumpUnlocked();
            bool newState = !currentState;
            
            bool result = SetAbility("hasWalljump", newState, onError) && SetAbility("canWallJump", newState, onError);
            if (result)
            {
                onSuccess?.Invoke($"Wall Jump {(newState ? "unlocked" : "locked")}");
            }
            return result;
        }

        public bool ToggleSuperDash(Action<string> onSuccess = null, Action<string> onError = null)
        {
            bool currentState = IsSuperDashUnlocked();
            bool newState = !currentState;
            
            if (SetAbility("hasSuperDash", newState, onError))
            {
                onSuccess?.Invoke($"Super Dash {(newState ? "unlocked" : "locked")}");
                return true;
            }
            return false;
        }

        public bool ToggleAcidArmor(Action<string> onSuccess = null, Action<string> onError = null)
        {
            bool currentState = IsAcidArmorUnlocked();
            bool newState = !currentState;
            
            if (SetAbility("hasAcidArmour", newState, onError))
            {
                onSuccess?.Invoke($"Acid Armor {(newState ? "unlocked" : "locked")}");
                return true;
            }
            return false;
        }

        public bool ToggleDreamgate(Action<string> onSuccess = null, Action<string> onError = null)
        {
            bool currentState = IsDreamgateUnlocked();
            bool newState = !currentState;
            
            if (SetAbility("hasDreamGate", newState, onError))
            {
                onSuccess?.Invoke($"Dreamgate {(newState ? "unlocked" : "locked")}");
                return true;
            }
            return false;
        }

        #endregion

        #region Bulk Unlock Methods

        public bool UnlockAllMovement(Action<string> onSuccess = null, Action<string> onError = null)
        {
            try
            {
                SetAbility("hasDoubleJump", true, null);
                SetAbility("hasDash", true, null);
                SetAbility("canDash", true, null);
                SetAbility("hasShadowDash", true, null);
                SetAbility("canShadowDash", true, null);
                SetAbility("hasWalljump", true, null);
                SetAbility("canWallJump", true, null);
                SetAbility("hasSuperDash", true, null);
                SetAbility("hasAcidArmour", true, null);
                SetAbility("hasDreamGate", true, null);
                
                onSuccess?.Invoke("All abilities unlocked!");
                return true;
            }
            catch (Exception e)
            {
                onError?.Invoke($"Error unlocking all abilities: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        private bool GetAbilityBool(string fieldName, bool defaultValue)
        {
            if (heroController == null)
                return defaultValue;

            object playerData = PlayerDataHelper.GetPlayerData(heroController, (error) => logger.Log($"AbilitiesService.{fieldName}: {error}"));
            if (playerData == null)
                return defaultValue;

            return PlayerDataHelper.GetBool(playerData, fieldName, defaultValue, (error) => logger.Log($"AbilitiesService.{fieldName}: {error}"));
        }

        private bool SetAbility(string fieldName, bool value, Action<string> onError)
        {
            if (heroController == null)
            {
                onError?.Invoke("Hero controller not available");
                return false;
            }

            object playerData = PlayerDataHelper.GetPlayerData(heroController, (error) => 
            {
                logger.Log($"AbilitiesService.{fieldName}: {error}");
                onError?.Invoke(error);
            });
            
            if (playerData == null)
                return false;

            return PlayerDataHelper.SetBool(playerData, fieldName, value, (error) => 
            {
                logger.Log($"AbilitiesService.{fieldName}: {error}");
                onError?.Invoke(error);
            });
        }

        #endregion
    }
}
