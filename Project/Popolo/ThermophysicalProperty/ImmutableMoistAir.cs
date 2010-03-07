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
    /// <summary>�ǂݎ���p�����C</summary>
    public interface ImmutableMoistAir : ISerializable
    {

        /// <summary>��C��[kPa]���擾����</summary>
        double AtmosphericPressure
        {
            get;
        }

        /// <summary>�������x[C]���擾����</summary>
        double DryBulbTemperature
        {
            get;
        }

        /// <summary>�������x[C]���擾����</summary>
        double WetBulbTemperature
        {
            get;
        }

        /// <summary>��Ύ��x[kg/kg(DA)]���擾����</summary>
        double AbsoluteHumidity
        {
            get;
        }

        /// <summary>���Ύ��x[%]���擾����</summary>
        double RelativeHumidity
        {
            get;
        }

        /// <summary>�G���^���s�[[kJ/kg]���擾����</summary>
        double Enthalpy
        {
            get;
        }

        /// <summary>��e��[m3/kg]���擾����</summary>
        double SpecificVolume
        {
            get;
        }

        /// <summary>��C��Ԃ��R�s�[����</summary>
        /// <param name="air">�R�s�[��̎����C�I�u�W�F�N�g</param>
        void CopyTo(MoistAir air);

    }
}
