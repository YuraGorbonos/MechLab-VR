using System;
using System.Linq;
using Mirror;
using SkillForge.Core;
using SkillForge.Networking;
using SkillForge.VR;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.InputSystem.Controls;


namespace SkillForge.Demo
{
    public class DemoTaskSpace : MonoBehaviour
    {
        [Serializable]
        private class TargetView
        {
            public string targetId;
            public Transform guide;
            public Transform installationGuide;
            public GameObject[] visuals;
        }

        [SerializeField] private TargetView[] _targets;
        [SerializeField] private Transform _boardMount;

        public event Action<UpdateUIMessage> StepUpdated;
        public event Action<GameNetworkMessage> GameMessageReceived;

        private UpdateUIMessage _step;
        private Button _stepButton;
        private Text _stepButtonLabel;
        private Text _title;
        private Text _hint;
        private Transform _arrow;
        private Text _arrowLabel;

        private void Start()
        {
            BuildBoard();
            CreatePointer();
            _arrow.gameObject.SetActive(false);
            NetworkClient.RegisterHandler<UpdateUIMessage>(OnStep);
            NetworkClient.RegisterHandler<GameNetworkMessage>(OnGame);
            SetTitle("Ожидание запуска занятия");
        }

        private void OnDestroy()
        {
            NetworkClient.UnregisterHandler<UpdateUIMessage>();
            NetworkClient.UnregisterHandler<GameNetworkMessage>();

            if (_arrow != null)
            {
                Destroy(_arrow.gameObject);
            }
        }

private void Update()
{
    if (CanPerform(StepInput.Shortcut) && TriggerPressed())
    {
        NetworkMessageSender.SendIntention(_step.intention);
    }

    PointAtCurrentStep();
}

private static bool TriggerPressed()
{
    if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
    {
        return true;
    }

    foreach (var device in InputSystem.devices)
    {
        var trigger = device.TryGetChildControl<ButtonControl>("triggerPressed");
        if (trigger != null && trigger.wasPressedThisFrame)
        {
            return true;
        }
    }

    return false;
}

private void CreatePointer()
{
    var shader = Shader.Find("Universal Render Pipeline/Unlit");
    var glow = new Material(shader);
    glow.SetColor("_BaseColor", new Color(1f, 0.78f, 0.12f, 1f));
    var arrow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    Destroy(arrow.GetComponent<Collider>());
    arrow.name = "StepArrow";
    arrow.transform.localScale = new Vector3(0.035f, 0.16f, 0.035f);
    arrow.GetComponent<Renderer>().sharedMaterial = glow;
    var halo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    Destroy(halo.GetComponent<Collider>());
    halo.name = "StepHalo";
    halo.transform.SetParent(arrow.transform, false);
    halo.transform.localPosition = new Vector3(0f, -1.15f, 0f);
    halo.transform.localScale = new Vector3(6f, 0.25f, 6f);
    halo.GetComponent<Renderer>().sharedMaterial = glow;
    var canvasObject = new GameObject("StepLabel");
    canvasObject.transform.SetParent(arrow.transform, false);
    canvasObject.transform.localPosition = new Vector3(0f, 1.55f, 0f);
    var canvas = canvasObject.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.WorldSpace;
    canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(760f, 180f);
    canvasObject.transform.localScale = Vector3.one * 0.0015f;
    var textObject = new GameObject("Text");
    textObject.transform.SetParent(canvasObject.transform, false);
    textObject.AddComponent<RectTransform>().sizeDelta = new Vector2(740f, 170f);
    _arrowLabel = textObject.AddComponent<Text>();
    _arrowLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    _arrowLabel.fontSize = 40;
    _arrowLabel.alignment = TextAnchor.MiddleCenter;
    _arrowLabel.color = new Color(1f, 0.93f, 0.55f);
    _arrowLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
    _arrow = arrow.transform;
}

private void PointAtCurrentStep()
{
    if (_arrow == null || !_arrow.gameObject.activeSelf)
    {
        return;
    }

    var target = _step.input == StepInput.TabletButton
        ? _stepButton.transform
        : GetGuide(_step.intention.targetObject);
    if (target == null)
    {
        return;
    }

    var point = target.position + Vector3.up * 0.42f;
    var renderers = target.GetComponentsInChildren<Renderer>(false);
    if (renderers.Length > 0)
    {
        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        point = bounds.center + Vector3.up * (bounds.extents.y + 0.22f);
    }

    point.y += Mathf.Sin(Time.time * 3f) * 0.035f;
    _arrow.position = point;
    if (_arrowLabel != null)
    {
        _arrowLabel.text = _step.instruction;
        var camera = Camera.main;
        if (camera != null)
        {
            _arrowLabel.transform.rotation = Quaternion.LookRotation(_arrowLabel.transform.position - camera.transform.position);
        }
    }
}

