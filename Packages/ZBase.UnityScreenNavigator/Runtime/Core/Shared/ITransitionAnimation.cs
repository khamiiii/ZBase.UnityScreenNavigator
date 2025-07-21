using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZBase.UnityScreenNavigator.Foundation.Animation;

namespace ZBase.UnityScreenNavigator.Core
{
    public interface ITransitionAnimation : IAnimation
    {
        void SetPartner(RectTransform partnerRectTransform);
        
        void Setup(RectTransform rectTransform);
    }

    internal static class TransitionAnimationExtensions
    {
        public static async UniTask PlayAsync(this ITransitionAnimation self, IProgress<float> progress = null, CancellationToken ct = default)
        {
            var player = new AnimationPlayer(self);

            progress?.Report(0.0f);
            player.Play();

            try
            {
                while (player.IsFinished == false)
                {
                    await UniTask.NextFrame(ct);

                    player.Update(Time.unscaledDeltaTime);
                    progress?.Report(player.Time / self.Duration);
                }
            }
            catch (OperationCanceledException e)
            {
                player.SetTime(player.Animation.Duration);
                progress?.Report(player.Time / self.Duration);
                //Debug.Log($"Canceled transition anim");
                throw;
            }
        }
    }
}