using System;
using System.Collections.Generic;
using System.Linq;
using FunticoGamesSDK;
using FunticoGamesSDK.ViewModels;
using FunticoGamesSDK.RoomsProviders;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
#if USE_FUNTICO_MATCHMAKING
using FunticoGamesSDK.APIModels.Matchmaking;
using FunticoGamesSDK.Matchmaking.Models;
#endif
using FunticoGamesSDK.APIModels.UserData;
using Newtonsoft.Json;
using UnityEngine;

public class SetupSDK : MonoBehaviour
{
    private RoomData currentRoomData;
    private int currentScore = 0;
    private List<RoomViewModel> availableRooms = new List<RoomViewModel>();
    private Vector2 scrollPosition = Vector2.zero;
    private bool _finishing;
#if USE_FUNTICO_MATCHMAKING
    private bool _inQueue;
    private bool _pendingAccept;
    private Guid _pendingMatchId;
    private int _acceptTimeoutSeconds;
    private string _matchmakingStatus = "";
    private string _matchmakingEventId = "";
    private MatchmakingRegion _matchmakingRegion = MatchmakingRegion.Europe;
    private readonly string[] _regionNames = Enum.GetNames(typeof(MatchmakingRegion));
#endif

    // ===== 1. INITIALIZATION =====
    private async void Start()
    {
        await InitializeSDK();
#if USE_FUNTICO_MATCHMAKING
        SubscribeToMatchmakingEvents();
#endif
#if SERVER || UNITY_SERVER
        return;
#endif
        await CheckReconnection();
        await LoadGameData();
    }

#if USE_FUNTICO_MATCHMAKING
    private void SubscribeToMatchmakingEvents()
    {
        FunticoMatchmaking.Instance.OnMatchStatus += status =>
        {
            _matchmakingStatus = status;
            Debug.Log($"Matchmaking Status: {status}");
        };

        FunticoMatchmaking.Instance.OnAcceptMatch += acceptData =>
        {
            _pendingAccept = true;
            _pendingMatchId = acceptData.MatchId;
            _acceptTimeoutSeconds = acceptData.TimeoutSeconds;
            _matchmakingStatus = $"Match found! Accept within {acceptData.TimeoutSeconds}s";
            Debug.Log($"Accept Match: {acceptData.MatchId}, Timeout: {acceptData.TimeoutSeconds}s");
        };

        FunticoMatchmaking.Instance.OnMatchFound += result =>
        {
            _inQueue = false;
            _pendingAccept = false;
            _matchmakingStatus = $"Match Found! ID: {result.MatchId}";
            Debug.Log($"Match Found: {result.MatchId}, Server: {result.ServerUrl}");
        };

        FunticoMatchmaking.Instance.OnMatchCancelled += reason =>
        {
            _pendingAccept = false;
            _matchmakingStatus = $"Match Cancelled: {reason}";
            Debug.Log($"Match Cancelled: {reason}");
        };

        FunticoMatchmaking.Instance.OnMatchError += error =>
        {
            _inQueue = false;
            _pendingAccept = false;
            _matchmakingStatus = $"Match Error: {error}";
            Debug.LogError($"Match Error: {error}");
        };

        FunticoMatchmaking.Instance.OnConnectionClosed += () =>
        {
            _inQueue = false;
            _pendingAccept = false;
            _matchmakingStatus = string.Empty;
            Debug.Log("Matchmaking connection closed");
        };
    }
#endif
    
    private async UniTask InitializeSDK()
    {
        await FunticoSDK.Instance.Initialize(
            FunticoSDK.Environment.STAGING,
            "private-key",
            "public-key",
            "user-token",
            new MyErrorHandler()
        );
        Debug.Log("SDK Initialized!");
    }
    
