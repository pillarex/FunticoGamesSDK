# Funtico Matchmaking Service

The matchmaking service for Funtico Games SDK allows finding players for real-time multiplayer games.

## Table of Contents

1. [Installation](#installation)
2. [API Reference](#api-reference)
3. [Data Models](#data-models)
4. [Matchmaking Flow](#matchmaking-flow)
5. [Usage Example](#usage-example)

---

## Installation

The Matchmaking service is an optional SDK module and requires additional setup.

### Step 1: Install SignalR Dependencies

Matchmaking uses SignalR for real-time communication. Since the SDK is installed as a package in the `Packages/` folder, DLL files must be installed separately in the `Assets/` folder.

1. Create folder `Assets/Plugins/SignalR` in your project

2. Copy one of the installation scripts from the SDK to this folder:
   - `Packages/com.funticogames.sdk/Matchmaking/SignalR/signalr-installation-way1.ps1`
   - or `Packages/com.funticogames.sdk/Matchmaking/SignalR/signalr-installation-way2.ps1`
   
   > **Note:** Both scripts perform the same function but use different PowerShell syntax. If one doesn't work, try the other.

3. (Optional) Configure the variables at the top of the script:
   ```powershell
   $signalRVersion = "10.0.1"      # SignalR Client version
   $netTarget = "netstandard2.0"   # Target framework (netstandard2.0 or netstandard2.1)
   ```

4. Open PowerShell and navigate to the folder:
   ```powershell
   cd Assets\Plugins\SignalR
   ```

5. Run the installation script:
   ```powershell
   .\signalr-installation-way1.ps1
   ```

6. Wait for the download to complete and refresh the Unity project.

> **Note:** For detailed instructions for different OS and troubleshooting, see `Packages/com.funticogames.sdk/Matchmaking/SignalR/SETUP.md` or the [original repository](https://github.com/evanlindsey/Unity-WebGL-SignalR/blob/main/Unity/Assets/Plugins/SignalR/SETUP.md).

### Step 2: Add Scripting Define Symbol

The Matchmaking module is enabled via a define symbol:

1. Open **Edit > Project Settings > Player**
2. Expand the **Other Settings** section
3. Find the **Scripting Define Symbols** field
4. Add the symbol:
   ```
   USE_FUNTICO_MATCHMAKING
   ```

5. Click **Apply** and wait for recompilation.

### Step 3: Verification

After installation, verify that:
- DLL files are present in `Assets/Plugins/SignalR/dll/`
- No compilation errors in Console
- The `FunticoMatchmaking` class is available in code

---

## API Reference

### FunticoMatchmaking

Main class for accessing the matchmaking service. Uses the singleton pattern.

#### Instance Access

```csharp
FunticoMatchmaking.Instance
```

### Methods

#### JoinQueue

Joins the match search queue.

```csharp
public UniTask JoinQueue(string eventId, MatchmakingRegion region, int size = 2)
```

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `eventId` | `string` | Id of event you're trying to join |
| `region` | `MatchmakingRegion` | Region for match search |
| `size` | `int` | Number of players in the match (2-16). Default: `2` |

**Example:**
```csharp
await FunticoMatchmaking.Instance.JoinQueue("event-guid", MatchmakingRegion.Europe, 2);
```

---

#### AcceptMatch

Accepts a pending match proposal received via `OnAcceptMatch`.

```csharp
public void AcceptMatch()
```

**Example:**
```csharp
FunticoMatchmaking.Instance.AcceptMatch();
```

---

#### DeclineMatch

Declines a pending match proposal. The connection is closed after declining.

```csharp
public void DeclineMatch()
```

**Example:**
```csharp
FunticoMatchmaking.Instance.DeclineMatch();
```

---

#### LeaveQueue

Leaves the match search queue. The connection is closed after leaving.

```csharp
public void LeaveQueue()
```

**Example:**
```csharp
FunticoMatchmaking.Instance.LeaveQueue();
```

---

#### GetServerSetupData

Returns server setup data from environment variables. **Server-side only** (requires `SERVER` or `UNITY_SERVER` define).

```csharp
public ServerSetupData GetServerSetupData()
```

**Returns:** `ServerSetupData` - Server configuration including match ID and players data

**Example:**
```csharp
#if SERVER || UNITY_SERVER
var setupData = FunticoMatchmaking.Instance.GetServerSetupData();
Debug.Log($"Match ID: {setupData.MatchId}");

foreach (var player in setupData.Players)
{
    Debug.Log($"Player key: {player.Key}, Name: {player.Value.UserName}");
}
#endif
```

**Note:** This method reads from environment variables (`MatchId`, `User_Keys`) that are set by the matchmaking API when spawning game server instances.

---

#### Dispose

Releases resources and closes the connection.

```csharp
public void Dispose()
```

---

### Events

#### OnMatchStatus

Called when the search status is updated.

```csharp
public event Action<string> OnMatchStatus;
```

**Example:**
```csharp
FunticoMatchmaking.Instance.OnMatchStatus += status =>
{
    Debug.Log($"Matchmaking Status: {status}");
};
```

---

#### OnAcceptMatch

Called when a potential match is found and requires player confirmation. The player should call `AcceptMatch()` or `DeclineMatch()` within the timeout period.

```csharp
public event Action<AcceptMatchServer> OnAcceptMatch;
```

**Example:**
```csharp
FunticoMatchmaking.Instance.OnAcceptMatch += acceptData =>
{
    Debug.Log($"Match proposal: {acceptData.MatchId}, accept within {acceptData.TimeoutSeconds}s");
    // Show accept/decline UI to the player
};
```

---

#### OnMatchFound

Called when all players have accepted and the match is confirmed. Contains the server URL to connect to.

```csharp
public event Action<MatchResult> OnMatchFound;
```

**Example:**
```csharp
FunticoMatchmaking.Instance.OnMatchFound += result =>
{
    Debug.Log($"Match Found! ID: {result.MatchId}");
    Debug.Log($"Server: {result.ServerUrl}");
    
    foreach (var opponent in result.Opponents)
    {
        Debug.Log($"Opponent: {opponent.UserName}");
    }
};
```

---

#### OnMatchCancelled

Called when a match proposal is cancelled. This happens when another player declines, leaves, or fails to accept within the timeout. Received by players who have already accepted and by players who haven't acted yet.

```csharp
public event Action<string> OnMatchCancelled;
```

**Example:**
```csharp
FunticoMatchmaking.Instance.OnMatchCancelled += reason =>
{
    Debug.Log($"Match Cancelled: {reason}");
};
```

---

#### OnMatchError

Called when the player pressed Accept but the API did not allow them to join the room (e.g. validation failure).

```csharp
public event Action<string> OnMatchError;
```

**Example:**
```csharp
FunticoMatchmaking.Instance.OnMatchError += error =>
{
    Debug.LogError($"Match Error: {error}");
};
```

---

#### OnConnectionStarted

Called when the SignalR connection to the matchmaking server is established.

```csharp
public event Action OnConnectionStarted;
```

---

#### OnConnectionClosed

Called when the SignalR connection to the matchmaking server is closed (either by the server or by calling `Dispose`/`LeaveQueue`/`DeclineMatch`).

```csharp
public event Action OnConnectionClosed;
```

---

## Data Models

### MatchmakingRegion

Enum with available regions for match search.

| Value | Code | Description |
|-------|------|-------------|
| `Europe` | EU | Europe |
| `Asia` | AS | Asia |
| `NorthAmerica` | NA | North America |
| `SouthAmerica` | SA | South America |
| `MiddleEast` | ME | Middle East |

---

### MatchResult

Result of a found match.

| Property | Type | Description |
|----------|------|-------------|
| `MatchId` | `Guid` | Unique match identifier |
| `ServerUrl` | `string` | Server URL for connection |
| `Opponents` | `List<OpponentData>` | List of opponents |

---

### AcceptMatchServer

Match acceptance proposal data.

| Property | Type | Description |
|----------|------|-------------|
| `MatchId` | `Guid` | Unique match identifier |
| `TimeoutSeconds` | `int` | Time (in seconds) the player has to accept or decline |

---

### OpponentData

Opponent data in a match.

| Property | Type | Description |
|----------|------|-------------|
| `UserName` | `string` | User name |
| `FunticoId` | `int` | Unique ID in Funtico platform (same as `UserData.PlatformId`) |
| `AvatarUrl` | `string` | User avatar URL |
| `AvatarType` | `string` | Avatar type |
| `BorderUrl` | `string` | Profile border URL |
| `SdkUserId` | `int` | User ID in SDK system (same as `UserData.UserId`) |

---

### ServerSetupData

Server configuration data (server-side only).

| Property | Type | Description |
|----------|------|-------------|
| `MatchId` | `string` | Match identifier |
| `RoomSettings` | `string` | Room configuration settings |
| `Players` | `Dictionary<string, OpponentData>` | Player data keyed by player identifier |

---

## Matchmaking Flow

```
JoinQueue() ──► OnMatchStatus (status updates while searching)
                    │
                    ▼
              OnAcceptMatch (match found, waiting for confirmation)
                    │
           ┌────────┼────────┐
           ▼        ▼        ▼
     AcceptMatch() DeclineMatch() (timeout)
           │        │        │
           ▼        │        │
      OnMatchFound  │        │
      (all accepted)│        │
           │        ▼        ▼
           │     OnMatchCancelled
           │     (received by those who accepted
           │      or didn't act yet)
           ▼
      OnMatchError
      (accept was rejected by API)
```

---

## Usage Example

For a complete matchmaking usage example, see [Assets/Example/SetupSDK.cs](./Assets/Example/SetupSDK.cs).

Key sections:
- **Event subscription**: lines 50-97
- **UI controls**: lines 425-514

---

## Important Notes

1. **SDK Initialization**: Ensure that `FunticoSDK.Instance.Initialize()` is called before using matchmaking.

2. **WebGL Support**: The SignalR plugin supports WebGL via JavaScript interop (`SignalR.jslib`). After building a WebGL project, you must add the SignalR script to the `<head>` section of `index.html`:
   ```html
   <script src="https://unpkg.com/@microsoft/signalr@10.0.0/dist/browser/signalr.min.js"></script>
   ```
   More details: [Unity-WebGL-SignalR - Client JS File](https://github.com/evanlindsey/Unity-WebGL-SignalR?tab=readme-ov-file#client-js-file)

3. **Reconnection**: SignalR automatically reconnects on connection loss with the following intervals: 0s, 1s, 3s, 10s.

4. **Match Size**: The `size` parameter in `JoinQueue` defines the total number of players in the match (including the current player). Supported values are 2 to 16.

---

## See Also

- [README.md](./README.md) - General SDK information
- [USAGE.md](./USAGE.md) - Detailed description of all SDK methods
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Internal SDK architecture
- [Assets/FunticoGamesSDK/Matchmaking/SignalR/SETUP.md](./Assets/FunticoGamesSDK/Matchmaking/SignalR/SETUP.md) - SignalR setup
