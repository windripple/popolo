using System;

using System.IO;
using System.Text;
using System.Collections.Generic;

using Popolo.Weather;
using Popolo.Weather.Converter;
using Popolo.ThermalLoad;
using Popolo.ThermophysicalProperty;
using Popolo.CircuitNetwork;
using Popolo.BuildingInformationModeling.NetWork.TSC21;
using Popolo.ThermalComfort;


using Excel = Microsoft.Office.Interop.Excel;

namespace Popolo.Utility
{
    class Program
    {
        static void Main(string[] args)
        {

            //TSC21テスト
            //tscTest();

            //室モデルテスト
            //RoomModelTest1();
            //RoomModelTest2();

            //壁体熱貫流率計算テスト
            //wallOverallHeatTransferCoefTest();

            //壁熱貫流テスト
            //wallHeatTransferTest();
            //wallHeatTransferTest1();
            //wallHeatTransferTest2();
            //wallHeatTransferTest3();

            //窓熱取得テスト
            //windowTest();
            //windowTest1();

            //気象データ変換テスト
            //wdataConvertTest1();
            //wdataConvertTest2();

            //湿り空気物性書き出しテスト
            //outputMoistAirState();

            //回路網計算テスト
            //circuitTest1();
            //circuitTest2();
            //circuitTest3();
            //Console.WriteLine();
            //circuitTest4();

            //気象テスト
            //SkyTest();
            //weatherTest();

            //BESTest
            //BESTest.ReadAndWriteBESTestWeatherData("DRYCOLD.TMY", "BESTestWeather.csv");
            //MakeBESTestResult();
            //MakeBESTResultExcelSheet();

            //SchedulerTest
            //SchedulerTest.MakeScheduler();

            //ガラステスト
            //glassPanesTest();

            //多数室テスト
            //multiRoomTest();

            //人体モデルテスト
            //humanBodyTest();

            //応答係数テスト
            rFactorSample();
        }

        #region 室モデルテスト