    // ===== 2. CHECK FOR UNFINISHED SESSIONS =====
    private async UniTask CheckReconnection()
    {
        var sessionsResponse = await FunticoSDK.Instance.UserHasUnfinishedSession_Client();

        var hasUnfinished = sessionsResponse != null && sessionsResponse.SavedSessions.Count > 0;
        if (hasUnfinished)
        {
            bool shouldResume = await ShowResumeDialog();
            if (shouldResume)
            {
                var session = sessionsResponse.SavedSessions.First();
                string sessionData = await FunticoSDK.Instance.ReconnectToUnfinishedSession_Client(session.Id);
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
        if (!room.IsPrePaid && !room.IsFree && !room.UserCanJoinWithVoucher && room.Ticket.Type == TicketType.Currency)
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
        
        var currentSession = await FunticoSDK.Instance.CreateSession_Client(
            JsonConvert.SerializeObject(sessionData), 0, currentRoomData.EventId, currentRoomData.SessionOrMatchId
        );
        
        // if (created)
        // {
            StartGameplay().Forget();
        // }
    }
    
    // ===== 6. GAMEPLAY & EVENT RECORDING =====
    private async UniTask StartGameplay()
    {
        currentScore = 0;
        // Start game logic...
        while (!_finishing)
        {
            OnScoreChanged(currentScore + 100);
            await UniTask.Delay(1000);
        }

        _finishing = false;
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
        _finishing = true;
        bool success = await FunticoSDK.Instance.FinishRoomSession_Client(
            currentRoomData.EventId,
            currentRoomData.SessionOrMatchId,
            currentScore
        );
        
        if (success)
        {
            await ShowResults();
            currentRoomData = null;
            currentScore = 0;
            Debug.Log("Room session finished successfully!");
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
    private async UniTask<bool> ShowResumeDialog() { /* Show UI */ return true; }

    private void RestoreGameState(string data)
    {
        /* Restore game */
        StartGameplay().Forget(); 
    }
    private void ShowMainMenu() { /* Show menu */ }

    private void DisplayUserInfo(UserData user, BalanceResponse balance)
    {
        Debug.Log(JsonConvert.SerializeObject(user));
    }

    private void DisplayAvailableRooms(List<RoomViewModel> rooms)
    {
        availableRooms = rooms ?? new List<RoomViewModel>();
        Debug.Log($"Loaded {availableRooms.Count} rooms");
#if USE_FUNTICO_MATCHMAKING
        if (string.IsNullOrEmpty(_matchmakingEventId) && availableRooms.Count > 0)
            _matchmakingEventId = availableRooms[0].Guid;
#endif
    }

    private void ShowError(string message)
    {
        Debug.LogError(message);
    }

    private void DisplayLeaderboard(RoomLeaderboardViewModel leaderboard)
    {
        Debug.Log($"leaderboard.YourTicoEarnings = {leaderboard.YourTicoEarnings}");
    }
    
    // ===== 10. GUI DISPLAY =====
    private void OnGUI()
    {
        // Display current room session controls
        if (currentRoomData != null)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 200));
            GUILayout.BeginVertical(GUI.skin.box);
            
            GUILayout.Label("Current Room Session", GUI.skin.box, GUILayout.Width(280), GUILayout.Height(30));
            GUILayout.Space(10);
            
            GUILayout.Label($"Event ID: {currentRoomData.EventId}", GUILayout.Height(20));
            GUILayout.Label($"Session ID: {currentRoomData.SessionOrMatchId}", GUILayout.Height(20));
            GUILayout.Label($"Current Score: {currentScore}", GUILayout.Height(20));
            
            GUILayout.Space(10);
            
            // Finish Room button
            if (GUILayout.Button("Finish Room", GUILayout.Height(40)))
            {
                OnGameComplete();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
            return;
        }
        
        // Display available rooms list
        if (availableRooms == null || availableRooms.Count == 0)
            return;
            
        GUILayout.BeginArea(new Rect(10, 10, 600, Screen.height - 20));
        GUILayout.Label("Available Rooms", GUI.skin.box, GUILayout.Width(580), GUILayout.Height(30));
        
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(600), GUILayout.Height(Screen.height - 50));
        
        foreach (var room in availableRooms)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            
            // Room name
            GUILayout.Label($"Name: {room.Name ?? "N/A"}", GUILayout.Height(25));
            
            // Room type
            GUILayout.Label($"Tier: {room.Tier}", GUILayout.Height(20));
            
            // Entry fee
            string entryFee = room.IsFree ? "Free" : $"{room.EntryFee} {room.Ticket?.Type}";
            GUILayout.Label($"Entry Fee: {entryFee}", GUILayout.Height(20));
            
            GUILayout.Label($"Prize: {room.TotalPrize}", GUILayout.Height(20));
            
            // Join button
            if (GUILayout.Button("Join Room", GUILayout.Height(40)))
            {
                OnJoinRoomButtonClick(room.Guid);
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(5);
        }
        
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        
#if USE_FUNTICO_MATCHMAKING
        // Matchmaking controls
        DrawMatchmakingControls();
#endif
    }

#if USE_FUNTICO_MATCHMAKING
    private bool _guiPendingAccept;
    private bool _guiInQueue;

    private void DrawMatchmakingControls()
    {
        if (Event.current.type == EventType.Layout)
        {
            _guiPendingAccept = _pendingAccept;
            _guiInQueue = _inQueue;
        }

        GUILayout.BeginArea(new Rect(Screen.width - 310, Screen.height - 360, 300, 350));
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.Label("Matchmaking", GUI.skin.box, GUILayout.Width(280), GUILayout.Height(30));
        GUILayout.Space(5);

        GUILayout.Label("Event ID:", GUILayout.Height(20));
        _matchmakingEventId = GUILayout.TextField(_matchmakingEventId, GUILayout.Height(22));
        GUILayout.Space(5);
        
        GUILayout.Label("Region:", GUILayout.Height(20));
        var selectedIndex = (int)_matchmakingRegion;
        var newIndex = GUILayout.SelectionGrid(selectedIndex, _regionNames, 3, GUILayout.Height(50));
        if (newIndex != selectedIndex)
        {
            _matchmakingRegion = (MatchmakingRegion)newIndex;
        }
        
        GUILayout.Space(5);
        
        GUILayout.Label(string.IsNullOrEmpty(_matchmakingStatus) ? " " : $"Status: {_matchmakingStatus}", GUILayout.Height(20));
        
        GUILayout.Space(5);

        GUILayout.Label(_guiPendingAccept ? $"Accept within {_acceptTimeoutSeconds}s" : " ", GUILayout.Height(20));

        GUILayout.BeginHorizontal();
        if (_guiPendingAccept)
        {
            if (GUILayout.Button("Accept", GUILayout.Height(40)))
                OnAcceptMatchClick();
            if (GUILayout.Button("Decline", GUILayout.Height(40)))
                OnDeclineMatchClick();
        }
        else
        {
            if (GUILayout.Button(_guiInQueue ? "Leave Queue" : "Join Queue", GUILayout.Height(40)))
            {
                if (_guiInQueue) OnLeaveQueueClick();
                else OnJoinQueueClick();
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private async void OnJoinQueueClick()
    {
        _inQueue = true;
        _matchmakingStatus = "Joining queue...";
        await FunticoMatchmaking.Instance.JoinQueue(_matchmakingEventId, _matchmakingRegion, 2);
        if (_inQueue) _matchmakingStatus = "In queue, waiting for match...";
    }

    private void OnLeaveQueueClick()
    {
        _matchmakingStatus = "Leaving queue...";
        FunticoMatchmaking.Instance.LeaveQueue();
        _inQueue = false;
        _pendingAccept = false;
        _matchmakingStatus = "Left queue";
    }

    private void OnAcceptMatchClick()
    {
        _pendingAccept = false;
        _matchmakingStatus = "Accepting match...";
        FunticoMatchmaking.Instance.AcceptMatch();
    }

    private void OnDeclineMatchClick()
    {
        _pendingAccept = false;
        _matchmakingStatus = "Declining match...";
        FunticoMatchmaking.Instance.DeclineMatch();
    }
#endif
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