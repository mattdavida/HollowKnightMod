using UnityEngine;
using System;
using System.Reflection;

namespace HollowKnight.UserInterface
{
    /// <summary>
    /// GUI component for the Cheats tab.
    /// Implements all features from Hollow Knight's built-in CheatManager in a professional GUI.
    /// </summary>
    public class CheatsTabGUI
    {
        // Collapsible section states
        private bool showToggleFeatures = true;
        private bool showActionAmounts = true;
        private bool showQuickActions = true;
        private bool showAbilities = true;
        private bool showSpells = true;
        private bool showCharms = true;
        
        // Input field variables
        private string healthAmount = "1";
        private string geoAmount = "1000";
        private string dreamOrbsAmount = "1000";
        private string charmSlotsAmount = "12";
        
        // Charm dropdown system
        private bool showCharmDropdown = false;
        private Vector2 charmDropdownScroll = Vector2.zero;
        private string charmSearchFilter = "";
        private int selectedCharmIndex = 0;
        private string[] charmNames = null;
        private string[] filteredCharmNames = null;

        /// <summary>
        /// Renders the Cheats tab GUI with all CheatManager features.
        /// </summary>
        public void Render(GuiContext context, ref Vector2 scrollPosition, Rect windowRect, Action<string> onToast)
        {
            // Calculate scroll view height with safety check
            float scrollViewHeight = Mathf.Max(windowRect.height - 140, 400f);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(380), GUILayout.Height(scrollViewHeight));

            // === TOGGLE FEATURES ===
            showToggleFeatures = DrawCollapsingHeader("Toggle Features", showToggleFeatures);
            if (showToggleFeatures)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                // Invincibility Toggle (Checkbox)
                bool currentInvincibility = context.InvincibilityService.IsInvincible();
                bool newInvincibility = GUILayout.Toggle(currentInvincibility, "Invincibility");
                if (newInvincibility != currentInvincibility)
                {
                    context.InvincibilityService.ToggleInvincibility(onToast, onToast);
                }

                // Infinite Air Jump Toggle (Checkbox)
                bool currentInfiniteAirJump = context.InfiniteAirJumpService.IsInfiniteAirJumpEnabled();
                bool newInfiniteAirJump = GUILayout.Toggle(currentInfiniteAirJump, "Infinite Air Jump");
                if (newInfiniteAirJump != currentInfiniteAirJump)
                {
                    context.InfiniteAirJumpService.ToggleInfiniteAirJump(onToast, onToast);
                }

                // Auto Soul Refill Toggle (Checkbox)
                bool currentAutoSoulRefill = false;
                if (context.ModInstance != null)
                {
                    var modType = context.ModInstance.GetType();
                    var isAutoSoulRefillMethod = modType.GetMethod("IsAutoSoulRefillEnabled");
                    if (isAutoSoulRefillMethod != null)
                    {
                        currentAutoSoulRefill = (bool)isAutoSoulRefillMethod.Invoke(context.ModInstance, null);
                    }
                }
                
                bool newAutoSoulRefill = GUILayout.Toggle(currentAutoSoulRefill, "Auto Soul Refill (every 1 second)");
                if (newAutoSoulRefill != currentAutoSoulRefill)
                {
                    if (context.ModInstance != null)
                    {
                        var modType = context.ModInstance.GetType();
                        var toggleMethod = modType.GetMethod("ToggleAutoSoulRefill");
                        if (toggleMethod != null)
                        {
                            toggleMethod.Invoke(context.ModInstance, null);
                            string status = newAutoSoulRefill ? "enabled (every 1 second)" : "disabled";
                            onToast?.Invoke($"Auto Soul Refill {status}");
                        }
                    }
                }

                // Insta Kill Toggle (Checkbox)
                bool currentInstaKill = context.InstaKillService.IsInstaKillEnabled();
                bool newInstaKill = GUILayout.Toggle(currentInstaKill, "Insta Kill");
                if (newInstaKill != currentInstaKill)
                {
                    context.InstaKillService.ToggleInstaKill(onToast, onToast);
                }

