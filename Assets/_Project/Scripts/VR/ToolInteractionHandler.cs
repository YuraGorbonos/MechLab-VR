using SkillForge.Core;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SkillForge.VR
{
    public class ToolInteractionHandler : MonoBehaviour
    {
        public string toolId;
        public float interactionRadius = 0.1f;

        public bool TryInteract(out Intention intention)
        {
            intention = default;

            var grabInteractable = GetComponent<XRGrabInteractable>();
            if (grabInteractable == null)
            {
                Debug.LogWarning($"[ToolInteractionHandler] XRGrabInteractable not found on '{gameObject.name}'.");
                return false;
            }

            if (!grabInteractable.isSelected)
                return false;

            InteractableObject nearest = null;
            var nearestSqrDist = float.MaxValue;

            var interactables = FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
            foreach (var interactable in interactables)
            {
                var sqrDist = (transform.position - interactable.transform.position).sqrMagnitude;
                if (sqrDist <= interactionRadius * interactionRadius && sqrDist < nearestSqrDist)
                {
                    nearest = interactable;
                    nearestSqrDist = sqrDist;
                }
            }

            if (nearest == null)
            {
                Debug.Log($"[ToolInteractionHandler] No interactable object found within radius {interactionRadius}.");
                return false;
            }

            var action = DetermineAction(nearest);

            intention = new Intention
            {
                playerId = 1,
                intentType = "use_tool",
                targetObject = nearest.GetTargetObjectId(),
                tool = toolId,
                action = action
            };

            Debug.Log($"[ToolInteractionHandler] Intention formed: target={nearest.GetTargetObjectId()}, tool={toolId}, action={action}");
            return true;
        }

        private string DetermineAction(InteractableObject target)
        {
            foreach (var validAction in target.validActions)
            {
                if (target.CanPerformAction(validAction, toolId))
                    return validAction;
            }

            return "use";
        }
    }
}
