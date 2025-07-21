using System;
using UnityEngine;
using ZBase.UnityScreenNavigator.Core;

namespace ZBase.UnityScreenNavigator.Tests.PlayMode.Foundation
{
    public class FakeTransitionAnimation : ITransitionAnimation
    {
        public float Progress { get; private set; }
        public float Duration { get; }

        public FakeTransitionAnimation(float duration)
        {
            Duration = duration;
        }
        
        public void SetTime(float time)
        {
            time = Math.Min(Duration, time);
            Progress = time / Duration;
        }

        public void SetPartner(RectTransform partnerRectTransform)
        {
        }

        public void Setup(RectTransform rectTransform)
        {
        }
    }
}