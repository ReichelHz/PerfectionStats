# Fish Data Refactoring - Single Source of Truth

## Problem
The progress bar and detail view were using different data sources:
- **Progress bar**: Used `GetFishCaught()` which counted ALL fish in `player.fishCaught` dictionary and compared to `config.FishSpeciesTotalCount` (62)
- **Detail view**: Used `GetFishDetails()` with a hardcoded list of 59 fish

This caused inconsistent totals: **62 in the bar vs 59 in details**.

## Solution
Created a **single source of truth** method that provides all fish data consistently.

### New Data Model
```csharp
private class FishData
{
    public int TotalCount { get; set; }
    public int CaughtCount { get; set; }
    public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
}
```

### Single Method: `GetFishData()`
```csharp
private FishData GetFishData()
{
    // 1. Get player's caught fish IDs
    var caughtFishIds = new HashSet<int>(
        Game1.player.fishCaught?.Keys.Select(k => int.Parse(k)) ?? Enumerable.Empty<int>()
    );
    
    // 2. Define ALL vanilla catchable fish (definitive list of 59)
    var allFish = new Dictionary<int, string>
    {
        {128, "Pufferfish"}, {129, "Anchovy"}, /* ... */
    };

    // 3. Calculate caught count (only fish in our list)
    int caughtCount = allFish.Keys.Count(fishId => caughtFishIds.Contains(fishId));

    // 4. Build detail items
    var detailItems = allFish
        .OrderBy(f => f.Value)
        .Select(fish => new CategoryDetailsMenu.DetailItem
        {
            Name = fish.Value,
            IsCompleted = caughtFishIds.Contains(fish.Key)
        })
        .ToList();

    // 5. Return all data together
    return new FishData
    {
        TotalCount = allFish.Count,      // 59
        CaughtCount = caughtCount,        // X/59
        DetailItems = detailItems         // For detail view
    };
}
```

### Usage in Progress Bar
```csharp
private void InitializeCategories()
{
    var fishData = GetFishData();
    categories.Add(new ProgressCategory { 
        Name = "Fish Species", 
        Completed = fishData.CaughtCount,  // X
        Total = fishData.TotalCount        // 59
    });
}
```

### Usage in Detail View
```csharp
private void OpenCategoryDetails(string categoryName)
{
    if (categoryName == "Fish Species")
    {
        var fishData = GetFishData();
        items = fishData.DetailItems;  // Use same data
    }
    // ...
}
```

## Key Benefits
✅ **Consistency**: Both views now show the same total (59)  
✅ **Single source**: The fish list is defined in ONE place  
✅ **No duplication**: Removed redundant `GetFishCaught()` and `GetFishDetails()`  
✅ **Maintainability**: Adding a fish? Update ONE dictionary  
✅ **Accuracy**: Only counts fish that are in the official list  

## Removed Methods
- ❌ `GetFishCaught()` - replaced by `GetFishData().CaughtCount`
- ❌ `GetFishDetails()` - replaced by `GetFishData().DetailItems`

---

# Cooking Recipes Refactoring - Single Source of Truth

## Problem
The progress bar and detail view were using different data sources:
- **Progress bar**: Used `farmer.recipesCooked.Count()` and `config.VanillaCategories.CookingRecipesTotalCount`
- **Detail view**: Used `GetCookingRecipeDetails()` with a hardcoded list of recipes

This caused inconsistent totals and maintenance issues.

## Solution
Created a **single source of truth** method that retrieves cooking recipes from game data.

### New Data Model
```csharp
private class CookingRecipeData
{
    public int TotalCount { get; set; }
    public int CookedCount { get; set; }
    public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
}
```

### Single Method: `GetCookingRecipeData()`
```csharp
private CookingRecipeData GetCookingRecipeData()
{
    // 1. Get player's cooked recipes
    var cookedRecipes = new HashSet<string>(
        Game1.player.cookingRecipes?.Keys ?? Enumerable.Empty<string>()
    );
    
    // 2. Get all available cooking recipes from game data (NOT hardcoded)
    var allRecipes = CraftingRecipe.cookingRecipes;

    // 3. Calculate cooked count
    int cookedCount = allRecipes.Keys.Count(recipeName => cookedRecipes.Contains(recipeName));

    // 4. Build detail items
    var detailItems = allRecipes.Keys
        .OrderBy(recipeName => recipeName)
        .Select(recipeName => new CategoryDetailsMenu.DetailItem
        {
            Name = recipeName,
            IsCompleted = cookedRecipes.Contains(recipeName)
        })
        .ToList();

    // 5. Return all data together
    return new CookingRecipeData
    {
        TotalCount = allRecipes.Count,
        CookedCount = cookedCount,
        DetailItems = detailItems
    };
}
```

