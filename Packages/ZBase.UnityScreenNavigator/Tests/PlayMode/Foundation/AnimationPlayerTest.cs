using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using ZBase.UnityScreenNavigator.Core;
using ZBase.UnityScreenNavigator.Foundation.Animation;

namespace ZBase.UnityScreenNavigator.Tests.PlayMode.Foundation
{
    public class AnimationPlayerTest
    {
        [Test]
        public void Play_Progressing()
        {
            const float duration = 1.0f;
            var animation = new FakeAnimation(duration);
            var player = new AnimationPlayer(animation);
            player.Play();
            Assert.That(animation.Progress, Is.EqualTo(0.0f));
            player.Update(0.1f);
            Assert.That(animation.Progress, Is.EqualTo(0.1f / 1.0f));
        }
        
        [Test]
        public void NotPlay_NotProgressing()
        {
            const float duration = 1.0f;
            var animation = new FakeAnimation(duration);
            var player = new AnimationPlayer(animation);
            Assert.That(animation.Progress, Is.EqualTo(0.0f));
            player.Update(0.1f);
            Assert.That(animation.Progress, Is.EqualTo(0.0f));
        }
        
        [Test]
        public void Stop_NotProgressing()
        {
            const float duration = 1.0f;
            var animation = new FakeAnimation(duration);
            var player = new AnimationPlayer(animation);
            player.Play();
            Assert.That(animation.Progress, Is.EqualTo(0.0f));
            player.Update(0.1f);
            Assert.That(animation.Progress, Is.EqualTo(0.1f / 1.0f));
            player.Stop();
            player.Update(0.1f);
            Assert.That(animation.Progress, Is.EqualTo(0.1f / 1.0f));
        }
        
        [Test]
        public void Reset_CanReset()
        {
            const float duration = 1.0f;
            var animation = new FakeAnimation(duration);
            var player = new AnimationPlayer(animation);
            player.Play();
            Assert.That(animation.Progress, Is.EqualTo(0.0f));
            player.Update(0.1f);
            Assert.That(animation.Progress, Is.EqualTo(0.1f / 1.0f));
            player.Reset();
            Assert.That(animation.Progress, Is.EqualTo(0.0f));
        }
        
        [Test]
        public void SetTime_CanSet()
        {
            const float duration = 1.0f;
            var animation = new FakeAnimation(duration);
            var player = new AnimationPlayer(animation);
            player.SetTime(0.3f);
            Assert.That(animation.Progress, Is.EqualTo(0.3f));
        }
        
        [Test]
        public void SetTime_NegativeValue_ProgressIsZero()
        {
            const float duration = 1.0f;
            var animation = new FakeAnimation(duration);
            var player = new AnimationPlayer(animation);
            player.SetTime(-1f);
            Assert.That(animation.Progress, Is.EqualTo(0.0f));
        }
        
        [Test]
        public void SetTime_LargeValue_ProgressIsOne()
        {
            const float duration = 1.0f;
            var animation = new FakeAnimation(duration);
            var player = new AnimationPlayer(animation);
            player.SetTime(float.MaxValue);
            Assert.That(animation.Progress, Is.EqualTo(1.0f));
        }
        
        [Test]
        public void IsFinished_SetTimeBeyondDuration_True()
        {
            const float duration = 1.0f;
            var animation = new FakeAnimation(duration);
            var player = new AnimationPlayer(animation);
            player.SetTime(float.MaxValue);
            Assert.That(player.IsFinished, Is.True);
        }

        [UnityTest]
        public IEnumerator Canceled_CompleteWhenCanceled_ProgressIsOne() => UniTask.ToCoroutine(async () => 
        {
            const float duration = 1.0f;
            var animation = new FakeTransitionAnimation(duration);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(0.1f));
            try
            {
                await animation.PlayAsync(completeWhenCanceled: true, ct: cts.Token);
            }
            catch (OperationCanceledException e)
            {
                Assert.AreEqual(animation.Progress, 1f, 0.1f);
            }
        });
        
        [UnityTest]
        public IEnumerator Canceled_NotProgressing() => UniTask.ToCoroutine(async () => 
        {
            const float duration = 1.0f;
            var animation = new FakeTransitionAnimation(duration);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(0.1f));
            try
            {
                await animation.PlayAsync(completeWhenCanceled: false, ct: cts.Token);
            }
            catch (OperationCanceledException e)
            {
                Assert.AreEqual(animation.Progress, 0.1f, 0.1f);
            }
        });
    }
}
