using System;
using System.Collections.Generic;
using System.IO;

using Popolo.ThermophysicalProperty;
using Popolo.CircuitNetwork;
using Popolo.ThermalComfort;
using Popolo.Weather;
using Popolo.ThermalLoad;

namespace SamplePrograms
{
    class Program
    {
        /// <summary></summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            #region Chapter 1

            //sample1();

            #endregion

            #region Chapter 2

            //sample2_1();

            //sample2_2();

            #endregion

            #region Chapter 3

            //circuitTest1();

            //circuitTest2();

            //circuitTest3();

            #endregion

            #region Chapter 4

            //humanBodyTest();

            #endregion

            #region Chapter 5

            //weatherTest();

            #endregion

            #region Chapter 6

            //glassPanesTest();

            //windowTest();

            //airFlowWindowTest();

            //wallLayersTest();

            //wallTest1();

            //wallTest2();

            //wallTest3();

            //AirStateAndHeatLoadTest1();

            //AirStateAndHeatLoadTest2();

            #endregion

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
            mAir.HumidityRatio = 0.018;
            mAir.RelativeHumidity = 50.0;
            mAir.WetBulbTemperature = 22;
            mAir.SpecificVolume = 0.86;
            mAir.Enthalpy = 58.0;
            mAir.AtmosphericPressure = 101.325;

            //Output values of properties
            Console.WriteLine("Drybulb Temperature:" + mAir.DryBulbTemperature);
            Console.WriteLine("Absolute Humidity:" + mAir.HumidityRatio);
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
            mAir = MoistAir.GetAirStateFromDBHR(25, 0.012);

            //Write value of the moist air to standard output stream.
            Console.WriteLine("Dry bulb temperature:" + mAir.DryBulbTemperature.ToString("F1"));
            Console.WriteLine("Absolute humidity:" + mAir.HumidityRatio.ToString("F3"));
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

        #region Chapter 3

        /// <summary>Circuit test 1</summary>
        /// <remarks>Calculating energy flow between two nodes</remarks>
        private static void circuitTest1()
        {
            //Create new instance of Node class.
            Node node1 = new Node("SampleNode1", 0, 10);
            Node node2 = new Node("SampleNode2", 0, 0);

            //Create new instance of Channel class and connect nodes.
            Channel channel = new Channel("SampleChannel", 2, 1.2);
            channel.Connect(node1, node2);

            //Calculate energy flow.
            double flow = channel.GetFlow();

            Console.WriteLine("Energy flow is : " + flow.ToString("F2"));
            Console.Read();
        }

        /// <summary>Circuit test 2</summary>
        /// <remarks>Calculating water pipe network</remarks>
        private static void circuitTest2()
        {
            Circuit circuit = new Circuit("Circuit network of water pipe");

            //Add nodes to circuit network
            ImmutableNode node1 = circuit.AddNode(new Node("1", 0, 0));
            ImmutableNode node2 = circuit.AddNode(new Node("2", 0, 0));
            ImmutableNode node3 = circuit.AddNode(new Node("3", 0, 0));
            //Set external water flow
            circuit.SetExternalFlow(-7.06, node1);
            circuit.SetExternalFlow(7.06, node3);

            //Create channels
            Channel chA = new Channel("A", 167, 2);
            Channel chB = new Channel("B", 192, 2);
            Channel chC = new Channel("C", 840, 2);
            Channel chD = new Channel("D", 4950, 2);

            //Connect nodes with channels
            ImmutableChannel channelA = circuit.ConnectNodes(node1, node3, chA);
            ImmutableChannel channelB = circuit.ConnectNodes(node1, node2, chB);
            ImmutableChannel channelC = circuit.ConnectNodes(node2, node3, chC);
            ImmutableChannel channelD = circuit.ConnectNodes(node2, node3, chD);

            //Create solver
            CircuitSolver cSolver = new CircuitSolver(circuit);
            cSolver.Solve();
            Console.WriteLine("Water flow A is " + channelA.GetFlow().ToString("F2"));
            Console.WriteLine("Water flow B is " + channelB.GetFlow().ToString("F2"));
            Console.WriteLine("Water flow C is " + channelC.GetFlow().ToString("F2"));
            Console.WriteLine("Water flow D is " + channelD.GetFlow().ToString("F2"));
            Console.Read();
        }

        /// <summary>Circuit test 3</summary>
        /// <remarks>Calculating heat transfer through a wall</remarks>
        private static void circuitTest3()
        {
            Circuit circuit = new Circuit("Heat transfer network through wall");

            //Add nodes to circuit network
            ImmutableNode[] nodes = new ImmutableNode[6];
            nodes[0] = circuit.AddNode(new Node("Room 1", 0));
            nodes[1] = circuit.AddNode(new Node("Plywood", 17.9));
            nodes[2] = circuit.AddNode(new Node("Concrete", 232));
            nodes[3] = circuit.AddNode(new Node("Air gap", 0));
            nodes[4] = circuit.AddNode(new Node("Rock wool", 4.2));
            nodes[5] = circuit.AddNode(new Node("Room 2", 0));

            //Set boundary conditions (Room air temperatures).
            circuit.SetBoundaryNode(true, nodes[0]);
            circuit.SetBoundaryNode(true, nodes[5]);
            //Set air temperatures.
            circuit.SetPotential(20, nodes[0]);
            circuit.SetPotential(10, nodes[5]);
            for (int i = 1; i < 5; i++) circuit.SetPotential(10, nodes[i]); //Initialize wall temperatures to 10 C.

            //Connect nodes.
            ImmutableChannel channel01 = circuit.ConnectNodes(nodes[0], nodes[1], new Channel("Room 1-Plywood", 174, 1));
            ImmutableChannel channel12 = circuit.ConnectNodes(nodes[1], nodes[2], new Channel("Plywood-Concrete", 109, 1));
            ImmutableChannel channel34 = circuit.ConnectNodes(nodes[2], nodes[3], new Channel("Concrete-Air gap", 86, 1));
            ImmutableChannel channel45 = circuit.ConnectNodes(nodes[3], nodes[4], new Channel("Air gap-Rock wook", 638, 1));
            ImmutableChannel channel56 = circuit.ConnectNodes(nodes[4], nodes[5], new Channel("Rock wool-Room 2", 703, 1));

            CircuitSolver cSolver = new CircuitSolver(circuit);
            cSolver.TimeStep = 3600;

            for (int i = 0; i < nodes.Length; i++) Console.Write(nodes[i].Name + "  ");
            Console.WriteLine();
            for (int i = 0; i < 24; i++)
            {
                cSolver.Solve();
                Console.Write((i + 1) + "H : ");
                for (int j = 0; j < nodes.Length; j++) Console.Write(nodes[j].Potential.ToString("F1") + "  ");
                Console.WriteLine();
            }
            Console.Read();
        }

        #endregion

        #region Chapter 4

