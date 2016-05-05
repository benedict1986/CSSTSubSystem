using System.Collections.ObjectModel;
using System.Windows;
using OxyPlot;
using OxyPlot.Series;
using TRFCommonLibs;

namespace CSSTClientGraphsModule
{
    public class CSSTClientGraphsModel
    {
        public ObservableCollection<Point>[] timeGraphData { get; set; }
        public ObservableCollection<Point>[] healthyTimeGraphData { get; set; }
        public ObservableCollection<Point>[] bodySwingGraphData { get; set; }
        public ObservableCollection<Point>[] healthyBodySwingGraphData { get; set; }
        public ObservableCollection<Point>[] armSwingGraphData { get; set; }
        public ObservableCollection<Point>[] healthyArmSwingGraphData { get; set; }
        public ObservableCollection<ColumnItem>[] timePoints { get; set; }
        public ObservableCollection<DataPoint>[] bodySwingPoints { get; set; }
        public ObservableCollection<DataPoint>[] armSwingPoints { get; set; }
        public Range timePlotYRange { get; set; }
        public Range bodySwingPlotYRange { get; set; }
        public Range armSwingPlotYRange { get; set; }
        public CSSTClientGraphsModel()
        {
            this.timeGraphData = new[] { new ObservableCollection<Point>(), new ObservableCollection<Point>() };
            this.healthyTimeGraphData = new[] { new ObservableCollection<Point>(), new ObservableCollection<Point>() };
            this.bodySwingGraphData = new[] { new ObservableCollection<Point>(), new ObservableCollection<Point>(), new ObservableCollection<Point>() };
            this.healthyBodySwingGraphData = new[] { new ObservableCollection<Point>(), new ObservableCollection<Point>(), new ObservableCollection<Point>() };
            this.armSwingGraphData = new[] { new ObservableCollection<Point>(), new ObservableCollection<Point>(), new ObservableCollection<Point>(), new ObservableCollection<Point>() };
            this.healthyArmSwingGraphData = new[] { new ObservableCollection<Point>(), new ObservableCollection<Point>(), new ObservableCollection<Point>() };
            this.bodySwingPoints = new[] { new ObservableCollection<DataPoint>(), new ObservableCollection<DataPoint>(), new ObservableCollection<DataPoint>() };
            this.armSwingPoints = new[] { new ObservableCollection<DataPoint>(), new ObservableCollection<DataPoint>(), new ObservableCollection<DataPoint>(), new ObservableCollection<DataPoint>() };
            this.timePoints = new[] { new ObservableCollection<ColumnItem>(), new ObservableCollection<ColumnItem>() };
            this.timePlotYRange = new Range(0, 2);
            this.bodySwingPlotYRange = new Range(-0.4, 0.4);
            this.armSwingPlotYRange = new Range(-0.4, 0.4);
        }
    }
}
