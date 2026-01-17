using StardewValley;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerfectionStats.ProgressProviders
{
    internal class MuseumItemProgressProvider
    {
        public class MuseumItemProgressData
        {
            public int TotalCount { get; set; }
            public int DonatedCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
        }

        public MuseumItemProgressData GetProgress()
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
                            // Get English name from object data, not localized DisplayName
                            string englishName = GetEnglishObjectName(itemId, obj);
                            if (!string.IsNullOrEmpty(englishName))
                            {
                                allMuseumItems[itemId] = englishName;
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
                return new MuseumItemProgressData
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

            return new MuseumItemProgressData
            {
                TotalCount = allMuseumItems.Count,
                DonatedCount = donatedCount,
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
