using System.Collections.Generic;
using SkillForge.Core;

namespace SkillForge.Core
{
    public interface IReportGenerator
    {
        SessionReport Generate(ScenarioConfig config, List<ActionLogEntry> actionLog, float durationSeconds);
    }
}
