using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Xml.Serialization;
using CSSTCommonLibs;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using Microsoft.Kinect;
using Prism.Events;
using TRFCommonLibs;
using TRFUserGender = TRFCommonLibs.TRFGlobalVariables.TRFUser.Gender;
using BodySegments = CSSTCommonLibs.CSSTGlobalVariables.CSSTAnalysis.BodySegments;
using BodyParameters = CSSTCommonLibs.CSSTGlobalVariables.CSSTAnalysis.BodyParameters;
namespace CSSTServerModule
{
    [Guid("16FA89F5-5BA2-4492-92AE-179F61CBEE03")]
    public class CSSTDataAnalysis
    {
        private readonly Guid _guid = typeof(CSSTDataAnalysis).GUID;
        private readonly SubscriptionToken _csstDataAnalysisRequestedEventST;
        private CSSTAnalysisResult _csstAnalysisResult;

        public CSSTDataAnalysis()
        {
            this._csstAnalysisResult = new CSSTAnalysisResult();
            this._csstDataAnalysisRequestedEventST = CSSTEventHelpers.ReceiveCSSTDataAnalysisRequestedEvent(this.CSSTDataAnalysisRequested, arg => arg.receivers.Contains(this._guid));
        }

        ~CSSTDataAnalysis()
        {
            CSSTEventHelpers.DisposeCSSTDataAnalysisRequestedEvent(this._csstDataAnalysisRequestedEventST);
        }

        #region Event Handlers
        private void CSSTDataAnalysisRequested(TRFEventArg e)
        {
            TRFUser user = (TRFUser)e.payload;
            Matrix<double> body3DMatrix = this.Body3DListToMatrix(user.motion3D);
            this.Assessment(body3DMatrix, (TRFUserGender)Enum.ToObject(typeof(TRFUserGender), user.gender));

            CSSTEventHelpers.SendCSSTDataAnalysisResultReadyEvent(this._guid, this._csstAnalysisResult);
        }
        #endregion

        public void Assessment(Matrix<double> rawData, TRFUserGender gender)
        {
            Matrix<double> segment1 = this.SegmentBody3DMatrixToStages(rawData, 0);
            Matrix<double> spineShoulder = segment1.SubMatrix(30, 30, 6, 3);
            Matrix<double> spineBase = segment1.SubMatrix(30, 30, 0, 3);
            Matrix<double> rotationMatrix = this.EstimateRotationMatrix((spineShoulder - spineBase).ColumnSums() / 30,
                Vector<double>.Build.DenseOfArray(new double[] { 0, 1, 0 }));
            Matrix<double> rotatedBody = this.RotateBodies(rotationMatrix, rawData);
            Matrix<double> rotatedSegment1 = this.SegmentBody3DMatrixToStages(rotatedBody, 0);
            Matrix<double> rotatedSegment2 = this.SegmentBody3DMatrixToStages(rotatedBody, 1);
            Matrix<double> rotatedSegment3 = this.SegmentBody3DMatrixToStages(rotatedBody, 2);

            #region Duration
            //Get the middle part of the data
            double highest = rotatedSegment1.Column(7).SubVector(30, 30).Maximum();
            double lowest = rotatedSegment2.Column(7).SubVector(30, 30).Minimum();

            Matrix<double> fftResult = this.FastFourierTransform(rotatedSegment3.Column(7), 30);
            int cycle = (int)Math.Round(fftResult.Column(1).At(fftResult.Column(0).MaximumIndex()) * 10);

            Tuple<Vector<double>, Vector<double>> standSit = this.EstimateStandSit(rotatedSegment3.Column(7), highest, lowest);
            if (standSit == null)
            {
                this._csstAnalysisResult = null;
                return;
            }
            List<double>[] standSitTime = new List<double>[2];
            standSitTime[0] = new List<double>();
            standSitTime[1] = new List<double>();
            int j = 0;
            for (int i = 0; i < standSit.Item1.Count; i++)
            {
                standSitTime[j].Add(j == 0 ? (standSit.Item1[i] - standSit.Item2[i])*0.03 : (standSit.Item2[i] - standSit.Item1[i]) * 0.03);
                j = j == 0 ? 1 : 0;
            }
            #endregion

            Matrix<double> referenceMatrix = rotatedSegment1.SubMatrix(30, 30, 6, 3) - rotatedSegment1.SubMatrix(30, 30, 0, 3);
            Vector<double> reference = Vector<double>.Build.Dense(3);
            for (int i = 0; i < 30; i++)
            {
                reference += referenceMatrix.Row(i);
            }
            reference /= 30;
            double[] bodySwingAmplitude = this.BodySwingAmplitude(reference, rotatedSegment3, gender);
            List<double>[] armSwingAmplitude = this.ArmSwingAmplitude(reference, rotatedSegment3);

            this._csstAnalysisResult.height = rotatedSegment3.Column(7).ToList();
            this._csstAnalysisResult.cycleNumber = standSitTime[0].Count;
            this._csstAnalysisResult.healthyCycleNumberBoundary = new Tuple<double, double>(10, 15);
            this._csstAnalysisResult.upTime = standSitTime[0];
            this._csstAnalysisResult.downTime = standSitTime[1];
            this._csstAnalysisResult.upTimeMean = standSitTime[0].Mean();
            this._csstAnalysisResult.upTimeStd = standSitTime[0].StandardDeviation();
            this._csstAnalysisResult.upTimeLimit = new Tuple<double, double>(standSitTime[0].Min(), standSitTime[0].Max());
            this._csstAnalysisResult.healthyUpTimeBoundary = new Tuple<double, double>(0.5, 1.2);
            this._csstAnalysisResult.downTimeMean = standSitTime[1].Mean();
            this._csstAnalysisResult.downTimeStd = standSitTime[1].StandardDeviation();
            this._csstAnalysisResult.downTimeLimit = new Tuple<double, double>(standSitTime[1].Min(), standSitTime[1].Max());
            this._csstAnalysisResult.healthyDownTimeBoundary = new Tuple<double, double>(0.5, 1.2);
            this._csstAnalysisResult.bodySwingAmplitude = bodySwingAmplitude.ToList();
            this._csstAnalysisResult.bodySwingMean = bodySwingAmplitude.Mean();
            this._csstAnalysisResult.bodySwingrStd = bodySwingAmplitude.StandardDeviation();
            this._csstAnalysisResult.bodySwingLimit = new Tuple<double, double>(bodySwingAmplitude.Min(), bodySwingAmplitude.Max());
            this._csstAnalysisResult.healthyBodySwingBoundary = new Tuple<double, double>(0, 0.2);
            this._csstAnalysisResult.leftArmSwingAmplitude = armSwingAmplitude[0];
            this._csstAnalysisResult.rightArmSwingAmplitude = armSwingAmplitude[1];
            armSwingAmplitude[0].AddRange(armSwingAmplitude[1]);
            List<double> totalArmSwingAmplitude = armSwingAmplitude[0];
            totalArmSwingAmplitude.AddRange(armSwingAmplitude[1]);
            this._csstAnalysisResult.armSwingMean = totalArmSwingAmplitude.Mean();
            this._csstAnalysisResult.armSwingStd = totalArmSwingAmplitude.StandardDeviation();
            this._csstAnalysisResult.healthyArmSwingBoundary = new Tuple<double, double>(0, 0.2);
            this._csstAnalysisResult.armLimit = new Tuple<double, double>(totalArmSwingAmplitude.Min(), totalArmSwingAmplitude.Max());

        }

