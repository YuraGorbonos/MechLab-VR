using System.Collections.Generic;
using SkillForge.Core;

namespace SkillForge.Education
{
    public class ReportGenerator
    {
        // TODO: integrate QuestPDF for PDF report generation

        public SessionReport Generate(ScenarioConfig config, List<ActionLogEntry> actionLog, float durationSeconds)
        {
            var completedSteps = new HashSet<string>();
            var skippedSteps = 0;

            foreach (var entry in actionLog)
            {
                if (entry.success)
                {
                    completedSteps.Add(entry.targetObject);
                }
            }

            int totalSteps = config?.steps?.Length ?? 0;

            var score = totalSteps > 0 ? (float)completedSteps.Count / totalSteps * 100f : 0f;
            var grade = score >= 90f ? "A" : score >= 75f ? "B" : score >= 60f ? "C" : score >= 40f ? "D" : "F";

            return new SessionReport
            {
                scenarioId = config?.scenarioId ?? string.Empty,
                mode = config?.mode ?? SessionMode.Training,
                durationSeconds = durationSeconds,
                totalSteps = totalSteps,
                completedSteps = completedSteps.Count,
                skippedSteps = skippedSteps,
                actionLog = new List<ActionLogEntry>(actionLog),
                score = score,
                grade = grade
            };
        }
    }
}
