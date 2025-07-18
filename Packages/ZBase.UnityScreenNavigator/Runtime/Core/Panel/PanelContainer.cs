using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZBase.UnityScreenNavigator.Core.Controls;
using ZBase.UnityScreenNavigator.Foundation;

namespace ZBase.UnityScreenNavigator.Core.Panel
{
    public class PanelContainer : ControlContainerBase
    {
        private readonly List<IPanelContainerCallbackReceiver> _callbackReceivers = new();
        private readonly List<ViewRef<Panel>> _panels = new();

        private bool _isActivePanelStacked;
        
        /// <summary>
        /// True if in transition.
        /// </summary>
        public bool IsInTransition { get; private set; }
        
        /// <summary>
        /// Stacked panels.
        /// </summary>
        public IReadOnlyList<ViewRef<Panel>> Panels => _panels;

        public ViewRef<Panel> Current => _panels[^1];

        protected override void Awake()
        {
            base.Awake();
            
            this._callbackReceivers.AddRange(GetComponents<IPanelContainerCallbackReceiver>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Cleanup(params object[] args)
        {
            CleanupAndForget(args).Forget();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Cleanup(Memory<object> args = default)
        {
            CleanupAndForget(args).Forget();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask CleanupAsync(params object[] args)
        {
            await CleanupAsyncInternal(args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask CleanupAsync(Memory<object> args = default)
        {
            await CleanupAsyncInternal(args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTaskVoid CleanupAndForget(Memory<object> args)
        {
            await CleanupAsyncInternal(args);
        }
        
        private async UniTask CleanupAsyncInternal(Memory<object> args)
        {
            IsInTransition = false;

            var sheets = _panels;

            foreach (var sheetRef in sheets)
            {
                await sheetRef.View.BeforeReleaseAsync(args);
                DestroyAndForget(sheetRef);
            }

            sheets.Clear();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            var panels = _panels;

            foreach (var panelRef in panels)
            {
                (Panel panel, var resourcePath) = panelRef;
                DestroyAndForget(panel, resourcePath, PoolingPolicy.DisablePooling).Forget();
            }
            
            panels.Clear();
        }
        
        /// <summary>
        /// Add a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void AddCallbackReceiver(IPanelContainerCallbackReceiver callbackReceiver)
        {
            _callbackReceivers.Add(callbackReceiver);
        }

        /// <summary>
        /// Remove a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void RemoveCallbackReceiver(IPanelContainerCallbackReceiver callbackReceiver)
        {
            _callbackReceivers.Remove(callbackReceiver);
        }
        
        /// <summary>
        /// Searches through the <see cref="Panel"/> stack
        /// and returns the index of the Panel loaded from <paramref name="resourcePath"/>
        /// that has been recently pushed into this container if any.
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="index">
        /// Return a value greater or equal to 0 if there is
        /// a Panel loaded from this <paramref name="resourcePath"/>.
        /// </param>
        /// <returns>
        /// True if there is a Panel loaded from this <paramref name="resourcePath"/>.
        /// </returns>
        public bool FindIndexOfRecentlyPushed(string resourcePath, out int index)
        {
            if (resourcePath == null)
            {
                throw new ArgumentNullException(nameof(resourcePath));
            }

            var panels = this._panels;

            for (var i = panels.Count - 1; i >= 0; i--)
            {
                if (string.Equals(resourcePath, panels[i].ResourcePath))
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }
        
        /// <summary>
        /// Searches through the <see cref="Panel"/> stack
        /// and destroys the Panel loaded from <paramref name="resourcePath"/>
        /// that has been recently pushed into this container if any.
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="ignoreFront">Do not destroy if the Panel is in the front.</param>
        /// <returns>
        /// True if there is a Panel loaded from this <paramref name="resourcePath"/>.
        /// </returns>
        public void DestroyRecentlyPushed(string resourcePath, bool ignoreFront = true)
        {
            if (resourcePath == null)
            {
                throw new ArgumentNullException(nameof(resourcePath));
            }

            var frontIndex = _panels.Count - 1;

            if (FindIndexOfRecentlyPushed(resourcePath, out var index) == false)
            {
                return;
            }

            if (ignoreFront && frontIndex == index)
            {
                return;
            }

            var panel = _panels[index];
            _panels.RemoveAt(index);

            DestroyAndForget(panel);
        }

        #region BringToFront

        /// <summary>
        /// Bring an instance of <see cref="Panel"/> to the front.
        /// </summary>
        /// <param name="ignoreFront">Ignore if the panel is already in the front.</param>
        /// <remarks>Fire-and-forget</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BringToFront(PanelOptions options, bool ignoreFront, params object[] args)
        {
            BringToFrontAndForget(options, ignoreFront, args).Forget();
        }
        
        /// <summary>
        /// Bring an instance of <see cref="Panel"/> to the front.
        /// </summary>
        /// <param name="ignoreFront">Ignore if the panel is already in the front.</param>
        /// <remarks>Fire-and-forget</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BringToFront(PanelOptions options, bool ignoreFront, Memory<object> args = default)
        {
            BringToFrontAndForget(options, ignoreFront, args).Forget();
        }

        /// <summary>
        /// Bring an instance of <see cref="Panel"/> to the front.
        /// </summary>
        /// <param name="ignoreFront">Ignore if the panel is already in the front.</param>
        /// <remarks>Asynchronous</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask BringToFrontAsync(PanelOptions options, bool ignoreFront, params object[] args)
        {
            await BringToFrontAsyncInternal(options, ignoreFront, args);
        }
        
        /// <summary>
        /// Bring an instance of <see cref="Panel"/> to the front.
        /// </summary>
        /// <param name="ignoreFront">Ignore if the panel is already in the front.</param>
        /// <remarks>Asynchronous</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask BringToFrontAsync(PanelOptions options, bool ignoreFront, Memory<object> args = default)
        {
            await BringToFrontAsyncInternal(options, ignoreFront, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTaskVoid BringToFrontAndForget(PanelOptions options, bool ignoreFront, Memory<object> args)
        {
            await BringToFrontAsyncInternal(options, ignoreFront, args);
        }

        private async UniTask BringToFrontAsyncInternal(PanelOptions options, bool ignoreFront, Memory<object> args)
        {
            var resourcePath = options.options.resourcePath;

            if (resourcePath == null)
            {
                throw new ArgumentNullException(nameof(resourcePath));
            }

            var frontIndex = _panels.Count - 1;

            if (FindIndexOfRecentlyPushed(resourcePath, out var index) == false)
            {
                return;
            }

            if (ignoreFront && frontIndex == index)
            {
                return;
            }

            var enterPanel = _panels[index].View;
            enterPanel.Settings = Settings;

            var panelId = enterPanel.GetInstanceID();
            _panels.RemoveAt(index);

            RectTransform.RemoveChild(enterPanel.transform);

            options.options.onLoaded?.Invoke(enterPanel, args);

            await enterPanel.AfterLoadAsync(RectTransform, args);

            ViewRef<Panel>? exitPanelRef = _panels.Count == 0 ? null : _panels[^1];
            Panel exitPanel = exitPanelRef.HasValue ? exitPanelRef.Value.View : null;
            var exitPanelId = exitPanel == false ? (int?) null : exitPanel.GetInstanceID();

            if (exitPanel)
            {
                exitPanel.Settings = Settings;
            }

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforePush(enterPanel, exitPanel, args);
            }

            if (exitPanel)
            {
                await exitPanel.BeforeExitAsync(true, args);
            }

            await enterPanel.BeforeEnterAsync(true, args);

            // Play Animations
            var animExit = exitPanel
                ? exitPanel.ExitAsync(true, options.options.playAnimation, enterPanel)
                : default;

            var animEnter = enterPanel.EnterAsync(true, options.options.playAnimation, exitPanel);

            await UniTask.WhenAll(animExit, animEnter);

            // End Transition
            if (_isActivePanelStacked == false && exitPanelId.HasValue)
            {
                _panels.RemoveAt(_panels.Count - 1);
            }

            _panels.Add(new ViewRef<Panel>(enterPanel, resourcePath, options.options.poolingPolicy));
            IsInTransition = false;

            // Postprocess
            if (exitPanel)
            {
                exitPanel.AfterExit(true, args);
            }

            enterPanel.AfterEnter(true, args);

            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.AfterPush(enterPanel, exitPanel, args);
            }

            // Unload unused Panel
            if (_isActivePanelStacked == false && exitPanelRef.HasValue)
            {
                await exitPanel.BeforeReleaseAsync(args);

                DestroyAndForget(exitPanelRef.Value);
            }

            _isActivePanelStacked = options.stack;

            if (Settings.EnableInteractionInTransition == false)
            {
                Interactable = true;
            }
        }

        #endregion

        #region Push

        /// <summary>
        /// Push an instance of <typeparamref name="TPanel"/>.
        /// </summary>
        /// <remarks>Fire-and-forget</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push<TPanel>(PanelOptions options, params object[] args)
            where TPanel : Panel
        {
            PushAndForget<TPanel>(options, args).Forget();
        }
        
        /// <summary>
        /// Push an instance of <typeparamref name="TPanel"/>.
        /// </summary>
        /// <remarks>Fire-and-forget</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push<TPanel>(PanelOptions options, Memory<object> args = default)
            where TPanel : Panel
        {
            PushAndForget<TPanel>(options, args).Forget();
        }

        /// <summary>
        /// Push an instance of <see cref="Panel"/>.
        /// </summary>
        /// <remarks>Fire-and-forget</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(PanelOptions options, params object[] args)
        {
            PushAndForget<Panel>(options, args).Forget();
        }
        
        /// <summary>
        /// Push an instance of <see cref="Panel"/>.
        /// </summary>
        /// <remarks>Fire-and-forget</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(PanelOptions options, Memory<object> args = default)
        {
            PushAndForget<Panel>(options, args).Forget();
        }

        /// <summary>
        /// Push an instance of <typeparamref name="TPanel"/>.
        /// </summary>
        /// <remarks>Asynchronous</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask PushAsync<TPanel>(PanelOptions options, params object[] args)
            where TPanel : Panel
        {
            await PushAsyncInternal<TPanel>(options, args);
        }
        
        /// <summary>
        /// Push an instance of <typeparamref name="TPanel"/>.
        /// </summary>
        /// <remarks>Asynchronous</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask PushAsync<TPanel>(PanelOptions options, Memory<object> args = default)
            where TPanel : Panel
        {
            await PushAsyncInternal<TPanel>(options, args);
        }

        /// <summary>
        /// Push an instance of <see cref="Panel"/>.
        /// </summary>
        /// <remarks>Asynchronous</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask PushAsync(PanelOptions options, params object[] args)
        {
            await PushAsyncInternal<Panel>(options, args);
        }
        
        /// <summary>
        /// Push an instance of <see cref="Panel"/>.
        /// </summary>
        /// <remarks>Asynchronous</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask PushAsync(PanelOptions options, Memory<object> args = default)
        {
            await PushAsyncInternal<Panel>(options, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTaskVoid PushAndForget<TPanel>(PanelOptions options, Memory<object> args)
            where TPanel : Panel
        {
            await PushAsyncInternal<Panel>(options, args);
        }

        private async UniTask PushAsyncInternal<TPanel>(PanelOptions options, Memory<object> args)
            where TPanel : Panel
        {
            var resourcePath = options.options.resourcePath;

            if (resourcePath == null)
            {
                throw new ArgumentNullException(nameof(resourcePath));
            }

            if (IsInTransition)
            {
                ErrorIfCannotTransition();
                return;
            }

            IsInTransition = true;
            
            if (Settings.EnableInteractionInTransition == false)
            {
                Interactable = false;
            }

            var enterPanel = await GetViewAsync<TPanel>(options.options);
            options.options.onLoaded?.Invoke(enterPanel, args);

            await enterPanel.AfterLoadAsync(RectTransform, args);

            ViewRef<Panel>? exitPanelRef = _panels.Count == 0 ? null : _panels[^1];
            Panel exitPanel = exitPanelRef?.View;
            var exitPanelId = exitPanel == null ? (int?) null : exitPanel.GetInstanceID();

            if (exitPanel)
            {
                exitPanel.Settings = Settings;
            }

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforePush(enterPanel, exitPanel, args);
            }

            if (exitPanel)
            {
                await exitPanel.BeforeExitAsync(true, args);
            }

            await enterPanel.BeforeEnterAsync(true, args);

            // Play Animations
            var animExit = exitPanel
                ? exitPanel.ExitAsync(true, options.options.playAnimation, enterPanel)
                : default;

            var animEnter = enterPanel.EnterAsync(true, options.options.playAnimation, exitPanel);

            await UniTask.WhenAll(animExit, animEnter);

            // End Transition
            if (_isActivePanelStacked == false && exitPanelId.HasValue)
            {
                _panels.RemoveAt(_panels.Count - 1);
            }

            _panels.Add(new ViewRef<Panel>(enterPanel, resourcePath, options.options.poolingPolicy));
            IsInTransition = false;

            // Postprocess
            if (exitPanel)
            {
                exitPanel.AfterExit(true, args);
            }

            enterPanel.AfterEnter(true, args);

            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.AfterPush(enterPanel, exitPanel, args);
            }

            // Unload unused Panel
            if (_isActivePanelStacked == false && exitPanelRef.HasValue)
            {
                await exitPanel.BeforeReleaseAsync(args);

                DestroyAndForget(exitPanelRef.Value);
            }

            _isActivePanelStacked = options.stack;
            
            if (Settings.EnableInteractionInTransition == false)
            {
                Interactable = true;
            }
        }

        #endregion

        #region Pop

        /// <summary>
        /// Pop current instance of <see cref="Panel"/>.
        /// </summary>
        /// <remarks>Fire-and-forget</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pop(bool playAnimation, params object[] args)
        {
            PopAndForget(playAnimation, args).Forget();
        }
        
        /// <summary>
        /// Pop current instance of <see cref="Panel"/>.
        /// </summary>
        /// <remarks>Fire-and-forget</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pop(bool playAnimation, Memory<object> args = default)
        {
            PopAndForget(playAnimation, args).Forget();
        }

        /// <summary>
        /// Pop current instance of <see cref="Panel"/>.
        /// </summary>
        /// <remarks>Asynchronous</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask PopAsync(bool playAnimation, params object[] args)
        {
            await PopAsyncInternal(playAnimation, args);
        }
        
        /// <summary>
        /// Pop current instance of <see cref="Panel"/>.
        /// </summary>
        /// <remarks>Asynchronous</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask PopAsync(bool playAnimation, Memory<object> args = default)
        {
            await PopAsyncInternal(playAnimation, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTaskVoid PopAndForget(bool playAnimation, Memory<object> args)
        {
            await PopAsyncInternal(playAnimation, args);
        }

        private async UniTask PopAsyncInternal(bool playAnimation, Memory<object> args)
        {
            if (_panels.Count == 0)
            {
                ErrorIfCannotTransitionBecauseNoPanel();
                return;
            }

            if (IsInTransition)
            {
                ErrorIfCannotTransition();
                return;
            }

            IsInTransition = true;
            
            if (Settings.EnableInteractionInTransition == false)
            {
                Interactable = false;
            }

            var lastPanel = _panels.Count - 1;
            var exitPanelRef = _panels[lastPanel];
            var exitPanel = exitPanelRef.View;
            exitPanel.Settings = Settings;

            var enterPanel = _panels.Count == 1 ? null : _panels[^2].View;

            if (enterPanel)
            {
                enterPanel.Settings = Settings;
            }

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforePop(enterPanel, exitPanel, args);
            }

            await exitPanel.BeforeExitAsync(false, args);

            if (enterPanel)
            {
                await enterPanel.BeforeEnterAsync(false, args);
            }

            // Play Animations
            var animExit = exitPanel.ExitAsync(false, playAnimation, enterPanel);

            var animEnter = enterPanel
                ? enterPanel.EnterAsync(false, playAnimation, exitPanel)
                : default;

            await UniTask.WhenAll(animExit, animEnter);

            // End Transition
            _panels.RemoveAt(lastPanel);
            IsInTransition = false;

            // Postprocess
            exitPanel.AfterExit(false, args);

            if (enterPanel)
            {
                enterPanel.AfterEnter(false, args);
            }

            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.AfterPop(enterPanel, exitPanel, args);
            }

            // Unload unused Panel
            await exitPanel.BeforeReleaseAsync(args);

            DestroyAndForget(exitPanelRef);

            _isActivePanelStacked = true;
            
            if (Settings.EnableInteractionInTransition == false)
            {
                Interactable = true;
            }
        }

        #endregion
        
        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorCannotFindParent(RectTransform rectTransform)
        {
            UnityEngine.Debug.LogError($"Cannot find any parent {nameof(PanelContainer)} component", rectTransform);
        }

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfCannotFindContainer(string containerName)
        {
            UnityEngine.Debug.LogError($"Cannot find any {nameof(PanelContainer)} by name `{containerName}`");
        }

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfCannotTransition()
        {
            UnityEngine.Debug.LogError("Cannot transition because there is a panel already in transition.");
        }

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfCannotTransitionBecauseNoPanel()
        {
            UnityEngine.Debug.LogError("Cannot transition because there is no panel loaded on the stack.");
        }
    }
}