### Usage in Progress Bar
```csharp
private void InitializeCategories()
{
    var cookingData = GetCookingRecipeData();
    categories.Add(new ProgressCategory { 
        Name = "Cooking Recipes", 
        Completed = cookingData.CookedCount,
        Total = cookingData.TotalCount
    });
}
```

### Usage in Detail View
```csharp
private void OpenCategoryDetails(string categoryName)
{
    if (categoryName == "Cooking Recipes")
    {
        var cookingData = GetCookingRecipeData();
        items = cookingData.DetailItems;
    }
    // ...
}
```

## Key Benefits
✅ **Consistency**: Both views now show the same total  
✅ **Dynamic data**: Uses `CraftingRecipe.cookingRecipes` from game data  
✅ **No hardcoding**: Recipe list comes from the game itself  
✅ **No config dependency**: No longer needs `config.VanillaCategories.CookingRecipesTotalCount`  
✅ **Automatic updates**: Works with game updates and content packs  

## Removed Methods
- ❌ `GetCookingRecipeDetails()` - replaced by `GetCookingRecipeData().DetailItems`
- ❌ Removed dependency on `config.VanillaCategories.CookingRecipesTotalCount`

---

# Crafting Recipes Refactoring - Single Source of Truth

## Problem
The progress bar and detail view were using different data sources:
- **Progress bar**: Used `farmer.craftingRecipes.Values.Count()` and `config.VanillaCategories.CraftingRecipesTotalCount`
- **Detail view**: Used `GetCraftingRecipeDetails()` with a hardcoded list of recipes

This caused inconsistent totals and maintenance issues.

## Solution
Created a **single source of truth** method that retrieves crafting recipes from game data.

### New Data Model
```csharp
private class CraftingRecipeData
{
    public int TotalCount { get; set; }
    public int CraftedCount { get; set; }
    public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
}
```

### Single Method: `GetCraftingRecipeData()`
```csharp
private CraftingRecipeData GetCraftingRecipeData()
{
    // 1. Get player's crafted recipes
    var craftedRecipes = new HashSet<string>(
        Game1.player.craftingRecipes?.Keys ?? Enumerable.Empty<string>()
    );
    
    // 2. Get all available crafting recipes from game data (NOT hardcoded)
    var allRecipes = CraftingRecipe.craftingRecipes;

    // 3. Calculate crafted count
    int craftedCount = allRecipes.Keys.Count(recipeName => craftedRecipes.Contains(recipeName));

    // 4. Build detail items
    var detailItems = allRecipes.Keys
        .OrderBy(recipeName => recipeName)
        .Select(recipeName => new CategoryDetailsMenu.DetailItem
        {
            Name = recipeName,
            IsCompleted = craftedRecipes.Contains(recipeName)
        })
        .ToList();

    // 5. Return all data together
    return new CraftingRecipeData
    {
        TotalCount = allRecipes.Count,
        CraftedCount = craftedCount,
        DetailItems = detailItems
    };
}
```

### Usage in Progress Bar
```csharp
private void InitializeCategories()
{
    var craftingData = GetCraftingRecipeData();
    categories.Add(new ProgressCategory { 
        Name = "Crafting Recipes", 
        Completed = craftingData.CraftedCount,
        Total = craftingData.TotalCount
    });
}
```

### Usage in Detail View
```csharp
private void OpenCategoryDetails(string categoryName)
{
    if (categoryName == "Crafting Recipes")
    {
        var craftingData = GetCraftingRecipeData();
        items = craftingData.DetailItems;
    }
    // ...
}
```

## Key Benefits
✅ **Consistency**: Both views now show the same total  
✅ **Dynamic data**: Uses `CraftingRecipe.craftingRecipes` from game data  
✅ **No hardcoding**: Recipe list comes from the game itself  
✅ **No config dependency**: No longer needs `config.VanillaCategories.CraftingRecipesTotalCount`  
✅ **Automatic updates**: Works with game updates and content packs  

## Removed Methods
- ❌ `GetCraftingRecipeDetails()` - replaced by `GetCraftingRecipeData().DetailItems`
- ❌ Removed dependency on `config.VanillaCategories.CraftingRecipesTotalCount`

