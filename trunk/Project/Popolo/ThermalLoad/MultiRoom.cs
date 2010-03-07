/* MultiRoom.cs
 * 
 * Copyright (C) 2009 E.Togashi
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

using GSLNET;
using Popolo.ThermophysicalProperty;

namespace Popolo.ThermalLoad
{
    /// <summary>多数室</summary>
    public class MultiRoom : ImmutableMultiRoom
    {

        #region インスタンス変数

        /// <summary>壁および窓のFIO変更フラグ</summary>
        private bool hasFIOChanged = false;

        /// <summary>計算対象のRoomリストを保持</summary>
        private Room[] rooms;

        /// <summary>ゾーンリストを保持</summary>
        private Zone[] zones;

        /// <summary>表面リスト</summary>
        private ISurface[] surfaces;

        /// <summary>表面温度計算式の逆行列</summary>
        private Matrix xa;

        /// <summary>放射熱伝達比率[-]</summary>
        private Matrix phi;

        /// <summary>ゾーン室温計算用行列</summary>
        private Matrix bMatrix, xbMatrix;

        /// <summary>ゾーン室温計算用ベクトル</summary>
        private Vector bVector, bbVector;

        /// <summary>AX行列におけるRoom要素の開始列数を保持</summary>
        private Dictionary<Room, uint> axRmIndices = new Dictionary<Room, uint>();

        /// <summary>AX行列におけるZone要素の開始列数を保持</summary>
        private Dictionary<Zone, uint> axZnIndices = new Dictionary<Zone, uint>();

        /// <summary>表面とRoomとの対応を保持</summary>
        private Dictionary<ISurface, Room> sfToRm = new Dictionary<ISurface, Room>();

        /// <summary>ZoneとRoomとの対応を保持</summary>
        private Dictionary<Zone, Room> znToRm = new Dictionary<Zone, Room>();

        /// <summary>壁表面の温度[C]を保持するベクトル</summary>
        private Vector surfaceTemperatures;

        /// <summary>他の表面の平均温度[C]を保持するベクトル</summary>
        private Vector surfaceMRTs;

        /// <summary>各ゾーンのRX,RAを保持するベクトル</summary>
        private Dictionary<Zone, Vector> rxVector = new Dictionary<Zone, Vector>();
        private Dictionary<Zone, Vector> raVector = new Dictionary<Zone, Vector>();

        /// <summary>定数項を保持するベクトル</summary>
        private Vector crxVector, craVector;

        /// <summary>ゾーン空気温度を保持するベクトル</summary>
        private Vector tzVector;

        /// <summary>各Roomに含まれる壁表面の数を保持</summary>
        private uint[] sfNumber;

        /// <summary>各Roomに含まれるゾーンの数を保持</summary>
        private uint[] znNumber;

        /// <summary>ARマトリクス</summary>
        private double[,] arMatrix;

        /// <summary>CAベクトル</summary>
        private double[] caVector;

        /// <summary>ARとBの置換行列配列</summary>
        private uint[] arbPerm;

        /// <summary>計算時間間隔[sec]</summary>
        private double timeStep = 60;

        /// <summary>室間換気量[m3/h]を保持（室へ流入する流量）</summary>
        private Dictionary<ImmutableZone, Dictionary<ImmutableZone, double>> airFlowToZone = new Dictionary<ImmutableZone, Dictionary<ImmutableZone, double>>();

        #endregion

        #region プロパティ

        /// <summary>計算時間間隔[sec]を取得する</summary>
        public double TimeStep
        {
            get
            {
                return timeStep;
            }
        }

        /// <summary>室リストを取得する</summary>
        public ImmutableRoom[] Rooms
        {
            get
            {
                return rooms;
            }
        }

        /// <summary>現在の日時を取得する</summary>
        public DateTime CurrentDateTime
        {
            private set;
            get;
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="rooms">計算対象のRoomリスト</param>
        public MultiRoom(Room[] rooms)
        {
            this.rooms = rooms;

            Initialize();
        }

        /// <summary>初期化処理</summary>
        public void Initialize()
        {
            sfNumber = new uint[rooms.Length];
            znNumber = new uint[rooms.Length];

            //壁表面とゾーンの数を積算
            uint sfSum = 0;
            uint sfZnSum = 0;
            uint znSum = 0;
            List<ISurface> sfs = new List<ISurface>();
            List<Zone> zns = new List<Zone>();
            for (int i = 0; i < rooms.Length; i++)
            {
                //AX行列におけるRoom要素の開始列数
                if(axRmIndices.ContainsKey(rooms[i])) axRmIndices[rooms[i]] = sfSum;
                else axRmIndices.Add(rooms[i], sfSum);

                //壁表面数
                sfNumber[i] = rooms[i].SurfaceNumber;
                sfSum += rooms[i].SurfaceNumber;

                //ゾーン数
                znNumber[i] = rooms[i].ZoneNumber;
                znSum += rooms[i].ZoneNumber;
                zns.AddRange(rooms[i].getZone());
                for (uint j = 0; j < rooms[i].ZoneNumber; j++)
                {
                    Zone zn = rooms[i].getZone(j);

                    if (znToRm.ContainsKey(zn)) znToRm[zn] = rooms[i];
                    else znToRm.Add(zn, rooms[i]);

                    //AX行列におけるZone要素の開始列数
                    if (axZnIndices.ContainsKey(zn)) axZnIndices[zn] = sfZnSum;
                    else axZnIndices.Add(zn, sfZnSum);
                    sfZnSum += (uint)zn.Surfaces.Length;
                }

                //壁表面リスト
                sfs.AddRange(rooms[i].getSurface());
                for (uint j = 0; j < rooms[i].SurfaceNumber; j++)
                {
                    if (sfToRm.ContainsKey(rooms[i].getSurface(j))) sfToRm[rooms[i].getSurface(j)] = rooms[i];
                    else sfToRm.Add(rooms[i].getSurface(j), rooms[i]);
                }
            }
            surfaces = sfs.ToArray();
            zones = zns.ToArray();

            //行列・ベクトルのメモリ領域を用意
            tzVector = new Vector(znSum);
            arbPerm = new uint[znSum];
            arMatrix = new double[znSum, znSum];
            bMatrix = new Matrix(znSum, znSum);
            xa = new Matrix(sfSum, sfSum);
            phi = new Matrix(sfSum, sfSum);
            surfaceTemperatures = new Vector(sfSum);
            surfaceMRTs = new Vector(sfSum);
            crxVector = new Vector(sfSum);
            craVector = new Vector(sfSum);
            caVector = new double[znSum];
            bMatrix = new Matrix(znSum, znSum);
            xbMatrix = new Matrix(znSum, znSum);
            bVector = new Vector(znSum);
            bbVector = new Vector(znSum);
            for (int i = 0; i < rooms.Length; i++)
            {
                for (uint j = 0; j < rooms[i].ZoneNumber; j++)
                {
                    if(! rxVector.ContainsKey(rooms[i].getZone(j))) rxVector.Add(rooms[i].getZone(j), new Vector(sfSum));
                    if (!raVector.ContainsKey(rooms[i].getZone(j))) raVector.Add(rooms[i].getZone(j), new Vector(sfSum));
                }
            }
            for (int i = 0; i < zones.Length; i++)
            {
                if (!airFlowToZone.ContainsKey(zones[i])) airFlowToZone.Add(zones[i], new Dictionary<ImmutableZone, double>());
            }

            //表面温度計算式の逆行列を計算する
            makeXAMatrix();

            //壁および窓構成変更イベントへの対応
            foreach (ISurface sf in surfaces)
            {
                sf.FIOChangeEvent += new EventHandler(sf_FIOChangeEvent);
            }
        }

        /// <summary>壁および窓のFIおよびFO変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sf_FIOChangeEvent(object sender, EventArgs e)
        {
            hasFIOChanged = true;
        }

        #endregion

        #region publicメソッド

        /// <summary>室の絶対湿度を更新する</summary>
        public void UpdateRoomHumidities()
        {
            //行列入れ替え配列を作成
            uint ffZones = makePermVectorAH();

            //Bマトリクス・Bベクトル・Tマトリクスを作成
            for (uint i = 0; i < zones.Length; i++)
            {
                //Bマトリクスを作成
                //ゾーンの水蒸気容量/時間間隔[W/K]を計算
                double airSV = MoistAir.GetAirStateFromDBAH(zones[i].CurrentDrybulbTemperature,
                            zones[i].CurrentAbsoluteHumidity, MoistAir.Property.SpecificVolume);
                double zSH = (zones[i].Volume / airSV + zones[i].LatentHeatCapacity) / TimeStep;
                double cgo = zones[i].VentilationVolume / airSV / 3600d;
                Dictionary<ImmutableZone, double> aFlow = airFlowToZone[zones[i]];

                for (uint j = 0; j < zones.Length; j++)
                {
                    double cgr = 0;
                    //対角成分
                    if (i == j)
                    {
                        foreach (Zone key in aFlow.Keys) cgr += aFlow[key];
                        cgr = cgr / airSV / 3600d;
                        bMatrix.SetValue(arbPerm[i], arbPerm[j], zSH + cgo + cgr);
                    }
                    //その他成分
                    else
                    {
                        if (aFlow.ContainsKey(zones[j])) cgr = aFlow[zones[j]] / airSV / 3600d;
                        else cgr = 0;
                        bMatrix.SetValue(arbPerm[i], arbPerm[j], -cgr);
                    }
                }

                //Bベクトル・Tマトリクス
                bVector.SetValue(arbPerm[i], zSH * zones[i].CurrentAbsoluteHumidity
                    + cgo * zones[i].VentilationAirState.AbsoluteHumidity
                    + zones[i].integrateLatentHeatGain() / MoistAir.LatentHeatOfVaporization / 1000);
                if (zones[i].ControlAbsoluteHumidity)
                {
                    tzVector.SetValue(arbPerm[i], zones[i].AbsoluteHumiditySetPoint);
                }
                else
                {
                    bVector.AddValue(arbPerm[i], zones[i].LatentHeatSupply / MoistAir.LatentHeatOfVaporization / 1000);
                    //-供給熱量[W]が即ち熱負荷[W]
                    zones[i].CurrentLatentHeatLoad = -zones[i].LatentHeatSupply;
                }
            }

            //定数部分を差し引く
            uint ccZones = (uint)(zones.Length - ffZones);
            for (uint i = 0; i < ffZones; i++)
            {
                double br = bVector.GetValue(i);
                for (uint j = 0; j < ccZones; j++)
                {
                    br -= bMatrix.GetValue(i, j + ffZones) * tzVector.GetValue(j + ffZones);
                }
                bVector.SetValue(i, br);
            }

            MatrixView mView = new MatrixView(bMatrix, 0, 0, ffZones, ffZones);
            VectorView bView = new VectorView(bVector, 0, ffZones);
            VectorView tView = new VectorView(tzVector, 0, ffZones);
            //空気湿度が変動するゾーンを逆行列で解く
            if (0 < ffZones)
            {
                int sig;
                MatrixView xmView = new MatrixView(xbMatrix, 0, 0, ffZones, ffZones);
                Permutation bPerm = new Permutation(xmView.ColumnSize, true);
                LinearAlgebra.LUDecomposition(ref mView, ref bPerm, out sig);
                LinearAlgebra.LUInvert(mView, bPerm, ref xmView);

                Blas.DGemv(Blas.TransposeType.NoTranspose, 1, xmView, bView, 0, ref tView);
            }

            //絶対湿度をゾーンに設定
            for (uint i = 0; i < zones.Length; i++) zones[i].setAbsoluteHumidity(tzVector.GetValue(arbPerm[i]));

            if (0 < ccZones)
            {
                //絶対湿度指定ゾーンの熱負荷を計算
                //ゾーン絶対湿度に依存する成分
                mView.Initialize(bMatrix, ffZones, 0, ccZones, (uint)zones.Length);
                tView.Initialize(tzVector, 0, tzVector.Size);
                bView.Initialize(bbVector, ffZones, ccZones);
                Blas.DGemv(Blas.TransposeType.NoTranspose, 1, mView, tView, 0, ref bView);
                //
                for (uint i = 0; i < zones.Length; i++)
                {
                    if (zones[i].ControlAbsoluteHumidity)
                    {
                        zones[i].CurrentLatentHeatLoad = - MoistAir.LatentHeatOfVaporization * 1000
                            * (bbVector.GetValue(arbPerm[i]) - bVector.GetValue(arbPerm[i]));
                    }
                }
            }
        }

        /// <summary>計算時間間隔が正常か否か</summary>
        /// <returns>正常の場合は真</returns>
        private bool isTimeStepCorrect()
        {
            //ゾーンのタイムステップ
            foreach (Zone zn in zones)
            {
                if (zn.TimeStep != TimeStep) return false;
            }

            //表面のタイムステップ
            foreach (ISurface sf in surfaces)
            {
                if (sf is WallSurface)
                {
                    if (((WallSurface)sf).TimeStep != TimeStep) return false;
                }
            }

            return true;
        }

        /// <summary>室の乾球温度を更新する</summary>
        public void UpdateRoomTemperatures()
        {
            //タイムステップ確認
            if (!isTimeStepCorrect()) throw new Exception("計算時間間隔が不正です");

            //壁および窓の構成が変化した場合には逆行列を初期化
            if (hasFIOChanged) makeXAMatrix();

            //各Roomの放射を壁面に設定
            setRadiationToSurface();

            //壁表面計算用行列を作成
            makeARMatrixAndCAVector();

            //行列入れ替え配列を作成
            uint ffZones = makePermVectorDB();

            //Bマトリクス・Bベクトル・Tマトリクスを作成
            for (uint i = 0; i < zones.Length; i++)
            {
                //Bマトリクスを作成
                //ゾーンの熱容量/時間間隔[W/K]を計算
                double airSV = MoistAir.GetAirStateFromDBAH(zones[i].CurrentDrybulbTemperature,
                            zones[i].CurrentAbsoluteHumidity, MoistAir.Property.SpecificVolume);
                double cpAir = MoistAir.GetSpecificHeat(zones[i].CurrentAbsoluteHumidity) * 1000;
                double zSH = (zones[i].Volume / airSV * cpAir + zones[i].SensibleHeatCapacity) / TimeStep;
                double cgo = zones[i].VentilationVolume / airSV / 3600d * cpAir;
                Dictionary<ImmutableZone, double> aFlow = airFlowToZone[zones[i]];
                
                for (uint j = 0; j < zones.Length; j++)
                {
                    double cgr = 0;
                    //対角成分
                    if (i == j)
                    {
                        foreach (Zone key in aFlow.Keys) cgr += aFlow[key];
                        cgr = cgr / airSV / 3600d * cpAir;
                        bMatrix.SetValue(arbPerm[i], arbPerm[j], zSH + arMatrix[i, j] + cgo + cgr);
                    }
                    //その他成分
                    else
                    {
                        if (aFlow.ContainsKey(zones[j])) cgr = aFlow[zones[j]] / airSV / 3600d * cpAir;
                        else cgr = 0;
                        bMatrix.SetValue(arbPerm[i], arbPerm[j], -(arMatrix[i, j] + cgr));
                    }
                }

                //Bベクトル・Tマトリクス
                bVector.SetValue(arbPerm[i], zSH * zones[i].CurrentDrybulbTemperature
                    + caVector[i] + cgo * zones[i].VentilationAirState.DryBulbTemperature
                    + zones[i].integrateConvectiveHeatGain());
                if (zones[i].ControlDrybulbTemperature)
                {
                    tzVector.SetValue(arbPerm[i], zones[i].DrybulbTemperatureSetPoint);                    
                }
                else
                {
                    bVector.AddValue(arbPerm[i], zones[i].SensibleHeatSupply);
                    //-供給熱量[W]が即ち熱負荷[W]
                    zones[i].CurrentSensibleHeatLoad = -zones[i].SensibleHeatSupply;                    
                }
            }

            //定数部分を差し引く
            uint ccZones = (uint)(zones.Length - ffZones);
            for (uint i = 0; i < ffZones; i++)
            {
                double br = bVector.GetValue(i);
                for (uint j = 0; j < ccZones; j++)
                {
                    br -= bMatrix.GetValue(i, j + ffZones) * tzVector.GetValue(j + ffZones);
                }
                bVector.SetValue(i, br);
            }

            MatrixView mView = new MatrixView(bMatrix, 0, 0, ffZones, ffZones);            
            VectorView bView = new VectorView(bVector, 0, ffZones);
            VectorView tView = new VectorView(tzVector, 0, ffZones);
            //空気温度が変動するゾーンを逆行列で解く
            if (0 < ffZones)
            {
                int sig;
                MatrixView xmView = new MatrixView(xbMatrix, 0, 0, ffZones, ffZones);
                Permutation bPerm = new Permutation(xmView.ColumnSize, true);
                LinearAlgebra.LUDecomposition(ref mView, ref bPerm, out sig);
                LinearAlgebra.LUInvert(mView, bPerm, ref xmView);                
                
                Blas.DGemv(Blas.TransposeType.NoTranspose, 1, xmView, bView, 0, ref tView);
            }

            //温度をゾーンに設定
            for (uint i = 0; i < zones.Length; i++) zones[i].setDrybulbTemperature(tzVector.GetValue(arbPerm[i]));

            if (0 < ccZones)
            {
                //空気温度指定ゾーンの熱負荷を計算
                //ゾーン空気温度に依存する成分
                mView.Initialize(bMatrix, ffZones, 0, ccZones, (uint)zones.Length);
                tView.Initialize(tzVector, 0, tzVector.Size);
                bView.Initialize(bbVector, ffZones, ccZones);
                Blas.DGemv(Blas.TransposeType.NoTranspose, 1, mView, tView, 0, ref bView);
                //
                for (uint i = 0; i < zones.Length; i++)
                {
                    if (zones[i].ControlDrybulbTemperature)
                    {
                        zones[i].CurrentSensibleHeatLoad = -(bbVector.GetValue(arbPerm[i]) - bVector.GetValue(arbPerm[i]));
                    }
                }
            }

            //表面温度を計算する
            surfaceTemperatures.SetValue(0);
            foreach (Zone spc in rxVector.Keys)
            {
                Vector vec = raVector[spc];
                for (uint i = 0; i < vec.Size; i++) surfaceTemperatures.AddValue(i, vec.GetValue(i) * spc.CurrentDrybulbTemperature);
            }
            for (uint i = 0; i < craVector.Size; i++) surfaceTemperatures.AddValue(i, craVector.GetValue(i));

            //室温を表面に設定
            for (int i = 0; i < zones.Length; i++)
            {
                ISurface[] sfs = zones[i].getSurfaces();
                for (int j = 0; j < sfs.Length; j++)
                {
                    //表面近傍の空気温度を設定
                    sfs[j].AirTemperature = zones[i].CurrentDrybulbTemperature * sfs[j].ConvectiveRate;
                }
            }

            //放射を表面に設定
            Blas.DGemv(Blas.TransposeType.NoTranspose, 1, phi, surfaceTemperatures, 0, ref surfaceMRTs);
            for (uint i = 0; i < surfaces.Length; i++)
            {
                surfaces[i].Radiation += surfaceMRTs.GetValue(i) * surfaces[i].OverallHeatTransferCoefficient;
            }

            //平均放射温度[C]を計算して設定
            foreach (Room room in rooms)
            {
                ImmutableSurface[] sfs = room.GetSurface();
                double mrt = 0;
                double aSum = 0;
                for (int i = 0; i < sfs.Length; i++)
                {
                    mrt += sfs[i].Temperature * sfs[i].Area;
                    aSum += sfs[i].Area;
                }
                mrt /= aSum;
                Zone[] zns = room.getZone();
                for (int i = 0; i < zns.Length; i++) zns[i].setMeanRadiantTemperature(mrt);
            }
        }

        /// <summary>計算時間間隔[s]を設定する</summary>
        /// <param name="timeStep">計算時間間隔[s]</param>
        public void SetTimeStep(double timeStep)
        {
            this.timeStep = timeStep;
            foreach (Zone zn in zones)
            {
                zn.TimeStep = timeStep;
            }
            makeXAMatrix();
        }

        /// <summary>室間換気量[m3/h]を設定する</summary>
        /// <param name="upstreamZone">上流（空気が吹き出す側）の室</param>
        /// <param name="downstreamZone">下流（空気が吹き込む側）の室</param>
        /// <param name="airFlow">室間換気量[m3/h]</param>
        public void SetAirFlow(ImmutableZone upstreamZone, ImmutableZone downstreamZone, double airFlow)
        {
            Dictionary<ImmutableZone, double> af = this.airFlowToZone[downstreamZone];
            if (af.ContainsKey(upstreamZone))
            {
                if (airFlow == 0) af.Remove(upstreamZone);
                else af[upstreamZone] = airFlow;
            }
            else if (airFlow != 0) af.Add(upstreamZone, airFlow);
        }

        /// <summary>室間換気量[m3/h]を取得する</summary>
        /// <param name="upstreamZone">上流（空気が吹き出す側）の室</param>
        /// <param name="downstreamZone">下流（空気が吹き込む側）の室</param>
        /// <returns>室間換気量[m3/h]</returns>
        public double GetAirFlow(ImmutableZone upstreamZone, ImmutableZone downstreamZone)
        {
            Dictionary<ImmutableZone, double> af = this.airFlowToZone[downstreamZone];
            if (af.ContainsKey(upstreamZone)) return af[upstreamZone];
            else return 0;
        }

        /// <summary>現在の日時を設定する</summary>
        /// <param name="dTime">現在の日時</param>
        public void SetCurrentDateTime(DateTime dTime)
        {
            CurrentDateTime = dTime;
            for (int i = 0; i < rooms.Length; i++) rooms[i].SetCurrentDateTime(dTime);
        }

        #endregion

        #region privateメソッド

        /// <summary>表面温度計算式の逆行列を計算する</summary>
        private void makeXAMatrix()
        {
            //単位行列を作成
            Matrix ax = new Matrix(xa.Rows, xa.Columns);
            ax.MakeUnitMatrix();

            //行列AXを作成
            for (uint i = 0; i < surfaces.Length; i++)
            {
                ISurface sf;
                Room rm;

                //同一Roomに属する表面との係数
                rm = sfToRm[surfaces[i]];
                uint sIndex = axRmIndices[rm];
                for (uint j = 0; j < rm.SurfaceNumber; j++)
                {
                    sf = rm.getSurface(j);
                    phi.SetValue(i, sIndex + j, rm.GetRadiativeHeatTransferRate(surfaces[i], sf) * surfaces[i].RadiativeRate);
                    ax.AddValue(i, sIndex + j, -rm.GetRadiativeHeatTransferRate(surfaces[i], sf)
                        * surfaces[i].RadiativeRate * surfaces[i].FI);
                }

                //逆側の表面が属するRoomの、他の表面との係数
                sf = surfaces[i].OtherSideSurface;
                if (sfToRm.ContainsKey(sf))
                {
                    rm = sfToRm[sf];
                    sIndex = axRmIndices[rm];
                    for (uint j = 0; j < rm.SurfaceNumber; j++)
                    {
                        ISurface sf2 = rm.getSurface(j);
                        ax.SetValue(i, sIndex + j, -rm.GetRadiativeHeatTransferRate(sf, sf2)
                            * sf.RadiativeRate * surfaces[i].FO);
                    }
                }
            }

            //逆行列XAを計算
            int sig;
            Permutation perm = new Permutation(xa.Rows);
            perm.Initialize();
            LinearAlgebra.LUDecomposition(ref ax, ref perm, out sig);
            LinearAlgebra.LUInvert(ax, perm, ref xa);

            //FIO変更フラグを初期化
            hasFIOChanged = false;
        }

        /// <summary>行列入れ替え配列を作成する</summary>
        /// <returns>空気温度が変動するゾーンの数</returns>
        private uint makePermVectorDB()
        {
            //空気温度が変動するゾーンの数を取得
            uint freeFloatZones = 0;
            for (int i = 0; i < zones.Length; i++)
            {
                if (!zones[i].ControlDrybulbTemperature) freeFloatZones++;
            }

            int ffNum = 0;
            int ccNum = 0;
            for (uint i = 0; i < zones.Length; i++)
            {
                if (!zones[i].ControlDrybulbTemperature)
                {
                    arbPerm[i] = (uint)ffNum;
                    ffNum++;
                }
                else
                {
                    arbPerm[i] = (uint)(freeFloatZones + ccNum);
                    ccNum++;
                }
            }

            return freeFloatZones;
        }

        /// <summary>行列入れ替え配列を作成する</summary>
        /// <returns>絶対湿度が変動するゾーンの数</returns>
        private uint makePermVectorAH()
        {
            //空気温度が変動するゾーンの数を取得
            uint freeFloatZones = 0;
            for (int i = 0; i < zones.Length; i++)
            {
                if (!zones[i].ControlAbsoluteHumidity) freeFloatZones++;
            }

            int ffNum = 0;
            int ccNum = 0;
            for (uint i = 0; i < zones.Length; i++)
            {
                if (!zones[i].ControlAbsoluteHumidity)
                {
                    arbPerm[i] = (uint)ffNum;
                    ffNum++;
                }
                else
                {
                    arbPerm[i] = (uint)(freeFloatZones + ccNum);
                    ccNum++;
                }
            }

            return freeFloatZones;
        }

        /// <summary>各Roomの放射を表面に設定する</summary>
        internal void setRadiationToSurface()
        {
            for (int i = 0; i < rooms.Length; i++)
            {
                rooms[i].setRadiationToSurface();
            }
        }

        /// <summary>AR行列とCAベクトルを作成する</summary>
        private void makeARMatrixAndCAVector()
        {
            for (uint i = 0; i < surfaces.Length; i++)
            {
                ISurface sf = surfaces[i];

                //面している空間を取得
                Zone zn1 = sf.FacingZone;
                rxVector[zn1].SetValue(i, sf.FI * sf.ConvectiveRate);

                //逆側の空間を取得
                ISurface sf2 = surfaces[i].OtherSideSurface;
                Zone zn2 = sf2.FacingZone;
                if (zn2 == null)
                {
                    //窓の場合は吸収日射取得分を考慮
                    if (sf is WindowSurface)
                    {
                        ImmutableWindow win = ((WindowSurface)sf).WindowBody;
                        crxVector.SetValue(i, sf.FO * (sf2.GetSolAirTemperature() +
                            win.AbsorbedHeatGain / win.Glass.OverallHeatTransferCoefficient / win.SurfaceArea) + sf.CF
                                               + sf.FI * sf.Radiation / sf.OverallHeatTransferCoefficient + sf.FPT);
                    }
                    //壁体の場合
                    else
                    {
                        crxVector.SetValue(i, sf.FO * sf2.GetSolAirTemperature() + sf.CF
                           + sf.FI * sf.Radiation / sf.OverallHeatTransferCoefficient + sf.FPT);
                    }
                }
                //逆側の空間の乾球温度が状態変数の場合
                else if (rxVector.ContainsKey(zn2))
                {
                    rxVector[zn2].SetValue(i, sf.FO * sf2.ConvectiveRate);
                    crxVector.SetValue(i, sf.CF
                        + sf.FI * sf.Radiation / sf.OverallHeatTransferCoefficient
                        + sf.FO * sf2.Radiation / sf2.OverallHeatTransferCoefficient + sf.FPT);
                }
                //逆側の空間の乾球温度が境界条件の場合
                else
                {
                    crxVector.SetValue(i, sf.FO * sf2.GetSolAirTemperature() + sf.CF
                           + sf.FI * sf.Radiation / sf.OverallHeatTransferCoefficient + sf.FPT);
                }
            }
            
            //行列演算
            foreach (Zone spc in rxVector.Keys)
            {
                Vector vec = raVector[spc];
                Blas.DGemv(Blas.TransposeType.NoTranspose, 1, xa, rxVector[spc], 0, ref vec);
            }
            Blas.DGemv(Blas.TransposeType.NoTranspose, 1, xa, crxVector, 0, ref craVector);

            //ゾーン温度計算行列を作成
            for (uint i = 0; i < zones.Length; i++)
            {
                ISurface[] sfs = zones[i].getSurfaces();
                uint stIndex = axZnIndices[zones[i]];

                //ARマトリクスを作成
                for (uint j = 0; j < zones.Length; j++)
                {
                    arMatrix[i, j] = 0;                    

                    //対角成分の場合
                    if (i == j)
                    {
                        for (uint k = 0; k < sfs.Length; k++)
                        {
                            arMatrix[i, j] += sfs[k].Area * sfs[k].OverallHeatTransferCoefficient * sfs[k].ConvectiveRate
                                * (1d - raVector[zones[i]].GetValue(stIndex + k));
                        }
                    }
                    //その他
                    else
                    {
                        for (uint k = 0; k < sfs.Length; k++)
                        {
                            arMatrix[i, j] += sfs[k].Area * sfs[k].OverallHeatTransferCoefficient * sfs[k].ConvectiveRate
                                * raVector[zones[j]].GetValue(stIndex + k);
                        }
                    }
                }

                //CAベクトル
                caVector[i] = 0;
                for (uint j = 0; j < sfs.Length; j++)
                {
                    caVector[i] += sfs[j].Area * sfs[j].OverallHeatTransferCoefficient * sfs[j].ConvectiveRate
                               * craVector.GetValue(stIndex + j);
                }
            }
        }

        #endregion

    }

    /// <summary>読み取り専用の多数室</summary>
    public interface ImmutableMultiRoom
    {

        #region プロパティ

        /// <summary>計算時間間隔[sec]を取得する</summary>
        double TimeStep
        {
            get;
        }

        /// <summary>室リストを取得する</summary>
        ImmutableRoom[] Rooms
        {
            get;
        }

        /// <summary>現在の日時を取得する</summary>
        DateTime CurrentDateTime
        {
            get;
        }

        #endregion

        #region publicメソッド

        /// <summary>室間換気量[m3/h]を取得する</summary>
        /// <param name="upstreamZone">上流（空気が吹き出す側）の室</param>
        /// <param name="downstreamZone">下流（空気を吸い込む側）の室</param>
        /// <returns>室間換気量[m3/h]</returns>
        double GetAirFlow(ImmutableZone upstreamZone, ImmutableZone downstreamZone);

        #endregion

    }

}
