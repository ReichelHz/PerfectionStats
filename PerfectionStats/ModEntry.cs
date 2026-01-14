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
        private const int ButtonSize = 64;

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            
            string startingMessage = i18n.Get("template.start", new { mod = helper.ModRegistry.ModID, folder = helper.DirectoryPath });
            Monitor.Log(startingMessage, LogLevel.Trace);

            config = helper.ReadConfig<Config>();

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
                    
                    // Position button right after the last tab with same spacing
                    int buttonX = lastTab.bounds.X + lastTab.bounds.Width + 8; // 8 pixels spacing like other tabs
                    int buttonY = lastTab.bounds.Y;
                    
                    perfectionButton = new ClickableTextureComponent(
                        new Rectangle(buttonX, buttonY, ButtonSize, ButtonSize),
                        Game1.mouseCursors,
                        new Rectangle(346, 392, 8, 8), // Star icon
                        4f
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
            // Draw the button if GameMenu is active
            if (Game1.activeClickableMenu is GameMenu && perfectionButton != null)
            {
                bool isHovering = perfectionButton.containsPoint(Game1.getMouseX(), Game1.getMouseY());
                
                // Draw button background box
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
                
                // Draw star icon centered in the button
                Vector2 iconPosition = new Vector2(
                    perfectionButton.bounds.X + (perfectionButton.bounds.Width / 2) - 16, // 16 = half of icon size (8*4 scale / 2)
                    perfectionButton.bounds.Y + (perfectionButton.bounds.Height / 2) - 16
                );
                
                e.SpriteBatch.Draw(
                    Game1.mouseCursors,
                    iconPosition,
                    new Rectangle(346, 392, 8, 8),
                    isHovering ? Color.White : Color.LightGray,
                    0f,
                    Vector2.Zero,
                    4f,
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
            // Check if clicked on button
            if (Game1.activeClickableMenu is GameMenu && perfectionButton != null)
            {
                if (e.Button == SButton.MouseLeft)
                {
                    int mouseX = Game1.getMouseX();
                    int mouseY = Game1.getMouseY();
                    
                    if (perfectionButton.containsPoint(mouseX, mouseY))
                    {
                        Monitor.Log("Perfection Stats button clicked!", LogLevel.Info);
                        Helper.Input.Suppress(e.Button);
                        
                        // Open custom menu
                        Game1.activeClickableMenu = new PerfectionStatsMenu(
                            Game1.uiViewport.Width / 2 - 400,
                            Game1.uiViewport.Height / 2 - 300,
                            800,
                            600
                        );
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
