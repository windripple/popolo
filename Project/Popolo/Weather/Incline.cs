/* Incline.cs
 * 
 * Copyright (C) 2008 E.Togashi
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
using System.Collections.Generic;
using System.Text;

namespace Popolo.Weather
{
    /// <summary>傾斜面</summary>
    public class Incline : ImmutableIncline
    {

        #region enumerators

        /// <summary>既定義の方位</summary>
        public enum Orientation
        {
            /// <summary>南</summary>
            S,
            /// <summary>南南西</summary>
            SSW,
            /// <summary>南西</summary>
            SW,
            /// <summary>西南西</summary>
            WSW,
            /// <summary>西</summary>
            W,
            /// <summary>西北西</summary>
            WNW,
            /// <summary>北西</summary>
            NW,
            /// <summary>北北西</summary>
            NNW,
            /// <summary>北</summary>
            N,
            /// <summary>北北東</summary>
            NNE,
            /// <summary>北東</summary>
            NE,
            /// <summary>東北東</summary>
            ENE,
            /// <summary>東</summary>
            E,
            /// <summary>東南東</summary>
            ESE,
            /// <summary>南東</summary>
            SE,
            /// <summary>南南東</summary>
            SSE
        }

        #endregion

        #region Instance variables

        /// <summary>方位角および傾斜角</summary>
        private double horizontalAngle, verticalAngle;

        /// <summary>方向余弦</summary>
        private double ws, ww, wz;

        /// <summary>天空への形態係数[-]</summary>
        private double configurationFactorToSky;

        /// <summary>正弦、余弦</summary>
        private double sinAlpha, sinBeta, cosAlpha, cosBeta;

        #endregion

        #region Properties

        /// <summary>方位角[radian]（南を0、東を負、西を正とする）を取得する</summary>
        public double HorizontalAngle
        {
            get
            {
                return horizontalAngle;
            }
        }

        /// <summary>傾斜角[radian]（水平面を0、垂直面を1/2πとする）を取得する</summary>
        public double VerticalAngle
        {
            get
            {
                return verticalAngle;
            }
        }

        /// <summary>Z軸に関する方向余弦を取得する</summary>
        public double DirectionCosineZ
        {
            get
            {
                return wz;
            }
        }

        /// <summary>南北軸に関する方向余弦を取得する（南向きを正とする）</summary>
        public double DirectionCosineSN
        {
            get
            {
                return ws;
            }
        }

        /// <summary>東西軸に関する方向余弦を取得する（西向きを正とする）</summary>
        public double DirectionCosineWE
        {
            get
            {
                return ww;
            }
        }

        /// <summary>天空への形態係数[-]を取得する</summary>
        public double ConfigurationFactorToSky
        {
            get
            {
                return configurationFactorToSky;
            }
        }

        #endregion

        #region Constructor

        /// <summary>Constructor</summary>
        /// <param name="horizontalAngle">方位角[radian]（南を0、東を負、西を正とする）</param>
        /// <param name="verticalAngle">傾斜角[radian]（水平面を0、垂直面を1/2πとする）</param>
        public Incline(double horizontalAngle, double verticalAngle)
        {
            Initialize(horizontalAngle, verticalAngle);
        }

        /// <summary>Constructor</summary>
        /// <param name="ori">方位</param>
        /// <param name="verticalAngle">傾斜角[radian]（水平面を0、垂直面を1/2πとする）</param>
        public Incline(Orientation ori, double verticalAngle)
        {
            Initialize(ori, verticalAngle);
        }

        /// <summary>コピーConstructor</summary>
        /// <param name="incline">傾斜面</param>
        public Incline(ImmutableIncline incline)
        {
            Initialize(incline.HorizontalAngle, incline.VerticalAngle);
        }

        #endregion

        #region public methods

        /// <summary>傾斜面情報をコピーする</summary>
        /// <param name="incline">傾斜面オブジェクト</param>
        public void Copy(ImmutableIncline incline)
        {
            Initialize(incline.HorizontalAngle, incline.VerticalAngle);
        }

        /// <summary>初期化処理</summary>
        /// <param name="horizontalAngle">方位角[radian]（南を0、東を負、西を正とする）</param>
        /// <param name="verticalAngle">傾斜角[radian]（水平面を0、垂直面を1/2πとする）</param>
        public void Initialize(double horizontalAngle, double verticalAngle)
        {
            //方位角は-180~180
            if (Math.PI < horizontalAngle || horizontalAngle < -Math.PI) this.horizontalAngle = horizontalAngle % Math.PI;
            else this.horizontalAngle = horizontalAngle;

            //傾斜角は0~180
            this.verticalAngle = verticalAngle % Math.PI;
            if (Math.PI < this.verticalAngle)
            {
                this.verticalAngle = Math.PI - this.verticalAngle;
            }

            GetDirectionCosine(this.horizontalAngle, this.verticalAngle, out wz, out ws, out ww);
            configurationFactorToSky = GetConfigurationFactorToSky(verticalAngle);

            sinAlpha = Math.Sin(horizontalAngle);
            sinBeta = Math.Sin(verticalAngle);
            cosAlpha = Math.Cos(horizontalAngle);
            cosBeta = Math.Cos(verticalAngle);
        }

        /// <summary>初期化処理</summary>
        /// <param name="ori">方位</param>
        /// <param name="verticalAngle">傾斜角[radian]（水平面を0、垂直面を1/2πとする）</param>
        public void Initialize(Orientation ori, double verticalAngle)
        {
            switch (ori)
            {
                case Orientation.S:
                    Initialize(0d, verticalAngle);
                    return;
                case Orientation.SSE:
                    Initialize(-Math.PI / 8 * 1, verticalAngle);
                    return;
                case Orientation.SE:
                    Initialize(-Math.PI / 8 * 2, verticalAngle);
                    return;
                case Orientation.ESE:
                    Initialize(-Math.PI / 8 * 3, verticalAngle);
                    return;
                case Orientation.E:
                    Initialize(-Math.PI / 8 * 4, verticalAngle);
                    return;
                case Orientation.ENE:
                    Initialize(-Math.PI / 8 * 5, verticalAngle);
                    return;
                case Orientation.NE:
                    Initialize(-Math.PI / 8 * 6, verticalAngle);
                    return;
                case Orientation.NNE:
                    Initialize(-Math.PI / 8 * 7, verticalAngle);
                    return;
                case Orientation.N:
                    Initialize(Math.PI, verticalAngle);
                    return;
                case Orientation.SSW:
                    Initialize(Math.PI / 8 * 1, verticalAngle);
                    return;
                case Orientation.SW:
                    Initialize(Math.PI / 8 * 2, verticalAngle);
                    return;
                case Orientation.WSW:
                    Initialize(Math.PI / 8 * 3, verticalAngle);
                    return;
                case Orientation.W:
                    Initialize(Math.PI / 8 * 4, verticalAngle);
                    return;
                case Orientation.WNW:
                    Initialize(Math.PI / 8 * 5, verticalAngle);
                    return;
                case Orientation.NW:
                    Initialize(Math.PI / 8 * 6, verticalAngle);
                    return;
                case Orientation.NNW:
                    Initialize(Math.PI / 8 * 7, verticalAngle);
                    return;
            }
        }

        /// <summary>向きを逆転させる</summary>
        public void Reverse()
        {
            horizontalAngle += Math.PI;
            verticalAngle += Math.PI;

            Initialize(horizontalAngle, verticalAngle);
        }

        /// <summary>プロファイル角および傾斜面の法線を基準とした太陽方位角の正接を求める</summary>
        /// <param name="sun">太陽</param>
        /// <param name="tanPhi">プロファイル角の正接</param>
        /// <param name="tanGamma">傾斜面の法線を基準とした太陽方位角の正接</param>
        public void GetTanPhiAndGamma(ImmutableSun sun, out double tanPhi, out double tanGamma)
        {
            double cosTheta = Math.Max(this.GetDirectSolarRadiationRate(sun), 0.01);

            double sh = Math.Sin(sun.Altitude);
            double ch = Math.Cos(sun.Altitude);
            double ss = ch * Math.Cos(sun.Orientation);
            double sw = ch * Math.Sin(sun.Orientation);

            tanPhi = (sh * sinBeta - sw * (cosBeta * sinAlpha) - ss * (cosBeta * cosAlpha)) / cosTheta;
            tanGamma = (sw * cosAlpha - ss * sinAlpha) / cosTheta;
        }

        /// <summary>傾斜面の法線に対する太陽光線入射角の余弦cosθ[-]を計算する</summary>
        /// <param name="sun">太陽</param>
        /// <returns>傾斜面の法線に対する太陽光線入射角の余弦cosθ[-]</returns>
        public double GetDirectSolarRadiationRate(ImmutableSun sun)
        {
            return Math.Max(0, GetDirectSolarRadiationRateToIncline(sun, this));
        }

        #endregion

        #region public staticメソッド

        /// <summary>天空に対する傾斜面の形態係数[-]を計算する</summary>
        /// <param name="beta">傾斜面の傾斜角[radian]（水平面を0、垂直面を1/2πとする）</param>
        /// <returns>天空に対する傾斜面の形態係数[-]</returns>
        public static double GetConfigurationFactorToSky(double beta)
        {
            return (1d + Math.Cos(beta)) / 2d;
        }

        /// <summary>傾斜面の法線の方向余弦を計算する</summary>
        /// <param name="alpha">方位角[radian]（南を0、東を負、西を正とする）</param>
        /// <param name="beta">傾斜角[radian]（水平面を0、垂直面を1/2πとする）</param>
        /// <param name="wz">垂直成分</param>
        /// <param name="ws">南北軸の成分（南向きを正とする）</param>
        /// <param name="ww">東西軸の成分（西向きを正とする）</param>
        public static void GetDirectionCosine(double alpha, double beta,
            out double wz, out double ws, out double ww)
        {
            wz = Math.Cos(beta);
            double sb = Math.Sin(beta);
            ws = sb * Math.Cos(alpha);
            ww = sb * Math.Sin(alpha);
        }

        /// <summary>傾斜面の法線に対する太陽光線入射角の余弦cosθ[-]を計算する</summary>
        /// <param name="wz">傾斜面法線の方向余弦の垂直成分</param>
        /// <param name="ws">傾斜面法線の方向余弦の南北軸の成分（南向きを正とする）</param>
        /// <param name="ww">傾斜面法線の方向余弦の東西軸の成分（西向きを正とする）</param>
        /// <param name="altitude">太陽高度[radian]</param>
        /// <param name="orientation">太陽方位角[radian]</param>
        /// <returns>傾斜面に対する太陽光線入射角の余弦cosθ[-]</returns>
        public static double GetDirectSolarRadiationRateToIncline(double wz, double ws, double ww,
            double altitude, double orientation)
        {
            if (altitude == 0 && orientation == 0) return 0;
            wz *= Math.Sin(altitude);
            double ch = Math.Cos(altitude);
            ws *= ch * Math.Cos(orientation);
            ww *= ch * Math.Sin(orientation);
            return wz + ws + ww;
        }

        /// <summary>傾斜面の法線に対する太陽光線入射角の余弦cosθ[-]を計算する</summary>
        /// <param name="alpha">方位角[radian]（南を0、東を負、西を正とする）</param>
        /// <param name="beta">傾斜角[radian]（水平面を0、垂直面を1/2πとする）</param>
        /// <param name="altitude">太陽高度[radian]</param>
        /// <param name="orientation">太陽方位角[radian]</param>
        /// <returns>傾斜面に対する太陽光線入射角の余弦cosθ[-]</returns>
        /// <remarks>方位角を直接に入力するメソッドの方が高速</remarks>
        public static double GetDirectSolarRadiationRateToIncline(double alpha, double beta,
            double altitude, double orientation)
        {
            double wz, ws, ww;
            GetDirectionCosine(alpha, beta, out wz, out ws, out ww);
            return GetDirectSolarRadiationRateToIncline(wz, ws, ww, altitude, orientation);
        }

        /// <summary>傾斜面の法線に対する太陽光線入射角の余弦cosθ[-]を計算する</summary>
        /// <param name="sun">太陽</param>
        /// <param name="incline">傾斜面</param>
        /// <returns>傾斜面に対する太陽光線入射角の余弦cosθ[-]</returns>
        public static double GetDirectSolarRadiationRateToIncline(ImmutableSun sun, ImmutableIncline incline)
        {
            return GetDirectSolarRadiationRateToIncline(incline.DirectionCosineZ, incline.DirectionCosineSN, incline.DirectionCosineWE,
                sun.Altitude, sun.Orientation);
        }

        #endregion

    }

    /// <summary>読み取り専用傾斜面</summary>
    public interface ImmutableIncline
    {

        #region Properties

        /// <summary>方位角[radian]（南を0、東を負、西を正とする）を取得する</summary>
        double HorizontalAngle
        {
            get;
        }

        /// <summary>傾斜角[radian]（水平面を0、垂直面を1/2πとする）を取得する</summary>
        double VerticalAngle
        {
            get;
        }

        /// <summary>Z軸に関する方向余弦を取得する</summary>
        double DirectionCosineZ
        {
            get;
        }

        /// <summary>南北軸に関する方向余弦を取得する（南向きを正とする）</summary>
        double DirectionCosineSN
        {
            get;
        }

        /// <summary>東西軸に関する方向余弦を取得する（西向きを正とする）</summary>
        double DirectionCosineWE
        {
            get;
        }

        /// <summary>天空への形態係数[-]を取得する</summary>
        double ConfigurationFactorToSky
        {
            get;
        }

        #endregion

        #region public methods

        /// <summary>プロファイル角および傾斜面の法線を基準とした太陽方位角の正接を求める</summary>
        /// <param name="sun">太陽</param>
        /// <param name="tanPhi">プロファイル角の正接</param>
        /// <param name="tanGamma">傾斜面の法線を基準とした太陽方位角の正接</param>
        void GetTanPhiAndGamma(ImmutableSun sun, out double tanPhi, out double tanGamma);

        /// <summary>傾斜面の法線に対する太陽光線入射角の余弦cosθ[-]を計算する</summary>
        /// <param name="sun">太陽</param>
        /// <returns>傾斜面の法線に対する太陽光線入射角の余弦cosθ[-]</returns>
        double GetDirectSolarRadiationRate(ImmutableSun sun);

        #endregion

    }

}
