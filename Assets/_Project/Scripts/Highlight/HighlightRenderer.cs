using Mirror;
using UnityEngine;

namespace SkillForge.Highlight
{
    public class HighlightRenderer : NetworkBehaviour
    {
        public string objectId;

        [SyncVar(hook = nameof(OnHighlightChanged))]
        private bool _isHighlighted;

        [SyncVar]
        private Color _highlightColor = Color.yellow;

        private Color _originalColor;
        private MeshRenderer _meshRenderer;
        private bool _hasInitialized;

        private void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            if (_meshRenderer != null)
            {
                _originalColor = _meshRenderer.material.color;
                _hasInitialized = true;
            }
        }

        public void SetHighlight(bool active, Color color)
        {
            _highlightColor = color;
            _isHighlighted = active;
        }

        public void SetHighlight(bool active)
        {
            _isHighlighted = active;
        }

        private void OnHighlightChanged(bool oldValue, bool newValue)
        {
            if (!_hasInitialized && _meshRenderer == null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
                if (_meshRenderer == null) return;
                _originalColor = _meshRenderer.material.color;
                _hasInitialized = true;
            }

            if (_meshRenderer == null) return;

            _meshRenderer.material.color = newValue ? _highlightColor : _originalColor;
        }
    }
}