        private double[] BodySwingAmplitude(Vector<double> reference, Matrix<double> rotatedSegment3, TRFUserGender gender)
        {
            Matrix<double> com = this.CalculateCenterOfMass(rotatedSegment3, gender);
            Matrix<double> bodyVectors = rotatedSegment3.SubMatrix(0, rotatedSegment3.RowCount, 60, 3) - com;
            double[] distances = new double[bodyVectors.RowCount];
            for (int i = 0; i < bodyVectors.RowCount; i++)
            {
                distances[i] = this.DistanceToAxis(bodyVectors.Row(i), reference);
            }
            return distances;
        }

        private List<double>[] ArmSwingAmplitude(Vector<double> reference, Matrix<double> rotatedSegment3)
        {
            Matrix<double> left = rotatedSegment3.SubMatrix(0, rotatedSegment3.RowCount, 12, 3) - rotatedSegment3.SubMatrix(0, rotatedSegment3.RowCount, 15, 3);
            Matrix<double> right = rotatedSegment3.SubMatrix(0, rotatedSegment3.RowCount, 24, 3) - rotatedSegment3.SubMatrix(0, rotatedSegment3.RowCount, 27, 3);
            List<double>[] distances = new List<double>[2];
            distances[0] = new List<double>();
            distances[1] = new List<double>();
            for (int i = 0; i < rotatedSegment3.RowCount; i++)
            {
                distances[0].Add(this.DistanceToAxis(left.Row(i), reference));
                distances[1].Add(this.DistanceToAxis(right.Row(i), reference));
            }
            return distances;
        }