        /// <summary>Sample program calculating human body</summary>
        private static void humanBodyTest()
        {
            //This is constructor to make standard human body.
            //HumanBody body = new HumanBody();

            //Make human body model : Weight 70kg, Height 1.6m, Age 35, Female, Cardiac index 2.58, Fat 20%
            HumanBody body = new HumanBody(70, 1.6, 35, false, 2.58, 20);

            //Set clothing index [clo]
            body.SetClothingIndex(0);
            //Set dry-bulb temperature [C]
            body.SetDrybulbTemperature(42);
            //Set mean radiant temperature [C]
            body.SetMeanRadiantTemperature(42);
            //Set velocity [m/s]
            body.SetVelocity(1.0);
            //Set relative humidity [%]
            body.SetRelativeHumidity(50);

            //Use Nodes enumarator to set bouncary condition to particular position
            body.SetDrybulbTemperature(HumanBody.Nodes.RightHand, 20);

            //Updating body state
            Console.WriteLine("Time     |  R.Shoulder C temp  |  R.Shoulder S temp  |  L.Shoulder C temp  |  L.Shoulder S temp");
            for (int i = 0; i < 15; i++)
            {
                body.Update(120);
                ImmutableBodyPart rightShoulder = body.GetBodyPart(HumanBody.Nodes.RightShoulder);
                ImmutableBodyPart leftShoulder = body.GetBodyPart(HumanBody.Nodes.LeftShoulder);
                Console.Write(((i + 1) * 120) + "sec | ");
                Console.Write(rightShoulder.GetTemperature(BodyPart.Segments.Core).ToString("F2") + " | ");
                Console.Write(rightShoulder.GetTemperature(BodyPart.Segments.Skin).ToString("F2") + " | ");
                Console.Write(leftShoulder.GetTemperature(BodyPart.Segments.Core).ToString("F2") + " | ");
                Console.Write(leftShoulder.GetTemperature(BodyPart.Segments.Skin).ToString("F2") + " | ");
                Console.WriteLine();
            }

            Console.Read();
        }

        #endregion

        #region Chapter 5

        /// <summary>Sample program calculating weather state</summary>
        private static void weatherTest()
        {
            //Create an instance of the Sun class. (Location is Tokyo)
            Sun sun = new Sun(Sun.City.Tokyo);

            //Set date and time information(1983/12/21 12:00)
            DateTime dTime = new DateTime(1983, 12, 21, 12, 0, 0);
            sun.Update(dTime);

            //Create instances of the Incline class. Vertical south east surface and 45 degree west surface.
            Incline seInc = new Incline(Incline.Orientation.SE, 0.5 * Math.PI);
            Incline wInc = new Incline(Incline.Orientation.W, 0.25 * Math.PI);

            //Estimate direct normal and diffuse horizontal radiation from global horizontal radiation (467 W/m2)
            sun.EstimateDiffuseAndDirectNormalRadiation(467);

            //Calculate insolation rate on the inclined plane.
            double cosThetaSE, cosThetaW;
            cosThetaSE = seInc.GetDirectSolarRadiationRate(sun);
            cosThetaW = wInc.GetDirectSolarRadiationRate(sun);

            Console.WriteLine("Location:Tokyo, Date and time:12/21 12:00");
            Console.WriteLine("Altitude of sun=" + Sky.RadianToDegree(sun.Altitude).ToString("F1") + " degree");
            Console.WriteLine("Orientation of sun=" + Sky.RadianToDegree(sun.Orientation).ToString("F1") + " degree");
            Console.WriteLine("Direct normal radiation=" + sun.DirectNormalRadiation.ToString("F1") + " W/m2");
            Console.WriteLine("Diffuse horizontal radiation=" + sun.GlobalHorizontalRadiation.ToString("F1") + " W/m2");
            Console.WriteLine("Direct normal radiation to SE surface=" + (sun.DirectNormalRadiation * cosThetaSE).ToString("F1") + " W/m2");
            Console.WriteLine("Direct normal radiation to W surface=" + (sun.DirectNormalRadiation * cosThetaW).ToString("F1") + " W/m2");

            Console.Read();
        }

        #endregion

        #region Chapter 6_1

        /// <summary>Sample program calculating the charateristics of a glass panes</summary>
        private static void glassPanesTest()
        {
            //Create an array of the GlassPanes.Glass class 
            GlassPanes.Pane[] panes = new GlassPanes.Pane[3];

            //Create a transparent glass (3mm).
            panes[0] = new GlassPanes.Pane(0.79, 0.07, 0.85, 0.07, 131);    //Inside of the window
            panes[1] = new GlassPanes.Pane(0.79, 0.07, 0.85, 0.07, 131);
            panes[2] = new GlassPanes.Pane(0.79, 0.07, 0.85, 0.07, 131);    //Outside of the window

            //Create an instance of GlassPanes class
            GlassPanes glass = new GlassPanes(panes);

            //Set heat transfer coefficients[W/(m2-K)] of air gap
            glass.SetHeatTransferCoefficientsOfGaps(0, 1 / 0.12);
            glass.SetHeatTransferCoefficientsOfGaps(1, 1 / 0.12);

            //Set overall heat transfer coefficients[W/(m2-K)] at the surface of glass.
            glass.SetInsideFilmCoefficient(9.26);      //Inside of the window
            glass.SetOutsideFilmCoefficient(23.26);    //Outside of the window

            //Check the characteristics of the glass panes.
            Console.WriteLine("Transparent glass(3mm) * 3");
            Console.WriteLine("Overall transmissivity[-] = " + glass.OverallTransmissivity.ToString("F2"));
            Console.WriteLine("Overall absorptivity[-] = " + glass.OverallAbsorptivity.ToString("F2"));
            Console.WriteLine("Heat transfer coefficient of glass[-] = " + glass.ThermalTransmittanceOfGlass.ToString("F2"));
            Console.WriteLine("Heat transmission coefficient[-] = " + glass.ThermalTransmittance.ToString("F2"));
            Console.WriteLine();

            //Change the outside glass pane to heat reflecting glass(6mm)
            panes[0] = new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.HeatReflectingGlass06mm);

            //Check the characteristics of a single glass pane.
            Console.WriteLine("Heat reflecting glass(6mm)");
            Console.WriteLine("Transmissivity[-] = " + panes[0].OuterSideTransmissivity.ToString("F2"));
            Console.WriteLine("absorptivity[-] = " + panes[0].OuterSideAbsorptivity.ToString("F2"));
            Console.WriteLine("Reflectivity[-] = " + panes[0].OuterSideReflectivity.ToString("F2"));
            Console.WriteLine();

            //Create an instance of GlassPanes class. Other properties are same as above
            glass = new GlassPanes(panes);
            glass.SetHeatTransferCoefficientsOfGaps(0, 1 / 0.12);
            glass.SetHeatTransferCoefficientsOfGaps(1, 1 / 0.12);
            glass.SetInsideFilmCoefficient(9.26);      //Inside of the window
            glass.SetOutsideFilmCoefficient(23.26);    //Outside of the window

            //Check the characteristics of the glass panes.
            Console.WriteLine("Heat reflecting glass(6mm) + Transparent glass(3mm) * 2");
            Console.WriteLine("Overall transmissivity[-] = " + glass.OverallTransmissivity.ToString("F2"));
            Console.WriteLine("Overall absorptivity[-] = " + glass.OverallAbsorptivity.ToString("F2"));
            Console.WriteLine("Heat transfer coefficient of glass[-] = " + glass.ThermalTransmittanceOfGlass.ToString("F2"));
            Console.WriteLine("Heat transmission coefficient[-] = " + glass.ThermalTransmittance.ToString("F2"));

            Console.Read();
        }

        /// <summary>Sample program calculating the heat gain from the window</summary>
        private static void windowTest()
        {
            //A sample weather data
            //direct normal radiation [W/m2]
            double[] wdIdn = new double[] { 0, 0, 0, 0, 0, 244, 517, 679, 774, 829, 856, 862, 847, 809, 739, 619, 415, 97, 0, 0, 0, 0, 0, 0 };
            //diffuse horizontal radiation [W/m2]
            double[] wdIsky = new double[] { 0, 0, 0, 0, 21, 85, 109, 116, 116, 113, 110, 109, 111, 114, 116, 114, 102, 63, 0, 0, 0, 0, 0, 0 };
            //drybulb temperature [C]
            double[] wdDbt = new double[] { 27, 27, 27, 27, 27, 28, 29, 30, 31, 32, 32, 33, 33, 33, 34, 33, 32, 32, 31, 30, 29, 29, 28, 28 };
            //nocturnal radiation [W/m2]
            double[] wdRN = new double[] { 24, 24, 24, 24, 24, 24, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 26, 25, 25, 25, 25, 24, 24, 24 };

            //Create a window with a single 3mm transparent glass pane
            GlassPanes.Pane pane = new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.TransparentGlass03mm);
            GlassPanes glassPane = new GlassPanes(pane);
            Window window = new Window(glassPane);

