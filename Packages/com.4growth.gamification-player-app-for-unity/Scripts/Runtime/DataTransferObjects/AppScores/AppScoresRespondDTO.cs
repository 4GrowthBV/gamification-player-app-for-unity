using System;
using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.AppScores
{
    public class AppScoresRespondDTO
    {
        [Serializable]
        public class Attributes
        {
            public DateTime StartedAt
            {
                get
                {
                    return DateTime.Parse(started_at);
                }
            }

            public DateTime? EndedAt
            {
                get
                {
                    if(string.IsNullOrEmpty(ended_at))
                    {
                        return null;
                    }

                    return DateTime.Parse(ended_at);
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

                    return DateTime.Parse(completed_at);
                }
            }

            public string started_at;

            #nullable enable
            public string? ended_at;

            [MicroGameCompletedAt]
            public string? completed_at;
            #nullable disable

            [MicroGameScore]
            public int score;
        }

        [Serializable]
        public class Link
        {
            [GotoLinkUrl]
            public string show;
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }
            
            public string id;
            public string type;
            public Attributes attributes;
            public Link links;
        }

        public Data data;
    }
}
