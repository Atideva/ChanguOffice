using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    public struct CorgiEngineStarEvent
    {
        public string SceneName;
        public int StarID;

        public CorgiEngineStarEvent(string sceneName, int starID)
        {
            SceneName = sceneName;
            StarID = starID;
        }

        static CorgiEngineStarEvent e;

        public static void Trigger(string sceneName, int starID)
        {
            e.SceneName = sceneName;
            e.StarID = starID;
            MMEventManager.TriggerEvent(e);
        }
    }
}