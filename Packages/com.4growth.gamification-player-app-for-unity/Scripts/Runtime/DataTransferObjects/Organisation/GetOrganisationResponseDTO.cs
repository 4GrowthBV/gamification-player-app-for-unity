using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.Session;
using UnityEngine;

namespace GamificationPlayer
{
    public class GetOrganisationResponseDTO
    {
        [Serializable]
        public class Attributes
        {
            [OrganisationName]
            public string name;

            [OrganisationSubdomain]
            public string subdomain;

            [OrganisationWebhookUrl]
            public string webhook_url;

            [OrganisationPrimaryColor]
            public string primary_color;
            public bool registration_enabled;
            public bool anonymous_enabled;
            public string introduction;

            [OrganisationDefaultLanguage]
            public string default_language;
            public List<string> available_languages;
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }

            public float Time { get; set; }

            [OrganisationId]
            public string id;
            
            public string type;
            public Attributes attributes;

            public Data()
            {
                attributes = new Attributes();
            }
        }

        public Data data;

        public GetOrganisationResponseDTO()
        {
            data = new Data();
        }
    }
}
