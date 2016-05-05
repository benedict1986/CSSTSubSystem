using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using CSSTCommonLibs;
using Prism.Events;
using TRFCommonLibs;
using System.Runtime.InteropServices;
using OxyPlot;
using OxyPlot.Series;


namespace CSSTClientGraphsModule
{
    [Guid("431A4566-D6D0-407F-BBF0-CA2BF7B87E35")]
    public class CSSTClientGraphsViewModel : TRFNotifyPropertyChanged
    {
        private readonly Guid _guid = typeof (CSSTClientGraphsViewModel).GUID;
        private readonly CSSTClientGraphsModel _csstClientGraphsModel;
        private readonly SubscriptionToken _csstDataAnalysisResultReadyEventST;

        public ObservableCollection<Point>[] timeGraphData
        {
            get { return this._csstClientGraphsModel.timeGraphData; }
            set
            {
                if (value == null || this._csstClientGraphsModel.timeGraphData == value) return;
                this._csstClientGraphsModel.timeGraphData = value;
                this.NotifyPropertyChanged();
            }
        }
        public ObservableCollection<Point>[] healthyTimeGraphData {
            get { return this._csstClientGraphsModel.healthyTimeGraphData; }
            set
            {
                if (value == null || this._csstClientGraphsModel.healthyTimeGraphData == value) return;
                this._csstClientGraphsModel.healthyTimeGraphData = value;
                this.NotifyPropertyChanged();
            } }
        public ObservableCollection<Point>[] armSwingGraphData
        {
            get { return this._csstClientGraphsModel.armSwingGraphData; }
            set
            {
                if (value == null || this._csstClientGraphsModel.armSwingGraphData == value) return;
                this._csstClientGraphsModel.armSwingGraphData = value;
                this.NotifyPropertyChanged();
            }
        }
        public ObservableCollection<Point>[] healthyArmSwingGraphData {
            get { return this._csstClientGraphsModel.healthyArmSwingGraphData; }
            set
            {
                if (value == null || this._csstClientGraphsModel.healthyArmSwingGraphData == value) return;
                this._csstClientGraphsModel.healthyArmSwingGraphData = value;
                this.NotifyPropertyChanged();
            }
        }
        //public Range<double> timeGraphYRange
        //{
        //    get { return this._csstClientGraphsModel.timeGraphYRange; }
        //    set
        //    {
        //        if (this._csstClientGraphsModel.timeGraphYRange == value) return;
        //        this._csstClientGraphsModel.timeGraphYRange = value;
        //        this.NotifyPropertyChanged();
        //    }
        //}
        //public Range<double> bodySwingGraphYRange
        //{
        //    get { return this._csstClientGraphsModel.bodySwingGraphYRange; }
        //    set
        //    {
        //        if (this._csstClientGraphsModel.bodySwingGraphYRange == value) return;
        //        this._csstClientGraphsModel.bodySwingGraphYRange = value;
        //        this.NotifyPropertyChanged();
        //    }
        //}
        //public Range<double> armSwingGraphYRange
        //{
        //    get { return this._csstClientGraphsModel.armSwingGraphYRange; }
        //    set
        //    {
        //        if (this._csstClientGraphsModel.armSwingGraphYRange == value) return;
        //        this._csstClientGraphsModel.armSwingGraphYRange = value;
        //        this.NotifyPropertyChanged();
        //    }
        //}
        public ObservableCollection<ColumnItem>[] timePoints
        {
            get { return this._csstClientGraphsModel.timePoints; }
            set
            {
                this._csstClientGraphsModel.timePoints = value;
                this.NotifyPropertyChanged();
            }
        }
        public ObservableCollection<DataPoint>[] bodySwingPoints {
            get { return this._csstClientGraphsModel.bodySwingPoints; }
            set
            {
                this._csstClientGraphsModel.bodySwingPoints = value;
                this.NotifyPropertyChanged();
            } }
        public ObservableCollection<DataPoint>[] armSwingPoints
        {
            get { return this._csstClientGraphsModel.armSwingPoints; }
            set
            {
                this._csstClientGraphsModel.armSwingPoints = value;
                this.NotifyPropertyChanged();
            }
        }
        public Range timePlotYRange
        {
            get { return this._csstClientGraphsModel.timePlotYRange; }
            set
            {
                this._csstClientGraphsModel.timePlotYRange = value;
                this.NotifyPropertyChanged();
            }
        }

