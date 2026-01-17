using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
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
        
        // Fixed frame dimensions
        private const int FixedMenuWidth = 800;
        private const int FixedMenuHeight = 620;  // Increased from 600 to ensure bottom border is visible
        
        // Content margins
        private const int ContentMarginLeft = 64;
        private const int ContentMarginTop = 120;
        private const int ContentMarginBottom = 20; // Add bottom padding for frame border
        
        // Vanilla scrollbar
        private Rectangle scrollBarTrack;
        private Rectangle scrollBarThumb;
        private bool scrolling = false; // Track if dragging scrollbar

        public class DetailItem
        {
            public string Name { get; set; }
            public bool IsCompleted { get; set; }
        }

        public CategoryDetailsMenu(string categoryName, List<DetailItem> items)
            : base(
                Game1.uiViewport.Width / 2 - FixedMenuWidth / 2,
                Game1.uiViewport.Height / 2 - FixedMenuHeight / 2,
                FixedMenuWidth,
                FixedMenuHeight)
        {
            this.categoryName = categoryName;
            this.items = items ?? new List<DetailItem>();

            // Close button - outside top-right corner
            closeButton = new ClickableTextureComponent(
                new Rectangle(
                    xPositionOnScreen + width + 8,
                    yPositionOnScreen + 8,
                    64,
                    64
                ),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                4f
            );

            // Scroll buttons - vanilla pattern
            int scrollButtonX = xPositionOnScreen + width - 32;
            int scrollButtonY = yPositionOnScreen + ContentMarginTop;
            int scrollableHeight = height - ContentMarginTop - 32;
            
            scrollUpButton = new ClickableTextureComponent(
                new Rectangle(scrollButtonX, scrollButtonY, 44, 48),
                Game1.mouseCursors,
                new Rectangle(421, 459, 11, 12),
                4f
            );

            scrollDownButton = new ClickableTextureComponent(
                new Rectangle(scrollButtonX, scrollButtonY + scrollableHeight - 48, 44, 48),
                Game1.mouseCursors,
                new Rectangle(421, 472, 11, 12),
                4f
            );
            
            // Setup scrollbar
            SetupScrollbar();
        }
        
        private void SetupScrollbar()
        {
            int contentHeight = items.Count * ItemHeight;
            int visibleHeight = height - IClickableMenu.borderWidth * 2 - 120;
            
            if (contentHeight > visibleHeight)
            {
                scrollBarTrack = new Rectangle(
                    xPositionOnScreen + width - IClickableMenu.borderWidth - 48,
                    yPositionOnScreen + IClickableMenu.borderWidth + 100,
                    24,
                    height - IClickableMenu.borderWidth * 2 - 200
                );
                
                int scrollBarHeight = Math.Max(24, (int)((float)visibleHeight / contentHeight * scrollBarTrack.Height));
                scrollBarThumb = new Rectangle(scrollBarTrack.X + 4, scrollBarTrack.Y, 16, scrollBarHeight);
            }
        }

        public override void draw(SpriteBatch b)
        {
            // Draw fade background
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            // Draw fixed menu frame
            Game1.drawDialogueBox(
                xPositionOnScreen,
                yPositionOnScreen,
                width,
                height,
                false,
                true
            );

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

            // Draw items list with scroll
            int yOffset = yPositionOnScreen + ContentMarginTop;
            int maxVisible = ItemsPerPage;
            int endIndex = Math.Min(scrollPosition + maxVisible, items.Count);

            for (int i = scrollPosition; i < endIndex; i++)
            {
                var item = items[i];
                int itemY = yOffset + ((i - scrollPosition) * ItemHeight);
                
                // Draw item name with appropriate color
                Color textColor = item.IsCompleted ? new Color(92, 62, 28) : Color.Red;
                string displayText = item.IsCompleted ? $"✓ {item.Name}" : $"✗ {item.Name}";
                
                Utility.drawTextWithShadow(b, displayText, Game1.smallFont,
                    new Vector2(xPositionOnScreen + ContentMarginLeft, itemY), textColor);
            }

            // Draw scrollbar if needed
            if (items.Count > ItemsPerPage)
            {
                DrawVanillaScrollbar(b);
                scrollUpButton.draw(b);
                scrollDownButton.draw(b);
            }

            // Draw close button
            closeButton.draw(b);

            // Draw mouse
            drawMouse(b);
        }
        
        private void DrawVanillaScrollbar(SpriteBatch b)
        {
            // Calculate scrollbar dimensions
            int scrollbarX = xPositionOnScreen + width - 32;
            int scrollbarTop = yPositionOnScreen + ContentMarginTop + 52;
            int scrollableHeight = height - ContentMarginTop - 32;
            int scrollbarBottom = scrollbarTop + scrollableHeight - 104; // Between arrows
            int trackHeight = scrollbarBottom - scrollbarTop;
            
            // Store scrollbar track rectangle
            scrollBarTrack = new Rectangle(
                scrollbarX,
                scrollbarTop,
                24,
                trackHeight
            );
            
            // Calculate thumb size and position
            int totalContentHeight = items.Count * ItemHeight;
            int visibleHeight = ItemsPerPage * ItemHeight;
            float contentRatio = (float)visibleHeight / totalContentHeight;
            int thumbHeight = (int)(trackHeight * contentRatio);
            thumbHeight = Math.Max(thumbHeight, 48);
            
            int maxScrollItems = Math.Max(0, items.Count - ItemsPerPage);
            float scrollPercentage = maxScrollItems > 0 ? (float)scrollPosition / maxScrollItems : 0;
            int thumbY = scrollbarTop + (int)((trackHeight - thumbHeight) * scrollPercentage);
            
            // Store thumb rectangle
            scrollBarThumb = new Rectangle(
                scrollbarX,
                thumbY,
                24,
                thumbHeight
            );
            
            // Draw scrollbar track
            IClickableMenu.drawTextureBox(
                b,
                Game1.mouseCursors,
                new Rectangle(403, 383, 6, 6),
                scrollBarTrack.X,
                scrollBarTrack.Y,
                scrollBarTrack.Width,
                scrollBarTrack.Height,
                Color.White,
                4f,
                false
            );
            
            // Draw scrollbar thumb
            IClickableMenu.drawTextureBox(
                b,
                Game1.mouseCursors,
                new Rectangle(435, 463, 4, 10),
                scrollBarThumb.X + 2,
                scrollBarThumb.Y,
                scrollBarThumb.Width - 4,
                scrollBarThumb.Height,
                Color.White,
                2f,
                false
            );
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

            int maxScrollItems = Math.Max(0, items.Count - ItemsPerPage);

            // Arrow button clicks
            if (scrollUpButton.containsPoint(x, y) && scrollPosition > 0)
            {
                scrollPosition--;
                if (playSound) Game1.playSound("shwip");
                return;
            }

            if (scrollDownButton.containsPoint(x, y) && scrollPosition < maxScrollItems)
            {
                scrollPosition++;
                if (playSound) Game1.playSound("shwip");
                return;
            }
            
            // Scrollbar thumb drag - start dragging
            if (scrollBarThumb.Contains(x, y))
            {
                scrolling = true;
                return;
            }
            
            // Scrollbar track click - jump to position
            if (scrollBarTrack.Contains(x, y))
            {
                float clickPercent = (float)(y - scrollBarTrack.Y) / scrollBarTrack.Height;
                scrollPosition = (int)(clickPercent * maxScrollItems);
                scrollPosition = Math.Max(0, Math.Min(maxScrollItems, scrollPosition));
                if (playSound) Game1.playSound("shiny4");
                return;
            }
        }
        
        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            
            // Handle scrollbar thumb dragging
            if (scrolling)
            {
                int maxScrollItems = Math.Max(0, items.Count - ItemsPerPage);
                
                float dragPercent = (float)(y - scrollBarTrack.Y) / scrollBarTrack.Height;
                scrollPosition = (int)(dragPercent * maxScrollItems);
                scrollPosition = Math.Max(0, Math.Min(maxScrollItems, scrollPosition));
            }
        }
        
        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            scrolling = false; // Stop dragging
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
