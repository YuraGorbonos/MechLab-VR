using Mirror;
using SkillForge.Core;
using SkillForge.Networking;
using UnityEngine;
using Zenject;

namespace SkillForge.Networking
{
    public class IntentionSender : NetworkBehaviour
    {
        [Inject] private GameManager _gameManager;

        public override void OnStartServer()
        {
            base.OnStartServer();
            AppContainer.ServerContainer?.Inject(this);
        }

        [Command]
        public void CmdSendIntention(Intention intent)
        {
            if (_gameManager != null)
            {
                _gameManager.ProcessIntention(intent);
            }
            else
            {
                Debug.LogError("[IntentionSender] GameManager not resolved.");
            }
        }
    }
}
