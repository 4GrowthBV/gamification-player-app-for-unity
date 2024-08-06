// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using System;
using System.Globalization;

namespace GamificationPlayer.DTO.ModuleSession
{
    public class UpdateModuleSessionRequestDTO
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

            public string ended_at;

            #nullable enable
            public string? completed_at;
            #nullable disable
            
            public int score;

            public Attributes(DateTime endedAt, int score, DateTime? completedAt)
            {
                this.ended_at = endedAt.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                this.score = score;
                this.completed_at = completedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }

            public string type = "module_session";

            public Attributes attributes;

            public Data(DateTime endedAt, int score, DateTime? completedAt)
            {
                this.attributes = new Attributes(endedAt, score, completedAt);
            }
        }

        public Data data;

        public UpdateModuleSessionRequestDTO(DateTime endedAt, 
            int score = 0, 
            DateTime? completedAt = null)
        {
            this.data = new Data(endedAt, score, completedAt);
        }
    }
}