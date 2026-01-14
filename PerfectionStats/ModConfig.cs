namespace PerfectionStats
{
    public class ModConfig
    {
        public VanillaConfig VanillaCategories { get; set; }
        public SVEConfig SVECategories { get; set; }
        public RidesideConfig RidesideCategories { get; set; }

        public ModConfig()
        {
            VanillaCategories = new VanillaConfig();
            SVECategories = new SVEConfig();
            RidesideCategories = new RidesideConfig();
        }
    }

    public class VanillaConfig
    {
        public int FishSpeciesTotalCount { get; set; } = 62;
        public int CookingRecipesTotalCount { get; set; } = 82;
        public int CraftingRecipesTotalCount { get; set; } = 104;
        public int MuseumItemsTotalCount { get; set; } = 95;
        public int FriendshipsTotalCount { get; set; } = 25;
        public int CropsGrownTotalCount { get; set; } = 26;
        public int ForageablesFoundTotalCount { get; set; } = 20;
    }

    public class SVEConfig
    {
        public int SVEFishSpeciesTotalCount { get; set; } = 25;
        public int SVENPCsTotalCount { get; set; } = 8;
        public int SVEArtifactsTotalCount { get; set; } = 12;
        public int SVECropsTotalCount { get; set; } = 15;
    }

    public class RidesideConfig
    {
        public int RidesideNPCsTotalCount { get; set; } = 12;
        public int RidesideUniqueItemsTotalCount { get; set; } = 18;
        public int RidesideQuestsTotalCount { get; set; } = 20;
    }
}
