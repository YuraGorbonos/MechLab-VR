using Zenject;
using SkillForge.VR;
using SkillForge.UI;
using SkillForge.Networking;

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
        public DebugClientUI debugClientUI;
        public IntentionSender intentionSender;

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

            if (debugClientUI != null)
                Container.Bind<DebugClientUI>().FromInstance(debugClientUI).AsSingle();

            if (intentionSender != null)
                Container.Bind<IntentionSender>().FromInstance(intentionSender).AsSingle();
        }
    }
}
