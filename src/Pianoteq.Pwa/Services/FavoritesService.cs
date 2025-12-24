using Blazored.LocalStorage;

namespace Pianoteq.Pwa.Services;

public class FavoritesService
{
    private const string FAVORITES_KEY = "pianoteq_favorites";
    private readonly ILocalStorageService _localStorage;
    private HashSet<string> _favorites = new();

    public FavoritesService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        var stored = await _localStorage.GetItemAsync<List<string>>(FAVORITES_KEY);
        _favorites = stored != null ? new HashSet<string>(stored) : new HashSet<string>();
    }

    public bool IsFavorite(string presetName) => _favorites.Contains(presetName);

    public async Task ToggleFavoriteAsync(string presetName)
    {
        if (_favorites.Contains(presetName))
        {
            _favorites.Remove(presetName);
        }
        else
        {
            _favorites.Add(presetName);
        }
        await SaveAsync();
    }

    public async Task AddFavoriteAsync(string presetName)
    {
        _favorites.Add(presetName);
        await SaveAsync();
    }

    public async Task RemoveFavoriteAsync(string presetName)
    {
        _favorites.Remove(presetName);
        await SaveAsync();
    }

    public List<string> GetAllFavorites() => _favorites.ToList();

    private async Task SaveAsync()
    {
        await _localStorage.SetItemAsync(FAVORITES_KEY, _favorites.ToList());
    }
}
