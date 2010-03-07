/* Sky.cs
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

namespace Popolo.Weather
{
    /// <summary>天空に関する計算処理</summary>
    /// <remarks>
    /// 数値計算で学ぶ光と熱の建築環境学, pp.20, 丸善, 宿谷昌則, 1993
    /// パソコンによる空気調和計算法, 宇田川光弘, 1986
    /// </remarks>
    public static class Sky
    {

        #region 定数

        /// <summary>DegreeをRadianに変換する係数</summary>
        const double DEG_TO_RAD = 2d * Math.PI / 360d;

        /// <summary>黒体の放射定数[W/m2-K4]</summary>
        const double BLACK_RADIATION = 5.67e-8;

        #endregion

        #region 角度変換に関する処理

        /// <summary>度をラジアンに変換する</summary>
        /// <param name="degree">度</param>
        /// <returns>ラジアン</returns>
        public static double DegreeToRadian(double degree)
        {
            return degree * DEG_TO_RAD;
        }

        /// <summary>ラジアンを度に変換する</summary>
        /// <param name="radian">ラジアン</param>
        /// <returns>度</returns>
        public static double RadianToDegree(double radian)
        {
            return radian / DEG_TO_RAD;
        }

        #endregion

        #region 放射関連の処理

        /// <summary>大気放射量[W/m2]を計算する</summary>
        /// <param name="temperature">外気乾球温度[C]</param>
        /// <param name="cloudCover">雲量[-]</param>
        /// <param name="waterVaporPartialPressure">水蒸気分圧[kPa]</param>
        /// <returns>大気放射量[W/m2]</returns>
        public static double GetAtmosphericRadiation(double temperature, double cloudCover, double waterVaporPartialPressure)
        {
            double br = 0.51 + 0.209 * Math.Sqrt(waterVaporPartialPressure);
            double cc6 = 0.062 * cloudCover;
            return ((1d - cc6) * br + cc6) * BLACK_RADIATION * Math.Pow(temperature + 273.15, 4);
        }

        /// <summary>夜間放射量[W/m2]を計算する</summary>
        /// <param name="temperature">外気乾球温度[C]</param>
        /// <param name="cloudCover">雲量[-]</param>
        /// <param name="waterVaporPartialPressure">水蒸気分圧[kPa]</param>
        /// <returns>夜間放射量[W/m2]</returns>
        public static double GetNocturnalRadiation(double temperature, double cloudCover, double waterVaporPartialPressure)
        {
            double br = 0.51 + 0.209 * Math.Sqrt(waterVaporPartialPressure);
            double cc6 = 0.062 * cloudCover;
            return (1d - cc6) * (1d - br) * BLACK_RADIATION * Math.Pow(temperature + 273.15, 4);
        }

        #endregion

    }
}
