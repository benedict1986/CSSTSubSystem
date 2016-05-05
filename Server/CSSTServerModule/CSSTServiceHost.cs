using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Configuration;

namespace CSSTServerModule
{
    public class CSSTServiceHost : ServiceHost
    {
        public CSSTServiceHost(Type serviceType, params Uri[] baseAddresses) : base(serviceType, baseAddresses) { }

        protected override void ApplyConfiguration()
        {    
            string dir = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string myConfigFilePath = Path.Combine(dir, "ServerUserModules\\CSSTServerModule.config");
            if (!File.Exists(myConfigFilePath))
            {
                base.ApplyConfiguration();
                return;
            }
            var configFileMap = new ExeConfigurationFileMap {ExeConfigFilename = myConfigFilePath};
            var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap,
                ConfigurationUserLevel.None);
            var serviceModel = ServiceModelSectionGroup.GetSectionGroup(config);
            if (serviceModel == null)
            {
                base.ApplyConfiguration();
                return;
            }
            foreach (ServiceElement serviceElement in serviceModel.Services.Services)
            {
                this.LoadConfigurationSection(serviceElement);
                return;
            }
        }
    }
}
