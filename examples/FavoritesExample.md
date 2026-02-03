# Working with Favorite Presets

This example demonstrates how to work with favorite presets in Pianoteq.

## What You Can Do

✅ **Get all favorite presets**
```csharp
var favorites = await client.GetFavoritePresetsAsync();
Console.WriteLine($"You have {favorites.Count} favorites");
```

✅ **Navigate through favorites**
```csharp
await client.NextFavouritePresetAsync();  // Jump to next favorite
await client.PrevFavouritePresetAsync();  // Jump to previous favorite
```

✅ **Check if current preset is a favorite**
```csharp
bool isFavorite = await client.IsCurrentPresetFavoriteAsync();
```

✅ **Filter and search favorites**
```csharp
var favorites = await client.GetFavoritePresetsAsync();

// Find favorites by tag
var jazzFavorites = favorites.Where(f => 
    f.Tags != null && f.Tags.Contains("jazz", StringComparer.OrdinalIgnoreCase)
).ToList();

// Find favorites by name
var steinwayFavorites = favorites.Where(f => 
    f.Name.Contains("Steinway", StringComparison.OrdinalIgnoreCase)
).ToList();
```

## What You Cannot Do (UI Only)

❌ **Set/unset favorites programmatically**
- Pianoteq's JSON-RPC API does not provide methods to mark or unmark presets as favorites
- You must use the Pianoteq UI to manage your favorites
- Once marked in the UI, they will appear via the API

❌ **Modify preset tags programmatically**
- Tags are also read-only via the API
- Manage them through the Pianoteq UI

## Complete Example

```csharp
using Pianoteq.Client;

var client = new PianoteqClient("http://retropie:8081");

// Get all favorites
var favorites = await client.GetFavoritePresetsAsync();

if (favorites.Count == 0)
{
    Console.WriteLine("No favorites yet!");
    Console.WriteLine("Mark some presets as favorites in Pianoteq UI first.");
    return;
}

Console.WriteLine($"Found {favorites.Count} favorite presets:\n");

// List all favorites with their tags
foreach (var fav in favorites)
{
    Console.WriteLine($"★ {fav.Name}");
    if (!string.IsNullOrEmpty(fav.Bank))
        Console.WriteLine($"  Bank: {fav.Bank}");
    if (fav.Tags != null && fav.Tags.Any())
        Console.WriteLine($"  Tags: {string.Join(", ", fav.Tags)}");
    Console.WriteLine();
}

// Navigate through favorites
Console.WriteLine("Press Enter to cycle through your favorites...");
Console.ReadLine();

for (int i = 0; i < Math.Min(5, favorites.Count); i++)
{
    await client.NextFavouritePresetAsync();
    var info = await client.GetInfoAsync();
    Console.WriteLine($"Now playing: {info.CurrentPreset?.Name}");
    await Task.Delay(2000); // Wait 2 seconds
}
```

## For Your MudBlazor App

When building your UI, you can:

1. **Display favorites in a special section**
   ```csharp
   var favorites = await client.GetFavoritePresetsAsync();
   // Show in a "⭐ Favorites" category
   ```

2. **Add quick navigation buttons**
   ```csharp
   // "Previous Favorite" and "Next Favorite" buttons
   ```

3. **Show favorite status indicator**
   ```csharp
   bool isFav = await client.IsCurrentPresetFavoriteAsync();
   // Display a star icon if true
   ```

4. **Add a note in the UI**
   ```
   "💡 To manage favorites, use Pianoteq's UI. 
      Your favorites will automatically appear here."
   ```
