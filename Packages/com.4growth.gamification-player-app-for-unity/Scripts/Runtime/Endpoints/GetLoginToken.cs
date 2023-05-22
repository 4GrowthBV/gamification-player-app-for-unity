using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.LoginToken;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetLoginToken(GetLoginTokenCallback onReady, Guid organisations = default, Guid userId = default)
        {
            if(userId == default)
            {
                sessionData.TryGetLatestUserId(out userId);
            }

            if(organisations == default)
            {
                sessionData.TryGetLatestOrganisationId(out organisations);
            }

            yield return CoGetLoginToken(organisations, userId, onReady);
        }

        public IEnumerator CoGetLoginToken(Guid organisations, Guid userId, GetLoginTokenCallback onReady)
        {
            var url = string.Format("{0}/organisations/{1}/users/{2}/login-token", environmentConfig.API_URL, organisations, userId);

            if(environmentConfig.TurnOnLogging) Debug.Log(url);

            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.certificateHandler = new ForceAcceptAll();

            yield return webRequest.SendWebRequest();

            var token = string.Empty;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError("Connection Error.");
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(": HTTP Error: " + webRequest.error);
                    if(environmentConfig.TurnOnLogging) Debug.LogError(":\nReceived: " + webRequest.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.Success:
                    if(environmentConfig.TurnOnLogging) Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    var userDataRoot = webRequest.downloadHandler.text.FromJson<GetLoginTokenResponseDTO>();
                    sessionData.AddToLog(userDataRoot.data, false);
                    token = userDataRoot.data.attributes.token;
                    break;
            }

            onReady?.Invoke(webRequest.result, token);
        }
    }
}
