using System.Collections.Generic;
using SkillForge.Core;

namespace SkillForge.Players
{
    public class PlayerManager : IPlayerManager
    {
        private readonly Dictionary<string, int> _occupiedObjects;

        public PlayerManager()
        {
            _occupiedObjects = new Dictionary<string, int>();
        }

        public bool TryAcquireObject(int playerId, string objectId)
        {
            if (string.IsNullOrEmpty(objectId))
                return false;

            if (_occupiedObjects.TryGetValue(objectId, out var existingPlayerId))
            {
                if (existingPlayerId == playerId)
                    return true;

                return false;
            }

            _occupiedObjects[objectId] = playerId;
            return true;
        }

        public void ReleaseObject(int playerId, string objectId)
        {
            if (_occupiedObjects.TryGetValue(objectId, out var existingPlayerId) && existingPlayerId == playerId)
            {
                _occupiedObjects.Remove(objectId);
            }
        }

        public bool IsObjectFree(string objectId)
        {
            return !_occupiedObjects.ContainsKey(objectId);
        }
    }
}
