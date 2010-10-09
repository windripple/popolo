/* ResponseFactor.cs
 * 
 * Copyright (C) 2008 E.Togashi
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or (at
 * your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 */

using System;

namespace Popolo.ThermalLoad
{
    /// <summary>応答係数計算クラス</summary>
    public static class ResponseFactor
    {
        
        #region クラス変数

        private static double[,] matP, matPd, matPi, matPid, matPd2, matPo, matPod;
        
        #endregion
        
        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        static ResponseFactor()
        {
            matP = new double[2, 2];
            matPd = new double[2, 2];
            matPi = new double[2, 2];
            matPid = new double[2, 2];
            matPd2 = new double[2, 2];
            matPo = new double[2, 2];
            matPod = new double[2, 2];
        }

        #endregion

        #region publicメソッド

        /// <summary>共通比Cを用いた応答係数を計算する</summary>
        /// <param name="timeStep">タイムステップ[sec]</param>
        /// <param name="filmCoefficient1">1側総合熱伝達率[W/m2K]</param>
        /// <param name="filmCoefficient2">2側総合熱伝達率[W/m2K]</param>
        /// <param name="wallLayers">壁層</param>
        /// <param name="rfNumber">応答係数の数</param>
        /// <param name="rFactorX">1側貫流応答係数リスト</param>
        /// <param name="rFactorY">吸熱応答係数</param>
        /// <param name="rFactorZ">2側貫流応答係数</param>
        /// <param name="commonRatio">共通比</param>
        public static void GetResponseFactor(double timeStep, double filmCoefficient1, double filmCoefficient2, WallLayers wallLayers, uint rfNumber,
            ref double[] rFactorX, ref double[] rFactorY, ref double[] rFactorZ, out double commonRatio)
        {
            const int G_NUMBER = 8;

            double[] beta = new double[G_NUMBER];
            double[,] g_xyz = new double[3, G_NUMBER];
            double[,] rf = new double[3, rfNumber];
            double[] g_xyz_0 = new double[3];

            //熱抵抗リストと熱容量リストを初期化
            double[] res = new double[wallLayers.LayerNumber + 2];
            double[] cap = new double[wallLayers.LayerNumber + 2];
            res[0] = 1.0d / filmCoefficient1;
            res[res.Length - 1] = 1.0d / filmCoefficient2;
            for (uint i = 0; i < wallLayers.LayerNumber; i++)
            {
                WallLayers.Layer wl = wallLayers.GetLayer(i);
                res[i + 1] = wl.Resistance;
                cap[i + 1] = wl.HeatCapacityPerUnitArea;
            }

            for (int i = 0; i < G_NUMBER; i++)
            {
                uint iter = 0;  //反復計算回数
                double bm = 0;  //初期値は0

                //Beta(i)を収束計算
                while (true)
                {
                    //各壁層の行列Pを計算する
                    for (uint j = 0; j < res.Length; j++)
                    {
                        double rCap = cap[j] * res[j];
                        double w = rCap * bm;
                        if (0 < w)
                        {
                            w = Math.Sqrt(w);
                            double cw = Math.Cos(w);
                            double sw = Math.Sin(w);
                            matPi[0, 0] = matPi[1, 1] = cw;
                            matPi[0, 1] = res[j] * sw / w;
                            matPi[1, 0] = -w * sw / res[j];
                            matPid[0, 0] = matPid[1, 1] = 0.5 * rCap * sw / w;
                            matPid[0, 1] = 0.5 * res[j] * (sw / w - cw) / bm;
                            matPid[1, 0] = 0.5 * cap[j] * (sw / w + cw);
                        }
                        else
                        {
                            matPi[0, 0] = 1.0;
                            matPi[0, 1] = res[j];
                            matPi[1, 0] = 0.0;
                            matPi[1, 1] = 1.0;
                            if (cap[j] == 0) initMatrix(matPid, 0.0);
                            else
                            {
                                matPid[0, 0] = matPid[1, 1] = 0.5 * rCap;
                                matPid[0, 1] = res[j] * rCap / 6.0d;
                                matPid[1, 0] = cap[j];
                            }
                        }
                        if (j == 0)
                        {
                            copyMatrix(matPi, matP);
                            copyMatrix(matPid, matPd);
                        }
                        else
                        {
                            copyMatrix(matP, matPo);
                            copyMatrix(matPd, matPod);
                            multiplicateMatrix(matPo, matPi, ref matP);
                            multiplicateMatrix(matPod, matPi, ref matPd);
                            multiplicateMatrix(matPo, matPid, ref matPd2);
                            addMatrix(matPd, matPd2, ref matPd);
                        }
                    }
                    if (i == 0 && iter == 0)
                    {
                        double p2 = matP[0, 1] * matP[0, 1];
                        g_xyz_0[0] = (matPd[0, 0] * matP[0, 1] - matP[0, 0] * matPd[0, 1]) / p2;
                        g_xyz_0[1] = -matPd[0, 1] / p2;
                        g_xyz_0[2] = (matPd[1, 1] * matP[0, 1] - matP[1, 1] * matPd[0, 1]) / p2;
                    }

                    //収束判定
                    if (Math.Abs(matP[0, 1]) < 1.0e-10d) break;

                    //Nラプソン法でbetaを更新
                    double bmm = 0;
                    if (0 < i)
                    {
                        for (int j = 0; j < i; j++)
                        {
                            bmm += 1.0d / (beta[j] - bm);
                        }
                    }
                    bm += matP[0, 1] / (matPd[0, 1] - matP[0, 1] * bmm);
                    iter++;
                    if (20 < iter) throw new Exception("Iteration Error");
                }

                //betaを設定
                beta[i] = bm;
                g_xyz[1, i] = 1.0d / (bm * bm * matPd[0, 1]);
                g_xyz[0, i] = matP[0, 0] * g_xyz[1, i];
                g_xyz[2, i] = matP[1, 1] * g_xyz[1, i];
            }

            for (int xyz = 0; xyz < 3; xyz++)
            {
                rf[xyz, 0] = g_xyz_0[xyz];
                rf[xyz, 1] = -g_xyz_0[xyz];
                for (int i = 2; i < rfNumber; i++) rf[xyz, i] = 0;
            }

            for (int i = 0; i < G_NUMBER; i++)
            {
                double eb = Math.Exp(-beta[i] * timeStep);
                for (int xyz = 0; xyz < 3; xyz++)
                {
                    rf[xyz, 0] += g_xyz[xyz, i] * eb;
                    rf[xyz, 1] += g_xyz[xyz, i] * (eb - 2.0d) * eb;
                    for (int j = 2; j < rfNumber; j++) rf[xyz, j] += g_xyz[xyz, i] * (1.0d - eb) * (1.0d - eb) * Math.Exp(-beta[i] * (j - 1) * timeStep);
                }
            }
            double kValue = wallLayers.GetThermalTransmission(filmCoefficient1, filmCoefficient2);
            for (int xyz = 0; xyz < 3; xyz++)
            {
                rf[xyz, 0] = kValue + rf[xyz, 0] / timeStep;
                for (int i = 1; i < rfNumber; i++) rf[xyz, i] /= timeStep;
            }

            //公比
            commonRatio = Math.Exp(-beta[0] * timeStep);
            rFactorX[0] = rf[0, 0];
            rFactorY[0] = rf[1, 0];
            rFactorZ[0] = rf[2, 0];

            for (int i = 1; i < rfNumber; i++)
            {
                rFactorX[i] = rf[0, i] - commonRatio * rf[0, i - 1];
                rFactorY[i] = rf[1, i] - commonRatio * rf[1, i - 1];
                rFactorZ[i] = rf[2, i] - commonRatio * rf[2, i - 1];
            }
        }