        private Transform GetGuide(string targetId)
        {
            var target = Array.Find(_targets, view => view.targetId == targetId);
            if (target == null)
            {
                return null;
            }

            return target.guide.gameObject.activeInHierarchy ? target.guide : target.installationGuide;
        }

        private void OnStep(UpdateUIMessage message)
        {
            _step = message;
            var hasStep = !string.IsNullOrEmpty(message.intention.action);
            var hasButton = hasStep && message.input == StepInput.TabletButton;
            _stepButton.gameObject.SetActive(hasButton);
            _stepButtonLabel.text = message.buttonLabel;
            _arrow.gameObject.SetActive(hasStep);
            if (hasStep)
            {
                SetTitle($"Шаг {message.stepIndex + 1}. {message.instruction}");
                SetHint(message.hint);
            }

            ApplyVisuals(message.removedTargetIds);
            PointAtCurrentStep();
            StepUpdated?.Invoke(message);
        }

        private void OnGame(GameNetworkMessage message)
        {
            if (message.type == "error")
            {
                SetHint(message.payload);
            }
            else if ((message.type == "session_state" && message.payload == "0") || message.type == "report")
            {
                _step = default;
                _stepButton.gameObject.SetActive(false);
                _arrow.gameObject.SetActive(false);
                SetTitle("Смена завершена.");
                if (message.type == "report")
                {
                    SetHint(message.payload);
                }
            }

            GameMessageReceived?.Invoke(message);
        }

        public void TryUseTool(ToolInteractionHandler tool)
        {
            if (CanPerform(StepInput.HeldTool) && tool.TryInteract(out var intention) &&
                intention.targetObject == _step.intention.targetObject)
            {
                intention.intentType = _step.intention.intentType;
                intention.action = _step.intention.action;
                NetworkMessageSender.SendIntention(intention);
            }
        }

        private bool CanPerform(StepInput input)
        {
            return _step.input == input && !string.IsNullOrEmpty(_step.intention.action) &&
                   NetworkClient.isConnected;
        }

        private void ApplyVisuals(string[] removedTargetIds)
        {
            foreach (var target in _targets)
            {
                var visible = removedTargetIds == null || !removedTargetIds.Contains(target.targetId);
                foreach (var visual in target.visuals)
                {
                    visual.SetActive(visible);
                }
            }
        }

        private void BuildBoard()
        {
            var canvasObject = new GameObject("TaskBoard");
            canvasObject.transform.SetParent(_boardMount, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            var rect = canvas.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900f, 420f);

            var panel = new GameObject("Panel");
            panel.transform.SetParent(canvasObject.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var image = panel.AddComponent<Image>();
            image.color = new Color(0.08f, 0.1f, 0.12f, 0.92f);
            image.raycastTarget = false;

            _title = CreateText(panel.transform, "Title", new Vector2(0f, 40f), 40);
            _hint = CreateText(panel.transform, "Hint", new Vector2(0f, -120f), 26);
            _hint.color = new Color(0.95f, 0.78f, 0.35f);

            var buttonObject = new GameObject("StepButton");
            buttonObject.transform.SetParent(panel.transform, false);
            var buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(460f, 78f);
            buttonRect.anchoredPosition = new Vector2(0f, -40f);
            var buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.16f, 0.34f, 0.48f);
            _stepButton = buttonObject.AddComponent<Button>();
            _stepButton.targetGraphic = buttonImage;
            _stepButton.onClick.AddListener(OnStepButtonClicked);
            _stepButtonLabel = CreateText(buttonObject.transform, "Label", Vector2.zero, 32);
            buttonObject.SetActive(false);
        }

        private void OnStepButtonClicked()
        {
            if (CanPerform(StepInput.TabletButton))
            {
                NetworkMessageSender.SendIntention(_step.intention);
            }
        }

        private static Text CreateText(Transform parent, string name, Vector2 position, int size)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var textRect = textObject.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(840f, 180f);
            textRect.anchoredPosition = position;
            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.94f, 0.96f, 0.94f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private void SetTitle(string value)
        {
            if (_title != null) _title.text = value;
        }

        private void SetHint(string value)
        {
            if (_hint != null) _hint.text = value;
        }
    }
}
