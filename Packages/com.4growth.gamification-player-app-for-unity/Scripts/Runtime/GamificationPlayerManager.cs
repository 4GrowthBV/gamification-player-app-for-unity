using System;
using System.Collections;
using System.Linq;
using GamificationPlayer.DTO.ExternalEvents;
using GamificationPlayer.Session;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    /// <summary>
    /// Represents a method that is called when the device flow is started.
    /// </summary>
    /// <param name="loginUrl">The URL where the user can login via a different device.</param>
    public delegate void StartDeviceFlowCallback(string loginUrl);

    /// <summary>
    /// Represents a method that is called when there is a new loginURL.
    /// </summary>
    /// <param name="loginToken">The loginURL that can be used to log in the user in the gamification player.</param>
    public delegate void OnUserLoggedInEvent(string redirectURL);

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
        /// Occurs when there is a new login token.
        /// </summary>
        public static event OnUserLoggedInEvent OnUserLoggedIn;

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

        public static bool IsInitialized { get { return instance.isInitialized; } }
        
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
        /// Attempts to end the latest module session.
        /// </summary>
        /// <param name="score">The score of the module session.</param>
        /// <param name="isCompleted">Indicates whether the module session was completed. The module can be ended without completing if the user ends before the end.</param>
        /// <param name="onDone">An optional callback that will be called when the operation is completed. If the operation is successful, the callback will be called without any arguments. If the operation fails, the callback will be called without any arguments.</param>
        public static void StopMicroGame(int score, bool isCompleted, Action onDone = null)
        {
            instance.GStopMicroGame(score, isCompleted, onDone);
        }

        /// <summary>
        /// Starts the device flow and calls the specified callback when it is started.
        /// </summary>
        /// <param name="onStart">The callback to be called when the device flow is started.</param>
        public static void StartDeviceFlow(StartDeviceFlowCallback onStart)
        {
            instance.GStartDeviceFlow(onStart);
        }

        /// <summary>
        /// Determines whether the device flow is currently active.
        /// Device flow is being used when the user is logging via a QR code.
        /// </summary>
        /// <returns>true if the device flow is active; otherwise, false.</returns>
        public static bool IsDeviceFlowActive()
        {
            return instance.isDeviceFlowActive;
        }

        /// <summary>
        /// Stops the device flow
        /// Device flow is being used when the user is logging via a QR code.
        /// </summary>
        public static void StopDeviceFlow()
        {
            instance.GStopDeviceFlow();
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

        [SerializeField]
        private bool checkServerTimeOnStartUp = false;

        [SerializeField]
        [Header("Mock settings for testing")]
        private EnvironmentConfig gamificationPlayerMockConfig;

        [SerializeField]
        private string absoluteURLTest = string.Empty;

        [SerializeField]
        private float refreshDataEveryXSeconds = 60f;

        private float refreshDataTimer = 0f;

        protected GamificationPlayerEndpoints gamificationPlayerEndpoints;

        protected SessionLogData sessionData;

        private bool isDeviceFlowActive;

        private bool isUserActive = false;

        private bool isInitialized = false;

        private MicroGamePayload currentMicroGamePayload;

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

                var dto = json.FromJson<MicroGamePayload>();

                sessionData.AddToLog(dto);
            }
