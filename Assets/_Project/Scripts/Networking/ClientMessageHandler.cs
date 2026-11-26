using Mirror;
using SkillForge.UI;
using UnityEngine;

namespace SkillForge.Networking
{
    public class ClientMessageHandler : NetworkBehaviour
    {
        private void Start()
        {
            if (isClientOnly || isLocalPlayer)
            {
                NetworkClient.RegisterHandler<UpdateUIMessage>(OnUIUpdate);
            }
        }

        private void OnUIUpdate(UpdateUIMessage msg)
        {
            var tablet = FindObjectOfType<TabletUI>();
            if (tablet != null)
            {
                tablet.UpdateUI(msg.instruction, msg.stepIndex);
            }
        }
    }
}
