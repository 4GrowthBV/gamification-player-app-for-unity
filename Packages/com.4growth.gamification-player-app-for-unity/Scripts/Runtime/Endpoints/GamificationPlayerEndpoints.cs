using System;
using System.IO;
using GamificationPlayer.DTO.MicroGame;
using GamificationPlayer.DTO.TakeAway;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public delegate void GetDeviceFlowCallback(UnityWebRequest.Result result, bool isValidated, string userId);

    public delegate void AnnounceDeviceFlowCallback(UnityWebRequest.Result result, string loginUrl);

    public delegate void GetModuleSessionIdCallback(UnityWebRequest.Result result, Guid moduleSessionId);

    public delegate void GetModuleSessionCallback(UnityWebRequest.Result result);

    public delegate void GetOrganisationCallback(UnityWebRequest.Result result, GetOrganisationResponseDTO dto);

    public delegate void GetMicroGameCallback(UnityWebRequest.Result result, GetMicroGameResponseDTO dto);

    public delegate void GetUserStatisticsCallback(UnityWebRequest.Result result);

    public delegate void EndModuleSessionCallback(UnityWebRequest.Result result);

    public delegate void AppScoresCallback(UnityWebRequest.Result result, string gotoPageUrl);

    public delegate void EndChallengeSessionCallback(UnityWebRequest.Result result);

    public delegate void GetLoginTokenCallback(UnityWebRequest.Result result, string token);

    public delegate void GetActiveBattleCallback(UnityWebRequest.Result result);

    public delegate void GetTakeAwaySessionCallback(UnityWebRequest.Result result, GetTakeAwaySessionsResponseDTO dto);

    public delegate void CreateTakeAwaySessionCallback(UnityWebRequest.Result result, TakeAwaySessionResponseDTO dto);

    public delegate void StoreTakeAwayResultCallback(UnityWebRequest.Result result);

    public delegate void UpdateTakeAwaySessionCallback(UnityWebRequest.Result result, TakeAwaySessionResponseDTO dto);
    
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

        private UnityWebRequest GetUnityWebRequestPOSTZip(string webRequestString, byte[] fileData, string fileName = "takeaway.zip")
        {
            UnityWebRequest webRequest;

            if (fileData == null || fileData.Length == 0)
            {
                throw new ArgumentException("File data cannot be null or empty.", nameof(fileData));
            }

            if (environmentConfig.IsMockServer)
            {
                // For the mock server, use GET
                webRequest = UnityWebRequest.Get(webRequestString);
            }
            else
            {
                // Create a multipart form data request
                WWWForm form = new WWWForm();
                form.AddBinaryData("file", fileData, fileName, "application/zip");

                webRequest = UnityWebRequest.Post(webRequestString, form);
            }

            return webRequest;
        }
    }
}