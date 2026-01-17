namespace PerfectionStats
{
    /// <summary>
    /// Centralized user-facing strings for the mod.
    /// All text shown to players should be defined here.
    /// Future: This will be replaced with a proper localization system.
    /// </summary>
    internal static class UIStrings
    {
        // Main menu - vanilla style title
        public const string MenuTitle = "Perfection Stats";
        public const string OverallPerfectionLabel = "PERFECTION";

        // Vanilla categories - EXACT standard labels
        public const string FishSpecies = "Fish Species";
        public const string CookingRecipes = "Cooking Recipes";
        public const string CraftingRecipes = "Crafting Recipes";
        public const string MuseumItems = "Museum Items";
        public const string Friendships = "Friendship (8+ Hearts)";  // Singular per standard
        public const string CropsGrown = "Crops Grown";
        public const string Forageables = "Forageables";

        // SVE categories
        public const string SVEFishSpecies = "SVE: Fish Species";
        public const string SVENPCsBefriended = "SVE: NPCs Befriended";
        public const string SVEArtifacts = "SVE: Artifacts";
        public const string SVECrops = "SVE: Crops";

        // Ridgeside Village categories
        public const string RidesideNPCsMet = "Rideside: NPCs Met";
        public const string RidesideItems = "Rideside: Items";
        public const string RidesideQuests = "Rideside: Quests";

        // Subtitle formats for mod detection
        public const string SubtitleSVEAndRideside = "(SVE + Rideside)";
        public const string SubtitleSVE = "(SVE)";
        public const string SubtitleRideside = "(Rideside)";

        // Detail view and UI elements
        public const string CompletedLabel = "Completed";
        public const string MissingLabel = "Missing";
        public const string DetailsLabel = "Details";
        public const string CloseLabel = "Close";

        // Format strings (for future localization)
        public static string FormatCompletionStatus(int completed, int total)
        {
            return $"{completed} / {total} {CompletedLabel}";
        }
    }
}
