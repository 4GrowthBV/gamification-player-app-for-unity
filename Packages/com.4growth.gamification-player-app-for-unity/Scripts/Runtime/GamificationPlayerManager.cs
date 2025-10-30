using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.ExternalEvents;
using GamificationPlayer.Session;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    /// <summary>
    /// Represents a method that is called when a module is started.
    /// </summary>
    /// <param name="moduleIdentifier">The module identifier of the started module.</param>
    public delegate void OnModuleStartEvent(Guid moduleIdentifier);

    /// <summary>
    /// Represents a method that is called when there is an error on the page.
    /// </summary>
    public delegate void OnErrorEvent();

    /// <summary>
    /// Represents a method that is called when a page is loaded.
    /// After this method is called, it can be checked if the user is logged in.
    /// </summary>
    public delegate void OnPageViewEvent();

    /// <summary>
    /// Represents a method that is called when an external event is fired by the gamification player.
    /// Most of these events are handled by this package. This method can be used to check for custom events, such as the "quitEvent".
    /// </summary>
    /// <param name="eventType">The type of event that is being called.</param>
    public delegate void OnExternalEvent(string eventType);

    /// <summary>
    /// Represents a method that is called when the server time is received.
    /// </summary>
    /// <param name="dateTime">The date and time of the server.</param>
    public delegate void OnServerTimeEvent(DateTime dateTime);

    /// <summary>
    /// Represents a method that is called when a MicroGame is opened via the gamification player.
    /// </summary>
    /// <param name="identifier">The identifier of the MicroGame.</param>
    public delegate void OnMicroGameOpenedEvent(MicroGamePayload microGame);

    /// <summary>
    /// Represents a method that is called when the language is changed or first time that the languages is set
    /// </summary>
    /// <param name="identifier">The identifier of the languages.</param>
    public delegate void OnLanguageSetEvent(string identifier);

    /// <summary>
    /// Represents a method that is called when a tile click event is fired by the gamification player.
    /// </summary>
    /// <param name="identifier">Identifier of the tile being clicked. Example: 'fitness_video'.</param>
    public delegate void OnTileClickEvent(string identifier);

    /// <summary>
    /// Represents a method that is called when a quit event is fired by the gamification player.
    /// </summary>
    public delegate void OnQuitEvent();


    public class GamificationPlayerManager : MonoBehaviour
    {
        /// <summary>
        /// Occurs when a module is started.
        /// </summary>
        public static event OnModuleStartEvent OnModuleStart;

        /// <summary>
        /// Occurs when a page is loaded with an error
        /// </summary>
        public static event OnErrorEvent OnError;

        /// <summary>
        /// Occurs when a page is loaded of the gamification player.
        /// </summary>
        public static event OnPageViewEvent OnPageView;

        /// <summary>
        /// Occurs when an external event is fired by the gamification player.
        /// </summary>
        public static event OnExternalEvent OnEvent;

        /// <summary>
        /// Occurs when the server time is received.
        /// </summary>
        public static event OnServerTimeEvent OnServerTime;

        /// <summary>
        /// Occurs when a MicroGame is opened via the gamification player.
        /// </summary>
        public static event OnMicroGameOpenedEvent OnMicroGameOpened;

        /// <summary>
        /// Occurs when a language is changed or first time that the languages is set
        /// </summary>
        public static event OnLanguageSetEvent OnLanguageSet;

        /// <summary>
        /// Occurs when a tile click event is fired by the gamification player.
        /// </summary>
        public static event OnTileClickEvent OnTileClick;

        /// <summary>
        /// Occurs when a quit event is fired by the gamification player.
        /// </summary>
        public static event OnQuitEvent OnQuit;

        private static GamificationPlayerManager instance;

        public static bool IsInitialized 
        { 
            get 
            { 
                if(instance == null)
                {
                    return false;
                }
                
                return instance.isInitialized; 
            } 
        }

        /// <summary>
        /// Gets the session log data instance for accessing queryable data
        /// </summary>
        public static SessionLogData SessionLogData
        {
            get
            {
                if(instance == null)
                {
                    return null;
                }
                
                return instance.sessionData;
            }
        }

        /// <summary>
        /// Gets the gamification player endpoints instance for making API calls
        /// </summary>
        public static GamificationPlayerEndpoints GamificationPlayerEndpoints
        {
            get
            {
                if(instance == null)
                {
                    return null;
                }
                
                return instance.gamificationPlayerEndpoints;
            }
        }
        
        /// <summary>
        /// Clears the non-persistence database and configures the class to use the mock server settings and mock requests for the mock server (GET requests instead of PATCH requests).
        /// </summary>
        public static void UseMockServer()
        {
            instance.GUseMockServer();
        }

        /// <summary>
        /// Determines whether an user is currently logged in in the gamification player.
        /// </summary>
        /// <returns>true if an user is logged in; otherwise, false.</returns>
        public static bool IsUserActive()
        {
            return instance.GIsUserActive();
        }

        /// <summary>
        /// Attempts to get the latest login token
        /// </summary>
        /// <param name="language">The latest login token, if it is available.</param>
        /// <returns>true if the latest login token was successfully retrieved; otherwise, false.</returns>
        public static bool TryGetLatestLoginToken(out string language)
        {
            return instance.GTryGetLatestLoginToken(out language);
        }

        /// <summary>
        /// Attempts to get the current language
        /// </summary>
        /// <param name="language">The identifier of the current language, if it is available.</param>
        /// <returns>true if the current language was successfully retrieved; otherwise, false.</returns>
        public static bool TryGetCurrentLanguage(out string language)
        {
            return instance.GTryGetCurrentLanguage(out language);
        }

        /// <summary>
        /// Attempts to get the identifier of the active user.
        /// </summary>
        /// <param name="id">The identifier of the active user, if it is available.</param>
        /// <returns>true if the active user's identifier was successfully retrieved; otherwise, false.</returns>
        public static bool TryGetActiveUserId(out Guid id)
        {
            return instance.GTryGetActiveUserId(out id);
        }

        /// <summary>
        /// Determines whether a module session is currently active.
        /// </summary>
        /// <returns>true if a module session is active; otherwise, false.</returns>
        public static bool IsMicroGameActive()
        {
            return instance.GIsMicroGameActive();
        }

        /// <summary>
        /// Attempts to get the identifier of the latest active module session.
        /// </summary>
        /// <param name="id">The identifier of the latest active module session, if it is available.</param>
        /// <returns>true if the latest active module session's identifier was successfully retrieved; otherwise, false.</returns>
        public static bool TryGetLatestModuleId(out Guid id)
        {
            return instance.GTryGetLatestModuleId(out id);
        }

        /// <summary>
        /// Attempts to end the latest MicroGame.
        /// </summary>
        /// <param name="score">The score of the MicroGame.</param>
        /// <param name="isCompleted">Indicates whether the MicroGame was completed. The module can be ended without completing if the user ends before the end.</param>
        /// <param name="onDone">An optional callback that will be called when the operation is completed. If the operation is successful, the callback will be called without any arguments. If the operation fails, the callback will be called without any arguments.</param>
        public static void StopMicroGame(int score, bool isCompleted, AppScoresCallback appScoresCallback = null)
        {
            instance.GStopMicroGame(score, isCompleted, appScoresCallback);
        }

        /// <summary>
        /// Attempts to restart the previous module session.
        /// </summary>
        public static void RestartMicroGame()
        {
            instance.GRestartMicroGame();
        }

        /// <summary>
        /// Processes an external message in JSON format.
        /// This method is used to process messages from a vuplex webview.
        /// The message is expected to be in the following format:
        /// {
        ///    "data": { "type": "string" }
        /// }
        /// </summary>
        /// <param name="jsonMessage">The JSON message to be processed.</param>
        public static void ProcessExternalMessage(string jsonMessage)
        {
            instance.GProcessExternalMessage(jsonMessage);
        }

        /// <summary>
        /// Attempts to get the current server time.
        /// </summary>
        /// <param name="dateTime">The current server time, if it is available.</param>
        /// <returns>true if the server time was successfully retrieved; otherwise, false.</returns>
        public static bool TryGetServerTime(out DateTime dateTime)
        {
            return instance.GTryGetServerTime(out dateTime);
        }

        /// <summary>
        /// Attempts to get the latest data that has been sync with the server.
        /// </summary>
        /// <param name="dateTime">The current latest data synced with the server, if it is available.</param>
        /// <returns>true if current latest data synced with the server was already retrieved; otherwise, false.</returns>
        public static bool TryGetLatestData<TQueryable>(out string value)
            where TQueryable : Session.IQueryable
        {
            return instance.GTryGetLatestData<TQueryable>(out value);
        }

        /// <summary>
        /// Attempts to get the latest data that has been sync with the server.
        /// </summary>
        /// <param name="dateTime">The current latest data synced with the server, if it is available.</param>
        /// <returns>true if current latest data synced with the server was already retrieved; otherwise, false.</returns>
        public static bool TryGetLatestData<TQueryable>(out string[] value)
            where TQueryable : Session.IQueryable
        {
            return instance.GTryGetLatestData<TQueryable>(out value);
        }

        /// <summary>
        /// Attempts to get the latest data that has been sync with the server.
        /// </summary>
        /// <typeparam name="TQueryable"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetLatestData<TQueryable>(out bool value)
            where TQueryable : Session.IQueryable
        {
            return instance.GTryGetLatestData<TQueryable>(out value);
        }

        /// <summary>
        /// Attempts to get the latest data that has been sync with the server.
        /// </summary>
        /// <typeparam name="TQueryable"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetLatestData<TQueryable>(out int value)
            where TQueryable : Session.IQueryable
        {
            return instance.GTryGetLatestData<TQueryable>(out value);
        }

        /// <summary>
        /// Listen to when data gets synced with the server.
        /// </summary>
        public static void ListenToData<TQueryable>(Action<object> callback)
            where TQueryable : Session.IQueryable
        {
            instance.GListenToData<TQueryable>(callback);
        }

        /// <summary>
        /// Remove the event that listen to when data gets synced with the server.
        /// </summary>
        public static void RemoveListener(Action<object> callback)
        {
            instance.GRemoveListener(callback);
        }

        /// <summary>
        /// Gets the current server time and raises the OnServerTime when it is received.
        /// </summary>
        public static void GetServerTime()
        {
            instance.GGetServerOffSetTime();
        }

        /// <summary>
        /// Returns if the user can login directly via the login token
        /// </summary>
        public static bool CanUserLoginViaLoginToken()
        {
            return TryGetActiveUserId(out _) &&
                TryGetLatestLoginToken(out _) && 
                TryGetLatestSubdomain(out _);
        }

        /// <summary>
        /// Attempts to get the identifier of the latest MicroGame.
        /// </summary>
        /// <param name="microGamePayload">The identifier of the latest MicroGame, if it is available.</param>
        /// <returns>true if the latest MicroGame's identifier was successfully retrieved; otherwise, false.</returns>
        public static bool TryGetCurrentMicroGamePayload(out MicroGamePayload microGamePayload)
        {
            return instance.GTryGetCurrentMicroGamePayload(out microGamePayload);
        }

        /// <summary>
        /// Attempts to get the latest subdomain.
        /// </summary>
        /// <param name="subdomain">The latest subdomain, if it is available.</param>
        /// <returns>true if the latest subdomain was successfully retrieved; otherwise, false.</returns>
        public static bool TryGetLatestSubdomain(out string subdomain)
        {
            return instance.GTryGetLatestSubdomain(out subdomain);
        }

        /// <summary>
        /// Attempts to get the latest environment config.
        /// </summary>
        /// <param name="environmentConfig">The latest environment config, if it is available.</param>
        public static EnvironmentConfig GetEnvironmentConfig()
        {
            return instance.GGetEnvironmentConfig();
        }

        /// <summary>
        /// Clear data from the session log data.
        /// </summary>
        public static void ClearData()
        {
            instance.GClearData();
        }

        /// <summary>
        /// Attempts to start a MicroGame, if successvol it will fires the OnMicroGameOpened event.
        /// </summary>
        /// <param name="guid">The identifier of the MicroGame to start.</param>
        /// <param name="player">The player data to be used for the MicroGame, keep empty if user already logged in</param>
        /// <param name="environment">The environment data to be used for the MicroGame, keep empty if user already logged in</param>
        public static void StartMicroGame(Guid guid,
            MicroGamePayload.Player player = null, 
            MicroGamePayload.Environment environment = null)
        {
            instance.GStartMicroGame(guid, player, environment);
        }

        public static void GetMicroGame(Guid guid, Action<DTO.MicroGame.GetMicroGameResponseDTO> callback)
        {
            instance.GGetMicroGame(guid, callback);
        }

        public static void GetMicroGames(Action<DTO.MicroGame.GetMicroGamesResponseDTO> callback)
        {
            instance.GGetMicroGames(callback);
        }

        public static void OfflineSync(List<DTO.OfflineSync.OfflineSyncRequestDTO.DataItem> dataItems, OfflineSyncCallback callback)
        {
            instance.GOfflineSync(dataItems, callback);
        }

        public static void GetModuleSession(GetModuleSessionCallback callback)
        {
            instance.GGetModuleSession(callback);
        }

        /// <summary>
        /// Open a MicroGame based on the MicroGamePayload.
        /// This method is used to open a MicroGame based on the MicroGamePayload that can be configured in Unity Editor for testing purposes.
        /// </summary>
        /// <param name="microGamePayload">The MicroGamePayload to be used to open the MicroGame.</param>
        public static void OpenMicroGameBasedOnMicroGamePayload(MicroGamePayload microGamePayload)
        {
            instance.GOpenMicroGameBasedOnMicroGamePayload(microGamePayload);
        }

        /// <summary>
        /// Change the environment of the gamification player.
        /// </summary>
        /// <param name="environmentDomain">The domain of the environment to change to.</param>
        public static void ChangeEnvironment(string environmentDomain)
        {
            instance.GChangeEnvironment(environmentDomain);
        }

        public static void AddTakeAwayResultToActiveSession(byte[] fileData, StoreTakeAwayResultCallback callback)
        {
            instance.GAddTakeAwayResult(fileData, callback);
        }

        [SerializeField]
        private bool checkServerTimeOnStartUp = false;

        [SerializeField]
        private string defaultEnvironment = ".eu";

        [SerializeField]
        [Header("Mock settings for testing")]
        private EnvironmentConfig gamificationPlayerMockConfig;

        [SerializeField]
        private string absoluteURLTest = string.Empty;

        protected GamificationPlayerEndpoints gamificationPlayerEndpoints;

        protected SessionLogData sessionData;

        private bool isUserActive = false;

        private bool isInitialized = false;

        private MicroGamePayload currentMicroGamePayload;
        private MicroGamePayload finishedMicroGamePayload;

        private DateTime latestStartedGame;

        public void Awake()
        {
            if(instance != null)
            {
                Destroy(this);

                return;
            }

            DontDestroyOnLoad(this);

            instance = this;

            sessionData = new SessionLogData();

#if UNITY_WEBGL
            var absoluteURL = GetAbsoluteURL();
            if(!string.IsNullOrEmpty(absoluteURL) && 
                absoluteURL.Contains("moduleData"))
            {
                Uri url = new Uri(absoluteURL);
                var query = System.Web.HttpUtility.ParseQueryString(url.Query);
                string jwt = query["moduleData"];

                GamificationPlayerConfig.TryGetEnvironmentConfig(absoluteURL, out var environmentConfig);

                var json = JWTHelper.GetJSONWebTokenPayload(jwt, environmentConfig.JSONWebTokenSecret);

                var dto = json.FromJson<MicroGamePayload>(false);

                sessionData.AddToLog(dto);
            }
#else
            sessionData.ListenTo<Language>(LanguageSet);
            sessionData.ListenTo<OrganisationDefaultLanguage>(LanguageSet);
#endif

            gamificationPlayerEndpoints = new GamificationPlayerEndpoints(GGetEnvironmentConfig(), sessionData);
            
            if(checkServerTimeOnStartUp)
            {
                GGetServerOffSetTime();
            }
        }

        private void LanguageSet(object obj)
        {
            OnLanguageSet?.Invoke(obj as string);
        }

        private void OnDestroy()
        {
#if !UNITY_WEBGL
            if(sessionData != null)
            {
                sessionData.RemoveListener(LanguageSet);
            }
#endif
        }

        private string GetAbsoluteURL()
        {
            if(!string.IsNullOrEmpty(absoluteURLTest))
            {
                return absoluteURLTest;
            }

            return Application.absoluteURL;
        }

        private void GChangeEnvironment(string environmentDomain)
        {
            sessionData.ClearData();

            if(GamificationPlayerConfig.TryGetEnvironmentConfig(environmentDomain, out var environmentConfig))
            {
                gamificationPlayerEndpoints = new GamificationPlayerEndpoints(environmentConfig, sessionData);
            }
        }

        private EnvironmentConfig GGetEnvironmentConfig()
        {
            if(gamificationPlayerEndpoints != null)
            {
                return gamificationPlayerEndpoints.EnvironmentConfig;
            }

            EnvironmentConfig environmentConfig;

            if(sessionData.TryGetLatestEnvironmentDomain(out var env))
            {
                if(GamificationPlayerConfig.TryGetEnvironmentConfig(env, out environmentConfig))
                {
                    return environmentConfig;
                }
            }

#if PROD_BUILD
            var environment = ".app";
#elif STAG_BUILD
            var environment = ".eu";
#else
            var environment = defaultEnvironment;
#endif

            GamificationPlayerConfig.TryGetEnvironmentConfig(environment, out environmentConfig);

            return environmentConfig;
        }

        public void Start()
        {
#if UNITY_WEBGL
            isInitialized = true;
#endif
            if(sessionData.TryGetLatestModuleId(out Guid id))
            {
                InvokeModuleStart(id);
            }

            if(sessionData.TryGetLatestMicroGamePayload(out MicroGamePayload microGamePayload))
            {
                InvokeMicroGameOpened(microGamePayload);
            }

#if !UNITY_WEBGL
            if(GHaveUserCredentials())
            {
                var isGetLoginToken = false;
                var isGetOrganisation = false;
                var isGetUser = false;
                StartCoroutine(gamificationPlayerEndpoints.CoGetLoginToken((_, __) => { 
                    isGetLoginToken = true; 
                    if(isGetLoginToken && isGetOrganisation && isGetUser)
                    {
                        isInitialized = true;
                    }
                }));
                StartCoroutine(gamificationPlayerEndpoints.CoGetOrganisation((_, __) => { 
                    isGetOrganisation = true; 
                    if(isGetLoginToken && isGetOrganisation && isGetUser)
                    {
                        isInitialized = true;
                    }                  
                }));
                StartCoroutine(gamificationPlayerEndpoints.CoGetUser((_, __) => { 
                    isGetUser = true; 
                    if(isGetLoginToken && isGetOrganisation && isGetUser)
                    {
                        isInitialized = true;
                    }
                }));
            } else
            {
                isInitialized = true;
            }
#endif
        }

        private void GRemoveListener(Action<object> callback)
        {
            sessionData.RemoveListener(callback);
        }

        private void GListenToData<TQueryable>(Action<object> callback) 
            where TQueryable : IQueryable
        {
            sessionData.ListenTo<TQueryable>(callback);
        }

        private bool GTryGetLatestData<TQueryable>(out string[] value) 
            where TQueryable : IQueryable
        {
            return sessionData.TryGetLatest<TQueryable>(out value);
        }

        private bool GTryGetLatestData<TQueryable>(out string value) 
            where TQueryable : IQueryable
        {
            return sessionData.TryGetLatest<TQueryable>(out value);
        }

        private bool GTryGetLatestData<TQueryable>(out bool value) 
            where TQueryable : IQueryable
        {
            return sessionData.TryGetLatest<TQueryable>(out value);
        }

        private bool GTryGetLatestData<TQueryable>(out int value) 
            where TQueryable : IQueryable
        {
            return sessionData.TryGetLatest<TQueryable>(out value);
        }

        private bool GTryGetServerTime(out DateTime dateTime)
        {
            dateTime = DateTime.Now;

            if(sessionData.TryGetLatestServerTime(out var serverTime))
            {
                sessionData.TryGetWhenServerTime(out var realtimeSinceStartup);

                var secondsToAdd = Time.realtimeSinceStartup - realtimeSinceStartup;

                dateTime = serverTime.AddSeconds(secondsToAdd);

                return true;
            }

            return false;
        }

        private void GGetServerOffSetTime()
        {
            StartCoroutine(gamificationPlayerEndpoints.CoGetTime((result, dateTime) =>
            {
                if(result == UnityWebRequest.Result.Success)
                {
                    OnServerTime?.Invoke(dateTime);
                } else
                {
                    StartCoroutine(ActionAfterXSeconds(GGetServerOffSetTime, 5f));
                }
            }));
        }

        private bool GTryGetLatestLoginToken(out string language)
        {
            return sessionData.TryGetLatestLoginToken(out language);
        }

        private bool GTryGetCurrentLanguage(out string language)
        {
            return sessionData.TryGetLatestLanguage(out language);
        }

        private bool GTryGetCurrentMicroGamePayload(out MicroGamePayload payload)
        {
            payload = currentMicroGamePayload;

            return currentMicroGamePayload != null;
        }

        private bool GTryGetLatestSubdomain(out string subdomain)
        {
            return sessionData.TryGetLatestSubdomain(out subdomain);
        }

        private void GUseMockServer()
        {
            sessionData = new SessionLogData();

            gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerMockConfig, sessionData);
        }

        private void GProcessExternalMessage(string jsonMessage)
        {
            var message = jsonMessage.FromJson<StandardDTO>();

            OnEvent?.Invoke(message.data.Type);

            if(message.data.Type == "moduleSessionStarted")
            {
                ModuleSessionStarted(jsonMessage);
            }

            if(message.data.Type == "pageView")
            {
                PageView(jsonMessage);
            }

            if (message.data.Type == "microGameOpened" ||
                message.data.Type == "fitnessContentOpened")
            {
                MicroGameOpened(jsonMessage);
            }

            if(message.data.Type == "error")
            {
                Error(jsonMessage);
            }

            if(message.data.Type == "quitEvent")
            {
                OnQuit?.Invoke();
            }

            if(message.data.Type == "tileClick")
            {
                var dto = jsonMessage.FromJson<TileClickDTO>();

                OnTileClick?.Invoke(dto.data.attributes.identifier);
            }

            if(message.data.Type == "link")
            {
                var dto = jsonMessage.FromJson<LinkDTO>();

                Application.OpenURL(dto.data.attributes.link);
            }
        }

        private void Error(string jsonMessage)
        {
            var dto = jsonMessage.FromJson<ErrorDTO>();

            sessionData.AddToLog(dto.data);

            OnError?.Invoke();
        }

        private void ModuleSessionStarted(string jsonMessage)
        {
            var dto = jsonMessage.FromJson<ModuleSessionStartedDTO>();

            if(sessionData.TryGetLatestModuleSessionId(out Guid latestModuleSessionId))
            {
                if(latestModuleSessionId == Guid.Parse(dto.data.attributes.module_session_id))
                {
                    sessionData.AddToLog(dto.data);

                    return;
                }
            }

            sessionData.AddToLog(dto.data);

            sessionData.TryGetLatestModuleId(out Guid id);
            
            InvokeModuleStart(id);
        }

        private void GGetMicroGame(Guid guid, Action<DTO.MicroGame.GetMicroGameResponseDTO> callback)
        {
            StartCoroutine(gamificationPlayerEndpoints.CoGetMicroGame(guid, (result, dto) =>
            {
                callback?.Invoke(dto);
            }));
        }

        private void GGetMicroGames(Action<DTO.MicroGame.GetMicroGamesResponseDTO> callback)
        {
            StartCoroutine(gamificationPlayerEndpoints.CoGetMicroGames((result, dto) =>
            {
                callback?.Invoke(dto);
            }));
        }

        private void GOfflineSync(List<DTO.OfflineSync.OfflineSyncRequestDTO.DataItem> dataItems, OfflineSyncCallback callback)
        {
            StartCoroutine(gamificationPlayerEndpoints.CoOfflineSync(dataItems, callback));
        }

        private void GGetModuleSession(GetModuleSessionCallback callback)
        {
            StartCoroutine(gamificationPlayerEndpoints.CoGetModuleSession(callback));
        }

        private void GStartMicroGame(Guid guid, 
            MicroGamePayload.Player player = null, 
            MicroGamePayload.Environment environment = null)
        {
            //We set the currentMicroGamePayload to NOT null to make sure that the program knows there is a MicroGame active before the server responds
            currentMicroGamePayload = new MicroGamePayload();
            StartCoroutine(gamificationPlayerEndpoints.CoGetMicroGame(guid, (result, dto) =>
            {
                if(result == UnityWebRequest.Result.Success)
                {
                    sessionData.TryGetLatestOrganisationId(out Guid organisationId);
                    sessionData.TryGetLatestUserId(out Guid userId);

                    sessionData.TryGetLatest<UserAvatar>(out string userAvatar);
                    sessionData.TryGetLatest<UserName>(out string userName);
                    sessionData.TryGetLatest<Language>(out string language);
                    sessionData.TryGetLatest<EnvironmentDomain>(out string environmentDomain);
                    sessionData.TryGetLatest<EnvironmentType>(out string environmentType);
                    sessionData.TryGetLatest<OrganisationSubdomain>(out string organisationSubdomain);
                    sessionData.TryGetLatest<OrganisationResellerID>(out string organisationResellerId);
                    sessionData.TryGetLatest<OrganisationName>(out string organisationName);
                    sessionData.TryGetLatest<UserTags>(out string[] userTags);

                    var webTokenPayload = new MicroGamePayload
                    {
                        player = player ?? new MicroGamePayload.Player
                        {
                            user_id = userId.ToString(),
                            user_avatar = userAvatar,
                            user_name = userName,
                            language = language,
                            user_tags = userTags
                        },

                        organisation = new MicroGamePayload.Organisation
                        {
                            id = organisationId.ToString(),
                            name = organisationName,    
                            subdomain = organisationSubdomain,
                            reseller_id = organisationResellerId
                        },

                        session = new MicroGamePayload.Session(),

                        battle = new MicroGamePayload.Battle(),

                        micro_game = new MicroGamePayload.MicroGame
                        {
                            name = dto.data.attributes.name,
                            id = guid.ToString(),
                            identifier = dto.data.attributes.identifier,
                            extra_data = dto.data.attributes.extra_data
                        },

                        module = null,

                        environment = environment ?? new MicroGamePayload.Environment
                        {
                            domain = environmentDomain,
                            type = environmentType
                        }
                    };

                    var maxScore = 10000;

                    webTokenPayload.micro_game.stars = new MicroGamePayload.MicroGame.Stars
                    {
                        one = (int)(dto.data.attributes.star_thresholds[0] / 100f * maxScore),
                        two = (int)(dto.data.attributes.star_thresholds[1] / 100f * maxScore),
                        three = (int)(dto.data.attributes.star_thresholds[2] / 100f * maxScore),
                        four = (int)(dto.data.attributes.star_thresholds[3] / 100f * maxScore),
                        five = (int)(dto.data.attributes.star_thresholds[4] / 100f * maxScore)
                    };

                    sessionData.AddToLog(webTokenPayload);

                    InvokeMicroGameOpened(webTokenPayload);   
                } else
                {
                    currentMicroGamePayload = null;
                }
            }));
        }

        private void GOpenMicroGameBasedOnMicroGamePayload(MicroGamePayload microGamePayload)
        {
            sessionData.AddToLog(microGamePayload);

            InvokeMicroGameOpened(microGamePayload);    
        }

        private void MicroGameOpened(string jsonMessage)
        {
            var dto = jsonMessage.FromJson<MicroGameOpenedDTO>();
            
            sessionData.AddToLog(dto.data);

            var JSONWebTokenPayload = JWTHelper.GetJSONWebTokenPayload(dto.data.attributes.module_data, 
                gamificationPlayerEndpoints.EnvironmentConfig.JSONWebTokenSecret);
            var webTokenPayload = JSONWebTokenPayload.FromJson<MicroGamePayload>(false);

            sessionData.AddToLog(webTokenPayload);

            InvokeMicroGameOpened(webTokenPayload);    
        }

        private void PageView(string jsonMessage)
        {
            var dto = jsonMessage.FromJson<PageViewDTO>(false);

            sessionData.TryGetLatestOrganisationId(out Guid latestOrganisationId);
            sessionData.TryGetLatestUserId(out Guid latestUserId);

            sessionData.AddToLog(dto.data);

            var isDifferentOrganisation = false;
            var isDifferentUser = false;

            if(latestOrganisationId != Guid.Empty &&
                sessionData.TryGetLatestOrganisationId(out Guid organisationId))
            {
                isDifferentOrganisation = latestOrganisationId != organisationId;
            }
            
            if(latestUserId != Guid.Empty &&
                sessionData.TryGetLatestUserId(out Guid userId))
            {
                isDifferentUser = latestUserId != userId;
            }

            if(isDifferentOrganisation || isDifferentUser)
            {
                sessionData.ClearData();
                sessionData.AddToLog(dto.data);
            }

            if(!GHaveUserCredentials())
            {
                isUserActive = false;
                sessionData.ClearData();

                //re-add the page view to the log
                sessionData.AddToLog(dto.data);
            } else
            {
                isUserActive = true;
            }

            OnPageView?.Invoke();

            if(!sessionData.TryGetLatestSubdomain(out _) && 
                sessionData.TryGetLatestOrganisationId(out _))
            {
                StartCoroutine(gamificationPlayerEndpoints.CoGetOrganisation());

                StartCoroutine(gamificationPlayerEndpoints.CoGetActiveBattle()); 
            }

            if(!sessionData.TryGetLatest<UserName>(out string _) && 
                GHaveUserCredentials())
            {
                StartCoroutine(gamificationPlayerEndpoints.CoGetUser());
            }

            if(!sessionData.TryGetLatest<UserScore>(out int _) && 
                GHaveUserCredentials())
            {
                StartCoroutine(gamificationPlayerEndpoints.CoGetUserStatistics());
            }
        }

        private void GClearData()
        {
            sessionData.ClearData();
        }

        private bool GIsUserActive()
        {
            return isUserActive;
        }

        private bool GHaveUserCredentials()
        {
            return sessionData.TryGetLatestUserId(out _) &&
                sessionData.TryGetLatestOrganisationId(out _);
        }

        private bool GTryGetActiveUserId(out Guid id)
        {
            if(GHaveUserCredentials())
            {
                return sessionData.TryGetLatestUserId(out id);
            }

            id = default;

            return false;
        }

        private bool GIsMicroGameActive()
        {
            return currentMicroGamePayload != null;
        }

        private bool GTryGetLatestModuleId(out Guid id)
        {
            return sessionData.TryGetLatestModuleId(out id);
        }

        private void GRestartMicroGame()
        {
            if(finishedMicroGamePayload == null)
            {
                // MicroGame is still running
                // We can savely restart without any issues
                return;
            }

            currentMicroGamePayload = finishedMicroGamePayload;

            GTryGetServerTime(out latestStartedGame);
        }

        private void GStopMicroGame(int score, bool isCompleted, AppScoresCallback onDone = null)
        {
            GTryGetServerTime(out DateTime now);

            if(currentMicroGamePayload == null)
            {
                Debug.LogError("No MicroGame playload to end!!");
                return;
            }
            
            StartCoroutine(gamificationPlayerEndpoints.CoAppScores(latestStartedGame, now, score, isCompleted, (result, gotoLinkUrl) =>
            {
                finishedMicroGamePayload = currentMicroGamePayload;

                currentMicroGamePayload = null;

                onDone?.Invoke(result, gotoLinkUrl);
            }, currentMicroGamePayload.integration));
        }

        private void GAddTakeAwayResult(byte[] fileData, StoreTakeAwayResultCallback callback)
        {
            GTryGetServerTime(out DateTime now);

            if(currentMicroGamePayload == null)
            {
                callback?.Invoke(UnityWebRequest.Result.ProtocolError);
                Debug.LogError("No MicroGame playload to add the take away result!!");
                return;
            }

            StartCoroutine(gamificationPlayerEndpoints.CoCreateTakeAwaySession(latestStartedGame, now, (result, dto) =>
            {
                if(result == UnityWebRequest.Result.Success)
                {
                    StartCoroutine(gamificationPlayerEndpoints.CoStoreTakeAwayResult(fileData, (result) =>
                    {
                        callback?.Invoke(result);
                    }));
                } else
                {
                    callback?.Invoke(UnityWebRequest.Result.ProtocolError);
                    Debug.LogError("Failed to create take away session!!");
                }
            }));
        }

        protected void InvokeModuleStart(Guid moduleId)
        {
            OnModuleStart?.Invoke(moduleId);
        }

        protected void InvokeMicroGameOpened(MicroGamePayload microGame)
        {
            GTryGetServerTime(out latestStartedGame);

            currentMicroGamePayload = microGame;

            finishedMicroGamePayload = null;

            OnMicroGameOpened?.Invoke(microGame);
        }
        
        protected IEnumerator ActionAfterXSeconds(Action action, float seconds)
        {
            yield return new WaitForSeconds(seconds);

            action?.Invoke();
        }
    }
}