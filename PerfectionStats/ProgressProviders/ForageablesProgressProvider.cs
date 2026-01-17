using StardewValley;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerfectionStats.ProgressProviders
{
    internal class ForageablesProgressProvider
    {
        public class ForageablesProgressData
        {
            public int TotalCount { get; set; }
            public int FoundCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
        }

        public ForageablesProgressData GetProgress()
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
                            if (obj.Price > 0)
                            {
                                // Get English name from object data, not localized DisplayName
                                string englishName = GetEnglishObjectName(itemId, obj);
                                if (!string.IsNullOrEmpty(englishName))
                                {
                                    allForageables[itemId] = englishName;
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
                return new ForageablesProgressData
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

            return new ForageablesProgressData
            {
                TotalCount = allForageables.Count,
                FoundCount = foundCount,
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
