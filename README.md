# Unity Solana MWA Example

A demo Unity Android app showcasing the [Solana.Unity-SDK](https://github.com/magicblock-labs/Solana.Unity-SDK) Mobile Wallet Adapter v2 API. Covers authentication, transaction signing, and wallet lifecycle management.

## Prerequisites

- Unity 6 (6000.4+) with Android Build Support (IL2CPP)
- An MWA-compatible wallet installed on the device: [Phantom](https://phantom.app/) or [Solflare](https://solflare.com/)
- An Android device or Solana Seeker

## Setup

1. Clone this repository
2. Clone the [Solana.Unity-SDK](https://github.com/magicblock-labs/Solana.Unity-SDK)
3. Open `Packages/manifest.json` and update the `com.solana.unity_sdk` path to point at your local SDK clone:
   ```json
   "com.solana.unity_sdk": "file:/path/to/your/Solana.Unity-SDK"
   ```
4. Open the project in Unity
5. Switch to Android platform in Build Settings (Player Settings: IL2CPP, ARM64)

## Scenes

### Auth

Authentication lifecycle with all session management methods:

- **Login** — authorize with wallet (silent reconnect if cached session exists)
- **Sign-In With Solana (SIWS)** — authorize + sign a proof message in a single wallet interaction
- **Reconnect** — restore a cached session without opening the wallet
- **Disconnect** — clear local state (cache + auth token)
- **Deauthorize** — revoke the session at the wallet level
- **Clone Authorization** — create a second independent auth token from the current session
- **Network Switching** — switch between DevNet and MainNet (clears cache on switch)

### Operations

Wallet operations (navigate here after connecting in the Auth scene):

- **SignTransaction** — sign a single transaction
- **SignAllTransactions** — sign a batch of transactions
- **SignMessage** — sign an arbitrary message string
- **SignAndSendTransactions** — sign and broadcast a transaction, with exhaustive result handling
- **GetCapabilities** — query the wallet's supported features and limits

## Project Structure

```
Assets/Scripts/
  AdapterManager.cs             Singleton managing the SolanaMobileWalletAdapter instance,
                                network switching, and auto-reconnect on app launch
  AuthScreenController.cs       Auth scene UI — buttons for all session methods
  OperationsScreenController.cs Operations scene UI — buttons for signing and capabilities
  DemoAuthorizationCache.cs     Custom IAuthorizationCache using a dedicated PlayerPrefs key
  SceneNavigator.cs             Scene loading utility
  DemoBuild.cs                  Editor-only CLI build helper
```

## Building

**From Unity:** File > Build Settings > Build (Android selected)

**From CLI:**
```bash
Unity -batchmode -nographics -projectPath . -quit -buildTarget Android -executeMethod DemoBuild.BuildApk
```

The APK is written to `Builds/Demo.apk`.

## Testing

Build the APK, install on an Android device with Phantom or Solflare, and walk through both scenes. The Auth scene covers the full session lifecycle; the Operations scene covers all signing and query methods.
