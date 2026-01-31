#if MELONLOADER
using MelonLoader;
#elif BEPINEX
using BepInEx;
using BepInEx.Logging;
#endif
using UnityEngine;
using System;
using System.Reflection;
using HarmonyLib;
using UniverseLib;
using UniverseLib.UI;
using HollowKnight.Core;
using HollowKnight.Interfaces;
using HollowKnight.Framework;
using HollowKnight.Services;
using HollowKnight.UserInterface;

namespace HollowKnight
{
#if MELONLOADER
    public class HollowKnightMod : MelonMod
#elif BEPINEX
    [BepInPlugin("com.hollowknight.cheats", "Hollow Knight Cheats", "1.0.0")]
    public class HollowKnightMod : BaseUnityPlugin
#endif
    {
        // GUI Variables
        private bool showGUI = false;
        
        // Minimum window dimensions
        private const float MIN_WINDOW_WIDTH = 400f;
        private const float MIN_WINDOW_HEIGHT = 600f;
        private Rect windowRect = new Rect(Screen.width - 420, (Screen.height * 0.01f), 400, (Screen.height * 0.98f));
        
        private UIBase uiBase;
        private bool universeLibInitialized = false;
        
        // Toast notification system
        private ToastSystem toastSystem = new ToastSystem();
        
        // Confirmation modal system
        private ConfirmationSystem confirmationSystem = new ConfirmationSystem();
        
        // Services
        private HealthService healthService;
        private CurrencyService currencyService;
        private InvincibilityService invincibilityService;
        private InfiniteAirJumpService infiniteAirJumpService;
        private InstaKillService instaKillService;
        private AbilitiesService abilitiesService;
        private SpellsService spellsService;
        private ItemsService itemsService;
        
        // GUI Components
        private GuiContext guiContext;
        private CheatsTabGUI cheatsTabGUI = new CheatsTabGUI();
        
        // Tab system
        private int selectedTab = 0;
        private string[] tabNames = new string[] { "Cheats" };
        
        // Scroll positions
        private Vector2 cheatsScrollPosition = Vector2.zero;
        
        // HeroController reference
        private Component heroController;
        
        // Auto Soul Refill system
        private bool autoRefillSoul = false;
        private float soulRefillTimer = 0f;
        private const float SOUL_REFILL_INTERVAL = 1.0f; // Every 1 second
        
        // Framework abstraction layer
#if MELONLOADER
        private IModLogger logger = new MelonLoggerAdapter();
#elif BEPINEX
        private IModLogger logger;
#endif
        private IInputHandler inputHandler = new UnityInputAdapter();
        private ITimeProvider timeProvider = new UnityTimeAdapter();

        // Configuration for Toggle Features
#if BEPINEX
        private BepInEx.Configuration.ConfigEntry<bool> invincibilityConfig;
        private BepInEx.Configuration.ConfigEntry<bool> infiniteAirJumpConfig;
        private BepInEx.Configuration.ConfigEntry<bool> autoSoulRefillConfig;
        private BepInEx.Configuration.ConfigEntry<bool> instaKillConfig;
        private BepInEx.Configuration.ConfigEntry<bool> allCharmsCost1Config;
#elif MELONLOADER
        private MelonPreferences_Category toggleFeaturesCategory;
        private MelonPreferences_Entry<bool> invincibilityConfig;
        private MelonPreferences_Entry<bool> infiniteAirJumpConfig;
        private MelonPreferences_Entry<bool> autoSoulRefillConfig;
        private MelonPreferences_Entry<bool> instaKillConfig;
        private MelonPreferences_Entry<bool> allCharmsCost1Config;
#endif

#if MELONLOADER
        [System.Obsolete]
        public override void OnApplicationStart()
        {
            InitializeMod();
        }

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            logger.Log("Hollow Knight Cheats Mod Initialized!");
        }
#elif BEPINEX
        void Awake()
        {
            // Initialize BepInEx logger
            logger = new BepInExLoggerAdapter(Logger);
            InitializeMod();
        }
#endif