        public Range bodySwingPlotYRange
        {
            get { return this._csstClientGraphsModel.bodySwingPlotYRange; }
            set
            {
                this._csstClientGraphsModel.bodySwingPlotYRange = value;
                this.NotifyPropertyChanged();
            }
        }
        public Range armSwingPlotYRange {
            get { return this._csstClientGraphsModel.armSwingPlotYRange; }
            set
            {
                this._csstClientGraphsModel.armSwingPlotYRange = value;
                this.NotifyPropertyChanged();
            } }

        public CSSTClientGraphsViewModel()
        {
            AppDomain.CurrentDomain.AssemblyResolve += this.CSSTAssemblyResolveHandler;

            this._csstClientGraphsModel = new CSSTClientGraphsModel();
            this._csstDataAnalysisResultReadyEventST = CSSTEventHelpers.ReceiveCSSTDataAnalysisResultReadyEvent(this.CSSTDataAnalysisResultReady, arg => arg.receivers.Contains(this._guid), ThreadOption.UIThread);
        }
        ~CSSTClientGraphsViewModel()
        {
            CSSTEventHelpers.DisposeCSSTDataAnalysisResultReadyEvent(this._csstDataAnalysisResultReadyEventST);
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
                strTempAssmbPath = Environment.CurrentDirectory + "\\ClientUserModules\\";
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

        #region Event Handlers

        private void CSSTDataAnalysisResultReady(TRFEventArg e)
        {
            CSSTAnalysisResult result = (CSSTAnalysisResult)e.payload;
            int i;
            this.timePoints[0].Clear();
            this.timePoints[1].Clear();
            for (i = 0; i < result.upTime.Count; i++)
            {
                this.timePoints[0].Add(new ColumnItem(result.upTime[i], i));
            }
            
            for (i = 0; i < result.downTime.Count; i++)
            {
                this.timePoints[1].Add(new ColumnItem(result.downTime[i], i));
            }


            //Range<double> range1 = new Range<double>();
            //Range<double> range2 = new Range<double>();
            //if (result.upTime.Count != 0)
            //    range1 = new Range<double>(result.upTime.Min(), result.upTime.Max());
            //if (result.downTime.Count != 0)
            //    range2 = new Range<double>(result.downTime.Min(), result.downTime.Max());
            this.timePlotYRange = new Range(Math.Round(Math.Min(result.upTime.Min(), result.downTime.Min()) * 0.9, 3), Math.Round(Math.Max(result.upTime.Max(), result.downTime.Max()) * 1.1, 3));

            foreach (ObservableCollection<DataPoint> dataset in this.bodySwingPoints)
            {
                dataset.Clear();
            }
            int cycle = 1;
            for (i = 0; i < result.bodySwingAmplitude.Count; i++)
            {
                double x = 0;
                if (cycle == 1)
                {
                    if (i * 0.03 <= result.upTime[cycle - 1] + result.downTime[cycle - 1])
                    {
                        x = i * 0.03 / (result.upTime[cycle - 1] + result.downTime[cycle - 1]);
                    }
                }
                else
                {
                    double m = i * 0.03;
                    for (int j = 0; j < cycle - 1; j++)
                    {
                        m = m - result.upTime[j] - result.downTime[j];
                    }

                    if (cycle - 1 >= result.downTime.Count)
                    {
                        if (m <= result.upTime[cycle - 1] || Math.Abs(m - result.upTime[cycle - 1]) < 0.000000001)
                        {
                            x = m / (result.upTime[cycle - 1]) + cycle - 1;
                        }
                    }
                    else
                    {
                        if (m <= result.upTime[cycle - 1] + result.downTime[cycle - 1] || Math.Abs(m - result.upTime[cycle - 1] - result.downTime[cycle - 1]) < 0.000000001)
                        {
                            x = m / (result.upTime[cycle - 1] + result.downTime[cycle - 1]) + cycle - 1;
                        }
                    }
                }

                this.bodySwingPoints[0].Add(new DataPoint(x, result.bodySwingAmplitude[i]));
                this.bodySwingPoints[1].Add(new DataPoint(x, result.healthyBodySwingBoundary.Item1));
                this.bodySwingPoints[2].Add(new DataPoint(x, result.healthyBodySwingBoundary.Item2));
                if (Math.Abs(x - (double)cycle) < 0.000000001)
                {
                    cycle++;
                    if (cycle - 1 >= result.upTime.Count)
                        break;
                }
            }
            double low = Math.Min(result.healthyBodySwingBoundary.Item1, result.bodySwingLimit.Item1) > 0
                ? Math.Min(result.healthyBodySwingBoundary.Item1, result.bodySwingLimit.Item1)*0.9
                : Math.Min(result.healthyBodySwingBoundary.Item1, result.bodySwingLimit.Item1)*1.1;
            double high = Math.Max(result.healthyBodySwingBoundary.Item2, result.bodySwingLimit.Item2) > 0
                ? Math.Max(result.healthyBodySwingBoundary.Item2, result.bodySwingLimit.Item2) * 1.1
                : Math.Max(result.healthyBodySwingBoundary.Item2, result.bodySwingLimit.Item2) * 0.9;
            this.bodySwingPlotYRange = new Range(Math.Round(low, 3), Math.Round(high, 3));

            foreach (ObservableCollection<DataPoint> dataset in this.armSwingPoints)
            {
                dataset.Clear();
            }
            cycle = 1;
            for (i = 0; i < result.leftArmSwingAmplitude.Count; i++)
            {
                double x = 0;
                if (cycle == 1)
                {
                    if (i * 0.03 <= result.upTime[cycle - 1] + result.downTime[cycle - 1])
                    {
                        x = i * 0.03 / (result.upTime[cycle - 1] + result.downTime[cycle - 1]);
                    }
                }
                else
                {
                    double m = i * 0.03;
                    for (int j = 0; j < cycle - 1; j++)
                    {
                        m = m - result.upTime[j] - result.downTime[j];
                    }

                    if (cycle - 1 >= result.downTime.Count)
                    {
                        if (m <= result.upTime[cycle - 1] || Math.Abs(m - result.upTime[cycle - 1]) < 0.000000001)
                        {
                            x = m / (result.upTime[cycle - 1]) + cycle - 1;
                        }
                    }
                    else
                    {
                        if (m <= result.upTime[cycle - 1] + result.downTime[cycle - 1] || Math.Abs(m - result.upTime[cycle - 1] - result.downTime[cycle - 1]) < 0.000000001)
                        {
                            x = m / (result.upTime[cycle - 1] + result.downTime[cycle - 1]) + cycle - 1;
                        }
                    }

                }

                this.armSwingPoints[0].Add(new DataPoint(x, result.leftArmSwingAmplitude[i]));
                this.armSwingPoints[2].Add(new DataPoint(x, result.healthyArmSwingBoundary.Item1));
                this.armSwingPoints[3].Add(new DataPoint(x, result.healthyArmSwingBoundary.Item2));
                if (Math.Abs(x - (double)cycle) < 0.000000001)
                {
                    cycle++;
                    if (cycle - 1 >= result.upTime.Count)
                        break; ;
                }
            }
            cycle = 1;
            for (i = 0; i < result.rightArmSwingAmplitude.Count; i++)
            {
                double x = 0;
                if (cycle == 1)
                {
                    if (i * 0.03 <= result.upTime[cycle - 1] + result.downTime[cycle - 1])
                    {
                        x = i * 0.03 / (result.upTime[cycle - 1] + result.downTime[cycle - 1]);
                    }
                }
                else
                {
                    double m = i * 0.03;
                    for (int j = 0; j < cycle - 1; j++)
                    {
                        m = m - result.upTime[j] - result.downTime[j];
                    }

                    if (cycle - 1 >= result.downTime.Count)
                    {
                        if (m <= result.upTime[cycle - 1] || Math.Abs(m - result.upTime[cycle - 1]) < 0.000000001)
                        {
                            x = m / (result.upTime[cycle - 1]) + cycle - 1;
                        }
                    }
                    else
                    {
                        if (m <= result.upTime[cycle - 1] + result.downTime[cycle - 1] || Math.Abs(m - result.upTime[cycle - 1] - result.downTime[cycle - 1]) < 0.000000001)
                        {
                            x = m / (result.upTime[cycle - 1] + result.downTime[cycle - 1]) + cycle - 1;
                        }
                    }
                }
                this.armSwingPoints[1].Add(new DataPoint(x, result.rightArmSwingAmplitude[i]));
                if (Math.Abs(x - (double)cycle) < 0.000000001)
                {
                    cycle++;
                    if (cycle - 1 >= result.upTime.Count)
                        break; ;
                }
            }
            low = Math.Min(result.healthyArmSwingBoundary.Item1, result.armLimit.Item1) > 0
                ? Math.Min(result.healthyArmSwingBoundary.Item1, result.armLimit.Item1) * 0.9
                : Math.Min(result.healthyArmSwingBoundary.Item1, result.armLimit.Item1) * 1.1;
            high = Math.Max(result.healthyArmSwingBoundary.Item2, result.armLimit.Item2) > 0
                ? Math.Max(result.healthyArmSwingBoundary.Item2, result.armLimit.Item2) * 1.1
                : Math.Max(result.healthyArmSwingBoundary.Item2, result.armLimit.Item2) * 0.9;
            this.armSwingPlotYRange = new Range(Math.Round(low, 3), Math.Round(high, 3));

        }
        #endregion
    }
}
