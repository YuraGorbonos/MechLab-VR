using System;
using System.Collections.Generic;
using Mirror;
using SkillForge.Core;
using SkillForge.Players;
using SkillForge.UI;
using UnityEngine;
using Zenject;

namespace SkillForge.Networking
{
    public class CustomNetworkManager : NetworkManager
    {
        [Inject] private IPlayerManager _playerManager;
        [Inject] private GameManager _gameManager;

        public ServerUI serverUI;

        public event Action<int> OnPlayerJoined;
        public event Action<int> OnPlayerLeft;

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
            OnPlayerJoined?.Invoke(playerId);
            serverUI?.AddLog($"Player {playerId} connected.");
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
            OnPlayerLeft?.Invoke(playerId);
            serverUI?.AddLog($"Player {playerId} disconnected.");

            base.OnServerDisconnect(conn);
        }

        [Server]
        public void StartSession(SessionConfig config)
        {
            if (_gameManager != null)
            {
                _gameManager.StartSession(config);
                serverUI?.UpdateSessionState(true);
                serverUI?.AddLog($"Session started: {config.scenarioId} ({config.mode})");
            }
            else
            {
                Debug.LogError("[CustomNetworkManager] GameManager not found.");
                serverUI?.AddLog("Error: GameManager not found.");
            }
        }
    }
}
