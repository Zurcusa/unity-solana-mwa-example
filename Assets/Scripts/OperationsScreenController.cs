using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.SolanaMobileStack;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OperationsScreenController : MonoBehaviour
{
    [SerializeField] private Text statusText;
    [SerializeField] private Text chainLabel;
    [SerializeField] private Button signTxButton;
    [SerializeField] private Button signAllButton;
    [SerializeField] private Button signMsgButton;
    [SerializeField] private Button signAndSendButton;
    [SerializeField] private Button getCapabilitiesButton;
    [SerializeField] private Button devnetButton;
    [SerializeField] private Button mainnetButton;
    [SerializeField] private Text networkLabel;
    [SerializeField] private Button navButton;

    private bool _busy;

    private void Start()
    {
        signTxButton.onClick.AddListener(() => Run(DoSignTransaction));
        signAllButton.onClick.AddListener(() => Run(DoSignAll));
        signMsgButton.onClick.AddListener(() => Run(DoSignMessage));
        signAndSendButton.onClick.AddListener(() => Run(DoSignAndSend));
        getCapabilitiesButton.onClick.AddListener(() => Run(DoGetCapabilities));
        devnetButton.onClick.AddListener(() => SwitchNetwork(RpcCluster.DevNet));
        mainnetButton.onClick.AddListener(() => SwitchNetwork(RpcCluster.MainNet));
        navButton.onClick.AddListener(() => SceneManager.LoadScene("Auth"));
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
        signTxButton.onClick.RemoveAllListeners();
        signAllButton.onClick.RemoveAllListeners();
        signMsgButton.onClick.RemoveAllListeners();
        signAndSendButton.onClick.RemoveAllListeners();
        getCapabilitiesButton.onClick.RemoveAllListeners();
        devnetButton.onClick.RemoveAllListeners();
        mainnetButton.onClick.RemoveAllListeners();
        navButton.onClick.RemoveAllListeners();
    }

    private void SwitchNetwork(RpcCluster cluster)
    {
        AdapterManager.Instance.SwitchNetwork(cluster);
        SetStatus($"Switched to {cluster}. Please login from Auth screen.");
    }

    private void RefreshUI()
    {
        var mgr = AdapterManager.Instance;
        if (mgr == null) return;
        bool connected = mgr.IsConnected;
        networkLabel.text = $"Network: {mgr.CurrentCluster}";
        devnetButton.interactable = mgr.CurrentCluster != RpcCluster.DevNet;
        mainnetButton.interactable = mgr.CurrentCluster != RpcCluster.MainNet;
        chainLabel.text = connected
            ? $"Chain: {mgr.ConnectedChain}"
            : "Chain: not connected";
        signTxButton.interactable = connected;
        signAllButton.interactable = connected;
        signMsgButton.interactable = connected;
        signAndSendButton.interactable = connected;
    }

    private void UpdateConnectionStatus()
    {
        var mgr = AdapterManager.Instance;
        if (mgr == null) return;
        SetStatus(mgr.IsConnected
            ? $"Connected: {mgr.ConnectedAddress}\nChain: {mgr.ConnectedChain}"
            : "Not connected — go to Auth screen first");
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
        catch (JsonRpcException jrpc) when (jrpc.Code == -7)
        {
            SetStatus("Chain not supported by wallet. Switch wallet network and try again.");
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

    private async Task DoSignTransaction()
    {
        var tx = await BuildDemoTransaction();
        var signed = await AdapterManager.Instance.Adapter.SignTransaction(tx);
        SetStatus(signed != null ? "Transaction signed successfully." : "Returned null.");
    }

    private async Task DoSignAll()
    {
        var txs = new[] { await BuildDemoTransaction(), await BuildDemoTransaction() };
        var signed = await AdapterManager.Instance.Adapter.SignAllTransactions(txs);
        SetStatus($"Signed {signed?.Length ?? 0} transaction(s).");
    }

    private async Task DoSignMessage()
    {
        var sig = await AdapterManager.Instance.Adapter.SignMessage("Solana Unity SDK demo message");
        SetStatus($"Signature: {ToHex(sig)}");
    }

    private async Task DoSignAndSend()
    {
        var tx = await BuildDemoTransaction();
        var result = await AdapterManager.Instance.Adapter.SignAndSendTransactions(
            new[] { tx },
            new SendOptions { Commitment = Commitment.Confirmed });

        SetStatus(result switch
        {
            SignAndSendTxResult.Success s => $"Success! {s.Signatures.Length} sig(s).\n{ToHex(s.Signatures[0])}",
            SignAndSendTxResult.UserDenied => "Transaction declined.",
            SignAndSendTxResult.AuthRevoked => "Session expired — please reconnect.",
            SignAndSendTxResult.WalletUnreachable => "Wallet not reachable.",
            SignAndSendTxResult.InvalidPayloads ip => $"Invalid payloads. Valid: {FormatBools(ip.Valid)}",
            SignAndSendTxResult.NotSubmitted ns => $"Not submitted. {ns.PartialSignatures?.Length ?? 0} partial sig(s).",
            SignAndSendTxResult.TooManyPayloads tm => $"Too many payloads. Max: {tm.MaxTransactionsPerRequest?.ToString() ?? "?"}",
            SignAndSendTxResult.ChainNotSupported => "Chain not supported by wallet.",
            _ => result.GetType().Name,
        });
    }

    private async Task DoGetCapabilities()
    {
        var caps = await AdapterManager.Instance.Adapter.GetCapabilities();
        var sb = new StringBuilder();
        sb.AppendLine($"max_transactions: {caps.MaxTransactionsPerRequest}");
        sb.AppendLine($"max_messages: {caps.MaxMessagesPerRequest}");
        sb.AppendLine($"tx_versions: [{string.Join(", ", caps.SupportedTransactionVersions ?? Array.Empty<string>())}]");
        sb.AppendLine($"clone_auth: {caps.SupportsCloneAuthorization}");
        sb.AppendLine($"sign_and_send: {caps.SupportsSignAndSendTransactions}");
        if (caps.Features != null && caps.Features.Length > 0)
            foreach (var f in caps.Features)
                sb.AppendLine($"  • {f}");
        SetStatus(sb.ToString());
    }

    private async Task<Transaction> BuildDemoTransaction(ulong lamports = 100)
    {
        var adapter = AdapterManager.Instance.Adapter;
        var blockHash = await adapter.ActiveRpcClient.GetLatestBlockHashAsync();
        return new Transaction
        {
            RecentBlockHash = blockHash.Result.Value.Blockhash,
            FeePayer = adapter.Account.PublicKey,
            Instructions = new List<TransactionInstruction>
            {
                SystemProgram.Transfer(adapter.Account.PublicKey, adapter.Account.PublicKey, lamports)
            },
            Signatures = new List<SignaturePubKeyPair>
            {
                new SignaturePubKeyPair { PublicKey = adapter.Account.PublicKey, Signature = new byte[64] }
            },
        };
    }

    private void SetButtons(bool enabled)
    {
        signTxButton.interactable = enabled && AdapterManager.Instance.IsConnected;
        signAllButton.interactable = enabled && AdapterManager.Instance.IsConnected;
        signMsgButton.interactable = enabled && AdapterManager.Instance.IsConnected;
        signAndSendButton.interactable = enabled && AdapterManager.Instance.IsConnected;
        getCapabilitiesButton.interactable = enabled;
    }

    private static string ToHex(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return "(empty)";
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.AppendFormat("{0:x2}", b);
        return sb.ToString();
    }

    private static string FormatBools(bool[] arr) =>
        arr == null ? "(null)" : "[" + string.Join(", ", arr) + "]";
}
