using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZBase.UnityScreenNavigator.Core.Views;
using ZBase.UnityScreenNavigator.Foundation;
using ZBase.UnityScreenNavigator.Foundation.PriorityCollection;

namespace ZBase.UnityScreenNavigator.Core.Panel
{
    [DisallowMultipleComponent]
    public class Panel : View, IPanelLifecycleEvent
    {
        [SerializeField]
        private int _renderingOrder;
        
        [SerializeField] private PanelTransitionAnimationContainer _animationContainer = new();

        private readonly UniquePriorityList<IPanelLifecycleEvent> _lifecycleEvents = new();
        private Progress<float> _transitionProgressReporter;

        private Progress<float> TransitionProgressReporter
        {
            get
            {
                return _transitionProgressReporter ??= new Progress<float>(SetTransitionProgress);
            }
        }

        public PanelTransitionAnimationContainer AnimationContainer => _animationContainer;
        
        public bool IsTransitioning { get; private set; }

        /// <summary>
        /// Return the transition animation type currently playing.
        /// If not in transition, return null.
        /// </summary>
        public PanelTransitionAnimationType? TransitionAnimationType { get; private set; }

        /// <summary>
        /// Progress of the transition animation.
        /// </summary>
        public float TransitionAnimationProgress { get; private set; }

        /// <summary>
        /// Event when the transition animation progress changes.
        /// </summary>
        public event Action<float> TransitionAnimationProgressChanged;
        
        public UniTask Initialize(Memory<object> args, CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }

        public UniTask WillPushEnter(Memory<object> args, CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }

        public void DidPushEnter(Memory<object> args)
        {
        }

        public UniTask WillPushExit(Memory<object> args, CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }

        public void DidPushExit(Memory<object> args)
        {
        }

        public UniTask WillPopEnter(Memory<object> args, CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }

        public void DidPopEnter(Memory<object> args)
        {
        }

        public UniTask WillPopExit(Memory<object> args, CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }

        public void DidPopExit(Memory<object> args)
        {
        }

        public UniTask Cleanup(Memory<object> args, CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }
        
        public void AddLifecycleEvent(IPanelLifecycleEvent lifecycleEvent, int priority = 0)
        {
            _lifecycleEvents.Add(lifecycleEvent, priority);
        }

        public void RemoveLifecycleEvent(IPanelLifecycleEvent lifecycleEvent)
        {
            _lifecycleEvents.Remove(lifecycleEvent);
        }

        internal async UniTask AfterLoadAsync(RectTransform parentTransform, Memory<object> args, CancellationToken ct)
        {
            _lifecycleEvents.Add(this, 0);
            SetIdentifer();

            Parent = parentTransform;
            RectTransform.FillParent(Parent);

            // Set order of rendering.
            var siblingIndex = 0;

            for (var i = 0; i < Parent.childCount; i++)
            {
                var child = Parent.GetChild(i);
                var childPanel = child.GetComponent<Panel>();

                siblingIndex = i;

                if (_renderingOrder >= childPanel._renderingOrder)
                {
                    continue;
                }

                break;
            }

            RectTransform.SetSiblingIndex(siblingIndex);
            Alpha = 0.0f;

            var tasks = _lifecycleEvents.Select(x => x.Initialize(args, ct));
            await WaitForAsync(tasks);
        }

        internal async UniTask BeforeEnterAsync(bool push, Memory<object> args, CancellationToken ct)
        {
            IsTransitioning = true;
            TransitionAnimationType = push ? PanelTransitionAnimationType.PushEnter : PanelTransitionAnimationType.PopEnter;
            gameObject.SetActive(true);
            RectTransform.FillParent(Parent);
            SetTransitionProgress(0.0f);

            Alpha = 0.0f;

            var tasks = push
                ? _lifecycleEvents.Select(x => x.WillPushEnter(args, ct))
                : _lifecycleEvents.Select(x => x.WillPopEnter(args, ct));
            
            await WaitForAsync(tasks);
        }

        internal async UniTask<StubEnter> EnterAsync(bool push, bool playAnimation, Panel partnerPanel, CancellationToken ct)
        {
            Alpha = 1.0f;

            if (playAnimation)
            {
                var anim = GetAnimation(push, true, partnerPanel);

                if (partnerPanel)
                {
                    anim.SetPartner(partnerPanel.RectTransform);
                }

                anim.Setup(RectTransform);

                await anim.PlayAsync(TransitionProgressReporter, AnimationContainer.CompleteAnimationWhenCanceled, ct);
            }

            RectTransform.FillParent(Parent);
            SetTransitionProgress(1.0f);
            return default;
        }

        internal void AfterEnter(bool push, Memory<object> args)
        {
            if (push)
            {
                foreach (var lifecycleEvent in _lifecycleEvents)
                {
                    lifecycleEvent.DidPushEnter(args);
                }
            }
            else
            {
                foreach (var lifecycleEvent in _lifecycleEvents)
                {
                    lifecycleEvent.DidPopEnter(args);
                }
            }

            IsTransitioning = false;
            TransitionAnimationType = null;
        }

        internal async UniTask BeforeExitAsync(bool push, Memory<object> args, CancellationToken ct)
        {
            IsTransitioning = true;
            TransitionAnimationType = push
                ? PanelTransitionAnimationType.PushExit
                : PanelTransitionAnimationType.PopExit;

            gameObject.SetActive(true);
            RectTransform.FillParent(Parent);
            SetTransitionProgress(0.0f);

            Alpha = 1.0f;

            var tasks = push
                ? _lifecycleEvents.Select(x => x.WillPushExit(args, ct))
                : _lifecycleEvents.Select(x => x.WillPopExit(args, ct));

            await WaitForAsync(tasks);
        }

        internal async UniTask<StubExit> ExitAsync(bool push, bool playAnimation, Panel partnerPanel, CancellationToken ct)
        {
            if (playAnimation)
            {
                var anim = GetAnimation(push, false, partnerPanel);

                if (partnerPanel)
                {
                    anim.SetPartner(partnerPanel.RectTransform);
                }

                anim.Setup(RectTransform);

                await anim.PlayAsync(TransitionProgressReporter, AnimationContainer.CompleteAnimationWhenCanceled, ct);
            }
            
            Alpha = 0.0f;
            SetTransitionProgress(1.0f);
            return default;
        }

        internal void AfterExit(bool push, Memory<object> args)
        {
            if (push)
            {
                foreach (var lifecycleEvent in _lifecycleEvents)
                {
                    lifecycleEvent.DidPushExit(args);
                }
            }
            else
            {
                foreach (var lifecycleEvent in _lifecycleEvents)
                {
                    lifecycleEvent.DidPopExit(args);
                }
            }

            gameObject.SetActive(false);
            IsTransitioning = false;
            TransitionAnimationType = null;
        }

        internal async UniTask BeforeReleaseAsync(Memory<object> args, CancellationToken ct)
        {
            var tasks = _lifecycleEvents.Select(x => x.Cleanup(args, ct));
            await WaitForAsync(tasks);
        }

        private void SetTransitionProgress(float progress)
        {
            TransitionAnimationProgress = progress;
            TransitionAnimationProgressChanged?.Invoke(progress);
        }

        private ITransitionAnimation GetAnimation(bool push, bool enter, Panel partner)
        {
            var partnerIdentifier = partner == true ? partner.Identifier : string.Empty;
            var anim = _animationContainer.GetAnimation(push, enter, partnerIdentifier);

            if (anim == null)
            {
                return Settings.GetDefaultScreenTransitionAnimation(push, enter);
            }

            return anim;
        }
    }
}
