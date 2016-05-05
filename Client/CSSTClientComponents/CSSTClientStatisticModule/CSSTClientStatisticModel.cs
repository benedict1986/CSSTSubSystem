namespace CSSTClientStatisticModule
{
    public class CSSTClientStatisticModel
    {
        public string cycleNumber { get; set; }
        public string upTimeMeanStd { get; set; }
        public string downTimeMeanStd { get; set; }
        public string bodySwingMeanStd { get; set; }
        public string armSwingMeanStd { get; set; }
        public string severityLevelLabel { get; set; }
        public double severityLevelProgressBarValue { get; set; }
        public string healthyCycleNumber { get; set; }
        public string healthyUpTime { get; set; }
        public string healthyDownTime { get; set; }
        public string healthyBodySwing { get; set; }
        public string healthyArmSwing { get; set; }
    }
}