        private void InitializeMod()
        {
            // Apply Harmony patches to enable CheatManager
            ApplyHarmonyPatches();
            
            // Initialize CheatManager early (now that IsCheatsEnabled returns true)
            InitializeCheatManager();
            
            // Initialize services
            healthService = new HealthService(logger);
            currencyService = new CurrencyService(logger);
            invincibilityService = new InvincibilityService(logger);
            infiniteAirJumpService = new InfiniteAirJumpService(logger);
            instaKillService = new InstaKillService(logger);
            abilitiesService = new AbilitiesService(logger);
            spellsService = new SpellsService(logger);
            itemsService = new ItemsService(logger);
            
            // Initialize configuration
            InitializeConfiguration();
            
            // Set config save callbacks
            invincibilityService.SetConfigSaveCallback(SaveToggleState);
            infiniteAirJumpService.SetConfigSaveCallback(SaveToggleState);
            instaKillService.SetConfigSaveCallback(SaveToggleState);
            
            // Initialize GUI context
            guiContext = new GuiContext
            {
                ToastSystem = toastSystem,
                ConfirmationSystem = confirmationSystem,
                Logger = logger,
                HealthService = healthService,
                CurrencyService = currencyService,
                InvincibilityService = invincibilityService,
                InfiniteAirJumpService = infiniteAirJumpService,
                InstaKillService = instaKillService,
                AbilitiesService = abilitiesService,
                SpellsService = spellsService,
                ItemsService = itemsService,
                InputHandler = inputHandler,
                TimeProvider = timeProvider,
                ModInstance = this
            };
            
            logger.Log("Hollow Knight Cheats Mod v1.0 - Ready!");
            logger.Log("Controls: INSERT/TILDE = Toggle GUI");
        }

        /// <summary>
        /// Applies Harmony patches to enable developer cheats.
        /// Patches CheatManager.IsCheatsEnabled to always return true.
        /// </summary>
        private void ApplyHarmonyPatches()
        {
            try
            {
                var harmony = new HarmonyLib.Harmony("com.hollowknight.cheats");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                logger.Log("Harmony patches applied - CheatManager will be enabled!");
            }
            catch (Exception e)
            {
                logger.Log($"Error applying Harmony patches: {e.Message}");
                logger.Log($"Stack trace: {e.StackTrace}");
            }
        }

