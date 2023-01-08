using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamificationPlayer
{
    public interface ILoggableData
    {
        public string Type { get; }

        public float Time { get; set; }
    }
}