            //Set wall surface information
            WindowSurface outsideWindowSurface = window.GetSurface(true);
            outsideWindowSurface.FilmCoefficient = 23d;
            outsideWindowSurface.Albedo = 0.2;
            WindowSurface insideWindowSurface = window.GetSurface(false);
            insideWindowSurface.FilmCoefficient = 9.3;

            //Set incline of an outdoor surface : South, vertical incline
            window.OutSideIncline = new Incline(Incline.Orientation.S, 0.5 * Math.PI);

            //There is no sun shade
            window.Shade = SunShade.EmptySunShade;

            //Initialize sun. Tokyo : 7/21 0:00
            Sun sun = new Sun(Sun.City.Tokyo);
            DateTime dTime = new DateTime(2001, 7, 21, 0, 0, 0);
            sun.Update(dTime);
            window.Sun = sun;

            //Indoor drybulb temperature is constant (25C)
            window.IndoorDrybulbTemperature = 25;

            //Result : Title line
            Console.WriteLine(" Time |Transmission[W]|Absorption[W]|Transfer[W]|Convective[W]|Radiative[W]");

            //execute simulation
            for (int i = 0; i < 24; i++)
            {
                //Set radiations (calculate global horizontal radiation from direct normal and diffuse horizontal radiation)
                sun.SetGlobalHorizontalRadiation(wdIsky[i], wdIdn[i]);
                //Set nocturnal radiation
                window.NocturnalRadiation = wdRN[i];
                //Set outdoor temperature
                window.OutdoorDrybulbTemperature = wdDbt[i];

                //Output result
                Console.WriteLine(dTime.ToShortTimeString().PadLeft(5) + " | " + window.TransmissionHeatGain.ToString("F1").PadLeft(13) + " | " +
                  window.AbsorbedHeatGain.ToString("F1").PadLeft(11) + " | " + window.TransferHeatGain.ToString("F1").PadLeft(9) + " | " +
                  window.ConvectiveHeatGain.ToString("F1").PadLeft(11) + " | " + window.RadiativeHeatGain.ToString("F1").PadLeft(11));

                //Update time
                dTime = dTime.AddHours(1);
                sun.Update(dTime);
            }

