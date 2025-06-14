using System;
using UnityEngine;

namespace Utils
{
    [Serializable, CreateAssetMenu(fileName = "URLConfig", menuName = "MyConfigs/URLConfig", order = 0)]
    public class URLConfig : ScriptableObject
    {
        public string LocalesURL;
    }
}