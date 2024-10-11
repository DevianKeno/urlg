using System;

namespace RL.UI
{
    [Serializable]
    public class TransitionOptions
    {
        public LeanTweenType Tween = LeanTweenType.easeInOutQuart;
        public float AnimationSpeed = 1.5f;
        public float Delay = 0.25f;
        public float Multiplier = 1.2f;
    }
}