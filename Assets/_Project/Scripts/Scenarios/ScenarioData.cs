using UnityEngine;

namespace SkillForge.Core
{
    [CreateAssetMenu(menuName = "SkillForge/Scenario")]
    public class ScenarioData : ScriptableObject
    {
        public string scenarioId;
        public string scenarioName;
        public ScenarioStep[] steps;
    }
}