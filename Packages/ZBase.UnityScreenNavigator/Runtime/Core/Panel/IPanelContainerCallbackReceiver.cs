using System;

namespace ZBase.UnityScreenNavigator.Core.Panel
{
    public interface IPanelContainerCallbackReceiver
    {
        void BeforePush(Panel enterScreen, Panel exitScreen, Memory<object> args);

        void AfterPush(Panel enterScreen, Panel exitScreen, Memory<object> args);

        void BeforePop(Panel enterScreen, Panel exitScreen, Memory<object> args);

        void AfterPop(Panel enterScreen, Panel exitScreen, Memory<object> args);
    }
}