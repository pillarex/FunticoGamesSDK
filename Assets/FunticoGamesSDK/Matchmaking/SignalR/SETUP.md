# SignalR Plugin Setup

This guide explains how to set up the SignalR plugin dependencies on Windows, macOS, and Linux.

## Prerequisites

### 1. Install PowerShell

**Windows:**
PowerShell 5.1 is built-in. Alternatively, install PowerShell Core from:
https://github.com/PowerShell/PowerShell/releases

**macOS (Homebrew):**
```bash
brew install powershell/tap/powershell
```

**Linux (Ubuntu/Debian):**
```bash
sudo apt-get install -y powershell
```

**Verify installation:**
```bash
pwsh --version
```

### 2. Install NuGet CLI

**Windows:**
Download from https://www.nuget.org/downloads and add to PATH, or use Chocolatey:
```powershell
choco install nuget.commandline
```

**macOS (Homebrew):**
```bash
brew install nuget
```

**Linux:**
```bash
sudo apt-get install -y nuget
# or download manually from https://www.nuget.org/downloads
```

**Verify installation:**
```bash
nuget help
```

### 3. Install .NET SDK (For Server Development)

The server project targets .NET 10. To run or modify the server, install the [.NET 10 SDK](https://dotnet.microsoft.com/download).

**Windows:**
Download from: https://dotnet.microsoft.com/download

**macOS (Homebrew):**
```bash
brew install dotnet
```

**Linux:**
Follow instructions at: https://learn.microsoft.com/en-us/dotnet/core/install/linux

**Note:** Unity 6 generates `.slnx` solution files which require .NET SDK 9.0.200+. You can also use the traditional `.sln` file with older SDK versions.

## Installing SignalR Dependencies

Since the SDK is installed as a package in `Packages/`, DLL files must be installed separately in the `Assets/` folder.

### Step 1: Create folder and copy script

1. Create folder `Assets/Plugins/SignalR` in your Unity project

2. Copy one of the installation scripts from the SDK package:
   - `Packages/com.funticogames.sdk/Matchmaking/SignalR/signalr-installation-way1.ps1`
   - or `Packages/com.funticogames.sdk/Matchmaking/SignalR/signalr-installation-way2.ps1`
   
   > **Note:** Both scripts perform the same function but use different PowerShell syntax. If one doesn't work, try the other.

3. (Optional) Configure the script variables at the top of the file:
   ```powershell
   $signalRVersion = "10.0.1"      # SignalR Client version
   $netTarget = "netstandard2.0"   # Target framework (netstandard2.0 or netstandard2.1)
   ```

### Step 2: Run the script

**Windows:**

1. Open PowerShell and navigate to the folder:
   ```powershell
   cd path\to\Unity\Assets\Plugins\SignalR
   ```

2. Run the setup script:
   ```powershell
   .\signalr-installation-way1.ps1
   ```

**macOS / Linux:**

1. Open a terminal and navigate to the folder:
   ```bash
   cd /path/to/Unity/Assets/Plugins/SignalR
   ```

2. Run the setup script:
   ```bash
   pwsh ./signalr-installation-way1.ps1
   ```

### What the script does

The script will:
- Download `Microsoft.AspNetCore.SignalR.Client` and all dependencies from NuGet
- Extract the `netstandard2.0` compatible DLLs to the `dll/` folder
- Clean up temporary files

Refresh Unity (or restart it) to load the new assemblies.

## Troubleshooting

### "Permission denied" errors with NuGet cache

If you see errors like:
```
Access to the path '/Users/yourname/.local/share/NuGet/...' is denied.
```

Fix the permissions:
```bash
sudo chown -R $(whoami):staff ~/.local/share
```

### "nuget: command not found"

Ensure NuGet is installed and in your PATH:
```bash
brew install nuget
```

### "HubConnection could not be found" in Unity

1. Ensure the DLLs exist in `Assets/Plugins/SignalR/dll/`
2. Refresh the Unity project (Assets > Refresh or Ctrl/Cmd+R)
3. Check the Unity Console for any import errors

### Script creates weird folder names like `.\temp`

This was a bug in older versions of the script. Update to the latest `signalr.ps1` which uses `Join-Path` for cross-platform compatibility.

## Updating SignalR Version

To update to a newer version of SignalR:

1. Edit the installation script in `Assets/Plugins/SignalR/` and change the version number:
   ```powershell
   $signalRVersion = "10.0.1"  # Change to desired version
   ```

2. Delete the existing `dll/` folder:
   ```bash
   rm -rf Assets/Plugins/SignalR/dll/
   ```

3. Re-run the script:
   ```bash
   pwsh ./signalr-installation-way1.ps1
   ```

## Platform Support

- **macOS**: Fully supported with PowerShell Core
- **Windows**: Fully supported with PowerShell 5.1+ or PowerShell Core
- **Linux**: Supported with PowerShell Core

## Files

In SDK package (`Packages/com.funticogames.sdk/Matchmaking/SignalR/`):
- `signalr-installation-way1.ps1` - Installation script (variant 1)
- `signalr-installation-way2.ps1` - Installation script (variant 2)
- `SignalR.cs` - C# wrapper for SignalR
- `SignalR.jslib` - JavaScript interop for WebGL
- `version.txt` - Current SignalR version

In your project (`Assets/Plugins/SignalR/`):
- `dll/` - Output directory containing the SignalR assemblies (created by script)
