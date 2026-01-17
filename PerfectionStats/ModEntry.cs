using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace PerfectionStats
{
    public class ModEntry : Mod
    {
        internal static ModEntry Instance;
        internal Config config;
        internal ITranslationHelper i18n => Helper.Translation;
        private ClickableTextureComponent perfectionButton;
        private Texture2D trophyTexture;
        private const int ButtonSize = 64;

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            
            string startingMessage = i18n.Get("template.start", new { mod = helper.ModRegistry.ModID, folder = helper.DirectoryPath });
            Monitor.Log(startingMessage, LogLevel.Trace);

            config = helper.ReadConfig<Config>();

            // Load trophy texture
            trophyTexture = helper.ModContent.Load<Texture2D>("assets/trofeo.png");
            Monitor.Log("Trophy texture loaded", LogLevel.Debug);

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Display.RenderedActiveMenu += Display_RenderedActiveMenu;
            helper.Events.Input.ButtonPressed += Input_ButtonPressedForButton;
            
            Monitor.Log("PerfectionStats initialized", LogLevel.Info);
        }

        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            // Create button when GameMenu opens
            if (e.NewMenu is GameMenu gameMenu)
            {
                Monitor.Log("GameMenu opened, creating Perfection Stats button", LogLevel.Debug);
                
                // Get the last tab (exit tab) to position our button next to it
                var tabs = gameMenu.tabs;
                if (tabs != null && tabs.Count > 0)
                {
                    var lastTab = tabs[tabs.Count - 1];
                    
                    // Position button right after the last tab with smaller spacing (2 pixels instead of 8)
                    int buttonX = lastTab.bounds.X + lastTab.bounds.Width + 2;
                    int buttonY = lastTab.bounds.Y;
                    
                    perfectionButton = new ClickableTextureComponent(
                        new Rectangle(buttonX, buttonY, ButtonSize, ButtonSize),
                        trophyTexture,
                        new Rectangle(0, 0, trophyTexture.Width, trophyTexture.Height),
                        1f
                    )
                    {
                        myID = 99999,
                        name = "perfection-stats-button"
                    };
                    
                    Monitor.Log($"Button created at ({buttonX}, {buttonY}) next to '{lastTab.name}' tab", LogLevel.Debug);
                }
            }
            else
            {
                perfectionButton = null;
            }
        }

        private void Display_RenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            // Draw the button on all tabs EXCEPT the map tab (index 3)
            if (Game1.activeClickableMenu is GameMenu gameMenu && perfectionButton != null)
            {
                // Don't draw on map tab (currentTab == 3)
                if (gameMenu.currentTab == 3)
                    return;

                bool isHovering = perfectionButton.containsPoint(Game1.getMouseX(), Game1.getMouseY());
                
                // Draw button background box (same style as other tabs)
                IClickableMenu.drawTextureBox(
                    e.SpriteBatch,
                    Game1.mouseCursors,
                    new Rectangle(16, 368, 16, 16),
                    perfectionButton.bounds.X,
                    perfectionButton.bounds.Y,
                    perfectionButton.bounds.Width,
                    perfectionButton.bounds.Height,
                    isHovering ? Color.Wheat : Color.White,
                    4f,
                    false
                );
                
                // Calculate trophy icon position to center it in the button with offset adjustment
                float trophyScale = (float)(ButtonSize - 16) / trophyTexture.Width; // Leave 8 pixels margin on each side
                int scaledWidth = (int)(trophyTexture.Width * trophyScale);
                int scaledHeight = (int)(trophyTexture.Height * trophyScale);
                
                // Adjust position with offset to center properly (move right and down slightly)
                int offsetX = 3; // Move 3 pixels to the right
                int offsetY = 3; // Move 3 pixels down
                
                Vector2 trophyPosition = new Vector2(
                    perfectionButton.bounds.X + (ButtonSize - scaledWidth) / 2 + offsetX,
                    perfectionButton.bounds.Y + (ButtonSize - scaledHeight) / 2 + offsetY
                );
                
                // Draw trophy icon
                e.SpriteBatch.Draw(
                    trophyTexture,
                    trophyPosition,
                    new Rectangle(0, 0, trophyTexture.Width, trophyTexture.Height),
                    isHovering ? Color.White : new Color(220, 220, 220),
                    0f,
                    Vector2.Zero,
                    trophyScale,
                    SpriteEffects.None,
                    0.88f
                );
                
                // Draw tooltip if hovering
                if (isHovering)
                {
                    IClickableMenu.drawHoverText(e.SpriteBatch, "Perfection Stats", Game1.smallFont);
                }
            }
        }

        private void Input_ButtonPressedForButton(object sender, ButtonPressedEventArgs e)
        {
            // Check if clicked on button (skip on map tab)
            if (Game1.activeClickableMenu is GameMenu gameMenu && perfectionButton != null)
            {
                // Don't allow clicking on map tab
                if (gameMenu.currentTab == 3)
                    return;

                if (e.Button == SButton.MouseLeft)
                {
                    int mouseX = Game1.getMouseX();
                    int mouseY = Game1.getMouseY();
                    
                    if (perfectionButton.containsPoint(mouseX, mouseY))
                    {
                        Monitor.Log("Perfection Stats button clicked!", LogLevel.Info);
                        Helper.Input.Suppress(e.Button);
                        
                        // Open custom menu - now uses vanilla container pattern
                        Game1.activeClickableMenu = new PerfectionStatsMenu();
                    }
                }
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            e.Button.TryGetKeyboard(out Keys keyPressed);

            if (keyPressed.Equals(config.debugKey))
                Monitor.Log(i18n.Get("template.key"), LogLevel.Info);
        }
    }
}
