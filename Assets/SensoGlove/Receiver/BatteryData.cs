using System;
using UnityEngine;

namespace Senso
{
    [Serializable]
    public class BatteryData
    {
        public string type;
        public int level;
    }

    [Serializable]
    public class BatteryDataFull
    {
        public BatteryData data;
    }
}

