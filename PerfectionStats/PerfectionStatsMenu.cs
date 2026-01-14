using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace PerfectionStats
{
    internal class PerfectionStatsMenu : IClickableMenu
    {
        private ClickableTextureComponent closeButton;
        
        public PerfectionStatsMenu(int x, int y, int width, int height)
            : base(x, y, width, height, true)
        {
            // Create close button (X) in top-right corner
            closeButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 36, yPositionOnScreen + 8, 48, 48),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                4f
            );
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            
            // Check if close button was clicked
            if (closeButton.containsPoint(x, y))
            {
                exitThisMenu();
                if (playSound)
                    Game1.playSound("bigDeSelect");
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            closeButton.scale = closeButton.containsPoint(x, y) ? 1.1f : 1f;
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            
            // Close on Escape
            if (key == Keys.Escape)
            {
                exitThisMenu();
            }
        }

        public override void draw(SpriteBatch b)
        {
            // Draw fade background
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            
            // Draw the main menu background
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);

            // Draw title
            string title = "Perfection Tracker";
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            Vector2 titlePosition = new Vector2(
                xPositionOnScreen + (width - titleSize.X) / 2,
                yPositionOnScreen + 24
            );
            Utility.drawTextWithShadow(b, title, Game1.dialogueFont, titlePosition, Game1.textColor);

            // Draw content
            string content = "Welcome to the Perfection Stats menu!\n\n" +
                           "Here you can track your progress towards 100% perfection.\n\n" +
                           "This is a work in progress. More features coming soon!";
            
            Vector2 contentPosition = new Vector2(
                xPositionOnScreen + 64,
                yPositionOnScreen + 120
            );
            Utility.drawTextWithShadow(b, content, Game1.smallFont, contentPosition, Game1.textColor);

            // Draw close button
            closeButton.draw(b);

            // Draw mouse cursor
            drawMouse(b);
        }
    }
}
