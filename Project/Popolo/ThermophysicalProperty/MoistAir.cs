/* MoistAir.cs
 * 
 * Copyright (C) 2007 E.Togashi
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at
 * your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
 */

using System;
using System.Runtime.Serialization;

namespace Popolo.ThermophysicalProperty
{

    /// <summary>湿り空気クラス</summary>
    /// <remarks>HVACSIM+(J)および『パソコンによる空気調和計算法（宇田川光弘）』を参照</remarks>
    [Serializable]
    public class MoistAir : ICloneable, ImmutableMoistAir
    {

        #region 定数宣言

        /// <summary>シリアライズ用バージョン情報</summary>
        private double S_VERSION = 1.3;

        /// <summary>絶対温度と摂氏との変換用定数</summary>
        private const double TCONV = 273.15d;

        /// <summary>乾き空気の定圧比熱[kJ/kg-K]</summary>
        private const double CP_AIR = 1.005d;
        
        /// <summary>水蒸気の定圧比熱[kJ/kg-K]</summary>
        private const double CP_VAPOR = 1.846d;

        /// <summary>水比熱[kJ/kg-K]</summary>
        private const double CP_WATER = 4.187d;
        
        /// <summary>水の蒸発潜熱[kJ/kg]</summary>
        private const double HFG = 2501.0d;

        /// <summary>1気圧=101.325[kPa]</summary>
        private const double ATM = 101.325d;

        /// <summary>乾き空気のガス定数[kJ/(kg K)]</summary>
        private const double GAS_CONSTANT_DRY_AIR = 0.287055;

        #endregion

        #region 列挙型定義

        /// <summary>湿り空気物性種類</summary>
        public enum Property
        {
            /// <summary>乾球温度[CDB]</summary>
            DryBulbTemperature = 0,
            /// <summary>湿球温度[CWB]</summary>
            WetBulbTemperature = 1,
            /// <summary>絶対湿度[kg/kg]</summary>
            HumidityRatio = 2,
            /// <summary>相対湿度[%]</summary>
            RelativeHumidity = 3,
            /// <summary>エンタルピー[kJ/kg]</summary>
            Enthalpy = 4,
            /// <summary>水蒸気分圧[kPa]</summary>
            WaterPartialPressure = 5,
            /// <summary>比容積[m3/kg]</summary>
            SpecificVolume = 6,
            /// <summary>飽和温度[C]</summary>
            SaturatedTemperature = 7
        }

        #endregion

        #region インスタンス変数

        /// <summary>乾球温度[℃]</summary>
        private double dryBulbTemp;

        /// <summary>湿球温度[℃]</summary>
        private double wetBulbTemp;

        /// <summary>絶対湿度[kg/kg]</summary>
        private double humidityRatio;

        /// <summary>相対湿度[%]</summary>
        private double relativeHumid;

        /// <summary>エンタルピー[kJ/kg]</summary>
        private double enthalpy;

        /// <summary>比容積[m3/kg]</summary>
        private double specificVolume;

        /// <summary>大気圧[kPa]</summary>
        private double atmosphericPressure;

        /// <summary>編集番号</summary>
        private uint revision = 0;

        /// <summary>標準空気</summary>
        private static readonly MoistAir standardAir;

        #endregion

        #region プロパティ

        /// <summary>乾き空気比熱[kJ/kg-K]を取得する</summary>
        public static double DryAirSpecificHeat
        {
            get
            {
                return CP_AIR;
            }
        }

        /// <summary>水の蒸発潜熱[kJ/kg]</summary>
        public static double LatentHeatOfVaporization
        {
            get
            {
                return HFG;
            }
        }

        /// <summary>乾球温度[C]を設定・取得する</summary>
        public double DryBulbTemperature
        {
            get
            {
                return dryBulbTemp;
            }
            set
            {
                if (dryBulbTemp != value)
                {
                    dryBulbTemp = value;
                    revision++;
                }
            }
        }

        /// <summary>湿球温度[C]を設定・取得する</summary>
        public double WetBulbTemperature
        {
            get
            {
                return wetBulbTemp;
            }
            set
            {
                if (wetBulbTemp != value)
                {
                    wetBulbTemp = value;
                    revision++;
                }
            }
        }

        /// <summary>絶対湿度[kg/kg(DA)]を設定・取得する</summary>
        public double HumidityRatio
        {
            get
            {
                return humidityRatio;
            }
            set
            {
                if (humidityRatio != value)
                {
                    humidityRatio = value;
                    revision++;
                }
            }
        }

        /// <summary>相対湿度[%]を設定・取得する</summary>
        public double RelativeHumidity
        {
            get
            {
                return relativeHumid;
            }
            set
            {
                if (relativeHumid != value)
                {
                    relativeHumid = value;
                    revision++;
                }
            }
        }

        /// <summary>エンタルピー[kJ/kg]を設定・取得する</summary>
        public double Enthalpy
        {
            get
            {
                return enthalpy;
            }
            set
            {
                if (enthalpy != value)
                {
                    enthalpy = value;
                    revision++;
                }
            }
        }

        /// <summary>比容積[m3/kg]を設定・取得する</summary>
        public double SpecificVolume
        {
            get
            {
                return specificVolume;
            }
            set
            {
                if (specificVolume != value)
                {
                    specificVolume = value;
                    revision++;
                }
            }
        }

        /// <summary>大気圧[kPa]を設定・取得する</summary>
        public double AtmosphericPressure
        {
            get
            {
                return atmosphericPressure;
            }
            set
            {
                if (atmosphericPressure != value)
                {
                    atmosphericPressure = value;
                    revision++;
                }
            }
        }