        private Matrix<double> CalculateCenterOfMass(Matrix<double> body, TRFUserGender gender)
        {
            double w;
            Matrix<double> com = Matrix<double>.Build.Dense(body.RowCount, 3);
            int n = Enum.GetNames(typeof(BodySegments)).Length;
            Matrix<double> head = body.SubMatrix(0, body.RowCount, 12, 3);
            Matrix<double> neck = body.SubMatrix(0, body.RowCount, 9, 3);
            com += BodyParameters.WeightPercentage[gender][BodySegments.Head] * ((2* (head - neck) * (1 - BodyParameters.COMPercentage[gender][BodySegments.Head] / 100)) + neck);
            Matrix<double> spineShoulder = body.SubMatrix(0, body.RowCount, 6, 3);
            Matrix<double> spineBase = body.SubMatrix(0, body.RowCount, 0, 3);
            com += BodyParameters.WeightPercentage[gender][BodySegments.Trunk] * (((spineBase - spineShoulder) * BodyParameters.COMPercentage[gender][BodySegments.Trunk] / 100) + spineShoulder);

            Matrix<double> leftShoulder = body.SubMatrix(0, body.RowCount, 15, 3);
            Matrix<double> leftElbow = body.SubMatrix(0, body.RowCount, 18, 3);
            com += BodyParameters.WeightPercentage[gender][BodySegments.UpperArm] * (((leftElbow - leftShoulder) * BodyParameters.COMPercentage[gender][BodySegments.UpperArm] / 100) + leftShoulder);
            Matrix<double> leftWrist = body.SubMatrix(0, body.RowCount, 18, 3);
            com += BodyParameters.WeightPercentage[gender][BodySegments.LowerArm] * (((leftWrist - leftElbow) * BodyParameters.COMPercentage[gender][BodySegments.LowerArm] / 100) + leftElbow);
            Matrix<double> leftHandTip = body.SubMatrix(0, body.RowCount, 27, 3);
            com += BodyParameters.WeightPercentage[gender][BodySegments.Hand] * (((leftHandTip - leftWrist) * BodyParameters.COMPercentage[gender][BodySegments.Hand] / 100) + leftWrist);

            Matrix<double> rightShoulder = body.SubMatrix(0, body.RowCount, 33, 3);
            Matrix<double> rightElbow = body.SubMatrix(0, body.RowCount, 36, 3);
            com += BodyParameters.WeightPercentage[gender][BodySegments.UpperArm] * (((rightElbow - rightShoulder) * BodyParameters.COMPercentage[gender][BodySegments.UpperArm] / 100) + rightShoulder);
            Matrix<double> rightWrist = body.SubMatrix(0, body.RowCount, 39, 3);
            com += BodyParameters.WeightPercentage[gender][BodySegments.LowerArm] * (((rightWrist - rightElbow) * BodyParameters.COMPercentage[gender][BodySegments.LowerArm] / 100) + rightElbow);
            Matrix<double> rightHandTip = body.SubMatrix(0, body.RowCount, 45, 3);
            com += BodyParameters.WeightPercentage[gender][BodySegments.Hand] * (((rightHandTip - rightElbow) * BodyParameters.COMPercentage[gender][BodySegments.Hand] / 100) + rightElbow);

            Matrix<double> leftHip = body.SubMatrix(0, body.RowCount, 51, 3);
            Matrix<double> leftKnee = body.SubMatrix(0, body.RowCount, 54, 3);
            com += BodyParameters.WeightPercentage[gender][BodySegments.Thigh] * (((leftKnee - leftHip) * BodyParameters.COMPercentage[gender][BodySegments.Thigh] / 100) + leftHip);
            Matrix<double> leftAnkle = body.SubMatrix(0, body.RowCount, 57, 3);
            com += BodyParameters.WeightPercentage[gender][BodySegments.Shank] * (((leftAnkle - leftKnee) * BodyParameters.COMPercentage[gender][BodySegments.Shank] / 100) + leftKnee);
            //Matrix<double> leftFoot = body.SubMatrix(0, body.RowCount, 60, 3);
            //com += 0.01 * BodyParameters.WeightPercentage[gender][BodySegments.Foot] * (((leftFoot - leftAnkle) * BodyParameters.COMPercentage[gender][BodySegments.Foot] / 100) + leftAnkle);

            Matrix<double> rightHip = body.SubMatrix(0, body.RowCount, 63, 3);
            Matrix<double> rightKnee = body.SubMatrix(0, body.RowCount, 66, 3);
            com += BodyParameters.WeightPercentage[gender][BodySegments.Thigh] * (((rightKnee - rightHip) * BodyParameters.COMPercentage[gender][BodySegments.Thigh] / 100) + rightHip);
            Matrix<double> rightAnkle = body.SubMatrix(0, body.RowCount, 69, 3);
            com += BodyParameters.WeightPercentage[gender][BodySegments.Shank] * (((rightAnkle - rightKnee) * BodyParameters.COMPercentage[gender][BodySegments.Shank] / 100) + rightKnee);
            //Matrix<double> rightFoot = body.SubMatrix(0, body.RowCount, 72, 3);
            //com += 0.01 * BodyParameters.WeightPercentage[gender][BodySegments.Foot] * (((rightFoot - rightAnkle) * BodyParameters.COMPercentage[gender][BodySegments.Foot] / 100) + rightAnkle);
            return com/100;
        }

        private Matrix<double> Body3DListToMatrix(List<TRFBody3D> trfBody3Ds)
        {
            int row = trfBody3Ds.Count;
            int column = trfBody3Ds[0].GetType().GetProperties().Count();
            Matrix<double> body = Matrix<double>.Build.Dense(row, 76);
            for (int r = 0; r < row; r++)
            {
                //if (r == 0)
                //    body[r, 0] = 0;
                //else
                //    body[r, 0] =
                //        (trfBody3Ds[r]._timestamp - trfBody3Ds[0]._timestamp).TotalMilliseconds;
                int c = 0;
                foreach (Joint j in trfBody3Ds[r].joints3D.Values)
                {
                    body[r, c++] = j.Position.X;
                    body[r, c++] = j.Position.Y;
                    body[r, c++] = j.Position.Z;
                }
                body[r, c++] = trfBody3Ds[r]._purpose;
            }

            return body;
        }

