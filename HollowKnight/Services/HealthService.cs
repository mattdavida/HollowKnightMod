using System;
using System.Reflection;
using UnityEngine;
using HollowKnight.Interfaces;
using HollowKnight.Core;

namespace HollowKnight.Services
{
    /// <summary>
    /// Service for managing player health operations.
    /// Handles adding health and setting max health for Hollow Knight.
    /// </summary>
    public class HealthService
    {
        private readonly IModLogger logger;
        private Component heroController;
        public HealthService(IModLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Sets the hero controller reference. Must be called when hero is available.
        /// </summary>
        /// <param name="heroController">The hero controller component</param>
        public void SetHeroController(Component heroController)
        {
            this.heroController = heroController;
        }

        /// <summary>
        /// Adds the specified amount of health to the player.
        /// Calls HeroController.AddHealth which internally calls PlayerData.AddHealth and sends FSM events.
        /// </summary>
        /// <param name="amount">Amount of health to add (can be negative to remove)</param>
        /// <param name="onSuccess">Callback for success message</param>
        /// <param name="onError">Callback for error message</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool AddHealth(int amount, Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                string error = "Cannot add health: Hero controller not available";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }

            try
            {
                Type heroType = heroController.GetType();
                
                // Call HeroController.AddHealth(int) - this calls playerData.AddHealth AND sends FSM event
                MethodInfo addHealthMethod = heroType.GetMethod("AddHealth");
                if (addHealthMethod != null)
                {
                    addHealthMethod.Invoke(heroController, new object[] { amount });
                    string success = $"Added {amount} health";
                    logger.Log(success);
                    onSuccess?.Invoke(success);
                    return true;
                }
                else
                {
                    string error = "AddHealth method not found on HeroController";
                    logger.Log(error);
                    onError?.Invoke(error);
                    return false;
                }
            }
            catch (Exception e)
            {
                string error = $"Error adding health: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Sets the exact maximum health for the player.
        /// Smart logic: calculates difference between current and target, then applies the delta.
        /// </summary>
        /// <param name="targetMaxHealth">Target max health value (1-10)</param>
        /// <param name="onSuccess">Callback for success message</param>
        /// <param name="onError">Callback for error message</param>
        /// <returns>True if operation successful, false otherwise</returns>
        public bool SetMaxHealthExact(int targetMaxHealth, Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                string error = "Cannot set max health: Hero controller not available";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }

            try
            {
                // Validate minimum health
                if (targetMaxHealth < 1)
                {
                    string warning = "Warning: Minimum health is 1!";
                    logger.Log("Attempted to set max health below 1 - setting to 1 instead");
                    onError?.Invoke(warning);
                    targetMaxHealth = 1;
                }

                // Validate maximum to prevent UI issues (Hollow Knight UI caps at 10)
                if (targetMaxHealth > 10)
                {
                    string warning = "Warning: Max health capped at 10!";
                    logger.Log("Attempted to set max health above 10 - capping at 10");
                    onError?.Invoke(warning);
                    targetMaxHealth = 10;
                }

                // Get playerData from HeroController using helper
                object playerData = PlayerDataHelper.GetPlayerData(heroController, (error) =>
                {
                    logger.Log($"HealthService.SetMaxHealthExact: {error}");
                    onError?.Invoke(error);
                });
                
                if (playerData == null)
                {
                    return false;
                }
                
                // Get current maxHealth value using helper
                int currentMaxHealth = PlayerDataHelper.GetInt(playerData, "maxHealth", 0, (error) =>
                {
                    logger.Log($"HealthService.SetMaxHealthExact.maxHealth: {error}");
                    onError?.Invoke(error);
                });
                int difference = targetMaxHealth - currentMaxHealth;
                
                logger.Log($"Current max health: {currentMaxHealth}, Target: {targetMaxHealth}, Difference: {difference}");
                
                // Call HeroController.AddToMaxHealth(int) - this calls playerData.AddToMaxHealth AND awards achievements
                Type heroType = heroController.GetType();
                MethodInfo addToMaxHealthMethod = heroType.GetMethod("AddToMaxHealth");
                if (addToMaxHealthMethod != null)
                {
                    addToMaxHealthMethod.Invoke(heroController, new object[] { difference });
                    logger.Log($"Set max health to {targetMaxHealth}");
                    
                    string successMsg = $"Max health set to {targetMaxHealth} - Save & reload to see UI update!";
                    onSuccess?.Invoke(successMsg);
                    return true;
                }
                else
                {
                    string error = "AddToMaxHealth method not found on HeroController";
                    logger.Log(error);
                    onError?.Invoke(error);
                    return false;
                }
            }
            catch (Exception e)
            {
                string error = $"Error setting max health: {e.Message}";
                logger.Log($"Error setting exact max health: {e.Message}");
                onError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Refills player health to maximum.
        /// Calls HeroController.MaxHealth which internally calls PlayerData.MaxHealth and sends FSM events.
        /// </summary>
        /// <param name="onSuccess">Callback for success message</param>
        /// <param name="onError">Callback for error message</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool RefillHealth(Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                string error = "Cannot refill health: Hero controller not available";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }

            try
            {
                Type heroType = heroController.GetType();
                
                // Call HeroController.MaxHealth() - this calls playerData.MaxHealth AND sends FSM event
                MethodInfo maxHealthMethod = heroType.GetMethod("MaxHealth");
                if (maxHealthMethod != null)
                {
                    maxHealthMethod.Invoke(heroController, null);
                    string success = "Health refilled to maximum";
                    logger.Log(success);
                    onSuccess?.Invoke(success);
                    return true;
                }
                else
                {
                    string error = "MaxHealth method not found on HeroController";
                    logger.Log(error);
                    onError?.Invoke(error);
                    return false;
                }
            }
            catch (Exception e)
            {
                string error = $"Error refilling health: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }
    }
}

