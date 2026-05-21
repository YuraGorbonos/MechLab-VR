using System.Collections.Generic;
using UnityEngine;

namespace SkillForge.VR
{
    public class InteractableObject : MonoBehaviour
    {
        public string objectId;
        public List<string> validActions = new List<string>();
        public string requiredToolId;

        public bool CanPerformAction(string action, string toolId)
        {
            if (!validActions.Contains(action))
                return false;

            if (!string.IsNullOrEmpty(requiredToolId) && requiredToolId != toolId)
                return false;

            return true;
        }

        public string GetTargetObjectId()
        {
            return objectId;
        }
    }
}
