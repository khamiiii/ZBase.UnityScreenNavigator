using ZBase.UnityScreenNavigator.Core.Views;

namespace ZBase.UnityScreenNavigator.Core.Panel
{
    public readonly struct PanelOptions
    {
        public readonly bool stack;
        public readonly ViewOptions options;
        public readonly string identifier;

        public PanelOptions(in ViewOptions options, string identifier, bool stack = true)
        {
            this.options = options;
            this.stack = stack;
            this.identifier = identifier;
        }
        
        public PanelOptions(
            string resourcePath
            , string identifier
            , bool playAnimation = true
            , OnViewLoadedCallback onLoaded = null
            , bool loadAsync = true
            , bool stack = true
            , PoolingPolicy poolingPolicy = PoolingPolicy.UseSettings
        )
        {
            this.options = new(resourcePath, playAnimation, onLoaded, loadAsync, poolingPolicy);
            this.stack = stack;
            this.identifier = identifier;
        }
        
        public static implicit operator ViewOptions(in PanelOptions options)
            => options.options;
    }
}