        private double DistanceToAxis(Vector<double> v, Vector<double> axis)
        {
            double cosTheta = v.DotProduct(axis) / (v.L2Norm() * axis.L2Norm());
            double angle = Math.Acos(cosTheta);
            return Math.Sin(angle) * v.L2Norm();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body3DMatrix"></param>
        /// <param name="stage">0 for calibration stage, 1 for preparation stage, 2 for assessment stage</param>
        /// <returns></returns>
        private Matrix<double> SegmentBody3DMatrixToStages(Matrix<double> body3DMatrix, int stage)
        {
            List<Vector<double>> vectors = body3DMatrix.EnumerateRows().Where(row => Math.Abs(row.Last() - stage) < double.Epsilon).ToList();
            Matrix<double> matrix = Matrix<double>.Build.DenseOfRowVectors(vectors);
            return matrix.RemoveColumn(matrix.ColumnCount - 1);
        }

        /// <summary>
        /// The algorithm comes from http://www.euclideanspace.com/maths/algebra/vectors/angleBetween/
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private Matrix<double> EstimateRotationMatrix(Vector<double> from, Vector<double> to)
        {
            Vector<double> unitFrom = from.Divide(from.L2Norm());
            Vector<double> unitTo = to.Divide(to.L2Norm());
            double angle = Math.Acos(unitFrom.DotProduct(unitTo));
            Vector<double> axis = this.CrossProduct(unitFrom, unitTo);
            // skew-symmetric matrix
            Matrix<double> k = Matrix<double>.Build.Dense(3, 3, new double[] { 0, -axis[2], axis[1], axis[2], 0, -axis[0], -axis[1], axis[0], 0 });
            Matrix<double> r = Matrix<double>.Build.DenseIdentity(3) + k + k * k * (1 - Math.Cos(angle)) / Math.Pow(axis.L2Norm(), 2);
            return r.Transpose();
        }

        private Vector<double> CrossProduct(Vector<double> v1, Vector<double> v2)
        {
            Vector<double> result = Vector<double>.Build.Dense(3);
            result[0] = v1[1] * v2[2] - v1[2] * v2[1];
            result[1] = -v1[0] * v2[2] + v1[2] * v2[0];
            result[2] = v1[0] * v2[1] - v1[1] * v2[0];

            return result;

        }

        public Matrix<double> RotateBodies(Matrix<double> rotationMatrix, Matrix<double> body3DMatrix)
        {
            Matrix<double> newBody3DMatrix = Matrix<double>.Build.Dense(body3DMatrix.RowCount, body3DMatrix.ColumnCount);
            int rowCount = newBody3DMatrix.RowCount;
            int columnCount = newBody3DMatrix.ColumnCount;
            int jointCount = columnCount / 3;

            for (int r = 0; r < rowCount; r++)
            {
                for (int j = 0; j < jointCount; j++)
                {
                    newBody3DMatrix.SetSubMatrix(r, 3 * j, (rotationMatrix * body3DMatrix.SubMatrix(r, 1, 3 * j, 3).Transpose()).Transpose());
                }
            }
            newBody3DMatrix.SetColumn(columnCount - 1, body3DMatrix.Column(columnCount - 1));
            return newBody3DMatrix;
        }
        /// <summary>
        /// Convert data in an axis of a joint to a vector
        /// </summary>
        /// <param name="myBody"></param>
        /// <param name="jt"></param>
        /// <param name="axis">0 for X, 1 for Y, 2, for Z</param>
        /// <returns></returns>
        private Vector<double> ExtractVectorFromBody3DList(List<TRFBody3D> myBody, JointType jt, int axis)
        {
            int row = myBody.Count;
            Vector<double> vector = Vector<double>.Build.Dense(row == 0 ? 1 : row);
            for (int r = 0; r < row; r++)
            {
                switch (axis)
                {
                    case 0:
                        vector[r] = myBody[r].joints3D[jt].Position.X;
                        break;
                    case 1:
                        vector[r] = myBody[r].joints3D[jt].Position.Y;
                        break;
                    case 2:
                        vector[r] = myBody[r].joints3D[jt].Position.Z;
                        break;
                }
            }

            return vector;
        }

        /// <summary>
        /// Calculate the FFT for one dimensional signal
        /// </summary>
        /// <param name="signal">1D signal</param>
        /// <param name="fs">Sampling frequency</param>
        /// <returns>Frequency and amplitude</returns>
        private Matrix<double> FastFourierTransform(Vector<double> signal, double fs)
        {
            signal = signal.Subtract(signal.Mean());
            Complex[] data = new Complex[signal.Count];
            int L = signal.Count;
            for (int i = 0; i < L; i++)
            {
                data[i] = new Complex(signal[i], 0);
            }
            Fourier.Forward(data, FourierOptions.Matlab);
            Vector<double> spectrum1 = Vector<double>.Build.Dense(data.Length);
            for (int i = 0; i < L; i++)
            {
                spectrum1[i] = Complex.Abs(data[i] / L);
            }
            Vector<double> spectrum2 = spectrum1.SubVector(0, L / 2 + 1);
            spectrum2.SetSubVector(1, spectrum2.Count - 2, 2 * spectrum2.SubVector(1, spectrum2.Count - 2));
            Vector<double> frequencies = Vector<double>.Build.Dense(L / 2 + 1);
            for (int i = 0; i < L / 2 + 1; i++)
            {
                frequencies[i] = fs * i / L;
            }
            return Matrix<double>.Build.DenseOfColumnVectors(spectrum2, frequencies);
        }

        private Tuple<Vector<double>, Vector<double>> EstimateStandSit(Vector<double> signal, double highest, double lowest)
        {
            List<double> stand = new List<double>();
            List<double> sit = new List<double>();
            Vector<double> s1 = Vector<double>.Build.Dense((signal - signal.Mean()).Select(Math.Abs).ToArray());
            Matrix<double> peaksS1 = this.FindPeaks(-s1, 50);
            var a = peaksS1.EnumerateRows()
                .Where(
                    (v) =>
                        signal[(int) v[0]] < ((highest - lowest)*3/4 + lowest) &&
                        signal[(int) v[0]] > (highest - lowest)*1/4 + lowest).OrderBy((v) => v[0]);
            if (!a.Any())
                return null;
            Matrix<double> effectivePeaks =
                Matrix<double>.Build.DenseOfRowVectors(a);
            bool findTop = false; // An assessment always starts from bottom
            int rowCount = effectivePeaks.RowCount;
            for (int i = 0; i < rowCount; i++)
            {
                Matrix<double> peaks;
                if (i == 0)
                {
                    peaks = this.FindPeaks((findTop ? 1 : -1) * signal.SubVector(1, (int)effectivePeaks.Row(i)[0]));
                    if (peaks.RowCount == 0)
                        return null;
                    (findTop ? stand : sit).Add(peaks.EnumerateRows().Max((v) => v[0]));
                }
                else
                {
                    peaks = this.FindPeaks((findTop ? 1 : -1) * signal.SubVector((int)effectivePeaks.Row(i - 1)[0], (int)(effectivePeaks.Row(i) - effectivePeaks.Row(i - 1))[0]));
                    if (peaks.RowCount == 0)
                        return null;
                    var tempPeaks = peaks.EnumerateRows().OrderBy((v) => v[0]).ToList();
                    (findTop ? stand : sit).AddRange(new[] { tempPeaks.First()[0] + (int)effectivePeaks.Row(i - 1)[0], tempPeaks.Last()[0] + (int)effectivePeaks.Row(i - 1)[0] });
                }
                findTop = !findTop;

                if (i == rowCount - 1)
                {
                    peaks = this.FindPeaks((findTop ? 1 : -1) * signal.SubVector((int)effectivePeaks.Row(i)[0], signal.Count - (int)effectivePeaks.Row(i)[0]));
                    (findTop ? stand : sit).Add(peaks?.EnumerateRows().Min((v) => v[0]) + (int)effectivePeaks.Row(i)[0] ?? signal.Count - 1);
                }
            }
            return new Tuple<Vector<double>, Vector<double>>(Vector<double>.Build.DenseOfEnumerable(stand), Vector<double>.Build.DenseOfEnumerable(sit));
        }

        /// <summary>
        /// Find peaks in a 1D signal
        /// TODO: Return matrix or vector?
        /// </summary>
        /// <param name="signal">1D signal</param>
        /// <param name="nPeak">Required number of peaks</param>
        /// <param name="minPeakDistance">The minimum distance (samples) between two peaks</param>
        /// <returns></returns>
        private Matrix<double> FindPeaks(Vector<double> signal, int nPeak = 0, int minPeakDistance = 0)
        {
            int originalSignalLength = signal.Count;

            Matrix<double> signalWithIndex =
                Matrix<double>.Build.DenseOfColumnArrays(new List<double[]>()
                {
                    Enumerable.Range(0, originalSignalLength).Select(x => (double) x).ToArray(),
                    signal.ToArray()
                });

            var round1 =
                signalWithIndex.EnumerateRows()
                    .Where(vector =>
                        vector[0] > 0 && vector[0] < originalSignalLength - 1)
                    .Where(v => v[1] > signalWithIndex[(int)v[0] - 1, 1] &&
                          v[1] > signalWithIndex[(int)v[0] + 1, 1]);

            List<Vector<double>> round2 = new List<Vector<double>>();
            if (minPeakDistance != 0)
            {
                List<Vector<double>> roundTemp = round1.OrderBy((v) => v[1]).ToList();
                int i = roundTemp.Count - 1;
                while (i >= 0)
                {
                    Vector<double> highest = roundTemp[i];
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (Math.Abs(highest[0] - roundTemp[j][0]) < minPeakDistance)
                        {
                            roundTemp.Remove(roundTemp[j]);
                        }
                    }
                    round2.Add(highest);
                    roundTemp.RemoveAt(roundTemp.Count - 1);
                    i = roundTemp.Count - 1;
                }
            }
            else
                round2 = round1.ToList();

            if (nPeak != 0)
            {
                round2 = round2.OrderByDescending((v) => v[1]).ToList();
                if (round2.Count > nPeak)
                    round2 = round2.GetRange(0, nPeak);
            }
            return round2.Count == 0 ? null : Matrix<double>.Build.DenseOfRowVectors(round2);
        }

