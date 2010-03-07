/* InterZoneAirExchange.cs
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
using System.Collections.Generic;
using System.Text;

using Popolo.ThermophysicalProperty;

namespace Popolo.ThermalLoad
{
    /// <summary>ゾーン間換気クラス</summary>
    public class InterZoneAirExchange
    {

        #region インスタンス変数

        /// <summary>ゾーンの換気量リスト</summary>
        private Dictionary<ImmutableZone, Dictionary<ImmutableZone, double>> zones = new Dictionary<ImmutableZone, Dictionary<ImmutableZone, double>>();

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="zones">ゾーン間換気を行うゾーンリスト</param>
        public InterZoneAirExchange(ImmutableZone[] zones)
        {
            //初期化
            for (int i = 0; i < zones.Length; i++) this.zones.Add(zones[i], new Dictionary<ImmutableZone, double>());
        }

        #endregion

        #region publicメソッド

        /// <summary>ゾーン間換気量[m3/h]を設定する</summary>
        /// <param name="zone1">ゾーン1</param>
        /// <param name="zone2">ゾーン2</param>
        /// <param name="airExchangeVolume">ゾーン間換気量[m3/h]</param>
        /// <returns>設定成功の真偽</returns>
        public bool SetAirExchangeVolume(ImmutableZone zone1, ImmutableZone zone2, double airExchangeVolume)
        {
            if (!zones.ContainsKey(zone1) || !zones.ContainsKey(zone2)) return false;
            
            Dictionary<ImmutableZone, double> val = zones[zone1];
            if (airExchangeVolume <= 0) val.Remove(zone2);
            else val[zone2] = airExchangeVolume;

            val = zones[zone2];
            if (airExchangeVolume <= 0) val.Remove(zone1);
            else val[zone1] = airExchangeVolume;

            return true;
        }

        /// <summary>ゾーン間換気量[m3/h]を取得する</summary>
        /// <param name="zone1">ゾーン1</param>
        /// <param name="zone2">ゾーン2</param>
        /// <returns>ゾーン間換気量[m3/h]</returns>
        public double GetAirExchangeVolume(ImmutableZone zone1, ImmutableZone zone2)
        {
            if (!zones.ContainsKey(zone1) || !zones.ContainsKey(zone2)) return 0;

            Dictionary<ImmutableZone, double> val = zones[zone1];
            if (val.ContainsKey(zone2)) return val[zone2];
            else return 0;
        }

        /// <summary>ゾーン一覧を取得する</summary>
        /// <returns>ゾーン一覧</returns>
        public ImmutableZone[] GetZones()
        {
            List<ImmutableZone> zns = new List<ImmutableZone>();
            foreach (ImmutableZone zn in zones.Keys) zns.Add(zn);

            return zns.ToArray();
        }

        /// <summary>ゾーンへ流入する空気の状態と体積[m3/h]を取得する</summary>
        /// <param name="zone">ゾーン</param>
        /// <param name="airState">空気状態</param>
        /// <param name="volume">空気体積[m3/h]</param>
        public void GetExchangeAir(ImmutableZone zone, out ImmutableMoistAir airState, out double volume)
        {
            Dictionary<ImmutableZone, double> val = zones[zone];
            volume = 0;
            int cnt = val.Keys.Count;

            double[] dbt = new double[cnt];
            double[] ahd = new double[cnt];
            double[] vol = new double[cnt];
            int index = 0;
            foreach (ImmutableZone zn in val.Keys)
            {
                dbt[index] = zn.CurrentDrybulbTemperature;
                ahd[index] = zn.CurrentAbsoluteHumidity;
                vol[index] = val[zn];
                volume += vol[index];
                index++;
            }

            //混合
            airState = MoistAir.BlendAir(dbt, ahd, vol);
        }

        #endregion

    }
}
