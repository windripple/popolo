/* Zone.cs
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
using System.Collections.Generic;

using Popolo.ThermophysicalProperty;

namespace Popolo.ThermalLoad
{
    /// <summary>ゾーンクラス</summary>
    /// <remarks>平均周壁温度を利用する方法</remarks>
    public class Zone : ImmutableZone
    {

        #region インスタンス変数

        /// <summary>現在の乾球温度[C]</summary>
        private double drybulbTemperature = 26;

        /// <summary>現在の絶対湿度[kg/kg(DA)]</summary>
        private double absoluteHumidity = 0.012;

        /// <summary>計算時間間隔[sec]</summary>
        private double timeStep = 60;

        /// <summary>室容積[m3]</summary>
        private double volume = 1;

        /// <summary>空気以外の顕熱容量[J/K]</summary>
        /// <remarks>
        /// 事務所の場合、室容積あたり12000[J/m3-K]程度
        /// 木村健一, 事務所建築の家具の熱容量, 日本建築学会関東支部第29会研究発表会, 1961.01
        /// 住宅の場合は事務所の半分程度
        /// </remarks>
        private double sensibleHeatCapacity;

        /// <summary>空気以外の水蒸気容量[kg]</summary>
        private double latentHeatCapacity;

        /// <summary>発熱要素リスト</summary>
        private List<IHeatGain> heatGains = new List<IHeatGain>();

        /// <summary>室温計算用係数</summary>
        private double ar;

        /// <summary>壁面の総合熱伝達率[W/m2-K]</summary>
        /// <remarks>
        /// 全壁面で一定の値とする
        /// 屋内で9.3[W/m2-K], 屋外で23[W/m2-K] 程度
        /// </remarks>
        private double heatTransferCoefficient = 9.3;

        /// <summary>総合熱伝達率[W/m2-K]のうち、対流熱伝達の割合[-]</summary>
        private double convectiveRate = 0.45;

        /// <summary>総合熱伝達率[W/m2-K]のうち、放射熱伝達の割合[-]</summary>
        private double radiativeRate = 0.55;

        /// <summary>壁面積合算値[m2]</summary>
        private double allSurfaceArea;

        /// <summary>換気量[m3/h]</summary>
        private double ventilationVolume;

        /// <summary>換気空気状態</summary>
        private ImmutableMoistAir ventilationAirState = new MoistAir();

        /// <summary>AFI積算</summary>
        private double afiSum;

        /// <summary>SDT</summary>
        private double sdt;

        /// <summary>状態保持用の実数</summary>
        private double brm, brc, brmx, brcx;

        /// <summary>初回の計算か否か</summary>
        private bool firstCalculation = true;

        /// <summary>AFT</summary>
        private double aft = 0;

        /// <summary>室に属する表面リスト</summary>
        private List<ISurface> surfaces = new List<ISurface>();

        /// <summary>室に属する窓表面リスト</summary>
        private List<WindowSurface> windowSurfaces = new List<WindowSurface>();

        /// <summary>表面への短波長放射成分入射比率[-]</summary>
        private Dictionary<ISurface, double> shortWaveRadiationToSurface = new Dictionary<ISurface, double>();

        /// <summary>表面への長波長放射成分入射比率[-]</summary>
        private Dictionary<ISurface, double> longWaveRadiationToSurface = new Dictionary<ISurface, double>();

        /// <summary>表面への短波長放射成分入射比率[-]：合算して1.0になるように調整済み</summary>
        private double[] shortWaveRadiationRate;

        /// <summary>表面への短波長放射成分入射比率[-]：合算して1.0になるように調整済み</summary>
        private double[] longWaveRadiationRate;

        /// <summary>状態値に変更がある場合は真</summary>
        private bool hasChanged = true;

        #endregion

        #region プロパティ

        /// <summary>名称を設定・取得する</summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>室容積[m3]を設定・取得する</summary>
        public double Volume
        {
            get
            {
                return volume;
            }
            set
            {
                if (0 <= value) volume = value;
            }
        }

        /// <summary>計算時間間隔[sec]を設定・取得する</summary>
        public double TimeStep
        {
            get
            {
                return timeStep;
            }
            set
            {
                if (0 < value)
                {
                    timeStep = value;
                    initializeParameters();
                }
            }
        }

        /// <summary>空気以外の顕熱容量[J/K]を設定・取得する</summary>
        public double SensibleHeatCapacity
        {
            get
            {
                return sensibleHeatCapacity;
            }
            set
            {
                if (0 <= value) sensibleHeatCapacity = value;
            }
        }

        /// <summary>顕熱供給[W]（暖房を正とする）を設定・取得する</summary>
        public double SensibleHeatSupply
        {
            get;
            set;
        }

        /// <summary>空気以外の水蒸気容量[kg]を設定・取得する</summary>
        public double LatentHeatCapacity
        {
            get
            {
                return latentHeatCapacity;
            }
            set
            {
                if (0 <= value) latentHeatCapacity = value;
            }
        }

        /// <summary>潜熱供給[W]（加湿を正とする）を設定・取得する</summary>
        public double LatentHeatSupply
        {
            get;
            set;
        }

        /// <summary>壁面の総合熱伝達率[W/m2-K]を設定・取得する</summary>
        public double HeatTransferCoefficient
        {
            get
            {
                return heatTransferCoefficient;
            }
            set
            {
                if (0 < value)
                {
                    heatTransferCoefficient = value;
                }
            }
        }

        /// <summary>換気量[m3/h]を設定・取得する</summary>
        public double VentilationVolume
        {
            get
            {
                return ventilationVolume;
            }
            set
            {
                if (0 <= value)
                {
                    hasChanged = true;
                    ventilationVolume = value;
                }
            }
        }

        /// <summary>換気空気状態を設定・取得する</summary>
        public ImmutableMoistAir VentilationAirState
        {
            get
            {
                return ventilationAirState;
            }
            set
            {
                hasChanged = true;
                ventilationAirState = value;
            }
        }

        /// <summary>乾球温度[C]を制御するか否かを設定・取得する</summary>
        public bool ControlDrybulbTemperature
        {
            get;
            set;
        }

        /// <summary>絶対湿度[kg/kg(DA)]を制御するか否かを設定・取得する</summary>
        public bool ControlAbsoluteHumidity
        {
            get;
            set;
        }

        /// <summary>乾球温度設定値[C]を設定・取得する</summary>
        public double DrybulbTemperatureSetPoint
        {
            get;
            set;
        }

        /// <summary>絶対湿度設定値[kg/kg(DA)]を設定・取得する</summary>
        public double AbsoluteHumiditySetPoint
        {
            get;
            set;
        }

        /// <summary>壁表面リストを取得する</summary>
        public ImmutableSurface[] Surfaces
        {
            get
            {
                return surfaces.ToArray();
            }
        }

        /// <summary>発熱要素を取得する</summary>
        public IHeatGain[] HeatGains
        {
            get
            {
                return heatGains.ToArray();
            }
        }

        /// <summary>大気圧[kPa]を設定・取得する</summary>
        public double AtmosphericPressure
        {
            get;
            set;
        }

        /// <summary>現在の乾球温度[C]を取得する</summary>
        public double CurrentDrybulbTemperature
        {
            get
            {
                return drybulbTemperature;
            }
        }

        /// <summary>現在の絶対湿度[kg/kg]を取得する</summary>
        public double CurrentAbsoluteHumidity
        {
            get
            {
                return absoluteHumidity;
            }
        }

        /// <summary>現在の顕熱負荷[kW]を取得する</summary>
        public double CurrentSensibleHeatLoad
        {
            get;
            internal set;
        }

        /// <summary>現在の潜熱負荷[kW]を取得する</summary>
        public double CurrentLatentHeatLoad
        {
            get;
            internal set;
        }

        /// <summary>現在の平均放射温度[C]を取得する</summary>
        public double CurrentMeanRadiantTemperature
        {
            get;
            private set;
        }

        /// <summary>現在の日時を設定・取得する</summary>
        public DateTime CurrentDateTime
        {
            get;
            set;
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        public Zone()
        {
            AtmosphericPressure = 101.325;
            ControlAbsoluteHumidity = false;
            ControlDrybulbTemperature = false;
            AbsoluteHumiditySetPoint = 0.018;
            DrybulbTemperatureSetPoint = 26;
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="name">ゾーン名称</param>
        public Zone(string name)
        {
            this.Name = name;

            AtmosphericPressure = 101.325;
            ControlAbsoluteHumidity = false;
            ControlDrybulbTemperature = false;
            AbsoluteHumiditySetPoint = 0.018;
            DrybulbTemperatureSetPoint = 26;
        }

        #endregion

        #region モデル構築処理

        /// <summary>表面を追加する</summary>
        /// <param name="surface">追加する表面</param>
        /// <returns>追加成功の真偽</returns>
        public bool AddSurface(ISurface surface)
        {
            if (!this.surfaces.Contains(surface))
            {
                //窓の場合
                if (surface is WindowSurface) windowSurfaces.Add((WindowSurface)surface);

                //表面にゾーンを登録
                surface.FacingZone = this;

                //各種係数を初期化
                surfaces.Add(surface);
                initializeParameters();

                //放射成分入射比率[-]を初期化
                shortWaveRadiationToSurface.Add(surface, surface.Area);
                initializeShortWaveRadiationRate();
                longWaveRadiationToSurface.Add(surface, surface.Area);
                initializeLongWaveRadiationRate();

                //面積変更イベント登録
                surface.AreaChangeEvent += new EventHandler(surface_AreaChangeEvent);

                return true;
            }
            else return false;
        }

        /// <summary>表面を削除する</summary>
        /// <param name="surface">削除する表面</param>
        /// <returns>削除成功の真偽</returns>
        public bool RemoveSurface(ISurface surface)
        {
            if (surfaces.Contains(surface))
            {
                if (surface is WindowSurface) windowSurfaces.Remove((WindowSurface)surface);

                surfaces.Remove(surface);
                shortWaveRadiationToSurface.Remove(surface);
                initializeShortWaveRadiationRate();
                longWaveRadiationToSurface.Remove(surface);
                initializeLongWaveRadiationRate();

                //面積変更イベント解除
                surface.AreaChangeEvent -= new EventHandler(surface_AreaChangeEvent);

                return true;
            }
            else return false;
        }

        /// <summary>窓を追加する</summary>
        /// <param name="window">窓</param>
        /// <returns>追加成功の真偽</returns>
        public bool AddWindow(Window window)
        {
            return AddSurface(window.GetSurface(false));
        }

        /// <summary>窓を削除する</summary>
        /// <param name="window">削除する窓</param>
        /// <returns>削除成功の真偽</returns>
        public bool RemoveWindow(Window window)
        {
            return RemoveSurface(window.GetSurface(false));
        }

        /// <summary>発熱要素を追加する</summary>
        /// <param name="heatGain">発熱要素</param>
        /// <returns>追加成功の真偽</returns>
        public bool AddHeatGain(IHeatGain heatGain)
        {
            if (!this.heatGains.Contains(heatGain))
            {
                heatGains.Add(heatGain);
                return true;
            }
            else return false;
        }

        /// <summary>発熱要素を削除する</summary>
        /// <param name="heatGain">削除する発熱要素</param>
        /// <returns>削除成功の真偽</returns>
        public bool RemoveHeatGain(IHeatGain heatGain)
        {
            if (this.heatGains.Contains(heatGain))
            {
                this.heatGains.Remove(heatGain);
                return true;
            }
            else return false;
        }

        /// <summary>温湿度を初期化する</summary>
        /// <param name="drybulbTemperature">乾球温度[C]</param>
        /// <param name="absoluteHumidity">絶対湿度[kg/kg(DA)]</param>
        public void InitializeAirState(double drybulbTemperature, double absoluteHumidity)
        {
            this.drybulbTemperature = drybulbTemperature;
            this.absoluteHumidity = absoluteHumidity;
        }

        /// <summary>計算用の各種パラメータを初期化する</summary>
        private void initializeParameters()
        {
            //時間間隔[sec]
            /*foreach (ISurface ws in surfaces)
            {
                if (ws is WallSurface)
                {
                    WallSurface wws = (WallSurface)ws;
                    Wall wl = (Wall)wws.WallBody;
                    wl.TimeStep = this.timeStep;
                }
            }*/

            //SDT, ARを更新
            afiSum = 0;
            allSurfaceArea = 0;
            foreach (ISurface ws in surfaces)
            {
                allSurfaceArea += ws.Area;
                afiSum += ws.Area * ws.FI;
            }
            sdt = allSurfaceArea - radiativeRate * afiSum;
            ar = allSurfaceArea * heatTransferCoefficient * convectiveRate * (1d - convectiveRate * afiSum / sdt);
        }

        /// <summary>総合熱伝達率[W/m2-K]のうち、対流熱伝達の割合[-]を設定する</summary>
        public void SetConvectiveRate(double convectiveRate)
        {
            this.convectiveRate = Math.Max(Math.Min(convectiveRate, 1), 0);
            this.radiativeRate = 1 - convectiveRate;
            foreach (ISurface ws in surfaces)
            {
                ws.ConvectiveRate = this.convectiveRate;
            }
            //パラメータ初期化
            initializeParameters();
        }

        #endregion

        #region 放射成分設定関連の処理
        
        /// <summary>窓面の短波長放射成分入射比率[-]を設定する</summary>
        /// <param name="window">窓面</param>
        /// <param name="rate">窓面の短波長放射成分入射比率[-]</param>
        /// <remarks>
        /// 室内にある他の窓や壁の設定値と比較しながらプログラム内部で0～1の範囲に調整される。
        /// デフォルトでは面積比が設定される。
        /// </remarks>
        public void SetShortWaveRadiationRate(ImmutableWindow window, double rate)
        {
            WindowSurface ws = window.GetSurface(false);

            //窓面が存在しない場合
            if (!shortWaveRadiationToSurface.ContainsKey(ws)) return;

            shortWaveRadiationToSurface[ws] = rate;

            //短波長放射成分入射比率[-]を初期化
            initializeShortWaveRadiationRate();
        }

        /// <summary>窓面の短波長放射成分入射比率[-]を取得する</summary>
        /// <param name="window">窓面</param>
        /// <returns>窓面の短波長放射成分入射比率[-]</returns>
        public double GetShortWaveRadiationRate(ImmutableWindow window)
        {
            WindowSurface ws = window.GetSurface(false);

            if (shortWaveRadiationToSurface.ContainsKey(ws)) return shortWaveRadiationToSurface[ws];
            else return 0;
        }
        
        /// <summary>窓面の長波長放射成分入射比率[-]を設定する</summary>
        /// <param name="window">窓面</param>
        /// <param name="rate">窓面の長波長放射成分入射比率[-]</param>
        /// <remarks>
        /// 室内にある他の窓や壁の設定値と比較しながらプログラム内部で0～1の範囲に調整される。
        /// デフォルトでは面積比が設定される。
        /// </remarks>
        public void SetLongWaveRadiationRate(ImmutableWindow window, double rate)
        {
            WindowSurface ws = window.GetSurface(false);

            //窓面が存在しない場合
            if (!longWaveRadiationToSurface.ContainsKey(ws)) return;

            longWaveRadiationToSurface[ws] = rate;

            //長波長放射成分入射比率[-]を初期化
            initializeLongWaveRadiationRate();
        }

        /// <summary>窓面の長波長放射成分入射比率[-]を取得する</summary>
        /// <param name="window">窓面</param>
        /// <returns>窓面の長波長放射成分入射比率[-]</returns>
        public double GetLongWaveRadiationRate(ImmutableWindow window)
        {
            WindowSurface ws = window.GetSurface(false);

            if (longWaveRadiationToSurface.ContainsKey(ws)) return longWaveRadiationToSurface[ws];
            else return 0;
        }

        /// <summary>短波長放射成分入射比率[-]を設定する</summary>
        /// <param name="surface">表面</param>
        /// <param name="rate">短波長放射成分入射比率[-]</param>
        /// <remarks>
        /// 室内にある他の窓や壁の設定値と比較しながらプログラム内部で0～1の範囲に調整される。
        /// デフォルトでは面積比が設定される。
        /// </remarks>
        public void SetShortWaveRadiationRate(ISurface surface, double rate)
        {
            //壁面が存在しない場合
            if (!shortWaveRadiationToSurface.ContainsKey(surface)) return;

            shortWaveRadiationToSurface[surface] = rate;

            //短波長放射成分入射比率[-]を初期化
            initializeShortWaveRadiationRate();
        }

        /// <summary>短波長放射成分入射比率[-]を取得する</summary>
        /// <param name="surface">表面</param>
        /// <returns>短波長放射成分入射比率[-]</returns>
        public double GetShortWaveRadiationRate(ISurface surface)
        {
            if (shortWaveRadiationToSurface.ContainsKey(surface)) return shortWaveRadiationToSurface[surface];
            else return 0;
        }

        /// <summary>長波長放射成分入射比率[-]を設定する</summary>
        /// <param name="surface">表面</param>
        /// <param name="rate">長波長放射成分入射比率[-]</param>
        /// <remarks>
        /// 室内にある他の窓や壁の設定値と比較しながらプログラム内部で0～1の範囲に調整される。
        /// デフォルトでは面積比が設定される。
        /// </remarks>
        public void SetLongWaveRadiationRate(ISurface surface, double rate)
        {
            //壁面が存在しない場合
            if (!longWaveRadiationToSurface.ContainsKey(surface)) return;

            longWaveRadiationToSurface[surface] = rate;

            //長波長放射成分入射比率[-]を初期化
            initializeLongWaveRadiationRate();
        }

        /// <summary>長波長放射成分入射比率[-]を取得する</summary>
        /// <param name="surface">表面</param>
        /// <returns>長波長放射成分入射比率[-]</returns>
        public double GetLongWaveRadiationRate(ISurface surface)
        {
            if (longWaveRadiationToSurface.ContainsKey(surface)) return longWaveRadiationToSurface[surface];
            else return 0;
        }

        /// <summary>短波長放射成分入射比率[-]を初期化する</summary>
        private void initializeShortWaveRadiationRate()
        {
            shortWaveRadiationRate = new double[surfaces.Count];

            double slSum = 0;
            for (int i = 0; i < surfaces.Count; i++) slSum += shortWaveRadiationToSurface[surfaces[i]];
            for (int i = 0; i < surfaces.Count; i++) shortWaveRadiationRate[i] = shortWaveRadiationToSurface[surfaces[i]] / slSum;
        }

        /// <summary>長波長放射成分入射比率[-]を初期化する</summary>
        private void initializeLongWaveRadiationRate()
        {
            longWaveRadiationRate = new double[surfaces.Count];

            double slSum = 0;
            for (int i = 0; i < surfaces.Count; i++) slSum += longWaveRadiationToSurface[surfaces[i]];
            for (int i = 0; i < surfaces.Count; i++) longWaveRadiationRate[i] = longWaveRadiationToSurface[surfaces[i]] / slSum;
        }

        #endregion

        #region モデル更新処理

        /// <summary>室の乾球温度[C]を取得する</summary>
        /// <param name="sensibleHeatSupply">顕熱供給[W]（暖房を正とする）</param>
        /// <returns>室の乾球温度[C]</returns>
        public double GetNextDrybulbTemperature(double sensibleHeatSupply)
        {
            if (firstCalculation) return drybulbTemperature;

            preprocess();
            return (brc + sensibleHeatSupply) / brm;
        }

        /// <summary>絶対湿度[kg/kg(DA)]を取得する</summary>
        /// <param name="latentHeatSupply">潜熱供給[W]（加湿を正とする）</param>
        /// <returns>室の絶対湿度[kg/kg(DA)]</returns>
        public double GetNextAbsoluteHumidity(double latentHeatSupply)
        {
            if (firstCalculation) return absoluteHumidity;

            preprocess();
            return (brcx + latentHeatSupply / MoistAir.LatentHeatOfVaporization / 1000) / brmx;
        }

        /// <summary>周壁平均温度[C]を計算する</summary>
        /// <returns>周壁平均温度[C]</returns>
        public double GetNextMeanRadiantTemperature()
        {
            double dbt;
            if (ControlDrybulbTemperature) dbt = DrybulbTemperatureSetPoint;
            else dbt = GetNextDrybulbTemperature(SensibleHeatSupply);

            return (aft + convectiveRate * afiSum * dbt) / sdt;
        }

        /// <summary>顕熱負荷[W]を計算する</summary>
        /// <param name="drybulbTemperatureSetPoint">室温設定値[C]</param>
        /// <returns>顕熱負荷[W]</returns>
        public double GetNextSensibleHeatLoad(double drybulbTemperatureSetPoint)
        {
            if (firstCalculation) return 0;

            //顕熱負荷[W]を計算
            preprocess();
            return brc - brm * drybulbTemperatureSetPoint;
        }

        /// <summary>潜熱負荷[W]を計算する</summary>
        /// <param name="absoluteHumiditySetPoint">絶対湿度設定値[kg/kg]</param>
        /// <returns>潜熱負荷[W]</returns>
        public double GetNextLatentHeatLoad(double absoluteHumiditySetPoint)
        {
            if (firstCalculation) return 0;

            preprocess();
            return MoistAir.LatentHeatOfVaporization * 1000 * (brcx - brmx * absoluteHumiditySetPoint);
        }

        /// <summary>前処理を行う</summary>
        private void preprocess()
        {
            if (!hasChanged) return;
            updateBRCandBRM();
            updateBRCXandBRMX();
            firstCalculation = false;
            hasChanged = false;
        }

        /// <summary>計算時間間隔が正常か否か</summary>
        /// <returns>正常の場合は真</returns>
        private bool isTimeStepCorrect()
        {
            foreach (ISurface sf in surfaces)
            {
                if (sf is WallSurface)
                {
                    if (((WallSurface)sf).TimeStep != TimeStep) return false;
                }
            }
            return true;
        }

        /// <summary>状態を更新する</summary>
        public void Update()
        {
            if (!isTimeStepCorrect()) throw new Exception("計算時間間隔が不正です");

            preprocess();

            //室空気状態を更新
            if (ControlDrybulbTemperature)
            {
                drybulbTemperature = DrybulbTemperatureSetPoint;
                CurrentSensibleHeatLoad = GetNextSensibleHeatLoad(drybulbTemperature);
            }
            else
            {
                drybulbTemperature = GetNextDrybulbTemperature(SensibleHeatSupply);
                CurrentSensibleHeatLoad = - SensibleHeatSupply;
            }
            if (ControlAbsoluteHumidity)
            {
                absoluteHumidity = AbsoluteHumiditySetPoint;
                CurrentLatentHeatLoad = GetNextLatentHeatLoad(absoluteHumidity);
            }
            else
            {
                absoluteHumidity = GetNextAbsoluteHumidity(LatentHeatSupply);
                CurrentLatentHeatLoad = - LatentHeatSupply;
            }
            CurrentMeanRadiantTemperature = GetNextMeanRadiantTemperature();

            //登録されている表面に空気温度[C]と放射[W/m2]を設定する
            //放射成分を積算
            double hGainRS = 0;
            double hGainRL = 0;
            foreach (IHeatGain hg in heatGains) hGainRL += hg.GetRadiativeHeatGain(this);
            foreach (WindowSurface ws in windowSurfaces) hGainRS += ws.WindowBody.TransmissionHeatGain;
            for (int i = 0; i < surfaces.Count; i++)
            {
                double sa = surfaces[i].Area;
                //表面近傍の空気温度を設定
                surfaces[i].AirTemperature = surfaces[i].ConvectiveRate * drybulbTemperature;
                //放射[W/m2]を設定
                if (surfaces[i] is WallSurface) surfaces[i].Radiation = (hGainRS * shortWaveRadiationRate[i] + hGainRL * longWaveRadiationRate[i]) / sa
                    + CurrentMeanRadiantTemperature * surfaces[i].RadiativeRate * surfaces[i].OverallHeatTransferCoefficient;
                else if (surfaces[i] is WindowSurface) surfaces[i].Radiation = (hGainRL * longWaveRadiationRate[i]) / sa
                    + CurrentMeanRadiantTemperature * surfaces[i].RadiativeRate * surfaces[i].OverallHeatTransferCoefficient;   //短波長成分は透過
            }

            //時刻を更新
            CurrentDateTime.AddSeconds(timeStep);

            hasChanged = true;
        }

        /// <summary>BRCおよびBRMを更新する</summary>
        private void updateBRCandBRM()
        {
            //室空気の湿り空気比熱[J/(kg-K)]を計算
            double cpAir = MoistAir.GetSpecificHeat(absoluteHumidity) * 1000;

            //室の熱容量[J/K]を更新
            double airSV = MoistAir.GetAirStateFromDBAH(drybulbTemperature, absoluteHumidity, MoistAir.Property.SpecificVolume, AtmosphericPressure);
            double rSH = volume / airSV * cpAir + sensibleHeatCapacity;

            //熱取得[W]を積算
            double hGainC = 0;
            double hGainRS = 0;
            double hGainRL = 0;
            foreach (IHeatGain hg in heatGains)
            {
                hGainC += hg.GetConvectiveHeatGain(this);
                hGainRL += hg.GetRadiativeHeatGain(this);
            }
            foreach (WindowSurface ws in windowSurfaces)
            {
                hGainRS += ws.WindowBody.TransmissionHeatGain;
            }

            //BRMを計算
            //換気量[kg/s]
            double vVol = ventilationVolume / airSV / 3600;
            double caGo = cpAir * vVol; //換気による熱移動量[W/K]
            brm = rSH / timeStep + ar + caGo;

            //BRCを計算
            aft = 0;
            hGainRS /= heatTransferCoefficient;
            hGainRL /= heatTransferCoefficient;
            
            //壁体のCA
            for (int i = 0; i < surfaces.Count; i++)
            {
                double sa = surfaces[i].Area;
                double rsn = 0;
                if(surfaces[i] is WallSurface) rsn = (hGainRS * shortWaveRadiationRate[i] + hGainRL * longWaveRadiationRate[i]) / sa;
                else if (surfaces[i] is WindowSurface) rsn = hGainRL * longWaveRadiationRate[i] / sa;  //短波長成分は透過
                aft += (surfaces[i].FO * surfaces[i].OtherSideSurface.GetSolAirTemperature()
                    + surfaces[i].FI * rsn + surfaces[i].CF) * sa;
            }

            double cA = allSurfaceArea * heatTransferCoefficient * convectiveRate * aft / sdt;
            brc = rSH / timeStep * drybulbTemperature + cA + caGo * ventilationAirState.DryBulbTemperature + hGainC;
        }

        /// <summary>BRCXおよびBRMXを更新する</summary>
        private void updateBRCXandBRMX()
        {
            //室内発生潜熱量を計算
            double le = integrateLatentHeatGain();

            double vVol = ventilationVolume / ventilationAirState.SpecificVolume / 3600;
            double br = (volume / MoistAir.GetAirStateFromDBAH(drybulbTemperature, absoluteHumidity, MoistAir.Property.SpecificVolume) + latentHeatCapacity) / timeStep;
            brmx = br + vVol;
            brcx = br * absoluteHumidity + vVol * ventilationAirState.AbsoluteHumidity + le / (MoistAir.LatentHeatOfVaporization * 1000);
        }

        #endregion

        #region その他のメソッド

        /// <summary>表面リストを取得する</summary>
        /// <returns>表面リスト</returns>
        internal ISurface[] getSurfaces()
        {
            return surfaces.ToArray();
        }

        /// <summary>ゾーンの放射熱取得[W]を積算する</summary>
        /// <param name="isShortWave">短波長放射か否か</param>
        /// <returns>ゾーンの放射熱取得[W]</returns>
        internal double integrateRadiativeHeatGain(bool isShortWave)
        {
            double rh = 0;
            if (isShortWave)
            {
                foreach (WindowSurface ws in windowSurfaces) rh += ws.WindowBody.TransmissionHeatGain;
            }
            else
            {
                foreach (IHeatGain hg in heatGains) rh += hg.GetRadiativeHeatGain(this);
            }
            return rh;
        }

        /// <summary>ゾーンの対流熱取得[W]を積算する</summary>
        /// <returns>ゾーンの対流熱取得[W]</returns>
        internal double integrateConvectiveHeatGain()
        {
            double rh = 0;
            foreach (IHeatGain hg in heatGains) rh += hg.GetConvectiveHeatGain(this);
            return rh;
        }

        /// <summary>ゾーンの潜熱取得[W]を積算する</summary>
        /// <returns>ゾーンの潜熱取得[W]</returns>
        internal double integrateLatentHeatGain()
        {
            double rh = 0;
            foreach (IHeatGain hg in heatGains) rh += hg.GetLatentHeatGain(this);
            return rh;
        }

        /// <summary>蓄熱量[kJ]を取得する</summary>
        /// <param name="initialTemperature">初期温度[C]</param>
        /// <returns>熱量[kJ]</returns>
        public double GetHeatStorage(double initialTemperature)
        {
            //室の熱容量[kJ/K]を計算
            double cpAir = MoistAir.GetSpecificHeat(absoluteHumidity);            
            double airSV = MoistAir.GetAirStateFromDBAH(drybulbTemperature, absoluteHumidity, MoistAir.Property.SpecificVolume, AtmosphericPressure);
            double rSH = volume / airSV * cpAir +sensibleHeatCapacity / 1000d;

            //温度差[K]と熱容量[kJ/K]から蓄熱量[kJ]を計算
            return (drybulbTemperature - initialTemperature) * rSH;
        }

        #endregion

        #region internalメソッド

        /// <summary>乾球温度[C]を設定する</summary>
        /// <param name="dbTemp">乾球温度[C]</param>
        internal void setDrybulbTemperature(double dbTemp)
        {
            this.drybulbTemperature = dbTemp;
        }

        /// <summary>絶対湿度[kg/kg(DA)]を設定する</summary>
        /// <param name="aHumid">絶対湿度[kg/kg(DA)]</param>
        internal void setAbsoluteHumidity(double aHumid)
        {
            this.absoluteHumidity = aHumid;
        }

        /// <summary>平均放射温度[C]を設定する</summary>
        /// <param name="mrt">平均放射温度[C]</param>
        internal void setMeanRadiantTemperature(double mrt)
        {
            this.CurrentMeanRadiantTemperature = mrt;
        }

        #endregion

        #region privateメソッド

        /// <summary>面積変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void surface_AreaChangeEvent(object sender, EventArgs e)
        {
            initializeShortWaveRadiationRate();
            initializeLongWaveRadiationRate();
        }

        #endregion

    }

    #region 読み取り専用のゾーン

    /// <summary>読み取り専用のゾーンinterface</summary>
    public interface ImmutableZone
    {
        /// <summary>名称を取得する</summary>
        string Name
        {
            get;
        }

        /// <summary>室容積[m3]を取得する</summary>
        double Volume
        {
            get;
        }

        /// <summary>計算時間間隔[sec]を取得する</summary>
        double TimeStep
        {
            get;
        }

        /// <summary>空気以外の顕熱容量[J/K]を取得する</summary>
        double SensibleHeatCapacity
        {
            get;
        }

        /// <summary>顕熱供給[W]（暖房を正とする）を取得する</summary>
        double SensibleHeatSupply
        {
            get;
        }

        /// <summary>空気以外の水蒸気容量[kg]を取得する</summary>
        double LatentHeatCapacity
        {
            get;
        }

        /// <summary>潜熱供給[W]（加湿を正とする）を取得する</summary>
        double LatentHeatSupply
        {
            get;
        }

        /// <summary>壁面の総合熱伝達率[W/m2-K]を取得する</summary>
        double HeatTransferCoefficient
        {
            get;
        }

        /// <summary>換気量[m3/h]を取得する</summary>
        double VentilationVolume
        {
            get;
        }

        /// <summary>換気空気状態を取得する</summary>
        ImmutableMoistAir VentilationAirState
        {
            get;
        }

        /// <summary>乾球温度[C]を制御するか否かを取得する</summary>
        bool ControlDrybulbTemperature
        {
            get;
        }

        /// <summary>絶対湿度[kg/kg(DA)]を制御するか否かを取得する</summary>
        bool ControlAbsoluteHumidity
        {
            get;
        }

        /// <summary>乾球温度設定値[C]を取得する</summary>
        double DrybulbTemperatureSetPoint
        {
            get;
        }

        /// <summary>絶対湿度設定値[kg/kg(DA)]を取得する</summary>
        double AbsoluteHumiditySetPoint
        {
            get;
        }

        /// <summary>表面リストを取得する</summary>
        ImmutableSurface[] Surfaces
        {
            get;
        }

        /// <summary>発熱要素を取得する</summary>
        IHeatGain[] HeatGains
        {
            get;
        }

        /// <summary>大気圧[kPa]を取得する</summary>
        double AtmosphericPressure
        {
            get;
        }

        /// <summary>現在の乾球温度[C]を取得する</summary>
        double CurrentDrybulbTemperature
        {
            get;
        }

        /// <summary>現在の絶対湿度[kg/kg]を取得する</summary>
        double CurrentAbsoluteHumidity
        {
            get;
        }

        /// <summary>現在の顕熱負荷[kW]を取得する</summary>
        double CurrentSensibleHeatLoad
        {
            get;
        }

        /// <summary>現在の潜熱負荷[kW]を取得する</summary>
        double CurrentLatentHeatLoad
        {
            get;
        }

        /// <summary>現在の平均放射温度[C]を取得する</summary>
        double CurrentMeanRadiantTemperature
        {
            get;
        }

        /// <summary>現在の日時を取得する</summary>
        DateTime CurrentDateTime
        {
            get;
        }

        /// <summary>蓄熱量[kJ]を取得する</summary>
        /// <param name="initialTemperature">初期温度[C]</param>
        /// <returns>熱量[kJ]</returns>
        double GetHeatStorage(double initialTemperature);

    }

    #endregion

}
