/* GlassPanes.cs
 * 
 * Copyright (C) 2009 E.Togashi
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

namespace Popolo.ThermalLoad
{
    /// <summary>ガラス層</summary>
    public class GlassPanes : ImmutableGlassPanes
    {

        #region 列挙型定義

        /// <summary>ガラス間に充填する気体</summary>
        public enum GapMaterial
        {
            /// <summary>真空//直せE.Togashi</summary>
            SHINKU,
            /// <summary>空気</summary>
            Air,
            /// <summary>アルゴン</summary>
            Algon,
            /// <summary>クリプトン//直せE.Togashi</summary>
            Curipton
        }

        #endregion

        #region インスタンス変数

        /// <summary>入射角特性係数[-]</summary>
        private List<double> angularDependenceCoefficients = new List<double>();

        /// <summary>空隙の総合熱伝達率[W/(m2-K)]</summary>
        private List<double> heatTransferCoefficientsOfAirGaps;

        /// <summary>外表面総合熱伝達率[W/m2-K]</summary>
        private double outsideOverallHeatTransferCoefficient = 1d / 0.043;

        /// <summary>内表面総合熱伝達率[W/m2-K]</summary>
        private double insideOverallHeatTransferCoefficient = 1d / 0.108;

        #endregion

        #region プロパティ

        /// <summary>総合透過率[-]を取得する</summary>
        public double OverallTransmissivity
        {
            get;
            private set;
        }

        /// <summary>総合吸収率[-]を取得する</summary>
        public double OverallAbsorptivity
        {
            get;
            private set;
        }

        /// <summary>ガラスの熱貫流率[W/(m2-K)]を取得する</summary>
        public double ThermalTransmittanceOfGlass
        {
            get;
            private set;
        }

        /// <summary>熱貫流率[W/(m2-K)]を取得する</summary>
        public double ThermalTransmittance
        {
            get;
            private set;
        }

        /// <summary>ガラスを取得する</summary>
        public Pane[] Panes
        {
            get;
            private set;
        }

        /// <summary>外表面総合熱伝達率[W/m2-K]を取得する</summary>
        public double OutsideFilmCoefficient
        {
            get
            {
                return outsideOverallHeatTransferCoefficient;
            }
        }

        /// <summary>内表面総合熱伝達率[W/m2-K]を取得する</summary>
        public double InsideFilmCoefficient
        {
            get
            {
                return insideOverallHeatTransferCoefficient;
            }
        }

        /// <summary>空隙の総合熱伝達率[W/(m2-K)]を取得する</summary>
        public double[] HeatTransferCoefficientsOfGaps
        {
            get {
                return heatTransferCoefficientsOfAirGaps.ToArray();
            }
        }

        /// <summary>入射角特性係数[-]の係数を設定・取得する</summary>
        /// <remarks>Σ(an * cosθ^n)</remarks>
        public double[] AngularDependenceCoefficients
        {
            get
            {
                return angularDependenceCoefficients.ToArray();
            }
            set
            {
                angularDependenceCoefficients.Clear();
                angularDependenceCoefficients.AddRange(value);
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="overallTransmittance">総合透過率[-]</param>
        /// <param name="overallAbsorptance">総合吸収率[-]</param>
        /// <param name="heatTransferCoefficient">ガラスの熱貫流率[W/m2-K]</param>
        public GlassPanes(double overallTransmittance, double overallAbsorptance, double heatTransferCoefficient)
        {
            //入射角特性係数[-]を初期化
            angularDependenceCoefficients.AddRange(new double[] { 3.4167, -4.389, 2.4948, -0.5224 });

            this.OverallTransmissivity = overallTransmittance;
            this.OverallAbsorptivity = overallAbsorptance;
            this.ThermalTransmittanceOfGlass = heatTransferCoefficient;

            initialize();
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="panes">
        /// ガラス板リスト
        /// 内側から外側の順
        /// </param>
        public GlassPanes(Pane[] panes)
        {
            //入射角特性係数[-]を初期化
            angularDependenceCoefficients.AddRange(new double[] { 3.4167, -4.389, 2.4948, -0.5224 });

            //ガラスを設定
            Panes = panes;

            //空隙の総合熱伝達率リストを用意して初期化
            heatTransferCoefficientsOfAirGaps = new List<double>();
            for (int i = 0; i < panes.Length - 1; i++) heatTransferCoefficientsOfAirGaps.Add(1d / 0.12);

            //特性を初期化
            initialize();
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="pane">ガラス板</param>
        public GlassPanes(Pane pane)
        {
            //入射角特性係数[-]を初期化
            angularDependenceCoefficients.AddRange(new double[] { 3.4167, -4.389, 2.4948, -0.5224 });

            //ガラスを設定
            Panes = new Pane[] { pane };

            //空隙の総合熱伝達率リストを用意して初期化
            heatTransferCoefficientsOfAirGaps = new List<double>();

            //特性を初期化
            initialize();
        }

        /// <summary>初期化する</summary>
        private void initialize()
        {
            //層が詳細に定義されている場合
            if (Panes != null)
            {
                double[] absorptance = new double[Panes.Length];

                //熱貫流率[W/(m2K)]を計算
                ThermalTransmittanceOfGlass = 0;
                for (int i = 0; i < heatTransferCoefficientsOfAirGaps.Count; i++) ThermalTransmittanceOfGlass += 1d / heatTransferCoefficientsOfAirGaps[i];
                for (int i = 0; i < Panes.Length; i++) ThermalTransmittanceOfGlass += 1d / Panes[i].HeatTransferCoefficient;
                ThermalTransmittance = ThermalTransmittanceOfGlass;
                ThermalTransmittanceOfGlass = 1d / ThermalTransmittanceOfGlass;
                ThermalTransmittance += 1 / outsideOverallHeatTransferCoefficient + 1 / insideOverallHeatTransferCoefficient;
                ThermalTransmittance = 1d / ThermalTransmittance;

                //総合透過率[-]を計算
                OverallTransmissivity = Panes[0].OuterSideTransmissivity;
                double overallReflectance = Panes[0].OuterSideReflectivity;
                absorptance[0] = Panes[0].OuterSideAbsorptivity;
                for (int i = 1; i < Panes.Length; i++)
                {
                    double xr = Panes[i].OuterSideTransmissivity / (1d - Panes[i].InnerSideReflectivity * overallReflectance);
                    for (int j = 0; j < i; j++) absorptance[j] *= xr;
                    absorptance[i] = Panes[i].OuterSideAbsorptivity + Panes[i].InnerSideAbsorptivity * overallReflectance * xr;
                    overallReflectance = Panes[i].OuterSideReflectivity + Panes[i].InnerSideTransmissivity* overallReflectance * xr;
                    OverallTransmissivity *= xr;
                }

                //総合吸収率[-]を計算
                OverallAbsorptivity = 0;
                double rSum = 1d / insideOverallHeatTransferCoefficient + 1d / Panes[0].HeatTransferCoefficient;
                for (int i = 0; i < Panes.Length; i++)
                {
                    OverallAbsorptivity += (1d - ThermalTransmittance * rSum) * absorptance[i];
                    if (i != Panes.Length - 1) rSum += 1d / heatTransferCoefficientsOfAirGaps[i] + 1d / Panes[i].HeatTransferCoefficient;
                }
            }
            //簡易の場合
            else
            {
                ThermalTransmittance = 1d / ThermalTransmittanceOfGlass + 
                    1d / outsideOverallHeatTransferCoefficient + 1d / insideOverallHeatTransferCoefficient;
                ThermalTransmittance = 1d / ThermalTransmittance;
            }
        }

        #endregion

        #region publicメソッド

        /// <summary>外表面総合熱伝達率[W/m2-K]を設定する</summary>
        /// <param name="outsideOverallHeatTransferCoefficient">外表面総合熱伝達率[W/m2-K]</param>
        public void SetOutsideFilmCoefficient(double outsideOverallHeatTransferCoefficient)
        {
            if (outsideOverallHeatTransferCoefficient <= 0) return;
            if (this.outsideOverallHeatTransferCoefficient == outsideOverallHeatTransferCoefficient) return;

            this.outsideOverallHeatTransferCoefficient = outsideOverallHeatTransferCoefficient;

            //特性を初期化
            initialize();
        }

        /// <summary>内表面総合熱伝達率[W/m2-K]を設定する</summary>
        /// <param name="insideOverallHeatTransferCoefficient">内表面総合熱伝達率[W/m2-K]</param>
        public void SetInsideFilmCoefficient(double insideOverallHeatTransferCoefficient)
        {
            if (insideOverallHeatTransferCoefficient <= 0) return;
            if (this.insideOverallHeatTransferCoefficient == insideOverallHeatTransferCoefficient) return;

            this.insideOverallHeatTransferCoefficient = insideOverallHeatTransferCoefficient;

            //特性を初期化
            initialize();
        }

        /// <summary>ガラス間の気体の総合熱伝達率[W/(m2-K)]を設定する</summary>
        /// <param name="index">空気層の番号：室内側から0,1,2,3...</param>
        /// <param name="heatTransferCoefficient">空気層の総合熱伝達率[W/(m2-K)]</param>
        public void SetHeatTransferCoefficientsOfGaps(int index, double heatTransferCoefficient)
        {
            //簡易ガラス層の場合は終了
            if (heatTransferCoefficientsOfAirGaps == null) return;

            if (0 < heatTransferCoefficient)
            {
                if (heatTransferCoefficientsOfAirGaps[index] != heatTransferCoefficient)
                {
                    heatTransferCoefficientsOfAirGaps[index] = heatTransferCoefficient;
                    initialize();
                }
            }
        }

        /// <summary>ガラス間の気体の総合熱伝達率[W/(m2-K)]を設定する</summary>
        /// <param name="index">空気層の番号：室内側から0,1,2,3...</param>
        /// <param name="gMaterial">ガラス間充填気体</param>
        public void SetHeatTransferCoefficientsOfGaps(int index, GapMaterial gMaterial)
        {
            switch (gMaterial)
            {
                case GapMaterial.Air:
                    SetHeatTransferCoefficientsOfGaps(index, 99999);//修正
                    break;
                case GapMaterial.Algon:
                    SetHeatTransferCoefficientsOfGaps(index, 99999);//修正
                    break;
                case GapMaterial.Curipton:
                    SetHeatTransferCoefficientsOfGaps(index, 99999);//修正
                    break;
                case GapMaterial.SHINKU:
                    SetHeatTransferCoefficientsOfGaps(index, 99999);//修正
                    break;
            }
            throw new Exception("未実装");
        }


        /// <summary>ガラス層オブジェクトをコピーする</summary>
        /// <param name="glassPanes">ガラス層オブジェクト</param>
        public void Copy(ImmutableGlassPanes glassPanes)
        {
            this.OverallAbsorptivity = glassPanes.OverallAbsorptivity;
            this.OverallTransmissivity = glassPanes.OverallTransmissivity;
            this.ThermalTransmittanceOfGlass = glassPanes.ThermalTransmittanceOfGlass;

            if (Panes != null)
            {
                this.Panes = new Pane[glassPanes.Panes.Length];
                for (int i = 0; i < this.Panes.Length; i++) this.Panes[i] = new Pane(glassPanes.Panes[i]);
                this.heatTransferCoefficientsOfAirGaps.Clear();
                double[] aGap = glassPanes.HeatTransferCoefficientsOfGaps;
                for (int i = 0; i < aGap.Length; i++)
                {
                    this.heatTransferCoefficientsOfAirGaps.Add(aGap[i]);
                }
            }
            this.angularDependenceCoefficients.Clear();
            double[] ac = glassPanes.AngularDependenceCoefficients;
            for (int i = 0; i < ac.Length; i++)
            {
                angularDependenceCoefficients.Add(ac[i]);
            }
            SetOutsideFilmCoefficient(glassPanes.OutsideFilmCoefficient);
            SetInsideFilmCoefficient(glassPanes.InsideFilmCoefficient);

            initialize();
        }

        /// <summary>ガラスの入射角特性[-]を計算する</summary>
        /// <param name="cosineIncidentAngle">入射角の余弦（cosθ）</param>
        /// <returns>ガラスの入射角特性[-]</returns>
        public double GetIncidentAngleCharacteristic(double cosineIncidentAngle)
        {
            double ci = cosineIncidentAngle;
            double val = 0;
            for (int i = angularDependenceCoefficients.Count - 1; 0 <= i; i--)
            {
                val = ci * (val + angularDependenceCoefficients[i]);
            }
            return Math.Max(0, Math.Min(1, val));
        }

        #endregion

        #region インナークラス定義

        /// <summary>ガラス板</summary>
        public class Pane
        {

            #region 列挙型定義

            /// <summary>ガラス板の種類</summary>
            public enum PredifinedGlassPane
            {
                /// <summary>透明ガラス(3mm)</summary>
                TransparentGlass03mm,
                /// <summary>透明ガラス(6mm)</summary>
                TransparentGlass06mm,
                /// <summary>透明ガラス(12mm)</summary>
                TransparentGlass12mm,
                /// <summary>吸熱ガラス(3mm)</summary>
                HeatAbsorbingGlass03mm,
                /// <summary>吸熱ガラス(6mm)</summary>
                HeatAbsorbingGlass06mm,
                /// <summary>反射ガラス(6mm)</summary>
                HeatReflectingGlass06mm,
            }

            #endregion

            #region プロパティ

            /// <summary>Gets heat transfer coefficient [W/(m2K)].</summary>
            public double HeatTransferCoefficient
            {
                get;
                private set;
            }

            /// <summary>外側透過率[-]を取得する</summary>
            public double OuterSideTransmissivity
            {
                get;
                private set;
            }

            /// <summary>外側吸収率[-]を取得する</summary>
            public double OuterSideAbsorptivity
            {
                get;
                private set;
            }

            /// <summary>外側反射率[-]を取得する</summary>
            public double OuterSideReflectivity
            {
                get;
                private set;
            }

            /// <summary>内側透過率[-]を取得する</summary>
            public double InnerSideTransmissivity
            {
                get;
                private set;
            }

            /// <summary>内側吸収率[-]を取得する</summary>
            public double InnerSideAbsorptivity
            {
                get;
                private set;
            }

            /// <summary>内側反射率[-]を取得する</summary>
            public double InnerSideReflectivity
            {
                get;
                private set;
            }

            #endregion

            #region コンストラクタ

            /// <summary>コンストラクタ</summary>
            /// <param name="transmittance">透過率[-]</param>
            /// <param name="reflectance">反射率[-]</param>
            /// <param name="heatTransferCoefficient">Heat transfer coefficient [W/(m2K)]</param>
            public Pane(double transmittance, double reflectance, double heatTransferCoefficient)
            {
                if (transmittance < 0 || reflectance < 0 || 1 < (transmittance + reflectance)) throw new Exception("ガラス物性エラー");

                OuterSideTransmissivity = InnerSideTransmissivity = transmittance;
                OuterSideReflectivity = InnerSideReflectivity = reflectance;
                OuterSideAbsorptivity = InnerSideAbsorptivity = 1 - (OuterSideTransmissivity + OuterSideReflectivity);
                HeatTransferCoefficient = heatTransferCoefficient;
            }

            /// <summary>コンストラクタ</summary>
            /// <param name="outerSideTransmittance">外側透過率[-]</param>
            /// <param name="outerSideReflectance">外側反射率[-]</param>
            /// <param name="innerSideTransmittance">内側透過率[-]</param>
            /// <param name="innerSideReflectance">内側反射率[-]</param>
            /// <param name="heatTransferCoefficient">Heat transfer coefficient [W/(m2K)]</param>
            public Pane(double outerSideTransmittance, double outerSideReflectance,
                double innerSideTransmittance, double innerSideReflectance, double heatTransferCoefficient)
            {
                if (outerSideTransmittance < 0 || outerSideReflectance < 0 || 1 < (outerSideTransmittance + outerSideReflectance) ||
                    innerSideTransmittance < 0 || innerSideReflectance < 0 || 1 < (innerSideTransmittance + innerSideReflectance)) throw new Exception("ガラス物性エラー");

                OuterSideTransmissivity = outerSideTransmittance;
                InnerSideTransmissivity = innerSideTransmittance;
                OuterSideReflectivity = outerSideReflectance;
                InnerSideReflectivity = innerSideReflectance;
                OuterSideAbsorptivity = 1 - (OuterSideTransmissivity + OuterSideReflectivity);
                InnerSideAbsorptivity = 1 - (InnerSideTransmissivity + InnerSideReflectivity);
                HeatTransferCoefficient = heatTransferCoefficient;
            }

            /// <summary>コンストラクタ</summary>
            /// <param name="predifinedGlass">ガラス板の種類</param>
            public Pane(PredifinedGlassPane predifinedGlass)
            {
                const double THCG = 0.79d;  //Thermal conductivity [W/(mK)] of single glass

                switch (predifinedGlass)
                {
                    case PredifinedGlassPane.TransparentGlass03mm:
                        OuterSideTransmissivity = InnerSideTransmissivity = 0.85;
                        OuterSideReflectivity = InnerSideReflectivity = 0.07;
                        HeatTransferCoefficient = THCG / 0.003;
                        break;
                    case PredifinedGlassPane.TransparentGlass06mm:
                        OuterSideTransmissivity = InnerSideTransmissivity = 0.79;
                        OuterSideReflectivity = InnerSideReflectivity = 0.07;
                        HeatTransferCoefficient = THCG / 0.006;
                        break;
                    case PredifinedGlassPane.TransparentGlass12mm:
                        OuterSideTransmissivity = InnerSideTransmissivity = 0.69;
                        OuterSideReflectivity = InnerSideReflectivity = 0.07;
                        HeatTransferCoefficient = THCG / 0.012;
                        break;
                    case PredifinedGlassPane.HeatAbsorbingGlass03mm:
                        OuterSideTransmissivity = InnerSideTransmissivity = 0.74;
                        OuterSideReflectivity = InnerSideReflectivity = 0.07;
                        HeatTransferCoefficient = THCG / 0.003;
                        break;
                    case PredifinedGlassPane.HeatAbsorbingGlass06mm:
                        OuterSideTransmissivity = InnerSideTransmissivity = 0.60;
                        OuterSideReflectivity = InnerSideReflectivity = 0.06;
                        HeatTransferCoefficient = THCG / 0.006;
                        break;
                    case PredifinedGlassPane.HeatReflectingGlass06mm:
                        OuterSideTransmissivity = InnerSideTransmissivity = 0.60;
                        OuterSideReflectivity = InnerSideReflectivity = 0.30;
                        HeatTransferCoefficient = THCG / 0.012;
                        break;
                }
                OuterSideAbsorptivity = InnerSideAbsorptivity = 1 - (OuterSideTransmissivity + OuterSideReflectivity);
            }

            /// <summary>コンストラクタ</summary>
            /// <param name="pane">コピー対象のガラスオブジェクト</param>
            public Pane(Pane pane)
            {
                this.OuterSideAbsorptivity = pane.OuterSideAbsorptivity;
                this.OuterSideReflectivity = pane.OuterSideReflectivity;
                this.OuterSideTransmissivity = pane.OuterSideTransmissivity;
                this.InnerSideAbsorptivity = pane.InnerSideAbsorptivity;
                this.InnerSideReflectivity = pane.InnerSideReflectivity;
                this.InnerSideTransmissivity = pane.InnerSideTransmissivity;
                this.HeatTransferCoefficient = pane.HeatTransferCoefficient;
            }

            #endregion

        }

        #endregion

    }

    /// <summary>読み取り専用のガラス層</summary>
    public interface ImmutableGlassPanes
    {

        #region プロパティ

        /// <summary>総合透過率[-]を取得する</summary>
        double OverallTransmissivity
        {
            get;
        }

        /// <summary>総合吸収率[-]を取得する</summary>
        double OverallAbsorptivity
        {
            get;
        }

        /// <summary>ガラスの熱貫流率[W/m2-K]を取得する</summary>
        double ThermalTransmittanceOfGlass
        {
            get;
        }

        /// <summary>熱貫流率[W/(m2-K)]を取得する</summary>
        double ThermalTransmittance
        {
            get;
        }

        /*/// <summary>熱取得の内、対流成分の割合[-]を取得する</summary>
        double ConvectiveRate
        {
            get;
        }*/

        /*/// <summary>熱取得の内、放射成分の割合[-]を取得する</summary>
        double RadiativeRate
        {
            get;
        }*/

        /*/// <summary>長波長の放射率[-]を取得する</summary>
        double LongWaveEmissivity
        {
            get;
        }*/

        /// <summary>ガラスを取得する</summary>
        GlassPanes.Pane[] Panes
        {
            get;
        }

        /// <summary>外表面総合熱伝達率[W/m2-K]を取得する</summary>
        double OutsideFilmCoefficient
        {
            get;
        }

        /// <summary>内表面総合熱伝達率[W/m2-K]を取得する</summary>
        double InsideFilmCoefficient
        {
            get;
        }

        /// <summary>空隙の総合熱伝達率[W/(m2-K)]を取得する</summary>
        double[] HeatTransferCoefficientsOfGaps
        {
            get;
        }

        /// <summary>入射角特性係数[-]の係数を取得する</summary>
        /// <remarks>Σ(an * cosθ^n)</remarks>
        double[] AngularDependenceCoefficients
        {
            get;
        }

        #endregion

    }

}
