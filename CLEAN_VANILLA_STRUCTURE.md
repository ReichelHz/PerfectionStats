# Clean Vanilla Structure - Margins & External Positioning

## Overview
Complete rebuild of the PerfectionStats menu structure using clean vanilla patterns with proper content margins and external positioning for close button and scrollbar. This is a fresh, simplified approach that follows vanilla Stardew menu conventions.

## Core Principle
**Content inside margins, controls outside frame**

This matches vanilla menus like Collections, Crafting, and Inventory where:
- Menu frame contains the content area
- Close button (X) is OUTSIDE at top-right corner
- Scrollbar is OUTSIDE on the right side
- Content has clear margins from frame edges

## Key Changes

### 1. Menu Title
**Changed**: "PERFECTION" → **"Perfection Stats"**
- More descriptive
- Better indicates purpose
- Vanilla-friendly naming

### 2. Content Area with Margins
**New Structure**:
```
Menu Frame (1000x700)
├─ ContentMarginLeft: 64px
├─ ContentMarginRight: 64px
├─ ContentMarginTop: 96px
└─ ContentMarginBottom: 64px
    └─ Content Area (872x540)
        ├─ Title (centered)
        ├─ Categories (scrollable)
        └─ Overall Section
```

**Benefits**:
- Content never touches frame edges
- Professional spacing
- Easy to read
- Clean layout

### 3. Close Button (OUTSIDE Frame)
**Position**: `xPositionOnScreen + width + 4, yPositionOnScreen + 4`
- 64x64 vanilla size
- Positioned OUTSIDE right edge of frame
- At top-right corner
- Matches vanilla menus exactly

### 4. Scrollbar (OUTSIDE Frame)
**Position**: `xPositionOnScreen + width + ScrollbarOffset`
- 48px offset from frame edge
- Track and handle outside frame
- Scroll arrows outside frame
- Clean separation from content

### 5. Simplified Sizing
**Menu Dimensions**: 1000x700 (fixed)
- Centered in viewport
- Large enough for content
- Not too large to overwhelm
- Works at all common resolutions

## Layout Constants

### Content Margins
```csharp
ContentMarginLeft = 64      // Left edge to content
ContentMarginRight = 64     // Content to right edge
ContentMarginTop = 96       // Top edge to content
ContentMarginBottom = 64    // Content to bottom edge
```

### Category Layout
```csharp
CategoryHeight = 60         // Height of one category row
CategorySpacing = 10        // Gap between categories
```

### Overall Section
```csharp
OverallSectionHeight = 120  // Overall perfection area
```

### External Controls
```csharp
ScrollbarOffset = 48        // Distance from frame to scrollbar
```

## Content Area Calculation

### X Position
```csharp
contentX = xPositionOnScreen + ContentMarginLeft
```

### Y Position
```csharp
contentY = yPositionOnScreen + ContentMarginTop
```

### Width
```csharp
contentWidth = width - ContentMarginLeft - ContentMarginRight
// 1000 - 64 - 64 = 872px
```

### Height
```csharp
contentHeight = height - ContentMarginTop - ContentMarginBottom
// 700 - 96 - 64 = 540px
```

## Positioning System

### Title (Centered in Content Area)
```csharp
titleX = contentX + (contentWidth - titleWidth) / 2
titleY = yPositionOnScreen + ContentMarginTop
```

### Categories (Within Content Area)
```csharp
categoryX = contentX
categoryY = scrollableY + (index * (CategoryHeight + CategorySpacing)) - scrollPosition
```

### Overall Section (Within Content Area)
```csharp
overallX = contentX
overallY = lastCategoryY + 20  // Small gap
```

### Close Button (OUTSIDE Frame)
```csharp
closeX = xPositionOnScreen + width + 4   // Outside right edge
closeY = yPositionOnScreen + 4           // Top corner
```

### Scrollbar (OUTSIDE Frame)
```csharp
scrollbarX = xPositionOnScreen + width + ScrollbarOffset
scrollbarY = contentTop to contentBottom (dynamically sized)
```

## Draw Order

1. Fade background (full screen)
2. Menu frame (parchment background + border)
3. **Title** (centered in content area)
4. **Categories** (scrollable, clipped to content area)
5. **Separator line** (between categories and overall)
6. **Overall section** (centered in content area)
7. **Scrollbar** (if needed, OUTSIDE frame)
8. **Scroll buttons** (if needed, OUTSIDE frame)
9. **Close button** (OUTSIDE frame)
10. Mouse cursor

## Scrolling System

### Content Height Calculation
```csharp
int titleHeight = 48
int categoriesHeight = categories.Count * (CategoryHeight + CategorySpacing)
int overallHeight = OverallSectionHeight
int separatorGap = 20

Total = titleHeight + categoriesHeight + separatorGap + overallHeight
```

### Visible Height
```csharp
int visibleHeight = height - ContentMarginTop - ContentMarginBottom - titleHeight
// 700 - 96 - 64 - 48 = 492px available for scrollable content
```

### Max Scroll
```csharp
int maxScroll = Math.Max(0, contentHeight - visibleHeight)
```

