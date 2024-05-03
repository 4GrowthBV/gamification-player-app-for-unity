using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetUser(GetUserCallback onReady = null)
        {
            if(!sessionData.TryGetLatestUserId(out var userId))
            {   
                onReady?.Invoke(UnityWebRequest.Result.ProtocolError, new GetUserResponseDTO());

                yield break;
            }

            yield return CoGetUser(userId, onReady);
        }

        private IEnumerator CoGetUser(Guid userId, GetUserCallback onReady = null)
        {
            string webRequestString = string.Format("{0}/users/{1}", environmentConfig.API_URL, userId);

            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if(environmentConfig.TryGetMockDTO<GetUserResponseDTO>(out var dto))
            {
                sessionData.AddToLog(dto.data, false);
                sessionData.AddToLog(new UserTagsDataHelper(dto), false);
                onReady?.Invoke(UnityWebRequest.Result.Success, dto);
            } 
            else
            {
                UnityWebRequest webRequest = UnityWebRequest.Get(webRequestString);
                webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.certificateHandler = new ForceAcceptAll();

                yield return webRequest.SendWebRequest();

                GetUserResponseDTO obj = null;

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
                        obj = webRequest.downloadHandler.text.FromJson<GetUserResponseDTO>(false);
                        sessionData.AddToLog(obj.data, false);           
                        sessionData.AddToLog(new UserTagsDataHelper(obj), false);
                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}