using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.Session;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoOfflineSync(List<DTO.OfflineSync.OfflineSyncRequestDTO.DataItem> dataItems,
            OfflineSyncCallback onReady = null)
        {
            sessionData.TryGetLatestId<OrganisationId>(out var organisationId);
            sessionData.TryGetLatestId<UserId>(out var userId);

            var requestDto = new DTO.OfflineSync.OfflineSyncRequestDTO();
            requestDto.Meta = new DTO.OfflineSync.OfflineSyncRequestDTO.MetaData();
            requestDto.Meta.OrganisationId = organisationId.ToString();
            requestDto.Meta.UserId = userId.ToString();
            requestDto.Data = dataItems;

            yield return CoOfflineSync(requestDto, onReady);
        }

        public IEnumerator CoOfflineSync(DTO.OfflineSync.OfflineSyncRequestDTO requestDto, OfflineSyncCallback onReady = null)
        {
            string url = string.Format("{0}/offline-sync", environmentConfig.API_URL);

            string json = requestDto.ToJson();
            if (environmentConfig.TurnOnLogging)
                Debug.Log($"{url}\nRequest: {json}");

            if (environmentConfig.TryGetMockDTO<DTO.OfflineSync.OfflineSyncResponseDTO>(out var mockDto))
            {
                onReady?.Invoke(UnityWebRequest.Result.Success, mockDto);
                yield break;
            }

            UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.certificateHandler = new ForceAcceptAll();

            yield return webRequest.SendWebRequest();

            DTO.OfflineSync.OfflineSyncResponseDTO responseDto = null;
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
                    if (environmentConfig.TurnOnLogging)
                        Debug.LogError(":\nReceived: " + webRequest.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.Success:
                    if (environmentConfig.TurnOnLogging)
                        Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    responseDto = webRequest.downloadHandler.text.FromJson<DTO.OfflineSync.OfflineSyncResponseDTO>();
                    break;
            }

            onReady?.Invoke(webRequest.result, responseDto);
        }
    }
}
