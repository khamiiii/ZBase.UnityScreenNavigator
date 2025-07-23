using ZBase.UnityScreenNavigator.Core.Views;

namespace ZBase.UnityScreenNavigator.Core.Panel
{
    public readonly struct PanelOptions
    {
        public readonly ViewOptions options;
        public readonly string identifier;

        public PanelOptions(in ViewOptions options, string identifier)
        {
            this.options = options;
            this.identifier = identifier;
        }
        
        public PanelOptions(
            string resourcePath
            , string identifier
            , bool playAnimation = true
            , OnViewLoadedCallback onLoaded = null
            , bool loadAsync = true
            , PoolingPolicy poolingPolicy = PoolingPolicy.UseSettings
        )
        {
            this.options = new(resourcePath, playAnimation, onLoaded, loadAsync, poolingPolicy);
            this.identifier = identifier;
        }
        
        public static implicit operator ViewOptions(in PanelOptions options)
            => options.options;
    }
}