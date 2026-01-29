# Funtico Games SDK

## Description

**Funtico Games SDK** is a Unity SDK for integrating games with the Funtico Games platform. The SDK provides a complete set of tools for working with tournaments, rooms, user profiles, and session management.

## Documentation

- 📖 [SDK Usage Guide](./USAGE.md) - detailed instructions on setup and SDK usage
- 🔧 [Internal Architecture](./ARCHITECTURE.md) - description of internal logic and SDK services

## Installation

### Dependencies

The SDK requires:

1. ✅ **Newtonsoft.Json** (3.2.1+) - installs automatically with SDK
2. ⚠️ **UniTask** - must be installed separately
3. ⚠️ **Unity WebP** - must be installed separately

### Step 1: Install Dependencies

**Important**: Install these BEFORE installing the SDK!

**📖 Detailed guides:**
- [UniTask Installation Guide](https://github.com/Cysharp/UniTask?tab=readme-ov-file#install-via-git-url)
- [Unity WebP Installation Guide](https://github.com/netpyoung/unity.webp#installation)

For **Unity WebP**, it's recommended to use OpenUPM registry.

**Manual Setup via Project Settings:**
1. Open `Edit > Project Settings > Package Manager`
2. Add a new Scoped Registry:
   - **Name**: `OpenUPM`
   - **URL**: `https://package.openupm.com`
   - **Scope(s)**: `com.netpyoung.webp`
3. Click `Save`
4. Open Package Manager and install `WebP` from "My Registries"

After that use Package Manager to import packages:
1. Open `Window > Package Manager`
2. Click `+` > `Add package from git URL...`
3. Add each URL:
   - `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`
   - `https://github.com/netpyoung/unity.webp.git?path=unity_project/Assets/unity.webp`

Wait for Unity to finish importing before proceeding to Step 2.

### Step 2: Install SDK

**Method A: Via Package Manager (Recommended)**

1. Open `Window > Package Manager`
2. Click `+` button in the top-left corner
3. Select `Add package from git URL...`
4. Enter: `https://github.com/pillarex/FunticoGamesSDK.git?path=Assets/FunticoGamesSDK`
5. Click `Add`

**Method B: Install Everything at Once via manifest.json**

If you prefer installing via git URLs instead of OpenUPM, add all to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.funticogames.sdk": "https://github.com/pillarex/FunticoGamesSDK.git?path=Assets/FunticoGamesSDK",
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.netpyoung.webp": "https://github.com/netpyoung/unity.webp.git?path=unity_project/Assets/unity.webp"
  }
}
```

**Note**: 
- Newtonsoft.Json will be installed automatically by the SDK

### Verification

After installation:

1. Wait for Unity compilation to complete (may take a few minutes)
2. Check the Console for any errors
3. Verify packages appear in Package Manager:
   - Funtico Games SDK
   - UniTask
   - Unity WebP
   - Newtonsoft Json

### Troubleshooting

**Dependencies not found:**
- Make sure you installed all dependencies BEFORE installing the SDK
- Check that Git is installed on your system (Unity needs it for git URLs)
- For Unity WebP: verify OpenUPM scoped registry is properly configured in Project Settings

**Compilation errors:**
- Clear Package Manager cache: delete `Library/PackageCache` folder
- Restart Unity Editor
- Reinstall packages one by one

**Need more help?**
- [UniTask Installation Guide](https://github.com/Cysharp/UniTask?tab=readme-ov-file#install-via-git-url)
- [Unity WebP Installation Guide](https://github.com/netpyoung/unity.webp#installation)

## Quick Start

```csharp
using System;
using FunticoGamesSDK;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private async void Start()
    {
        // Create error handler
        IErrorHandler errorHandler = new MyErrorHandler();
        
        // Initialize SDK
        await FunticoSDK.Instance.Initialize(
            env: FunticoSDK.Environment.STAGING,
            privateGameKey: "your-private-key",
            publicGameKey: "your-public-key",
            userToken: "user-platform-token",
            errorHandler: errorHandler
        );
        
        Debug.Log("SDK initialized successfully!");
        
        // Get user data
        var userData = await FunticoSDK.Instance.GetUserData();
        Debug.Log($"User: {userData.Name}");
    }
}

// Example error handler
public class MyErrorHandler : IErrorHandler
{
    public void ShowError(string errorMessage, string errorTitle = "Error", 
        string buttonText = "OK", Action additionalActionOnCloseClick = null)
    {
        Debug.LogError($"{errorTitle}: {errorMessage}");
        additionalActionOnCloseClick?.Invoke();
    }
}
```

## Access Keys

The SDK requires the following keys:

- **Public Game Key** - public key for your game
- **Private Game Key** - private key for your game
- **User Token** - user token from Funtico platform

⚠️ **Important**: Never publish your private key publicly!

## Environments

The SDK supports two environments:

- `FunticoSDK.Environment.STAGING` - test environment for development
- `FunticoSDK.Environment.PROD` - production environment

**Recommendation**: Use STAGING for development and testing, PROD - only for release versions.

## Support

If you encounter issues:

1. Check that all dependencies are installed
2. Make sure you're using an up-to-date Unity version (2021.3+)
3. Verify your access keys are correct
4. Contact Funtico Games technical support

---

**SDK Version**: 1.0.0
