using System;
using System.Collections;
using System.Linq;
using GamificationPlayer.DTO.ExternalEvents;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public class GamificationPlayerManagerForWebGL : GamificationPlayerManager
    {
        public void Start()
        {
            Uri url = new Uri(Application.absoluteURL);
            var query = System.Web.HttpUtility.ParseQueryString(url.Query);
            string jwt = query["moduleData"];

            var json = JWTHelper.GetJSONWebTokenPayload(jwt, GamificationPlayerConfig.EnviromentConfig.JSONWebTokenSecret);

            var dto = json.FromJson<MicroGamePayload>();

            sessionData.AddToLog(dto);

            GetLoginToken();
        }

        private void GetLoginToken()
        {
            StartCoroutine(gamificationPlayerEndpoints.CoGetLoginToken(GetLoginTokenResult));
        }

        private void GetLoginTokenResult(UnityWebRequest.Result result, string token)
        {
            if(result == UnityWebRequest.Result.Success)
            {
                InvokeUserLoggedIn("");

                if(sessionData.TryGetLatestModuleId(out Guid id))
                {
                    InvokeModuleStart(id);
                }

                if(sessionData.TryGetLatestMicroGamePayload(out MicroGamePayload microGamePayload))
                {
                    InvokeMicroGameOpened(microGamePayload);
                }
            } else
            {
                StartCoroutine(ActionAfterXSeconds(() =>
                {
                    StartCoroutine(gamificationPlayerEndpoints.CoGetLoginToken(GetLoginTokenResult));
                }, 4f));
            }
        }
    }
}