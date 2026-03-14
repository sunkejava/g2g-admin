using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using G2G.Admin.API.Data;
using G2G.Admin.API.Entities;

namespace G2G.Admin.API.Services;

public interface ISettingService
{
    Task<List<Setting>> GetAllAsync();
    Task<Setting?> GetByKeyAsync(string key);
    Task<string?> GetValueAsync(string key, string? defaultValue = null);
    Task<Setting> SetAsync(string key, string value, string? description = null);
    Task<bool> DeleteAsync(string key);
    Task ClearCacheAsync();
}

public class SettingService : ISettingService
{
    private readonly G2GDbContext _dbContext;
    private readonly IMemoryCache _cache;

    public SettingService(G2GDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<List<Setting>> GetAllAsync()
    {
        return await _dbContext.Settings.OrderBy(s => s.Key).ToListAsync();
    }

    public async Task<Setting?> GetByKeyAsync(string key)
    {
        return await _dbContext.Settings.FirstOrDefaultAsync(s => s.Key == key);
    }

    public async Task<string?> GetValueAsync(string key, string? defaultValue = null)
    {
        if (_cache.TryGetValue(key, out string? cachedValue))
        {
            return cachedValue;
        }

        var setting = await _dbContext.Settings.FindAsync(key);
        var value = setting?.Value ?? defaultValue;
        
        _cache.Set(key, value, TimeSpan.FromMinutes(10));
        return value;
    }

    public async Task<Setting> SetAsync(string key, string value, string? description = null)
    {
        var setting = await _dbContext.Settings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null)
        {
            setting = new Setting { Key = key };
            _dbContext.Settings.Add(setting);
        }

        setting.Value = value;
        setting.Description = description ?? setting.Description;
        await _dbContext.SaveChangesAsync();

        _cache.Set(key, value, TimeSpan.FromMinutes(10));
        return setting;
    }

    public async Task<bool> DeleteAsync(string key)
    {
        var setting = await _dbContext.Settings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) return false;

        _dbContext.Settings.Remove(setting);
        await _dbContext.SaveChangesAsync();
        _cache.Remove(key);
        return true;
    }

    public async Task ClearCacheAsync()
    {
        _cache.Remove("settings");
        var settings = await GetAllAsync();
        foreach (var setting in settings)
        {
            _cache.Remove(setting.Key);
        }
    }
}
