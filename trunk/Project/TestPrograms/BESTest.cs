using System;
using System.Text;

using System.IO;

using Popolo.ThermalLoad;
using Popolo.Weather;
using Popolo.Weather.Converter;
using Popolo.ThermophysicalProperty;

namespace Popolo.Utility
{
    static class BESTest
    {

        #region 定数宣言

        private static bool CALCULATE_ELEVATION_EFFECT = true;

        #endregion

        #region 列挙型定義

        /// <summary>テストケース</summary>
        [Flags]
        public enum TestCase : long
        {
            None = 0,
            C195 = 1,
            C200 = 2,
            C210 = 4,
            C215 = 8,
            C220 = 16,
            C230 = 32,
            C240 = 64,
            C250 = 128,
            C270 = 256,
            C280 = 512,
            C290 = 1024,
            C300 = 2048,
            C310 = 4096,
            C320 = 8192,
            C395 = 16384,
            C400 = 32768,
            C410 = C400 * 2,
            C420 = C410 * 2,
            C430 = C420 * 2,
            C440 = C430 * 2,
            C600 = C440 * 2,
            C610 = C600 * 2,
            C620 = C610 * 2,
            C630 = C620 * 2,
            C640 = C630 * 2,
            C650 = C640 * 2,
            C800 = C650 * 2,
            C810 = C800 * 2,
            C900 = C810 * 2,
            C910 = C900 * 2,
            C920 = C910 * 2,
            C930 = C920 * 2,
            C940 = C930 * 2,
            C950 = C940 * 2,
            C960 = C950 * 2,
            C990 = C960 * 2,
            C600FF = C990 * 2,
            C650FF = C600FF * 2,
            C900FF = C650FF * 2,
            C950FF = C900FF * 2,
            ControlBangBang = C195 | C200 | C210 | C215 | C220 | C230 | C240 | C250 | C270 | C280 | C290 | C300 | C310,
            ControlDeadBand = C320 | C395 | C400 | C410 | C420 | C430 | C440 | C600 | C610 | C620 | C630 | C800 | C810 | C900 | C910 | C920 | C930 | C960 | C990,
            ControlSetBack = C640 | C940,
            ControlVenting = C650 | C950,
            ControlNone = C600FF | C650FF | C900FF | C950FF,
            HeavyWeight = C800 | C810 | C900 | C910 | C920 | C930 | C940 | C950 | C900FF | C950FF | C990,
            HasHeatGain = C240 | C420 | C430 | C440 | C600 | C610 | C620 | C630 | C640 | C650 |
                C800 | C810 | C900 | C910 | C920 | C930 | C940 | C950 | C990 | C600FF | C650FF | C900FF | C950FF,
            HasHighConcuctanceWall = C200 | C210 | C215 | C220 | C230 | C240 | C250 | C400 | C410 | C420 | C430 | C800,
            NoInfiltration = C195 | C200 | C210 | C215 | C220 | C240 | C250 | C270 | C280 |
                C290 | C300 | C310 | C320 | C395 | C395 | C400,
            LowIntIREmissivity = C195 | C200 | C210,
            LowExtIREmissivity = C195 | C200 | C215,
            LowIntSWEmissivity = C280 | C440 | C810,
            HighIntSWEmissivity = C270 | C290 | C300 | C310 | C320,
            LowExtSWEmissivity = C195 | C200 | C210 | C215 | C220 | C230 | C240 | C270 | C280 |
                C290 | C300 | C310 | C320 | C395 | C400 | C410 | C420,
            NoWindow = C195 | C395,
            HasHighConductanceWall = C210 | C215 | C220 | C230 | C240 | C250 | C400 | C410 | C420 | C430 | C440 | C800,
            HasEWWindow = C300 | C310 | C620 | C630 | C920 | C930,
            HasSunShade = C290 | C310 | C610 | C630 | C910 | C930 | C990,
        }

        #endregion

        #region テスト処理

        public static void Test(TestCase testCase, string weatherDataPath, string outputFilePath)
        {
            bool isBangBang = ((testCase & TestCase.ControlBangBang) == testCase);
            bool isDeadBand = ((testCase & TestCase.ControlDeadBand) == testCase);
            bool isFreeFloat = ((testCase & TestCase.ControlNone) == testCase);
            bool isSetBack = ((testCase & TestCase.ControlSetBack) == testCase);
            bool isVenting27 = ((testCase & TestCase.ControlVenting) == testCase);
            bool noInfiltration = ((testCase & TestCase.NoInfiltration) == testCase); 

            //モデルを作成
            Zone[] rooms;
            Wall[] walls;
            Window[] windows;
            Outdoor[] outdoor;
            Sun sun;
            makeBuilding(testCase, out rooms, out walls, out windows, out outdoor, out sun);

            //読み込み書き出し処理
            using (StreamReader sReader = new StreamReader(weatherDataPath))
            using (StreamWriter sWriter = new StreamWriter(outputFilePath, false, Encoding.GetEncoding("Shift_JIS")))
            {

                //書き出し1行目
                sWriter.Write("日付");
                //壁表面への入射量[W/m2]
                ImmutableSurface[] wss = outdoor[0].WallSurfaces;
                for (int i = 0; i < Math.Min(5, wss.Length); i++) sWriter.Write("," + wss[i].Name);
                if (testCase == TestCase.C990) sWriter.Write(",");
                sWriter.Write(",室乾球温度[C],顕熱負荷[W],窓1透過日射[W/m2],窓2透過日射[W/m2]");
                if (testCase == TestCase.C960) sWriter.Write(",SunZone室温[C]");
                sWriter.WriteLine();

                string sBuff;
                string[] strs;
                sReader.ReadLine();
                double atm = MoistAir.GetAtmosphericPressure(1609); //大気圧[kPa]
                if (!CALCULATE_ELEVATION_EFFECT) atm = 101.325;
                DateTime dt = new DateTime(1999, 1, 1, 0, 30, 0);
                double prevDBT = 0;
                double prevAHD = 0.00264185858928463;
                bool isStarting = true;
                while ((sBuff = sReader.ReadLine()) != null)
                {
                    sWriter.Write(dt.ToString());

                    strs = sBuff.Split(',');
                    double dbt = (double.Parse(strs[2]) + prevDBT) / 2d;
                    double ahd = (double.Parse(strs[3]) + prevAHD) / 2d;
                    double iDn = double.Parse(strs[4]);
                    double iHol = double.Parse(strs[5]);
                    double iSky = double.Parse(strs[6]);
                    double nr = double.Parse(strs[8]);
                    double gdbt1 = double.Parse(strs[10]);  //地中温度（0.675m）
                    double gdbt2 = double.Parse(strs[12]);  //地中温度（2.65m）
                    prevDBT = double.Parse(strs[2]);
                    prevAHD = double.Parse(strs[3]);

                    //外気状態作成
                    outdoor[0].AirState = MoistAir.GetAirStateFromDBHR(dbt, ahd, atm);
                    if (outdoor[1] != null) outdoor[1].AirState = outdoor[0].AirState;
                    foreach (Zone rm in rooms) rm.VentilationAirState = outdoor[0].AirState;
                    if (testCase == TestCase.C990)
                    {
                        outdoor[0].GroundTemperature = gdbt1;
                        outdoor[1].GroundTemperature = gdbt2;
                    }

                    //換気量[m3/h]
                    if (testCase == TestCase.C230) rooms[0].VentilationVolume = rooms[0].Volume;
                    else if (noInfiltration) rooms[0].VentilationVolume = 0;
                    else if ((isVenting27 || (testCase == TestCase.C650FF) || (testCase == TestCase.C950FF))
                        && (dt.Hour < 7 || 18 <= dt.Hour))
                    {
                        rooms[0].VentilationVolume = rooms[0].Volume + 1703.14;
                    }
                    else rooms[0].VentilationVolume = rooms[0].Volume * 0.5;
                    if (!CALCULATE_ELEVATION_EFFECT) rooms[0].VentilationVolume *= 0.82;

                    //太陽の情報を更新
                    sun.Update(dt);
                    //日の出の場合の補正
                    if (sun.SunRiseTime.Hour == sun.CurrentDateTime.Hour)
                    {
                        sun.Update(sun.CurrentDateTime.AddMinutes(30));
                    }
                    //日没の場合の補正
                    if (sun.SunSetTime.Hour == sun.CurrentDateTime.Hour) sun.Update(sun.CurrentDateTime.AddMinutes(-30));
                    sun.DirectNormalRadiation = iDn;
                    sun.DiffuseHorizontalRadiation = iSky;
                    sun.GlobalHorizontalRadiation = iHol;

                    //相当外気温度を更新
                    outdoor[0].SetWallSurfaceBoundaryState();
                    outdoor[0].NocturnalRadiation = nr;
                    if (outdoor[1] != null)
                    {
                        outdoor[1].SetWallSurfaceBoundaryState();
                        outdoor[1].NocturnalRadiation = nr;
                    }

                    //斜面への入射量[W/m2]を計算
                    for (int i = 0; i < Math.Min(5, wss.Length); i++) sWriter.Write("," + outdoor[0].GetRadiationToIncline(wss[i].Incline, wss[i].Albedo, 0));
                    if (testCase == TestCase.C990) sWriter.Write(",");

                    //予備計算
                    if (isStarting)
                    {
                        //22度で安定させる
                        foreach (Zone room in rooms)
                        {
                            room.ControlDrybulbTemperature = true;
                            room.DrybulbTemperatureSetPoint = 20;
                        }
                        //予備計算を24時間行う
                        for (int i = 0; i < 24; i++)
                        {
                            foreach (Wall wall in walls) wall.Update();
                            foreach (Zone room in rooms) room.Update();
                        }
                        isStarting = false;

                        foreach (Zone room in rooms) room.ControlDrybulbTemperature = false;
                    }


                    //壁の熱流CFを更新
                    foreach (Wall wall in walls) wall.Update();

                    //室制御
                    double rmDbt = rooms[0].GetNextDrybulbTemperature(0);
                    if (isBangBang)
                    {
                        rooms[0].DrybulbTemperatureSetPoint = 20;
                        rooms[0].ControlDrybulbTemperature = true;
                    }
                    else if(isDeadBand)
                    {
                        rooms[0].ControlDrybulbTemperature = true;
                        if (rmDbt < 20) rooms[0].DrybulbTemperatureSetPoint = 20;
                        else if (27 < rmDbt) rooms[0].DrybulbTemperatureSetPoint = 27;
                        else rooms[0].ControlDrybulbTemperature = false;
                    }
                    else if (isSetBack)
                    {
                        rooms[0].ControlDrybulbTemperature = true;
                        if (27 < rmDbt) rooms[0].DrybulbTemperatureSetPoint = 27;
                        else if ((7 <= dt.Hour && dt.Hour < 23) && rmDbt < 20) rooms[0].DrybulbTemperatureSetPoint = 20;
                        else if (rmDbt < 10) rooms[0].DrybulbTemperatureSetPoint = 10;
                        else rooms[0].ControlDrybulbTemperature = false;
                    }
                    else if (isVenting27)
                    {
                        rooms[0].ControlDrybulbTemperature = false;
                        if (7 <= dt.Hour && dt.Hour < 18 && 27 < rmDbt)
                        {
                            rooms[0].DrybulbTemperatureSetPoint = 27;
                            rooms[0].ControlDrybulbTemperature = true;
                        }
                    }
                    else rooms[0].ControlDrybulbTemperature = false;

                    //室を更新
                    foreach (Zone room in rooms) room.Update();

                    sWriter.Write("," + rooms[0].CurrentDrybulbTemperature + "," +rooms[0].CurrentSensibleHeatLoad);
                    if (0 < windows.Length)
                    {
                        sWriter.Write("," + (windows[0].TransmissionHeatGain / windows[0].SurfaceArea) +
                          "," + (windows[1].TransmissionHeatGain / windows[1].SurfaceArea));
                    }
                    else
                    {
                        sWriter.Write(",-,-");
                    }
                    if (testCase == TestCase.C960) sWriter.Write("," + rooms[1].CurrentDrybulbTemperature);

                    sWriter.WriteLine();

                    dt = dt.AddHours(1);
                }
            }
        }

