using Zenject;
using SkillForge.Core;
using SkillForge.Education;
using SkillForge.WorkContexts;
using SkillForge.Validation;
using SkillForge.Players;
using SkillForge.Highlight;
using SkillForge.Networking;

namespace SkillForge.Core
{
    public class ServerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IEducationModule>().To<TrainingSystem>().AsSingle();
            Container.Bind<IWorkContext>().To<CarWorkContext>().AsSingle();
            Container.Bind<IValidationService>().To<ValidationService>().AsSingle();
            Container.Bind<IPlayerManager>().To<PlayerManager>().AsSingle();
            Container.Bind<IHighlightService>().To<HighlightService>().AsSingle();
            Container.Bind<IDiagnosticService>().To<DiagnosticService>().AsSingle();
            Container.Bind<IActionLogger>().To<ActionLogger>().AsSingle();
            Container.Bind<IReportGenerator>().To<ReportGenerator>().AsSingle();
            Container.Bind<GameManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}
