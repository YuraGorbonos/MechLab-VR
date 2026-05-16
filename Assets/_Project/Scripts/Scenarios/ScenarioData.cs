using UnityEngine;

namespace SkillForge.Core
{
    [CreateAssetMenu(menuName = "SkillForge/Scenario")]
    public class ScenarioData : ScriptableObject
    {
        public string scenarioName;
        public ScenarioStep[] steps;
    }
}