        #endregion

        #region 建物の作成処理

        private static void makeBuilding(TestCase tCase, out Zone[] rooms, out Wall[] walls, out Window[] windows, out Outdoor[] outdoor, out Sun sun)
        {
            if (tCase == TestCase.C960)
            {
                makeSunZoneBuilding(out rooms, out walls, out windows, out outdoor, out sun);
                return;
            }
            else if (tCase == TestCase.C990)
            {
                makeGroundCouplingBuilding(out rooms, out walls, out windows, out outdoor, out sun);
                return;
            }

            bool hasEWWindow = (tCase & TestCase.HasEWWindow) == tCase;
            bool hasSunShade = (tCase & TestCase.HasSunShade) == tCase;
            bool hasHeatGain = (tCase & TestCase.HasHeatGain) == tCase;
            bool hasHighConcuctanceWall = (tCase & TestCase.HasHighConcuctanceWall) == tCase;
            bool isLowIntIREmissivity = (tCase & TestCase.LowIntIREmissivity) == tCase;
            bool isLowExtIREmissivity = (tCase & TestCase.LowExtIREmissivity) == tCase;
            bool isLowIntSWEmissivity = (tCase & TestCase.LowIntSWEmissivity) == tCase;
            bool isHighIntSWEmissivity = (tCase & TestCase.HighIntSWEmissivity) == tCase;
            bool noInfiltration = (tCase & TestCase.NoInfiltration) == tCase;
            bool isLowExtSWEmissivity = (tCase & TestCase.LowExtSWEmissivity) == tCase;
            bool isHeavyWeight = (tCase & TestCase.HeavyWeight) == tCase;
            bool noWindow = (tCase & TestCase.NoWindow) == tCase;

            //放射率
            double extswEmissivity, extlwEmissivity;
            if (isLowExtIREmissivity) extlwEmissivity = 0.1;
            else extlwEmissivity = 0.9;
            if (tCase == TestCase.C250) extswEmissivity = 0.9;
            else if (isLowExtSWEmissivity) extswEmissivity = 0.1;
            else extswEmissivity = 0.6;

            //表面熱伝達率
            double ao, aowin, ai;
            if (isLowExtIREmissivity)
            {
                ao = 25.2;
                aowin = 16.9;
            }
            else
            {
                ao = 29.3;
                aowin = 21;
            }
            if (isLowIntIREmissivity) ai = 3.73;
            else ai = 8.29;

            //室を作成
            rooms = new Zone[1];
            rooms[0] = new Zone();
            rooms[0].Volume = 8 * 6 * 2.7;  //室容積[m3]
            //内部負荷[W]
            if (hasHeatGain) rooms[0].AddHeatGain(new ConstantHeatGain(200 * 0.4, 200 * 0.6, 0));
            if (CALCULATE_ELEVATION_EFFECT) rooms[0].AtmosphericPressure = MoistAir.GetAtmosphericPressure(1609);
            else rooms[0].AtmosphericPressure = 101.325d;
            rooms[0].TimeStep = 3600;
            rooms[0].InitializeAirState(20, 0.01);
            rooms[0].FilmCoefficient = ai;
            //対流成分
            rooms[0].SetConvectiveRate(3.16 / ai);
            //漏気量
            if (tCase == TestCase.C230) rooms[0].VentilationVolume = rooms[0].Volume;
            else if (noInfiltration) rooms[0].VentilationVolume = 0;
            else rooms[0].VentilationVolume = rooms[0].Volume * 0.5;
            if (! CALCULATE_ELEVATION_EFFECT) rooms[0].VentilationVolume *= 0.82;

            //外界を作成
            outdoor = new Outdoor[2];
            outdoor[0] = new Outdoor();
            outdoor[0].GroundTemperature = 10;
            sun = new Sun(39.8, 360 - 104.9, 360 - 105);
            outdoor[0].Sun = sun;

            //壁構成を作成
            WallLayers exwL, flwL, rfwL;
            makeWallLayer(!isHeavyWeight, out exwL, out flwL, out rfwL);

            //壁表面を作成
            WallSurface ews, iws;

            //壁リストを作成
            if (hasHighConcuctanceWall) walls = new Wall[7];
            else walls = new Wall[6];
            //屋根を作成
            walls[0] = new Wall(rfwL);
            walls[0].Name = "屋根";
            walls[0].SurfaceArea = 48;
            walls[0].SetIncline(new Incline(Incline.Orientation.N, 0), false);
            walls[0].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[0].SetFilmCoefficient(ao, false);//外表面総合熱伝達率[W/(m2K)]
            walls[0].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[0].GetSurface(true);
            ews = walls[0].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            rooms[0].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //床を作成
            walls[1] = new Wall(flwL);
            walls[1].Name = "床";
            walls[1].SurfaceArea = 48;
            walls[1].SetIncline(new Incline(Incline.Orientation.N, Math.PI), false);
            walls[1].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[1].SetFilmCoefficient(0.04, false);//外表面総合熱伝達率[W/(m2K)]//地面絶縁体
            walls[1].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[1].GetSurface(true);
            ews = walls[1].GetSurface(false);
            rooms[0].AddSurface(iws);
            outdoor[0].AddGroundWallSurface(ews);

            //北外壁を作成
            walls[2] = new Wall(exwL);
            walls[2].Name = "北外壁";
            walls[2].SurfaceArea = 8 * 2.7;
            walls[2].SetIncline(new Incline(Incline.Orientation.N, 0.5 * Math.PI), false);
            walls[2].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[2].SetFilmCoefficient(ao, false);//外表面総合熱伝達率[W/(m2K)]
            walls[2].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[2].GetSurface(true);
            ews = walls[2].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            ews.Albedo = 0.2;
            rooms[0].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //東外壁を作成
            walls[3] = new Wall(exwL);
            walls[3].Name = "東外壁";
            if (hasEWWindow) walls[3].SurfaceArea = 6 * 2.7 - 6;
            else walls[3].SurfaceArea = 6 * 2.7;
            walls[3].SetIncline(new Incline(Incline.Orientation.E, 0.5 * Math.PI), false);
            walls[3].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[3].SetFilmCoefficient(ao, false);//外表面総合熱伝達率[W/(m2K)]
            walls[3].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[3].GetSurface(true);
            ews = walls[3].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            ews.Albedo = 0.2;
            rooms[0].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //西外壁を作成
            walls[4] = new Wall(exwL);
            walls[4].Name = "西外壁";
            if (hasEWWindow) walls[4].SurfaceArea = 6 * 2.7 - 6;
            else walls[4].SurfaceArea = 6 * 2.7;
            walls[4].SetIncline(new Incline(Incline.Orientation.W, 0.5 * Math.PI), false);
            walls[4].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[4].SetFilmCoefficient(ao, false);//外表面総合熱伝達率[W/(m2K)]
            walls[4].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[4].GetSurface(true);
            ews = walls[4].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            ews.Albedo = 0.2;
            rooms[0].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //南外壁を作成
            walls[5] = new Wall(exwL);
            walls[5].Name = "南外壁";
            if (noWindow || hasEWWindow) walls[5].SurfaceArea = 8 * 2.7;
            else walls[5].SurfaceArea = 8 * 2.7 - 6d - 6d;
            walls[5].SetIncline(new Incline(Incline.Orientation.S, 0.5 * Math.PI), false);
            walls[5].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[5].SetFilmCoefficient(ao, false);//外表面総合熱伝達率[W/(m2K)]
            walls[5].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[5].GetSurface(true);
            ews = walls[5].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            ews.Albedo = 0.2;
            rooms[0].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //窓を作成
            if (!noWindow && !hasHighConcuctanceWall)
            {
                GlassPanes glassPane = new GlassPanes(0.74745, 0.043078, 1d / (1d / 333 + 1d / 333 + 1d / 6.297));
                //glassPane.LongWaveEmissivity = extlwEmissivity;
                //glassPane.ConvectiveRate = 3.16 / ai;
                glassPane.AngularDependenceCoefficients = new double[] { 1.3930, 5.5401, -19.5736, 19.0379 };
                Window window1 = new Window(glassPane);
                Window window2 = new Window(glassPane);
                //長波長吸収率・アルベド
                WindowSurface ws;
                ws = window1.GetSurface(true);
                ws.LongWaveEmissivity = extlwEmissivity;
                ws.FilmCoefficient = aowin;//表面総合熱伝達率[W/(m2K)]
                ws.Albedo = 0.2;
                ws = window2.GetSurface(true);
                ws.LongWaveEmissivity = extlwEmissivity;
                ws.FilmCoefficient = aowin;//表面総合熱伝達率[W/(m2K)]
                ws.Albedo = 0.2;
                //対流・放射成分
                ws = window1.GetSurface(false);
                ws.ConvectiveRate = 3.16 / ai;
                ws.FilmCoefficient = ai;//表面総合熱伝達率[W/(m2K)]
                ws = window2.GetSurface(false);
                ws.ConvectiveRate = 3.16 / ai;
                ws.FilmCoefficient = ai;//表面総合熱伝達率[W/(m2K)]
                //窓面積
                window1.SurfaceArea = 6;
                window2.SurfaceArea = 6;
                
                if (hasEWWindow)
                {
                    window1.OutSideIncline = new Incline(Incline.Orientation.E, 0.5 * Math.PI);
                    window2.OutSideIncline = new Incline(Incline.Orientation.W, 0.5 * Math.PI);
                    if (hasSunShade)
                    {
                        window1.Shade = SunShade.MakeGridSunShade(3, 2, 1, 0, 0, 0, 0, window1.OutSideIncline);
                        window2.Shade = SunShade.MakeGridSunShade(3, 2, 1, 0, 0, 0, 0, window2.OutSideIncline);
                    }
                }
                else
                {
                    window1.OutSideIncline = new Incline(Incline.Orientation.S, 0.5 * Math.PI);
                    window2.OutSideIncline = new Incline(Incline.Orientation.S, 0.5 * Math.PI);
                    if (hasSunShade)
                    {
                        window1.Shade = SunShade.MakeHorizontalSunShade(3, 2, 1, 4.5, 0.5, 0.5, window1.OutSideIncline);
                        window2.Shade = SunShade.MakeHorizontalSunShade(3, 2, 1, 0.5, 4.5, 0.5, window2.OutSideIncline);
                    }
                }
                //室と外界に追加
                rooms[0].AddWindow(window1);
                rooms[0].AddWindow(window2);
                outdoor[0].AddWindow(window1);
                outdoor[0].AddWindow(window2);
                windows = new Window[] { window1, window2 };
            }
            else windows = new Window[0];

            //HighConductanceWallを作成
            if (hasHighConcuctanceWall)
            {
                walls[6] = new Wall(makeHighConductanceWall());
                walls[6].SurfaceArea = 12;
                walls[6].SetIncline(new Incline(Incline.Orientation.S, 0), false);
                walls[6].SetFilmCoefficient(ai, true);
                walls[6].SetFilmCoefficient(aowin, false);
                walls[6].InitializeTemperature(25);
                //壁表面の設定
                iws = walls[6].GetSurface(true);
                ews = walls[6].GetSurface(false);
                ews.SolarAbsorptance = extswEmissivity;
                ews.LongWaveEmissivity = extlwEmissivity;
                rooms[0].AddSurface(iws);
                outdoor[0].AddWallSurface(ews);
            }

            //短波長放射入射比率を設定
            if (hasEWWindow)
            {
                if (isLowIntSWEmissivity)
                {
                    rooms[0].SetShortWaveRadiationRate(walls[1].GetSurface(true), 0.642);   //床面
                    rooms[0].SetShortWaveRadiationRate(walls[0].GetSurface(true), 0.168);   //天井面
                    rooms[0].SetShortWaveRadiationRate(walls[3].GetSurface(true), 0.025);   //東面
                    rooms[0].SetShortWaveRadiationRate(walls[4].GetSurface(true), 0.025);   //西面
                    rooms[0].SetShortWaveRadiationRate(walls[2].GetSurface(true), 0.0525);   //北面
                    rooms[0].SetShortWaveRadiationRate(walls[5].GetSurface(true), 0.0525);   //南面
                    if (0 < windows.Length)
                    {
                        rooms[0].SetShortWaveRadiationRate(windows[0], 0.0175);
                        rooms[0].SetShortWaveRadiationRate(windows[1], 0.0175);
                    }
                }
                else
                {
                    rooms[0].SetShortWaveRadiationRate(walls[1].GetSurface(true), 0.651);   //床面
                    rooms[0].SetShortWaveRadiationRate(walls[0].GetSurface(true), 0.177);   //天井面
                    rooms[0].SetShortWaveRadiationRate(walls[3].GetSurface(true), 0.027);   //東面
                    rooms[0].SetShortWaveRadiationRate(walls[4].GetSurface(true), 0.027);   //西面
                    rooms[0].SetShortWaveRadiationRate(walls[2].GetSurface(true), 0.056);   //北面
                    rooms[0].SetShortWaveRadiationRate(walls[5].GetSurface(true), 0.056);   //南面
                    if (0 < windows.Length)
                    {
                        rooms[0].SetShortWaveRadiationRate(windows[0], 0.003);
                        rooms[0].SetShortWaveRadiationRate(windows[1], 0.003);
                    }
                }
            }
            else
            {
                if (isLowIntSWEmissivity)
                {
                    rooms[0].SetShortWaveRadiationRate(walls[1].GetSurface(true), 0.244);   //床面
                    rooms[0].SetShortWaveRadiationRate(walls[0].GetSurface(true), 0.192);   //天井面
                    rooms[0].SetShortWaveRadiationRate(walls[3].GetSurface(true), 0.057);   //東面
                    rooms[0].SetShortWaveRadiationRate(walls[4].GetSurface(true), 0.057);   //西面
                    rooms[0].SetShortWaveRadiationRate(walls[2].GetSurface(true), 0.082);   //北面
                    rooms[0].SetShortWaveRadiationRate(walls[5].GetSurface(true), 0.065);   //南面
                    if (0 < windows.Length)
                    {
                        rooms[0].SetShortWaveRadiationRate(windows[0], 0.152);
                        rooms[0].SetShortWaveRadiationRate(windows[1], 0.152);
                    }
                }
                else if (isHighIntSWEmissivity)
                {
                    rooms[0].SetShortWaveRadiationRate(walls[1].GetSurface(true), 0.651);   //床面
                    rooms[0].SetShortWaveRadiationRate(walls[0].GetSurface(true), 0.177);   //天井面
                    rooms[0].SetShortWaveRadiationRate(walls[3].GetSurface(true), 0.041);   //東面
                    rooms[0].SetShortWaveRadiationRate(walls[4].GetSurface(true), 0.041);   //西面
                    rooms[0].SetShortWaveRadiationRate(walls[2].GetSurface(true), 0.056);   //北面
                    rooms[0].SetShortWaveRadiationRate(walls[5].GetSurface(true), 0.028);   //南面
                    if (0 < windows.Length)
                    {
                        rooms[0].SetShortWaveRadiationRate(windows[0], 0.003);
                        rooms[0].SetShortWaveRadiationRate(windows[1], 0.003);
                    }
                }
                else
                {
                    rooms[0].SetShortWaveRadiationRate(walls[1].GetSurface(true), 0.642);   //床面
                    rooms[0].SetShortWaveRadiationRate(walls[0].GetSurface(true), 0.168);   //天井面
                    rooms[0].SetShortWaveRadiationRate(walls[3].GetSurface(true), 0.038);   //東面
                    rooms[0].SetShortWaveRadiationRate(walls[4].GetSurface(true), 0.038);   //西面
                    rooms[0].SetShortWaveRadiationRate(walls[2].GetSurface(true), 0.053);   //北面
                    rooms[0].SetShortWaveRadiationRate(walls[5].GetSurface(true), 0.026);   //南面
                    if (0 < windows.Length)
                    {
                        rooms[0].SetShortWaveRadiationRate(windows[0], 0.026);
                        rooms[0].SetShortWaveRadiationRate(windows[1], 0.026);
                    }
                }
            }

            //対流成分設定
            rooms[0].SetConvectiveRate(3.16 / ai);
            outdoor[0].SetConvectiveRate(24.67 / ao);
            //HighConductanceWallの対流成分
            if (hasHighConcuctanceWall)
            {
                ews = walls[6].GetSurface(false);
                ews.ConvectiveRate = 16.37 / aowin;
            }
        }

