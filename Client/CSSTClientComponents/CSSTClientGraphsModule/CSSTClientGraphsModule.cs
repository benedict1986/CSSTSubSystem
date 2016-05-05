using Prism.Modularity;
using Prism.Regions;

namespace CSSTClientGraphsModule
{
    public class CSSTClientGraphsModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public CSSTClientGraphsModule(IRegionManager regionManager)
        {
            this._regionManager = regionManager;
        }

        public void Initialize()
        {
            this._regionManager.RegisterViewWithRegion("CSSTClientGraphsRegion", typeof(CSSTClientGraphsView));
        }
    }
}
