using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    public struct CorgiEngineEvent
    {
        public CorgiEngineEventTypes EventType;
        public Character OriginCharacter;

        public CorgiEngineEvent(CorgiEngineEventTypes eventType, Character originCharacter = null)
        {
            EventType = eventType;
            OriginCharacter = originCharacter;
        }

        static CorgiEngineEvent e;

        public static void Trigger(CorgiEngineEventTypes eventType, Character originCharacter = null)
        {
            e.EventType = eventType;
            e.OriginCharacter = originCharacter;
            MMEventManager.TriggerEvent(e);
        }
    }
}