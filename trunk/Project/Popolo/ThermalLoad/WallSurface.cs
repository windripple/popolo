/* WallSurface.cs
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

namespace Popolo.ThermalLoad
{
    /// <summary>壁表面クラス</summary>
    public class WallSurface : ISurface
    {

        #region event定義

        /// <summary>FIおよびFO変更イベント</summary>
        public event EventHandler FIOChangeEvent;

        /// <summary>面積変更イベント</summary>
        public event EventHandler AreaChangeEvent;

        /// <summary>総合熱伝達率変更イベント</summary>
        public event EventHandler FilmCoefficientChangeEvent;

        #endregion

        #region インスタンス変数

        /// <summary>壁オブジェクト</summary>
        private Wall wall;

        /// <summary>壁面1か否か</summary>
        private bool isSide1 = true;

        /// <summary>表面熱伝達率[W/m2-K]</summary>
        private double filmCoefficient = 9.3;

        /// <summary>アルベド[-]</summary>
        private double albedo = 0.2;

        /// <summary>総合熱伝達率[W/m2-K]のうち、対流熱伝達の割合[-]</summary>
        private double convectiveRate =0.45;

        /// <summary>総合熱伝達率[W/m2-K]のうち、放射熱伝達の割合[-]</summary>
        private double radiativeRate = 0.55;

        /// <summary>日影率[-]</summary>
        private double shadingRate = 0.0;

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="wall">壁オブジェクト</param>
        /// <param name="isSide1">壁面1か否か</param>
        internal WallSurface(Wall wall, bool isSide1)
        {
            this.wall = wall;
            this.isSide1 = isSide1;

            //短波長・長波長の放射率[-]を初期化
            SolarAbsorptance = 0.9;
            LongWaveEmissivity = 0.6;

            //イベント登録
            wall.FIOChangeEvent += new EventHandler(wall_FIOChangeEvent);
            wall.AreaChangeEvent += new EventHandler(wall_AreaChangeEvent);
        }

        #endregion

        #region プロパティ

        /// <summary>面している空間を設定・取得する</summary>
        public Zone FacingZone
        {
            get;
            set;
        }

        /// <summary>壁オブジェクト（読み取り専用）を取得する</summary>
        public ImmutableWall WallBody
        {
            get
            {
                return wall;
            }
        }

        /// <summary>壁名称を取得する</summary>
        public string Name
        {
            get
            {
                return wall.Name;
            }
        }

        /// <summary>壁面1か否かを取得する</summary>
        public bool IsSide1
        {
            get
            {
                return isSide1;
            }
        }

        /// <summary>傾斜面情報を取得する</summary>
        public ImmutableIncline Incline
        {
            get
            {
                if (isSide1) return wall.Incline1;
                else return wall.Incline2;
            }
        }

        /// <summary>壁の向こう側の空気温度[℃]を取得する</summary>
        public double OtherSideSolAirTemperature
        {
            get
            {
                return wall.GetSolAirTemperature(! isSide1);
            }
        }

        /// <summary>壁表面温度[℃]を取得する</summary>
        public double Temperature
        {
            get
            {
                return wall.GetWallTemprature(isSide1);
            }
        }

        /// <summary>FIを取得する</summary>
        public double FI
        {
            get
            {
                if (isSide1) return wall.FI1;
                else return wall.FI2;
            }
        }

        /// <summary>FOを取得する</summary>
        public double FO
        {
            get
            {
                if (isSide1) return wall.FO1;
                else return wall.FO2;
            }
        }

        /// <summary>CFを取得する</summary>
        public double CF
        {
            get
            {
                if (isSide1) return wall.CF1;
                else return wall.CF2;
            }
        }

        /// <summary>壁近傍の空気温度[C]を設定・取得する</summary>
        public double AirTemperature
        {
            get;
            set;
        }

        /// <summary>壁近傍の放射量[W/m2]を設定・取得する</summary>
        public double Radiation
        {
            get;
            set;
        }

        /// <summary>表面総合熱伝達率[W/(m^2K)]を設定・取得する</summary>
        public double FilmCoefficient
        {
            get
            {
                return filmCoefficient;
            }
            set
            {
                if (filmCoefficient != value)
                {
                    filmCoefficient = value;
                    if (FilmCoefficientChangeEvent != null) FilmCoefficientChangeEvent(this, new EventArgs());
                }
            }
        }

        /// <summary>日射吸収率[-]を設定・取得する</summary>
        public double SolarAbsorptance
        {
            get;
            set;
        }

        /// <summary>長波長の放射率[-]を設定・取得する</summary>
        public double LongWaveEmissivity
        {
            get;
            set;
        }

        /// <summary>壁面積[m2]を取得する</summary>
        public double Area
        {
            get
            {
                return wall.SurfaceArea;
            }
        }

        /// <summary>計算時間間隔[sec]を取得する</summary>
        public double TimeStep
        {
            get
            {
                return wall.TimeStep;
            }
        }

        /// <summary>アルベド[-]を設定・取得する</summary>
        public double Albedo
        {
            get
            {
                return albedo;
            }
            set
            {
                albedo = Math.Min(Math.Max(value, 0), 1);
            }
        }

        /// <summary>総合熱伝達率[W/m2-K]のうち、対流熱伝達の割合[-]を設定・取得する</summary>
        public double ConvectiveRate
        {
            get
            {
                return convectiveRate;
            }
            set
            {
                convectiveRate = Math.Max(Math.Min(value, 1), 0);
                radiativeRate = 1.0d - convectiveRate;
            }
        }

        /// <summary>総合熱伝達率[W/m2-K]のうち、放射熱伝達の割合[-]を設定・取得する</summary>
        public double RadiativeRate
        {
            get
            {
                return radiativeRate;
            }
            set
            {
                radiativeRate = Math.Max(Math.Min(value, 1), 0);
                convectiveRate = 1.0d - radiativeRate;
            }
        }

        /// <summary>逆側の表面を取得する</summary>
        public ISurface OtherSideSurface
        {
            get
            {
                return wall.GetSurface(!isSide1);
            }
        }

        /// <summary>FPTを取得する</summary>
        public double FPT
        {
            get
            {
                if (isSide1) return wall.FPT1;
                else return wall.FPT2;
            }
        }

        /// <summary>日影率[-]を設定・取得する</summary>
        public double ShadingRate
        {
            get
            {
                return shadingRate;
            }
            set
            {
                shadingRate = Math.Max(Math.Min(1, value), 0);
            }
        }

        #endregion

        #region publicメソッド

        /// <summary>周辺の空気から壁への熱移動量[W]を計算する</summary>
        /// <returns>周辺の空気から壁への熱移動量[W]</returns>
        public double GetHeatTransfer()
        {
            return (GetSolAirTemperature() - Temperature) * FilmCoefficient * Area;
        }

        /// <summary>放射を考慮した相当温度[C]を計算する</summary>
        /// <returns>相当温度[C]</returns>
        public double GetSolAirTemperature()
        {
            return AirTemperature + Radiation / FilmCoefficient;
        }

        /// <summary>表面素材</summary>
        public enum SurfaceMaterial
        {
            /// <summary>完全黒体</summary>
            BlackBody,
            /// <summary>アスファルト</summary>
            Asphalt,
            /// <summary>黒色塗料</summary>
            BlackPaint,
            /// <summary>赤れんが</summary>
            RedBrick,
            /// <summary>暗色タイル</summary>
            DarkTile,
            /// <summary>コンクリート</summary>
            Concrete,
            /// <summary>暗色塗料</summary>
            DarkPaint,
            /// <summary>石</summary>
            Stone,
            /// <summary>クリーム色のれんが・タイル</summary>
            CreamColorBrick,
            /// <summary>クリーム色塗料</summary>
            CreamColorPaint,
            /// <summary>漆喰</summary>
            Plaster,
            /// <summary>光沢アルミニウムペイント</summary>
            AluminumPaint,
            /// <summary>ブロンズペイント</summary>
            BronzePaint,
            /// <summary>黄銅</summary>
            Brass,
            /// <summary>アルミニウム</summary>
            Aluminum,
            /// <summary>トタン・亜鉛</summary>
            Zinc,
            /// <summary>磨いた黄銅</summary>
            PolishedBrass,
            /// <summary>磨いたアルミニウム</summary>
            PolishedAluminum,
            /// <summary>ブリキ</summary>
            TinPlate
        }

        /// <summary>放射率[-]を初期化する</summary>
        /// <param name="sMaterial">表面素材</param>
        /// <remarks>空調・衛生技術データブック（株式会社テクノ菱和）のデータの平均値を利用</remarks>
        public void InitializeEmissivity(SurfaceMaterial sMaterial)
        {
            switch (sMaterial)
            {
                case SurfaceMaterial.BlackBody:
                    LongWaveEmissivity = 1.0;
                    SolarAbsorptance = 1.0;
                    break;
                case SurfaceMaterial.Asphalt:
                case SurfaceMaterial.BlackPaint:
                    LongWaveEmissivity = 0.94;
                    SolarAbsorptance = 0.915;
                    break;
                case SurfaceMaterial.RedBrick:
                case SurfaceMaterial.DarkTile:
                case SurfaceMaterial.DarkPaint:
                case SurfaceMaterial.Concrete:
                case SurfaceMaterial.Stone:
                    LongWaveEmissivity = 0.9;
                    SolarAbsorptance = 0.725;
                    break;
                case SurfaceMaterial.CreamColorBrick:
                case SurfaceMaterial.CreamColorPaint:
                case SurfaceMaterial.Plaster:
                    LongWaveEmissivity = 0.9;
                    SolarAbsorptance = 0.4;
                    break;
                case SurfaceMaterial.AluminumPaint:
                case SurfaceMaterial.BronzePaint:
                    LongWaveEmissivity = 0.5;
                    SolarAbsorptance = 0.4;
                    break;
                case SurfaceMaterial.Brass:
                case SurfaceMaterial.Aluminum:
                case SurfaceMaterial.Zinc:
                    LongWaveEmissivity = 0.25;
                    SolarAbsorptance = 0.525;
                    break;
                case SurfaceMaterial.PolishedBrass:
                    LongWaveEmissivity = 0.035;
                    SolarAbsorptance = 0.4;
                    break;
                case SurfaceMaterial.PolishedAluminum:
                case SurfaceMaterial.TinPlate:
                    LongWaveEmissivity = 0.03;
                    SolarAbsorptance = 0.25;
                    break;
            }
        }

        #endregion

        #region privateメソッド

        /// <summary>壁のFIおよびFO変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wall_FIOChangeEvent(object sender, EventArgs e)
        {
            if (this.FIOChangeEvent != null) FIOChangeEvent(sender, e);
        }

        /// <summary>壁面積変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wall_AreaChangeEvent(object sender, EventArgs e)
        {
            if (this.AreaChangeEvent != null) AreaChangeEvent(sender, e);
        }

        #endregion

    }

}
