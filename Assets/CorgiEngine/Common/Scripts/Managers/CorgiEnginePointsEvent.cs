using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    public struct CorgiEnginePointsEvent
    {
        public PointsMethods PointsMethod;
        public int Points;

        public CorgiEnginePointsEvent(PointsMethods pointsMethod, int points)
        {
            PointsMethod = pointsMethod;
            Points = points;
        }

        static CorgiEnginePointsEvent e;

        public static void Trigger(PointsMethods pointsMethod, int points)
        {
            e.PointsMethod = pointsMethod;
            e.Points = points;
            MMEventManager.TriggerEvent(e);
        }
    }
}