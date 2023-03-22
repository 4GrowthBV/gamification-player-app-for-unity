using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.ChallengeSession;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoEndChallengeSession(bool isCompleted, EndChallengeSessionCallback onReady)
        {
            if(!sessionData.TryGetLatestChallengeSessionId(out var challengeSessionId))
            {   
                onReady?.Invoke(UnityWebRequest.Result.ProtocolError);

                yield break;
            }

            yield return CoEndChallengeSession(challengeSessionId, isCompleted, onReady);
        }

        private IEnumerator CoEndChallengeSession(Guid challengeSessionId, bool isCompleted, EndChallengeSessionCallback onReady)
        {
            DateTime? completedAt = null;
            if(isCompleted)
            {
                completedAt = DateTime.Now;
            }
            
            var challengeSession = new UpdateChallendeSessionRequestDTO(DateTime.Now, completedAt);
            sessionData.AddToLog(challengeSession.data);

            string data = challengeSession.ToJson();
            
            string webRequestString = string.Format("{0}/challenge-sessions/{1}?_method=PATCH", environmentConfig.API_URL, challengeSessionId);

            if(environmentConfig.TurnOnLogging) Debug.Log(data);
            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            UnityWebRequest webRequest = GetUnityWebRequestPOST(webRequestString, data);
            webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.certificateHandler = new ForceAcceptAll();

            Debug.Log(webRequestString);

            yield return webRequest.SendWebRequest();

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
                    var obj = webRequest.downloadHandler.text.FromJson<UpdateChallengeSessionResponseDTO>();
                    sessionData.AddToLog(obj.data);
                    break;
            }

            onReady?.Invoke(webRequest.result);
        }
    }
}
