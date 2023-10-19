using System;
using System.Collections;
using System.Linq;
using GamificationPlayer.DTO.Battle;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetOpenBattleInvitationsForUser(GetOpenBattleInvitationsForUserCallback onReady = null)
        {
            if(!sessionData.TryGetLatestOrganisationId(out var organisationId))
            {   
                onReady?.Invoke(UnityWebRequest.Result.ProtocolError, 0);

                yield break;
            }

            if(!sessionData.TryGetLatestUserId(out var userId))
            {   
                onReady?.Invoke(UnityWebRequest.Result.ProtocolError, 0);

                yield break;
            }

            yield return CoGetOpenBattleInvitationsForUser(organisationId, userId, onReady);
        }

        private IEnumerator CoGetOpenBattleInvitationsForUser(Guid organisationId, Guid userId, GetOpenBattleInvitationsForUserCallback onReady = null)
        {
            string webRequestString = string.Format("{0}/organisations/{1}", environmentConfig.API_URL, organisationId);

            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if(environmentConfig.TryGetMockDTO<GetOpenBattleInvitationsForUserDTO>(out var dto))
            {
                var dummy = new TotalOpenBattleInvitationForUserDTO
                {
                    total = dto.data.Count()
                };
                sessionData.AddToLog(dummy, false);   
                onReady?.Invoke(UnityWebRequest.Result.Success, dto.data.Count());
            } 
            else
            {
                UnityWebRequest webRequest = UnityWebRequest.Get(webRequestString);
                webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.certificateHandler = new ForceAcceptAll();

                yield return webRequest.SendWebRequest();

                GetOpenBattleInvitationsForUserDTO obj = null;

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
                        obj = webRequest.downloadHandler.text.FromJson<GetOpenBattleInvitationsForUserDTO>();
                        var dummy = new TotalOpenBattleInvitationForUserDTO
                        {
                            total = obj.data.Count()
                        };
                        sessionData.AddToLog(dummy, false);           
                        break;
                }
                var total = obj == null ? 0 : obj.data.Count();
                onReady?.Invoke(webRequest.result, total);
            }
        }
    }
}
