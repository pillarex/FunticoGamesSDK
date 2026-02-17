using System;

namespace FunticoGamesSDK
{
    public static class APIConstants
    {
        public const string API_PROD = "https://funtico-sdk.azurewebsites.net";
        public const string API_STAGING = "https://funtico-sdk-staging.azurewebsites.net";
        public const string FUNTICO_BASE_PROD = "https://funtico.com";
        public const string FUNTICO_BASE_STAGING = "https://staging.funtico.com";
        public const string FUNTICO_API_BASE_PROD = "https://api.funtico.com";
        public const string FUNTICO_API_BASE_STAGING = "https://staging.api.funtico.com";
        private static FunticoSDK.Environment _env = FunticoSDK.Environment.STAGING;

        public static string API => _env switch {
            FunticoSDK.Environment.STAGING => API_STAGING,
            FunticoSDK.Environment.PROD => API_PROD,
            _ => throw new ArgumentOutOfRangeException()
        };

        public static string FUNTICO_BASE => _env switch {
            FunticoSDK.Environment.STAGING => FUNTICO_BASE_STAGING,
            FunticoSDK.Environment.PROD => FUNTICO_BASE_PROD,
            _ => throw new ArgumentOutOfRangeException()
        };

        public static string FUNTICO_API_BASE => _env switch {
            FunticoSDK.Environment.STAGING => FUNTICO_API_BASE_STAGING,
            FunticoSDK.Environment.PROD => FUNTICO_API_BASE_PROD,
            _ => throw new ArgumentOutOfRangeException()
        };

        public static void SetupEnvironment(FunticoSDK.Environment env)
        {
            _env = env;
        }

        #region Funtico

        public static string GENERATE_GAME_LINK = FUNTICO_API_BASE + "/api/v1/web/generate-game-link";
        public static string VERIFICATION = FUNTICO_BASE + "/profile/verification";
        public static string USERS_INFO = FUNTICO_API_BASE + "/api/v1/web/users";
        public static string FUNTICO_SHOP = FUNTICO_BASE + "/shop";

        #endregion

        #region Authentication
        public static string GUEST_LOGIN = API + "/Auth/guest-login";
        public static string FUNTICO_LOGIN = API + "/Auth/funtico-auth";
        #endregion
        
        #region User
        public static string USER_BALANCE = API + "/User/get-balance";
        public static string USER_ALL_DATA = API + "/User/get-full-data";
        public static string VOUCHERS = API + "/User/vouchers";
        public static string VOUCHERS_STATIC_DATA = API + "/User/vouchers-data";
        #endregion

        #region Rooms

        public static readonly string Get_Room = API + "/Rooms/get-room";
        public static readonly string Get_Rooms = API + "/Rooms/get-rooms";
        public static readonly string Post_Join_Room = API + "/Rooms/join-room";
        public static readonly string Post_Room_Started = API + "/Rooms/start-room-sp";
        public static readonly string Post_Score_Match = API + "/Rooms/end-room-sp";
        public static readonly string Get_Pre_Paid = API + "/Rooms/pre-paid";
        public static readonly string Get_Room_Settings = API + "/Rooms/game-settings";
        public static readonly string Get_Room_Settings_Server = API + "/Rooms/game-settings-server";
        public static readonly string Post_Room_Started_Server = API + "/Rooms/start-room-mp";
        public static readonly string Post_Score_Match_Server = API + "/Rooms/end-room-mp";
        public static readonly string Post_Score_List_Match_Server = API + "/Rooms/end-room-all-users";
        public static readonly string Get_Room_Leaders = API + "/Rooms/leaders";
        public static readonly string Get_Rooms_History = API + "/Rooms/history";
        public static readonly string Get_Room_Session_History = API + "/Rooms/get-history";
        public static readonly string Get_Tiers = API + "/Rooms/tiers";

        #endregion

        #region Session

        #region Client

        public static readonly string UNFINISHED_SESSION = API + "/Session/any-unfinished-sessions";
        public static readonly string CREATE_SESSION = API + "/Session/create-session";
        public static readonly string UPDATE_SESSION = API + "/Session/update-session";
        public static readonly string RECONNECT_TO_SESSION = API + "/Session/reconnect-to-session";

        #endregion

        #region Server

        public static readonly string SERVER_CREATE_SESSION = API + "/Session/create-server-session";
        public static readonly string SERVER_CLOSE_SESSION = API + "/Session/close-server-session";
        public static readonly string SERVER_USER_LEAVE_SESSION = API + "/Session/server-session-user-leave";

        #endregion

        #endregion

#if USE_FUNTICO_MATCHMAKING
        #region Matchmaking

        public static string MATCHMAKER_HUB => API + "/matchmaker";

        #endregion
#endif

        public static string WithQuery(string url, params string[] additional) =>
            $"{url}?{string.Join("&", additional)}";
        public static string GetUrlAPIWithId(string urlAPI, string id) => $"{urlAPI}/{id}";
        public static string GetUrlAPIWithId(string urlAPI, float id) => $"{urlAPI}/{id}";
    }
}