        private static void makeSunZoneBuilding(out Zone[] rooms, out Wall[] walls, out Window[] windows, out Outdoor[] outdoor, out Sun sun)
        {
            const double ai = 8.29;
            const double ao = 29.3;
            const double aowin = 21;
            const double extswEmissivity = 0.6d;
            const double extlwEmissivity = 0.9d;

            //室を作成
            rooms = new Zone[2];

            //BackZoneを作成
            rooms[0] = new Zone();
            rooms[0].Volume = 8 * 6 * 2.7;  //室容積[m3]
            //内部負荷[W]
            rooms[0].AddHeatGain(new ConstantHeatGain(200 * 0.4, 200 * 0.6, 0));
            if (CALCULATE_ELEVATION_EFFECT) rooms[0].AtmosphericPressure = MoistAir.GetAtmosphericPressure(1609);
            else rooms[0].AtmosphericPressure = 101.325d;
            rooms[0].TimeStep = 3600;
            rooms[0].InitializeAirState(20, 0.01);
            rooms[0].FilmCoefficient = ai;
            //対流成分
            rooms[0].SetConvectiveRate(3.16 / ai);
            //漏気量
            rooms[0].VentilationVolume = rooms[0].Volume * 0.5;
            if (!CALCULATE_ELEVATION_EFFECT) rooms[0].Volume *= 0.82;

            //SunZoneを作成
            rooms[1] = new Zone();
            rooms[1].Volume = 8 * 2 * 2.7;  //室容積[m3]
            if (CALCULATE_ELEVATION_EFFECT) rooms[1].AtmosphericPressure = MoistAir.GetAtmosphericPressure(1609);
            else rooms[1].AtmosphericPressure = 101.325d;
            rooms[1].TimeStep = 3600;
            rooms[1].InitializeAirState(20, 0.01);
            rooms[1].FilmCoefficient = ai;
            //対流成分
            rooms[1].SetConvectiveRate(3.16 / ai);
            //漏気量
            rooms[1].VentilationVolume = rooms[1].Volume * 0.5;
            if (!CALCULATE_ELEVATION_EFFECT) rooms[1].Volume *= 0.82;

            //外界を作成
            outdoor = new Outdoor[2];
            outdoor[0] = new Outdoor();
            outdoor[0].GroundTemperature = 10;
            sun = new Sun(39.8, 360 - 104.9, 360 - 105);
            outdoor[0].Sun = sun;

            //壁構成を作成
            WallLayers exwLL, flwLL, rfwLL, exwLH, flwLH, rfwLH;
            makeWallLayer(true, out exwLL, out flwLL, out rfwLL);
            makeWallLayer(true, out exwLH, out flwLH, out rfwLH);

            //壁表面を作成
            WallSurface ews, iws;

            //壁を作成
            walls = new Wall[11];

            //屋根を作成1
            walls[0] = new Wall(rfwLL);
            walls[0].Name = "屋根";
            walls[0].SurfaceArea = 48;
            walls[0].SetIncline(new Incline(Incline.Orientation.N, 0), false);
            walls[0].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[0].SetFilmCoefficient(ao, false);//外表面総合熱伝達率[W/(m2K)]
            walls[0].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[0].GetSurface(true);
            ews = walls[0].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            rooms[0].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //屋根を作成2
            walls[1] = new Wall(rfwLH);
            walls[1].Name = "屋根";
            walls[1].SurfaceArea = 16;
            walls[1].SetIncline(new Incline(Incline.Orientation.N, 0), false);
            walls[1].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[1].SetFilmCoefficient(ao, false);//外表面総合熱伝達率[W/(m2K)]
            walls[1].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[1].GetSurface(true);
            ews = walls[1].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            rooms[1].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //床を作成1
            walls[2] = new Wall(flwLL);
            walls[2].Name = "床";
            walls[2].SurfaceArea = 48;
            walls[2].SetIncline(new Incline(Incline.Orientation.N, Math.PI), false);
            walls[2].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[2].SetFilmCoefficient(0.04, false);//外表面総合熱伝達率[W/(m2K)]//地面絶縁体
            walls[2].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[2].GetSurface(true);
            ews = walls[2].GetSurface(false);
            rooms[0].AddSurface(iws);
            outdoor[0].AddGroundWallSurface(ews);

            //床を作成2
            walls[3] = new Wall(flwLH);
            walls[3].Name = "床";
            walls[3].SurfaceArea = 8 * 2;
            walls[3].SetIncline(new Incline(Incline.Orientation.N, Math.PI), false);
            walls[3].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[3].SetFilmCoefficient(0.04, false);//外表面総合熱伝達率[W/(m2K)]//地面絶縁体
            walls[3].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[3].GetSurface(true);
            ews = walls[3].GetSurface(false);
            rooms[1].AddSurface(iws);
            outdoor[0].AddGroundWallSurface(ews);

            //北外壁を作成
            walls[4] = new Wall(exwLL);
            walls[4].Name = "北外壁";
            walls[4].SurfaceArea = 8 * 2.7;
            walls[4].SetIncline(new Incline(Incline.Orientation.N, 0.5 * Math.PI), false);
            walls[4].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[4].SetFilmCoefficient(ao, false);//外表面総合熱伝達率[W/(m2K)]
            walls[4].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[4].GetSurface(true);
            ews = walls[4].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            ews.Albedo = 0.2;
            rooms[0].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //南外壁を作成
            walls[5] = new Wall(exwLH);
            walls[5].Name = "南外壁";
            walls[5].SurfaceArea = 8 * 2.7 - 6d - 6d;
            walls[5].SetIncline(new Incline(Incline.Orientation.S, 0.5 * Math.PI), false);
            walls[5].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[5].SetFilmCoefficient(ao, false);//外表面総合熱伝達率[W/(m2K)]
            walls[5].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[5].GetSurface(true);
            ews = walls[5].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            ews.Albedo = 0.2;
            rooms[1].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //東外壁を作成1
            walls[6] = new Wall(exwLL);
            walls[6].Name = "東外壁";
            walls[6].SurfaceArea = 6 * 2.7;
            walls[6].SetIncline(new Incline(Incline.Orientation.E, 0.5 * Math.PI), false);
            walls[6].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[6].SetFilmCoefficient(ao, false);//外表面総合熱伝達率[W/(m2K)]
            walls[6].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[6].GetSurface(true);
            ews = walls[6].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            ews.Albedo = 0.2;
            rooms[0].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //東外壁を作成2
            walls[7] = new Wall(exwLH);
            walls[7].Name = "東外壁";
            walls[7].SurfaceArea = 2 * 2.7;
            walls[7].SetIncline(new Incline(Incline.Orientation.E, 0.5 * Math.PI), false);
            walls[7].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[7].SetFilmCoefficient(ao, false);//外表面総合熱伝達率[W/(m2K)]
            walls[7].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[7].GetSurface(true);
            ews = walls[7].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            ews.Albedo = 0.2;
            rooms[1].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //西外壁を作成1
            walls[8] = new Wall(exwLL);
            walls[8].Name = "西外壁";
            walls[8].SurfaceArea = 6 * 2.7;
            walls[8].SetIncline(new Incline(Incline.Orientation.W, 0.5 * Math.PI), false);
            walls[8].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[8].SetFilmCoefficient(ao, false);//外表面総合熱伝達率[W/(m2K)]
            walls[8].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[8].GetSurface(true);
            ews = walls[8].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            ews.Albedo = 0.2;
            rooms[0].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //西外壁を作成2
            walls[9] = new Wall(exwLH);
            walls[9].Name = "西外壁";
            walls[9].SurfaceArea = 2 * 2.7;
            walls[9].SetIncline(new Incline(Incline.Orientation.W, 0.5 * Math.PI), false);
            walls[9].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[9].SetFilmCoefficient(ao, false);//外表面総合熱伝達率[W/(m2K)]
            walls[9].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[9].GetSurface(true);
            ews = walls[9].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            ews.Albedo = 0.2;
            rooms[1].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //共用壁を作成
            WallLayers.Layer layer = new WallLayers.Layer(new WallMaterial("CommonWall", 0.510, 1400d * 1000d / 1000d), 0.2, 4);
            WallLayers cWL = new WallLayers();
            cWL.AddLayer(layer);
            walls[10] = new Wall(cWL);
            walls[10].Name = "共用壁";
            walls[10].SurfaceArea = 8 * 2.7;
            walls[10].SetIncline(new Incline(Incline.Orientation.S, 0.5 * Math.PI), false);
            walls[10].SetFilmCoefficient(ai, true); //内表面総合熱伝達率[W/(m2K)]
            walls[10].SetFilmCoefficient(ai, false);//内表面総合熱伝達率[W/(m2K)]
            walls[10].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[10].GetSurface(true);
            ews = walls[10].GetSurface(false);
            rooms[0].AddSurface(iws);
            rooms[1].AddSurface(ews);

            //窓の作成
            GlassPanes glassPane = new GlassPanes(0.74745, 0.043078, 1d / (1d / 333 + 1d / 333 + 1d / 6.297));
            //glassPane.LongWaveEmissivity = extlwEmissivity;
            //glassPane.ConvectiveRate = 3.16 / ai;
            glassPane.AngularDependenceCoefficients = new double[] { 1.3930, 5.5401, -19.5736, 19.0379 };
            Window window1 = new Window(glassPane);
            Window window2 = new Window(glassPane);
            //長波長吸収率・アルベド
            WindowSurface ws;
            ws = window1.GetSurface(true);
            ws.LongWaveEmissivity = extlwEmissivity;
            ws.FilmCoefficient = aowin;//表面総合熱伝達率[W/(m2K)]
            ws.Albedo = 0.2;
            ws = window2.GetSurface(true);
            ws.LongWaveEmissivity = extlwEmissivity;
            ws.FilmCoefficient = aowin;//表面総合熱伝達率[W/(m2K)]
            ws.Albedo = 0.2;
            //対流・放射成分
            ws = window1.GetSurface(false);
            ws.ConvectiveRate = 3.16 / ai;
            ws.FilmCoefficient = ai;//表面総合熱伝達率[W/(m2K)]
            ws = window2.GetSurface(false);
            ws.ConvectiveRate = 3.16 / ai;
            ws.FilmCoefficient = ai;//表面総合熱伝達率[W/(m2K)]
            //窓面積
            window1.SurfaceArea = 6;
            window2.SurfaceArea = 6;            
            window1.OutSideIncline = new Incline(Incline.Orientation.S, 0.5 * Math.PI);
            window2.OutSideIncline = new Incline(Incline.Orientation.S, 0.5 * Math.PI);
            //室と外界に追加
            rooms[1].AddWindow(window1);
            rooms[1].AddWindow(window2);
            outdoor[0].AddWindow(window1);
            outdoor[0].AddWindow(window2);
            windows = new Window[] { window1, window2 };

            //放射率
            rooms[1].SetShortWaveRadiationRate(walls[3].GetSurface(true), 0.6);     //床面
            rooms[1].SetShortWaveRadiationRate(walls[1].GetSurface(true), 0.06);    //天井面
            rooms[1].SetShortWaveRadiationRate(walls[7].GetSurface(true), 0.02);    //東面
            rooms[1].SetShortWaveRadiationRate(walls[9].GetSurface(true), 0.02);    //西面
            rooms[1].SetShortWaveRadiationRate(walls[10].GetSurface(false), 0.2);    //北面
            rooms[1].SetShortWaveRadiationRate(walls[5].GetSurface(true), 0.03);    //南面
            rooms[1].SetShortWaveRadiationRate(window1, 0.035);
            rooms[1].SetShortWaveRadiationRate(window2, 0.035);

            rooms[1].ControlDrybulbTemperature = false;

            //対流成分設定
            rooms[0].SetConvectiveRate(3.16 / ai);
            rooms[1].SetConvectiveRate(3.16 / ai);
            outdoor[0].SetConvectiveRate(24.67 / ao);
        }

