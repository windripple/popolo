/* SunShade.cs
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

using Popolo.Weather;

namespace Popolo.ThermalLoad
{
    /// <summary>日除けクラス</summary>
    public class SunShade : ImmutableSunShade
    {

        #region 列挙型定義

        /// <summary>日除けの形状</summary>
        public enum Shape
        {
            /// <summary>無し</summary>
            None = 0,
            /// <summary>水平庇</summary>
            Horizontal = 1,
            /// <summary>水平庇（無限大長）</summary>
            LongHorizontal = 2,
            /// <summary>袖壁（左）</summary>
            VerticalLeft = 3,
            /// <summary>袖壁（右）</summary>
            VerticalRight = 4,
            /// <summary>袖壁（両方）</summary>
            VerticalBoth = 5,
            /// <summary>袖壁（無限大長:左）</summary>
            LongVerticalLeft = 6,
            /// <summary>袖壁（無限大長:右）</summary>
            LongVerticalRight = 7,
            /// <summary>袖壁（無限大長:両方）</summary>
            LongVerticalBoth = 8,
            /// <summary>ルーバー</summary>
            Grid = 9
        }

        #endregion

        #region インスタンス変数

        /// <summary>傾斜面</summary>
        private Incline incline = new Incline(0d, 0.5 * Math.PI);

        /// <summary>日除けの形状</summary>
        private Shape ssShape = Shape.Horizontal;

        /// <summary>日除けが裏返しか否か</summary>
        private bool isReverse = false;

        /// <summary>窓高[m]</summary>
        private double windowHeight;

        /// <summary>窓幅[m]</summary>
        private double windowWidth;

        /// <summary>張り出し幅[m]</summary>
        private double pendent;

        /// <summary>上部マージン[m]</summary>
        private double topMargin;

        /// <summary>下部マージン[m]</summary>
        private double bottomMargin;

        /// <summary>左側マージン[m]</summary>
        private double leftMargin;

        /// <summary>右側マージン[m]</summary>
        private double rightMargin;

        #endregion

        #region プロパティ

        /// <summary>空の日除けを取得する</summary>
        public static SunShade EmptySunShade
        {
            get;
            private set;
        }

        /// <summary>名称を設定・取得する</summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>傾斜面を設定・取得する</summary>
        public ImmutableIncline Incline
        {
            set
            {
                incline.Copy(value);
            }
            get
            {
                return incline;
            }
        }

        /// <summary>日除けの種類を取得する</summary>
        public Shape SunShadeShape
        {
            get
            {
                return ssShape;
            }
        }

        /// <summary>庇が裏返しか否かを設定・取得する</summary>
        public bool IsReverse
        {
            set
            {
                isReverse = value;
            }
            get
            {
                return isReverse;
            }
        }

        /// <summary>窓高さ[m]を取得する</summary>
        public double WindowHeight
        {
            get
            {
                return windowHeight;
            }
        }

        /// <summary>窓幅[m]を取得する</summary>
        public double WindowWidth
        {
            get
            {
                return windowWidth;
            }
        }

        /// <summary>張り出し幅[m]を取得する</summary>
        public double Pendent
        {
            get
            {
                return pendent;
            }
        }

        /// <summary>上部マージン[m]を取得する</summary>
        public double TopMargin
        {
            get
            {
                return topMargin;
            }
        }

        /// <summary>下部マージン[m]を取得する</summary>
        public double BottomMargin
        {
            get
            {
                return bottomMargin;
            }
        }

        /// <summary>左側マージン[m]を取得する</summary>
        public double LeftMargin
        {
            get
            {
                return leftMargin;
            }
        }

        /// <summary>右側マージン[m]を取得する</summary>
        public double RightMargin
        {
            get
            {
                return rightMargin;
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        private SunShade() { }

        static SunShade()
        {
            SunShade ss = new SunShade();
            ss.ssShape = Shape.None;
            EmptySunShade = ss;
        }

        #endregion

        #region publicメソッド

        /// <summary>日除けをコピーする</summary>
        /// <param name="sunShade">日除け</param>
        public void Copy(ImmutableSunShade sunShade)
        {
            this.bottomMargin = sunShade.BottomMargin;
            this.incline.Copy(sunShade.Incline);
            this.IsReverse = sunShade.IsReverse;
            this.leftMargin = sunShade.LeftMargin;
            this.Name = sunShade.Name;
            this.pendent = sunShade.Pendent;
            this.rightMargin = sunShade.RightMargin;
            this.ssShape = sunShade.SunShadeShape;
            this.topMargin = sunShade.TopMargin;
            this.windowHeight = sunShade.WindowHeight;
            this.windowWidth = sunShade.WindowWidth;            
        }

        /// <summary>日影面積率[-]を計算する</summary>
        /// <param name="sun">太陽</param>
        /// <returns>日影面積率[-]</returns>
        public double GetShadowRate(ImmutableSun sun)
        {
            double sr = 0;
            if (ssShape == Shape.None) sr = 0;
            else
            {
                double tanGamma, tanPhi;
                if (incline.GetDirectSolarRadiationRate(sun) <= 0) return 1;
                incline.GetTanPhiAndGamma(sun, out tanPhi, out tanGamma);
                double da = pendent * tanGamma;
                double dp = pendent * tanPhi;

                switch (ssShape)
                {
                    case Shape.Horizontal:
                        sr =fnasdw1(da, dp, windowWidth, windowHeight, leftMargin, topMargin, rightMargin);
                        break;
                    case Shape.VerticalLeft:
                        sr = fnasdw1(dp, da, windowHeight, windowWidth, topMargin, leftMargin, bottomMargin);
                        break;
                    case Shape.VerticalRight:
                        da = -da;
                        sr = fnasdw1(dp, da, windowHeight, windowWidth, topMargin, leftMargin, bottomMargin);
                        break;
                    case Shape.VerticalBoth:
                        da = Math.Abs(da);
                        sr = fnasdw1(dp, da, windowHeight, windowWidth, topMargin, leftMargin, bottomMargin);
                        break;
                    case Shape.LongHorizontal:
                        sr = fnasdw2(dp, windowHeight, windowWidth, topMargin);
                        break;
                    case Shape.LongVerticalLeft:
                        sr = fnasdw2(da, windowWidth, windowHeight, leftMargin);
                        break;
                    case Shape.LongVerticalRight:
                        da = -da;
                        sr = fnasdw2(da, windowWidth, windowHeight, rightMargin);
                        break;
                    case Shape.LongVerticalBoth:
                        da = Math.Abs(da);
                        sr = fnasdw2(da, windowWidth, windowHeight, leftMargin);    //ここ、問題有り。左右非対称考慮できてない
                        break;
                    case Shape.Grid:
                        sr = fnasdw3(da, dp, windowWidth, windowHeight, leftMargin, topMargin, rightMargin, bottomMargin);
                        break;
                }
                sr = sr / (windowHeight * windowWidth);
                if (IsReverse) return 1 - sr;
            }

            if (isReverse) return 1 - sr;
            else return sr;
        }

        #endregion

        #region public staticメソッド

        /// <summary>水平庇を作成する</summary>
        /// <param name="windowWidth">窓幅[m]</param>
        /// <param name="windowHeight">窓高[m]</param>
        /// <param name="pendent">張り出し幅[m]</param>
        /// <param name="leftMargin">左側マージン[m]</param>
        /// <param name="rightMargin">右側マージン[m]</param>
        /// <param name="topMargin">上部マージン[m]</param>
        /// <param name="incline">傾斜面</param>
        /// <returns>水平庇</returns>
        public static SunShade MakeHorizontalSunShade(double windowWidth, double windowHeight, double pendent,
            double leftMargin, double rightMargin, double topMargin, ImmutableIncline incline)
        {
            SunShade ss = new SunShade();
            ss.ssShape = Shape.Horizontal;
            ss.windowWidth = windowWidth;
            ss.windowHeight = windowHeight;
            ss.pendent = pendent;
            ss.leftMargin = leftMargin;
            ss.rightMargin = rightMargin;
            ss.topMargin = topMargin;
            ss.incline.Copy(incline);
            return ss;
        }

        /// <summary>水平庇（無限大長）を作成する</summary>
        /// <param name="windowWidth">窓幅[m]</param>
        /// <param name="windowHeight">窓高[m]</param>
        /// <param name="pendent">張り出し幅[m]</param>
        /// <param name="topMargin">上部マージン[m]</param>
        /// <param name="incline">傾斜面</param>
        /// <returns>水平庇（無限大長）</returns>
        public static SunShade MakeHorizontalSunShade(double windowWidth, double windowHeight, double pendent,
            double topMargin, ImmutableIncline incline)
        {
            SunShade ss = new SunShade();
            ss.ssShape = Shape.LongHorizontal;
            ss.windowWidth = windowWidth;
            ss.windowHeight = windowHeight;
            ss.pendent = pendent;
            ss.topMargin = topMargin;
            ss.incline.Copy(incline);
            return ss;
        }

        /// <summary>袖壁を作成する</summary>
        /// <param name="windowWidth">窓幅[m]</param>
        /// <param name="windowHeight">窓高[m]</param>
        /// <param name="pendent">張り出し幅[m]</param>
        /// <param name="sideMargin">横側マージン[m]</param>
        /// <param name="isLeftSide">左側か否か（右の場合はfalse）</param>
        /// <param name="topMargin">上部マージン[m]</param>
        /// <param name="bottomMargin">下部マージン[m]</param>
        /// <param name="incline">傾斜面</param>
        /// <returns>袖壁</returns>
        public static SunShade MakeVerticalSunShade(double windowWidth, double windowHeight, double pendent,
            double sideMargin, bool isLeftSide, double topMargin, double bottomMargin, ImmutableIncline incline)
        {
            SunShade ss = new SunShade();
            ss.windowWidth = windowWidth;
            ss.windowHeight = windowHeight;
            ss.pendent = pendent;
            if (isLeftSide)
            {
                ss.ssShape = Shape.VerticalLeft;
                ss.leftMargin = sideMargin;
            }
            else
            {
                ss.ssShape = Shape.VerticalRight;
                ss.rightMargin = sideMargin;
            }
            ss.topMargin = topMargin;
            ss.bottomMargin = bottomMargin;
            ss.incline.Copy(incline);
            return ss;
        }

        /// <summary>袖壁（無限長）を作成する</summary>
        /// <param name="windowWidth">窓幅[m]</param>
        /// <param name="windowHeight">窓高[m]</param>
        /// <param name="pendent">張り出し幅[m]</param>
        /// <param name="sideMargin">横側マージン[m]</param>
        /// <param name="isLeftSide">左側か否か（右の場合はfalse）</param>
        /// <param name="incline">傾斜面</param>
        /// <returns>袖壁（無限長）</returns>
        public static SunShade MakeVerticalSunShade(double windowWidth, double windowHeight, double pendent,
            double sideMargin, bool isLeftSide, ImmutableIncline incline)
        {
            SunShade ss = new SunShade();
            ss.windowWidth = windowWidth;
            ss.windowHeight = windowHeight;
            ss.pendent = pendent;
            if (isLeftSide)
            {
                ss.ssShape = Shape.LongVerticalLeft;
                ss.leftMargin = sideMargin;
            }
            else
            {
                ss.ssShape = Shape.LongVerticalRight;
                ss.rightMargin = sideMargin;
            }
            ss.incline.Copy(incline);
            return ss;
        }

        /// <summary>袖壁を作成する</summary>
        /// <param name="windowWidth">窓幅[m]</param>
        /// <param name="windowHeight">窓高[m]</param>
        /// <param name="pendent">張り出し幅[m]</param>
        /// <param name="sideMargin">横側マージン[m]</param>
        /// <param name="topMargin">上部マージン[m]</param>
        /// <param name="bottomMargin">下部マージン[m]</param>
        /// <param name="incline">傾斜面</param>
        /// <returns>袖壁</returns>
        public static SunShade MakeVerticalSunShade(double windowWidth, double windowHeight, double pendent,
            double sideMargin, double topMargin, double bottomMargin, ImmutableIncline incline)
        {
            SunShade ss = new SunShade();
            ss.windowWidth = windowWidth;
            ss.windowHeight = windowHeight;
            ss.pendent = pendent;
            ss.ssShape = Shape.VerticalBoth;
            ss.leftMargin = sideMargin;
            ss.rightMargin = sideMargin;
            ss.topMargin = topMargin;
            ss.bottomMargin = bottomMargin;
            ss.incline.Copy(incline);
            return ss;
        }

        /// <summary>袖壁（無限長）を作成する</summary>
        /// <param name="windowWidth">窓幅[m]</param>
        /// <param name="windowHeight">窓高[m]</param>
        /// <param name="pendent">張り出し幅[m]</param>
        /// <param name="sideMargin">横側マージン[m]</param>
        /// <param name="incline">傾斜面</param>
        /// <returns>袖壁（無限長）</returns>
        public static SunShade MakeVerticalSunShade(double windowWidth, double windowHeight, double pendent,
            double sideMargin, ImmutableIncline incline)
        {
            SunShade ss = new SunShade();
            ss.windowWidth = windowWidth;
            ss.windowHeight = windowHeight;
            ss.pendent = pendent;
            ss.ssShape = Shape.LongVerticalBoth;
            ss.leftMargin = sideMargin;
            ss.rightMargin = sideMargin;
            ss.incline.Copy(incline);
            return ss;
        }

        /// <summary>ルーバーを作成する</summary>
        /// <param name="windowWidth">窓幅[m]</param>
        /// <param name="windowHeight">窓高[m]</param>
        /// <param name="pendent">張り出し幅[m]</param>
        /// <param name="leftMargin">左側マージン[m]</param>
        /// <param name="rightMargin">右側マージン[m]</param>
        /// <param name="topMargin">上部マージン[m]</param>
        /// <param name="bottomMargin">下部マージン[m]</param>
        /// <param name="incline">傾斜面</param>
        /// <returns>ルーバー</returns>
        public static SunShade MakeGridSunShade(double windowWidth, double windowHeight, double pendent,
            double leftMargin, double rightMargin, double topMargin, double bottomMargin, ImmutableIncline incline)
        {
            SunShade ss = new SunShade();
            ss.ssShape = Shape.Grid;
            ss.windowWidth = windowWidth;
            ss.windowHeight = windowHeight;
            ss.pendent = pendent;
            ss.leftMargin = leftMargin;
            ss.rightMargin = rightMargin;
            ss.topMargin = topMargin;
            ss.bottomMargin = bottomMargin;
            ss.incline.Copy(incline);
            return ss;
        }

        #endregion

        #region privateメソッド

        private static double fnasdw1(double da, double dp, double wr, double hr, double wi1, double hi, double wi2)
        {
            if (dp <= 0) return 0;

            double wi;

            if (0 < da) wi = wi1;
            else wi = wi2;

            double dad = Math.Abs(da);
            double dhad = wi * dp / Math.Max(wi, dad) - hi;
            double dha = Math.Min(Math.Max(0, dhad), hr);
            double dhbd = (wi + wr) * dp / Math.Max(wi + wr, dad) - hi;
            double dhb = Math.Min(Math.Max(0, dhbd), hr);

            double dwa;
            if (dp <= hi) dwa = 0;
            else
            {
                double dwad = (wi + wr) - hi * dad / dp;
                dwa = Math.Min(Math.Max(0, dwad), wr);
            }

            double dwbd = (wi + wr) - (hi + hr) * dad / Math.Max(hi + hr, dp);
            double dwb = Math.Min(Math.Max(0, dwbd), wr);

            return dwa * dha + 0.5 * (dwa + dwb) * (dhb - dha);
        }

        private static double fnasdw2(double dp, double hr, double wr, double hi)
        {
            if (dp <= 0) return 0;

            double dh = Math.Min(Math.Max(0, dp - hi), hr);
            return wr * dh;
        }

        private static double fnasdw3(double da, double dp, double wr, double hr,
            double wi1, double hi1, double wi2, double hi2)
        {
            double dw1 = Math.Min(Math.Max(0, da - wi1), wr);
            double dw2 = Math.Min(Math.Max(0, -(da + wi2)), wr);
            double dh1 = Math.Min(Math.Max(0, dp - hi1), hr);
            double dh2 = Math.Min(Math.Max(0, -(dp + hi2)), hr);
            return wr * (dh1 + dh2) + (dw1 + dw2) * (hr - dh1 - dh2);
        }

        #endregion

    }

    #region 読み取り専用の日除け

    /// <summary>読み取り専用の日除け</summary>
    public interface ImmutableSunShade
    {

        #region プロパティ

        /// <summary>名称を取得する</summary>
        string Name
        {
            get;
        }

        /// <summary>傾斜面を取得する</summary>
        ImmutableIncline Incline
        {
            get;
        }

        /// <summary>日除けの種類を取得する</summary>
        SunShade.Shape SunShadeShape
        {
            get;
        }

        /// <summary>庇が裏返しか否かを取得する</summary>
        bool IsReverse
        {
            get;
        }

        /// <summary>窓高さ[m]を取得する</summary>
        double WindowHeight
        {
            get;
        }

        /// <summary>窓幅[m]を取得する</summary>
        double WindowWidth
        {
            get;
        }

        /// <summary>張り出し幅[m]を取得する</summary>
        double Pendent
        {
            get;
        }

        /// <summary>上部マージン[m]を取得する</summary>
        double TopMargin
        {
            get;
        }

        /// <summary>下部マージン[m]を取得する</summary>
        double BottomMargin
        {
            get;
        }

        /// <summary>左側マージン[m]を取得する</summary>
        double LeftMargin
        {
            get;
        }

        /// <summary>右側マージン[m]を取得する</summary>
        double RightMargin
        {
            get;
        }

        #endregion

        #region publicメソッド

        /// <summary>日影面積率[-]を計算する</summary>
        /// <param name="sun">太陽</param>
        /// <returns>日影面積率[-]</returns>
        double GetShadowRate(ImmutableSun sun);

        #endregion

    }

    #endregion

}
