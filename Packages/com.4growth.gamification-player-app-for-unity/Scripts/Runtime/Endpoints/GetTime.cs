using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetTime(Action<UnityWebRequest.Result, DateTime> onReady)
        {
            string webRequestString = string.Format("{0}/time", environmentConfig.API_URL);

            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if(environmentConfig.TryGetMockDTO<TimeResponseDTO>(out var dto))
            {
                sessionData.AddToLog(dto.data, false);
                dto.data.attributes.now = DateTime.Now.ToString();
                var dateTime = dto.data.attributes.now;
                DateTime dt = DateTime.Parse(dateTime);
                Debug.Log(dto.data.attributes.now);
                onReady?.Invoke(UnityWebRequest.Result.Success, dt);
            } 
            else
            {
                UnityWebRequest webRequest = UnityWebRequest.Get(webRequestString);
                webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.certificateHandler = new ForceAcceptAll();

                yield return webRequest.SendWebRequest();

                var dateTime = DateTime.Now.ToString();

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
                        var obj = webRequest.downloadHandler.text.FromJson<TimeResponseDTO>();
                        sessionData.AddToLog(obj.data, false);           
                        dateTime = obj.data.attributes.now;
                        break;
                }
                
                DateTime dt = DateTime.Parse(dateTime);
                onReady?.Invoke(webRequest.result, dt);
            }
        }
    }
}
