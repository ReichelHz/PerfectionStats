using StardewValley;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerfectionStats.ProgressProviders
{
    internal class CropsProgressProvider
    {
        public class CropsProgressData
        {
            public int TotalCount { get; set; }
            public int GrownCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
        }

        public CropsProgressData GetProgress()
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
                                if (obj.Price > 0)
                                {
                                    // Get English name from object data, not localized DisplayName
                                    string englishName = GetEnglishObjectName(itemId, obj);
                                    if (!string.IsNullOrEmpty(englishName))
                                    {
                                        allCrops[itemId] = englishName;
                                    }
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
                return new CropsProgressData
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

            return new CropsProgressData
            {
                TotalCount = allCrops.Count,
                GrownCount = grownCount,
                DetailItems = detailItems
            };
        }

        /// <summary>
        /// Gets the English (non-localized) name for an object.
        /// This ensures detail lists always show English names regardless of game language.
        /// </summary>
        private string GetEnglishObjectName(int itemId, StardewValley.Object obj)
        {
            try
            {
                // In Stardew Valley, object data is stored with the Name property being internal/English
                // while DisplayName is localized. We use Name instead of DisplayName.
                if (!string.IsNullOrEmpty(obj.Name))
                {
                    return obj.Name;
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Error getting English name for item {itemId}: {ex.Message}", LogLevel.Trace);
            }

            // Last resort fallback: use item ID
            return $"Item_{itemId}";
        }
    }
}
