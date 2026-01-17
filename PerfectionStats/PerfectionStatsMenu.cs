using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using PerfectionStats.ProgressProviders;

namespace PerfectionStats
{
    internal class PerfectionStatsMenu : IClickableMenu
    {
        private ClickableTextureComponent closeButton;
        private List<ProgressCategory> categories;
        
        // FIXED FRAME DIMENSIONS (like OptionsMenu)
        private const int MenuWidth = 1000;
        private const int MenuHeight = 720;
        
        // LAYOUT SECTIONS (vanilla pattern)
        private const int HeaderHeight = 100;
        private const int FooterHeight = 140;
        
        // Content margins
        private const int ContentMarginLeft = 72;
        private const int ContentMarginRight = 64;
        
        // Category row constants
        private const int CategoryHeight = 64;
        private const int CategorySpacing = 10;
        
        private bool hasSVE = false;
        private bool hasRideside = false;
        
        // Progress providers - separated concerns for calculating progress
        private readonly FishProgressProvider fishProgressProvider = new FishProgressProvider();
        private readonly CookingRecipeProgressProvider cookingProgressProvider = new CookingRecipeProgressProvider();
        private readonly CraftingRecipeProgressProvider craftingProgressProvider = new CraftingRecipeProgressProvider();
        private readonly MuseumItemProgressProvider museumProgressProvider = new MuseumItemProgressProvider();
        private readonly FriendshipProgressProvider friendshipProgressProvider = new FriendshipProgressProvider();
        private readonly CropsProgressProvider cropsProgressProvider = new CropsProgressProvider();
        private readonly ForageablesProgressProvider forageablesProgressProvider = new ForageablesProgressProvider();

        // Cached progress data - computed once and reused
        private FishProgressProvider.FishProgressData fishData;
        private CookingRecipeProgressProvider.CookingRecipeProgressData cookingData;
        private CraftingRecipeProgressProvider.CraftingRecipeProgressData craftingData;
        private MuseumItemProgressProvider.MuseumItemProgressData museumData;
        private FriendshipProgressProvider.FriendshipProgressData friendshipData;
        private CropsProgressProvider.CropsProgressData cropsData;
        private ForageablesProgressProvider.ForageablesProgressData forageablesData;

        private class ProgressCategory
        {
            public string Name { get; set; }
            public int Completed { get; set; }
            public int Total { get; set; }
            public ClickableTextureComponent DetailsButton { get; set; }
            
            public float GetProgress() => Total > 0 ? (float)Completed / Total : 0f;
            public int GetPercentage() => Total > 0 ? (int)(GetProgress() * 100) : 0;
        }

        public PerfectionStatsMenu()
            : base(
                (Game1.uiViewport.Width - MenuWidth) / 2,
                (Game1.uiViewport.Height - MenuHeight) / 2,
                MenuWidth,
                MenuHeight,
                true)
        {
            DetectInstalledMods();
            ComputeProgressData();
            InitializeCategories();
            InitializeCloseButton();
        }
        
        private void InitializeCloseButton()
        {
            // Vanilla close button
            closeButton = new ClickableTextureComponent(
                new Rectangle(
                    xPositionOnScreen + width - 48 - 16,
                    yPositionOnScreen + 16,
                    48,
                    48
                ),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                4f
            );
        }
        
        private void ComputeProgressData()
        {
            // Compute all progress data using providers - separated from UI logic
            fishData = fishProgressProvider.GetProgress();
            cookingData = cookingProgressProvider.GetProgress();
            craftingData = craftingProgressProvider.GetProgress();
            museumData = museumProgressProvider.GetProgress();
            friendshipData = friendshipProgressProvider.GetProgress();
            cropsData = cropsProgressProvider.GetProgress();
            forageablesData = forageablesProgressProvider.GetProgress();
        }
        
        private void DetectInstalledMods()
        {
            try
            {
                hasSVE = ModEntry.Instance.Helper.ModRegistry.IsLoaded("spacechase0.StardewValleyExpanded");
                hasRideside = ModEntry.Instance.Helper.ModRegistry.IsLoaded("DailyLunatic.RidesideVillage");
                
                ModEntry.Instance.Monitor.Log($"Mod Detection: SVE={hasSVE}, Rideside={hasRideside}", LogLevel.Debug);
            }
            catch (Exception ex) 
            { 
                ModEntry.Instance.Monitor.Log($"Error detecting mods: {ex.Message}", LogLevel.Debug);
                hasSVE = false;
                hasRideside = false;
            }
        }

