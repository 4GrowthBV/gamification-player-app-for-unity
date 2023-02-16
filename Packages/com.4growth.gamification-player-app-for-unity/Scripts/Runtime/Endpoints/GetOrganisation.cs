using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetOrganisation(GetOrganisationCallback onReady = null)
        {
            if(!sessionData.TryGetLatestOrganisationId(out var organisationId))
            {   
                onReady?.Invoke(UnityWebRequest.Result.ProtocolError, null);

                yield break;
            }

            yield return CoGetOrganisation(organisationId, onReady);
        }

        private IEnumerator CoGetOrganisation(Guid organisationId, GetOrganisationCallback onReady = null)
        {
            string webRequestString = string.Format("{0}/organisations/{1}", enviromentConfig.API_URL, organisationId);

            if(enviromentConfig.TurnOnLogging) Debug.Log(webRequestString);

            UnityWebRequest webRequest = UnityWebRequest.Get(webRequestString);
            webRequest.SetRequestHeader("X-Api-Key", enviromentConfig.APIKey);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.certificateHandler = new ForceAcceptAll();

            yield return webRequest.SendWebRequest();

            GetOrganisationResponseDTO obj = null;

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
                    if(enviromentConfig.TurnOnLogging) Debug.LogError(":\nReceived: " + webRequest.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.Success:
                    if(enviromentConfig.TurnOnLogging) Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    obj = webRequest.downloadHandler.text.FromJson<GetOrganisationResponseDTO>();
                    sessionData.AddToLog(obj.data);           
                    break;
            }

            onReady?.Invoke(webRequest.result, obj);
        }
    }
}
