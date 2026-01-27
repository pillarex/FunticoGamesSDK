using System;
using System.Collections.Generic;
using System.Linq;
using FunticoGamesSDK;
using FunticoGamesSDK.ViewModels;
using FunticoGamesSDK.RoomsProviders;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.APIModels.Matchmaking;
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
    private bool _inQueue;
    private string _matchmakingStatus = "";
    private MatchmakingRegion _matchmakingRegion = MatchmakingRegion.Europe;
    private readonly string[] _regionNames = Enum.GetNames(typeof(MatchmakingRegion));

    // ===== 1. INITIALIZATION =====
    private async void Start()
    {
        await InitializeSDK();
        SubscribeToMatchmakingEvents();
        await CheckReconnection();
        await LoadGameData();
    }

    private void SubscribeToMatchmakingEvents()
    {
        FunticoSDK.Instance.OnMatchStatus += status =>
        {
            _matchmakingStatus = status;
            Debug.Log($"Matchmaking Status: {status}");
        };

        FunticoSDK.Instance.OnMatchFound += result =>
        {
            _inQueue = false;
            _matchmakingStatus = $"Match Found! ID: {result.MatchId}";
            Debug.Log($"Match Found: {result.MatchId}, Server: {result.ServerUrl}");
        };
    }
    
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
        
        // Matchmaking controls
        DrawMatchmakingControls();
    }

    private void DrawMatchmakingControls()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 310, Screen.height - 260, 300, 250));
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.Label("Matchmaking", GUI.skin.box, GUILayout.Width(280), GUILayout.Height(30));
        GUILayout.Space(5);
        
        // Region selection
        GUILayout.Label("Region:", GUILayout.Height(20));
        var selectedIndex = (int)_matchmakingRegion;
        var newIndex = GUILayout.SelectionGrid(selectedIndex, _regionNames, 3, GUILayout.Height(50));
        if (newIndex != selectedIndex)
        {
            _matchmakingRegion = (MatchmakingRegion)newIndex;
        }
        
        GUILayout.Space(5);
        
        // Status
        if (!string.IsNullOrEmpty(_matchmakingStatus))
        {
            GUILayout.Label($"Status: {_matchmakingStatus}", GUILayout.Height(20));
        }
        
        GUILayout.Space(5);
        
        // Join/Leave Queue buttons
        if (_inQueue)
        {
            if (GUILayout.Button("Leave Queue", GUILayout.Height(40)))
            {
                OnLeaveQueueClick();
            }
        }
        else
        {
            if (GUILayout.Button("Join Queue", GUILayout.Height(40)))
            {
                OnJoinQueueClick();
            }
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private async void OnJoinQueueClick()
    {
        _inQueue = true;
        _matchmakingStatus = "Joining queue...";
        await FunticoSDK.Instance.JoinQueue(_matchmakingRegion, 2);
        if (_inQueue) _matchmakingStatus = "In queue, waiting for match...";
    }

    private async void OnLeaveQueueClick()
    {
        _matchmakingStatus = "Leaving queue...";
        await FunticoSDK.Instance.LeaveQueue();
        _inQueue = false;
        _matchmakingStatus = "Left queue";
    }
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