        //private void AssessmentNumberTime(Vector<double> signal)
        //{
        //    Matrix<double> fft = this.FastFourierTransform(signal, 30);
        //    Matrix<double> peak = this.FindPeaks(fft.Column(0), 1, 1);
        //    double frequency = 0;
        //    for (int i = 0; i < fft.RowCount; i++)
        //    {
        //        if (fft[i, 0].Equals(peak[0, 1]))
        //            frequency = fft[i, 1];
        //    }
        //    int cycles = (int)(Math.Round(frequency * 30));
        //    Matrix<double> peakTop = this.FindPeaks(signal, 1000, signal.Count / (2 * cycles));
        //    Matrix<double> peakBottom = this.FindPeaks(-signal, 1000, signal.Count / (2 * cycles));
        //    Matrix<double> tbCombo = Matrix<double>.Build.Dense(peakTop.RowCount + peakBottom.RowCount, 3);
        //    tbCombo.SetSubMatrix(0, peakTop.RowCount, 0, 2, peakTop);
        //    tbCombo.SetSubMatrix(0, peakTop.RowCount, 2, 1, Matrix<double>.Build.Dense(peakTop.RowCount, 1, 0));
        //    tbCombo.SetSubMatrix(peakTop.RowCount, peakBottom.RowCount, 0, 2, peakBottom);
        //    tbCombo.SetSubMatrix(peakTop.RowCount, peakBottom.RowCount, 2, 1, Matrix<double>.Build.Dense(peakBottom.RowCount, 1, 1));
        //    Matrix<double> tbComboSorted = this.SortMatrixColumn(tbCombo, 0, 0);
        //    Matrix<double> tbComboReduced = Matrix<double>.Build.Dense(tbComboSorted.RowCount * 2, tbComboSorted.ColumnCount, -1);
        //    int j = 0;
        //    int k;
        //    int m = 0;
        //    int length = tbComboSorted.RowCount;
        //    while (j < length - 1)
        //    {
        //        k = j + 1;
        //        if (tbComboSorted[k, 2].Equals(tbComboSorted[j, 2]))
        //        {
        //            j = k;
        //            continue;
        //        }
        //        while (k < length)
        //        {
        //            if (tbComboSorted[k, 0] - tbComboSorted[j, 0] >= (double)signal.Count / (4 * cycles))
        //            {
        //                tbComboReduced.SetRow(m, tbComboSorted.Row(j));
        //                m++;
        //                tbComboReduced.SetRow(m, tbComboSorted.Row(k));
        //                m++;
        //                j++;
        //                break;
        //            }
        //            k++;
        //            if (k < length)
        //            {
        //                if (tbComboSorted[k, 2].Equals(tbComboSorted[j, 2]))
        //                {
        //                    j = k;
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                j++;
        //            }
        //        }
        //    }
        //    length = tbComboReduced.RowCount;
        //    //for (int n = 1; n < length;)
        //    //{
        //    //    if (tbComboReduced.Row(n).Equals(tbComboReduced.Row(n - 1)))
        //    //    {
        //    //        tbComboReduced = tbComboReduced.RemoveRow(n);
        //    //        length--;
        //    //    }

