using System.Collections.Generic;
using Mirror;
using SkillForge.Core;
using SkillForge.Networking;
using UnityEngine;
using UnityEngine.UI;using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;


namespace SkillForge.Demo
{
    public class DemoTaskSpace : MonoBehaviour
    {
        private static readonly Intention[] Steps =
        {
            new Intention { intentType = "scan", targetObject = "diag_port", tool = "diagnostic_scanner", action = "scan" },
            new Intention { intentType = "scan", targetObject = "brakes", action = "scan" },
            new Intention { intentType = "use_tool", targetObject = "bolt_caliper_front_left_01", tool = "wrench", action = "loosen" },
            new Intention { intentType = "use_tool", targetObject = "caliper_front_left", tool = "wrench", action = "remove" },
            new Intention { intentType = "use_tool", targetObject = "brake_pad_front_left", action = "remove" },
            new Intention { intentType = "use_tool", targetObject = "brake_pad_front_left", action = "install" },
            new Intention { intentType = "use_tool", targetObject = "caliper_front_left", tool = "wrench", action = "install" },
            new Intention { intentType = "use_tool", targetObject = "bolt_caliper_front_left_01", tool = "wrench", action = "tighten" }
        };

        private int _step;
        private Text _title;
        private Text _hint;
        private readonly List<GameObject> _calipers = new List<GameObject>();
        private readonly List<GameObject> _pads = new List<GameObject>();
        private readonly List<GameObject> _bolts = new List<GameObject>();
        private Transform _arrow;
        private Text _arrowLabel;
        private string _instruction = "Подключите сканер к автомобилю";
        private static readonly string[] GuideTargets = { "BrakeAssembly", "brake_disc", "bolt_01", "brake_caliper", "brake_pad_inner", "brake_disc", "brake_disc", "bolt_01" };


private void Start()
{
    BuildBoard();
    CreatePointer();
    CacheParts();
    ApplyVisuals();
    PointAtCurrentStep();
    NetworkClient.RegisterHandler<UpdateUIMessage>(OnStep);
    NetworkClient.RegisterHandler<GameNetworkMessage>(OnGame);
    SetTitle("Стрелка показывает деталь. Курок или E — выполнить.");
}

private static void AddGarageColliders()
{
    var garage = GameObject.Find("Garage");
    if (garage == null)
    {
        return;
    }

    foreach (var filter in garage.GetComponentsInChildren<MeshFilter>(true))
    {
        if (filter.sharedMesh == null || filter.GetComponent<Collider>() != null)
        {
            continue;
        }

        var collider = filter.gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = filter.sharedMesh;
    }
}


private void Update()
{
    if (TriggerPressed())
    {
        Perform();
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

    var index = Mathf.Clamp(_step, 0, GuideTargets.Length - 1);
    var target = FindGuide(GuideTargets[index]) ?? FindGuide("BrakeAssembly");
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
        _arrowLabel.text = _instruction;
        var camera = Camera.main;
        if (camera != null)
        {
            _arrowLabel.transform.rotation = Quaternion.LookRotation(_arrowLabel.transform.position - camera.transform.position);
        }
    }
}

private static Transform FindGuide(string partName)
{
    Transform hidden = null;
    foreach (var item in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
    {
        if (item.name != partName && !item.name.StartsWith(partName))
        {
            continue;
        }

        if (item.gameObject.activeInHierarchy)
        {
            return item;
        }

        hidden = item;
    }

    return hidden;
}


private void OnStep(UpdateUIMessage message)
{
    _step = message.stepIndex;
    _instruction = message.instruction;
    SetTitle($"Шаг {message.stepIndex + 1}. {message.instruction}");
    SetHint("Стрелка на детали. Курок — выполнить");
    ApplyVisuals();
    PointAtCurrentStep();
}

private void OnGame(GameNetworkMessage message)
{
    if (message.type == "error")
    {
        SetHint(message.payload);
    }
    else if (message.type == "report")
    {
        SetTitle("Смена завершена.");
        SetHint(message.payload);
        Show(true, true, true);
        if (_arrow != null)
        {
            _arrow.gameObject.SetActive(false);
        }
    }
}

        private void Perform()
        {
            if (!NetworkClient.isConnected)
            {
                SetHint("Нет соединения с сервером.");
                return;
            }

            if (_step < 0 || _step >= Steps.Length)
            {
                return;
            }

            NetworkMessageSender.SendIntention(Steps[_step]);
        }

        private void CacheParts()
        {
            foreach (var item in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                var partName = item.name;
                if (partName.Contains("brake_caliper")) _calipers.Add(item.gameObject);
                else if (partName.Contains("brake_pad")) _pads.Add(item.gameObject);
                else if (partName.Contains("bolt_")) _bolts.Add(item.gameObject);
            }
        }

        private void ApplyVisuals()
        {
            var caliperOn = _step < 4 || _step >= 6;
            var padsOn = _step < 5 || _step >= 6;
            Show(caliperOn, padsOn, caliperOn);
        }

        private void Show(bool caliper, bool pads, bool bolts)
        {
            foreach (var item in _calipers) item.SetActive(caliper);
            foreach (var item in _pads) item.SetActive(pads);
            foreach (var item in _bolts) item.SetActive(bolts);
        }

        private void BuildBoard()
        {
            var canvasObject = new GameObject("TaskBoard");
            canvasObject.transform.SetParent(transform, false);
            canvasObject.transform.localPosition = new Vector3(0f, 1.35f, 0.8f);
            canvasObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = canvas.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900f, 420f);
            canvasObject.transform.localScale = Vector3.one * 0.0016f;

            var panel = new GameObject("Panel");
            panel.transform.SetParent(canvasObject.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var image = panel.AddComponent<Image>();
            image.color = new Color(0.08f, 0.1f, 0.12f, 0.92f);

            _title = CreateText(panel.transform, "Title", new Vector2(0f, 40f), 40);
            _hint = CreateText(panel.transform, "Hint", new Vector2(0f, -120f), 26);
            _hint.color = new Color(0.95f, 0.78f, 0.35f);
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