        /// <summary>室の温湿度変動テスト(Zoneクラス)</summary>
        private static void RoomModelTest1()
        {
            //気象データ:乾球温度,絶対湿度,夜間放射,直達日射,天空日射
            double[] dbt = new double[] { 24.2, 24.1, 24.1, 24.2, 24.3, 24.2, 24.4, 25.1, 26.1, 27.1, 28.8, 29.9,
                30.7, 31.2, 31.6, 31.4, 31.3, 30.8, 29.4, 28.1, 27.5, 27.1, 26.6, 26.3 };
            double[] ahd = new double[] { 0.0134, 0.0136, 0.0134, 0.0133, 0.0131, 0.0134, 0.0138, 0.0142, 0.0142, 0.0140, 0.0147, 0.0149, 
                0.0142, 0.0146, 0.0140, 0.0145, 0.0144, 0.0146, 0.0142, 0.0136, 0.0136, 0.0135, 0.0136, 0.0140 };
            double[] nrd = new double[] { 32, 30, 30, 29, 26, 24, 24, 25, 25, 25, 24, 24, 24, 23, 24, 24, 24, 24, 23, 23, 24, 26, 25, 23 };
            double[] dnr = new double[] { 0, 0, 0, 0, 0, 0, 106, 185, 202, 369, 427, 499, 557, 522, 517, 480, 398, 255, 142, 2, 0, 0, 0, 0 };
            double[] drd = new double[] { 0, 0, 0, 0, 0, 0, 36, 115, 198, 259, 314, 340, 340, 349, 319, 277, 228, 167, 87, 16, 0, 0, 0, 0 };

            //屋外を作成
            Outdoor outdoor = new Outdoor();
            Sun sun = new Sun(Sun.City.Tokyo);
            outdoor.Sun = sun;
            outdoor.GroundTemperature = 25;

            //傾斜を作成
            Incline nIn = new Incline(Incline.Orientation.N, 0.5 * Math.PI);    //北
            Incline eIn = new Incline(Incline.Orientation.E, 0.5 * Math.PI);    //東
            Incline wIn = new Incline(Incline.Orientation.W, 0.5 * Math.PI);    //西
            Incline sIn = new Incline(Incline.Orientation.S, 0.5 * Math.PI);    //南
            Incline hIn = new Incline(Incline.Orientation.S, 0);                //水平

            //ゾーンを作成
            Zone[] zones = new Zone[4];
            Zone wpZone = zones[0] = new Zone("西室ペリメータ");
            wpZone.Volume = 3 * 5 * 3;
            Zone wiZone = zones[1] = new Zone("西室インテリア");
            wiZone.Volume = 4 * 5 * 3;
            Zone epZone = zones[2] = new Zone("東室ペリメータ");
            epZone.Volume = 3 * 5 * 3;
            Zone eiZone = zones[3] = new Zone("東室インテリア");
            eiZone.Volume = 4 * 5 * 3;
            foreach (Zone zn in zones)
            {
                zn.VentilationVolume = 10;  //換気量[CMH](ゾーン間換気もこのプロパティを援用する)
                zn.TimeStep = 3600;
                zn.DrybulbTemperatureSetPoint = 26;
                zn.HumidityRatioSetPoint = 0.01;
            }

            //東側インテリアに発熱体を設定
            eiZone.AddHeatGain(new ConstantHeatGain(100, 100, 20));

            //壁構成を作成:400mmコンクリート
            WallLayers wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.ReinforcedConcrete), 0.4));

            //窓構成を作成
            GlassPanes gPanes = new GlassPanes(new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.HeatReflectingGlass06mm));

            //壁体をゾーンに追加
            Wall[] walls = new Wall[18];
            List<WallSurface> outdoorSurfaces = new List<WallSurface>();
            Wall wpwWall = walls[0] = new Wall(wl, "西室ペリメータ西壁");
            wpwWall.SurfaceArea = 3 * 3;
            outdoorSurfaces.Add(wpwWall.GetSurface(true));
            wpZone.AddSurface(wpwWall.GetSurface(false));
            wpwWall.SetIncline(wIn, true);

            Wall wpcWall = walls[1] = new Wall(wl, "西室ペリメータ天井");
            wpcWall.SurfaceArea = 3 * 5;
            outdoorSurfaces.Add(wpcWall.GetSurface(true));
            wpZone.AddSurface(wpcWall.GetSurface(false));
            wpcWall.SetIncline(hIn, true);

            Wall wpfWall = walls[2] = new Wall(wl, "西室ペリメータ床");
            wpfWall.SurfaceArea = 3 * 5;
            outdoor.AddGroundWallSurface(wpfWall.GetSurface(true));
            wpZone.AddSurface(wpfWall.GetSurface(false));

            Wall winWall = walls[3] = new Wall(wl, "西室インテリア北壁");
            winWall.SurfaceArea = 3 * 5;
            outdoorSurfaces.Add(winWall.GetSurface(true));
            wiZone.AddSurface(winWall.GetSurface(false));
            winWall.SetIncline(nIn, true);

            Wall wiwWall = walls[4] = new Wall(wl, "西室インテリア西壁");
            wiwWall.SurfaceArea = 3 * 4;
            outdoorSurfaces.Add(wiwWall.GetSurface(true));
            wiZone.AddSurface(wiwWall.GetSurface(false));
            wiwWall.SetIncline(wIn, true);

            Wall wicWall = walls[5] = new Wall(wl, "西室インテリア天井");
            wicWall.SurfaceArea = 4 * 5;
            outdoorSurfaces.Add(wicWall.GetSurface(true));
            wiZone.AddSurface(wicWall.GetSurface(false));
            wicWall.SetIncline(hIn, true);

            Wall wifWall = walls[6] = new Wall(wl, "西室インテリア床");
            wifWall.SurfaceArea = 4 * 5;
            outdoor.AddGroundWallSurface(wifWall.GetSurface(true));
            wiZone.AddSurface(wifWall.GetSurface(false));

            Wall epwWall = walls[7] = new Wall(wl, "東室ペリメータ東壁");
            epwWall.SurfaceArea = 3 * 3;
            outdoorSurfaces.Add(epwWall.GetSurface(true));
            epZone.AddSurface(epwWall.GetSurface(false));
            epwWall.SetIncline(eIn, true);

            Wall epcWall = walls[8] = new Wall(wl, "東室ペリメータ天井");
            epcWall.SurfaceArea = 3 * 5;
            outdoorSurfaces.Add(epcWall.GetSurface(true));
            epZone.AddSurface(epcWall.GetSurface(false));
            epcWall.SetIncline(hIn, true);

            Wall epfWall = walls[9] = new Wall(wl, "東室ペリメータ床");
            epfWall.SurfaceArea = 3 * 5;
            outdoor.AddGroundWallSurface(epfWall.GetSurface(true));
            epZone.AddSurface(epfWall.GetSurface(false));

            Wall einWall = walls[10] = new Wall(wl, "東室インテリア北壁");
            einWall.SurfaceArea = 5 * 3;
            outdoorSurfaces.Add(einWall.GetSurface(true));
            eiZone.AddSurface(einWall.GetSurface(false));
            einWall.SetIncline(nIn, true);

            Wall eiwWall = walls[11] = new Wall(wl, "東室インテリア東壁");
            eiwWall.SurfaceArea = 4 * 3;
            outdoorSurfaces.Add(eiwWall.GetSurface(true));
            eiZone.AddSurface(eiwWall.GetSurface(false));
            eiwWall.SetIncline(eIn, true);

            Wall eicWall = walls[12] = new Wall(wl, "東室インテリア天井");
            eicWall.SurfaceArea = 4 * 5;
            outdoorSurfaces.Add(eicWall.GetSurface(true));
            eiZone.AddSurface(eicWall.GetSurface(false));
            eicWall.SetIncline(hIn, true);

            Wall eifWall = walls[13] = new Wall(wl, "東室インテリア床");
            eifWall.SurfaceArea = 4 * 5;
            outdoor.AddGroundWallSurface(eifWall.GetSurface(true));
            eiZone.AddSurface(eifWall.GetSurface(false));

            Wall cpWall = walls[14] = new Wall(wl, "ペリメータ部の内壁");
            cpWall.SurfaceArea = 3 * 3;
            wpZone.AddSurface(cpWall.GetSurface(true));
            epZone.AddSurface(cpWall.GetSurface(false));

            Wall ciWall = walls[15] = new Wall(wl, "インテリア部の内壁");
            ciWall.SurfaceArea = 4 * 3;
            wiZone.AddSurface(ciWall.GetSurface(true));
            eiZone.AddSurface(ciWall.GetSurface(false));

            Wall wpsWall = walls[16] = new Wall(wl, "西側ペリメータ南壁");
            wpsWall.SurfaceArea = 5 * 3 - 3 * 2;
            outdoorSurfaces.Add(wpsWall.GetSurface(true));
            wpZone.AddSurface(wpsWall.GetSurface(false));
            wpsWall.SetIncline(sIn, true);

            Wall epsWall = walls[17] = new Wall(wl, "東側ペリメータ南壁");
            epsWall.SurfaceArea = 5 * 3 - 3 * 2;
            outdoorSurfaces.Add(epsWall.GetSurface(true));
            epZone.AddSurface(epsWall.GetSurface(false));
            epsWall.SetIncline(sIn, true);

            //外表面を初期化
            foreach (WallSurface ws in outdoorSurfaces)
            {
                //屋外に追加
                outdoor.AddWallSurface(ws);
                //放射率を初期化
                ws.InitializeEmissivity(WallSurface.SurfaceMaterial.Concrete);
            }

            //窓をゾーンに追加
            Window wWind = new Window(gPanes, "西室ペリメータ南窓");
            wWind.SurfaceArea = 3 * 2;
            wpZone.AddWindow(wWind);
            outdoor.AddWindow(wWind);

            Window eWind = new Window(gPanes, "東室ペリメータ南窓");
            eWind.SurfaceArea = 3 * 2;
            eWind.Shade = SunShade.MakeHorizontalSunShade(3, 2, 1, 1, 1, 0.5, sIn);
            wpZone.AddWindow(eWind);
            outdoor.AddWindow(eWind);

            //タイトル行書き出し
            StreamWriter sWriter = new StreamWriter("室の温湿度変動テスト1.csv", false, Encoding.GetEncoding("Shift_JIS"));
            foreach (Zone zn in zones) sWriter.Write(zn.Name + "乾球温度[C], " + zn.Name + "絶対湿度[kg/kgDA], " + zn.Name + "顕熱負荷[W], " + zn.Name + "潜熱負荷[W], ");
            sWriter.WriteLine();

            //計算実行
            for (int i = 0; i < 100; i++)
            {
                DateTime dTime = new DateTime(2007, 8, 3, 0, 0, 0);
                for (int j = 0; j < 24; j++)
                {
                    //時刻を設定
                    sun.Update(dTime);
                    foreach (Zone zn in zones) zn.CurrentDateTime = dTime;

                    //空調設定
                    bool operating = (8 <= dTime.Hour && dTime.Hour <= 19);
                    foreach (Zone zn in zones)
                    {
                        zn.ControlHumidityRatio = operating;
                        zn.ControlDrybulbTemperature = operating;
                    }

                    //気象条件を設定
                    outdoor.AirState = new MoistAir(dbt[j], ahd[j]);
                    outdoor.NocturnalRadiation = nrd[j];
                    sun.SetGlobalHorizontalRadiation(drd[j], dnr[j]);

                    //換気の設定
                    eiZone.VentilationAirState = new MoistAir(epZone.CurrentDrybulbTemperature, eiZone.CurrentHumidityRatio);
                    epZone.VentilationAirState = new MoistAir(eiZone.CurrentDrybulbTemperature, eiZone.CurrentHumidityRatio);
                    wpZone.VentilationAirState = outdoor.AirState;
                    wiZone.VentilationAirState = new MoistAir(wpZone.CurrentDrybulbTemperature, wpZone.CurrentHumidityRatio);

                    //外壁表面の状態を設定
                    outdoor.SetWallSurfaceBoundaryState();

                    //壁体を更新
                    foreach (Wall wal in walls) wal.Update();

                    //ゾーンを更新
                    foreach (Zone zn in zones) zn.Update();

                    //時刻を更新
                    dTime = dTime.AddHours(1);

                    //書き出し設定
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

        /// <summary>室の温湿度変動テスト(MultiRoomクラス)</summary>
        private static void RoomModelTest2()
        {
            //気象データ:乾球温度,絶対湿度,夜間放射,直達日射,天空日射
            double[] dbt = new double[] { 24.2, 24.1, 24.1, 24.2, 24.3, 24.2, 24.4, 25.1, 26.1, 27.1, 28.8, 29.9,
                30.7, 31.2, 31.6, 31.4, 31.3, 30.8, 29.4, 28.1, 27.5, 27.1, 26.6, 26.3 };
            double[] ahd = new double[] { 0.0134, 0.0136, 0.0134, 0.0133, 0.0131, 0.0134, 0.0138, 0.0142, 0.0142, 0.0140, 0.0147, 0.0149, 
                0.0142, 0.0146, 0.0140, 0.0145, 0.0144, 0.0146, 0.0142, 0.0136, 0.0136, 0.0135, 0.0136, 0.0140 };
            double[] nrd = new double[] { 32, 30, 30, 29, 26, 24, 24, 25, 25, 25, 24, 24, 24, 23, 24, 24, 24, 24, 23, 23, 24, 26, 25, 23 };
            double[] dnr = new double[] { 0, 0, 0, 0, 0, 0, 106, 185, 202, 369, 427, 499, 557, 522, 517, 480, 398, 255, 142, 2, 0, 0, 0, 0 };
            double[] drd = new double[] { 0, 0, 0, 0, 0, 0, 36, 115, 198, 259, 314, 340, 340, 349, 319, 277, 228, 167, 87, 16, 0, 0, 0, 0 };

            //屋外を作成
            Outdoor outdoor = new Outdoor();
            Sun sun = new Sun(Sun.City.Tokyo);
            outdoor.Sun = sun;
            outdoor.GroundTemperature = 25;

            //傾斜を作成
            Incline nIn = new Incline(Incline.Orientation.N, 0.5 * Math.PI);    //北
            Incline eIn = new Incline(Incline.Orientation.E, 0.5 * Math.PI);    //東
            Incline wIn = new Incline(Incline.Orientation.W, 0.5 * Math.PI);    //西
            Incline sIn = new Incline(Incline.Orientation.S, 0.5 * Math.PI);    //南
            Incline hIn = new Incline(Incline.Orientation.S, 0);                //水平

            //ゾーンを作成
            Zone[] zones = new Zone[4];
            Zone wpZone = zones[0] = new Zone("西室ペリメータ");
            wpZone.Volume = 3 * 5 * 3;
            Zone wiZone = zones[1] = new Zone("西室インテリア");
            wiZone.Volume = 4 * 5 * 3;
            Zone epZone = zones[2] = new Zone("東室ペリメータ");
            epZone.Volume = 3 * 5 * 3;
            Zone eiZone = zones[3] = new Zone("東室インテリア");
            eiZone.Volume = 4 * 5 * 3;
            foreach (Zone zn in zones)
            {
                zn.TimeStep = 3600;
                zn.DrybulbTemperatureSetPoint = 26;
                zn.HumidityRatioSetPoint = 0.01;
            }

            //東側インテリアに発熱体を設定
            eiZone.AddHeatGain(new ConstantHeatGain(100, 100, 20));

            //壁構成を作成:400mmコンクリート
            WallLayers wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.ReinforcedConcrete), 0.4));

            //窓構成を作成
            GlassPanes gPanes = new GlassPanes(new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.HeatReflectingGlass06mm));

            //壁体をゾーンに追加
            Wall[] walls = new Wall[18];
            List<WallSurface> outdoorSurfaces = new List<WallSurface>();
            Wall wpwWall = walls[0] = new Wall(wl, "西室ペリメータ西壁");
            wpwWall.SurfaceArea = 3 * 3;
            outdoorSurfaces.Add(wpwWall.GetSurface(true));
            wpZone.AddSurface(wpwWall.GetSurface(false));
            wpwWall.SetIncline(wIn, true);

            Wall wpcWall = walls[1] = new Wall(wl, "西室ペリメータ天井");
            wpcWall.SurfaceArea = 3 * 5;
            outdoorSurfaces.Add(wpcWall.GetSurface(true));
            wpZone.AddSurface(wpcWall.GetSurface(false));
            wpcWall.SetIncline(hIn, true);

            Wall wpfWall = walls[2] = new Wall(wl, "西室ペリメータ床");
            wpfWall.SurfaceArea = 3 * 5;
            outdoor.AddGroundWallSurface(wpfWall.GetSurface(true));
            wpZone.AddSurface(wpfWall.GetSurface(false));

            Wall winWall = walls[3] = new Wall(wl, "西室インテリア北壁");
            winWall.SurfaceArea = 3 * 5;
            outdoorSurfaces.Add(winWall.GetSurface(true));
            wiZone.AddSurface(winWall.GetSurface(false));
            winWall.SetIncline(nIn, true);

            Wall wiwWall = walls[4] = new Wall(wl, "西室インテリア西壁");
            wiwWall.SurfaceArea = 3 * 4;
            outdoorSurfaces.Add(wiwWall.GetSurface(true));
            wiZone.AddSurface(wiwWall.GetSurface(false));
            wiwWall.SetIncline(wIn, true);

            Wall wicWall = walls[5] = new Wall(wl, "西室インテリア天井");
            wicWall.SurfaceArea = 4 * 5;
            outdoorSurfaces.Add(wicWall.GetSurface(true));
            wiZone.AddSurface(wicWall.GetSurface(false));
            wicWall.SetIncline(hIn, true);

            Wall wifWall = walls[6] = new Wall(wl, "西室インテリア床");
            wifWall.SurfaceArea = 4 * 5;
            outdoor.AddGroundWallSurface(wifWall.GetSurface(true));
            wiZone.AddSurface(wifWall.GetSurface(false));

            Wall epwWall = walls[7] = new Wall(wl, "東室ペリメータ東壁");
            epwWall.SurfaceArea = 3 * 3;
            outdoorSurfaces.Add(epwWall.GetSurface(true));
            epZone.AddSurface(epwWall.GetSurface(false));
            epwWall.SetIncline(eIn, true);

            Wall epcWall = walls[8] = new Wall(wl, "東室ペリメータ天井");
            epcWall.SurfaceArea = 3 * 5;
            outdoorSurfaces.Add(epcWall.GetSurface(true));
            epZone.AddSurface(epcWall.GetSurface(false));
            epcWall.SetIncline(hIn, true);

            Wall epfWall = walls[9] = new Wall(wl, "東室ペリメータ床");
            epfWall.SurfaceArea = 3 * 5;
            outdoor.AddGroundWallSurface(epfWall.GetSurface(true));
            epZone.AddSurface(epfWall.GetSurface(false));

            Wall einWall = walls[10] = new Wall(wl, "東室インテリア北壁");
            einWall.SurfaceArea = 5 * 3;
            outdoorSurfaces.Add(einWall.GetSurface(true));
            eiZone.AddSurface(einWall.GetSurface(false));
            einWall.SetIncline(nIn, true);

            Wall eiwWall = walls[11] = new Wall(wl, "東室インテリア東壁");
            eiwWall.SurfaceArea = 4 * 3;
            outdoorSurfaces.Add(eiwWall.GetSurface(true));
            eiZone.AddSurface(eiwWall.GetSurface(false));
            eiwWall.SetIncline(eIn, true);

            Wall eicWall = walls[12] = new Wall(wl, "東室インテリア天井");
            eicWall.SurfaceArea = 4 * 5;
            outdoorSurfaces.Add(eicWall.GetSurface(true));
            eiZone.AddSurface(eicWall.GetSurface(false));
            eicWall.SetIncline(hIn, true);

            Wall eifWall = walls[13] = new Wall(wl, "東室インテリア床");
            eifWall.SurfaceArea = 4 * 5;
            outdoor.AddGroundWallSurface(eifWall.GetSurface(true));
            eiZone.AddSurface(eifWall.GetSurface(false));

            Wall cpWall = walls[14] = new Wall(wl, "ペリメータ部の内壁");
            cpWall.SurfaceArea = 3 * 3;
            wpZone.AddSurface(cpWall.GetSurface(true));
            epZone.AddSurface(cpWall.GetSurface(false));

            Wall ciWall = walls[15] = new Wall(wl, "インテリア部の内壁");
            ciWall.SurfaceArea = 4 * 3;
            wiZone.AddSurface(ciWall.GetSurface(true));
            eiZone.AddSurface(ciWall.GetSurface(false));

            Wall wpsWall = walls[16] = new Wall(wl, "西側ペリメータ南壁");
            wpsWall.SurfaceArea = 5 * 3 - 3 * 2;
            outdoorSurfaces.Add(wpsWall.GetSurface(true));
            wpZone.AddSurface(wpsWall.GetSurface(false));
            wpsWall.SetIncline(sIn, true);

            Wall epsWall = walls[17] = new Wall(wl, "東側ペリメータ南壁");
            epsWall.SurfaceArea = 5 * 3 - 3 * 2;
            outdoorSurfaces.Add(epsWall.GetSurface(true));
            epZone.AddSurface(epsWall.GetSurface(false));
            epsWall.SetIncline(sIn, true);

            //外表面を初期化
            foreach (WallSurface ws in outdoorSurfaces)
            {
                //屋外に追加
                outdoor.AddWallSurface(ws);
                //放射率を初期化
                ws.InitializeEmissivity(WallSurface.SurfaceMaterial.Concrete);
            }

            //窓をゾーンに追加
            Window wWind = new Window(gPanes, "西室ペリメータ南窓");
            wWind.SurfaceArea = 3 * 2;
            wpZone.AddWindow(wWind);
            outdoor.AddWindow(wWind);

            Window eWind = new Window(gPanes, "東室ペリメータ南窓");
            eWind.SurfaceArea = 3 * 2;
            eWind.Shade = SunShade.MakeHorizontalSunShade(3, 2, 1, 1, 1, 0.5, sIn);
            wpZone.AddWindow(eWind);
            outdoor.AddWindow(eWind);

            //多数室オブジェクトを作成
            Room eRm = new Room(new Zone[] { epZone, eiZone }); //東側の室
            Room wRm = new Room(new Zone[] { wpZone, wiZone }); //西側の室
            MultiRoom mRoom = new MultiRoom(new Room[] { eRm, wRm });   //多数室
            mRoom.SetTimeStep(3600);

            //換気の設定
            wpZone.VentilationVolume = 10;  //西室ペリメータのみ外気導入
            mRoom.SetAirFlow(wpZone, wiZone, 10);
            mRoom.SetAirFlow(epZone, eiZone, 10);
            mRoom.SetAirFlow(eiZone, epZone, 10);

            //短波長放射の入射比率を調整:ペリメータ床面6割、その他は面積比率
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

            //タイトル行書き出し
            StreamWriter sWriter = new StreamWriter("室の温湿度変動テスト2.csv", false, Encoding.GetEncoding("Shift_JIS"));
            foreach (Zone zn in zones) sWriter.Write(zn.Name + "乾球温度[C], " + zn.Name + "絶対湿度[kg/kgDA], " + zn.Name + "顕熱負荷[W], " + zn.Name + "潜熱負荷[W], ");
            sWriter.WriteLine();

            //計算実行
            for (int i = 0; i < 100; i++)
            {
                DateTime dTime = new DateTime(2007, 8, 3, 0, 0, 0);
                for (int j = 0; j < 24; j++)
                {
                    //時刻を設定
                    sun.Update(dTime);
                    mRoom.SetCurrentDateTime(dTime);

                    //空調設定
                    bool operating = (8 <= dTime.Hour && dTime.Hour <= 19);
                    foreach (Zone zn in zones)
                    {
                        zn.ControlHumidityRatio = operating;
                        zn.ControlDrybulbTemperature = operating;
                    }

                    //気象条件を設定
                    outdoor.AirState = new MoistAir(dbt[j], ahd[j]);
                    outdoor.NocturnalRadiation = nrd[j];
                    sun.SetGlobalHorizontalRadiation(drd[j], dnr[j]);

                    //換気の設定
                    wpZone.VentilationAirState = outdoor.AirState;

                    //外壁表面の状態を設定
                    outdoor.SetWallSurfaceBoundaryState();

                    //壁体を更新
                    foreach (Wall wal in walls) wal.Update();

                    //多数室を更新
                    mRoom.UpdateRoomTemperatures();
                    mRoom.UpdateRoomHumidities();

                    //時刻を更新
                    dTime = dTime.AddHours(1);

                    //書き出し設定
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

        //パソコンによる空気調和計算法 pp.180
        private static void makeRoom(double timeStep, out Zone room, out Wall exWall, out Wall inWall, out Wall ceiling, out Wall floor,
            out Sun sun, out Outdoor outdoor)
        {
            //太陽を作成//東京
            sun = new Sun(35, 139, 134);

            //外界を作成
            outdoor = new Outdoor();
            outdoor.Sun = sun;

            //室を作成
            room = new Zone();
            room.Volume = 353;
            room.SensibleHeatCapacity = 3500 * 1000;
            room.InitializeAirState(26, 0.018);
            room.VentilationVolume = room.Volume * 0.2;
            room.TimeStep = timeStep;

            WallLayers layers;
            WallLayers.Layer layer;

            //外壁を作成
            layers = new WallLayers();
            layer = new WallLayers.Layer(new WallMaterial("アルミ化粧板", 210, 2373), 0.002);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("中空層", 1d / 0.086d, 0), 0);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("ロックウール", 0.042, 84), 0.050);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("コンクリート", 1.4, 1934), 0.150);
            layers.AddLayer(layer);
            exWall = new Wall(layers);
            exWall.SurfaceArea = 22.4;
            exWall.TimeStep = timeStep;
            WallSurface ews = exWall.GetSurface(true);
            ews.SolarAbsorptance = 0.7;
            ews.LongWaveEmissivity = 0.9;
            ews.Albedo = 0.2;
            WallSurface iws = exWall.GetSurface(false);
            ews.FilmCoefficient = 23;
            iws.FilmCoefficient = 9.3;
            exWall.SetIncline(new Incline(Incline.Orientation.SW, 0.5 * Math.PI), true);
            room.AddSurface(iws);       //部屋に追加
            outdoor.AddWallSurface(ews);    //外界に追加
            room.SetShortWaveRadiationRate(iws, iws.Area);    //放射成分吸収比率を設定
            room.SetLongWaveRadiationRate(iws, iws.Area);    //放射成分吸収比率を設定

            //内壁を作成
            layers = new WallLayers();
            layer = new WallLayers.Layer(new WallMaterial("コンクリート", 1.4, 1934), 0.120, 2);
            layers.AddLayer(layer);
            inWall = new Wall(layers);
            inWall.SurfaceArea = 100.8;
            inWall.TimeStep = timeStep;
            iws = inWall.GetSurface(true);
            iws.FilmCoefficient = 9.3;
            room.AddSurface(iws);
            room.SetShortWaveRadiationRate(iws, iws.Area);    //放射成分吸収比率を設定
            room.SetLongWaveRadiationRate(iws, iws.Area);    //放射成分吸収比率を設定

            //床と天井を作成
            layers = new WallLayers();
            layer = new WallLayers.Layer(new WallMaterial("カーペット", 0.08, 318), 0.015);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("コンクリート", 1.4, 1934), 0.150);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("中空層", 1d / 0.086d, 0), 0);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("石膏ボード", 0.17, 1030), 0.012);
            layers.AddLayer(layer);
            //床
            floor = new Wall(layers);
            floor.SurfaceArea = 98;
            floor.TimeStep = timeStep;
            iws = floor.GetSurface(true);
            iws.FilmCoefficient = 9.3;
            room.AddSurface(iws);
            room.SetShortWaveRadiationRate(iws, iws.Area * 2.0);    //放射成分吸収比率を設定**多め
            room.SetLongWaveRadiationRate(iws, iws.Area * 2.0);    //放射成分吸収比率を設定**多め
            //天井
            ceiling = new Wall(layers);
            ceiling.SurfaceArea = 98;
            ceiling.TimeStep = timeStep;
            iws = ceiling.GetSurface(false);
            iws.FilmCoefficient = 9.3;
            room.AddSurface(iws);
            room.SetShortWaveRadiationRate(iws, iws.Area);    //放射成分吸収比率を設定
            room.SetLongWaveRadiationRate(iws, iws.Area);    //放射成分吸収比率を設定

            //室温・隣室温度を設定
            inWall.AirTemperature2 = ceiling.AirTemperature1 = floor.AirTemperature2 = 25;

            //窓を作成
            GlassPanes glassPanes = new GlassPanes(0.79, 0.04, 190);
            Window window = new Window(glassPanes);
            WindowSurface ws = window.GetSurface(true);
            ws.FilmCoefficient = 23;
            ws.Albedo = 0.2;
            window.SurfaceArea = 28;
            window.OutSideIncline = new Incline(Incline.Orientation.SW, 0.5 * Math.PI);
            ws = window.GetSurface(false);
            ws.FilmCoefficient = 9.3;
           
            room.AddWindow(window);
            outdoor.AddWindow(window);
            window.ShadowRate = 0;
            room.SetShortWaveRadiationRate(window, window.SurfaceArea);    //放射成分吸収比率を設定
            room.SetLongWaveRadiationRate(window, window.SurfaceArea);    //放射成分吸収比率を設定

            //壁体初期化
            inWall.InitializeTemperature(22);
            exWall.InitializeTemperature(22);
            floor.InitializeTemperature(22);
            ceiling.InitializeTemperature(22);
        }

        private static void RoomModelTest()
        {
            //夏か否か
            bool IS_SUMMER = true;

            //タイムステップ
            const double TIME_STEP = 1800;

            //室を作成
            Zone room;
            Sun sun;
            Wall exWall, inWall, floor, ceiling;
            Outdoor oDoor;
            makeRoom(TIME_STEP, out room, out exWall, out inWall, out ceiling, out floor, out sun, out oDoor);
            Wall[] walls = new Wall[] { exWall, inWall, ceiling, floor };
            Zone[] rooms = new Zone[] { room };

            //発熱要素
            ConstantHeatGain hGain;
            if(IS_SUMMER) hGain = new ConstantHeatGain(55 * 16 * 0.5 + 2900 * 0.6 + 500 * 0.6, 55 * 16 * 0.5 + 2900 * 0.4 + 500 * 0.4, (119 - 55) * 16);
            else hGain = new ConstantHeatGain((71 * 16 * 0.5 + 2900 * 0.6 + 500 * 0.6) * 0.5, (71 * 16 * 0.5 + 2900 * 0.4 + 500 * 0.4) * 0.5, ((119 - 71) * 16) * 0.5);

            //設計用気象データ
            double[] wdIdn, wdIsky, wdDbt, wdAhd, wdRN;
            if (IS_SUMMER)
            {
                wdIdn = new double[] { 0, 0, 0, 0, 0, 244, 517, 679, 774, 829, 856, 862, 847, 809, 739, 619, 415, 97, 0, 0, 0, 0, 0, 0 };
                wdIsky = new double[] { 0, 0, 0, 0, 21, 85, 109, 116, 116, 113, 110, 109, 111, 114, 116, 114, 102, 63, 0, 0, 0, 0, 0, 0 };
                wdDbt = new double[] { 27.4, 27.1, 26.8, 26.5, 26.9, 27.7, 28.8, 29.8, 30.8, 31.5, 32.1, 32.6, 32.9, 33.2, 33.5, 33.1, 32.4, 31.5, 30.6, 29.8, 29.1, 28.5, 28.1, 27.7 };
                wdAhd = new double[] { 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018 };
                wdRN = new double[] { 24, 24, 24, 24, 24, 24, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 26, 25, 25, 25, 25, 24, 24, 24 };
                room.DrybulbTemperatureSetPoint = 26;
                room.HumidityRatioSetPoint = 0.0105;
            }
            else
            {
                wdIdn = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                wdIsky = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                wdDbt = new double[] { 0.3, 0.1, 0, -0.2, -0.4, -0.5, -0.2, 0.5, 1.2, 1.9, 2.4, 2.8, 3.1, 3.3, 3.5, 3.3, 3, 2.6, 2.1, 1.7, 1.3, 0.9, 0.7, 0.4 };
                wdAhd = new double[] { 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014, 0.0014 };
                //wdRN = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                wdRN = new double[] { 124, 123, 123, 123, 123, 122, 123, 124, 125, 127, 128, 128, 129, 129, 130, 129, 129, 128, 127, 126, 126, 125, 124, 124 };
                room.DrybulbTemperatureSetPoint = 22;
                room.HumidityRatioSetPoint = 0.0082;
            }
            
            using (StreamWriter sWriter = new StreamWriter("test.csv", false, Encoding.GetEncoding("Shift_JIS")))
            {
                sWriter.WriteLine("時刻,室乾球温度[C],顕熱負荷[W],室絶対湿度[kg/kg(DA)],潜熱負荷[W], 周壁平均温度[C]");

                DateTime dt;
                if(IS_SUMMER) dt = new DateTime(1999, 7, 21);
                else dt = new DateTime(1999, 1, 21);

                for (int i = 0; i < (10 * 24 + 1) * 60 * 60 / TIME_STEP; i++)
                {
                    //外気条件を更新
                    int wdIndex = dt.Hour;
                    wdIndex--;
                    if (wdIndex < 0) wdIndex = 23;
                    double dbt = wdDbt[wdIndex];
                    double ahd = wdAhd[wdIndex];
                    oDoor.AirState = MoistAir.GetAirStateFromDBHR(dbt, ahd);
                    room.VentilationAirState = MoistAir.GetAirStateFromDBHR(dbt, ahd);

                    //太陽の情報を更新
                    sun.Update(dt);
                    sun.SetGlobalHorizontalRadiation(wdIsky[wdIndex], wdIdn[wdIndex]);

                    //隣室温度を設定
                    ceiling.AirTemperature1 = floor.AirTemperature2 = room.CurrentDrybulbTemperature;
                    inWall.AirTemperature2 = room.CurrentDrybulbTemperature * 0.7 + dbt * 0.3;

                    //室内発熱要素を設定
                    if (9 <= dt.Hour && dt.Hour <= 18) room.AddHeatGain(hGain);
                    else room.RemoveHeatGain(hGain);
                    
                    //室温制御を更新
                    room.ControlDrybulbTemperature = (8 <= dt.Hour && dt.Hour <= 18);
                    room.ControlHumidityRatio = (8 <= dt.Hour && dt.Hour <= 18);

                    //相当外気温度を更新
                    oDoor.SetWallSurfaceBoundaryState();
                    oDoor.NocturnalRadiation = wdRN[wdIndex];

                    //壁の熱流CFを更新
                    foreach (Wall wall in walls) wall.Update();

                    //室を更新
                    room.Update();  

                    //書き出し
                    sWriter.WriteLine(dt.ToShortTimeString() + "," + room.CurrentDrybulbTemperature + ", " +
                        room.CurrentSensibleHeatLoad + "," + room.CurrentHumidityRatio + ", " + room.CurrentLatentHeatLoad + "," + room.CurrentMeanRadiantTemperature);

                    dt = dt.AddSeconds(TIME_STEP);
                    if (i == 24 * 60 * 60 / TIME_STEP)
                    {
                        if (IS_SUMMER) dt = new DateTime(1999, 7, 21);
                        else dt = new DateTime(1999, 1, 21);
                    }
                    
                }
            }
        }

        #endregion

        #region 気象データ変換テスト

        /// <summary>気象データ変換テスト1</summary>
        private static void wdataConvertTest1()
        {
            bool success;
            StringBuilder sBuilder = new StringBuilder();

            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "wData\\shd2006*");
            foreach (string file in files)
            {
                WeatherDataTable wdt = PMDConverter.ToPWeatherData(file, out success);
                if (success)
                {
                    LocationInformation lInfo;
                    if (PMDConverter.GetLocationInformation(int.Parse(file.Substring(file.Length - 3)), out lInfo))
                    {
                        //EESLISM用気象データの作成処理
                        sBuilder.AppendLine(EESConverter.MakeSupwData(wdt));
                        Console.Write("『" + lInfo.Name + "』を変換中...");
                        EESConverter.FromPWeather(wdt, "wData\\" + lInfo.Name + ".has", out success);
                        if (success)
                        {
                            File.Copy("wData\\" + lInfo.Name + ".has", "wData\\" + lInfo.EnglishName + ".has", true);
                            Console.WriteLine("成功");

                            //CSVデータとしても書き出す
                            makeHourlyWeatherData("wData\\" + wdt.Location.Name + ".csv", wdt);
                        }
                        else Console.WriteLine("失敗");
                    }
                }
            }
            sBuilder.AppendLine("end");
            StreamWriter sWriter = new StreamWriter("supw.efl");
            sWriter.WriteLine(sBuilder.ToString());
            sWriter.Close();
        }

        private static void wdataConvertTest2()
        {
            //TMY1形式のデータ
            bool suc;
            WeatherDataTable wdt = TMY1Converter.ToPWeatherData("wdata\\DRYCOLD.TMY", out suc);
        }

        private static void makeHourlyWeatherData(string csvFilePath, WeatherDataTable wdt)
        {
            using (StreamWriter sWriter = new StreamWriter(csvFilePath, false, Encoding.GetEncoding("Shift_JIS")))
            {
                //タイトル行
                LocationInformation linfo = wdt.Location;
                sWriter.WriteLine("地点," + linfo.Name + ",緯度," + linfo.Latitude + ",経度," + linfo.Longitude + ",海抜," + linfo.Elevation);
                sWriter.WriteLine("日時,乾球温度[C],絶対湿度[kg/kg(DA)],法線面直達日射量[W/m2],水平面天空日射量[W/m2],夜間放射量[W/m2],雲量(10分比),風向(degree),風速[m/s],大気圧[atm]");

                //1時間間隔のデータに変更
                WeatherDataTable houlyWDTable = wdt.ConvertToHoulyDataTable();
                ImmutableWeatherRecord wrs;
                for (int i = 0; i < houlyWDTable.WeatherRecordNumber; i++)
                {
                    wrs = houlyWDTable.GetWeatherRecord(i);
                    //夜間放射量[W/m2]を推定する
                    double wvp = MoistAir.GetWaterVaporPressure(wrs.GetData(WeatherRecord.RecordType.HumidityRatio).Value, wrs.GetData(WeatherRecord.RecordType.AtmosphericPressure).Value);
                    double nrd = Sky.GetNocturnalRadiation(wrs.GetData(WeatherRecord.RecordType.DryBulbTemperature).Value, wrs.GetData(WeatherRecord.RecordType.TotalSkyCover).Value * 10, wvp);

                    sWriter.WriteLine(wrs.DataDTime.ToString() + ","
                        + wrs.GetData(WeatherRecord.RecordType.DryBulbTemperature).Value.ToString("F1") + ","
                        + wrs.GetData(WeatherRecord.RecordType.HumidityRatio).Value.ToString("F5") + ","
                        + wrs.GetData(WeatherRecord.RecordType.DirectNormalRadiation).Value.ToString("F1") + ","
                        + wrs.GetData(WeatherRecord.RecordType.DiffuseHorizontalRadiation).Value.ToString("F1") + ","
                        + nrd.ToString("F1") + ","
                        + (wrs.GetData(WeatherRecord.RecordType.TotalSkyCover).Value * 10).ToString("F0") + ","
                        + wrs.GetData(WeatherRecord.RecordType.WindDirection).Value.ToString("F0") + ","
                        + wrs.GetData(WeatherRecord.RecordType.WindSpeed).Value.ToString("F1") + ","
                        + wrs.GetData(WeatherRecord.RecordType.AtmosphericPressure).Value.ToString("F2"));
                }
            }
        }

        #endregion

        #region 壁熱貫流テスト

        /// <summary>壁熱貫流テスト</summary>
        private static void wallHeatTransferTest()
        {
            WallLayers layers = new WallLayers();
            WallLayers.Layer layer;
            layer = new WallLayers.Layer(new WallMaterial("合板", 0.19, 716), 0.025);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("コンクリート", 1.4, 1934), 0.120);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("空気層", 1d / 0.086, 0), 0.020);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("ロックウール", 0.042, 84), 0.050);
            layers.AddLayer(layer);
            Wall wall = new Wall(layers);

            wall.TimeStep = 3600;
            wall.AirTemperature1 = 20;
            wall.AirTemperature2 = 10;
            wall.InitializeTemperature(10); //壁体内温度は10℃均一とする
            wall.SurfaceArea = 1;            

            Console.WriteLine("温度分布の推移");
            Console.WriteLine("合板, コンクリート, 空気層, ロックウール");
            double[] temps;
            for (int i = 0; i < 24; i++)
            {
                wall.Update();
                temps = wall.GetTemperatures();
                Console.Write((i + 1).ToString("F0").PadLeft(2) + "時間後 | ");
                for (int j = 0; j < temps.Length - 1; j++) Console.Write(((temps[j] + temps[j + 1]) / 2d).ToString("F1") + " | ");
                Console.WriteLine();
            }

            //定常状態まで進める
            for (int i = 0; i < 1000; i++) wall.Update();
            Console.WriteLine();
            Console.WriteLine("定常状態の温度分布");
            temps = wall.GetTemperatures();
            for (int j = 0; j < temps.Length - 1; j++) Console.Write(((temps[j] + temps[j + 1]) / 2d).ToString("F1") + " | ");

            Console.WriteLine();
            Console.WriteLine("定常状態の熱流1: " + wall.GetHeatTransfer(true).ToString("F1"));
            Console.WriteLine("定常状態の熱流2: " + wall.GetHeatTransfer(false).ToString("F1"));
            Console.WriteLine("定常状態の熱流3: " + wall.GetStaticHeatTransfer().ToString("F1"));

            Console.Read();
        }

        /// <summary>壁熱貫流テスト（冷温水配管埋設）</summary>
        private static void wallHeatTransferTest2()
        {
            WallLayers wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.FrexibleBoard), 0.0165));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial("水", 0.59, 4186), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial("水", 0.59, 4186), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.ExtrudedPolystyreneFoam_3), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.009));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.AirGap), 0.015));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.009));
            Wall wall = new Wall(wl);
            wall.TimeStep = 300;
            wall.AirTemperature1 = 20;
            wall.AirTemperature2 = 10;
            wall.SurfaceArea = 6.48;

            //配管を埋設
            Tube tube = new Tube(0.84, 0.346, 4186);            
            wall.AddTube(tube, 1);
            tube.SetFlowRate(0);    //最初は流量0
            tube.FluidTemperature = 30;

            wall.InitializeTemperature(20); //壁体温度を初期化

            for (int i = 0; i < wall.Layers.LayerNumber; i++) Console.Write("温度" + i + ", ");
            Console.WriteLine("配管への熱移動量[W], 配管出口温度[C]");
            for (int i = 0; i < 100; i++)
            {
                if (i == 50) tube.SetFlowRate(0.54);    //通水開始
                wall.Update();
                double[] tmp = wall.GetTemperatures();
                for (int j = 0; j < tmp.Length - 1; j++) Console.Write(((tmp[j] + tmp[j + 1]) / 2d).ToString("F1") + ", ");
                Console.Write(wall.GetHeatTransferToTube(1).ToString("F0") + ", " + tube.GetOutletFluidTemperature().ToString("F1"));
                Console.WriteLine();
            }
            Console.Read();
        }

        /// <summary>壁熱貫流テスト（潜熱蓄熱材）</summary>
        private static void wallHeatTransferTest3()
        {
            //初期温度
            const double INIT_TEMP = 35;

            //壁層を作成
            WallLayers wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.FrexibleBoard), 0.0165));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial("ダミー材料", 1, 1), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial("ダミー材料", 1, 1), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.ExtrudedPolystyreneFoam_3), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.009));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.AirGap), 0.015));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.009));
            
            //壁体を作成
            Wall wall = new Wall(wl);
            wall.TimeStep = 1200;
            wall.AirTemperature1 = 20;
            wall.AirTemperature2 = 20;
            wall.SurfaceArea = 6.48;

            //潜熱蓄熱材1を作成して設定
            LatentHeatStorageMaterial material1;
            material1 = new LatentHeatStorageMaterial(19, new WallMaterial("パッシブ・スミターマル（凝固）", 0.19, 3.6 * 1400));
            material1.AddMaterial(23, new WallMaterial("パッシブ・スミターマル（遷移）", (0.19 + 0.22) / 2d, 15.1 * 1400));
            material1.AddMaterial(100, new WallMaterial("パッシブ・スミターマル（融解）", 0.22, 3.6 * 1400));
            material1.Initialize(INIT_TEMP);
            wall.SetLatentHeatStorageMaterial(1, material1);

            //潜熱蓄熱材2を作成して設定
            LatentHeatStorageMaterial material2;
            material2 = new LatentHeatStorageMaterial(30, new WallMaterial("スミターマル（凝固）", 0.19, 3.6 * 1390));
            material2.AddMaterial(32, new WallMaterial("スミターマル（遷移）", (0.19 + 0.22) / 2d, 63.25 * 1400));
            material2.AddMaterial(100, new WallMaterial("スミターマル（融解）", 0.22, 3.5 * 1410));
            material2.Initialize(INIT_TEMP);
            wall.SetLatentHeatStorageMaterial(2, material2);

            //潜熱蓄熱材の間に配管を埋設
            Tube tube = new Tube(0.84, 0.346, 4186);
            wall.AddTube(tube, 1);
            tube.SetFlowRate(0);
            tube.FluidTemperature = 40;

            //壁体温度を初期化
            wall.InitializeTemperature(INIT_TEMP);

            for (int i = 0; i < wall.Layers.LayerNumber; i++) Console.Write("温度" + i + ", ");
            Console.WriteLine("蓄熱量[kJ]");
            for (int i = 0; i < 200; i++)
            {
                if (i == 100)
                {
                    tube.SetFlowRate(0.54);    //通水開始
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

        /// <summary>壁熱貫流テスト（潜熱蓄熱材）</summary>
        private static void wallHeatTransferTest4()
        {
            WallLayers layers = new WallLayers();
            WallLayers.Layer layer;
            layer = new WallLayers.Layer(new WallMaterial("コンクリート", 1.4, 1934), 0.060);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("潜熱蓄熱材A", 1.4, 1934), 0.030);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("潜熱蓄熱材B", 1.4, 1934), 0.030);
            layers.AddLayer(layer);
            layer = new WallLayers.Layer(new WallMaterial("コンクリート", 1.4, 1934), 0.060);
            layers.AddLayer(layer);
            Wall wall = new Wall(layers);

            LatentHeatStorageMaterial material = new LatentHeatStorageMaterial(11, new WallMaterial("潜熱蓄熱材A1", 1.4, 1934));
            material.AddMaterial(12, new WallMaterial("潜熱蓄熱材A2", 1.4, 1934 * 40));
            material.AddMaterial(100, new WallMaterial("潜熱蓄熱材A3", 1.4, 1934));
            wall.SetLatentHeatStorageMaterial(1, material);

            material = new LatentHeatStorageMaterial(13, new WallMaterial("潜熱蓄熱材B1", 1.4, 1934));
            material.AddMaterial(14, new WallMaterial("潜熱蓄熱材B2", 1.4, 1934 * 40));
            material.AddMaterial(100, new WallMaterial("潜熱蓄熱材B3", 1.4, 1934));
            wall.SetLatentHeatStorageMaterial(2, material);

            wall.AirTemperature1 = 20;
            wall.AirTemperature2 = 10;
            wall.SurfaceArea = 175;
            wall.GetSurface(true).ConvectiveRate = 1;
            wall.GetSurface(false).ConvectiveRate = 1;
            wall.TimeStep = 3600;

            StreamWriter sWriter = new StreamWriter("test.csv");
            wall.InitializeTemperature(10);
            Console.WriteLine("壁面温度[C]");
            for (int kkk = 0; kkk < 4; kkk++)
            {
                if (kkk % 2 == 0) wall.AirTemperature1 = 20;
                else wall.AirTemperature1 = 10;
                for (int i = 0; i < 24; i++)
                {
                    wall.Update();
                    double[] tmp = wall.GetTemperatures();
                    for (int j = 0; j < tmp.Length; j++) Console.Write(tmp[j].ToString("F2").PadLeft(5) + " | ");
                    Console.WriteLine();

                    for (int j = 0; j < tmp.Length; j++) sWriter.Write(tmp[j].ToString("F2").PadLeft(5) + " , ");
                    sWriter.WriteLine();
                }
            }
            sWriter.Close();

            Console.Read();
        }

        #endregion

        #region 窓熱取得テスト

        private static void windowTest1()
        {
            //気象データ//直達日射,天空放射,乾球温度,夜間放射
            double[] wdIdn = new double[] { 0, 0, 0, 0, 0, 244, 517, 679, 774, 829, 856, 862, 847, 809, 739, 619, 415, 97, 0, 0, 0, 0, 0, 0 };
            double[] wdIsky = new double[] { 0, 0, 0, 0, 21, 85, 109, 116, 116, 113, 110, 109, 111, 114, 116, 114, 102, 63, 0, 0, 0, 0, 0, 0 };
            double[] wdDbt = new double[] { 27, 27, 27, 27, 27, 28, 29, 30, 31, 32, 32, 33, 33, 33, 34, 33, 32, 32, 31, 30, 29, 29, 28, 28 };
            double[] wdRN = new double[] { 24, 24, 24, 24, 24, 24, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 26, 25, 25, 25, 25, 24, 24, 24 };

            //3mm透明ガラスの窓を作成
            GlassPanes.Pane pane = new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.TransparentGlass03mm);
            GlassPanes glassPane = new GlassPanes(pane);
            Window window = new Window(glassPane);
            
            //表面総合熱伝達率を設定
            WindowSurface ws = window.GetSurface(true);
            ws.FilmCoefficient = 23d;
            ws = window.GetSurface(false);
            ws.FilmCoefficient = 9.3;

            //屋外面の傾斜を設定//南向き垂直壁
            Incline incline = new Incline(Incline.Orientation.S, 0.5 * Math.PI);
            window.OutSideIncline = incline;

            //地表面反射率：アルベドを設定
            ws.Albedo = 0.2;

            //日除けは無し
            window.Shade = SunShade.EmptySunShade;

            //7月21日0:00の東京の太陽を作成
            Sun sun = new Sun(Sun.City.Tokyo);
            DateTime dTime = new DateTime(2001, 7, 21, 0, 0, 0);
            sun.Update(dTime);
            window.Sun = sun;

            //室内側乾球温度は25度で一定とする
            ws = window.GetSurface(false);
            ws.AirTemperature = 25;

            //計算結果タイトル行
            Console.WriteLine(" 時刻 | 透過日射[W] | 吸収日射[W] | 貫流熱[W] | 対流熱取得[W] | 放射熱取得[W]");

            //終日の計算実行
            for (int i = 0; i < 24; i++)
            {
                //日射量設定
                sun.SetGlobalHorizontalRadiation(wdIsky[i], wdIdn[i]);
                //夜間放射設定
                window.NocturnalRadiation = wdRN[i];
                //外気乾球温度を設定
                ws = window.GetSurface(true);
                ws.AirTemperature = wdDbt[i];

                //計算結果書き出し
                Console.WriteLine(dTime.ToShortTimeString().PadLeft(5) + " | " +
                    window.TransmissionHeatGain.ToString("F1").PadLeft(11) + " | " +
                    window.AbsorbedHeatGain.ToString("F1").PadLeft(11) + " | " +
                    window.TransferHeatGain.ToString("F1").PadLeft(9) + " | " +
                    window.ConvectiveHeatGain.ToString("F1").PadLeft(13) + " | " +
                    window.RadiativeHeatGain.ToString("F1").PadLeft(13));

                //時刻更新
                dTime = dTime.AddHours(1);
                sun.Update(dTime);
            }

            Console.Read();
        }

        private static void windowTest()
        {
            //パソコンによる空気調和計算法。pp.117

            //設計用気象データ
            double[] wdIdn = new double[] { 0, 0, 0, 0, 0, 244, 517, 679, 774, 829, 856, 862, 847, 809, 739, 619, 415, 97, 0, 0, 0, 0, 0, 0 };
            double[] wdIsky = new double[] { 0, 0, 0, 0, 21, 85, 109, 116, 116, 113, 110, 109, 111, 114, 116, 114, 102, 63, 0, 0, 0, 0, 0, 0 };
            double[] wdDbt = new double[] { 27.4, 27.1, 26.8, 26.5, 26.9, 27.7, 28.8, 29.8, 30.8, 31.5, 32.1, 32.6, 32.9, 33.2, 33.5, 33.1, 32.4, 31.5, 30.6, 29.8, 29.1, 28.5, 28.1, 27.7 };
            double[] wdAhd = new double[] { 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018, 0.018 };
            double[] wdRN = new double[] { 24, 24, 24, 24, 24, 24, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 26, 25, 25, 25, 25, 24, 24, 24 };

            GlassPanes wPropS = new GlassPanes(0.85, 0.02, 6.4);
            GlassPanes wPropE = new GlassPanes(0.85, 0.02, 6.4);
            Window winS = new Window(wPropS);
            WindowSurface ws = winS.GetSurface(true);
            winS.SurfaceArea = 1;
            ws.Albedo = 0.2;
            winS.OutSideIncline = new Incline(Incline.Orientation.S, Math.PI * 0.5);
            Window winE = new Window(wPropE);
            ws = winE.GetSurface(true);
            winE.SurfaceArea = 1;
            winE.OutSideIncline = new Incline(Incline.Orientation.E, Math.PI * 0.5);
            ws.Albedo = 0.2;

            SunShade ss1 = SunShade.MakeHorizontalSunShade(2, 1.8, 0.6, 0.25, 0.25, 0, winS.OutSideIncline);
            SunShade ss2 = SunShade.MakeVerticalSunShade(2, 1.8, 0.6, 0.5, winS.OutSideIncline);
            SunShade ss3 = SunShade.MakeGridSunShade(3, 2, 1.3, 0.6, 0.6, 0.3, -0.9, winE.OutSideIncline);

            Sun sun = new Sun(Sun.City.Tokyo);
            DateTime dTime = new DateTime(2001, 7, 21, 5, 0, 0);
            sun.Update(dTime);
            winS.Sun = sun;
            winE.Sun = sun;

            for (int i = 4; i < 18; i++)
            {
                sun.SetGlobalHorizontalRadiation(wdIsky[i], wdIdn[i]);
                winS.NocturnalRadiation = wdRN[i];
                winE.NocturnalRadiation = wdRN[i];
                ws = winS.GetSurface(true);
                ws.AirTemperature = wdDbt[i];
                ws = winE.GetSurface(true);
                ws.AirTemperature = wdDbt[i];

                Console.Write((i + 1).ToString() + "h: " + winS.TransmissionHeatGain.ToString("F0") + ", " + winS.AbsorbedHeatGain.ToString("F0") + ", ");
                winS.ShadowRate = ss1.GetShadowRate(sun);
                Console.Write(winS.ShadowRate.ToString("F3") + ", " + winS.TransmissionHeatGain.ToString("F0") + ", " + winS.AbsorbedHeatGain.ToString("F0") + ", ");
                winS.ShadowRate = ss2.GetShadowRate(sun);
                Console.Write(winS.ShadowRate.ToString("F3") + ", " + winS.TransmissionHeatGain.ToString("F0") + ", " + winS.AbsorbedHeatGain.ToString("F0") + ", ");
                winE.ShadowRate = ss3.GetShadowRate(sun);
                Console.WriteLine(winE.ShadowRate.ToString("F3") + ", " + winE.TransmissionHeatGain.ToString("F0") + ", " + winE.AbsorbedHeatGain.ToString("F0"));
                
                dTime = dTime.AddHours(1);
                sun.Update(dTime);
            }
        }

        #endregion

        #region 湿り空気物性書き出しテスト

        private static void outputMoistAirState()
        {
            StreamWriter sWriterl1 = new StreamWriter("MAir1.csv");
            StreamWriter sWriterl2 = new StreamWriter("MAir2.csv");
            double dbTemp = -10;
            while (dbTemp <= 50)
            {
                double aHumidMax = MoistAir.GetSaturatedHumidityRatio(dbTemp, MoistAir.Property.DryBulbTemperature);
                sWriterl2.Write(aHumidMax + "," + dbTemp);
                double aHumid = 0;
                double wbTemp;
                while (aHumid <= Math.Min(0.037, aHumidMax))
                {
                    wbTemp = MoistAir.GetAirStateFromDBHR(dbTemp, aHumid, MoistAir.Property.WetBulbTemperature, 101.325);
                    sWriterl1.WriteLine(dbTemp + "," + aHumid + "," + wbTemp);
                    sWriterl2.Write("," + wbTemp);
                    aHumid += 0.0005;
                }
                wbTemp = MoistAir.GetAirStateFromDBHR(dbTemp, aHumidMax, MoistAir.Property.WetBulbTemperature, 101.325);
                sWriterl1.WriteLine(dbTemp + "," + aHumid + "," + wbTemp);
                sWriterl2.Write("," + wbTemp);

                sWriterl2.WriteLine();
                dbTemp += 0.5;
            }
            sWriterl2.Close();
            sWriterl1.Close();
        }

        #endregion

        #region 回路網計算テスト

        /// <summary>回路網テスト1</summary>
        /// <remarks>2節点の流量計算</remarks>
        private static void circuitTest1()
        {
            //節点作成
            Node node1 = new Node("SampleNode1", 0, 10);
            Node node2 = new Node("SampleNode2", 0, 0);

            //流路作成・接続
            Channel channel = new Channel("SampleChannel", 2, 1.2);
            channel.Connect(node1, node2);

            //流量計算
            double flow = channel.GetFlow();

            Console.WriteLine("流量 : " + flow);
            Console.Read();
        }

        /// <summary>回路網テスト2</summary>
        /// <remarks>水回路の水量計算</remarks>
        private static void circuitTest2()
        {
            //設備基礎理論pp.337より***********************************************
            //      ------A------
            //      |           |
            //  ->--1--B--2--C--3-->-
            //            |     |
            //            ---D--
            Circuit circuit = new Circuit("水回路の水量計算");

            //節点追加
            ImmutableNode node1 = circuit.AddNode(new Node("1", 0, 0));
            ImmutableNode node2 = circuit.AddNode(new Node("2", 0, 0));
            ImmutableNode node3 = circuit.AddNode(new Node("3", 0, 0));

            //流路作成
            Channel chA = new Channel("A", 167, 2);
            Channel chB = new Channel("B", 192, 2);
            Channel chC = new Channel("C", 840, 2);
            Channel chD = new Channel("D", 4950, 2);
            //外部の系への流出流量
            circuit.SetExternalFlow(-7.06, node1);
            circuit.SetExternalFlow(7.06, node3);

            //接続処理
            ImmutableChannel channelA = circuit.ConnectNodes(node1, node3, chA);
            ImmutableChannel channelB = circuit.ConnectNodes(node1, node2, chB);
            ImmutableChannel channelC = circuit.ConnectNodes(node2, node3, chC);
            ImmutableChannel channelD = circuit.ConnectNodes(node2, node3, chD);

            //ソルバを用意
            CircuitSolver cSolver = new CircuitSolver(circuit);
            cSolver.Solve();
            Console.WriteLine("流路A流量:" + channelA.GetFlow());
            Console.WriteLine("流路B流量:" + channelB.GetFlow());
            Console.WriteLine("流路C流量:" + channelC.GetFlow());
            Console.WriteLine("流路D流量:" + channelD.GetFlow());
            Console.Read();
        }

        /// <summary>回路網テスト3</summary>
        /// <remarks>壁体の熱流計算</remarks>
        private static void circuitTest3()
        {
            Circuit circuit = new Circuit("壁体の熱流計算");

            //節点追加
            ImmutableNode[] nodes = new ImmutableNode[6];
            nodes[0] = circuit.AddNode(new Node("室1", 0));
            nodes[1] = circuit.AddNode(new Node("合板", 17.9));
            nodes[2] = circuit.AddNode(new Node("コンクリート", 232));
            nodes[3] = circuit.AddNode(new Node("空気層", 0));
            nodes[4] = circuit.AddNode(new Node("ロックウール", 4.2));
            nodes[5] = circuit.AddNode(new Node("室2", 0));

            //空気温度を境界条件とする
            circuit.SetBoundaryNode(true, nodes[0]);
            circuit.SetBoundaryNode(true, nodes[5]);
            //空気温度設定
            circuit.SetPotential(20, nodes[0]);
            circuit.SetPotential(10, nodes[5]);
            for (int i = 1; i < 5; i++) circuit.SetPotential(10, nodes[i]); //壁体内温度は10℃均一とする

            //接続処理
            ImmutableChannel channel01 = circuit.ConnectNodes(nodes[0], nodes[1], new Channel("室1-合板", 174, 1));
            ImmutableChannel channel12 = circuit.ConnectNodes(nodes[1], nodes[2], new Channel("合板-コンクリート", 109, 1));
            ImmutableChannel channel34 = circuit.ConnectNodes(nodes[2], nodes[3], new Channel("コンクリート-空気層", 86, 1));
            ImmutableChannel channel45 = circuit.ConnectNodes(nodes[3], nodes[4], new Channel("空気層-ロックウール", 638, 1));
            ImmutableChannel channel56 = circuit.ConnectNodes(nodes[4], nodes[5], new Channel("ロックウール-室2", 703, 1));

            CircuitSolver cSolver = new CircuitSolver(circuit);
            cSolver.TimeStep = 3600;

            for (int i = 0; i < nodes.Length; i++) Console.Write(nodes[i].Name + ", ");
            Console.WriteLine();
            for (int i = 0; i < 24; i++)
            {
                cSolver.Solve();
                Console.Write((i + 1) + "H, ");
                for (int j = 0; j < nodes.Length; j++) Console.Write(nodes[j].Potential.ToString("F1") + ", ");
                Console.WriteLine();
            }
            Console.Read();
        }

        /// <summary>回路網テスト4</summary>
        /// <remarks>壁体の熱流計算（潜熱変化付き）</remarks>
        private static void circuitTest4()
        {
            Circuit circuit = new Circuit("潜熱蓄熱材を持つ床の熱流計算");

            //節点追加 Initialize("", 0.350, 1600.0, mType);
            ImmutableNode[] nodes = new ImmutableNode[7];
            nodes[0] = circuit.AddNode(new Node("室内", 0));
            nodes[1] = circuit.AddNode(new Node("フレキシブルボード", 1600.0 * 0.0165));
            nodes[2] = circuit.AddNode(new Node("スミターマル20C", 0));
            nodes[3] = circuit.AddNode(new Node("発熱層", 0));
            nodes[4] = circuit.AddNode(new Node("スミターマル30C", 0));
            nodes[5] = circuit.AddNode(new Node("ロックウール", 84.0 * 0.065));
            nodes[6] = circuit.AddNode(new Node("床下", 0));

            //空気温度を境界条件とする
            circuit.SetBoundaryNode(true, nodes[0]);
            circuit.SetBoundaryNode(true, nodes[5]);
            //空気温度設定
            circuit.SetPotential(20, nodes[0]);
            circuit.SetPotential(10, nodes[5]);

            //接続処理
            ImmutableChannel channel01 = circuit.ConnectNodes(nodes[0], nodes[1], new Channel("室1-合板", 173.32, 1));
            ImmutableChannel channel12 = circuit.ConnectNodes(nodes[1], nodes[2], new Channel("合板-コンクリート", 108.65, 1));
            ImmutableChannel channel34 = circuit.ConnectNodes(nodes[2], nodes[3], new Channel("コンクリート-空気層", 128.86, 1));
            ImmutableChannel channel45 = circuit.ConnectNodes(nodes[3], nodes[4], new Channel("空気層-ロックウール", 681.24, 1));
            ImmutableChannel channel56 = circuit.ConnectNodes(nodes[4], nodes[5], new Channel("ロックウール-室2", 702.76, 1));
            ImmutableChannel channel67 = circuit.ConnectNodes(nodes[5], nodes[6], new Channel("ロックウール-室2", 702.76, 1));

            CircuitSolver cSolver = new CircuitSolver(circuit);
            cSolver.TimeStep = 3600;

            for (int i = 0; i < nodes.Length; i++) Console.Write(nodes[i].Name + "   ");
            Console.WriteLine();
            for (int i = 0; i < 24; i++)
            {
                cSolver.Solve();
                Console.Write((i + 1) + "H : ");
                for (int j = 0; j < nodes.Length; j++) Console.Write(nodes[j].Potential.ToString("F1") + "   ");
                Console.WriteLine();
            }
            Console.Read();
        }

        #endregion

        #region Skyテスト

        private static void SkyTest()
        {
            //北西の垂直壁面への入射日射量を計算
            //太陽高度 45.2°, 方位角83.3°とする

            //傾斜面の方向余弦を取得
            double ws, ww, wz;
            Incline.GetDirectionCosine(Sky.DegreeToRadian(112.5), Sky.DegreeToRadian(90), out wz, out ws, out ww);
            //直達日射量の比率を計算
            Console.WriteLine("Cosθ=" + Incline.GetDirectSolarRadiationRateToIncline(wz, ws, ww, Sky.DegreeToRadian(45.2), Sky.DegreeToRadian(83.3)));
            //0.615程度となるはず

            //夜間放射テスト
            //外気温度-0.5度、水蒸気分圧0.23[kPa]、雲量0の夜間放射
            Console.WriteLine(Sky.GetNocturnalRadiation(-0.5, 0, 0.23));
            //122[W/m2]程度となるはず
        }

        /// <summary>気象状態計算の例</summary>
        private static void weatherTest()
        {
            //東京における太陽を作成
            Sun sun = new Sun(Sun.City.Tokyo);

            //太陽位置を12月21日12時に調整
            DateTime dTime = new DateTime(1983, 12, 21, 12, 0, 0);
            sun.Update(dTime);
            sun.GlobalHorizontalRadiation = 467;

            Console.WriteLine("東京の12月21日12時における");
            Console.WriteLine("太陽高度=" + Sky.RadianToDegree(sun.Altitude).ToString("F1") + " 度");
            Console.WriteLine("太陽方位=" + Sky.RadianToDegree(sun.Orientation).ToString("F1") + " 度");
            Console.WriteLine("水平面全天日射=" + sun.GlobalHorizontalRadiation.ToString("F1") + " W/m2");

            //傾斜面を作成（南西の垂直面と東の45°傾斜面）
            Incline seInc = new Incline(Incline.Orientation.SE, 0.5 * Math.PI);
            Incline wInc = new Incline(Incline.Orientation.W, 0.25 * Math.PI);

            //直散分離の手法を取得
            Array methods = Enum.GetValues(typeof(Sun.DiffuseAndDirectNormalRadiationEstimatingMethod));
            foreach (Sun.DiffuseAndDirectNormalRadiationEstimatingMethod method in methods)
            {
                //直散分離を実行して太陽に設定
                sun.EstimateDiffuseAndDirectNormalRadiation(sun.GlobalHorizontalRadiation, method);

                //傾斜面へ入射する直達日射成分を計算する
                double cosThetaSE, cosThetaW;
                cosThetaSE = seInc.GetDirectSolarRadiationRate(sun);
                cosThetaW = wInc.GetDirectSolarRadiationRate(sun);

                Console.WriteLine();
                Console.WriteLine("直散分離手法 : " + method.ToString());
                Console.WriteLine("法線面直達日射=" + sun.DirectNormalRadiation.ToString("F1") + " W/m2");
                Console.WriteLine("水平面直達日射=" + (sun.DirectNormalRadiation * Math.Sin(sun.Altitude)).ToString("F1") + " W/m2");
                Console.WriteLine("天空日射=" + sun.DiffuseHorizontalRadiation.ToString("F1") + " W/m2");
                Console.WriteLine("南西垂直面の直達日射=" + (sun.DirectNormalRadiation * cosThetaSE).ToString("F1") + " W/m2");
                Console.WriteLine("東45度面の直達日射=" + (sun.DirectNormalRadiation * cosThetaW).ToString("F1") + " W/m2");

            }
            Console.Read();
        }

        #endregion

        #region TSC21テスト

        private static void tscTest()
        {
            string str = "aaaBB[ccD]eee_f_g_hhhII[jJ]kkk_ll";
            TSCObject tscObj;
            bool success = TSCObject.TryMakeTSCObjectFromTSCCode(str, out tscObj);
        }

        #endregion

        #region ガラステスト

        private static void glassPanesTest()
        {
            //ガラス板を作成
            GlassPanes.Pane[] panes = new GlassPanes.Pane[2];

            //室内側は6mmの透明ガラス、室外側は6mmの熱線吸収ガラスの場合
            panes[0] = new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.TransparentGlass06mm);
            panes[1] = new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.HeatAbsorbingGlass06mm);

            //物性確認
            Console.WriteLine("透明ガラスの透過率=" + panes[0].InnerSideTransmissivity.ToString("F2"));
            Console.WriteLine("透明ガラスの吸収率=" + panes[0].InnerSideAbsorptivity.ToString("F2"));
            Console.WriteLine("熱線吸収ガラスの透過率=" + panes[1].InnerSideTransmissivity.ToString("F2"));
            Console.WriteLine("熱線吸収ガラスの吸収率=" + panes[1].InnerSideAbsorptivity.ToString("F2"));
            Console.WriteLine();            

            //ガラス作成
            GlassPanes glass = new GlassPanes(panes);

            //空気層の総合熱伝達率[W/(m2-K)]を設定
            glass.SetHeatTransferCoefficientsOfGaps(0, 6d);

            Console.WriteLine("室内側：透明ガラス　室外側：熱線吸収ガラス");
            Console.WriteLine("総合透過率[-] = " + glass.OverallTransmissivity.ToString("F3"));
            Console.WriteLine("総合吸収率[-] = " + glass.OverallAbsorptivity.ToString("F3"));
            //Console.WriteLine("熱貫流率[W/(m2-K)]" + glass.OverallHeatTransferCoefficient.ToString("F3"));
            Console.WriteLine();

            //室内側は6mmの熱線吸収ガラス、室外側は6mmの透明ガラスの場合
            panes[0] = new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.HeatAbsorbingGlass06mm);
            panes[1] = new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.TransparentGlass06mm);

            //ガラス作成
            glass = new GlassPanes(panes);

            //空気層の総合熱伝達率[W/(m2-K)]を設定
            glass.SetHeatTransferCoefficientsOfGaps(0, 6d);

            Console.WriteLine("室内側：熱線吸収ガラス　室外側：透明ガラス");
            Console.WriteLine("総合透過率[-] = " + glass.OverallTransmissivity.ToString("F3"));
            Console.WriteLine("総合吸収率[-] = " + glass.OverallAbsorptivity.ToString("F3"));
            //Console.WriteLine("熱貫流率[W/(m2-K)]" + glass.OverallHeatTransferCoefficient.ToString("F3"));

            Console.Read();
        }

        #endregion

        #region 多数室テスト

        /// <summary>多数室テスト</summary>
        /// <remarks>
        /// 東西2室で壁・窓表面の相互放射を考慮した計算を行う。
        /// 東側は南北で2ゾーンに分割する。
        /// 壁体は全て200mmのコンクリートとする。
        /// 幅×奥行き×高さ = 8m×7m×3mとする。
        /// 地面（床）は考慮しない。
        /// 
        ///        N1           N2
        ///   -------------------------
        ///   |           |           |
        ///   |           |    znE1   | E1
        ///   |           |           |
        /// W |    znW    |- - - - - -|
        ///   |           |           |
        ///   |           |    znE2   | E2
        ///   |           |           |
        ///   ----+++++----------------
        ///        S1           S2
        /// 
        /// </remarks>
        private static void multiRoomTest()
        {
            const double TIME_STEP = 3600;
            const double INIT_TEMP = 15;
            const double H_GAIN = 0;
            const int ITER_NUM = 100;
            const double W_AI = 8;
            const double E_AI = 9.3;
            const double W_AO = 20;
            const double E_AO = 23;
            bool USE_TUBE = false;

            //モデル作成処理*********************************************************

            //屋外
            Outdoor outDoor = new Outdoor();
            Sun sun = new Sun(Sun.City.Tokyo);
            sun.Update(new DateTime(2001, 1, 1, 0, 0, 0));
            outDoor.Sun = sun;

            //壁リスト
            Wall[] walls = new Wall[12];

            //ゾーンを作成
            Zone znW = new Zone();
            znW.Volume = 7 * 4 * 3;
            znW.SensibleHeatCapacity = znW.Volume * 12000;//単位容積あたり12kJ
            Zone znE1 = new Zone();
            znE1.Volume = 3.5 * 4 * 3;
            znE1.SensibleHeatCapacity = znE1.Volume * 12000;//単位容積あたり12kJ
            Zone znE2 = new Zone();
            znE2.Volume = 3.5 * 4 * 3;
            znE2.SensibleHeatCapacity = znE2.Volume * 12000;//単位容積あたり12kJ
            znW.TimeStep = znE1.TimeStep = znE2.TimeStep = TIME_STEP;

            //壁構成を作成（コンクリート）
            WallLayers layers;
            WallLayers.Layer layer;
            layers = new WallLayers();
            layer = new WallLayers.Layer(new WallMaterial("コンクリート", 1.4, 1934), 0.150, 2);
            layers.AddLayer(layer);

            //西外壁
            walls[0] = new Wall(layers);
            walls[0].SurfaceArea = 7 * 3;
            WallSurface ews = walls[0].GetSurface(true);
            WallSurface iws = walls[0].GetSurface(false);
            ews.FilmCoefficient = W_AO;
            iws.FilmCoefficient = W_AI;
            walls[0].SetIncline(new Incline(Incline.Orientation.W, 0.5 * Math.PI), true);
            znW.AddSurface(iws);
            outDoor.AddWallSurface(ews);

            //東外壁1
            walls[1] = new Wall(layers);
            walls[1].SurfaceArea = 3.5 * 3;
            ews = walls[1].GetSurface(true);
            iws = walls[1].GetSurface(false);
            ews.FilmCoefficient = E_AO;
            iws.FilmCoefficient = E_AI;
            walls[1].SetIncline(new Incline(Incline.Orientation.E, 0.5 * Math.PI), true);
            znE1.AddSurface(iws);
            outDoor.AddWallSurface(ews);

            //東外壁2
            walls[2] = new Wall(layers);
            walls[2].SurfaceArea = 3.5 * 3;
            ews = walls[2].GetSurface(true);
            iws = walls[2].GetSurface(false);
            ews.FilmCoefficient = E_AO;
            iws.FilmCoefficient = E_AI;
            walls[2].SetIncline(new Incline(Incline.Orientation.E, 0.5 * Math.PI), true);
            znE2.AddSurface(iws);
            outDoor.AddWallSurface(ews);

            //北外壁1
            walls[3] = new Wall(layers);
            walls[3].SurfaceArea = 4 * 3;
            ews = walls[3].GetSurface(true);
            iws = walls[3].GetSurface(false);
            ews.FilmCoefficient = W_AO;
            iws.FilmCoefficient = W_AI;
            walls[3].SetIncline(new Incline(Incline.Orientation.N, 0.5 * Math.PI), true);
            znW.AddSurface(iws);
            outDoor.AddWallSurface(ews);

            //北外壁2
            walls[4] = new Wall(layers);
            walls[4].SurfaceArea = 4 * 3;
            ews = walls[4].GetSurface(true);
            iws = walls[4].GetSurface(false);
            ews.FilmCoefficient = E_AO;
            iws.FilmCoefficient = E_AI;
            walls[4].SetIncline(new Incline(Incline.Orientation.N, 0.5 * Math.PI), true);
            znE1.AddSurface(iws);
            outDoor.AddWallSurface(ews);

            //南外壁1
            walls[5] = new Wall(layers);
            walls[5].SurfaceArea = 4 * 3;
            ews = walls[5].GetSurface(true);
            iws = walls[5].GetSurface(false);
            ews.FilmCoefficient = W_AO;
            iws.FilmCoefficient = W_AI;
            walls[5].SetIncline(new Incline(Incline.Orientation.S, 0.5 * Math.PI), true);
            znW.AddSurface(iws);
            outDoor.AddWallSurface(ews);

            //南外壁2
            walls[6] = new Wall(layers);
            walls[6].SurfaceArea = 4 * 3;
            ews = walls[6].GetSurface(true);
            iws = walls[6].GetSurface(false);
            ews.FilmCoefficient = E_AO;
            iws.FilmCoefficient = E_AI;
            walls[6].SetIncline(new Incline(Incline.Orientation.S, 0.5 * Math.PI), true);
            znE2.AddSurface(iws);
            outDoor.AddWallSurface(ews);

            //屋根1
            walls[7] = new Wall(layers);
            walls[7].SurfaceArea = 4 * 7;
            ews = walls[7].GetSurface(true);
            iws = walls[7].GetSurface(false);
            ews.FilmCoefficient = W_AO;
            iws.FilmCoefficient = W_AI;
            walls[7].SetIncline(new Incline(Incline.Orientation.N, 0), true);
            znW.AddSurface(iws);
            outDoor.AddWallSurface(ews);

            //屋根2
            walls[8] = new Wall(layers);
            walls[8].SurfaceArea = 4 * 3.5;
            ews = walls[8].GetSurface(true);
            iws = walls[8].GetSurface(false);
            ews.FilmCoefficient = E_AO;
            iws.FilmCoefficient = E_AI;
            walls[8].SetIncline(new Incline(Incline.Orientation.N, 0), true);
            znE1.AddSurface(iws);
            outDoor.AddWallSurface(ews);

            //屋根3
            walls[9] = new Wall(layers);
            walls[9].SurfaceArea = 4 * 3.5;
            ews = walls[9].GetSurface(true);
            iws = walls[9].GetSurface(false);
            ews.FilmCoefficient = E_AO;
            iws.FilmCoefficient = E_AI;
            walls[9].SetIncline(new Incline(Incline.Orientation.N, 0), true);
            znE2.AddSurface(iws);
            outDoor.AddWallSurface(ews);

            //内壁1
            walls[10] = new Wall(layers);
            walls[10].SurfaceArea = 7 * 3.5;
            ews = walls[10].GetSurface(true);
            iws = walls[10].GetSurface(false);
            ews.FilmCoefficient = E_AI;
            iws.FilmCoefficient = W_AI;
            znW.AddSurface(iws);
            znE1.AddSurface(ews);

            //内壁2
            walls[11] = new Wall(layers);
            walls[11].SurfaceArea = 7 * 3.5;
            ews = walls[11].GetSurface(true);
            iws = walls[11].GetSurface(false);
            ews.FilmCoefficient = E_AI;
            iws.FilmCoefficient = W_AI;
            znW.AddSurface(iws);
            znE2.AddSurface(ews);

            //南窓
            GlassPanes gPanes = new GlassPanes(new GlassPanes.Pane[] {
                new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.TransparentGlass03mm),
                new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.HeatAbsorbingGlass03mm)
            });
            //外側ブラインド
            GlassPanes gPanesWithBlind = new GlassPanes(new GlassPanes.Pane[] {
                new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.TransparentGlass03mm),
                new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.HeatAbsorbingGlass03mm),
                new GlassPanes.Pane(0.05, 0.35, 9999)
            });
            Window window = new Window(gPanes, new Incline(Incline.Orientation.S, 0.5 * Math.PI));
            window.SurfaceArea = 1 * 3;
            WindowSurface ws1 = window.GetSurface(true);
            WindowSurface ws2 = window.GetSurface(false);
            ws1.LongWaveEmissivity = 0.7;
            ws1.FilmCoefficient = W_AO;
            ws2.FilmCoefficient = W_AI;
            znW.AddWindow(window);
            outDoor.AddWindow(window);

            /*//debug
            for (int i = 0; i < walls.Length; i++)
            {
                walls[i].GetSurface(true).ConvectiveRate = 0;
                walls[i].GetSurface(false).ConvectiveRate = 0;
            }
            //window.GetSurface(false).ConvectiveRate = 0;
            //debug*/

            //発熱体*****************************************************************
            znW.AddHeatGain(new ConstantHeatGain(0, H_GAIN, 0));

            //制御*******************************************************************
            znE1.ControlDrybulbTemperature = false;
            znE1.DrybulbTemperatureSetPoint = 20;
            znE2.ControlDrybulbTemperature = false;
            znE2.DrybulbTemperatureSetPoint = 20;
            znW.ControlDrybulbTemperature = false;
            znW.DrybulbTemperatureSetPoint = 20;

            //外界条件設定***********************************************************
            outDoor.AirState = new MoistAir(30, 0.020);
            outDoor.SetWallSurfaceBoundaryState();

            //天井に冷水配管を設置
            if (USE_TUBE)
            {
                Tube tube = new Tube(0.999, 0.275, 4186);
                tube.SetFlowRate(0.222);
                tube.FluidTemperature = 10;
                walls[7].AddTube(tube, 0);
                walls[8].AddTube(tube, 0);
                walls[9].AddTube(tube, 0);
            }

            //室温・壁温初期化*******************************************************
            for (int i = 0; i < walls.Length; i++) walls[i].InitializeTemperature(INIT_TEMP);
            znE1.InitializeAirState(INIT_TEMP, 0.015);
            znE2.InitializeAirState(INIT_TEMP, 0.015);
            znW.InitializeAirState(INIT_TEMP, 0.015);

            //多数室オブジェクトを作成・初期化
            Room[] rooms = new Room[2];
            rooms[0] = new Room(new Zone[] { znW });
            rooms[1] = new Room(new Zone[] { znE1, znE2 });
            MultiRoom mRoom = new MultiRoom(rooms);
            mRoom.SetTimeStep(TIME_STEP);
            //初期化
            mRoom.Initialize();

            //室間換気設定***********************************************************
            znW.VentilationVolume = znW.Volume * 0;
            znE1.VentilationVolume = znE1.Volume * 0;
            znE2.VentilationVolume = znE2.Volume * 0;
            znW.VentilationAirState = outDoor.AirState;
            znE1.VentilationAirState = outDoor.AirState;
            znE2.VentilationAirState = outDoor.AirState;
            //mRoom.SetAirFlow(znE1, znE2, 10);
            //mRoom.SetAirFlow(znE2, znW, 10);
            //mRoom.SetAirFlow(znW, znE1, 10);

            //熱収支確認用変数*******************************************************
            double heatTransferToExWall = 0;
            double heatTransferToTube = 0;
            double heatTransferToWindow = 0;
            double oaLoad = 0;

            //外気データ読み込み*****************************************************
            StreamReader sReader = new StreamReader("BESTestWeather.csv");
            sReader.ReadLine();

            //室温更新テスト*********************************************************
            for (int i = 0; i < ITER_NUM; i++)
            {
                //外気条件設定
                string[] wData = sReader.ReadLine().Split(',');
                outDoor.AirState = new MoistAir(double.Parse(wData[2]), double.Parse(wData[3]));
                sun.DirectNormalRadiation = double.Parse(wData[4]);
                sun.GlobalHorizontalRadiation = double.Parse(wData[5]);
                sun.DiffuseHorizontalRadiation = double.Parse(wData[6]);
                outDoor.NocturnalRadiation = double.Parse(wData[8]);

                //12~13時は外側ブラインドを利用
                if (sun.CurrentDateTime.Hour == 12 && sun.CurrentDateTime.Minute == 0) window.Initialize(gPanesWithBlind);
                if (sun.CurrentDateTime.Hour == 13 && sun.CurrentDateTime.Minute == 0) window.Initialize(gPanes);

                Console.WriteLine(
                    znW.CurrentDrybulbTemperature.ToString("F3").PadLeft(5) + " | "
                    + znE1.CurrentDrybulbTemperature.ToString("F3").PadLeft(5) + " | "
                    + znE2.CurrentDrybulbTemperature.ToString("F3").PadLeft(5) + " | "
                    + znW.CurrentSensibleHeatLoad.ToString("F2").PadLeft(5) + " | "
                    + znE1.CurrentSensibleHeatLoad.ToString("F2").PadLeft(5) + " | "
                    + znE2.CurrentSensibleHeatLoad.ToString("F2").PadLeft(5) + " | "
                    + walls[0].GetWallTemprature(true).ToString("F2").PadLeft(5) + " | "
                    + walls[0].GetWallTemprature(false).ToString("F2").PadLeft(5) + " | "
                    );

                //壁体状態更新
                for (int j = 0; j < walls.Length; j++) walls[j].Update();
                //外気条件を壁に設定
                outDoor.SetWallSurfaceBoundaryState();
                //室状態更新
                mRoom.UpdateRoomTemperatures();
                mRoom.UpdateRoomHumidities();

                //壁体への熱移動量を積算
                for (int j = 0; j < 10; j++) heatTransferToExWall += walls[j].GetHeatTransfer(true);
                //窓面への熱移動量を積算
                heatTransferToWindow += window.AbsorbedHeatGain + window.TransferHeatGain + window.TransmissionHeatGain;
                heatTransferToWindow -= rooms[0].TransmissionHeatLossFromWindow + rooms[1].TransmissionHeatLossFromWindow;
                //チューブへの熱移動量を積算
                heatTransferToTube += walls[7].GetHeatTransferToTube(0) + walls[8].GetHeatTransferToTube(0) + walls[9].GetHeatTransferToTube(0);
                //外気負荷を計算
                Zone[] zns = new Zone[] { znE1, znE2, znW };
                for (int j = 0; j < zns.Length; j++)
                {
                    if (zns[j].VentilationVolume != 0)
                    {
                        double airDS = 1d / (MoistAir.GetAirStateFromDBHR(zns[j].CurrentDrybulbTemperature, zns[j].CurrentHumidityRatio, MoistAir.Property.SpecificVolume));
                        double cpAir = MoistAir.GetSpecificHeat(zns[j].CurrentHumidityRatio);
                        oaLoad += zns[j].VentilationVolume * airDS * cpAir * (zns[j].VentilationAirState.DryBulbTemperature - zns[j].CurrentDrybulbTemperature);
                    }
                }

                //日時更新
                sun.Update(sun.CurrentDateTime.AddSeconds(TIME_STEP));
            }
            sReader.Close();

            //熱収支を書き出し
            //屋外から壁体への熱移動量[MJ]
            heatTransferToExWall *= TIME_STEP / 1000000d;
            //窓面への熱移動量[MJ]
            heatTransferToWindow *= TIME_STEP / 1000000d;
            //壁体からチューブへの熱移動量[MJ]
            heatTransferToTube *= TIME_STEP / 1000000d;
            //外気負荷[MJ]
            oaLoad *= TIME_STEP / 1000000d / 3.6;
            //壁体蓄熱量[MJ]
            double wallHeatStorage = 0;
            for (int i = 0; i < walls.Length; i++) wallHeatStorage += walls[i].GetHeatStorage(INIT_TEMP);
            wallHeatStorage /= 1000d;
            //室蓄熱量[MJ]
            double zoneHeatStorage = (znE1.GetHeatStorage(INIT_TEMP) + znE2.GetHeatStorage(INIT_TEMP) + znW.GetHeatStorage(INIT_TEMP)) / 1000d;
            //発熱量
            double heatGain = (H_GAIN * TIME_STEP * ITER_NUM) / 1000000;

            //書き出し
            Console.WriteLine("壁体への熱移動[MJ] | 窓面への熱移動[MJ] | 壁体の蓄熱量[MJ] | 室の蓄熱量[MJ] | 発熱量[MJ] | チューブへの熱移動[MJ] | 外気負荷[MJ]");
            Console.WriteLine(heatTransferToExWall.ToString("F2") + " | " +  heatTransferToWindow.ToString("F2") + " | " +
                wallHeatStorage.ToString("F2") + " | " + zoneHeatStorage.ToString("F2") + " | " + heatGain + " | " + heatTransferToTube.ToString("F2") + " | " + oaLoad.ToString("F2"));
            Console.WriteLine("熱収支[MJ] = " + (heatTransferToExWall + heatTransferToWindow - heatTransferToTube - wallHeatStorage - zoneHeatStorage + heatGain + oaLoad));

            Console.Read();
        }

        #endregion

        #region 壁体熱貫流率計算テスト

        private static void wallOverallHeatTransferCoefTest()
        {
            //多層壁オブジェクトを作成
            WallLayers wLayers = new WallLayers("熱貫流率計算用多層壁");

            //壁層の素材を作成
            WallMaterial[] materials = new WallMaterial[4];
            //第1層：合板
            materials[0] = new WallMaterial(WallMaterial.PredefinedMaterials.Plywood);
            //第2層：非密閉空気層
            materials[1] = new WallMaterial(WallMaterial.PredefinedMaterials.AirGap);
            //第3層：鉄筋コンクリート
            materials[2] = new WallMaterial(WallMaterial.PredefinedMaterials.ReinforcedConcrete);
            //第4層：漆喰
            materials[3] = new WallMaterial("漆喰", 0.7, 1000);

            //壁の各層を作成
            //合板:20mm
            wLayers.AddLayer(new WallLayers.Layer(materials[0], 0.02));
            //空気層:厚みは関係なし
            wLayers.AddLayer(new WallLayers.Layer(materials[1], 0.01));
            //鉄筋コンクリート:150mm
            wLayers.AddLayer(new WallLayers.Layer(materials[2], 0.15));
            //漆喰:10mm
            wLayers.AddLayer(new WallLayers.Layer(materials[3], 0.01));

            //結果書き出し
            Console.WriteLine("壁層の構成");
            for (uint i = 0; i < wLayers.LayerNumber; i++)
            {
                WallLayers.Layer layer = wLayers.GetLayer(i);
                Console.WriteLine("第" + (i + 1) + "層：" + layer.Material.Name + "(" + layer.Thickness + "m)");
            }
            Console.WriteLine("熱貫流率=" + wLayers.GetThermalTransmission().ToString("F1") + " W/(m2-K)");
            Console.WriteLine();

            //軽量コンクリートに変えてみる
            wLayers.ReplaceLayer(2, new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.LightweightConcrete), 0.15));

            //結果書き出し
            Console.WriteLine("壁層の構成");
            for (uint i = 0; i < wLayers.LayerNumber; i++)
            {
                WallLayers.Layer layer = wLayers.GetLayer(i);
                Console.WriteLine("第" + (i + 1) + "層：" + layer.Material.Name + "(" + layer.Thickness + "m)");
            }
            Console.WriteLine("熱貫流率=" + wLayers.GetThermalTransmission().ToString("F1") + " W/(m2-K)");

            Console.Read();
        }

        #endregion

        #region 人体モデルテスト

        /// <summary>人体モデル計算の例</summary>
        private static void humanBodyTest()
        {
            //標準体躯の場合は引数不要
            //HumanBody body = new HumanBody();

            //人体モデルを作成:体重70kg,身長1.6m,年齢35歳,女性,心係数2.58,体脂肪率20%
            HumanBody body = new HumanBody(70, 1.6, 35, false, 2.58, 20);

            //着衣量[clo]を設定
            body.SetClothingIndex(0);
            //乾球温度[C]を設定
            body.SetDrybulbTemperature(42);
            //放射温度[C]を設定
            body.SetMeanRadiantTemperature(42);
            //気流速度[m/s]を設定
            body.SetVelocity(1.0);
            //相対湿度[%]を設定
            body.SetRelativeHumidity(50);

            //特定の部位の条件のみを設定したい場合（右手先のみ乾球温度20Cとした）
            body.SetDrybulbTemperature(HumanBody.Nodes.RightHand, 20);

            //時間を経過させ、状態を書き出す
            Console.WriteLine("時刻  |  右肩コア温度  |  右肩皮膚温度  |  左肩コア温度  |  左肩皮膚温度");
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

        #region 応答係数テスト

        private static void rFactorSample()
        {
            WallLayers wallLayers = new WallLayers();
            wallLayers.AddLayer(new WallLayers.Layer(new WallMaterial("コンクリート", 1.4, 1934), 0.15));
            wallLayers.AddLayer(new WallLayers.Layer(new WallMaterial("ロックウール", 0.042, 84), 0.05));
            wallLayers.AddLayer(new WallLayers.Layer(new WallMaterial("中空層", 11.6, 0), 0.02));
            wallLayers.AddLayer(new WallLayers.Layer(new WallMaterial("アルミ化粧板", 210, 2373), 0.002));

            double[] rfx = new double[8];
            double[] rfy = new double[8];
            double[] rfz = new double[8];
            double commonRatio = 0;
            ResponseFactor.GetResponseFactor(3600, 9.3, 23.0, wallLayers, 8, ref rfx, ref rfy, ref rfz, out commonRatio);

            Console.WriteLine(wallLayers.GetThermalTransmission(9.3, 23));

            double[] temperatures1 = new double[] { 27.4, 27.1, 26.8, 26.5, 26.9, 27.7, 28.8, 29.8, 30.8, 31.5, 32.1, 32.6, 32.9, 33.2, 33.5, 33.1, 32.4, 31.5, 30.6, 29.8, 29.1, 28.5, 28.1, 27.7 };
            double[] temperatures2 = new double[24];
            for (int i = 0; i < temperatures2.Length; i++) temperatures2[i] = 26.0d;
            double[] qloads = new double[24];
            int hour = 0;
            double kValue = wallLayers.GetThermalTransmission(9.3,23.0);
            double q1 = 0;
            double q2 = 0;
            while (true)
            {
                Console.Write(hour.ToString() + "時：　");

                ResponseFactor.GetHeatFlow(temperatures1, temperatures2, rfx, rfy, rfz, commonRatio, q1, q2, out q1, out q2);
                Console.WriteLine(temperatures1[0].ToString("F1") + " C  " + q2.ToString("F1") + " W/m2  " + (q2 / kValue).ToString("F1") + " C");
                if (Math.Abs(q2 - qloads[hour]) < 0.0001) break;
                else qloads[hour] = q2;

                //温度をずらす
                double tmp = temperatures1[0];
                for (int j = 1; j < temperatures1.Length; j++) temperatures1[j - 1] = temperatures1[j];
                temperatures1[temperatures1.Length - 1] = tmp;

                hour++;
                if (hour == 24) hour = 0;
            }
        }

        #endregion

    }
}