                // All Charms Cost 1 Toggle (Checkbox)
                bool currentAllCharmsCost1 = context.ItemsService.AreAllCharmCostsOne();
                bool newAllCharmsCost1 = GUILayout.Toggle(currentAllCharmsCost1, "All Charms Cost 1");
                if (newAllCharmsCost1 != currentAllCharmsCost1)
                {
                    context.ItemsService.ToggleAllCharmCosts(onToast, onToast);
                    
                    // Save config state
                    if (context.ModInstance != null)
                    {
                        var modType = context.ModInstance.GetType();
                        var saveMethod = modType.GetMethod("SaveToggleState");
                        if (saveMethod != null)
                        {
                            saveMethod.Invoke(context.ModInstance, new object[] { "AllCharmsCost1", newAllCharmsCost1 });
                        }
                    }
                }

                GUILayout.EndVertical();
                GUILayout.Space(10);
            }

            // === ACTION AMOUNTS ===
            showActionAmounts = DrawCollapsingHeader("Action Amounts", showActionAmounts);
            if (showActionAmounts)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                // Health
                GUILayout.BeginHorizontal();
                GUILayout.Label("Health:", GUILayout.Width(100));
                healthAmount = GUILayout.TextField(healthAmount, GUILayout.Width(60));
                if (GUILayout.Button("Add", GUILayout.Width(90)))
                {
                    if (int.TryParse(healthAmount, out int amount))
                    {
                        context.HealthService.AddHealth(amount, onToast, onToast);
                    }
                    else
                    {
                        onToast?.Invoke("Invalid amount");
                    }
                }
                if (GUILayout.Button("Set Max", GUILayout.Width(90)))
                {
                    context.ConfirmationSystem.ShowConfirmation(
                        "Set Max Health",
                        "Set max health to 10? Save & reload to see UI update.",
                        () => context.HealthService.SetMaxHealthExact(10, onToast, onToast)
                    );
                }
                GUILayout.EndHorizontal();

                // Geo
                GUILayout.BeginHorizontal();
                GUILayout.Label("Geo:", GUILayout.Width(100));
                geoAmount = GUILayout.TextField(geoAmount, GUILayout.Width(60));
                if (GUILayout.Button("Add", GUILayout.Width(90)))
                {
                    if (int.TryParse(geoAmount, out int amount))
                    {
                        context.CurrencyService.AddGeo(amount, onToast, onToast);
                    }
                    else
                    {
                        onToast?.Invoke("Invalid amount");
                    }
                }
                GUILayout.EndHorizontal();

                // Dream Orbs
                GUILayout.BeginHorizontal();
                GUILayout.Label("Dream Orbs:", GUILayout.Width(100));
                dreamOrbsAmount = GUILayout.TextField(dreamOrbsAmount, GUILayout.Width(60));
                if (GUILayout.Button("Add", GUILayout.Width(90)))
                {
                    if (int.TryParse(dreamOrbsAmount, out int amount))
                    {
                        context.ItemsService.AddDreamOrbs(amount, onToast, onToast);
                    }
                    else
                    {
                        onToast?.Invoke("Invalid amount");
                    }
                }
                GUILayout.EndHorizontal();

