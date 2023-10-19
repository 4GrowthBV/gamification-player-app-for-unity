using System;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public delegate void GetDeviceFlowCallback(UnityWebRequest.Result result, bool isValidated, string userId);

    public delegate void AnnounceDeviceFlowCallback(UnityWebRequest.Result result, string loginUrl);

    public delegate void GetModuleSessionIdCallback(UnityWebRequest.Result result, Guid moduleSessionId);

    public delegate void GetOrganisationCallback(UnityWebRequest.Result result, GetOrganisationResponseDTO dto);

    public delegate void EndModuleSessionCallback(UnityWebRequest.Result result);

    public delegate void AppScoresCallback(UnityWebRequest.Result result);

    public delegate void EndChallengeSessionCallback(UnityWebRequest.Result result);

    public delegate void GetLoginTokenCallback(UnityWebRequest.Result result, string token);

    public delegate void GetActiveBattleCallback(UnityWebRequest.Result result);

    public delegate void GetUserCallback(UnityWebRequest.Result result, GetUserResponseDTO dto);

    public delegate void GetOpenBattleInvitationsForUserCallback(UnityWebRequest.Result result, int total);


    public partial class GamificationPlayerEndpoints
    {
        private class ForceAcceptAll : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }

        public EnvironmentConfig EnvironmentConfig
        {
            get
            {
                return environmentConfig;
            }
        }

        private EnvironmentConfig environmentConfig;
        private ISessionLogData sessionData;

        public GamificationPlayerEndpoints(EnvironmentConfig environmentConfig,
            ISessionLogData sessionData)
        {
            this.environmentConfig = environmentConfig;
            this.sessionData = sessionData;
        }

        private UnityWebRequest GetUnityWebRequestPOST(string webRequestString, string data)
        {
            UnityWebRequest webRequest;
            if(environmentConfig.IsMockServer)
            {
                webRequest = UnityWebRequest.Get(webRequestString);
            } else
            {   
                webRequest = UnityWebRequest.Put(webRequestString, data);
                webRequest.method = "POST";
            }

            return webRequest;
        }
    }
}