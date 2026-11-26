using SkillForge.Core;
using SkillForge.Networking;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SkillForge.VR
{
    public class PlayerController : MonoBehaviour
    {
        private XRDirectInteractor _rightInteractor;

        private void Start()
        {
            _rightInteractor = GetComponentInChildren<XRDirectInteractor>();
            if (_rightInteractor == null)
            {
                Debug.LogError("[PlayerController] XRDirectInteractor not found on controller.");
            }
        }

private void Update()
{
    var keyboard = UnityEngine.InputSystem.Keyboard.current;
    if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
    {
        OnTriggerPressed();
    }
}

        private void OnTriggerPressed()
        {
            if (_rightInteractor == null || _rightInteractor.interactablesSelected.Count == 0) return;

            var selectedObject = _rightInteractor.interactablesSelected[0].transform.gameObject;

            var handler = selectedObject.GetComponent<ToolInteractionHandler>();
            if (handler == null) return;

            if (handler.TryInteract(out var intent))
            {
                Debug.Log($"[PlayerController] Sending intention: type={intent.intentType}, target={intent.targetObject}, tool={intent.tool}, action={intent.action}");
                NetworkMessageSender.SendIntention(intent);
            }
        }
    }
}
