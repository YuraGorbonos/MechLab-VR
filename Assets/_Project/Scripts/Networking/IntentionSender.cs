using Mirror;
using SkillForge.Core;
using UnityEngine;
using Zenject;

namespace SkillForge.Networking
{
    public class IntentionSender : NetworkBehaviour
    {
        [Inject(Optional = true)]
        private GameManager _gameManager;

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (_gameManager == null)
            {
                AppContainer.ServerContainer?.Inject(this);
            }
        }

        [Command]
        public void CmdSendIntention(Intention intent)
        {
            Debug.Log($"[IntentionSender] CmdSendIntention received: type={intent.intentType}, target={intent.targetObject}, playerId={intent.playerId}");

            if (_gameManager != null)
            {
                _gameManager.ProcessIntention(intent);
            }
            else
            {
                Debug.LogError("[IntentionSender] GameManager not resolved.");
            }
        }

        [Command]
        public void CmdRequestEndSession()
        {
            if (_gameManager != null)
            {
                _gameManager.CmdRequestEndSession();
            }
            else
            {
                Debug.LogError("[IntentionSender] GameManager not resolved for EndSession.");
            }
        }
    }
}