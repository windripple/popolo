/* ISurface.cs
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
    /// <summary>表面インターフェース</summary>
    public interface ISurface : ImmutableSurface
    {

        #region プロパティ

        /// <summary>面している空間を設定・取得する</summary>
        new Zone FacingZone
        {
            set;
            get;
        }

        /// <summary>壁近傍の空気温度[C]を設定・取得する</summary>
        new double AirTemperature
        {
            set;
            get;
        }

        /// <summary>壁近傍の放射量[W/m2]を設定・取得する</summary>
        new double Radiation
        {
            set;
            get;
        }

        /// <summary>総合熱伝達率[W/m2-K]のうち、対流熱伝達の割合[-]を設定・取得する</summary>
        new double ConvectiveRate
        {
            set;
            get;
        }

        #endregion

    }

    /// <summary>読み取り専用の表面</summary>
    public interface ImmutableSurface
    {

        #region event定義

        /// <summary>FIおよびFO変更イベント</summary>
        event EventHandler FIOChangeEvent;

        /// <summary>面積変更イベント</summary>
        event EventHandler AreaChangeEvent;

        #endregion

        #region プロパティ

        /// <summary>名称を取得する</summary>
        string Name
        {
            get;
        }

        /// <summary>面している空間を取得する</summary>
        Zone FacingZone
        {
            get;
        }

        /// <summary>逆側の表面を取得する</summary>
        ISurface OtherSideSurface
        {
            get;
        }

        /// <summary>FIを取得する</summary>
        double FI
        {
            get;
        }

        /// <summary>FOを取得する</summary>
        double FO
        {
            get;
        }

        /// <summary>CFを取得する</summary>
        double CF
        {
            get;
        }

        /// <summary>壁近傍の空気温度[C]を取得する</summary>
        double AirTemperature
        {
            get;
        }

        /// <summary>壁近傍の放射量[W/m2]を取得する</summary>
        double Radiation
        {
            get;
        }

        /// <summary>表面熱伝達率[W/m2-K]を取得する</summary>
        double FilmCoefficient
        {
            get;
        }

        /// <summary>総合熱伝達率[W/m2-K]のうち、対流熱伝達の割合[-]を取得する</summary>
        double ConvectiveRate
        {
            get;
        }

        /// <summary>総合熱伝達率[W/m2-K]のうち、放射熱伝達の割合[-]を取得する</summary>
        double RadiativeRate
        {
            get;
        }

        /// <summary>面積[m2]を取得する</summary>
        double Area
        {
            get;
        }

        /// <summary>FPTを取得する</summary>
        double FPT
        {
            get;
        }

        /// <summary>表面温度を取得する</summary>
        double Temperature
        {
            get;
        }

        /// <summary>傾斜面情報を取得する</summary>
        ImmutableIncline Incline
        {
            get;
        }

        /// <summary>アルベド[-]を取得する</summary>
        double Albedo
        {
            get;
        }

        #endregion

        #region publicメソッド

        /// <summary>放射を考慮した相当温度[C]を計算する</summary>
        /// <returns>相当温度[C]</returns>
        double GetSolAirTemperature();

        /// <summary>周辺の空気から表面への熱移動量[W]を計算する</summary>
        /// <returns>周辺の空気から表面への熱移動量[W]</returns>
        double GetHeatTransfer();

        #endregion

    }

}