        private void InitializeCategories()
        {
            categories = new List<ProgressCategory>();

            var farmer = Game1.player;
            if (farmer == null) return;

            var config = ModEntry.Instance.Helper.ReadConfig<ModConfig>();

            // ===== VANILLA STARDEW VALLEY =====
            // Use pre-computed progress data from providers
            categories.Add(new ProgressCategory { Name = UIStrings.FishSpecies, Completed = fishData.CaughtCount, Total = fishData.TotalCount });
            categories.Add(new ProgressCategory { Name = UIStrings.CookingRecipes, Completed = cookingData.CookedCount, Total = cookingData.TotalCount });
            categories.Add(new ProgressCategory { Name = UIStrings.CraftingRecipes, Completed = craftingData.CraftedCount, Total = craftingData.TotalCount });
            categories.Add(new ProgressCategory { Name = UIStrings.MuseumItems, Completed = museumData.DonatedCount, Total = museumData.TotalCount });
            categories.Add(new ProgressCategory { Name = UIStrings.Friendships, Completed = friendshipData.BestFriendsCount, Total = friendshipData.TotalCount });
            categories.Add(new ProgressCategory { Name = UIStrings.CropsGrown, Completed = cropsData.GrownCount, Total = cropsData.TotalCount });
            categories.Add(new ProgressCategory { Name = UIStrings.Forageables, Completed = forageablesData.FoundCount, Total = forageablesData.TotalCount });

            // ===== STARDEW VALLEY EXPANDED =====
            if (hasSVE)
            {
                categories.Add(new ProgressCategory { Name = UIStrings.SVEFishSpecies, Completed = GetSVEFishCaught(), Total = config.SVECategories.SVEFishSpeciesTotalCount });
                categories.Add(new ProgressCategory { Name = UIStrings.SVENPCsBefriended, Completed = GetSVEFriends(), Total = config.SVECategories.SVENPCsTotalCount });
                categories.Add(new ProgressCategory { Name = UIStrings.SVEArtifacts, Completed = GetSVEArtifacts(), Total = config.SVECategories.SVEArtifactsTotalCount });
                categories.Add(new ProgressCategory { Name = UIStrings.SVECrops, Completed = GetSVECrops(), Total = config.SVECategories.SVECropsTotalCount });
            }

            // ===== RIDGESIDE VILLAGE =====
            if (hasRideside)
            {
                categories.Add(new ProgressCategory { Name = UIStrings.RidesideNPCsMet, Completed = GetRidesideFriends(), Total = config.RidesideCategories.RidesideNPCsTotalCount });
                categories.Add(new ProgressCategory { Name = UIStrings.RidesideItems, Completed = GetRidesideItems(), Total = config.RidesideCategories.RidesideUniqueItemsTotalCount });
                categories.Add(new ProgressCategory { Name = UIStrings.RidesideQuests, Completed = GetRidesideQuests(), Total = config.RidesideCategories.RidesideQuestsTotalCount });
            }
            
            // Initialize detail buttons for all categories
            for (int i = 0; i < categories.Count; i++)
            {
                categories[i].DetailsButton = new ClickableTextureComponent(
                    new Rectangle(0, 0, 32, 32),
                    Game1.mouseCursors,
                    new Rectangle(80, 0, 13, 13),
                    2.0f
                )
                {
                    myID = i,
                    name = $"details_{i}"
                };
            }
        }

        private int GetSVEFishCaught() => !hasSVE ? 0 : 5;
        private int GetSVEFriends() => !hasSVE ? 0 : 4;
        private int GetSVEArtifacts() => !hasSVE ? 0 : 5;
        private int GetSVECrops() => !hasSVE ? 0 : 8;
        private int GetRidesideFriends() => !hasRideside ? 0 : 6;
        private int GetRidesideItems() => !hasRideside ? 0 : 8;
        private int GetRidesideQuests() => !hasRideside ? 0 : 5;

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            if (key == Keys.Escape) exitThisMenu();
        }

