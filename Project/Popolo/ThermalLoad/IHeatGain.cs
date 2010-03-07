/* IHeatGain.cs
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

namespace Popolo.ThermalLoad
{
    /// <summary>発熱要素インターフェース</summary>
    public interface IHeatGain
    {

        /// <summary>熱取得[W]の内、対流成分を取得する</summary>
        /// <param name="zone">発熱要素が属するゾーン</param>
        /// <returns>熱取得の対流成分[kW]</returns>
        double GetConvectiveHeatGain(ImmutableZone zone);

        /// <summary>熱取得[W]の内、放射成分を取得する</summary>
        /// <param name="zone">発熱要素が属するゾーン</param>
        /// <returns>熱取得の放射成分[kW]</returns>
        double GetRadiativeHeatGain(ImmutableZone zone);

        /// <summary>潜熱負荷[W]を取得する</summary>
        /// <param name="zone">発熱要素が属するゾーン</param>s
        /// <returns>潜熱負荷[W]</returns>
        double GetLatentHeatGain(ImmutableZone zone);

    }
}
