/* ImmutableMoistAir.cs
 * 
 * Copyright (C) 2007 E.Togashi
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
using System.Runtime.Serialization;

namespace Popolo.ThermophysicalProperty
{
    /// <summary>読み取り専用湿り空気</summary>
    public interface ImmutableMoistAir : ISerializable
    {

        /// <summary>大気圧[kPa]を取得する</summary>
        double AtmosphericPressure
        {
            get;
        }

        /// <summary>乾球温度[C]を取得する</summary>
        double DryBulbTemperature
        {
            get;
        }

        /// <summary>湿球温度[C]を取得する</summary>
        double WetBulbTemperature
        {
            get;
        }

        /// <summary>絶対湿度[kg/kg(DA)]を取得する</summary>
        double AbsoluteHumidity
        {
            get;
        }

        /// <summary>相対湿度[%]を取得する</summary>
        double RelativeHumidity
        {
            get;
        }

        /// <summary>エンタルピー[kJ/kg]を取得する</summary>
        double Enthalpy
        {
            get;
        }

        /// <summary>比容積[m3/kg]を取得する</summary>
        double SpecificVolume
        {
            get;
        }

        /// <summary>空気状態をコピーする</summary>
        /// <param name="air">コピー先の湿り空気オブジェクト</param>
        void CopyTo(MoistAir air);

    }
}
