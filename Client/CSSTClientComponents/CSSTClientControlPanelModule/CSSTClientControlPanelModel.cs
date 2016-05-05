using System;
using System.Collections.ObjectModel;
using CSSTCommonLibs;
using TRFCommonLibs;
using TRFUserGender = TRFCommonLibs.TRFGlobalVariables.TRFUser.Gender;

namespace CSSTClientControlPanelModule
{
    public class CSSTClientControlPanelModel
    {
        #region Properties
        public ObservableCollection<TRFKeyValuePair<sbyte, string>> activityModeItemsSource { get; set; }
        public TRFKeyValuePair<sbyte, string> activityModeSelectedItem { get; set; }
        public ObservableCollection<TRFKeyValuePair<int, string>> subjectNameItemsSource { get; set; }
        public TRFKeyValuePair<int, string> subjectNameSelectedItem { get; set; }
        public ObservableCollection<TRFKeyValuePair<int, string>> subjectSessionItemsSource { get; set; }
        public TRFKeyValuePair<int, string> subjectSessionSelectedItem { get; set; }
        public string subjectNameText { get; set; }
        public string subjectAge { get; set; }
        public string subjectHeight { get; set; }
        public string subjectWeight { get; set; }
        public ObservableCollection<TRFKeyValuePair<sbyte, string>> subjectGenderItemsSource { get; set; }
        public TRFKeyValuePair<sbyte, string> subjectGenderSelectedItem { get; set; }
        public ObservableCollection<TRFKeyValuePair<sbyte, string>> mdHistoryItemsSource { get; set; }
        public TRFKeyValuePair<sbyte, string> mdHistorySelectedItem { get; set; }
        public string mdHistoryDetail { get; set; }
        public string saveDataPath { get; set; }
        public bool isActivityModeEnabled { get; set; }
        public bool isSubjectNameEnabled { get; set; }
        public bool isSubjectSessionEnabled { get; set; }
        public bool isMdHistoryEnabled { get; set; }
        public bool isSubjectAgeEnabled { get; set; }
        public bool isSubjectGenderEnabled { get; set; }
        public bool isSubjectHeightEnabled { get; set; }
        public bool isSubjectWeightEnabled { get; set; }
        public bool isMdHistoryDetailEnabled { get; set; }
        public bool isSaveDataPathEnabled { get; set; }
        public bool isSelectSaveDataPathEnabled { get; set; }
        public bool isStartButtonEnabled { get; set; }
        public bool isStopButtonEnabled { get; set; }
        public bool isSubjectNameEditable { get; set; }
        #endregion
        public TRFUser selectedTRFUser { get; set; }
        public CSSTSession csstSession { get; set; }
        public sbyte kinectDetectedBodyNumber { get; set; }
        public CSSTClientControlPanelModel()
        {
            Array csstActivityModes = Enum.GetValues(typeof(CSSTGlobalVariables.CSSTActivityMode));
            this.activityModeItemsSource = new ObservableCollection<TRFKeyValuePair<sbyte, string>>();
            foreach (CSSTGlobalVariables.CSSTActivityMode csstActivityMode in csstActivityModes)
            {
                this.activityModeItemsSource.Add(new TRFKeyValuePair<sbyte, string>((sbyte)csstActivityMode, csstActivityMode.ToString()));
            }
            this.activityModeSelectedItem = this.activityModeItemsSource[0];
            Array userGenders = Enum.GetValues(typeof(TRFUserGender));
            this.subjectGenderItemsSource = new ObservableCollection<TRFKeyValuePair<sbyte, string>>();
            foreach (TRFUserGender gender in userGenders)
            {
                this.subjectGenderItemsSource.Add(new TRFKeyValuePair<sbyte, string>((sbyte)gender, gender.ToString()));
            }
            this.subjectGenderSelectedItem = this.subjectGenderItemsSource[0];
            Array mdHistoryOptions = Enum.GetValues(typeof(TRFGlobalVariables.YesNo));
            this.mdHistoryItemsSource = new ObservableCollection<TRFKeyValuePair<sbyte, string>>();
            foreach (TRFGlobalVariables.YesNo option in mdHistoryOptions)
            {
                this.mdHistoryItemsSource.Add(new TRFKeyValuePair<sbyte, string>((sbyte)option, option.ToString()));
            }
            this.mdHistorySelectedItem = this.mdHistoryItemsSource[0];
            this.subjectNameItemsSource = new ObservableCollection<TRFKeyValuePair<int, string>>() { new TRFKeyValuePair<int, string>(0, "") };
            this.subjectNameSelectedItem = new TRFKeyValuePair<int, string>(0, "");
            this.subjectSessionItemsSource = new ObservableCollection<TRFKeyValuePair<int, string>>() { new TRFKeyValuePair<int, string>(0, "") };
            this.subjectSessionSelectedItem = new TRFKeyValuePair<int, string>(0, "");

            this.isActivityModeEnabled = true;
            this.isSubjectNameEnabled = true;
            this.isSubjectSessionEnabled = false;
            this.isMdHistoryEnabled = true;
            this.isSubjectAgeEnabled = true;
            this.isSubjectGenderEnabled = true;
            this.isSubjectHeightEnabled = true;
            this.isSubjectWeightEnabled = true;
            this.isMdHistoryDetailEnabled = true;
            this.isSaveDataPathEnabled = true;
            this.isSelectSaveDataPathEnabled = true;
            this.isStartButtonEnabled = true;
            this.isStopButtonEnabled = true;
            this.isSubjectNameEditable = true;
        }
    }
}
