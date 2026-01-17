# English-Only Localization Cleanup

## Overview
Refactored the mod to use consistent English text throughout, removing all Spanish strings and centralizing user-facing text to prepare for future localization.

## Changes Made

### 1. Created UIStrings Class

**New File**: `PerfectionStats\UIStrings.cs`

Centralized all user-facing strings in a single static class:
- Menu titles and labels
- Category names (vanilla, SVE, Ridgeside)
- Subtitle formats
- Detail view labels
- Format methods for dynamic text

**Benefits:**
- Single source of truth for all UI text
- Easy to find and update strings
- Prepared for future i18n implementation
- No scattered hardcoded strings

### 2. Updated PerfectionStatsMenu.cs

#### Replaced Spanish Comments:
- ❌ `"mucho más grande, acorde al menú de opciones"` 
- ✅ `"larger size matching options menu"`

- ❌ `"Escala muy aumentada para mayor visibilidad"`
- ✅ `"Very large scale for visibility"`

- ❌ `"Calcular el área disponible para las categorías"`
- ✅ `"Calculate available area for categories"`

- ❌ `"Dibujar línea separadora dorada"`
- ✅ `"Draw golden separator line"`

- ❌ `"Fondo de la barra (gris oscuro)"`
- ✅ `"Bar background (dark gray)"`

- ❌ `"Color de la fruta estelar"`
- ✅ `"Stardrop color"`

And many more...

#### Used Centralized Strings:
```csharp
// Before
categories.Add(new ProgressCategory { Name = "Fish Species", ... });

// After
categories.Add(new ProgressCategory { Name = UIStrings.FishSpecies, ... });
```

All category names, titles, and labels now use `UIStrings` constants.

### 3. Updated CategoryDetailsMenu.cs

#### Replaced Spanish Comments:
- ❌ `"Close button - mucho más grande"`
- ✅ `"Close button - larger size"`

- ❌ `"Scroll buttons - mucho más grandes"`
- ✅ `"Scroll buttons - large and visible"`

#### Used Format Method:
```csharp
// Before
string statusText = $"{completed} / {total} Completed";

// After
string statusText = UIStrings.FormatCompletionStatus(completed, total);
```

## Complete String List

### Main Menu
- `"Perfection Tracker"` - Menu title
- `"OVERALL PERFECTION"` - Overall section label

### Vanilla Categories
- `"Fish Species"`
- `"Cooking Recipes"`
- `"Crafting Recipes"`
- `"Museum Items"`
- `"Friendships (8+ Hearts)"`
- `"Crops Grown"`
- `"Forageables"`

### SVE Categories
- `"SVE: Fish Species"`
- `"SVE: NPCs Befriended"`
- `"SVE: Artifacts"`
- `"SVE: Crops"`

### Ridgeside Village Categories
- `"Rideside: NPCs Met"`
- `"Rideside: Items"`
- `"Rideside: Quests"`

### Mod Detection Subtitles
- `"(SVE + Rideside)"`
- `"(SVE)"`
- `"(Rideside)"`

### Detail View
- `"Completed"` - Used in "{X} / {Y} Completed"

## Future Localization Preparation

The code is now structured to easily add localization:

### Current Structure:
```csharp
internal static class UIStrings
{
    public const string FishSpecies = "Fish Species";
    // ...
}
```

### Future i18n Structure (example):
```csharp
internal class UIStrings
{
    private static ITranslationHelper translations;
    
    public static void Initialize(ITranslationHelper helper)
    {
        translations = helper;
    }
    
    public static string FishSpecies => translations.Get("category.fish-species");
    // ...
}
```

## Testing Checklist

- ✅ Build successful
- ✅ No Spanish text in code comments
- ✅ No hardcoded English strings (all centralized)
- ✅ All category names use UIStrings
- ✅ All menu labels use UIStrings
- ✅ Detail view uses UIStrings
- ✅ Format methods handle dynamic text

## Code Quality Improvements

1. **Consistency** - All English, no mixed languages
2. **Maintainability** - Easy to find and update text
3. **Readability** - English comments for international developers
4. **Scalability** - Ready for i18n implementation
5. **Documentation** - Clear purpose and future path

## Notes

- No i18n system implemented yet (as requested)
- English-only for now
- All strings centralized in `UIStrings.cs`
- Format methods prepared for parameterized translations
- Comments updated to English for consistency
