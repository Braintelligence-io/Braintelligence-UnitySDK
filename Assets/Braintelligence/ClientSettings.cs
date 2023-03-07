using UnityEngine;

namespace Braintelligence
{
    [CreateAssetMenu(menuName = "Create ClientSettings", fileName = "ClientSettings", order = 0)]
    public class ClientSettings : ScriptableObject
    {
        public string GameKey = "12345";
        public string LocalServerIP = string.Empty;
        public int LocalServerPort = 9050;
        public Log.Level LogLevel;
    }
}