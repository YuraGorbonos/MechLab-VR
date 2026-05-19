using UnityEngine;
using SkillForge.Core;

namespace SkillForge.Education
{
    public class ScenarioProvider : MonoBehaviour
    {
        [SerializeField]
        private ScenarioData _defaultScenario;

        // TODO: assign in Inspector

        public ScenarioData DefaultScenario => _defaultScenario;

        public ScenarioConfig LoadScenario(ScenarioData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[ScenarioProvider] ScenarioData is null, using default.");
                data = _defaultScenario;
            }

            if (data == null)
            {
                Debug.LogError("[ScenarioProvider] No scenario available.");
                return null;
            }

            return new ScenarioConfig
            {
                scenarioId = data.name,
                steps = data.steps,
                mode = SessionMode.Training
            };
        }
    }
}
