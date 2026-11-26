using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SkillForge.Demo
{
    public class DemoComfortVignette : MonoBehaviour
    {
        [SerializeField] private float _maxIntensity = 0.58f;
        [SerializeField] private float _moveStart = 0.25f;
        [SerializeField] private float _moveFull = 1.4f;

        private Vignette _vignette;
        private Vector3 _lastPosition;
        private bool _hasPosition;
        private CanvasGroup _group;




private void LateUpdate()
{
    if (_group == null && !BuildOverlay())
    {
        return;
    }

    var camera = ActiveCamera();
    if (camera == null)
    {
        return;
    }

    var position = camera.transform.position;
    if (!_hasPosition)
    {
        _lastPosition = position;
        _hasPosition = true;
        return;
    }

    var speed = (position - _lastPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
    _lastPosition = position;
    var amount = Mathf.InverseLerp(_moveStart, _moveFull, speed);
    _group.alpha = Mathf.Lerp(_group.alpha, amount * _maxIntensity, Time.deltaTime * 10f);
}

private bool BuildOverlay()
{
    var camera = ActiveCamera();
    if (camera == null)
    {
        return false;
    }

    var canvasObject = new GameObject("VignetteOverlay");
    canvasObject.transform.SetParent(camera.transform, false);
    var canvas = canvasObject.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceCamera;
    canvas.worldCamera = camera;
    canvas.planeDistance = 0.08f;
    canvas.sortingOrder = 500;
    _group = canvasObject.AddComponent<CanvasGroup>();
    _group.alpha = 0f;
    _group.blocksRaycasts = false;
    _group.interactable = false;

    var imageObject = new GameObject("Ring");
    imageObject.transform.SetParent(canvasObject.transform, false);
    var rect = imageObject.AddComponent<RectTransform>();
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;
    var image = imageObject.AddComponent<UnityEngine.UI.RawImage>();
    image.texture = MakeVignette();
    image.raycastTarget = false;
    return true;
}

private static Texture2D MakeVignette()
{
    const int size = 128;
    var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
    var center = (size - 1) * 0.5f;
    for (var y = 0; y < size; y++)
    {
        for (var x = 0; x < size; x++)
        {
            var dx = (x - center) / center;
            var dy = (y - center) / center;
            var radius = Mathf.Sqrt(dx * dx + dy * dy);
            var alpha = Mathf.Clamp01((radius - 0.42f) / 0.58f);
            texture.SetPixel(x, y, new Color(0f, 0f, 0f, alpha * alpha));
        }
    }

    texture.Apply();
    return texture;
}


        private static Camera ActiveCamera()
        {
            if (Camera.main != null && Camera.main.isActiveAndEnabled)
            {
                return Camera.main;
            }

            var cameras = Camera.allCameras;
            for (var i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != null && cameras[i].isActiveAndEnabled)
                {
                    return cameras[i];
                }
            }

            return null;
        }
    }
}
