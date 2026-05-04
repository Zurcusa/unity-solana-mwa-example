using System.Threading.Tasks;
using Newtonsoft.Json;
using Solana.Unity.SolanaMobileStack;
using UnityEngine;

public class DemoAuthorizationCache : IAuthorizationCache
{
    private const string Key = "SolanaDemo.MWA.Auth";

    public Task<AuthorizationRecord?> GetAsync()
    {
        string json = PlayerPrefs.GetString(Key, null);
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
        PlayerPrefs.SetString(Key, JsonConvert.SerializeObject(record));
        PlayerPrefs.Save();
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
        return Task.CompletedTask;
    }
}
