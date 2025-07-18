using ZBase.UnityScreenNavigator.Core.Views;

namespace ZBase.UnityScreenNavigator.Core.Panel
{
    public readonly struct PanelOptions
    {
        public readonly bool stack;
        public readonly ViewOptions options;

        public PanelOptions(in ViewOptions options, bool stack = true)
        {
            this.options = options;
            this.stack = stack;
        }
        
        public PanelOptions(
            string resourcePath
            , bool playAnimation = true
            , OnViewLoadedCallback onLoaded = null
            , bool loadAsync = true
            , bool stack = true
            , PoolingPolicy poolingPolicy = PoolingPolicy.UseSettings
        )
        {
            this.options = new(resourcePath, playAnimation, onLoaded, loadAsync, poolingPolicy);
            this.stack = stack;
        }
        
        public static implicit operator PanelOptions(in ViewOptions options)
            => new(options);

        public static implicit operator PanelOptions(string resourcePath)
            => new(new ViewOptions(resourcePath));

        public static implicit operator ViewOptions(in PanelOptions options)
            => options.options;
    }
}