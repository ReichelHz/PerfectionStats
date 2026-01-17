using StardewValley;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerfectionStats.ProgressProviders
{
    internal class FishProgressProvider
    {
        public class FishProgressData
        {
            public int TotalCount { get; set; }
            public int CaughtCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
        }

        public FishProgressData GetProgress()
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
                            // Get English name from object data, not localized DisplayName
                            string englishName = GetEnglishObjectName(itemId, obj);
                            allFish[itemId] = englishName;
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

            return new FishProgressData
            {
                TotalCount = allFish.Count,
                CaughtCount = caughtCount,
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
