using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace PerfectionStats
{
    internal class CategoryDetailsMenu : IClickableMenu
    {
        private readonly string categoryName;
        private readonly List<DetailItem> items;
        private ClickableTextureComponent closeButton;
        private int scrollPosition = 0;
        private ClickableTextureComponent scrollUpButton;
        private ClickableTextureComponent scrollDownButton;
        private const int ItemHeight = 36;
        private const int ItemsPerPage = 10;

        public class DetailItem
        {
            public string Name { get; set; }
            public bool IsCompleted { get; set; }
        }

        public CategoryDetailsMenu(string categoryName, List<DetailItem> items)
            : base(Game1.viewport.Width / 2 - 400, Game1.viewport.Height / 2 - 300, 800, 600)
        {
            this.categoryName = categoryName;
            this.items = items ?? new List<DetailItem>();

            // Close button - larger size
            closeButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 80, yPositionOnScreen + 16, 80, 80),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                5f
            );

            // Scroll buttons - large and visible
            scrollUpButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 16, yPositionOnScreen + 140, 112, 112),
                Game1.mouseCursors,
                new Rectangle(421, 459, 11, 12),
                8f
            );

            scrollDownButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 16, yPositionOnScreen + height - 140, 112, 112),
                Game1.mouseCursors,
                new Rectangle(421, 472, 11, 12),
                8f
            );
        }

        public override void draw(SpriteBatch b)
        {
            // Draw fade background
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            // Draw background
            var parchmentColor = new Color(245, 234, 200);
            b.Draw(Game1.fadeToBlackRect, new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height), parchmentColor);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(16, 368, 16, 16),
                xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 4f, false);

            // Draw title
            string title = categoryName;
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            var titlePos = new Vector2(xPositionOnScreen + (width - titleSize.X) / 2, yPositionOnScreen + 24);
            Utility.drawTextWithShadow(b, title, Game1.dialogueFont, titlePos, new Color(92, 62, 28));

            // Draw completion status
            int completed = items.Count(i => i.IsCompleted);
            int total = items.Count;
            string statusText = UIStrings.FormatCompletionStatus(completed, total);
            Vector2 statusSize = Game1.smallFont.MeasureString(statusText);
            var statusPos = new Vector2(xPositionOnScreen + (width - statusSize.X) / 2, titlePos.Y + titleSize.Y + 8);
            Utility.drawTextWithShadow(b, statusText, Game1.smallFont, statusPos, new Color(120, 78, 36));

            // Draw items list
            int yOffset = yPositionOnScreen + 120;
            int maxVisible = ItemsPerPage;
            int endIndex = System.Math.Min(scrollPosition + maxVisible, items.Count);

            for (int i = scrollPosition; i < endIndex; i++)
            {
                var item = items[i];
                int itemY = yOffset + ((i - scrollPosition) * ItemHeight);
                
                // Draw item name with appropriate color
                Color textColor = item.IsCompleted ? new Color(92, 62, 28) : Color.Red;
                string displayText = item.IsCompleted ? $"✓ {item.Name}" : $"✗ {item.Name}";
                
                Utility.drawTextWithShadow(b, displayText, Game1.smallFont,
                    new Vector2(xPositionOnScreen + 40, itemY), textColor);
            }

            // Draw scroll buttons if needed
            if (items.Count > ItemsPerPage)
            {
                scrollUpButton.draw(b);
                scrollDownButton.draw(b);
            }

            // Draw close button
            closeButton.draw(b);

            // Draw mouse
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (closeButton.containsPoint(x, y))
            {
                exitThisMenu();
                if (playSound) Game1.playSound("bigDeSelect");
                return;
            }

            if (scrollUpButton.containsPoint(x, y) && scrollPosition > 0)
            {
                scrollPosition--;
                if (playSound) Game1.playSound("shwip");
                return;
            }

            if (scrollDownButton.containsPoint(x, y) && scrollPosition < items.Count - ItemsPerPage)
            {
                scrollPosition++;
                if (playSound) Game1.playSound("shwip");
                return;
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);

            if (direction > 0 && scrollPosition > 0)
            {
                scrollPosition--;
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && scrollPosition < items.Count - ItemsPerPage)
            {
                scrollPosition++;
                Game1.playSound("shiny4");
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            closeButton.scale = closeButton.containsPoint(x, y) ? 1.1f : 1f;
            scrollUpButton.scale = scrollUpButton.containsPoint(x, y) ? 1.1f : 1f;
            scrollDownButton.scale = scrollDownButton.containsPoint(x, y) ? 1.1f : 1f;
        }
    }
}
