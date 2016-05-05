using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CSSTCommonLibs;
using Prism.Commands;
using Prism.Events;
using TRFCommonLibs;
using YesNo = TRFCommonLibs.TRFGlobalVariables.YesNo;
using UserGender = TRFCommonLibs.TRFGlobalVariables.TRFUser.Gender;
using UserType = TRFCommonLibs.TRFGlobalVariables.TRFUser.Type;
using UserStatus = TRFCommonLibs.TRFGlobalVariables.TRFUser.Status;
using CSSTStages = CSSTCommonLibs.CSSTGlobalVariables.CSSTStages;
using TRFUserOperation = TRFCommonLibs.TRFGlobalVariables.TRFUser.Operation;
namespace CSSTClientControlPanelModule
{
    [Guid("7211052A-30E3-437E-8205-2839524F674C")]
    public class CSSTClientControlPanelViewModel : TRFNotifyPropertyChanged
    {
        #region Controls Properties
        public ObservableCollection<TRFKeyValuePair<sbyte, string>> activityModeItemsSource
        {
            get { return this._csstClientControlPanelModel.activityModeItemsSource; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.activityModeItemsSource == value)
                    this._csstClientControlPanelModel.activityModeItemsSource = value;
                this.NotifyPropertyChanged();
            }
        }
        public TRFKeyValuePair<sbyte, string> activityModeSelectedItem
        {
            get
            {
                return this._csstClientControlPanelModel.activityModeSelectedItem;
            }
            set
            {
                if (value.Equals(null) || this._csstClientControlPanelModel.activityModeSelectedItem.Equals(value)) return;
                this._csstClientControlPanelModel.activityModeSelectedItem = value;
                this.NotifyPropertyChanged();

                this.isSubjectSessionEnabled = value.key != (sbyte)CSSTGlobalVariables.CSSTActivityMode.Record;
            }
        }
        public ObservableCollection<TRFKeyValuePair<int, string>> subjectNameItemsSource
        {
            get { return this._csstClientControlPanelModel.subjectNameItemsSource; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.subjectNameItemsSource == value)
                    this._csstClientControlPanelModel.subjectNameItemsSource = value;
                this.NotifyPropertyChanged();
            }
        }
        public TRFKeyValuePair<int, string> subjectNameSelectedItem
        {
            get { return this._csstClientControlPanelModel.subjectNameSelectedItem; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.subjectNameSelectedItem == value) return;
                this._csstClientControlPanelModel.subjectNameSelectedItem = value;
                this._csstClientControlPanelModel.subjectNameText = value.value;
                this.NotifyPropertyChanged();
                this.isSubjectNameEditable = value.key == 0 && value.value == "";
                if (value.key != 0)
                {
                    TRFEventHelpers.SendTRFUserRequestedEvent(this._guid, value.key);
                }
                else
                {
                    this._selectedTRFUser = null;
                    this.CleanControls();
                }
            }
        }
        public ObservableCollection<TRFKeyValuePair<int, string>> subjectSessionItemsSource
        {
            get { return this._csstClientControlPanelModel.subjectSessionItemsSource; }
            set
            {
                this._csstClientControlPanelModel.subjectSessionItemsSource = value;
                this.NotifyPropertyChanged();
            }
        }
        public TRFKeyValuePair<int, string> subjectSessionSelectedItem
        {
            get { return this._csstClientControlPanelModel.subjectSessionSelectedItem; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.subjectSessionSelectedItem == value) return;
                this._csstClientControlPanelModel.subjectSessionSelectedItem = value;
                this.NotifyPropertyChanged();
            }
        }
        public string subjectNameText
        {
            get { return this._csstClientControlPanelModel.subjectNameText; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.subjectNameText == value) return;
                this._csstClientControlPanelModel.subjectNameText = value;
                this.NotifyPropertyChanged();
            }
        }
        public string subjectAge
        {
            get { return this._csstClientControlPanelModel.subjectAge; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.subjectAge == value) return;
                this._csstClientControlPanelModel.subjectAge = value;
                this.NotifyPropertyChanged();
            }
        }
        public string subjectHeight
        {
            get { return this._csstClientControlPanelModel.subjectHeight; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.subjectHeight == value) return;
                this._csstClientControlPanelModel.subjectHeight = value;
                this.NotifyPropertyChanged();
            }
        }
        public string subjectWeight
        {
            get { return this._csstClientControlPanelModel.subjectWeight; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.subjectWeight == value) return;
                this._csstClientControlPanelModel.subjectWeight = value;
                this.NotifyPropertyChanged();
            }
        }
        public ObservableCollection<TRFKeyValuePair<sbyte, string>> subjectGenderItemsSource
        {
            get { return this._csstClientControlPanelModel.subjectGenderItemsSource; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.subjectGenderItemsSource == value)
                    this._csstClientControlPanelModel.subjectGenderItemsSource = value;
                this.NotifyPropertyChanged();
            }
        }
        public TRFKeyValuePair<sbyte, string> subjectGenderSelectedItem
        {
            get { return this._csstClientControlPanelModel.subjectGenderSelectedItem; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.subjectGenderSelectedItem == value) return;
                this._csstClientControlPanelModel.subjectGenderSelectedItem = value;
                this.NotifyPropertyChanged();
            }
        }
        public ObservableCollection<TRFKeyValuePair<sbyte, string>> mdHistoryItemsSource
        {
            get { return this._csstClientControlPanelModel.mdHistoryItemsSource; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.mdHistoryItemsSource == value) return;
                this._csstClientControlPanelModel.mdHistoryItemsSource = value;
                this.NotifyPropertyChanged();
            }
        }
        public TRFKeyValuePair<sbyte, string> mdHistorySelectedItem
        {
            get { return this._csstClientControlPanelModel.mdHistorySelectedItem; }
            set
            {
                if (value.Equals(null) || this._csstClientControlPanelModel.mdHistorySelectedItem.Equals(value)) return;
                this._csstClientControlPanelModel.mdHistorySelectedItem = value;
                this.NotifyPropertyChanged();


                if (value.key == (sbyte)TRFGlobalVariables.YesNo.Yes)
                {
                    this.isMdHistoryDetailEnabled = true;
                }
                else if (value.key == (sbyte)TRFGlobalVariables.YesNo.No)
                {
                    this.mdHistoryDetail = "";
                    this.isMdHistoryDetailEnabled = false;
                }

            }
        }
        public string mdHistoryDetail
        {
            get { return this._csstClientControlPanelModel.mdHistoryDetail; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.mdHistoryDetail == value) return;
                this._csstClientControlPanelModel.mdHistoryDetail = value;
                this.NotifyPropertyChanged();
            }
        }
        public string saveDataPath
        {
            get { return this._csstClientControlPanelModel.saveDataPath; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.saveDataPath == value) return;
                this._csstClientControlPanelModel.saveDataPath = value;
                this.NotifyPropertyChanged();
            }
        }
        public DelegateCommand<object> startButtonCommand { get; private set; }
        public DelegateCommand<object> stopButtonCommand { get; private set; }
        public DelegateCommand<object> selectSaveDataPathCommand { get; private set; }
        public DelegateCommand<object> createSubjectButtonCommand { get; private set; }
        public bool isActivityModeEnabled
        {
            get { return this._csstClientControlPanelModel.isActivityModeEnabled; }
            set
            {
                this._csstClientControlPanelModel.isActivityModeEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool isSubjectNameEnabled
        {
            get { return this._csstClientControlPanelModel.isSubjectNameEnabled; }
            set
            {
                this._csstClientControlPanelModel.isSubjectNameEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool isSubjectSessionEnabled
        {
            get { return this._csstClientControlPanelModel.isSubjectSessionEnabled; }
            set
            {
                this._csstClientControlPanelModel.isSubjectSessionEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool isMdHistoryEnabled
        {
            get { return this._csstClientControlPanelModel.isMdHistoryEnabled; }
            set
            {
                this._csstClientControlPanelModel.isMdHistoryEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool isSubjectAgeEnabled
        {
            get { return this._csstClientControlPanelModel.isSubjectAgeEnabled; }
            set
            {
                this._csstClientControlPanelModel.isSubjectAgeEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool isSubjectGenderEnabled
        {
            get { return this._csstClientControlPanelModel.isSubjectGenderEnabled; }
            set
            {
                this._csstClientControlPanelModel.isSubjectGenderEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool isSubjectHeightEnabled
        {
            get { return this._csstClientControlPanelModel.isSubjectHeightEnabled; }
            set
            {
                this._csstClientControlPanelModel.isSubjectHeightEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool isSubjectWeightEnabled
        {
            get { return this._csstClientControlPanelModel.isSubjectWeightEnabled; }
            set
            {
                this._csstClientControlPanelModel.isSubjectWeightEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool isMdHistoryDetailEnabled
        {
            get { return this._csstClientControlPanelModel.isMdHistoryDetailEnabled; }
            set
            {
                this._csstClientControlPanelModel.isMdHistoryDetailEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool isSaveDataPathEnabled
        {
            get { return this._csstClientControlPanelModel.isSaveDataPathEnabled; }
            set
            {
                this._csstClientControlPanelModel.isSaveDataPathEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool isSelectSaveDataPathEnabled
        {
            get { return this._csstClientControlPanelModel.isSelectSaveDataPathEnabled; }
            set
            {
                this._csstClientControlPanelModel.isSelectSaveDataPathEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool isStartButtonEnabled
        {
            get { return this._csstClientControlPanelModel.isStartButtonEnabled; }
            set
            {
                this._csstClientControlPanelModel.isStartButtonEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool isStopButtonEnabled
        {
            get { return this._csstClientControlPanelModel.isStopButtonEnabled; }
            set
            {
                this._csstClientControlPanelModel.isStopButtonEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool isSubjectNameEditable
        {
            get { return this._csstClientControlPanelModel.isSubjectNameEditable; }
            set
            {
                if (this._csstClientControlPanelModel.isSubjectNameEditable == value) return;
                this._csstClientControlPanelModel.isSubjectNameEditable = value;
                this.NotifyPropertyChanged();
            }
        }
        #endregion

        private readonly Guid _guid = typeof(CSSTClientControlPanelViewModel).GUID;
        private TRFUser _selectedTRFUser
        {
            get { return this._csstClientControlPanelModel.selectedTRFUser; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.selectedTRFUser == value) return;
                this._csstClientControlPanelModel.selectedTRFUser = value;
            }
        }

        private CSSTSession _csstSession
        {
            get { return this._csstClientControlPanelModel.csstSession; }
            set
            {
                if (value == null || this._csstClientControlPanelModel.csstSession == value) return;
                this._csstClientControlPanelModel.csstSession = value;
            }
        }
        private sbyte _kinectDetectedBodyNumber
        {
            get { return this._csstClientControlPanelModel.kinectDetectedBodyNumber; }
            set
            {
                if (this._csstClientControlPanelModel.kinectDetectedBodyNumber != value)
                    this._csstClientControlPanelModel.kinectDetectedBodyNumber = value;
            }
        }
        private readonly CSSTClientControlPanelModel _csstClientControlPanelModel;
        private readonly SubscriptionToken _trfUserListReadyEventST;
        private readonly SubscriptionToken _trfUserReadyEventST;
        private readonly SubscriptionToken _csstDataSavedEventST;
        private readonly SubscriptionToken _csstStageChangedEventST;
        private readonly SubscriptionToken _trfKinectBodyNumberChangedEventST;
        public CSSTClientControlPanelViewModel()
        {
            this._csstClientControlPanelModel = new CSSTClientControlPanelModel();

            this._trfUserListReadyEventST = TRFEventHelpers.ReceiveTRFUserListReadyEvent(this.TRFUserListReady, null, ThreadOption.UIThread);
            this._trfUserReadyEventST = TRFEventHelpers.ReceiveTRFUserReadyEvent(this.TRFUserReady, null, ThreadOption.UIThread);
            this._csstDataSavedEventST = CSSTEventHelpers.ReceiveCSSTDataSavedEvent(this.CSSTDataSaved, null, ThreadOption.UIThread);
            this._csstStageChangedEventST = CSSTEventHelpers.ReceiveCSSTStageChangedEvent(this.CSSTStageChanged, null, ThreadOption.UIThread);
            this.selectSaveDataPathCommand = new DelegateCommand<object>(this.RunSelectSaveDataPathCommand);
            this._trfKinectBodyNumberChangedEventST = TRFEventHelpers.ReceiveTRFKinectBodyNumberChangedEvent(this.TRFKinectBodyNumberChanged);

            this.startButtonCommand = new DelegateCommand<object>(this.RunStartButtonCommand);
            this.stopButtonCommand = new DelegateCommand<object>(this.RunStopButtonCommand);
        }

        

        ~CSSTClientControlPanelViewModel()
        {
            TRFEventHelpers.DisposeTRFUserListReadyEvent(this._trfUserListReadyEventST);
            TRFEventHelpers.DisposeTRFUserReadyEvent(this._trfUserReadyEventST);
            CSSTEventHelpers.DisposeCSSTDataSavedEvent(this._csstDataSavedEventST);
            CSSTEventHelpers.DisposeCSSTStageChangedEvent(this._csstStageChangedEventST);
        }

        #region Events Handlers
        private void TRFUserListReady(TRFEventArg e)
        {
            if (e.payload == null) return;
            List<TRFUserListItem> userList = (List<TRFUserListItem>) e.payload;
            this.subjectNameItemsSource.Clear();
            this.subjectNameItemsSource.Add(new TRFKeyValuePair<int, string>(0, ""));
            foreach (TRFUserListItem item in userList)
            {
                this.subjectNameItemsSource.Add(new TRFKeyValuePair<int, string>(item.id, item.name));
            }
        }

        private void TRFUserReady(TRFEventArg e)
        {
            TRFUser user = (TRFUser) e.payload;
            this._selectedTRFUser = user;

            if (string.IsNullOrWhiteSpace(user.movementDisorderHistory))
            {
                this.mdHistorySelectedItem = new TRFKeyValuePair<sbyte, string>((sbyte) YesNo.No, YesNo.No.ToString());
            }
            else
            {
                this.mdHistorySelectedItem = new TRFKeyValuePair<sbyte, string>((sbyte)YesNo.Yes, YesNo.Yes.ToString());
                this.mdHistoryDetail = user.movementDisorderHistory;
            }

            this.subjectHeight = user.height.ToString(CultureInfo.InvariantCulture);
            this.subjectAge = user.age.ToString();
            this.subjectWeight = user.weight.ToString(CultureInfo.InvariantCulture);
            this.saveDataPath = user.saveDataPath;
        }

        private void CSSTStageChanged(TRFEventArg e)
        {
            TRFKeyValuePair<CSSTStages, CSSTStages> stage = (TRFKeyValuePair<CSSTStages, CSSTStages>) e.payload;
            if (stage.value == CSSTStages.CSSTFinished)
            {
                this.isActivityModeEnabled = true;
                this.isSubjectNameEnabled = true;
                this.isSubjectSessionEnabled = true;
                this.isMdHistoryEnabled = true;
                this.isSubjectAgeEnabled = true;
                this.isSubjectGenderEnabled = true;
                this.isSubjectHeightEnabled = true;
                this.isSubjectWeightEnabled = true;
                this.isMdHistoryDetailEnabled = true;
                this.isSaveDataPathEnabled = true;
                this.isSelectSaveDataPathEnabled = true;
                this.isStartButtonEnabled = true;
                this.isStopButtonEnabled = false;

                this.subjectNameSelectedItem = this.subjectNameItemsSource[0];
                this.CleanControls();
                this._selectedTRFUser = null;
                this._csstSession = null;
                this._kinectDetectedBodyNumber = 0;
            }
        }

        private void CSSTDataSaved(TRFEventArg e)
        {
            //Console.Write("hjello");
        }

        private void TRFKinectBodyNumberChanged(TRFEventArg e)
        {
            this._kinectDetectedBodyNumber = (sbyte) e.payload;
        }
        #endregion

        #region Buttons
        #region Select Save Data Path Command
        private void RunSelectSaveDataPathCommand(object obj)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            this.saveDataPath = dialog.SelectedPath + Path.DirectorySeparatorChar;
        }
        #endregion

        #region Start Button Command
        private void RunStartButtonCommand(object obj)
        {
            if (this.StartAssessment())
            {
                this.isActivityModeEnabled = false;
                this.isSubjectNameEnabled = false;
                this.isSubjectSessionEnabled = false;
                this.isMdHistoryEnabled = false;
                this.isSubjectAgeEnabled = false;
                this.isSubjectGenderEnabled = false;
                this.isSubjectHeightEnabled = false;
                this.isSubjectWeightEnabled = false;
                this.isMdHistoryDetailEnabled = false;
                this.isSaveDataPathEnabled = false;
                this.isSelectSaveDataPathEnabled = false;
                this.isStartButtonEnabled = false;
                this.isStopButtonEnabled = true;
            }
        }
        #endregion

        #region Stop Button Command
        private void RunStopButtonCommand(object obj)
        {
            CSSTEventHelpers.SendCSSTTerminationRequestedEvent(this._guid, true);
        }
        #endregion
        #endregion

        private void CleanControls()
        {
            this.mdHistorySelectedItem = this.mdHistoryItemsSource[0];
            this.subjectAge = "";
            this.subjectGenderSelectedItem = this.subjectGenderItemsSource[0];
            this.subjectHeight = "";
            this.subjectWeight = "";
            this.mdHistoryDetail = "";
            this.saveDataPath = "";
        }

        private bool StartAssessment()
        {
            if (this._kinectDetectedBodyNumber == 0)
            {
                TRFEventHelpers.SendTRFMessageReadyEvent(this._guid, new TRFMessage("No subject is detected."));
                return false;
            }
            else if (this._kinectDetectedBodyNumber > 1)
            {
                TRFEventHelpers.SendTRFMessageReadyEvent(this._guid, new TRFMessage("More than one subject is detected."));
                return false;
            }

            #region Velidation
            if (this.activityModeSelectedItem.Equals(null) || string.IsNullOrWhiteSpace(this.activityModeSelectedItem.value))
            {
                //GlobalFunctions.SendMyMessage(EventSender.HASAssessmentsCSSTControlPanelViewModel, new MyMessage("Activity model is not selected."), new List<EventReceiver> { EventReceiver.StatusBarViewModel });
                return false;
            }
            if (this.subjectNameSelectedItem.Equals(null))
            {
                //GlobalFunctions.SendMyMessage(EventSender.HASAssessmentsCSSTControlPanelViewModel, new MyMessage("Subject's name is not selected."), new List<EventReceiver> { EventReceiver.StatusBarViewModel });
                return false;
            }
            else if (string.IsNullOrWhiteSpace(this.subjectNameText))
            {
                //GlobalFunctions.SendMyMessage(EventSender.HASAssessmentsCSSTControlPanelViewModel, new MyMessage("Either select a subject or give the subject a name."), new List<EventReceiver> { EventReceiver.StatusBarViewModel });
                return false;
            }
            if (this.activityModeSelectedItem.key == (sbyte)CSSTGlobalVariables.CSSTActivityMode.Review &&
                (this.subjectSessionSelectedItem.Equals(null) || string.IsNullOrWhiteSpace(this.subjectSessionSelectedItem.value)))
            {
                //GlobalFunctions.SendMyMessage(EventSender.HASAssessmentsCSSTControlPanelViewModel, new MyMessage("Session is not selected."), new List<EventReceiver> { EventReceiver.StatusBarViewModel });
                return false;
            }
            if (this.mdHistorySelectedItem.Equals(null) || string.IsNullOrWhiteSpace(this.mdHistorySelectedItem.value))
            {
                //GlobalFunctions.SendMyMessage(EventSender.HASAssessmentsCSSTControlPanelViewModel, new MyMessage("Subject's movement disorder history is not selected."), new List<EventReceiver> { EventReceiver.StatusBarViewModel });
                return false;
            }
            int age;
            if (!int.TryParse(this.subjectAge, out age))
            {
                //GlobalFunctions.SendMyMessage(EventSender.HASAssessmentsCSSTControlPanelViewModel, new MyMessage("Subject's age is not valid."), new List<EventReceiver> { EventReceiver.StatusBarViewModel });
                return false;
            }
            if (this.subjectGenderSelectedItem.Equals(null) || string.IsNullOrWhiteSpace(this.subjectGenderSelectedItem.value))
            {
                //GlobalFunctions.SendMyMessage(EventSender.HASAssessmentsCSSTControlPanelViewModel, new MyMessage("Subject's gender is not selected."), new List<EventReceiver> { EventReceiver.StatusBarViewModel });
                return false;
            }
            double height;
            if (!double.TryParse(this.subjectHeight, out height))
            {
                //GlobalFunctions.SendMyMessage(EventSender.HASAssessmentsCSSTControlPanelViewModel, new MyMessage("Subject's height is not valid."), new List<EventReceiver> { EventReceiver.StatusBarViewModel });
                return false;
            }
            double weight;
            if (!double.TryParse(this.subjectWeight, out weight))
            {
                //GlobalFunctions.SendMyMessage(EventSender.HASAssessmentsCSSTControlPanelViewModel, new MyMessage("Subject's weight is not valid."), new List<EventReceiver> { EventReceiver.StatusBarViewModel });
                return false;
            }
            if (this.mdHistorySelectedItem.key == (sbyte)TRFGlobalVariables.YesNo.Yes && string.IsNullOrWhiteSpace(this.mdHistoryDetail))
            {
                //GlobalFunctions.SendMyMessage(EventSender.HASAssessmentsCSSTControlPanelViewModel, new MyMessage("Subject's movement disorder history detail is not valid."), new List<EventReceiver> { EventReceiver.StatusBarViewModel });
                return false;
            }
            #endregion

            TRFUser user = new TRFUser()
            {
                id = this.subjectNameSelectedItem.key,
                age = sbyte.Parse(this.subjectAge.Trim()),
                name =
                    this.subjectNameSelectedItem.value.Trim() != ""
                        ? this.subjectNameSelectedItem.value.Trim()
                        : this.subjectNameText.Trim(),
                username = this._selectedTRFUser == null?"": this._selectedTRFUser.username,
                password = this._selectedTRFUser == null? "":this._selectedTRFUser.password,
                gender = this.subjectGenderSelectedItem.key,
                height = float.Parse(this.subjectHeight.Trim()),
                weight = float.Parse(this.subjectWeight.Trim()),
                movementDisorderHistory = this.mdHistoryDetail.Trim(),
                registDatetime = this._selectedTRFUser?.registDatetime ?? DateTime.Now,
                userType = (sbyte)UserType.Subject,
                status = (sbyte)UserStatus.ActivatedOffline,
                saveDataPath = string.IsNullOrWhiteSpace(this.saveDataPath) ? "" : this.saveDataPath.Trim()
            };

            this._csstSession = new CSSTSession()
            {
                id = 0,
                activityMode = (sbyte)this.activityModeSelectedItem.key,
                dataTableName = "CSST_" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss_ffff"),
                endDateTime = DateTime.Now,
                startDateTime = DateTime.Now,
                trfUserId = user.id,
                trfUser = user
            };

            if (user.id != 0)
            {
                if (!TRFUser.TRFUserValueEqual(user, this._selectedTRFUser))
                {
                    this._csstSession.trfUser.trfUserOperation = TRFUserOperation.Update;
                }
            }
            else
            {
                this._csstSession.trfUser.trfUserOperation = TRFUserOperation.Insert;
            }
            CSSTEventHelpers.SendCSSTSaveSessionRequestedEvent(this._guid, this._csstSession);
            return true;
        }
    }
}
