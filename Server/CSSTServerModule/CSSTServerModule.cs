using System;
using System.Linq;
using System.Reflection;
using Prism.Modularity;
using Prism.Regions;
using TRFCommonLibs;

namespace CSSTServerModule
{
    public class CSSTServerModule : IModule
    {
        private readonly IRegionManager _regionManager;
        private readonly CSSTDatabaseController _csstDatabaseController;
        private readonly CSSTDataAnalysis _csstDataAnalysis;

        public CSSTServerModule(IRegionManager regionManager)
        {
            AppDomain.CurrentDomain.AssemblyResolve += this.CSSTAssemblyResolveHandler;
            this._regionManager = regionManager;
            this._csstDatabaseController = new CSSTDatabaseController();
            this._csstDataAnalysis = new CSSTDataAnalysis();
        }

        public void Initialize()
        {
            this._regionManager.RegisterViewWithRegion("TRFServerControlPanelRegion", typeof (CSSTServerView));
            string className = typeof (CSSTServerModule).Name;
            if (!TRFGlobalVariables.ModuleNameID.ContainsKey(className))
                TRFGlobalVariables.ModuleNameID.Add(className, TRFGlobalVariables.ModuleNameID.Count);
        }

        private Assembly CSSTAssemblyResolveHandler(object sender, ResolveEventArgs e)
        {
            //This handler is called only when the common language runtime tries to bind to the assembly and fails.
            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            string strTempAssmbPath = "";

            var objExecutingAssemblies = Assembly.GetExecutingAssembly();
            AssemblyName[] arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();

            //Loop through the array of referenced assembly names.
            if (arrReferencedAssmbNames.Any(strAssmbName => strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",", StringComparison.Ordinal)) == e.Name.Substring(0, e.Name.IndexOf(",", StringComparison.Ordinal))))
            {
                strTempAssmbPath = Environment.CurrentDirectory + "\\ServerUserModules\\";
                if (strTempAssmbPath.EndsWith("\\")) strTempAssmbPath += "\\";
                strTempAssmbPath += e.Name.Substring(0, e.Name.IndexOf(",", StringComparison.Ordinal)) + ".dll";
            }
            //Load the assembly from the specified path.
            Assembly myAssembly = null;
            if (!string.IsNullOrWhiteSpace(strTempAssmbPath))
                myAssembly = Assembly.LoadFrom(strTempAssmbPath);

            //Return the loaded assembly.
            return myAssembly;
        }
    }
}
