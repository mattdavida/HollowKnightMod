using System;
using System.Reflection;
using UnityEngine;
using HollowKnight.Interfaces;
using HollowKnight.Core;

namespace HollowKnight.Services
{
    /// <summary>
    /// Service for managing key items, charms, and collectibles.
    /// Based on logic from CheatManager for unlocking progression items.
    /// </summary>
    public class ItemsService
    {
        private readonly IModLogger logger;
        private object heroController;
        private int[] originalCharmCosts = null; // Store original charm costs for restoration

        public ItemsService(IModLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Sets the hero controller reference for item operations.
        /// </summary>
        public void SetHeroController(object controller)
        {
            heroController = controller;
        }

        #region State Checking Methods

        public bool IsDreamNailUnlocked()
        {
            return GetItemBool("hasDreamNail", false);
        }

        #endregion

        #region Toggle Methods

        /// <summary>
        /// Toggles the Dream Nail between locked and fully awakened.
        /// Toggle: Locked ↔ Awoken Dream Nail (max level)
        /// </summary>
        public bool ToggleDreamNail(Action<string> onSuccess = null, Action<string> onError = null)
        {
            bool hasDreamNail = GetItemBool("hasDreamNail", false);
            bool isUpgraded = GetItemBool("dreamNailUpgraded", false);
            
            // If either is unlocked, lock both. Otherwise, unlock both to max.
            bool shouldUnlock = !hasDreamNail && !isUpgraded;
            
            if (shouldUnlock)
            {
                // Locked → Awoken Dream Nail (max level)
                SetItem("hasDreamNail", true, null, null, null);
                if (SetItem("dreamNailUpgraded", true, null, null, onError))
                {
                    onSuccess?.Invoke("Dream Nail unlocked (Awoken)");
                    return true;
                }
            }
            else
            {
                // Any unlocked state → Locked
                SetItem("hasDreamNail", false, null, null, null);
                if (SetItem("dreamNailUpgraded", false, null, null, onError))
                {
                    onSuccess?.Invoke("Dream Nail locked");
                    return true;
                }
            }
            
            return false;
        }

        #endregion

        #region Bulk Unlock Methods

        /// <summary>
        /// Unlocks the Dream Nail (used by bulk operations).
        /// </summary>
        public bool UnlockDreamNail(Action<string> onSuccess = null, Action<string> onError = null)
        {
            return SetItem("hasDreamNail", true, "Dream Nail unlocked!", onSuccess, onError);
        }

        /// <summary>
        /// Upgrades the Dream Nail to Awoken Dream Nail.
        /// Also unlocks regular Dream Nail if not already unlocked.
        /// </summary>
        public bool UpgradeDreamNail(Action<string> onSuccess = null, Action<string> onError = null)
        {
            // Ensure Dream Nail is unlocked first
            SetItem("hasDreamNail", true, null, null, null);
            return SetItem("dreamNailUpgraded", true, "Dream Nail upgraded to Awoken Dream Nail!", onSuccess, onError);
        }

        /// <summary>
        /// Unlocks all nail arts (Great Slash, Dash Slash, Cyclone Slash).
        /// </summary>
        public bool UnlockAllNailArts(Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                string error = "Cannot unlock nail arts: Hero controller not available";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }

            try
            {
                Type heroType = heroController.GetType();
                FieldInfo playerDataField = heroType.GetField("playerData", BindingFlags.Public | BindingFlags.Instance);

                if (playerDataField != null)
                {
                    object playerData = playerDataField.GetValue(heroController);
                    Type playerDataType = playerData.GetType();

                    SetField(playerData, playerDataType, "hasNailArt", true);
                    SetField(playerData, playerDataType, "hasDashSlash", true);
                    SetField(playerData, playerDataType, "hasCyclone", true);
                    SetField(playerData, playerDataType, "hasUpwardSlash", true);
                    SetField(playerData, playerDataType, "hasAllNailArts", true);

                    string success = "All Nail Arts unlocked!";
                    logger.Log(success);
                    onSuccess?.Invoke(success);
                    return true;
                }
                else
                {
                    string error = "playerData field not found";
                    logger.Log(error);
                    onError?.Invoke(error);
                    return false;
                }
            }
            catch (Exception e)
            {
                string error = $"Error unlocking nail arts: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Unlocks all 40 charms and sets charm slots to 11.
        /// </summary>
        public bool UnlockAllCharms(Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                string error = "Cannot unlock charms: Hero controller not available";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }

            try
            {
                Type heroType = heroController.GetType();
                FieldInfo playerDataField = heroType.GetField("playerData", BindingFlags.Public | BindingFlags.Instance);

                if (playerDataField != null)
                {
                    object playerData = playerDataField.GetValue(heroController);
                    Type playerDataType = playerData.GetType();

                    SetField(playerData, playerDataType, "hasCharm", true);
                    
                    // Unlock all 40 charms
                    for (int i = 1; i <= 40; i++)
                    {
                        SetField(playerData, playerDataType, $"gotCharm_{i}", true);
                    }

                    // Set Kingsoul/Void Heart to completed state
                    SetField(playerData, playerDataType, "royalCharmState", 3);
                    
                    // Set charm slots to 12 (max with all notch upgrades + overcharm)
                    SetField(playerData, playerDataType, "charmSlots", 12);

                    string success = "All 40 charms unlocked with 12 charm slots!";
                    logger.Log(success);
                    onSuccess?.Invoke(success);
                    return true;
                }
                else
                {
                    string error = "playerData field not found";
                    logger.Log(error);
                    onError?.Invoke(error);
                    return false;
                }
            }
            catch (Exception e)
            {
                string error = $"Error unlocking charms: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Sets the number of charm slots (notches).
        /// </summary>
        public bool SetCharmSlots(int slots, Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                onError?.Invoke("Cannot set charm slots: Hero controller not available");
                return false;
            }

            if (slots < 0)
            {
                onError?.Invoke("Charm slots cannot be negative");
                return false;
            }

            object playerData = PlayerDataHelper.GetPlayerData(heroController, onError);
            if (playerData == null) return false;

            if (PlayerDataHelper.SetInt(playerData, "charmSlots", slots, onError))
            {
                onSuccess?.Invoke($"Charm slots set to {slots}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the current number of charm slots.
        /// </summary>
        public int GetCharmSlots()
        {
            if (heroController == null) return 0;
            object playerData = PlayerDataHelper.GetPlayerData(heroController);
            if (playerData == null) return 0;
            return PlayerDataHelper.GetInt(playerData, "charmSlots", 0);
        }

        /// <summary>
        /// Toggles a specific charm (1-40, including Kingsoul/Void Heart at 36).
        /// </summary>
        public bool ToggleCharm(int charmNumber, Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (charmNumber < 1 || charmNumber > 40)
            {
                onError?.Invoke($"Invalid charm number: {charmNumber}");
                return false;
            }

            if (heroController == null)
            {
                onError?.Invoke("Cannot toggle charm: Hero controller not available");
                return false;
            }

            object playerData = PlayerDataHelper.GetPlayerData(heroController, onError);
            if (playerData == null) return false;

            bool currentState = PlayerDataHelper.GetBool(playerData, $"gotCharm_{charmNumber}", false);
            bool newState = !currentState;

            // Ensure hasCharm is true when unlocking any charm
            if (newState)
            {
                PlayerDataHelper.SetBool(playerData, "hasCharm", true, null);
            }

            if (PlayerDataHelper.SetBool(playerData, $"gotCharm_{charmNumber}", newState, onError))
            {
                // Special handling for Charm 36 (Kingsoul/Void Heart)
                // royalCharmState: 0=none, 1=half1, 2=half2(Kingsoul), 3=Void Heart
                if (charmNumber == 36)
                {
                    int royalState = newState ? 3 : 0; // Unlock to Void Heart, lock to none
                    PlayerDataHelper.SetInt(playerData, "royalCharmState", royalState, null);
                }

                string status = newState ? "unlocked" : "locked";
                onSuccess?.Invoke($"Charm {charmNumber} {status}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a specific charm is unlocked.
        /// </summary>
        public bool IsCharmUnlocked(int charmNumber)
        {
            if (charmNumber < 1 || charmNumber > 40) return false;
            if (heroController == null) return false;
            
            object playerData = PlayerDataHelper.GetPlayerData(heroController);
            if (playerData == null) return false;
            
            return PlayerDataHelper.GetBool(playerData, $"gotCharm_{charmNumber}", false);
        }

        /// <summary>
        /// Returns array of all charm names for the dropdown.
        /// Format: "CharmNumber|Name" to avoid index mapping issues.
        /// Gets names from CharmDisplay instances (id field maps directly to charm number).
        /// </summary>
        public string[] GetCharmNames()
        {
            // Hollow Knight has 40 charms including Kingsoul/Void Heart (charm 36)
            var charmNames = new System.Collections.Generic.List<string>();

            var nameById = GetCharmNamesByIdFromCharmDisplays();

            for (int i = 1; i <= 40; i++)
            {
                string charmName;

                if (nameById != null && nameById.TryGetValue(i, out string rawName))
                {
                    charmName = CleanCharmName(rawName);
                }
                else
                {
                    charmName = $"Charm {i}";
                }

                charmNames.Add($"{i}|{charmName}");
            }

            return charmNames.ToArray();
        }

        /// <summary>
        /// Finds all CharmDisplay MonoBehaviours and returns a mapping of charm id -> GameObject name.
        /// CharmDisplay.id is 1-indexed and matches the charm number used everywhere else.
        /// Uses Resources.FindObjectsOfTypeAll so inactive objects (charms menu not yet opened) are included.
        /// </summary>
        private System.Collections.Generic.Dictionary<int, string> GetCharmNamesByIdFromCharmDisplays()
        {
            try
            {
                Type charmDisplayType = Type.GetType("CharmDisplay, Assembly-CSharp");
                if (charmDisplayType == null)
                {
                    logger.Log("CharmDisplay type not found in Assembly-CSharp");
                    return null;
                }

                MethodInfo findAllMethod = typeof(Resources).GetMethod("FindObjectsOfTypeAll", new Type[] { typeof(Type) });
                if (findAllMethod == null)
                {
                    logger.Log("Resources.FindObjectsOfTypeAll method not found");
                    return null;
                }

                Array allObjects = findAllMethod.Invoke(null, new object[] { charmDisplayType }) as Array;

                if (allObjects == null || allObjects.Length == 0)
                {
                    logger.Log("No CharmDisplay instances found");
                    return null;
                }

                logger.Log($"Found {allObjects.Length} CharmDisplay instance(s)");

                FieldInfo idField = charmDisplayType.GetField("id", BindingFlags.Public | BindingFlags.Instance);
                if (idField == null)
                {
                    logger.Log("CharmDisplay.id field not found");
                    return null;
                }

                var nameById = new System.Collections.Generic.Dictionary<int, string>();

                foreach (object obj in allObjects)
                {
                    Component comp = obj as Component;
                    if (comp == null) continue;

                    int id = (int)idField.GetValue(obj);
                    if (id < 1 || id > 40) continue;

                    // Only record the first occurrence per id (avoids duplicates from scene reloads)
                    if (!nameById.ContainsKey(id))
                    {
                        nameById[id] = comp.gameObject.name;
                    }
                }

                logger.Log($"Resolved {nameById.Count} charm names from CharmDisplay instances");
                return nameById;
            }
            catch (Exception e)
            {
                logger.Log($"Error getting charm names from CharmDisplay: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Cleans up a charm GameObject name (e.g., "Charm GeoGatherer" -> "GeoGatherer").
        /// </summary>
        private string CleanCharmName(string gameObjectName)
        {
            if (string.IsNullOrEmpty(gameObjectName)) return "Unknown";
            
            // Remove "Charm " prefix if present
            if (gameObjectName.StartsWith("Charm "))
            {
                return gameObjectName.Substring(6);
            }
            
            return gameObjectName;
        }

        /// <summary>
        /// Unlocks all key items for progression (Lantern, Tram Pass, City Keys, King's Brand, etc.).
        /// </summary>
        public bool UnlockAllKeyItems(Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                string error = "Cannot unlock key items: Hero controller not available";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }

            try
            {
                Type heroType = heroController.GetType();
                FieldInfo playerDataField = heroType.GetField("playerData", BindingFlags.Public | BindingFlags.Instance);

                if (playerDataField != null)
                {
                    object playerData = playerDataField.GetValue(heroController);
                    Type playerDataType = playerData.GetType();

                    // Movement abilities
                    SetField(playerData, playerDataType, "hasDash", true);
                    SetField(playerData, playerDataType, "canDash", true);
                    SetField(playerData, playerDataType, "hasShadowDash", true);
                    SetField(playerData, playerDataType, "canShadowDash", true);
                    SetField(playerData, playerDataType, "hasWalljump", true);
                    SetField(playerData, playerDataType, "canWallJump", true);
                    SetField(playerData, playerDataType, "hasSuperDash", true);
                    
                    // Dream Nail
                    SetField(playerData, playerDataType, "hasDreamNail", true);
                    SetField(playerData, playerDataType, "dreamNailUpgraded", true);
                    
                    // Traversal items
                    SetField(playerData, playerDataType, "hasLantern", true);
                    SetField(playerData, playerDataType, "hasAcidArmour", true);
                    SetField(playerData, playerDataType, "hasTramPass", true);
                    
                    // Keys
                    SetField(playerData, playerDataType, "hasLoveKey", true);
                    SetField(playerData, playerDataType, "hasWhiteKey", true);
                    SetField(playerData, playerDataType, "hasSlykey", true);
                    SetField(playerData, playerDataType, "hasKingsBrand", true);
                    SetField(playerData, playerDataType, "simpleKeys", 5);

                    string success = "All key items unlocked!";
                    logger.Log(success);
                    onSuccess?.Invoke(success);
                    return true;
                }
                else
                {
                    string error = "playerData field not found";
                    logger.Log(error);
                    onError?.Invoke(error);
                    return false;
                }
            }
            catch (Exception e)
            {
                string error = $"Error unlocking key items: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Adds Dream Orbs to the player's essence count.
        /// </summary>
        public bool AddDreamOrbs(int amount, Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                string error = "Cannot add dream orbs: Hero controller not available";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }

            try
            {
                Type heroType = heroController.GetType();
                FieldInfo playerDataField = heroType.GetField("playerData", BindingFlags.Public | BindingFlags.Instance);

                if (playerDataField != null)
                {
                    object playerData = playerDataField.GetValue(heroController);
                    Type playerDataType = playerData.GetType();

                    FieldInfo dreamOrbsField = playerDataType.GetField("dreamOrbs");
                    if (dreamOrbsField != null)
                    {
                        int currentOrbs = (int)dreamOrbsField.GetValue(playerData);
                        int newTotal = currentOrbs + amount;
                        dreamOrbsField.SetValue(playerData, newTotal);

                        string success = $"Added {amount} Essence! Total: {newTotal}";
                        logger.Log(success);
                        onSuccess?.Invoke(success);
                        return true;
                    }
                    else
                    {
                        string error = "dreamOrbs field not found";
                        logger.Log(error);
                        onError?.Invoke(error);
                        return false;
                    }
                }
                else
                {
                    string error = "playerData field not found";
                    logger.Log(error);
                    onError?.Invoke(error);
                    return false;
                }
            }
            catch (Exception e)
            {
                string error = $"Error adding dream orbs: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to set a boolean item field.
        /// </summary>
        private bool SetItem(string fieldName, bool value, string successMessage, Action<string> onSuccess, Action<string> onError)
        {
            if (heroController == null)
            {
                string error = "Cannot set item: Hero controller not available";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }

            try
            {
                Type heroType = heroController.GetType();
                FieldInfo playerDataField = heroType.GetField("playerData", BindingFlags.Public | BindingFlags.Instance);

                if (playerDataField != null)
                {
                    object playerData = playerDataField.GetValue(heroController);
                    Type playerDataType = playerData.GetType();

                    FieldInfo itemField = playerDataType.GetField(fieldName);
                    if (itemField != null)
                    {
                        itemField.SetValue(playerData, value);
                        
                        if (successMessage != null)
                        {
                            logger.Log(successMessage);
                            onSuccess?.Invoke(successMessage);
                        }
                        return true;
                    }
                    else
                    {
                        string error = $"{fieldName} field not found";
                        logger.Log(error);
                        onError?.Invoke(error);
                        return false;
                    }
                }
                else
                {
                    string error = "playerData field not found";
                    logger.Log(error);
                    onError?.Invoke(error);
                    return false;
                }
            }
            catch (Exception e)
            {
                string error = $"Error setting {fieldName}: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Unlocks all map areas.
        /// Based on CheatManager's "All Map" functionality.
        /// </summary>
        public bool UnlockAllMaps(Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                string error = "Cannot unlock maps: Hero controller not available";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }

            try
            {
                Type heroType = heroController.GetType();
                FieldInfo playerDataField = heroType.GetField("playerData", BindingFlags.Public | BindingFlags.Instance);
                
                if (playerDataField != null)
                {
                    object playerData = playerDataField.GetValue(heroController);
                    Type playerDataType = playerData.GetType();
                    
                    // Unlock all map areas as per CheatManager
                    SetField(playerData, playerDataType, "hasMap", true);
                    SetField(playerData, playerDataType, "mapDirtmouth", true);
                    SetField(playerData, playerDataType, "mapCrossroads", true);
                    SetField(playerData, playerDataType, "mapGreenpath", true);
                    SetField(playerData, playerDataType, "mapFogCanyon", false);
                    SetField(playerData, playerDataType, "mapRoyalGardens", true);
                    SetField(playerData, playerDataType, "mapFungalWastes", false);
                    SetField(playerData, playerDataType, "mapCity", true);
                    SetField(playerData, playerDataType, "mapWaterways", false);
                    SetField(playerData, playerDataType, "mapMines", true);
                    SetField(playerData, playerDataType, "mapDeepnest", true);
                    SetField(playerData, playerDataType, "mapCliffs", true);
                    SetField(playerData, playerDataType, "mapOutskirts", true);
                    SetField(playerData, playerDataType, "mapRestingGrounds", true);
                    SetField(playerData, playerDataType, "mapAbyss", true);
                    SetField(playerData, playerDataType, "openedMapperShop", true);
                    
                    string success = "All map areas unlocked!";
                    logger.Log(success);
                    onSuccess?.Invoke(success);
                    return true;
                }
                else
                {
                    string error = "playerData field not found";
                    logger.Log(error);
                    onError?.Invoke(error);
                    return false;
                }
            }
            catch (Exception e)
            {
                string error = $"Error unlocking maps: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Unlocks all stag stations.
        /// Based on CheatManager's OpenStagStations method.
        /// </summary>
        public bool UnlockAllStags(Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                string error = "Cannot unlock stag stations: Hero controller not available";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }

            try
            {
                Type type = heroController.GetType();
                PropertyInfo playerDataProperty = type.GetProperty("playerData");
                
                if (playerDataProperty != null)
                {
                    object playerData = playerDataProperty.GetValue(heroController);
                    Type playerDataType = playerData.GetType();
                    
                    // Unlock all stag stations as per CheatManager
                    SetField(playerData, playerDataType, "openedTown", true);
                    SetField(playerData, playerDataType, "openedTownBuilding", true);
                    SetField(playerData, playerDataType, "openedCrossroads", true);
                    SetField(playerData, playerDataType, "openedGreenpath", true);
                    SetField(playerData, playerDataType, "openedRuins1", true);
                    SetField(playerData, playerDataType, "openedRuins2", true);
                    SetField(playerData, playerDataType, "openedFungalWastes", true);
                    SetField(playerData, playerDataType, "openedRoyalGardens", true);
                    SetField(playerData, playerDataType, "openedRestingGrounds", true);
                    SetField(playerData, playerDataType, "openedDeepnest", true);
                    SetField(playerData, playerDataType, "openedStagNest", true);
                    SetField(playerData, playerDataType, "openedHiddenStation", true);
                    SetField(playerData, playerDataType, "gladeDoorOpened", true);
                    SetField(playerData, playerDataType, "troupeInTown", true);
                    
                    string success = "All stag stations unlocked!";
                    logger.Log(success);
                    onSuccess?.Invoke(success);
                    return true;
                }
                else
                {
                    string error = "playerData property not found";
                    logger.Log(error);
                    onError?.Invoke(error);
                    return false;
                }
            }
            catch (Exception e)
            {
                string error = $"Error unlocking stag stations: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Gets a boolean item field value from PlayerData.
        /// </summary>
        private bool GetItemBool(string fieldName, bool defaultValue)
        {
            if (heroController == null)
                return defaultValue;

            object playerData = PlayerDataHelper.GetPlayerData(heroController, (error) => logger.Log($"ItemsService.{fieldName}: {error}"));
            if (playerData == null)
                return defaultValue;

            return PlayerDataHelper.GetBool(playerData, fieldName, defaultValue, (error) => logger.Log($"ItemsService.{fieldName}: {error}"));
        }

        /// <summary>
        /// Helper method to set a field without callbacks.
        /// </summary>
        private void SetField(object playerData, Type playerDataType, string fieldName, object value)
        {
            FieldInfo field = playerDataType.GetField(fieldName);
            if (field != null)
            {
                field.SetValue(playerData, value);
            }
        }

        /// <summary>
        /// Unlocks ALL powerups at once: abilities, spells (level 1), nail arts, charms, and key items.
        /// Matches CheatManager's GetAllPowerups() functionality.
        /// </summary>
        public bool UnlockAllPowerups(Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                string error = "Cannot unlock all powerups: Hero controller not available";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }

            try
            {
                object playerData = PlayerDataHelper.GetPlayerData(heroController, onError);
                if (playerData == null) return false;

                // Abilities (matching CheatManager's GetAllPowerups)
                PlayerDataHelper.SetBool(playerData, "canDash", true, null);
                PlayerDataHelper.SetBool(playerData, "hasDash", true, null);
                PlayerDataHelper.SetBool(playerData, "hasWalljump", true, null);
                PlayerDataHelper.SetBool(playerData, "canWallJump", true, null);
                PlayerDataHelper.SetBool(playerData, "hasSuperDash", true, null);
                PlayerDataHelper.SetBool(playerData, "hasShadowDash", true, null);
                PlayerDataHelper.SetBool(playerData, "canShadowDash", true, null);
                PlayerDataHelper.SetBool(playerData, "hasDoubleJump", true, null);
                PlayerDataHelper.SetBool(playerData, "hasAcidArmour", true, null);
                PlayerDataHelper.SetBool(playerData, "hasDreamGate", true, null);
                PlayerDataHelper.SetBool(playerData, "hasQuill", true, null);

                // Dream Nail
                PlayerDataHelper.SetBool(playerData, "hasDreamNail", true, null);
                PlayerDataHelper.SetBool(playerData, "dreamNailUpgraded", true, null);

                // Key Items
                PlayerDataHelper.SetBool(playerData, "hasLantern", true, null);
                PlayerDataHelper.SetBool(playerData, "hasTramPass", true, null);
                PlayerDataHelper.SetBool(playerData, "hasLoveKey", true, null);
                PlayerDataHelper.SetBool(playerData, "hasWhiteKey", true, null);
                PlayerDataHelper.SetBool(playerData, "hasKingsBrand", true, null);

                // Spells (level 1 as per CheatManager - only if not already unlocked)
                PlayerDataHelper.SetBool(playerData, "hasSpell", true, null);
                int fireballLevel = PlayerDataHelper.GetInt(playerData, "fireballLevel", 0);
                if (fireballLevel == 0)
                {
                    PlayerDataHelper.SetInt(playerData, "fireballLevel", 1, null);
                }
                int quakeLevel = PlayerDataHelper.GetInt(playerData, "quakeLevel", 0);
                if (quakeLevel == 0)
                {
                    PlayerDataHelper.SetInt(playerData, "quakeLevel", 1, null);
                }
                int screamLevel = PlayerDataHelper.GetInt(playerData, "screamLevel", 0);
                if (screamLevel == 0)
                {
                    PlayerDataHelper.SetInt(playerData, "screamLevel", 1, null);
                }

                // Nail Arts
                PlayerDataHelper.SetBool(playerData, "hasNailArt", true, null);
                PlayerDataHelper.SetBool(playerData, "hasDashSlash", true, null);
                PlayerDataHelper.SetBool(playerData, "hasCyclone", true, null);
                PlayerDataHelper.SetBool(playerData, "hasUpwardSlash", true, null);

                // All Charms (1-40, including Kingsoul/Void Heart)
                PlayerDataHelper.SetBool(playerData, "hasCharm", true, null);
                for (int i = 1; i <= 40; i++)
                {
                    PlayerDataHelper.SetBool(playerData, $"gotCharm_{i}", true, null);
                }

                // Set Kingsoul/Void Heart to Void Heart state (royalCharmState = 3)
                PlayerDataHelper.SetInt(playerData, "royalCharmState", 3, null);

                // Charm Slots (12 is max with all notch upgrades + overcharm)
                PlayerDataHelper.SetInt(playerData, "charmSlots", 12, null);

                string success = "All powerups unlocked! (Abilities, Spells, Nail Arts, Charms, Key Items)";
                logger.Log(success);
                onSuccess?.Invoke(success);
                return true;
            }
            catch (Exception e)
            {
                string error = $"Error unlocking all powerups: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }

        #endregion

        #region Charm Cost Manipulation

        /// <summary>
        /// Sets all charm costs to 1 notch.
        /// Stores original values for restoration.
        /// </summary>
        public bool SetAllCharmCostsToOne(Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                onError?.Invoke("Hero controller not available");
                return false;
            }

            object playerData = PlayerDataHelper.GetPlayerData(heroController, onError);
            if (playerData == null) return false;

            try
            {
                // Store original costs if not already stored AND they're not already all 1
                // (if they're all 1, that means they were saved modified, not original)
                if (originalCharmCosts == null)
                {
                    bool allCostsAreOne = true;
                    int[] currentCosts = new int[40];
                    
                    for (int i = 1; i <= 40; i++)
                    {
                        currentCosts[i - 1] = PlayerDataHelper.GetInt(playerData, $"charmCost_{i}", 0);
                        if (currentCosts[i - 1] != 1)
                        {
                            allCostsAreOne = false;
                        }
                    }
                    
                    // Only store if they're not all 1 already (otherwise use defaults)
                    if (!allCostsAreOne)
                    {
                        originalCharmCosts = currentCosts;
                        logger.Log("Stored original charm costs for restoration");
                    }
                    else
                    {
                        // Use game's default charm costs as fallback
                        originalCharmCosts = new int[40] 
                        { 
                            3, 1, 2, 4, 2, 2, 1, 1, 1, 1,  // Charms 1-10
                            1, 3, 2, 1, 2, 2, 1, 3, 3, 3,  // Charms 11-20
                            4, 2, 2, 2, 3, 1, 2, 2, 4, 1,  // Charms 21-30
                            2, 3, 2, 2, 3, 5, 1, 3, 2, 3   // Charms 31-40
                        };
                        logger.Log("All costs were 1 (loaded from modified save) - using default charm costs");
                    }
                }

                // Set all charm costs to 1
                for (int i = 1; i <= 40; i++)
                {
                    PlayerDataHelper.SetInt(playerData, $"charmCost_{i}", 1, onError);
                }

                onSuccess?.Invoke("All charm costs set to 1");
                logger.Log("All charm costs set to 1");
                return true;
            }
            catch (Exception e)
            {
                string error = $"Error setting charm costs: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Restores charm costs to their original values.
        /// </summary>
        public bool RestoreCharmCosts(Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                onError?.Invoke("Hero controller not available");
                return false;
            }

            object playerData = PlayerDataHelper.GetPlayerData(heroController, onError);
            if (playerData == null) return false;

            // If we don't have stored originals, use game defaults
            if (originalCharmCosts == null)
            {
                originalCharmCosts = new int[40] 
                { 
                    3, 1, 2, 4, 2, 2, 1, 1, 1, 1,  // Charms 1-10
                    1, 3, 2, 1, 2, 2, 1, 3, 3, 3,  // Charms 11-20
                    4, 2, 2, 2, 3, 1, 2, 2, 4, 1,  // Charms 21-30
                    2, 3, 2, 2, 3, 5, 1, 3, 2, 3   // Charms 31-40
                };
                logger.Log("No stored originals - using game default charm costs");
            }

            try
            {
                // Restore all charm costs
                for (int i = 1; i <= 40; i++)
                {
                    PlayerDataHelper.SetInt(playerData, $"charmCost_{i}", originalCharmCosts[i - 1], onError);
                }

                onSuccess?.Invoke("Charm costs restored");
                logger.Log("Charm costs restored to original values");
                return true;
            }
            catch (Exception e)
            {
                string error = $"Error restoring charm costs: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Checks if all charm costs are currently set to 1.
        /// </summary>
        public bool AreAllCharmCostsOne()
        {
            if (heroController == null) return false;

            object playerData = PlayerDataHelper.GetPlayerData(heroController);
            if (playerData == null) return false;

            // Check if all charm costs are 1
            for (int i = 1; i <= 40; i++)
            {
                int cost = PlayerDataHelper.GetInt(playerData, $"charmCost_{i}", 0);
                if (cost != 1) return false;
            }

            return true;
        }

        /// <summary>
        /// Toggles all charm costs between 1 and their original values.
        /// </summary>
        public bool ToggleAllCharmCosts(Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (AreAllCharmCostsOne())
            {
                return RestoreCharmCosts(onSuccess, onError);
            }
            else
            {
                return SetAllCharmCostsToOne(onSuccess, onError);
            }
        }

        #endregion
    }
}