        private static void makeGroundCouplingBuilding(out Zone[] rooms, out Wall[] walls, out Window[] windows, out Outdoor[] outdoor, out Sun sun)
        {
            //放射率
            double extswEmissivity = 0.9;
            double extlwEmissivity = 0.6;

            //表面熱伝達率
            double ao = 29.3;
            double aowin = 21;
            double ai = 8.29;
            double ago = 100000;

            //室を作成
            rooms = new Zone[1];
            rooms[0] = new Zone();
            rooms[0].Volume = 8 * 6 * 2.7;  //室容積[m3]
            //内部負荷[W]
            rooms[0].AddHeatGain(new ConstantHeatGain(200 * 0.4, 200 * 0.6, 0));
            if (CALCULATE_ELEVATION_EFFECT) rooms[0].AtmosphericPressure = MoistAir.GetAtmosphericPressure(1609);
            else rooms[0].AtmosphericPressure = 101.325d;
            rooms[0].TimeStep = 3600;
            rooms[0].InitializeAirState(20, 0.01);
            rooms[0].FilmCoefficient = ai;
            //対流成分
            rooms[0].SetConvectiveRate(3.16 / ai);
            //漏気量
            rooms[0].VentilationVolume = rooms[0].Volume * 0.5;
            if (!CALCULATE_ELEVATION_EFFECT) rooms[0].Volume *= 0.82;

            //外界を作成
            outdoor = new Outdoor[2];
            outdoor[0] = new Outdoor();
            outdoor[1] = new Outdoor();
            sun = new Sun(39.8, 360 - 104.9, 360 - 105);
            outdoor[0].Sun = sun;

            //壁構成を作成
            WallLayers exwL, flwL, rfwL, grwL;
            makeWallLayer(true, out exwL, out flwL, out rfwL);
            makeGroundWallLayer(out flwL, out grwL);

            //壁表面を作成
            WallSurface ews, iws;

            //壁リストを作成
            walls = new Wall[9];
            //屋根を作成
            walls[0] = new Wall(rfwL);
            walls[0].Name = "屋根";
            walls[0].SurfaceArea = 48;
            walls[0].SetIncline(new Incline(Incline.Orientation.N, 0), false);
            walls[0].SetFilmCoefficient(ai, true);   //内表面総合熱伝達率[W/(m2K)]
            walls[0].SetFilmCoefficient(ao, false);  //外表面総合熱伝達率[W/(m2K)]
            walls[0].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[0].GetSurface(true);
            ews = walls[0].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            rooms[0].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //床を作成
            walls[1] = new Wall(flwL);
            walls[1].Name = "床";
            walls[1].SurfaceArea = 48;
            walls[1].SetIncline(new Incline(Incline.Orientation.N, Math.PI), false);
            walls[1].SetFilmCoefficient(ai, true);       //内表面総合熱伝達率[W/(m2K)]
            walls[1].SetFilmCoefficient(ago, false);    //外表面総合熱伝達率[W/(m2K)]//地面絶縁体
            walls[1].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[1].GetSurface(true);
            ews = walls[1].GetSurface(false);
            rooms[0].AddSurface(iws);
            outdoor[1].AddGroundWallSurface(ews);

            //北外壁を作成
            walls[2] = new Wall(exwL);
            walls[2].Name = "北外壁";
            walls[2].SurfaceArea = 8 * 1.35;
            walls[2].SetIncline(new Incline(Incline.Orientation.N, 0.5 * Math.PI), false);
            walls[2].SetFilmCoefficient(ai, true);   //内表面総合熱伝達率[W/(m2K)]
            walls[2].SetFilmCoefficient(ao, false);  //外表面総合熱伝達率[W/(m2K)]
            walls[2].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[2].GetSurface(true);
            ews = walls[2].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            ews.Albedo = 0.2;
            rooms[0].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //北外壁（土中）を作成
            walls[3] = new Wall(grwL);
            walls[3].Name = "北外壁（土中）";
            walls[3].SurfaceArea = 8 * 1.35;
            walls[3].SetIncline(new Incline(Incline.Orientation.N, 0.5 * Math.PI), false);
            walls[3].SetFilmCoefficient(ai, true);   //内表面総合熱伝達率[W/(m2K)]
            walls[3].SetFilmCoefficient(ago, false); //外表面総合熱伝達率[W/(m2K)]
            walls[3].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[3].GetSurface(true);
            ews = walls[3].GetSurface(false);
            rooms[0].AddSurface(iws);
            outdoor[0].AddGroundWallSurface(ews);

            //東外壁を作成
            walls[4] = new Wall(exwL);
            walls[4].Name = "東外壁";
            walls[4].SurfaceArea = 6 * 1.35;
            walls[4].SetIncline(new Incline(Incline.Orientation.E, 0.5 * Math.PI), false);
            walls[4].SetFilmCoefficient(ai, true);   //内表面総合熱伝達率[W/(m2K)]
            walls[4].SetFilmCoefficient(ao, false);  //外表面総合熱伝達率[W/(m2K)]
            walls[4].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[4].GetSurface(true);
            ews = walls[4].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            ews.Albedo = 0.2;
            rooms[0].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //東外壁（土中）を作成
            walls[5] = new Wall(grwL);
            walls[5].Name = "東外壁（土中）";
            walls[5].SurfaceArea = 6 * 1.35;
            walls[5].SetIncline(new Incline(Incline.Orientation.E, 0.5 * Math.PI), false);
            walls[5].SetFilmCoefficient(ai, true);   //内表面総合熱伝達率[W/(m2K)]
            walls[5].SetFilmCoefficient(ago, false); //外表面総合熱伝達率[W/(m2K)]
            walls[5].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[5].GetSurface(true);
            ews = walls[5].GetSurface(false);
            rooms[0].AddSurface(iws);
            outdoor[0].AddGroundWallSurface(ews);

            //西外壁を作成
            walls[6] = new Wall(exwL);
            walls[6].Name = "西外壁";
            walls[6].SurfaceArea = 6 * 1.35;
            walls[6].SetIncline(new Incline(Incline.Orientation.W, 0.5 * Math.PI), false);
            walls[6].SetFilmCoefficient(ai, true);   //内表面総合熱伝達率[W/(m2K)]
            walls[6].SetFilmCoefficient(ao, false);  //外表面総合熱伝達率[W/(m2K)]
            walls[6].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[6].GetSurface(true);
            ews = walls[6].GetSurface(false);
            ews.SolarAbsorptance = extswEmissivity;
            ews.LongWaveEmissivity = extlwEmissivity;
            ews.Albedo = 0.2;
            rooms[0].AddSurface(iws);
            outdoor[0].AddWallSurface(ews);

            //西外壁（土中）を作成
            walls[7] = new Wall(grwL);
            walls[7].Name = "西外壁（土中）";
            walls[7].SurfaceArea = 6 * 1.35;
            walls[7].SetIncline(new Incline(Incline.Orientation.W, 0.5 * Math.PI), false);
            walls[7].SetFilmCoefficient(ai, true);   //内表面総合熱伝達率[W/(m2K)]
            walls[7].SetFilmCoefficient(ago, false); //外表面総合熱伝達率[W/(m2K)]
            walls[7].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[7].GetSurface(true);
            ews = walls[7].GetSurface(false);
            rooms[0].AddSurface(iws);
            outdoor[0].AddGroundWallSurface(ews);

            //南外壁（土中）
            walls[8] = new Wall(grwL);
            walls[8].Name = "南外壁（土中）";
            walls[8].SurfaceArea = 8 * 1.35;
            walls[8].SetIncline(new Incline(Incline.Orientation.S, 0.5 * Math.PI), false);
            walls[8].SetFilmCoefficient(ai, true);   //内表面総合熱伝達率[W/(m2K)]
            walls[8].SetFilmCoefficient(ago, false); //外表面総合熱伝達率[W/(m2K)]
            walls[8].InitializeTemperature(25);
            //壁表面の設定
            iws = walls[8].GetSurface(true);
            ews = walls[8].GetSurface(false);
            rooms[0].AddSurface(iws);
            outdoor[0].AddGroundWallSurface(ews);

            //窓を作成
            GlassPanes glassPanes = new GlassPanes(0.74745, 0.043078, 1d / (1d / 333 + 1d / 333 + 1d / 6.297));
            //glassPanes.LongWaveEmissivity = extlwEmissivity;
            //glassPanes.ConvectiveRate = 3.16 / ai;
            glassPanes.AngularDependenceCoefficients = new double[] { 1.3930, 5.5401, -19.5736, 19.0379 };
            Window window1 = new Window(glassPanes);
            Window window2 = new Window(glassPanes);
            //長波長吸収率
            WindowSurface ws;
            ws = window1.GetSurface(true);
            ws.LongWaveEmissivity = extlwEmissivity;
            ws.FilmCoefficient = aowin; //表面総合熱伝達率[W/(m2K)]
            ws.Albedo = 0.2;
            ws = window2.GetSurface(true);
            ws.LongWaveEmissivity = extlwEmissivity;
            ws.FilmCoefficient = aowin; //表面総合熱伝達率[W/(m2K)]
            ws.Albedo = 0.2;
            //対流・放射成分
            ws = window1.GetSurface(false);
            ws.FilmCoefficient = ai; //表面総合熱伝達率[W/(m2K)]
            ws.ConvectiveRate = 3.16 / ai;
            ws = window2.GetSurface(false);
            ws.FilmCoefficient = ai; //表面総合熱伝達率[W/(m2K)]
            ws.ConvectiveRate = 3.16 / ai;
            //窓面積
            window1.SurfaceArea = 5.4;
            window2.SurfaceArea = 5.4;
           
            window1.OutSideIncline = new Incline(Incline.Orientation.S, 0.5 * Math.PI);
            window2.OutSideIncline = new Incline(Incline.Orientation.S, 0.5 * Math.PI);
            //室と外界に追加
            rooms[0].AddWindow(window1);
            rooms[0].AddWindow(window2);
            outdoor[0].AddWindow(window1);
            outdoor[0].AddWindow(window2);
            windows = new Window[] { window1, window2 };

            //短波長放射入射比率を設定
            rooms[0].SetShortWaveRadiationRate(walls[1].GetSurface(true), 0.642);           //床面
            rooms[0].SetShortWaveRadiationRate(walls[0].GetSurface(true), 0.168);           //天井面
            rooms[0].SetShortWaveRadiationRate(walls[4].GetSurface(true), 0.038 * 0.5);     //東面
            rooms[0].SetShortWaveRadiationRate(walls[6].GetSurface(true), 0.038 * 0.5);     //西面
            rooms[0].SetShortWaveRadiationRate(walls[2].GetSurface(true), 0.053 * 0.5);     //北面
            rooms[0].SetShortWaveRadiationRate(walls[5].GetSurface(true), 0.038 * 0.5);     //東面（土中）
            rooms[0].SetShortWaveRadiationRate(walls[7].GetSurface(true), 0.038 * 0.5);     //西面（土中）
            rooms[0].SetShortWaveRadiationRate(walls[3].GetSurface(true), 0.053 * 0.5);     //北面（土中）
            rooms[0].SetShortWaveRadiationRate(walls[8].GetSurface(true), 0.026 * 10.8 / 9.6);   //南面（土中）
            if (0 < windows.Length)
            {
                rooms[0].SetShortWaveRadiationRate(windows[0], 0.0175 * 10.8 / 12d);
                rooms[0].SetShortWaveRadiationRate(windows[1], 0.0175 * 10.8 / 12d);
            }

            //対流成分設定
            rooms[0].SetConvectiveRate(3.16 / ai);
            outdoor[0].SetConvectiveRate(24.67 / ao);
        }

