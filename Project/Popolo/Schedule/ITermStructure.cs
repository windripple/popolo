/* ITermStructure.cs
 * 
 * Copyright (C) 2007 E.Togashi
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

using System.Runtime.Serialization;

namespace Popolo.Schedule
{
    /// <remarks>�G�߂⎞�ԑѓ��̊��ԍ\�������C���^�[�t�F�[�X</remarks>
    public interface ITermStructure : ICloneable, ImmutableITermStructure
    {

        /// <summary>ItermStructure��ID���擾�E�ݒ肷��</summary>
        new int ID { get; set; }

        /// <summary>ITermStructure�̖��̂��擾�E�ݒ肷��</summary>
        new string Name { get; set; }

    }

    /// <summary>�ǂݎ���p���ԍ\���C���^�[�t�F�[�X</summary>
    public interface ImmutableITermStructure
    {
        /// <summary>ItermStructure��ID���擾����</summary>
        int ID { get; }

        /// <summary>ITermStructure�̖��̂��擾����</summary>
        string Name { get; }

        /// <summary>�G�߂⎞�ԑѓ��̊��Ԗ��̃��X�g���擾����</summary>
        /// <returns>�G�߂⎞�ԑѓ��̊��Ԗ��̃��X�g</returns>
        string[] GetTermNames();

        /// <summary>�������w�肵�ċG�߂⎞�ԑѓ��̊��Ԗ��̂��擾����</summary>
        /// <param name="dateTime">����</param>
        /// <returns>�G�߂⎞�ԑѓ��̊��Ԗ���</returns>
        string GetTermName(DateTime dateTime);
    }

}