                // Charm Slots
                GUILayout.BeginHorizontal();
                GUILayout.Label("Charm Slots:", GUILayout.Width(100));
                charmSlotsAmount = GUILayout.TextField(charmSlotsAmount, GUILayout.Width(60));
                if (GUILayout.Button("Set", GUILayout.Width(90)))
                {
                    if (int.TryParse(charmSlotsAmount, out int amount))
                    {
                        context.ItemsService.SetCharmSlots(amount, onToast, onToast);
                    }
                    else
                    {
                        onToast?.Invoke("Invalid amount");
                    }
                }
                if (GUILayout.Button("Set Max", GUILayout.Width(90)))
                {
                    context.ConfirmationSystem.ShowConfirmation(
                        "Set Max Charm Slots",
                        "Set charm slots to 12 (max)?",
                        () => context.ItemsService.SetCharmSlots(12, onToast, onToast)
                    );
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            GUILayout.Space(5);

            // === QUICK ACTIONS ===
            showQuickActions = DrawCollapsingHeader("Quick Actions", showQuickActions);
            if (showQuickActions)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Refill Health"))
                {
                    context.HealthService.RefillHealth(onToast, onToast);
                }
                if (GUILayout.Button("Refill Soul"))
                {
                    context.CurrencyService.RefillSoul(onToast, onToast);
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("All Charms"))
                {
                    context.ConfirmationSystem.ShowConfirmation(
                        "All Charms",
                        "Unlock all 40 charms with 10 charm slots?",
                        () => context.ItemsService.UnlockAllCharms(onToast, onToast)
                    );
                }

                if (GUILayout.Button("All Key Items"))
                {
                    context.ConfirmationSystem.ShowConfirmation(
                        "All Key Items",
                        "Unlock all key items and abilities?",
                        () => context.ItemsService.UnlockAllKeyItems(onToast, onToast)
                    );
                }

                if (GUILayout.Button("All Nail Arts"))
                {
                    context.ConfirmationSystem.ShowConfirmation(
                        "All Nail Arts",
                        "Unlock all nail arts?",
                        () => context.ItemsService.UnlockAllNailArts(onToast, onToast)
                    );
                }

                if (GUILayout.Button("All Map"))
                {
                    context.ConfirmationSystem.ShowConfirmation(
                        "All Map",
                        "Reveal all map areas?",
                        () => context.ItemsService.UnlockAllMaps(onToast, onToast)
                    );
                }

                if (GUILayout.Button("All Stags"))
                {
                    context.ConfirmationSystem.ShowConfirmation(
                        "All Stags",
                        "Unlock all stag stations?",
                        () => context.ItemsService.UnlockAllStags(onToast, onToast)
                    );
                }

                if (GUILayout.Button("All Powerups"))
                {
                    context.ConfirmationSystem.ShowConfirmation(
                        "All Powerups",
                        "Unlock EVERYTHING? (Abilities, Spells, Nail Arts, Charms, Key Items)",
                        () => context.ItemsService.UnlockAllPowerups(onToast, onToast)
                    );
                }

                GUILayout.EndVertical();
            }

            GUILayout.Space(5);

            // === INVINCIBILITY ===
            // === ABILITIES ===
            showAbilities = DrawCollapsingHeader("Abilities", showAbilities);
            if (showAbilities)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                GUILayout.BeginHorizontal();
                
                // Dash
                bool dashUnlocked = context.AbilitiesService.IsDashUnlocked();
                GUI.color = dashUnlocked ? Color.green : Color.white;
                if (GUILayout.Button(dashUnlocked ? "Dash ✓" : "Dash"))
                {
                    context.AbilitiesService.ToggleDash(onToast, onToast);
                }
                GUI.color = Color.white;
                
                // Shadow Dash
                bool shadowDashUnlocked = context.AbilitiesService.IsShadowDashUnlocked();
                GUI.color = shadowDashUnlocked ? Color.green : Color.white;
                if (GUILayout.Button(shadowDashUnlocked ? "Shadow Dash ✓" : "Shadow Dash"))
                {
                    context.AbilitiesService.ToggleShadowDash(onToast, onToast);
                }
                GUI.color = Color.white;
                
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                
                // Double Jump
                bool doubleJumpUnlocked = context.AbilitiesService.IsDoubleJumpUnlocked();
                GUI.color = doubleJumpUnlocked ? Color.green : Color.white;
                if (GUILayout.Button(doubleJumpUnlocked ? "Double Jump ✓" : "Double Jump"))
                {
                    context.AbilitiesService.ToggleDoubleJump(onToast, onToast);
                }
                GUI.color = Color.white;
                
                // Wall Jump
                bool wallJumpUnlocked = context.AbilitiesService.IsWallJumpUnlocked();
                GUI.color = wallJumpUnlocked ? Color.green : Color.white;
                if (GUILayout.Button(wallJumpUnlocked ? "Wall Jump ✓" : "Wall Jump"))
                {
                    context.AbilitiesService.ToggleWallJump(onToast, onToast);
                }
                GUI.color = Color.white;
                
                GUILayout.EndHorizontal();

                // Super Dash
                bool superDashUnlocked = context.AbilitiesService.IsSuperDashUnlocked();
                GUI.color = superDashUnlocked ? Color.green : Color.white;
                if (GUILayout.Button(superDashUnlocked ? "Super Dash ✓" : "Super Dash"))
                {
                    context.AbilitiesService.ToggleSuperDash(onToast, onToast);
                }
                GUI.color = Color.white;

                GUILayout.BeginHorizontal();
                
