using SkillForge.Core;
using System.Linq;
using UnityEngine;

namespace SkillForge.Education
{
    public class ScenarioProvider : MonoBehaviour
    {
        // TODO: assign in Inspector

        public ScenarioData DefaultScenario => _defaultScenario;

        [SerializeField]
        private ScenarioData _defaultScenario;

        [SerializeField] private ScenarioData[] _additionalScenarios;

        public ScenarioData[] GetScenarios()
        {
            return new[] { _defaultScenario }
                .Concat(_additionalScenarios ?? System.Array.Empty<ScenarioData>())
                .Where(scenario => scenario != null && !string.IsNullOrEmpty(scenario.scenarioId))
                .Distinct()
                .ToArray();
        }

        public ScenarioData GetScenario(string scenarioId)
        {
            return GetScenarios().FirstOrDefault(scenario => scenario.scenarioId == scenarioId);
        }

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
                       scenarioId = data.scenarioId,
                       steps = data.steps,
                       mode = SessionMode.Training
                   };
        }
    }
}