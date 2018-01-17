using System;
using UnityEngine;

namespace Senso
{
    [Serializable]
    public class NetData
    {
        public string src;
        public string name;
        public string fullname;
        public string type;
        [NonSerialized]
        public string packet;
    }
}
