/* WindowSurface.cs
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

using Popolo.Weather;

namespace Popolo.ThermalLoad
{
    /// <summary>窓表面クラス</summary>
    public class WindowSurface : ISurface
    {

        #region event定義

        /// <summary>FIおよびFO変更イベント</summary>
        public event EventHandler FIOChangeEvent;

        /// <summary>面積変更イベント</summary>
        public event EventHandler AreaChangeEvent;

        #endregion

        #region インスタンス変数

        /// <summary>壁オブジェクト</summary>
        private Window window;

        /// <summary>外部か否か</summary>
        private bool isOutside = true;

        /// <summary>アルベド[-]</summary>
        private double albedo = 0.2;

        /// <summary>総合熱伝達率[W/m2-K]のうち、対流熱伝達の割合[-]</summary>
        private double convectiveRate = 0.45;

        /// <summary>総合熱伝達率[W/m2-K]のうち、放射熱伝達の割合[-]</summary>
        private double radiativeRate = 0.55;

        #endregion

        #region プロパティ

        /// <summary>面している空間を設定・取得する</summary>
        public Zone FacingZone
        {
            get;
            set;
        }

        /// <summary>窓オブジェクト（読み取り専用）を取得する</summary>
        public ImmutableWindow WindowBody
        {
            get
            {
                return window;
            }
        }

        /// <summary>窓名称を取得する</summary>
        public string Name
        {
            get
            {
                return window.Name;
            }
        }

        /// <summary>外表面か否かを取得する</summary>
        public bool IsOutside
        {
            get
            {
                return isOutside;
            }
        }

        /// <summary>傾斜面情報を取得する</summary>
        public ImmutableIncline Incline
        {
            get
            {
                if (isOutside) return window.OutSideIncline;
                else return null;
            }
        }

        /// <summary>表面温度[℃]を取得する</summary>
        public double Temperature
        {
            get
            {
                if (isOutside) return window.OutSideSurfaceTemperature;
                else return window.InSideSurfaceTemperature;
            }
        }

        /// <summary>FIを取得する</summary>
        public double FI
        {
            get
            {
                if (isOutside) return 0;
                else return window.FI;
            }
        }

        /// <summary>FOを取得する</summary>
        public double FO
        {
            get
            {
                if (isOutside) return 0;
                else return window.FO;
            }
        }

        /// <summary>CFを取得する</summary>
        public double CF
        {
            get
            {
                return 0;
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
                if (isOutside) return window.Glass.OutsideFilmCoefficient;
                else return window.Glass.InsideFilmCoefficient;
            }
            set
            {
                if (isOutside) window.setOutsideOverallHeatTransferCoefficient(value);
                else window.setInsideOverallHeatTransferCoefficient(value);
            }
        }

        /// <summary>短波長の放射率[-]を設定・取得する</summary>
        public double ShortWaveEmissivity
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

        /// <summary>面積[m2]を取得する</summary>
        public double Area
        {
            get
            {
                return window.SurfaceArea;
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
                return window.GetSurface(! isOutside);
            }
        }

        /// <summary>FPTを取得する</summary>
        public double FPT
        {
            get
            {
                return 0;
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="window">壁オブジェクト</param>
        /// <param name="isOutside">外部か否か</param>
        internal WindowSurface(Window window, bool isOutside)
        {
            this.window = window;
            this.isOutside = isOutside;

            window.FIOChangeEvent += new EventHandler(window_FIOChangeEvent);
            window.AreaChangeEvent += new EventHandler(window_AreaChangeEvent);
        }

        /// <summary>ガラスのFIおよびFO変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void window_FIOChangeEvent(object sender, EventArgs e)
        {
            if (FIOChangeEvent != null) FIOChangeEvent(sender, e);
        }

        /// <summary>窓面積変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void window_AreaChangeEvent(object sender, EventArgs e)
        {
            if (this.AreaChangeEvent != null) AreaChangeEvent(sender, e);
        }

        #endregion

        #region publicメソッド

        /// <summary>周辺の空気から窓への熱移動量[W]を計算する</summary>
        /// <returns>周辺の空気から窓への熱移動量[W]</returns>
        public double GetHeatTransfer()
        {
            return (GetSolAirTemperature() - Temperature) * FilmCoefficient * Area;
        }

        /// <summary>放射[W/m2]を考慮した相当温度[C]を計算する</summary>
        /// <returns>相当温度[C]</returns>
        public double GetSolAirTemperature()
        {
            return AirTemperature + Radiation / FilmCoefficient;
        }

        #endregion        

    }
}
