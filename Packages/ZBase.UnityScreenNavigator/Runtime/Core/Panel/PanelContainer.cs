using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZBase.UnityScreenNavigator.Core.Controls;
using ZBase.UnityScreenNavigator.Foundation;

namespace ZBase.UnityScreenNavigator.Core.Panel
{
    public class PanelContainer : ControlContainerBase
    {
        private readonly List<IPanelContainerCallbackReceiver> _callbackReceivers = new();
        private ViewRef<Panel>? _activePanel;
        
        /// <summary>
        /// True if in transition.
        /// </summary>
        public bool IsInTransition { get; private set; }
        

        public ViewRef<Panel>? ActivePanel => _activePanel;

        private CancellationTokenSource _transitionCts;
        private ViewRef<Panel> _transitioningPanel;

        protected override void Awake()
        {
            base.Awake();
            
            _callbackReceivers.AddRange(GetComponents<IPanelContainerCallbackReceiver>());
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

            var panelRef = _activePanel;
            if (panelRef.HasValue)
            {
                await panelRef.Value.View.BeforeReleaseAsync(args, default);

                DestroyAndForget(panelRef.Value);
            }

            _activePanel = null;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_activePanel.HasValue)
            {
                var (panel, resourcePath) = _activePanel.Value;
                DestroyAndForget(panel, resourcePath, PoolingPolicy.DisablePooling).Forget();

                _activePanel = null;
            }
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
        
        public bool IsTransitioningIn(string identifier)
        {
            if (!_transitioningPanel.View || _transitioningPanel.View.Identifier != identifier)
                return false;

            var animationType = _transitioningPanel.View.TransitionAnimationType;
            return animationType is PanelTransitionAnimationType.Enter;
        }

        public bool IsTransitioningOut(string identifier)
        {
            if (!_transitioningPanel.View || _transitioningPanel.View.Identifier != identifier)
                return false;

            var animationType = _transitioningPanel.View.TransitionAnimationType;
            return animationType is PanelTransitionAnimationType.Exit;
        }

        #region Show

        /// <summary>
        /// Push an instance of <typeparamref name="TPanel"/>.
        /// </summary>
        /// <remarks>Fire-and-forget</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Show<TPanel>(PanelOptions options, params object[] args)
            where TPanel : Panel
        {
            ShowAndForget<TPanel>(options, args).Forget();
        }
        
        /// <summary>
        /// Push an instance of <typeparamref name="TPanel"/>.
        /// </summary>
        /// <remarks>Fire-and-forget</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Show<TPanel>(PanelOptions options, Memory<object> args = default)
            where TPanel : Panel
        {
            ShowAndForget<TPanel>(options, args).Forget();
        }

        /// <summary>
        /// Push an instance of <see cref="Panel"/>.
        /// </summary>
        /// <remarks>Fire-and-forget</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Show(PanelOptions options, params object[] args)
        {
            ShowAndForget<Panel>(options, args).Forget();
        }
        
        /// <summary>
        /// Push an instance of <see cref="Panel"/>.
        /// </summary>
        /// <remarks>Fire-and-forget</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Show(PanelOptions options, Memory<object> args = default)
        {
            ShowAndForget<Panel>(options, args).Forget();
        }

        /// <summary>
        /// Push an instance of <typeparamref name="TPanel"/>.
        /// </summary>
        /// <remarks>Asynchronous</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask ShowAsync<TPanel>(PanelOptions options, params object[] args)
            where TPanel : Panel
        {
            await ShowAsyncInternal<TPanel>(options, args);
        }
        
        /// <summary>
        /// Push an instance of <typeparamref name="TPanel"/>.
        /// </summary>
        /// <remarks>Asynchronous</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask ShowAsync<TPanel>(PanelOptions options, Memory<object> args = default)
            where TPanel : Panel
        {
            await ShowAsyncInternal<TPanel>(options, args);
        }

        /// <summary>
        /// Push an instance of <see cref="Panel"/>.
        /// </summary>
        /// <remarks>Asynchronous</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask ShowAsync(PanelOptions options, params object[] args)
        {
            await ShowAsyncInternal<Panel>(options, args);
        }
        
