using System;
using GamificationPlayer.DTO.ExternalEvents;
using UnityEngine;

namespace GamificationPlayer
{
    public class GamificationPlayerManagerForWebGL : GamificationPlayerManager
    {
        public void Start()
        {
            if(!string.IsNullOrEmpty(Application.absoluteURL))
            {
                Uri url = new Uri(Application.absoluteURL);
                var query = System.Web.HttpUtility.ParseQueryString(url.Query);
                string jwt = query["moduleData"];

                var json = JWTHelper.GetJSONWebTokenPayload(jwt, GamificationPlayerConfig.EnviromentConfig.JSONWebTokenSecret);

                var dto = json.FromJson<MicroGamePayload>();

                sessionData.AddToLog(dto);

                if(sessionData.TryGetLatestModuleId(out Guid id))
                {
                    InvokeModuleStart(id);
                }

                if(sessionData.TryGetLatestMicroGamePayload(out MicroGamePayload microGamePayload))
                {
                    InvokeMicroGameOpened(microGamePayload);
                }
            }
        }
    }
}