using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Popolo.ThermophysicalProperty;
using Popolo.Weather;
using Popolo.ThermalLoad;

namespace AIGStandardResidence
{
    class Program
    {

        #region 定数宣言

        const double AO = 20 * 1.163;
        const double AI = 8 * 1.163;
        const double TIME_STEP = 3600;

        #endregion        

        static void Main(string[] args)
        {

            //makeWeatherData();

            //Create an instance of the Outdoor class
            Outdoor outdoor = new Outdoor();
            Sun sun = new Sun(Sun.City.Tokyo);  //Located in Tokyo
            outdoor.Sun = sun;
            outdoor.GroundTemperature = 20;     //Ground temperature is assumed to be constant

            //Create an instance of the Incline class
            Dictionary<string, Incline> inclines = new Dictionary<string, Incline>();
            inclines.Add("N", new Incline(Incline.Orientation.N, 0.5 * Math.PI)); //North, Vertical
            inclines.Add("E", new Incline(Incline.Orientation.E, 0.5 * Math.PI)); //East, Vertical
            inclines.Add("W", new Incline(Incline.Orientation.W, 0.5 * Math.PI)); //West, Vertical
            inclines.Add("S", new Incline(Incline.Orientation.S, 0.5 * Math.PI)); //South, Vertical
            inclines.Add("H", new Incline(Incline.Orientation.S, 0)); //Horizontal

            //壁層を作成
            Dictionary<string, WallLayers> wallLayers;
            makeWallLayers(out wallLayers);

            //室を作成
            Dictionary<string, Zone> zones;
            makeZones(out zones);

            //窓を作成
            Dictionary<string, Window> windows;
            Dictionary<string, Wall> frames;
            makeWindows(zones, inclines, outdoor, wallLayers, out windows, out frames);

            //建具を作成
            Dictionary<string, Wall> doors;
            makeDoors(zones, wallLayers, inclines, outdoor, out doors);

            //外壁を作成
            Dictionary<string, Wall> exWalls;
            makeExWalls(zones, wallLayers, inclines, windows, frames, doors, outdoor, out exWalls);

            //内壁を作成
            Dictionary<string, Wall> inWalls;
            makeInWalls(zones, wallLayers, doors, out inWalls);

            //床を作成
            Dictionary<string, Wall> floors;
            makeFloors(zones, wallLayers, out floors);

            //Creat an insances of the Room class and MultiRoom class
            List<Room> rooms = new List<Room>();
            foreach (string key in zones.Keys) rooms.Add(new Room(new Zone[] { zones[key] }));
            MultiRoom mRoom = new MultiRoom(rooms.ToArray());
            mRoom.SetTimeStep(TIME_STEP);

            //換気経路を設定
            setAirFlow(mRoom, zones);

            //空調制御OFF
            foreach (string key in zones.Keys) zones[key].ControlDrybulbTemperature = false;

            //Output title wrine to standard output stream
            using (StreamWriter sWriter = new StreamWriter("out.csv", false, Encoding.GetEncoding("Shift_JIS")))
            {
                sWriter.Write("日時,");
                foreach (string key in zones.Keys) sWriter.Write(zones[key].Name + "室温[C], " + zones[key].Name + "顕熱負荷[W], ");
                sWriter.WriteLine();

                DateTime dTime = new DateTime(1999, 11, 1, 0, 0, 0);

                List<string[]> wData = new List<string[]>();
                using (StreamReader sReader = new StreamReader("weather.csv"))
                {
                    //タイトル行
                    sReader.ReadLine();
                    string strBuff;
                    while ((strBuff = sReader.ReadLine()) != null) wData.Add(strBuff.Split(','));
                }

                //反復計算
                int iterNum = 0;
                bool iter = true;
                bool output = false;
                double lastTemp = 9999;
                while (iter)
                {
                    iterNum++;

                    if (output) iter = false;
                    for (int j = 0; j < wData.Count; j++)
                    {
                        string[] strData = wData[j];

                        dTime = DateTime.Parse(strData[0]);
                        sun.Update(dTime);
                        mRoom.SetCurrentDateTime(dTime);

                        //Set weather state.
                        outdoor.AirState = new MoistAir(double.Parse(strData[1]), double.Parse(strData[2]));
                        outdoor.NocturnalRadiation = double.Parse(strData[5]);
                        outdoor.GroundTemperature = double.Parse(strData[1]);
                        sun.SetGlobalHorizontalRadiation(double.Parse(strData[3]), double.Parse(strData[4]));

                        //Set ventilation air state.
                        foreach (string key in zones.Keys) zones[key].VentilationAirState = outdoor.AirState;

                        //空調制御
                        setHVACControl(dTime, zones);

                        //Update boundary state of outdoor facing surfaces.
                        outdoor.SetWallSurfaceBoundaryState();

                        //Update the walls.
                        foreach (string key in frames.Keys) frames[key].Update();
                        foreach (string key in doors.Keys) doors[key].Update();
                        foreach (string key in exWalls.Keys) exWalls[key].Update();
                        foreach (string key in inWalls.Keys) inWalls[key].Update();
                        foreach (string key in floors.Keys) floors[key].Update();

                        //Update the MultiRoom object.
                        mRoom.UpdateRoomTemperatures();
                        mRoom.UpdateRoomHumidities();

                        if (output)
                        {
                            sWriter.Write(dTime.ToString() + ",");
                            foreach (string key in zones.Keys) sWriter.Write(zones[key].CurrentDrybulbTemperature + ", " + zones[key].CurrentSensibleHeatLoad + ", ");
                            sWriter.WriteLine();
                        }
                    }
                    double err = Math.Abs(zones["床下"].CurrentDrybulbTemperature - lastTemp);
                    if (err < 0.01) output = true;
                    lastTemp = zones["床下"].CurrentDrybulbTemperature;
                    Console.WriteLine(iterNum + ":" + err);
                }
            }

        }

        #region 室作成処理