        /// <summary>熱流[W/m2]を計算する</summary>
        /// <param name="tempSeries">過去の温度差履歴</param>
        /// <param name="rFactors">応答係数</param>
        /// <param name="commonRatio">共通比</param>
        /// <param name="lastHeatLoad">前タイムステップの熱流[W/m2]</param>
        /// <returns>熱流[W/m2]</returns>
        public static double GetHeatLoad(double[] tempSeries, double[] rFactors, double commonRatio, double lastHeatLoad)
        {
            double qload = 0;
            for (int i = 0; i < rFactors.Length; i++)
            {
                qload += tempSeries[i] * rFactors[i];
            }
            return qload + commonRatio * lastHeatLoad;
        }

        #endregion

        #region privateメソッド

        /// <summary>行列積を計算する</summary>
        /// <param name="matA"></param>
        /// <param name="matB"></param>
        /// <param name="matC"></param>
        private static void multiplicateMatrix(double[,] matA, double[,] matB, ref double[,] matC)
        {
            int colNumA = matA.GetLength(0); //行数A
            int rowNumA = matA.GetLength(1); //列数A
            int rowNumB = matB.GetLength(1); //列数B

            for (int i = 0; i < colNumA; i++)
            {
                for (int j = 0; j < rowNumB; j++)
                {
                    matC[i, j] = 0;
                    for (int k = 0; k < rowNumA; k++)
                    {
                        matC[i, j] += matA[i, k] * matB[k, j];
                    }
                }
            }
        }

        /// <summary>行列和を計算する</summary>
        /// <param name="matA"></param>
        /// <param name="matB"></param>
        /// <param name="matC"></param>
        private static void addMatrix(double[,] matA, double[,] matB, ref double[,] matC)
        {
            int colNum = matA.GetLength(0); //行数
            int rowNum = matA.GetLength(1); //列数

            for (int i = 0; i < colNum; i++)
            {
                for (int j = 0; j < rowNum; j++)
                {
                    matC[i, j] = matA[i, j] + matB[i, j];
                }
            }
        }

        /// <summary>行列要素をinitialValueで初期化する</summary>
        /// <param name="matrix"></param>
        /// <param name="initialValue"></param>
        private static void initMatrix(double[,] matrix, double initialValue)
        {
            int colNum = matrix.GetLength(0); //行数
            int rowNum = matrix.GetLength(1); //列数

            for (int i = 0; i < colNum; i++)
            {
                for (int j = 0; j < rowNum; j++)
                {
                    matrix[i, j] = initialValue;
                }
            }
        }

        /// <summary>行列Aを行列Bにコピーする</summary>
        /// <param name="matA"></param>
        /// <param name="matB"></param>
        private static void copyMatrix(double[,] matA, double[,] matB)
        {
            int colNum = matA.GetLength(0); //行数
            int rowNum = matA.GetLength(1); //列数

            for (int i = 0; i < colNum; i++)
            {
                for (int j = 0; j < rowNum; j++)
                {
                    matB[i, j] = matA[i, j];
                }
            }
        }

        #endregion

    }
}
