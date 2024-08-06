using System;
using System.Globalization;
using UnityEngine;

namespace GamificationPlayer.DTO.ChallengeSession
{
    public class UpdateChallendeSessionRequestDTO
    {
        [Serializable]
        public class Attributes
        {
            public DateTime EndedAt
            {
                get
                {
                    var date = ended_at.Replace("Z", "");

                    return DateTime.ParseExact(date, 
                        "yyyy-MM-ddTHH:mm:ss", 
                        CultureInfo.InvariantCulture);
                }
            }
            public string ended_at;

            public DateTime? CompletedAt
            {
                get
                {
                    if(string.IsNullOrEmpty(completed_at))
                    {
                        return null;
                    }

                    var date = completed_at.Replace("Z", "");

                    return DateTime.ParseExact(date, 
                        "yyyy-MM-ddTHH:mm:ss", 
                        CultureInfo.InvariantCulture);
                }
            }

            #nullable enable
            public string? completed_at;
            #nullable disable

            public Attributes(DateTime endedAt, DateTime? completedAt)
            {
                this.ended_at = endedAt.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                this.completed_at = completedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }
            public string type = "challenge_session";
        
            public Attributes attributes;

            public Data(DateTime endedAt, DateTime? completedAt)
            {
                attributes = new Attributes(endedAt, completedAt);
            }            
        }

        public Data data;

        public UpdateChallendeSessionRequestDTO(DateTime endedAt, DateTime? completedAt)
        {
            data = new Data(endedAt, completedAt);
        }
    }
}