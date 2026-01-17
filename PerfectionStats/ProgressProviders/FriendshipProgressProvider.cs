using StardewValley;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerfectionStats.ProgressProviders
{
    internal class FriendshipProgressProvider
    {
        public class FriendshipProgressData
        {
            public int TotalCount { get; set; }
            public int BestFriendsCount { get; set; }
            public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
        }

        public FriendshipProgressData GetProgress()
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
                                // Use internal Name (English) instead of displayName (localized)
                                // This ensures detail lists always show English NPC names
                                allNPCs[character.Name] = character.Name;
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
                                // Use internal Name (English) instead of displayName (localized)
                                allNPCs[friendshipKey] = npc.Name;
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
                return new FriendshipProgressData
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

            // Build detail items list using internal English names
            var detailItems = allNPCs
                .OrderBy(npc => npc.Value)
                .Select(npc => new CategoryDetailsMenu.DetailItem
                {
                    Name = npc.Value,
                    IsCompleted = playerFriendships.ContainsKey(npc.Key) && playerFriendships[npc.Key].Points >= 2000
                })
                .ToList();

            ModEntry.Instance.Monitor.Log($"Friendship stats: {bestFriendsCount}/{allNPCs.Count} NPCs at 8+ hearts", LogLevel.Debug);

            return new FriendshipProgressData
            {
                TotalCount = allNPCs.Count,
                BestFriendsCount = bestFriendsCount,
                DetailItems = detailItems
            };
        }
    }
}
