﻿using System;
using System.Collections.Generic;
using System.Text;

using Popolo.ThermophysicalProperty;
using Popolo.CircuitNetwork;
using Popolo.ThermalComfort;
using Popolo.Weather;
using Popolo.ThermalLoad;

namespace SamplePrograms
{
    class Program
    {
        static void Main(string[] args)
        {
            AirFlowWindow afWindow = new AirFlowWindow(new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.TransparentGlass03mm), 0.05,
                new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.TransparentGlass03mm), 0.05, 1, 1);
            afWindow.SetInletAirTemperature(15);
            afWindow.IndoorTemperature = 24;
            afWindow.OutdoorTemperature = 35;

            for (int i = 0; i < 5; i++)
            {
                afWindow.SetAirFlowVolume(100 * (i + 1) * 2);
                Console.WriteLine(afWindow.GetExteriorGlassTemperature().ToString("F3") + ", " +
                    afWindow.GetBlindTemperature().ToString("F3") + ", " +
                    afWindow.GetInteriorGlassTemperature().ToString("F3") + ", " +
                    afWindow.GetExteriorAirFlowTemperature().ToString("F3") + ", " +
                    afWindow.GetInteriorAirFlowTemperature().ToString("F3"));
            }

            Console.WriteLine("*******************");

            //weatherTest();

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

    }
}