        private static void makeZones(out Dictionary<string, Zone> zones)
        {
            zones = new Dictionary<string, Zone>();

            Zone zn;

            zn = new Zone("屋根裏");
            zn.Volume = 1.82 * 7.28 / 2 * 8.645;
            zones.Add(zn.Name, zn);

            zn = new Zone("床下");
            zn.Volume = 8.645 * 7.28 * 0.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("階段室");
            zn.Volume = 1.7 * 2.7 + 3.3 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("1F居間");
            zn.Volume = 20.5 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("1F台所");
            zn.Volume = 8.7 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("1F和室");
            zn.Volume = 11.6 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("1F押入");
            zn.Volume = 1.7 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("1F洗面所");
            zn.Volume = 5.0 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("1F浴室");
            zn.Volume = 3.3 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("1FWC");
            zn.Volume = 1.7 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("1F廊下");
            zn.Volume = 8.7 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("2F主寝室");
            zn.Volume = 17.4 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("2F押入1");
            zn.Volume = 3.1 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("2F子供室1");
            zn.Volume = 10.7 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("2F押入2");
            zn.Volume = 0.8 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("2F子供室2");
            zn.Volume = 9.9 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("2F押入3");
            zn.Volume = 1.7 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("2F予備室");
            zn.Volume = 10.1 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("2FWC");
            zn.Volume = 1.7 * 2.4;
            zones.Add(zn.Name, zn);

            zn = new Zone("2F廊下");
            zn.Volume = 4.2 * 2.4;
            zones.Add(zn.Name, zn);

            foreach (string key in zones.Keys)
            {
                zn = zones[key];
                zn.TimeStep = TIME_STEP;
                //熱容量
                if (zn.Name != "床下" && zn.Name != "屋根裏")
                {
                    zn.SensibleHeatCapacity = 4186 * 4.5 * zn.Volume;
                    //zn.SensibleHeatCapacity = 30000 * zn.Volume;
                }
            }
        }

        #endregion

        #region 壁層作成処理

