namespace SkillForge.Core
{
    public interface IPlayerManager
    {
        bool TryAcquireObject(int playerId, string objectId);
        void ReleaseObject(int playerId, string objectId);
        bool IsObjectFree(string objectId);
    }
}
