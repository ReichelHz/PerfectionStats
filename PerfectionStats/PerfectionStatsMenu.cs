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
            
            var allFish = new Dictionary<int, string>();
            
            try
            {
                // Dynamically discover all fish from game data
                // This automatically includes fish from content mods
                for (int itemId = 0; itemId < 2000; itemId++)
                {
                    try
                    {
                        // Create a temporary object to check its properties
                        var obj = new StardewValley.Object(itemId.ToString(), 1, false, -1, 0);
                        
                        // Check if this is a fish using the Type property
                        // This works for vanilla and modded fish
                        if (obj.Type != null && obj.Type.Equals("Fish"))
                        {
                            allFish[itemId] = obj.DisplayName;
                        }
                    }
                    catch
                    {
                        // Skip invalid item IDs
                        continue;
                    }
                }
                
                ModEntry.Instance.Monitor.Log($"Found {allFish.Count} catchable fish (vanilla + mods)", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Error loading fish data: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }

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

            ModEntry.Instance.Monitor.Log($"Fish stats: {caughtCount}/{allFish.Count} fish caught", LogLevel.Debug);

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
            
            var allRecipes = new Dictionary<string, string>();
            
            try
            {
                // Dynamically load all cooking recipes from game data
                // This includes recipes added by content mods
                var recipeData = CraftingRecipe.cookingRecipes;
                
                if (recipeData != null && recipeData.Count > 0)
                {
                    // Add all recipes to our dictionary
                    foreach (var recipeName in recipeData.Keys)
                    {
                        // Use recipe name as both key and display name
                        // The game handles localization through the CraftingRecipe class
                        allRecipes[recipeName] = recipeName;
                    }
                }
                
                // If the static dictionary is empty, try creating CraftingRecipe objects
                // to ensure we capture all recipes including those added dynamically
                if (allRecipes.Count == 0)
                {
                    ModEntry.Instance.Monitor.Log("cookingRecipes dictionary is empty, using fallback detection", LogLevel.Debug);
                    
                    // Try to detect recipes by checking if they can be instantiated
                    // This is a fallback for edge cases where the dictionary isn't populated
                    var knownRecipes = new List<string>
                    {
                        "Fried Egg", "Omelet", "Pancakes", "Bread", "Tortilla", "Pizza",
                        "Maki Roll", "Salmon Dinner", "Fish Taco", "Fried Calamari",
                        // Add more as fallback, but this shouldn't normally be needed
                    };
                    
                    foreach (var recipeName in knownRecipes)
                    {
                        try
                        {
                            var recipe = new CraftingRecipe(recipeName, true);
                            if (recipe != null)
                            {
                                allRecipes[recipeName] = recipe.DisplayName;
                            }
                        }
                        catch
                        {
                            // Recipe doesn't exist, skip it
                            continue;
                        }
                    }
                }
                
                ModEntry.Instance.Monitor.Log($"Found {allRecipes.Count} cooking recipes (vanilla + mods)", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Error loading cooking recipes: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }

            // If we still couldn't load any recipes, return empty data
            if (allRecipes.Count == 0)
            {
                ModEntry.Instance.Monitor.Log("No cooking recipes found - returning empty data", LogLevel.Warn);
                return new CookingRecipeData
                {
                    TotalCount = 0,
                    CookedCount = 0,
                    DetailItems = new List<CategoryDetailsMenu.DetailItem>()
                };
            }

            // Calculate cooked count
            int cookedCount = allRecipes.Keys.Count(recipeName => cookedRecipes.Contains(recipeName));

            // Build detail items list using display names
            var detailItems = allRecipes
                .OrderBy(r => r.Value)
                .Select(recipe => new CategoryDetailsMenu.DetailItem
                {
                    Name = recipe.Value,
                    IsCompleted = cookedRecipes.Contains(recipe.Key)
                })
                .ToList();

            ModEntry.Instance.Monitor.Log($"Cooking stats: {cookedCount}/{allRecipes.Count} recipes cooked", LogLevel.Debug);

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
            
            var allRecipes = new Dictionary<string, string>();
            
            try
            {
                // Dynamically load all crafting recipes from game data
                // This includes recipes added by content mods
                var recipeData = CraftingRecipe.craftingRecipes;
                
                if (recipeData != null && recipeData.Count > 0)
                {
                    // Create CraftingRecipe objects to get proper display names
                    // This ensures modded recipes show their correct localized names
                    foreach (var recipeName in recipeData.Keys)
                    {
                        try
                        {
                            var recipe = new CraftingRecipe(recipeName, false);
                            if (recipe != null && !string.IsNullOrEmpty(recipe.DisplayName))
                            {
                                // Use internal name as key, display name as value
                                allRecipes[recipeName] = recipe.DisplayName;
                            }
                            else
                            {
                                // Fallback to internal name if display name is unavailable
                                allRecipes[recipeName] = recipeName;
                            }
                        }
                        catch
                        {
                            // If recipe instantiation fails, use internal name
                            allRecipes[recipeName] = recipeName;
                        }
                    }
                }
                
                ModEntry.Instance.Monitor.Log($"Found {allRecipes.Count} crafting recipes (vanilla + mods)", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Error loading crafting recipes: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }

            // If we couldn't load any recipes, return empty data
            if (allRecipes.Count == 0)
            {
                ModEntry.Instance.Monitor.Log("No crafting recipes found - returning empty data", LogLevel.Warn);
                return new CraftingRecipeData
                {
                    TotalCount = 0,
                    CraftedCount = 0,
                    DetailItems = new List<CategoryDetailsMenu.DetailItem>()
                };
            }

            // Calculate crafted count - compare against player's crafted recipes
            int craftedCount = allRecipes.Keys.Count(recipeName => craftedRecipes.Contains(recipeName));

            // Build detail items list using display names
            var detailItems = allRecipes
                .OrderBy(r => r.Value)
                .Select(recipe => new CategoryDetailsMenu.DetailItem
                {
                    Name = recipe.Value,
                    IsCompleted = craftedRecipes.Contains(recipe.Key)
                })
                .ToList();

            ModEntry.Instance.Monitor.Log($"Crafting stats: {craftedCount}/{allRecipes.Count} recipes crafted", LogLevel.Debug);

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
                
                // Dynamically discover all museum items from game data
                // This automatically includes items from content mods
                for (int itemId = 0; itemId < 2000; itemId++)
                {
                    try
                    {
                        // Create a temporary object to check its properties
                        var obj = new StardewValley.Object(itemId.ToString(), 1, false, -1, 0);
                        
                        // Check if this is an artifact or mineral using the Type property
                        // This works for vanilla and modded museum items
                        if (obj.Type != null && (obj.Type.Equals("Arch") || obj.Type.Equals("Minerals")))
                        {
                            // Verify it has a valid display name
                            if (!string.IsNullOrEmpty(obj.DisplayName))
                            {
                                allMuseumItems[itemId] = obj.DisplayName;
                            }
                        }
                    }
                    catch
                    {
                        // Skip invalid item IDs
                        continue;
                    }
                }
                
                ModEntry.Instance.Monitor.Log($"Found {allMuseumItems.Count} donatable museum items (vanilla + mods)", LogLevel.Debug);
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
            var allNPCs = new Dictionary<string, string>();
            
            try
            {
                // Dynamically discover all befriendable NPCs from game data
                // This includes NPCs added by content mods
                
                // Iterate through all locations to find NPCs
                foreach (var location in Game1.locations)
                {
                    if (location?.characters == null) continue;
                    
                    foreach (var character in location.characters)
                    {
                        if (character != null && !string.IsNullOrEmpty(character.Name))
                        {
                            // Check if this NPC can be befriended using CanSocialize
                            // This property filters out non-befriendable characters
                            if (character.CanSocialize && !allNPCs.ContainsKey(character.Name))
                            {
                                // Use display name for proper localization and mod support
                                allNPCs[character.Name] = character.displayName ?? character.Name;
                            }
                        }
                    }
                }
                
                // Also check for NPCs in the player's friendship data
                // This catches NPCs that might not be on a location currently
                if (playerFriendships != null)
                {
                    foreach (var friendshipKey in playerFriendships.Keys)
                    {
                        if (!allNPCs.ContainsKey(friendshipKey))
                        {
                            // Try to get the NPC to verify they're befriendable
                            var npc = Game1.getCharacterFromName(friendshipKey);
                            if (npc != null && npc.CanSocialize)
                            {
                                allNPCs[friendshipKey] = npc.displayName ?? friendshipKey;
                            }
                        }
                    }
                }
                
                ModEntry.Instance.Monitor.Log($"Found {allNPCs.Count} befriendable NPCs (vanilla + mods)", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Error loading NPC data: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }

            // If no NPCs found, return empty data
            if (allNPCs.Count == 0)
            {
                ModEntry.Instance.Monitor.Log("No befriendable NPCs found - returning empty data", LogLevel.Warn);
                return new FriendshipData
                {
                    TotalCount = 0,
                    BestFriendsCount = 0,
                    DetailItems = new List<CategoryDetailsMenu.DetailItem>()
                };
            }

            // Calculate best friends count (8+ hearts = 2000+ points)
            int bestFriendsCount = 0;
            foreach (var npcName in allNPCs.Keys)
            {
                if (playerFriendships.ContainsKey(npcName) && playerFriendships[npcName].Points >= 2000)
                {
                    bestFriendsCount++;
                }
            }

            // Build detail items list using display names
            var detailItems = allNPCs
                .OrderBy(npc => npc.Value)
                .Select(npc => new CategoryDetailsMenu.DetailItem
                {
                    Name = npc.Value,
                    IsCompleted = playerFriendships.ContainsKey(npc.Key) && playerFriendships[npc.Key].Points >= 2000
                })
                .ToList();

            ModEntry.Instance.Monitor.Log($"Friendship stats: {bestFriendsCount}/{allNPCs.Count} NPCs at 8+ hearts", LogLevel.Debug);

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
            var allCrops = new Dictionary<int, string>();
            
            try
            {
                // Dynamically discover all crops from game data
                // This automatically includes crops from content mods
                for (int itemId = 0; itemId < 2000; itemId++)
                {
                    try
                    {
                        // Create a temporary object to check its properties
                        var obj = new StardewValley.Object(itemId.ToString(), 1, false, -1, 0);
                        
                        // Check if this is a crop using the Category property
                        // Crops typically have categories: -75 (Vegetable), -79 (Fruit), -80 (Flower)
                        // Also check for other crop-related categories
                        if (obj.Category != null)
                        {
                            int category = obj.Category;
                            
                            // Include vegetables, fruits, and flowers (the main crop categories)
                            if (category == -75 || category == -79 || category == -80)
                            {
                                // Verify it can actually be shipped (has a price and isn't a special item)
                                if (obj.Price > 0 && !string.IsNullOrEmpty(obj.DisplayName))
                                {
                                    allCrops[itemId] = obj.DisplayName;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Skip invalid item IDs
                        continue;
                    }
                }
                
                ModEntry.Instance.Monitor.Log($"Found {allCrops.Count} growable crops (vanilla + mods)", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Error loading crops data: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }

            // If no crops found, return empty data
            if (allCrops.Count == 0)
            {
                ModEntry.Instance.Monitor.Log("No crops found - returning empty data", LogLevel.Warn);
                return new CropsData
                {
                    TotalCount = 0,
                    GrownCount = 0,
                    DetailItems = new List<CategoryDetailsMenu.DetailItem>()
                };
            }

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

            ModEntry.Instance.Monitor.Log($"Crops stats: {grownCount}/{allCrops.Count} crops grown", LogLevel.Debug);

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
            var allForageables = new Dictionary<int, string>();
            
            try
            {
                // Dynamically discover all forageables from game data
                // This automatically includes forageables from content mods
                for (int itemId = 0; itemId < 2000; itemId++)
                {
                    try
                    {
                        // Create a temporary object to check its properties
                        var obj = new StardewValley.Object(itemId.ToString(), 1, false, -1, 0);
                        
                        // Check if this is a forageable using the Category property
                        // Forageables have category -81 (Forage)
                        if (obj.Category == -81)
                        {
                            // Verify it can actually be shipped (has a price and isn't a special item)
                            if (obj.Price > 0 && !string.IsNullOrEmpty(obj.DisplayName))
                            {
                                allForageables[itemId] = obj.DisplayName;
                            }
                        }
                    }
                    catch
                    {
                        // Skip invalid item IDs
                        continue;
                    }
                }
                
                ModEntry.Instance.Monitor.Log($"Found {allForageables.Count} forageable items (vanilla + mods)", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Error loading forageables data: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }

            // If no forageables found, return empty data
            if (allForageables.Count == 0)
            {
                ModEntry.Instance.Monitor.Log("No forageables found - returning empty data", LogLevel.Warn);
                return new ForageablesData
                {
                    TotalCount = 0,
                    FoundCount = 0,
                    DetailItems = new List<CategoryDetailsMenu.DetailItem>()
                };
            }

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

            ModEntry.Instance.Monitor.Log($"Forageables stats: {foundCount}/{allForageables.Count} items found", LogLevel.Debug);

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