        static void makeWallLayers(out Dictionary<string, WallLayers> wallLayers)
        {
            bool iDoorEqualToiWall = false;
            bool noIDoor = false;

            WallLayers wl;
            wallLayers = new Dictionary<string, WallLayers>();

            //外部ドア
            wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.012));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.GlassWoolInsulation_24K), 0.05));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.012));
            wallLayers.Add("外部ドア", wl);

            //室内ドア
            wl = new WallLayers();
            if (iDoorEqualToiWall)
            {
                wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Carpet), 0.012));
                wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.012));
                wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.AirGap), 0.02));
                wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.PlasterBoard), 0.012));
            }
            else
            {
                wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.004));
                wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.SealedAirGap), 0.02));
                wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.004));
            }
            if (noIDoor) wl.AddLayer(new WallLayers.Layer(new WallMaterial("完全断熱材", 0.00000001, 0.0001), 10)); //断熱性能完全にしてドアなしを模擬
            wallLayers.Add("室内ドア", wl);

            //屋根1
            wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.AsbestosPlate), 0.012));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.012));
            wallLayers.Add("屋根1", wl);

            //屋根2
            wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.GlassWoolInsulation_24K), 0.05));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.PlasterBoard), 0.012));
            wallLayers.Add("屋根2", wl);

            //外壁
            wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Mortar), 0.030));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.009));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.AirGap), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.GlassWoolInsulation_24K), 0.05));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.PlasterBoard), 0.012));
            wallLayers.Add("外壁", wl);

            //内壁
            wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.PlasterBoard), 0.012));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.AirGap), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.PlasterBoard), 0.012));
            wallLayers.Add("内壁", wl);

            //2F床
            wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Carpet), 0.012));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.012));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.AirGap), 0.02));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.PlasterBoard), 0.012));
            wallLayers.Add("2F床", wl);

            //1F床
            wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.010));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.012));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.GlassWoolInsulation_24K), 0.05));
            wallLayers.Add("1F床", wl);

            //1F和室床
            wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Tatami), 0.060));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Plywood), 0.012));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.GlassWoolInsulation_24K), 0.05));
            wallLayers.Add("1F和室床", wl);

            //サッシ//熱貫流率5.6W/m2K程度
            wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Aluminum), 0.005));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial("調整層", 36, 0), 0.005));
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Aluminum), 0.005));
            wallLayers.Add("サッシ", wl);

            //地面
            wl = new WallLayers();
            wl.AddLayer(new WallLayers.Layer(new WallMaterial(WallMaterial.PredefinedMaterials.Soil), 1, 4));
            wallLayers.Add("地面", wl);
        }

        #endregion

        #region 窓作成処理

        private static void makeWindows(
            Dictionary<string, Zone> zones,Dictionary<string, Incline> inclines, Outdoor outdoor, Dictionary<string, WallLayers> wallLayers,
            out Dictionary<string, Window> windows, out Dictionary<string, Wall> frames)
        {
            bool makeWindowFrame = true;

            const double WIN1720 = 2.89;//1.7 * 2.0;   //
            const double WIN1712 = 1.64;//1.7 * 1.2;   // 
            const double WIN0512 = 0.42;//0.5 * 1.2;   // 
            const double WIN1745 = 0.45;//1.7 * 0.45;  // 

            //外側Low-e 6mm, 内側フロート 6mm
            GlassPanes.Pane[] panes = new GlassPanes.Pane[2];
            panes[0] = new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.TransparentGlass06mm);
            panes[1] = new GlassPanes.Pane(GlassPanes.Pane.PredifinedGlassPane.HeatReflectingGlass06mm);
            GlassPanes gPanes = new GlassPanes(panes);
            gPanes.SetHeatTransferCoefficientsOfGaps(0, 5.9);

            windows = new Dictionary<string, Window>();
            Window win;
            frames = new Dictionary<string, Wall>();
            Wall frm;

            win = new Window(gPanes, "WI1-1");
            win.SurfaceArea = WIN1720;
            win.OutSideIncline = inclines["S"];
            zones["1F居間"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS1-1");
            frm.SurfaceArea = Math.Max(1.7 * 2.0 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["S"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["1F居間"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI1-2");
            win.SurfaceArea = WIN1720;
            win.OutSideIncline = inclines["S"];
            zones["1F居間"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS1-2");
            frm.SurfaceArea = Math.Max(1.7 * 2.0 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["S"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["1F居間"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI1-3");
            win.SurfaceArea = WIN1720;
            win.OutSideIncline = inclines["S"];
            zones["1F和室"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS1-3");
            frm.SurfaceArea = Math.Max(1.7 * 2.0 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["S"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["1F和室"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI1-4");
            win.SurfaceArea = WIN0512;
            win.OutSideIncline = inclines["E"];
            zones["1F浴室"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS1-4");
            frm.SurfaceArea = Math.Max(0.5 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["E"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["1F浴室"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI1-5");
            win.SurfaceArea = WIN0512;
            win.OutSideIncline = inclines["N"];
            zones["1F洗面所"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS1-5");
            frm.SurfaceArea = Math.Max(0.5 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["1F洗面所"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI1-6");
            win.SurfaceArea = WIN0512;
            win.OutSideIncline = inclines["N"];
            zones["1FWC"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS1-6");
            frm.SurfaceArea = Math.Max(0.5 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["1FWC"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI1-7");
            win.SurfaceArea = WIN1745;
            win.OutSideIncline = inclines["W"];
            zones["1F台所"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS1-7");
            frm.SurfaceArea = Math.Max(1.7 * 0.45 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["W"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["1F台所"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI1-8");
            win.SurfaceArea = WIN0512;
            win.OutSideIncline = inclines["W"];
            zones["1F居間"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS1-8");
            frm.SurfaceArea = Math.Max(0.5 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["W"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["1F居間"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI1-9");
            win.SurfaceArea = WIN0512;
            win.OutSideIncline = inclines["W"];
            zones["1F居間"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS1-9");
            frm.SurfaceArea = Math.Max(0.5 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["W"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["1F居間"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI2-1");
            win.SurfaceArea = WIN1712;
            win.OutSideIncline = inclines["S"];
            zones["2F主寝室"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS2-1");
            frm.SurfaceArea = Math.Max(1.7 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["S"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["2F主寝室"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI2-2");
            win.SurfaceArea = WIN1712;
            win.OutSideIncline = inclines["S"];
            zones["2F主寝室"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS2-2");
            frm.SurfaceArea = Math.Max(1.7 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["S"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["2F主寝室"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI2-3");
            win.SurfaceArea = WIN1712;
            win.OutSideIncline = inclines["S"];
            zones["2F子供室1"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS2-3");
            frm.SurfaceArea = Math.Max(1.7 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["S"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["2F子供室1"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI2-4");
            win.SurfaceArea = WIN0512;
            win.OutSideIncline = inclines["E"];
            zones["2F子供室1"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS2-4");
            frm.SurfaceArea = Math.Max(0.5 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["E"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["2F子供室1"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI2-5");
            win.SurfaceArea = WIN0512;
            win.OutSideIncline = inclines["E"];
            zones["2F子供室2"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS2-5");
            frm.SurfaceArea = Math.Max(0.5 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["E"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["2F子供室2"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI2-6");
            win.SurfaceArea = WIN1712;
            win.OutSideIncline = inclines["N"];
            zones["2F子供室2"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS2-6");
            frm.SurfaceArea = Math.Max(1.7 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["2F子供室2"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI2-7");
            win.SurfaceArea = WIN0512;
            win.OutSideIncline = inclines["N"];
            zones["階段室"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS2-7");
            frm.SurfaceArea = Math.Max(0.5 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["階段室"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI2-8");
            win.SurfaceArea = WIN0512;
            win.OutSideIncline = inclines["N"];
            zones["2FWC"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS2-8");
            frm.SurfaceArea = Math.Max(0.5 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["2FWC"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI2-9");
            win.SurfaceArea = WIN1712;
            win.OutSideIncline = inclines["N"];
            zones["2F予備室"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS2-9");
            frm.SurfaceArea = Math.Max(1.7 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["2F予備室"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            win = new Window(gPanes, "WI2-10");
            win.SurfaceArea = WIN0512;
            win.OutSideIncline = inclines["E"];
            zones["2F主寝室"].AddWindow(win);
            outdoor.AddWindow(win);
            windows.Add(win.Name, win);

            frm = new Wall(wallLayers["サッシ"], "SS2-10");
            frm.SurfaceArea = Math.Max(0.5 * 1.2 - win.SurfaceArea, 0.0001);
            frm.SetIncline(inclines["E"], true);
            outdoor.AddWallSurface(frm.GetSurface(true));
            zones["2F主寝室"].AddSurface(frm.GetSurface(false));
            frames.Add(frm.Name, frm);

            //総合熱伝達率設定
            foreach (string key in windows.Keys)
            {
                windows[key].GetSurface(true).FilmCoefficient = AO;
                windows[key].GetSurface(false).FilmCoefficient = AI;
            }
            //総合熱伝達率設定
            foreach (string key in frames.Keys)
            {
                frames[key].GetSurface(true).FilmCoefficient = AO;
                frames[key].GetSurface(false).FilmCoefficient = AI;

                if (! makeWindowFrame) frames[key].SurfaceArea = 0.000001;
            }
        }

        #endregion

        #region 建具作成処理

        private static void makeDoors(
            Dictionary<string, Zone> zones, Dictionary<string, WallLayers> wallLayers, 
            Dictionary<string, Incline> inclines, Outdoor outdoor, out Dictionary<string, Wall> doors)
        {
            doors = new Dictionary<string, Wall>();
            Wall door;

            door = new Wall(wallLayers["外部ドア"], "DR1-1");
            door.SurfaceArea = 1.0 * 2.0;
            door.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(door.GetSurface(true));
            zones["1F廊下"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["外部ドア"], "DR1-2");
            door.SurfaceArea = 0.8 * 2.0;
            door.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(door.GetSurface(true));
            zones["1F台所"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR1-3");
            door.SurfaceArea = 0.8 * 2.0;
            zones["1F浴室"].AddSurface(door.GetSurface(true));
            zones["1F洗面所"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR1-4");
            door.SurfaceArea = 0.8 * 2.0;
            zones["1F廊下"].AddSurface(door.GetSurface(true));
            zones["1F洗面所"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR1-5");
            door.SurfaceArea = 0.8 * 2.0;
            zones["1F廊下"].AddSurface(door.GetSurface(true));
            zones["1FWC"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR1-6");
            door.SurfaceArea = 0.8 * 2.0;
            zones["1F居間"].AddSurface(door.GetSurface(true));
            zones["1F廊下"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR1-7");
            door.SurfaceArea = 0.8 * 2.0;
            zones["1F和室"].AddSurface(door.GetSurface(true));
            zones["1F廊下"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR1-8");
            door.SurfaceArea = 0.8 * 2.0;
            zones["1F台所"].AddSurface(door.GetSurface(true));
            zones["1F廊下"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR1-9");
            door.SurfaceArea = 1.7 * 2.0;
            zones["1F和室"].AddSurface(door.GetSurface(true));
            zones["1F押入"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR2-1");
            door.SurfaceArea = 1.7 * 2.0 * 2;
            zones["2F主寝室"].AddSurface(door.GetSurface(true));
            zones["2F押入1"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR2-2");
            door.SurfaceArea = 0.8 * 2.0;
            zones["2F主寝室"].AddSurface(door.GetSurface(true));
            zones["2F廊下"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR2-3");
            door.SurfaceArea = 0.8 * 2.0;
            zones["2F子供室1"].AddSurface(door.GetSurface(true));
            zones["2F押入2"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR2-4");
            door.SurfaceArea = 1.7 * 2.0;
            zones["2F子供室2"].AddSurface(door.GetSurface(true));
            zones["2F押入3"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR2-5");
            door.SurfaceArea = 0.8 * 2.0;
            zones["2F子供室2"].AddSurface(door.GetSurface(true));
            zones["2F廊下"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR2-6");
            door.SurfaceArea = 0.8 * 2.0;
            zones["2F予備室"].AddSurface(door.GetSurface(true));
            zones["2F廊下"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR2-7");
            door.SurfaceArea = 0.8 * 2.0;
            zones["2FWC"].AddSurface(door.GetSurface(true));
            zones["2F廊下"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            door = new Wall(wallLayers["室内ドア"], "DR2-8");
            door.SurfaceArea = 0.8 * 2.0;
            zones["2F子供室1"].AddSurface(door.GetSurface(true));
            zones["2F廊下"].AddSurface(door.GetSurface(false));
            doors.Add(door.Name, door);

            //総合熱伝達率設定
            foreach (string key in doors.Keys)
            {
                doors[key].GetSurface(true).FilmCoefficient = AI;
                doors[key].GetSurface(false).FilmCoefficient = AI;
                doors[key].TimeStep = TIME_STEP;
            }
            //外部ドア
            doors["DR1-1"].GetSurface(true).FilmCoefficient = AO;
            doors["DR1-2"].GetSurface(true).FilmCoefficient = AO;
        }

        #endregion

        #region 外壁作成処理

        private static void makeExWalls(
             Dictionary<string, Zone> zones, Dictionary<string, WallLayers> wallLayers,
            Dictionary<string, Incline> inclines, Dictionary<string, Window> windows, Dictionary<string, Wall> frames
            , Dictionary<string, Wall> doors, Outdoor outdoor, out Dictionary<string, Wall> exWalls)
        {
            exWalls = new Dictionary<string, Wall>();
            Wall exWall;

            exWall = new Wall(wallLayers["外壁"], "EW1-1");
            exWall.SurfaceArea = 5.005 * 2.7 - windows["WI1-1"].SurfaceArea - windows["WI1-2"].SurfaceArea
                 - frames["SS1-1"].SurfaceArea - frames["SS1-2"].SurfaceArea;
            exWall.SetIncline(inclines["S"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["1F居間"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW1-2");
            exWall.SurfaceArea = 3.64 * 2.7 - windows["WI1-3"].SurfaceArea - frames["SS1-3"].SurfaceArea;
            exWall.SetIncline(inclines["S"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["1F和室"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW1-3");
            exWall.SurfaceArea = 1.82 * 2.7;
            exWall.SetIncline(inclines["E"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["1F和室"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW1-4");
            exWall.SurfaceArea = 1.82 * 2.7;
            exWall.SetIncline(inclines["E"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["1F押入"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW1-5");
            exWall.SurfaceArea = 1.82 * 2.7 - windows["WI1-4"].SurfaceArea - frames["SS1-4"].SurfaceArea;
            exWall.SetIncline(inclines["E"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["1F浴室"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW1-6");
            exWall.SurfaceArea = 1.82 * 2.7;
            exWall.SetIncline(inclines["E"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["1F洗面所"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW1-7");
            exWall.SurfaceArea = 2.73 * 2.7 - windows["WI1-5"].SurfaceArea - frames["SS1-5"].SurfaceArea;
            exWall.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["1F洗面所"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW1-8");
            exWall.SurfaceArea = 0.91 * 2.7 - windows["WI1-6"].SurfaceArea - frames["SS1-6"].SurfaceArea;
            exWall.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["1FWC"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW1-9");
            exWall.SurfaceArea = 0.91 * 2.7;
            exWall.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["階段室"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW1-10");
            exWall.SurfaceArea = 1.82 * 2.7 - doors["DR1-1"].SurfaceArea;
            exWall.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["1F廊下"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW1-11");
            exWall.SurfaceArea = 2.275 * 2.7 - doors["DR1-2"].SurfaceArea;
            exWall.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["1F台所"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW1-12");
            exWall.SurfaceArea = 3.185 * 2.7 - windows["WI1-7"].SurfaceArea - frames["SS1-7"].SurfaceArea;
            exWall.SetIncline(inclines["W"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["1F台所"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW1-13");
            exWall.SurfaceArea = 4.095 * 2.7 - windows["WI1-8"].SurfaceArea - windows["WI1-9"].SurfaceArea
                 - frames["SS1-8"].SurfaceArea - frames["SS1-9"].SurfaceArea;
            exWall.SetIncline(inclines["W"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["1F居間"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW2-1");
            exWall.SurfaceArea = 5.005 * 2.7 - windows["WI2-1"].SurfaceArea - windows["WI2-2"].SurfaceArea
                 - frames["SS2-1"].SurfaceArea - frames["SS2-2"].SurfaceArea;
            exWall.SetIncline(inclines["S"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["2F主寝室"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW2-2");
            exWall.SurfaceArea = 3.64 * 2.7 - windows["WI2-3"].SurfaceArea - frames["SS2-3"].SurfaceArea;
            exWall.SetIncline(inclines["S"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["2F子供室1"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW2-3");
            exWall.SurfaceArea = 2.73 * 2.7 - windows["WI2-4"].SurfaceArea - frames["SS2-4"].SurfaceArea;
            exWall.SetIncline(inclines["E"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["2F子供室1"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW2-4");
            exWall.SurfaceArea = 0.91 * 2.7;
            exWall.SetIncline(inclines["E"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["2F押入3"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW2-5");
            exWall.SurfaceArea = 3.64 * 2.7 - windows["WI2-5"].SurfaceArea - frames["SS2-5"].SurfaceArea;
            exWall.SetIncline(inclines["E"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["2F子供室2"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW2-6");
            exWall.SurfaceArea = 2.73 * 2.7 - windows["WI2-6"].SurfaceArea - frames["SS2-6"].SurfaceArea;
            exWall.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["2F子供室2"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW2-7");
            exWall.SurfaceArea = 1.82 * 2.7 - windows["WI2-7"].SurfaceArea - frames["SS2-7"].SurfaceArea;
            exWall.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["階段室"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW2-8");
            exWall.SurfaceArea = 0.91 * 2.7 - windows["WI2-8"].SurfaceArea - frames["SS2-8"].SurfaceArea;
            exWall.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["2FWC"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW2-9");
            exWall.SurfaceArea = 3.185 * 2.7 - windows["WI2-9"].SurfaceArea - frames["SS2-9"].SurfaceArea;
            exWall.SetIncline(inclines["N"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["2F予備室"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW2-10");
            exWall.SurfaceArea = 3.185 * 2.7;
            exWall.SetIncline(inclines["W"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["2F予備室"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW2-11");
            exWall.SurfaceArea = 0.91 * 2.7;
            exWall.SetIncline(inclines["W"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["2F押入1"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["外壁"], "EW2-12");
            exWall.SurfaceArea = 3.185 * 2.7;
            exWall.SetIncline(inclines["W"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["2F主寝室"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            exWall = new Wall(wallLayers["屋根1"], "EW-RF");
            exWall.SurfaceArea = 8.645 * 7.280;
            exWall.SetIncline(inclines["H"], true);
            outdoor.AddWallSurface(exWall.GetSurface(true));
            zones["屋根裏"].AddSurface(exWall.GetSurface(false));
            exWalls.Add(exWall.Name, exWall);

            //総合熱伝達率設定
            foreach (string key in exWalls.Keys)
            {
                exWalls[key].GetSurface(true).FilmCoefficient = AO;
                exWalls[key].GetSurface(false).FilmCoefficient = AI;
                exWalls[key].TimeStep = TIME_STEP;
            }

            exWall = new Wall(wallLayers["地面"], "EW-SOIL");
            exWall.SurfaceArea = 8.645 * 7.280;
            outdoor.AddGroundWallSurface(exWall.GetSurface(true));
            zones["床下"].AddSurface(exWall.GetSurface(false));
            exWall.TimeStep = TIME_STEP;
            exWalls.Add(exWall.Name, exWall);
        }

        #endregion

        #region 内壁作成処理

        private static void makeInWalls(
             Dictionary<string, Zone> zones, Dictionary<string, WallLayers> wallLayers, Dictionary<string, Wall> doors,
            out Dictionary<string, Wall> inWalls)
        {

            inWalls = new Dictionary<string, Wall>();
            Wall inWall;

            inWall = new Wall(wallLayers["内壁"], "IW1-1");
            inWall.SurfaceArea = 3.64 * 2.7;
            zones["1F居間"].AddSurface(inWall.GetSurface(true));
            zones["1F和室"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW1-2");
            inWall.SurfaceArea = 0.91 * 2.7;
            zones["1F和室"].AddSurface(inWall.GetSurface(true));
            zones["1F押入"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW1-3");
            inWall.SurfaceArea = 1.82 * 2.7;
            zones["1F和室"].AddSurface(inWall.GetSurface(true));
            zones["1F廊下"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW1-4");
            inWall.SurfaceArea = 0.91 * 2.7;
            zones["1F和室"].AddSurface(inWall.GetSurface(true));
            zones["1F浴室"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW1-5");
            inWall.SurfaceArea = 0.91 * 2.7;
            zones["1F押入"].AddSurface(inWall.GetSurface(true));
            zones["1F浴室"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW1-6");
            inWall.SurfaceArea = 1.82 * 2.7;
            zones["1F廊下"].AddSurface(inWall.GetSurface(true));
            zones["1F浴室"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW1-7");
            inWall.SurfaceArea = 1.82 * 2.7 - doors["DR1-3"].SurfaceArea;
            zones["1F洗面所"].AddSurface(inWall.GetSurface(true));
            zones["1F浴室"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW1-8");
            inWall.SurfaceArea = 0.91 * 2.7 - doors["DR1-4"].SurfaceArea;
            zones["1F洗面所"].AddSurface(inWall.GetSurface(true));
            zones["1F廊下"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW1-9");
            inWall.SurfaceArea = 1.82 * 2.7;
            zones["1F洗面所"].AddSurface(inWall.GetSurface(true));
            zones["1FWC"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW1-10");
            inWall.SurfaceArea = 1.82 * 2.7;
            zones["階段室"].AddSurface(inWall.GetSurface(true));
            zones["1F浴室"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW1-11");
            inWall.SurfaceArea = 3.185 * 2.7 - doors["DR1-8"].SurfaceArea;
            zones["1F台所"].AddSurface(inWall.GetSurface(true));
            zones["1F廊下"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW1-12");
            inWall.SurfaceArea = 2.73 * 2.7 - doors["DR1-6"].SurfaceArea;
            zones["1F居間"].AddSurface(inWall.GetSurface(true));
            zones["1F廊下"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW1-13");
            inWall.SurfaceArea = 0.91 * 2.7 - doors["DR1-5"].SurfaceArea;
            zones["1FWC"].AddSurface(inWall.GetSurface(true));
            zones["1F廊下"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-1");
            inWall.SurfaceArea = 3.64 * 2.7;
            zones["2F主寝室"].AddSurface(inWall.GetSurface(true));
            zones["2F子供室1"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-2");
            inWall.SurfaceArea = 3.185 * 2.7;
            zones["2F主寝室"].AddSurface(inWall.GetSurface(true));
            zones["2F予備室"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-3");
            inWall.SurfaceArea = 1.82 * 2.7;
            zones["2F子供室1"].AddSurface(inWall.GetSurface(true));
            zones["2F押入3"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-4");
            inWall.SurfaceArea = 0.91 * 2.7;
            zones["2F押入2"].AddSurface(inWall.GetSurface(true));
            zones["2F押入3"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-5");
            inWall.SurfaceArea = 0.91 * 2.7;
            zones["2F押入2"].AddSurface(inWall.GetSurface(true));
            zones["2F子供室2"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-6");
            inWall.SurfaceArea = 1.82 * 2.7;
            zones["階段室"].AddSurface(inWall.GetSurface(true));
            zones["2F子供室2"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-7");
            inWall.SurfaceArea = 1.82 * 2.7;
            zones["階段室"].AddSurface(inWall.GetSurface(true));
            zones["2FWC"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-8");
            inWall.SurfaceArea = 1.82 * 2.7;
            zones["2F予備室"].AddSurface(inWall.GetSurface(true));
            zones["2FWC"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-9");
            inWall.SurfaceArea = 0.91 * 2.7;
            zones["階段室"].AddSurface(inWall.GetSurface(true));
            zones["2F廊下"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-10");
            inWall.SurfaceArea = 1.82 * 2.7 - doors["DR2-2"].SurfaceArea;
            zones["2F主寝室"].AddSurface(inWall.GetSurface(true));
            zones["2F廊下"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-11");
            inWall.SurfaceArea = 0.91 * 2.7 - doors["DR2-8"].SurfaceArea;
            zones["2F子供室1"].AddSurface(inWall.GetSurface(true));
            zones["2F廊下"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-12");
            inWall.SurfaceArea = 0.91 * 2.7;
            zones["2F主寝室"].AddSurface(inWall.GetSurface(true));
            zones["2F押入1"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-13");
            inWall.SurfaceArea = 0.91 * 2.7 - doors["DR2-7"].SurfaceArea;
            zones["2FWC"].AddSurface(inWall.GetSurface(true));
            zones["2F廊下"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-14");
            inWall.SurfaceArea = 1.82 * 2.7 - doors["DR2-5"].SurfaceArea;
            zones["2F子供室2"].AddSurface(inWall.GetSurface(true));
            zones["2F廊下"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            inWall = new Wall(wallLayers["内壁"], "IW2-15");
            inWall.SurfaceArea = 0.91 * 2.7;
            zones["2F子供室1"].AddSurface(inWall.GetSurface(true));
            zones["2F押入2"].AddSurface(inWall.GetSurface(false));
            inWalls.Add(inWall.Name, inWall);

            //総合熱伝達率設定
            foreach (string key in inWalls.Keys)
            {
                inWalls[key].GetSurface(true).FilmCoefficient = AI;
                inWalls[key].GetSurface(false).FilmCoefficient = AI;
                inWalls[key].TimeStep = TIME_STEP;
            }
        }

        #endregion

        #region 床作成処理

        private static void makeFloors(Dictionary<string, Zone> zones,
            Dictionary<string, WallLayers> wallLayers, out Dictionary<string, Wall> floors)
        {
            floors = new Dictionary<string, Wall>();
            Wall floor;

            floor = new Wall(wallLayers["屋根2"], "CE-1");
            floor.SurfaceArea = 17.4;
            zones["屋根裏"].AddSurface(floor.GetSurface(true));
            zones["2F主寝室"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["屋根2"], "CE-2");
            floor.SurfaceArea = 10.7;
            zones["屋根裏"].AddSurface(floor.GetSurface(true));
            zones["2F子供室1"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["屋根2"], "CE-3");
            floor.SurfaceArea = 10.1;
            zones["屋根裏"].AddSurface(floor.GetSurface(true));
            zones["2F予備室"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["屋根2"], "CE-4");
            floor.SurfaceArea = 9.9;
            zones["屋根裏"].AddSurface(floor.GetSurface(true));
            zones["2F子供室2"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["屋根2"], "CE-5");
            floor.SurfaceArea = 4.2;
            zones["屋根裏"].AddSurface(floor.GetSurface(true));
            zones["2F廊下"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["屋根2"], "CE-6");
            floor.SurfaceArea = 3.1;
            zones["屋根裏"].AddSurface(floor.GetSurface(true));
            zones["2F押入1"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["屋根2"], "CE-7");
            floor.SurfaceArea = 0.8;
            zones["屋根裏"].AddSurface(floor.GetSurface(true));
            zones["2F押入2"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["屋根2"], "CE-8");
            floor.SurfaceArea = 1.7;
            zones["屋根裏"].AddSurface(floor.GetSurface(true));
            zones["2F押入3"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["屋根2"], "CE-9");
            floor.SurfaceArea = 1.7;
            zones["屋根裏"].AddSurface(floor.GetSurface(true));
            zones["2FWC"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["屋根2"], "CE-10");
            floor.SurfaceArea = 3.3;
            zones["屋根裏"].AddSurface(floor.GetSurface(true));
            zones["階段室"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-1");
            floor.SurfaceArea = 17.4;
            zones["2F主寝室"].AddSurface(floor.GetSurface(true));
            zones["1F居間"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-2");
            floor.SurfaceArea = 17.4;
            zones["2F押入1"].AddSurface(floor.GetSurface(true));
            zones["1F居間"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-3");
            floor.SurfaceArea = 10.7 - 0.91 * 0.91;
            zones["1F和室"].AddSurface(floor.GetSurface(true));
            zones["2F子供室1"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-4");
            floor.SurfaceArea = 0.91 * 0.91;
            zones["2F子供室1"].AddSurface(floor.GetSurface(true));
            zones["1F押入"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-5");
            floor.SurfaceArea = 0.8;
            zones["2F押入2"].AddSurface(floor.GetSurface(true));
            zones["1F和室"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-6");
            floor.SurfaceArea = 0.8;
            zones["2F押入3"].AddSurface(floor.GetSurface(true));
            zones["1F和室"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-7");
            floor.SurfaceArea = 0.8;
            zones["2F押入3"].AddSurface(floor.GetSurface(true));
            zones["1F押入"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-8");
            floor.SurfaceArea = 3.185 * 2.275;
            zones["2F予備室"].AddSurface(floor.GetSurface(true));
            zones["1F台所"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-9");
            floor.SurfaceArea = 3.185 * 0.91;
            zones["2F予備室"].AddSurface(floor.GetSurface(true));
            zones["1F廊下"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-10");
            floor.SurfaceArea = 4.2;
            zones["2F廊下"].AddSurface(floor.GetSurface(true));
            zones["1F廊下"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-11");
            floor.SurfaceArea = 1.7;
            zones["2FWC"].AddSurface(floor.GetSurface(true));
            zones["1F廊下"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-12");
            floor.SurfaceArea = 1.82 * 0.91;
            zones["2F子供室2"].AddSurface(floor.GetSurface(true));
            zones["1F廊下"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-13");
            floor.SurfaceArea = 2.73 * 1.82;
            zones["2F子供室2"].AddSurface(floor.GetSurface(true));
            zones["1F洗面所"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-14");
            floor.SurfaceArea = 1.82 * 1.82;
            zones["2F子供室2"].AddSurface(floor.GetSurface(true));
            zones["1F浴室"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["2F床"], "FL2-15");
            floor.SurfaceArea = 0.91 * 1.82;
            zones["階段室"].AddSurface(floor.GetSurface(true));
            zones["1FWC"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["1F床"], "FL1-1");
            floor.SurfaceArea = 20.5;
            zones["1F居間"].AddSurface(floor.GetSurface(true));
            zones["床下"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["1F床"], "FL1-2");
            floor.SurfaceArea = 11.6;
            zones["1F和室"].AddSurface(floor.GetSurface(true));
            zones["床下"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["1F床"], "FL1-3");
            floor.SurfaceArea = 7.2;
            zones["1F台所"].AddSurface(floor.GetSurface(true));
            zones["床下"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["1F床"], "FL1-4");
            floor.SurfaceArea = 5.0;
            zones["1F洗面所"].AddSurface(floor.GetSurface(true));
            zones["床下"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["1F床"], "FL1-5");
            floor.SurfaceArea = 3.3;
            zones["1F浴室"].AddSurface(floor.GetSurface(true));
            zones["床下"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["1F床"], "FL1-6");
            floor.SurfaceArea = 1.7;
            zones["1FWC"].AddSurface(floor.GetSurface(true));
            zones["床下"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["1F床"], "FL1-7");
            floor.SurfaceArea = 1.7;
            zones["1F廊下"].AddSurface(floor.GetSurface(true));
            zones["床下"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            floor = new Wall(wallLayers["1F床"], "FL1-8");
            floor.SurfaceArea = 1.7;
            zones["1F押入"].AddSurface(floor.GetSurface(true));
            zones["床下"].AddSurface(floor.GetSurface(false));
            floors.Add(floor.Name, floor);

            //総合熱伝達率設定
            foreach (string key in floors.Keys)
            {
                floors[key].GetSurface(true).FilmCoefficient = (4.6 + 1.5) / 2 + 4.7;
                floors[key].GetSurface(true).ConvectiveRate = (4.6 + 1.5) / 2 / floors[key].GetSurface(true).FilmCoefficient;
                floors[key].GetSurface(false).FilmCoefficient = (4.6 + 1.5) / 2 + 4.7;
                floors[key].GetSurface(false).ConvectiveRate = (4.6 + 1.5) / 2 / floors[key].GetSurface(false).FilmCoefficient;

                //上下別の設定
                /*floors[key].GetSurface(true).FilmCoefficient = 4.6 + 4.7;
                floors[key].GetSurface(true).ConvectiveRate = 4.6 / (4.6 + 4.7);
                floors[key].GetSurface(false).FilmCoefficient = 1.5 + 4.7;
                floors[key].GetSurface(false).ConvectiveRate = 1.5 / (1.5 + 4.7);*/

                floors[key].TimeStep = TIME_STEP;
            }
        }

        #endregion

        #region 換気経路を設定

        private static void setAirFlow(MultiRoom mRoom, Dictionary<string, Zone> zones)
        {
            bool setAirFlow = true;

            if (setAirFlow)
            {
                //外気
                zones["床下"].VentilationVolume = zones["床下"].Volume * 2.0;
                zones["屋根裏"].VentilationVolume = zones["屋根裏"].Volume * 2.0;
                zones["1F台所"].VentilationVolume = 9;
                zones["1F居間"].VentilationVolume = 25;
                zones["1F和室"].VentilationVolume = 15;
                zones["1F廊下"].VentilationVolume = 9;
                zones["1F洗面所"].VentilationVolume = 10;
                zones["2F主寝室"].VentilationVolume = 23;
                zones["2F子供室1"].VentilationVolume = 13;
                zones["2F子供室2"].VentilationVolume = 13;
                zones["2F予備室"].VentilationVolume = 13;
                zones["2F廊下"].VentilationVolume = 7;

                //1F室間換気
                mRoom.SetAirFlow(zones["1F居間"], zones["1F廊下"], 25);
                mRoom.SetAirFlow(zones["1F台所"], zones["1F廊下"], 9);
                mRoom.SetAirFlow(zones["1F和室"], zones["1F廊下"], 15);
                mRoom.SetAirFlow(zones["1F廊下"], zones["1FWC"], 29);
                mRoom.SetAirFlow(zones["1F廊下"], zones["1F洗面所"], 29);
                mRoom.SetAirFlow(zones["1F洗面所"], zones["1F浴室"], 39);

                //2F室間換気
                mRoom.SetAirFlow(zones["階段室"], zones["2F廊下"], 7);
                mRoom.SetAirFlow(zones["2F主寝室"], zones["2F廊下"], 23);
                mRoom.SetAirFlow(zones["2F子供室1"], zones["2F廊下"], 13);
                mRoom.SetAirFlow(zones["2F子供室2"], zones["2F廊下"], 13);
                mRoom.SetAirFlow(zones["2F予備室"], zones["2F廊下"], 13);
                mRoom.SetAirFlow(zones["2F廊下"], zones["2FWC"], 69);
            }
            else
            {
                foreach (string key in zones.Keys)
                {
                    Zone zn = zones[key];
                    zn.VentilationVolume = zn.Volume * 0.3;
                }
                zones["1F台所"].VentilationVolume = zones["1F台所"].Volume * 0.5;
                zones["1F居間"].VentilationVolume = zones["1F居間"].Volume * 0.5;
                zones["1F和室"].VentilationVolume = zones["1F和室"].Volume * 0.5;
                zones["2F主寝室"].VentilationVolume = zones["2F主寝室"].Volume * 0.5;
                zones["2F子供室1"].VentilationVolume = zones["2F子供室1"].Volume * 0.5;
                zones["2F子供室2"].VentilationVolume = zones["2F子供室2"].Volume * 0.5;
                zones["2F予備室"].VentilationVolume = zones["2F予備室"].Volume * 0.5;
            }
        }

        #endregion

        #region 空調制御

        private static void setHVACControl(DateTime dTime, Dictionary<string, Zone> zones)
        {
            //夏季
            bool controlHour = dTime.Hour <= 21 && 7 <= dTime.Hour;
            //冬季
            //bool controlHour = (dTime.Hour < 9 && 6 <= dTime.Hour) || (dTime.Hour < 22 && 16 <= dTime.Hour);

            bool controlOCZones = true;
            bool controlStorage = false;
            bool controlNonOCZones = false;

            //非居室
            foreach (string key in zones.Keys)
            {
                Zone zn = zones[key];
                zn.ControlDrybulbTemperature = controlNonOCZones && controlHour;
                zn.DrybulbTemperatureSetPoint = 26;
            }

            //押入
            zones["1F押入"].ControlDrybulbTemperature = controlStorage && controlHour;
            zones["2F押入1"].ControlDrybulbTemperature = controlStorage && controlHour;
            zones["2F押入2"].ControlDrybulbTemperature = controlStorage && controlHour;
            zones["2F押入3"].ControlDrybulbTemperature = controlStorage && controlHour;

            //居室
            zones["1F台所"].ControlDrybulbTemperature = controlOCZones && controlHour;
            zones["1F居間"].ControlDrybulbTemperature = controlOCZones && controlHour;
            zones["1F和室"].ControlDrybulbTemperature = controlOCZones && controlHour;
            zones["2F主寝室"].ControlDrybulbTemperature = controlOCZones && controlHour;
            zones["2F子供室1"].ControlDrybulbTemperature = controlOCZones && controlHour;
            zones["2F子供室2"].ControlDrybulbTemperature = controlOCZones && controlHour;
            zones["2F予備室"].ControlDrybulbTemperature = controlOCZones && controlHour;
        }

        #endregion

        #region その他の処理

        private static void makeWeatherData()
        {
            using (StreamWriter sWriter = new StreamWriter("weather2.csv", false, Encoding.GetEncoding("Shift_JIS")))
            {
                sWriter.WriteLine("日時,乾球温度,絶対湿度,法線面直達日射量,天空日射,夜間放射");
                bool suc;
                WeatherDataTable wdt = Popolo.Weather.Converter.HASConverter.ToPWeatherData("TokyoEA.has", out suc);
                DateTime dt = new DateTime(1999, 1, 1, 0, 0, 0);
                for (int i = 0; i < wdt.WeatherRecordNumber; i++)
                {
                    ImmutableWeatherRecord wr = wdt.GetWeatherRecord(i);
                    sWriter.WriteLine(
                        dt.ToString() + "," +
                        wr.GetData(WeatherRecord.RecordType.DryBulbTemperature).Value + "," +
                        wr.GetData(WeatherRecord.RecordType.HumidityRatio).Value + "," +
                        wr.GetData(WeatherRecord.RecordType.DirectNormalRadiation).Value + "," +
                        wr.GetData(WeatherRecord.RecordType.DiffuseHorizontalRadiation).Value + "," +
                        wr.GetData(WeatherRecord.RecordType.NocturnalRadiation).Value);
                    dt = dt.AddHours(1);
                }
            }
        }

        #endregion

    }
}
