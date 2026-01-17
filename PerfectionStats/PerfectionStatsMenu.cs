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

        // NEW: Fish data model - single source of truth
        private class FishData
        {
            public int TotalCount { get; set; }
            public int CaughtCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
        }

        // NEW: Cooking recipe data model - single source of truth
        private class CookingRecipeData
        {
            public int TotalCount { get; set; }
            public int CookedCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
        }

        // NEW: Crafting recipe data model - single source of truth
        private class CraftingRecipeData
        {
            public int TotalCount { get; set; }
            public int CraftedCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
        }

        // NEW: Museum item data model - single source of truth
        private class MuseumItemData
        {
            public int TotalCount { get; set; }
            public int DonatedCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
        }

        // NEW: Friendship data model - single source of truth
        private class FriendshipData
        {
            public int TotalCount { get; set; }
            public int BestFriendsCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
        }

        // NEW: Crops data model - single source of truth
        private class CropsData
        {
            public int TotalCount { get; set; }
            public int GrownCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
        }

        // NEW: Forageables data model - single source of truth
        private class ForageablesData
        {
            public int TotalCount { get; set; }
            public int FoundCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
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

            // Get fish data using single source of truth
            var fishData = GetFishData();

            // Get cooking recipe data using single source of truth
            var cookingData = GetCookingRecipeData();

            // Get crafting recipe data using single source of truth
            var craftingData = GetCraftingRecipeData();

            // Get museum item data using single source of truth
            var museumData = GetMuseumItemData();

            // Get friendship data using single source of truth
            var friendshipData = GetFriendshipData();

            // Get crops data using single source of truth
            var cropsData = GetCropsData();

            // Get forageables data using single source of truth
            var forageablesData = GetForageablesData();

            // ===== VANILLA STARDEW VALLEY =====
            categories.Add(new ProgressCategory { Name = "Fish Species", Completed = fishData.CaughtCount, Total = fishData.TotalCount });
            categories.Add(new ProgressCategory { Name = "Cooking Recipes", Completed = cookingData.CookedCount, Total = cookingData.TotalCount });
            categories.Add(new ProgressCategory { Name = "Crafting Recipes", Completed = craftingData.CraftedCount, Total = craftingData.TotalCount });
            categories.Add(new ProgressCategory { Name = "Museum Items", Completed = museumData.DonatedCount, Total = museumData.TotalCount });
            categories.Add(new ProgressCategory { Name = "Friendships (8+ Hearts)", Completed = friendshipData.BestFriendsCount, Total = friendshipData.TotalCount });
            categories.Add(new ProgressCategory { Name = "Crops Grown", Completed = cropsData.GrownCount, Total = cropsData.TotalCount });
            categories.Add(new ProgressCategory { Name = "Forageables", Completed = forageablesData.FoundCount, Total = forageablesData.TotalCount });

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
                categories.Add(new ProgressCategory { Name = "Rideside: NPCs Met", Completed = GetRidesideFriends(), Total = config.RidesideCategories.RidesideNPCsTotalCount });
                categories.Add(new ProgressCategory { Name = "Rideside: Items", Completed = GetRidesideItems(), Total = config.RidesideCategories.RidesideUniqueItemsTotalCount });
                categories.Add(new ProgressCategory { Name = "Rideside: Quests", Completed = GetRidesideQuests(), Total = config.RidesideCategories.RidesideQuestsTotalCount });
            }

            UpdateButtonPositions();
        }

        // NEW: Single source of truth for fish data
        private FishData GetFishData()
        {
            var caughtFishIds = new HashSet<int>(
                Game1.player.fishCaught?.Keys.Select(k => int.Parse(k)) ?? Enumerable.Empty<int>()
            );
            
            // Definitive list of all vanilla catchable fish
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

            // Calculate caught count
            int caughtCount = allFish.Keys.Count(fishId => caughtFishIds.Contains(fishId));

            // Build detail items list
            var detailItems = allFish
                .OrderBy(f => f.Value)
                .Select(fish => new CategoryDetailsMenu.DetailItem
                {
                    Name = fish.Value,
                    IsCompleted = caughtFishIds.Contains(fish.Key)
                })
                .ToList();

            return new FishData
            {
                TotalCount = allFish.Count,
                CaughtCount = caughtCount,
                DetailItems = detailItems
            };
        }

        // NEW: Single source of truth for cooking recipe data
        private CookingRecipeData GetCookingRecipeData()
        {
            var cookedRecipes = new HashSet<string>(
                Game1.player.cookingRecipes?.Keys ?? Enumerable.Empty<string>()
            );
            
            // Get all available cooking recipes from game data
            var allRecipes = CraftingRecipe.cookingRecipes;
            
            if (allRecipes == null || allRecipes.Count == 0)
            {
                // Fallback if data not loaded
                return new CookingRecipeData
                {
                    TotalCount = 0,
                    CookedCount = 0,
                    DetailItems = new List<CategoryDetailsMenu.DetailItem>()
                };
            }

            // Calculate cooked count
            int cookedCount = allRecipes.Keys.Count(recipeName => cookedRecipes.Contains(recipeName));

            // Build detail items list
            var detailItems = allRecipes.Keys
                .OrderBy(recipeName => recipeName)
                .Select(recipeName => new CategoryDetailsMenu.DetailItem
                {
                    Name = recipeName,
                    IsCompleted = cookedRecipes.Contains(recipeName)
                })
                .ToList();

            return new CookingRecipeData
            {
                TotalCount = allRecipes.Count,
                CookedCount = cookedCount,
                DetailItems = detailItems
            };
        }

        // NEW: Single source of truth for crafting recipe data
        private CraftingRecipeData GetCraftingRecipeData()
        {
            var craftedRecipes = new HashSet<string>(
                Game1.player.craftingRecipes?.Keys ?? Enumerable.Empty<string>()
            );
            
            // Get all available crafting recipes from game data
            var allRecipes = CraftingRecipe.craftingRecipes;
            
            if (allRecipes == null || allRecipes.Count == 0)
            {
                // Fallback if data not loaded
                return new CraftingRecipeData
                {
                    TotalCount = 0,
                    CraftedCount = 0,
                    DetailItems = new List<CategoryDetailsMenu.DetailItem>()
                };
            }

            // Calculate crafted count
            int craftedCount = allRecipes.Keys.Count(recipeName => craftedRecipes.Contains(recipeName));

            // Build detail items list
            var detailItems = allRecipes.Keys
                .OrderBy(recipeName => recipeName)
                .Select(recipeName => new CategoryDetailsMenu.DetailItem
                {
                    Name = recipeName,
                    IsCompleted = craftedRecipes.Contains(recipeName)
                })
                .ToList();

            return new CraftingRecipeData
            {
                TotalCount = allRecipes.Count,
                CraftedCount = craftedCount,
                DetailItems = detailItems
            };
        }

        // NEW: Single source of truth for museum item data
        private MuseumItemData GetMuseumItemData()
        {
            var allMuseumItems = new Dictionary<int, string>();
            var donatedItemIds = new HashSet<int>();
            
            try
            {
                // Get the museum location
                var museum = Game1.locations.OfType<StardewValley.Locations.LibraryMuseum>().FirstOrDefault();
                
                if (museum != null && museum.museumPieces != null)
                {
                    // Get donated item IDs from museum pieces
                    // museumPieces is NetStringDictionary<Vector2, string>
                    foreach (var position in museum.museumPieces.Keys)
                    {
                        string itemIdString = museum.museumPieces[position];
                        if (int.TryParse(itemIdString, out int itemId))
                        {
                            donatedItemIds.Add(itemId);
                        }
                    }
                    
                    ModEntry.Instance.Monitor.Log($"Found {donatedItemIds.Count} donated items", LogLevel.Debug);
                }
                else
                {
                    ModEntry.Instance.Monitor.Log("Museum location not found or has no pieces collection", LogLevel.Debug);
                }
                
                // Iterate through all possible item IDs to find artifacts and minerals
                // Vanilla Stardew Valley uses IDs 0-1000 for most items
                for (int itemId = 0; itemId < 1000; itemId++)
                {
                    try
                    {
                        // Create a temporary object using the correct constructor
                        // Object(string itemId, int initialStack, bool isRecipe, int price, int quality)
                        var obj = new StardewValley.Object(itemId.ToString(), 1, false, -1, 0);
                        
                        // Check if this is an artifact or mineral
                        // Use the Type property which returns the type string
                        if (obj.Type != null && (obj.Type.Equals("Arch") || obj.Type.Equals("Minerals")))
                        {
                            allMuseumItems[itemId] = obj.DisplayName;
                        }
                    }
                    catch
                    {
                        // Skip invalid item IDs
                        continue;
                    }
                }
                
                ModEntry.Instance.Monitor.Log($"Found {allMuseumItems.Count} donatable museum items", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Error loading museum items: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }

            // If we couldn't load any items, return empty data
            if (allMuseumItems.Count == 0)
            {
                ModEntry.Instance.Monitor.Log("No museum items found - returning empty data", LogLevel.Warn);
                return new MuseumItemData
                {
                    TotalCount = 0,
                    DonatedCount = 0,
                    DetailItems = new List<CategoryDetailsMenu.DetailItem>()
                };
            }

            // Calculate donated count
            int donatedCount = allMuseumItems.Keys.Count(itemId => donatedItemIds.Contains(itemId));

            // Build detail items list
            var detailItems = allMuseumItems
                .OrderBy(item => item.Value)
                .Select(item => new CategoryDetailsMenu.DetailItem
                {
                    Name = item.Value,
                    IsCompleted = donatedItemIds.Contains(item.Key)
                })
                .ToList();

            ModEntry.Instance.Monitor.Log($"Museum stats: {donatedCount}/{allMuseumItems.Count} items donated", LogLevel.Debug);

            return new MuseumItemData
            {
                TotalCount = allMuseumItems.Count,
                DonatedCount = donatedCount,
                DetailItems = detailItems
            };
        }

        // NEW: Single source of truth for friendship data
        private FriendshipData GetFriendshipData()
        {
            var playerFriendships = Game1.player.friendshipData;
            
            // Definitive list of all vanilla befriendable NPCs
            var allNPCs = new List<string>
            {
                "Abigail", "Alex", "Caroline", "Clint", "Demetrius", "Dwarf", "Elliott",
                "Emily", "Evelyn", "George", "Gus", "Haley", "Harvey", "Jas", "Jodi",
                "Kent", "Krobus", "Leah", "Lewis", "Linus", "Marnie", "Maru", "Pam",
                "Penny", "Pierre", "Robin", "Sam", "Sandy", "Sebastian", "Shane", "Vincent",
                "Willy", "Wizard"
            };

            // Calculate best friends count (8+ hearts = 2000+ points)
            int bestFriendsCount = 0;
            foreach (var npc in allNPCs)
            {
                if (playerFriendships.ContainsKey(npc) && playerFriendships[npc].Points >= 2000)
                {
                    bestFriendsCount++;
                }
            }

            // Build detail items list
            var detailItems = allNPCs
                .OrderBy(npc => npc)
                .Select(npc => new CategoryDetailsMenu.DetailItem
                {
                    Name = npc,
                    IsCompleted = playerFriendships.ContainsKey(npc) && playerFriendships[npc].Points >= 2000
                })
                .ToList();

            return new FriendshipData
            {
                TotalCount = allNPCs.Count,
                BestFriendsCount = bestFriendsCount,
                DetailItems = detailItems
            };
        }

        // NEW: Single source of truth for crops data
        private CropsData GetCropsData()
        {
            var shippedItems = Game1.player.basicShipped;
            
            // Definitive list of all vanilla crops tracked for perfection
            var allCrops = new Dictionary<int, string>
            {
                // Spring Crops
                {24, "Parsnip"}, {192, "Potato"}, {190, "Cauliflower"}, {188, "Green Bean"},
                {250, "Kale"}, {252, "Rhubarb"}, {273, "Rice"}, {256, "Tomato"},
                {248, "Garlic"}, {400, "Strawberry"}, {433, "Coffee Bean"},
                
                // Summer Crops
                {254, "Melon"}, {260, "Hot Pepper"}, {262, "Wheat"}, {264, "Radish"},
                {266, "Red Cabbage"}, {268, "Starfruit"}, {270, "Corn"}, {272, "Eggplant"},
                {274, "Artichoke"}, {276, "Pumpkin"}, {278, "Bok Choy"}, {280, "Yam"},
                {304, "Hops"}, {398, "Grape"}, {376, "Poppy"}, {591, "Tulip"},
                
                // Fall Crops
                {284, "Beet"}, {300, "Amaranth"}, {282, "Cranberries"}, {595, "Fairy Rose"},
                {593, "Summer Spangle"}, {597, "Blue Jazz"}, {421, "Sunflower"},
                
                // Special
                {454, "Ancient Fruit"}, {427, "Tulip Bulb"}, {830, "Taro Root"}
            };

            // Calculate grown count (items that have been shipped at least once)
            int grownCount = allCrops.Keys.Count(cropId => 
                shippedItems.ContainsKey(cropId.ToString()) && shippedItems[cropId.ToString()] > 0);

            // Build detail items list
            var detailItems = allCrops
                .OrderBy(crop => crop.Value)
                .Select(crop => new CategoryDetailsMenu.DetailItem
                {
                    Name = crop.Value,
                    IsCompleted = shippedItems.ContainsKey(crop.Key.ToString()) && shippedItems[crop.Key.ToString()] > 0
                })
                .ToList();

            return new CropsData
            {
                TotalCount = allCrops.Count,
                GrownCount = grownCount,
                DetailItems = detailItems
            };
        }

        // NEW: Single source of truth for forageables data
        private ForageablesData GetForageablesData()
        {
            var shippedItems = Game1.player.basicShipped;
            
            // Definitive list of all vanilla forageables tracked for perfection
            var allForageables = new Dictionary<int, string>
            {
                // Spring Forageables
                {16, "Wild Horseradish"}, {18, "Daffodil"}, {20, "Leek"}, {22, "Dandelion"},
                
                // Summer Forageables
                {396, "Spice Berry"}, {398, "Grape"}, {402, "Sweet Pea"},
                
                // Fall Forageables
                {281, "Chanterelle"}, {404, "Common Mushroom"}, {406, "Wild Plum"},
                {408, "Hazelnut"}, {410, "Blackberry"},
                
                // Winter Forageables
                {412, "Winter Root"}, {414, "Crystal Fruit"}, {416, "Snow Yam"},
                {418, "Crocus"},
                
                // Beach Forageables
                {372, "Clam"}, {718, "Cockle"}, {719, "Mussel"}, {723, "Oyster"},
                {393, "Coral"}, {397, "Sea Urchin"}, {394, "Rainbow Shell"}
            };

            // Calculate found count (items that have been shipped at least once)
            int foundCount = allForageables.Keys.Count(itemId => 
                shippedItems.ContainsKey(itemId.ToString()) && shippedItems[itemId.ToString()] > 0);

            // Build detail items list
            var detailItems = allForageables
                .OrderBy(item => item.Value)
                .Select(item => new CategoryDetailsMenu.DetailItem
                {
                    Name = item.Value,
                    IsCompleted = shippedItems.ContainsKey(item.Key.ToString()) && shippedItems[item.Key.ToString()] > 0
                })
                .ToList();

            return new ForageablesData
            {
                TotalCount = allForageables.Count,
                FoundCount = foundCount,
                DetailItems = detailItems
            };
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

            // Use single source of truth for fish data
            if (categoryName == "Fish Species")
            {
                var fishData = GetFishData();
                items = fishData.DetailItems;
            }
            else if (categoryName == "Cooking Recipes")
            {
                var cookingData = GetCookingRecipeData();
                items = cookingData.DetailItems;
            }
            else if (categoryName == "Crafting Recipes")
            {
                var craftingData = GetCraftingRecipeData();
                items = craftingData.DetailItems;
            }
            else if (categoryName == "Museum Items")
            {
                var museumData = GetMuseumItemData();
                items = museumData.DetailItems;
            }
            else if (categoryName == "Friendships (8+ Hearts)")
            {
                var friendshipData = GetFriendshipData();
                items = friendshipData.DetailItems;
            }
            else if (categoryName == "Crops Grown")
            {
                var cropsData = GetCropsData();
                items = cropsData.DetailItems;
            }
            else if (categoryName == "Forageables")
            {
                var forageablesData = GetForageablesData();
                items = forageablesData.DetailItems;
            }
            // Add other categories as needed...

            Game1.activeClickableMenu = new CategoryDetailsMenu(categoryName, items);
        }

        // REMOVED: GetFishDetails() - replaced by GetFishData()
        // REMOVED: GetCookingRecipeDetails() - replaced by GetCookingRecipeData()
        // REMOVED: GetCraftingRecipeDetails() - replaced by GetCraftingRecipeData()
        // REMOVED: GetMuseumItemDetails() - replaced by GetMuseumItemData()
        // REMOVED: GetBestFriends() - replaced by GetFriendshipData()
        // REMOVED: GetFriendshipDetails() - replaced by GetFriendshipData()
        // REMOVED: GetCropsDetails() - replaced by GetCropsData()
        // REMOVED: GetForageablesDetails() - replaced by GetForageablesData()
    }
}
