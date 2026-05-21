using SkillForge.Core;
using SkillForge.Networking;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SkillForge.VR
{
    public class PlayerController : MonoBehaviour
    {
        private IntentionSender _intentionSender;
        private XRDirectInteractor _rightInteractor;

        private void Start()
        {
            _intentionSender = FindObjectOfType<IntentionSender>();
            if (_intentionSender == null)
            {
                Debug.LogError("[PlayerController] IntentionSender not found in scene.");
            }

            _rightInteractor = GetComponentInChildren<XRDirectInteractor>();
            if (_rightInteractor == null)
            {
                Debug.LogError("[PlayerController] XRDirectInteractor not found on controller.");
            }
        }

        private void Update()
        {
            if (_intentionSender == null) return;

            if (Input.GetKeyDown(KeyCode.Space))
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
                _intentionSender.CmdSendIntention(intent);
            }
        }
    }
}