---

# Museum Items Refactoring - Single Source of Truth

## Problem
The progress bar and detail view were using different data sources:
- **Progress bar**: Used `GetMuseumItems()` which counted `museum.museumPieces.Count()` and compared to `config.VanillaCategories.MuseumItemsTotalCount`
- **Detail view**: Used `GetMuseumItemDetails()` with a hardcoded dictionary of only ~25 items

This caused inconsistent totals and incomplete information.

## Solution
Created a **single source of truth** method that retrieves museum items from game data.

### New Data Model
```csharp
private class MuseumItemData
{
    public int TotalCount { get; set; }
    public int DonatedCount { get; set; }
    public List<CategoryDetailsMenu.DetailItem> DetailItems { get; set; }
}
```

### Single Method: `GetMuseumItemData()`
```csharp
private MuseumItemData GetMuseumItemData()
{
    // 1. Get museum location and donated items
    var museum = Game1.locations.OfType<StardewValley.Locations.LibraryMuseum>().FirstOrDefault();
    var donatedItemIds = new HashSet<int>(
        museum.museumPieces.Values.Select(v => int.Parse(v))
    );
    
    // 2. Load all artifacts and minerals from game data (NOT hardcoded)
    var objectData = Game1.content.Load<Dictionary<int, string>>("Data\\ObjectInformation");
    var allMuseumItems = new Dictionary<int, string>();
    
    foreach (var kvp in objectData)
    {
        var data = kvp.Value.Split('/');
        if (data.Length > 3)
        {
            string type = data[3];
            string name = data[0];
            
            // Include Artifacts and Minerals
            if (type.Contains("Arch") || type.Contains("Minerals"))
            {
                allMuseumItems[kvp.Key] = name;
            }
        }
    }

    // 3. Calculate donated count
    int donatedCount = allMuseumItems.Keys.Count(itemId => donatedItemIds.Contains(itemId));

    // 4. Build detail items
    var detailItems = allMuseumItems
        .OrderBy(item => item.Value)
        .Select(item => new CategoryDetailsMenu.DetailItem
        {
            Name = item.Value,
            IsCompleted = donatedItemIds.Contains(item.Key)
        })
        .ToList();

    // 5. Return all data together
    return new MuseumItemData
    {
        TotalCount = allMuseumItems.Count,
        DonatedCount = donatedCount,
        DetailItems = detailItems
    };
}
```

### Usage in Progress Bar
```csharp
private void InitializeCategories()
{
    var museumData = GetMuseumItemData();
    categories.Add(new ProgressCategory { 
        Name = "Museum Items", 
        Completed = museumData.DonatedCount,
        Total = museumData.TotalCount
    });
}
```

### Usage in Detail View
```csharp
private void OpenCategoryDetails(string categoryName)
{
    if (categoryName == "Museum Items")
    {
        var museumData = GetMuseumItemData();
        items = museumData.DetailItems;
    }
    // ...
}
```

## Key Benefits
✅ **Consistency**: Both views now show the same total  
✅ **Dynamic data**: Uses `Data\ObjectInformation` from game content  
✅ **Complete list**: Shows ALL artifacts and minerals (not just 25)  
✅ **No hardcoding**: Item list comes from the game itself  
✅ **No config dependency**: No longer needs `config.VanillaCategories.MuseumItemsTotalCount`  
✅ **Automatic updates**: Works with game updates  

## Removed Methods
- ❌ `GetMuseumItems()` - replaced by `GetMuseumItemData().DonatedCount`
- ❌ `GetMuseumItemDetails()` - replaced by `GetMuseumItemData().DetailItems`
- ❌ Removed dependency on `config.VanillaCategories.MuseumItemsTotalCount`

---

## Next Steps (Future Work)
This same pattern can be applied to:
- Friendships (next priority)
- SVE content (when adding mod support)
- Ridgeside content (when adding mod support)

## Summary
**Fish Species**, **Cooking Recipes**, **Crafting Recipes**, and **Museum Items** now all use a single source of truth pattern:
1. One method retrieves all data
2. Progress bar uses `.CaughtCount`/`.CookedCount`/`.CraftedCount`/`.DonatedCount` and `.TotalCount`
3. Detail view uses `.DetailItems`
4. No duplication, no inconsistencies
5. Dynamic data from game APIs (except fish which uses curated list)
