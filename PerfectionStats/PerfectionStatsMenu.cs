using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

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
            // Close button - mucho más grande, acorde al menú de opciones
            closeButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 80, yPositionOnScreen + 16, 80, 80),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                5f
            );

            // Scroll arrows - mucho más grandes y visibles
            scrollUpButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 16, yPositionOnScreen + 140, 112, 112),
                Game1.mouseCursors,
                new Rectangle(421, 459, 11, 12),
                8f // Escala muy aumentada para mayor visibilidad
            );

            scrollDownButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 16, yPositionOnScreen + height - OverallSectionHeight - 180, 112, 112),
                Game1.mouseCursors,
                new Rectangle(421, 472, 11, 12),
                8f // Escala muy aumentada para mayor visibilidad
            );

            DetectInstalledMods();
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

        private void InitializeCategories()
        {
            categories = new List<ProgressCategory>();

            var farmer = Game1.player;
            if (farmer == null) return;

            var config = ModEntry.Instance.Helper.ReadConfig<ModConfig>();

            // ===== VANILLA STARDEW VALLEY =====
            categories.Add(new ProgressCategory { Name = "Fish Species", Completed = GetFishCaught(), Total = config.VanillaCategories.FishSpeciesTotalCount });
            categories.Add(new ProgressCategory { Name = "Cooking Recipes", Completed = farmer.recipesCooked.Count(), Total = config.VanillaCategories.CookingRecipesTotalCount });
            categories.Add(new ProgressCategory { Name = "Crafting Recipes", Completed = farmer.craftingRecipes.Values.Count(), Total = config.VanillaCategories.CraftingRecipesTotalCount });
            categories.Add(new ProgressCategory { Name = "Museum Items", Completed = GetMuseumItems(), Total = config.VanillaCategories.MuseumItemsTotalCount });
            categories.Add(new ProgressCategory { Name = "Friendships (8+ Hearts)", Completed = GetBestFriends(), Total = config.VanillaCategories.FriendshipsTotalCount });
            categories.Add(new ProgressCategory { Name = "Crops Grown", Completed = GetCropsGrown(), Total = config.VanillaCategories.CropsGrownTotalCount });
            categories.Add(new ProgressCategory { Name = "Forageables", Completed = GetForageablesFound(), Total = config.VanillaCategories.ForageablesFoundTotalCount });

            // ===== STARDEW VALLEY EXPANDED =====
            if (hasSVE)
            {
                categories.Add(new ProgressCategory { Name = "SVE: Fish Species", Completed = GetSVEFishCaught(), Total = config.SVECategories.SVEFishSpeciesTotalCount });
                categories.Add(new ProgressCategory { Name = "SVE: NPCs Befriended", Completed = GetSVEFriends(), Total = config.SVECategories.SVENPCsTotalCount });
                categories.Add(new ProgressCategory { Name = "SVE: Artifacts", Completed = GetSVEArtifacts(), Total = config.SVECategories.SVEArtifactsTotalCount });
                categories.Add(new ProgressCategory { Name = "SVE: Crops", Completed = GetSVECrops(), Total = config.SVECategories.SVECropsTotalCount });
            }

            // ===== RIDGESIDE VILLAGE =====
            if (hasRideside)
            {
                categories.Add(new ProgressCategory { Name = "Ridgeside: NPCs Met", Completed = GetRidesideFriends(), Total = config.RidesideCategories.RidesideNPCsTotalCount });
                categories.Add(new ProgressCategory { Name = "Ridgeside: Items", Completed = GetRidesideItems(), Total = config.RidesideCategories.RidesideUniqueItemsTotalCount });
                categories.Add(new ProgressCategory { Name = "Ridgeside: Quests", Completed = GetRidesideQuests(), Total = config.RidesideCategories.RidesideQuestsTotalCount });
            }

            UpdateButtonPositions();
        }

        private int GetFishCaught() => Game1.player.fishCaught != null ? Game1.player.fishCaught.Keys.Count() : 0;
        
        private int GetMuseumItems()
        {
            // Obtener items donados al museo correctamente
            var museum = Game1.locations.OfType<StardewValley.Locations.LibraryMuseum>().FirstOrDefault();
            if (museum != null)
            {
                return museum.museumPieces.Count();
            }
            return 0;
        }
        
        private int GetBestFriends()
        {
            try
            {
                var friendships = Game1.player.friendshipData;
                return friendships.Values.Count(f => f.Points >= 2000);
            }
            catch { return 0; }
        }
        private int GetCropsGrown() => Math.Min(Game1.player.farmingLevel.Value * 2, 26);
        private int GetForageablesFound() => Math.Min(Game1.player.foragingLevel.Value, 20);
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
                    new Rectangle(0, 0, 32, 32), // Lupa más pequeña
                    Game1.mouseCursors,
                    new Rectangle(80, 0, 13, 13),
                    2.5f // Escala reducida
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

            // Calcular el área disponible para las categorías
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

            // Dibujar línea separadora dorada
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
            string title = "Perfection Tracker";
            string subtitle = hasSVE && hasRideside ? "(SVE + Rideside)" : hasSVE ? "(SVE)" : hasRideside ? "(Rideside)" : string.Empty;

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

            // Draw progress bar (RECTANGULAR SIMPLE)
            int barY = yPos + 26;
            int barHeight = 24;
            int barWidth = categoryWidth - 80;

            // Fondo de la barra (gris oscuro)
            b.Draw(Game1.staminaRect, 
                new Rectangle(contentX, barY, barWidth, barHeight), 
                new Color(60, 60, 60));

            // Borde de la barra (más oscuro)
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

            // Relleno de la barra (color púrpura de fruta estelar)
            float progress = category.GetProgress();
            int fillWidth = (int)(barWidth * progress);
            
            if (fillWidth > 4)
            {
                // Color de la fruta estelar (#B700FF)
                Color stardropColor = new Color(183, 0, 255);
                b.Draw(Game1.staminaRect, 
                    new Rectangle(contentX + 2, barY + 2, fillWidth - 4, barHeight - 4), 
                    stardropColor);
                
                // Brillo en la parte superior
                b.Draw(Game1.staminaRect, 
                    new Rectangle(contentX + 2, barY + 2, fillWidth - 4, 4), 
                    Color.White * 0.3f);
            }

            // Dibujar porcentaje DENTRO de la barra
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
            string label = "OVERALL PERFECTION";
            
            Vector2 ls = Game1.smallFont.MeasureString(label);
            Utility.drawTextWithShadow(b, label, Game1.smallFont,
                new Vector2(xPositionOnScreen + (width - ls.X) / 2, y), new Color(92, 62, 28));

            // Overall bar (RECTANGULAR SIMPLE)
            int barWidth = width - 140;
            int barX = xPositionOnScreen + (width - barWidth) / 2;
            int barY = y + 30;
            int barH = 24;

            // Fondo de la barra (gris oscuro)
            b.Draw(Game1.staminaRect, 
                new Rectangle(barX, barY, barWidth, barH), 
                new Color(60, 60, 60));

            // Borde de la barra
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

            // Relleno
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

            // Porcentaje DEBAJO de la barra
            string pct = $"{overallPercent}%";
            Vector2 ps = Game1.smallFont.MeasureString(pct);
            Utility.drawTextWithShadow(b, pct, Game1.smallFont,
                new Vector2(barX + (barWidth - ps.X) / 2, barY + barH + 4), new Color(92, 62, 28));
        }

        private int CalculateOverallPercent()
        {
            if (categories == null || categories.Count == 0) return 0;
            float sum = 0f;
            foreach (var c in categories) sum += c.GetProgress();
            return (int)(sum / categories.Count * 100f);
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
                    
                    // Abrir ventana de detalles
                    OpenCategoryDetails(category.Name);
                    return;
                }
            }
        }

        private void OpenCategoryDetails(string categoryName)
        {
            List<CategoryDetailsMenu.DetailItem> items = new List<CategoryDetailsMenu.DetailItem>();

            switch (categoryName)
            {
                case "Fish Species":
                    items = GetFishDetails();
                    break;
                case "Cooking Recipes":
                    items = GetCookingRecipeDetails();
                    break;
                case "Crafting Recipes":
                    items = GetCraftingRecipeDetails();
                    break;
                case "Museum Items":
                    items = GetMuseumItemDetails();
                    break;
                case "Friendships (8+ Hearts)":
                    items = GetFriendshipDetails();
                    break;
                case "Crops Grown":
                    items = GetCropsDetails();
                    break;
                case "Forageables":
                    items = GetForageablesDetails();
                    break;
                default:
                    items.Add(new CategoryDetailsMenu.DetailItem { Name = "Details not available yet", IsCompleted = false });
                    break;
            }

            Game1.activeClickableMenu = new CategoryDetailsMenu(categoryName, items);
        }

        private List<CategoryDetailsMenu.DetailItem> GetFishDetails()
        {
            var items = new List<CategoryDetailsMenu.DetailItem>();
            var caughtFish = Game1.player.fishCaught?.Keys.Select(k => int.Parse(k)).ToList() ?? new List<int>();
            
            // Lista de IDs de peces del juego (principales)
            var allFish = new Dictionary<int, string>
            {
                {128, "Pufferfish"}, {129, "Anchovy"}, {130, "Tuna"}, {131, "Sardine"}, {132, "Bream"},
                {136, "Largemouth Bass"}, {137, "Smallmouth Bass"}, {138, "Rainbow Trout"}, {139, "Salmon"},
                {140, "Walleye"}, {141, "Perch"}, {142, "Carp"}, {143, "Catfish"}, {144, "Pike"},
                {145, "Sunfish"}, {146, "Red Mullet"}, {147, "Herring"}, {148, "Eel"}, {149, "Octopus"},
                {150, "Red Snapper"}, {151, "Squid"}, {154, "Sea Cucumber"}, {155, "Super Cucumber"},
                {156, "Ghostfish"}, {158, "Stonefish"}, {159, "Crimsonfish"}, {160, "Angler"},
                {161, "Ice Pip"}, {162, "Lava Eel"}, {163, "Legend"}, {164, "Sandfish"}, {165, "Scorpion Carp"},
                {682, "Mutant Carp"}, {698, "Sturgeon"}, {699, "Tiger Trout"}, {700, "Bullhead"},
                {701, "Tilapia"}, {702, "Chub"}, {704, "Dorado"}, {705, "Albacore"}, {706, "Shad"},
                {707, "Lingcod"}, {708, "Halibut"}, {715, "Lobster"}, {716, "Crayfish"}, {717, "Crab"},
                {718, "Cockle"}, {719, "Mussel"}, {720, "Shrimp"}, {721, "Snail"}, {722, "Periwinkle"},
                {723, "Oyster"}, {734, "Woodskip"}, {775, "Glacierfish"}, {795, "Void Salmon"},
                {796, "Slimejack"}, {798, "Midnight Squid"}, {799, "Spook Fish"}, {800, "Blobfish"}
            };

            foreach (var fish in allFish.OrderBy(f => f.Value))
            {
                items.Add(new CategoryDetailsMenu.DetailItem
                {
                    Name = fish.Value,
                    IsCompleted = caughtFish.Contains(fish.Key)
                });
            }

            return items;
        }

        private List<CategoryDetailsMenu.DetailItem> GetCookingRecipeDetails()
        {
            var items = new List<CategoryDetailsMenu.DetailItem>();
            var knownRecipes = Game1.player.cookingRecipes.Keys.ToList();
            
            // Lista de recetas de cocina principales
            var allRecipes = new List<string>
            {
                "Fried Egg", "Omelet", "Pancakes", "Bread", "Tortilla", "Triple Shot Espresso",
                "Hashbrowns", "Algae Soup", "Pale Broth", "Plum Pudding", "Artichoke Dip",
                "Stir Fry", "Roasted Hazelnuts", "Pumpkin Pie", "Radish Salad", "Fruit Salad",
                "Blackberry Cobbler", "Cranberry Candy", "Bruschetta", "Coleslaw", "Fiddlehead Risotto",
                "Poppyseed Muffin", "Chowder", "Fish Stew", "Escargot", "Lobster Bisque",
                "Maple Bar", "Crab Cakes", "Shrimp Cocktail", "Tom Kha Soup", "Trout Soup",
                "Chocolate Cake", "Pink Cake", "Rhubarb Pie", "Cookie", "Spaghetti",
                "Fried Mushroom", "Salmon Dinner", "Pepper Poppers", "Pizza", "Parsnip Soup",
                "Maki Roll", "Tortilla", "Red Plate", "Eggplant Parmesan", "Rice Pudding",
                "Ice Cream", "Blueberry Tart", "Autumn's Bounty", "Pumpkin Soup", "Super Meal",
                "Cranberry Sauce", "Stuffing", "Farmer's Lunch", "Survival Burger", "Dish O' The Sea",
                "Miner's Treat", "Roots Platter"
            };

            foreach (var recipe in allRecipes.OrderBy(r => r))
            {
                items.Add(new CategoryDetailsMenu.DetailItem
                {
                    Name = recipe,
                    IsCompleted = knownRecipes.Contains(recipe)
                });
            }

            return items;
        }

        private List<CategoryDetailsMenu.DetailItem> GetCraftingRecipeDetails()
        {
            var items = new List<CategoryDetailsMenu.DetailItem>();
            var knownRecipes = Game1.player.craftingRecipes.Keys.ToList();
            
            // Lista de recetas de crafteo principales
            var allRecipes = new List<string>
            {
                "Cherry Bomb", "Bomb", "Mega Bomb", "Gate", "Wood Fence", "Stone Fence",
                "Iron Fence", "Hardwood Fence", "Sprinkler", "Quality Sprinkler", "Iridium Sprinkler",
                "Mayonnaise Machine", "Bee House", "Preserves Jar", "Cheese Press", "Loom",
                "Keg", "Oil Maker", "Cask", "Basic Fertilizer", "Quality Fertilizer", "Speed-Gro",
                "Deluxe Speed-Gro", "Basic Retaining Soil", "Quality Retaining Soil", "Wild Seeds (Sp)",
                "Wild Seeds (Su)", "Wild Seeds (Fa)", "Wild Seeds (Wi)", "Ancient Seeds",
                "Grass Starter", "Tea Sapling", "Fiber Seeds", "Wood Floor", "Straw Floor",
                "Weathered Floor", "Crystal Floor", "Stone Floor", "Wood Path", "Gravel Path",
                "Cobblestone Path", "Stepping Stone Path", "Crystal Path", "Torch", "Campfire",
                "Wooden Brazier", "Stone Brazier", "Gold Brazier", "Carved Brazier", "Stump Brazier",
                "Barrel Brazier", "Skull Brazier", "Marble Brazier", "Wood Lamp-post", "Iron Lamp-post"
            };

            foreach (var recipe in allRecipes.OrderBy(r => r))
            {
                items.Add(new CategoryDetailsMenu.DetailItem
                {
                    Name = recipe,
                    IsCompleted = knownRecipes.Contains(recipe)
                });
            }

            return items;
        }

        private List<CategoryDetailsMenu.DetailItem> GetMuseumItemDetails()
        {
            var items = new List<CategoryDetailsMenu.DetailItem>();
            var museum = Game1.locations.OfType<StardewValley.Locations.LibraryMuseum>().FirstOrDefault();
            var donatedItems = museum?.museumPieces.Values.Select(v => int.Parse(v)).ToList() ?? new List<int>();
            
            // Lista simplificada de items del museo
            var allItems = new Dictionary<int, string>
            {
                {96, "Dwarf Scroll I"}, {97, "Dwarf Scroll II"}, {98, "Dwarf Scroll III"}, {99, "Dwarf Scroll IV"},
                {100, "Chipped Amphora"}, {101, "Arrowhead"}, {103, "Ancient Doll"}, {104, "Elvish Jewelry"},
                {105, "Chewing Stick"}, {106, "Ornamental Fan"}, {107, "Dinosaur Egg"}, {108, "Rare Disc"},
                {109, "Ancient Sword"}, {110, "Rusty Spoon"}, {111, "Rusty Spur"}, {112, "Rusty Cog"},
                {113, "Chicken Statue"}, {114, "Ancient Seed"}, {115, "Prehistoric Tool"}, {120, "Amethyst"},
                {122, "Aquamarine"}, {124, "Jade"}, {330, "Clay"}, {535, "Geode"}, {536, "Frozen Geode"}
            };

            foreach (var item in allItems.OrderBy(i => i.Value))
            {
                items.Add(new CategoryDetailsMenu.DetailItem
                {
                    Name = item.Value,
                    IsCompleted = donatedItems.Contains(item.Key)
                });
            }

            return items;
        }

        private List<CategoryDetailsMenu.DetailItem> GetFriendshipDetails()
        {
            var items = new List<CategoryDetailsMenu.DetailItem>();
            var friendships = Game1.player.friendshipData;
            
            var allNPCs = new List<string>
            {
                "Abigail", "Alex", "Caroline", "Clint", "Demetrius", "Dwarf", "Elliott",
                "Emily", "Evelyn", "George", "Gus", "Haley", "Harvey", "Jas", "Jodi",
                "Kent", "Krobus", "Leah", "Lewis", "Linus", "Marnie", "Maru", "Pam",
                "Penny", "Pierre", "Robin", "Sam", "Sandy", "Sebastian", "Shane", "Vincent",
                "Willy", "Wizard"
            };

            foreach (var npc in allNPCs.OrderBy(n => n))
            {
                bool hasHighFriendship = friendships.ContainsKey(npc) && friendships[npc].Points >= 2000;
                items.Add(new CategoryDetailsMenu.DetailItem
                {
                    Name = npc,
                    IsCompleted = hasHighFriendship
                });
            }

            return items;
        }

        private List<CategoryDetailsMenu.DetailItem> GetCropsDetails()
        {
            var items = new List<CategoryDetailsMenu.DetailItem>();
            
            // Placeholder - necesitarías implementar la lógica real de cultivos
            items.Add(new CategoryDetailsMenu.DetailItem { Name = "Parsnip", IsCompleted = true });
            items.Add(new CategoryDetailsMenu.DetailItem { Name = "Green Bean", IsCompleted = true });
            items.Add(new CategoryDetailsMenu.DetailItem { Name = "Cauliflower", IsCompleted = false });
            items.Add(new CategoryDetailsMenu.DetailItem { Name = "Potato", IsCompleted = true });
            items.Add(new CategoryDetailsMenu.DetailItem { Name = "Details in development", IsCompleted = false });

            return items;
        }

        private List<CategoryDetailsMenu.DetailItem> GetForageablesDetails()
        {
            var items = new List<CategoryDetailsMenu.DetailItem>();
            
            // Placeholder - necesitarías implementar la lógica real
            items.Add(new CategoryDetailsMenu.DetailItem { Name = "Wild Horseradish", IsCompleted = true });
            items.Add(new CategoryDetailsMenu.DetailItem { Name = "Daffodil", IsCompleted = true });
            items.Add(new CategoryDetailsMenu.DetailItem { Name = "Leek", IsCompleted = false });
            items.Add(new CategoryDetailsMenu.DetailItem { Name = "Details in development", IsCompleted = false });

            return items;
        }
    }
}
