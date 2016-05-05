using System;
using System.Collections.Generic;
using TRFCommonLibs;

namespace CSSTCommonLibs
{
    [Serializable]
    public class CSSTSession
    {
        public int id { get; set; }
        public sbyte activityMode { get; set; }
        public string dataTableName { get; set; }
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        public int trfUserId { get; set; }

        public TRFUser trfUser;
    }
    [Serializable]
    public class CSSTAnalysisResult
    {
        public List<double> height { get; set; }
        public int cycleNumber { get; set; }
        public Tuple<double, double> healthyCycleNumberBoundary { get; set; }
        public List<double> upTime { get; set; }
        public double upTimeMean { get; set; }
        public double upTimeStd { get; set; }
        public Tuple<double, double> upTimeLimit { get; set; } 
        public Tuple<double, double> healthyUpTimeBoundary { get; set; }
        public List<double> downTime { get; set; }
        public double downTimeMean { get; set; }
        public double downTimeStd { get; set; }
        public Tuple<double, double> downTimeLimit { get; set; } 
        public Tuple<double, double> healthyDownTimeBoundary { get; set; }
        public List<double> bodySwingAmplitude { get; set; }
        public double bodySwingMean { get; set; }
        public double bodySwingrStd { get; set; }
        public Tuple<double, double> healthyBodySwingBoundary { get; set; }
        public Tuple<double, double> bodySwingLimit { get; set; }
        public List<double> leftArmSwingAmplitude { get; set; }
        public List<double> rightArmSwingAmplitude { get; set; }
        public double armSwingMean { get; set; }
        public double armSwingStd { get; set; }
        public Tuple<double, double> healthyArmSwingBoundary { get; set; }
        public Tuple<double, double> armLimit { get; set; }
    }
}
