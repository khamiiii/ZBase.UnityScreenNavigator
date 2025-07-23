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
        /// Called just before this panel is displayed by the Show transition.
        /// </summary>
        /// <returns></returns>
        UniTask WillEnter(Memory<object> args, CancellationToken ct);

        /// <summary>
        /// Called just after this panel is displayed by the Show transition.
        /// </summary>
        void DidEnter(Memory<object> args);

        /// <summary>
        /// Called just before this panel is hidden by the Hide transition.
        /// </summary>
        /// <returns></returns>
        UniTask WillExit(Memory<object> args, CancellationToken ct);

        /// <summary>
        /// Called just after this panel is hidden by the Hide transition.
        /// </summary>
        void DidExit(Memory<object> args);

        /// <summary>
        /// Called just before this panel is released.
        /// </summary>
        /// <returns></returns>
        UniTask Cleanup(Memory<object> args, CancellationToken ct);
    }
}