                // Acid Armor
                bool acidArmorUnlocked = context.AbilitiesService.IsAcidArmorUnlocked();
                GUI.color = acidArmorUnlocked ? Color.green : Color.white;
                if (GUILayout.Button(acidArmorUnlocked ? "Acid Armor ✓" : "Acid Armor"))
                {
                    context.AbilitiesService.ToggleAcidArmor(onToast, onToast);
                }
                GUI.color = Color.white;
                
                // Dreamgate
                bool dreamgateUnlocked = context.AbilitiesService.IsDreamgateUnlocked();
                GUI.color = dreamgateUnlocked ? Color.green : Color.white;
                if (GUILayout.Button(dreamgateUnlocked ? "Dreamgate ✓" : "Dreamgate"))
                {
                    context.AbilitiesService.ToggleDreamgate(onToast, onToast);
                }
                GUI.color = Color.white;
                
                GUILayout.EndHorizontal();

                // Dream Nail
                bool dreamNailUnlocked = context.ItemsService.IsDreamNailUnlocked();
                GUI.color = dreamNailUnlocked ? Color.green : Color.white;
                if (GUILayout.Button(dreamNailUnlocked ? "Dream Nail ✓" : "Dream Nail"))
                {
                    context.ItemsService.ToggleDreamNail(onToast, onToast);
                }
                GUI.color = Color.white;

