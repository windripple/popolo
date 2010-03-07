/* BodyPart.cs
 * 
 * Copyright (C) 2009 E.Togashi, S.Tanabe
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

using GSLNET;

namespace Popolo.ThermalComfort
{
    /// <summary>体の部位</summary>
    public class BodyPart : ImmutableBodyPart
    {

        #region 定数宣言

        /// <summary>ルイス係数[K/kPa]</summary>
        private const double LOUIS_COEFFICIENT = 16.5;

        #endregion

        #region 列挙型定義

        /// <summary>要素</summary>
        [Flags]
        public enum Segments
        {
            /// <summary>無し</summary>
            None = 0,
            /// <summary>コア層</summary>
            Core = 1,
            /// <summary>筋肉層</summary>
            Muscle = 2,
            /// <summary>脂肪層</summary>
            Fat = 4,
            /// <summary>皮膚層</summary>
            Skin = 8,
            /// <summary>動脈</summary>
            Artery = 16,
            /// <summary>表在静脈</summary>
            SuperficialVein = 32,
            /// <summary>深部動脈</summary>
            DeepVein = 64,
            /// <summary>AVA</summary>
            AVA = 128
        }

        #endregion

        #region インスタンス変数

        #region その他

        /// <summary>JOSモデル（脂肪層および筋肉層をコア層に統合）か否か</summary>
        private readonly bool isJOSModel = true;

        /// <summary>接続先の体部位</summary>
        internal List<BodyPart> bpConnectTo = new List<BodyPart>();

        /// <summary>接続元の体部位</summary>
        internal BodyPart bpConnectFrom = null;

        /// <summary>AVA血管への血流割合</summary>
        private double avaRate = 0;

        /// <summary>皮膚接触部の割合[-]</summary>
        private double contactPortionRate = 0;

        /// <summary>着衣量[clo]</summary>
        private double clothingIndex;

        /// <summary>気流速度[m/s]</summary>
        private double velocity;

        #endregion

        #region 血流量

        /// <summary>皮膚の基礎血流量[L/h]</summary>
        private double baseBloodFlow_Skin = 0;

        /// <summary>筋肉層の基礎血流量[L/h]</summary>
        private double baseBloodFlow_Muscle = 0;

        /// <summary>AVA血流量[L/h]</summary>
        /// <remarks>四肢末端部のみ有効</remarks>
        private double bloodFlow_AVA;

        /// <summary>コア部血流量[L/h]</summary>
        private double bloodFlow_Core;

        /// <summary>筋肉層血流量[L/h]</summary>
        private double bloodFlow_Muscle;

        /// <summary>脂肪層血流量[L/h]</summary>
        private double bloodFlow_Fat;

        /// <summary>皮膚部血流量[L/h]</summary>
        private double bloodFlow_Skin;

        /// <summary>動脈血流[L/h]</summary>
        private double bloodFlow_Artery;

        /// <summary>表在静脈血流[L/h]</summary>
        /// <remarks>腕や足のみ有効</remarks>
        private double bloodFlow_SuperficialVein;

        /// <summary>深部動脈血流[L/h]</summary>
        private double bloodFlow_DeepVein;

        #endregion

        #region 熱容量

        /// <summary>皮膚層の熱容量[Wh/K]</summary>
        private double heatCapacity_Skin;

        /// <summary>動脈の熱容量[Wh/K]</summary>
        private double heatCapacity_Artery;

        /// <summary>深部静脈の熱容量[Wh/K]</summary>
        private double heatCapacity_DeepVein;

        /// <summary>表在静脈の熱容量[Wh/K]</summary>
        private double heatCapacity_SuperficialVein;

        /// <summary>コア部の熱容量[Wh/K]</summary>
        private double heatCapacity_Core;

        /// <summary>脂肪層の熱容量[Wh/K]</summary>
        private double heatCapacity_Fat;

        /// <summary>筋肉層の熱容量[Wh/K]</summary>
        private double heatCapacity_Muscle;

        #endregion

        #region 温度

        /// <summary>コアの温度[C]</summary>
        internal double coreTemperature;

        /// <summary>動脈の温度[C]</summary>
        internal double arteryTemperature;

        /// <summary>深部静脈の温度[C]</summary>
        internal double deepVeinTemperature;

        /// <summary>表在静脈の温度[C]</summary>
        internal double superficialVeinTemperature;

        /// <summary>筋肉の温度[C]</summary>
        internal double muscleTemperature;

        /// <summary>脂肪の温度[C]</summary>
        internal double fatTemperature;

        #endregion

        #region 熱コンダクタンス

        /// <summary>コア-皮膚間の熱コンダクタンス[W/K]</summary>
        private double heatConductance_Core_Skin;

        /// <summary>動脈-深部静脈間の熱コンダクタンス[W/K]</summary>
        private double heatConductance_Artery_DeepVein;

        /// <summary>血管(動脈・静脈共通)-コア間の熱コンダクタンス[W/K]</summary>
        private double heatConductance_Vein_Core;

        /// <summary>表在静脈-皮膚間の熱コンダクタンス[W/K]</summary>
        private double heatConductance_SuperficialVein_Skin;

        /// <summary>コア層-筋肉層間の熱コンダクタンス[W/K]</summary>
        private double heatConductance_Core_Muscle;

        /// <summary>筋肉層-脂肪層間の熱コンダクタンス[W/K]</summary>
        private double heatConductance_Muscle_Fat;

        /// <summary>脂肪層-皮膚層間の熱コンダクタンス[W/K]</summary>
        private double heatConductance_Fat_Skin;

        #endregion

        #region 代謝量

        /// <summary>コア部の代謝量[W]</summary>
        private double metabolicRate_Core;

        /// <summary>筋肉層の代謝量[W]</summary>
        private double metabolicRate_Muscle;

        /// <summary>脂肪層の代謝量[W]</summary>
        private double metabolicRate_Fat;

        /// <summary>皮膚部の代謝量[W]</summary>
        private double metabolicRate_Skin;

        #endregion

        #endregion

        #region プロパティ

        /// <summary>JOSモデル（脂肪層および筋肉層をコア層に統合）か否かを取得する</summary>
        public bool IsJOSModel
        {
            get
            {
                return isJOSModel;
            }
        }

        /// <summary>体を取得する</summary>
        public HumanBody Body
        {
            get;
            private set;
        }

        /// <summary>体の部位を取得する</summary>
        public HumanBody.Nodes Position
        {
            get;
            private set;
        }

        /// <summary>体表面積[m2]を取得する</summary>
        public double SurfaceArea
        {
            get;
            internal set;
        }

        /// <summary>重量[kg]を取得する</summary>
        public double Weight
        {
            get;
            internal set;
        }

        /// <summary>体脂肪率[%]を取得する</summary>
        public double FatPercentage
        {
            get;
            internal set;
        }

        /// <summary>仕事量[W]を取得する</summary>
        public double WorkLoad
        {
            get;
            private set;
        }

        /// <summary>皮膚-物体間の熱コンダクタンス[W/(m2 K)]を取得する</summary>
        /// <remarks>単位はW/(m2 K)</remarks>
        public double HeatConductance_Skin_Material
        {
            get;
            internal set;
        }

        /// <summary>皮膚-空気間の熱コンダクタンス[W/K]を取得する</summary>
        public double HeatConductance_Skin_Air
        {
            get;
            private set;
        }

        /// <summary>皮膚-空気間の湿気熱コンダクタンス[W/kPa]を取得する</summary>
        public double LatentHeatConductance_Skin_Air
        {
            get;
            private set;
        }

        /// <summary>放射熱伝達率[W/(m2 K)]を取得する</summary>
        public double RadiativeHeatTransferCoefficient
        {
            get;
            private set;
        }

        /// <summary>対流熱伝達率[W/(m2 K)]を取得する</summary>
        public double ConvectiveHeatTransferCoefficient
        {
            get;
            private set;
        }

        /// <summary>接触部の皮膚温度[C]を取得する</summary>
        public double SkinTemperature_Contact
        {
            get;
            internal set;
        }

        /// <summary>非接触部の皮膚温度[C]を取得する</summary>
        public double SkinTemperature_NonContact
        {
            get;
            internal set;
        }

        /// <summary>皮膚接触部の割合[-]を取得する</summary>
        public double ContactPortionRate
        {
            get
            {
                return contactPortionRate;
            }
            private set
            {
                contactPortionRate = Math.Max(0, Math.Min(1, value));
            }
        }

        /// <summary>皮膚非接触部の割合[-]を取得する</summary>
        public double NonContactPortionRate
        {
            get
            {
                return 1 - ContactPortionRate;
            }
            private set
            {
                ContactPortionRate = value;
            }
        }

        /// <summary>着衣量[clo]を取得する</summary>
        public double ClothingIndex
        {
            get
            {
                return clothingIndex;
            }
            internal set
            {
                clothingIndex = Math.Max(0, value);
            }
        }

        /// <summary>コアのセットポイント[C]を取得する</summary>
        public double SetPoint_Core
        {
            get;
            internal set;
        }

        /// <summary>皮膚のセットポイント[C]を取得する</summary>
        public double SetPoint_Skin
        {
            get;
            internal set;
        }

        /// <summary>発汗による蒸発熱損失[W]を取得する</summary>
        public double EvaporativeHeatLoss_Sweat
        {
            get;
            private set;
        }

        /// <summary>発汗および不感蒸泄による蒸発熱損失[W]を取得する</summary>
        public double EvaporativeHeatLoss
        {
            get;
            private set;
        }

        /// <summary>ふるえによる熱生成量[W]を取得する</summary>
        public double ShiveringLoad
        {
            get;
            private set;
        }

        /// <summary>接触物体の温度[C]を取得する</summary>
        public double MaterialTemperature
        {
            get;
            internal set;
        }

        /// <summary>近傍の空気の相対湿度[%]を取得する</summary>
        public double RelativeHumidity
        {
            get;
            internal set;
        }

        /// <summary>平均放射温度[C]を取得する</summary>
        public double MeanRadiantTemperature
        {
            get;
            internal set;
        }

        /// <summary>近傍の空気の乾球温度[C]を取得する</summary>
        public double DrybulbTemperature
        {
            get;
            internal set;
        }

        /// <summary>気流速度[m/s]を取得する</summary>
        public double Velocity
        {
            get
            {
                return velocity;
            }
            internal set
            {
                velocity = Math.Max(0, value);
                updateConvectiveHeatTransferCoefficients();
            }
        }

        /// <summary>作用温度[C]を設定・取得する</summary>
        public double OperativeTemperature
        {
            get
            {
                return (DrybulbTemperature * ConvectiveHeatTransferCoefficient + MeanRadiantTemperature * RadiativeHeatTransferCoefficient)
                    / (ConvectiveHeatTransferCoefficient + RadiativeHeatTransferCoefficient);
            }
        }

        /// <summary>接続先の体の部位一覧を取得する</summary>
        public ImmutableBodyPart[] BodyPartConnectTo
        {
            get
            {
                return bpConnectTo.ToArray();
            }
        }

        /// <summary>接続もとの体の部位を取得する</summary>
        public ImmutableBodyPart BodyPartConnectFrom
        {
            get
            {
                return bpConnectFrom;
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="body">体</param>
        /// <param name="position">部位情報</param>
        internal BodyPart(HumanBody body, HumanBody.Nodes position)
        {
            if (position == HumanBody.Nodes.None || 
                position == HumanBody.Nodes.TerminalPart) throw new Exception("部位初期化エラー");

            Body = body;
            Position = position;

            //モデル種別を設定//ここをコメントアウト
            isJOSModel = false;
            //if (position == HumanBody.Nodes.Head) isJOSModel = false;

            //体表面積を初期化
            initSurfaceArea();
            //重量を初期化
            initWeight();
            //熱容量[Wh/K]を初期化
            initHeatCapacity();
            //代謝量[W]を初期化
            initMetabolicRate();
            //熱コンダクタンス[W/K]を初期化する
            initHeatConductance();
            //基礎血流量[L/h]を初期化する
            initInternalBloodFlow();
        }

        #endregion

        #region 初期化処理

        /// <summary>体表面積[m2]を初期化する</summary>
        private void initSurfaceArea()
        {
            double sfRate = Body.SurfaceArea / HumanBody.STANDARD_SURFACE_AREA;

            switch (Position)
            {
                case HumanBody.Nodes.Head:
                    SurfaceArea = 0.110 * sfRate;
                    return;
                case HumanBody.Nodes.Neck:
                    SurfaceArea = 0.029 * sfRate;
                    return;
                case HumanBody.Nodes.Chest:
                    SurfaceArea = 0.175 * sfRate;
                    return;
                case HumanBody.Nodes.Back:
                    SurfaceArea = 0.161 * sfRate;
                    return;
                case HumanBody.Nodes.Pelvis:
                    SurfaceArea = 0.221 * sfRate;
                    return;
                case HumanBody.Nodes.LeftShoulder:
                case HumanBody.Nodes.RightShoulder:
                    SurfaceArea = 0.096 * sfRate;
                    return;
                case HumanBody.Nodes.LeftArm:
                case HumanBody.Nodes.RightArm:
                    SurfaceArea = 0.063 * sfRate;
                    return;
                case HumanBody.Nodes.LeftHand:
                case HumanBody.Nodes.RightHand:
                    SurfaceArea = 0.050 * sfRate;
                    return;
                case HumanBody.Nodes.LeftThigh:
                case HumanBody.Nodes.RightThigh:
                    SurfaceArea = 0.209 * sfRate;
                    return;
                case HumanBody.Nodes.LeftLeg:
                case HumanBody.Nodes.RightLeg:
                    SurfaceArea = 0.112 * sfRate;
                    return;
                case HumanBody.Nodes.LeftFoot:
                case HumanBody.Nodes.RightFoot:
                    SurfaceArea = 0.056 * sfRate;
                    return;
            }
        }

        /// <summary>重量[kg]を初期化する</summary>
        private void initWeight()
        {
            double wtRate = Body.Weight / HumanBody.STANDARD_WEIGHT;

            switch (Position)
            {
                case HumanBody.Nodes.Head:
                    Weight = 3.176 * wtRate;
                    return;
                case HumanBody.Nodes.Neck:
                    Weight = 0.844 * wtRate;
                    return;
                case HumanBody.Nodes.Chest:
                    Weight = 12.4 * wtRate;
                    return;
                case HumanBody.Nodes.Back:
                    Weight = 11.03 * wtRate;
                    return;
                case HumanBody.Nodes.Pelvis:
                    Weight = 17.57 * wtRate;
                    return;
                case HumanBody.Nodes.LeftShoulder:
                case HumanBody.Nodes.RightShoulder:
                    Weight = 2.163 * wtRate;
                    return;
                case HumanBody.Nodes.LeftArm:
                case HumanBody.Nodes.RightArm:
                    Weight = 1.373 * wtRate;
                    return;
                case HumanBody.Nodes.LeftHand:
                case HumanBody.Nodes.RightHand:
                    Weight = 0.335 * wtRate;
                    return;
                case HumanBody.Nodes.LeftThigh:
                case HumanBody.Nodes.RightThigh:
                    Weight = 7.013 * wtRate;
                    return;
                case HumanBody.Nodes.LeftLeg:
                case HumanBody.Nodes.RightLeg:
                    Weight = 3.343 * wtRate;
                    return;
                case HumanBody.Nodes.LeftFoot:
                case HumanBody.Nodes.RightFoot:
                    Weight = 0.48 * wtRate;
                    return;
            }
        }

        /// <summary>熱容量[Wh/K]を初期化する</summary>
        private void initHeatCapacity()
        {
            double wRate = Body.Weight / HumanBody.STANDARD_WEIGHT;
            double bfRateST = Body.CardiacIndexAtRest * 60 * Body.SurfaceArea / HumanBody.STANDARD_BLOOD_FLOW;

            switch (Position)
            {
                case HumanBody.Nodes.Head:
                    heatCapacity_Core = 2.539 * 0.800 * wRate;  //65MNモデルに従って分配
                    heatCapacity_Muscle = 2.539 * 0.120 * wRate;
                    heatCapacity_Fat = 2.539 * 0.080 * wRate;
                    heatCapacity_Skin = 0.22 * wRate;
                    heatCapacity_Artery = 0.096 * bfRateST;
                    heatCapacity_DeepVein = 0.321 * bfRateST;
                    heatCapacity_SuperficialVein = 0.0;
                    break;
                case HumanBody.Nodes.Neck:
                    heatCapacity_Core = 0.674 * 0.800 * wRate;
                    heatCapacity_Muscle = 0.674 * 0.120 * wRate;
                    heatCapacity_Fat = 0.674 * 0.080 * wRate;
                    heatCapacity_Skin = 0.058 * wRate;
                    heatCapacity_Artery = 0.025 * bfRateST;
                    heatCapacity_DeepVein = 0.085 * bfRateST;
                    heatCapacity_SuperficialVein = 0.0;                    
                    break;
                case HumanBody.Nodes.Chest:
                    double cHcapC = (11.841 * wRate - Body.HeatCapacity_CentralBloodPool / 2d);
                    heatCapacity_Core = cHcapC * 0.290 * wRate;
                    heatCapacity_Muscle = cHcapC * 0.562 * wRate;
                    heatCapacity_Fat = cHcapC * 0.148 * wRate;
                    heatCapacity_Skin = 0.441 * wRate;
                    heatCapacity_Artery = 0.12 * bfRateST;
                    heatCapacity_DeepVein = 0.424 * bfRateST;
                    heatCapacity_SuperficialVein = 0.0;
                    break;
                case HumanBody.Nodes.Back:
                    double cHcapB = (10.894 * wRate - Body.HeatCapacity_CentralBloodPool / 2d);
                    heatCapacity_Core = cHcapB * 0.280 * wRate;
                    heatCapacity_Muscle = cHcapB * 0.570 * wRate;
                    heatCapacity_Fat = cHcapB * 0.150 * wRate;
                    heatCapacity_Skin = 0.406 * wRate;
                    heatCapacity_Artery = 0.111 * bfRateST;
                    heatCapacity_DeepVein = 0.39 * bfRateST;
                    heatCapacity_SuperficialVein = 0.0;
                    break;
                case HumanBody.Nodes.Pelvis:
                    heatCapacity_Core = 14.931 * 0.374 * wRate;
                    heatCapacity_Muscle = 14.931 * 0.496 * wRate;
                    heatCapacity_Fat = 14.931 * 0.130 * wRate;
                    heatCapacity_Skin = 0.556 * wRate;
                    heatCapacity_Artery = 0.265 * bfRateST;
                    heatCapacity_DeepVein = 0.832 * bfRateST;
                    heatCapacity_SuperficialVein = 0.0;
                    break;
                case HumanBody.Nodes.LeftShoulder:
                case HumanBody.Nodes.RightShoulder:
                    heatCapacity_Core = 1.764 * 0.281 * wRate;
                    heatCapacity_Muscle = 1.764 * 0.603 * wRate;
                    heatCapacity_Fat = 1.764 * 0.116 * wRate;
                    heatCapacity_Skin = 0.151 * wRate;
                    heatCapacity_Artery = 0.0186 * bfRateST;
                    heatCapacity_DeepVein = 0.046 * bfRateST;
                    heatCapacity_SuperficialVein = 0.025 * bfRateST;
                    break;
                case HumanBody.Nodes.LeftArm:
                case HumanBody.Nodes.RightArm:
                    heatCapacity_Core = 1.154 * 0.284 * wRate;
                    heatCapacity_Muscle = 1.154 * 0.601 * wRate;
                    heatCapacity_Fat = 1.154 * 0.115 * wRate;
                    heatCapacity_Skin = 0.099 * wRate;
                    heatCapacity_Artery = 0.0091 * bfRateST;
                    heatCapacity_DeepVein = 0.024 * bfRateST;
                    heatCapacity_SuperficialVein = 0.015 * bfRateST;
                    break;
                case HumanBody.Nodes.LeftHand:
                case HumanBody.Nodes.RightHand:
                    heatCapacity_Core = 0.168 * 0.481 * wRate;
                    heatCapacity_Muscle = 0.168 * 0.214 * wRate;
                    heatCapacity_Fat = 0.168 * 0.305 * wRate;
                    heatCapacity_Skin = 0.099 * wRate;
                    heatCapacity_Artery = 0.0044 * bfRateST;
                    heatCapacity_DeepVein = 0.01 * bfRateST;
                    heatCapacity_SuperficialVein = 0.011 * bfRateST;
                    break;
                case HumanBody.Nodes.LeftThigh:
                case HumanBody.Nodes.RightThigh:
                    heatCapacity_Core = 5.6 * 0.286 * wRate;
                    heatCapacity_Muscle = 5.6 * 0.618 * wRate;
                    heatCapacity_Fat = 5.6 * 0.096 * wRate;
                    heatCapacity_Skin = 0.408 * wRate;
                    heatCapacity_Artery = 0.0813 * bfRateST;
                    heatCapacity_DeepVein = 0.207 * bfRateST;
                    heatCapacity_SuperficialVein = 0.074 * bfRateST;
                    break;
                case HumanBody.Nodes.LeftLeg:
                case HumanBody.Nodes.RightLeg:
                    heatCapacity_Core = 3.007 * 0.286 * wRate;
                    heatCapacity_Muscle = 3.007 * 0.618 * wRate;
                    heatCapacity_Fat = 3.007 * 0.096 * wRate;
                    heatCapacity_Skin = 0.219 * wRate;
                    heatCapacity_Artery = 0.04 * bfRateST;
                    heatCapacity_DeepVein = 0.1 * bfRateST;
                    heatCapacity_SuperficialVein = 0.05 * bfRateST;
                    break;
                case HumanBody.Nodes.LeftFoot:
                case HumanBody.Nodes.RightFoot:
                    heatCapacity_Core = 0.244 * 0.550 * wRate;
                    heatCapacity_Muscle = 0.244 * 0.146 * wRate;
                    heatCapacity_Fat = 0.244 * 0.304 * wRate;
                    heatCapacity_Skin = 0.128 * wRate;
                    heatCapacity_Artery = 0.0103 * bfRateST;
                    heatCapacity_DeepVein = 0.024 * bfRateST;
                    heatCapacity_SuperficialVein = 0.021 * bfRateST;
                    break;
            }

            //コア層から血管分の熱量を差し引く
            heatCapacity_Core -= heatCapacity_Artery + heatCapacity_DeepVein;
            //皮膚層から表在静脈分の熱量を差し引く
            heatCapacity_Skin -= heatCapacity_SuperficialVein;
        }

        /// <summary>基礎代謝量[W]を初期化する</summary>
        private void initMetabolicRate()
        {
            double mRate = Body.BasicMetabolicRate / HumanBody.STANDARD_MET;

            switch (Position)
            {
                case HumanBody.Nodes.Head:
                    metabolicRate_Core = 16.896 * 0.981 * mRate;   //コア層の代謝量[W]
                    metabolicRate_Muscle = 16.896 * 0.013 * mRate;   //筋肉層の代謝量[W]
                    metabolicRate_Fat = 16.896 * 0.006 * mRate;   //脂肪層の代謝量[W]
                    metabolicRate_Skin = 0.104 * mRate;    //皮膚層の代謝量[W]
                    return;
                case HumanBody.Nodes.Neck:
                    metabolicRate_Core = 0.274 * 0.981 * mRate;
                    metabolicRate_Muscle = 0.274 * 0.013 * mRate;
                    metabolicRate_Fat = 0.274 * 0.006 * mRate;
                    metabolicRate_Skin = 0.028 * mRate;
                    return;
                case HumanBody.Nodes.Chest:
                    metabolicRate_Core = 24.287 * 0.873 * mRate;
                    metabolicRate_Muscle = 24.287 * 0.104 * mRate;
                    metabolicRate_Fat = 24.287 * 0.023 * mRate;
                    metabolicRate_Skin = 0.179 * mRate;
                    return;
                case HumanBody.Nodes.Back:
                    metabolicRate_Core = 21.737 * 0.860 * mRate;
                    metabolicRate_Muscle = 21.737 * 0.117 * mRate;
                    metabolicRate_Fat = 21.737 * 0.023 * mRate;
                    metabolicRate_Skin = 0.158 * mRate;
                    return;
                case HumanBody.Nodes.Pelvis:
                    metabolicRate_Core = 12.921 * 0.623 * mRate;
                    metabolicRate_Muscle = 12.921 * 0.315 * mRate;
                    metabolicRate_Fat = 12.921 * 0.062 * mRate;
                    metabolicRate_Skin = 0.254 * mRate;
                    return;
                case HumanBody.Nodes.LeftShoulder:
                case HumanBody.Nodes.RightShoulder:
                    metabolicRate_Core = 1.215 * 0.150 * mRate;
                    metabolicRate_Muscle = 1.215 * 0.348 * mRate;
                    metabolicRate_Fat = 1.215 * 0.502 * mRate;
                    metabolicRate_Skin = 0.05 * mRate;
                    return;
                case HumanBody.Nodes.LeftArm:
                case HumanBody.Nodes.RightArm:
                    metabolicRate_Core = 0.346 * 0.272 * mRate;
                    metabolicRate_Muscle = 0.346 * 0.638 * mRate;
                    metabolicRate_Fat = 0.346 * 0.090 * mRate;
                    metabolicRate_Skin = 0.026 * mRate;
                    return;
                case HumanBody.Nodes.LeftHand:
                case HumanBody.Nodes.RightHand:
                    metabolicRate_Core = 0.09 * 0.500 * mRate;
                    metabolicRate_Muscle = 0.09 * 0.244 * mRate;
                    metabolicRate_Fat = 0.09 * 0.256 * mRate;
                    metabolicRate_Skin = 0.05 * mRate;
                    return;
                case HumanBody.Nodes.LeftThigh:
                case HumanBody.Nodes.RightThigh:
                    metabolicRate_Core = 1.318 * 0.260 * mRate;
                    metabolicRate_Muscle = 1.318 * 0.625 * mRate;
                    metabolicRate_Fat = 1.318 * 0.115 * mRate;
                    metabolicRate_Skin = 0.122 * mRate;
                    return;
                case HumanBody.Nodes.LeftLeg:
                case HumanBody.Nodes.RightLeg:
                    metabolicRate_Core = 0.357 * 0.286 * mRate;
                    metabolicRate_Muscle = 0.357 * 0.616 * mRate;
                    metabolicRate_Fat = 0.357 * 0.098 * mRate;
                    metabolicRate_Skin = 0.023 * mRate;
                    return;
                case HumanBody.Nodes.LeftFoot:
                case HumanBody.Nodes.RightFoot:
                    metabolicRate_Core = 0.212 * 0.573 * mRate;
                    metabolicRate_Muscle = 0.212 * 0.164 * mRate;
                    metabolicRate_Fat = 0.212 * 0.263 * mRate;
                    metabolicRate_Skin = 0.1 * mRate;
                    return;
            }
        }

        /// <summary>熱コンダクタンス[W/K]を初期化する</summary>
        private void initHeatConductance()
        {
            const double HEAD_RATE = 0.110 / (0.110 + 0.029);
            double saRate = Body.SurfaceArea / HumanBody.STANDARD_SURFACE_AREA;
            double wRate = Body.Weight / HumanBody.STANDARD_WEIGHT;
            double ssw = Math.Pow(saRate, 2) / wRate;
            double hcSum;

            switch (Position)
            {
                case HumanBody.Nodes.Head:
                    hcSum = (-0.015 * Body.FatPercentage + 3.642) * wRate / saRate;
                    heatConductance_Core_Skin = hcSum;
                    hcSum *= 23.489;
                    heatConductance_Core_Muscle = 1.601 * HEAD_RATE;
                    heatConductance_Muscle_Fat = 13.224 * HEAD_RATE;
                    heatConductance_Fat_Skin = 16.008 * HEAD_RATE;
                    heatConductance_Artery_DeepVein = 0;
                    heatConductance_Vein_Core = 0;
                    heatConductance_SuperficialVein_Skin = 0;
                    return;
                case HumanBody.Nodes.Neck:
                    hcSum = (-0.004 * Body.FatPercentage + 0.968) * wRate / saRate;
                    heatConductance_Core_Skin = hcSum;
                    hcSum *= 23.489;
                    heatConductance_Core_Muscle = 1.601 * (1 - HEAD_RATE);
                    heatConductance_Muscle_Fat = 13.224 * (1 - HEAD_RATE);
                    heatConductance_Fat_Skin = 16.008 * (1 - HEAD_RATE);
                    heatConductance_Artery_DeepVein = 0;
                    heatConductance_Vein_Core = 0;
                    heatConductance_SuperficialVein_Skin = 0;
                    return;
                case HumanBody.Nodes.Chest:
                    hcSum = (-0.017 * Body.FatPercentage + 2.037) * ssw;
                    heatConductance_Core_Skin = hcSum;
                    hcSum *= 26.178;
                    heatConductance_Core_Muscle = 0.616;
                    heatConductance_Muscle_Fat = 2.1;
                    heatConductance_Fat_Skin = 9.164;
                    heatConductance_Artery_DeepVein = 0;
                    heatConductance_Vein_Core = 0;
                    heatConductance_SuperficialVein_Skin = 0;
                    return;
                case HumanBody.Nodes.Back:
                    hcSum = (-0.016 * Body.FatPercentage + 1.876) * ssw;
                    heatConductance_Core_Skin = hcSum;
                    hcSum *= 25.786;
                    heatConductance_Core_Muscle = 0.594;
                    heatConductance_Muscle_Fat = 2.018;
                    heatConductance_Fat_Skin = 8.7;
                    heatConductance_Artery_DeepVein = 0;
                    heatConductance_Vein_Core = 0;
                    heatConductance_SuperficialVein_Skin = 0;
                    return;
                case HumanBody.Nodes.Pelvis:
                    hcSum = (-0.021 * Body.FatPercentage + 2.569) * ssw;
                    heatConductance_Core_Skin = hcSum;
                    hcSum *= 24.473;
                    heatConductance_Core_Muscle = 0.379;
                    heatConductance_Muscle_Fat = 1.276;
                    heatConductance_Fat_Skin = 5.104;
                    heatConductance_Artery_DeepVein = 0;
                    heatConductance_Vein_Core = 0;
                    heatConductance_SuperficialVein_Skin = 0;
                    return;
                case HumanBody.Nodes.LeftShoulder:
                case HumanBody.Nodes.RightShoulder:
                    hcSum = (-0.011 * Body.FatPercentage + 1.659) * ssw;
                    heatConductance_Core_Skin = hcSum;
                    hcSum *= 29.491;
                    heatConductance_Core_Muscle = 0.441;
                    heatConductance_Muscle_Fat = 2.946;
                    heatConductance_Fat_Skin = 7.308;
                    heatConductance_Artery_DeepVein = 0.537 * ssw;
                    heatConductance_Vein_Core = 0.586 * ssw;
                    heatConductance_SuperficialVein_Skin = 57.735;
                    return;
                case HumanBody.Nodes.LeftArm:
                case HumanBody.Nodes.RightArm:
                    hcSum = (-0.007 * Body.FatPercentage + 1.086) * ssw;
                    heatConductance_Core_Skin = hcSum;
                    hcSum *= 47.632;
                    heatConductance_Core_Muscle = 0.244;
                    heatConductance_Muscle_Fat = 2.227;
                    heatConductance_Fat_Skin = 7.888;
                    heatConductance_Artery_DeepVein = 0.351 * ssw;
                    heatConductance_Vein_Core = 0.383 * ssw;
                    heatConductance_SuperficialVein_Skin = 37.768;
                    return;
                case HumanBody.Nodes.LeftHand:
                case HumanBody.Nodes.RightHand:
                    hcSum = (-0.012 * Body.FatPercentage + 2.358) * ssw;
                    heatConductance_Core_Skin = hcSum;
                    hcSum *= 11.390;
                    heatConductance_Core_Muscle = 2.181;
                    heatConductance_Muscle_Fat = 6.484;
                    heatConductance_Fat_Skin = 5.858;
                    heatConductance_Artery_DeepVein = 0.762 * ssw;
                    heatConductance_Vein_Core = 1.534 * ssw;
                    heatConductance_SuperficialVein_Skin = 16.634;
                    return;
                case HumanBody.Nodes.LeftThigh:
                case HumanBody.Nodes.RightThigh:
                    hcSum = (-0.018 * Body.FatPercentage + 2.743) * ssw;
                    heatConductance_Core_Skin = hcSum;
                    hcSum *= 24.811;
                    heatConductance_Core_Muscle = 2.401;
                    heatConductance_Muscle_Fat = 4.536;
                    heatConductance_Fat_Skin = 30.16;
                    heatConductance_Artery_DeepVein = 0.826 * ssw;
                    heatConductance_Vein_Core = 0.81 * ssw;
                    heatConductance_SuperficialVein_Skin = 102.012;
                    return;
                case HumanBody.Nodes.LeftLeg:
                case HumanBody.Nodes.RightLeg:
                    hcSum = (-0.010 * Body.FatPercentage + 1.474) * ssw;
                    heatConductance_Core_Skin = hcSum;
                    hcSum *= 12.558;
                    heatConductance_Core_Muscle = 1.891;
                    heatConductance_Muscle_Fat = 2.656;
                    heatConductance_Fat_Skin = 7.54;
                    heatConductance_Artery_DeepVein = 0.444 * ssw;
                    heatConductance_Vein_Core = 0.435 * ssw;
                    heatConductance_SuperficialVein_Skin = 54.784;
                    return;
                case HumanBody.Nodes.LeftFoot:
                case HumanBody.Nodes.RightFoot:
                    hcSum = (-0.007 * Body.FatPercentage + 3.470) * ssw;
                    heatConductance_Core_Skin = hcSum;
                    hcSum *= 9.105;
                    heatConductance_Core_Muscle = 8.12;
                    heatConductance_Muscle_Fat = 10.266;
                    heatConductance_Fat_Skin = 8.178;
                    heatConductance_Artery_DeepVein = 0.992 * ssw;
                    heatConductance_Vein_Core = 1.816 * ssw;
                    heatConductance_SuperficialVein_Skin = 24.277;
                    return;
            }
        }

        /// <summary>部位内の血流量[L/h]を初期化する</summary>
        internal void initInternalBloodFlow()
        {
            double bfRate = Body.BaseBloodFlowRate / HumanBody.STANDARD_BLOOD_FLOW;

            switch (Position)
            {
                case HumanBody.Nodes.Head:
                    bloodFlow_Core = 32.228 * 0.974 * bfRate;
                    baseBloodFlow_Muscle = 32.228 * 0.019 * bfRate;
                    bloodFlow_Fat = 32.228 * 0.007 * bfRate;
                    baseBloodFlow_Skin = 5.725 * bfRate;
                    return;
                case HumanBody.Nodes.Neck:
                    bloodFlow_Core = 15.24 * 0.974 * bfRate;
                    baseBloodFlow_Muscle = 15.24 * 0.019 * bfRate;
                    bloodFlow_Fat = 15.24 * 0.007 * bfRate;
                    baseBloodFlow_Skin = 0.325 * bfRate;
                    return;
                case HumanBody.Nodes.Chest:
                    bloodFlow_Core = 89.214 * 0.897 * bfRate;
                    baseBloodFlow_Muscle = 89.214 * 0.088 * bfRate;
                    bloodFlow_Fat = 89.214 * 0.015 * bfRate;
                    baseBloodFlow_Skin = 1.967 * bfRate;
                    return;
                case HumanBody.Nodes.Back:
                    bloodFlow_Core = 87.663 * 0.894 * bfRate;
                    baseBloodFlow_Muscle = 87.663 * 0.090 * bfRate;
                    bloodFlow_Fat = 87.663 * 0.016 * bfRate;
                    baseBloodFlow_Skin = 1.475 * bfRate;
                    return;
                case HumanBody.Nodes.Pelvis:
                    bloodFlow_Core = 33.518 * 0.558 * bfRate;
                    baseBloodFlow_Muscle = 33.518 * 0.376 * bfRate;
                    bloodFlow_Fat = 33.518 * 0.066 * bfRate;
                    baseBloodFlow_Skin = 2.272 * bfRate;
                    return;
                case HumanBody.Nodes.LeftShoulder:
                case HumanBody.Nodes.RightShoulder:
                    bloodFlow_Core = 1.808 * 0.182 * bfRate;
                    baseBloodFlow_Muscle = 1.808 * 0.728 * bfRate;
                    bloodFlow_Fat = 1.808 * 0.090 * bfRate;
                    baseBloodFlow_Skin = 0.91 * bfRate;
                    return;
                case HumanBody.Nodes.LeftArm:
                case HumanBody.Nodes.RightArm:
                    bloodFlow_Core = 0.94 * 0.174 * bfRate;
                    baseBloodFlow_Muscle = 0.94 * 0.732 * bfRate;
                    bloodFlow_Fat = 0.94 * 0.090 * bfRate;
                    baseBloodFlow_Skin = 0.508 * bfRate;
                    return;
                case HumanBody.Nodes.LeftHand:
                case HumanBody.Nodes.RightHand:
                    bloodFlow_Core = 0.217 * 0.424 * bfRate;
                    baseBloodFlow_Muscle = 0.217 * 0.373 * bfRate;
                    bloodFlow_Fat = 0.217 * 0.203 * bfRate;
                    baseBloodFlow_Skin = 1.114 * bfRate;
                    return;
                case HumanBody.Nodes.LeftThigh:
                case HumanBody.Nodes.RightThigh:
                    bloodFlow_Core = 1.406 * 0.265 * bfRate;
                    baseBloodFlow_Muscle = 1.406 * 0.625 * bfRate;
                    bloodFlow_Fat = 1.406 * 0.110 * bfRate;
                    baseBloodFlow_Skin = 1.456 * bfRate;
                    return;
                case HumanBody.Nodes.LeftLeg:
                case HumanBody.Nodes.RightLeg:
                    bloodFlow_Core = 0.164 * 0.454 * bfRate;
                    baseBloodFlow_Muscle = 0.164 * 0.432 * bfRate;
                    bloodFlow_Fat = 0.164 * 0.114 * bfRate;
                    baseBloodFlow_Skin = 0.651 * bfRate;
                    return;
                case HumanBody.Nodes.LeftFoot:
                case HumanBody.Nodes.RightFoot:
                    bloodFlow_Core = 0.08 * 0.637 * bfRate;
                    baseBloodFlow_Muscle = 0.08 * 0.136 * bfRate;
                    bloodFlow_Fat = 0.08 * 0.227 * bfRate;
                    baseBloodFlow_Skin = 0.934 * bfRate;
                    return;
            }
        }

        #endregion

        #region publicメソッド

        /// <summary>皮膚からの顕熱損失[W]を計算する</summary>
        /// <returns>皮膚からの顕熱損失[W]</returns>
        public double GetSensibleHeatLoss()
        {
            return (SkinTemperature_NonContact - OperativeTemperature) * HeatConductance_Skin_Air * NonContactPortionRate
                + (SkinTemperature_Contact - MaterialTemperature) * HeatConductance_Skin_Material * SurfaceArea * ContactPortionRate;
        }

        /// <summary>各部位の熱容量[Wh/K]を取得する</summary>
        /// <param name="segment">部位</param>
        /// <returns>各部位の熱容量[Wh/K]</returns>
        public double GetHeatCapacity(Segments segment)
        {
            switch (segment)
            {
                case Segments.Artery:
                    return heatCapacity_Artery;
                case Segments.Core:
                    if (isJOSModel) return heatCapacity_Core + heatCapacity_Muscle + heatCapacity_Fat;
                    else return heatCapacity_Core;
                case Segments.DeepVein:
                    return heatCapacity_DeepVein;
                case Segments.Fat:
                    if (isJOSModel) return 0;
                    else return heatCapacity_Fat;
                case Segments.Muscle:
                    if (isJOSModel) return 0;
                    else return heatCapacity_Muscle;
                case Segments.Skin:
                    return heatCapacity_Skin;
                case Segments.SuperficialVein:
                    return heatCapacity_SuperficialVein;
                default:
                    return 0;
            }
        }

        /// <summary>各部位の温度[C]を取得する</summary>
        /// <param name="segment">部位</param>
        /// <returns>各部位の温度[C]</returns>
        public double GetTemperature(Segments segment)
        {
            switch (segment)
            {
                case Segments.Artery:
                    return arteryTemperature;
                case Segments.Core:
                    return coreTemperature;
                case Segments.DeepVein:
                    return deepVeinTemperature;
                case Segments.Fat:
                    return fatTemperature;
                case Segments.Muscle:
                    return muscleTemperature;
                case Segments.Skin:
                    return SkinTemperature_Contact * ContactPortionRate + SkinTemperature_NonContact * NonContactPortionRate;
                case Segments.SuperficialVein:
                    return superficialVeinTemperature;
                default:
                    return 0;
            }
        }

        /// <summary>部位間の熱コンダクタンス[W/K]を取得する</summary>
        /// <param name="segment1">部位1</param>
        /// <param name="segment2">部位2</param>
        /// <returns>部位間の熱コンダクタンス[W/K]</returns>
        public double GetHeatConductance(Segments segment1, Segments segment2)
        {
            switch (segment1 | segment2)
            {
                case (Segments.Core | Segments.Muscle):
                    return heatConductance_Core_Muscle;
                case (Segments.Muscle | Segments.Fat):
                    return heatConductance_Muscle_Fat;
                case (Segments.Fat | Segments.Skin):
                    return heatConductance_Fat_Skin;
                case (Segments.Core | Segments.Skin):
                    return heatConductance_Core_Skin;
                case (Segments.Artery | Segments.DeepVein):
                    return heatConductance_Artery_DeepVein;
                case (Segments.DeepVein | Segments.Core):
                case (Segments.Artery | Segments.Core):
                    return heatConductance_Vein_Core;
                case (Segments.SuperficialVein | Segments.Skin):
                    return heatConductance_SuperficialVein_Skin;
                default:
                    return 0;
            }
        }

        /// <summary>各部位の代謝量[W]を取得する</summary>
        /// <param name="component">部位</param>
        /// <returns>各部位の代謝量[W]</returns>
        public double GetMetabolicRate(Segments component)
        {
            switch (component)
            {
                case Segments.Core:
                    if (isJOSModel) return metabolicRate_Core + metabolicRate_Fat + metabolicRate_Muscle;
                    else return metabolicRate_Core;
                case Segments.Fat:
                    if (isJOSModel) return 0;
                    else return metabolicRate_Fat;
                case Segments.Muscle:
                    if (isJOSModel) return 0;
                    else return metabolicRate_Muscle;
                case Segments.Skin:
                    return metabolicRate_Skin;
                default:
                    return 0;
            }
        }

        /// <summary>各部位の血流量[L/h]を取得する</summary>
        /// <param name="segment">血管の種類</param>
        /// <returns>各部位の血流量[L/h]</returns>
        public double GetBloodFlow(Segments segment)
        {
            //血流を更新
            updateBloodFlow();

            switch (segment)
            {
                case Segments.Artery:
                    return bloodFlow_Artery;
                case Segments.AVA:
                    return bloodFlow_AVA;
                case Segments.Core:
                    if (isJOSModel) return bloodFlow_Core + bloodFlow_Muscle + bloodFlow_Fat;
                    else return bloodFlow_Core;
                case Segments.DeepVein:
                    return bloodFlow_DeepVein;
                case Segments.Skin:
                    return bloodFlow_Skin;
                case Segments.SuperficialVein:
                    return bloodFlow_SuperficialVein;
                case Segments.Muscle:
                    if (isJOSModel) return 0;
                    else return bloodFlow_Muscle;
                case Segments.Fat:
                    if (isJOSModel) return 0;
                    else return bloodFlow_Fat;
                default:
                    return 0;
            }
        }

        /// <summary>部位1から部位2への熱移動量[W]を計算する</summary>
        /// <param name="segment1">部位1</param>
        /// <param name="segment2">部位2</param>
        /// <returns>部位1から部位2への熱移動量[W]</returns>
        public double GetHeatTransfer(Segments segment1, Segments segment2)
        {
            double hTransfer = 0;
            const double RCS = HumanBody.RHO_C / 3.6d;  //流量単位がL/hなので、ここで単位を調整
            
            //温度差を計算
            double deltaT = GetTemperature(segment1) - GetTemperature(segment2);

            //伝導による熱移動を計算
            hTransfer = deltaT * GetHeatConductance(segment1, segment2);

            //熱交換を行わない部位の場合
            if (((segment1 | segment2) & (Segments.AVA | Segments.None)) != Segments.None)
            {
                return 0;
            }
            //動脈と表在静脈の場合
            else if ((segment1 | segment2) == (Segments.Artery | Segments.SuperficialVein))
            {
                //AVA血流による熱移動を追加
                if (segment1 == Segments.Artery) hTransfer += GetTemperature(segment1) * bloodFlow_AVA * RCS;
            }
            //component1が動脈の場合
            else if (segment1 == Segments.Artery && segment2 != Segments.DeepVein)
            {
                //動脈流による熱移動を追加
                hTransfer += GetBloodFlow(segment2) * GetTemperature(segment1) * RCS;
            }
            //component2が静脈の場合
            else if (segment2 == Segments.DeepVein && segment1 != Segments.Artery)
            {
                //静脈流による熱移動を追加
                hTransfer += GetBloodFlow(segment1) * GetTemperature(segment1) * RCS;
            }

            return hTransfer;
        }

        #endregion

        #region internalメソッド

        /// <summary>接触部の割合[-]を設定する</summary>
        /// <param name="contactPortionRate">接触部の割合[-]</param>
        internal void setContactPortionRate(double contactPortionRate)
        {
            //接触部の割合が大きくなる場合
            if (ContactPortionRate < contactPortionRate)
            {
                //接触部の温度を更新
                SkinTemperature_Contact = (SkinTemperature_Contact * ContactPortionRate
                    + SkinTemperature_NonContact * (contactPortionRate - ContactPortionRate)) / contactPortionRate;
            }
            //非接触部の割合が大きくなる場合
            else
            {
                //非接触部の温度を更新
                SkinTemperature_NonContact = (SkinTemperature_NonContact * NonContactPortionRate
                    + SkinTemperature_Contact * (ContactPortionRate - contactPortionRate)) / (1 - contactPortionRate);
            }
            //接触部の割合を更新
            ContactPortionRate = contactPortionRate;
        }

        /// <summary>行列要素を設定する</summary>
        /// <param name="zm">ZMベクトル（要素の単位は全てW）</param>
        /// <param name="bm">BM行列（要素の単位は全てW/K）</param>
        internal void makeMatrix(ref VectorView zm, ref MatrixView bm)
        {
            if (isJOSModel) makeJOSMatrix(ref zm, ref bm);
            else make132MNMatrix(ref zm, ref bm);
        }

        /// <summary>行列要素を設定する(JOSモデル)</summary>
        /// <param name="zm">ZMベクトル（要素の単位は全てW）</param>
        /// <param name="bm">BM行列（要素の単位は全てW/K）</param>
        private void makeJOSMatrix(ref VectorView zm, ref MatrixView bm)
        {
            const double RCS = HumanBody.RHO_C / 3.6d;  //流量単位がL/hなので、ここで単位を調整
            double dt = Body.timeStep;
            bool hasAVA = (heatCapacity_SuperficialVein != 0);
            double sum;

            //JOS計算用にコア・脂肪・筋肉層を統合
            double hcapCore = heatCapacity_Core + heatCapacity_Fat + heatCapacity_Muscle;
            double bfCore = bloodFlow_Core + bloodFlow_Fat + bloodFlow_Muscle;
            double mrCore = metabolicRate_Core + metabolicRate_Fat + metabolicRate_Muscle;

            //皮膚・空気間の熱コンダクタンスを更新
            updateHeatConductance_Skin_Air();

            //蒸発熱損失を計算
            //不感蒸泄分
            double eMax = LatentHeatConductance_Skin_Air * (HumanBody.getSaturatedVaporPressure(SkinTemperature_NonContact)
                - HumanBody.getVaporPressure(DrybulbTemperature, RelativeHumidity, Body.AtmosphericPressure));
            EvaporativeHeatLoss = 0.06 * eMax + EvaporativeHeatLoss_Sweat * 0.94;

            //ZMベクトルを設定*************************************************************************
            zm.SetValue(0, hcapCore * 3600 * coreTemperature / dt + mrCore + WorkLoad + ShiveringLoad);
            zm.SetValue(1, heatCapacity_Skin * 3600 * SkinTemperature_Contact / dt + MaterialTemperature * HeatConductance_Skin_Material * SurfaceArea + metabolicRate_Skin);
            zm.SetValue(2, heatCapacity_Skin * 3600 * SkinTemperature_NonContact / dt + OperativeTemperature * HeatConductance_Skin_Air + metabolicRate_Skin - EvaporativeHeatLoss);
            zm.SetValue(3, heatCapacity_Artery * 3600 * arteryTemperature / dt);
            zm.SetValue(4, heatCapacity_DeepVein * 3600 * deepVeinTemperature / dt);
            if (hasAVA) zm.SetValue(5, heatCapacity_SuperficialVein * 3600 * superficialVeinTemperature / dt);

            //BM行列を設定*****************************************************************************
            //コア層
            bm.SetValue(0, 0, hcapCore * 3600d / dt + RCS * bfCore
                + 2 * heatConductance_Vein_Core + heatConductance_Core_Skin);
            bm.SetValue(0, 1, -ContactPortionRate * heatConductance_Core_Skin);
            bm.SetValue(0, 2, -NonContactPortionRate * heatConductance_Core_Skin);
            bm.SetValue(0, 3, -(RCS * bfCore + heatConductance_Vein_Core));
            bm.SetValue(0, 4, -heatConductance_Vein_Core);
            if (hasAVA) bm.SetValue(0, 5, 0);

            //皮膚層接触部
            bm.SetValue(1, 0, -heatConductance_Core_Skin);
            bm.SetValue(1, 1, heatCapacity_Skin * 3600d / dt + RCS * bloodFlow_Skin + heatConductance_Core_Skin
                + heatConductance_SuperficialVein_Skin + HeatConductance_Skin_Material * SurfaceArea);
            bm.SetValue(1, 2, 0);
            bm.SetValue(1, 3, -RCS * bloodFlow_Skin);
            bm.SetValue(1, 4, 0);
            if (hasAVA) bm.SetValue(1, 5, -heatConductance_SuperficialVein_Skin);

            //皮膚層非接触部
            bm.SetValue(2, 0, -heatConductance_Core_Skin);
            bm.SetValue(2, 1, 0);
            bm.SetValue(2, 2, heatCapacity_Skin * 3600d / dt + RCS * bloodFlow_Skin + heatConductance_Core_Skin
                + heatConductance_SuperficialVein_Skin + HeatConductance_Skin_Air);
            bm.SetValue(2, 3, -RCS * bloodFlow_Skin);
            bm.SetValue(2, 4, 0);
            if (hasAVA) bm.SetValue(2, 5, -heatConductance_SuperficialVein_Skin);

            //動脈
            bm.SetValue(3, 0, -heatConductance_Vein_Core);
            bm.SetValue(3, 1, 0);
            bm.SetValue(3, 2, 0);
            bm.SetValue(3, 3, heatCapacity_Artery * 3600d / dt + RCS * bloodFlow_Artery + heatConductance_Vein_Core + heatConductance_Artery_DeepVein);
            bm.SetValue(3, 4, -heatConductance_Artery_DeepVein);
            if (hasAVA) bm.SetValue(3, 5, 0);

            //深部静脈
            bm.SetValue(4, 0, -(RCS * bfCore + heatConductance_Vein_Core));
            bm.SetValue(4, 1, -ContactPortionRate * RCS * bloodFlow_Skin);
            bm.SetValue(4, 2, -NonContactPortionRate * RCS * bloodFlow_Skin);
            bm.SetValue(4, 3, -heatConductance_Artery_DeepVein);
            sum = heatCapacity_DeepVein * 3600d / dt + RCS * bfCore + RCS * bloodFlow_Skin
                + heatConductance_Vein_Core + heatConductance_Artery_DeepVein;
            foreach (BodyPart bp in bpConnectTo)
            {
                if (Position == HumanBody.Nodes.Pelvis) sum += RCS * (bp.GetBloodFlow(Segments.DeepVein) + bp.GetBloodFlow(Segments.SuperficialVein));
                else sum += RCS * bp.GetBloodFlow(Segments.DeepVein);
            }
            bm.SetValue(4, 4, sum);
            if (hasAVA) bm.SetValue(4, 5, 0);

            //表在静脈
            if (hasAVA)
            {
                bm.SetValue(5, 0, 0);
                bm.SetValue(5, 1, -ContactPortionRate * heatConductance_SuperficialVein_Skin);
                bm.SetValue(5, 2, -NonContactPortionRate * heatConductance_SuperficialVein_Skin);
                //四肢末端部の場合
                if ((Position & HumanBody.Nodes.TerminalPart) != HumanBody.Nodes.None) bm.SetValue(5, 3, -RCS * GetBloodFlow(Segments.SuperficialVein));
                //その他
                else bm.SetValue(5, 3, 0);
                bm.SetValue(5, 4, 0);
                sum = heatCapacity_SuperficialVein * 3600d / dt + heatConductance_SuperficialVein_Skin;
                if ((Position & HumanBody.Nodes.TerminalPart) != HumanBody.Nodes.None) sum += RCS * GetBloodFlow(Segments.SuperficialVein);
                if (Position != HumanBody.Nodes.Pelvis)
                {
                    foreach (BodyPart bp in bpConnectTo) sum += RCS * bp.GetBloodFlow(Segments.SuperficialVein);
                }
                bm.SetValue(5, 5, sum);
            }
        }

        /// <summary>行列要素を設定する(132MNモデル)</summary>
        /// <param name="zm">ZMベクトル（要素の単位は全てW）</param>
        /// <param name="bm">BM行列（要素の単位は全てW/K）</param>
        private void make132MNMatrix(ref VectorView zm, ref MatrixView bm)
        {
            const double RCS = HumanBody.RHO_C / 3.6d;  //流量単位がL/hなので、ここで単位を調整
            double dt = Body.timeStep;
            bool hasAVA = (heatCapacity_SuperficialVein != 0);
            uint avaPos = 5;
            if(hasAVA) avaPos = 6;
            double sum;

            //皮膚・空気間の熱コンダクタンスを更新
            updateHeatConductance_Skin_Air();

            //蒸発熱損失を計算
            //不感蒸泄分
            double eMax = LatentHeatConductance_Skin_Air * (HumanBody.getSaturatedVaporPressure(SkinTemperature_NonContact)
                - HumanBody.getVaporPressure(DrybulbTemperature, RelativeHumidity, Body.AtmosphericPressure));
            EvaporativeHeatLoss = 0.06 * eMax + EvaporativeHeatLoss_Sweat * 0.94;

            //ZMベクトルを設定*************************************************************************
            zm.SetValue(0, heatCapacity_Core * 3600 * coreTemperature / dt + metabolicRate_Core);
            zm.SetValue(1, heatCapacity_Skin * 3600 * SkinTemperature_Contact / dt + MaterialTemperature * HeatConductance_Skin_Material * SurfaceArea + metabolicRate_Skin);
            zm.SetValue(2, heatCapacity_Skin * 3600 * SkinTemperature_NonContact / dt + OperativeTemperature * HeatConductance_Skin_Air + metabolicRate_Skin - EvaporativeHeatLoss);
            zm.SetValue(3, heatCapacity_Artery * 3600 * arteryTemperature / dt);
            zm.SetValue(4, heatCapacity_DeepVein * 3600 * deepVeinTemperature / dt);
            if (hasAVA) zm.SetValue(5, heatCapacity_SuperficialVein * 3600 * superficialVeinTemperature / dt);
            zm.SetValue(avaPos, heatCapacity_Muscle * 3600 * muscleTemperature / dt + metabolicRate_Muscle + WorkLoad + ShiveringLoad);
            zm.SetValue(avaPos + 1, heatCapacity_Fat * 3600 * fatTemperature / dt + metabolicRate_Fat);
            
            //BM行列を設定*****************************************************************************
            //コア層
            bm.SetValue(0, 0, heatCapacity_Core * 3600d / dt + RCS * bloodFlow_Core
                + 2 * heatConductance_Vein_Core + heatConductance_Core_Muscle);
            bm.SetValue(0, 1, 0);
            bm.SetValue(0, 2, 0);
            bm.SetValue(0, 3, -(RCS * bloodFlow_Core + heatConductance_Vein_Core));
            bm.SetValue(0, 4, -heatConductance_Vein_Core);
            if (hasAVA) bm.SetValue(0, 5, 0);
            bm.SetValue(0, avaPos, -heatConductance_Core_Muscle);
            bm.SetValue(0, avaPos + 1, 0);

            //皮膚層接触部
            bm.SetValue(1, 0, 0);
            bm.SetValue(1, 1, heatCapacity_Skin * 3600d / dt + RCS * bloodFlow_Skin + heatConductance_Fat_Skin
                + heatConductance_SuperficialVein_Skin + HeatConductance_Skin_Material * SurfaceArea);
            bm.SetValue(1, 2, 0);
            bm.SetValue(1, 3, -RCS * bloodFlow_Skin);
            bm.SetValue(1, 4, 0);
            if (hasAVA) bm.SetValue(1, 5, -heatConductance_SuperficialVein_Skin);
            bm.SetValue(1, avaPos, 0);
            bm.SetValue(1, avaPos + 1, -heatConductance_Fat_Skin);

            //皮膚層非接触部
            bm.SetValue(2, 0, 0);
            bm.SetValue(2, 1, 0);
            bm.SetValue(2, 2, heatCapacity_Skin * 3600d / dt + RCS * bloodFlow_Skin + heatConductance_Fat_Skin
                + heatConductance_SuperficialVein_Skin + HeatConductance_Skin_Air);
            bm.SetValue(2, 3, -RCS * bloodFlow_Skin);
            bm.SetValue(2, 4, 0);
            if (hasAVA) bm.SetValue(2, 5, -heatConductance_SuperficialVein_Skin);
            bm.SetValue(2, avaPos, 0);
            bm.SetValue(2, avaPos + 1, -heatConductance_Fat_Skin);

            //動脈
            bm.SetValue(3, 0, -heatConductance_Vein_Core);
            bm.SetValue(3, 1, 0);
            bm.SetValue(3, 2, 0);
            bm.SetValue(3, 3, heatCapacity_Artery * 3600d / dt + RCS * bloodFlow_Artery + heatConductance_Vein_Core + heatConductance_Artery_DeepVein);
            bm.SetValue(3, 4, -heatConductance_Artery_DeepVein);
            if (hasAVA) bm.SetValue(3, 5, 0);
            bm.SetValue(3, avaPos, 0);
            bm.SetValue(3, avaPos + 1, 0);

            //深部静脈
            bm.SetValue(4, 0, -(RCS * bloodFlow_Core + heatConductance_Vein_Core));
            bm.SetValue(4, 1, -ContactPortionRate * RCS * bloodFlow_Skin);
            bm.SetValue(4, 2, -NonContactPortionRate * RCS * bloodFlow_Skin);
            bm.SetValue(4, 3, -heatConductance_Artery_DeepVein);
            sum = heatCapacity_DeepVein * 3600d / dt
                + RCS * (bloodFlow_Core + bloodFlow_Muscle + bloodFlow_Fat + bloodFlow_Skin)
                + heatConductance_Vein_Core + heatConductance_Artery_DeepVein;
            foreach (BodyPart bp in bpConnectTo)
            {
                if (Position == HumanBody.Nodes.Pelvis) sum += RCS * (bp.GetBloodFlow(Segments.DeepVein) + bp.GetBloodFlow(Segments.SuperficialVein));
                else sum += RCS * bp.GetBloodFlow(Segments.DeepVein);
            }
            bm.SetValue(4, 4, sum);
            if (hasAVA) bm.SetValue(4, 5, 0);
            bm.SetValue(4, avaPos, -RCS * bloodFlow_Muscle);
            bm.SetValue(4, avaPos + 1, -RCS * bloodFlow_Fat);

            //表在静脈
            if (hasAVA)
            {
                bm.SetValue(5, 0, 0);
                bm.SetValue(5, 1, -ContactPortionRate * heatConductance_SuperficialVein_Skin);
                bm.SetValue(5, 2, -NonContactPortionRate * heatConductance_SuperficialVein_Skin);
                //四肢末端部の場合
                if ((Position & HumanBody.Nodes.TerminalPart) != HumanBody.Nodes.None) bm.SetValue(5, 3, -RCS * GetBloodFlow(Segments.SuperficialVein));
                //その他
                else bm.SetValue(5, 3, 0);
                bm.SetValue(5, 4, 0);
                sum = heatCapacity_SuperficialVein * 3600d / dt + heatConductance_SuperficialVein_Skin;
                if ((Position & HumanBody.Nodes.TerminalPart) != HumanBody.Nodes.None) sum += RCS * GetBloodFlow(Segments.SuperficialVein);
                if (Position != HumanBody.Nodes.Pelvis)
                {
                    foreach (BodyPart bp in bpConnectTo) sum += RCS * bp.GetBloodFlow(Segments.SuperficialVein);
                }
                bm.SetValue(5, 5, sum);
                bm.SetValue(5, avaPos, 0);
                bm.SetValue(5, avaPos + 1, 0);
            }

            //筋肉層
            bm.SetValue(avaPos, 0, -heatConductance_Core_Muscle);
            bm.SetValue(avaPos, 1, 0);
            bm.SetValue(avaPos, 2, 0);
            bm.SetValue(avaPos, 3, -RCS * bloodFlow_Muscle);
            bm.SetValue(avaPos, 4, 0);
            bm.SetValue(avaPos, 5, 0);
            if (hasAVA) bm.SetValue(avaPos, 5, 0);
            bm.SetValue(avaPos, avaPos, heatCapacity_Muscle * 3600d / dt + RCS * bloodFlow_Muscle + heatConductance_Core_Muscle + heatConductance_Muscle_Fat);
            bm.SetValue(avaPos, avaPos + 1, -heatConductance_Muscle_Fat);

            //脂肪層
            bm.SetValue(avaPos + 1, 0, 0);
            bm.SetValue(avaPos + 1, 1, -ContactPortionRate * heatConductance_Fat_Skin);
            bm.SetValue(avaPos + 1, 2, -NonContactPortionRate * heatConductance_Fat_Skin);
            bm.SetValue(avaPos + 1, 3, -RCS * bloodFlow_Fat);
            bm.SetValue(avaPos + 1, 4, 0);
            bm.SetValue(avaPos + 1, 5, 0);
            if (hasAVA) bm.SetValue(avaPos + 1, 5, 0);
            bm.SetValue(avaPos + 1, avaPos, -heatConductance_Muscle_Fat);
            bm.SetValue(avaPos + 1, avaPos + 1, heatCapacity_Fat * 3600d / dt + RCS * bloodFlow_Fat + heatConductance_Muscle_Fat + heatConductance_Fat_Skin);
        }

        /// <summary>温度[C]を設定する</summary>
        /// <param name="temperatures">温度[C]</param>
        internal void setTemperature(VectorView temperatures)
        {
            coreTemperature = temperatures.GetValue(0);
            SkinTemperature_Contact = temperatures.GetValue(1);
            SkinTemperature_NonContact = temperatures.GetValue(2);
            arteryTemperature = temperatures.GetValue(3);
            deepVeinTemperature = temperatures.GetValue(4);
            if (heatCapacity_SuperficialVein != 0) superficialVeinTemperature = temperatures.GetValue(5);

            if (! isJOSModel)
            {
                if (heatCapacity_SuperficialVein != 0)
                {
                    muscleTemperature = temperatures.GetValue(6);
                    fatTemperature = temperatures.GetValue(7);
                }
                else
                {
                    muscleTemperature = temperatures.GetValue(5);
                    fatTemperature = temperatures.GetValue(6);
                }
            }
        }

        /// <summary>体の温度[C]を初期化する</summary>
        /// <param name="temperature">体の温度[C]</param>
        internal void initializeTemperature(double temperature)
        {
            coreTemperature = temperature;
            muscleTemperature = temperature;
            fatTemperature = temperature;
            SkinTemperature_Contact = temperature;
            SkinTemperature_NonContact = temperature;
            deepVeinTemperature = temperature;
            arteryTemperature = temperature;
            superficialVeinTemperature = temperature;
        }

        /// <summary>体の部位を接続する</summary>
        /// <param name="bpConnectTo">接続先の体の部位</param>
        internal void connect(BodyPart bpConnectTo)
        {
            this.bpConnectTo.Add(bpConnectTo);
            bpConnectTo.bpConnectFrom = this;
        }

        /// <summary>仕事による負荷[W]を設定する</summary>
        /// <param name="workLoad">仕事による負荷[W]</param>
        internal void setWorkLoad(double workLoad)
        {
            switch (Position)
            {
                case HumanBody.Nodes.Head:
                    WorkLoad = 0;
                    return;
                case HumanBody.Nodes.Neck:
                    WorkLoad = 0;
                    return;
                case HumanBody.Nodes.Chest:
                    WorkLoad = 0.091 * workLoad;
                    return;
                case HumanBody.Nodes.Back:
                    WorkLoad = 0.080 * workLoad;
                    return;
                case HumanBody.Nodes.Pelvis:
                    WorkLoad = 0.129 * workLoad;
                    return;
                case HumanBody.Nodes.LeftShoulder:
                case HumanBody.Nodes.RightShoulder:
                    WorkLoad = 0.026 * workLoad;
                    return;
                case HumanBody.Nodes.LeftArm:
                case HumanBody.Nodes.RightArm:
                    WorkLoad = 0.014 * workLoad;
                    return;
                case HumanBody.Nodes.LeftHand:
                case HumanBody.Nodes.RightHand:
                    WorkLoad = 0.005 * workLoad;
                    return;
                case HumanBody.Nodes.LeftThigh:
                case HumanBody.Nodes.RightThigh:
                    WorkLoad = 0.201 * workLoad;
                    return;
                case HumanBody.Nodes.LeftLeg:
                case HumanBody.Nodes.RightLeg:
                    WorkLoad = 0.099 * workLoad;
                    return;
                case HumanBody.Nodes.LeftFoot:
                case HumanBody.Nodes.RightFoot:
                    WorkLoad = 0.005 * workLoad;
                    return;
            }
        }

        /// <summary>血流量[L/h]を更新する</summary>
        internal void updateBloodFlow()
        {
            bloodFlow_SuperficialVein = bloodFlow_AVA;
            bloodFlow_DeepVein = bloodFlow_Core + bloodFlow_Muscle + bloodFlow_Fat + bloodFlow_Skin;

            //下流の部位の血流量[L/h]を更新
            foreach (BodyPart bp in bpConnectTo)
            {
                bp.updateBloodFlow();
                if (Position == HumanBody.Nodes.Pelvis)
                {
                    bloodFlow_DeepVein += bp.bloodFlow_DeepVein + bp.bloodFlow_SuperficialVein;
                }
                else
                {
                    bloodFlow_SuperficialVein += bp.bloodFlow_SuperficialVein;
                    bloodFlow_DeepVein += bp.bloodFlow_DeepVein;
                }
            }

            //静脈血流の積算=動脈血流
            bloodFlow_Artery = bloodFlow_SuperficialVein + bloodFlow_DeepVein;
        }

        /// <summary>姿勢を更新する</summary>
        internal void updatePosture()
        {
            //姿勢が立位の場合
            if (Body.Posture == HumanBody.BodyPosture.Standing)
            {
                switch (Position)
                {
                    case HumanBody.Nodes.Head:
                        RadiativeHeatTransferCoefficient = 4.89;
                        //ConvectiveHeatTransferCoefficient = 4.48;
                        break;
                    case HumanBody.Nodes.Neck:
                        RadiativeHeatTransferCoefficient = 4.89;
                        //ConvectiveHeatTransferCoefficient = 4.48;
                        break;
                    case HumanBody.Nodes.Chest:
                        RadiativeHeatTransferCoefficient = 4.32;
                        //ConvectiveHeatTransferCoefficient = 2.97;
                        break;
                    case HumanBody.Nodes.Back:
                        RadiativeHeatTransferCoefficient = 4.09;
                        //ConvectiveHeatTransferCoefficient = 2.91;
                        break;
                    case HumanBody.Nodes.Pelvis:
                        RadiativeHeatTransferCoefficient = 4.32;
                        //ConvectiveHeatTransferCoefficient = 2.85;
                        break;
                    case HumanBody.Nodes.LeftShoulder:
                    case HumanBody.Nodes.RightShoulder:
                        RadiativeHeatTransferCoefficient = 4.55;
                        //ConvectiveHeatTransferCoefficient = 3.61;
                        break;
                    case HumanBody.Nodes.LeftArm:
                    case HumanBody.Nodes.RightArm:
                        RadiativeHeatTransferCoefficient = 4.43;
                        //ConvectiveHeatTransferCoefficient = 3.55;
                        break;
                    case HumanBody.Nodes.LeftHand:
                    case HumanBody.Nodes.RightHand:
                        RadiativeHeatTransferCoefficient = 4.21;
                        //ConvectiveHeatTransferCoefficient = 3.67;
                        break;
                    case HumanBody.Nodes.LeftThigh:
                    case HumanBody.Nodes.RightThigh:
                        RadiativeHeatTransferCoefficient = 4.77;
                        //ConvectiveHeatTransferCoefficient = 2.80;
                        break;
                    case HumanBody.Nodes.LeftLeg:
                    case HumanBody.Nodes.RightLeg:
                        RadiativeHeatTransferCoefficient = 5.34;
                        //ConvectiveHeatTransferCoefficient = 2.04;
                        break;
                    case HumanBody.Nodes.LeftFoot:
                    case HumanBody.Nodes.RightFoot:
                        RadiativeHeatTransferCoefficient = 6.14;
                        //ConvectiveHeatTransferCoefficient = 2.04;
                        break;
                }
            }
            //姿勢が座位の場合
            else if (Body.Posture == HumanBody.BodyPosture.Standing)
            {
                switch (Position)
                {
                    case HumanBody.Nodes.Head:
                        RadiativeHeatTransferCoefficient = 4.96;
                        //ConvectiveHeatTransferCoefficient = 4.75;
                        break;
                    case HumanBody.Nodes.Neck:
                        RadiativeHeatTransferCoefficient = 4.96;
                        //ConvectiveHeatTransferCoefficient = 4.75;
                        break;
                    case HumanBody.Nodes.Chest:
                        RadiativeHeatTransferCoefficient = 3.99;
                        //ConvectiveHeatTransferCoefficient = 3.12;
                        break;
                    case HumanBody.Nodes.Back:
                        RadiativeHeatTransferCoefficient = 4.64;
                        //ConvectiveHeatTransferCoefficient = 2.48;
                        break;
                    case HumanBody.Nodes.Pelvis:
                        RadiativeHeatTransferCoefficient = 4.21;
                        //ConvectiveHeatTransferCoefficient = 1.84;
                        break;
                    case HumanBody.Nodes.LeftShoulder:
                    case HumanBody.Nodes.RightShoulder:
                        RadiativeHeatTransferCoefficient = 4.96;
                        //ConvectiveHeatTransferCoefficient = 3.76;
                        break;
                    case HumanBody.Nodes.LeftArm:
                    case HumanBody.Nodes.RightArm:
                        RadiativeHeatTransferCoefficient = 4.21;
                        //ConvectiveHeatTransferCoefficient = 3.62;
                        break;
                    case HumanBody.Nodes.LeftHand:
                    case HumanBody.Nodes.RightHand:
                        RadiativeHeatTransferCoefficient = 4.74;
                        //ConvectiveHeatTransferCoefficient = 2.06;
                        break;
                    case HumanBody.Nodes.LeftThigh:
                    case HumanBody.Nodes.RightThigh:
                        RadiativeHeatTransferCoefficient = 4.10;
                        //ConvectiveHeatTransferCoefficient = 2.98;
                        break;
                    case HumanBody.Nodes.LeftLeg:
                    case HumanBody.Nodes.RightLeg:
                        RadiativeHeatTransferCoefficient = 4.74;
                        //ConvectiveHeatTransferCoefficient = 2.98;
                        break;
                    case HumanBody.Nodes.LeftFoot:
                    case HumanBody.Nodes.RightFoot:
                        RadiativeHeatTransferCoefficient = 6.36;
                        //ConvectiveHeatTransferCoefficient = 2.62;
                        break;
                }
            }
        }

        #endregion

        #region privateメソッド

        /// <summary>対流熱伝達率[W/(m2 K)]を更新する</summary>
        private void updateConvectiveHeatTransferCoefficients()
        {
            switch (Position)
            {
                case HumanBody.Nodes.Head:
                case HumanBody.Nodes.Neck:
                    if (Velocity < 0.2) ConvectiveHeatTransferCoefficient = 7.7;
                    else ConvectiveHeatTransferCoefficient = 15 * Math.Pow(Velocity, 0.62);
                    break;
                case HumanBody.Nodes.Chest:
                    if (Velocity < 0.2) ConvectiveHeatTransferCoefficient = 5.1;
                    else ConvectiveHeatTransferCoefficient = 11 * Math.Pow(Velocity, 0.67);
                    break;
                case HumanBody.Nodes.Back:
                    if (Velocity < 0.2) ConvectiveHeatTransferCoefficient = 5.0;
                    else ConvectiveHeatTransferCoefficient = 17 * Math.Pow(Velocity, 0.49);
                    break;
                case HumanBody.Nodes.Pelvis:
                    if (Velocity < 0.2) ConvectiveHeatTransferCoefficient = 4.9;
                    else ConvectiveHeatTransferCoefficient = 13 * Math.Pow(Velocity, 0.60);
                    break;
                case HumanBody.Nodes.LeftShoulder:
                case HumanBody.Nodes.RightShoulder:
                    if (Velocity < 0.2) ConvectiveHeatTransferCoefficient = 6.2;
                    else ConvectiveHeatTransferCoefficient = 17 * Math.Pow(Velocity, 0.59);
                    break;
                case HumanBody.Nodes.LeftArm:
                case HumanBody.Nodes.RightArm:
                    if (Velocity < 0.2) ConvectiveHeatTransferCoefficient = 6.1;
                    else ConvectiveHeatTransferCoefficient = 17 * Math.Pow(Velocity, 0.61);
                    break;
                case HumanBody.Nodes.LeftHand:
                case HumanBody.Nodes.RightHand:
                    if (Velocity < 0.2) ConvectiveHeatTransferCoefficient = 6.3;
                    else ConvectiveHeatTransferCoefficient = 20 * Math.Pow(Velocity, 0.60);
                    break;
                case HumanBody.Nodes.LeftThigh:
                case HumanBody.Nodes.RightThigh:
                    if (Velocity < 0.2) ConvectiveHeatTransferCoefficient = 4.8;
                    else ConvectiveHeatTransferCoefficient = 14 * Math.Pow(Velocity, 0.61);
                    break;
                case HumanBody.Nodes.LeftLeg:
                case HumanBody.Nodes.RightLeg:
                    if (Velocity < 0.2) ConvectiveHeatTransferCoefficient = 3.5;
                    else ConvectiveHeatTransferCoefficient = 15.8 * Math.Pow(Velocity, 0.74);
                    break;
                case HumanBody.Nodes.LeftFoot:
                case HumanBody.Nodes.RightFoot:
                    if (Velocity < 0.2) ConvectiveHeatTransferCoefficient = 3.5;
                    else ConvectiveHeatTransferCoefficient = 15.1 * Math.Pow(Velocity, 0.62);
                    break;
            }
        }

        /// <summary>皮膚-相当温度の熱コンダクタンスを更新する</summary>
        private void updateHeatConductance_Skin_Air()
        {
            double fcl = 1 + 0.3 * ClothingIndex;   //Clothing area factor
            HeatConductance_Skin_Air = SurfaceArea / (0.155 * ClothingIndex +
                1 / fcl / (RadiativeHeatTransferCoefficient + ConvectiveHeatTransferCoefficient * Body.convectiveHeatTransferCoefficientMod));
            LatentHeatConductance_Skin_Air = SurfaceArea * LOUIS_COEFFICIENT * 0.45
                / (0.155 * clothingIndex + 0.45 / (ConvectiveHeatTransferCoefficient * Body.convectiveHeatTransferCoefficientMod) / fcl);
        }

        #endregion

        #region 制御系の計算

        /// <summary>体温調節用の部位温冷感シグナルを計算する</summary>
        /// <returns>部位温冷感シグナル</returns>
        internal double getPartSignal()
        {
            switch (Position)
            {
                case HumanBody.Nodes.Head:
                    return (GetTemperature(Segments.Skin) - SetPoint_Skin) * 0.0549;
                case HumanBody.Nodes.Neck:
                    return (GetTemperature(Segments.Skin) - SetPoint_Skin) * 0.0146;
                case HumanBody.Nodes.Chest:
                    return (GetTemperature(Segments.Skin) - SetPoint_Skin) * 0.1492;
                case HumanBody.Nodes.Back:
                    return (GetTemperature(Segments.Skin) - SetPoint_Skin) * 0.1321;
                case HumanBody.Nodes.Pelvis:
                    return (GetTemperature(Segments.Skin) - SetPoint_Skin) * 0.2122;
                case HumanBody.Nodes.LeftShoulder:
                case HumanBody.Nodes.RightShoulder:
                    return (GetTemperature(Segments.Skin) - SetPoint_Skin) * 0.0227;
                case HumanBody.Nodes.LeftArm:
                case HumanBody.Nodes.RightArm:
                    return (GetTemperature(Segments.Skin) - SetPoint_Skin) * 0.0117;
                case HumanBody.Nodes.LeftHand:
                case HumanBody.Nodes.RightHand:
                    return (GetTemperature(Segments.Skin) - SetPoint_Skin) * 0.0923;
                case HumanBody.Nodes.LeftThigh:
                case HumanBody.Nodes.RightThigh:
                    return (GetTemperature(Segments.Skin) - SetPoint_Skin) * 0.0501;
                case HumanBody.Nodes.LeftLeg:
                case HumanBody.Nodes.RightLeg:
                    return (GetTemperature(Segments.Skin) - SetPoint_Skin) * 0.0251;
                case HumanBody.Nodes.LeftFoot:
                case HumanBody.Nodes.RightFoot:
                    return (GetTemperature(Segments.Skin) - SetPoint_Skin) * 0.0167;
                default:
                    throw new Exception("部位エラー");
            }
        }

        /// <summary>体温調節用の全身温冷感シグナルを設定する</summary>
        /// <param name="signal">体温調節用の全身温冷感シグナル</param>
        /// <param name="sweatSignal">発汗シグナル</param>
        /// <param name="shiveringSignal">ふるえシグナル</param>
        /// <param name="ectasiaSignal">血管拡張シグナル</param>
        /// <param name="coarctationSignal">血管収縮シグナル</param>
        internal void setBodySignal(double signal,
            double sweatSignal, double shiveringSignal, double ectasiaSignal, double coarctationSignal)
        {
            //係数計算
            double dsp = GetTemperature(Segments.Skin) - SetPoint_Skin;
            double pow1 = sweatSignal * Math.Pow(2, dsp / 10d);
            double pow2 = Math.Pow(2, dsp / 6d);

            //制御のON/OFF確認
            if ((Body.Control & HumanBody.ControlMethods.Sweating) == HumanBody.ControlMethods.None) pow1 = 0;
            if ((Body.Control & HumanBody.ControlMethods.Shivering) == HumanBody.ControlMethods.None) shiveringSignal = 0;
            if ((Body.Control & HumanBody.ControlMethods.SkinVasomotor) == HumanBody.ControlMethods.None)
            {
                ectasiaSignal = 0;
                coarctationSignal = 0;
                pow2 = 1;
            }

            //皮膚血管運動による皮膚部血流量と発汗・ふるえによる発生熱量を計算
            switch (Position)
            {
                case HumanBody.Nodes.Head:
                    EvaporativeHeatLoss_Sweat = pow1 * 0.0640;
                    ShiveringLoad = shiveringSignal * 0.0339;
                    bloodFlow_Skin = (baseBloodFlow_Skin + 0.1043 * ectasiaSignal) / (1 + 0.05 * coarctationSignal) * pow2;
                    break;
                case HumanBody.Nodes.Neck:
                    EvaporativeHeatLoss_Sweat = pow1 * 0.0170;
                    ShiveringLoad = shiveringSignal * 0.0436;
                    bloodFlow_Skin = (baseBloodFlow_Skin + 0.0277 * ectasiaSignal) / (1 + 0.05 * coarctationSignal) * pow2;
                    break;
                case HumanBody.Nodes.Chest:
                    EvaporativeHeatLoss_Sweat = pow1 * 0.1460;
                    ShiveringLoad = shiveringSignal * 0.2739;
                    bloodFlow_Skin = (baseBloodFlow_Skin + 0.0980 * ectasiaSignal) / (1 + 0.15 * coarctationSignal) * pow2;
                    break;
                case HumanBody.Nodes.Back:
                    EvaporativeHeatLoss_Sweat = pow1 * 0.1290;
                    ShiveringLoad = shiveringSignal * 0.2410;
                    bloodFlow_Skin = (baseBloodFlow_Skin + 0.0860 * ectasiaSignal) / (1 + 0.15 * coarctationSignal) * pow2;
                    break;
                case HumanBody.Nodes.Pelvis:
                    EvaporativeHeatLoss_Sweat = pow1 * 0.2060;
                    ShiveringLoad = shiveringSignal * 0.3875;
                    bloodFlow_Skin = (baseBloodFlow_Skin + 0.1380 * ectasiaSignal) / (1 + 0.15 * coarctationSignal) * pow2;
                    break;
                case HumanBody.Nodes.LeftShoulder:
                case HumanBody.Nodes.RightShoulder:
                    EvaporativeHeatLoss_Sweat = pow1 * 0.0510;
                    ShiveringLoad = shiveringSignal * 0.0024;
                    bloodFlow_Skin = (baseBloodFlow_Skin + 0.0313 * ectasiaSignal) / (1 + 0.05 * coarctationSignal) * pow2;
                    break;
                case HumanBody.Nodes.LeftArm:
                case HumanBody.Nodes.RightArm:
                    EvaporativeHeatLoss_Sweat = pow1 * 0.0260;
                    ShiveringLoad = shiveringSignal * 0.0014;
                    bloodFlow_Skin = (baseBloodFlow_Skin + 0.0163 * ectasiaSignal) / (1 + 0.05 * coarctationSignal) * pow2;
                    break;
                case HumanBody.Nodes.LeftHand:
                case HumanBody.Nodes.RightHand:
                    EvaporativeHeatLoss_Sweat = pow1 * 0.0155;
                    ShiveringLoad = shiveringSignal * 0.0002;
                    bloodFlow_Skin = (baseBloodFlow_Skin + 0.0605 * ectasiaSignal) / (1 + 0.35 * coarctationSignal) * pow2;
                    break;
                case HumanBody.Nodes.LeftThigh:
                case HumanBody.Nodes.RightThigh:
                    EvaporativeHeatLoss_Sweat = pow1 * 0.0730;
                    ShiveringLoad = shiveringSignal * 0.0039;
                    bloodFlow_Skin = (baseBloodFlow_Skin + 0.0920 * ectasiaSignal) / (1 + 0.05 * coarctationSignal) * pow2;
                    break;
                case HumanBody.Nodes.LeftLeg:
                case HumanBody.Nodes.RightLeg:
                    EvaporativeHeatLoss_Sweat = pow1 * 0.0360;
                    ShiveringLoad = shiveringSignal * 0.0018;
                    bloodFlow_Skin = (baseBloodFlow_Skin + 0.0230 * ectasiaSignal) / (1 + 0.05 * coarctationSignal) * pow2;
                    break;
                case HumanBody.Nodes.LeftFoot:
                case HumanBody.Nodes.RightFoot:
                    EvaporativeHeatLoss_Sweat = pow1 * 0.0175;
                    ShiveringLoad = shiveringSignal * 0.0004;
                    bloodFlow_Skin = (baseBloodFlow_Skin + 0.0500 * ectasiaSignal) / (1 + 0.35 * coarctationSignal) * pow2;
                    break;
            }

            //筋肉層血流量[L/h]を更新
            bloodFlow_Muscle = baseBloodFlow_Muscle + (WorkLoad + ShiveringLoad) / 1.16;
        }

        /// <summary>AVA血管への割合[-]を設定する</summary>
        /// <param name="avaRate">AVA血管への割合[-]</param>
        internal void setAVARate(double avaRate)
        {
            if ((Body.Control & HumanBody.ControlMethods.AVA) == HumanBody.ControlMethods.None)
            {
                bloodFlow_AVA = 0;
                return;
            }

            double bfRate = Body.BaseBloodFlowRate / HumanBody.STANDARD_BLOOD_FLOW;
            double wtRate = Body.Weight / HumanBody.STANDARD_WEIGHT;

            this.avaRate = Math.Max(0, Math.Min(1, avaRate));

            //四肢末端のみ設定を反映
            if ((Position == HumanBody.Nodes.LeftHand) |
               (Position == HumanBody.Nodes.RightHand))
            {
                bloodFlow_AVA = 1.71 * wtRate * bfRate * this.avaRate;
            }
            else if ((Position == HumanBody.Nodes.LeftFoot) |
               (Position == HumanBody.Nodes.RightFoot))
            {
                bloodFlow_AVA = 2.16 * wtRate * bfRate * this.avaRate;
            }
        }

        #endregion

    }

    /// <summary>体の部位（読み取り専用）</summary>
    public interface ImmutableBodyPart
    {

        #region プロパティ

        /// <summary>JOSモデル（脂肪層および筋肉層をコア層に統合）か否かを取得する</summary>
        bool IsJOSModel
        {
            get;
        }

        /// <summary>体を取得する</summary>
        HumanBody Body
        {
            get;
        }

        /// <summary>体の部位を取得する</summary>
        HumanBody.Nodes Position
        {
            get;
        }

        /// <summary>体表面積[m2]を取得する</summary>
        double SurfaceArea
        {
            get;
        }

        /// <summary>重量[kg]を取得する</summary>
        double Weight
        {
            get;
        }

        /// <summary>体脂肪率[%]を取得する</summary>
        double FatPercentage
        {
            get;
        }

        /// <summary>仕事量[W]を取得する</summary>
        double WorkLoad
        {
            get;
        }

        /// <summary>皮膚-物体間の熱コンダクタンス[W/(m2 K)]を取得する</summary>
        /// <remarks>単位はW/(m2 K)</remarks>
        double HeatConductance_Skin_Material
        {
            get;
        }

        /// <summary>皮膚-空気間の熱コンダクタンス[W/K]を取得する</summary>
        double HeatConductance_Skin_Air
        {
            get;
        }

        /// <summary>皮膚-空気間の湿気熱コンダクタンス[W/kPa]を取得する</summary>
        double LatentHeatConductance_Skin_Air
        {
            get;
        }

        /// <summary>放射熱伝達率[W/(m2 K)]を取得する</summary>
        double RadiativeHeatTransferCoefficient
        {
            get;
        }

        /// <summary>対流熱伝達率[W/(m2 K)]を取得する</summary>
        double ConvectiveHeatTransferCoefficient
        {
            get;
        }

        /// <summary>接触部の皮膚温度[C]を取得する</summary>
        double SkinTemperature_Contact
        {
            get;
        }

        /// <summary>非接触部の皮膚温度[C]を取得する</summary>
        double SkinTemperature_NonContact
        {
            get;
        }

        /// <summary>皮膚接触部の割合[-]を取得する</summary>
        double ContactPortionRate
        {
            get;
        }

        /// <summary>皮膚非接触部の割合[-]を取得する</summary>
        double NonContactPortionRate
        {
            get;
        }

        /// <summary>着衣量[clo]を取得する</summary>
        double ClothingIndex
        {
            get;
        }

        /// <summary>コアのセットポイント[C]を取得する</summary>
        double SetPoint_Core
        {
            get;
        }

        /// <summary>皮膚のセットポイント[C]を取得する</summary>
        double SetPoint_Skin
        {
            get;
        }

        /// <summary>発汗による蒸発熱損失[W]を取得する</summary>
        double EvaporativeHeatLoss_Sweat
        {
            get;
        }

        /// <summary>発汗および不感蒸泄による蒸発熱損失[W]を取得する</summary>
        double EvaporativeHeatLoss
        {
            get;
        }

        /// <summary>ふるえによる熱生成量[W]を取得する</summary>
        double ShiveringLoad
        {
            get;
        }

        /// <summary>接触物体の温度[C]を取得する</summary>
        double MaterialTemperature
        {
            get;
        }

        /// <summary>近傍の空気の相対湿度[%]を取得する</summary>
        double RelativeHumidity
        {
            get;
        }

        /// <summary>平均放射温度[C]を取得する</summary>
        double MeanRadiantTemperature
        {
            get;
        }

        /// <summary>近傍の空気の乾球温度[C]を取得する</summary>
        double DrybulbTemperature
        {
            get;
        }

        /// <summary>気流速度[m/s]を取得する</summary>
        double Velocity
        {
            get;
        }

        /// <summary>作用温度[C]を取得する</summary>
        double OperativeTemperature
        {
            get;
        }

        /// <summary>接続先の体の部位一覧を取得する</summary>
        ImmutableBodyPart[] BodyPartConnectTo
        {
            get;
        }

        /// <summary>接続もとの体の部位を取得する</summary>
        ImmutableBodyPart BodyPartConnectFrom
        {
            get;
        }

        #endregion

        #region publicメソッド

        /// <summary>顕熱損失[W]を計算する</summary>
        /// <returns>顕熱損失[W]</returns>
        double GetSensibleHeatLoss();

        /// <summary>血流量[L/h]を取得する</summary>
        /// <param name="bType">血管の種類</param>
        /// <returns>血流量[L/h]</returns>
        double GetBloodFlow(BodyPart.Segments bType);

        /// <summary>部位の熱容量[Wh/K]を取得する</summary>
        /// <param name="component">部位</param>
        /// <returns>部位の熱容量[Wh/K]</returns>
        double GetHeatCapacity(BodyPart.Segments component);

        /// <summary>温度[C]を取得する</summary>
        /// <param name="component">部位</param>
        /// <returns>温度[C]</returns>
        double GetTemperature(BodyPart.Segments component);

        /// <summary>部位間の熱コンダクタンス[W/K]を取得する</summary>
        /// <param name="component1">部位1</param>
        /// <param name="component2">部位2</param>
        /// <returns>部位間の熱コンダクタンス[W/K]</returns>
        double GetHeatConductance(BodyPart.Segments component1, BodyPart.Segments component2);

        /// <summary>代謝量[W]を取得する</summary>
        /// <param name="component">部位</param>
        /// <returns>代謝量[W]</returns>
        double GetMetabolicRate(BodyPart.Segments component);

        /// <summary>部位1から部位2への熱移動量[W]を計算する</summary>
        /// <param name="component1">部位1</param>
        /// <param name="component2">部位2</param>
        /// <returns>部位1から部位2への熱移動量[W]</returns>
        double GetHeatTransfer(BodyPart.Segments component1, BodyPart.Segments component2);

        #endregion
    }

}