        //    //    else if (tbComboReduced.Row(n).Equals(Vector<double>.Build.Dense(tbComboReduced.ColumnCount, -1)))
        //    //    {
        //    //        tbComboReduced = tbComboReduced.RemoveRow(n);
        //    //        length--;
        //    //    }
        //    //    else
        //    //        n++;
        //    //}

        //    Matrix<double> top = this.SelectFromColumn(tbComboReduced, 2, 0);
        //    Matrix<double> bottom = this.SelectFromColumn(tbComboReduced, 2, 1);
        //    if (top != null && bottom != null)
        //    {
        //        double meanTop = top.Column(1).Mean();
        //        double meanBottom = bottom.Column(1).Mean();
        //        double medium = (meanTop - meanBottom) / 2;
        //        this._csstAnalysisResult.cycleNumber = top.RowCount;
        //        if (tbComboReduced[tbComboReduced.RowCount - 1, 2].Equals(1))
        //        {
        //            if (signal.Last() > medium)
        //                this._csstAnalysisResult.cycleNumber = this._csstAnalysisResult.cycleNumber + 1;
        //        }

        //        this._csstAnalysisResult.upTime = new List<double>();
        //        this._csstAnalysisResult.downTime = new List<double>();
        //        for (int i = 1; i < tbComboReduced.RowCount; i++)
        //        {
        //            if (tbComboReduced[i - 1, 2].Equals(1) && tbComboReduced[i, 2].Equals(0))
        //            {
        //                this._csstAnalysisResult.upTime.Add(tbComboReduced[i, 0] - tbComboReduced[i - 1, 0]);
        //            }
        //            if (tbComboReduced[i - 1, 2].Equals(0) && tbComboReduced[i, 2].Equals(1))
        //            {
        //                this._csstAnalysisResult.downTime.Add(tbComboReduced[i, 0] - tbComboReduced[i - 1, 0]);
        //            }
        //        }

