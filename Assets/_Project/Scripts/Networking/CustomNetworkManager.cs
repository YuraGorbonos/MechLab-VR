using System;
using System.Collections.Generic;
using Mirror;
using SkillForge.Core;
using SkillForge.UI;
using UnityEngine;
using Zenject;

namespace SkillForge.Networking
{
    public class CustomNetworkManager : NetworkManager
    {
        public event Action<int> OnPlayerJoined;

        public event Action<int> OnPlayerLeft;

        public ServerUI serverUI;

        [Inject]
        private IPlayerManager _playerManager;

        [Inject]
        private GameManager _gameManager;

        private readonly Dictionary<int, HashSet<string>> _playerObjects;

        public CustomNetworkManager()
        {
            _playerObjects = new Dictionary<int, HashSet<string>>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            NetworkServer.RegisterHandler<IntentionMessage>(OnServerReceiveIntention);
            NetworkServer.RegisterHandler<EndSessionMessage>(OnServerReceiveEndSession);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // UpdateUIMessage handler is registered in ClientMessageHandler
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);

            int playerId = conn.connectionId;
            _playerObjects[playerId] = new HashSet<string>();

            if (_gameManager != null && _gameManager.IsSessionActive)
            {
                conn.Send(new GameNetworkMessage { type = "session_state", payload = "1" });
            }

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

        private void OnServerReceiveIntention(NetworkConnectionToClient conn, IntentionMessage msg)
        {
            if (_gameManager == null) return;

            msg.intention.playerId = conn.connectionId;
            _gameManager.ProcessIntention(msg.intention);
        }

        private void OnServerReceiveEndSession(NetworkConnectionToClient conn, EndSessionMessage msg)
        {
            if (_gameManager == null) return;

            _gameManager.RequestEndSession(conn);
        }

    }
}