        #endregion

        #region 壁構成の作成処理

        /// <summary>壁構成作成処理</summary>
        /// <param name="isLightWeight">軽い壁構成か否か</param>
        /// <param name="exteriorWallLayer">外壁構成</param>
        /// <param name="floorWallLayer">床構成</param>
        /// <param name="roofWallLayer">天井構成</param>
        private static void makeWallLayer(bool isLightWeight,
            out WallLayers exteriorWallLayer, out WallLayers floorWallLayer, out WallLayers roofWallLayer)
        {
            WallLayers.Layer layer;

            exteriorWallLayer = new WallLayers("Exterior");
            floorWallLayer = new WallLayers("Floor");
            roofWallLayer = new WallLayers("Roof");

            if (isLightWeight)
            {
                //外壁構成                
                layer = new WallLayers.Layer(new WallMaterial("Plasterboard", 0.160, 950d * 840d / 1000d), 0.012);
                exteriorWallLayer.AddLayer(layer);
                layer = new WallLayers.Layer(new WallMaterial("Fibreglas quilt", 0.04, 12d * 840d / 1000d), 0.066);
                exteriorWallLayer.AddLayer(layer);
                layer = new WallLayers.Layer(new WallMaterial("Wood Siding", 0.140, 530d * 900d / 1000d), 0.009);
                exteriorWallLayer.AddLayer(layer);

                //床構成                
                layer = new WallLayers.Layer(new WallMaterial("Timber flooring", 0.140, 650d * 1200d / 1000d), 0.025);
                floorWallLayer.AddLayer(layer);
                //layer = new WallLayers.Layer(new WallMaterial("Insulation", 0.04, 0), 0);
                //floorWallLayer.AddLayer(layer);

                //天井構成                
                layer = new WallLayers.Layer(new WallMaterial("Plasterboard", 0.160, 950d * 840d / 1000d), 0.010);
                roofWallLayer.AddLayer(layer);
                layer = new WallLayers.Layer(new WallMaterial("Fibreglas quilt", 0.04, 12d * 840d / 1000d), 0.1118);
                roofWallLayer.AddLayer(layer);
                layer = new WallLayers.Layer(new WallMaterial("Roofdeck", 0.140, 530d * 900d / 1000d), 0.019);
                roofWallLayer.AddLayer(layer);
            }
            else
            {
                //外壁構成
                layer = new WallLayers.Layer(new WallMaterial("Concrete Block", 0.510, 1400d * 1000d / 1000d), 0.1, 3);
                exteriorWallLayer.AddLayer(layer);
                layer = new WallLayers.Layer(new WallMaterial("Foam Insulation", 0.040, 10d * 1400d / 1000d), 0.0615);
                exteriorWallLayer.AddLayer(layer);
                layer = new WallLayers.Layer(new WallMaterial("Wood Siding", 0.140, 530d * 900d / 1000d), 0.009);
                exteriorWallLayer.AddLayer(layer);

                //床構成
                layer = new WallLayers.Layer(new WallMaterial("Concrete Slab", 1.130, 1400d * 1000d / 1000d), 0.08, 3);
                floorWallLayer.AddLayer(layer);
                //layer = new WallLayers.Layer(new WallMaterial("Insulation", 0.04, 0), 0);
                //floorWallLayer.AddLayer(layer);

                //天井構成
                layer = new WallLayers.Layer(new WallMaterial("Plasterboard", 0.160, 950d * 840d / 1000d), 0.010);
                roofWallLayer.AddLayer(layer);
                layer = new WallLayers.Layer(new WallMaterial("Fibreglas quilt", 0.04, 12d * 840d / 1000d), 0.1118);
                roofWallLayer.AddLayer(layer);
                layer = new WallLayers.Layer(new WallMaterial("Roofdeck", 0.140, 530d * 900d / 1000d), 0.019);
                roofWallLayer.AddLayer(layer);
            }
        }