        //        if (this._csstAnalysisResult.upTime.Count != 0)
        //        {
        //            Vector<double> upTemp = Vector<double>.Build.DenseOfEnumerable(this._csstAnalysisResult.upTime);
        //            this._csstAnalysisResult.upTimeMean = upTemp.Mean();
        //            this._csstAnalysisResult.upTimeStd = upTemp.StandardDeviation();
        //        }
        //        if (this._csstAnalysisResult.downTime.Count != 0)
        //        {
        //            Vector<double> downTemp = Vector<double>.Build.DenseOfEnumerable(this._csstAnalysisResult.downTime);
        //            this._csstAnalysisResult.downTimeMean = downTemp.Mean();
        //            this._csstAnalysisResult.downTimeStd = downTemp.StandardDeviation();
        //        }
        //    }
        //    else
        //    {
        //        this._csstAnalysisResult.upTime = new List<double>();
        //        this._csstAnalysisResult.downTime = new List<double>();
        //        this._csstAnalysisResult.upTimeMean = 0;
        //        this._csstAnalysisResult.upTimeStd = 0;
        //        this._csstAnalysisResult.downTimeMean = 0;
        //        this._csstAnalysisResult.downTimeStd = 0;
        //    }
        //}

        //private void AssessmentBodyArmSwing(List<TRFBody3D> bodies, TRFUserGender gender)
        //{
        //    Vector<double> Y = Vector<double>.Build.DenseOfArray(new double[] { 0, -1, 0 });
        //    Matrix<double> com = Matrix<double>.Build.Dense(bodies.Count, 3, 0);
        //    int i = 0;
        //    foreach (CameraSpacePoint csp in bodies.Select(body => this.CalculateCenterOfMass(body, gender)))
        //    {
        //        com.SetRow(i, Vector<double>.Build.DenseOfArray(new double[] { csp.X, csp.Y, csp.Z }));
        //        i++;
        //    }
        //    i = 0;
        //    this._csstAnalysisResult.bodySwing = new List<double>();
        //    foreach (TRFBody3D body in bodies)
        //    {
        //        double[] temp1 = new double[3];
        //        temp1[0] = body.joints3D[JointType.SpineShoulder].Position.X -
        //                  com[i, 0];
        //        temp1[1] = body.joints3D[JointType.SpineShoulder].Position.Y -
        //                  com[i, 1];
        //        temp1[2] = body.joints3D[JointType.SpineShoulder].Position.Z -
        //                  com[i, 2];
        //        Vector<double> shoulderCenterV = Vector<double>.Build.DenseOfArray(temp1);
        //        this._csstAnalysisResult.bodySwing.Add(this.DistanceToAxis(shoulderCenterV, Y));
        //        i++;
        //    }


        //    Vector<double> leftArmVector = Vector<double>.Build.Dense(bodies.Count, 0);
        //    Vector<double> rightArmVector = Vector<double>.Build.Dense(bodies.Count, 0);
        //    int j = 0;
        //    foreach (TRFBody3D body in bodies)
        //    {
        //        double[] left = new double[3];
        //        left[0] = body.joints3D[JointType.WristLeft].Position.X -
        //                  body.joints3D[JointType.ShoulderLeft].Position.X;
        //        left[1] = body.joints3D[JointType.WristLeft].Position.Y -
        //                  body.joints3D[JointType.ShoulderLeft].Position.Y;
        //        left[2] = body.joints3D[JointType.WristLeft].Position.Z -
        //                  body.joints3D[JointType.ShoulderLeft].Position.Z;
        //        Vector<double> leftArmV = Vector<double>.Build.DenseOfArray(left);
        //        double[] right = new double[3];
        //        right[0] = body.joints3D[JointType.WristRight].Position.X -
        //                  body.joints3D[JointType.ShoulderRight].Position.X;
        //        right[1] = body.joints3D[JointType.WristRight].Position.Y -
        //                  body.joints3D[JointType.ShoulderRight].Position.Y;
        //        right[2] = body.joints3D[JointType.WristRight].Position.Z -
        //                  body.joints3D[JointType.ShoulderRight].Position.Z;
        //        Vector<double> rightArmV = Vector<double>.Build.DenseOfArray(right);
        //        leftArmVector[j] = this.DistanceToAxis(leftArmV, Y);
        //        rightArmVector[j] = this.DistanceToAxis(rightArmV, Y);
        //        j++;
        //    }

        //    this._csstAnalysisResult.bodySwingMean = this._csstAnalysisResult.bodySwing.Mean();
        //    this._csstAnalysisResult.bodySwingrStd = this._csstAnalysisResult.bodySwing.StandardDeviation();
        //    this._csstAnalysisResult.spineShoulderLimit = new Tuple<double, double>(-0.6, 0.6);
        //    this._csstAnalysisResult.leftArmSwing = leftArmVector.ToList();
        //    this._csstAnalysisResult.rightArmSwing = rightArmVector.ToList();
        //    Vector<double> temp2 = Vector<double>.Build.Dense(leftArmVector.Count + rightArmVector.Count, 0);
        //    temp2.SetSubVector(0, leftArmVector.Count, leftArmVector);
        //    temp2.SetSubVector(leftArmVector.Count, rightArmVector.Count, rightArmVector);
        //    this._csstAnalysisResult.armSwingMean = temp2.Mean();
        //    this._csstAnalysisResult.armSwingStd = temp2.StandardDeviation();
        //    this._csstAnalysisResult.armLimit = new Tuple<double, double>(-0.3, 0.3);
        //}

