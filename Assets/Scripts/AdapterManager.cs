using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.SolanaMobileStack;
using UnityEngine;

public class AdapterManager : MonoBehaviour
{
    public static AdapterManager Instance { get; private set; }

    public SolanaMobileWalletAdapter Adapter { get; private set; }
    private const string ClusterPrefKey = "SolanaDemo.SelectedCluster";
    public RpcCluster CurrentCluster { get; private set; } = RpcCluster.DevNet;
    public bool IsConnected => Adapter?.Account != null;
    public string ConnectedAddress => Adapter?.Account?.PublicKey?.ToString() ?? "";
    public string ConnectedChain { get; private set; } = "unknown";

    public event System.Action OnConnectionChanged;

    private IAuthorizationCache _cache;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        if (Adapter == null)
        {
            RestoreSavedCluster();
            _cache = new DemoAuthorizationCache(CurrentCluster.ToString());
            CreateAdapter();
            TryAutoReconnect();
        }
    }

    public async void SwitchNetwork(RpcCluster cluster)
    {
        if (cluster == CurrentCluster) return;
        CurrentCluster = cluster;
        PlayerPrefs.SetString(ClusterPrefKey, cluster.ToString());
        PlayerPrefs.Save();
        await _cache.ClearAsync();
        _cache = new DemoAuthorizationCache(cluster.ToString());
        ConnectedChain = "unknown";
        CreateAdapter();
        OnConnectionChanged?.Invoke();
    }

    public async void NotifyConnectionChanged()
    {
        await UpdateChain();
        OnConnectionChanged?.Invoke();
    }

    private void RestoreSavedCluster()
    {
        string saved = PlayerPrefs.GetString(ClusterPrefKey, null);
        if (!string.IsNullOrEmpty(saved) && System.Enum.TryParse<RpcCluster>(saved, out var cluster))
            CurrentCluster = cluster;
    }

    private void CreateAdapter()
    {
        var options = new SolanaMobileWalletAdapterOptions
        {
            identityUri = "https://solana.unity-sdk.gg/",
            iconUri = "/favicon.ico",
            name = "Solana Unity Demo",
            keepConnectionAlive = true,
            Cache = _cache,
        };
        try
        {
            Adapter = new SolanaMobileWalletAdapter(options, CurrentCluster);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[AdapterManager] Adapter init: {ex.Message}");
        }
    }

    private async void TryAutoReconnect()
    {
        if (Adapter == null) return;
        try
        {
            var result = await Adapter.Reconnect();
            if (result is ReconnectResult.SilentSuccess)
            {
                await UpdateChain();
                Debug.Log($"[AdapterManager] Auto-reconnected: {ConnectedAddress} on {ConnectedChain}");
                OnConnectionChanged?.Invoke();
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"[AdapterManager] Auto-reconnect skipped: {ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task UpdateChain()
    {
        try
        {
            var record = await _cache.GetAsync();
            ConnectedChain = record?.Chain ?? "unknown";
        }
        catch
        {
            ConnectedChain = "unknown";
        }
    }
}
