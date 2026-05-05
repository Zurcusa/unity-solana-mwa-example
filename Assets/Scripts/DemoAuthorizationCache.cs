using System.Threading.Tasks;
using Newtonsoft.Json;
using Solana.Unity.SolanaMobileStack;
using UnityEngine;

public class DemoAuthorizationCache : IAuthorizationCache
{
    private const string KeyPrefix = "SolanaDemo.MWA.Auth";
    private readonly string _key;

    public DemoAuthorizationCache(string clusterSuffix = null)
    {
        _key = string.IsNullOrEmpty(clusterSuffix)
            ? KeyPrefix
            : $"{KeyPrefix}.{clusterSuffix}";
    }

    public Task<AuthorizationRecord?> GetAsync()
    {
        string json = PlayerPrefs.GetString(_key, null);
        if (string.IsNullOrEmpty(json))
            return Task.FromResult<AuthorizationRecord?>(null);
        try
        {
            return Task.FromResult<AuthorizationRecord?>(
                JsonConvert.DeserializeObject<AuthorizationRecord>(json));
        }
        catch
        {
            return Task.FromResult<AuthorizationRecord?>(null);
        }
    }

    public Task SetAsync(AuthorizationRecord record)
    {
        if (record == null) return Task.CompletedTask;
        PlayerPrefs.SetString(_key, JsonConvert.SerializeObject(record));
        PlayerPrefs.Save();
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        PlayerPrefs.DeleteKey(_key);
        PlayerPrefs.Save();
        return Task.CompletedTask;
    }
}