        ///// <summary>
        ///// This function sort a matrix according to values in a specific column
        ///// </summary>
        ///// <param name="input"></param>
        ///// <param name="referenceColumnIndex">The column used as the reference to sort</param>
        ///// <param name="order">0 is for Asc and 1 is for Desc</param>
        ///// <returns></returns>
        //private Matrix<double> SortMatrixColumn(Matrix<double> input, int referenceColumnIndex = 0, int order = 0)
        //{
        //    Vector<double> temp;
        //    int length = input.RowCount;
        //    if (order == 1)
        //        input = -1 * input;
        //    for (int i = 0; i < length; i++)
        //    {
        //        for (int j = i + 1; j < length; j++)
        //        {
        //            if (input[i, referenceColumnIndex] > input[j, referenceColumnIndex])
        //            {
        //                temp = input.Row(i);
        //                input.SetRow(i, input.Row(j));
        //                input.SetRow(j, temp);
        //            }
        //        }
        //    }
        //    if (order == 1)
        //        input = -1 * input;
        //    return input;
        //}

        //private Vector<double> SortVector(Vector<double> input, int order = 0)
        //{
        //    double temp;
        //    int length = input.Count;
        //    if (order == 1)
        //        input = -1 * input;
        //    for (int i = 0; i < length; i++)
        //    {
        //        for (int j = i + 1; j < length; j++)
        //        {
        //            if (input[i] > input[j])
        //            {
        //                temp = input[i];
        //                input[i] = input[j];
        //                input[j] = temp;
        //            }
        //        }
        //    }
        //    if (order == 1)
        //        input = -1 * input;
        //    return input;
        //}

        ///// <summary>
        ///// This function select from a matrix according to a specific value in a specific column
        ///// </summary>
        ///// <param name="input"></param>
        ///// <param name="index">The column index where the value is</param>
        ///// <param name="value">Select according to this value</param>
        ///// <returns></returns>
        //private Matrix<double> SelectFromColumn(Matrix<double> input, int index, double value)
        //{
        //    //int length = input.RowCount;
        //    //for (int i = 0; i < length;)
        //    //{
        //    //    if (!input[i, index].Equals(value))
        //    //    {
        //    //        if (input.RowCount == 1)
        //    //            return null;
        //    //        input = input.RemoveRow(i);
        //    //        length--;
        //    //    }
        //    //    else
        //    //    {
        //    //        i++;
        //    //    }
        //    //}
        //    return input;
        //}


        ///// <summary>
        ///// According to http://mobile-pt.com/files/timed_sit_to_stand_norms.pdf
        ///// if the age is lower than 60, we use 60-64 range
        ///// </summary>
        ////private void EstimateSeverityLevel(TRFUserGender gender, int age)
        ////{
        ////    switch (gender)
        ////    {
        ////        case TRFUserGender.Male:
        ////            {
        ////                if (age <= 64)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 14)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 14 && this._csstAnalysisResult.cycleNumber <= 19)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                else if (age >= 65 && age <= 69)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 12)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 12 && this._csstAnalysisResult.cycleNumber <= 18)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                else if (age >= 70 && age <= 74)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 12)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 12 && this._csstAnalysisResult.cycleNumber <= 17)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                else if (age >= 75 && age <= 79)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 11)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 11 && this._csstAnalysisResult.cycleNumber <= 17)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                else if (age >= 80 && age <= 84)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 10)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 10 && this._csstAnalysisResult.cycleNumber <= 15)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                else if (age >= 85 && age <= 89)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 8)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 8 && this._csstAnalysisResult.cycleNumber <= 14)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                else if (age >= 90 && age <= 94)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 7)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 7 && this._csstAnalysisResult.cycleNumber <= 12)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                break;
        ////            }
        ////        case UserGender.Female:
        ////            {
        ////                if (age <= 64)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 12)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 12 && this._csstAnalysisResult.cycleNumber <= 17)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                else if (age >= 65 && age <= 69)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 11)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 11 && this._csstAnalysisResult.cycleNumber <= 16)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                else if (age >= 70 && age <= 74)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 10)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 10 && this._csstAnalysisResult.cycleNumber <= 15)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                else if (age >= 75 && age <= 79)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 10)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 10 && this._csstAnalysisResult.cycleNumber <= 15)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                else if (age >= 80 && age <= 84)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 9)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 9 && this._csstAnalysisResult.cycleNumber <= 14)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                else if (age >= 85 && age <= 89)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 8)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 8 && this._csstAnalysisResult.cycleNumber <= 13)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                else if (age >= 90 && age <= 94)
        ////                {
        ////                    if (this._csstAnalysisResult.cycleNumber < 4)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Unhealthy;
        ////                    else if (this._csstAnalysisResult.cycleNumber >= 4 && this._csstAnalysisResult.cycleNumber <= 11)
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Healthy;
        ////                    else
        ////                        this._csstAnalysisResult.severityLevelLabel = GlobalVariables.CSSTAnalysis.SeverityLevel.Superior;
        ////                }
        ////                break;
        ////            }
        ////    }
        ////}

        //public void SaveResultToFile(string path)
        //{
        //    XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());
        //    StringWriter textWriter = new StringWriter();
        //    xmlSerializer.Serialize(textWriter, this);

        //    using (StreamWriter outputFile = new StreamWriter(path))
        //    {
        //        outputFile.WriteLine(textWriter.ToString());
        //    }
        //}
    }
}