#endif

            gamificationPlayerEndpoints = new GamificationPlayerEndpoints(GGetEnvironmentConfig(), sessionData);
            
            if(checkServerTimeOnStartUp)
            {
                GGetServerOffSetTime();
            }
        }

        private string GetAbsoluteURL()
        {
            if(!string.IsNullOrEmpty(absoluteURLTest))
            {
                return absoluteURLTest;
            }

            return Application.absoluteURL;
        }

        private EnvironmentConfig GGetEnvironmentConfig()
        {
            EnvironmentConfig environmentConfig;

            if(sessionData.TryGetLatestEnvironmentDomain(out var env))
            {
                if(GamificationPlayerConfig.TryGetEnvironmentConfig(env, out environmentConfig))
                {
                    return environmentConfig;
                }
            }

            var environment = ".it";
#if PROD_BUILD
            environment = ".app";
#elif STAG_BUILD
            environment = ".eu";
#else
            environment = ".it";
#endif

            GamificationPlayerConfig.TryGetEnvironmentConfig(environment, out environmentConfig);

            return environmentConfig;
        }

        public void Start()
        {
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
                var isGetActiveBattle = false;
                var isGetUser = false;
                StartCoroutine(gamificationPlayerEndpoints.CoGetLoginToken((_, __) => { 
                    isGetLoginToken = true; 
                    if(isGetLoginToken && isGetOrganisation && isGetActiveBattle && isGetUser)
                    {
                        isInitialized = true;
                    }
                }));
                StartCoroutine(gamificationPlayerEndpoints.CoGetOrganisation((_, __) => { 
                    isGetOrganisation = true; 
                    if(isGetLoginToken && isGetOrganisation && isGetActiveBattle && isGetUser)
                    {
                        isInitialized = true;
                    }
                }));
                StartCoroutine(gamificationPlayerEndpoints.CoGetUser((_, __) => { 
                    isGetUser = true; 
                    if(isGetLoginToken && isGetOrganisation && isGetActiveBattle && isGetUser)
                    {
                        isInitialized = true;
                    }
                }));
                StartCoroutine(gamificationPlayerEndpoints.CoGetActiveBattle((_) => { 
                    isGetActiveBattle = true; 
                    if(isGetLoginToken && isGetOrganisation && isGetActiveBattle && isGetUser)
                    {
                        isInitialized = true;
                    }
                }));
            } else
            {
                isInitialized = true;
            }
#else
            isInitialized = true;
