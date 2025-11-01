using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace GamificationPlayer
{
    public interface ILoggableData
    {
        public string Type { get; }
        
        [JsonIgnore]
        public float Time { get; set; }
    }
}
