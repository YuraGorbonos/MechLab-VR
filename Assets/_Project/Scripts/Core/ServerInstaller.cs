using Zenject;
using SkillForge.Core;
using SkillForge.Education;
using SkillForge.WorkContexts;
using SkillForge.Validation;
using SkillForge.Players;
using SkillForge.Highlight;
using SkillForge.Networking;
using SkillForge.UI;

namespace SkillForge.Core
{
    public class ServerInstaller : MonoInstaller
    {
        // TODO: assign in Inspector
        public ServerUI serverUI;
        public CustomNetworkManager customNetworkManager;
        public GameManager gameManager;
        public ScenarioData defaultScenario;

        public override void InstallBindings()
        {
            AppContainer.ServerContainer = Container;

            var trainingSystem = new TrainingSystem();
            trainingSystem.DefaultScenario = defaultScenario;
            Container.Bind<IEducationModule>().FromInstance(trainingSystem).AsSingle();
            Container.Bind<IWorkContext>().To<CarWorkContext>().AsSingle();
            Container.Bind<IValidationService>().To<ValidationService>().AsSingle();
            Container.Bind<IPlayerManager>().To<PlayerManager>().AsSingle();
            Container.Bind<IHighlightService>().To<HighlightService>().AsSingle();
            Container.Bind<IDiagnosticService>().To<DiagnosticService>().AsSingle();
            Container.Bind<IActionLogger>().To<ActionLogger>().AsSingle();
            Container.Bind<IReportGenerator>().To<ReportGenerator>().AsSingle();

            if (customNetworkManager != null)
                Container.Bind<CustomNetworkManager>().FromInstance(customNetworkManager).AsSingle();

            if (gameManager != null)
                Container.Bind<GameManager>().FromInstance(gameManager).AsSingle();

            if (serverUI != null)
                Container.Bind<ServerUI>().FromInstance(serverUI).AsSingle();
        }
    }
}
