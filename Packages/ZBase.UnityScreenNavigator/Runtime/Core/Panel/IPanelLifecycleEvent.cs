using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZBase.UnityScreenNavigator.Core.Panel
{
    public interface IPanelLifecycleEvent
    {
        /// <summary>
        /// Called just after this panel is loaded.
        /// </summary>
        /// <returns></returns>
        UniTask Initialize(Memory<object> args, CancellationToken ct);

        /// <summary>
        /// Called just before this panel is displayed by the Push transition.
        /// </summary>
        /// <returns></returns>
        UniTask WillPushEnter(Memory<object> args, CancellationToken ct);

        /// <summary>
        /// Called just after this panel is displayed by the Push transition.
        /// </summary>
        void DidPushEnter(Memory<object> args);

        /// <summary>
        /// Called just before this panel is hidden by the Push transition.
        /// </summary>
        /// <returns></returns>
        UniTask WillPushExit(Memory<object> args, CancellationToken ct);

        /// <summary>
        /// Called just after this panel is hidden by the Push transition.
        /// </summary>
        void DidPushExit(Memory<object> args);

        /// <summary>
        /// Called just before this panel is displayed by the Pop transition.
        /// </summary>
        /// <returns></returns>
        UniTask WillPopEnter(Memory<object> args, CancellationToken ct);

        /// <summary>
        /// Called just after this panel is displayed by the Pop transition.
        /// </summary>
        void DidPopEnter(Memory<object> args);

        /// <summary>
        /// Called just before this panel is hidden by the Pop transition.
        /// </summary>
        /// <returns></returns>
        UniTask WillPopExit(Memory<object> args, CancellationToken ct);

        /// <summary>
        /// Called just after this panel is hidden by the Pop transition.
        /// </summary>
        void DidPopExit(Memory<object> args);

        /// <summary>
        /// Called just before this panel is released.
        /// </summary>
        /// <returns></returns>
        UniTask Cleanup(Memory<object> args, CancellationToken ct);
    }
}