using System;
using System.Collections.Generic;
using System.Text;

using Popolo.ThermophysicalProperty;

namespace SamplePrograms
{
    class Program
    {
        static void Main(string[] args)
        {

            sample2_2();

        }

        #region Chapter 1

        private static void sample1()
        {
            double cpAir = MoistAir.GetSpecificHeat(0.018);
            Console.WriteLine("Specific heat of moist air at absolute humidity of 0.018 kg/kg(DA) is" + cpAir.ToString("F3") + "kJ/K");
            Console.Read();
        }

        #endregion

        #region Chapter 2

        private static void sample2_1()
        {
            //Creating instance of MoistAir class
            MoistAir mAir = new MoistAir();

            //Set values to property
            mAir.DryBulbTemperature = 25.6;
            mAir.AbsoluteHumidity = 0.018;
            mAir.RelativeHumidity = 50.0;
            mAir.WetBulbTemperature = 22;
            mAir.SpecificVolume = 0.86;
            mAir.Enthalpy = 58.0;
            mAir.AtmosphericPressure = 101.325;

            //Output values of properties
            Console.WriteLine("Drybulb Temperature:" + mAir.DryBulbTemperature);
            Console.WriteLine("Absolute Humidity:" + mAir.AbsoluteHumidity);
            Console.WriteLine("Relative Humidity:" + mAir.RelativeHumidity);
            Console.WriteLine("Wetbulb Temperature:" + mAir.WetBulbTemperature);
            Console.WriteLine("Specific Volume:" + mAir.SpecificVolume);
            Console.WriteLine("Enthalpy:" + mAir.Enthalpy);
            Console.WriteLine("Atmospheric Pressure:" + mAir.AtmosphericPressure);

            Console.Read();
        }

        private static void sample2_2()
        {
            //Create an instance of the MoistAIr class
            MoistAir mAir;

            //Calculate state of the moist air from given two properties (DB 25 °C, AH 0.012 kg/kg)
            mAir = MoistAir.GetAirStateFromDBAH(25, 0.012);

            //Write value of the moist air to standard output stream.
            Console.WriteLine("Dry bulb temperature:" + mAir.DryBulbTemperature.ToString("F1"));
            Console.WriteLine("Absolute humidity:" + mAir.AbsoluteHumidity.ToString("F3"));
            Console.WriteLine("Relative humidity:" + mAir.RelativeHumidity.ToString("F1"));
            Console.WriteLine("Wet bulb temperature:" + mAir.WetBulbTemperature.ToString("F1"));
            Console.WriteLine("Specific volume:" + mAir.SpecificVolume.ToString("F3"));
            Console.WriteLine("Enthalpy:" + mAir.Enthalpy.ToString("F1"));
            Console.WriteLine("Atmospheric pressure:" + mAir.AtmosphericPressure.ToString("F1"));
            Console.WriteLine();

            //Calculate relative humidity from dry bulb temperature and enthalpy.
            double rHumid = MoistAir.GetAirStateFromDBEN(25, 58, MoistAir.Property.RelativeHumidity);
            Console.WriteLine("Relative Humidity:" + rHumid.ToString("F1"));

            Console.Read();
        }

        #endregion

    }
}
