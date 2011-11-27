/* Days.cs
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
using System.Collections.Generic;

using System.Runtime.Serialization;

namespace Popolo.Schedule
{
    /// <summary>�j���N���X</summary>
    [Serializable]
    public class Days : ITermStructure, ISerializable, ImmutableDays
    {

        #region<�萔��`>

        /// <summary>�V���A���C�Y�p�o�[�W�������</summary>
        private double S_VERSION = 1.1;

        /// <summary>�X�P�W���[����`����N�i�[�N�Ɋ֌W�j</summary>
        private const int YEAR = 2001;

        #endregion//�萔��`

        #region<delegate��`>

        /// <summary>���̕ύX�C�x���g�n���h��</summary>
        public delegate void NameChangeEventHandler(object sender, EventArgs e);

        /// <summary>�j���ύX�C�x���g�n���h��</summary>
        public delegate void DayChangeEventHandler(object sender, DaysEventArgs e);

        #endregion//delegate��`

        #region<�C�x���g��`>

        /// <summary>���̕ύX�C�x���g</summary>
        public event NameChangeEventHandler NameChangeEvent;

        /// <summary>�j���ύX�C�x���g</summary>
        public event DayChangeEventHandler DayChangeEvent;

        #endregion//�C�x���g��`

        #region<enumerators>

        /// <summary>��`�ς݂̗j��</summary>
        public enum PredefinedDays
        {
            /// <summary>�S�j��</summary>
            AllWeek = 0,
            /// <summary>�T��</summary>
            WeekDayAndWeekEnd = 1,
            /// <summary>�j����</summary>
            OneWeek = 2,
        }

        #endregion//enumerators

        #region<Properties>

        /// <summary>ID��ݒ�E�擾����</summary>
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        /// <summary>���̂�ݒ�E�擾����</summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                if (NameChangeEvent != null) NameChangeEvent(this, new EventArgs());
            }
        }

        #endregion

        #region Instance variables

        /// <summary>ID</summary>
        private int id;

        /// <summary>����</summary>
        private string name;

        /// <summary>�O���[�v����</summary>
        private string[] termNames = new string[7];

        #endregion

        #region<Constructor>

        /// <summary>Constructor</summary>
        public Days()
        {
            //������
            Initialize(PredefinedDays.OneWeek);
        }

        /// <summary>Constructor</summary>
        /// <param name="predefinedDays">��`�ς݂̗j��</param>
        public Days(PredefinedDays predefinedDays)
        {
            //������
            Initialize(predefinedDays);
        }

        /// <summary>��`�ς̋G�߂ŏ���������</summary>
        /// <param name="predefinedDays">��`�ς݂̋G��</param>
        public void Initialize(PredefinedDays predefinedDays)
        {
            switch (predefinedDays)
            {
                case PredefinedDays.WeekDayAndWeekEnd:
                    name = "��������яT��";
                    termNames[0] = "���j";
                    for (int i = 1; i < 6; i++) termNames[i] = "����";
                    termNames[6] = "�y�j";
                    break;
                case PredefinedDays.OneWeek:
                    name = "�j����";
                    termNames[0] = "���j";
                    termNames[1] = "���j";
                    termNames[2] = "�Ηj";
                    termNames[3] = "���j";
                    termNames[4] = "�ؗj";
                    termNames[5] = "���j";
                    termNames[6] = "�y�j";
                    break;
                case PredefinedDays.AllWeek:
                    name = "�S�j��";
                    for (int i = 0; i < 7; i++) termNames[i] = "�S�j��";
                    break;
            }
        }

        #endregion//Constructor

        #region public methods

        /// <summary>�j���O���[�v���̂�ύX����</summary>
        /// <param name="dayOfWeek">�j��</param>
        /// <param name="termName">�j���O���[�v����</param>
        /// <returns>���̕ύX�����̐^�U</returns>
        public void SetTermName(DayOfWeek dayOfWeek, string termName)
        {
            termNames[(int)dayOfWeek] = termName;
            //�C�x���g�ʒm
            if (DayChangeEvent != null) DayChangeEvent(this, new DaysEventArgs(dayOfWeek, termName));
        }

        /// <summary>�j���O���[�v���̂��擾����</summary>
        /// <param name="dayOfWeek">�j��</param>
        /// <returns>�j���O���[�v����</returns>
        public string GetTermName(DayOfWeek dayOfWeek)
        {
            return termNames[(int)dayOfWeek];
        }

        /// <summary>�w��O���[�v�ɑ�����j�����X�g��Ԃ�</summary>
        /// <param name="termName">�O���[�v����</param>
        /// <returns>�w��O���[�v�ɑ�����j�����X�g</returns>
        public DayOfWeek[] GetDays(string termName)
        {
            List<DayOfWeek> dList = new List<DayOfWeek>();
            if (termNames[0] == termName) dList.Add(DayOfWeek.Sunday);
            if (termNames[1] == termName) dList.Add(DayOfWeek.Monday);
            if (termNames[2] == termName) dList.Add(DayOfWeek.Tuesday);
            if (termNames[3] == termName) dList.Add(DayOfWeek.Wednesday);
            if (termNames[4] == termName) dList.Add(DayOfWeek.Thursday);
            if (termNames[5] == termName) dList.Add(DayOfWeek.Friday);
            if (termNames[6] == termName) dList.Add(DayOfWeek.Saturday);
            return dList.ToArray();
        }

        #endregion

        #region<ITerm�C���^�[�t�F�[�X����>

        /// <summary>�j���O���[�v���̃��X�g���擾����</summary>
        /// <returns>�j���O���[�v���̃��X�g</returns>
        public string[] GetTermNames()
        {
            //�j���O���[�v���̃��X�g��ێ�
            List<string> gNames = new List<string>();
            foreach (string gName1 in termNames)
            {
                //�d���m�F
                bool hasName = false;
                foreach (string gName2 in gNames)
                {
                    //�d�����Ă���ꍇ��break;
                    if (gName2 == gName1)
                    {
                        hasName = true;
                        break;
                    }
                }
                //���o�^�̖��̂ł���Γo�^
                if (!hasName) gNames.Add(gName1);
            }
            return gNames.ToArray();
        }

        /// <summary>�j�����w�肵�ėj���O���[�v���̂��擾����</summary>
        /// <param name="dateTime">�j��</param>
        /// <returns>�j���O���[�v����</returns>
        public string GetTermName(DateTime dateTime)
        {
            return termNames[(int)dateTime.DayOfWeek];
        }

        #endregion//ITerm�C���^�[�t�F�[�X����

        #region<ICloneable�C���^�[�t�F�[�X����>

        /// <summary>DaysOfTheWeek�N���X�̕�����Ԃ�</summary>
        /// <returns>DaysOfTheWeek�N���X�̕���</returns>
        public object Clone()
        {
            Days daysOfTheWeek = (Days)this.MemberwiseClone();
            daysOfTheWeek.termNames = new string[7];
            this.termNames.CopyTo(daysOfTheWeek.termNames, 0);
            //�C�x���g������
            daysOfTheWeek.NameChangeEvent = null;
            daysOfTheWeek.DayChangeEvent = null;
            return daysOfTheWeek;
        }

        #endregion//ICloneable�C���^�[�t�F�[�X����

        #region<�V���A���C�Y�֘A�̏���>

        /// <summary>�f�V���A���C�Y�pConstructor</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected Days(SerializationInfo sInfo, StreamingContext context)
        {
            //�o�[�W�������
            double version = sInfo.GetDouble("S_Version");

            //ID
            if (1.0 < version) id = sInfo.GetInt32("ID");
            //����
            name = sInfo.GetString("Name");
            //�j����`���X�g
            termNames = (string[])sInfo.GetValue("GroupNames", typeof(string[]));
        }

        /// <summary>HvacSystem�V���A��������</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //�o�[�W�������
            info.AddValue("S_Version", S_VERSION);

            //ID
            info.AddValue("ID", id);
            //����
            info.AddValue("Name", name);
            //�j���O���[�v���̃��X�g
            info.AddValue("GroupNames", termNames);
        }

        #endregion//�V���A���C�Y�֘A�̏���

    }

    /// <summary>�j���֘A��EventArgs</summary>
    public class DaysEventArgs : EventArgs
    {

        #region<Instance variables>

        /// <summary>�ҏW�����j��</summary>
        private DayOfWeek dayOfWeek;

        /// <summary>�O���[�v����</summary>
        private string groupName;

        #endregion//Instance variables

        #region<Properties>

        /// <summary>�ҏW�����j�����擾����</summary>
        public DayOfWeek DayOfWeek
        {
            get
            {
                return dayOfWeek;
            }
        }

        /// <summary>�O���[�v���̂��擾����</summary>
        public string GroupName
        {
            get
            {
                return groupName;
            }
        }

        #endregion//Properties

        #region<Constructor>

        /// <summary>Constructor</summary>
        /// <param name="dayOfWeek">�ҏW�����j��</param>
        /// <param name="groupName">�O���[�v����</param>
        public DaysEventArgs(DayOfWeek dayOfWeek, string groupName)
        {
            this.dayOfWeek = dayOfWeek;
            this.groupName = groupName;
        }

        #endregion//Constructor

    }

    /// <summary>�ǂݎ���pDays�C���^�[�t�F�[�X</summary>
    public interface ImmutableDays : ImmutableITermStructure
    {
        /// <summary>�w��O���[�v�ɑ�����j�����X�g���擾����</summary>
        /// <param name="groupName">�O���[�v����</param>
        /// <returns>�w��O���[�v�ɑ�����j�����X�g</returns>
        DayOfWeek[] GetDays(string groupName);
    }

}
