using Zenject;
using SkillForge.VR;
using SkillForge.UI;

namespace SkillForge.Core
{
    public class ClientInstaller : MonoInstaller
    {
        // TODO: assign in Inspector
        public TabletUI tabletUI;
        public HighlightRenderer highlightRenderer;
        public AudioFeedback audioFeedback;
        public SessionEndScreen sessionEndScreen;
        public PlayerController playerController;

        public override void InstallBindings()
        {
            if (tabletUI != null)
                Container.Bind<TabletUI>().FromInstance(tabletUI).AsSingle();

            if (highlightRenderer != null)
                Container.Bind<HighlightRenderer>().FromInstance(highlightRenderer).AsSingle();

            if (audioFeedback != null)
                Container.Bind<AudioFeedback>().FromInstance(audioFeedback).AsSingle();

            if (sessionEndScreen != null)
                Container.Bind<SessionEndScreen>().FromInstance(sessionEndScreen).AsSingle();

            if (playerController != null)
                Container.Bind<PlayerController>().FromInstance(playerController).AsSingle();
        }
    }
}
