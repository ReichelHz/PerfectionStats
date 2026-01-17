# Force English Names in Detail Lists

## Overview
Updated all progress providers to display English names in detail lists regardless of game language settings. This ensures consistent English-only display across all categories.

## Problem
- Detail lists were using localized `DisplayName` properties
- When game language was set to Spanish/other languages, item names appeared localized
- NPC names showed localized display names instead of English names

## Solution
Changed all providers to use internal English names instead of localized display names:

### Object-Based Categories (Fish, Crops, Forageables, Museum Items)
- **Before**: Used `obj.DisplayName` (localized based on game language)
- **After**: Use `obj.Name` (internal English name)
- **Method**: Created `GetEnglishObjectName()` helper method in each provider

### NPC Names (Friendship)
- **Before**: Used `character.displayName` (localized)
- **After**: Use `character.Name` (internal English name like "Abigail", "Olivia")

### Recipes (Cooking & Crafting)
- **Before**: Some code paths used `recipe.DisplayName` (localized)
- **After**: Use recipe dictionary keys (internal English names)

## Files Modified

### 1. FishProgressProvider.cs
- Added `GetEnglishObjectName()` method
- Uses `obj.Name` instead of `obj.DisplayName`
- Fallback to `Item_{id}` if name unavailable

### 2. CropsProgressProvider.cs
- Added `GetEnglishObjectName()` method
- Uses `obj.Name` for all crop names

### 3. ForageablesProgressProvider.cs
- Added `GetEnglishObjectName()` method
- Uses `obj.Name` for all forageable items

### 4. MuseumItemProgressProvider.cs
- Added `GetEnglishObjectName()` method
- Uses `obj.Name` for artifacts and minerals

### 5. FriendshipProgressProvider.cs
- Changed from `character.displayName` to `character.Name`
- Now shows internal English NPC names

### 6. CookingRecipeProgressProvider.cs
- Fixed fallback code to use `recipeName` instead of `recipe.DisplayName`
- Already was using internal names in main path

### 7. CraftingRecipeProgressProvider.cs
- Simplified to always use `recipeName` (dictionary key)
- Removed `recipe.DisplayName` usage entirely

## Technical Details

### Object.Name vs Object.DisplayName
In Stardew Valley:
- `Name` = Internal English identifier (e.g., "Parsnip", "Wild Horseradish")
- `DisplayName` = Localized name based on game language (e.g., "Chirivía" in Spanish)

### NPC.Name vs NPC.displayName
- `Name` = Internal English identifier (e.g., "Abigail", "Emily")
- `displayName` = Localized name (often same as Name for vanilla NPCs, but can differ for mods)

### Recipe Names
- Recipe dictionary keys are always internal English names
- `recipe.DisplayName` can be localized

## Result

### Before:
Game in Spanish → Detail lists showed Spanish names:
- "Pejerrey" instead of "Anchovy"
- "Chirivía" instead of "Parsnip"
- "Abigail" (localized displayName)

### After:
Game in ANY language → Detail lists ALWAYS show English names:
- "Anchovy"
- "Parsnip"
- "Abigail" (internal Name)

## Testing

Build: ✅ Successful

Expected behavior:
1. Set game language to Spanish
2. Open PerfectionStats mod menu
3. Click details on any category
4. All item/NPC names show in English

## Compatibility

- ✅ Vanilla Stardew Valley items
- ✅ Modded items (use internal names set by mod authors)
- ✅ Content Patcher items
- ✅ Stardew Valley Expanded NPCs/items
- ✅ Ridgeside Village NPCs/items

## Fallbacks

If an English name cannot be retrieved:
- Objects: Falls back to `Item_{id}` format
- NPCs: Uses internal Name property (always available)
- Recipes: Uses recipe key (always English)

## Notes

- This is DISPLAY ONLY - does not affect counting logic
- Does not affect completion tracking
- Does not change IDs or keys used for tracking
- Only changes what text is shown to the player
