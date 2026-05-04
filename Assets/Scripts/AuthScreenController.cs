using System;
using System.Threading.Tasks;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.SolanaMobileStack;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthScreenController : MonoBehaviour
{
    [SerializeField] private Text statusText;
    [SerializeField] private Text chainLabel;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button siwsButton;
    [SerializeField] private Button reconnectButton;
    [SerializeField] private Button disconnectButton;
    [SerializeField] private Button deauthorizeButton;
    [SerializeField] private Button cloneAuthButton;
    [SerializeField] private Button devnetButton;
    [SerializeField] private Button mainnetButton;
    [SerializeField] private Text networkLabel;
    [SerializeField] private Button navButton;

    private bool _busy;

    private void Start()
    {
        loginButton.onClick.AddListener(() => Run(DoLogin));
        siwsButton.onClick.AddListener(() => Run(DoSiws));
        reconnectButton.onClick.AddListener(() => Run(DoReconnect));
        disconnectButton.onClick.AddListener(() => Run(DoDisconnect));
        deauthorizeButton.onClick.AddListener(() => Run(DoDeauthorize));
        cloneAuthButton.onClick.AddListener(() => Run(DoCloneAuth));
        devnetButton.onClick.AddListener(() => SwitchNetwork(RpcCluster.DevNet));
        mainnetButton.onClick.AddListener(() => SwitchNetwork(RpcCluster.MainNet));
        navButton.onClick.AddListener(() => SceneManager.LoadScene("Operations"));
    }

    private void OnEnable()
    {
        RefreshUI();
        if (AdapterManager.Instance != null)
            AdapterManager.Instance.OnConnectionChanged += OnConnectionChanged;
    }

    private void OnDisable()
    {
        if (AdapterManager.Instance != null)
            AdapterManager.Instance.OnConnectionChanged -= OnConnectionChanged;
    }

    private void OnConnectionChanged()
    {
        RefreshUI();
        UpdateConnectionStatus();
    }

    private void OnDestroy()
    {
        loginButton.onClick.RemoveAllListeners();
        siwsButton.onClick.RemoveAllListeners();
        reconnectButton.onClick.RemoveAllListeners();
        disconnectButton.onClick.RemoveAllListeners();
        deauthorizeButton.onClick.RemoveAllListeners();
        cloneAuthButton.onClick.RemoveAllListeners();
        devnetButton.onClick.RemoveAllListeners();
        mainnetButton.onClick.RemoveAllListeners();
        navButton.onClick.RemoveAllListeners();
    }

    private void SwitchNetwork(RpcCluster cluster)
    {
        AdapterManager.Instance.SwitchNetwork(cluster);
        SetStatus($"Switched to {cluster}. Please login.");
    }

    private void RefreshUI()
    {
        var mgr = AdapterManager.Instance;
        if (mgr == null) return;
        networkLabel.text = $"Network: {mgr.CurrentCluster}";
        devnetButton.interactable = mgr.CurrentCluster != RpcCluster.DevNet;
        mainnetButton.interactable = mgr.CurrentCluster != RpcCluster.MainNet;
        chainLabel.text = mgr.IsConnected
            ? $"Chain: {mgr.ConnectedChain}"
            : "Chain: not connected";
    }

    private void UpdateConnectionStatus()
    {
        var mgr = AdapterManager.Instance;
        if (mgr == null) return;
        SetStatus(mgr.IsConnected
            ? $"Connected: {mgr.ConnectedAddress}\nChain: {mgr.ConnectedChain}"
            : "Not connected");
    }

    private void SetStatus(string msg) => statusText.text = msg;

    private async void Run(Func<Task> op)
    {
        if (_busy) return;
        _busy = true;
        SetButtons(false);
        SetStatus("...");
        try
        {
            await op();
        }
        catch (OperationInFlightException)
        {
            SetStatus("Please wait for the current operation to complete.");
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}");
        }
        finally
        {
            _busy = false;
            SetButtons(true);
            RefreshUI();
        }
    }

    private async Task DoLogin()
    {
        var account = await AdapterManager.Instance.Adapter.Login();
        AdapterManager.Instance.NotifyConnectionChanged();
        SetStatus($"Connected: {account.PublicKey}\nChain: {AdapterManager.Instance.ConnectedChain}");
    }

    private async Task DoSiws()
    {
        var payload = new SignInPayload
        {
            Domain = "solana.unity-sdk.gg",
            Statement = "Sign in to Solana Unity Demo",
        };
        var (account, signInResult) = await AdapterManager.Instance.Adapter.LoginWithSignIn(payload);
        AdapterManager.Instance.NotifyConnectionChanged();
        var msg = $"Account: {account.PublicKey}\nChain: {AdapterManager.Instance.ConnectedChain}";
        if (signInResult != null)
            msg += $"\nSignature type: {signInResult.SignatureType ?? "ed25519"}\nSignature: {Truncate(signInResult.Signature, 40)}";
        else
            msg += "\n(wallet did not return sign_in_result)";
        SetStatus(msg);
    }

    private async Task DoReconnect()
    {
        var result = await AdapterManager.Instance.Adapter.Reconnect();
        AdapterManager.Instance.NotifyConnectionChanged();
        SetStatus(result switch
        {
            ReconnectResult.SilentSuccess s => $"Reconnected silently.\n{s.Account.PublicKey}\nChain: {AdapterManager.Instance.ConnectedChain}",
            ReconnectResult.FreshAuthorized f => $"Fresh authorization.\n{f.Account.PublicKey}\nChain: {AdapterManager.Instance.ConnectedChain}",
            ReconnectResult.NoCachedSession => "No cached session — login first.",
            ReconnectResult.Failed f => $"Failed: {f.Error.Message}",
            _ => result.GetType().Name,
        });
    }

    private async Task DoDisconnect()
    {
        await AdapterManager.Instance.Adapter.Disconnect();
        AdapterManager.Instance.NotifyConnectionChanged();
        SetStatus("Disconnected. Local cache cleared.");
    }

    private async Task DoDeauthorize()
    {
        var result = await AdapterManager.Instance.Adapter.Deauthorize();
        AdapterManager.Instance.NotifyConnectionChanged();
        SetStatus(result switch
        {
            DeauthorizeResult.FullyRevoked => "Authorization fully revoked.",
            DeauthorizeResult.LocalOnly lo => $"Local only.{(lo.WalletPackage != null ? $"\nPackage: {lo.WalletPackage}" : "")}",
            DeauthorizeResult.Failed f => $"Failed: {f.Error.Message}",
            _ => result.GetType().Name,
        });
    }

    private async Task DoCloneAuth()
    {
        try
        {
            var token = await AdapterManager.Instance.Adapter.CloneAuthorization();
            SetStatus($"Cloned token: {Truncate(token, 32)}");
        }
        catch (JsonRpcException jrpc) when (jrpc.Code == -5)
        {
            SetStatus("Wallet does not support clone_authorization.");
        }
    }

    private void SetButtons(bool enabled)
    {
        loginButton.interactable = enabled;
        siwsButton.interactable = enabled;
        reconnectButton.interactable = enabled;
        disconnectButton.interactable = enabled;
        deauthorizeButton.interactable = enabled;
        cloneAuthButton.interactable = enabled;
    }

    private static string Truncate(string v, int max) =>
        string.IsNullOrEmpty(v) ? "(empty)" : v.Length <= max ? v : v.Substring(0, max) + "…";
}
