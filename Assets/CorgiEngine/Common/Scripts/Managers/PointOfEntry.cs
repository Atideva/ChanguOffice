using System;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
    [Serializable]
    public struct PointOfEntry
    {
        public string Name;
        public Transform Position;
        public MMFeedbacks EntryFeedback;
    }
}