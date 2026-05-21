using Mirror;

namespace SkillForge.Networking
{
    public struct GameNetworkMessage : NetworkMessage
    {
        public string type;
        public string payload;
    }
}

