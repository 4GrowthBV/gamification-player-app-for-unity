using System;
using System.Globalization;

namespace GamificationPlayer.DTO.TakeAway
{
    public class UpdateTakeAwaySessionRequestDTO
    {
        [Serializable]
        public class Attributes
        {
            public DateTime StartedAt
            {
                get
                {
                    var date = started_at.Replace("Z", "");

                    return DateTime.ParseExact(date, 
                        "yyyy-MM-ddTHH:mm:ss", 
                        CultureInfo.InvariantCulture);
                }
            }

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

            public string started_at;

            public string ended_at;
            
            public Attributes(DateTime startedAt,
                DateTime endedAt)
            {
                this.started_at = startedAt.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                this.ended_at = endedAt.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }

            public float Time { get; set; }

            public string type = "take_away_session";

            public Attributes attributes;

            public Data(DateTime startedAt,
                DateTime endedAt)
            {
                attributes = new Attributes(startedAt, endedAt);
            }
        }

        public Data data;

        public UpdateTakeAwaySessionRequestDTO(DateTime startedAt,
            DateTime endedAt)
        {
            data = new Data(startedAt, endedAt);
        }
    }
}
