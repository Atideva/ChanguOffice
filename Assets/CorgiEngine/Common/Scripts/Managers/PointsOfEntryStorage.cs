namespace MoreMountains.CorgiEngine
{
    public class PointsOfEntryStorage
    {
        public string LevelName;
        public int PointOfEntryIndex;
        public Character.FacingDirections FacingDirection;

        public PointsOfEntryStorage(string levelName, int pointOfEntryIndex, Character.FacingDirections facingDirection)
        {
            LevelName = levelName;
            FacingDirection = facingDirection;
            PointOfEntryIndex = pointOfEntryIndex;
        }
    }
}