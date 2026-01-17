using StardewValley;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerfectionStats.ProgressProviders
{
    internal class CookingRecipeProgressProvider
    {
        public class CookingRecipeProgressData
        {
            public int TotalCount { get; set; }
            public int CookedCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
        }

        public CookingRecipeProgressData GetProgress()
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
                                // Use internal recipe name (English) instead of DisplayName (localized)
                                allRecipes[recipeName] = recipeName;
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
                return new CookingRecipeProgressData
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

            return new CookingRecipeProgressData
            {
                TotalCount = allRecipes.Count,
                CookedCount = cookedCount,
                DetailItems = detailItems
            };
        }
    }
}
