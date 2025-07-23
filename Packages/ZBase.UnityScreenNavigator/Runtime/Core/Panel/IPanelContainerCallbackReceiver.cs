using System;

namespace ZBase.UnityScreenNavigator.Core.Panel
{
    public interface IPanelContainerCallbackReceiver
    {
        void BeforeShow(Panel enterPanel, Panel exitPanel, Memory<object> args);

        void AfterShow(Panel enterPanel, Panel exitPanel, Memory<object> args);

        void BeforeHide(Panel exitPanel, Memory<object> args);

        void AfterHide(Panel exitPanel, Memory<object> args);
    }
}