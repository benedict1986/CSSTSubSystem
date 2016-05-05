using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using CSSTCommonLibs;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using TRFCommonLibs;

namespace CSSTClientShellModule
{
    [Guid("E6F9AE80-D68B-444E-94F1-D88846CC4AA1")]
    public class CSSTClientShellModule : IModule
    {
        private readonly IRegionManager _regionManager;
        private readonly ModuleCatalog _moduleCatalog;
        private readonly IModuleManager _moduleManager;
        private readonly Guid _guid = typeof(CSSTClientShellModule).GUID;
        private readonly SubscriptionToken _trfLoginUserReadyEventST;
        public CSSTClientShellModule(IRegionManager regionManager, IModuleCatalog moduleCatalog, IModuleManager moduleManager)
        {
            this._regionManager = regionManager;
            this._moduleCatalog = (ModuleCatalog)moduleCatalog;
            this._moduleManager = moduleManager;
            this._trfLoginUserReadyEventST = TRFEventHelpers.ReceiveTRFLoginUserReadyEvent(this.TRFLoginUserReady, null, ThreadOption.UIThread);
        }

        ~CSSTClientShellModule()
        {
        }

        public void Initialize()
        {
            if (TRFGlobalVariables.LoginTRFUser!= null)
            {
                this._regionManager.RegisterViewWithRegion("TestsRegion", typeof(CSSTClientShellView));
                this.LoadAndDisplayTestsModules();
            }

        }

        #region Event Handlers
        private void TRFLoginUserReady(TRFEventArg e)
        {
            if (e.payload != null)
            {
                this._regionManager.RegisterViewWithRegion("TestsRegion", typeof(CSSTClientShellView));
                this.LoadAndDisplayTestsModules();
            }
        }
        #endregion

        private void LoadAndDisplayTestsModules()
        {
            string path;
            Type type;
            if (!(from module in this._moduleCatalog.Modules
                  where module.ModuleName == "CSSTClientControlPanelModule"
                  select module).Any())
            {
                path = Environment.CurrentDirectory +
                              "\\ClientUserModules\\CSSTClientComponents\\CSSTClientControlPanelModule.dll";
                type =
                    Assembly.LoadFile(path).GetType("CSSTClientControlPanelModule.CSSTClientControlPanelModule");

                this._moduleCatalog.AddModule(new ModuleInfo
                {
                    ModuleName = type.Name,
                    ModuleType = type.AssemblyQualifiedName,
                    Ref = new Uri(path, UriKind.RelativeOrAbsolute).AbsoluteUri
                });
                this._moduleManager.LoadModule("CSSTClientControlPanelModule");
            }

            path = Environment.CurrentDirectory +
                   "\\ClientUserModules\\CSSTClientComponents\\TRFKinectVideoDisplayModule.dll";
            if (File.Exists(path))
            {
                type =
                    Assembly.LoadFile(path).GetType("TRFKinectVideoDisplayModule.TRFKinectVideoDisplayView");
                IRegion region = this._regionManager.Regions["CSSTKinectVideoDisplayRegion"];
                region.Add(Activator.CreateInstance(type));

            }

            if (!(from module in this._moduleCatalog.Modules
                  where module.ModuleName == "CSSTClientGraphsModule"
                  select module).Any())
            {
                path = Environment.CurrentDirectory +
                              "\\ClientUserModules\\CSSTClientComponents\\CSSTClientGraphsModule.dll";
                type =
                    Assembly.LoadFile(path).GetType("CSSTClientGraphsModule.CSSTClientGraphsModule");

                this._moduleCatalog.AddModule(new ModuleInfo
                {
                    ModuleName = type.Name,
                    ModuleType = type.AssemblyQualifiedName,
                    Ref = new Uri(path, UriKind.RelativeOrAbsolute).AbsoluteUri
                });
                this._moduleManager.LoadModule("CSSTClientGraphsModule");
            }

            if (!(from module in this._moduleCatalog.Modules
                  where module.ModuleName == "CSSTClientStatisticModule"
                  select module).Any())
            {
                path = Environment.CurrentDirectory +
                              "\\ClientUserModules\\CSSTClientComponents\\CSSTClientStatisticModule.dll";
                type =
                    Assembly.LoadFile(path).GetType("CSSTClientStatisticModule.CSSTClientStatisticModule");

                this._moduleCatalog.AddModule(new ModuleInfo
                {
                    ModuleName = type.Name,
                    ModuleType = type.AssemblyQualifiedName,
                    Ref = new Uri(path, UriKind.RelativeOrAbsolute).AbsoluteUri
                });
                this._moduleManager.LoadModule("CSSTClientStatisticModule");
            }

            if (!(from module in this._moduleCatalog.Modules
                  where module.ModuleName == "CSSTClientServiceModule"
                  select module).Any())
            {
                path = Environment.CurrentDirectory +
                              "\\ClientUserModules\\CSSTClientComponents\\CSSTClientServiceModule.dll";
                type =
                    Assembly.LoadFile(path).GetType("CSSTClientServiceModule.CSSTClientServiceModule");

                this._moduleCatalog.AddModule(new ModuleInfo
                {
                    ModuleName = type.Name,
                    ModuleType = type.AssemblyQualifiedName,
                    Ref = new Uri(path, UriKind.RelativeOrAbsolute).AbsoluteUri
                });
                this._moduleManager.LoadModule("CSSTClientServiceModule");
            }

            foreach (var view in this._regionManager.Regions["TestsRegion"].Views.Where(view => view.GetType() == typeof(CSSTClientShellView)))
            {
                this._regionManager.Regions["TestsRegion"].Activate(view);
            }

            CSSTEventHelpers.SendCSSTUserLoginRequestedEvent(this._guid, TRFGlobalVariables.LoginTRFUser);
            TRFEventHelpers.SendTRFUserListRequestedEvent(this._guid, true);
            TRFEventHelpers.SendTRFKinectColorImageRequestedEvent(this._guid, true);
            TRFEventHelpers.SendTRFKinectBodyDataRequestedEvent(this._guid, true);
        }
    }
}
