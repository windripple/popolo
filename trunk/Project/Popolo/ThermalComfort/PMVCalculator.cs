/* PMVCalculator.cs
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

namespace Popolo.ThermalComfort
{
    /// <summary>熱的快適性計算クラス</summary>
    public static class PMVCalculator
    {

        #region Constants

        /// <summary>相対誤差許容値</summary>
        private const double ERROR_TOL_R = 0.0001;

        /// <summary>絶対誤差許容値</summary>
        private const double ERROR_TOL_A = 0.00001;

        /// <summary>微分値計算定数</summary>
        private const double DELTA = 0.00001;

        /// <summary>代謝量換算係数[(W/m2)/met]</summary>
        private const double MET_TO_M = 58.15;

        /// <summary>絶対温度換算用数値</summary>
        private const double C_TO_K = 273.15;

        #endregion

        #region 列挙型

        /// <summary>仕事</summary>
        public enum Tasks
        {
            /// <summary>休息：睡眠</summary>
            Resting_Sleeping,
            /// <summary>休息：リクライニング</summary>
            Resting_Reclining,
            /// <summary>休息：座位安静</summary>
            Resting_Seated_Quiet,
            /// <summary>休息：立位安静</summary>
            Resting_Standing_Relaxed,
            /// <summary>歩行：0.9m/s</summary>
            Walking_Slow_09ms,
            /// <summary>歩行：1.2m/s</summary>
            Walking_Normal_12ms,
            /// <summary>歩行：1.8m/s</summary>
            Walking_Fast_18ms,
            /// <summary>オフィス作業：座位読み書き</summary>
            OfficeActivities_Seated_Reading_Writing,
            /// <summary>オフィス作業：タイピング</summary>
            OfficeActivities_Typing,
            /// <summary>オフィス作業：座位ファイル作業</summary>
            OfficeActivities_Filing_Seated,
            /// <summary>オフィス作業：立位ファイル作業</summary>
            OfficeActivities_Filing_Standing,
            /// <summary>オフィス作業：歩行</summary>
            OfficeActivities_Walking,
            /// <summary>オフィス作業：運搬・梱包</summary>
            OfficeActivities_Lifting_Packing,
            /// <summary>運転：自動車</summary>
            Driving_Automobile,
            /// <summary>運転：飛行機</summary>
            Driving_Aircraft_Routine,
            /// <summary>運転：飛行機着陸</summary>
            Driving_Aircraft_Instrument_Landing,
            /// <summary>運転：戦闘機</summary>
            Driving_Aircraft_Combat,
            /// <summary>運転：大型車両</summary>
            Driving_HeavyVehicle,
            /// <summary>その他：調理</summary>
            Other_Occupational_Cooking,
            /// <summary>その他：清掃</summary>
            Other_Occupational_HouseCleaning,
            /// <summary>その他：激しい着席中手作業</summary>
            Other_Occupational_Seated_HeavyLimbMovement,
            /// <summary>その他：のこ引き</summary>
            Other_Occupational_MachineWork_Sawing,
            /// <summary>その他：軽い機械作業</summary>
            Other_Occupational_MachineWork_Light,
            /// <summary>その他：激しい機械作業</summary>
            Other_Occupational_MachineWork_Heavy,
            /// <summary>その他：50kg程度の荷物運搬作業</summary>
            Other_Occupational_Handling50kgBags,
            /// <summary>その他：シャベルによる掘削作業</summary>
            Other_Occupational_PickAndShovelWork,
            /// <summary>その他：ダンス</summary>
            Other_Leisure_Dancing,
            /// <summary>その他：運動</summary>
            Other_Leisure_Exercise,
            /// <summary>その他：テニス</summary>
            Other_Leisure_Tennes,
            /// <summary>その他：バスケットボール</summary>
            Other_Leisure_Basketball,
            /// <summary>その他：レスリング</summary>
            Other_Leisure_Wrestling
        }

        #endregion

        #region public methods基本機能

        /// <summary>人体からの熱損失[W]を計算する</summary>
        /// <param name="dryBulbTemperature">乾球温度[CDB]</param>
        /// <param name="meanRadiantTemperature">平均放射温度[C]</param>
        /// <param name="relativeAirVelocity">気流速度[m/s]</param>
        /// <param name="relativeHumidity">相対湿度[%]</param>
        /// <param name="clothing">着衣量[clo]</param>
        /// <param name="metabolicRate">代謝量[met]</param>
        /// <param name="externalWork">外部仕事量[met]</param>
        /// <param name="heatLoss">人体からの熱損失[W]</param>
        /// <returns>計算成功の真偽</returns>
        public static bool TryCalculateHeatLossFromBody(double dryBulbTemperature, double meanRadiantTemperature, double relativeHumidity,
           double relativeAirVelocity, double clothing, double metabolicRate, double externalWork, out double heatLoss)
        {
            //初期化
            heatLoss = 0;
            
            double hc;
            double pa = relativeHumidity * 10.0 * Popolo.ThermophysicalProperty.MoistAir.GetSaturatedVaporPressure(dryBulbTemperature);  //[Pa]=(RH/100)*1000*[kPa]
            double dbtKelvin = C_TO_K + dryBulbTemperature;
            double mrtKelvin = C_TO_K + meanRadiantTemperature;

            //***代謝量***
            double m = metabolicRate * MET_TO_M;       //W/m2に換算
            double mw = m - externalWork * MET_TO_M;   //体内の発熱量

            //***着衣量***
            double icl = 0.155 * clothing;  //着衣の断熱性能[m2K/W]
            double fcl;                     //着衣面積係数
            if (icl < 0.078) fcl = 1.0 + 1.29 * icl;
            else fcl = 1.05 + 0.645 * icl;

            //対流
            double hcf = 12.1 * Math.Sqrt(relativeAirVelocity); //強制対流による熱伝導率

            //着衣表面温度の収束計算
            double tcla = dbtKelvin + (35.5 - dryBulbTemperature) / (3.5 * (6.45 * icl + 0.1));
            double p1 = icl * fcl;
            double p2 = p1 * 3.96;
            double p3 = p1 * 100;
            double p4 = p1 * dbtKelvin;
            double p5 = 308.7 - 0.028 * mw + p2 * Math.Pow(mrtKelvin / 100.0, 4);
            double xn = tcla / 100.0;
            int iterNum = 0;

            while (true)
            {
                //誤差微分値計算
                hc = Math.Max(hcf, 2.38 * Math.Pow(Math.Abs(100.0 * xn - dbtKelvin), 0.25));
                double err1 = Math.Abs((p5 + p4 * hc - p2 * Math.Pow(xn, 4)) / (100.0 + p3 * hc) - xn);

                //収束判定
                double errTol = Math.Abs(xn) * ERROR_TOL_R + ERROR_TOL_A;
                if (Math.Abs(err1) < errTol) break;

                double xnd = xn + DELTA;
                hc = Math.Max(hcf, 2.38 * Math.Pow(Math.Abs(100.0 * xnd - dbtKelvin), 0.25));
                double err2 = Math.Abs((p5 + p4 * hc - p2 * Math.Pow(xnd, 4)) / (100.0 + p3 * hc) - xnd);

                //状態値修正
                xn -= err1 / ((err2 - err1) / DELTA);

                //反復回数計算
                iterNum++;
                if (100 < iterNum) return false;
            }
            double tcl = 100.0 * xn - C_TO_K; //着衣表面温度

            //***熱損失計算***
            //heat loss diff. through skin
            double ediff = 3.05 * 0.001 * (5733.0 - 6.99 * mw - pa);
            //heat loss by sweating (comfort)
            double esw;
            if (MET_TO_M < mw) esw = 0.42 * (mw - MET_TO_M);
            else esw = 0;
            //latent respiration heat loss
            double lres = 1.7 * 0.00001 * m * (5867.0 - pa);
            //dry respiration heat loss
            double dres = 0.0014 * m * (34.0 - dryBulbTemperature);
            //heat loss by radiation
            double r = 3.96 * fcl * (Math.Pow(xn, 4) - Math.Pow(mrtKelvin / 100.0, 4));
            //heat loss by convection
            double c = fcl * hc * (tcl - dryBulbTemperature);

            //集計
            heatLoss = ediff + esw + lres + dres + r + c;
            return true;
        }

        /// <summary>PMV値[-]を計算する</summary>
        /// <param name="dryBulbTemperature">乾球温度[CDB]</param>
        /// <param name="meanRadiantTemperature">平均放射温度[C]</param>
        /// <param name="relativeAirVelocity">気流速度[m/s]</param>
        /// <param name="relativeHumidity">相対湿度[%]</param>
        /// <param name="clothing">着衣量[clo]</param>
        /// <param name="metabolicRate">代謝量[met]</param>
        /// <param name="externalWork">外部仕事量[met]</param>
        /// <param name="pmv">PMV値[-]</param>
        /// <returns>計算成功の真偽</returns>
        public static bool TryCalculatePMV(double dryBulbTemperature, double meanRadiantTemperature, double relativeHumidity,
           double relativeAirVelocity, double clothing, double metabolicRate, double externalWork, out double pmv)
        {
            pmv = 999999;

            double heatLoss;
            if (TryCalculateHeatLossFromBody(dryBulbTemperature, meanRadiantTemperature, relativeHumidity,
                relativeAirVelocity, clothing, metabolicRate, externalWork, out heatLoss))
            {
                pmv = TryCalculatePMV(metabolicRate, externalWork, heatLoss);

                return true;
            }
            else return false;
        }

        /// <summary>PMV値[-]を計算する</summary>
        /// <param name="metabolicRate">代謝量[met]</param>
        /// <param name="externalWork">外部仕事量[met]</param>
        /// <param name="heatLoss">人体からの熱損失[W]</param>
        /// <returns>PMV値[-]</returns>
        public static double TryCalculatePMV(double metabolicRate, double externalWork, double heatLoss)
        {
            double m = metabolicRate * MET_TO_M;       //W/m2に換算
            double mw = m - externalWork * MET_TO_M;   //体内の発熱量
            double ts = 0.303 * Math.Exp(-0.036 * m) + 0.028;
            return ts * (mw - heatLoss);
        }

        /// <summary>PPD値[%]を計算する</summary>
        /// <param name="pmv">PMV値</param>
        /// <returns>PPD値[%]</returns>
        public static double GetPPDFromPMV(double pmv)
        {
            return 100.0 - 95.0 * Math.Exp(-0.03353 * Math.Pow(pmv, 4) - 0.2179 * Math.Pow(pmv, 2));
        }

        /// <summary>PPD値[%]からPMV値[-]を求める</summary>
        /// <param name="ppd">PPD値[%]</param>
        /// <returns>PMV値[-]（正側のみ）</returns>
        public static double GetPMVFromPPD(double ppd)
        {
            if (ppd < 5 || 100 < ppd) throw new Exception("PPD値が不正です");

            double pexp = Math.Log(-(ppd - 100) / 95.0);

            double pmv = 1;
            int iterNum = 0;
            while (true)
            {
                double err1 = -0.03353 * Math.Pow(pmv, 4) - 0.2179 * Math.Pow(pmv, 2) - pexp;
                double errTol = Math.Abs(pexp) * ERROR_TOL_R + ERROR_TOL_A;
                if (Math.Abs(err1) < errTol) break;
                double pmvd = pmv + DELTA;
                double err2 = -0.03353 * Math.Pow(pmvd, 4) - 0.2179 * Math.Pow(pmvd, 2) - pexp;

                //状態値修正
                pmv -= err1 / ((err2 - err1) / DELTA);

                iterNum++;
                if (100 < iterNum) throw new Exception("PMV収束計算エラー");
            }
            return Math.Abs(pmv);
        }

        /// <summary>PMV値[-]から乾球温度[CDB]を求める</summary>
        /// <param name="pmv">PMV値[-]</param>
        /// <param name="meanRadiantTemperature">平均放射温度[C]</param>
        /// <param name="relativeAirVelocity">気流速度[m/s]</param>
        /// <param name="relativeHumidity">相対湿度[%]</param>
        /// <param name="clothing">着衣量[clo]</param>
        /// <param name="metabolicRate">代謝量[met]</param>
        /// <param name="externalWork">外部仕事量[met]</param>
        /// <param name="dryBulbTemperature">出力：乾球温度[CDB]</param>
        /// <returns>計算成功の真偽</returns>
        public static bool TryCalculateDryBulbTemperature(double pmv, double meanRadiantTemperature, double relativeHumidity,
            double relativeAirVelocity, double clothing, double metabolicRate, double externalWork, out double dryBulbTemperature)
        {
            //上下限値確認
            dryBulbTemperature = 50;
            double pmvh;
            if (!TryCalculatePMV(dryBulbTemperature, meanRadiantTemperature, relativeHumidity,
                relativeAirVelocity, clothing, metabolicRate, externalWork, out pmvh)) return false;
            if (pmvh < pmv) return false;
            dryBulbTemperature = -10;
            double pmvl;
            if (!TryCalculatePMV(dryBulbTemperature, meanRadiantTemperature, relativeHumidity,
                relativeAirVelocity, clothing, metabolicRate, externalWork, out pmvl)) return false;
            if (pmv < pmvl) return false;

            int iterNum = 0;
            dryBulbTemperature = 25;
            while (true)
            {
                double pmvBuff;
                if (!TryCalculatePMV(dryBulbTemperature, meanRadiantTemperature, relativeHumidity,
                    relativeAirVelocity, clothing, metabolicRate, externalWork, out pmvBuff)) return false;
                double err1 = pmvBuff - pmv;
                double errTol = Math.Abs(pmv) * ERROR_TOL_R + ERROR_TOL_A;
                if (Math.Abs(err1) < errTol) break;
                double dryBulbTemperatureD = dryBulbTemperature + DELTA;
                if (!TryCalculatePMV(dryBulbTemperatureD, meanRadiantTemperature, relativeHumidity,
                    relativeAirVelocity, clothing, metabolicRate, externalWork, out pmvBuff)) return false;
                double err2 = pmvBuff - pmv;

                //状態値修正
                dryBulbTemperature -= err1 / ((err2 - err1) / DELTA);

                iterNum++;
                if (100 < iterNum) return false;
            }

            return true;
        }

        /// <summary>PMV値[-]から相対湿度[%]を求める</summary>
        /// <param name="pmv">PMV値[-]</param>
        /// <param name="dryBulbTemperature">乾球温度[CDB]</param>
        /// <param name="meanRadiantTemperature">平均放射温度[C]</param>
        /// <param name="relativeAirVelocity">気流速度[m/s]</param>
        /// <param name="clothing">着衣量[clo]</param>
        /// <param name="metabolicRate">代謝量[met]</param>
        /// <param name="externalWork">外部仕事量[met]</param>
        /// <param name="relativeHumidity">出力：相対湿度[%]</param>
        /// <returns>計算成功の真偽</returns>
        public static bool TryCalculateRelativeHumidity(double pmv, double dryBulbTemperature, double meanRadiantTemperature,
            double relativeAirVelocity, double clothing, double metabolicRate, double externalWork, out double relativeHumidity)
        {
            //上下限値確認
            relativeHumidity = 0;
            double pmvl;
            if(!TryCalculatePMV(dryBulbTemperature, meanRadiantTemperature, relativeHumidity,
                relativeAirVelocity, clothing, metabolicRate, externalWork, out pmvl)) return false;
            if (pmv < pmvl) return false;
            relativeHumidity = 100;
            double pmvh;
            if(!TryCalculatePMV(dryBulbTemperature, meanRadiantTemperature, relativeHumidity,
                relativeAirVelocity, clothing, metabolicRate, externalWork, out pmvh)) return false;
            if (pmvh < pmv) return false;

            int iterNum = 0;
            relativeHumidity = 40;
            while (true)
            {
                double pmvBuff;
                if (!TryCalculatePMV(dryBulbTemperature, meanRadiantTemperature, relativeHumidity,
                    relativeAirVelocity, clothing, metabolicRate, externalWork, out pmvBuff)) return false;
                double err1 = pmvBuff - pmv;
                double errTol = Math.Abs(pmv) * ERROR_TOL_R + ERROR_TOL_A;
                if (Math.Abs(err1) < errTol) break;
                double relativeHumidityD = relativeHumidity + DELTA;
                if (!TryCalculatePMV(dryBulbTemperature, meanRadiantTemperature, relativeHumidityD,
                    relativeAirVelocity, clothing, metabolicRate, externalWork, out pmvBuff)) return false;
                double err2 = pmvBuff - pmv;

                //状態値修正
                relativeHumidity -= err1 / ((err2 - err1) / DELTA);

                iterNum++;
                if (100 < iterNum) return false;
            }

            return true;
        }

        #endregion

        #region public methods補助機能

        /// <summary>仕事に対する代謝量[met]を返す</summary>
        /// <param name="task">仕事種類</param>
        /// <returns>代謝量[met]</returns>
        public static double GetMet(Tasks task)
        {
            switch (task)
            {
                case Tasks.Driving_Aircraft_Combat:
                    return 2.4;
                case Tasks.Driving_Aircraft_Instrument_Landing:
                    return 1.8;
                case Tasks.Driving_Aircraft_Routine:
                    return 1.2;
                case Tasks.Driving_Automobile:
                    return 1.5;
                case Tasks.Driving_HeavyVehicle:
                    return 3.2;
                case Tasks.OfficeActivities_Filing_Seated:
                    return 1.2;
                case Tasks.OfficeActivities_Filing_Standing:
                    return 1.4;
                case Tasks.OfficeActivities_Lifting_Packing:
                    return 2.1;
                case Tasks.OfficeActivities_Seated_Reading_Writing:
                    return 1.0;
                case Tasks.OfficeActivities_Typing:
                    return 1.1;
                case Tasks.OfficeActivities_Walking:
                    return 1.7;
                case Tasks.Other_Leisure_Basketball:
                    return 5.8;
                case Tasks.Other_Leisure_Dancing:
                    return 3.4;
                case Tasks.Other_Leisure_Exercise:
                    return 3.5;
                case Tasks.Other_Leisure_Tennes:
                    return 3.8;
                case Tasks.Other_Leisure_Wrestling:
                    return 7.8;
                case Tasks.Other_Occupational_Cooking:
                    return 1.8;
                case Tasks.Other_Occupational_Handling50kgBags:
                    return 4.0;
                case Tasks.Other_Occupational_HouseCleaning:
                    return 2.7;
                case Tasks.Other_Occupational_MachineWork_Heavy:
                    return 4.0;
                case Tasks.Other_Occupational_MachineWork_Light:
                    return 1.8;
                case Tasks.Other_Occupational_MachineWork_Sawing:
                    return 2.2;
                case Tasks.Other_Occupational_PickAndShovelWork:
                    return 4.4;
                case Tasks.Other_Occupational_Seated_HeavyLimbMovement:
                    return 2.2;
                case Tasks.Resting_Reclining:
                    return 0.8;
                case Tasks.Resting_Seated_Quiet:
                    return 1.0;
                case Tasks.Resting_Sleeping:
                    return 0.7;
                case Tasks.Resting_Standing_Relaxed:
                    return 1.2;
                case Tasks.Walking_Fast_18ms:
                    return 3.8;
                case Tasks.Walking_Normal_12ms:
                    return 2.6;
                case Tasks.Walking_Slow_09ms:
                    return 2.0;
                default:
                    throw new Exception("代謝量が未定義です");
            }
        }

        /// <summary>仕事名称を返す</summary>
        /// <param name="task">仕事種類</param>
        /// <returns>仕事名称</returns>
        public static string GetTaskName(Tasks task)
        {
            //国際化のためのリソースを取得
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager("Popolo.Utility.Properties.Resources", asm);

            switch (task)
            {
                case Tasks.Resting_Sleeping:
                    return rm.GetString("ThermalComfort_TaskName01");
                case Tasks.Resting_Reclining:
                    return rm.GetString("ThermalComfort_TaskName02");
                case Tasks.Resting_Seated_Quiet:
                    return rm.GetString("ThermalComfort_TaskName03");
                case Tasks.Resting_Standing_Relaxed:
                    return rm.GetString("ThermalComfort_TaskName04");
                case Tasks.Walking_Slow_09ms:
                    return rm.GetString("ThermalComfort_TaskName05");
                case Tasks.Walking_Normal_12ms:
                    return rm.GetString("ThermalComfort_TaskName06");
                case Tasks.Walking_Fast_18ms:
                    return rm.GetString("ThermalComfort_TaskName07");
                case Tasks.OfficeActivities_Seated_Reading_Writing:
                    return rm.GetString("ThermalComfort_TaskName08");
                case Tasks.OfficeActivities_Typing:
                    return rm.GetString("ThermalComfort_TaskName09");
                case Tasks.OfficeActivities_Filing_Seated:
                    return rm.GetString("ThermalComfort_TaskName10");
                case Tasks.OfficeActivities_Filing_Standing:
                    return rm.GetString("ThermalComfort_TaskName11");
                case Tasks.OfficeActivities_Walking:
                    return rm.GetString("ThermalComfort_TaskName12");
                case Tasks.OfficeActivities_Lifting_Packing:
                    return rm.GetString("ThermalComfort_TaskName13");
                case Tasks.Driving_Automobile:
                    return rm.GetString("ThermalComfort_TaskName14");
                case Tasks.Driving_Aircraft_Routine:
                    return rm.GetString("ThermalComfort_TaskName15");
                case Tasks.Driving_Aircraft_Instrument_Landing:
                    return rm.GetString("ThermalComfort_TaskName16");
                case Tasks.Driving_Aircraft_Combat:
                    return rm.GetString("ThermalComfort_TaskName17");
                case Tasks.Driving_HeavyVehicle:
                    return rm.GetString("ThermalComfort_TaskName18");
                case Tasks.Other_Occupational_Cooking:
                    return rm.GetString("ThermalComfort_TaskName19");
                case Tasks.Other_Occupational_HouseCleaning:
                    return rm.GetString("ThermalComfort_TaskName20");
                case Tasks.Other_Occupational_Seated_HeavyLimbMovement:
                    return rm.GetString("ThermalComfort_TaskName21");
                case Tasks.Other_Occupational_MachineWork_Sawing:
                    return rm.GetString("ThermalComfort_TaskName22");
                case Tasks.Other_Occupational_MachineWork_Light:
                    return rm.GetString("ThermalComfort_TaskName23");
                case Tasks.Other_Occupational_MachineWork_Heavy:
                    return rm.GetString("ThermalComfort_TaskName24");
                case Tasks.Other_Occupational_Handling50kgBags:
                    return rm.GetString("ThermalComfort_TaskName25");
                case Tasks.Other_Occupational_PickAndShovelWork:
                    return rm.GetString("ThermalComfort_TaskName26");
                case Tasks.Other_Leisure_Dancing:
                    return rm.GetString("ThermalComfort_TaskName27");
                case Tasks.Other_Leisure_Exercise:
                    return rm.GetString("ThermalComfort_TaskName28");
                case Tasks.Other_Leisure_Tennes:
                    return rm.GetString("ThermalComfort_TaskName29");
                case Tasks.Other_Leisure_Basketball:
                    return rm.GetString("ThermalComfort_TaskName30");
                case Tasks.Other_Leisure_Wrestling:
                    return rm.GetString("ThermalComfort_TaskName31");
                default:
                    return null;
            }
        }

        #endregion

    }
}
