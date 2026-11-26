using Mirror;
using SkillForge.Core;
using UnityEngine;

namespace SkillForge.Networking
{
    public static class NetworkMessageSender
    {
        public static void SendIntention(Intention intention)
        {
            if (NetworkClient.active)
            {
                NetworkClient.Send(new IntentionMessage { intention = intention });
            }
            else
            {
                Debug.LogError("[NetworkMessageSender] Not connected to server.");
            }
        }

        public static void SendEndSession()
        {
            if (NetworkClient.active)
            {
                NetworkClient.Send(new EndSessionMessage());
            }
            else
            {
                Debug.LogError("[NetworkMessageSender] Not connected to server.");
            }
        }
    }
}
