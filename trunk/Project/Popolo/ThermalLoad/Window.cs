/* Window.cs
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

using Popolo.ThermophysicalProperty;
using Popolo.Weather;

namespace Popolo.ThermalLoad
{
    /// <summary>窓クラス</summary>
    public class Window : ImmutableWindow
    {

        #region event定義

        /// <summary>FIおよびFO変更イベント</summary>
        public event EventHandler FIOChangeEvent;

        /// <summary>面積変更イベント</summary>
        public event EventHandler AreaChangeEvent;

        #endregion

        #region Instance variables

        /// <summary>状態値変更フラグ</summary>
        private bool hasChanged = true;

        /// <summary>室外側表面</summary>
        private WindowSurface outsideSurface;

        /// <summary>室内側表面</summary>
        private WindowSurface insideSurface;

        /// <summary>太陽編集番号</summary>
        private uint sunRev = 0;

        /// <summary>ガラス層</summary>
        private GlassPanes glassPanes = new GlassPanes(0.7, 0.2, 6.4);

        /// <summary>屋外側の傾斜面</summary>
        private Incline incline = new Incline(Incline.Orientation.S, 0.5 * Math.PI);

        /// <summary>透過日射による熱取得[W]</summary>
        private double transmissionHeatGain = 0;

        /// <summary>吸収日射による熱取得[W]</summary>
        private double absorbedHeatGain = 0;

        /// <summary>温度差による貫流熱取得[W]</summary>
        private double transferHeatGain = 0;

        /// <summary>日影面積率[-]</summary>
        private double shadowRate = 0.0d;

        /// <summary>太陽オブジェクト</summary>
        private ImmutableSun sun;

        /// <summary>熱取得の内、放射成分[W]</summary>
        private double radiativeHeatGain;

        /// <summary>熱取得の内、対流成分[W]</summary>
        private double convectiveHeatGain;

        /// <summary>日除け</summary>
        private ImmutableSunShade sunShade = SunShade.EmptySunShade;

        /// <summary>窓面積[m2]</summary>
        private double surfaceArea = 1;

        /// <summary>夜間放射[W/m2]</summary>
        private double nocturnalRadiation = 0;

        /// <summary>室外側表面温度[C]</summary>
        private double outdoorSurfaceTemperature;

        /// <summary>室内側表面温度[C]</summary>
        private double indoorSurfaceTemperature;

        #endregion

        #region Properties

        /// <summary>名称を設定・取得する</summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>日影面積率[-]を設定・取得する</summary>
        public double ShadowRate
        {
            get
            {
                return shadowRate;
            }
            set
            {
                shadowRate = value;
                hasChanged = true;
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
                sun = value;
                hasChanged = true;
            }
        }

        /// <summary>屋外側の傾斜面情報を設定・取得する</summary>
        public ImmutableIncline OutSideIncline
        {
            get
            {
                return incline;
            }
            set
            {
                incline.Copy(value);
                hasChanged = true;
            }
        }

        /// <summary>透過日射による熱取得[W]を取得する</summary>
        public double TransmissionHeatGain
        {
            get
            {
                updateState();
                return transmissionHeatGain;
            }
        }

        /// <summary>吸収日射による熱取得[W]を取得する</summary>
        public double AbsorbedHeatGain
        {
            get
            {
                updateState();
                return absorbedHeatGain;
            }
        }

        /// <summary>温度差による貫流熱取得[W]を取得する</summary>
        public double TransferHeatGain
        {
            get
            {
                updateState();
                return transferHeatGain;
            }
        }

        /// <summary>熱取得の内、放射成分[W]を取得する</summary>
        public double RadiativeHeatGain
        {
            get
            {
                updateState();
                return radiativeHeatGain;
            }
        }

        /// <summary>熱取得の内、対流成分[W]を取得する</summary>
        public double ConvectiveHeatGain
        {
            get
            {
                updateState();
                return convectiveHeatGain;
            }
        }

        /// <summary>FIを取得する</summary>
        public double FI
        {
            get
            {
                return 1 - FO;
            }
        }

        /// <summary>FOを取得する</summary>
        public double FO
        {
            get;
            private set;
        }

        /// <summary>ガラスの特性を取得する</summary>
        public ImmutableGlassPanes Glass
        {
            get
            {
                return glassPanes;
            }
        }

        /// <summary>日除けを設定・取得する</summary>
        public ImmutableSunShade Shade
        {
            get
            {
                return sunShade;
            }
            set
            {
                sunShade = value;
                hasChanged = true;
            }
        }

        /// <summary>窓面積[m2]を設定・取得する</summary>
        public double SurfaceArea
        {
            get
            {
                return surfaceArea;
            }
            set
            {
                if (0 < value)
                {
                    surfaceArea = value;
                    hasChanged = true;
                    if (AreaChangeEvent != null) AreaChangeEvent(this, new EventArgs());
                }
            }
        }

        /// <summary>夜間放射[W/m2]を設定・取得する</summary>
        public double NocturnalRadiation
        {
            get
            {
                return nocturnalRadiation;
            }
            set
            {
                nocturnalRadiation = Math.Max(0, value);
                hasChanged = true;
            }
        }

        /// <summary>室外側表面温度[C]を取得する</summary>
        public double OutSideSurfaceTemperature
        {
            get
            {
                updateState();
                return outdoorSurfaceTemperature;
            }
        }

        /// <summary>室内側表面温度[C]を取得する</summary>
        public double InSideSurfaceTemperature
        {
            get
            {
                updateState();
                return indoorSurfaceTemperature;
            }
        }

        /// <summary>屋外の窓近傍の乾球温度[C]を設定・取得する</summary>
        public double OutdoorDrybulbTemperature
        {
            get
            {
                return outsideSurface.AirTemperature;
            }
            set
            {
                outsideSurface.AirTemperature = value;
            }
        }

        /// <summary>屋内の窓近傍の乾球温度[C]を設定・取得する</summary>
        public double IndoorDrybulbTemperature
        {
            get
            {
                return insideSurface.AirTemperature;
            }
            set
            {
                insideSurface.AirTemperature = value;
            }
        }

        /// <summary>熱貫流率[W/(m2-K)]を取得する</summary>
        public double HeatTransmissionCoefficient
        {
            get
            {
                return glassPanes.ThermalTransmittance;
            }
        }

        #endregion

        #region Constructor

        /// <summary>Constructor</summary>
        /// <param name="glassPanes">ガラス層</param>
        public Window(ImmutableGlassPanes glassPanes)
        {
            initialize(glassPanes, null, null, null);
        }

        /// <summary>Constructor</summary>
        /// <param name="glassPanes">ガラス層</param>
        /// <param name="incline">屋外側の傾斜面</param>
        public Window(ImmutableGlassPanes glassPanes, ImmutableIncline incline)
        {
            initialize(glassPanes, incline, null, null);
        }

        /// <summary>Constructor</summary>
        /// <param name="glassPanes">ガラス層</param>
        /// <param name="incline">屋外側の傾斜面情報</param>
        /// <param name="sunShade">日除け</param>
        public Window(ImmutableGlassPanes glassPanes, ImmutableIncline incline, ImmutableSunShade sunShade)
        {
            initialize(glassPanes, incline, sunShade, null);
        }

        /// <summary>Constructor</summary>
        /// <param name="glassPanes">ガラス層</param>
        /// <param name="name">窓名称</param>
        public Window(ImmutableGlassPanes glassPanes, string name)
        {
            initialize(glassPanes, null, null, name);
        }

        /// <summary>Constructor</summary>
        /// <param name="glassPanes">ガラス層</param>
        /// <param name="incline">屋外側の傾斜面</param>
        /// <param name="name">窓名称</param>
        public Window(ImmutableGlassPanes glassPanes, ImmutableIncline incline, string name)
        {
            initialize(glassPanes, incline, null, name);
        }

        /// <summary>Constructor</summary>
        /// <param name="glassPanes">ガラス層</param>
        /// <param name="incline">屋外側の傾斜面情報</param>
        /// <param name="sunShade">日除け</param>
        /// <param name="name">窓名称</param>
        public Window(ImmutableGlassPanes glassPanes, ImmutableIncline incline, ImmutableSunShade sunShade, string name)
        {
            initialize(glassPanes, incline, sunShade, name);
        }

        /// <summary>初期化する</summary>
        /// <param name="glassPanes">ガラス層</param>
        /// <param name="incline">屋外側の傾斜面情報</param>
        /// <param name="sunShade">日除け</param>
        /// <param name="name">窓名称</param>
        private void initialize(ImmutableGlassPanes glassPanes, ImmutableIncline incline, ImmutableSunShade sunShade, string name)
        {
            this.glassPanes.Copy(glassPanes);
            FO = glassPanes.ThermalTransmittance /
                    glassPanes.InsideFilmCoefficient;
            if(incline != null) this.incline.Copy(incline);
            if (sunShade != null)
            {
                SunShade ss = SunShade.EmptySunShade;
                ss.Copy(sunShade);
                this.sunShade = ss;
            }
            if(name != null) Name = name;
            hasChanged = true;
            makeSurface();
        }

        /// <summary>表面を作成する</summary>
        private void makeSurface()
        {
            outsideSurface = new WindowSurface(this, true);
            insideSurface = new WindowSurface(this, false);

            outsideSurface.FilmCoefficient = glassPanes.OutsideFilmCoefficient;
            insideSurface.FilmCoefficient = glassPanes.InsideFilmCoefficient;
        }

        #endregion

        #region public methods

        /// <summary>ガラス層を初期化する</summary>
        /// <param name="glassPanes">ガラス層</param>
        public void Initialize(ImmutableGlassPanes glassPanes)
        {
            this.glassPanes.Copy(glassPanes);
            FO = glassPanes.ThermalTransmittance /
                    glassPanes.InsideFilmCoefficient;

            if (FIOChangeEvent != null) FIOChangeEvent(this, new EventArgs());
        }

        /// <summary>窓表面を取得する</summary>
        /// <param name="isOutSide">外部か否か</param>
        /// <returns>窓表面</returns>
        public WindowSurface GetSurface(bool isOutSide)
        {
            if (isOutSide) return outsideSurface;
            else return insideSurface;
        }

        /// <summary>ガラスの標準入射角特性[-]を計算する</summary>
        /// <param name="cosineIncidentAngle">入射角の余弦（cosθ）</param>
        /// <returns>ガラスの標準入射角特性[-]</returns>
        public double GetStandardIncidentAngleCharacteristic(double cosineIncidentAngle)
        {
            return glassPanes.GetIncidentAngleCharacteristic(cosineIncidentAngle);
        }
        
        /// <summary>外表面総合熱伝達率[W/m2-K]を設定する</summary>
        /// <param name="outsideFilmCoefficient">外表面総合熱伝達率[W/m2-K]</param>
        internal void setOutsideFilmCoefficient(double outsideFilmCoefficient)
        {
            glassPanes.SetOutsideFilmCoefficient(outsideFilmCoefficient);
            FO = glassPanes.ThermalTransmittance /
                    glassPanes.InsideFilmCoefficient;
        }

        /// <summary>内表面総合熱伝達率[W/m2-K]を設定する</summary>
        /// <param name="insideFilmCoefficient">内表面総合熱伝達率[W/m2-K]</param>
        internal void setInsideFilmCoefficient(double insideFilmCoefficient)
        {
            glassPanes.SetInsideFilmCoefficient(insideFilmCoefficient);
            FO = glassPanes.ThermalTransmittance /
                    glassPanes.InsideFilmCoefficient;
        }

        /// <summary>放射[W/m2]を考慮した相当温度[C]を計算する</summary>
        /// <param name="isOutside">外壁か否か</param>
        /// <returns>相当温度[C]</returns>
        public double GetSolAirTemperature(bool isOutside)
        {
            if (isOutside) return outsideSurface.GetSolAirTemperature();
            else return insideSurface.GetSolAirTemperature();
        }
        
        #endregion

        #region private methods

        /// <summary>状態を更新する</summary>
        private void updateState()
        {
            if (!hasChanged && 
                (sunRev == sun.Revision)) return;

            if (sunShade != SunShade.EmptySunShade)
            {
                shadowRate = sunShade.GetShadowRate(sun);
            }

            //係数を計算
            double cosineDN = incline.GetDirectSolarRadiationRate(sun);
            if (cosineDN < 0.01) cosineDN = 0.01;
            double idn = cosineDN * sun.DirectNormalRadiation;
            double id = incline.ConfigurationFactorToSky * sun.DiffuseHorizontalRadiation +
                (1 - incline.ConfigurationFactorToSky) * outsideSurface.Albedo * sun.GlobalHorizontalRadiation;
            double charac = GetStandardIncidentAngleCharacteristic(cosineDN);
            double buff = SurfaceArea * ((1d - shadowRate) * idn * charac + 0.91 * id);

            //透過日射による熱取得[W]を更新
            transmissionHeatGain = glassPanes.OverallTransmissivity * buff;

            //吸収日射による熱取得[W]
            absorbedHeatGain = glassPanes.OverallAbsorptivity * buff;

            //外表面の放射を設定
            outsideSurface.Radiation = absorbedHeatGain / surfaceArea / glassPanes.ThermalTransmittance * outsideSurface.FilmCoefficient
                - outsideSurface.LongWaveEmissivity * incline.ConfigurationFactorToSky * NocturnalRadiation;

            //温度差による貫流熱取得[W]を計算
            double insideSAT = GetSolAirTemperature(false);
            double outsideSAT = GetSolAirTemperature(true);
            transferHeatGain = surfaceArea * glassPanes.ThermalTransmittance * (outsideSAT - insideSAT);

            //対流・放射成分に分ける//吸収日射熱取得が二重にかかってないか？要確認2011.06.16
            double at = absorbedHeatGain + transferHeatGain;
            convectiveHeatGain = at * insideSurface.ConvectiveRate;// glassPanes.ConvectiveRate;
            radiativeHeatGain = at * insideSurface.RadiativeRate;// glassPanes.RadiativeRate;

            //表面温度を計算
            outdoorSurfaceTemperature = FI * outsideSAT + FO * insideSAT;
            indoorSurfaceTemperature = FI * insideSAT + FO * outsideSAT;

            hasChanged = false;
            sunRev = sun.Revision;
        }

        #endregion

    }

    #region 読み取り専用の窓

    /// <summary>読み取り専用の窓</summary>
    public interface ImmutableWindow
    {
        #region Properties

        /// <summary>名称を取得する</summary>
        string Name
        {
            get;
        }

        /// <summary>日影面積率[-]を取得する</summary>
        double ShadowRate
        {
            get;
        }

        /// <summary>太陽を取得する</summary>
        ImmutableSun Sun
        {
            get;
        }

        /// <summary>屋外側傾斜面情報を取得する</summary>
        ImmutableIncline OutSideIncline
        {
            get;
        }

        /// <summary>透過日射による熱取得[W]を取得する</summary>
        double TransmissionHeatGain
        {
            get;
        }

        /// <summary>吸収日射による熱取得[W]を取得する</summary>
        double AbsorbedHeatGain
        {
            get;
        }

        /// <summary>温度差による貫流熱取得[W]を取得する</summary>
        double TransferHeatGain
        {
            get;
        }

        /// <summary>熱取得の内、放射成分[W]を取得する</summary>
        double RadiativeHeatGain
        {
            get;
        }

        /// <summary>熱取得の内、対流成分[W]を取得する</summary>
        double ConvectiveHeatGain
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

        /*/// <summary>複層ガラスを取得する</summary>
        ImmutableGlassPanes Glass
        {
            get;
        }*/

        /// <summary>日除けを取得する</summary>
        ImmutableSunShade Shade
        {
            get;
        }

        /// <summary>窓面積[m2]を取得する</summary>
        double SurfaceArea
        {
            get;
        }

        /// <summary>夜間放射[W/m2]を取得する</summary>
        double NocturnalRadiation
        {
            get;
        }

        /// <summary>室外側表面温度[C]を取得する</summary>
        double OutSideSurfaceTemperature
        {
            get;
        }

        /// <summary>室内側表面温度[C]を取得する</summary>
        double InSideSurfaceTemperature
        {
            get;
        }

        /// <summary>屋外の窓近傍の乾球温度[C]を取得する</summary>
        double OutdoorDrybulbTemperature
        {
            get;
        }

        /// <summary>屋内の窓近傍の乾球温度[C]を取得する</summary>
        double IndoorDrybulbTemperature
        {
            get;
        }

        /// <summary>熱貫流率[W/(m2-K)]を取得する</summary>
        double HeatTransmissionCoefficient
        {
            get;
        }

        /// <summary>窓表面を取得する</summary>
        /// <param name="isOutSide">外部か否か</param>
        /// <returns>窓表面</returns>
        WindowSurface GetSurface(bool isOutSide);

        #endregion
    }
    
    #endregion

}
