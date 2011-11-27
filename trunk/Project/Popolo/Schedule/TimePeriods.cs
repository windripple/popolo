/* TimePeriods.cs
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
    /// <summary>���ԑуN���X</summary>
    [Serializable]
    public class TimePeriods : ITermStructure, ISerializable, ImmutableTimePeriods
    {

        #region<�萔��`>

        /// <summary>�V���A���C�Y�p�o�[�W�������</summary>
        private double S_VERSION = 1.1;

        /// <summary>�X�P�W���[����`����N�i�[�N�Ɋ֌W�j</summary>
        private const int YEAR = 2001;

        /// <summary>�X�P�W���[����`���錎</summary>
        private const int MONTH = 1;

        /// <summary>�X�P�W���[����`�����</summary>
        private const int DAY = 1;

        #endregion//�萔��`

        #region<delegate��`>

        /// <summary>���̕ύX�C�x���g�n���h��</summary>
        public delegate void NameChangeEventHandler(object sender, EventArgs e);

        /// <summary>�����ђǉ��C�x���g�n���h��</summary>
        public delegate void TimePeriodAddEventHandler(object sender, TimePeriodsEventArgs e);

        /// <summary>�����ѕύX�C�x���g�n���h��</summary>
        public delegate void TimePeriodChangeEventHandler(object sender, TimePeriodsEventArgs e);

        /// <summary>�����э폜�C�x���g�n���h��</summary>
        public delegate void TimePeriodRemoveEventHandler(object sender, TimePeriodsEventArgs e);

        #endregion//delegate��`

        #region<�C�x���g��`>

        /// <summary>���̕ύX�C�x���g</summary>
        public event NameChangeEventHandler NameChangeEvent;

        /// <summary>�����ђǉ��C�x���g</summary>
        public event TimePeriodAddEventHandler TimePeriodAddEvent;

        /// <summary>�����ѕύX�C�x���g</summary>
        public event TimePeriodChangeEventHandler TimePeriodChangeEvent;

        /// <summary>�����э폜�C�x���g</summary>
        public event TimePeriodRemoveEventHandler TimePeriodRemoveEvent;

        #endregion//�C�x���g��`

        #region<enumerators>

        /// <summary>��`�ς݂̎��ԑ�</summary>
        public enum PredefinedTimePeriods
        {
            /// <summary>�I��</summary>
            AllDay = 0,
            /// <summary>���ԕ�</summary>
            Hourly = 1,
            /// <summary>�c�Ǝ���</summary>
            BusinessHours = 2,
            /// <summary>����</summary>
            DayAndNight = 3
        }

        #endregion//enumerators

        #region<Properties>

        /// <summary>���ԑ�ID��ݒ�E�擾����</summary>
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

        /// <summary>���ԑі��̂�ݒ�E�擾����</summary>
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

        /// <summary>��`�������ԑѐ����擾����</summary>
        public int Count
        {
            get
            {
                return timePeriodNames.Count;
            }
        }

        #endregion

        #region<Instance variables>

        /// <summary>ID</summary>
        private int id;

        /// <summary>����</summary>
        private string name;

        /// <summary>���ԑі���</summary>
        private List<string> timePeriodNames = new List<string>();

        /// <summary>���ԑъJ�n�����i���̃��X�g��+1�̃��X�g�ƂȂ�j</summary>
        private List<DateTime> timePeriodStartTimes = new List<DateTime>();

        #endregion//Instance variables

        #region<Constructor>

        /// <summary>Constructor</summary>
        public TimePeriods()
        {
            //������
            timePeriodNames.Add("�I��");
            timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 0, 0, 0));
            timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY + 1, 0, 0, 0));
        }

        /// <summary>Constructor</summary>
        /// <param name="predefinedTimePeriods">��`�ς̎��ԑ�</param>
        public TimePeriods(PredefinedTimePeriods predefinedTimePeriods)
        {
            Initialize(predefinedTimePeriods);
        }

        /// <summary>��`�ς̎��ԑтŏ���������</summary>
        /// <param name="predefinedTimePeriods">��`�ς̎��ԑ�</param>
        public void Initialize(PredefinedTimePeriods predefinedTimePeriods)
        {
            timePeriodNames.Clear();
            timePeriodStartTimes.Clear();
            switch (predefinedTimePeriods)
            {
                case PredefinedTimePeriods.AllDay:
                    name = "�I��";
                    timePeriodNames.Add("�I��");
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 0, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY + 1, 0, 0, 0));
                    break;
                case PredefinedTimePeriods.Hourly:
                    name = "���ԕ�";
                    timePeriodNames.Add("0��");
                    timePeriodNames.Add("1��");
                    timePeriodNames.Add("2��");
                    timePeriodNames.Add("3��");
                    timePeriodNames.Add("4��");
                    timePeriodNames.Add("5��");
                    timePeriodNames.Add("6��");
                    timePeriodNames.Add("7��");
                    timePeriodNames.Add("8��");
                    timePeriodNames.Add("9��");
                    timePeriodNames.Add("10��");
                    timePeriodNames.Add("11��");
                    timePeriodNames.Add("12��");
                    timePeriodNames.Add("13��");
                    timePeriodNames.Add("14��");
                    timePeriodNames.Add("15��");
                    timePeriodNames.Add("16��");
                    timePeriodNames.Add("17��");
                    timePeriodNames.Add("18��");
                    timePeriodNames.Add("19��");
                    timePeriodNames.Add("20��");
                    timePeriodNames.Add("21��");
                    timePeriodNames.Add("22��");
                    timePeriodNames.Add("23��");
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 0, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 1, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 2, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 3, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 4, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 5, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 6, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 7, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 8, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 9, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 10, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 11, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 12, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 13, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 14, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 15, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 16, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 17, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 18, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 19, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 20, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 21, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 22, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 23, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY + 1, 0, 0, 0));
                    break;
                case PredefinedTimePeriods.BusinessHours:
                    name = "�c�Ǝ��ԑ�";
                    timePeriodNames.Add("��c�Ǝ���");
                    timePeriodNames.Add("�c�Ǝ���");
                    timePeriodNames.Add("��c�Ǝ���");
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 0, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 8, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 19, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY + 1, 0, 0, 0));
                    break;
                case PredefinedTimePeriods.DayAndNight:
                    name = "����";
                    timePeriodNames.Add("���");
                    timePeriodNames.Add("����");
                    timePeriodNames.Add("���");
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 0, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 7, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 23, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY + 1, 0, 0, 0));
                    break;
            }
        }

        #endregion//Constructor

        #region public methods

        /// <summary>���ԑт�ǉ�����</summary>
        /// <param name="timePeriodName">���ԑі���</param>
        /// <param name="timePeriodStartTime">���ԑъJ�n����</param>
        /// <returns>�ǉ������̐^�U�i�w�莞���Ɋ��Ɏ��ԑт���`����Ă���ꍇ�͎��s�j</returns>
        public bool AddTimePeriod(string timePeriodName, DateTime timePeriodStartTime)
        {
            DateTime dTime = new DateTime(YEAR, MONTH, DAY, timePeriodStartTime.Hour, timePeriodStartTime.Minute, timePeriodStartTime.Second);
            int sIndex = 0;
            //�K�؂Ȉʒu�ɑ}������
            for (int i = 1; i < timePeriodStartTimes.Count; i++)
            {
                int instPoint = dTime.CompareTo(timePeriodStartTimes[i]);
                if (instPoint < 0)
                {
                    sIndex = i;
                    timePeriodStartTimes.Insert(i, dTime);
                    timePeriodNames.Insert(i, timePeriodName);
                    break;
                }
                else if (instPoint == 0) return false;
            }
            //�C�x���g�ʒm
            if (TimePeriodAddEvent != null) TimePeriodAddEvent(this, new TimePeriodsEventArgs(sIndex, timePeriodName, timePeriodStartTimes[sIndex], timePeriodStartTimes[sIndex + 1]));
            return true;
        }

        /// <summary>���ԑі��̂��擾����</summary>
        /// <param name="timePeriodIndex">���ԑєԍ�</param>
        /// <returns>���ԑі���</returns>
        public string GetTimePeriodName(int timePeriodIndex)
        {
            return timePeriodNames[timePeriodIndex];
        }

        /// <summary>���ԑя����擾����</summary>
        /// <param name="timePeriodIndex">���ԑєԍ�</param>
        /// <param name="timePeriodName">���ԑі���</param>
        /// <param name="timePeriodStartTime">���ԑъJ�n����</param>
        /// <param name="timePeriodEndTime">���ԑяI������</param>
        public void GetTimePeriod(int timePeriodIndex, out string timePeriodName, out DateTime timePeriodStartTime, out DateTime timePeriodEndTime)
        {
            timePeriodName = timePeriodNames[timePeriodIndex];
            timePeriodStartTime = timePeriodStartTimes[timePeriodIndex];
            timePeriodEndTime = timePeriodStartTimes[timePeriodIndex + 1].AddMinutes(-1);
        }

        /// <summary>���ԑя����擾����</summary>
        /// <param name="timePeriodName">���ԑі���</param>
        /// <param name="timePeriodStartDTimes">���ԑъJ�n�������X�g</param>
        /// <param name="timePeriodEndDTimes">���ԑяI�����X�g</param>
        public void GetTimePeriods(string timePeriodName, out DateTime[] timePeriodStartDTimes, out DateTime[] timePeriodEndDTimes)
        {
            //�Y�����鎞�ԑт����݂��Ȃ��ꍇ
            if (!timePeriodNames.Contains(timePeriodName))
            {
                timePeriodStartDTimes = new DateTime[0];
                timePeriodEndDTimes = new DateTime[0];
                return;
            }
            //���ԑт����݂���ꍇ�͂��ׂĂ̊��Ԃ𒲂ׂ�
            List<DateTime> dtStart = new List<DateTime>();
            List<DateTime> dtEnd = new List<DateTime>();
            for (int i = 0; i < timePeriodNames.Count; i++)
            {
                if (timePeriodNames[i] == timePeriodName)
                {
                    dtStart.Add(this.timePeriodStartTimes[i]);
                    dtEnd.Add(this.timePeriodStartTimes[i + 1].AddDays(-1));
                }
            }
            //�z��
            timePeriodStartDTimes = dtStart.ToArray();
            timePeriodEndDTimes = dtEnd.ToArray();
        }

        /// <summary>���ԑт��폜����</summary>
        /// <param name="timePeriodIndex">���ԑєԍ�</param>
        public bool RemoveTimePeriod(int timePeriodIndex)
        {
            //���ԑт��������`����Ă��Ȃ��ꍇ�͎��s
            if (timePeriodNames.Count - 1 < timePeriodIndex) return false;
            else
            {
                DateTime dtStart, dtEnd;
                string sName = timePeriodNames[timePeriodIndex];
                //���ԑі��̂��폜
                timePeriodNames.RemoveAt(timePeriodIndex);
                //���ԑъJ�n�I���������X�V****************
                //�擪���ԑт̏ꍇ
                if (timePeriodIndex == 0)
                {
                    dtStart = timePeriodStartTimes[0];
                    dtEnd = timePeriodStartTimes[1];
                    timePeriodStartTimes.RemoveAt(1);
                }
                //�ŏI���ԑт̏ꍇ
                else if (timePeriodIndex == timePeriodNames.Count)
                {
                    dtStart = timePeriodStartTimes[timePeriodIndex];
                    dtEnd = timePeriodStartTimes[timePeriodIndex + 1];
                    timePeriodStartTimes.RemoveAt(timePeriodIndex);
                }
                //���̑��̎����т̏ꍇ
                else
                {
                    //���Ԏ������v�Z����
                    dtStart = timePeriodStartTimes[timePeriodIndex];
                    dtEnd = timePeriodStartTimes[timePeriodIndex + 1];
                    TimeSpan tSpan = dtEnd - dtStart;
                    DateTime dtMiddle = dtStart.AddSeconds(tSpan.Seconds / 2);
                    timePeriodStartTimes.RemoveAt(timePeriodIndex + 1);
                    timePeriodStartTimes[timePeriodIndex] = dtMiddle;
                }
                //�C�x���g�ʒm
                if (TimePeriodRemoveEvent != null) TimePeriodRemoveEvent(this, new TimePeriodsEventArgs(timePeriodIndex, sName, dtStart, dtEnd));
                //�폜����
                return true;
            }
        }

        /// <summary>���ԑі��̂�ύX����</summary>
        /// <param name="timePeriodIndex">���ԑєԍ�</param>
        /// <param name="timePeriodName">���ԑі���</param>
        /// <returns>���̕ύX�����̐^�U</returns>
        public bool ChangeTimePeriodName(int timePeriodIndex, string timePeriodName)
        {
            //���ԑєԍ��͈͊O�w��̏ꍇ�͏I��
            if (timePeriodNames.Count - 1 < timePeriodIndex) return false;
            //���ԑі��̂�ύX
            timePeriodNames[timePeriodIndex] = timePeriodName;
            //�C�x���g�ʒm
            if (TimePeriodChangeEvent != null) TimePeriodChangeEvent(this, new TimePeriodsEventArgs(timePeriodIndex, timePeriodName, timePeriodStartTimes[timePeriodIndex], timePeriodStartTimes[timePeriodIndex + 1]));
            return true;
        }

        /// <summary>���ԑђ[��������ύX����</summary>
        /// <param name="timePeriodIndex">���ԑєԍ�</param>
        /// <param name="timePeriodDateTime">���ԑђ[������</param>
        /// <param name="isStartDateTime">���ԑъJ�n�����̐ݒ肩�ۂ�</param>
        /// <returns>���ԑђ[��������ύX�����̐^�U</returns>
        public bool ChangeTimePeriodDateTime(int timePeriodIndex, DateTime timePeriodDateTime, bool isStartDateTime)
        {
            //���ԑъJ�n�����̏ꍇ
            if (isStartDateTime)
            {
                //�ŏ��̎��ԑы�؂�DateTime�̏ꍇ�͏I��
                if (timePeriodNames.Count - 1 < timePeriodIndex || timePeriodIndex <= 0) return false;
                //���ԑѕύX�\�͈͊O�̏ꍇ�͏I��
                if (timePeriodDateTime <= timePeriodStartTimes[timePeriodIndex - 1] || timePeriodStartTimes[timePeriodIndex + 1] <= timePeriodDateTime) return false;
                //���ԑъJ�n������ύX����
                timePeriodStartTimes[timePeriodIndex] = timePeriodDateTime;
            }
            //���ԑяI�������̏ꍇ
            else
            {
                //�Ō�̎��ԑы�؂�DateTime�̏ꍇ�͏I��
                if (timePeriodNames.Count - 2 < timePeriodIndex || timePeriodIndex < 0) return false;
                //���ԑѕύX�\�͈͊O�̏ꍇ�͏I��
                timePeriodDateTime = timePeriodDateTime.AddMinutes(1);
                if (timePeriodDateTime <= timePeriodStartTimes[timePeriodIndex] || timePeriodStartTimes[timePeriodIndex + 2] <= timePeriodDateTime) return false;
                //���ԑяI��������ύX����
                timePeriodStartTimes[timePeriodIndex + 1] = timePeriodDateTime;
            }
            
            //�C�x���g�ʒm
            if (TimePeriodChangeEvent != null) TimePeriodChangeEvent(this, new TimePeriodsEventArgs(timePeriodIndex, timePeriodNames[timePeriodIndex], timePeriodStartTimes[timePeriodIndex], timePeriodStartTimes[timePeriodIndex + 1]));
            return true;
        }

        #endregion

        #region<ITerm�C���^�[�t�F�[�X����>

        /// <summary>���ԑі��̃��X�g���擾����</summary>
        /// <returns>���ԑі��̃��X�g</returns>
        public string[] GetTermNames()
        {
            //���ԑі��̃��X�g��ێ�
            List<string> sNames = new List<string>();
            foreach (string sName1 in timePeriodNames)
            {
                //�d���m�F
                bool hasName = false;
                foreach (string sName2 in sNames)
                {
                    //�d�����Ă���ꍇ��break;
                    if (sName2 == sName1)
                    {
                        hasName = true;
                        break;
                    }
                }
                //���o�^�̖��̂ł���Γo�^
                if (!hasName) sNames.Add(sName1);
            }
            return sNames.ToArray();
        }

        /// <summary>�������w�肵�Ď��ԑі��̂��擾����</summary>
        /// <param name="dateTime">����</param>
        /// <returns>���ԑі���</returns>
        public string GetTermName(DateTime dateTime)
        {
            DateTime dt = new DateTime(YEAR, MONTH, DAY, dateTime.Hour, dateTime.Minute, dateTime.Second);
            for (int i = 0; i < timePeriodStartTimes.Count; i++)
            {
                if (dt < timePeriodStartTimes[i]) return timePeriodNames[i - 1];
            }
            throw new Exception("�X�P�W���[����`�͈͊O");
        }

        #endregion//ITerm�C���^�[�t�F�[�X����

        #region<ICloneable�C���^�[�t�F�[�X����>

        /// <summary>TimePeriods�N���X�̕�����Ԃ�</summary>
        /// <returns>TimePeriods�N���X�̕���</returns>
        public object Clone()
        {
            TimePeriods timePeriods = (TimePeriods)this.MemberwiseClone();
            timePeriods.timePeriodNames = new List<string>();
            timePeriods.timePeriodStartTimes = new List<DateTime>();
            foreach (string sName in timePeriodNames) timePeriods.timePeriodNames.Add(sName);
            foreach (DateTime dTime in timePeriodStartTimes) timePeriods.timePeriodStartTimes.Add(dTime);
            //�C�x���g��������
            timePeriods.NameChangeEvent = null;
            timePeriods.TimePeriodAddEvent = null;
            timePeriods.TimePeriodChangeEvent = null;
            timePeriods.TimePeriodRemoveEvent = null;
            return timePeriods;
        }

        #endregion//ICloneable�C���^�[�t�F�[�X����

        #region<�V���A���C�Y�֘A�̏���>

        /// <summary>�f�V���A���C�Y�pConstructor</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected TimePeriods(SerializationInfo sInfo, StreamingContext context)
        {
            //�o�[�W�������
            double version = sInfo.GetDouble("S_Version");

            //ID
            if (1.0 < version) id = sInfo.GetInt32("ID");
            //����
            name = sInfo.GetString("Name");
            //���ԑі��̃��X�g
            timePeriodNames.AddRange((string[])sInfo.GetValue("TimePeriodNames", typeof(string[])));
            //���ԑъJ�n�������X�g
            timePeriodStartTimes.AddRange((DateTime[])sInfo.GetValue("TimePeriodStartTimes", typeof(DateTime[])));    
        }

        /// <summary>TimePeriods�V���A��������</summary>
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
            //���ԑі��̃��X�g
            info.AddValue("TimePeriodNames", timePeriodNames.ToArray());
            //���ԑъJ�n�������X�g
            info.AddValue("TimePeriodStartTimes", timePeriodStartTimes.ToArray());
        }

        #endregion//�V���A���C�Y�֘A�̏���

    }

    /// <summary>���ԑъ֘A��EventArgs</summary>
    public class TimePeriodsEventArgs : EventArgs
    {

        #region<Instance variables>

        /// <summary>���ԑєԍ�</summary>
        private int timePeriodIndex;

        /// <summary>���ԑі���</summary>
        private string timePeriodName;

        /// <summary>���ԑъJ�n����</summary>
        private DateTime timePeriodStart;

        /// <summary>���ԑяI������</summary>
        private DateTime timePeriodEnd;

        #endregion//Instance variables

        #region<Properties>

        /// <summary>���ԑєԍ����擾����</summary>
        public int TimePeriodIndex
        {
            get
            {
                return timePeriodIndex;
            }
        }

        /// <summary>���ԑі��̂��擾����</summary>
        public string TimePeriodName
        {
            get
            {
                return timePeriodName;
            }
        }

        /// <summary>���ԑъJ�n�������擾����</summary>
        public DateTime TimePeriodStart
        {
            get
            {
                return timePeriodStart;
            }
        }

        /// <summary>���ԑяI���������擾����</summary>
        public DateTime TimePeriodEnd
        {
            get
            {
                return timePeriodEnd;
            }
        }

        #endregion//Properties

        #region<Constructor>

        /// <summary>Constructor</summary>
        /// <param name="timePeriodIndex">���ԑєԍ�</param>
        /// <param name="timePeriodName">���ԑі���</param>
        /// <param name="timePeriodStart">���ԑъJ�n����</param>
        /// <param name="timePeriodEnd">���ԑяI������</param>
        public TimePeriodsEventArgs(int timePeriodIndex, string timePeriodName, DateTime timePeriodStart, DateTime timePeriodEnd)
        {
            this.timePeriodIndex = timePeriodIndex;
            this.timePeriodName = timePeriodName;
            this.timePeriodStart = timePeriodStart;
            this.timePeriodEnd = timePeriodEnd;
        }

        #endregion//Constructor

    }

    /// <summary>�ǂݎ���pTimePeriods�C���^�[�t�F�[�X</summary>
    public interface ImmutableTimePeriods : ImmutableITermStructure
    {
        /// <summary>��`�������ԑѐ����擾����</summary>
        int Count
        {
            get;
        }

        /// <summary>���ԑі��̂��擾����</summary>
        /// <param name="timePeriodIndex">���ԑєԍ�</param>
        /// <returns>���ԑі���</returns>
        string GetTimePeriodName(int timePeriodIndex);

        /// <summary>���ԑя����擾����</summary>
        /// <param name="timePeriodIndex">���ԑєԍ�</param>
        /// <param name="timePeriodName">���ԑі���</param>
        /// <param name="timePeriodStartTime">���ԑъJ�n����</param>
        /// <param name="timePeriodEndTime">���ԑяI������</param>
        void GetTimePeriod(int timePeriodIndex, out string timePeriodName, out DateTime timePeriodStartTime, out DateTime timePeriodEndTime);

        /// <summary>���ԑя����擾����</summary>
        /// <param name="timePeriodName">���ԑі���</param>
        /// <param name="timePeriodStartDTimes">���ԑъJ�n�������X�g</param>
        /// <param name="timePeriodEndDTimes">���ԑяI�����X�g</param>
        void GetTimePeriods(string timePeriodName, out DateTime[] timePeriodStartDTimes,
            out DateTime[] timePeriodEndDTimes);

    }

}