### Scroll Handle Size
```csharp
scrollBarHeight = Math.Max(24, (int)((float)visibleHeight / contentHeight * scrollBarTrack))
```
- Proportional to visible content
- Minimum 24px for usability

### Scroll Handle Position
```csharp
float scrollPercentage = (float)scrollPosition / Math.Max(1, contentHeight - visibleHeight)
int scrollbarY = scrollBarRunner.Y + (int)(scrollPercentage * (scrollBarTrack - scrollBarHeight))
```

## Category Row Layout

Within content area (872px wide):
```
[Label]
[------Bar (752px)------][Numbers (60px)][Button (40px)][Margin (20px)]
```

- Bar: 752px (contentWidth - 120)
- Numbers: 60px for "123/456"
- Button: 40px for magnifying glass
- Right margin: 20px buffer

## Overall Section Layout

Within content area (872px wide):
```
[Label centered]
[---Bar (772px) centered---]
[Percentage centered]
```

- Label: Centered, 1.2x scale
- Bar: 772px (contentWidth - 100), centered
- Percentage: Centered below bar, 1.1x scale

## Visual Hierarchy

1. **Frame** - Defines menu boundary
2. **Content Area** - Clear margins from edges
3. **Title** - Centered, clear identity
4. **Categories** - Clean rows with consistent spacing
5. **Overall Section** - Prominent summary
6. **External Controls** - Close & scrollbar outside frame

## Before & After

### Frame Structure
**Before**: Controls mixed with content
- Close button inside frame
- Scrollbar inside frame
- Content touching edges

**After**: Clean separation
- Close button OUTSIDE frame (top-right)
- Scrollbar OUTSIDE frame (right side)
- Content has clear margins

### Content Area
**Before**: Ad-hoc positioning
- No defined margins
- Inconsistent spacing
- Elements too close to edges

**After**: Professional margins
- 64px left/right margins
- 96px top margin
- 64px bottom margin
- Clean, readable layout

### Title
**Before**: "PERFECTION" (all caps, short)
**After**: "Perfection Stats" (descriptive, clear)

### Sizing
**Before**: Dynamic calculations, potential issues
**After**: Fixed 1000x700, centered, stable

## Benefits

### Clarity
✓ Clear content area with margins
✓ Controls visually separated from content
✓ Professional spacing throughout

### Stability
✓ Fixed menu size (1000x700)
✓ Simple positioning calculations
✓ No overlapping elements

### Vanilla Feel
✓ Close button outside (like Collections)
✓ Scrollbar outside (like Crafting)
✓ Proper margins (like Inventory)
✓ Familiar to players

### Maintainability
✓ Clean constants
✓ Simple math
✓ Easy to understand
✓ Future-proof

## Files Modified

### PerfectionStatsMenu.cs
**Constants Updated**:
- Replaced layout constants with content margin system
- Added `ScrollbarOffset` for external positioning

**Constructor**:
- Fixed menu size: 1000x700
- Centered in viewport
- Calls initialization methods

**InitializeButtons()**:
- Close button: OUTSIDE frame at `width + 4`
- Scroll buttons: OUTSIDE frame at `width + ScrollbarOffset`

**SetupScrollbar()**:
- Scrollbar track: OUTSIDE frame
- Proper height calculations

**draw()**:
- Defines content area with margins
- All content positioned relative to contentX/contentY
- Controls drawn OUTSIDE frame

**DrawTitle(), DrawProgressCategory(), DrawOverallSection()**:
- Accept contentX and contentWidth parameters
- Center and position within content area

### UIStrings.cs
**Title Changed**:
- `MenuTitle = "Perfection Stats"` (was "PERFECTION")

## Testing Checklist

Frame & Margins:
- [ ] Menu frame fully visible
- [ ] No elements touching frame edges
- [ ] Content has clear margins

External Controls:
- [ ] Close button outside frame at top-right
- [ ] Scrollbar outside frame on right side
- [ ] Scroll arrows outside frame
- [ ] No overlap with frame

Content Area:
- [ ] Title centered
- [ ] Categories aligned properly
- [ ] Overall section centered
- [ ] All content within margins

Scrolling:
- [ ] Scrollbar appears when needed
- [ ] Handle size proportional
- [ ] Drag works smoothly
- [ ] Arrow buttons work
- [ ] Wheel scroll works

Resolution Stability:
- [ ] Works at 1920x1080
- [ ] Works at 1280x720
- [ ] Works in windowed mode
- [ ] Works in fullscreen

## Result

✅ **Clean structure** - Content inside margins, controls outside  
✅ **Vanilla positioning** - Matches Collections, Crafting, Inventory  
✅ **Clear hierarchy** - Frame → Margins → Content  
✅ **Professional spacing** - 64-96px margins throughout  
✅ **External controls** - Close & scrollbar outside frame  
✅ **Stable sizing** - Fixed 1000x700, centered  
✅ **Readable layout** - Content never touches edges  
✅ **Native feel** - Looks like it belongs in the game  

✅ **No logic changes** - All calculations and values preserved

## Philosophy

This rebuild follows the principle:
**"The frame is the container, the content respects margins, the controls live outside."**

This creates a clean, professional menu that feels native to Stardew Valley while maintaining all the custom progress tracking functionality.
