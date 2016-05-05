using System;
using System.Collections.Generic;
using Microsoft.Kinect;
using TRFUserGender = TRFCommonLibs.TRFGlobalVariables.TRFUser.Gender;
namespace CSSTCommonLibs
{
    public class CSSTGlobalVariables
    {
        public class Event
        {
            public static Dictionary<string, Guid> Sender = new Dictionary<string, Guid>()
            {
                {"CSSTClientShellModule", new Guid("E6F9AE80-D68B-444E-94F1-D88846CC4AA1")},
                {"CSSTClientControlPanelViewModel", new Guid("7211052A-30E3-437E-8205-2839524F674C")},
                {"CSSTServerService", new Guid("D643A2F7-3327-4DAE-8909-4F0CA2187FDC")},
                {"CSSTDatabaseController", new Guid("80E0CAC7-D14B-4A84-983B-6DBDDF87EA6E")},
                {"CSSTServiceCallback", new Guid("210D694B-E981-487B-A72D-80B44ED0A971")},
                {"CSSTDataAnalysis", new Guid("16FA89F5-5BA2-4492-92AE-179F61CBEE03")}
            };

            public static Dictionary<string, Guid> Receiver = new Dictionary<string, Guid>()
            {
                {"CSSTClientControlPanelViewModel", new Guid("7211052A-30E3-437E-8205-2839524F674C")},
                {"CSSTClientService", new Guid("EA8711A2-AB9D-4333-AB37-B5DCB895A5F1")},
                {"CSSTServerService", new Guid("D643A2F7-3327-4DAE-8909-4F0CA2187FDC")},
                {"CSSTDatabaseController", new Guid("80E0CAC7-D14B-4A84-983B-6DBDDF87EA6E")},
                {"CSSTDataAnalysis", new Guid("16FA89F5-5BA2-4492-92AE-179F61CBEE03")},
                {"CSSTClientGraphsViewModel", new Guid("431A4566-D6D0-407F-BBF0-CA2BF7B87E35") },
                {"CSSTClientStatisticViewModel", new Guid("463297CB-3F65-4BEA-9BA7-5F973F00504C") }
            };
        }

        public enum CSSTActivityMode
        {
            Record,
            Review
        }

        public struct CSSTDatabase
        {
            public struct ConnectionParameters
            {
                public const string Ip = "127.0.0.1";
                public const string Port = "3307";
                public const string Username = "root";
                public const string Password = "usbw";
            }

            public struct DatabaseName
            {
                public static string CSSTMain = "CSST_Main";
                public static string TRFMotion3D = "trf_motion_3d";
                public static string TRFMotion2D = "trf_motion_2d";
            }

            public struct DataTableName
            {
                public static string CSSTSession = "CSSTSession";
            }
        }

        public enum CSSTStages
        {
            Login,
            CSSTInitialised,
            CSSTSessionDataReceived,
            CSSTCalibrated,
            CSSTStarted,
            CSSTStopped,
            CSSTAnalysed,
            CSSTFinished,
            Logout
        }

        public struct CSSTAnalysis
        {
            public struct BodyParameters
            {
                public static Dictionary<TRFUserGender, Dictionary<BodySegments, double>> WeightPercentage = new Dictionary
                    <TRFUserGender, Dictionary<BodySegments, double>>()
                {
                    {
                        TRFUserGender.Female, new Dictionary<BodySegments, double>()
                        {
                            {BodySegments.Head, 6.68},
                            {BodySegments.Trunk, 42.57},
                            {BodySegments.UpperArm, 2.55},
                            {BodySegments.LowerArm, 1.38},
                            {BodySegments.Hand, 0.56},
                            {BodySegments.Thigh, 14.78},
                            {BodySegments.Shank, 4.81},
                            {BodySegments.Foot, 1.29}
                        }
                    },
                    {
                        TRFUserGender.Male, new Dictionary<BodySegments, double>()
                        {
                            {BodySegments.Head, 6.94},
                            {BodySegments.Trunk, 43.46},
                            {BodySegments.UpperArm, 2.71},
                            {BodySegments.LowerArm, 1.62},
                            {BodySegments.Hand, 0.61},
                            {BodySegments.Thigh, 14.16},
                            {BodySegments.Shank, 4.33},
                            {BodySegments.Foot, 1.37}
                        }
                    }
                };

                public static Dictionary<TRFUserGender, Dictionary<BodySegments, double>> COMPercentage = new Dictionary
                    <TRFUserGender, Dictionary<BodySegments, double>>()
                {
                    {
                        TRFUserGender.Female, new Dictionary<BodySegments, double>()
                        {
                            {BodySegments.Head, 58.94},
                            {BodySegments.Trunk, 41.51},
                            {BodySegments.UpperArm, 57.54},
                            {BodySegments.LowerArm, 45.59},
                            {BodySegments.Hand, 74.74},
                            {BodySegments.Thigh, 36.12},
                            {BodySegments.Shank, 44.16},
                            {BodySegments.Foot, 40.14}
                        }
                    },
                    {
                        TRFUserGender.Male, new Dictionary<BodySegments, double>()
                        {
                            {BodySegments.Head, 59.76},
                            {BodySegments.Trunk, 44.86},
                            {BodySegments.UpperArm, 57.72},
                            {BodySegments.LowerArm, 45.74},
                            {BodySegments.Hand, 79.00},
                            {BodySegments.Thigh, 40.95},
                            {BodySegments.Shank, 44.59},
                            {BodySegments.Foot, 44.15}
                        }
                    }
                };
            }

            public enum BodySegments
            {
                Head,
                Trunk,
                UpperArm,
                LowerArm,
                Hand,
                Thigh,
                Shank,
                Foot
            }

            public static Dictionary<BodySegments, KeyValuePair<JointType, JointType>> BodyPartsStartEnd = new Dictionary
                <BodySegments, KeyValuePair<JointType, JointType>>()
            {
                {BodySegments.Head, new KeyValuePair<JointType, JointType>(JointType.Head, JointType.Neck)},
                {BodySegments.Trunk, new KeyValuePair<JointType, JointType>(JointType.SpineShoulder, JointType.SpineBase)},
                {BodySegments.UpperArm, new KeyValuePair<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft)},
                {BodySegments.LowerArm, new KeyValuePair<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft)},
                {BodySegments.Hand,new KeyValuePair<JointType, JointType>(JointType.WristLeft, JointType.HandTipLeft)},
                {BodySegments.Thigh, new KeyValuePair<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft)},
                {BodySegments.Shank, new KeyValuePair<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft)},
                {BodySegments.Foot, new KeyValuePair<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft)}
            };
        }

        public struct SeverityLevel
        {
            public const string Superior = "Superior";
            public const string Healthy = "Healthy";
            public const string Unhealthy = "Unhealthy";
        }

        public enum MotionDatPurpose
        {
            Calibration,
            Preparation,
            Assessment
        }
    }
}
