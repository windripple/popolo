/* Outdoor.cs
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

using Popolo.ThermophysicalProperty;
using Popolo.Weather;

namespace Popolo.ThermalLoad
{
    /// <summary>out door class</summary>
    public class Outdoor : ImmutableOutdoor
    {

        #region Instance variables

        /// <summary>sun</summary>
        private ImmutableSun sun = new Sun(35, 139, 134);

        /// <summary>Outdoor air state</summary>
        private MoistAir airState = new MoistAir();

        /// <summary>List of wall surfaces which face to outdoor</summary>
        private List<WallSurface> wallSurfaces = new List<WallSurface>();

        /// <summary>List of windows</summary>
        private List<Window> windows = new List<Window>();

        /// <summary>List of wall surfaces located under ground</summary>
        private List<WallSurface> groundWallSurfaces = new List<WallSurface>();

        /// <summary>Nocturnal radiation[W/m2]</summary>
        private double nocturnalRadiation = 0;

        #endregion

        #region Properties

        /// <summary>外気条件を設定・取得する</summary>
        public ImmutableMoistAir AirState
        {
            set
            {
                value.CopyTo(this.airState);

                foreach (Window win in windows)
                {
                    WindowSurface ws = win.GetSurface(true);
                    ws.AirTemperature = value.DryBulbTemperature;
                }
            }
            get
            {
                return airState;
            }
        }

        /// <summary>太陽を設定・取得する</summary>
        public ImmutableSun Sun
        {
            get
            {
                return sun;
            }
            set
            {
                if (sun != null)
                {
                    sun = value;
                    foreach (Window win in windows)
                    {
                        win.Sun = value;
                    }
                }
            }
        }

        /// <summary>壁表面リストを取得する</summary>
        public ImmutableSurface[] WallSurfaces
        {
            get
            {
                return wallSurfaces.ToArray();
            }
        }

        /// <summary>地中の壁表面リストを取得する</summary>
        public ImmutableSurface[] GroundWallSurfaces
        {
            get
            {
                return groundWallSurfaces.ToArray();
            }
        }

        /// <summary>窓リストを取得する</summary>
        public ImmutableWindow[] Windows
        {
            get
            {
                return windows.ToArray();
            }
        }

        /// <summary>地中温度[C]を設定・取得する</summary>
        public double GroundTemperature
        {
            get;
            set;
        }

        /// <summary>夜間放射[W/m2]を設定・取得する</summary>
        public double NocturnalRadiation
        {
            get {
                return nocturnalRadiation;
            }
            set {
                nocturnalRadiation = Math.Max(0, value);
                foreach (Window win in windows)
                {
                    win.NocturnalRadiation = nocturnalRadiation;
                }
            }
        }

        #endregion

        #region public methods

        /// <summary>登録されている壁表面の境界条件を設定する</summary>
        public void SetWallSurfaceBoundaryState()
        {
            foreach (WallSurface ws in wallSurfaces)
            {
                ImmutableIncline ic = ws.Incline;
                ws.Radiation = ws.SolarAbsorptance * GetRadiationToIncline(ic, ws.Albedo, ws.ShadingRate)
                    - ws.LongWaveEmissivity * ic.ConfigurationFactorToSky * NocturnalRadiation;
                ws.AirTemperature = airState.DryBulbTemperature;
            }
            foreach (WallSurface ws in groundWallSurfaces)
            {
                ws.Radiation = 0;
                ws.AirTemperature = GroundTemperature;
            }
        }        

        /// <summary>壁表面を追加する</summary>
        /// <param name="wallSurface">追加する壁表面</param>
        /// <returns>追加成功の真偽</returns>
        public bool AddWallSurface(WallSurface wallSurface)
        {
            if (!this.wallSurfaces.Contains(wallSurface))
            {
                wallSurfaces.Add(wallSurface);
                return true;
            }
            else return false;
        }

        /// <summary>壁表面を削除する</summary>
        /// <param name="wallSurface">削除する壁表面</param>
        /// <returns>削除成功の真偽</returns>
        public bool RemoveWallSurface(WallSurface wallSurface)
        {
            if (this.wallSurfaces.Contains(wallSurface))
            {
                this.wallSurfaces.Remove(wallSurface);
                return true;
            }
            else return false;
        }

        /// <summary>地中の壁表面を追加する</summary>
        /// <param name="wallSurface">追加する地中の壁表面</param>
        /// <returns>追加成功の真偽</returns>
        public bool AddGroundWallSurface(WallSurface wallSurface)
        {
            if (!this.groundWallSurfaces.Contains(wallSurface))
            {
                wallSurface.ConvectiveRate = 1d;
                groundWallSurfaces.Add(wallSurface);
                return true;
            }
            else return false;
        }

        /// <summary>地中の壁表面を削除する</summary>
        /// <param name="wallSurface">削除する地中の壁表面</param>
        /// <returns>削除成功の真偽</returns>
        public bool RemoveGroundWallSurface(WallSurface wallSurface)
        {
            if (this.groundWallSurfaces.Contains(wallSurface))
            {
                this.groundWallSurfaces.Remove(wallSurface);
                return true;
            }
            else return false;
        }

        /// <summary>窓を追加する</summary>
        /// <param name="window">追加する窓</param>
        /// <returns>追加成功の真偽</returns>
        public bool AddWindow(Window window)
        {
            if (!this.windows.Contains(window))
            {
                window.Sun = this.Sun;
                WindowSurface ws = window.GetSurface(true);
                ws.AirTemperature = this.AirState.DryBulbTemperature;
                windows.Add(window);
                return true;
            }
            else return false;
        }

        /// <summary>窓を削除する</summary>
        /// <param name="window">削除する窓</param>
        /// <returns>削除成功の真偽</returns>
        public bool RemoveWindow(Window window)
        {
            if (this.windows.Contains(window))
            {
                this.windows.Remove(window);
                return true;
            }
            else return false;
        }

        /// <summary>斜面に入射する放射量[W/m2]を計算する</summary>
        /// <param name="incline">斜面</param>
        /// <param name="albedo">アルベド[-]</param>
        /// <param name="shadingRate">日影率[-]</param>
        /// <returns>斜面に入射する放射量[W/m2]</returns>
        public double GetRadiationToIncline(ImmutableIncline incline, double albedo, double shadingRate)
        {
            //直達成分
            double dsRate = incline.GetDirectSolarRadiationRate(sun);
            //日影部分に関しては直達日射は0
            dsRate *= sun.DirectNormalRadiation * (1d - shadingRate);
            //拡散成分
            double dfRad = incline.ConfigurationFactorToSky * sun.DiffuseHorizontalRadiation;
            //地表面反射成分
            double alRad = (1 - incline.ConfigurationFactorToSky) * albedo * sun.GlobalHorizontalRadiation;

            return dsRate + dfRad + alRad;
        }

        /*/// <summary>登録された壁表面に対流熱伝達の割合を設定する</summary>
        /// <param name="convectiveRate">対流熱伝達の割合</param>
        public void SetConvectiveRate(double convectiveRate)
        {
            foreach (WallSurface ws in wallSurfaces)
            {
                ws.ConvectiveRate = convectiveRate;
            }
        }*/

        /// <summary>表面の総合熱伝達率[W/(m^2-K)]を設定する</summary>
        /// <param name="filmCoefficient">表面の総合熱伝達率[W/(m^2-K)]</param>
        public void SetFilmCoefficient(double filmCoefficient)
        {
            foreach (WallSurface ws in wallSurfaces)
            {
                ws.FilmCoefficient = filmCoefficient;
            }
        }

        //外部風速の関数として実装すること!（放射対流成分比率関係なし???）
        //public void SetOverallHeatTransferCoefficientFromWindVelocity() { }

        #endregion

    }

    /// <summary>読み取り専用の屋外</summary>
    public interface ImmutableOutdoor
    {

        #region Properties

        /// <summary>外気条件を取得する</summary>
        ImmutableMoistAir AirState
        {
            get;
        }

        /// <summary>太陽を取得する</summary>
        ImmutableSun Sun
        {
            get;
        }

        /// <summary>壁表面リストを取得する</summary>
        ImmutableSurface[] WallSurfaces
        {
            get;
        }

        /// <summary>地中の壁表面リストを取得する</summary>
        ImmutableSurface[] GroundWallSurfaces
        {
            get;
        }

        /// <summary>窓リストを取得する</summary>
        ImmutableWindow[] Windows
        {
            get;
        }

        /// <summary>地中温度[C]を取得する</summary>
        double GroundTemperature
        {
            get;
        }

        /// <summary>夜間放射[W/m2]を取得する</summary>
        double NocturnalRadiation
        {
            get;
        }

        #endregion

    }

}
