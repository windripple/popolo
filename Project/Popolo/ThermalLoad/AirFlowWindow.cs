/* AirFlowWindow.cs
 *
 * Copyright (C) 2010 E.Togashi
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

using System;

using Popolo.ThermophysicalProperty;
using Popolo.Weather;
using Popolo.Numerics;

namespace Popolo.ThermalLoad
{
    /// <summary>Air flow window class</summary>
    public class AirFlowWindow
    {

        #region constant

        private readonly double CPA = MoistAir.GetSpecificHeat(0.015);

        #endregion

        #region instance variables

        /// <summary>has boundary conditions related to matrix changed or not.</summary>
        private bool hasMatrixBoundaryChanged = true;

        /// <summary>has boundary conditions not related to matrix changed or not.</summary>
        private bool hasBoundaryChanged = true;

        /// <summary>Inlet air temperature of air flow window.</summary>
        private double inletAirTemperature;

        /// <summary>glass pane of interior side[m]</summary>
        private GlassPanes.Pane interiorGlassPane;

        /// <summary>glass pnae of exterior side[m]</summary>
        private GlassPanes.Pane exteriorGlassPane;

        /// <summary></summary>
        private double interiorSideAverageTemperature = 25;

        /// <summary></summary>
        private double exteriorSideAverageTemperature = 25;

        /// <summary>Interior side film coefficient [W/(m2K)]</summary>
        private double interiorFilmCoefficient = 9.3d;

        /// <summary>Exterior side film coefficient [W/(m2K)]</summary>
        private double exteriorFilmCoefficient = 21.0d;

        /// <summary>Solar absorption [-] at exterior glass</summary>
        private double solarAbsorptionRateAtExteriorGlass = 0;

        /// <summary>Solar absorption [-] at blind</summary>
        private double solarAbsorptionRateAtBlind = 0;

        /// <summary>Solar absorption [-] at interior glass</summary>
        private double solarAbsorptionRateAtInteriorGlass = 0;

        /// <summary></summary>
        private double wi3, we3;

        /// <summary>inside temperature[C]</summary>
        private double itemp;

        /// <summary>outside temperature[C]</summary>
        private double otemp;

        /// <summary>convective heat transfer coefficient [W/(m2K)] at interior air gap</summary>
        private double interiorAirGapConvectiveHeatTransferCoefficient;

        /// <summary>convective heat transfer coefficient [W/(m2K)] at exterior air gap</summary>
        private double exteriorAirGapConvectiveHeatTransferCoefficient;

        /// <summary>radiative heat transfer coefficient [W/(m2K)] at interior air gap</summary>
        private double interiorAirGapRadiativeHeatTransferCoefficient;

        /// <summary>radiative heat transfer coefficient [W/(m2K)] at exterior air gap</summary>
        private double exteriorAirGapRadiativeHeatTransferCoefficient;

        /// <summary>sun</summary>
        private ImmutableSun sun;

        #endregion

        #region properties

        /// <summary>Incline of exterior side.</summary>
        public ImmutableIncline OutSideIncline
        {
            get;
            private set;
        }

        /// <summary>Gets or Sets temperature [C] around interior glass.</summary>
        public double IndoorTemperature
        {
            get
            {
                return itemp;
            }
            set
            {
                hasBoundaryChanged = true;
                itemp = value;
            }
        }

        /// <summary>Gets or Sets temperature [C] around exterior glass.</summary>
        public double OutdoorTemperature
        {
            get
            {
                return otemp;
            }
            set
            {
                hasBoundaryChanged = true;
                otemp = value;
            }
        }

        /// <summary>Gets the interior side air gap thickness [m]</summary>
        public double InteriorAirGap
        {
            private set;
            get;
        }

        /// <summary>Gets the exterior side air gap thickness [m]</summary>
        public double ExteriorAirGap
        {
            private set;
            get;
        }

        /// <summary>Gets the width [m] of the air flow window</summary>
        public double WindowWidth
        {
            private set;
            get;
        }

        /// <summary>Gets the height [m] of the air flow window</summary>
        public double WindowHeight
        {
            private set;
            get;
        }

        /// <summary>Gets the air flow volume [CMH] at interior side.</summary>
        public double InteriorSideAirFlowVolume
        {
            private set;
            get;
        }

        /// <summary>Gets the air flow volume [CMH] at exterior side.</summary>
        public double ExteriorSideAirFlowVolume
        {
            private set;
            get;
        }

        /// <summary>Gets the air flow volume [m/s] at interior side.</summary>
        public double InteriorSideAirVelocity
        {
            private set;
            get;
        }

        /// <summary>Gets the air flow volume [m/s] at exterior side.</summary>
        public double ExteriorSideAirVelocity
        {
            private set;
            get;
        }

        /// <summary>Sets and Gets interior side film coefficient [W/(m2K)].</summary>
        public double InteriorSideFilmCoefficient
        {
            get
            {
                return interiorFilmCoefficient;
            }
            set
            {
                if (0 < value)
                {
                    interiorFilmCoefficient = value;
                    hasMatrixBoundaryChanged = true;
                }
            }
        }

        /// <summary>Sets and Gets exterior side film coefficient [W/(m2K)].</summary>
        public double ExteriorSideFilmCoefficient
        {
            get
            {
                return exteriorFilmCoefficient;
            }
            set
            {
                if (0 < value)
                {
                    exteriorFilmCoefficient = value;
                    hasMatrixBoundaryChanged = true;
                }
            }
        }

        /// <summary>Sets or Gets sun.</summary>
        public ImmutableSun Sun
        {
            get
            {
                return sun;
            }
            set
            {
                sun = value;
                hasBoundaryChanged = true;
            }
        }

        /// <summary>Gets the nocturnal radiation [W/m2]</summary>
        public double NocturnalRadiation
        {
            get;
            private set;
        }

        #endregion

        #region instance variables for numeric calculation

        /// <summary>Matrix A</summary>
        private Matrix aMatrix = new Matrix(3, 3);

        /// <summary>Inverse Matrix B</summary>
        private Matrix bMatrix = new Matrix(3, 3);

        /// <summary>Matrix T</summary>
        private Vector tVector = new Vector(3);

        /// <summary>Matrix Z</summary>
        private Vector zVector = new Vector(3);

        #endregion

        #region constructor

        /// <summary>Constructor</summary>
        /// <param name="interiorGlassPane">glass pane of interior side</param>
        /// <param name="interiorAirGap">interior side air gap thickness[m]</param>
        /// <param name="exteriorGlassPane">glass pnae of exterior side</param>
        /// <param name="exteriorAirGap">exterior side air gap thickness[m]</param>
        /// <param name="windowWidth">width [m] of the air flow window</param>
        /// <param name="windowHeight">height [m] of the air flow window</param>
        /// <param name="outsideIncline"></param>
        public AirFlowWindow(GlassPanes.Pane interiorGlassPane, double interiorAirGap,
            GlassPanes.Pane exteriorGlassPane, double exteriorAirGap,double windowWidth, double windowHeight,
            ImmutableIncline outsideIncline)
        {
            //initialize temperatures.
            tVector.SetValue(25);

            this.interiorGlassPane = interiorGlassPane;
            this.exteriorGlassPane = exteriorGlassPane;
            this.ExteriorAirGap = exteriorAirGap;
            this.InteriorAirGap = interiorAirGap;
            this.WindowWidth = windowWidth;
            this.WindowHeight = windowHeight;

            SetBlind(0.75, 0.07);

            this.OutSideIncline = outsideIncline;
        }

        #endregion

        #region public methods (Setting boundary conditions)

        /// <summary>Set nocturnal radiation[W/m2] at window surface</summary>
        /// <param name="nocturnalRadiation">nocturnal radiation[W/m2] at window surface</param>
        public void SetNocturnalRadiation(double nocturnalRadiation)
        {
            if (this.NocturnalRadiation != nocturnalRadiation)
            {
                this.NocturnalRadiation = nocturnalRadiation;
                hasBoundaryChanged = true;
            }
        }

        /// <summary>Set inlet air temperature of air flow window.</summary>
        /// <param name="inletAirTemperature">temperature of inlet air</param>
        public void SetInletAirTemperature(double inletAirTemperature)
        {
            if (this.inletAirTemperature != inletAirTemperature)
            {
                this.inletAirTemperature = inletAirTemperature;
                hasMatrixBoundaryChanged = true;
            }
        }

        /// <summary>Sets the air flow volume [CMH].</summary>
        /// <param name="airFlowVolume">The air flow volume [CMH].</param>
        public void SetAirFlowVolume(double airFlowVolume)
        {
            double iaf =  InteriorAirGap / (InteriorAirGap + ExteriorAirGap) * airFlowVolume;
            SetAirFlowVolume(iaf, airFlowVolume - iaf);
        }

        /// <summary>Sets the air flow volume [CMH].</summary>
        /// <param name="interiorSideAirFlowVolume">The air flow volume [CMH] at interior side</param>
        /// <param name="exteriorSideAirFlowVolume">The air flow volume [CMH] at exterior side</param>
        public void SetAirFlowVolume(double interiorSideAirFlowVolume, double exteriorSideAirFlowVolume)
        {
            this.InteriorSideAirFlowVolume = interiorSideAirFlowVolume;
            this.ExteriorSideAirFlowVolume = exteriorSideAirFlowVolume;

            InteriorSideAirVelocity = InteriorSideAirFlowVolume / (InteriorAirGap * WindowWidth) / 3600d;   //interior side velocity [m/s]
            ExteriorSideAirVelocity = ExteriorSideAirFlowVolume / (ExteriorAirGap * WindowWidth) / 3600d;   //exterior side velocity [m/s]

            hasMatrixBoundaryChanged = true;
        }

        /// <summary>Sets the transmittance, reflectance and absorptance of the blind</summary>
        /// <param name="transmittance">transmittance of the blind</param>
        /// <param name="reflectance">reflectance of the blind</param>
        public void SetBlind(double transmittance, double reflectance)
        {
            double xr;
            double absorptance = 1d - transmittance - reflectance;

            if (transmittance < 0 || reflectance < 0 || absorptance < 0) throw new Exception("Transmittance, Reflectance and Absorptance must take value between 0 to 1");

            //calculate solar absorptance
            double overallTransmittance = interiorGlassPane.OuterSideTransmissivity;
            double overallReflectance = interiorGlassPane.OuterSideReflectivity;
            solarAbsorptionRateAtInteriorGlass = interiorGlassPane.OuterSideAbsorptivity;
            xr = transmittance / (1d - reflectance * overallReflectance);
            solarAbsorptionRateAtInteriorGlass *= xr;
            solarAbsorptionRateAtBlind = absorptance + absorptance * overallReflectance * xr;
            overallReflectance = reflectance + transmittance * overallReflectance * xr;
            overallTransmittance *= xr;
            //
            xr = exteriorGlassPane.OuterSideTransmissivity / (1d - exteriorGlassPane.InnerSideReflectivity * overallReflectance);
            solarAbsorptionRateAtInteriorGlass *= xr;
            solarAbsorptionRateAtBlind *= xr;
            solarAbsorptionRateAtExteriorGlass = exteriorGlassPane.OuterSideAbsorptivity + exteriorGlassPane.InnerSideAbsorptivity * overallReflectance * xr;
            overallReflectance = exteriorGlassPane.OuterSideReflectivity + exteriorGlassPane.InnerSideTransmissivity * overallReflectance * xr;
            overallTransmittance *= xr;            
            
            hasMatrixBoundaryChanged = true;
        }

        #endregion

        #region public methods (Getting state)

        /// <summary>Gets temperature[C] of the blind</summary>
        /// <returns>temperature[C] of the blind</returns>
        public double GetBlindTemperature()
        {
            //update matirx if boundary conditions have changed
            updateMatrix();
            //update state
            updateState();

            return tVector.GetValue(1);
        }

        /// <summary>Gets temperature[C] of the interior glass</summary>
        /// <returns>temperature[C] of the interior glass</returns>
        public double GetInteriorGlassTemperature()
        {
            //update matirx if boundary conditions have changed
            updateMatrix();
            //update state
            updateState();

            return tVector.GetValue(2);
        }

        /// <summary>Gets temperature[C] of the exterior glass</summary>
        /// <returns>temperature[C] of the exterior glass</returns>
        public double GetExteriorGlassTemperature()
        {
            //update matirx if boundary conditions have changed
            updateMatrix();
            //update state
            updateState();

            return tVector.GetValue(0);
        }

        /// <summary>Gets the average temperature[C] of exterior air flow</summary>
        /// <returns>average temperature[C] of exterior air flow</returns>
        public double GetExteriorAirFlowTemperature()
        {
            //update matirx if boundary conditions have changed
            updateMatrix();
            //update state
            updateState();

            if (ExteriorSideAirVelocity <= 0) return (tVector.GetValue(0) + tVector.GetValue(1)) * 0.5;
            else return (tVector.GetValue(0) + tVector.GetValue(1)) * 0.5 * (1 - we3) + we3 * inletAirTemperature;
        }

        /// <summary>Gets the average temperature[C] of interior air flow</summary>
        /// <returns>average temperature[C] of interior air flow</returns>
        public double GetInteriorAirFlowTemperature()
        {
            //update matirx if boundary conditions have changed
            updateMatrix();
            //update state
            updateState();

            if (InteriorSideAirVelocity <= 0) return (tVector.GetValue(1) + tVector.GetValue(2)) * 0.5;
            else return (tVector.GetValue(1) + tVector.GetValue(2)) * 0.5 * (1 - wi3) + wi3 * inletAirTemperature;
        }

        /// <summary>Gets the convective heat transfer coefficient [W/(m2K)] at interior air gap.</summary>
        public double GetInteriorAirGapConvectiveHeatTransferCoefficient()
        {
            //update matirx if boundary conditions have changed
            updateMatrix();

            return interiorAirGapConvectiveHeatTransferCoefficient;
        }

        /// <summary>Gets the convective heat transfer coefficient [W/(m2K)] at exterior air gap.</summary>
        public double GetExteriorAirGapConvectiveHeatTransferCoefficient()
        {
            //update matirx if boundary conditions have changed
            updateMatrix();

            return exteriorAirGapConvectiveHeatTransferCoefficient;
        }

        /// <summary>Gets the radiative heat transfer coefficient [W/(m2K)] at interior air gap.</summary>
        public double GetInteriorAirGapRadiativeHeatTransferCoefficient()
        {
            //update matirx if boundary conditions have changed
            updateMatrix();

            return interiorAirGapRadiativeHeatTransferCoefficient;
        }

        /// <summary>Gets the radiative heat transfer coefficient [W/(m2K)] at exterior air gap.</summary>
        public double GetExteriorAirGapRadiativeHeatTransferCoefficient()
        {
            //update matirx if boundary conditions have changed
            updateMatrix();

            return exteriorAirGapRadiativeHeatTransferCoefficient;
        }

        /// <summary>Gets the outlet air flow temperature [C]</summary>
        /// <returns>outlet air flow temperature [C]</returns>
        public double GetOutletAirTemperature()
        {
            //update matirx if boundary conditions have changed
            updateMatrix();
            //update state
            updateState();

            double teo, tio;
            double dens = 1.293 / (1 + inletAirTemperature / 273.15);
            if (ExteriorSideAirFlowVolume <= 0) teo = 0;
            else
            {
                double tes = 0.5 * (tVector.GetValue(0) + tVector.GetValue(1));
                double whe = (2 * exteriorAirGapConvectiveHeatTransferCoefficient) / (CPA * ExteriorSideAirFlowVolume / 3.6d * dens);
                double eps = 1 - Math.Exp(-whe);
                teo = tes - (tes - inletAirTemperature) * Math.Exp(-whe);
            }
            if (InteriorSideAirFlowVolume <= 0) tio = 0;
            else
            {
                double tis = 0.5 * (tVector.GetValue(1) + tVector.GetValue(2));
                double whi = (2 * interiorAirGapConvectiveHeatTransferCoefficient) / (CPA * InteriorSideAirFlowVolume / 3.6d * dens);
                double eps = 1 - Math.Exp(-whi);
                tio = tis - (tis - inletAirTemperature) * Math.Exp(-whi);
            }

            double eRate = ExteriorSideAirFlowVolume / (ExteriorSideAirFlowVolume + InteriorSideAirFlowVolume);
            return teo * eRate + tio * (1 - eRate);
        }

        /// <summary>Gets heat removal by airflow [W/m2]</summary>
        /// <returns>heat removal by airflow [W/m2]</returns>
        public double GetHeatRemovalByAirFlow()
        {
            double outT = GetOutletAirTemperature();
            double dens = 1.293 / (1 + inletAirTemperature / 273.15);

            return (outT - inletAirTemperature) * (ExteriorSideAirFlowVolume + InteriorSideAirFlowVolume) * dens * CPA / 3.6;
        }

        #endregion

        #region private methods

        private void updateState()
        {
            if (!hasBoundaryChanged) return;

            //Update solar absorptions**********************************
            //if (sunRev == sun.Revision) return;

            //debug
            double albedo = 0.5;
            double shadowRate = 0;
            double emissivity = 0.9;
            //debug

            //Calculate coefficients for glass
            double cosineDN = OutSideIncline.GetDirectSolarRadiationRate(sun);
            if (cosineDN < 0.01) cosineDN = 0.01;
            double idn = cosineDN * sun.DirectNormalRadiation;
            double id = OutSideIncline.ConfigurationFactorToSky * sun.DiffuseHorizontalRadiation +
                (1 - OutSideIncline.ConfigurationFactorToSky) * albedo * sun.GlobalHorizontalRadiation;
            double charac = GetStandardIncidentAngleCharacteristic(cosineDN);
            double radiation = (1d - shadowRate) * idn * charac + 0.91 * id;

            //
            double od = OutdoorTemperature - NocturnalRadiation * emissivity * OutSideIncline.ConfigurationFactorToSky / ExteriorSideFilmCoefficient;

            //Make zVector**********************************
            double kiw = 1 / (1 / interiorGlassPane.HeatTransferCoefficient + 1 / interiorFilmCoefficient);
            double kew = 1 / (1 / exteriorGlassPane.HeatTransferCoefficient + 1 / exteriorFilmCoefficient);
            zVector.SetValue(0, solarAbsorptionRateAtExteriorGlass * radiation 
                // - NocturnalRadiation * emissivity * OutSideIncline.ConfigurationFactorToSky +
                +kew * od + exteriorAirGapConvectiveHeatTransferCoefficient * we3 * inletAirTemperature);
            zVector.SetValue(1, solarAbsorptionRateAtBlind * radiation + 
                exteriorAirGapConvectiveHeatTransferCoefficient * we3 * inletAirTemperature + interiorAirGapConvectiveHeatTransferCoefficient * wi3 * inletAirTemperature);
            zVector.SetValue(2, solarAbsorptionRateAtInteriorGlass * radiation + 
                kiw * IndoorTemperature + interiorAirGapConvectiveHeatTransferCoefficient * wi3 * inletAirTemperature);

            //Blas.DGemv(Blas.TransposeType.NoTranspose, 1, bMatrix, zVector, 0, ref tVector);
            bMatrix.VectorProduct(zVector, ref tVector, 1, 0);

            hasBoundaryChanged = false;

            //DEBUG
            double sum1 = solarAbsorptionRateAtExteriorGlass * radiation +
                exteriorAirGapConvectiveHeatTransferCoefficient * (GetExteriorAirFlowTemperature() - tVector.GetValue(0)) +
                exteriorAirGapRadiativeHeatTransferCoefficient * (tVector.GetValue(1) - tVector.GetValue(0)) +
                kew * (OutdoorTemperature - tVector.GetValue(0));
            double sum2 = solarAbsorptionRateAtBlind * radiation +
                exteriorAirGapConvectiveHeatTransferCoefficient * (GetExteriorAirFlowTemperature() - tVector.GetValue(1)) +
                exteriorAirGapRadiativeHeatTransferCoefficient * (tVector.GetValue(0) - tVector.GetValue(1)) +
                interiorAirGapConvectiveHeatTransferCoefficient * (GetInteriorAirFlowTemperature() - tVector.GetValue(1)) +
                interiorAirGapRadiativeHeatTransferCoefficient * (tVector.GetValue(2) - tVector.GetValue(1));
            double sum3 = solarAbsorptionRateAtInteriorGlass * radiation +
                interiorAirGapConvectiveHeatTransferCoefficient * (GetInteriorAirFlowTemperature() - tVector.GetValue(2)) +
                interiorAirGapRadiativeHeatTransferCoefficient * (tVector.GetValue(1) - tVector.GetValue(2)) +
                kiw * (IndoorTemperature - tVector.GetValue(2));
        }

        /// <summary>Update the matrix</summary>
        private void updateMatrix()
        {
            if (!hasMatrixBoundaryChanged) return;

            //Update heat transfer coefficients
            updateHeatTransferCoefficientAirGap();
            
            //Update inverse matrix***************************************
            double dens = 1.293 / (1 + inletAirTemperature / 273.15);
            //interior side
            double whi, epwi, wi1;
            if (InteriorSideAirFlowVolume <= 0)
            {
                whi = 0;
                epwi = 0;
                wi1 = 0.5;
                wi3 = 0;
            }
            else
            {
                whi = (2 * interiorAirGapConvectiveHeatTransferCoefficient) / (CPA * InteriorSideAirFlowVolume / 3.6d * dens);
                epwi = 1 - Math.Exp(-whi);
                wi1 = (1 - epwi / whi) * 0.5;
                wi3 = epwi / whi;
            }
            double kiw = 1 / (1 / interiorGlassPane.HeatTransferCoefficient + 1 / interiorFilmCoefficient);
            //exterior side
            double whe, epwe, we1;
            if (ExteriorSideAirFlowVolume <= 0)
            {
                whe = 0;
                epwe = 0;
                we1 = 0.5;
                we3 = 0;
            }
            else
            {
                whe = (2 * exteriorAirGapConvectiveHeatTransferCoefficient) / (CPA * ExteriorSideAirFlowVolume / 3.6d * dens);
                epwe = 1 - Math.Exp(-whe);
                we1 = (1 - epwe / whe) * 0.5;
                we3 = epwe / whe;
            }
            double kew = 1 / (1 / exteriorGlassPane.HeatTransferCoefficient + 1 / exteriorFilmCoefficient);

            //make matrix A
            aMatrix.SetValue(0, 0, exteriorAirGapConvectiveHeatTransferCoefficient * (1 - we1) + exteriorAirGapRadiativeHeatTransferCoefficient + kew);
            aMatrix.SetValue(0, 1, -(exteriorAirGapConvectiveHeatTransferCoefficient * we1 + exteriorAirGapRadiativeHeatTransferCoefficient));
            aMatrix.SetValue(0, 2, 0);
            aMatrix.SetValue(1, 0, -(exteriorAirGapRadiativeHeatTransferCoefficient + exteriorAirGapConvectiveHeatTransferCoefficient * we1));
            aMatrix.SetValue(1, 1, exteriorAirGapConvectiveHeatTransferCoefficient * (1 - we1) + exteriorAirGapRadiativeHeatTransferCoefficient +
                interiorAirGapConvectiveHeatTransferCoefficient * (1 - wi1) + interiorAirGapRadiativeHeatTransferCoefficient);
            aMatrix.SetValue(1, 2, -(interiorAirGapRadiativeHeatTransferCoefficient + interiorAirGapConvectiveHeatTransferCoefficient * wi1));
            aMatrix.SetValue(2, 0, 0);
            aMatrix.SetValue(2, 1, -(interiorAirGapConvectiveHeatTransferCoefficient * wi1 + interiorAirGapRadiativeHeatTransferCoefficient));
            aMatrix.SetValue(2, 2, interiorAirGapConvectiveHeatTransferCoefficient * (1 - wi1) + interiorAirGapRadiativeHeatTransferCoefficient + kiw);

            //calculate inverse matrix A- with LU decomposition
            aMatrix.GetInverse(ref bMatrix);

            hasMatrixBoundaryChanged = false;
            hasBoundaryChanged = true;
        }

        /// <summary>update overall heat transfer coefficient of air gaps</summary>
        private void updateHeatTransferCoefficientAirGap()
        {
            //calculate convective heat transfer coefficients
            interiorAirGapConvectiveHeatTransferCoefficient = getConvectiveHeatTransferCoefficient(tVector.GetValue(2), interiorSideAverageTemperature, InteriorSideAirVelocity);
            exteriorAirGapConvectiveHeatTransferCoefficient = getConvectiveHeatTransferCoefficient(tVector.GetValue(0), exteriorSideAverageTemperature, ExteriorSideAirVelocity);
            
            //calculate radiative heat transfer coefficients
            interiorAirGapRadiativeHeatTransferCoefficient = (4 * 5.67e-8) / (1 / 0.9 + 1 / 0.9 - 1) * Math.Pow((tVector.GetValue(1) + tVector.GetValue(2)) / 2d + 273.15, 3);
            exteriorAirGapRadiativeHeatTransferCoefficient = (4 * 5.67e-8) / (1 / 0.9 + 1 / 0.9 - 1) * Math.Pow((tVector.GetValue(0) + tVector.GetValue(1)) / 2d + 273.15, 3);
        }

        /// <summary>Calculate a convective heat transfer coefficient</summary>
        /// <param name="surfaceTemperature">temperature of surface (blind or glass)</param>
        /// <param name="airTemperature">temperature of air</param>
        /// <param name="airVelocity">velocity of air</param>
        /// <returns>convective heat transfer coefficient</returns>
        private double getConvectiveHeatTransferCoefficient(double surfaceTemperature, double airTemperature, double airVelocity)
        {
            //return 11.2 * airVelocity - 0.05;//非常に良い近似結果

            double aveTemp = (surfaceTemperature + airTemperature) * 0.5;

            //Thermodynamic properties of air (interior side).
            double dvis = MoistAir.GetDynamicViscosity(aveTemp);    //Dynamic Viscosity [m2/s]
            double tcd = MoistAir.GetThermalConductivity(aveTemp);  //Thermal conductivity [W/(mK)]
            double des = 1.293 / (1 + aveTemp / 273.15);            //Density [kg/m3]
            double tds = tcd / des / CPA / 1000;                    //Thermal diffusivity [m2/s]
            double exc = MoistAir.GetExpansionCoefficient(aveTemp); //Expansion coefficient [1/K]

            //Calculate Prandtl number
            double prtl = dvis / tds;

            //natural convection
            if (airVelocity <= 0)
            {
                //Calculate Grashof number
                double dt = Math.Abs(surfaceTemperature - airTemperature);
                double grs = (9.8 * Math.Pow(des, 2) * exc * dt * Math.Pow(WindowHeight, 3)) / Math.Pow(dvis, 2);

                //Calculate convective heat transfer coefficients [W/(m2 K)]
                double grpr = grs * prtl;
                if (grpr < 1e9) return 0.56 * Math.Pow(grpr, 0.25) * tcd / WindowHeight;
                else return 0.13 * Math.Pow(grpr, 1 / 3d) * tcd / WindowHeight;
            }
            //forced convection
            else
            {
                //Calculate Reynolds number
                double rey = airVelocity * WindowHeight / dvis;

                //Calculate convective heat transfer coefficients [W/(m2 K)]
                if (rey < 500000) return 0.664 * Math.Pow(prtl, 1d / 3d) * Math.Pow(rey, 0.5) * tcd / WindowHeight;
                else return 0.037 * Math.Pow(prtl, 1d / 3d) * Math.Pow(rey, 0.8) * tcd / WindowHeight;
            }
        }

        /// <summary>ガラスの標準入射角特性[-]を計算する</summary>
        /// <param name="cosineIncidentAngle">入射角の余弦（cosθ）</param>
        /// <returns>ガラスの標準入射角特性[-]</returns>
        public double GetStandardIncidentAngleCharacteristic(double cosineIncidentAngle)
        {
            double[] angularDependenceCoefficients = new double[] { 3.4167, -4.389, 2.4948, -0.5224 };
            double ci = cosineIncidentAngle;
            double val = 0;
            for (int i = angularDependenceCoefficients.Length - 1; 0 <= i; i--)
            {
                val = ci * (val + angularDependenceCoefficients[i]);
            }
            return Math.Max(0, Math.Min(1, val));
        }

        #endregion

    }
}
