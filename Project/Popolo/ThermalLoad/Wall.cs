/* Wall.cs
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

using Popolo.Weather;

using GSLNET;

namespace Popolo.ThermalLoad
{
    /// <summary>壁体クラス</summary>
    public class Wall : ImmutableWall
    {

        #region event定義

        /// <summary>FIおよびFO変更イベント</summary>
        public event EventHandler FIOChangeEvent;

        /// <summary>面積変更イベント</summary>
        public event EventHandler AreaChangeEvent;

        #endregion

        #region インスタンス変数

        /// <summary>壁面積[m2]</summary>
        private double surfaceArea = 1;

        /// <summary>1側壁表面</summary>
        private WallSurface wallSurface1;

        /// <summary>2側壁表面</summary>
        private WallSurface wallSurface2;

        /// <summary>1側の傾斜情報（デフォルトでは南向き垂直壁）</summary>
        private ImmutableIncline incline1 = new Incline(Incline.Orientation.S, 0.5 * Math.PI);

        /// <summary>2側の傾斜情報（デフォルトでは北向き垂直壁）</summary>
        private ImmutableIncline incline2 = new Incline(Math.PI, 0.5 * Math.PI);

        /// <summary>壁構成</summary>
        private ImmutableWallLayers wallLayers;

        /// <summary>計算時間間隔[sec]</summary>
        private double timeStep = 3600;

        /// <summary>逆行列</summary>
        private Matrix uxMatrix = new Matrix(1, 1);

        /// <summary>CFベクトル</summary>
        private Vector cfVector = new Vector(1);

        /// <summary>内部温度[C]ベクトル</summary>
        private Vector temperatures = new Vector(1);

        /// <summary>FI,FO成分ベクトル</summary>
        private Matrix ux0mMatrix = new Matrix(1, 1);

        /// <summary>凍結保存された温度分布ベクトル</summary>
        private Vector temperaturesFRZ = new Vector(1);

        /// <summary>最後に計算した際の壁近傍の相当温度[C]</summary>
        private double prevSolAirTemp1, prevSolAirTemp2;

        /// <summary>壁体内に埋め込まれたチューブ</summary>
        private Dictionary<uint, Tube> tubes = new Dictionary<uint, Tube>();

        /// <summary>潜熱蓄熱材料</summary>
        private Dictionary<uint, LatentHeatStorageMaterial> lMaterials = new Dictionary<uint, LatentHeatStorageMaterial>();
        private Dictionary<uint, int> lMaterialIndex = new Dictionary<uint, int>();

        /// <summary>Updateメソッドが呼ばれたか否か</summary>
        private bool hasUpdated = false;

        /// <summary>一時記憶領域</summary>
        private Matrix uMatrix = new Matrix(1, 1);
        private Matrix uMatrix2 = new Matrix(1, 1);
        private Permutation perm = new Permutation(1);
        private double[] uL = new double[1];
        private double[] uR = new double[1];
        private double[] res = new double[2];
        private double[] cap = new double[2];
        private Vector solAirTemp = new Vector(2);
        private Vector fpt1 = new Vector(2);
        private Vector fpt2 = new Vector(2);

        /// <summary>チューブの流量変更の真偽</summary>
        private bool hasTubeFlowRateChanged = false;

        #endregion

        #region プロパティ

        /// <summary>名称を設定・取得する</summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>1側のFPTを取得する</summary>
        public double FPT1
        {
            get
            {
                return fpt2.GetValue(0);
            }
        }

        /// <summary>2側のFPTを取得する</summary>
        public double FPT2
        {
            get
            {
                return fpt2.GetValue(fpt2.Size - 1);
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
                if (0 < value && timeStep != value)
                {
                    timeStep = value;
                    initializeUXMatrix();
                }
            }
        }

        /// <summary>壁面積[m2]を設定・取得する</summary>
        public double SurfaceArea
        {
            set
            {
                if (0 < value)
                {
                    surfaceArea = value;
                    if (AreaChangeEvent != null) AreaChangeEvent(this, new EventArgs());
                }
            }
            get
            {
                return surfaceArea;
            }
        }

        /// <summary>壁構成を設定・取得する</summary>
        public ImmutableWallLayers Layers
        {
            set
            {
                wallLayers = value;
            }
            get
            {
                return wallLayers;
            }
        }

        /// <summary>1側の空気温度[℃]を設定・取得する</summary>
        public double AirTemperature1
        {
            get
            {
                return wallSurface1.AirTemperature;
            }
            set
            {
                wallSurface1.AirTemperature = value;
            }
        }

        /// <summary>2側の空気温度[℃]を設定・取得する</summary>
        public double AirTemperature2
        {
            get
            {
                return wallSurface2.AirTemperature;
            }
            set
            {
                wallSurface2.AirTemperature = value;
            }
        }

        /// <summary>1側の放射量[W/m2]を設定・取得する</summary>
        public double Radiation1
        {
            get
            {
                return wallSurface1.Radiation;
            }
            set
            {
                wallSurface1.Radiation = value;
            }
        }

        /// <summary>2側の放射量[W/m2]を設定・取得する</summary>
        public double Radiation2
        {
            get
            {
                return wallSurface2.Radiation;
            }
            set
            {
                wallSurface2.Radiation = value;
            }
        }

        /// <summary>1側の傾斜面情報を取得する</summary>
        public ImmutableIncline Incline1
        {
            get
            {
                return incline1;
            }
        }

        /// <summary>2側の傾斜面情報を取得する</summary>
        public ImmutableIncline Incline2
        {
            get
            {
                return incline2;
            }
        }

        /// <summary>FI1を取得する</summary>
        internal double FI1
        {
            get
            {
                return ux0mMatrix.GetValue(0, 0);
            }
        }

        /// <summary>FO1を取得する</summary>
        internal double FO1
        {
            get
            {
                return ux0mMatrix.GetValue(0, 1);
            }
        }

        /// <summary>FI2を取得する</summary>
        internal double FI2
        {
            get
            {
                return ux0mMatrix.GetValue(ux0mMatrix.Columns - 1, 1);
            }
        }

        /// <summary>FO2を取得する</summary>
        internal double FO2
        {
            get
            {
                return ux0mMatrix.GetValue(ux0mMatrix.Columns - 1, 0);
            }
        }

        /// <summary>1側のCFを取得する</summary>
        internal double CF1
        {
            get
            {
                return cfVector.GetValue(0);
            }
        }

        /// <summary>2側のCFを取得する</summary>
        internal double CF2
        {
            get
            {
                return cfVector.GetValue(cfVector.Size - 1);
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="wallLayers">壁構成</param>
        /// <param name="name">壁体名称</param>
        public Wall(ImmutableWallLayers wallLayers, string name)
        {
            this.Name = name;
            wallSurface1 = new WallSurface(this, true);
            wallSurface2 = new WallSurface(this, false);

            this.wallLayers = (ImmutableWallLayers)wallLayers.Clone();
            initialize();

            //表面の総合熱伝達率変更イベントに登録
            wallSurface1.FilmCoefficientChangeEvent += new EventHandler(wallSurface_FilmCoefficientChangeEvent);
            wallSurface2.FilmCoefficientChangeEvent += new EventHandler(wallSurface_FilmCoefficientChangeEvent);
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="wallLayers">壁構成</param>
        public Wall(ImmutableWallLayers wallLayers)
        {
            wallSurface1 = new WallSurface(this, true);
            wallSurface2 = new WallSurface(this, false);

            this.wallLayers = (ImmutableWallLayers)wallLayers.Clone();
            initialize();

            //表面の総合熱伝達率変更イベントに登録
            wallSurface1.FilmCoefficientChangeEvent += new EventHandler(wallSurface_FilmCoefficientChangeEvent);
            wallSurface2.FilmCoefficientChangeEvent += new EventHandler(wallSurface_FilmCoefficientChangeEvent);
        }

        /// <summary>壁表面の総合熱伝達率変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wallSurface_FilmCoefficientChangeEvent(object sender, EventArgs e)
        {
            initialize();
        }

        #endregion

        #region publicメソッド//熱伝導計算関連の処理

        /// <summary>状態を更新する</summary>
        public void Update()
        {
            //壁体内温度を更新する
            updateTemperatures();

            //潜熱蓄熱材の状態番号を保存
            foreach (uint index in lMaterials.Keys)
            {
                lMaterialIndex[index] = lMaterials[index].CurrentMaterialIndex;
            }

            //冷温水流量が変化した場合にはUXマトリクスを更新
            if (hasTubeFlowRateChanged)
            {
                addTubeCoefToUMatrix();
                makeUXMatrix();
                hasTubeFlowRateChanged = false;
            }
            Blas.DGemv(Blas.TransposeType.NoTranspose, 1.0, uxMatrix, temperatures, 0.0d, ref cfVector);

            //FPTを更新する
            if (tubes.Count != 0)
            {
                for (uint i = 0; i < fpt1.Size; i++)
                {
                    if (tubes.ContainsKey(i - 1)) fpt1.SetValue(i, getPcWp(i - 1) * tubes[i - 1].FluidTemperature);
                    else fpt1.SetValue(i, 0);
                }
                Blas.DGemv(Blas.TransposeType.NoTranspose, 1.0, uxMatrix, fpt1, 0.0d, ref fpt2);
            }

            //チューブに交換熱量[W]を設定
            foreach (uint index in tubes.Keys)
            {
                tubes[index].HeatTransferToFluid = GetHeatTransferToTube(index);
            }

            hasUpdated = true;
        }

        /// <summary>周辺の空気から壁への熱移動量[W]を計算する</summary>
        /// <param name="isSide1">1側か否か</param>
        /// <returns>周辺の空気から壁への熱移動量[W]</returns>
        public double GetHeatTransfer(bool isSide1)
        {
            if (isSide1) return wallSurface1.GetHeatTransfer();
            else return wallSurface2.GetHeatTransfer();
        }

        /// <summary>壁面温度[℃]を取得する</summary>
        /// <param name="isSide1">1側か否か</param>
        /// <returns>壁面温度[℃]</returns>
        public double GetWallTemprature(bool isSide1)
        {
            updateTemperatures();
            if (isSide1) return temperatures.GetValue(0);
            else return temperatures.GetValue(temperatures.Size - 1);
        }

        /// <summary>壁体内部の温度分布を取得する</summary>
        /// <returns>壁体内部の温度分布</returns>
        public double[] GetTemperatures()
        {
            updateTemperatures();
            return temperatures.ToArray();
        }

        /// <summary>相当温度[C]を計算する</summary>
        /// <param name="isSide1">1側か否か</param>
        /// <returns>相当温度[C]</returns>
        public double GetSolAirTemperature(bool isSide1)
        {
            if (isSide1) return wallSurface1.GetSolAirTemperature();
            else return wallSurface2.GetSolAirTemperature();
        }

        /// <summary>熱貫流率[W/(m^2K)]を計算する</summary>
        /// <returns>熱貫流率[W/(m^2K)]</returns>
        public double GetFilmCoefficient()
        {
            return Layers.GetThermalTransmission(wallSurface1.FilmCoefficient, wallSurface2.FilmCoefficient);
        }

        /// <summary>定常状態において壁を通過して表面1から表面2へと向かう熱移動量[W]を計算する</summary>
        /// <returns>定常状態において壁を通過して表面1から表面2へと向かう熱移動量[W]</returns>
        public double GetStaticHeatTransfer()
        {
            return GetFilmCoefficient() * SurfaceArea *
                (wallSurface1.GetSolAirTemperature() - wallSurface2.GetSolAirTemperature());
        }

        /// <summary>冷温水配管への熱移動量[W]を計算する</summary>
        /// <param name="index">冷温水配管が埋設されている層の番号</param>
        /// <returns>冷温水配管への熱移動量[W]</returns>
        public double GetHeatTransferToTube(uint index)
        {
            if (tubes.ContainsKey(index))
            {
                updateTemperatures();

                ImmutableTube tube = tubes[index];
                double wp = getWp(index);
                return wp * (temperatures.GetValue(index + 1) - tube.FluidTemperature) * surfaceArea;
            }
            else return 0;
        }

        #endregion

        #region publicメソッド//モデル構築関連の処理

        /// <summary>傾斜を設定する</summary>
        /// <param name="incline">傾斜面</param>
        /// <param name="isSide1">1側か否か</param>
        /// <remarks>裏面は自動で設定される</remarks>
        public void SetIncline(ImmutableIncline incline, bool isSide1)
        {
            if (isSide1)
            {
                incline1 = incline;
                Incline ic = new Incline(incline1);
                ic.Reverse();
                incline2 = ic;
            }
            else
            {
                incline2 = incline;
                Incline ic = new Incline(incline2);
                ic.Reverse();
                incline1 = ic;
            }
        }

        /// <summary>壁表面オブジェクトを取得する</summary>
        /// <param name="isSide1">壁面1か否か</param>
        /// <returns>壁表面オブジェクト</returns>
        public WallSurface GetSurface(bool isSide1)
        {
            if (isSide1) return wallSurface1;
            else return wallSurface2;
        }

        /// <summary>表面熱伝達率[W/(m^2K)]を設定する</summary>
        /// <param name="hCoef">表面熱伝達率[W/(m^2K)]</param>
        /// <param name="isSide1">1側か否か</param>
        public void SetFilmCoefficient(double hCoef, bool isSide1)
        {
            if (isSide1) wallSurface1.FilmCoefficient = hCoef;
            else wallSurface2.FilmCoefficient = hCoef;
        }

        /// <summary>表面熱伝達率[W/(m^2K)]を設定する</summary>
        /// <param name="hCoef1">1側表面熱伝達率[W/(m^2K)]</param>
        /// <param name="hCoef2">2側表面熱伝達率[W/(m^2K)]</param>
        public void SetFilmCoefficient(double hCoef1, double hCoef2)
        {
            wallSurface1.FilmCoefficient = hCoef1;
            wallSurface2.FilmCoefficient = hCoef2;
        }

        /// <summary>表面熱伝達率[W/(m^2K)]を取得する</summary>
        /// <param name="isSide1">1側か否か</param>
        /// <returns>表面熱伝達率[W/(m^2K)]</returns>
        public double GetFilmCoefficient(bool isSide1)
        {
            if (isSide1) return wallSurface1.FilmCoefficient;
            else return wallSurface2.FilmCoefficient;
        }

        /// <summary>総合熱伝達率[W/m2-K]のうち、対流熱伝達の割合[-]を設定する</summary>
        /// <param name="convectiveRate">対流熱伝達の割合[-]</param>
        /// <param name="isSide1">1側か否か</param>
        public void SetConvectiveRate(double convectiveRate, bool isSide1)
        {
            if (isSide1) wallSurface1.ConvectiveRate = convectiveRate;
            else wallSurface2.ConvectiveRate = convectiveRate;
        }

        /// <summary>総合熱伝達率[W/m2-K]のうち、放射熱伝達の割合[-]を設定する</summary>
        /// <param name="radiativeRate">放射熱伝達の割合[-]</param>
        /// <param name="isSide1">1側か否か</param>
        public void SetRadiativeRate(double radiativeRate, bool isSide1)
        {
            if (isSide1) wallSurface1.RadiativeRate = radiativeRate;
            else wallSurface2.RadiativeRate = radiativeRate;
        }

        /// <summary>総合熱伝達率[W/m2-K]のうち、対流熱伝達の割合[-]を取得する</summary>
        /// <param name="isSide1">1側か否か</param>
        /// <returns>対流熱伝達の割合[-]</returns>
        public double GetConvectiveRate(bool isSide1)
        {
            if (isSide1) return wallSurface1.ConvectiveRate;
            else return wallSurface2.ConvectiveRate;
        }

        /// <summary>総合熱伝達率[W/m2-K]のうち、放射熱伝達の割合[-]を取得する</summary>
        /// <param name="isSide1">1側か否か</param>
        /// <returns>放射熱伝達の割合[-]</returns>
        public double GetRadiativeRate(bool isSide1)
        {
            if (isSide1) return wallSurface1.RadiativeRate;
            else return wallSurface2.RadiativeRate;
        }

        /// <summary>壁層に配管を埋め込む</summary>
        /// <param name="tube">配管</param>
        /// <param name="index">壁層番号（壁体の分割数を加算した番号）</param>
        public void AddTube(Tube tube, uint index)
        {
            if (tubes.ContainsKey(index))
            {
                tubes[index].FlowRateChangeEvent -= flowRateChangeEvent;
            }
            tubes[index] = tube;
            hasTubeFlowRateChanged = true;
            tubes[index].FlowRateChangeEvent += flowRateChangeEvent;
        }

        /// <summary>壁層から配管外す</summary>
        /// <param name="index">壁層番号（壁体の分割数を加算した番号）</param>
        public void RemoveTube(uint index)
        {
            if (tubes.ContainsKey(index))
            {
                tubes[index].FlowRateChangeEvent -= flowRateChangeEvent;
                tubes.Remove(index);
            }
        }

        /// <summary>チューブの流量変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void flowRateChangeEvent(object sender, EventArgs e)
        {
            hasTubeFlowRateChanged = true;
        }

        #endregion

        #region publicメソッド//凍結・解凍処理

        /// <summary>状態を凍結保存する</summary>
        public void FreezeState()
        {
            cfVector.CopyTo(ref temperaturesFRZ);
        }

        /// <summary>状態を凍結保存する</summary>
        /// <param name="state">状態を保存する実数値配列</param>
        public void FreezeState(out double[] state)
        {
            state = cfVector.ToArray();
        }

        /// <summary>状態を解凍復元する</summary>
        public void DefrostState()
        {
            temperaturesFRZ.CopyTo(ref cfVector);
            hasUpdated = false;
        }

        /// <summary>状態を解凍復元する</summary>
        /// <param name="state">状態を示す実数値配列</param>
        public void DefrostState(double[] state)
        {
            temperaturesFRZ.FromArray(state);
        }

        #endregion

        #region publicメソッド//潜熱蓄熱材料設定関連の処理

        /// <summary>潜熱蓄熱材料を設定する</summary>
        /// <param name="layerIndex">潜熱蓄熱材料を設定する壁層番号</param>
        /// <param name="lMaterial">潜熱蓄熱材料</param>
        public void SetLatentHeatStorageMaterial(uint layerIndex, LatentHeatStorageMaterial lMaterial)
        {
            //壁層の数以下の場合は設定
            if (layerIndex < cap.Length - 2)
            {
                lMaterials[layerIndex] = lMaterial;
                lMaterialIndex[layerIndex] = lMaterial.CurrentMaterialIndex;
            }
            //初期化
            initialize();
        }

        /// <summary>潜熱蓄熱材料の設定を解除する</summary>
        /// <param name="layerIndex">潜熱蓄熱材料の設定を解除する壁層番号</param>
        public void RemoveLatentHeatStorageMaterial(uint layerIndex)
        {
            if (lMaterials.ContainsKey(layerIndex))
            {
                lMaterials.Remove(layerIndex);
                lMaterialIndex.Remove(layerIndex);
            }
            //初期化
            initialize();
        }

        /// <summary>潜熱蓄熱材料の状態番号を取得する</summary>
        /// <param name="layerIndex">潜熱蓄熱材料が設定されている壁層番号</param>
        /// <returns>潜熱蓄熱材料の状態番号</returns>
        public uint GetLatentHeatStorageMaterialIndex(uint layerIndex)
        {
            double aveT = (temperatures.GetValue(layerIndex) + temperatures.GetValue(layerIndex + 1)) / 2;
            return lMaterials[layerIndex].getMaterialIndex(aveT);
        }

        #endregion

        #region その他のpublicメソッド

        /// <summary>壁温を初期化する</summary>
        /// <param name="wallTemperature">壁温[C]</param>
        public void InitializeTemperature(double wallTemperature)
        {
            solAirTemp.SetValue(0, GetSolAirTemperature(true));
            solAirTemp.SetValue(1, GetSolAirTemperature(false));
            bool hasLMat = false;
            foreach (uint index in lMaterials.Keys)
            {
                lMaterials[index].Initialize(wallTemperature);
                lMaterialIndex[index] = lMaterials[index].CurrentMaterialIndex;
                hasLMat = true;
            }
            //潜熱材料がある場合にはcap配列等も初期化
            if (hasLMat) initialize();

            initializeUXMatrix();
            temperatures.SetValue(wallTemperature);
            cfVector.SetValue(wallTemperature);
            Blas.DGemv(Blas.TransposeType.NoTranspose, -1, ux0mMatrix, solAirTemp, 1, ref cfVector);

            hasUpdated = true;
            updateTemperatures();
        }

        /// <summary>壁体の蓄熱量[kJ]を計算する</summary>
        /// <param name="temperature1">初期温度[C]</param>
        /// <returns>壁体の蓄熱量[kJ]</returns>
        public double GetHeatStorage(double temperature1)
        {
            double heatStorage = 0;
            for (uint i = 0; i < wallLayers.LayerNumber; i++)
            {
                WallLayers.Layer layer = wallLayers.GetLayer(i);
                double temp = (temperatures.GetValue(i) + temperatures.GetValue(i + 1)) / 2;
                if (lMaterials.ContainsKey(i))
                {
                    heatStorage += lMaterials[i].GetHeatStorage(temperature1, temp) * layer.Thickness;
                }
                else
                {                    
                    heatStorage += layer.HeatCapacityPerUnitArea * (temp - temperature1) / 1000d;
                }
            }
            return heatStorage * surfaceArea;
        }

        /// <summary>壁体の蓄熱量[kJ]を計算する</summary>
        /// <param name="temps">初期温度[C]</param>
        /// <returns>壁体の蓄熱量[kJ]</returns>
        public double GetHeatStorage(double[] temps)
        {
            double heatStorage = 0;
            for (uint i = 0; i < wallLayers.LayerNumber; i++)
            {
                WallLayers.Layer layer = wallLayers.GetLayer(i);
                double temp = (temperatures.GetValue(i) + temperatures.GetValue(i + 1)) / 2;
                if (lMaterials.ContainsKey(i))
                {
                    heatStorage += lMaterials[i].GetHeatStorage(temps[i], temp) * layer.Thickness;
                }
                else
                {
                    heatStorage += layer.HeatCapacityPerUnitArea * (temp - temps[i]) / 1000d;
                }
            }
            return heatStorage * surfaceArea;
        }

        #endregion

        #region privateメソッド

        /// <summary>初期化する</summary>
        private void initialize()
        {
            WallLayers.Layer[] layers = wallLayers.GetLayer();
            if (layers.Length == 0) return;

            //接点情報を作成
            uint mNumber = (uint)wallLayers.LayerNumber + 2;
            res = new double[mNumber];
            cap = new double[mNumber];

            res[0] = 1d / wallSurface1.FilmCoefficient;
            cap[0] = 0;
            res[res.Length - 1] = 1d / wallSurface2.FilmCoefficient;
            cap[cap.Length - 1] = 0;
            for (uint i = 0; i < layers.Length; i++)
            {
                if (lMaterials.ContainsKey(i))
                {
                    LatentHeatStorageMaterial lm = lMaterials[i];
                    res[i + 1] = layers[i].Thickness / lm.CurrentMaterial.ThermalConductivity;
                    cap[i + 1] = layers[i].Thickness * lm.CurrentMaterial.VolumetricSpecificHeat * 1000d;
                }
                else
                {
                    res[i + 1] = layers[i].Resistance;
                    cap[i + 1] = layers[i].HeatCapacityPerUnitArea;
                }
            }

            initializeUXMatrix();
        }

        /// <summary>UXマトリクスを初期化する</summary>
        private void initializeUXMatrix()
        {
            //Uマトリクスを作成
            makeUMatrix();
            //チューブの係数を追加
            addTubeCoefToUMatrix();
            //逆行列を作成
            makeUXMatrix();
        }

        /// <summary>UX行列を更新する</summary>
        private void makeUMatrix()
        {
            uint mNumber = (uint)res.Length - 1;

            //逆行列計算領域を確保
            if (uMatrix.Columns != mNumber)
            {
                uMatrix = new Matrix(mNumber, mNumber);
                uMatrix2 = new Matrix(mNumber, mNumber);
                perm = new Permutation(mNumber);
                uxMatrix = new Matrix(mNumber, mNumber);
                cfVector = new Vector(mNumber);                
                uL = new double[mNumber];
                uR = new double[mNumber];
                ux0mMatrix = new Matrix(mNumber, 2);
                temperatures = new Vector(mNumber);
                temperaturesFRZ = new Vector(mNumber);
                fpt1 = new Vector(mNumber);
                fpt2 = new Vector(mNumber);
            }
            uMatrix.InitializeValue(0);
            perm.Initialize();

            for (int i = 0; i < mNumber; i++)
            {
                double c = 0.5 * (cap[i] + cap[i + 1]);
                uL[i] = timeStep / (c * res[i]);
                uR[i] = timeStep / (c * res[i + 1]);
            }

            uMatrix.SetValue(0, 0, 1d + uL[0] + uR[0]);
            for (uint i = 1; i < mNumber; i++)
            {
                uMatrix.SetValue(i, i, 1d + uL[i] + uR[i]);
                uMatrix.SetValue(i, i - 1, -uL[i]);                
                uMatrix.SetValue(i - 1, i, -uR[i - 1]);
            }
        }

        /// <summary>チューブの係数を設定する</summary>
        private void addTubeCoefToUMatrix()
        {
            //uMatrix2
            for (uint i = 0; i < uMatrix.Columns; i++)
            {
                for (uint j = 0; j < uMatrix.Rows; j++)
                {
                    if (i == j && tubes.ContainsKey(i - 1))
                    {
                        uMatrix2.SetValue(i, j, uMatrix.GetValue(i, j) + getPcWp(i - 1));
                    }
                    else
                    {
                        uMatrix2.SetValue(i, j, uMatrix.GetValue(i, j));
                    }                    
                }
            }
        }

        /// <summary>UXマトリクスを作成する</summary>
        private void makeUXMatrix()
        {
            uint mNumber = (uint)res.Length - 1;

            double ul0 = uL[0];
            double urm = uR[mNumber - 1];

            //逆行列の計算
            int sig;
            LinearAlgebra.LUDecomposition(ref uMatrix2, ref perm, out sig);
            LinearAlgebra.LUInvert(uMatrix2, perm, ref uxMatrix);

            //FIの計算
            for (uint i = 0; i < mNumber; i++)
            {
                ux0mMatrix.SetValue(i, 0, ul0 * uxMatrix.GetValue(i, 0));
                ux0mMatrix.SetValue(i, 1, urm * uxMatrix.GetValue(i, uxMatrix.Columns - 1));
            }

            //FIO変更イベント
            if (FIOChangeEvent != null) FIOChangeEvent(this, new EventArgs());
        }

        /// <summary>PCWPを計算する</summary>
        /// <param name="index"></param>
        /// <returns>PCWP</returns>
        private double getPcWp(uint index)
        {
            ImmutableTube tube = tubes[index];
            if (tube.FluidFlowRate == 0) return 0;

            double pc = timeStep / (0.5 * (cap[index + 1] + cap[index + 2]));
            double cf = 1d / res[index + 1] + 1d / res[index + 2];
            double ecga = tube.Epsilon * tube.FluidSpecificHeat * tube.FluidFlowRate / surfaceArea;
            double wp = ecga / (1d + ecga / cf * (1d / tube.FinEfficiency - 1));
            return pc * wp;
        }

        /// <summary>WPを計算する</summary>
        /// <param name="index"></param>
        /// <returns>WP</returns>
        private double getWp(uint index)
        {
            ImmutableTube tube = tubes[index];
            if (tube.FluidFlowRate == 0) return 0;

            double cf = 1d / res[index + 1] + 1d / res[index + 2];
            double ecga = tube.Epsilon * tube.FluidSpecificHeat * tube.FluidFlowRate / surfaceArea;
            return ecga / (1d + ecga / cf * (1d / tube.FinEfficiency - 1));
        }

        /// <summary>壁内の温度分布を更新する</summary>
        private void updateTemperatures()
        {
            double sa1 = GetSolAirTemperature(true);
            double sa2 = GetSolAirTemperature(false);

            //周囲の温度・放射量に変化がなくUpdateされていない場合は終了
            if (prevSolAirTemp1 == sa1 &&
                prevSolAirTemp2 == sa2 &&
                !hasUpdated) return;

            prevSolAirTemp1 = sa1;
            prevSolAirTemp2 = sa2;
            hasUpdated = false;

            //両端の相当温度を利用して温度を更新
            solAirTemp.SetValue(0, sa1);
            solAirTemp.SetValue(1, sa2);
            cfVector.CopyTo(ref temperatures);
            Blas.DGemv(Blas.TransposeType.NoTranspose, 1, ux0mMatrix, solAirTemp, 1, ref temperatures);

            //冷温水チューブの影響を加える
            if (tubes.Count != 0)
            {
                for (uint i = 0; i < temperatures.Size; i++) temperatures.AddValue(i, fpt2.GetValue(i));
            }

            //潜熱蓄熱材料の相変化を確認
            if (lMaterials.Count != 0)
            {
                //潜熱蓄熱材の状態番号を復元
                foreach (uint index in lMaterials.Keys)
                {
                    lMaterials[index].CurrentMaterialIndex = lMaterialIndex[index];
                }

                bool needUXMUpdate = false;
                //各潜熱蓄熱層の状態を更新（温度に従って状態を割り戻す）                
                foreach (uint index in lMaterials.Keys)
                {
                    LatentHeatStorageMaterial lMat = lMaterials[index];
                    bool hasPhaseChanged = lMat.updateState((temperatures.GetValue(index) + temperatures.GetValue(index + 1)) / 2d);
                    //状態が変化した場合
                    if (hasPhaseChanged)
                    {
                        needUXMUpdate = true;

                        //左端の場合//左端の潜熱蓄熱材の温度を設定
                        if (index == 0) temperatures.SetValue(0, lMaterials[0].CurrentTemperature);
                        //右端の場合//右端の潜熱蓄熱材の温度を設定
                        else if (index == temperatures.Size - 2) temperatures.SetValue(temperatures.Size - 1, lMaterials[index].CurrentTemperature);
                        //その他の場合//左右の材料の平均温度を設定
                        else
                        {
                            double leftRate = temperatures.GetValue(index) / (temperatures.GetValue(index) + temperatures.GetValue(index + 1)) * 2;
                            temperatures.SetValue(index, getAverageTemperature(index, temperatures.GetValue(index), lMat.CurrentTemperature * leftRate));
                            temperatures.SetValue(index + 1, getAverageTemperature(index + 1, lMat.CurrentTemperature * (2 - leftRate), temperatures.GetValue(index + 1)));
                        }
                    }
                }

                //潜熱蓄熱材が状態変化した場合にはUXMatrixを更新
                if (needUXMUpdate) initialize();
            }
        }

        private double getAverageTemperature(uint index, double tmp1, double tmp2)
        {
            WallLayers.Layer layer1 = wallLayers.GetLayer(index - 1);
            WallLayers.Layer layer2 = wallLayers.GetLayer(index);
            double vsh1, vsh2;
            if (lMaterials.ContainsKey(index)) vsh1 = lMaterials[index].CurrentMaterial.VolumetricSpecificHeat;
            else vsh1 = layer1.Material.VolumetricSpecificHeat;
            if (lMaterials.ContainsKey(index + 1)) vsh2 = lMaterials[index + 1].CurrentMaterial.VolumetricSpecificHeat;
            else vsh2 = layer2.Material.VolumetricSpecificHeat;
            double cp1 = layer1.Thickness * vsh1;
            double cp2 = layer2.Thickness * vsh2;
            double q = cp1 * tmp1 + cp2 * tmp2;
            return q / (cp1 + cp2);
        }

        #endregion

    }

    #region 読み取り専用の壁体interface

    /// <summary>読み取り専用の壁体</summary>
    public interface ImmutableWall
    {

        /// <summary>名称を取得する</summary>
        string Name
        {
            get;
        }

        /// <summary>壁面積[m2]を取得する</summary>
        double SurfaceArea
        {
            get;
        }

        /// <summary>壁構成を取得する</summary>
        ImmutableWallLayers Layers
        {
            get;
        }

        /// <summary>1側空気温度[℃]を取得する</summary>
        double AirTemperature1
        {
            get;
        }

        /// <summary>2側空気温度[℃]を取得する</summary>
        double AirTemperature2
        {
            get;
        }

        /// <summary>1側の放射量[W/m2]を取得する</summary>
        double Radiation1
        {
            get;
        }

        /// <summary>2側の放射量[W/m2]を取得する</summary>
        double Radiation2
        {
            get;
        }

        /// <summary>1側の傾斜面情報を取得する</summary>
        ImmutableIncline Incline1
        {
            get;
        }

        /// <summary>2側の傾斜面情報を取得する</summary>
        ImmutableIncline Incline2
        {
            get;
        }

        /// <summary>1側のFPTを取得する</summary>
        double FPT1
        {
            get;
        }

        /// <summary>2側のFPTを取得する</summary>
        double FPT2
        {
            get;
        }

        /// <summary>計算時間間隔[sec]を取得する</summary>
        double TimeStep
        {
            get;
        }

        /// <summary>表面熱伝達率[W/(m^2K)]を取得する</summary>
        /// <param name="isSide1">1側か否か</param>
        /// <returns>表面熱伝達率[W/(m^2K)]</returns>
        double GetFilmCoefficient(bool isSide1);

        /// <summary>熱貫流率[W/(m^2K)]を取得する</summary>
        /// <returns>熱貫流率[W/(m^2K)]</returns>
        double GetFilmCoefficient();

        /// <summary>壁面温度[℃]を取得する</summary>
        /// <param name="isSide1">1側か否か</param>
        /// <returns>壁面温度[℃]</returns>
        double GetWallTemprature(bool isSide1);

        /// <summary>壁体の蓄熱量[kJ]を計算する</summary>
        /// <param name="temperature1">初期温度[C]</param>
        /// <returns>壁体の蓄熱量[kJ]</returns>
        double GetHeatStorage(double temperature1);

        /// <summary>壁体の蓄熱量[kJ]を計算する</summary>
        /// <param name="temps">初期温度[C]</param>
        /// <returns>壁体の蓄熱量[kJ]</returns>
        double GetHeatStorage(double[] temps);

        /// <summary>温度分布を取得する</summary>
        double[] GetTemperatures();

        /// <summary>相当温度[C]を計算する</summary>
        /// <param name="isSide1">1側か否か</param>
        /// <returns>相当温度[C]</returns>
        double GetSolAirTemperature(bool isSide1);

        /// <summary>チューブへの熱移動量[W]を計算する</summary>
        /// <param name="index">チューブが設定されている層の番号</param>
        /// <returns>チューブへの熱移動量[W]</returns>
        double GetHeatTransferToTube(uint index);

    }

    #endregion

}
