using Mirror;
using SkillForge.Core;
using SkillForge.Networking;
using UnityEngine;
using Zenject;

namespace SkillForge.VR
{
    public class PlayerController : NetworkBehaviour
    {
        [Inject] private GameManager _gameManager;

        private int _playerId;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            _playerId = connectionToClient.connectionId;
            Debug.Log($"[PlayerController] Local player started: {_playerId}");
        }

        [Command]
        public void CmdSendIntention(Intention intent)
        {
            intent.playerId = _playerId;
            _gameManager.ProcessIntention(intent);
        }
    }
}
