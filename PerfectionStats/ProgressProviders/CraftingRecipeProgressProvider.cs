using StardewValley;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerfectionStats.ProgressProviders
{
    internal class CraftingRecipeProgressProvider
    {
        public class CraftingRecipeProgressData
        {
            public int TotalCount { get; set; }
            public int CraftedCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
        }

        public CraftingRecipeProgressData GetProgress()
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
                    // Use internal recipe names (English) instead of localized DisplayName
                    // This ensures detail lists always show English names
                    foreach (var recipeName in recipeData.Keys)
                    {
                        // Use internal name as both key and display value
                        // Recipe keys are always in English
                        allRecipes[recipeName] = recipeName;
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
                return new CraftingRecipeProgressData
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

            return new CraftingRecipeProgressData
            {
                TotalCount = allRecipes.Count,
                CraftedCount = craftedCount,
                DetailItems = detailItems
            };
        }
    }
}
