using Prism.Modularity;
using Prism.Regions;

namespace CSSTClientStatisticModule
{
    public class CSSTClientStatisticModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public CSSTClientStatisticModule(IRegionManager regionManager)
        {
            this._regionManager = regionManager;
        }

        public void Initialize()
        {
            this._regionManager.RegisterViewWithRegion("CSSTClientStatisticRegion", typeof(CSSTClientStatisticView));
        }
    }
}