        /// <summary>HighConductanceWallを作成する</summary>
        /// <returns>HighConductanceWall</returns>
        private static WallLayers makeHighConductanceWall()
        {
            WallLayers hcWall = new WallLayers("HighConductanceWall");
            //外壁構成
            WallLayers.Layer layer;
            layer = new WallLayers.Layer(new WallMaterial("Glass", 1.06, 0.75 * 2500), 3.175 / 1000);
            hcWall.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("Air-gap", 6.297, 0), 0);
            hcWall.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("Glass", 1.06, 0.75 * 2500), 3.175 / 1000);
            hcWall.AddLayer(layer);

            return hcWall;
        }

        /// <summary>土中の壁構成を作成する</summary>
        private static void makeGroundWallLayer(out  WallLayers floorWallLayer, out WallLayers groundWallLayer)
        {
            const double GROUND_THICKNESS = 1.3;

            groundWallLayer = new WallLayers("GroundWall");
            floorWallLayer = new WallLayers("FloorWall");
            WallLayers.Layer layer;

            //土中壁
            layer = new WallLayers.Layer(new WallMaterial("Concrete Block", 0.510, 1400d * 1000d / 1000d), 0.1, 3);
            groundWallLayer.AddLayer(layer);
            //土の厚み
            layer = new WallLayers.Layer(new WallMaterial("Ground", 1.3, 800d * 1500d / 1000d), GROUND_THICKNESS, 6);
            groundWallLayer.AddLayer(layer);

            //床
            layer = new WallLayers.Layer(new WallMaterial("Concrete Slab", 1.130, 1400d * 1000d / 1000d), 0.08, 3);
            floorWallLayer.AddLayer(layer);
            //土の厚み
            layer = new WallLayers.Layer(new WallMaterial("Ground", 1.3, 800d * 1500d / 1000d), GROUND_THICKNESS, 6);
            floorWallLayer.AddLayer(layer);
        }

        #endregion

        #region 気象データ作成処理

        /// <summary>気象データ作成処理</summary>
        /// <param name="wdataPath">元の気象データのパス</param>
        /// <param name="outputPath">変換後の気象データのパス</param>
        public static void ReadAndWriteBESTestWeatherData(string wdataPath, string outputPath)
        {
            bool success;
            WeatherDataTable wdTable = TMY1Converter.ToPWeatherData(wdataPath, out success);

            using (StreamWriter sWriter = new StreamWriter(outputPath, false, Encoding.GetEncoding("Shift_JIS")))
            {
                sWriter.WriteLine("日付,時刻,乾球温度[C],絶対湿度[kg/kg],法線面直達日射[W/m2],全天日射[W/m2],拡散日射[W/m2],雲量,夜間放射[W/m2],太陽高度[radian],地中温度(0.675m)[C],地中温度(1.35m)[C],地中温度(2.35m)[C]");

                ImmutableWeatherRecord wr;
                ImmutableWeatherData wd;
                DateTime dTime;

                //年平均気温、年較差、最大平均気温日の通日を計算
                double dbt;
                double dbtAve = 0;
                double dbtMin = 100;
                double dbtMax = -100;
                double dbtMinSum = 0;
                double dbtMaxSum = 0;
                double maxDay = 1;
                double daySum = 0;
                double maxDaySum = 0;
                double[] maxAve = new double[12];
                double[] minAve = new double[12];
                dTime = new DateTime(1999, 1, 1, 0, 0, 0);
                int days = 0;
                for (int i = 0; i < wdTable.WeatherRecordNumber; i++)
                {
                    wr = wdTable.GetWeatherRecord(i); 
                    dbt = wr.GetData(WeatherRecord.RecordType.DryBulbTemperature).Value;
                    dbtAve += dbt;
                    dbtMin = Math.Min(dbt, dbtMin);
                    dbtMax = Math.Max(dbt, dbtMax);
                    daySum += dbt;

                    if (dTime.Hour == 23)
                    {
                        days++;
                        dbtMaxSum += dbtMax;
                        dbtMinSum += dbtMin;
                        dbtMax = -100;
                        dbtMin = 100;
                        if (maxDaySum < daySum)
                        {
                            maxDaySum = daySum;
                            maxDay = (i / 24) + 1;
                        }
                        daySum = 0;
                    }

                    dTime = dTime.AddHours(1);

                    if ((dTime.Day == 1) && (dTime.Hour == 0))
                    {
                        if (dTime.Month == 1)
                        {
                            maxAve[11] = dbtMaxSum / days;
                            minAve[11] = dbtMinSum / days;
                        }
                        else
                        {
                            maxAve[dTime.Month - 2] = dbtMaxSum / days;
                            minAve[dTime.Month - 2] = dbtMinSum / days;
                        }
                        dbtMaxSum = dbtMinSum = 0;
                        days = 0;
                    }
                }
                dbtAve /= 8760d;
                for (int i = 0; i < maxAve.Length; i++)
                {
                    dbtMax = Math.Max(maxAve[i], dbtMax);
                    dbtMin = Math.Min(minAve[i], dbtMin);
                }

                Sun sun = new Sun(39.8, 360 - 104.9, 360 - 105);

                for (int i = 0; i < wdTable.WeatherRecordNumber; i++)
                {
                    wr = wdTable.GetWeatherRecord(i);

                    dTime = wr.DataDTime;
                    sun.Update(dTime.AddMinutes(30));
                    sWriter.Write(dTime.ToShortDateString() + "," + dTime.ToShortTimeString() + ",");

                    dbt = wr.GetData(WeatherRecord.RecordType.DryBulbTemperature).Value;
                    sWriter.Write(dbt + ",");

                    double dpt = wr.GetData(WeatherRecord.RecordType.DewPointTemperature).Value;
                    double atm = wr.GetData(WeatherRecord.RecordType.AtmosphericPressure).Value;
                    double ahd = MoistAir.GetSaturatedHumidityRatio(dpt, MoistAir.Property.DryBulbTemperature, atm);
                    sWriter.Write(ahd + ",");

                    wd = wr.GetData(WeatherRecord.RecordType.DirectNormalRadiation);
                    sWriter.Write(wd.Value + ",");
                    sun.DirectNormalRadiation = wd.Value;

                    wd = wr.GetData(WeatherRecord.RecordType.GlobalHorizontalRadiation);
                    sWriter.Write(wd.Value + ",");
                    sun.GlobalHorizontalRadiation = wd.Value;

                    //日の出の場合の補正
                    if (sun.SunRiseTime.Hour == sun.CurrentDateTime.Hour) sun.Update(sun.CurrentDateTime.AddMinutes(30));
                    //日没の場合の補正
                    if (sun.SunSetTime.Hour == sun.CurrentDateTime.Hour) sun.Update(sun.CurrentDateTime.AddMinutes(-30));

                    sWriter.Write(Math.Max(0, Sun.GetDiffuseHorizontalRadiation(sun.DirectNormalRadiation, sun.GlobalHorizontalRadiation, sun.Altitude)) + ","); ;

                    wd = wr.GetData(WeatherRecord.RecordType.TotalSkyCover);
                    double cc;
                    if (wd.Source == WeatherData.DataSource.MissingValue) cc = 10;
                    else cc = wd.Value;
                    sWriter.Write(cc + ",");

                    double wbp = MoistAir.GetWaterVaporPressure(ahd, atm);
                    double nr = Sky.GetNocturnalRadiation(dbt, cc, wbp);
                    sWriter.Write(nr + ",");

                    //太陽高度
                    sWriter.Write(sun.Altitude + ",");

                    //地中温度[C]
                    double nd = dTime.DayOfYear + dTime.Hour / 24d + (dTime.Minute + 30d) / 24d / 60d;
                    //0.675m
                    double tgrz = dbtAve + 0.5 * (dbtMax - dbtMin) * Math.Exp(-0.526 * 0.675) * Math.Cos((nd - maxDay - 30.556 * 0.675) * 0.017214);
                    sWriter.Write(tgrz + ",");
                    //1.35m
                    tgrz = dbtAve + 0.5 * (dbtMax - dbtMin) * Math.Exp(-0.526 * 1.35) * Math.Cos((nd - maxDay - 30.556 * 1.35) * 0.017214);
                    sWriter.Write(tgrz + ",");
                    //2.65m
                    tgrz = dbtAve + 0.5 * (dbtMax - dbtMin) * Math.Exp(-0.526 * 2.65) * Math.Cos((nd - maxDay - 30.556 * 2.65) * 0.017214);
                    sWriter.Write(tgrz + ",");

                    sWriter.WriteLine();

                    dTime = dTime.AddHours(1);
                }

            }

        }

        #endregion

    }
}