        public override void draw(SpriteBatch b)
        {
            // Draw fade background
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            // Draw fixed menu frame
            DrawParchmentBackground(b);

            // HEADER SECTION (Fixed)
            DrawHeader(b);

            // CONTENT SECTION (Middle) - Draw only categories that fit
            DrawContent(b);

            // FOOTER SECTION (Fixed) - Overall perfection bar
            DrawFooter(b);

            // Draw close button
            closeButton.draw(b);

            // Draw mouse
            drawMouse(b);
        }
        
        private void DrawHeader(SpriteBatch b)
        {
            // Draw title
            string title = UIStrings.MenuTitle;
            string subtitle = hasSVE && hasRideside ? UIStrings.SubtitleSVEAndRideside : 
                              hasSVE ? UIStrings.SubtitleSVE : 
                              hasRideside ? UIStrings.SubtitleRideside : 
                              string.Empty;

            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            var titlePos = new Vector2(
                xPositionOnScreen + (width - titleSize.X) / 2, 
                yPositionOnScreen + 32
            );
            Utility.drawTextWithShadow(b, title, Game1.dialogueFont, titlePos, new Color(92, 62, 28));

            if (!string.IsNullOrEmpty(subtitle))
            {
                Vector2 subSize = Game1.smallFont.MeasureString(subtitle);
                var subPos = new Vector2(
                    xPositionOnScreen + (width - subSize.X) / 2, 
                    titlePos.Y + titleSize.Y + 4
                );
                Utility.drawTextWithShadow(b, subtitle, Game1.smallFont, subPos, new Color(120, 78, 36));
            }
            
            // Gold divider line below header
            int dividerY = yPositionOnScreen + HeaderHeight - 4;
            b.Draw(Game1.staminaRect, 
                new Rectangle(xPositionOnScreen + 40, dividerY, width - 80, 2), 
                new Color(218, 165, 32));
        }
        
        private void DrawContent(SpriteBatch b)
        {
            // Content area boundaries
            int contentX = xPositionOnScreen + ContentMarginLeft;
            int contentY = yPositionOnScreen + HeaderHeight;
            int contentWidth = width - ContentMarginLeft - ContentMarginRight;
            int contentHeight = height - HeaderHeight - FooterHeight;
            
            // Calculate how many categories fit (NO SCROLLING)
            int categoriesThatFit = (contentHeight - 20) / (CategoryHeight + CategorySpacing);
            int categoriesToDraw = Math.Min(categoriesThatFit, categories.Count);
            
            // Draw categories that fit
            for (int i = 0; i < categoriesToDraw; i++)
            {
                int categoryY = contentY + 10 + (i * (CategoryHeight + CategorySpacing));
                DrawProgressCategory(b, categories[i], categoryY, contentX, contentWidth);
            }
        }
        
        private void DrawFooter(SpriteBatch b)
        {
            // Footer area for overall perfection bar
            int footerY = yPositionOnScreen + height - FooterHeight;
            int contentX = xPositionOnScreen + ContentMarginLeft;
            int contentWidth = width - ContentMarginLeft - ContentMarginRight;
            
            // Gold divider line above footer
            b.Draw(Game1.staminaRect, 
                new Rectangle(xPositionOnScreen + 40, footerY, width - 80, 2), 
                new Color(218, 165, 32));
            
            // Draw overall perfection section
            DrawOverallSection(b, footerY + 10, contentX, contentWidth);
        }
        
