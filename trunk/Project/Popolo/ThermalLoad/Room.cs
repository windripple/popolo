/* Room.cs
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

namespace Popolo.ThermalLoad
{
    /// <summary>室</summary>
    /// <remarks>
    /// 壁表面の相互放射を考慮する1以上のゾーンのまとまり。
    /// 壁体の両面が同一のRoomに属してはならない。
    /// </remarks>
    public class Room : ImmutableRoom
    {

        #region インスタンス変数

        /// <summary>放射熱伝達比率[-]</summary>
        private Dictionary<ISurface, Dictionary<ISurface, double>> phi = 
            new Dictionary<ISurface, Dictionary<ISurface, double>>();

        /// <summary>ゾーンリスト</summary>
        private Zone[] zones;

        /// <summary>表面リスト</summary>
        private List<ISurface> surfaces = new List<ISurface>();

        /// <summary>表面への短波長放射成分入射比率[-]</summary>
        private Dictionary<ISurface, double> shortWaveRadiationToSurface = new Dictionary<ISurface, double>();

        /// <summary>表面への長波長放射成分入射比率[-]</summary>
        private Dictionary<ISurface, double> longWaveRadiationToSurface = new Dictionary<ISurface, double>();

        /// <summary>表面への短波長放射成分入射比率[-]：合算して1.0になるように調整済み</summary>
        private double[] shortWaveRadiationRate;

        /// <summary>表面への短波長放射成分入射比率[-]：合算して1.0になるように調整済み</summary>
        private double[] longWaveRadiationRate;

        #endregion

        #region プロパティ

        /// <summary>名称を設定・取得する</summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>壁表面の数を取得する</summary>
        public uint SurfaceNumber
        {
            get
            {
                return (uint)surfaces.Count;
            }
        }

        /// <summary>ゾーンの数を取得する</summary>
        public uint ZoneNumber
        {
            get
            {
                return (uint)zones.Length;
            }
        }

        /// <summary>窓からの透過日射損失[W]を取得する</summary>
        public double TransmissionHeatLossFromWindow
        {
            get;
            private set;
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
        /// <param name="zones">ゾーンリスト</param>
        public Room(Zone[] zones)
        {
            initialize(zones);
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="zones">ゾーンリスト</param>
        /// <param name="name">室名称</param>
        public Room(Zone[] zones, string name)
        {
            Name = name;
            initialize(zones);
        }

        /// <summary>初期化する</summary>
        /// <param name="zones">ゾーンリスト</param>
        private void initialize(Zone[] zones)
        {
            this.zones = zones;

            //表面リストを作成
            for (int i = 0; i < zones.Length; i++)
            {
                surfaces.AddRange(zones[i].getSurfaces());
            }

            //全表面積を計算
            double surfaceArea = 0;
            for (int i = 0; i < surfaces.Count; i++) surfaceArea += surfaces[i].Area;

            //放射熱伝達比率を面積比で初期化
            for (int i = 0; i < surfaces.Count; i++)
            {
                Dictionary<ISurface, double> lis = new Dictionary<ISurface, double>();
                phi.Add(surfaces[i], lis);

                for (int j = 0; j < surfaces.Count; j++) lis[surfaces[j]] = surfaces[j].Area / surfaceArea;

                longWaveRadiationToSurface.Add(surfaces[i], surfaces[i].Area);
                shortWaveRadiationToSurface.Add(surfaces[i], surfaces[i].Area);
            }

            //長波長・短波長放射成分入射比率[-]を初期化する
            initializeLongWaveRadiationRate();
            initializeShortWaveRadiationRate();

            //面積変更イベント登録
            for (int i = 0; i < surfaces.Count; i++) surfaces[i].AreaChangeEvent += new EventHandler(Room_AreaChangeEvent);
        }

        /// <summary>面積変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Room_AreaChangeEvent(object sender, EventArgs e)
        {
            //全表面積を計算
            double surfaceArea = 0;
            for (int i = 0; i < surfaces.Count; i++) surfaceArea += surfaces[i].Area;

            //放射熱伝達比率を面積比で初期化
            for (int i = 0; i < surfaces.Count; i++)
            {
                for (int j = 0; j < surfaces.Count; j++) phi[surfaces[i]][surfaces[j]] = surfaces[j].Area / surfaceArea;

                longWaveRadiationToSurface[surfaces[i]] = surfaces[i].Area;
                shortWaveRadiationToSurface[surfaces[i]] = surfaces[i].Area;
            }

            //長波長・短波長放射成分入射比率[-]を初期化する
            initializeLongWaveRadiationRate();
            initializeShortWaveRadiationRate();
        }

        #endregion

        #region 放射成分設定関連の処理

        /// <summary>放射成分を表面に設定する</summary>
        internal void setRadiationToSurface()
        {
            TransmissionHeatLossFromWindow = 0;

            double swr = IntegrateRadiativeHeatGain(true);
            double lwr = IntegrateRadiativeHeatGain(false);
            for (int i = 0; i < surfaces.Count; i++)
            {
                if (surfaces[i] is WindowSurface)
                {
                    TransmissionHeatLossFromWindow += shortWaveRadiationRate[i] * swr;
                    surfaces[i].Radiation = longWaveRadiationRate[i] * lwr / surfaces[i].Area;
                }
                else surfaces[i].Radiation = (shortWaveRadiationRate[i] * swr + longWaveRadiationRate[i] * lwr) / surfaces[i].Area;
            }
        }

        /// <summary>窓面の短波長放射成分入射比率[-]を設定する</summary>
        /// <param name="window">窓面</param>
        /// <param name="rate">窓面の短波長放射成分入射比率[-]</param>
        /// <remarks>
        /// 室内にある他の窓や壁の設定値と比較しながらプログラム内部で0～1の範囲に調整される。
        /// デフォルトでは面積比が設定される。
        /// </remarks>
        public void SetShortWaveRadiationRate(ImmutableWindow window, double rate)
        {
            WindowSurface ws = window.GetSurface(false);

            //窓面が存在しない場合
            if (!shortWaveRadiationToSurface.ContainsKey(ws)) return;

            shortWaveRadiationToSurface[ws] = rate;

            //短波長放射成分入射比率[-]を初期化
            initializeShortWaveRadiationRate();
        }

        /// <summary>窓面の短波長放射成分入射比率[-]を取得する</summary>
        /// <param name="window">窓面</param>
        /// <returns>窓面の短波長放射成分入射比率[-]</returns>
        public double GetShortWaveRadiationRate(ImmutableWindow window)
        {
            WindowSurface ws = window.GetSurface(false);

            if (shortWaveRadiationToSurface.ContainsKey(ws)) return shortWaveRadiationToSurface[ws];
            else return 0;
        }

        /// <summary>窓面の長波長放射成分入射比率[-]を設定する</summary>
        /// <param name="window">窓面</param>
        /// <param name="rate">窓面の長波長放射成分入射比率[-]</param>
        /// <remarks>
        /// 室内にある他の窓や壁の設定値と比較しながらプログラム内部で0～1の範囲に調整される。
        /// デフォルトでは面積比が設定される。
        /// </remarks>
        public void SetLongWaveRadiationRate(ImmutableWindow window, double rate)
        {
            WindowSurface ws = window.GetSurface(false);

            //窓面が存在しない場合
            if (!longWaveRadiationToSurface.ContainsKey(ws)) return;

            longWaveRadiationToSurface[ws] = rate;

            //長波長放射成分入射比率[-]を初期化
            initializeLongWaveRadiationRate();
        }

        /// <summary>窓面の長波長放射成分入射比率[-]を取得する</summary>
        /// <param name="window">窓面</param>
        /// <returns>窓面の長波長放射成分入射比率[-]</returns>
        public double GetLongWaveRadiationRate(ImmutableWindow window)
        {
            WindowSurface ws = window.GetSurface(false);

            if (longWaveRadiationToSurface.ContainsKey(ws)) return longWaveRadiationToSurface[ws];
            else return 0;
        }

        /// <summary>短波長放射成分入射比率[-]を設定する</summary>
        /// <param name="surface">表面</param>
        /// <param name="rate">短波長放射成分入射比率[-]</param>
        /// <remarks>
        /// 室内にある他の窓や壁の設定値と比較しながらプログラム内部で0～1の範囲に調整される。
        /// デフォルトでは面積比が設定される。
        /// </remarks>
        public void SetShortWaveRadiationRate(ISurface surface, double rate)
        {
            //壁面が存在しない場合
            if (!shortWaveRadiationToSurface.ContainsKey(surface)) return;

            shortWaveRadiationToSurface[surface] = rate;

            //短波長放射成分入射比率[-]を初期化
            initializeShortWaveRadiationRate();
        }

        /// <summary>短波長放射成分入射比率[-]を取得する</summary>
        /// <param name="surface">表面</param>
        /// <returns>短波長放射成分入射比率[-]</returns>
        public double GetShortWaveRadiationRate(ISurface surface)
        {
            if (shortWaveRadiationToSurface.ContainsKey(surface)) return shortWaveRadiationToSurface[surface];
            else return 0;
        }

        /// <summary>長波長放射成分入射比率[-]を設定する</summary>
        /// <param name="surface">表面</param>
        /// <param name="rate">長波長放射成分入射比率[-]</param>
        /// <remarks>
        /// 室内にある他の窓や壁の設定値と比較しながらプログラム内部で0～1の範囲に調整される。
        /// デフォルトでは面積比が設定される。
        /// </remarks>
        public void SetLongWaveRadiationRate(ISurface surface, double rate)
        {
            //壁面が存在しない場合
            if (!longWaveRadiationToSurface.ContainsKey(surface)) return;

            longWaveRadiationToSurface[surface] = rate;

            //長波長放射成分入射比率[-]を初期化
            initializeLongWaveRadiationRate();
        }

        /// <summary>長波長放射成分入射比率[-]を取得する</summary>
        /// <param name="surface">表面</param>
        /// <returns>長波長放射成分入射比率[-]</returns>
        public double GetLongWaveRadiationRate(ISurface surface)
        {
            if (longWaveRadiationToSurface.ContainsKey(surface)) return longWaveRadiationToSurface[surface];
            else return 0;
        }

        /// <summary>短波長放射成分入射比率[-]を初期化する</summary>
        private void initializeShortWaveRadiationRate()
        {
            shortWaveRadiationRate = new double[surfaces.Count];

            double slSum = 0;
            for (int i = 0; i < surfaces.Count; i++) slSum += shortWaveRadiationToSurface[surfaces[i]];
            for (int i = 0; i < surfaces.Count; i++) shortWaveRadiationRate[i] = shortWaveRadiationToSurface[surfaces[i]] / slSum;
        }

        /// <summary>長波長放射成分入射比率[-]を初期化する</summary>
        private void initializeLongWaveRadiationRate()
        {
            longWaveRadiationRate = new double[surfaces.Count];

            double slSum = 0;
            for (int i = 0; i < surfaces.Count; i++) slSum += longWaveRadiationToSurface[surfaces[i]];
            for (int i = 0; i < surfaces.Count; i++) longWaveRadiationRate[i] = longWaveRadiationToSurface[surfaces[i]] / slSum;
        }

        #endregion

        #region publicメソッド

        /// <summary>表面1から表面2への放射熱交換係数[-]を取得する</summary>
        /// <param name="surface1">表面1</param>
        /// <param name="surface2">表面2</param>
        /// <returns>表面1から表面2への放射熱交換係数[-]</returns>
        public double GetRadiativeHeatTransferRate(ISurface surface1, ISurface surface2)
        {
            return phi[surface1][surface2];
        }

        /// <summary>表面1から表面2への放射熱交換係数[-]を設定する</summary>
        /// <param name="surface1">表面1</param>
        /// <param name="surface2">表面2</param>
        /// <param name="rate">表面1から表面2への放射熱交換係数[-]</param>
        public void SetRadiativeHeatTransferRate(ISurface surface1, ISurface surface2, double rate)
        {
            phi[surface1][surface2] = rate;
        }

        /// <summary>指定の表面を保持しているか否かを返す</summary>
        /// <param name="surface">表面</param>
        /// <returns>保持しているか否か</returns>
        public bool HasSurface(ISurface surface)
        {
            return surfaces.Contains(surface);
        }

        /// <summary>表面を取得する</summary>
        /// <param name="index">表面の番号</param>
        /// <returns>表面</returns>
        internal ISurface getSurface(uint index)
        {
            return surfaces[(int)index];
        }

        /// <summary>表面リストを取得する</summary>
        /// <returns>表面リスト</returns>
        internal ISurface[] getSurface()
        {
            return surfaces.ToArray();
        }

        /// <summary>表面を取得する</summary>
        /// <param name="index">表面の番号</param>
        /// <returns>表面</returns>
        public ImmutableSurface GetSurface(uint index)
        {
            return getSurface(index);
        }

        /// <summary>表面リストを取得する</summary>
        /// <returns>表面リスト</returns>
        public ImmutableSurface[] GetSurface()
        {
            return getSurface();
        }

        /// <summary>ゾーンを取得する</summary>
        /// <param name="index">ゾーンの番号</param>
        /// <returns>ゾーン</returns>
        internal Zone getZone(uint index)
        {
            return zones[(int)index];
        }

        /// <summary>ゾーンリストを取得する</summary>
        /// <returns>ゾーンリスト</returns>
        internal Zone[] getZone()
        {
            return zones;
        }

        /// <summary>ゾーンを取得する</summary>
        /// <param name="index">ゾーンの番号</param>
        /// <returns>ゾーン</returns>
        public ImmutableZone GetZone(uint index)
        {
            return getZone(index);
        }

        /// <summary>ゾーンリストを取得する</summary>
        /// <returns>ゾーンリスト</returns>
        public ImmutableZone[] GetZone()
        {
            return getZone();
        }

        /// <summary>室の放射熱取得[W]を積算する</summary>
        /// <param name="isShortWave">短波長放射か否か</param>
        /// <returns>室の放射熱取得[W]</returns>
        public double IntegrateRadiativeHeatGain(bool isShortWave)
        {
            double rh = 0;
            for (int i = 0; i < zones.Length; i++) rh += zones[i].integrateRadiativeHeatGain(isShortWave);
            return rh;
        }

        /// <summary>室の対流熱取得[W]を積算する</summary>
        /// <returns>室の対流熱取得[W]</returns>
        public double IntegrateConvectiveHeatGain()
        {
            double rh = 0;
            for (int i = 0; i < zones.Length; i++) rh += zones[i].integrateConvectiveHeatGain();
            return rh;
        }

        /// <summary>現在の日時を設定する</summary>
        /// <param name="dTime">現在の日時</param>
        public void SetCurrentDateTime(DateTime dTime)
        {
            CurrentDateTime = dTime;
            for (int i = 0; i < zones.Length; i++) zones[i].CurrentDateTime = dTime;
        }

        #endregion

    }

    /// <summary>読み取り専用Room</summary>
    public interface ImmutableRoom
    {

        #region プロパティ

        /// <summary>名称を取得する</summary>
        string Name
        {
            get;
        }

        /// <summary>壁表面の数を取得する</summary>
        uint SurfaceNumber
        {
            get;
        }

        /// <summary>ゾーンの数を取得する</summary>
        uint ZoneNumber
        {
            get;
        }

        /// <summary>窓からの透過日射損失[W]を取得する</summary>
        double TransmissionHeatLossFromWindow
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

        /// <summary>窓面の短波長放射成分入射比率[-]を取得する</summary>
        /// <param name="window">窓面</param>
        /// <returns>窓面の短波長放射成分入射比率[-]</returns>
        double GetShortWaveRadiationRate(ImmutableWindow window);

        /// <summary>窓面の長波長放射成分入射比率[-]を取得する</summary>
        /// <param name="window">窓面</param>
        /// <returns>窓面の長波長放射成分入射比率[-]</returns>
        double GetLongWaveRadiationRate(ImmutableWindow window);

        /// <summary>短波長放射成分入射比率[-]を取得する</summary>
        /// <param name="surface">表面</param>
        /// <returns>短波長放射成分入射比率[-]</returns>
        double GetShortWaveRadiationRate(ISurface surface);

        /// <summary>長波長放射成分入射比率[-]を取得する</summary>
        /// <param name="surface">表面</param>
        /// <returns>長波長放射成分入射比率[-]</returns>
        double GetLongWaveRadiationRate(ISurface surface);

        /// <summary>表面1から表面2への放射熱伝達比率[-]を取得する</summary>
        /// <param name="surface1">表面1</param>
        /// <param name="surface2">表面2</param>
        /// <returns>表面1から表面2への放射熱伝達比率[-]</returns>
        double GetRadiativeHeatTransferRate(ISurface surface1, ISurface surface2);

        /// <summary>指定の表面を保持しているか否かを返す</summary>
        /// <param name="surface">表面</param>
        /// <returns>保持しているか否か</returns>
        bool HasSurface(ISurface surface);

        /// <summary>表面を取得する</summary>
        /// <param name="index">表面の番号</param>
        /// <returns>表面</returns>
        ImmutableSurface GetSurface(uint index);

        /// <summary>表面リストを取得する</summary>
        /// <returns>表面リスト</returns>
        ImmutableSurface[] GetSurface();

        /// <summary>ゾーンを取得する</summary>
        /// <param name="index">ゾーンの番号</param>
        /// <returns>ゾーン</returns>
        ImmutableZone GetZone(uint index);

        /// <summary>ゾーンリストを取得する</summary>
        /// <returns>ゾーンリスト</returns>
        ImmutableZone[] GetZone();

        /// <summary>室の放射熱取得[W]を積算する</summary>
        /// <param name="isShortWave">短波長放射か否か</param>
        /// <returns>室の放射熱取得[W]</returns>
        double IntegrateRadiativeHeatGain(bool isShortWave);

        /// <summary>室の対流熱取得[W]を積算する</summary>
        /// <returns>室の対流熱取得[W]</returns>
        double IntegrateConvectiveHeatGain();

        #endregion

    }

}