            Console.Read();
        }

        /// <summary>Sample program calculating the heat gain from the air flow window</summary>
        private static void airFlowWindowTest()
        {
            AirFlowWindow afWindow = new AirFlowWindow(new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.TransparentGlass06mm), 0.04,
                    new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.TransparentGlass06mm), 0.04, 0.8, 1.59, new Incline(Incline.Orientation.S, 0.5 * Math.PI));
            Sun sun = new Sun(Sun.City.Tokyo);
            afWindow.InteriorSideOverallHeatTransferCoefficient = 15 * 4.186 / 3.6;
            afWindow.ExteriorSideOverallHeatTransferCoefficient = 8 * 4.186 / 3.6;
            afWindow.Sun = sun;

            using (StreamReader sReader = new StreamReader("bnd.csv"))
            using (StreamWriter sWriter = new StreamWriter("out.csv"))
            {
                sun.Update(new DateTime(1985, 11, 21, 0, 30, 0));
                afWindow.SetAirFlowVolume(0.22 * 0.032 * 3600 * 0.5, 0.22 * 0.032 * 3600 * 0.5);
                afWindow.SetBlind(0.05, 0.45);
                for (int i = 0; i < 24; i++)
                {
                    string buff = sReader.ReadLine();
                    string[] sb = buff.Split(',');
                    afWindow.OutdoorTemperature = double.Parse(sb[2]);
                    afWindow.IndoorTemperature = double.Parse(sb[6]);
                    afWindow.SetInletAirTemperature(afWindow.IndoorTemperature);
                    afWindow.SetNocturnalRadiation(double.Parse(sb[7]));
                    sun.SetGlobalHorizontalRadiation(double.Parse(sb[9]), double.Parse(sb[8]));
                    sWriter.WriteLine(
                        afWindow.GetExteriorGlassTemperature() + "," + afWindow.GetBlindTemperature() + "," +
                        afWindow.GetInteriorGlassTemperature() + "," + afWindow.GetHeatRemovalByAirFlow());

                    sun.Update(sun.CurrentDateTime.AddHours(1));
                }

                sun.Update(new DateTime(1985, 11, 19, 0, 30, 0));
                afWindow.SetAirFlowVolume(0.22 * 0.032 * 3600, 0.22 * 0.032 * 3600);
                afWindow.SetBlind(0.05, 0.45);
                for (int i = 0; i < 24; i++)
                {
                    string buff = sReader.ReadLine();
                    string[] sb = buff.Split(',');
                    afWindow.OutdoorTemperature = double.Parse(sb[2]);
                    afWindow.IndoorTemperature = double.Parse(sb[6]);
                    afWindow.SetInletAirTemperature(afWindow.IndoorTemperature);
                    afWindow.SetNocturnalRadiation(double.Parse(sb[7]));
                    sun.SetGlobalHorizontalRadiation(double.Parse(sb[9]), double.Parse(sb[8]));
                    sWriter.WriteLine(
                        afWindow.GetExteriorGlassTemperature() + "," + afWindow.GetBlindTemperature() + "," +
                        afWindow.GetInteriorGlassTemperature() + "," + afWindow.GetHeatRemovalByAirFlow());

                    sun.Update(sun.CurrentDateTime.AddHours(1));
                }

                sun.Update(new DateTime(1985, 12, 3, 0, 30, 0));
                afWindow.SetAirFlowVolume(0.22 * 0.032 * 3600 * 2, 0.22 * 0.032 * 3600 * 2);
                afWindow.SetBlind(0.05, 0.45);
                for (int i = 0; i < 24; i++)
                {
                    string buff = sReader.ReadLine();
                    string[] sb = buff.Split(',');
                    afWindow.OutdoorTemperature = double.Parse(sb[2]);
                    afWindow.IndoorTemperature = double.Parse(sb[6]);
                    afWindow.SetInletAirTemperature(afWindow.IndoorTemperature);
                    afWindow.SetNocturnalRadiation(double.Parse(sb[7]));
                    sun.SetGlobalHorizontalRadiation(double.Parse(sb[9]), double.Parse(sb[8]));
                    sWriter.WriteLine(
                        afWindow.GetExteriorGlassTemperature() + "," + afWindow.GetBlindTemperature() + "," +
                        afWindow.GetInteriorGlassTemperature() + "," + afWindow.GetHeatRemovalByAirFlow());

                    sun.Update(sun.CurrentDateTime.AddHours(1));
                }

                sun.Update(new DateTime(1985, 10, 2, 0, 30, 0));
                afWindow.SetAirFlowVolume(0.22 * 0.032 * 3600, 0.22 * 0.032 * 3600);
                afWindow.SetBlind(0.027, 0.243);
                for (int i = 0; i < 24; i++)
                {
                    string buff = sReader.ReadLine();
                    string[] sb = buff.Split(',');
                    afWindow.OutdoorTemperature = double.Parse(sb[2]);
                    afWindow.IndoorTemperature = double.Parse(sb[6]);
                    afWindow.SetInletAirTemperature(afWindow.IndoorTemperature);
                    afWindow.SetNocturnalRadiation(double.Parse(sb[7]));
                    sun.SetGlobalHorizontalRadiation(double.Parse(sb[9]), double.Parse(sb[8]));
                    sWriter.WriteLine(
                        afWindow.GetExteriorGlassTemperature() + "," + afWindow.GetBlindTemperature() + "," +
                        afWindow.GetInteriorGlassTemperature() + "," + afWindow.GetHeatRemovalByAirFlow());

                    sun.Update(sun.CurrentDateTime.AddHours(1));
                }
            }

            /*sun.Update(new DateTime(1985, 10, 2, 0, 30, 0));
            Incline sinc = new Incline(Incline.Orientation.S, 0.5 * Math.PI);
            Console.WriteLine(sun.GetExtraterrestrialRadiation());
            Console.WriteLine(sun.Altitude);
            for (int i = 0; i < 24; i++)
            {
                sun.Update(sun.CurrentDateTime.AddHours(1));
                //Console.WriteLine(sun.Altitude);
                Console.WriteLine(sinc.GetDirectSolarRadiationRate(sun));
            }*/
        }

        /// <summary>Sample program calculating the thermal transimission of the wall layers</summary>
        private static void wallLayersTest()
        {
            //Create an instance of WallLayers
            WallLayers wLayers = new WallLayers("Sample wall layer");

            //Make an array of materials
            WallMaterial[] materials = new WallMaterial[4];
            //The first layer : plywood
            materials[0] = new WallMaterial(WallMaterial.PredefinedMaterials.Plywood);
            //The second layer : air gap
            materials[1] = new WallMaterial(WallMaterial.PredefinedMaterials.AirGap);
            //The thirg layer : concrete
            materials[2] = new WallMaterial(WallMaterial.PredefinedMaterials.ReinforcedConcrete);
            //The fourth layer : white Wash
            materials[3] = new WallMaterial("White Wash", 0.7, 1000);

            //Add a layer to WallLayers object
            //plywood : 20mm
            wLayers.AddLayer(new WallLayers.Layer(materials[0], 0.02));
            //air gap : heat conductance doesn't depend on thickness
            wLayers.AddLayer(new WallLayers.Layer(materials[1], 0.01));
            //concrete : 150mm
            wLayers.AddLayer(new WallLayers.Layer(materials[2], 0.15));
            //white Wash : 10mm
            wLayers.AddLayer(new WallLayers.Layer(materials[3], 0.01));

            //output result
            Console.WriteLine("Wall composition");
            for (uint i = 0; i < wLayers.LayerNumber; i++)
            {
                WallLayers.Layer layer = wLayers.GetLayer(i);
                Console.WriteLine("Layer " + (i + 1) + "：" + layer.Material.Name + "(" + layer.Thickness + "m)");
            }
            Console.WriteLine("Thermal transmission = " + wLayers.GetThermalTransmission().ToString("F1") + " W/(m2-K)");
            Console.WriteLine();

            //Replace concrete to light weight concrete
            wLayers.ReplaceLayer(2, new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.LightweightConcrete), 0.15));

            //output result
            Console.WriteLine("Wall composition");
            for (uint i = 0; i < wLayers.LayerNumber; i++)
            {
                WallLayers.Layer layer = wLayers.GetLayer(i);
                Console.WriteLine("Layer " + (i + 1) + "：" + layer.Material.Name + "(" + layer.Thickness + "m)");
            }
            Console.WriteLine("Thermal transmission = " + wLayers.GetThermalTransmission().ToString("F1") + " W/(m2-K)");
            Console.WriteLine();

            Console.Read();
        }

        /// <summary>Sample program calculating the unsteady heat conduction of wall</summary>
        private static void wallTest1()
        {
            WallLayers layers = new WallLayers();
            WallLayers.Layer layer;
            layer = new WallLayers.Layer(new WallMaterial("Plywood", 0.19, 716), 0.025);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("Concrete", 1.4, 1934), 0.120);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("Air gap", 1d / 0.086, 0), 0.020);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("Rock wool", 0.042, 84), 0.050);
            layers.AddLayer(layer);
            Wall wall = new Wall(layers);

            wall.TimeStep = 3600;
            wall.AirTemperature1 = 20;
            wall.AirTemperature2 = 10;
            wall.InitializeTemperature(10); //Initial temperature is 10 C
            wall.SurfaceArea = 1;

            Console.WriteLine("Plywood, Concrete, Air gap, Rock wool");
            double[] temps;
            for (int i = 0; i < 24; i++)
            {
                wall.Update();
                temps = wall.GetTemperatures();
                Console.Write((i + 1).ToString("F0").PadLeft(2) + "Hour | ");
                for (int j = 0; j < temps.Length - 1; j++) Console.Write(((temps[j] + temps[j + 1]) / 2d).ToString("F1") + " | ");
                Console.WriteLine();
            }

            //Iterate until wall become steady state
            for (int i = 0; i < 1000; i++) wall.Update();
            Console.WriteLine();
            Console.WriteLine("Steady state");
            temps = wall.GetTemperatures();
            for (int j = 0; j < temps.Length - 1; j++) Console.Write(((temps[j] + temps[j + 1]) / 2d).ToString("F1") + " | ");

            Console.WriteLine();
            Console.WriteLine("Heat transfer at steady state 1: " + wall.GetHeatTransfer(true).ToString("F1"));
            Console.WriteLine("Heat transfer at steady state 2: " + wall.GetHeatTransfer(false).ToString("F1"));
            Console.WriteLine("Heat transfer at steady state 3: " + wall.GetStaticHeatTransfer().ToString("F1"));

            Console.Read();
        }

        /// <summary>Sample program calculating the unsteady heat conduction of wall with heating tube</summary>
        private static void wallTest2()
        {
            WallLayers wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.FrexibleBoard), 0.0165));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial("Water", 0.59, 4186), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial("Water", 0.59, 4186), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.ExtrudedPolystyreneFoam_3), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.009));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.AirGap), 0.015));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.009));
            Wall wall = new Wall(wl);
            wall.TimeStep = 300;
            wall.AirTemperature1 = 20;
            wall.AirTemperature2 = 10;
            wall.SurfaceArea = 6.48;

            Tube tube = new Tube(0.84, 0.346, 4186);
            //installing tube to wall
            wall.AddTube(tube, 1);
            tube.SetFlowRate(0);  //initial flow rate is 0 kg/s
            tube.FluidTemperature = 30;

            wall.InitializeTemperature(20); //initialize temperature of the wall

            for (int i = 0; i < wall.Layers.LayerNumber; i++) Console.Write("temperature" + i + ", ");
            Console.WriteLine("heat transfer to the tube[W], outlet temperature of fluid[C]");
            for (int i = 0; i < 100; i++)
            {
                if (i == 50) tube.SetFlowRate(0.54);  //start heating
                wall.Update();
                double[] tmp = wall.GetTemperatures();
                for (int j = 0; j < tmp.Length - 1; j++) Console.Write(((tmp[j] + tmp[j + 1]) / 2d).ToString("F1") + ", ");
                Console.Write(wall.GetHeatTransferToTube(1).ToString("F0") + ", " + tube.GetOutletFluidTemperature().ToString("F1"));
                Console.WriteLine();
            }
            Console.Read();
        }

        /// <summary>Sample program calculating the unsteady heat conduction of wall with latent heat storage material</summary>
        private static void wallTest3()
        {
            //Initial temperature
            const double INIT_TEMP = 35;

            //Create an instance of WallLayers class
            WallLayers wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.FrexibleBoard), 0.0165));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial("dummy", 1, 1), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial("dummy", 1, 1), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.ExtrudedPolystyreneFoam_3), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.009));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.AirGap), 0.015));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.009));

            //Create an instance of Wall class
            Wall wall = new Wall(wl);
            wall.TimeStep = 1200;
            wall.AirTemperature1 = 20;
            wall.AirTemperature2 = 20;
            wall.SurfaceArea = 6.48;

            //Create an instance of LatentHeatStorageMaterial class
            LatentHeatStorageMaterial pmc1;
            pmc1 = new LatentHeatStorageMaterial(19, new WallMaterial("PCM1 (Solid)", 0.19, 3.6 * 1400));
            pmc1.AddMaterial(23, new WallMaterial("PCM1 (Two phase)", (0.19 + 0.22) / 2d, 15.1 * 1400));
            pmc1.AddMaterial(100, new WallMaterial("PCM1 (Liquid)", 0.22, 3.6 * 1400));
            pmc1.Initialize(INIT_TEMP);
            //Set PCM to second wall layer
            wall.SetLatentHeatStorageMaterial(1, pmc1);

            //Create an instance of LatentHeatStorageMaterial class
            LatentHeatStorageMaterial pcm2;
            pcm2 = new LatentHeatStorageMaterial(30, new WallMaterial("PCM2 (Solid)", 0.19, 3.6 * 1390));
            pcm2.AddMaterial(32, new WallMaterial("PCM2 (Two phase)", (0.19 + 0.22) / 2d, 63.25 * 1400));
            pcm2.AddMaterial(100, new WallMaterial("PCM2 (Liquid)", 0.22, 3.5 * 1410));
            pcm2.Initialize(INIT_TEMP);
            //Set PCM to third wall layer
            wall.SetLatentHeatStorageMaterial(2, pcm2);

            //Install heating tube between PMCs
            Tube tube = new Tube(0.84, 0.346, 4186);
            wall.AddTube(tube, 1);
            tube.SetFlowRate(0);
            tube.FluidTemperature = 40;

            //Initialize wall temperature
            wall.InitializeTemperature(INIT_TEMP);

            for (int i = 0; i < wall.Layers.LayerNumber; i++) Console.Write("Temperature" + i + ", ");
            Console.WriteLine("Heat storage[kJ]");
            for (int i = 0; i < 200; i++)
            {
                if (i == 100)
                {
                    tube.SetFlowRate(0.54); //Start heating
                    wall.AirTemperature1 = 30;
                    wall.AirTemperature2 = 30;
                }
                wall.Update();
                double[] tmp = wall.GetTemperatures();
                for (int j = 0; j < tmp.Length - 1; j++) Console.Write(((tmp[j] + tmp[j + 1]) / 2d).ToString("F1") + ", ");
                Console.Write(wall.GetHeatStorage(INIT_TEMP).ToString("F0"));
                Console.WriteLine();
            }
            Console.Read();
        }

        #endregion

        #region Chapter 6_2

        /// <summary>Sample program calculating the air state and heat load of the building (Zone class)</summary>
        private static void AirStateAndHeatLoadTest1()
        {
            //A sample weather data
            //Drybulb temperature [C]
            double[] dbt = new double[] { 24.2, 24.1, 24.1, 24.2, 24.3, 24.2, 24.4, 25.1, 26.1, 27.1, 28.8, 29.9,
                30.7, 31.2, 31.6, 31.4, 31.3, 30.8, 29.4, 28.1, 27.5, 27.1, 26.6, 26.3 };
            //Humidity ratio [kg/kg(DA)]
            double[] hum = new double[] { 0.0134, 0.0136, 0.0134, 0.0133, 0.0131, 0.0134, 0.0138, 0.0142, 0.0142, 0.0140, 0.0147, 0.0149,
                0.0142, 0.0146, 0.0140, 0.0145, 0.0144, 0.0146, 0.0142, 0.0136, 0.0136, 0.0135, 0.0136, 0.0140 };
            //Nocturnal radiation [W/m2]
            double[] nrd = new double[] { 32, 30, 30, 29, 26, 24, 24, 25, 25, 25, 24, 24, 24, 23, 24, 24, 24, 24, 23, 23, 24, 26, 25, 23 };
            //Direct normal radiation [W/m2]
            double[] dnr = new double[] { 0, 0, 0, 0, 0, 0, 106, 185, 202, 369, 427, 499, 557, 522, 517, 480, 398, 255, 142, 2, 0, 0, 0, 0 };
            //Diffuse horizontal radiation [W/m2]
            double[] drd = new double[] { 0, 0, 0, 0, 0, 0, 36, 115, 198, 259, 314, 340, 340, 349, 319, 277, 228, 167, 87, 16, 0, 0, 0, 0 };

            //Create an instance of the Outdoor class
            Outdoor outdoor = new Outdoor();
            Sun sun = new Sun(Sun.City.Tokyo);  //Located in Tokyo
            outdoor.Sun = sun;
            outdoor.GroundTemperature = 25;     //Ground temperature is assumed to be constant

            //Create an instance of the Incline class
            Incline nIn = new Incline(Incline.Orientation.N, 0.5 * Math.PI); //North, Vertical
            Incline eIn = new Incline(Incline.Orientation.E, 0.5 * Math.PI); //East, Vertical
            Incline wIn = new Incline(Incline.Orientation.W, 0.5 * Math.PI); //West, Vertical
            Incline sIn = new Incline(Incline.Orientation.S, 0.5 * Math.PI); //South, Vertical
            Incline hIn = new Incline(Incline.Orientation.S, 0);  //Horizontal

            //Create an instance of the Zone class
            Zone[] zones = new Zone[4];
            Zone wpZone = zones[0] = new Zone("West perimeter zone");
            wpZone.Volume = 3 * 5 * 3;  //Ceiling height is 3m
            Zone wiZone = zones[1] = new Zone("West interior zone");
            wiZone.Volume = 4 * 5 * 3;
            Zone epZone = zones[2] = new Zone("East perimeter zone");
            epZone.Volume = 3 * 5 * 3;
            Zone eiZone = zones[3] = new Zone("East interior zone");
            eiZone.Volume = 4 * 5 * 3;
            foreach (Zone zn in zones)
            {
                zn.VentilationVolume = 10; //Ventilation volume[CMH]
                zn.TimeStep = 3600;
                zn.DrybulbTemperatureSetPoint = 26;
                zn.HumidityRatioSetPoint = 0.01;
            }

            //Set a heat production element to the east interior zone
            //Convective sensible heat=100W, Radiative sensible heat=100W, Latent heat=20W
            eiZone.AddHeatGain(new ConstantHeatGain(100, 100, 20));

            //Create an instance of the WallLayers class : Concrete,400mm
            WallLayers wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.ReinforcedConcrete), 0.4));

            //Create an instance of the GlassPanes class:Low-emissivity coating single glass
            GlassPanes gPanes = new GlassPanes(new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.HeatReflectingGlass06mm));

            //Set wall surfaces to the zone objects
            Wall[] walls = new Wall[18];
            List<WallSurface> outdoorSurfaces = new List<WallSurface>();
            Wall wpwWall = walls[0] = new Wall(wl, "West wall in the west perimeter zone");
            wpwWall.SurfaceArea = 3 * 3;
            outdoorSurfaces.Add(wpwWall.GetSurface(true));
            wpZone.AddSurface(wpwWall.GetSurface(false));
            wpwWall.SetIncline(wIn, true);

            Wall wpcWall = walls[1] = new Wall(wl, "Ceiling in the west perimeter zone");
            wpcWall.SurfaceArea = 3 * 5;
            outdoorSurfaces.Add(wpcWall.GetSurface(true));
            wpZone.AddSurface(wpcWall.GetSurface(false));
            wpcWall.SetIncline(hIn, true);

            Wall wpfWall = walls[2] = new Wall(wl, "Floor in the west perimeter zone");
            wpfWall.SurfaceArea = 3 * 5;
            outdoor.AddGroundWallSurface(wpfWall.GetSurface(true));
            wpZone.AddSurface(wpfWall.GetSurface(false));

            Wall winWall = walls[3] = new Wall(wl, "North wall in the west interior zone");
            winWall.SurfaceArea = 3 * 5;
            outdoorSurfaces.Add(winWall.GetSurface(true));
            wiZone.AddSurface(winWall.GetSurface(false));
            winWall.SetIncline(nIn, true);

            Wall wiwWall = walls[4] = new Wall(wl, "West wall in the west interior zone");
            wiwWall.SurfaceArea = 3 * 4;
            outdoorSurfaces.Add(wiwWall.GetSurface(true));
            wiZone.AddSurface(wiwWall.GetSurface(false));
            wiwWall.SetIncline(wIn, true);

            Wall wicWall = walls[5] = new Wall(wl, "Ceiling in the west interior zone");
            wicWall.SurfaceArea = 4 * 5;
            outdoorSurfaces.Add(wicWall.GetSurface(true));
            wiZone.AddSurface(wicWall.GetSurface(false));
            wicWall.SetIncline(hIn, true);

            Wall wifWall = walls[6] = new Wall(wl, "Floor in the west interior zone");
            wifWall.SurfaceArea = 4 * 5;
            outdoor.AddGroundWallSurface(wifWall.GetSurface(true));
            wiZone.AddSurface(wifWall.GetSurface(false));

            Wall epwWall = walls[7] = new Wall(wl, "East wall in the east perimeter zone");
            epwWall.SurfaceArea = 3 * 3;
            outdoorSurfaces.Add(epwWall.GetSurface(true));
            epZone.AddSurface(epwWall.GetSurface(false));
            epwWall.SetIncline(eIn, true);

            Wall epcWall = walls[8] = new Wall(wl, "Ceiling in the east perimeter zone");
            epcWall.SurfaceArea = 3 * 5;
            outdoorSurfaces.Add(epcWall.GetSurface(true));
            epZone.AddSurface(epcWall.GetSurface(false));
            epcWall.SetIncline(hIn, true);

            Wall epfWall = walls[9] = new Wall(wl, "Floor in the east perimeter zone");
            epfWall.SurfaceArea = 3 * 5;
            outdoor.AddGroundWallSurface(epfWall.GetSurface(true));
            epZone.AddSurface(epfWall.GetSurface(false));

            Wall einWall = walls[10] = new Wall(wl, "North wall in the east interior zone");
            einWall.SurfaceArea = 5 * 3;
            outdoorSurfaces.Add(einWall.GetSurface(true));
            eiZone.AddSurface(einWall.GetSurface(false));
            einWall.SetIncline(nIn, true);

            Wall eiwWall = walls[11] = new Wall(wl, "East wall in the east interior zone");
            eiwWall.SurfaceArea = 4 * 3;
            outdoorSurfaces.Add(eiwWall.GetSurface(true));
            eiZone.AddSurface(eiwWall.GetSurface(false));
            eiwWall.SetIncline(eIn, true);

            Wall eicWall = walls[12] = new Wall(wl, "Ceiling in the east interior zone");
            eicWall.SurfaceArea = 4 * 5;
            outdoorSurfaces.Add(eicWall.GetSurface(true));
            eiZone.AddSurface(eicWall.GetSurface(false));
            eicWall.SetIncline(hIn, true);

            Wall eifWall = walls[13] = new Wall(wl, "Floor in the east interior zone");
            eifWall.SurfaceArea = 4 * 5;
            outdoor.AddGroundWallSurface(eifWall.GetSurface(true));
            eiZone.AddSurface(eifWall.GetSurface(false));

            Wall cpWall = walls[14] = new Wall(wl, "Inner wall at perimeter");
            cpWall.SurfaceArea = 3 * 3;
            wpZone.AddSurface(cpWall.GetSurface(true));
            epZone.AddSurface(cpWall.GetSurface(false));

            Wall ciWall = walls[15] = new Wall(wl, "Inner wall at interior");
            ciWall.SurfaceArea = 4 * 3;
            wiZone.AddSurface(ciWall.GetSurface(true));
            eiZone.AddSurface(ciWall.GetSurface(false));

            Wall wpsWall = walls[16] = new Wall(wl, "South wall in the west perimeter zone");
            wpsWall.SurfaceArea = 5 * 3 - 3 * 2;    //Reduce window surface area
            outdoorSurfaces.Add(wpsWall.GetSurface(true));
            wpZone.AddSurface(wpsWall.GetSurface(false));
            wpsWall.SetIncline(sIn, true);

            Wall epsWall = walls[17] = new Wall(wl, "South wall in the east perimeter zone");
            epsWall.SurfaceArea = 5 * 3 - 3 * 2;    //Reduce window surface area
            outdoorSurfaces.Add(epsWall.GetSurface(true));
            epZone.AddSurface(epsWall.GetSurface(false));
            epsWall.SetIncline(sIn, true);

            //Initialize outdoor surfaces
            foreach (WallSurface ws in outdoorSurfaces)
            {
                //Add wall surfaces to Outdoor object
                outdoor.AddWallSurface(ws);
                //Initialize emissivity of surface
                ws.InitializeEmissivity(WallSurface.SurfaceMaterial.Concrete);
            }

            //Add windows to the west zone
            Window wWind = new Window(gPanes, "Window in the west perimeter zone");
            wWind.SurfaceArea = 3 * 2;
            wpZone.AddWindow(wWind);
            outdoor.AddWindow(wWind);
            //Add windows to the east zone
            Window eWind = new Window(gPanes, "Window in the east perimeter zone");
            eWind.SurfaceArea = 3 * 2;
            //Set horizontal sun shade.
            eWind.Shade = SunShade.MakeHorizontalSunShade(3, 2, 1, 1, 1, 0.5, sIn);
            wpZone.AddWindow(eWind);
            outdoor.AddWindow(eWind);

            //Output title wrine to standard output stream
            StreamWriter sWriter = new StreamWriter("AirStateAndHeatLoadTest1.csv");
            foreach (Zone zn in zones) sWriter.Write(zn.Name + "Drybulb temperature[C], " + zn.Name +
                                      "Humidity ratio[kg/kgDA], " + zn.Name + "Sensible heat load[W], " + zn.Name + "Latent heat load[W], ");
            sWriter.WriteLine();

            //Update the state (Iterate 100 times to make state steady)
            for (int i = 0; i < 100; i++)
            {
                DateTime dTime = new DateTime(2007, 8, 3, 0, 0, 0);
                for (int j = 0; j < 24; j++)
                {
                    //Set date and time to Sun and Zone object.
                    sun.Update(dTime);
                    foreach (Zone zn in zones) zn.CurrentDateTime = dTime;

                    //Operate HVAC system (8:00~19:00)
                    bool operating = (8 <= dTime.Hour && dTime.Hour <= 19);
                    foreach (Zone zn in zones)
                    {
                        zn.ControlHumidityRatio = operating;
                        zn.ControlDrybulbTemperature = operating;
                    }

                    //Set weather state.
                    outdoor.AirState = new MoistAir(dbt[j], hum[j]);
                    outdoor.NocturnalRadiation = nrd[j];
                    sun.SetGlobalHorizontalRadiation(drd[j], dnr[j]);

                    //Set ventilation air state.
                    eiZone.VentilationAirState = new MoistAir(epZone.CurrentDrybulbTemperature, eiZone.CurrentHumidityRatio);
                    epZone.VentilationAirState = new MoistAir(eiZone.CurrentDrybulbTemperature, eiZone.CurrentHumidityRatio);
                    wpZone.VentilationAirState = outdoor.AirState;
                    wiZone.VentilationAirState = new MoistAir(wpZone.CurrentDrybulbTemperature, wpZone.CurrentHumidityRatio);

                    //Update boundary state of outdoor facing surfaces.
                    outdoor.SetWallSurfaceBoundaryState();

                    //Update the walls.
                    foreach (Wall wal in walls) wal.Update();

                    //Update the zones.
                    foreach (Zone zn in zones) zn.Update();

                    //Update date and time
                    dTime = dTime.AddHours(1);

                    //If it is last iteration, output result to CSV text.
                    if (i == 99)
                    {
                        foreach (Zone zn in zones)
                        {
                            sWriter.Write(zn.CurrentDrybulbTemperature.ToString("F1") + ", " + zn.CurrentHumidityRatio.ToString("F3") + ", " +
                            zn.CurrentSensibleHeatLoad.ToString("F0") + ", " + zn.CurrentLatentHeatLoad.ToString("F0") + ", ");
                        }
                        sWriter.WriteLine();
                    }
                }
            }

            sWriter.Close();
        }

        /// <summary>Sample program calculating the air state and heat load of the building (MultiRoom class)</summary>
        private static void AirStateAndHeatLoadTest2()
        {
            //A sample weather data
            //Drybulb temperature [C]
            double[] dbt = new double[] { 24.2, 24.1, 24.1, 24.2, 24.3, 24.2, 24.4, 25.1, 26.1, 27.1, 28.8, 29.9,
                30.7, 31.2, 31.6, 31.4, 31.3, 30.8, 29.4, 28.1, 27.5, 27.1, 26.6, 26.3 };
            //Humidity ratio [kg/kg(DA)]
            double[] hum = new double[] { 0.0134, 0.0136, 0.0134, 0.0133, 0.0131, 0.0134, 0.0138, 0.0142, 0.0142, 0.0140, 0.0147, 0.0149,
                0.0142, 0.0146, 0.0140, 0.0145, 0.0144, 0.0146, 0.0142, 0.0136, 0.0136, 0.0135, 0.0136, 0.0140 };
            //Nocturnal radiation [W/m2]
            double[] nrd = new double[] { 32, 30, 30, 29, 26, 24, 24, 25, 25, 25, 24, 24, 24, 23, 24, 24, 24, 24, 23, 23, 24, 26, 25, 23 };
            //Direct normal radiation [W/m2]
            double[] dnr = new double[] { 0, 0, 0, 0, 0, 0, 106, 185, 202, 369, 427, 499, 557, 522, 517, 480, 398, 255, 142, 2, 0, 0, 0, 0 };
            //Diffuse horizontal radiation [W/m2]
            double[] drd = new double[] { 0, 0, 0, 0, 0, 0, 36, 115, 198, 259, 314, 340, 340, 349, 319, 277, 228, 167, 87, 16, 0, 0, 0, 0 };

            //Create an instance of the Outdoor class
            Outdoor outdoor = new Outdoor();
            Sun sun = new Sun(Sun.City.Tokyo);  //Located in Tokyo
            outdoor.Sun = sun;
            outdoor.GroundTemperature = 25;     //Ground temperature is assumed to be constant

            //Create an instance of the Incline class
            Incline nIn = new Incline(Incline.Orientation.N, 0.5 * Math.PI); //North, Vertical
            Incline eIn = new Incline(Incline.Orientation.E, 0.5 * Math.PI); //East, Vertical
            Incline wIn = new Incline(Incline.Orientation.W, 0.5 * Math.PI); //West, Vertical
            Incline sIn = new Incline(Incline.Orientation.S, 0.5 * Math.PI); //South, Vertical
            Incline hIn = new Incline(Incline.Orientation.S, 0);  //Horizontal

            //Create an instance of the Zone class
            Zone[] zones = new Zone[4];
            Zone wpZone = zones[0] = new Zone("West perimeter zone");
            wpZone.Volume = 3 * 5 * 3;  //Ceiling height is 3m
            Zone wiZone = zones[1] = new Zone("West interior zone");
            wiZone.Volume = 4 * 5 * 3;
            Zone epZone = zones[2] = new Zone("East perimeter zone");
            epZone.Volume = 3 * 5 * 3;
            Zone eiZone = zones[3] = new Zone("East interior zone");
            eiZone.Volume = 4 * 5 * 3;
            foreach (Zone zn in zones)
            {
                zn.VentilationVolume = 10; //Ventilation volume[CMH]
                zn.TimeStep = 3600;
                zn.DrybulbTemperatureSetPoint = 26;
                zn.HumidityRatioSetPoint = 0.01;
            }

            //Set a heat production element to the east interior zone
            //Convective sensible heat=100W, Radiative sensible heat=100W, Latent heat=20W
            eiZone.AddHeatGain(new ConstantHeatGain(100, 100, 20));

            //Create an instance of the WallLayers class : Concrete,400mm
            WallLayers wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.ReinforcedConcrete), 0.4));

            //Create an instance of the GlassPanes class:Low-emissivity coating single glass
            GlassPanes gPanes = new GlassPanes(new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.HeatReflectingGlass06mm));

            //Set wall surfaces to the zone objects
            Wall[] walls = new Wall[18];
            List<WallSurface> outdoorSurfaces = new List<WallSurface>();
            Wall wpwWall = walls[0] = new Wall(wl, "West wall in the west perimeter zone");
            wpwWall.SurfaceArea = 3 * 3;
            outdoorSurfaces.Add(wpwWall.GetSurface(true));
            wpZone.AddSurface(wpwWall.GetSurface(false));
            wpwWall.SetIncline(wIn, true);

            Wall wpcWall = walls[1] = new Wall(wl, "Ceiling in the west perimeter zone");
            wpcWall.SurfaceArea = 3 * 5;
            outdoorSurfaces.Add(wpcWall.GetSurface(true));
            wpZone.AddSurface(wpcWall.GetSurface(false));
            wpcWall.SetIncline(hIn, true);

            Wall wpfWall = walls[2] = new Wall(wl, "Floor in the west perimeter zone");
            wpfWall.SurfaceArea = 3 * 5;
            outdoor.AddGroundWallSurface(wpfWall.GetSurface(true));
            wpZone.AddSurface(wpfWall.GetSurface(false));

            Wall winWall = walls[3] = new Wall(wl, "North wall in the west interior zone");
            winWall.SurfaceArea = 3 * 5;
            outdoorSurfaces.Add(winWall.GetSurface(true));
            wiZone.AddSurface(winWall.GetSurface(false));
            winWall.SetIncline(nIn, true);

            Wall wiwWall = walls[4] = new Wall(wl, "West wall in the west interior zone");
            wiwWall.SurfaceArea = 3 * 4;
            outdoorSurfaces.Add(wiwWall.GetSurface(true));
            wiZone.AddSurface(wiwWall.GetSurface(false));
            wiwWall.SetIncline(wIn, true);

            Wall wicWall = walls[5] = new Wall(wl, "Ceiling in the west interior zone");
            wicWall.SurfaceArea = 4 * 5;
            outdoorSurfaces.Add(wicWall.GetSurface(true));
            wiZone.AddSurface(wicWall.GetSurface(false));
            wicWall.SetIncline(hIn, true);

            Wall wifWall = walls[6] = new Wall(wl, "Floor in the west interior zone");
            wifWall.SurfaceArea = 4 * 5;
            outdoor.AddGroundWallSurface(wifWall.GetSurface(true));
            wiZone.AddSurface(wifWall.GetSurface(false));

            Wall epwWall = walls[7] = new Wall(wl, "East wall in the east perimeter zone");
            epwWall.SurfaceArea = 3 * 3;
            outdoorSurfaces.Add(epwWall.GetSurface(true));
            epZone.AddSurface(epwWall.GetSurface(false));
            epwWall.SetIncline(eIn, true);

            Wall epcWall = walls[8] = new Wall(wl, "Ceiling in the east perimeter zone");
            epcWall.SurfaceArea = 3 * 5;
            outdoorSurfaces.Add(epcWall.GetSurface(true));
            epZone.AddSurface(epcWall.GetSurface(false));
            epcWall.SetIncline(hIn, true);

            Wall epfWall = walls[9] = new Wall(wl, "Floor in the east perimeter zone");
            epfWall.SurfaceArea = 3 * 5;
            outdoor.AddGroundWallSurface(epfWall.GetSurface(true));
            epZone.AddSurface(epfWall.GetSurface(false));

            Wall einWall = walls[10] = new Wall(wl, "North wall in the east interior zone");
            einWall.SurfaceArea = 5 * 3;
            outdoorSurfaces.Add(einWall.GetSurface(true));
            eiZone.AddSurface(einWall.GetSurface(false));
            einWall.SetIncline(nIn, true);

            Wall eiwWall = walls[11] = new Wall(wl, "East wall in the east interior zone");
            eiwWall.SurfaceArea = 4 * 3;
            outdoorSurfaces.Add(eiwWall.GetSurface(true));
            eiZone.AddSurface(eiwWall.GetSurface(false));
            eiwWall.SetIncline(eIn, true);

            Wall eicWall = walls[12] = new Wall(wl, "Ceiling in the east interior zone");
            eicWall.SurfaceArea = 4 * 5;
            outdoorSurfaces.Add(eicWall.GetSurface(true));
            eiZone.AddSurface(eicWall.GetSurface(false));
            eicWall.SetIncline(hIn, true);

            Wall eifWall = walls[13] = new Wall(wl, "Floor in the east interior zone");
            eifWall.SurfaceArea = 4 * 5;
            outdoor.AddGroundWallSurface(eifWall.GetSurface(true));
            eiZone.AddSurface(eifWall.GetSurface(false));

            Wall cpWall = walls[14] = new Wall(wl, "Inner wall at perimeter");
            cpWall.SurfaceArea = 3 * 3;
            wpZone.AddSurface(cpWall.GetSurface(true));
            epZone.AddSurface(cpWall.GetSurface(false));

            Wall ciWall = walls[15] = new Wall(wl, "Inner wall at interior");
            ciWall.SurfaceArea = 4 * 3;
            wiZone.AddSurface(ciWall.GetSurface(true));
            eiZone.AddSurface(ciWall.GetSurface(false));

            Wall wpsWall = walls[16] = new Wall(wl, "South wall in the west perimeter zone");
            wpsWall.SurfaceArea = 5 * 3 - 3 * 2;    //Reduce window surface area
            outdoorSurfaces.Add(wpsWall.GetSurface(true));
            wpZone.AddSurface(wpsWall.GetSurface(false));
            wpsWall.SetIncline(sIn, true);

            Wall epsWall = walls[17] = new Wall(wl, "South wall in the east perimeter zone");
            epsWall.SurfaceArea = 5 * 3 - 3 * 2;    //Reduce window surface area
            outdoorSurfaces.Add(epsWall.GetSurface(true));
            epZone.AddSurface(epsWall.GetSurface(false));
            epsWall.SetIncline(sIn, true);

            //Initialize outdoor surfaces
            foreach (WallSurface ws in outdoorSurfaces)
            {
                //Add wall surfaces to Outdoor object
                outdoor.AddWallSurface(ws);
                //Initialize emissivity of surface
                ws.InitializeEmissivity(WallSurface.SurfaceMaterial.Concrete);
            }

            //Add windows to the west zone
            Window wWind = new Window(gPanes, "Window in the west perimeter zone");
            wWind.SurfaceArea = 3 * 2;
            wpZone.AddWindow(wWind);
            outdoor.AddWindow(wWind);
            //Add windows to the east zone
            Window eWind = new Window(gPanes, "Window in the east perimeter zone");
            eWind.SurfaceArea = 3 * 2;
            //Set horizontal sun shade.
            eWind.Shade = SunShade.MakeHorizontalSunShade(3, 2, 1, 1, 1, 0.5, sIn);
            wpZone.AddWindow(eWind);
            outdoor.AddWindow(eWind);

            //Creat an insances of the Room class and MultiRoom class
            Room eRm = new Room(new Zone[] { epZone, eiZone }); //East room
            Room wRm = new Room(new Zone[] { wpZone, wiZone }); //Weast room
            MultiRoom mRoom = new MultiRoom(new Room[] { eRm, wRm }); //Multi room (east and west rooms)
            mRoom.SetTimeStep(3600);

            //Set ventilation volume
            wpZone.VentilationVolume = 10; //Only west perimeter zone has outdoor air ventilation
            mRoom.SetAirFlow(wpZone, wiZone, 10);
            mRoom.SetAirFlow(epZone, eiZone, 10);
            mRoom.SetAirFlow(eiZone, epZone, 10);

            //Set short wave radiation distribution:60% of short wave is distributed to perimeter floor.
            double sfSum = 0;
            foreach (ISurface isf in eRm.GetSurface()) sfSum += isf.Area;
            sfSum -= epfWall.SurfaceArea;
            foreach (ISurface isf in eRm.GetSurface()) eRm.SetShortWaveRadiationRate(isf, isf.Area / sfSum * 0.4);
            eRm.SetShortWaveRadiationRate(epfWall.GetSurface(false), 0.6);
            sfSum = 0;
            foreach (ISurface isf in wRm.GetSurface()) sfSum += isf.Area;
            sfSum -= wpfWall.SurfaceArea;
            foreach (ISurface isf in wRm.GetSurface()) wRm.SetShortWaveRadiationRate(isf, isf.Area / sfSum * 0.4);
            wRm.SetShortWaveRadiationRate(wpfWall.GetSurface(false), 0.6);

            //Output title wrine to standard output stream
            StreamWriter sWriter = new StreamWriter("AirStateAndHeatLoadTest2.csv");
            foreach (Zone zn in zones) sWriter.Write(zn.Name + "Drybulb temperature[C], " + zn.Name +
                                      "Humidity ratio[kg/kgDA], " + zn.Name + "Sensible heat load[W], " + zn.Name + "Latent heat load[W], ");
            sWriter.WriteLine();

            //Update the state (Iterate 100 times to make state steady)
            for (int i = 0; i < 100; i++)
            {
                DateTime dTime = new DateTime(2007, 8, 3, 0, 0, 0);
                for (int j = 0; j < 24; j++)
                {
                    //Set date and time to Sun and Zone object.
                    sun.Update(dTime);
                    mRoom.SetCurrentDateTime(dTime);

                    //Operate HVAC system (8:00~19:00)
                    bool operating = (8 <= dTime.Hour && dTime.Hour <= 19);
                    foreach (Zone zn in zones)
                    {
                        zn.ControlHumidityRatio = operating;
                        zn.ControlDrybulbTemperature = operating;
                    }

                    //Set weather state.
                    outdoor.AirState = new MoistAir(dbt[j], hum[j]);
                    outdoor.NocturnalRadiation = nrd[j];
                    sun.SetGlobalHorizontalRadiation(drd[j], dnr[j]);

                    //Set ventilation air state.
                    wpZone.VentilationAirState = outdoor.AirState;

                    //Update boundary state of outdoor facing surfaces.
                    outdoor.SetWallSurfaceBoundaryState();

                    //Update the walls.
                    foreach (Wall wal in walls) wal.Update();

                    //Update the MultiRoom object.
                    mRoom.UpdateRoomTemperatures();
                    mRoom.UpdateRoomHumidities();

                    //Update date and time
                    dTime = dTime.AddHours(1);

                    //If it is last iteration, output result to CSV text.
                    if (i == 99)
                    {
                        foreach (Zone zn in zones)
                        {
                            sWriter.Write(zn.CurrentDrybulbTemperature.ToString("F1") + ", " + zn.CurrentHumidityRatio.ToString("F3") + ", " +
                            zn.CurrentSensibleHeatLoad.ToString("F0") + ", " + zn.CurrentLatentHeatLoad.ToString("F0") + ", ");
                        }
                        sWriter.WriteLine();
                    }
                }
            }

            sWriter.Close();
        }

        #endregion

    }
}
