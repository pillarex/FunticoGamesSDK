# Funtico SDK - Internal Architecture

This document describes the internal architecture, services, and logic of the Funtico SDK.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Core Components](#core-components)
3. [Service Layer](#service-layer)
4. [Network Layer](#network-layer)
5. [API Models](#api-models)
6. [Data Flow](#data-flow)
7. [Security & Encryption](#security--encryption)
8. [Caching Strategy](#caching-strategy)
9. [Asset Loading](#asset-loading)

---

## Architecture Overview

### High-Level Structure

```
┌─────────────────────────────────────────────────────┐
│              FunticoSDK (Facade)                    │
│  Singleton pattern - single entry point for all SDK │
└────────────┬────────────────────────────────────────┘
             │
    ┌────────┴──────────┬──────────────┬──────────────┐
    │                   │              │              │
┌───▼──────────┐  ┌────▼─────────┐ ┌──▼────────┐ ┌──▼──────────┐
│ Auth Service │  │ User Service │ │  Rooms    │ │  Session    │
│   Provider   │  │   Provider   │ │ Provider  │ │  Managers   │
└──────┬───────┘  └──────┬───────┘ └─────┬─────┘ └──────┬──────┘
       │                 │               │              │
       └────────┬────────┴───────┬───────┴──────────────┘
                │                │
         ┌──────▼────────┐  ┌────▼─────────┐
         │  HTTP Client  │  │ Assets Loader│
         └───────────────┘  └──────────────┘
```

### Design Patterns

1. **Facade Pattern** - `FunticoSDK` provides a simplified interface to complex subsystems
2. **Singleton Pattern** - Single instance of SDK accessed via `FunticoSDK.Instance`
3. **Strategy Pattern** - Different session management strategies for client/server
4. **Repository Pattern** - Data providers abstract data access logic
5. **Dependency Injection** - Services receive dependencies through constructors

---

## Core Components

### FunticoSDK (Main Facade)

**File**: `Assets/FunticoGamesSDK/FunticoSDK/FunticoSDKCore.cs`

The main entry point implemented as a partial class split across multiple files:

- `FunticoSDKCore.cs` - Core initialization and setup
- `FunticoSDKAuthService.cs` - Authentication methods delegation
- `FunticoSDKUserDataService.cs` - User data methods delegation
- `FunticoSDKRoomsService.cs` - Rooms/tournaments methods delegation
- `FunticoSDKClientSessionManager.cs` - Client session management delegation
- `FunticoSDKServerSessionManager.cs` - Server session management delegation

**Responsibilities:**
- SDK initialization and configuration
- Service orchestration and dependency injection
- Delegates all operations to appropriate service providers
- Warmup services on initialization (pre-load user data)

**Key Design Decisions:**
- Partial class structure for better code organization
- Each service has its own file for maintainability
- Lazy initialization of services
- Automatic data pre-loading during initialization

**Initialization Flow:**

```
Initialize()
   │
   ├─> SetupEnvironment(env)
   ├─> SetupAuthDataService()
   ├─> SetupUserDataService()
   ├─> SetupServerSessionManager()
   ├─> SetupClientSessionManager()
   ├─> SetupRoomsProvider()
   ├─> HTTPClient.Setup()
   └─> WarmupServices()
         │
         └─> Authentication(userToken)
         └─> GetBalance() & GetUserData() in parallel
```

---

## Service Layer

### 1. Authentication Service

**File**: `Assets/FunticoGamesSDK/AuthDataProviders/AuthDataProvider.cs`

**Purpose**: Manages user authentication with Funtico platform.

**Implementation Details:**

```csharp
public class AuthDataProvider : IAuthDataProvider
{
    private LoginResponse _loginResponse;
    private string _platformToken;
    
    public async UniTask<LoginResponse> Authentication(string platformToken)
    {
        // Store platform token
        _platformToken = platformToken;
        
        // Send authentication request to API
        var (_, response) = await HTTPClient.Post<LoginResponse>(
            APIConstants.FUNTICO_LOGIN, 
            new { Token = platformToken }
        );
        
        // Cache login response
        _loginResponse = response;
        return _loginResponse;
    }
}
```

**Key Features:**
- Stores both platform token and internal user token
- Uses HTTP POST to authenticate
- Caches `LoginResponse` containing user token
- User token is automatically included in all subsequent API requests

**Token Flow:**
1. User provides platform token (from Funtico platform)
2. SDK exchanges platform token for internal user token
3. User token stored and used for all API requests
4. Token added automatically to request headers by HTTPClient

---

### 2. User Data Service

**File**: `Assets/FunticoGamesSDK/UserDataProviders/UserDataService.cs`

**Purpose**: Manages user profile data, balance, and vouchers with caching.

**Architecture:**

```csharp
public class UserDataService : IUserDataService
{
    // Cached data
    private UserData Data { get; set; }
    private BalanceResponse Balance { get; set; }
    private List<VoucherData> Vouchers { get; set; }
    private List<VoucherStaticData> _vouchersStaticData;
}
```

**Caching Strategy:**

```
GetUserData(useCache)
   │
   ├─> if (Data == null || !useCache)
   │     └─> UpdateUserData()
   │           └─> HTTP GET /api/user/data
   │                 └─> Cache result in Data
   │
   └─> return Data
```

**Balance Checking Logic:**

```csharp
public bool CanAffordFromCache(EntryFeeType type, int amount)
{
    var data = GetCachedBalance();
    switch (type)
    {
        case EntryFeeType.Tico:
            return data.Diamonds >= amount;
        case EntryFeeType.SemifinalsTickets:
            return data.SemiFinalTickets >= amount;
        case EntryFeeType.FinalTickets:
            return data.FinalTickets >= amount;
        case EntryFeeType.PrivateTickets:
            return data.PrivateTickets >= amount;
        case EntryFeeType.Free:
            return true;
        default:
            return true; // fallback to API check
    }
}
```

**Vouchers Processing:**
1. Loads static voucher definitions (tier images, names) - cached permanently
2. Loads user's voucher data (counts, progress)
3. Merges static and dynamic data
4. Returns complete voucher list with images and progress

**Key Features:**
- Three-level caching: UserData, Balance, Vouchers
- Separate methods for cached (instant) and fresh data
- Smart voucher merging (static + dynamic data)
- Balance validation for all currency types

---

### 3. Rooms Provider

**File**: `Assets/FunticoGamesSDK/RoomsProviders/RoomsProvider.cs`

**Purpose**: Manages tournament rooms, joining, prize pools, and leaderboards.

**Class Hierarchy:**

```
RoomsProviderInternal (base class)
   │
   └─> RoomsProvider (public interface)
```

**Dependencies:**
- `IUserDataService` - Check user balance before joining
- `IAuthDataProvider` - User authentication
- `IClientSessionManager` - Client session tracking
- `IServerSessionManager` - Server session tracking
- `IErrorHandler` - Display errors to user

**Join Room Flow:**

```
JoinRoom(roomGuid)
   │
   ├─> GetRoom(roomGuid) // Get room details
   │
   ├─> Check affordability
   │     └─> CanAfford(ticket)
   │           └─> UserDataService.CanAffordFromCache()
   │
   ├─> JoinSinglePlayerRoom(roomGuid, useVoucher)
   │     └─> HTTP POST /api/rooms/join
   │
   ├─> Check if already joined
   │     ├─> Yes: Use existing sessionId
   │     └─> No: Call EnterSinglePlayerRoom()
   │                └─> HTTP POST /api/rooms/enter
   │
   └─> Return RoomData {EventId, SessionOrMatchId}
```

**Room Validation:**
1. Check if room is free
2. Check if user has voucher
3. Check user balance for entry fee
4. Show error via IErrorHandler if validation fails

**Leaderboard Retrieval:**
- Fetches from API: `/api/rooms/leaderboard`
- Includes player rankings, scores, rewards
- Supports pending state (results being processed)
- Returns user's place, earnings, multiplier

**Key Features:**
- Automatic balance checking before joining
- Voucher support for free entry
- Pre-paid event handling
- Reconnection to existing sessions
- Prize pool calculation and distribution

---

### 4. Session Management

#### Client Session Manager

**File**: `Assets/FunticoGamesSDK/SessionsManagement/ClientSessionManager.cs`

**Purpose**: Manages game sessions on client side with encryption.

**Architecture:**

```csharp
public class ClientSessionManager : IClientSessionManager
{
    private readonly IUserDataService _userDataService;
    private List<string> _sessionLogs = new List<string>();
    private SavedSessionResponse _currentSessionData;
}
```

**Session Data Structure:**

```json
{
  "Data": "custom game state JSON",
  "EventsList": ["event1", "event2", "..."]
}
```

**Saved Session Response:**

```csharp
public class SavedSessionResponse
{
    public string Id { get; set; }              // Unique session identifier
    public string SessionId { get; set; }        // Event/Room identifier
    public string SaveSessionId { get; set; }    // Match/Session identifier
    public string Data { get; set; }             // Encrypted session data
    public int GameType { get; set; }            // Type of game
    public string Hash { get; set; }             // Data integrity hash
    public float ReconnectTime { get; set; }     // Time to reconnect
}
```

**Encryption Flow:**

```
CreateSession(json, gameType, eventId, saveSessionId)
   │
   ├─> Create SessionModel
   │     ├─> Data: json
   │     └─> EventsList: _sessionLogs
   │
   ├─> Generate encryption key
   │     └─> key = First 8 chars of GUID(UserId)
   │
   ├─> Encrypt data
   │     └─> AESNonDynamic.Encrypt(sessionModel, key)
   │
   ├─> Create SavedSessionResponse
   │     ├─> Data: encrypted string
   │     ├─> GameType: gameType
   │     ├─> SessionId: eventId
   │     ├─> SaveSessionId: saveSessionId
   │     └─> Hash: HashString(encryptedData, eventId)
   │
   └─> HTTP POST /api/session/create
         ├─> Send encrypted data with metadata
         └─> Store _currentSessionData for updates
```

**Key Features:**
- AES encryption for session data
- Encryption key derived from user UserId (GUID-based)
- Event logging for game replay/debugging
- Session restoration on reconnection
- Automatic session cleanup on close
- Multiple sessions support (user can have several unfinished sessions)
- Game type tracking (Tournament, PVP, Practice, etc.)
- Data integrity verification with hash

**Game Types:**

```csharp
public enum GameTypeEnum
{
    Indirect_PVP = 0,    // Asynchronous PVP
    Direct_PVP = 1,      // Real-time PVP
    Tournament = 3,      // Tournament mode
    Rooms = 4,           // Room-based games
    Practice = 5,        // Practice/training mode
}
```

**Security Measures:**
- Data encrypted before sending to server
- Encryption key never sent to server
- Key derivation uses user-specific data (UserId)
- Unique key for each user
- Data integrity verified with hash

#### Server Session Manager

**File**: `Assets/FunticoGamesSDK/SessionsManagement/ServerSessionManager.cs`

**Purpose**: Manages game sessions on server side (no encryption needed).

**Architecture:**

```csharp
public class ServerSessionManager : IServerSessionManager
{
    // Dictionary: userId -> session events
    private readonly Dictionary<int, List<string>> _sessionLogs;
}
```

**Multi-User Handling:**
- Maintains separate session logs per user
- Uses userId as dictionary key
- Supports concurrent sessions
- Each user has isolated event list

**Update Session Flow:**

Client tracks current session in `_currentSessionData`:

```
UpdateSession_Client(json)
   │
   ├─> Check if _currentSessionData exists
   │     └─> If null: Log error, return false
   │
   ├─> Encrypt updated data
   │     └─> Same encryption key as create
   │
   ├─> Add metadata from _currentSessionData
   │     ├─> Id: session identifier
   │     └─> Hash: HMAC(data, sessionId)
   │
   └─> HTTP POST /api/session/update
         └─> Update existing session
```

**Session Data Flow:**

```
CreateSession_Server(userId, json)
   │
   ├─> Create SessionModel
   │     ├─> Data: json
   │     └─> EventsList: _sessionLogs[userId]
   │
   ├─> HTTP POST /api/session/create?userId={userId}
   │     └─> Send unencrypted data (server-to-server)
   │
   └─> Initialize _sessionLogs[userId] = []
```

**Key Differences from Client:**
- No encryption (server-to-server communication)
- Manages multiple users simultaneously
- Requires userId parameter for all operations
- Uses query parameters to identify user

---

## Network Layer

### HTTP Client

**File**: `Assets/FunticoGamesSDK/NetworkUtils/HTTPClient.cs`

**Purpose**: Centralized HTTP communication with API.

**Architecture:**

```csharp
public static class HTTPClient
{
    private static string _sessionId;
    private static string _publicGameKey;
    private static string _privateGameKey;
    private static IErrorHandler _errorHandler;
    private static IAuthDataProvider _authDataProvider;
}
```

**Request Types:**

1. **GET Request**
```csharp
public static async Task<Tuple<bool, T>> Get<T>(string endPoint)
{
    // 1. Create UnityWebRequest
    // 2. Add authentication headers
    // 3. Send request
    // 4. Wait for completion
    // 5. Deserialize JSON response
    // 6. Return (success, data)
}
```

2. **POST Request**
```csharp
public static async Task<Tuple<bool, T>> Post<T>(string endPoint, object data)
{
    // 1. Serialize data to JSON
    // 2. Create UnityWebRequest with JSON body
    // 3. Add authentication headers
    // 4. Send request
    // 5. Wait for completion
    // 6. Deserialize JSON response
    // 7. Return (success, data)
}
```

**Request Headers:**

```
Authorization: Bearer {userToken}
X-Game-Public-Key: {publicGameKey}
X-Session-Id: {sessionId}
Content-Type: application/json
```

**Server-Side Headers (with SERVER or UNITY_SERVER defines):**

```
X-Game-Private-Key: {privateGameKey}
X-Request-Hash: HMAC-SHA256(endpoint + requestBody, privateGameKey)
```

**Error Handling:**
1. Network errors → Log and return (false, default)
2. HTTP errors (4xx, 5xx) → Invoke error handler, log, return error data
3. JSON parsing errors → Log and return (false, default)
4. Success → Return (true, deserialized data)

**Logging:**
- All requests logged with method, status code, response
- Dedicated logger for server builds
- Error responses logged separately

**Key Features:**
- Automatic authentication header injection
- Session tracking with unique session ID
- Server-side request signing with HMAC
- Comprehensive error handling
- Generic return type support
- Async/await pattern with UniTask

---

## API Models

### Session-Related Models

#### SavedSessionResponse

Complete session information returned by the API.

```csharp
public class SavedSessionResponse
{
    public string Id { get; set; }              // Unique session identifier
    public string SessionId { get; set; }        // Event/Room identifier
    public string SaveSessionId { get; set; }    // Match/Session identifier  
    public string Data { get; set; }             // Encrypted session data
    public int GameType { get; set; }            // Type of game (GameTypeEnum)
    public string Hash { get; set; }             // Data integrity hash
    public float ReconnectTime { get; set; }     // Time available to reconnect (seconds)
}
```

**Usage:**
- Returned by `CreateSession_Client()`
- Contains all metadata needed to restore session
- `Data` field is encrypted client-side before sending
- `Hash` verifies data integrity

#### UnfinishedSessionsResponse

List of unfinished sessions for a user.

```csharp
public class UnfinishedSessionsResponse
{
    public List<SavedSessionResponse> SavedSessions { get; set; }
}
```

**Usage:**
- Returned by `UserHasUnfinishedSession_Client()`
- User may have multiple unfinished sessions
- Each session contains full metadata for reconnection
- Sessions sorted by creation time (newest first)

#### SessionModel

Internal session data structure (before encryption).

```csharp
public class SessionModel
{
    public string Data { get; set; }            // Custom game state (JSON)
    public List<string> EventsList { get; set; } // Recorded game events
}
```

**Usage:**
- Internal only - not sent directly to API
- Encrypted and wrapped in SavedSessionResponse
- `Data` contains custom game state defined by developer
- `EventsList` contains all recorded events

#### GameTypeEnum

Types of game sessions supported by the platform.

```csharp
public enum GameTypeEnum
{
    Indirect_PVP = 0,    // Asynchronous player vs player
    Direct_PVP = 1,      // Real-time player vs player
    Tournament = 3,      // Tournament mode
    Rooms = 4,           // Room/lobby based games
    Practice = 5,        // Practice/training mode
}
```

**Usage:**
- Passed to `CreateSession_Client()`
- Helps platform identify session type
- Affects reconnection behavior and time limits
- Used for analytics and matchmaking

### Model Relationships

```
User creates session
   │
   ├─> CreateSession_Client()
   │     ├─> Input: SessionModel (internal)
   │     │     ├─> Data: custom JSON
   │     │     └─> EventsList: []
   │     │
   │     ├─> Encrypt SessionModel
   │     │
   │     └─> Output: SavedSessionResponse
   │           ├─> Data: encrypted SessionModel
   │           ├─> SessionId: eventId
   │           ├─> SaveSessionId: matchId
   │           └─> GameType: enum value
   │
   └─> Session stored on server

User reconnects
   │
   ├─> UserHasUnfinishedSession_Client()
   │     └─> Returns: UnfinishedSessionsResponse
   │           └─> SavedSessions: [session1, session2, ...]
   │
   └─> ReconnectToUnfinishedSession_Client(sessionId)
         ├─> Input: session.Id
         ├─> Get SavedSessionResponse from server
         ├─> Decrypt Data field
         └─> Parse SessionModel
               ├─> Data: restore game state
               └─> EventsList: restore events
```

---

## Data Flow

### Complete Game Flow Example

```
1. SDK INITIALIZATION
   User launches game
      │
      ├─> FunticoSDK.Initialize()
      │     ├─> Setup services
      │     ├─> HTTPClient.Setup() (configure headers)
      │     └─> WarmupServices()
      │           ├─> Authentication(platformToken)
      │           │     └─> POST /api/auth/login
      │           │           └─> Store userToken
      │           │
      │           └─> Parallel load:
      │                 ├─> GetBalance()
      │                 └─> GetUserData()
      │
      └─> SDK ready

2. ROOM BROWSING
   User opens rooms list
      │
      ├─> GetRooms(tier)
      │     └─> GET /api/rooms?tier=X
      │           └─> Returns List<RoomViewModel>
      │
      └─> Display rooms in UI

3. ROOM DETAILS
   User clicks on room
      │
      ├─> GetRoom(roomGuid)
      │     └─> GET /api/rooms/{guid}
      │           └─> Returns RoomViewModel
      │
      ├─> GetPrizePoolDistribution(roomGuid)
      │     └─> GET /api/rooms/{guid}/prizes
      │           └─> Returns prize breakdown
      │
      └─> Display room details

4. JOIN ROOM
   User clicks "Join"
      │
      ├─> Validate affordability
      │     └─> CanAffordFromCache(feeType, amount)
      │           └─> Check cached balance
      │
      ├─> JoinRoom(roomGuid)
      │     ├─> POST /api/rooms/join
      │     │     └─> Deduct entry fee
      │     │
      │     └─> POST /api/rooms/enter
      │           └─> Create match/session
      │
      └─> Return RoomData {EventId, SessionOrMatchId}

5. CREATE SESSION
   Game starts loading
      │
      ├─> CreateSession_Client(gameStateJson)
      │     ├─> Encrypt session data
      │     │     ├─> Generate key from PlatformId + PrivateKey
      │     │     └─> AES.Encrypt(data)
      │     │
      │     └─> POST /api/session/create
      │           └─> Save encrypted session
      │
      └─> Initialize _sessionLogs = []

6. GAMEPLAY
   User plays game
      │
      ├─> OnScoreChange()
      │     └─> RecordEvent_Client("score_update")
      │           └─> _sessionLogs.Add(event)
      │
      ├─> OnLevelComplete()
      │     └─> RecordEvent_Client("level_complete")
      │           └─> _sessionLogs.Add(event)
      │
      └─> OnCheckpoint()
            └─> UpdateSession_Client(checkpointData)
                  ├─> Encrypt updated data + events
                  └─> POST /api/session/update
                        └─> Server stores updated session

7. FINISH GAME
   User completes game
      │
      └─> FinishRoomSession_Client(eventId, sessionId, score)
            ├─> POST /api/rooms/finish
            │     ├─> Submit final score
            │     ├─> Close session
            │     └─> Trigger prize distribution
            │
            └─> CloseCurrentSession_Client()
                  └─> Clear _sessionLogs

8. VIEW RESULTS
   Show leaderboard
      │
      └─> GetLeaderboard(eventId, sessionId, matchId)
            └─> GET /api/rooms/leaderboard
                  ├─> Player rankings
                  ├─> User's place & earnings
                  └─> Prize distribution

9. RECONNECTION (if disconnected during game)
   User relaunches game
      │
      ├─> UserHasUnfinishedSession_Client()
      │     └─> GET /api/session/unfinished
      │           └─> Returns UnfinishedSessionsResponse
      │                 └─> List of SavedSessionResponse
      │
      ├─> Show list of sessions (or auto-select)
      │     └─> User selects session to resume
      │
      └─> ReconnectToUnfinishedSession_Client(sessionId)
            ├─> GET /api/session/reconnect?id={sessionId}
            │     └─> Returns SavedSessionResponse
            │
            ├─> Store _currentSessionData
            │
            ├─> Decrypt session data
            │     └─> AES.Decrypt(data.Data, key)
            │           └─> key = First 8 chars of GUID(UserId)
            │
            ├─> Restore _sessionLogs
            ├─> Restore room data from session
            │     ├─> EventId = session.SessionId
            │     └─> SessionOrMatchId = session.SaveSessionId
            │
            └─> Restore game state
```

---

## Security & Encryption

### Client-Side Encryption

**Algorithm**: AES-256 (AESNonDynamic)

**Key Generation:**

```csharp
private string GetEncryptionKey()
{
    return IntToGuidHelper.IntToGuid(_userDataService.GetCachedUserData().UserId)
           .ToString()
           .Take(8)
           .ToString();
}
```

**Process:**
1. Convert user's UserId to GUID format
2. Take first 8 characters of the GUID string
3. Use as encryption key (8-character string)
4. Key is deterministic (same for user)
5. Key never transmitted to server

**Encryption Flow:**

```
Session Data (JSON)
   │
   ├─> Serialize to JSON string
   │
   ├─> Generate encryption key
   │     └─> key = First 8 chars of GUID(UserId)
   │
   ├─> AES.Encrypt(json, key)
   │     └─> Returns base64 encrypted string
   │
   ├─> Create SavedSessionResponse
   │     ├─> Data: encrypted string
   │     ├─> SessionId: eventId
   │     ├─> SaveSessionId: saveSessionId
   │     ├─> GameType: gameType enum value
   │     └─> Hash: HMAC(encryptedData, eventId)
   │
   └─> Send to server with metadata
```

**Decryption Flow:**

```
SavedSessionResponse from Server
   │
   ├─> Extract encrypted Data field
   │
   ├─> Generate same encryption key
   │     └─> key = First 8 chars of GUID(UserId)
   │
   ├─> AES.Decrypt(Data, key)
   │     └─> Returns JSON string
   │
   └─> Deserialize JSON to SessionModel
         ├─> Data: custom game state
         └─> EventsList: recorded events
```

### Server-Side Security

**Request Signing (SERVER builds only):**

```csharp
#if SERVER || UNITY_SERVER
var requestHash = HashUtils.HMACSHA256(
    endpoint + JsonConvert.SerializeObject(requestBody), 
    _privateGameKey
);
request.SetRequestHeader("X-Request-Hash", requestHash);
#endif
```

**Verification Process:**
1. Client calculates HMAC-SHA256 hash of request
2. Hash includes endpoint + request body + private key
3. Hash sent in X-Request-Hash header
4. Server verifies hash matches expected value
5. Prevents request tampering

**Why No Encryption on Server:**
- Server-to-server communication (trusted network)
- HTTPS already encrypts transport layer
- Request signing prevents tampering
- Reduces server processing overhead

---

## Caching Strategy

### Multi-Level Caching

```
┌─────────────────────────────────────┐
│     APPLICATION LAYER               │
│  GetCachedUserData() - instant      │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│     CACHE LAYER                     │
│  Data, Balance, Vouchers            │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│     SERVICE LAYER                   │
│  GetUserData(useCache=true)         │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│     NETWORK LAYER                   │
│  HTTP GET /api/user/data            │
└─────────────────────────────────────┘
```

### Cache Behavior

**UserData Cache:**
```
GetUserData(useCache)
   │
   ├─> useCache == true && Data != null?
   │     ├─> Yes: return Data (instant)
   │     └─> No: fetch from API
   │                └─> UpdateUserData()
   │                      └─> Cache result
   │
   └─> GetCachedUserData()
         └─> return Data (instant, may be null)
```

**Cache Invalidation:**
- Manual: Call GetUserData(useCache: false)
- Automatic: None (application-lifetime cache)
- Recommendation: Refresh after transactions

### When to Use Cache

✅ **Use cached data for:**
- UI updates (frequent reads)
- Balance checks before actions
- User info display
- Affordability validation

❌ **Fetch fresh data for:**
- After joining a room
- After purchases/transactions
- After receiving rewards
- Critical validation

---

## Asset Loading

### Asset Loader Architecture

**File**: `Assets/FunticoGamesSDK/AssetsProvider/AssetsLoader.cs`

**Providers:**

1. **ResourcesAssetProvider** - Loads from Unity Resources folder
2. **UrlAssetProvider** - Loads from URLs with caching

**Loading Strategy:**

```
LoadSpriteAsync(address)
   │
   ├─> Is URL?
   │     ├─> Yes: UrlAssetProvider
   │     │     ├─> Check cache
   │     │     │     ├─> Cached: return immediately
   │     │     │     └─> Not cached: download
   │     │     │
   │     │     ├─> Download texture
   │     │     │     └─> UnityWebRequestTexture
   │     │     │
   │     │     ├─> Resize if needed
   │     │     │     └─> TextureResizerUtility
   │     │     │
   │     │     ├─> Add to cache
   │     │     └─> Create sprite
   │     │
   │     └─> No: ResourcesAssetProvider
   │           └─> Resources.LoadAsync<Sprite>()
   │
   └─> Return Sprite
```

### URL Asset Caching

**Cache Structure:**

```csharp
public class UrlAssetProvider
{
    private LimitedSizeDictionary<string, Texture2D> _cache;
    
    public bool IsCached(string url, out Texture2D texture)
    {
        return _cache.TryGetValue(url, out texture);
    }
    
    public void AddToCache(string url, Texture2D texture)
    {
        _cache.Add(url, texture);
    }
}
```

**LimitedSizeDictionary:**
- Inherits from Dictionary
- Limited capacity (prevents memory overflow)
- FIFO eviction (oldest entries removed first)
- Thread-safe operations

### Image Optimization

**Texture Resizing:**

```csharp
public class TextureResizeOptions
{
    public int MaxSize { get; set; }
    
    public TextureResizeOptions(int maxSize = 512)
    {
        MaxSize = maxSize;
    }
}
```

**Resize Strategy:**
- Download full-resolution image
- Check if width or height exceeds MaxSize
- If yes: scale down proportionally
- Maintain aspect ratio
- Reduce memory usage for UI display

**WebP Support:**
- Automatic detection of WebP format
- Uses Unity WebP library for decoding
- Converts to Texture2D
- Same caching as other formats

---

## Performance Optimizations

### 1. Parallel Loading

**During Initialization:**
```csharp
await UniTask.WhenAll(
    _userDataService.GetBalance(false), 
    _userDataService.GetUserData(false)
);
```

**Benefits:**
- Both requests run simultaneously
- Reduces initialization time by ~50%
- Uses UniTask for efficient async

### 2. Lazy Initialization

**SDK services:**
- Only created when Initialize() is called
- Not created on application start
- Reduces startup overhead

### 3. Request Batching

**Vouchers loading:**
```csharp
// Load static data once
if (_vouchersStaticData == null)
{
    var staticData = await HTTPClient.Get<VoucherStaticDataResponse>();
    _vouchersStaticData = staticData.Data;
}

// Load dynamic data
var dynamicData = await HTTPClient.Get<VoucherResponse>();

// Merge in memory
ProcessVouchersResponse(dynamicData);
```

### 4. Memory Management

**Asset caching with limits:**
- LimitedSizeDictionary prevents unlimited growth
- FIFO eviction strategy
- Configurable cache size
- Automatic cleanup of old assets

### 5. Network Efficiency

**Conditional requests:**
- useCache parameter prevents unnecessary API calls
- Cached data returned instantly
- Fresh data only when needed

---

## Error Handling Strategy

### Error Handler Interface

```csharp
public interface IErrorHandler
{
    void ShowError(
        string errorMessage, 
        string errorTitle = "Error", 
        string buttonText = "OK", 
        Action additionalActionOnCloseClick = null
    );
}
```

### Error Propagation

```
HTTP Request Error
   │
   ├─> HTTPClient logs error
   │     └─> Logger.LogError()
   │
   ├─> Return (false, default)
   │
   ├─> Service layer checks result
   │     └─> if (!success) return null
   │
   ├─> SDK method returns null/false
   │
   └─> Application handles null result
```

### Common Error Scenarios

**1. Authentication Failed:**
- HTTPClient returns (false, null)
- Initialize() fails
- Application should retry or show login

**2. Insufficient Balance:**
- CanAfford returns false
- JoinRoom validates and shows error via IErrorHandler
- User sees "Not enough currency" popup

**3. Network Timeout:**
- UnityWebRequest times out
- HTTPClient logs error
- Returns (false, default)
- Application can retry

**4. Invalid Session:**
- API returns 401 Unauthorized
- HTTPClient logs error
- May need re-authentication

---

## Testing Considerations

### Unit Testing

**Testable components:**
- `UserDataService.CanAffordFromCache()` - pure logic
- `TextureResizerUtility` - deterministic
- Encryption/decryption - reversible

**Mockable interfaces:**
- `IUserDataService`
- `IAuthDataProvider`
- `IRoomsProvider`
- `IClientSessionManager`
- `IServerSessionManager`
- `IErrorHandler`

### Integration Testing

**Test scenarios:**
1. Full initialization flow
2. Room join → play → finish flow
3. Session reconnection flow
4. Balance validation
5. Error handling

---

## Best Practices for SDK Users

### 1. Initialize Early
```csharp
// ✅ Good: Initialize in main menu
private async void Awake()
{
    await FunticoSDK.Instance.Initialize(...);
}

// ❌ Bad: Initialize before each operation
private async void JoinRoom()
{
    await FunticoSDK.Instance.Initialize(...);
    await FunticoSDK.Instance.JoinRoom(...);
}
```

### 2. Use Caching Appropriately
```csharp
// ✅ Good: Use cached data for UI
private void UpdateUI()
{
    var balance = FunticoSDK.Instance.GetCachedBalance();
    coinsText.text = balance.Coins.ToString();
}

// ❌ Bad: Fetch fresh data every frame
private async void Update()
{
    var balance = await FunticoSDK.Instance.GetBalance(useCache: false);
}
```

### 3. Handle Errors Gracefully
```csharp
// ✅ Good: Check for null and handle
var roomData = await FunticoSDK.Instance.JoinRoom(guid);
if (roomData == null)
{
    ShowError("Failed to join room");
    return;
}

// ❌ Bad: Assume success
var roomData = await FunticoSDK.Instance.JoinRoom(guid);
StartGame(roomData); // May crash if null
```

### 4. Validate Before Actions
```csharp
// ✅ Good: Check affordability first
if (!FunticoSDK.Instance.CanAffordFromCache(feeType, amount))
{
    ShowShop();
    return;
}

await JoinRoom();

// ❌ Bad: Try to join without checking
await JoinRoom(); // Will fail with error
```

---

## Conclusion

The Funtico SDK is built with a modular, extensible architecture that prioritizes:

- **Security**: Client-side encryption, server-side signing
- **Performance**: Caching, parallel loading, lazy initialization
- **Maintainability**: Partial classes, dependency injection, interface-based design
- **Reliability**: Comprehensive error handling, session recovery
- **Ease of Use**: Facade pattern, clear API, automatic setup

For usage examples and API reference, see:
- [README](./README.md) - Installation and quick start
- [USAGE](./USAGE.md) - Complete API reference with examples

---

**Questions or Issues?**

Contact Funtico Games technical support or open an issue on GitHub.

