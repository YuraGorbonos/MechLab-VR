using System.Collections.Generic;
using Mirror;
using SkillForge.Core;
using SkillForge.Players;
using UnityEngine;
using Zenject;

namespace SkillForge.Networking
{
    public class CustomNetworkManager : NetworkManager
    {
        [Inject] private IPlayerManager _playerManager;

        private readonly Dictionary<int, HashSet<string>> _playerObjects;

        public CustomNetworkManager()
        {
            _playerObjects = new Dictionary<int, HashSet<string>>();
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);

            int playerId = conn.connectionId;
            _playerObjects[playerId] = new HashSet<string>();

            Debug.Log($"[CustomNetworkManager] Player {playerId} connected.");
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            int playerId = conn.connectionId;

            if (_playerObjects.TryGetValue(playerId, out var objects))
            {
                foreach (var objectId in objects)
                {
                    _playerManager.ReleaseObject(playerId, objectId);
                }

                objects.Clear();
            }

            _playerObjects.Remove(playerId);

            Debug.Log($"[CustomNetworkManager] Player {playerId} disconnected.");

            base.OnServerDisconnect(conn);
        }

        [Server]
        public void StartSession(SessionConfig config)
        {
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.StartSession(config);
            }
            else
            {
                Debug.LogError("[CustomNetworkManager] GameManager not found in scene.");
            }
        }
    }
}