                GUILayout.EndVertical();
            }

            GUILayout.Space(5);

            // === SPELLS ===
            showSpells = DrawCollapsingHeader("Spells", showSpells);
            if (showSpells)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                // Fireball
                int fireballLevel = context.SpellsService.GetFireballLevel();
                GUI.color = fireballLevel > 0 ? Color.green : Color.white;
                string fireballText = fireballLevel == 0 ? "Fireball" : 
                                     fireballLevel == 1 ? "Fireball [1/2] ✓" : "Fireball [2/2] ✓";
                if (GUILayout.Button(fireballText))
                {
                    context.SpellsService.ToggleFireball(onToast, onToast);
                }
                GUI.color = Color.white;

                // Quake
                int quakeLevel = context.SpellsService.GetQuakeLevel();
                GUI.color = quakeLevel > 0 ? Color.green : Color.white;
                string quakeText = quakeLevel == 0 ? "Quake" : 
                                  quakeLevel == 1 ? "Quake [1/2] ✓" : "Quake [2/2] ✓";
                if (GUILayout.Button(quakeText))
                {
                    context.SpellsService.ToggleQuake(onToast, onToast);
                }
                GUI.color = Color.white;

                // Scream
                int screamLevel = context.SpellsService.GetScreamLevel();
                GUI.color = screamLevel > 0 ? Color.green : Color.white;
                string screamText = screamLevel == 0 ? "Scream" : 
                                   screamLevel == 1 ? "Scream [1/2] ✓" : "Scream [2/2] ✓";
                if (GUILayout.Button(screamText))
                {
                    context.SpellsService.ToggleScream(onToast, onToast);
                }
                GUI.color = Color.white;

                GUILayout.EndVertical();
            }

            GUILayout.Space(5);

            // === CHARMS ===
            showCharms = DrawCollapsingHeader("Charms", showCharms);
            if (showCharms)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                // Initialize charm names on first render
                if (charmNames == null)
                {
                    FilterCharms(context);
                }

                if (charmNames != null && charmNames.Length > 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Charm:", GUILayout.Width(80));

                    // Dropdown button - show display name only (extract from "number|name" format)
                    string currentSelection = "Select Charm";
                    if (selectedCharmIndex < charmNames.Length)
                    {
                        string[] parts = charmNames[selectedCharmIndex].Split('|');
                        currentSelection = parts.Length == 2 ? $"{parts[0]}. {parts[1]}" : charmNames[selectedCharmIndex];
                    }

                    if (GUILayout.Button($"{currentSelection} ▼", GUILayout.Width(180)))
                    {
                        showCharmDropdown = !showCharmDropdown;
                        if (showCharmDropdown)
                        {
                            charmSearchFilter = ""; // Reset search when opening
                            FilterCharms(context);
                        }
                    }
                    GUILayout.EndHorizontal();

                    // Dropdown with search
                    if (showCharmDropdown)
                    {
                        GUILayout.BeginVertical(GUI.skin.box);

                        // Search box
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Search:", GUILayout.Width(50));
                        string newFilter = GUILayout.TextField(charmSearchFilter, GUILayout.Width(130));
                        if (newFilter != charmSearchFilter)
                        {
                            charmSearchFilter = newFilter;
                            FilterCharms(context);
                            charmDropdownScroll = Vector2.zero; // Reset scroll when filtering
                        }
                        GUILayout.EndHorizontal();

                        // Clear search button
                        if (!string.IsNullOrEmpty(charmSearchFilter))
                        {
                            if (GUILayout.Button("Clear", GUILayout.Width(60)))
                            {
                                charmSearchFilter = "";
                                FilterCharms(context);
                            }
                        }

                        // Scrollable filtered list
                        charmDropdownScroll = GUILayout.BeginScrollView(charmDropdownScroll, GUILayout.Height(150));

                        if (filteredCharmNames != null && filteredCharmNames.Length > 0)
                        {
                            for (int i = 0; i < filteredCharmNames.Length; i++)
                            {
                                string charmEntry = filteredCharmNames[i];

                                // Find the index in the original array
                                int originalIndex = Array.IndexOf(charmNames, charmEntry);

                                // Parse charm number from "number|name" format
                                string[] parts = charmEntry.Split('|');
                                if (parts.Length != 2) continue;
                                
                                if (!int.TryParse(parts[0], out int charmNumber)) continue;
                                string displayName = $"{parts[0]}. {parts[1]}";

                                // Check if this charm is unlocked
                                bool isUnlocked = context.ItemsService.IsCharmUnlocked(charmNumber);
                                GUI.color = isUnlocked ? Color.green : Color.white;

                                if (GUILayout.Button(isUnlocked ? $"{displayName} ✓" : displayName, GUI.skin.label))
                                {
                                    selectedCharmIndex = originalIndex;
                                    showCharmDropdown = false;
                                    charmSearchFilter = ""; // Clear search after selection
                                }

                                GUI.color = Color.white;
                            }
                        }
                        else
                        {
                            GUILayout.Label("No charms match search", GUI.skin.label);
                        }

                        GUILayout.EndScrollView();
                        GUILayout.EndVertical();
                    }

                    // Toggle button for selected charm
                    GUILayout.BeginHorizontal();
                    if (selectedCharmIndex < charmNames.Length)
                    {
                        // Parse charm number from "number|name" format
                        string[] parts = charmNames[selectedCharmIndex].Split('|');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int selectedCharmNumber))
                        {
                            bool isUnlocked = context.ItemsService.IsCharmUnlocked(selectedCharmNumber);
                            GUI.color = isUnlocked ? Color.green : Color.white;

                            if (GUILayout.Button(isUnlocked ? "Toggle ✓" : "Toggle"))
                            {
                                context.ItemsService.ToggleCharm(selectedCharmNumber, onToast, onToast);
                            }

                            GUI.color = Color.white;
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }

            GUILayout.Space(5);

            // === ITEMS ===
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws a collapsing header button and returns the new collapsed state.
        /// </summary>
        private bool DrawCollapsingHeader(string title, bool isExpanded)
        {
            GUIStyle headerStyle = new GUIStyle(GUI.skin.button);
            headerStyle.alignment = TextAnchor.MiddleLeft;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = isExpanded ? Color.green : Color.white;

            string buttonText = (isExpanded ? "▼ " : "▶ ") + title;
            if (GUILayout.Button(buttonText, headerStyle, GUILayout.Height(25)))
            {
                return !isExpanded;
            }
            return isExpanded;
        }

        /// <summary>
        /// Filters charm names based on search query.
        /// </summary>
        private void FilterCharms(GuiContext context)
        {
            if (charmNames == null)
            {
                charmNames = context.ItemsService.GetCharmNames();
            }

            if (string.IsNullOrEmpty(charmSearchFilter))
            {
                filteredCharmNames = charmNames;
            }
            else
            {
                var filtered = new System.Collections.Generic.List<string>();
                string lowerFilter = charmSearchFilter.ToLower();

                foreach (string name in charmNames)
                {
                    if (name.ToLower().Contains(lowerFilter))
                    {
                        filtered.Add(name);
                    }
                }

                filteredCharmNames = filtered.ToArray();
            }
        }
    }
}