        /// <summary>
        /// Initializes the CheatManager by calling its Init() method.
        /// Must be called after Harmony patches are applied so IsCheatsEnabled returns true.
        /// </summary>
        private void InitializeCheatManager()
        {
            try
            {
                Type cheatManagerType = Type.GetType("CheatManager, Assembly-CSharp");
                if (cheatManagerType == null)
                {
                    logger.Log("CheatManager type not found - cannot initialize early");
                    return;
                }

                // Get the Init method
                MethodInfo initMethod = cheatManagerType.GetMethod("Init", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (initMethod == null)
                {
                    logger.Log("CheatManager.Init() method not found");
                    return;
                }

                // Call Init() - this will create the CheatManager GameObject since IsCheatsEnabled is now true
                initMethod.Invoke(null, null);
                logger.Log("CheatManager.Init() called - CheatManager should now be available");

                // Verify the instance was created
                FieldInfo instanceField = cheatManagerType.GetField("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (instanceField != null)
                {
                    object instance = instanceField.GetValue(null);
                    if (instance != null)
                    {
                        logger.Log("CheatManager instance successfully created!");
                    }
                    else
                    {
                        logger.Log("CheatManager instance is still null after Init() - may need scene to be loaded");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log($"Error initializing CheatManager: {e.Message}");
                logger.Log($"Stack trace: {e.StackTrace}");
            }
        }

#if MELONLOADER
        public override void OnUpdate()
#elif BEPINEX
        void Update()
#endif
        {
            // Find and set HeroController if not already set
            if (heroController == null)
            {
                GameObject knight = GameObject.Find("Knight");
                if (knight != null)
                {
                    heroController = knight.GetComponent("HeroController") as Component;
                    if (heroController != null)
                    {
                        healthService.SetHeroController(heroController);
                        currencyService.SetHeroController(heroController);
                        invincibilityService.SetHeroController(heroController);
                        infiniteAirJumpService.SetHeroController(heroController);
                        abilitiesService.SetHeroController(heroController);
                        spellsService.SetHeroController(heroController);
                        itemsService.SetHeroController(heroController);
                        logger.Log("HeroController found and set!");
                        
                        // Apply config state (toggle features) from saved preferences
                        ApplyConfigState();
                    }
                }
            }
            
            // GUI Toggle (Insert or Tilde key)
            if (inputHandler.GetKeyDown(KeyCode.Insert) || inputHandler.GetKeyDown(KeyCode.BackQuote))
            {
                showGUI = !showGUI;

                // Initialize UniverseLib on first GUI open
                if (showGUI && !universeLibInitialized)
                {
                    try
                    {
                        var config = new UniverseLib.Config.UniverseLibConfig()
                        {
                            Disable_EventSystem_Override = false,
                            Force_Unlock_Mouse = true
                        };

                        float startupDelay = 1f;
                        Universe.Init(startupDelay, OnUniverseLibInitialized, LogHandler, config);
                        logger.Log("UniverseLib initialization started...");
                    }
                    catch (Exception e)
                    {
                        logger.Log($"Failed to initialize UniverseLib: {e.Message}");
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                    }
                }

                // Use UniversalUI if available
                if (universeLibInitialized && uiBase != null)
                {
                    UniversalUI.SetUIActive("HollowKnightCheatGUI", showGUI);
                }
                else
                {
                    if (showGUI)
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                    }
                    else
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                }

                logger.Log($"GUI {(showGUI ? "Enabled" : "Disabled")}");
                toastSystem.ShowToast($"GUI {(showGUI ? "Opened" : "Closed")}");
            }

            // Handle auto soul refill timer
            if (autoRefillSoul && heroController != null)
            {
                soulRefillTimer += timeProvider.DeltaTime;
                if (soulRefillTimer >= SOUL_REFILL_INTERVAL)
                {
                    currencyService.RefillSoul(null, null); // Silent refill (no toast spam)
                    soulRefillTimer = 0f;
                }
            }

            // Enforce toggle feature states (game may reset them on death, scene changes, etc.)
            if (heroController != null)
            {
#if BEPINEX
                bool shouldBeInvincible = invincibilityConfig != null && invincibilityConfig.Value;
                bool shouldHaveInfiniteAirJump = infiniteAirJumpConfig != null && infiniteAirJumpConfig.Value;
#elif MELONLOADER
                bool shouldBeInvincible = invincibilityConfig != null && invincibilityConfig.Value;
                bool shouldHaveInfiniteAirJump = infiniteAirJumpConfig != null && infiniteAirJumpConfig.Value;
#else
                bool shouldBeInvincible = false;
                bool shouldHaveInfiniteAirJump = false;
#endif
                
                // Re-enable invincibility if it was reset
                if (shouldBeInvincible && invincibilityService != null && !invincibilityService.IsInvincible())
                {
                    invincibilityService.SetInvincibility(true, null, null); // Silent re-enable
                }
                
                // Re-enable infinite air jump if it was reset
                if (shouldHaveInfiniteAirJump && infiniteAirJumpService != null && !infiniteAirJumpService.IsInfiniteAirJumpEnabled())
                {
                    infiniteAirJumpService.SetInfiniteAirJump(true, null, null); // Silent re-enable
                }
            }

            // Update toast system
            toastSystem.Update(timeProvider.DeltaTime);
        }

#if MELONLOADER
        public override void OnGUI()
#elif BEPINEX
        void OnGUI()
#endif
        {
            if (showGUI)
            {
                // Dynamically calculate window dimensions
                float desiredWidth = 400f;
                float desiredHeight = Screen.height * 0.98f;
                float desiredX = Screen.width - 420f;
                float desiredY = Screen.height * 0.01f;
                
                desiredWidth = Mathf.Max(desiredWidth, MIN_WINDOW_WIDTH);
                desiredHeight = Mathf.Max(desiredHeight, MIN_WINDOW_HEIGHT);
                
                bool needsRepositioning = windowRect.x + windowRect.width > Screen.width || 
                                         windowRect.y + windowRect.height > Screen.height ||
                                         windowRect.x < 0 || windowRect.y < 0;
                
                if (needsRepositioning)
                {
                    windowRect.x = desiredX;
                    windowRect.y = desiredY;
                }
                
                windowRect.width = desiredWidth;
                windowRect.height = desiredHeight;
                
                if (windowRect.x + windowRect.width > Screen.width)
                    windowRect.x = Screen.width - windowRect.width;
                if (windowRect.y + windowRect.height > Screen.height)
                    windowRect.y = 0;
                
                Color originalBackground = GUI.backgroundColor;
                GUI.backgroundColor = new Color(originalBackground.r, originalBackground.g, originalBackground.b, 1.0f);

                windowRect = GUI.Window(0, windowRect, GuiWindow, "HOLLOW KNIGHT CHEATS");

                GUI.backgroundColor = originalBackground;
            }
            
            // Render confirmation modal AFTER main window (so it appears on top)
            confirmationSystem.RenderModal();
        }

        private void GuiWindow(int windowID)
        {
            try
            {
                GUILayout.BeginVertical();
                
                // Title
                GUILayout.Label("HOLLOW KNIGHT CHEATS", GUI.skin.box);
                GUILayout.Space(5);
                
                // Tab system
                GUILayout.BeginHorizontal();
                for (int i = 0; i < tabNames.Length; i++)
                {
                    // Highlight selected tab
                    GUI.color = (selectedTab == i) ? Color.green : Color.white;
                    if (GUILayout.Button(tabNames[i], GUILayout.Height(30)))
                    {
                        selectedTab = i;
                    }
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                
                // Update context with current window rect
                guiContext.WindowRect = windowRect;
                
                // Render selected tab
                switch (selectedTab)
                {
                    case 0: // Cheats Tab
                        cheatsTabGUI.Render(guiContext, ref cheatsScrollPosition, windowRect, ShowToast);
                        break;
                }
                
                // Toast notification area (fixed at bottom)
                toastSystem.RenderToast();
                
                GUILayout.Space(5);
                
                // Close button
                if (GUILayout.Button("Close", GUILayout.Height(30)))
                {
                    showGUI = false;
                    if (universeLibInitialized && uiBase != null)
                    {
                        UniversalUI.SetUIActive("HollowKnightCheatGUI", false);
                    }
                    else
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                    toastSystem.ShowToast("GUI Closed");
                }

                GUILayout.EndVertical();

                GUI.DragWindow(new Rect(0, 0, windowRect.width, 30));
            }
            catch (Exception e)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("GUI Error - Please report this:", GUI.skin.box);
                GUILayout.Label(e.Message, GUI.skin.label);
                GUILayout.Space(5);
                if (GUILayout.Button("Close"))
                {
                    showGUI = false;
                }
                GUILayout.EndVertical();
                
                logger.Log($"GUI rendering error: {e}");
            }
        }
        
        private void ShowToast(string message)
        {
            toastSystem.ShowToast(message);
        }

        private void OnUniverseLibInitialized()
        {
            try
            {
                uiBase = UniversalUI.RegisterUI("HollowKnightCheatGUI", null);
                universeLibInitialized = true;
                logger.Log("UniverseLib initialized successfully!");
            }
            catch (Exception e)
            {
                logger.Log($"Failed to register UI with UniverseLib: {e.Message}");
            }
        }

        private void LogHandler(string message, LogType type)
        {
            logger.Log($"[UniverseLib] {message}");
        }

        /// <summary>
        /// Initializes configuration for toggle features.
        /// Creates config file: BepInEx/config/com.hollowknight.cheats.cfg or UserData/MelonPreferences.cfg
        /// </summary>
        private void InitializeConfiguration()
        {
#if BEPINEX
            // BepInEx creates: BepInEx/config/com.hollowknight.cheats.cfg
            invincibilityConfig = Config.Bind("ToggleFeatures", "Invincibility", false, 
                "Enable Invincibility on startup");
            infiniteAirJumpConfig = Config.Bind("ToggleFeatures", "Infinite Air Jump", false, 
                "Enable Infinite Air Jump on startup");
            autoSoulRefillConfig = Config.Bind("ToggleFeatures", "Auto Soul Refill", false, 
                "Enable Auto Soul Refill on startup (every 1 second)");
            instaKillConfig = Config.Bind("ToggleFeatures", "Insta Kill", false, 
                "Enable Insta Kill on startup");
            allCharmsCost1Config = Config.Bind("ToggleFeatures", "All Charms Cost 1", false, 
                "Set all charm costs to 1 notch on startup");
            
            logger.Log($"Configuration loaded - Invincibility: {invincibilityConfig.Value}, Infinite Air Jump: {infiniteAirJumpConfig.Value}, Auto Soul Refill: {autoSoulRefillConfig.Value}, Insta Kill: {instaKillConfig.Value}, All Charms Cost 1: {allCharmsCost1Config.Value}");
#elif MELONLOADER
            // MelonLoader creates: UserData/MelonPreferences.cfg
            toggleFeaturesCategory = MelonPreferences.CreateCategory("HollowKnightToggleFeatures", "Hollow Knight Toggle Features");
            
            invincibilityConfig = toggleFeaturesCategory.CreateEntry("Invincibility", false, 
                "Enable Invincibility on startup");
            infiniteAirJumpConfig = toggleFeaturesCategory.CreateEntry("InfiniteAirJump", false, 
                "Enable Infinite Air Jump on startup");
            autoSoulRefillConfig = toggleFeaturesCategory.CreateEntry("AutoSoulRefill", false, 
                "Enable Auto Soul Refill on startup (every 1 second)");
            instaKillConfig = toggleFeaturesCategory.CreateEntry("InstaKill", false, 
                "Enable Insta Kill on startup");
            allCharmsCost1Config = toggleFeaturesCategory.CreateEntry("AllCharmsCost1", false, 
                "Set all charm costs to 1 notch on startup");
            
            logger.Log($"Preferences loaded - Invincibility: {invincibilityConfig.Value}, Infinite Air Jump: {infiniteAirJumpConfig.Value}, Auto Soul Refill: {autoSoulRefillConfig.Value}, Insta Kill: {instaKillConfig.Value}, All Charms Cost 1: {allCharmsCost1Config.Value}");
#endif
        }

        /// <summary>
        /// Applies configuration state to game after hero controller is available.
        /// Called once when hero controller is first found.
        /// </summary>
        private void ApplyConfigState()
        {
            if (heroController == null)
                return;

#if BEPINEX
            bool applyInvincibility = invincibilityConfig.Value;
            bool applyInfiniteAirJump = infiniteAirJumpConfig.Value;
            bool applyAutoSoulRefill = autoSoulRefillConfig.Value;
            bool applyInstaKill = instaKillConfig.Value;
            bool applyAllCharmsCost1 = allCharmsCost1Config.Value;
#elif MELONLOADER
            bool applyInvincibility = invincibilityConfig.Value;
            bool applyInfiniteAirJump = infiniteAirJumpConfig.Value;
            bool applyAutoSoulRefill = autoSoulRefillConfig.Value;
            bool applyInstaKill = instaKillConfig.Value;
            bool applyAllCharmsCost1 = allCharmsCost1Config.Value;
#endif

            // Apply toggle feature states if configured
            if (applyInvincibility)
            {
                invincibilityService.SetInvincibility(true, 
                    (msg) => logger.Log($"Config: {msg}"), 
                    (err) => logger.Log($"Config Error: {err}"));
            }

            if (applyInfiniteAirJump)
            {
                infiniteAirJumpService.SetInfiniteAirJump(true, 
                    (msg) => logger.Log($"Config: {msg}"), 
                    (err) => logger.Log($"Config Error: {err}"));
            }

            if (applyAutoSoulRefill)
            {
                autoRefillSoul = true;
                soulRefillTimer = 0f;
                logger.Log("Config: Auto Soul Refill enabled");
            }

            if (applyInstaKill)
            {
                // Try to apply InstaKill, but don't error if CheatManager isn't ready yet
                // (it may need the scene to be fully loaded)
                bool success = instaKillService.SetInstaKill(true, 
                    (msg) => logger.Log($"Config: {msg}"), 
                    null); // Don't log errors on startup
                
                if (!success)
                {
                    logger.Log("Config: Insta Kill will be available once you're in-game (CheatManager not ready yet)");
                }
            }

            if (applyAllCharmsCost1)
            {
                itemsService.SetAllCharmCostsToOne(
                    (msg) => logger.Log($"Config: {msg}"), 
                    (err) => logger.Log($"Config Error: {err}"));
            }

            if (applyInvincibility || applyInfiniteAirJump || applyAutoSoulRefill || applyInstaKill || applyAllCharmsCost1)
            {
                logger.Log("Configuration state applied to game");
            }
        }

        /// <summary>
        /// Saves current toggle states to configuration.
        /// Called when invincibility toggles are changed.
        /// </summary>
        public void SaveToggleState(string toggleName, bool value)
        {
#if BEPINEX
            if (toggleName == "Invincibility")
            {
                invincibilityConfig.Value = value;
            }
            else if (toggleName == "InfiniteAirJump")
            {
                infiniteAirJumpConfig.Value = value;
            }
            else if (toggleName == "AutoSoulRefill")
            {
                autoSoulRefillConfig.Value = value;
            }
            else if (toggleName == "InstaKill")
            {
                instaKillConfig.Value = value;
            }
            else if (toggleName == "AllCharmsCost1")
            {
                allCharmsCost1Config.Value = value;
            }
#elif MELONLOADER
            if (toggleName == "Invincibility")
            {
                invincibilityConfig.Value = value;
            }
            else if (toggleName == "InfiniteAirJump")
            {
                infiniteAirJumpConfig.Value = value;
            }
            else if (toggleName == "AutoSoulRefill")
            {
                autoSoulRefillConfig.Value = value;
            }
            else if (toggleName == "InstaKill")
            {
                instaKillConfig.Value = value;
            }
            else if (toggleName == "AllCharmsCost1")
            {
                allCharmsCost1Config.Value = value;
            }
            
            // MelonLoader requires explicit save
            MelonPreferences.Save();
#endif
            logger.Log($"Config saved: {toggleName} = {value}");
        }

        /// <summary>
        /// Toggles auto soul refill on/off.
        /// </summary>
        public void ToggleAutoSoulRefill()
        {
            autoRefillSoul = !autoRefillSoul;
            soulRefillTimer = 0f; // Reset timer

            string status = autoRefillSoul ? "enabled (every 1 second)" : "disabled";
            logger.Log($"Auto Soul Refill: {status}");
            
            // Save to config
            SaveToggleState("AutoSoulRefill", autoRefillSoul);
        }

        /// <summary>
        /// Gets current auto soul refill state.
        /// </summary>
        public bool IsAutoSoulRefillEnabled()
        {
            return autoRefillSoul;
        }
    }
}


