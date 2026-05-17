using Mirror;
using SkillForge.Core;
using UnityEngine;

namespace SkillForge.UI
{
    public class HighlightRenderer : NetworkBehaviour
    {
        public void SetHighlight(string targetId, HighlightType type)
        {
            // TODO: render highlight on target object
            Debug.Log($"[HighlightRenderer] Highlight {type} on {targetId}");
        }

        public void ClearHighlights()
        {
            // TODO: clear all highlights
        }
    }
}
