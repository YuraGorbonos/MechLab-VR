using Mirror;
using SkillForge.Core;
using UnityEngine;

namespace SkillForge.UI
{
    public class SessionEndScreen : NetworkBehaviour
    {
        [SyncVar]
        private bool _isVisible;

        [SyncVar]
        private string _reportText;

        public void ShowReport(SessionReport report)
        {
            _isVisible = true;
            _reportText = FormatReport(report);
            // TODO: display report UI
        }

        private string FormatReport(SessionReport report)
        {
            return $"Scenario: {report.scenarioId}\n" +
                   $"Score: {report.score:F1}%\n" +
                   $"Grade: {report.grade}\n" +
                   $"Steps: {report.completedSteps}/{report.totalSteps}\n" +
                   $"Duration: {report.durationSeconds:F0}s";
        }
    }
}