#endif
        }

        private void GRemoveListener(Action<object> callback)
        {
            sessionData.RemoveListener(callback);
        }

        private void GListenToData<TQueryable>(Action<object> callback) 
            where TQueryable : Session.IQueryable
        {
            sessionData.ListenTo<TQueryable>(callback);
        }

        private bool GTryGetLatestData<TQueryable>(out string value) 
            where TQueryable : Session.IQueryable
        {
            return sessionData.TryGetLatest<TQueryable>(out value);
        }

        private bool GTryGetLatestData<TQueryable>(out bool value) 
            where TQueryable : Session.IQueryable
        {
            return sessionData.TryGetLatest<TQueryable>(out value);
        }

        private bool GTryGetLatestData<TQueryable>(out int value) 
            where TQueryable : Session.IQueryable
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

        private void MicroGameOpened(string jsonMessage)
        {
            var dto = jsonMessage.FromJson<MicroGameOpenedDTO>();
            
            sessionData.AddToLog(dto.data);

            var JSONWebTokenPayload = JWTHelper.GetJSONWebTokenPayload(dto.data.attributes.module_data, 
                gamificationPlayerEndpoints.EnvironmentConfig.JSONWebTokenSecret);
            var webTokenPayload = JSONWebTokenPayload.FromJson<MicroGamePayload>();

            sessionData.AddToLog(webTokenPayload);

            InvokeMicroGameOpened(webTokenPayload);    

            GTryGetServerTime(out latestStartedGame);
        }

        private void PageView(string jsonMessage)
        {
            sessionData.TryGetLatestLanguage(out var previousLanguage);

            var dto = jsonMessage.FromJson<PageViewDTO>();

            sessionData.AddToLog(dto.data);

            if(isDeviceFlowActive && 
                sessionData.TryGetLatestOrganisationId(out _) && 
                sessionData.TryGetLatestUserId(out _))
            {
                StopAllCoroutines();

                gamificationPlayerEndpoints.CoGetLoginToken(GetLoginTokenResult);
            }

            if(!GHaveUserCredentials())
            {
                isUserActive = false;
                sessionData.ClearData();
            } else
            {
                isUserActive = true;
            }

            OnPageView?.Invoke();

            if(sessionData.TryGetLatestLanguage(out var language))
            {
                if(previousLanguage != language)
                {
                    OnLanguageSet?.Invoke(language);
                }
            }

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

        private void Update()
        {
            refreshDataTimer+= Time.deltaTime;

            if(refreshDataTimer > refreshDataEveryXSeconds)
            {
                refreshDataTimer = 0f;

                RefreshData();
            }
        }

        private void RefreshData()
        {
            if(sessionData.TryGetLatestOrganisationId(out _))
            {
                StartCoroutine(gamificationPlayerEndpoints.CoGetActiveBattle()); 
            }

            if(GHaveUserCredentials())
            {
                StartCoroutine(gamificationPlayerEndpoints.CoGetOpenBattleInvitationsForUser());

                StartCoroutine(gamificationPlayerEndpoints.CoGetUser());

                StartCoroutine(gamificationPlayerEndpoints.CoGetUserStatistics());
            }
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

        private void GStopMicroGame(int score, bool isCompleted, Action onDone = null)
        {
            GTryGetServerTime(out DateTime now);

            if(currentMicroGamePayload == null)
            {
                Debug.LogError("No MicroGame playload to end!!");
                return;
            }
            
            StartCoroutine(gamificationPlayerEndpoints.CoAppScores(latestStartedGame, now, score, isCompleted, (_) =>
            {
                currentMicroGamePayload = null;

                onDone?.Invoke();
            }));
        }

        private void GStartDeviceFlow(StartDeviceFlowCallback onStart)
        {
            if(isDeviceFlowActive)
            {
                GStopDeviceFlow();
            }

            isDeviceFlowActive = true;

            StartCoroutine(gamificationPlayerEndpoints.CoAnnounceDeviceFlow((result, loginUrl) =>
            {
                if(result == UnityWebRequest.Result.Success)
                {
                    onStart?.Invoke(loginUrl);

                    StartCoroutine(ActionAfterXSeconds(CheckDeviceFlow, 4f));
                } else
                {
                    onStart?.Invoke(string.Empty);
                }
            }));
        }

        private void GStopDeviceFlow()
        {
            isDeviceFlowActive = false;

            StopAllCoroutines();
        }

        private void CheckDeviceFlow()
        {
            StartCoroutine(gamificationPlayerEndpoints.CoGetDeviceFlow((result, isValidated, userId) =>
            {
                if(result == UnityWebRequest.Result.Success && isValidated)
                {
                    StartCoroutine(gamificationPlayerEndpoints.CoGetOrganisation(GetOrganisationResult));
                    StartCoroutine(gamificationPlayerEndpoints.CoGetLoginToken(GetLoginTokenResult));
                } else
                {
                    StartCoroutine(ActionAfterXSeconds(CheckDeviceFlow, 4f));
                }
            }));
        }

        private void GetOrganisationResult(UnityWebRequest.Result result, GetOrganisationResponseDTO dto)
        {
            if(result == UnityWebRequest.Result.Success)
            {
                EndLoginFlowIfCorrect();
            } else
            {
                StartCoroutine(ActionAfterXSeconds(() =>
                {
                    StartCoroutine(gamificationPlayerEndpoints.CoGetOrganisation(GetOrganisationResult));
                }, 4f));
            }
        }

        private void GetLoginTokenResult(UnityWebRequest.Result result, string token)
        {
            if(result == UnityWebRequest.Result.Success)
            {
                EndLoginFlowIfCorrect();
            } else
            {
                StartCoroutine(ActionAfterXSeconds(() =>
                {
                    StartCoroutine(gamificationPlayerEndpoints.CoGetLoginToken(GetLoginTokenResult));
                }, 4f));
            }
        }

        private void EndLoginFlowIfCorrect()
        {
            if(sessionData.TryGetLatestSubdomain(out var subdomain) &&
                sessionData.TryGetLatestLoginToken(out var loginToken))
            {
                var redirectURL = string.Format("https://{0}.{1}login?otlToken={2}", subdomain, gamificationPlayerEndpoints.EnvironmentConfig.Webpage, loginToken);

                isDeviceFlowActive = false;
                
                InvokeUserLoggedIn(redirectURL);
            }
        }

        protected void InvokeUserLoggedIn(string redirectURL)
        {
            OnUserLoggedIn?.Invoke(redirectURL);
        }

        protected void InvokeModuleStart(Guid moduleId)
        {
            OnModuleStart?.Invoke(moduleId);
        }

        protected void InvokeMicroGameOpened(MicroGamePayload microGame)
        {
            currentMicroGamePayload = microGame;

            OnMicroGameOpened?.Invoke(microGame);
        }
        
        protected IEnumerator ActionAfterXSeconds(Action action, float seconds)
        {
            yield return new WaitForSeconds(seconds);

            action?.Invoke();
        }
    }
}