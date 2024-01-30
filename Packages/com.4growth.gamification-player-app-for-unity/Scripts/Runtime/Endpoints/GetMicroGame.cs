using System;
using System.Collections;
using GamificationPlayer.DTO.MicroGame;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetMicroGame(Guid microGameId, GetMicroGameCallback onReady = null)
        {
            string webRequestString = string.Format("{0}/micro-games/{1}", environmentConfig.API_URL, microGameId);

            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if(environmentConfig.TryGetMockDTO<GetMicroGameResponseDTO>(out var dto))
            {
                sessionData.AddToLog(dto.data, false);
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

                GetMicroGameResponseDTO obj = null;

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
                        obj = webRequest.downloadHandler.text.FromJson<GetMicroGameResponseDTO>();
                        sessionData.AddToLog(obj.data, false);           
                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}
