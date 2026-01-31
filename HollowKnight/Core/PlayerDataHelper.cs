using System;
using System.Linq;
using System.Reflection;

namespace HollowKnight.Core
{
    /// <summary>
    /// Helper class for safely accessing PlayerData from HeroController.
    /// Provides consistent error logging and handles both property and field access patterns.
    /// </summary>
    public static class PlayerDataHelper
    {
        /// <summary>
        /// Gets PlayerData object from HeroController with proper error handling.
        /// Tries both Property and Field access patterns.
        /// </summary>
        public static object GetPlayerData(object heroController, Action<string> onError = null)
        {
            if (heroController == null)
            {
                onError?.Invoke("HeroController is null");
                return null;
            }

            try
            {
                Type heroType = heroController.GetType();
                
                // Try as Field first (HeroController.playerData is a public field, confirmed from decompiled source)
                FieldInfo playerDataField = heroType.GetField("playerData", BindingFlags.Public | BindingFlags.Instance);
                if (playerDataField != null)
                {
                    object playerData = playerDataField.GetValue(heroController);
                    if (playerData != null)
                    {
                        return playerData;
                    }
                    else
                    {
                        onError?.Invoke("playerData field exists but returned null");
                        return null;
                    }
                }
                
                // Fallback: Try as Property (for compatibility with modded versions)
                PropertyInfo playerDataProperty = heroType.GetProperty("playerData", BindingFlags.Public | BindingFlags.Instance);
                if (playerDataProperty != null)
                {
                    object playerData = playerDataProperty.GetValue(heroController);
                    if (playerData != null)
                    {
                        return playerData;
                    }
                    else
                    {
                        onError?.Invoke("playerData property exists but returned null");
                        return null;
                    }
                }
                
                // If neither found, provide detailed error
                string availableMembers = string.Join(", ", heroType.GetMembers(BindingFlags.Public | BindingFlags.Instance).Select(m => m.Name).Take(20));
                onError?.Invoke($"playerData not found on {heroType.Name}. Available members: {availableMembers}...");
                return null;
            }
            catch (Exception e)
            {
                onError?.Invoke($"Exception accessing playerData: {e.Message}\nStack: {e.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Gets a boolean field value from PlayerData.
        /// </summary>
        public static bool GetBool(object playerData, string fieldName, bool defaultValue, Action<string> onError = null)
        {
            if (playerData == null)
            {
                onError?.Invoke($"PlayerData is null when reading {fieldName}");
                return defaultValue;
            }

            try
            {
                Type playerDataType = playerData.GetType();
                FieldInfo field = playerDataType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                
                if (field != null)
                {
                    if (field.FieldType == typeof(bool))
                    {
                        return (bool)field.GetValue(playerData);
                    }
                    else
                    {
                        onError?.Invoke($"{fieldName} exists but is type {field.FieldType.Name}, not bool");
                        return defaultValue;
                    }
                }
                else
                {
                    onError?.Invoke($"{fieldName} field not found on PlayerData (type: {playerDataType.Name})");
                    return defaultValue;
                }
            }
            catch (Exception e)
            {
                onError?.Invoke($"Exception reading {fieldName}: {e.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets a boolean field value in PlayerData.
        /// </summary>
        public static bool SetBool(object playerData, string fieldName, bool value, Action<string> onError = null)
        {
            if (playerData == null)
            {
                onError?.Invoke($"PlayerData is null when setting {fieldName}");
                return false;
            }

            try
            {
                Type playerDataType = playerData.GetType();
                FieldInfo field = playerDataType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                
                if (field != null)
                {
                    if (field.FieldType == typeof(bool))
                    {
                        field.SetValue(playerData, value);
                        return true;
                    }
                    else
                    {
                        onError?.Invoke($"{fieldName} exists but is type {field.FieldType.Name}, not bool");
                        return false;
                    }
                }
                else
                {
                    onError?.Invoke($"{fieldName} field not found on PlayerData (type: {playerDataType.Name})");
                    return false;
                }
            }
            catch (Exception e)
            {
                onError?.Invoke($"Exception setting {fieldName}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets an integer field value from PlayerData.
        /// </summary>
        public static int GetInt(object playerData, string fieldName, int defaultValue, Action<string> onError = null)
        {
            if (playerData == null)
            {
                onError?.Invoke($"PlayerData is null when reading {fieldName}");
                return defaultValue;
            }

            try
            {
                Type playerDataType = playerData.GetType();
                FieldInfo field = playerDataType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                
                if (field != null)
                {
                    if (field.FieldType == typeof(int))
                    {
                        return (int)field.GetValue(playerData);
                    }
                    else
                    {
                        onError?.Invoke($"{fieldName} exists but is type {field.FieldType.Name}, not int");
                        return defaultValue;
                    }
                }
                else
                {
                    onError?.Invoke($"{fieldName} field not found on PlayerData (type: {playerDataType.Name})");
                    return defaultValue;
                }
            }
            catch (Exception e)
            {
                onError?.Invoke($"Exception reading {fieldName}: {e.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets an integer field value in PlayerData.
        /// </summary>
        public static bool SetInt(object playerData, string fieldName, int value, Action<string> onError = null)
        {
            if (playerData == null)
            {
                onError?.Invoke($"PlayerData is null when setting {fieldName}");
                return false;
            }

            try
            {
                Type playerDataType = playerData.GetType();
                FieldInfo field = playerDataType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                
                if (field != null)
                {
                    if (field.FieldType == typeof(int))
                    {
                        field.SetValue(playerData, value);
                        return true;
                    }
                    else
                    {
                        onError?.Invoke($"{fieldName} exists but is type {field.FieldType.Name}, not int");
                        return false;
                    }
                }
                else
                {
                    onError?.Invoke($"{fieldName} field not found on PlayerData (type: {playerDataType.Name})");
                    return false;
                }
            }
            catch (Exception e)
            {
                onError?.Invoke($"Exception setting {fieldName}: {e.Message}");
                return false;
            }
        }
    }
}

