/* ConstantHeatGain.cs
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

namespace Popolo.ThermalLoad
{
    /// <summary>Simple heat gain class</summary>
    public class ConstantHeatGain : IHeatGain
    {

        #region Properties

        /// <summary>Get and set convective heat gain [W].</summary>
        public double ConvectiveHeatGain
        {
            get;
            set;
        }

        /// <summary>Get and set latent heat gain [W].</summary>
        public double LatentHeatGain
        {
            get;
            set;
        }

        /// <summary>Get and set radiative heat gain [W].</summary>
        public double RadiativeHeatGain
        {
            get;
            set;
        }

        #endregion

        #region Constructor

        /// <summary>Constructor</summary>
        /// <param name="convectiveHeatGain">熱取得の対流成分[W]</param>
        /// <param name="radiativeHeatGain">熱取得の放射成分[W]</param>
        /// <param name="latentHeatGain">潜熱負荷[W]</param>
        public ConstantHeatGain(
            double convectiveHeatGain,
            double radiativeHeatGain,
            double latentHeatGain)
        {
            this.ConvectiveHeatGain = convectiveHeatGain;
            this.RadiativeHeatGain = radiativeHeatGain;
            this.LatentHeatGain = latentHeatGain;
        }

        #endregion

        #region IHeatGain実装

        /// <summary>熱取得[W]の内、対流成分を取得する</summary>
        /// <param name="zone">発熱要素が属するゾーン</param>
        /// <returns>熱取得の対流成分[W]</returns>
        public double GetConvectiveHeatGain(ImmutableZone zone)
        {
            return ConvectiveHeatGain;
        }

        /// <summary>熱取得[W]の内、放射成分を取得する</summary>
        /// <param name="zone">発熱要素が属するゾーン</param>
        /// <returns>熱取得の放射成分[W]</returns>
        public double GetRadiativeHeatGain(ImmutableZone zone)
        {
            return RadiativeHeatGain;
        }

        /// <summary>潜熱負荷[W]を取得する</summary>
        /// <param name="zone">発熱要素が属するゾーン</param>
        /// <returns>潜熱負荷[W]</returns>
        public double GetLatentHeatGain(ImmutableZone zone)
        {
            return LatentHeatGain;
        }

        #endregion
        
    }
}
