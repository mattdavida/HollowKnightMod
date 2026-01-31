using UnityEngine;
using HollowKnight.Core;
using HollowKnight.Interfaces;
using HollowKnight.Services;

namespace HollowKnight.UserInterface
{
    /// <summary>
    /// Shared context object passed to GUI tabs.
    /// Contains all shared dependencies and state needed by multiple tabs.
    /// </summary>
    public class GuiContext
    {
        // Shared systems
        public ToastSystem ToastSystem { get; set; }
        public ConfirmationSystem ConfirmationSystem { get; set; }
        public IModLogger Logger { get; set; }
        
        // Services
        public HealthService HealthService { get; set; }
        public CurrencyService CurrencyService { get; set; }
        public InvincibilityService InvincibilityService { get; set; }
        public InfiniteAirJumpService InfiniteAirJumpService { get; set; }
        public InstaKillService InstaKillService { get; set; }
        public AbilitiesService AbilitiesService { get; set; }
        public SpellsService SpellsService { get; set; }
        public ItemsService ItemsService { get; set; }
        
        // Reference to mod for toggle methods
        public object ModInstance { get; set; }
        
        // Framework interfaces
        public IInputHandler InputHandler { get; set; }
        public ITimeProvider TimeProvider { get; set; }
        
        // GUI state
        public Rect WindowRect { get; set; }
    }
}

