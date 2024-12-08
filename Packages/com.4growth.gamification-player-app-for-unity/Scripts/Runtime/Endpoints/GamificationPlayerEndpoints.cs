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

        private UnityWebRequest GetUnityWebRequestPOSTZip(string webRequestString, string zipFilePath)
        {
            UnityWebRequest webRequest;

            // Resolve the full path to the file in StreamingAssets
            string fullPath = Path.Combine(Application.streamingAssetsPath, zipFilePath);

            if (File.Exists(fullPath))
            {
                // Read the file into a byte array
                byte[] fileData = File.ReadAllBytes(fullPath);

                if (environmentConfig.IsMockServer)
                {
                    // For the mock server, use GET
                    webRequest = UnityWebRequest.Get(webRequestString);
                }
                else
                {
                    // Create a multipart form data request
                    WWWForm form = new WWWForm();
                    form.AddBinaryData("file", fileData, Path.GetFileName(fullPath), "application/zip");

                    webRequest = UnityWebRequest.Post(webRequestString, form);
                }
            }
            else
            {
                throw new FileNotFoundException($"The file at path {fullPath} does not exist.");
            }

            return webRequest;
        }
    }
}