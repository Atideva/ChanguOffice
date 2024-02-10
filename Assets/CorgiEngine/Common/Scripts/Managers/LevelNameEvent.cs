using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    public struct LevelNameEvent
    {
        public string LevelName;

        public LevelNameEvent(string levelName)
        {
            LevelName = levelName;
        }

        static LevelNameEvent e;

        public static void Trigger(string levelName)
        {
            e.LevelName = levelName;
            MMEventManager.TriggerEvent(e);
        }
    }
}