        private void DrawParchmentBackground(SpriteBatch b)
        {
            var parchmentColor = new Color(245, 234, 200);
            b.Draw(Game1.fadeToBlackRect, new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height), parchmentColor);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(16, 368, 16, 16),
                xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 4f, false);
        }

        private void DrawProgressCategory(SpriteBatch b, ProgressCategory category, int yPos, int contentX, int contentWidth)
        {
            // Increased space for row to prevent overlap
            // Reserve space for: numbers (80px) + magnifying glass (48px) + gap (20px) = 148px
            int reservedRightSpace = 148;
            
            // Draw category name
            Utility.drawTextWithShadow(b, category.Name, Game1.smallFont,
                new Vector2(contentX, yPos), new Color(92, 62, 28));

            // Progress bar
            int barY = yPos + 30;
            int barHeight = 24;
            int barWidth = contentWidth - reservedRightSpace;

            // Bar background
            b.Draw(Game1.staminaRect, 
                new Rectangle(contentX, barY, barWidth, barHeight), 
                new Color(60, 60, 60));

            // Bar border
            b.Draw(Game1.staminaRect, new Rectangle(contentX, barY, barWidth, 2), new Color(30, 30, 30));
            b.Draw(Game1.staminaRect, new Rectangle(contentX, barY + barHeight - 2, barWidth, 2), new Color(30, 30, 30));
            b.Draw(Game1.staminaRect, new Rectangle(contentX, barY, 2, barHeight), new Color(30, 30, 30));
            b.Draw(Game1.staminaRect, new Rectangle(contentX + barWidth - 2, barY, 2, barHeight), new Color(30, 30, 30));

            // Bar fill
            float progress = category.GetProgress();
            int fillWidth = (int)(barWidth * progress);
            
            if (fillWidth > 4)
            {
                Color stardropColor = new Color(205, 92, 255);
                b.Draw(Game1.staminaRect, 
                    new Rectangle(contentX + 2, barY + 2, fillWidth - 4, barHeight - 4), 
                    stardropColor);
                
                b.Draw(Game1.staminaRect, 
                    new Rectangle(contentX + 2, barY + 2, fillWidth - 4, 4), 
                    Color.White * 0.3f);
            }

            // Percentage inside bar
            string percentText = $"{category.GetPercentage()}%";
            Vector2 percentSize = Game1.smallFont.MeasureString(percentText);
            Vector2 percentPos = new Vector2(
                contentX + (barWidth - percentSize.X) / 2, 
                barY + (barHeight - percentSize.Y) / 2
            );
            Utility.drawTextWithShadow(b, percentText, Game1.smallFont, percentPos, Color.White);

            // Numbers - fixed position with proper spacing
            string fractionText = $"{category.Completed}/{category.Total}";
            Vector2 fractionSize = Game1.smallFont.MeasureString(fractionText);
            Vector2 fractionPos = new Vector2(
                contentX + barWidth + 12, 
                barY + (barHeight - fractionSize.Y) / 2
            );
            Utility.drawTextWithShadow(b, fractionText, Game1.smallFont, fractionPos, new Color(92, 62, 28));

            // Magnifying glass button - fixed position, won't overlap
            int buttonX = contentX + contentWidth - 40;
            int buttonY = barY - 2;
            
            category.DetailsButton.bounds = new Rectangle(buttonX, buttonY, 32, 32);
            
            b.Draw(Game1.mouseCursors,
                new Vector2(buttonX, buttonY),
                new Rectangle(80, 0, 13, 13),
                Color.White * (category.DetailsButton.scale > 1f ? 1f : 0.7f),
                0f,
                Vector2.Zero,
                2.0f,
                SpriteEffects.None,
                0.9f);
        }

        private void DrawOverallSection(SpriteBatch b, int y, int contentX, int contentWidth)
        {
            string label = UIStrings.OverallPerfectionLabel;
            
            // Center label in content area
            Vector2 ls = Game1.smallFont.MeasureString(label) * 1.2f;
            Utility.drawTextWithShadow(b, label, Game1.smallFont,
                new Vector2(contentX + (contentWidth - ls.X) / 2, y + 12), new Color(92, 62, 28), 1.2f);

            // Overall bar
            int barWidth = contentWidth - 100;
            int barX = contentX + (contentWidth - barWidth) / 2;
            int barY = y + 48;
            int barH = 28;

            // Bar background
            b.Draw(Game1.staminaRect, 
                new Rectangle(barX, barY, barWidth, barH), 
                new Color(60, 60, 60));

            // Bar border
            b.Draw(Game1.staminaRect, new Rectangle(barX, barY, barWidth, 2), new Color(30, 30, 30));
            b.Draw(Game1.staminaRect, new Rectangle(barX, barY + barH - 2, barWidth, 2), new Color(30, 30, 30));
            b.Draw(Game1.staminaRect, new Rectangle(barX, barY, 2, barH), new Color(30, 30, 30));
            b.Draw(Game1.staminaRect, new Rectangle(barX + barWidth - 2, barY, 2, barH), new Color(30, 30, 30));

            // Bar fill
            int overallPercent = CalculateOverallPercent();
            int fillW = (int)(barWidth * overallPercent / 100f);
            
            if (fillW > 4)
            {
                Color stardropColor = new Color(205, 92, 255);
                b.Draw(Game1.staminaRect, 
                    new Rectangle(barX + 2, barY + 2, fillW - 4, barH - 4), 
                    stardropColor);
                
                b.Draw(Game1.staminaRect, 
                    new Rectangle(barX + 2, barY + 2, fillW - 4, 6), 
                    Color.White * 0.3f);
            }

            // Percentage below bar
            string pct = $"{overallPercent}%";
            Vector2 ps = Game1.smallFont.MeasureString(pct) * 1.1f;
            Utility.drawTextWithShadow(b, pct, Game1.smallFont,
                new Vector2(barX + (barWidth - ps.X) / 2, barY + barH + 12), new Color(92, 62, 28), 1.1f);
        }

        private int CalculateOverallPercent()
        {
            // Overall Perfection is a pure aggregation of existing category progress
            // It does NOT recalculate any game data - it only averages the progress
            // from all dynamically-calculated categories (fish, recipes, museum, etc.)
            // This ensures consistency with individual progress bars and automatically
            // includes any content added by mods through those categories.
            
            if (categories == null || categories.Count == 0) return 0;
            
            // Sum all category progress values (each category's GetProgress() returns 0.0 to 1.0)
            float totalProgress = 0f;
            foreach (var category in categories)
            {
                totalProgress += category.GetProgress();
            }
            
            // Calculate average progress across all categories and convert to percentage
            float averageProgress = totalProgress / categories.Count;
            int overallPercentage = (int)(averageProgress * 100f);
            
            return overallPercentage;
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            closeButton.scale = closeButton.containsPoint(x, y) ? 1.1f : 1f;

            foreach (var category in categories)
            {
                if (category.DetailsButton != null)
                    category.DetailsButton.scale = category.DetailsButton.containsPoint(x, y) ? 1.3f : 1f;
            }
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

            // Check details button clicks on visible categories
            int contentY = yPositionOnScreen + HeaderHeight;
            int contentHeight = height - HeaderHeight - FooterHeight;
            int categoriesThatFit = (contentHeight - 20) / (CategoryHeight + CategorySpacing);
            int categoriesToCheck = Math.Min(categoriesThatFit, categories.Count);
            
            for (int i = 0; i < categoriesToCheck; i++)
            {
                var category = categories[i];
                
                if (category.DetailsButton != null && category.DetailsButton.containsPoint(x, y))
                {
                    if (playSound) Game1.playSound("smallSelect");
                    ModEntry.Instance.Monitor.Log($"Details clicked for: {category.Name}", LogLevel.Debug);
                    OpenCategoryDetails(category.Name);
                    return;
                }
            }
        }
        
        public override void receiveScrollWheelAction(int direction)
        {
            // No scrolling
            base.receiveScrollWheelAction(direction);
        }
        
        private void OpenCategoryDetails(string categoryName)
        {
            List<CategoryDetailsMenu.DetailItem> items = new List<CategoryDetailsMenu.DetailItem>();

            // Use cached progress data from providers - no recalculation needed
            if (categoryName == UIStrings.FishSpecies)
            {
                items = fishData.DetailItems;
            }
            else if (categoryName == UIStrings.CookingRecipes)
            {
                items = cookingData.DetailItems;
            }
            else if (categoryName == UIStrings.CraftingRecipes)
            {
                items = craftingData.DetailItems;
            }
            else if (categoryName == UIStrings.MuseumItems)
            {
                items = museumData.DetailItems;
            }
            else if (categoryName == UIStrings.Friendships)
            {
                items = friendshipData.DetailItems;
            }
            else if (categoryName == UIStrings.CropsGrown)
            {
                items = cropsData.DetailItems;
            }
            else if (categoryName == UIStrings.Forageables)
            {
                items = forageablesData.DetailItems;
            }

            Game1.activeClickableMenu = new CategoryDetailsMenu(categoryName, items);
        }
    }
}
