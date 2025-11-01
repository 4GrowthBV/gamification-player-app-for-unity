using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.Session;
using Newtonsoft.Json;
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

            [OrganisationDefaultLanguage]
            public string default_language;

            public List<string> available_languages;

            [OrganisationLogo]
            public string logo;

            [OrganisationPrimaryColor]
            public string primary_color;

            [OrganisationSecondaryColor]
            public string secondary_color;

            [OrganisationAccentColor]
            public string accent_color;

            [OrganisationPrimaryFontFamily]
            public string primary_font_family;

            [OrganisationSecondaryFontFamily]
            public string secondary_font_family;

            [OrganisationPrimaryFontColor]
            public string primary_font_color;

            [OrganisationSecondaryFontColor]
            public string secondary_font_color;

            [OrganisationBackgroundColor]
            public string background_color;

            [OrganisationBackgroundImage]
            public string background_image;

            [OrganisationMobileBackgroundImage]
            public string background_mobile_image;

            [OrganisationTabletBackgroundImage]
            public string background_tablet_image;

            [OrganisationDesktopBackgroundImage]
            public string background_desktop_image;

            [OrganisationCardBackgroundColor]
            public string card_background_color;

            [OrganisationCardBorderRadius]
            public string card_border_radius;

            [OrganisationOpenGraphImage]
            public string open_graph_image;

            [OrganisationOpenGraphDescription]
            public string open_graph_description;  

            [OrganisationRegistrationEnabled]          
            public bool registration_enabled;

            [OrganisationAnonymousEnabled]
            public bool anonymous_enabled;

            [OrganisationIntroduction]
            public string introduction;

            [OrganisationWebhookUrl]
            public string webhook_url;

            [OrganisationAllowUpgradeToRegisteredUser]
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            private bool allow_upgrade_to_registered_user = false;

            [OrganisationResellerEnabled]
            private bool reseller_enabled;

            [OrganisationTimezone]
            private string timezone;

            [OrganisationIosAppUrl]
            private string ios_app_url;

            [OrganisationAndroidAppUrl]
            private string android_app_url;

            [OrganisationBranchioUrl]   
            private string branchio_url;
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
