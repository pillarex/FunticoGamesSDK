# Funtico SDK - Usage Guide

Complete reference guide for all public methods in Funtico SDK.

## Table of Contents

1. [Initialization](#initialization)
2. [Authentication Methods](#authentication-methods)
3. [User Data Methods](#user-data-methods)
4. [Rooms & Tournament Methods](#rooms--tournament-methods)
5. [Client Session Methods](#client-session-methods)
6. [Server Session Methods](#server-session-methods)
7. [Usage Examples](#usage-examples)

---

## Initialization

### `Initialize`

Initializes the SDK with required configuration. Must be called before using any other SDK methods.

**Signature:**
```csharp
public async UniTask Initialize(
    Environment env, 
    string privateGameKey, 
    string publicGameKey, 
    string userToken, 
    IErrorHandler errorHandler)
```

**Parameters:**
- `env` - Environment to use (`Environment.STAGING` for development, `Environment.PROD` for production)
- `privateGameKey` - Private game key obtained from Funtico developer dashboard
- `publicGameKey` - Public game key obtained from Funtico developer dashboard
- `userToken` - User authentication token from Funtico platform
- `errorHandler` - Implementation of `IErrorHandler` interface for handling SDK errors

**Returns:** `UniTask` - Completes when initialization is finished

**Description:**
Initializes all SDK services including authentication, user data management, session management, and rooms provider. Automatically authenticates the user and pre-loads user balance and profile data (on client builds).

**Usage:**
```csharp
await FunticoSDK.Instance.Initialize(
    FunticoSDK.Environment.STAGING,
    "your-private-key",
    "your-public-key",
    "user-platform-token",
    new MyErrorHandler()
);
```

**Important Notes:**
- Must be called before any other SDK method
- Should only be called once during application lifetime
- On client builds, automatically pre-loads user data and balance
- On server builds (SERVER or UNITY_SERVER defines), skips automatic authentication

---

## Authentication Methods

### `Authentication`

Re-authenticates user with a new platform token.

**Signature:**
```csharp
public UniTask<LoginResponse> Authentication(string platformToken)
```

**Parameters:**
- `platformToken` - Platform authentication token

**Returns:** `UniTask<LoginResponse>` - Login response containing authentication details

**Description:**
Manually authenticates or re-authenticates a user with the platform. This is automatically called during initialization, but can be used if token refresh is needed.

**Usage:**
```csharp
LoginResponse response = await FunticoSDK.Instance.Authentication("new-token");
if (response != null)
{
    Debug.Log("Authentication successful");
}
```

---

### `GetUserToken`

Gets the current user authentication token.

**Signature:**
```csharp
public string GetUserToken()
```

**Returns:** `string` - Current user token

**Description:**
Returns the internal user token that was generated after authentication. This is different from the platform token and is used for API requests.

**Usage:**
```csharp
string userToken = FunticoSDK.Instance.GetUserToken();
```

---

### `GetPlatformToken`

Gets the platform authentication token that was provided during initialization.

**Signature:**
```csharp
public string GetPlatformToken()
```

**Returns:** `string` - Platform token

**Description:**
Returns the original platform token that was passed during initialization or latest authentication call.

**Usage:**
```csharp
string platformToken = FunticoSDK.Instance.GetPlatformToken();
```

---

## User Data Methods

### `GetUserData`

Gets user profile data including name, level, experience, currency, and preferences.

**Signature:**
```csharp
public UniTask<UserData> GetUserData(bool useCache = true)
```

**Parameters:**
- `useCache` - If `true`, returns cached data if available; if `false`, always fetches fresh data from server

**Returns:** `UniTask<UserData>` - User profile data

**Description:**
Retrieves complete user profile information. On first call or when `useCache=false`, fetches from server and caches the result. Subsequent calls with `useCache=true` return cached data instantly.

**UserData Properties:**
- `Name` (string) - Username
- `UserId` (int) - Unique user identifier in SDK system (same as `SdkUserId` in server session models)
- `PlatformId` (int) - Unique user identifier in Funtico platform (same as `FunticoUserId` in server session models, used as `platformUserId` parameter in server methods)
- `Level` (int) - Player level
- `Rating` (int) - Player rating
- `Experience` (int) - Current experience points
- `ExperienceToLevel` (int) - Experience needed for next level
- `Coins` (double) - In-game coins balance
- `Diamonds` (double) - Premium currency balance
- `SemiFinalTickets` (uint) - Semi-final tickets count
- `FinalTickets` (uint) - Final tickets count
- `PrivateTickets` (uint) - Private room tickets count
- `KYCVerified` (bool) - KYC verification status
- `SoundsValue` (float) - Sound volume preference
- `MusicValue` (float) - Music volume preference

**Usage:**
```csharp
// Fetch from server
UserData userData = await FunticoSDK.Instance.GetUserData(useCache: false);

// Use cached data
UserData cachedUser = await FunticoSDK.Instance.GetUserData(useCache: true);
```

---

### `GetCachedUserData`

Gets cached user data instantly without any network call.

**Signature:**
```csharp
public UserData GetCachedUserData()
```

**Returns:** `UserData` - Cached user profile data (or null if not cached)

**Description:**
Returns previously cached user data instantly. Returns `null` if no data has been cached yet. Use this for frequent UI updates to avoid unnecessary network calls.

**Usage:**
```csharp
UserData userData = FunticoSDK.Instance.GetCachedUserData();
if (userData != null)
{
    usernameText.text = userData.Name;
    levelText.text = $"Level {userData.Level}";
}
```

---

### `GetBalance`

Gets user balance including all currency types and tickets.

**Signature:**
```csharp
public UniTask<BalanceResponse> GetBalance(bool useCache = true)
```

**Parameters:**
- `useCache` - If `true`, returns cached balance; if `false`, fetches fresh balance from server

**Returns:** `UniTask<BalanceResponse>` - User balance information

**Description:**
Retrieves user balance for all currency types. On first call or when `useCache=false`, fetches from server and caches the result.

**BalanceResponse Properties:**
- `Coins` (double) - In-game coins
- `Diamonds` (double) - Premium currency
- `SemiFinalTickets` (uint) - Semi-final tickets
- `FinalTickets` (uint) - Final tickets
- `PrivateTickets` (uint) - Private tickets
- `KYCVerified` (bool) - KYC verification status

**Usage:**
```csharp
// Fetch fresh balance
BalanceResponse balance = await FunticoSDK.Instance.GetBalance(useCache: false);

// Use cached balance
BalanceResponse cachedBalance = await FunticoSDK.Instance.GetBalance(useCache: true);
```

---

### `GetCachedBalance`

Gets cached balance instantly without any network call.

**Signature:**
```csharp
public BalanceResponse GetCachedBalance()
```

**Returns:** `BalanceResponse` - Cached balance data (or null if not cached)

**Description:**
Returns previously cached balance instantly. Returns `null` if no balance has been cached yet. Ideal for UI updates.

**Usage:**
```csharp
BalanceResponse balance = FunticoSDK.Instance.GetCachedBalance();
if (balance != null)
{
    coinsText.text = balance.Coins.ToString();
    diamondsText.text = balance.Diamonds.ToString();
}
```

---

### `GetVouchers`

Gets user vouchers that can be used for free room entry.

**Signature:**
```csharp
public UniTask<List<VoucherData>> GetVouchers(bool useCache = true)
```

**Parameters:**
- `useCache` - If `true`, returns cached vouchers; if `false`, fetches fresh data from server

**Returns:** `UniTask<List<VoucherData>>` - List of user vouchers

**Description:**
Retrieves all vouchers owned by the user. Vouchers allow free entry to tournament rooms of specific tiers.

**VoucherData Properties:**
- `ItemId` (int) - Voucher item identifier
- `ItemName` (string) - Voucher name
- `ItemImage` (string) - URL to voucher image
- `Tier` (int) - Associated room tier (0=Contender, 1=Challenger, 2=Champion)
- `Count` (int) - Number of available vouchers
- `PlayCount` (int) - Current play count progress
- `PlaysRequiredToActivate` (int) - Plays needed to activate one voucher

**Usage:**
```csharp
List<VoucherData> vouchers = await FunticoSDK.Instance.GetVouchers(useCache: true);
foreach (var voucher in vouchers)
{
    Debug.Log($"{voucher.ItemName}: {voucher.Count} available");
    Debug.Log($"Progress: {voucher.PlayCount}/{voucher.PlaysRequiredToActivate}");
}
```

---

### `GetCachedVouchers`

Gets cached vouchers instantly without any network call.

**Signature:**
```csharp
public List<VoucherData> GetCachedVouchers()
```

**Returns:** `List<VoucherData>` - Cached vouchers list (or null if not cached)

**Description:**
Returns previously cached vouchers instantly. Returns `null` if no vouchers have been cached yet.

**Usage:**
```csharp
List<VoucherData> vouchers = FunticoSDK.Instance.GetCachedVouchers();
if (vouchers != null)
{
    DisplayVouchers(vouchers);
}
```

---

### `CanAfford`

Checks if user can afford a specific entry fee (with server call).

**Signature:**
```csharp
public UniTask<bool> CanAfford(EntryFeeType type, int amount)
```

**Parameters:**
- `type` - Type of currency/ticket required (see EntryFeeType enum)
- `amount` - Amount required

**Returns:** `UniTask<bool>` - `true` if user can afford, `false` otherwise

**Description:**
Checks user balance against required amount. Fetches fresh balance from server before checking. Use this when you need to verify affordability with up-to-date balance.

**EntryFeeType Enum:**
- `IC = 1` - In-game currency
- `Tico = 2` - Platform currency (TICO)
- `SemifinalsTickets = 3` - Semi-final tickets
- `FinalTickets = 4` - Final tickets
- `Free = 5` - Free entry (always returns true)
- `InventoryItem = 6` - Inventory item
- `PrivateTickets = 7` - Private tickets

**Usage:**
```csharp
bool canAfford = await FunticoSDK.Instance.CanAfford(EntryFeeType.Tico, 1000);
if (!canAfford)
{
    Debug.Log("Not enough TICO!");
}
```

---

### `CanAffordFromCache`

Checks if user can afford entry fee using cached balance (instant).

**Signature:**
```csharp
public bool CanAffordFromCache(EntryFeeType type, int amount)
```

**Parameters:**
- `type` - Type of currency/ticket required
- `amount` - Amount required

**Returns:** `bool` - `true` if user can afford based on cached balance, `false` otherwise

**Description:**
Instantly checks affordability using cached balance data. No network call. Returns `false` if balance not cached yet.

**Usage:**
```csharp
bool canAfford = FunticoSDK.Instance.CanAffordFromCache(EntryFeeType.Tico, 500);
```

---

## Rooms & Tournament Methods

### `GetTiers`

Gets all available tournament tiers.

**Signature:**
```csharp
public UniTask<List<TierViewModel>> GetTiers()
```

**Returns:** `UniTask<List<TierViewModel>>` - List of available tiers

**Description:**
Retrieves all tournament tiers (Contender, Challenger, Champion) with their details and images.

**TierViewModel Properties:**
- `TierName` (string) - Display name of tier
- `Tier` (RoomTierEnum) - Tier enum value (0=Contender, 1=Challenger, 2=Champion)
- `EntryFeeLowerBound` (int) - Minimum entry fee for this tier
- `EntryFeeUpperBound` (int) - Maximum entry fee for this tier
- `TierImage` (Task<Sprite>) - Async task to load tier image
- `Hidden` (bool) - Whether tier should be hidden from UI

**Usage:**
```csharp
List<TierViewModel> tiers = await FunticoSDK.Instance.GetTiers();
foreach (var tier in tiers)
{
    if (!tier.Hidden)
    {
        Debug.Log($"Tier: {tier.TierName}");
        Debug.Log($"Entry Fee Range: {tier.EntryFeeLowerBound} - {tier.EntryFeeUpperBound}");
        
        // Load tier image
        Sprite tierSprite = await tier.TierImage;
    }
}
```

---

### `GetRooms`

Gets available tournament rooms, optionally filtered by tier.

**Signature:**
```csharp
public UniTask<List<RoomViewModel>> GetRooms(RoomTierEnum? tier)
```

**Parameters:**
- `tier` - Optional tier filter. Pass `null` to get all rooms, or specific tier (Contender/Challenger/Champion)

**Returns:** `UniTask<List<RoomViewModel>>` - List of available rooms

**Description:**
Retrieves list of available tournament rooms. Can be filtered by tier or retrieve all rooms.

**RoomViewModel Properties:**
- `Guid` (string) - Unique room identifier
- `Name` (string) - Room display name
- `Tier` (RoomTierEnum) - Room tier
- `EntryFee` (ulong) - Entry fee amount
- `EntryFeeUSDT` (float) - Approximate fee in USDT
- `TotalPrize` (ulong) - Total prize pool
- `IsFree` (bool) - Whether room is free to enter
- `RequireKyc` (bool) - Whether KYC verification is required
- `UserCanJoinWithVoucher` (bool) - Whether user has voucher for this room
- `Voucher` (VoucherData) - Associated voucher if available
- `Ticket` (Ticket) - Entry ticket details
- `FeeIcon` (Sprite) - Currency icon sprite
- `LoadRoomImageTask` (Func<Task<Sprite>>) - Function to load room image
- `IsPrePaid` (bool) - Whether room is pre-paid

**Usage:**
```csharp
// Get all rooms
List<RoomViewModel> allRooms = await FunticoSDK.Instance.GetRooms(null);

// Get only Champion tier rooms
List<RoomViewModel> championRooms = await FunticoSDK.Instance.GetRooms(RoomTierEnum.Champion);

foreach (var room in championRooms)
{
    Debug.Log($"{room.Name} - Entry: {room.EntryFee}, Prize: {room.TotalPrize}");
    
    if (room.RequireKyc)
        Debug.Log("KYC required for this room");
}
```

---

### `GetRoom`

Gets detailed information about a specific room.

**Signature:**
```csharp
public UniTask<RoomViewModel> GetRoom(string guid)
```

**Parameters:**
- `guid` - Room unique identifier

**Returns:** `UniTask<RoomViewModel>` - Room details

**Description:**
Retrieves detailed information about a specific room including entry requirements, prize pool, and user eligibility.

**Usage:**
```csharp
RoomViewModel room = await FunticoSDK.Instance.GetRoom("room-guid-here");

Debug.Log($"Room: {room.Name}");
Debug.Log($"Entry Fee: {room.EntryFee}");
Debug.Log($"Prize Pool: {room.TotalPrize}");
Debug.Log($"Can use voucher: {room.UserCanJoinWithVoucher}");

// Load room cover image
if (room.LoadRoomImageTask != null)
{
    Sprite roomImage = await room.LoadRoomImageTask();
    coverImage.sprite = roomImage;
}
```

---

### `GetRoomSettings`

Gets custom settings/configuration for a specific room.

**Signature:**
```csharp
public UniTask<string> GetRoomSettings(string guid)
```

**Parameters:**
- `guid` - Room unique identifier

**Returns:** `UniTask<string>` - JSON string containing room settings

**Description:**
Retrieves custom game settings configured for the room. The structure of returned JSON depends on your game's configuration.

**Usage:**
```csharp
string settingsJson = await FunticoSDK.Instance.GetRoomSettings("room-guid");

// Parse based on your game's settings structure
var settings = JsonConvert.DeserializeObject<MyGameSettings>(settingsJson);

Debug.Log($"Difficulty: {settings.difficulty}");
Debug.Log($"Time Limit: {settings.timeLimit}");
```

---

### `JoinRoom`

Joins a tournament room and creates a game session.

**Signature:**
```csharp
public UniTask<RoomData> JoinRoom(string roomGuid)
```

**Parameters:**
- `roomGuid` - Room unique identifier to join

**Returns:** `UniTask<RoomData>` - Room session data containing event ID and session ID (or null if join failed)

**Description:**
Attempts to join a tournament room. Automatically handles entry fee payment, voucher usage, and session creation. Returns room data needed to start the game session.

**RoomData Properties:**
- `EventId` (string) - Event/Room identifier
- `SessionOrMatchId` (string) - Session or match identifier for this game instance

**Usage:**
```csharp
RoomViewModel room = await FunticoSDK.Instance.GetRoom(roomGuid);

if (!room.IsFree)
{
    bool canAfford = FunticoSDK.Instance.CanAffordFromCache(
        room.Ticket.CurrencyType.ToEntryFeeType(),
        (int)room.EntryFee
    );
    
    if (!canAfford && !room.UserCanJoinWithVoucher)
    {
        Debug.Log("Cannot afford entry!");
        return;
    }
}

// Join the room
RoomData roomData = await FunticoSDK.Instance.JoinRoom(roomGuid);

if (roomData != null)
{
    Debug.Log($"Joined! Event ID: {roomData.EventId}");
    Debug.Log($"Session ID: {roomData.SessionOrMatchId}");
    
    // Start your game with these IDs
    StartGame(roomData);
}
else
{
    Debug.Log("Failed to join room");
}
```

**Important Notes:**
- Doesn't create game session, so you need to create it manually (because of additional data which required to start session)
- Automatically deducts entry fee from user balance
- Uses voucher if available
- Returns null if join fails (insufficient funds, room full, etc.)
- Error details shown via IErrorHandler

---

### `GetPrizePoolDistribution`

Gets prize pool distribution details for a room.

**Signature:**
```csharp
public UniTask<PrizePoolDistibutionViewModel> GetPrizePoolDistribution(string roomGuid)
```

**Parameters:**
- `roomGuid` - Room unique identifier

**Returns:** `UniTask<PrizePoolDistibutionViewModel>` - Prize distribution information

**Description:**
Retrieves detailed information about how prizes are distributed among winners in the room.

**PrizePoolDistibutionViewModel Properties:**
- `PrizePlaces` (List<PrizePlaceViewModel>) - Prize amounts for each place
- `TotalTicoAccumulated` (long) - Total TICO in prize pool
- `TotalPlayers` (int) - Total number of players
- `TicoPerPlayer` (long) - Average TICO per player
- `PlatformFeePercentage` (float) - Platform fee percentage
- `PlatformFee` (long) - Platform fee amount
- `TotalTicoDistributedToPlayers` (long) - Total TICO distributed to winners

**Usage:**
```csharp
var prizePool = await FunticoSDK.Instance.GetPrizePoolDistribution(roomGuid);

Debug.Log($"Total Prize Pool: {prizePool.TotalTicoDistributedToPlayers} TICO");
Debug.Log($"Platform Fee: {prizePool.PlatformFee} ({prizePool.PlatformFeePercentage}%)");
Debug.Log($"Players: {prizePool.TotalPlayers}");

// Display prize for each place
foreach (var place in prizePool.PrizePlaces)
{
    Debug.Log($"Place {place.Place}: {place.Prize} TICO");
}
```

---

### `GetLeaderboard`

Gets leaderboard/results for a completed or ongoing game session.

**Signature:**
```csharp
public UniTask<RoomLeaderboardViewModel> GetLeaderboard(
    string eventId, 
    string sessionId, 
    string matchId)
```

**Parameters:**
- `eventId` - Event/Room identifier
- `sessionId` - Session identifier
- `matchId` - Match identifier (can be null for single-player)

**Returns:** `UniTask<RoomLeaderboardViewModel>` - Leaderboard with results

**Description:**
Retrieves leaderboard showing player rankings, scores, and rewards for a game session.

**RoomLeaderboardViewModel Properties:**
- `State` (LeadersState) - Current leaderboard state (placed/distributed/returned/to_be_returned)
- `IsPending` (bool) - Whether results are still pending
- `ClosedUnsuccessfully` (bool) - Whether game was closed unsuccessfully
- `RoomName` (string) - Room name
- `RoomTier` (RoomTierEnum) - Room tier
- `YourFee` (Ticket) - Entry fee user paid
- `OriginalFee` (Ticket) - Original entry fee
- `FeeIcon` (Sprite) - Fee currency icon
- `IsFree` (bool) - Whether entry was free
- `YourTicoEarnings` (long) - User's TICO earnings
- `Multiplier` (float) - Win multiplier
- `UserPlace` (string) - User's placement (e.g., "1st", "2nd")
- `YourRewardsFirstRow` (List<RoomPrizeViewModel>) - User rewards (first row)
- `YourRewardsSecondRow` (List<RoomPrizeViewModel>) - User rewards (second row)
- `LeaderboardItems` (List<LeaderboardItemViewModel>) - All player rankings

**Usage:**
```csharp
var leaderboard = await FunticoSDK.Instance.GetLeaderboard(
    roomData.EventId, 
    roomData.SessionOrMatchId, 
    null
);

Debug.Log($"Your Place: {leaderboard.UserPlace}");
Debug.Log($"Your Earnings: {leaderboard.YourTicoEarnings} TICO");
Debug.Log($"Multiplier: x{leaderboard.Multiplier}");

if (leaderboard.IsPending)
{
    Debug.Log("Results still being processed...");
}

// Display all rankings
foreach (var item in leaderboard.LeaderboardItems)
{
    Debug.Log($"{item.Place}. {item.PlayerName} - {item.Score} points");
}
```

---

### `GetTierByEventId`

Gets the tier of a specific room/event.

**Signature:**
```csharp
public UniTask<RoomTierEnum?> GetTierByEventId(string eventId)
```

**Parameters:**
- `eventId` - Event/Room identifier

**Returns:** `UniTask<RoomTierEnum?>` - Tier enum or null if not found

**Description:**
Retrieves the tier classification for a specific room. Useful when you have event ID but need tier information.

**Usage:**
```csharp
RoomTierEnum? tier = await FunticoSDK.Instance.GetTierByEventId(eventId);

if (tier.HasValue)
{
    switch (tier.Value)
    {
        case RoomTierEnum.Contender:
            Debug.Log("Beginner tier");
            break;
        case RoomTierEnum.Challenger:
            Debug.Log("Intermediate tier");
            break;
        case RoomTierEnum.Champion:
            Debug.Log("Advanced tier");
            break;
    }
}
```

---

### `FinishRoomSession_Client`

Finishes a game session and submits final score (client-side).

**Signature:**
```csharp
public UniTask<bool> FinishRoomSession_Client(
    string eventId, 
    string sessionId, 
    int score)
```

**Parameters:**
- `eventId` - Event/Room identifier
- `sessionId` - Session identifier
- `score` - Final game score

**Returns:** `UniTask<bool>` - `true` if successfully finished, `false` otherwise

**Description:**
Completes a game session and submits the final score from the client. Use this in games which don't have servers which can verify progress and result.

**Usage:**
```csharp
public async void OnGameComplete(int finalScore)
{
    bool success = await FunticoSDK.Instance.FinishRoomSession_Client(
        roomData.EventId,
        roomData.SessionOrMatchId,
        finalScore
    );
    
    if (success)
    {
        Debug.Log("Score submitted successfully!");
        
        // Show leaderboard
        var leaderboard = await FunticoSDK.Instance.GetLeaderboard(
            roomData.EventId,
            roomData.SessionOrMatchId,
            null
        );
        DisplayResults(leaderboard);
    }
    else
    {
        Debug.Log("Failed to submit score");
    }
}
```

**Important Notes:**
- Only use in client builds (not on dedicated servers)
- Score must be final - cannot be changed after submission
- Automatically closes the session

---

### `FinishRoomSession_Server`

Finishes a game session and submits score from server (server-side).

**Signature:**
```csharp
public UniTask<bool> FinishRoomSession_Server(
    string eventId, 
    string sessionId, 
    int score, 
    int userId, 
    int funticoId,
    string userIp)
```

**Parameters:**
- `eventId` - Event/Room identifier
- `sessionId` - Session identifier
- `score` - Final game score
- `userId` - User identifier in SDK system (same as `UserData.UserId`)
- `funticoId` - User identifier in Funtico platform (same as `UserData.PlatformId`). Used internally to retrieve session events for the user
- `userIp` - User IP address (for security/validation)

**Returns:** `UniTask<bool>` - `true` if successfully finished, `false` otherwise

**Description:**
Completes a game session from a dedicated server. You don't need to additionally call FinishRoomSession_Client on clients if you use this method. The server session is closed automatically (fire-and-forget) after submission.

**Usage:**
```csharp
#if SERVER || UNITY_SERVER
public async void OnPlayerFinishGame(int userId, int funticoId, string userIp, int finalScore)
{
    bool success = await FunticoSDK.Instance.FinishRoomSession_Server(
        eventId,
        sessionId,
        finalScore,
        userId,
        funticoId,
        userIp
    );
    
    if (success)
    {
        Debug.Log($"Score submitted for user {userId}");
    }
}
#endif
```

**Important Notes:**
- Only use in server builds (with SERVER or UNITY_SERVER defines)
- Requires valid user IP for anti-cheat validation
- Cannot be called from client builds

---

### `FinishRoomSession_Server` (multiplayer overload)

Finishes a multiplayer game session and submits results for all participants at once (server-side).

**Signature:**
```csharp
public UniTask<bool> FinishRoomSession_Server(
    string eventId, 
    string sessionId, 
    List<FinishedUser> participants)
```

**Parameters:**
- `eventId` - Event/Room identifier
- `sessionId` - Session/match identifier
- `participants` - List of `FinishedUser` with results for each player

**Returns:** `UniTask<bool>` - `true` if results successfully submitted, `false` otherwise

**Description:**
Completes a multiplayer game session from a dedicated server. Submits scores and additional data for all participants in a single request. Game events recorded via `RecordEvent_Server` are automatically attached to each player's results. After submission, the server session is closed automatically.

**FinishedUser Properties:**
| Property | Type | Description |
|----------|------|-------------|
| `Score` | `int` | Player's final score |
| `UserId` | `int` | User identifier in SDK system (same as `UserData.UserId`) |
| `FunticoUserId` | `int` | User identifier in Funtico platform (same as `UserData.PlatformId`) |
| `UserIp` | `string` | User IP address (for validation) |
| `AdditionalData` | `string` | Optional additional data (JSON string) |
| `GameEvents` | `List<string>` | Game events (auto-filled from session logs) |

**Usage:**
```csharp
#if SERVER || UNITY_SERVER
public async void OnMatchComplete(List<PlayerResult> results)
{
    var participants = results.Select(r => new FinishedUser
    {
        Score = r.Score,
        UserId = r.UserId,
        UserIp = r.IpAddress,
        AdditionalData = JsonConvert.SerializeObject(new { kills = r.Kills, deaths = r.Deaths })
    }).ToList();

    bool success = await FunticoSDK.Instance.FinishRoomSession_Server(
        eventId,
        sessionId,
        participants
    );
    
    if (success)
    {
        Debug.Log("Match results submitted for all players");
    }
}
#endif
```

**Important Notes:**
- Only use in server builds (with SERVER or UNITY_SERVER defines)
- `GameEvents` for each player are automatically filled from events recorded via `RecordEvent_Server` — you don't need to set them manually
- Automatically closes the server session after submission (fire-and-forget)

---

## Client Session Methods

Use these methods in **client builds** for session management and reconnection.

### `UserHasUnfinishedSession_Client`

Checks if user has unfinished game sessions.

**Signature:**
```csharp
public UniTask<UnfinishedSessionsResponse> UserHasUnfinishedSession_Client()
```

**Returns:** `UniTask<UnfinishedSessionsResponse>` - Response containing list of unfinished sessions

**Description:**
Retrieves all interrupted or unfinished game sessions for the current user. Use this at game startup to offer session resumption. User may have multiple unfinished sessions if they started multiple games without finishing them.

**UnfinishedSessionsResponse Properties:**
- `SavedSessions` (List<SavedSessionResponse>) - List of unfinished sessions

**SavedSessionResponse Properties:**
- `Id` (string) - Unique session identifier
- `SessionId` (string) - Event/Room identifier
- `SaveSessionId` (string) - Match/Session identifier
- `GameType` (int) - Type of game (see GameTypeEnum)
- `ReconnectTime` (float) - Time remaining to reconnect
- `Data` (string) - Encrypted session data
- `Hash` (string) - Data integrity hash

**Usage:**
```csharp
UnfinishedSessionsResponse response = await FunticoSDK.Instance.UserHasUnfinishedSession_Client();

if (response != null && response.SavedSessions.Count > 0)
{
    Debug.Log($"Found {response.SavedSessions.Count} unfinished session(s)");
    
    // Show list to user or auto-reconnect to first
    var firstSession = response.SavedSessions[0];
    Debug.Log($"Session ID: {firstSession.Id}");
    Debug.Log($"Room ID: {firstSession.SessionId}");
    Debug.Log($"Time to reconnect: {firstSession.ReconnectTime}s");
}
```

---

### `ReconnectToUnfinishedSession_Client`

Reconnects to a specific unfinished session and retrieves session data.

**Signature:**
```csharp
public UniTask<string> ReconnectToUnfinishedSession_Client(string id)
```

**Parameters:**
- `id` - Unique session identifier (from UnfinishedSessionsResponse)

**Returns:** `UniTask<string>` - Decrypted JSON string containing session data (or null if failed)

**Description:**
Retrieves and decrypts the stored data from a specific unfinished session, allowing the user to resume gameplay from where they left off. The session ID must be obtained from `UserHasUnfinishedSession_Client()` response.

**Usage:**
```csharp
// First, get list of unfinished sessions
var sessionsResponse = await FunticoSDK.Instance.UserHasUnfinishedSession_Client();

if (sessionsResponse != null && sessionsResponse.SavedSessions.Count > 0)
{
    // Select session to reconnect (e.g., first one or let user choose)
    var session = sessionsResponse.SavedSessions[0];
    
    // Reconnect to selected session
    string sessionData = await FunticoSDK.Instance.ReconnectToUnfinishedSession_Client(session.Id);
    
    if (sessionData != null)
    {
        // Parse session data based on your game's structure
        var savedState = JsonConvert.DeserializeObject<GameState>(sessionData);
        
        // Restore room data
        currentRoomData = new RoomData()
        {
            EventId = session.SessionId,
            SessionOrMatchId = session.SaveSessionId
        };
        
        // Restore game state
        RestoreGame(savedState);
    }
}
```

---

### `CreateSession_Client`

Creates a new game session with custom data, most likely you would want to call it right after joining room, so in case when user can't successfully load scene, for example, he still will be able to reconnect.

**Signature:**
```csharp
public UniTask<SavedSessionResponse> CreateSession_Client(
    string json, 
    GameTypeEnum gameType, 
    string eventId, 
    string saveSessionId)
```

**Parameters:**
- `json` - JSON string containing custom session data
- `gameType` - Type of game session (see GameTypeEnum)
- `eventId` - Event/Room identifier (from RoomData.EventId)
- `saveSessionId` - Match/Session identifier (from RoomData.SessionOrMatchId)

**Returns:** `UniTask<SavedSessionResponse>` - Created session details (or null if failed)

**Description:**
Creates a new game session and stores encrypted custom data. This data can be retrieved if the session needs to be resumed later. The session is automatically tracked for reconnection.

**GameTypeEnum Values:**
- `Indirect_PVP = 0` - Asynchronous player vs player
- `Direct_PVP = 1` - Real-time player vs player
- `Tournament = 3` - Tournament mode
- `Rooms = 4` - Room/lobby based games
- `Practice = 5` - Practice/training mode
- `MultiplayerPvp = 6` - Real-time multiplayer PvP (server-authoritative)

**SavedSessionResponse Properties:**
- `Id` (string) - Unique session identifier
- `SessionId` (string) - Event/Room identifier
- `SaveSessionId` (string) - Match/Session identifier
- `GameType` (int) - Type of game
- `Data` (string) - Encrypted session data
- `Hash` (string) - Data integrity hash
- `ReconnectTime` (float) - Time available for reconnection

**Usage:**
```csharp
// After joining a room
RoomData roomData = await FunticoSDK.Instance.JoinRoom(roomGuid);

// Prepare session data
var sessionData = new 
{
    roomId = roomData.EventId,
    sessionId = roomData.SessionOrMatchId,
    startTime = DateTime.UtcNow,
    difficulty = "normal",
    customSettings = gameSettings
};

string json = JsonConvert.SerializeObject(sessionData);

// Create session
SavedSessionResponse sessionResponse = await FunticoSDK.Instance.CreateSession_Client(
    json,
    GameTypeEnum.Rooms,
    roomData.EventId,
    roomData.SessionOrMatchId
);

if (sessionResponse != null)
{
    Debug.Log($"Session created! ID: {sessionResponse.Id}");
    StartGame();
}
else
{
    Debug.LogError("Failed to create session");
}
```

**Important Notes:**
- Call after joining a room but before starting gameplay
- Store any data needed for session resumption
- Data is automatically encrypted using user-specific key
- Automatically tracked for reconnection
- Must provide eventId and saveSessionId from RoomData

---

### `UpdateSession_Client`

Updates an existing session with new data. You may want to use it if you want to have a reconnection feature.

**Signature:**
```csharp
public UniTask<bool> UpdateSession_Client(string json)
```

**Parameters:**
- `json` - JSON string containing updated session data

**Returns:** `UniTask<bool>` - `true` if session updated successfully, `false` otherwise

**Description:**
Updates the current session with new data. Use this to save game progress periodically or at checkpoints.

**Usage:**
```csharp
var updateData = new 
{
    currentScore = score,
    level = currentLevel,
    checkpoint = checkpointId,
    timestamp = DateTime.UtcNow
};

string json = JsonConvert.SerializeObject(updateData);
bool updated = await FunticoSDK.Instance.UpdateSession_Client(json);

if (updated)
{
    Debug.Log("Progress saved!");
}
```

**Important Notes:**
- Can be called multiple times during gameplay
- Overwrites previous session data
- Useful for checkpoint saves

---

### `CloseCurrentSession_Client`

⚠️ **This method will be called automatically, so you don't need to use it**

**Signature:**
```csharp
public void CloseCurrentSession_Client()
```

**Description:**
This method is not intended for manual use. Sessions are automatically closed when finishing a room session via `FinishRoomSession_Client`. Calling this manually will log a warning.

---

### `GetCurrentSessionEvents_Client`

Gets all recorded events from the current session.

**Signature:**
```csharp
public List<string> GetCurrentSessionEvents_Client()
```

**Returns:** `List<string>` - List of event JSON strings

**Description:**
Retrieves all events that were recorded during the current session using `RecordEvent_Client`.

**Usage:**
```csharp
List<string> events = FunticoSDK.Instance.GetCurrentSessionEvents_Client();

foreach (string eventJson in events)
{
    var eventData = JsonConvert.DeserializeObject<GameEvent>(eventJson);
    Debug.Log($"Event: {eventData.type} at {eventData.timestamp}");
}
```

---

### `RecordEvent_Client`

Records a custom event during the game session, you need to use it, so we can restore user progression by proccessing those events.

**Signature:**
```csharp
public void RecordEvent_Client(string eventInfo)
```

**Parameters:**
- `eventInfo` - JSON string containing event information

**Description:**
Records a game event that can be retrieved later.

**Usage:**
```csharp
public void OnPowerUpUsed(string powerUpName)
{
    var eventData = new 
    {
        type = "powerup_used",
        powerup = powerUpName,
        timestamp = DateTime.UtcNow
    };
    
    FunticoSDK.Instance.RecordEvent_Client(JsonConvert.SerializeObject(eventData));
}
```

---

## Server Session Methods

Use these methods in **dedicated server builds** (with SERVER or UNITY_SERVER defines).

The server session system uses a **session-based** approach: one session is created for all players in the match. The server manages the session lifecycle — creating it when the match starts, tracking player events, handling player disconnections, and closing the session when the match ends.

### `CreateSession_Server`

Creates a new server-side session for a multiplayer match.

**Signature:**
```csharp
public UniTask<bool> CreateSession_Server(string serverUrl, string sessionId, List<ServerUserData> playersInfo)
```

**Parameters:**
- `serverUrl` - URL of the game server hosting the match
- `sessionId` - Session/match identifier
- `playersInfo` - List of `ServerUserData` with information about all players in the match

**Returns:** `UniTask<bool>` - `true` if created successfully

**Description:**
Creates a new game session on the server for a multiplayer match. Registers all participating players and initializes event logs for each player. The session is identified by a server-assigned ID returned from the API.

**ServerUserData Properties:**
| Property | Type | Description |
|----------|------|-------------|
| `JoinKey` | `string` | Unique key used by the player to join the match |
| `FunticoUserId` | `int` | User ID in Funtico platform (same as `UserData.PlatformId`) |
| `SdkUserId` | `int` | User ID in SDK system (same as `UserData.UserId`) |
| `UserStatus` | `UserGameStatus` | Current game status of the user |

**UserGameStatus Enum:**
| Value | Code | Description |
|-------|------|-------------|
| `Leave` | 0 | Player left the session |
| `InGame` | 1 | Player is currently in game |
| `Finished` | 2 | Player has finished the game |

**Usage:**
```csharp
#if SERVER || UNITY_SERVER
var players = new List<ServerUserData>
{
    new ServerUserData
    {
        JoinKey = "player1-join-key",
        FunticoUserId = 101,
        SdkUserId = 201,
        UserStatus = UserGameStatus.InGame
    },
    new ServerUserData
    {
        JoinKey = "player2-join-key",
        FunticoUserId = 102,
        SdkUserId = 202,
        UserStatus = UserGameStatus.InGame
    }
};

bool created = await FunticoSDK.Instance.CreateSession_Server(
    "wss://game-server.example.com:7777",
    matchId,
    players
);

if (created)
{
    Debug.Log("Server session created for all players");
}
#endif
```

---

### `UserLeaveSession_Server`

Notifies the API that a user has left the current session.

**Signature:**
```csharp
public UniTask<bool> UserLeaveSession_Server(int platformUserId)
```

**Parameters:**
- `platformUserId` - User identifier in Funtico platform (same as `UserData.PlatformId`)

**Returns:** `UniTask<bool>` - `true` if the leave was recorded successfully

**Description:**
Reports that a player has disconnected or left the current game session. This is used to track player presence and handle leave scenario. The session itself remains active for the remaining players.

**Usage:**
```csharp
#if SERVER || UNITY_SERVER
public async void OnPlayerDisconnected(int platformUserId)
{
    bool success = await FunticoSDK.Instance.UserLeaveSession_Server(platformUserId);
    if (success)
    {
        Debug.Log($"Player {platformUserId} leave recorded");
    }
}
#endif
```

---

### `CloseCurrentSession_Server`

Closes the current server session.

**Signature:**
```csharp
public UniTask<bool> CloseCurrentSession_Server()
```

**Returns:** `UniTask<bool>` - `true` if session closed successfully

**Description:**
This method is not intended for manual use. Sessions are automatically closed when finishing a room session via `FinishRoomSession_Server`. Calling this manually will log a warning.

---

### `GetCurrentSessionEvents_Server`

Gets all recorded events for a user's session.

**Signature:**
```csharp
public List<string> GetCurrentSessionEvents_Server(int platformUserId)
```

**Parameters:**
- `platformUserId` - User identifier in Funtico platform (same as `UserData.PlatformId`)

**Returns:** `List<string>` - List of event JSON strings

**Description:**
Retrieves all events recorded for a specific user's session.

**Usage:**
```csharp
#if SERVER || UNITY_SERVER
List<string> events = FunticoSDK.Instance.GetCurrentSessionEvents_Server(platformUserId);
foreach (var eventJson in events)
{
    ProcessEvent(eventJson);
}
#endif
```

---

### `RecordEvent_Server`

Records an event for a user's session on the server.

**Signature:**
```csharp
public void RecordEvent_Server(int platformUserId, string eventInfo)
```

**Parameters:**
- `platformUserId` - User identifier in Funtico platform (same as `UserData.PlatformId`)
- `eventInfo` - JSON string containing event data

**Description:**
Records a game event for a specific user's session on the server.

**Usage:**
```csharp
#if SERVER || UNITY_SERVER
var eventData = new 
{
    type = "player_action",
    action = "jump",
    position = playerPosition,
    timestamp = DateTime.UtcNow
};

FunticoSDK.Instance.RecordEvent_Server(
    platformUserId, 
    JsonConvert.SerializeObject(eventData)
);
#endif
```

---

## Usage Examples

### Complete Game Flow Example

```csharp
using System;
using System.Collections.Generic;
using FunticoGamesSDK;
using FunticoGamesSDK.ViewModels;
using FunticoGamesSDK.RoomsProviders;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.APIModels.UserData;
using Newtonsoft.Json;
using UnityEngine;

public class GameFlowExample : MonoBehaviour
{
    private RoomData currentRoomData;
    private int currentScore = 0;
    
    // ===== 1. INITIALIZATION =====
    private async void Start()
    {
        await InitializeSDK();
        await CheckReconnection();
        await LoadGameData();
    }
    
    private async UniTask InitializeSDK()
    {
        await FunticoSDK.Instance.Initialize(
            FunticoSDK.Environment.STAGING,
            "private-key",
            "public-key",
            GetUserToken(),
            new MyErrorHandler()
        );
        Debug.Log("SDK Initialized!");
    }
    
    // ===== 2. CHECK FOR UNFINISHED SESSIONS =====
    private async UniTask CheckReconnection()
    {
        UnfinishedSessionsResponse sessionsResponse = 
            await FunticoSDK.Instance.UserHasUnfinishedSession_Client();
        
        bool hasUnfinished = sessionsResponse != null && sessionsResponse.SavedSessions.Count > 0;
        
        if (hasUnfinished)
        {
            // Show list of unfinished sessions to user or auto-select first
            var session = sessionsResponse.SavedSessions[0];
            
            bool shouldResume = await ShowResumeDialog(session);
            if (shouldResume)
            {
                string sessionData = await FunticoSDK.Instance.ReconnectToUnfinishedSession_Client(session.Id);
                
                // Restore room data from session
                currentRoomData = new RoomData()
                {
                    EventId = session.SessionId,
                    SessionOrMatchId = session.SaveSessionId
                };
                
                RestoreGameState(sessionData);
                return;
            }
        }
        
        ShowMainMenu();
    }
    
    // ===== 3. LOAD GAME DATA =====
    private async UniTask LoadGameData()
    {
        var userTask = await FunticoSDK.Instance.GetUserData();
        var balanceTask = await FunticoSDK.Instance.GetBalance();
        var roomsTask = await FunticoSDK.Instance.GetRooms(null);
        
        DisplayUserInfo(userTask, balanceTask);
        DisplayAvailableRooms(roomsTask);
    }
    
    // ===== 4. JOIN A ROOM =====
    public async void OnJoinRoomButtonClick(string roomGuid)
    {
        // Get room details
        RoomViewModel room = await FunticoSDK.Instance.GetRoom(roomGuid);
        
        // Validate KYC if required
        if (room.RequireKyc)
        {
            UserData userData = FunticoSDK.Instance.GetCachedUserData();
            if (!userData.KYCVerified)
            {
                ShowError("KYC verification required for this room");
                return;
            }
        }

        // Check affordability
        if (!room.IsFree && !room.UserCanJoinWithVoucher && room.Ticket.Type == TicketType.Currency)
        {
            bool canAfford = FunticoSDK.Instance.CanAffordFromCache(
                EntryFeeType.Tico,
                (int)room.EntryFee
            );
            
            if (!canAfford)
            {
                ShowError("Not enough currency!");
                return;
            }
        }
        
        // Join room
        currentRoomData = await FunticoSDK.Instance.JoinRoom(roomGuid);
        
        if (currentRoomData != null)
        {
            await StartGameSession();
        }
        else
        {
            ShowError("Failed to join room");
        }
    }
    
    // ===== 5. CREATE SESSION =====
    private async UniTask StartGameSession()
    {
        var sessionData = new
        {
            eventId = currentRoomData.EventId,
            sessionId = currentRoomData.SessionOrMatchId,
            startTime = DateTime.UtcNow,
            version = Application.version
        };
        
        SavedSessionResponse sessionResponse = await FunticoSDK.Instance.CreateSession_Client(
            JsonConvert.SerializeObject(sessionData),
            GameTypeEnum.Rooms,
            currentRoomData.EventId,
            currentRoomData.SessionOrMatchId
        );
        
        if (sessionResponse != null)
        {
            Debug.Log($"Session created! ID: {sessionResponse.Id}");
            StartGameplay();
        }
        else
        {
            Debug.LogError("Failed to create session");
        }
    }
    
    // ===== 6. GAMEPLAY & EVENT RECORDING =====
    private void StartGameplay()
    {
        currentScore = 0;
        // Start game logic...
    }
    
    public void OnScoreChanged(int newScore)
    {
        currentScore = newScore;
        
        // Record score update event
        RecordGameEvent("score_update", new { score = currentScore });
    }
    
    public void OnLevelComplete(int level)
    {
        RecordGameEvent("level_complete", new { level, score = currentScore });
    }
    
    public void OnPowerUpUsed(string powerUpId)
    {
        RecordGameEvent("powerup_used", new { powerUpId });
    }
    
    private void RecordGameEvent(string eventType, object eventData)
    {
        var gameEvent = new
        {
            type = eventType,
            data = eventData,
            timestamp = DateTime.UtcNow
        };
        
        FunticoSDK.Instance.RecordEvent_Client(
            JsonConvert.SerializeObject(gameEvent)
        );
    }
    
    // ===== 7. UPDATE SESSION (CHECKPOINTS) =====
    public async void OnCheckpointReached(int checkpointId)
    {
        var updateData = new
        {
            checkpoint = checkpointId,
            score = currentScore,
            timestamp = DateTime.UtcNow
        };
        
        await FunticoSDK.Instance.UpdateSession_Client(
            JsonConvert.SerializeObject(updateData)
        );
        
        Debug.Log("Checkpoint saved!");
    }
    
    // ===== 8. FINISH GAME =====
    public async void OnGameComplete()
    {
        bool success = await FunticoSDK.Instance.FinishRoomSession_Client(
            currentRoomData.EventId,
            currentRoomData.SessionOrMatchId,
            currentScore
        );
        
        if (success)
        {
            await ShowResults();
        }
        else
        {
            ShowError("Failed to submit score");
        }
    }
    
    // ===== 9. SHOW LEADERBOARD =====
    private async UniTask ShowResults()
    {
        RoomLeaderboardViewModel leaderboard = await FunticoSDK.Instance.GetLeaderboard(
            currentRoomData.EventId,
            currentRoomData.SessionOrMatchId,
            null
        );
        
        Debug.Log($"Your Place: {leaderboard.UserPlace}");
        Debug.Log($"Your Earnings: {leaderboard.YourTicoEarnings} TICO");
        Debug.Log($"Multiplier: x{leaderboard.Multiplier}");
        
        DisplayLeaderboard(leaderboard);
    }
    
    // Helper methods (implement based on your UI)
    private string GetUserToken() => PlayerPrefs.GetString("UserToken");
    private async UniTask<bool> ShowResumeDialog() { /* Show UI */ return true; }
    private void RestoreGameState(string data) { /* Restore game */ }
    private void ShowMainMenu() { /* Show menu */ }
    private void DisplayUserInfo(UserData user, BalanceResponse balance) { /* Update UI */ }
    private void DisplayAvailableRooms(List<RoomViewModel> rooms) { /* Update UI */ }
    private void ShowError(string message) { /* Show error */ }
    private void DisplayLeaderboard(RoomLeaderboardViewModel leaderboard) { /* Show UI */ }
}

// Example error handler
public class MyErrorHandler : IErrorHandler
{
    public void ShowError(string errorMessage, string errorTitle = "Error", 
        string buttonText = "OK", Action additionalActionOnCloseClick = null)
    {
        Debug.LogError($"{errorTitle}: {errorMessage}");
    }
}
```

---

For more information, see:
- [README](./README.md) - Installation and quick start
- [ARCHITECTURE](./ARCHITECTURE.md) - Internal architecture and services