        /// <summary>
        /// Push an instance of <see cref="Panel"/>.
        /// </summary>
        /// <remarks>Asynchronous</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask ShowAsync(PanelOptions options, Memory<object> args = default)
        {
            await ShowAsyncInternal<Panel>(options, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTaskVoid ShowAndForget<TPanel>(PanelOptions options, Memory<object> args)
            where TPanel : Panel
        {
            await ShowAsyncInternal<TPanel>(options, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTask ShowAsyncInternal<TPanel>(PanelOptions options, Memory<object> args)
            where TPanel : Panel
        {
            ForceCompleteTransition();
            
            await UniTask.WaitUntil((() => IsInTransition == false));
            await ShowAsyncInternal<TPanel>(options, args, _transitionCts.Token);
        }
        
        private async UniTask ShowAsyncInternal<TPanel>(PanelOptions options, Memory<object> args, CancellationToken ct)
            where TPanel : Panel
        {
            var resourcePath = options.options.resourcePath;

            if (IsInTransition)
            {
                ErrorIfCannotTransition();
                return;
            }

            if (_activePanel?.View.Identifier == options.identifier)
            {
                WarningIfCannotTransitionBecauseActive(options.identifier);
                return;
            }

            IsInTransition = true;
            
            if (Settings.EnableInteractionInTransition == false)
            {
                Interactable = false;
            }
            
            var enterPanel = await GetViewAsync<TPanel>(options.options);
            //Set identifier BEFORE onLoaded
            enterPanel.Identifier = options.identifier;
            options.options.onLoaded?.Invoke(enterPanel, args);
            
            await enterPanel.AfterLoadAsync(RectTransform, args, ct);

            ViewRef<Panel>? exitPanelRef = _activePanel;
            var exitPanel = exitPanelRef?.View;

            if (exitPanel) 
                exitPanel.Settings = Settings;

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforeShow(enterPanel, exitPanel, args);
            }

            if (exitPanel)
            {
                await exitPanel.BeforeExitAsync(args, ct);
            }

            await enterPanel.BeforeEnterAsync(args, ct);
            
            // Play Animation
            var animExit = exitPanel
                ? exitPanel.ExitAsync(options.options.playAnimation, enterPanel, ct)
                : default;

            var animEnter = enterPanel.EnterAsync(options.options.playAnimation, exitPanel, ct);

            try
            {
                await UniTask.WhenAll(animExit, animEnter);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            finally
            {
                _activePanel = new ViewRef<Panel>(enterPanel, resourcePath, options.options.poolingPolicy);
                IsInTransition = false;
                
                // Postprocess
                if (exitPanel)
                {
                    exitPanel.AfterExit(args);
                }

                enterPanel.AfterEnter(args);

                foreach (var callbackReceiver in _callbackReceivers)
                {
                    callbackReceiver.AfterShow(enterPanel, exitPanel, args);
                }

                // Unload unused Panel
                if (exitPanelRef.HasValue)
                {
                    if (!ct.IsCancellationRequested)
                        await exitPanel.BeforeReleaseAsync(args, ct);
                    else
                        exitPanel.BeforeReleaseAsync(args, ct).Forget();

                    DestroyAndForget(exitPanelRef.Value);
                }

                if (Settings.EnableInteractionInTransition == false)
                {
                    Interactable = true;
                }
            }
        }

        #endregion

        #region Hide

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Hide(string identifier, bool playAnimation, params object[] args)
        {
            HideAndForget(identifier, playAnimation, args).Forget();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Hide(string identifier, bool playAnimation, Memory<object> args)
        {
            HideAndForget(identifier, playAnimation, args).Forget();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask HideAsync(string identifier, bool playAnimation, params object[] args)
        {
            await HideAsyncInternal(identifier, playAnimation, args);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask HideAsync(string identifier, bool playAnimation, Memory<object> args)
        {
            await HideAsyncInternal(identifier, playAnimation, args);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTaskVoid HideAndForget(string identifier, bool playAnimation, Memory<object> args)
        {
            await HideAsyncInternal(identifier, playAnimation, args);
        }
        
        private async UniTask HideAsyncInternal(string identifier, bool playAnimation, Memory<object> args = default)
        {
            ForceCompleteTransition();
            
            await UniTask.WaitUntil((() => IsInTransition == false));
            
            await HideAsyncInternal(identifier, playAnimation, args, _transitionCts.Token);
        }
        
        private async UniTask HideAsyncInternal(string identifier, bool playAnimation, Memory<object> args, CancellationToken ct)
        {
            if (!_activePanel.HasValue || _activePanel.Value.View.Identifier != identifier)
            {
                ErrorIfCannotHideBecauseNoPanel(identifier);
                return;
            }

            if (IsInTransition)
            {
                ErrorIfCannotTransition();
                return;
            }
            
            IsInTransition = true;

            if (Settings.EnableInteractionInTransition == false) 
                Interactable = false;

            var exitPanelRef = _activePanel.Value;
            var exitPanel = exitPanelRef.View;
            exitPanel.Settings = Settings;

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforeHide(exitPanel, args);
            }

            if (!ct.IsCancellationRequested)
                await exitPanel.BeforeExitAsync(args, ct);
            else
                exitPanel.BeforeExitAsync(args, ct).Forget();

            // Play Animations
            try
            {
                await exitPanel.ExitAsync(playAnimation, null, ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            finally
            {
                // End Transition (Cleanup, regardless of cancellation)
                _activePanel = null;
                IsInTransition = false;
                
                // Postprocess
                exitPanel.AfterExit(args);

                foreach (var callbackReceiver in _callbackReceivers)
                {
                    callbackReceiver.AfterHide(exitPanel, args);
                }

                // Unload unused Panel
                if (!ct.IsCancellationRequested)
                    await exitPanel.BeforeReleaseAsync(args, ct);
                else
                    exitPanel.BeforeReleaseAsync(args, ct).Forget();

                DestroyAndForget(exitPanelRef);

                if (Settings.EnableInteractionInTransition == false)
                {
                    Interactable = true;
                }
            }
        }

        #endregion
        
        #region Toggle

        public void Toggle(PanelOptions options, params object[] args)
        {
            ToggleAsyncInternal<Panel>(options, args).Forget();
        }
        
        public void Toggle(PanelOptions options, Memory<object> args = default)
        {
            ToggleAsyncInternal<Panel>(options, args).Forget();
        }
        
        public async UniTask ToggleAsync(PanelOptions options, Memory<object> args)
        {
            await ToggleAsyncInternal<Panel>(options, args);
        }
        
        public async UniTask ToggleAsync(PanelOptions options, params object[] args)
        {
            await ToggleAsyncInternal<Panel>(options, args);
        }

        private async UniTask ToggleAsyncInternal<TPanel>(PanelOptions options, Memory<object> args)
            where TPanel : Panel
        {
            ForceCompleteTransition();
            
            await UniTask.WaitUntil((() => IsInTransition == false));
            await ToggleAsyncInternal<TPanel>(options, args, _transitionCts.Token);
        }
        
        private async UniTask ToggleAsyncInternal<TPanel>(PanelOptions options, Memory<object> args, CancellationToken ct)
            where TPanel : Panel
        {
            var newIdentifier = options.identifier;
            var currentIdentifier = _activePanel?.View.Identifier;

            if (newIdentifier == currentIdentifier)
            {
                await HideAsyncInternal(newIdentifier, options.options.playAnimation, args, ct);
            }
            else
            {
                await ShowAsyncInternal<TPanel>(options, args, ct);
            }
        }

        #endregion

        public void ForceCompleteTransition()
        {
            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            _transitionCts = new CancellationTokenSource();
        }
        
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
        
        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfCannotHideBecauseNoPanel(string identifier)
        {
            UnityEngine.Debug.LogError($"Cannot hide `{identifier}` because there is no panel loaded on the stack.");
        }
        
        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void WarningIfCannotTransitionBecauseActive(string pageIdentifier)
        {
            UnityEngine.Debug.LogWarning($"Cannot transition because the panel {pageIdentifier} is already active.");
        }
    }
}