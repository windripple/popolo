/* HumanBody.cs
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

using Popolo.Numerics;

namespace Popolo.ThermalComfort
{

    /// <summary>人体の非定常モデル</summary>
    /// <remarks>
    /// 田辺新一, 佐藤孝広, 和田良祐
    /// 人間-熱環境系快適性数値シミュレータ(その29) : 人体熱モデルREALの開発-着衣層モデル化と実験値との比較
    /// 日本建築学会大会学術講演梗概集, 2004, pp.517-518
    /// 
    /// 上記文献にあるモデルを後退差分式に改良したもの
    /// </remarks>
    public class HumanBody
    {

        #region enumerators

        /// <summary>体の部位</summary>
        [Flags]
        public enum Nodes
        {
            /// <summary>不定</summary>
            None = 0,
            /// <summary>頭</summary>
            Head = 1,
            /// <summary>首</summary>
            Neck = 2,
            /// <summary>胸</summary>
            Chest = 4,
            /// <summary>背中</summary>
            Back = 8,
            /// <summary>腰</summary>
            Pelvis = 16,
            /// <summary>左肩</summary>
            LeftShoulder = 32,
            /// <summary>左腕</summary>
            LeftArm = 64,
            /// <summary>左手</summary>
            LeftHand = 128,
            /// <summary>右肩</summary>
            RightShoulder = 256,
            /// <summary>右腕</summary>
            RightArm = 512,
            /// <summary>右手</summary>
            RightHand = 1024,
            /// <summary>左太股</summary>
            LeftThigh = 2048,
            /// <summary>左ふくらはぎ</summary>
            LeftLeg = 4096,
            /// <summary>左足</summary>
            LeftFoot = 8192,
            /// <summary>右太股</summary>
            RightThigh = 16384,
            /// <summary>右ふくらはぎ</summary>
            RightLeg = 32768,
            /// <summary>右足</summary>
            RightFoot = 65536,
            /// <summary>四肢末端部</summary>
            TerminalPart = LeftHand | RightHand | LeftFoot | RightFoot
        }

        /// <summary>姿勢</summary>
        public enum BodyPosture
        {
            /// <summary>座位</summary>
            Sitting,
            /// <summary>立位</summary>
            Standing
        }

        /// <summary>体温制御方法</summary>
        [Flags]
        public enum ControlMethods
        {
            /// <summary>無し</summary>
            None = 0,
            /// <summary>皮膚血管運動</summary>
            SkinVasomotor = 1,
            /// <summary>発汗</summary>
            Sweating = 2,
            /// <summary>ふるえ</summary>
            Shivering = 4,
            /// <summary>AVA血流</summary>
            AVA = 8,
            /// <summary>全て</summary>
            All = SkinVasomotor | Sweating | Shivering | AVA
        }

        #endregion

        #region Constants

        /// <summary>シグナル変化限界値</summary>
        /// <remarks>可変タイムステップによる計算高速化のための定数</remarks>
        private const double MAX_SIGNAL_CHANGE = 0.1;//0.1

        /// <summary>血液の体積比熱[kJ/(LK)]</summary>
        public const double RHO_C = 4.186;

        /// <summary>標準体躯の表面積[m2]</summary>
        public const double STANDARD_SURFACE_AREA = 1.87;

        /// <summary>標準体躯の重量[kg]</summary>
        public const double STANDARD_WEIGHT = 74.43;

        /// <summary>標準体躯の代謝量[W]</summary>
        public const double STANDARD_MET = 84.656;

        /// <summary>標準体躯の血流量[L/h]</summary>
        public const double STANDARD_BLOOD_FLOW = 290.004;

        #endregion

        #region Instance variables

        /// <summary>部位-配列番号対応付け</summary>
        private Dictionary<Nodes, uint> bpDict = new Dictionary<Nodes, uint>();

        /// <summary>BM行列</summary>
        private Matrix bmMatrix;

        /// <summary>ZMベクトル</summary>
        private Vector zmVector;

        /// <summary>置換ベクトル</summary>
        private uint[] perm;

        /// <summary>体重[kg]</summary>
        private double weight;

        /// <summary>身長[m]</summary>
        private double height;

        /// <summary>年齢</summary>
        private double age;

        /// <summary>体脂肪率[-]</summary>
        private double fatPercentage = 0.15;

        /// <summary>体の部位</summary>
        private Dictionary<Nodes, BodyPart> parts = new Dictionary<Nodes, BodyPart>();

        /// <summary>安静時の心係数[L/(min m^2)]</summary>
        private double cardiacIndexAtRest;

        /// <summary>計算時間間隔[sec]</summary>
        internal double timeStep = 10;

        /// <summary>平均セットポイント</summary>
        private double averageCoreSetPoint, averageSkinSetPoint;

        /// <summary>前回の計算時の温冷感シグナル</summary>
        private double lastSignal = 9999;

        #endregion

        #region Properties

        /// <summary>体温制御方法を設定・取得する</summary>
        public ControlMethods Control
        {
            get;
            set;
        }

        /// <summary>体重[kg]を取得する</summary>
        public double Weight
        {
            private set
            {
                weight = Math.Max(20, Math.Min(value, 100));
            }
            get
            {
                return weight;
            }
        }

        /// <summary>身長[m]を取得する</summary>
        public double Height
        {
            private set
            {
                height = Math.Max(1.2, Math.Min(value, 2.6));
            }
            get
            {
                return height;
            }
        }

        /// <summary>年齢を取得する</summary>
        public double Age
        {
            private set
            {
                age = Math.Max(20, Math.Min(value, 100));
            }
            get
            {
                return age;
            }
        }

        /// <summary>男性か否かの情報を取得する</summary>
        public bool IsMale
        {
            get;
            private set;
        }

        /// <summary>安静時の心係数[L/(min m^2)]を取得する</summary>
        public double CardiacIndexAtRest
        {
            private set
            {
                cardiacIndexAtRest = Math.Max(2.0, Math.Min(value, 4.0));
            }
            get
            {
                return cardiacIndexAtRest;
            }
        }

        /// <summary>体脂肪率[%]を取得する</summary>
        public double FatPercentage
        {
            private set
            {
                fatPercentage = Math.Max(0, Math.Min(value, 100));
            }
            get
            {
                return fatPercentage;
            }
        }

        /// <summary>体表面積[m2]を取得する</summary>
        public double SurfaceArea
        {
            get;
            private set;
        }

        /// <summary>基礎代謝量[W]を取得する</summary>
        public double BasicMetabolicRate
        {
            get;
            private set;
        }

        /// <summary>基礎血流量[L/h]を取得する</summary>
        public double BaseBloodFlowRate
        {
            get;
            private set;
        }

        /// <summary>現在の全身血流量[L/h]を取得する</summary>
        public double CurrentBloodFlowRate
        {
            get;
            private set;
        }

        /// <summary>中央血液だまりの熱容量[Wh/K]を取得する</summary>
        public double HeatCapacity_CentralBloodPool
        {
            get;
            private set;
        }

        /// <summary>中央血液だまりの温度[C]を取得する</summary>
        public double CentralBloodPoolTemperature
        {
            get;
            private set;
        }

        /// <summary>姿勢を取得する</summary>
        public BodyPosture Posture
        {
            get;
            private set;
        }

        /// <summary>呼吸による熱損失[W]を取得する</summary>
        public double HeatLossByBreathing
        {
            get;
            private set;
        }

        /// <summary>大気圧[kPa]を設定・取得する</summary>
        public double AtmosphericPressure
        {
            get;
            set;
        }

        /// <summary>対流熱伝達率の補正係数[-]を取得する</summary>
        internal double convectiveHeatTransferCoefficientMod
        {
            get;
            private set;
        }

        #endregion

        #region Constructor

        /// <summary>Constructor</summary>
        /// <remarks>引数が無い場合には標準体躯となる</remarks>
        public HumanBody()
        {
            AtmosphericPressure = 101.325;

            initialize(74.43, 1.72, 25, true, 2.58, 15);
        }

        /// <summary>Constructor</summary>
        /// <param name="weight">体重[kg]</param>
        /// <param name="height">身長[m]</param>
        /// <param name="age">年齢</param>
        /// <param name="isMale">男性か否か</param>
        /// <param name="cardiacIndexAtRest">安静時の心係数[L/(min m^2)]</param>
        /// <param name="fatPercentage">体脂肪率[%]</param>
        public HumanBody(double weight, double height, double age, bool isMale, double cardiacIndexAtRest, double fatPercentage)
        {
            AtmosphericPressure = 101.325;

            initialize(weight, height, age, isMale, cardiacIndexAtRest, fatPercentage);
        }

        /// <summary>初期化処理</summary>
        /// <param name="weight">体重[kg]</param>
        /// <param name="height">身長[m]</param>
        /// <param name="age">年齢</param>
        /// <param name="isMale">男性か否か</param>
        /// <param name="cardiacIndexAtRest">安静時の心係数[L/(min m^2)]</param>
        /// <param name="fatPercentage">体脂肪率[%]</param>
        private void initialize(double weight, double height, double age, bool isMale, double cardiacIndexAtRest, double fatPercentage)
        {
            Weight = weight;
            Height = height;
            Age = age;
            IsMale = isMale;
            CardiacIndexAtRest = cardiacIndexAtRest;
            FatPercentage = fatPercentage;

            //体表面積を初期化する
            SurfaceArea = 0.202 * Math.Pow(Weight, 0.425) * Math.Pow(Height, 0.725);
            //代謝量[W]を初期化する
            initMetabolicRate();
            //血流量[L/h]を初期化する
            BaseBloodFlowRate = cardiacIndexAtRest * 60 * SurfaceArea;
            //中央血液だまりの熱容量[Wh/K]を初期化する
            HeatCapacity_CentralBloodPool = 1.999 * CardiacIndexAtRest * 60 * SurfaceArea / STANDARD_BLOOD_FLOW;

            //部位を作成
            Nodes[] pos = (Nodes[])Enum.GetValues(typeof(Nodes));
            foreach (Nodes bp in pos)
            {
                if (bp != Nodes.TerminalPart && bp != Nodes.None)
                {
                    parts.Add(bp, new BodyPart(this, bp));
                }
            }

            //部位を接続
            parts[Nodes.Neck].connect(parts[Nodes.Head]);
            parts[Nodes.Pelvis].connect(parts[Nodes.LeftThigh]);
            parts[Nodes.Pelvis].connect(parts[Nodes.RightThigh]);
            parts[Nodes.LeftThigh].connect(parts[Nodes.LeftLeg]);
            parts[Nodes.RightThigh].connect(parts[Nodes.RightLeg]);
            parts[Nodes.LeftLeg].connect(parts[Nodes.LeftFoot]);
            parts[Nodes.RightLeg].connect(parts[Nodes.RightFoot]);
            parts[Nodes.LeftShoulder].connect(parts[Nodes.LeftArm]);
            parts[Nodes.RightShoulder].connect(parts[Nodes.RightArm]);
            parts[Nodes.LeftArm].connect(parts[Nodes.LeftHand]);
            parts[Nodes.RightArm].connect(parts[Nodes.RightHand]);

            //姿勢を設定
            SetPosture(BodyPosture.Standing);

            //仕事量を設定
            SetWorkLoad(58);

            //気流速度[m/s]を設定
            SetVelocity(0);

            //着衣量[clo]を設定
            SetClothingIndex(Nodes.Chest, 0.62);
            SetClothingIndex(Nodes.Back, 0.74);
            SetClothingIndex(Nodes.Pelvis, 1.18);
            SetClothingIndex(Nodes.LeftShoulder, 0.45);
            SetClothingIndex(Nodes.RightShoulder, 0.45);
            SetClothingIndex(Nodes.LeftThigh, 0.38);
            SetClothingIndex(Nodes.LeftLeg, 0.69);
            SetClothingIndex(Nodes.LeftFoot, 1.23);
            SetClothingIndex(Nodes.RightThigh, 0.38);
            SetClothingIndex(Nodes.RightLeg, 0.69);
            SetClothingIndex(Nodes.RightFoot, 1.23);

            //体温を初期化
            InitializeTemperature(36);

            //行列初期化
            uint varSum = 0;
            foreach (Nodes bp in parts.Keys)
            {
                bpDict.Add(bp, varSum);

                varSum += 5;
                BodyPart part = parts[bp];
                //筋肉・脂肪・コアを統合するJOSモデルではない場合
                if (!part.IsJOSModel) varSum += 2;
                //AVAによる表在静脈がある場合
                if (part.GetHeatCapacity(BodyPart.Segments.SuperficialVein) != 0) varSum += 1;
            }

            //計算領域初期化
            varSum++;
            bmMatrix = new Matrix(varSum, varSum);
            zmVector = new Vector(varSum);
            perm = new uint[varSum];

            //セットポイント初期化
            initializeSetPoint();
        }

        #endregion

        #region 初期化処理

        /// <summary>代謝量[W]を初期化する</summary>
        private void initMetabolicRate()
        {
            double ag = Math.Max(25, Math.Min(80, Age));
            if (IsMale) BasicMetabolicRate = (-0.0011465874 * ag * ag - 0.0036442981 * ag + 42.7340474699) * SurfaceArea;
            else BasicMetabolicRate = (0.0004687168 * ag * ag - 0.1175557843 * ag + 41.0329149587) * SurfaceArea;
        }

        #endregion

        #region public methods

        /// <summary>状態を更新する</summary>
        /// <param name="timeStep">経過させる秒数[sec]</param>
        public void Update(double timeStep)
        {
            double remainTime = timeStep;

            while (0 < remainTime)
            {
                //制御信号を取得
                double wrmSig, cldSig, curSig;
                getSignal(out cldSig, out wrmSig);
                curSig = cldSig + wrmSig;
                
                //制御量が上限値を超えている場合には計算時間を1秒とする
                if (MAX_SIGNAL_CHANGE <= Math.Abs(curSig - lastSignal)) this.timeStep = Math.Min(1, remainTime);
                //その他の場合は計算時間間隔を倍にする
                else this.timeStep = Math.Min(this.timeStep * 2, remainTime);

                //残り時間を更新
                remainTime -= this.timeStep;

                //状態更新処理
                update(cldSig, wrmSig);

                //温冷感シグナルを保存
                lastSignal = curSig;
            }
        }

        /// <summary>状態を更新</summary>
        /// <param name="cldSig">寒さシグナル</param>
        /// <param name="wrmSig">暑さシグナル</param>
        private void update(double cldSig, double wrmSig)
        {
            //制御系を更新
            updateBodyControl(cldSig, wrmSig);

            //体温の計算
            updateBodyTemperature();
        }

        /// <summary>体の部位を取得する</summary>
        /// <param name="position">部位の位置</param>
        /// <returns>体の部位</returns>
        public ImmutableBodyPart GetBodyPart(Nodes position)
        {
            if (parts.ContainsKey(position)) return parts[position];
            else return null;
        }

        /// <summary>体の部位リストを取得する</summary>
        /// <returns>体の部位リスト</returns>
        public ImmutableBodyPart[] GetBodyPart()
        {
            List<ImmutableBodyPart> pts = new List<ImmutableBodyPart>();
            foreach (Nodes bp in parts.Keys) pts.Add(parts[bp]);
            return pts.ToArray();
        }
        
        /// <summary>体の温度[C]を初期化する</summary>
        /// <param name="temperature">体の温度[C]</param>
        public void InitializeTemperature(double temperature)
        {
            CentralBloodPoolTemperature = temperature;
            foreach (Nodes bp in parts.Keys) parts[bp].initializeTemperature(temperature);
        }

        /// <summary>体の温度[C]を初期化する</summary>
        /// <param name="bodyPosition">体の部位</param>
        /// <param name="temperature">体の温度[C]</param>
        public void InitializeTemperature(Nodes bodyPosition, double temperature)
        {
            parts[bodyPosition].initializeTemperature(temperature);
        }

        #endregion

        #region 状態取得処理

        /// <summary>全血流[L/h]を取得する</summary>
        /// <returns>全血流[L/h]</returns>
        public double GetBloodFlow()
        {
            double blSum = 0;
            blSum += parts[Nodes.Neck].GetBloodFlow(BodyPart.Segments.Artery);
            blSum += parts[Nodes.Chest].GetBloodFlow(BodyPart.Segments.Artery);
            blSum += parts[Nodes.Back].GetBloodFlow(BodyPart.Segments.Artery);
            blSum += parts[Nodes.Pelvis].GetBloodFlow(BodyPart.Segments.Artery);
            blSum += parts[Nodes.LeftShoulder].GetBloodFlow(BodyPart.Segments.Artery);
            blSum += parts[Nodes.RightShoulder].GetBloodFlow(BodyPart.Segments.Artery);
            return blSum;
        }

        /// <summary>顕熱損失[W]を計算する</summary>
        /// <returns>顕熱損失[W]</returns>
        public double GetSensibleHeatLossFromSkin()
        {
            double lSum = 0;
            foreach (Nodes bp in parts.Keys)
            {
                lSum += parts[bp].GetSensibleHeatLoss();
            }
            return lSum;
        }

        /// <summary>潜熱損失[W]を計算する</summary>
        /// <returns>潜熱損失[W]</returns>
        public double GetLatentHeatLossFromSkin()
        {
            double lSum = 0;
            foreach (Nodes bp in parts.Keys)
            {
                lSum += parts[bp].EvaporativeHeatLoss;
            }
            return lSum;
        }

        /// <summary>代謝量[W]を取得する</summary>
        /// <returns>代謝量[W]</returns>
        /// <remarks>基礎代謝・外部仕事・ふるえを考慮した値</remarks>
        public double GetMetabolicRate()
        {
            double mSum = BasicMetabolicRate;
            foreach (Nodes bp in parts.Keys)
            {
                BodyPart part = parts[bp];
                mSum += part.WorkLoad + part.ShiveringLoad;
            }
            return mSum;
        }

        /// <summary>全身の平均皮膚温[C]を取得する</summary>
        /// <returns>全身の平均皮膚温[C]</returns>
        public double GetAverageSkinTemperature()
        {
            double ctSum = 0;
            foreach (Nodes bp in parts.Keys)
            {
                BodyPart bPart = parts[bp];
                ctSum += bPart.GetTemperature(BodyPart.Segments.Skin) * bPart.SurfaceArea;
            }
            return ctSum / SurfaceArea;
        }

        /// <summary>血流により体の部位1から体の部位2に移動する熱量[W]を計算する</summary>
        /// <param name="bodyPart1">体の部位1</param>
        /// <param name="bodyPart2">体の部位2</param>
        /// <param name="bloodType">血流の種類（動脈・深部静脈・表在静脈）</param>
        /// <returns>血流によりbodyPosition1からbodyPosition2に移動する熱量[W]</returns>
        public double GetHeatTransferWithBloodFlow(ImmutableBodyPart bodyPart1, ImmutableBodyPart bodyPart2, BodyPart.Segments bloodType)
        {
            const double RCS = HumanBody.RHO_C / 3.6d;  //流量単位がL/hなので、ここで単位を調整

            if (bodyPart1 == null || bodyPart2 == null) return 0;

            List<ImmutableBodyPart> bps = new List<ImmutableBodyPart>();
            //動脈の場合
            if (bloodType == BodyPart.Segments.Artery)
            {
                //bp1が上流の場合には熱移動を計算
                bps.AddRange(bodyPart1.BodyPartConnectTo);
                if (bps.Contains(bodyPart2)) return bodyPart1.GetTemperature(bloodType) * bodyPart2.GetBloodFlow(bloodType) * RCS;
            }
            //静脈の場合
            else if (bloodType == BodyPart.Segments.DeepVein || bloodType == BodyPart.Segments.SuperficialVein)
            {
                //bp2が上流の場合には熱移動を計算
                bps.AddRange(bodyPart2.BodyPartConnectTo);
                if (bps.Contains(bodyPart1)) return bodyPart1.GetTemperature(bloodType) * bodyPart1.GetBloodFlow(bloodType) * RCS;
            }

            return 0;
        }

        #endregion

        #region 境界条件設定処理

        /// <summary>心係数[L/(min m^2)]を設定する</summary>
        /// <param name="cardiacIndex">心係数[L/(min m^2)]</param>
        public void SetCardiacIndex(double cardiacIndex)
        {
            //全身の血流量[L/h]を更新する
            BaseBloodFlowRate = cardiacIndexAtRest * 60 * SurfaceArea;
        }

        /// <summary>姿勢を設定する</summary>
        /// <param name="posture">姿勢</param>
        public void SetPosture(BodyPosture posture)
        {
            //姿勢を設定
            Posture = posture;
            //各部位の熱伝達率[W/(m2 K)]を姿勢に応じた値に更新
            foreach (Nodes bp in parts.Keys) parts[bp].updatePosture();
        }

        /// <summary>仕事量[W/m2]を設定する</summary>
        /// <param name="workLoad">仕事量[W/m2]</param>
        public void SetWorkLoad(double workLoad)
        {
            workLoad = Math.Max(0, workLoad * SurfaceArea - BasicMetabolicRate);
            foreach (Nodes bp in parts.Keys) parts[bp].setWorkLoad(workLoad);
        }

        /// <summary>接触面積割合[-]を設定する</summary>
        /// <param name="bodyPosition">体の部位</param>
        /// <param name="contactPortionRate">接触面積割合[-]</param>
        public void SetContactPortionRate(Nodes bodyPosition, double contactPortionRate)
        {
            parts[bodyPosition].setContactPortionRate(contactPortionRate);
        }

        /// <summary>物体への熱コンダクタンス[W/(m2 K)]を設定する</summary>
        /// <param name="bodyPosition">体の部位</param>
        /// <param name="heatConductance">熱コンダクタンス[W/(m2 K)]</param>
        public void SetHeatConductanceToMaterial(Nodes bodyPosition, double heatConductance)
        {
            parts[bodyPosition].HeatConductance_Skin_Material = heatConductance;
        }

        /// <summary>周辺空気の乾球温度[C]を設定する</summary>
        /// <param name="drybulbTemperature">周辺空気の乾球温度[C]</param>
        public void SetDrybulbTemperature(double drybulbTemperature)
        {
            foreach (Nodes bp in parts.Keys) parts[bp].DrybulbTemperature = drybulbTemperature;
        }

        /// <summary>周辺空気の乾球温度[C]を設定する</summary>
        /// <param name="bodyPosition">体の部位</param>
        /// <param name="drybulbTemperature">周辺空気の乾球温度[C]</param>
        public void SetDrybulbTemperature(Nodes bodyPosition, double drybulbTemperature)
        {
            parts[bodyPosition].DrybulbTemperature = drybulbTemperature;
        }

        /// <summary>周辺空気の相対湿度[%]を設定する</summary>
        /// <param name="relativeHumidity">周辺空気の相対湿度[%]</param>
        public void SetRelativeHumidity(double relativeHumidity)
        {
            foreach (Nodes bp in parts.Keys) parts[bp].RelativeHumidity = relativeHumidity;
        }

        /// <summary>周辺空気の相対湿度[%]を設定する</summary>
        /// <param name="bodyPosition">体の部位</param>
        /// <param name="relativeHumidity">周辺空気の相対湿度[%]</param>
        public void SetRelativeHumidity(Nodes bodyPosition, double relativeHumidity)
        {
            parts[bodyPosition].RelativeHumidity = relativeHumidity;
        }

        /// <summary>平均放射温度[C]を設定する</summary>
        /// <param name="meanRadiantTemperature">平均放射温度[C]</param>
        public void SetMeanRadiantTemperature(double meanRadiantTemperature)
        {
            foreach (Nodes bp in parts.Keys) parts[bp].MeanRadiantTemperature = meanRadiantTemperature;
        }

        /// <summary>平均放射温度[C]を設定する</summary>
        /// <param name="bodyPosition">体の部位</param>
        /// <param name="meanRadiantTemperature">平均放射温度[C]</param>
        public void SetMeanRadiantTemperature(Nodes bodyPosition, double meanRadiantTemperature)
        {
            parts[bodyPosition].MeanRadiantTemperature = meanRadiantTemperature;
        }

        /// <summary>接触物体の温度[C]を設定する</summary>
        /// <param name="materialTemperature">周辺空気の乾球温度[C]</param>
        public void SetMaterialTemperature(double materialTemperature)
        {
            foreach (Nodes bp in parts.Keys) parts[bp].MaterialTemperature = materialTemperature;
        }

        /// <summary>周辺空気の乾球温度[C]を設定する</summary>
        /// <param name="bodyPosition">体の部位</param>
        /// <param name="materialTemperature">接触物体の温度[C]</param>
        public void SetMaterialTemperature(Nodes bodyPosition, double materialTemperature)
        {
            parts[bodyPosition].MaterialTemperature = materialTemperature;
        }

        /// <summary>着衣量[clo]を設定する</summary>
        /// <param name="clo">着衣量[clo]</param>
        public void SetClothingIndex(double clo)
        {
            foreach (Nodes bp in parts.Keys) parts[bp].ClothingIndex = clo;
        }

        /// <summary>着衣量[clo]を設定する</summary>
        /// <param name="bodyPosition">体の部位</param>
        /// <param name="clo">着衣量[clo]</param>
        public void SetClothingIndex(Nodes bodyPosition, double clo)
        {
            parts[bodyPosition].ClothingIndex = clo;
        }

        /// <summary>気流速度[m/s]を設定する</summary>
        /// <param name="velocity">気流速度[m/s]</param>
        public void SetVelocity(double velocity)
        {
            foreach (Nodes bp in parts.Keys) parts[bp].Velocity = velocity;
        }

        /// <summary>気流速度[m/s]を設定する</summary>
        /// <param name="bodyPosition">体の部位</param>
        /// <param name="velocity">気流速度[m/s]</param>
        public void SetVelocity(Nodes bodyPosition, double velocity)
        {
            parts[bodyPosition].Velocity = velocity;
        }

        #endregion

        #region private methods

        /// <summary>体温を更新する</summary>
        private void updateBodyTemperature()
        {
            const double RCS = RHO_C / 3.6d;  //流量単位がL/hなので、ここで単位を調整

            //対流熱伝達率の補正係数を計算
            double sumHc = 0;
            double sumVl = 0;
            double sumArea = 0;
            foreach (Nodes bp in parts.Keys)
            {
                BodyPart part = parts[bp];
                sumVl += part.Velocity * part.SurfaceArea;
                sumHc += part.ConvectiveHeatTransferCoefficient * part.SurfaceArea;
                sumArea += part.SurfaceArea;
            }
            sumHc /= sumArea;
            sumVl /= sumArea;
            convectiveHeatTransferCoefficientMod = Math.Max(8.600001 * Math.Pow(sumVl, 0.53) / sumHc, 3d / sumHc);

            //行列初期化
            bmMatrix.Initialize(0);
            zmVector.SetValue(0);

            //部位温度に関する行列を用意
            Matrix bmView;
            Vector zmView;
            double coreSum = HeatCapacity_CentralBloodPool * 3600 / timeStep;
            foreach (Nodes bp in parts.Keys)
            {
                BodyPart part = parts[bp];
                uint startPoint = bpDict[bp];
                bool hasAVA = (part.GetHeatCapacity(BodyPart.Segments.SuperficialVein) != 0);

                //部位別の行列要素を設定******************
                uint size = 5;
                if (!part.IsJOSModel) size += 2;
                if (part.GetHeatCapacity(BodyPart.Segments.SuperficialVein) != 0) size += 1;
                bmView = new Matrix(size, size, startPoint, startPoint, bmMatrix);
                zmView = new Vector(size, startPoint, zmVector);
                part.makeMatrix(ref zmView, ref bmView);

                //部位間血流に関する行列要素を設定********
                BodyPart ptf = part.bpConnectFrom;
                //上流に部位が存在する場合
                if (ptf != null) bmMatrix.SetValue(startPoint + 3, bpDict[ptf.Position] + 3, - RCS * part.GetBloodFlow(BodyPart.Segments.Artery));
                //上流が中央血液だまりの場合
                else
                {
                    //動脈血流による熱移動
                    bmMatrix.SetValue(startPoint + 3, bmMatrix.Columns - 1, -RCS * part.GetBloodFlow(BodyPart.Segments.Artery));

                    //中央血液だまりへ向かう静脈血流による熱移動を計算
                    bmMatrix.SetValue(bmMatrix.Columns - 1, startPoint + 4, -RCS * part.GetBloodFlow(BodyPart.Segments.DeepVein));
                    if (hasAVA) bmMatrix.SetValue(bmMatrix.Columns - 1, startPoint + 5, -RCS * part.GetBloodFlow(BodyPart.Segments.SuperficialVein));
                    coreSum += RCS * part.GetBloodFlow(BodyPart.Segments.DeepVein) + RCS * part.GetBloodFlow(BodyPart.Segments.SuperficialVein);
                }

                //静脈血流による熱移動
                List<BodyPart> ptts = part.bpConnectTo;
                foreach (BodyPart ptt in ptts)
                {
                    uint stp2 = bpDict[ptt.Position];

                    //深部静脈血流による熱移動
                    bmMatrix.SetValue(startPoint + 4, stp2 + 4, -RCS * ptt.GetBloodFlow(BodyPart.Segments.DeepVein));
                    
                    //表在静脈血流による熱移動
                    if (part.Position == Nodes.Pelvis)
                    {
                        //腰部の場合は深部静脈に流れ込む
                        bmMatrix.SetValue(startPoint + 4, stp2 + 5, -RCS * ptt.GetBloodFlow(BodyPart.Segments.SuperficialVein));
                    }
                    else
                    {
                        //腰部以外の場合は表在静脈に流れ込む
                        if(ptt.GetHeatCapacity(BodyPart.Segments.SuperficialVein) != 0) bmMatrix.SetValue(startPoint + 5, stp2 + 5, -RCS * ptt.GetBloodFlow(BodyPart.Segments.SuperficialVein));
                    }
                }
            }

            //中央血液だまりに関する行列要素を設定
            bmMatrix.SetValue(bmMatrix.Columns - 1, bmMatrix.Columns - 1, coreSum);
            zmVector.SetValue(zmVector.Size - 1, HeatCapacity_CentralBloodPool * 3600 * CentralBloodPoolTemperature / timeStep);

            //胸部に関して呼吸による項を設定
            double wlSum = 0;
            foreach(Nodes bp in parts.Keys) wlSum += parts[bp].WorkLoad + parts[bp].ShiveringLoad;
            BodyPart head = parts[Nodes.Head];
            HeatLossByBreathing = (0.0014 * (34 - head.DrybulbTemperature)
                + 0.0173 * (5.867 - getVaporPressure(head.DrybulbTemperature, head.RelativeHumidity, 101.325)))
                * (wlSum + BasicMetabolicRate);
            zmVector.AddValue(bpDict[Nodes.Chest], -HeatLossByBreathing);

            //逆行列を計算
            for (uint i = 0; i < perm.Length; i++) perm[i] = i;
            bmMatrix.LUDecomposition();
            bmMatrix.LUSolve(ref zmVector);
            //LinearAlgebra.LUDecomposition(ref bmMatrix, ref perm);
            //LinearAlgebra.LUSolve(bmMatrix, perm, ref zmVector);

            //温度を更新・設定
            foreach (Nodes bp in parts.Keys)
            {
                BodyPart part = parts[bp];

                uint size = 5;
                if (!part.IsJOSModel) size += 2;
                if (part.GetHeatCapacity(BodyPart.Segments.SuperficialVein) != 0) size += 1;
                zmView = new Vector(size, bpDict[bp], zmVector);
                part.setTemperature(zmView);
            }
            CentralBloodPoolTemperature = zmVector.GetValue(zmVector.Size - 1);
        }

        #endregion

        #region 制御系統の計算

        /// <summary>セットポイントを初期化する</summary>
        private void initializeSetPoint()
        {
            //制御をOFF
            Control = ControlMethods.None;

            //PMV=0となる境界条件を設定
            SetDrybulbTemperature(28.8);
            SetRelativeHumidity(50);
            SetMeanRadiantTemperature(28.8);
            SetMaterialTemperature(28.8);
            SetClothingIndex(0);
            SetWorkLoad(58.2);

            //定常状態まで計算（48時間）
            timeStep = 3600;
            for (int i = 0; i < 48; i++) update(0, 0);

            //セットポイント設定
            foreach (Nodes bp in parts.Keys)
            {
                BodyPart part = parts[bp];
                part.SetPoint_Core = part.coreTemperature;
                part.SetPoint_Skin = part.GetTemperature(BodyPart.Segments.Skin);
            }
            Control = ControlMethods.All;

            //平均セットポイントを作成
            //体中心のコア
            double capSum = parts[Nodes.Chest].GetHeatCapacity(BodyPart.Segments.Core) 
                + parts[Nodes.Pelvis].GetHeatCapacity(BodyPart.Segments.Core)
                + parts[Nodes.Back].GetHeatCapacity(BodyPart.Segments.Core);
            averageCoreSetPoint = 0;
            averageCoreSetPoint += parts[Nodes.Chest].SetPoint_Core * parts[Nodes.Chest].GetHeatCapacity(BodyPart.Segments.Core);
            averageCoreSetPoint += parts[Nodes.Pelvis].SetPoint_Core * parts[Nodes.Pelvis].GetHeatCapacity(BodyPart.Segments.Core);
            averageCoreSetPoint += parts[Nodes.Back].SetPoint_Core * parts[Nodes.Back].GetHeatCapacity(BodyPart.Segments.Core);
            averageCoreSetPoint /= capSum;

            //全身の皮膚
            averageSkinSetPoint = 0;
            foreach (Nodes bp in parts.Keys)
            {
                averageSkinSetPoint += parts[bp].SetPoint_Skin * parts[bp].SurfaceArea;
            }
            averageSkinSetPoint /= SurfaceArea;
        }

        /// <summary>制御信号を計算する</summary>
        /// <param name="cldSignal">寒さシグナル</param>
        /// <param name="wrmSignal">暑さシグナル</param>
        private void getSignal(out double cldSignal, out double wrmSignal)
        {
            cldSignal = wrmSignal = 0;
            foreach (Nodes bp in parts.Keys)
            {
                double signal = parts[bp].getPartSignal();
                if (0 < signal) wrmSignal += signal;
                else cldSignal += signal;
            }
        }

        /// <summary>体温調節を更新する</summary>
        /// <param name="cldSignal">寒さシグナル</param>
        /// <param name="wrmSignal">暑さシグナル</param>
        private void updateBodyControl(double cldSignal, double wrmSignal)
        {
            //標準体躯の表面積との比
            double sfRate = SurfaceArea / STANDARD_SURFACE_AREA;

            //制御信号を取得する
            double signal = wrmSignal + cldSignal;

            //頭部コアのエラーシグナル
            BodyPart head = parts[Nodes.Head];
            double err1 = head.coreTemperature - head.SetPoint_Core;

            //各シグナルを計算
            double sweatSignal = Math.Max(0, (371.2 * err1 + 33.64 * signal)) * sfRate;              //発汗シグナル
            double shiveringSignal = (-24.36 * Math.Max(0, -err1) * cldSignal) * sfRate;   //ふるえシグナル
            double coarctationSignal = Math.Max(0, -10.8 * err1 - 10.8 * signal);        //血管収縮シグナル
            double ectasiaSignal = Math.Max(0, 117 * err1 + 7.5 * signal);           //血管拡張シグナル
            ectasiaSignal *= BaseBloodFlowRate / STANDARD_BLOOD_FLOW;

            //皮膚血管運動・発汗・ふるえ熱生産を計算
            foreach (Nodes bp in parts.Keys) parts[bp].setBodySignal(signal, sweatSignal, shiveringSignal, ectasiaSignal, coarctationSignal);

            //AVA血管反応を計算
            calculateAVA();

            //血流更新処理
            parts[Nodes.Neck].updateBloodFlow();
            parts[Nodes.Chest].updateBloodFlow();
            parts[Nodes.Back].updateBloodFlow();
            parts[Nodes.Pelvis].updateBloodFlow();
            parts[Nodes.LeftShoulder].updateBloodFlow();
            parts[Nodes.RightShoulder].updateBloodFlow();
        }

        /// <summary>AVA血管反応を計算する</summary>
        private void calculateAVA()
        {
            //AVA制御OFFの場合
            if ((Control | ControlMethods.AVA) == ControlMethods.None)
            {
                //四肢末端部へ設定
                parts[Nodes.LeftHand].setAVARate(0.0);
                parts[Nodes.RightHand].setAVARate(0.0);
                parts[Nodes.LeftFoot].setAVARate(0.0);
                parts[Nodes.RightFoot].setAVARate(0.0);
                return;
            }

            //体中心（腰・胸・背中）の平均温度を計算
            double capSum = parts[Nodes.Chest].GetHeatCapacity(BodyPart.Segments.Core)
                + parts[Nodes.Pelvis].GetHeatCapacity(BodyPart.Segments.Core)
                + parts[Nodes.Back].GetHeatCapacity(BodyPart.Segments.Core);
            double atCore = 0;
            atCore += parts[Nodes.Chest].coreTemperature * parts[Nodes.Chest].GetHeatCapacity(BodyPart.Segments.Core);
            atCore += parts[Nodes.Pelvis].coreTemperature * parts[Nodes.Pelvis].GetHeatCapacity(BodyPart.Segments.Core);
            atCore += parts[Nodes.Back].coreTemperature * parts[Nodes.Back].GetHeatCapacity(BodyPart.Segments.Core);
            atCore /= capSum;

            //全身の平均皮膚温を計算
            double atSkin = GetAverageSkinTemperature();

            //AVA開度の計算
            double ovaHand = 0.265 * (atSkin - (averageSkinSetPoint - 0.43))
                + 0.953 * (atCore - (averageCoreSetPoint - 0.1905)) + 0.9126;
            double ovaFoot = 0.265 * (atSkin - (averageSkinSetPoint - 0.97))
                + 0.953 * (atCore - (averageCoreSetPoint - 0.0095)) + 0.9126;

            //四肢末端部へ設定
            parts[Nodes.LeftHand].setAVARate(ovaHand);
            parts[Nodes.RightHand].setAVARate(ovaHand);
            parts[Nodes.LeftFoot].setAVARate(ovaFoot);
            parts[Nodes.RightFoot].setAVARate(ovaFoot);
        }

        #endregion

        #region internal staticメソッド

        /// <summary>乾球温度[℃]から飽和水蒸気分圧[kPa]を求める</summary>
        /// <param name="drybulbTemperature">乾球温度[℃]</param>
        /// <returns>飽和水蒸気分圧[kPa]</returns>
        /// <remarks>Wexler-Hylandによる式</remarks>
        internal static double getSaturatedVaporPressure(double drybulbTemperature)
        {
            //近似範囲確認
            if (drybulbTemperature < -100 || 200 < drybulbTemperature)
            {
                //throw new Exception("湿り空気計算範囲外");
            }
            double td = drybulbTemperature + 273.15d;
            //-100~0C//三重点
            if (drybulbTemperature < 0.01)
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

        /// <summary>乾球温度[℃]と相対湿度[%]と大気圧[kPa]から水蒸気分圧[kPa]を求める</summary>
        /// <param name="drybulbTemperature">乾球温度[℃]</param>
        /// <param name="relativeHumidity">相対湿度[%]</param>
        /// <param name="patm">大気圧[kPa]：1気圧は101.325[kPa]</param>
        /// <returns>水蒸気分圧[kPa]</returns>
        internal static double getVaporPressure(double drybulbTemperature, double relativeHumidity, double patm)
        {
            double ps = getSaturatedVaporPressure(drybulbTemperature);
            return 0.01d * relativeHumidity * ps;
        }

        #endregion

    }
}
