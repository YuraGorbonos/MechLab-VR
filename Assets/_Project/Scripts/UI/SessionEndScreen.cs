using SkillForge.Core;
using SkillForge.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkillForge.UI
{
    public class SessionEndScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _reportText;
        [SerializeField] private Button _openReportButton;

        private string _lastReportPath;

        private void Start()
        {
            if (_panel != null)
                _panel.SetActive(false);

            if (_openReportButton != null)
            {
                _openReportButton.onClick.AddListener(OnOpenReportClicked);
                _openReportButton.gameObject.SetActive(false);
            }

            GameManager.OnReportGenerated += OnReportGenerated;
        }

        private void OnDestroy()
        {
            GameManager.OnReportGenerated -= OnReportGenerated;
        }

        private void OnReportGenerated(string filePath)
        {
            _lastReportPath = filePath;

            if (_reportText != null)
                _reportText.text = $"Отчёт сохранён:\n{filePath}";

            if (_panel != null)
                _panel.SetActive(true);

            if (_openReportButton != null)
                _openReportButton.gameObject.SetActive(true);
        }

        private void OnOpenReportClicked()
        {
            if (!string.IsNullOrEmpty(_lastReportPath))
            {
                Application.OpenURL("file://" + _lastReportPath);
            }
        }

        public void ShowReport(SessionReport report)
        {
            if (_reportText != null)
                _reportText.text = $"Scenario: {report.scenarioId}\n" +
                                   $"Score: {report.score:F1}%\n" +
                                   $"Grade: {report.grade}\n" +
                                   $"Steps: {report.completedSteps}/{report.totalSteps}\n" +
                                   $"Duration: {report.durationSeconds:F0}s";
        }
    }
}
