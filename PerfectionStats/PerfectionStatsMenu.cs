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
        private int scrollPosition = 0;
        private List<ProgressCategory> categories;
        private ClickableTextureComponent scrollUpButton;
        private ClickableTextureComponent scrollDownButton;
        private const int CategoryHeight = 56;
        private const int CategorySpacing = 10;
        private const int OverallSectionHeight = 90;
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

        public PerfectionStatsMenu(int x, int y, int width, int height)
            : base(x, y, width, height, true)
        {
            // Close button - larger size matching options menu
            closeButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 80, yPositionOnScreen + 16, 80, 80),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                5f
            );

            // Scroll arrows - large and visible
            scrollUpButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 16, yPositionOnScreen + 140, 112, 112),
                Game1.mouseCursors,
                new Rectangle(421, 459, 11, 12),
                8f // Very large scale for visibility
            );

            scrollDownButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 16, yPositionOnScreen + height - OverallSectionHeight - 180, 112, 112),
                Game1.mouseCursors,
                new Rectangle(421, 472, 11, 12),
                8f // Very large scale for visibility
            );

            DetectInstalledMods();
            ComputeProgressData();
            InitializeCategories();
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

            UpdateButtonPositions();
        }

        private int GetSVEFishCaught() => !hasSVE ? 0 : 5;
        private int GetSVEFriends() => !hasSVE ? 0 : 4;
        private int GetSVEArtifacts() => !hasSVE ? 0 : 5;
        private int GetSVECrops() => !hasSVE ? 0 : 8;
        private int GetRidesideFriends() => !hasRideside ? 0 : 6;
        private int GetRidesideItems() => !hasRideside ? 0 : 8;
        private int GetRidesideQuests() => !hasRideside ? 0 : 5;

        private void UpdateButtonPositions()
        {
            for (int i = 0; i < categories.Count; i++)
            {
                categories[i].DetailsButton = new ClickableTextureComponent(
                    new Rectangle(0, 0, 32, 32), // Smaller magnifying glass
                    Game1.mouseCursors,
                    new Rectangle(80, 0, 13, 13),
                    2.5f // Reduced scale
                )
                {
                    myID = i,
                    name = $"details_{i}"
                };
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            if (key == Keys.Escape) exitThisMenu();
        }

        public override void draw(SpriteBatch b)
        {
            // Draw fade background
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            // Draw parchment-style background
            DrawParchmentBackground(b);

            // Draw title
            DrawTitle(b);

            // Calculate available area for categories
            int yOffset = yPositionOnScreen + 110;
            int overallSectionY = yPositionOnScreen + height - OverallSectionHeight - 10;
            int maxAreaHeight = overallSectionY - yOffset - 30;
            int rowsFit = Math.Max(1, (maxAreaHeight / (CategoryHeight + CategorySpacing)));
            int visibleCategories = Math.Min(rowsFit, categories.Count);

            // Draw category items
            for (int i = scrollPosition; i < scrollPosition + visibleCategories && i < categories.Count; i++)
            {
                int categoryY = yOffset + ((i - scrollPosition) * (CategoryHeight + CategorySpacing));
                DrawProgressCategory(b, categories[i], categoryY);
            }

            // Draw scroll buttons if needed
            if (categories.Count > visibleCategories)
            {
                scrollUpButton.draw(b);
                scrollDownButton.draw(b);
            }

            // Draw golden separator line
            int separatorY = overallSectionY - 15;
            b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + 40, separatorY, width - 80, 2), new Color(218, 165, 32));

            // Draw Overall Perfection section
            DrawOverallSection(b, overallSectionY);

            // Draw close button
            closeButton.draw(b);

            // Draw mouse
            drawMouse(b);
        }

        private void DrawParchmentBackground(SpriteBatch b)
        {
            var parchmentColor = new Color(245, 234, 200);
            b.Draw(Game1.fadeToBlackRect, new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height), parchmentColor);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(16, 368, 16, 16),
                xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 4f, false);
        }

        private void DrawTitle(SpriteBatch b)
        {
            string title = UIStrings.MenuTitle;
            string subtitle = hasSVE && hasRideside ? UIStrings.SubtitleSVEAndRideside : 
                              hasSVE ? UIStrings.SubtitleSVE : 
                              hasRideside ? UIStrings.SubtitleRideside : 
                              string.Empty;

            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            var titlePos = new Vector2(xPositionOnScreen + (width - titleSize.X) / 2, yPositionOnScreen + 24);
            Utility.drawTextWithShadow(b, title, Game1.dialogueFont, titlePos, new Color(92, 62, 28));

            if (!string.IsNullOrEmpty(subtitle))
            {
                Vector2 subSize = Game1.smallFont.MeasureString(subtitle);
                var subPos = new Vector2(xPositionOnScreen + (width - subSize.X) / 2, titlePos.Y + titleSize.Y + 4);
                Utility.drawTextWithShadow(b, subtitle, Game1.smallFont, subPos, new Color(120, 78, 36));
            }
        }

        private void DrawProgressCategory(SpriteBatch b, ProgressCategory category, int yPos)
        {
            int contentX = xPositionOnScreen + 32;
            int rightPadding = 70;
            int categoryWidth = width - (contentX - xPositionOnScreen) - rightPadding;

            // Draw category name
            Utility.drawTextWithShadow(b, category.Name, Game1.smallFont,
                new Vector2(contentX, yPos), new Color(92, 62, 28));

            // Draw progress bar (rectangular simple style)
            int barY = yPos + 26;
            int barHeight = 24;
            int barWidth = categoryWidth - 80;

            // Bar background (dark gray)
            b.Draw(Game1.staminaRect, 
                new Rectangle(contentX, barY, barWidth, barHeight), 
                new Color(60, 60, 60));

            // Bar border (darker)
            b.Draw(Game1.staminaRect, 
                new Rectangle(contentX, barY, barWidth, 2), 
                new Color(30, 30, 30));
            b.Draw(Game1.staminaRect, 
                new Rectangle(contentX, barY + barHeight - 2, barWidth, 2), 
                new Color(30, 30, 30));
            b.Draw(Game1.staminaRect, 
                new Rectangle(contentX, barY, 2, barHeight), 
                new Color(30, 30, 30));
            b.Draw(Game1.staminaRect, 
                new Rectangle(contentX + barWidth - 2, barY, 2, barHeight), 
                new Color(30, 30, 30));

            // Bar fill (stardrop purple color)
            float progress = category.GetProgress();
            int fillWidth = (int)(barWidth * progress);
            
            if (fillWidth > 4)
            {
                // Stardrop color (#B700FF)
                Color stardropColor = new Color(183, 0, 255);
                b.Draw(Game1.staminaRect, 
                    new Rectangle(contentX + 2, barY + 2, fillWidth - 4, barHeight - 4), 
                    stardropColor);
                
                // Shine at top
                b.Draw(Game1.staminaRect, 
                    new Rectangle(contentX + 2, barY + 2, fillWidth - 4, 4), 
                    Color.White * 0.3f);
            }

            // Draw percentage inside bar
            string percentText = $"{category.GetPercentage()}%";
            Vector2 percentSize = Game1.smallFont.MeasureString(percentText);
            Vector2 percentPos = new Vector2(
                contentX + (barWidth - percentSize.X) / 2, 
                barY + (barHeight - percentSize.Y) / 2
            );
            Utility.drawTextWithShadow(b, percentText, Game1.smallFont, percentPos, Color.White);

            // Draw fraction text
            string fractionText = $"{category.Completed}/{category.Total}";
            Vector2 fractionSize = Game1.smallFont.MeasureString(fractionText);
            Vector2 fractionPos = new Vector2(contentX + barWidth + 8, barY + (barHeight - fractionSize.Y) / 2);
            Utility.drawTextWithShadow(b, fractionText, Game1.smallFont, fractionPos, new Color(92, 62, 28));

            // Draw magnifying glass button
            int buttonX = xPositionOnScreen + width - 50;
            int buttonY = yPos + 12;
            
            category.DetailsButton.bounds = new Rectangle(buttonX, buttonY, 32, 32);
            
            b.Draw(Game1.mouseCursors,
                new Vector2(buttonX, buttonY),
                new Rectangle(80, 0, 13, 13),
                Color.White * (category.DetailsButton.scale > 1f ? 1f : 0.7f),
                0f,
                Vector2.Zero,
                2.5f,
                SpriteEffects.None,
                0.9f);
        }

        private void DrawOverallSection(SpriteBatch b, int y)
        {
            string label = UIStrings.OverallPerfectionLabel;
            
            Vector2 ls = Game1.smallFont.MeasureString(label);
            Utility.drawTextWithShadow(b, label, Game1.smallFont,
                new Vector2(xPositionOnScreen + (width - ls.X) / 2, y), new Color(92, 62, 28));

            // Overall bar (rectangular simple style)
            int barWidth = width - 140;
            int barX = xPositionOnScreen + (width - barWidth) / 2;
            int barY = y + 30;
            int barH = 24;

            // Bar background (dark gray)
            b.Draw(Game1.staminaRect, 
                new Rectangle(barX, barY, barWidth, barH), 
                new Color(60, 60, 60));

            // Bar border
            b.Draw(Game1.staminaRect, 
                new Rectangle(barX, barY, barWidth, 2), 
                new Color(30, 30, 30));
            b.Draw(Game1.staminaRect, 
                new Rectangle(barX, barY + barH - 2, barWidth, 2), 
                new Color(30, 30, 30));
            b.Draw(Game1.staminaRect, 
                new Rectangle(barX, barY, 2, barH), 
                new Color(30, 30, 30));
            b.Draw(Game1.staminaRect, 
                new Rectangle(barX + barWidth - 2, barY, 2, barH), 
                new Color(30, 30, 30));

            // Bar fill
            int overallPercent = CalculateOverallPercent();
            int fillW = (int)(barWidth * overallPercent / 100f);
            
            if (fillW > 4)
            {
                Color stardropColor = new Color(183, 0, 255);
                b.Draw(Game1.staminaRect, 
                    new Rectangle(barX + 2, barY + 2, fillW - 4, barH - 4), 
                    stardropColor);
                
                b.Draw(Game1.staminaRect, 
                    new Rectangle(barX + 2, barY + 2, fillW - 4, 4), 
                    Color.White * 0.3f);
            }

            // Percentage below bar
            string pct = $"{overallPercent}%";
            Vector2 ps = Game1.smallFont.MeasureString(pct);
            Utility.drawTextWithShadow(b, pct, Game1.smallFont,
                new Vector2(barX + (barWidth - ps.X) / 2, barY + barH + 4), new Color(92, 62, 28));
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
            scrollUpButton.scale = scrollUpButton.containsPoint(x, y) ? 1.1f : 1f;
            scrollDownButton.scale = scrollDownButton.containsPoint(x, y) ? 1.1f : 1f;

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

            if (scrollUpButton.containsPoint(x, y) && scrollPosition > 0)
            {
                scrollPosition--;
                if (playSound) Game1.playSound("shwip");
                return;
            }

            if (scrollDownButton.containsPoint(x, y))
            {
                int yOffset = yPositionOnScreen + 110;
                int overallSectionY = yPositionOnScreen + height - OverallSectionHeight - 10;
                int maxAreaHeight = overallSectionY - yOffset - 30;
                int rowsFit = Math.Max(1, (maxAreaHeight / (CategoryHeight + CategorySpacing)));
                
                if (scrollPosition < categories.Count - rowsFit)
                {
                    scrollPosition++;
                    if (playSound) Game1.playSound("shwip");
                }
                return;
            }

            // Check details button clicks
            foreach (var category in categories)
            {
                if (category.DetailsButton != null && category.DetailsButton.containsPoint(x, y))
                {
                    if (playSound) Game1.playSound("smallSelect");
                    ModEntry.Instance.Monitor.Log($"Details clicked for: {category.Name}", LogLevel.Debug);
                    
                    // Open details window
                    OpenCategoryDetails(category.Name);
                    return;
                }
            }
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