        /// <summary>編集番号を取得する</summary>
        public uint Revision
        {
            get
            {
                return revision;
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>静的コンストラクタ</summary>
        static MoistAir()
        {
            standardAir = new MoistAir();
            standardAir.AtmosphericPressure = ATM;
            standardAir.dryBulbTemp = 26;
            standardAir.relativeHumid = 60;
            standardAir.humidityRatio = fwphi(standardAir.dryBulbTemp, standardAir.relativeHumid, ATM);
            standardAir.wetBulbTemp = ftwb(standardAir.dryBulbTemp, standardAir.humidityRatio, ATM);
            standardAir.enthalpy = fhair(standardAir.dryBulbTemp, standardAir.humidityRatio);
            standardAir.specificVolume = getSpecificVolumeFromDBHR(standardAir.dryBulbTemp, standardAir.humidityRatio, ATM);
        }

        /// <summary>コンストラクタ</summary>
        public MoistAir()
        {
            //標準空気（DB26C : RH60% で初期化）
            if(standardAir != null) standardAir.CopyTo(this);
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="dryBulbTemp">乾球温度[K]</param>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        public MoistAir(double dryBulbTemp, double humidityRatio)
        {
            this.AtmosphericPressure = ATM;
            this.dryBulbTemp = dryBulbTemp;
            this.humidityRatio = humidityRatio;
            this.relativeHumid = fphi(dryBulbTemp, humidityRatio, ATM);
            this.enthalpy = fhair(dryBulbTemp, humidityRatio);
            this.wetBulbTemp = ftwb(dryBulbTemp, humidityRatio, ATM);
            this.specificVolume = getSpecificVolumeFromDBHR(dryBulbTemp, humidityRatio, ATM);
        }

        /// <summary>コピーコンストラクタ</summary>
        /// <param name="moistAir">コピーする湿り空気状態</param>
        public MoistAir(ImmutableMoistAir moistAir)
        {
            this.AtmosphericPressure = moistAir.AtmosphericPressure;
            this.dryBulbTemp = moistAir.DryBulbTemperature;
            this.humidityRatio = moistAir.HumidityRatio;
            this.relativeHumid = moistAir.RelativeHumidity;
            this.enthalpy = moistAir.Enthalpy;
            this.wetBulbTemp = moistAir.WetBulbTemperature;
            this.specificVolume = moistAir.SpecificVolume;
        }

        #endregion

        #region staticメソッド：飽和線関連

        /// <summary>飽和乾球温度[C]を求める</summary>
        /// <param name="val">基準となる物性値</param>
        /// <param name="airProperty">基準となる物性種類</param>
        /// <returns>飽和乾球温度[C]</returns>
        public static double GetSaturatedDrybulbTemperature(double val, Property airProperty)
        {
            return GetSaturatedDrybulbTemperature(val, airProperty, ATM);        
        }

        /// <summary>飽和乾球温度[C]を求める</summary>
        /// <param name="val">基準となる物性値</param>
        /// <param name="airProperty">基準となる物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>飽和乾球温度[C]</returns>
        public static double GetSaturatedDrybulbTemperature(double val, Property airProperty, double atm)
        {
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    return ftdew(val, atm);
                case Property.Enthalpy:
                    return ftsat(val, atm);
                case Property.WetBulbTemperature:
                    return val;
                case Property.DryBulbTemperature:
                    return val;
                case Property.SpecificVolume:
                    return getDryBulbTemperatureFromRHSV(100, val, atm);
                default:
                    throw new Exception("未実装");
            }
        }

        /// <summary>飽和絶対湿度[kg/kg(DA)]を求める</summary>
        /// <param name="val">基準となる物性値</param>
        /// <param name="airProperty">基準となる物性種類</param>
        /// <returns>飽和絶対湿度[kg/kg(DA)]</returns>
        public static double GetSaturatedHumidityRatio(double val, Property airProperty)
        {
            return GetSaturatedHumidityRatio(val, airProperty, ATM);
        }

        /// <summary>飽和絶対湿度[kg/kg(DA)]を求める</summary>
        /// <param name="val">基準となる物性値</param>
        /// <param name="airProperty">基準となる物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>飽和絶対湿度[kg/kg(DA)]</returns>
        public static double GetSaturatedHumidityRatio(double val, Property airProperty, double atm)
        {
            switch (airProperty)
            {
                case Property.Enthalpy:
                    double dbt1 = ftsat(val, atm);
                    return fwha(dbt1, val);
                case Property.WetBulbTemperature:
                case Property.DryBulbTemperature:
                    double ps = fpws(val);
                    return fwpw(ps, atm);
                default:
                    throw new Exception("未実装");
            }
        }

        /// <summary>飽和エンタルピー[kJ/kg]を求める</summary>
        /// <param name="val">基準となる物性値</param>
        /// <param name="airProperty">基準となる物性種類</param>
        /// <returns>飽和エンタルピー[kJ/kg]</returns>
        public static double GetSaturatedEnthalpy(double val, Property airProperty)
        {
            return GetSaturatedEnthalpy(val, airProperty, ATM);
        }

        /// <summary>飽和エンタルピー[kJ/kg]を求める</summary>
        /// <param name="val">基準となる物性値</param>
        /// <param name="airProperty">基準となる物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>飽和エンタルピー[kJ/kg]</returns>
        public static double GetSaturatedEnthalpy(double val, Property airProperty, double atm)
        {
            switch (airProperty)
            {
                case Property.Enthalpy:
                    return val;
                case Property.WetBulbTemperature:
                case Property.DryBulbTemperature:
                    return fhsat(val, atm);
                case Property.HumidityRatio:
                    double dbt = ftdew(val, atm);
                    return fhsat(dbt, atm);
                default:
                    throw new Exception("未実装");
            }
        }

        #endregion

        #region staticメソッド：空気状態関連

        #region 一般の空気状態

        /// <summary>2種の空気状態をもとに空気状態を特定する</summary>
        /// <param name="valueA">空気状態値1</param>
        /// <param name="valueB">空気状態値2</param>
        /// <param name="propertyA">空気状態種類1</param>
        /// <param name="propertyB">空気状態種類1</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirState(double valueA, double valueB, Property propertyA, Property propertyB)
        {
            return GetAirState(valueA, valueB, propertyA, propertyB, ATM);
        }

        /// <summary>2種の空気状態をもとに空気状態を特定する</summary>
        /// <param name="valueA">空気状態値1</param>
        /// <param name="valueB">空気状態値2</param>
        /// <param name="propertyA">空気状態種類1</param>
        /// <param name="propertyB">空気状態種類1</param>
        /// <param name="atm">気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirState(double valueA, double valueB, Property propertyA, Property propertyB, double atm)
        {
            switch (propertyA)
            {
                case Property.DryBulbTemperature:
                    switch (propertyB)
                    {
                        case Property.DryBulbTemperature:
                            throw new Exception("物性指定エラー");
                        case Property.WetBulbTemperature:
                            return GetAirStateFromDBWB(valueA, valueB, atm);
                        case Property.HumidityRatio:
                            return GetAirStateFromDBHR(valueA, valueB, atm);
                        case Property.RelativeHumidity:
                            return GetAirStateFromDBRH(valueA, valueB, atm);
                        case Property.Enthalpy:
                            return GetAirStateFromDBEN(valueA, valueB, atm);
                        default:
                            throw new Exception("物性指定エラー");
                    }
                case Property.WetBulbTemperature:
                    switch (propertyB)
                    {
                        case Property.DryBulbTemperature:
                            return GetAirStateFromDBWB(valueB, valueA, atm);
                        case Property.WetBulbTemperature:
                            throw new Exception("物性指定エラー");
                        case Property.HumidityRatio:
                            return GetAirStateFromWBHR(valueA, valueB, atm);
                        case Property.RelativeHumidity:
                            return GetAirStateFromWBRH(valueA, valueB, atm);
                        case Property.Enthalpy:
                            return GetAirStateFromWBEN(valueA, valueB, atm);
                        default:
                            throw new Exception("物性指定エラー");
                    }
                case Property.HumidityRatio:
                    switch (propertyB)
                    {
                        case Property.DryBulbTemperature:
                            return GetAirStateFromDBHR(valueB, valueA, atm);
                        case Property.WetBulbTemperature:
                            return GetAirStateFromWBHR(valueB, valueA, atm);
                        case Property.HumidityRatio:
                            throw new Exception("物性指定エラー");
                        case Property.RelativeHumidity:
                            return GetAirStateFromHRRH(valueA, valueB, atm);
                        case Property.Enthalpy:
                            return GetAirStateFromHREN(valueA, valueB, atm);
                        default:
                            throw new Exception("物性指定エラー");
                    }
                case Property.RelativeHumidity:
                    switch (propertyB)
                    {
                        case Property.DryBulbTemperature:
                            return GetAirStateFromDBRH(valueB, valueA, atm);
                        case Property.WetBulbTemperature:
                            return GetAirStateFromWBRH(valueB, valueA, atm);
                        case Property.HumidityRatio:
                            return GetAirStateFromHRRH(valueB, valueA, atm);
                        case Property.RelativeHumidity:
                            throw new Exception("物性指定エラー");
                        case Property.Enthalpy:
                            return GetAirStateFromRHEN(valueA, valueB, atm);
                        default:
                            throw new Exception("物性指定エラー");
                    }
                case Property.Enthalpy:
                    switch (propertyB)
                    {
                        case Property.DryBulbTemperature:
                            return GetAirStateFromDBEN(valueB, valueA, atm);
                        case Property.WetBulbTemperature:
                            return GetAirStateFromWBEN(valueB, valueA, atm);
                        case Property.HumidityRatio:
                            return GetAirStateFromHREN(valueB, valueA, atm);
                        case Property.RelativeHumidity:
                            return GetAirStateFromRHEN(valueB, valueA, atm);
                        case Property.Enthalpy:
                            throw new Exception("物性指定エラー");
                        default:
                            throw new Exception("物性指定エラー");
                    }
                default:
                    throw new Exception("物性指定エラー");
            }
        }

        #endregion

        #region 乾球温度に基づく計算

        /// <summary>乾球温度[C]および湿球温度[C]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromDBWB(double dryBulbTemp, double wetBulbTemp)
        {
            return GetAirStateFromDBWB(dryBulbTemp, wetBulbTemp, ATM);
        }

        /// <summary>乾球温度[C]および湿球温度[C]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromDBWB(double dryBulbTemp, double wetBulbTemp, double atm)
        {
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.DryBulbTemperature = dryBulbTemp;
            mAir.WetBulbTemperature = wetBulbTemp;
            mAir.HumidityRatio = fwtwb(dryBulbTemp, wetBulbTemp, atm);
            mAir.Enthalpy = fhair(dryBulbTemp, mAir.HumidityRatio);
            mAir.RelativeHumidity = fphi(dryBulbTemp, mAir.HumidityRatio, atm);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(dryBulbTemp, mAir.HumidityRatio, atm);
            return mAir;
        }

        /// <summary>乾球温度[C]および湿球温度[C]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromDBWB(double dryBulbTemp, double wetBulbTemp, Property airProperty)
        {
            return GetAirStateFromDBWB(dryBulbTemp, wetBulbTemp, airProperty, ATM);
        }

        /// <summary>乾球温度[C]および湿球温度[C]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromDBWB(double dryBulbTemp, double wetBulbTemp, Property airProperty, double atm)
        {
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    return fwtwb(dryBulbTemp, wetBulbTemp, atm);
                case Property.Enthalpy:
                    return fhair(dryBulbTemp, fwtwb(dryBulbTemp, wetBulbTemp, atm));
                case Property.WetBulbTemperature:
                    return wetBulbTemp;
                case Property.DryBulbTemperature:
                    return dryBulbTemp;
                case Property.RelativeHumidity:
                    return fphi(dryBulbTemp, fwtwb(dryBulbTemp, wetBulbTemp, atm), atm);
                case Property.SpecificVolume:
                    double aHumid = fwtwb(dryBulbTemp, wetBulbTemp, atm);
                    return getSpecificVolumeFromDBHR(dryBulbTemp, aHumid, atm);
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        /// <summary>乾球温度[C]および絶対湿度[kg/kg]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromDBHR(double dryBulbTemp, double humidityRatio)
        {
            return GetAirStateFromDBHR(dryBulbTemp, humidityRatio, ATM);
        }

        /// <summary>乾球温度[C]および絶対湿度[kg/kg]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromDBHR(double dryBulbTemp, double humidityRatio, double atm)
        {
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.DryBulbTemperature = dryBulbTemp;
            mAir.HumidityRatio = humidityRatio;
            mAir.WetBulbTemperature = ftwb(dryBulbTemp, humidityRatio, atm);
            mAir.Enthalpy = fhair(dryBulbTemp, humidityRatio);
            mAir.RelativeHumidity = fphi(dryBulbTemp, humidityRatio, atm);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(dryBulbTemp, humidityRatio, atm);
            return mAir;
        }

        /// <summary>乾球温度[C]および絶対湿度[kg/kg]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromDBHR(double dryBulbTemp, double humidityRatio, Property airProperty)
        {
            return GetAirStateFromDBHR(dryBulbTemp, humidityRatio, airProperty, ATM);
        }

        /// <summary>乾球温度[C]および絶対湿度[kg/kg]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromDBHR(double dryBulbTemp, double humidityRatio, Property airProperty, double atm)
        {
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    return humidityRatio;
                case Property.Enthalpy:
                    return fhair(dryBulbTemp, humidityRatio);
                case Property.WetBulbTemperature:
                    return ftwb(dryBulbTemp, humidityRatio, atm);
                case Property.DryBulbTemperature:
                    return dryBulbTemp;
                case Property.RelativeHumidity:
                    return fphi(dryBulbTemp, humidityRatio, atm);
                case Property.SpecificVolume:
                    return getSpecificVolumeFromDBHR(dryBulbTemp, humidityRatio, atm);
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        /// <summary>乾球温度[C]および相対湿度[%]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromDBRH(double dryBulbTemp, double relativeHumid)
        {
            return GetAirStateFromDBRH(dryBulbTemp, relativeHumid, ATM);
        }

        /// <summary>乾球温度[C]および相対湿度[%]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromDBRH(double dryBulbTemp, double relativeHumid, double atm)
        {
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.DryBulbTemperature = dryBulbTemp;
            mAir.RelativeHumidity = relativeHumid;
            mAir.HumidityRatio = fwphi(dryBulbTemp, relativeHumid, atm);
            mAir.WetBulbTemperature = ftwb(dryBulbTemp, mAir.HumidityRatio, atm);
            mAir.Enthalpy = fhair(dryBulbTemp, mAir.HumidityRatio);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(dryBulbTemp, mAir.HumidityRatio, atm);
            return mAir;
        }

        /// <summary>乾球温度[C]および相対湿度[%]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromDBRH(double dryBulbTemp, double relativeHumid, Property airProperty)
        {
            return GetAirStateFromDBRH(dryBulbTemp, relativeHumid, airProperty, ATM);
        }

        /// <summary>乾球温度[C]および相対湿度[%]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromDBRH(double dryBulbTemp, double relativeHumid, Property airProperty, double atm)
        {
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    return fwphi(dryBulbTemp, relativeHumid, atm);
                case Property.Enthalpy:
                    return fhair(dryBulbTemp, fwphi(dryBulbTemp, relativeHumid, atm));
                case Property.WetBulbTemperature:
                    return ftwb(dryBulbTemp, fwphi(dryBulbTemp, relativeHumid, atm), atm);
                case Property.DryBulbTemperature:
                    return dryBulbTemp;
                case Property.RelativeHumidity:
                    return relativeHumid;
                case Property.SpecificVolume:
                    double aHumid = fwphi(dryBulbTemp, relativeHumid, atm);
                    return getSpecificVolumeFromDBHR(dryBulbTemp, aHumid, atm);
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        /// <summary>乾球温度[C]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromDBEN(double dryBulbTemp, double enthalpy)
        {
            return GetAirStateFromDBEN(dryBulbTemp, enthalpy, ATM);
        }

        /// <summary>乾球温度[C]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromDBEN(double dryBulbTemp, double enthalpy, double atm)
        {
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.DryBulbTemperature = dryBulbTemp;
            mAir.Enthalpy = enthalpy;
            mAir.HumidityRatio = fwha(dryBulbTemp, enthalpy);
            mAir.WetBulbTemperature = ftwb(dryBulbTemp, mAir.HumidityRatio, atm);
            mAir.RelativeHumidity = fphi(dryBulbTemp, mAir.HumidityRatio, atm);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(dryBulbTemp, mAir.HumidityRatio, atm);
            return mAir;
        }

        /// <summary>乾球温度[C]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromDBEN(double dryBulbTemp, double enthalpy, Property airProperty)
        {
            return GetAirStateFromDBEN(dryBulbTemp, enthalpy, airProperty, ATM);
        }

        /// <summary>乾球温度[C]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromDBEN(double dryBulbTemp, double enthalpy, Property airProperty, double atm)
        {
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    return fwha(dryBulbTemp, enthalpy);
                case Property.Enthalpy:
                    return enthalpy;
                case Property.WetBulbTemperature:
                    return ftwb(dryBulbTemp, fwha(dryBulbTemp, enthalpy), atm);
                case Property.DryBulbTemperature:
                    return dryBulbTemp;
                case Property.RelativeHumidity:
                    return fphi(dryBulbTemp, fwha(dryBulbTemp, enthalpy), atm);
                case Property.SpecificVolume:
                    double aHumid = fwha(dryBulbTemp, enthalpy);
                    return getSpecificVolumeFromDBHR(dryBulbTemp, aHumid, atm);
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        /// <summary>乾球温度[C]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromDBSV(double dryBulbTemp, double specificVolume)
        {
            return GetAirStateFromDBSV(dryBulbTemp, specificVolume, ATM);
        }

        /// <summary>乾球温度[C]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromDBSV(double dryBulbTemp, double specificVolume, double atm)
        {
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.DryBulbTemperature = dryBulbTemp;
            mAir.HumidityRatio = getHumidityRatioFromDBSV(dryBulbTemp, specificVolume, atm);
            mAir.Enthalpy = fhair(dryBulbTemp, mAir.HumidityRatio);
            mAir.WetBulbTemperature = ftwb(dryBulbTemp, mAir.HumidityRatio, atm);
            mAir.RelativeHumidity = fphi(dryBulbTemp, mAir.HumidityRatio, atm);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(dryBulbTemp, mAir.HumidityRatio, atm);
            return mAir;
        }

        /// <summary>乾球温度[C]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromDBSV(double dryBulbTemp, double specificVolume, Property airProperty)
        {
            return GetAirStateFromDBSV(dryBulbTemp, specificVolume, airProperty, ATM);
        }

        /// <summary>乾球温度[C]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="dryBulbTemp">乾球温度[C]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromDBSV(double dryBulbTemp, double specificVolume, Property airProperty, double atm)
        {
            double ahmd;
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    return getHumidityRatioFromDBSV(dryBulbTemp, specificVolume, atm);
                case Property.Enthalpy:
                    ahmd = getHumidityRatioFromDBSV(dryBulbTemp, specificVolume, atm);
                    return fhair(dryBulbTemp, ahmd);
                case Property.WetBulbTemperature:
                    ahmd = getHumidityRatioFromDBSV(dryBulbTemp, specificVolume, atm);
                    return ftwb(dryBulbTemp, ahmd, atm);
                case Property.DryBulbTemperature:
                    return dryBulbTemp;
                case Property.RelativeHumidity:
                    ahmd = getHumidityRatioFromDBSV(dryBulbTemp, specificVolume, atm);
                    return fphi(dryBulbTemp, ahmd, atm);
                case Property.SpecificVolume:
                    return specificVolume;
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        #endregion

        #region 湿球温度に基づく計算

        /// <summary>湿球温度[C]および絶対湿度[kg/kg]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromWBHR(double wetBulbTemp, double humidityRatio)
        {
            return GetAirStateFromWBHR(wetBulbTemp, humidityRatio, ATM);
        }

        /// <summary>湿球温度[C]および絶対湿度[kg/kg]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromWBHR(double wetBulbTemp, double humidityRatio, double atm)
        {
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.HumidityRatio = humidityRatio;
            mAir.WetBulbTemperature = wetBulbTemp;
            mAir.DryBulbTemperature = fwwbdb(humidityRatio, wetBulbTemp, atm);
            mAir.Enthalpy = fhair(mAir.DryBulbTemperature, humidityRatio);
            mAir.RelativeHumidity = fphi(mAir.DryBulbTemperature, humidityRatio, atm);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(mAir.DryBulbTemperature, mAir.HumidityRatio, atm);
            return mAir;
        }

        /// <summary>湿球温度[C]および絶対湿度[kg/kg]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromWBHR(double wetBulbTemp, double humidityRatio, Property airProperty)
        {
            return GetAirStateFromWBHR(wetBulbTemp, humidityRatio, airProperty, ATM);
        }

        /// <summary>湿球温度[C]および絶対湿度[kg/kg]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromWBHR(double wetBulbTemp, double humidityRatio, Property airProperty, double atm)
        {
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    return humidityRatio;
                case Property.Enthalpy:
                    return fhair(fwwbdb(humidityRatio, wetBulbTemp, atm), humidityRatio);
                case Property.WetBulbTemperature:
                    return wetBulbTemp;
                case Property.DryBulbTemperature:
                    return fwwbdb(humidityRatio, wetBulbTemp, atm);
                case Property.RelativeHumidity:
                    return fphi(fwwbdb(humidityRatio, wetBulbTemp, atm), humidityRatio, atm);
                case Property.SpecificVolume:
                    double dbTemp = fwwbdb(humidityRatio, wetBulbTemp, atm);
                    return getSpecificVolumeFromDBHR(dbTemp, humidityRatio, atm);
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        /// <summary>湿球温度[C]および相対湿度[%]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromWBRH(double wetBulbTemp, double relativeHumid)
        {
            return GetAirStateFromWBRH(wetBulbTemp, relativeHumid, ATM);
        }

        /// <summary>湿球温度[C]および相対湿度[%]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromWBRH(double wetBulbTemp, double relativeHumid, double atm)
        {
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.WetBulbTemperature = wetBulbTemp;
            mAir.RelativeHumidity = relativeHumid;
            mAir.DryBulbTemperature = fndbrw(relativeHumid, wetBulbTemp, atm);
            mAir.HumidityRatio = fwphi(mAir.DryBulbTemperature, relativeHumid, atm);
            mAir.Enthalpy = fhair(mAir.DryBulbTemperature, mAir.HumidityRatio);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(mAir.DryBulbTemperature, mAir.HumidityRatio, atm);
            return mAir;
        }

        /// <summary>湿球温度[C]および相対湿度[%]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromWBRH(double wetBulbTemp, double relativeHumid, Property airProperty)
        {
            return GetAirStateFromWBRH(wetBulbTemp, relativeHumid, airProperty, ATM);
        }

        /// <summary>湿球温度[C]および相対湿度[%]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromWBRH(double wetBulbTemp, double relativeHumid, Property airProperty, double atm)
        {
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    return fwphi(fndbrw(relativeHumid, wetBulbTemp, atm), relativeHumid, atm);
                case Property.Enthalpy:
                    double dbt = fndbrw(relativeHumid, wetBulbTemp, atm);
                    return fhair(dbt, fwphi(dbt, relativeHumid, atm));
                case Property.WetBulbTemperature:
                    return wetBulbTemp;
                case Property.DryBulbTemperature:
                    return fndbrw(relativeHumid, wetBulbTemp, atm);
                case Property.RelativeHumidity:
                    return relativeHumid;
                case Property.SpecificVolume:
                    double dbTemp = fndbrw(relativeHumid, wetBulbTemp, atm);
                    double ahd = fwphi(dbTemp, relativeHumid, atm);
                    return getSpecificVolumeFromDBHR(dbTemp, ahd, atm);
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        /// <summary>湿球温度[C]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromWBEN(double wetBulbTemp, double enthalpy)
        {
            return GetAirStateFromWBEN(wetBulbTemp, enthalpy, ATM);
        }

        /// <summary>湿球温度[C]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromWBEN(double wetBulbTemp, double enthalpy, double atm)
        {
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.WetBulbTemperature = wetBulbTemp;
            mAir.Enthalpy = enthalpy;
            mAir.DryBulbTemperature = fndbhw(enthalpy, wetBulbTemp, atm);
            mAir.HumidityRatio = fwha(mAir.DryBulbTemperature, enthalpy);
            mAir.RelativeHumidity = fphi(mAir.DryBulbTemperature, mAir.HumidityRatio, atm);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(mAir.DryBulbTemperature, mAir.HumidityRatio, atm);
            return mAir;
        }

        /// <summary>湿球温度[C]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromWBEN(double wetBulbTemp, double enthalpy, Property airProperty)
        {
            return GetAirStateFromWBEN(wetBulbTemp, enthalpy, airProperty, ATM);
        }

        /// <summary>湿球温度[C]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromWBEN(double wetBulbTemp, double enthalpy, Property airProperty, double atm)
        {
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    return fwha(fndbhw(enthalpy, wetBulbTemp, atm), enthalpy);
                case Property.Enthalpy:
                    return enthalpy;
                case Property.WetBulbTemperature:
                    return wetBulbTemp;
                case Property.DryBulbTemperature:
                    return fndbhw(enthalpy, wetBulbTemp, atm);
                case Property.RelativeHumidity:
                    double dbt = fndbhw(enthalpy, wetBulbTemp, atm);
                    return fphi(dbt, fwha(dbt, enthalpy), atm);
                case Property.SpecificVolume:
                    double dbTemp = fndbhw(enthalpy, wetBulbTemp, atm);
                    double aHumid = fwha(dbTemp, enthalpy);
                    return getSpecificVolumeFromDBHR(dbTemp, aHumid, atm);
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        /// <summary>湿球温度[C]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromWBSV(double wetBulbTemp, double specificVolume)
        {
            return GetAirStateFromWBSV(wetBulbTemp, specificVolume);
        }

        /// <summary>湿球温度[C]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromWBSV(double wetBulbTemp, double specificVolume, double atm)
        {
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.WetBulbTemperature = wetBulbTemp;
            mAir.DryBulbTemperature = getDryBulbTemperatureFromWBSV(wetBulbTemp, specificVolume, atm);
            mAir.HumidityRatio = fwtwb(mAir.DryBulbTemperature, wetBulbTemp, atm);
            mAir.Enthalpy = fhair(mAir.DryBulbTemperature, mAir.HumidityRatio);
            mAir.RelativeHumidity = fphi(mAir.DryBulbTemperature, mAir.HumidityRatio, atm);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(mAir.DryBulbTemperature, mAir.HumidityRatio, atm);
            return mAir;
        }

        /// <summary>湿球温度[C]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromWBSV(double wetBulbTemp, double specificVolume, Property airProperty)
        {
            return GetAirStateFromWBSV(wetBulbTemp, specificVolume, airProperty, ATM);
        }

        /// <summary>湿球温度[C]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromWBSV(double wetBulbTemp, double specificVolume, Property airProperty, double atm)
        {
            double dbTemp;
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    dbTemp = getDryBulbTemperatureFromWBSV(wetBulbTemp, specificVolume, atm);
                    return fwtwb(dbTemp, wetBulbTemp, atm);
                case Property.Enthalpy:
                    dbTemp = getDryBulbTemperatureFromWBSV(wetBulbTemp, specificVolume, atm);
                    return fhair(dbTemp, fwtwb(dbTemp, wetBulbTemp, atm));
                case Property.WetBulbTemperature:
                    return wetBulbTemp;
                case Property.DryBulbTemperature:
                    return getDryBulbTemperatureFromWBSV(wetBulbTemp, specificVolume, atm);
                case Property.RelativeHumidity:
                    dbTemp = getDryBulbTemperatureFromWBSV(wetBulbTemp, specificVolume, atm);
                    return fphi(dbTemp, fwtwb(dbTemp, wetBulbTemp, atm), atm);
                case Property.SpecificVolume:
                    return specificVolume;
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        #endregion

        #region 絶対湿度に基づく計算

        /// <summary>絶対湿度[kg/kg]および相対湿度[%]から空気状態を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromHRRH(double humidityRatio, double relativeHumid)
        {
            return GetAirStateFromHRRH(humidityRatio, relativeHumid, ATM);
        }

        /// <summary>絶対湿度[kg/kg]および相対湿度[%]から空気状態を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromHRRH(double humidityRatio, double relativeHumid, double atm)
        {
            if (relativeHumid == 0.0)
            {
                //国際化のためのリソースを取得
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                System.Resources.ResourceManager rm = new System.Resources.ResourceManager("Popolo.Utility.Properties.Resources", asm);
                throw new InputValueOutOfRangeException(rm.GetString("MoistAIr_RelativeHumidity_Error1"), 100.0, 0.0, 0.0);
            }
                
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.HumidityRatio = humidityRatio;
            mAir.RelativeHumidity = relativeHumid;
            double ps = fpww(humidityRatio, atm) / relativeHumid * 100;
            mAir.DryBulbTemperature = ftpws(ps);
            mAir.WetBulbTemperature = ftwb(mAir.DryBulbTemperature, humidityRatio, atm);
            mAir.Enthalpy = fhair(mAir.DryBulbTemperature, humidityRatio);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(mAir.DryBulbTemperature, mAir.HumidityRatio, atm);
            return mAir;
        }

        /// <summary>絶対湿度[kg/kg]および相対湿度[%]から空気状態を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromHRRH(double humidityRatio, double relativeHumid, Property airProperty)
        {
            return GetAirStateFromHRRH(humidityRatio, relativeHumid, airProperty, ATM);
        }

        /// <summary>絶対湿度[kg/kg]および相対湿度[%]から空気状態を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromHRRH(double humidityRatio, double relativeHumid, Property airProperty, double atm)
        {
            double ps;
            if (relativeHumid == 0.0)
            {
                //国際化のためのリソースを取得
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                System.Resources.ResourceManager rm = new System.Resources.ResourceManager("Popolo.Utility.Properties.Resources", asm);
                throw new InputValueOutOfRangeException(rm.GetString("MoistAIr_RelativeHumidity_Error1"), 100.0, 0.0, 0.0);
            }
                switch (airProperty)
            {
                case Property.HumidityRatio:
                    return humidityRatio;
                case Property.Enthalpy:
                    ps = fpww(humidityRatio, atm) / relativeHumid * 100;
                    return fhair(ftpws(ps), humidityRatio);
                case Property.WetBulbTemperature:
                    ps = fpww(humidityRatio, atm) / relativeHumid * 100;
                    return ftwb(ftpws(ps), humidityRatio, atm);
                case Property.DryBulbTemperature:
                    ps = fpww(humidityRatio, atm) / relativeHumid * 100;
                    return ftpws(ps);
                case Property.RelativeHumidity:
                    return relativeHumid;
                case Property.SpecificVolume:
                    ps = fpww(humidityRatio, atm) / relativeHumid * 100;
                    return getSpecificVolumeFromDBHR(ftpws(ps), humidityRatio, atm);
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        /// <summary>絶対湿度[kg/kg]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromHREN(double humidityRatio, double enthalpy)
        {
            return GetAirStateFromHREN(humidityRatio, enthalpy, ATM);
        }

        /// <summary>絶対湿度[kg/kg]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromHREN(double humidityRatio, double enthalpy, double atm)
        {
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.HumidityRatio = humidityRatio;
            mAir.Enthalpy = enthalpy;
            mAir.DryBulbTemperature = ftdb(humidityRatio, enthalpy);
            mAir.WetBulbTemperature = ftwb(mAir.DryBulbTemperature, humidityRatio, atm);
            mAir.RelativeHumidity = fphi(mAir.DryBulbTemperature, humidityRatio, atm);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(mAir.DryBulbTemperature, mAir.HumidityRatio, atm);
            return mAir;
        }

        /// <summary>絶対湿度[kg/kg]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromHREN(double humidityRatio, double enthalpy, Property airProperty)
        {
            return GetAirStateFromHREN(humidityRatio, enthalpy, airProperty, ATM);
        }

        /// <summary>絶対湿度[kg/kg]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromHREN(double humidityRatio, double enthalpy, Property airProperty, double atm)
        {
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    return humidityRatio;
                case Property.Enthalpy:
                    return enthalpy;
                case Property.WetBulbTemperature:
                    return ftwb(ftdb(humidityRatio, enthalpy), humidityRatio, atm);
                case Property.DryBulbTemperature:
                    return ftdb(humidityRatio, enthalpy);
                case Property.RelativeHumidity:
                    return fphi(ftdb(humidityRatio, enthalpy), humidityRatio, atm);
                case Property.SpecificVolume:
                    double dbTemp = ftdb(humidityRatio, enthalpy);
                    return getSpecificVolumeFromDBHR(dbTemp, humidityRatio, atm);
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        /// <summary>絶対湿度[kg/kg]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromHRSV(double humidityRatio, double specificVolume)
        {
            return GetAirStateFromHRSV(humidityRatio, specificVolume);
        }

        /// <summary>絶対湿度[kg/kg]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromHRSV(double humidityRatio, double specificVolume, double atm)
        {
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.HumidityRatio = humidityRatio;
            mAir.DryBulbTemperature = getDryBulbTemperatureFromSVHR(specificVolume, humidityRatio, atm);
            mAir.Enthalpy = fhair(mAir.DryBulbTemperature, humidityRatio);
            mAir.WetBulbTemperature = ftwb(mAir.DryBulbTemperature, humidityRatio, atm);
            mAir.RelativeHumidity = fphi(mAir.DryBulbTemperature, humidityRatio, atm);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(mAir.DryBulbTemperature, mAir.HumidityRatio, atm);
            return mAir;
        }

        /// <summary>絶対湿度[kg/kg]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromHRSV(double humidityRatio, double specificVolume, Property airProperty)
        {
            return GetAirStateFromHRSV(humidityRatio, specificVolume, airProperty, ATM);
        }

        /// <summary>絶対湿度[kg/kg]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromHRSV(double humidityRatio, double specificVolume, Property airProperty, double atm)
        {
            double dbTemp;
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    return humidityRatio;
                case Property.Enthalpy:
                    dbTemp = getDryBulbTemperatureFromSVHR(specificVolume, humidityRatio, atm);
                    return fhair(dbTemp, humidityRatio);
                case Property.WetBulbTemperature:
                    dbTemp = getDryBulbTemperatureFromSVHR(specificVolume, humidityRatio, atm);
                    return ftwb(dbTemp, humidityRatio, atm);
                case Property.DryBulbTemperature:
                    return getDryBulbTemperatureFromSVHR(specificVolume, humidityRatio, atm);
                case Property.RelativeHumidity:
                    dbTemp = getDryBulbTemperatureFromSVHR(specificVolume, humidityRatio, atm);
                    return fphi(dbTemp, humidityRatio, atm);
                case Property.SpecificVolume:
                    return specificVolume;
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        #endregion

        #region 相対湿度に基づく計算

        /// <summary>相対湿度[%]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromRHEN(double relativeHumid, double enthalpy)
        {
            return GetAirStateFromRHEN(relativeHumid, enthalpy, ATM);
        }

        /// <summary>相対湿度[%]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromRHEN(double relativeHumid, double enthalpy, double atm)
        {
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.Enthalpy = enthalpy;
            mAir.RelativeHumidity = relativeHumid;
            mAir.DryBulbTemperature = fndbrh(relativeHumid, enthalpy, atm);
            mAir.HumidityRatio = fwha(mAir.DryBulbTemperature, enthalpy);
            mAir.WetBulbTemperature = ftwb(mAir.DryBulbTemperature, mAir.HumidityRatio, atm);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(mAir.DryBulbTemperature, mAir.HumidityRatio, atm);
            return mAir;
        }

        /// <summary>相対湿度[%]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromRHEN(double relativeHumid, double enthalpy, Property airProperty)
        {
            return GetAirStateFromRHEN(relativeHumid, enthalpy, airProperty, ATM);
        }

        /// <summary>相対湿度[%]およびエンタルピー[kJ/kg]から空気状態を計算する</summary>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromRHEN(double relativeHumid, double enthalpy, Property airProperty, double atm)
        {
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    return fwha(fndbrh(relativeHumid, enthalpy, atm), enthalpy);
                case Property.Enthalpy:
                    return enthalpy;
                case Property.WetBulbTemperature:
                    double dbt = fndbrh(relativeHumid, enthalpy, atm);
                    return ftwb(dbt, fwha(dbt, enthalpy), atm);
                case Property.DryBulbTemperature:
                    return fndbrh(relativeHumid, enthalpy, atm);
                case Property.RelativeHumidity:
                    return relativeHumid;
                case Property.SpecificVolume:
                    double dbTemp = fndbrh(relativeHumid, enthalpy, atm);
                    double aHumid = fwha(dbTemp, enthalpy);
                    return getSpecificVolumeFromDBHR(dbTemp, aHumid, atm);
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        /// <summary>相対湿度[%]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromRHSV(double relativeHumid, double specificVolume)
        {
            return GetAirStateFromRHSV(relativeHumid, specificVolume, ATM);
        }

        /// <summary>相対湿度[%]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromRHSV(double relativeHumid, double specificVolume, double atm)
        {
            MoistAir mAir = new MoistAir();
            mAir.AtmosphericPressure = atm;
            mAir.RelativeHumidity = relativeHumid;
            mAir.DryBulbTemperature = getDryBulbTemperatureFromRHSV(relativeHumid, specificVolume, atm);
            mAir.HumidityRatio = fwphi(mAir.DryBulbTemperature, relativeHumid, atm);
            mAir.WetBulbTemperature = ftwb(mAir.DryBulbTemperature, mAir.HumidityRatio, atm);
            mAir.Enthalpy = fhair(mAir.DryBulbTemperature, mAir.HumidityRatio);
            mAir.SpecificVolume = getSpecificVolumeFromDBHR(mAir.DryBulbTemperature, mAir.HumidityRatio, atm);
            return mAir;
        }

        /// <summary>相対湿度[%]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromRHSV(double relativeHumid, double specificVolume, Property airProperty)
        {
            return GetAirStateFromRHSV(relativeHumid, specificVolume, airProperty, ATM);
        }

        /// <summary>相対湿度[%]および比容積[m3/kg]から空気状態を計算する</summary>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="airProperty">計算する物性種類</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>空気状態</returns>
        public static double GetAirStateFromRHSV(double relativeHumid, double specificVolume, Property airProperty, double atm)
        {
            double dbTemp;
            switch (airProperty)
            {
                case Property.HumidityRatio:
                    dbTemp = getDryBulbTemperatureFromRHSV(relativeHumid, specificVolume, atm);
                    return fwphi(dbTemp, relativeHumid, atm);
                case Property.Enthalpy:
                    dbTemp = getDryBulbTemperatureFromRHSV(relativeHumid, specificVolume, atm);
                    return fhair(dbTemp, fwphi(dbTemp, relativeHumid, atm));
                case Property.WetBulbTemperature:
                    dbTemp = getDryBulbTemperatureFromRHSV(relativeHumid, specificVolume, atm);
                    return ftwb(dbTemp, fwphi(dbTemp, relativeHumid, atm), atm);
                case Property.DryBulbTemperature:
                    return getDryBulbTemperatureFromRHSV(relativeHumid, specificVolume, atm);
                case Property.RelativeHumidity:
                    return relativeHumid;
                case Property.SpecificVolume:
                    return specificVolume;
                default:
                    throw new Exception("物性種類エラー");
            }
        }

        #endregion

        #endregion

        #region staticメソッド：その他

        /// <summary>海抜[m]に応じた大気圧[kPa]を取得する</summary>
        /// <param name="altitude">海抜[m]</param>
        /// <returns>大気圧[kPa]</returns>
        public static double GetAtmosphericPressure(double altitude)
        {
            return ATM * Math.Pow(1d - 2.2558e-5 * altitude, 5.256);
        }

        /// <summary>乾球温度[℃]から飽和水蒸気分圧[kPa]を求める</summary>
        /// <param name="dryBulbTemperature">乾球温度[℃]</param>
        /// <returns>飽和水蒸気分圧[kPa]</returns>
        /// <remarks>Wexler-Hylandによる式</remarks>
        public static double GetSaturatedVaporPressure(double dryBulbTemperature)
        {
            return fpws(dryBulbTemperature);
        }

        /// <summary>湿り空気比熱[kJ/kg-K]を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg(DA)]</param>
        /// <returns>湿り空気比熱[kJ/kg-K]</returns>
        public static double GetSpecificHeat(double humidityRatio)
        {
            return CP_AIR + CP_VAPOR * humidityRatio;
        }

        /// <summary>水蒸気圧[kPa]を取得する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg(DA)]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>水蒸気圧[kPa]</returns>
        public static double GetWaterVaporPressure(double humidityRatio, double atm)
        {
            return (humidityRatio * atm) / (humidityRatio + 0.62198);
        }

        /// <summary>水蒸気圧[kPa]を取得する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg(DA)]</param>
        /// <returns>水蒸気圧[kPa]</returns>
        public static double GetWaterVaporPressure(double humidityRatio)
        {
            return GetWaterVaporPressure(humidityRatio, ATM);
        }

        /// <summary>動粘性係数[m2/s]を計算する</summary>
        /// <param name="drybulbTemperature">乾球温度[C]</param>
        /// <returns>動粘性係数[m2/s]</returns>
        public static double GetDynamicViscosity(double drybulbTemperature)
        {
            return (0.0074237 / (drybulbTemperature + 390.15)) * Math.Pow((drybulbTemperature + TCONV) / 293.15, 1.5) / (1.293 / (1 + drybulbTemperature / TCONV));
        }

        /// <summary>熱伝導率[W/(mK)]を計算する</summary>
        /// <param name="drybulbTemperature">乾球温度[C]</param>
        /// <returns>熱伝導率[W/(mK)]</returns>
        public static double GetThermalConductivity(double drybulbTemperature)
        {
            return 0.0241 + 0.000077 * drybulbTemperature;
        }

        #endregion

        #region 空気混合処理

        /// <summary>空気を混合する</summary>
        /// <param name="air">混合する空気の配列</param>
        /// <param name="rate">混合空気の割合</param>
        /// <returns>混合済み空気</returns>
        public static MoistAir BlendAir(ImmutableMoistAir[] air, double[] rate)
        {
            int airNum = air.Length;
            if (airNum != rate.Length) throw new Exception("MoistAir Class: BlendAir: 混合する空気の数と割合の数が一致しません");
            double rSum = 0.0d;
            double tSum = 0.0d;
            double trSum = 0.0d;
            double hSum = 0.0d;
            double hrSum = 0.0d;
            for (int i = 0; i < airNum; i++)
            {
                //if (air[i].DryBulbTemperature < -30.0d) air[i].DryBulbTemperature = -30.0d;
                //if (air[i].DryBulbTemperature > 65.0d) air[i].DryBulbTemperature = 65.0d;   //←もう少し上限はあるかも。要確認
                //if (air[i].HumidityRatio < 0.0d) air[i].HumidityRatio = 0.0d;
                //割合を積算
                if (rate[i] < 0) throw new Exception("湿空気の混合比率が0以下に設定されています");
                rSum += rate[i];
                //温度[K]を積算
                tSum += air[i].DryBulbTemperature;
                //温度[K]×割合を積算
                trSum += air[i].DryBulbTemperature * rate[i];
                //絶対湿度[kg/kg]を積算
                hSum += air[i].HumidityRatio;
                //絶対湿度[kg/kg]×割合を積算
                hrSum += air[i].HumidityRatio * rate[i];
            }
            //混合後の乾球温度および絶対湿度を計算
            double drybulbTempOut;
            double absHumidOut;
            if (rSum >= 1.0e-5d)
            {
                drybulbTempOut = trSum / rSum;
                absHumidOut = hrSum / rSum;
            }
            //割合の積算が小さい場合は発散を防ぐために混合空気の数で割る
            else
            {
                drybulbTempOut = tSum / airNum;
                absHumidOut = hSum / airNum;
            }
            //出口空気状態を計算**飽和した場合の処理が必要**
            return new MoistAir(drybulbTempOut, absHumidOut);
        }

        /// <summary>空気を混合する</summary>
        /// <param name="temp">混合する空気の乾球温度[K]の配列</param>
        /// <param name="wet">混合する空気の絶対湿度[kg/kg]の配列</param>
        /// <param name="rate">混合する空気の割合</param>
        /// <returns>混合済み空気</returns>
        public static MoistAir BlendAir(double[] temp, double[] wet, double[] rate)
        {
            int airNum = temp.Length;
            if (airNum != wet.Length || airNum != rate.Length) throw new Exception("Air Class: BlendAir: 混合する空気の数と割合の数が一致しません");
            double rSum = 0.0d;
            double tSum = 0.0d;
            double trSum = 0.0d;
            double hSum = 0.0d;
            double hrSum = 0.0d;
            for (int i = 0; i < airNum; i++)
            {
                if (temp[i] < -30.0d) temp[i] = -30.0d;
                if (temp[i] > 65.0d) temp[i] = 65.0d;   //←もう少し上限はあるかも。要確認
                if (wet[i] < 0.0d) wet[i] = 0.0d;
                //割合を積算
                if (rate[i] < 0) throw new Exception("湿空気の混合比率が0以下に設定されています");
                rSum += rate[i];
                //温度[K]を積算
                tSum += temp[i];
                //温度[K]×割合を積算
                trSum += temp[i] * rate[i];
                //絶対湿度[kg/kg]を積算
                hSum += wet[i];
                //絶対湿度[kg/kg]×割合を積算
                hrSum += wet[i] * rate[i];
            }
            //混合後の乾球温度および絶対湿度を計算
            double drybulbTempOut;
            double absHumidOut;
            if (rSum >= 1.0e-5d)
            {
                drybulbTempOut = trSum / rSum;
                absHumidOut = hrSum / rSum;
            }
            //割合の積算が小さい場合は発散を防ぐために混合空気の数で割る
            else
            {
                drybulbTempOut = tSum / airNum;
                absHumidOut = hSum / airNum;
            }
            //出口空気状態を計算**飽和した場合の処理が必要**
            return new MoistAir(drybulbTempOut, absHumidOut);
        }

        /// <summary>空気を混合する</summary>
        /// <param name="air1">混合空気1</param>
        /// <param name="air2">混合空気2</param>
        /// <param name="air1Rate">空気1混合割合</param>
        /// <param name="air2Rate">空気2混合割合</param>
        /// <returns>混合空気</returns>
        public static MoistAir BlendAir(ImmutableMoistAir air1, ImmutableMoistAir air2, double air1Rate, double air2Rate)
        {
            if (air1Rate < 0 || air2Rate < 0) throw new Exception("湿空気の混合比率が0以下に設定されています");
            double rate = air1Rate + air2Rate;
            double dbt = (air1.DryBulbTemperature * air1Rate + air2.DryBulbTemperature * air2Rate) / rate;
            double ahd = (air1.HumidityRatio * air1Rate + air2.HumidityRatio * air2Rate) / rate;
            return MoistAir.GetAirStateFromDBHR(dbt, ahd);
        }

        #endregion

        #region 潜顕分離処理

        /// <summary>湿り空気の顕熱差[kJ/kg]および潜熱差[kJ/kg]を計算する</summary>
        /// <param name="mAir1">比較湿り空気状態</param>
        /// <param name="mAir2">基準湿り空気状態</param>
        /// <param name="sensibleHeat">顕熱差[kJ/kg]</param>
        /// <param name="latentHeat">潜熱差[kJ/kg]</param>
        public static void CalculateHeatDifference(ImmutableMoistAir mAir1, ImmutableMoistAir mAir2, out double sensibleHeat, out double latentHeat)
        {
            sensibleHeat = CalculateSensibleHeatDifference(mAir1, mAir2);
            latentHeat = CalculateLatentHeatDifference(mAir1, mAir2);
        }

        /// <summary>湿り空気の顕熱差[kJ/kg]を計算する</summary>
        /// <param name="mAir1">比較湿り空気状態</param>
        /// <param name="mAir2">基準湿り空気状態</param>
        /// <returns>顕熱差[kJ/kg]</returns>
        public static double CalculateSensibleHeatDifference(ImmutableMoistAir mAir1, ImmutableMoistAir mAir2)
        {
            return (CP_AIR + CP_VAPOR * mAir1.HumidityRatio) * mAir1.DryBulbTemperature - (CP_AIR + CP_VAPOR * mAir2.HumidityRatio) * mAir2.DryBulbTemperature;
        }

        /// <summary>湿り空気の潜熱差[kJ/kg]を計算する</summary>
        /// <param name="mAir1">比較湿り空気状態</param>
        /// <param name="mAir2">基準湿り空気状態</param>
        /// <returns>潜熱差[kJ/kg]</returns>
        public static double CalculateLatentHeatDifference(ImmutableMoistAir mAir1, ImmutableMoistAir mAir2)
        {
            return (mAir1.HumidityRatio - mAir2.HumidityRatio) * HFG;
        }

        /// <summary>湿り空気の顕熱[kJ/kg]および潜熱[kJ/kg]を計算する</summary>
        /// <param name="mAir">湿り空気状態</param>
        /// <param name="sensibleHeat">顕熱[kJ/kg]</param>
        /// <param name="latentHeat">潜熱[kJ/kg]</param>
        public static void CalculateSensibleAndLatentHeat(ImmutableMoistAir mAir, out double sensibleHeat, out double latentHeat)
        {
            sensibleHeat = CalculateSensibleHeat(mAir);
            latentHeat = CalculateLatentHeat(mAir);
        }

        /// <summary>湿り空気の顕熱[kJ/kg]を計算する</summary>
        /// <param name="mAir">湿り空気</param>
        /// <returns>顕熱[kJ/kg]</returns>
        public static double CalculateSensibleHeat(ImmutableMoistAir mAir)
        {
            return mAir.DryBulbTemperature * CP_AIR;
        }

        /// <summary>湿り空気の潜熱[kJ/kg]を計算する</summary>
        /// <param name="mAir">湿り空気</param>
        /// <returns>潜熱[kJ/kg]</returns>
        public static double CalculateLatentHeat(ImmutableMoistAir mAir)
        {
            return (CP_VAPOR * mAir.DryBulbTemperature + HFG) * mAir.HumidityRatio;
        }

        #endregion

        #region 空気状態コピー処理

        /// <summary>空気状態をコピーする</summary>
        /// <param name="air">コピー先の空気</param>
        public void CopyTo(MoistAir air)
        {
            air.AtmosphericPressure = this.AtmosphericPressure;
            air.DryBulbTemperature = this.DryBulbTemperature;
            air.WetBulbTemperature = this.WetBulbTemperature;
            air.HumidityRatio = this.HumidityRatio;
            air.RelativeHumidity = this.RelativeHumidity;
            air.Enthalpy = this.Enthalpy;
            air.SpecificVolume = this.specificVolume;
        }

        #endregion

        #region ICloneableインターフェース実装

        /// <summary>MoistAirオブジェクトの複製を返す</summary>
        /// <returns>MoistAirオブジェクトの複製</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        #region ISerializableインターフェース実装

        /// <summary>デシリアライズ用コンストラクタ</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected MoistAir(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            double version = sInfo.GetDouble("S_Version");

            //乾球温度[℃]
            dryBulbTemp = sInfo.GetDouble("dryBulbTemp");
            //湿球温度[℃]
            wetBulbTemp = sInfo.GetDouble("wetBulbTemp");
            //絶対湿度[kg/kg]
            humidityRatio = sInfo.GetDouble("humidityRatio");
            //相対湿度[%]
            relativeHumid = sInfo.GetDouble("relativeHumid");
            //エンタルピー[kJ/kg]
            enthalpy = sInfo.GetDouble("enthalpy");
            //比容積[m3/kg]
            if (1.1 <= version) specificVolume = sInfo.GetDouble("specificVolume");
            //大気圧[kPa]
            if (1.2 <= version) AtmosphericPressure = sInfo.GetDouble("atmosphericPressure");
            //編集番号
            if (1.3 <= version) revision = sInfo.GetUInt16("revision");
        }

        /// <summary>MoistAirシリアル化処理</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //バージョン情報
            info.AddValue("S_Version", S_VERSION);

            //乾球温度[℃]
            info.AddValue("dryBulbTemp", DryBulbTemperature);
            //湿球温度[℃]
            info.AddValue("wetBulbTemp", WetBulbTemperature);
            //絶対湿度[kg/kg]
            info.AddValue("humidityRatio", HumidityRatio);
            //相対湿度[%]
            info.AddValue("relativeHumid", RelativeHumidity);
            //エンタルピー[kJ/kg]
            info.AddValue("enthalpy", Enthalpy);
            //比容積[m3/kg]
            info.AddValue("specificVolume", SpecificVolume);
            //大気圧[kPa]
            info.AddValue("atmosphericPressure", AtmosphericPressure);
            //大気圧[kPa]
            info.AddValue("revision", Revision);
        }

        #endregion

        #region HVACSIM+(J) および ASHRAE HANDBOOK 2005 FUNDAMENTALSより移植

        /// <summary>乾球温度[℃]から飽和水蒸気分圧[kPa]を求める</summary>
        /// <param name="tdb">乾球温度[℃]</param>
        /// <returns>飽和水蒸気分圧[kPa]</returns>
        /// <remarks>Wexler-Hylandによる式</remarks>
        private static double fpws(double tdb)
        {
            //近似範囲確認
            if (tdb < -100 || 200 < tdb)
            {
                //国際化のためのリソースを取得
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                System.Resources.ResourceManager rm = new System.Resources.ResourceManager("Popolo.Utility.Properties.Resources", asm);
                throw new InputValueOutOfRangeException(rm.GetString("MoistAir_DrybulbTemperature_Error1"), 200, -100, tdb);
            }
            double td = tdb + TCONV;
            //-100~0C//三重点
            if (tdb < 0.01)
            {
                const double c1 = -5.6745359e3d;
                const double c2 = 6.3925247d;
                const double c3 = -9.6778430e-3d;
                const double c4 = 6.2215701e-7d;
                const double c5 = 2.0747825e-9d;
                const double c6 = -9.4840240e-13d;
                const double c7 = 4.1635019d;
                return Math.Exp(c1 / td + c2 + c3 * td + c4 * Math.Pow(td, 2) + c5 * Math.Pow(td, 3) + c6 * Math.Pow(td, 4) + c7 * Math.Log(td)) / 1000.0d;
            }
            //0~200C
            else
            {
                const double c8 = -5.8002206e3;
                const double c9 = 1.3914993d;
                const double c10 = -4.8640239e-2d;
                const double c11 = 4.1764768e-5d;
                const double c12 = -1.4452093e-8d;
                const double c13 = 6.5459673d;
                return Math.Exp(c8 / td + c9 + c10 * td + c11 * td * td + c12 * td * td * td + c13 * Math.Log(td)) / 1000.0d;
            }
        }

        /// <summary>絶対湿度[kg/kg]と大気圧[kPa]から露点温度[℃]を求める</summary>
        /// <param name="w">絶対湿度[kg/kg]</param>
        /// <param name="patm">大気圧[kPa]：1気圧は101.325[kPa]</param>
        /// <returns>露点温度[℃]</returns>
        private static double ftdew(double w, double patm)
        {
            const double c0 = 6.54d;
            const double c1 = 14.526d;
            const double c2 = 0.7389d;
            const double c3 = 0.09486d;
            const double c4 = 0.4569d;
            double ps = fpww(w, patm);
            if (ps < 0.000001d)
            {
                return 0.0d;
            }
            else
            {
                double alpha = Math.Log(ps);
                if (0.611213d < ps)
                {
                    return c0 + alpha * (c1 + alpha * (c2 + alpha * c3)) + c4 * Math.Pow(ps, 0.1984d);
                }
                else
                {
                    return 6.09 + alpha * (12.608d + alpha * 0.4959d);
                }
            }
        }

        /// <summary>絶対湿度[kg/kg]と大気圧[kPa]から水蒸気分圧[kPa]を求める</summary>
        /// <param name="w">絶対湿度[kg/kg]</param>
        /// <param name="patm">大気圧[kPa]：1気圧は101.325[kPa]</param>
        /// <returns>水蒸気分圧[kPa]</returns>
        private static double fpww(double w, double patm)
        {
            return patm * w / (0.62198d + w);
        }

        /// <summary>水蒸気分圧[kPa]と大気圧[kPa]から絶対湿度[kg/kg]を求める</summary>
        /// <param name="pw">水蒸気分圧[kPa]</param>
        /// <param name="patm">大気圧[kPa]：1気圧は101.325[kPa]</param>
        /// <returns>絶対湿度[kg/kg]</returns>
        private static double fwpw(double pw, double patm)
        {
            return 0.62198d * pw / (patm - pw);
        }

        /// <summary>乾球温度[℃]と相対湿度[%]と大気圧[kPa]から絶対湿度[kg/kg]を求める</summary>
        /// <param name="tdb">乾球温度[℃]</param>
        /// <param name="phi">相対湿度[%]</param>
        /// <param name="patm">大気圧[kPa]：1気圧は101.325[kPa]</param>
        /// <returns>絶対湿度[kg/kg]</returns>
        private static double fwphi(double tdb, double phi, double patm)
        {
            double ps = fpws(tdb);
            double pw = 0.01d * phi * ps;
            return fwpw(pw, patm);
        }

        /// <summary>乾球温度[℃]と湿球温度[℃]と大気圧[kPa]から絶対湿度[kg/kg]を求める</summary>
        /// <param name="tdb">乾球温度[℃]</param>
        /// <param name="twb">湿球温度[℃]</param>
        /// <param name="patm">大気圧[kPa]：1気圧は101.325[kPa]</param>
        /// <returns>絶対湿度[kg/kg]</returns>
        private static double fwtwb(double tdb, double twb, double patm)
        {
            double pstwb = fpws(twb);
            double ws = fwpw(pstwb, patm);
            return (ws * (HFG + CP_VAPOR * twb - fhc(twb)) - CP_AIR * (tdb - twb)) / (HFG + CP_VAPOR * tdb - fhc(twb));
        }

        /// <summary>乾球温度[℃]とエンタルピー[kJ/kg]から絶対湿度[kg/kg]を求める</summary>
        /// <param name="tdb">乾球温度[℃]</param>
        /// <param name="ha">エンタルピー[kJ/kg]</param>
        /// <returns>絶対湿度[kg/kg]</returns>
        private static double fwha(double tdb, double ha)
        {
            return (ha - CP_AIR * tdb) / (CP_VAPOR * tdb + HFG);
        }

        /// <summary>絶対湿度[kg/kg]とエンタルピー[kJ/kg]から乾球温度[℃]を求める</summary>
        /// <param name="w">絶対湿度[kg/kg]</param>
        /// <param name="ha">エンタルピー[kJ/kg]</param>
        /// <returns>乾球温度[℃]</returns>
        private static double ftdb(double w, double ha)
        {
            return (ha - HFG * w) / (CP_AIR + CP_VAPOR * w);
        }

        /// <summary>乾球温度[℃]と絶対湿度[kg/kg]と大気圧[kPa]から相対湿度[%]を求める</summary>
        /// <param name="tdb">乾球温度[℃]</param>
        /// <param name="w">絶対湿度[kg/kg]</param>
        /// <param name="patm">大気圧[kPa]：1気圧は101.325[kPa]</param>
        /// <returns>相対湿度[%]</returns>
        private static double fphi(double tdb, double w, double patm)
        {
            double pw = fpww(w, patm);
            double ps = fpws(tdb);
            if (ps == 0.0d)
            {
                return 0.0d;
            }
            else
            {
                return 100.0d * pw / ps;
            }
        }

        /// <summary>乾球温度[℃]と絶対湿度[kg/kg]からエンタルピー[kJ/kg]を求める</summary>
        /// <param name="tdb">乾球温度[℃]</param>
        /// <param name="w">絶対湿度[kg/kg]</param>
        /// <returns>エンタルピー[kJ/kg]</returns>
        private static double fhair(double tdb, double w)
        {
            return CP_AIR * tdb + w * (CP_VAPOR * tdb + HFG);
        }

        /// <summary>飽和温度[℃]と大気圧[kPa]から飽和エンタルピー[kJ/kg]を求める</summary>
        /// <param name="tsat">飽和温度[℃]</param>
        /// <param name="patm">大気圧[kPa]：1気圧は101.325[kPa]</param>
        /// <returns>飽和エンタルピー[kJ/kg]</returns>
        private static double fhsat(double tsat, double patm)
        {
            double ps = fpws(tsat);
            double ws = fwpw(ps, patm);
            return fhair(tsat, ws);
        }

        /// <summary>飽和エンタルピー[kJ/kg]と大気圧[kPa]から飽和温度[℃]を求める</summary>
        /// <param name="hs">飽和エンタルピー[kJ/kg]</param>
        /// <param name="patm">大気圧[kPa]：1気圧は101.325[kPa]</param>
        /// <returns>飽和温度[℃]</returns>
        private static double ftsat(double hs, double patm)
        {
            const double c0 = -6.0055d;
            const double c1 = 0.6851d;
            const double c2 = -0.0056978d;
            const double c3 = 0.000035344d;
            const double c4 = -0.00000012891d;
            const double c5 = 0.00000000020165d;

            double ts1 = c0 + hs * (c1 + hs * (c2 + hs * (c3 + hs * (c4 + hs * c5))));
            double hs1 = fhsat(ts1, patm);
            double delth1 = hs1 - hs;
            double ts2 = 0.0d;
            double ftsat;
            if (Math.Abs(delth1) < 0.001d)
            {
                ftsat = ts1;
            }
            else
            {
                ts2 = ts1 - 5.0d;
                for (int i = 0; i < 50; i++)
                {
                    double hs2 = fhsat(ts2, patm);
                    double delth2 = hs2 - hs;
                    if (Math.Abs(delth2) > 0.001d)
                    {
                        double ts = 0.0d;
                        if (Math.Abs(delth2 - delth1) > 0.00001d)
                        {
                            ts = ts1 - delth1 * (ts2 - ts1) / (delth2 - delth1);
                        }
                        ts1 = ts2;
                        hs1 = hs2;
                        delth1 = delth2;
                        ts2 = ts;
                    }
                }
            }
            return ts2;
        }

        /// <summary>乾球温度[℃]と絶対湿度[kg/kg]と大気圧[kPa]から湿球温度[℃]を求める</summary>
        /// <param name="tdb">乾球温度[℃]</param>
        /// <param name="w">絶対湿度[kg/kg]</param>
        /// <param name="patm">大気圧[kPa]：1気圧は101.325[kPa]</param>
        /// <returns>湿球温度[℃]</returns>
        private static double ftwb(double tdb, double w, double patm)
        {
            double h = fhair(tdb, w);
            double twb1 = ftsat(h, patm) - 5.0d;
            double pws1 = fpws(twb1);
            double ws1 = fwpw(pws1, patm);
            double h1 = fhair(twb1, ws1) - CP_WATER * twb1 * (ws1 - w);
            double delth1 = h1 - h;
            double twb2 = twb1 + 5.0d;

            for (int i = 0; i < 50; i++)
            {
                double pws2 = fpws(twb2);
                double ws2 = fwpw(pws2, patm);
                double h2 = fhair(twb2, ws2) - CP_WATER * twb2 * (ws2 - w);
                double delth2 = h2 - h;
                if (Math.Abs(delth2) > 0.001d)
                {
                    double twb;
                    if (Math.Abs(delth2 - delth1) > 1E-30d)
                    {
                        twb = twb1 - delth1 * (twb2 - twb1) / (delth2 - delth1);
                    }
                    else
                    {
                        twb = twb1;
                    }
                    twb1 = twb2;
                    h1 = h2;        //←意味あるか？
                    delth1 = delth2;
                    twb2 = twb;
                }
            }
            return twb2;
        }

        #endregion

        #region private staticメソッド

        /// <summary>相対湿度[%]および比容積[m3/kg]から乾球温度[C]を求める</summary>
        /// <param name="relativeHumidity">相対湿度[%]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>乾球温度[C]</returns>
        private static double getDryBulbTemperatureFromRHSV(double relativeHumidity, double specificVolume, double atm)
        {
            //収束計算
            const double DELTA = 1.0e-10;
            const double TOL = 1.0e-9;
            int iterNum = 0;
            double dbt = 25;
            while (true)
            {
                double err1 = fwphi(dbt, relativeHumidity, atm) - getHumidityRatioFromDBSV(dbt, specificVolume, atm);
                if (Math.Abs(err1) < TOL) break;
                double err2 = fwphi(dbt + DELTA, relativeHumidity, atm) - getHumidityRatioFromDBSV(dbt + DELTA, specificVolume, atm);
                dbt -= err1 / ((err2 - err1) / DELTA);
                iterNum++;
                if (20 < iterNum) throw new Exception("Iteration Error");
            }
            return dbt;
        }

        /// <summary>湿球温度[C]および比容積[m3/kg]から乾球温度[C]を求める</summary>
        /// <param name="wbTemp">湿球温度[C]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>乾球温度[C]</returns>
        private static double getDryBulbTemperatureFromWBSV(double wbTemp, double specificVolume, double atm)
        {
            //収束計算
            const double DELTA = 1.0e-10;
            const double TOL = 1.0e-9;
            int iterNum = 0;
            double dbt = 25;
            while (true)
            {
                double err1 = fwtwb(dbt, wbTemp, atm) - getHumidityRatioFromDBSV(dbt, specificVolume, atm);
                if (Math.Abs(err1) < TOL) break;
                double err2 = fwtwb(dbt + DELTA, wbTemp, atm) - getHumidityRatioFromDBSV(dbt + DELTA, specificVolume, atm);
                dbt -= err1 / ((err2 - err1) / DELTA);
                iterNum++;
                if (20 < iterNum) throw new Exception("Iteration Error");
            }
            return dbt;
        }

        /// <summary>乾球温度[C]および絶対湿度[kg/kg]から湿り空気の比容積[m3/kg]を求める</summary>
        /// <param name="dbTemp">乾球温度[C]</param>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>比容積[m3/kg]</returns>
        private static double getSpecificVolumeFromDBHR(double dbTemp, double humidityRatio, double atm)
        {
            return ((dbTemp + TCONV) * GAS_CONSTANT_DRY_AIR) / atm * (1.0d + 1.6078d * humidityRatio);
        }

        /// <summary>比容積[m3/kg]および絶対湿度[kg/kg]から乾球温度[C]を求める</summary>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>乾球温度[C]</returns>
        private static double getDryBulbTemperatureFromSVHR(double specificVolume, double humidityRatio, double atm)
        {
            return specificVolume / (1.0 + 1.6078d * humidityRatio) * atm / GAS_CONSTANT_DRY_AIR - TCONV;
        }

        /// <summary>乾球温度[C]および比容積[m3/kg]から絶対湿度[kg/kg]を求める</summary>
        /// <param name="dryBulbTemperature">乾球温度[C]</param>
        /// <param name="specificVolume">比容積[m3/kg]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>絶対湿度[kg/kg]</returns>
        private static double getHumidityRatioFromDBSV(double dryBulbTemperature, double specificVolume, double atm)
        {
            return (specificVolume / (dryBulbTemperature + TCONV) / GAS_CONSTANT_DRY_AIR * atm - 1.0d) / 1.6078d;
        }

        /// <summary>水の温度から水の比エンタルピー[kJ/kg]を計算する</summary>
        /// <param name="waterTemp">水温度</param>
        /// <returns>比エンタルピー[kJ/kg]</returns>
        private static double fhc(double waterTemp)
        {
            //氷の場合
            if (waterTemp < 0) return -333.6d + 2.093 * waterTemp;
            //水の場合
            else return CP_WATER * waterTemp;
        }

        /// <summary>絶対湿度[kg/kg]と湿球温度[C]から乾球温度[C]を計算する</summary>
        /// <param name="humidityRatio">絶対湿度[kg/kg]</param>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>乾球温度[C]</returns>
        private static double fwwbdb(double humidityRatio, double wetBulbTemp, double atm)
        {
            double pstwb = fpws(wetBulbTemp);
            double ws = fwpw(pstwb, atm);
            return (CP_AIR * wetBulbTemp + (CP_VAPOR * wetBulbTemp + HFG - fhc(wetBulbTemp)) * ws - (HFG - fhc(wetBulbTemp)) * humidityRatio) / (CP_AIR + CP_VAPOR * humidityRatio);
        }

        /// <summary>飽和蒸気分圧[kPa]から乾球温度[C]を計算する</summary>
        /// <param name="pws">飽和蒸気分圧[kPa]</param>
        /// <returns>乾球温度[C]</returns>
        private static double ftpws(double pws)
        {
            //収束計算
            const double DELTA = 1.0e-10;
            const double TOL = 1.0e-9;
            int iterNum = 0;
            double dbt = 25;
            while (true)
            {
                double err1 = fpws(dbt) - pws;
                if (Math.Abs(err1) < TOL) break;
                double err2 = fpws(dbt + DELTA) - pws;
                dbt -= err1 / ((err2 - err1) / DELTA);
                iterNum++;
                if (20 < iterNum) throw new Exception("Iteration Error");
            }
            return dbt;
        }

        /// <summary>相対湿度[%]と湿球温度[C]から乾球温度[C]を計算する</summary>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>乾球温度[C]</returns>
        private static double fndbrw(double relativeHumid, double wetBulbTemp, double atm)
        {
            //収束計算
            const double DELTA = 1.0e-10;
            const double TOL = 1.0e-9;
            int iterNum = 0;
            double dbt = wetBulbTemp;
            while (true)
            {
                double err1 = dbt - fwwbdb(fwphi(dbt, relativeHumid, atm), wetBulbTemp, atm);
                if (Math.Abs(err1) < TOL) break;
                double err2 = (dbt + DELTA) - fwwbdb(fwphi(dbt + DELTA, relativeHumid, atm), wetBulbTemp, atm);
                dbt -= err1 / ((err2 - err1) / DELTA);
                iterNum++;
                if (20 < iterNum) throw new Exception("Iteration Error");
            }
            return dbt;
        }

        /// <summary>エンタルピー[kJ/kg]と湿球温度[C]から乾球温度[C]を計算する</summary>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="wetBulbTemp">湿球温度[C]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>乾球温度[C]</returns>
        private static double fndbhw(double enthalpy, double wetBulbTemp, double atm)
        {
            //収束計算
            const double DELTA = 1.0e-10;
            const double TOL = 1.0e-9;
            int iterNum = 0;
            double dbt = wetBulbTemp;
            while (true)
            {
                double err1 = dbt - fwwbdb(fwha(dbt, enthalpy), wetBulbTemp, atm);
                if (Math.Abs(err1) < TOL) break;
                double err2 = (dbt + DELTA) - fwwbdb(fwha(dbt + DELTA, enthalpy), wetBulbTemp, atm);
                dbt -= err1 / ((err2 - err1) / DELTA);
                iterNum++;
                if (20 < iterNum) throw new Exception("Iteration Error");
            }
            return dbt;
        }

        /// <summary>相対湿度[%]とエンタルピー[kJ/kg]から乾球温度[C]を計算する</summary>
        /// <param name="enthalpy">エンタルピー[kJ/kg]</param>
        /// <param name="relativeHumid">相対湿度[%]</param>
        /// <param name="atm">大気圧[kPa]</param>
        /// <returns>乾球温度[C]</returns>
        private static double fndbrh(double relativeHumid, double enthalpy, double atm)
        {
            //収束計算
            const double DELTA = 1.0e-10;
            const double TOL = 1.0e-3;
            int iterNum = 0;
            double dbt = 25;
            while (true)
            {
                double err1 = enthalpy - fhair(dbt, fwphi(dbt, relativeHumid, atm));
                if (Math.Abs(err1) < TOL) break;
                double err2 = enthalpy - fhair(dbt + DELTA, fwphi(dbt + DELTA, relativeHumid, atm));
                dbt -= err1 / ((err2 - err1) / DELTA);
                iterNum++;
                if (20 < iterNum) throw new Exception("Iteration Error");
            }
            return dbt;
        }

        #endregion

        #region MoistAir例外クラス

        /// <summary>入力データが計算可能な範囲内にない場合の例外</summary>
        public class InputValueOutOfRangeException : Exception
        {

            #region<インスタンス変数>

            /// <summary>変数の現在値、下限値、上限値</summary>
            private double maxValue, minValue, currentValue;

            #endregion//インスタンス変数

            #region<プロパティ>

            /// <summary>計算可能な上限値</summary>
            public double MaxValue
            {
                get
                {
                    return maxValue;
                }
            }

            /// <summary>計算可能な下限値</summary>
            public double MinValue
            {
                get
                {
                    return minValue;
                }
            }

            /// <summary>変数の現在値</summary>
            public double CurrentValue
            {
                get
                {
                    return currentValue;
                }
            }

            #endregion//プロパティ

            #region コンストラクタ

            /// <summary>コンストラクタ</summary>
            /// <param name="message">提示する例外メッセージ</param>
            /// <param name="maxValue">計算可能な上限値</param>
            /// <param name="minValue">計算可能な下限値</param>
            /// <param name="currentValue">変数の現在値</param>
            public InputValueOutOfRangeException(string message, double maxValue, double minValue, double currentValue)
                : base(message)
            {
                this.maxValue = maxValue;
                this.minValue = minValue;
                this.currentValue = currentValue;
            }

            #endregion

        }

        #endregion

    }

}
