using SkillForge.Networking;
using System.Collections;
using TMPro;
using UnityEngine;

namespace SkillForge.UI
{
    public class TabletUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _stepText;
        [SerializeField] private Transform _stepsListContainer;
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private float _errorDisplayDuration = 3f;

        private Coroutine _errorCoroutine;

        private void Start()
        {
            if (_errorText != null)
            {
                _errorText.gameObject.SetActive(false);
            }

            GameManager.OnErrorReceived += OnErrorReceived;
        }

        private void OnDestroy()
        {
            GameManager.OnErrorReceived -= OnErrorReceived;
        }

        public void UpdateUI(string instruction, int stepIndex)
        {
            if (_stepText != null)
            {
                _stepText.text = $"Step {stepIndex + 1}: {instruction}";
            }
        }

        public void DisplayError(string message)
        {
            if (_errorText == null) return;

            _errorText.text = $"Error: {message}";
            _errorText.gameObject.SetActive(true);

            if (_errorCoroutine != null)
            {
                StopCoroutine(_errorCoroutine);
            }

            _errorCoroutine = StartCoroutine(ClearErrorAfterDelay());
        }

        private IEnumerator ClearErrorAfterDelay()
        {
            yield return new WaitForSeconds(_errorDisplayDuration);

            if (_errorText != null)
            {
                _errorText.text = "";
                _errorText.gameObject.SetActive(false);
            }

            _errorCoroutine = null;
        }

        private void OnErrorReceived(string message)
        {
            DisplayError(message);
        }
    }
}
