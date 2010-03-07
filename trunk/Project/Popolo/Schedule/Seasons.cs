/* Seasons.cs
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
    /// <summary>�G�߃N���X</summary>
    [Serializable]
    public class Seasons : ITermStructure, ISerializable, ImmutableSeasons
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

        /// <summary>�G�ߒǉ��C�x���g�n���h��</summary>
        public delegate void SeasonAddEventHandler(object sender, SeasonsEventArgs e);

        /// <summary>�G�ߕύX�C�x���g�n���h��</summary>
        public delegate void SeasonChangeEventHandler(object sender, SeasonsEventArgs e);

        /// <summary>�G�ߍ폜�C�x���g�n���h��</summary>
        public delegate void SeasonRemoveEventHandler(object sender, SeasonsEventArgs e);

        #endregion//delegate��`

        #region<�C�x���g��`>

        /// <summary>���̕ύX�C�x���g</summary>
        public event NameChangeEventHandler NameChangeEvent;

        /// <summary>�G�ߒǉ��C�x���g</summary>
        public event SeasonAddEventHandler SeasonAddEvent;

        /// <summary>�G�ߕύX�C�x���g</summary>
        public event SeasonChangeEventHandler SeasonChangeEvent;

        /// <summary>�G�ߍ폜�C�x���g</summary>
        public event SeasonRemoveEventHandler SeasonRemoveEvent;

        #endregion//�C�x���g��`

        #region �񋓌^��`

        /// <summary>��`�ς݂̋G��</summary>
        public enum PredefinedSeasons
        {
            /// <summary>�N��</summary>
            AllYear = 0,
            /// <summary>�l�G</summary>
            FourSeasons = 1,
            /// <summary>�M���׌v�Z�p</summary>
            HeatLoadClassification = 2,
            /// <summary>�����̏j��</summary>
            Holiday = 3
        }

        #endregion

        #region �v���p�e�B

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

        /// <summary>��`�����G�ߐ����擾����</summary>
        public int Count
        {
            get
            {
                return seasonNames.Count;
            }
        }

        #endregion

        #region �C���X�^���X�ϐ�

        /// <summary>ID</summary>
        private int id;

        /// <summary>����</summary>
        private string name;

        /// <summary>�G�ߖ��̃��X�g</summary>
        private List<string> seasonNames = new List<string>();

        /// <summary>�G�ߊJ�n�������X�g�i���̃��X�g��+1�̃��X�g�ƂȂ�j</summary>
        private List<DateTime> seasonStartDTimes = new List<DateTime>();

        #endregion

        #region �R���X�g���N�^

        /// <summary>�R���X�g���N�^</summary>
        public Seasons()
        {
            //������
            name = "���̖��ݒ�̋G�ߒ�`";
            seasonNames.Add("�N��");
            seasonStartDTimes.Add(new DateTime(YEAR, 1, 1));
            seasonStartDTimes.Add(new DateTime(YEAR + 1, 1, 1));
        }

        /// <summary>�R���X�g���N�^</summary>
        /// <param name="predefinedSeasons">��`�ς݂̋G��</param>
        public Seasons(PredefinedSeasons predefinedSeasons)
        {
            Initialize(predefinedSeasons);
        }

        /// <summary>��`�ς̋G�߂ŏ���������</summary>
        /// <param name="predefinedSeasons">��`�ς݂̋G��</param>
        public void Initialize(PredefinedSeasons predefinedSeasons)
        {
            seasonNames.Clear();
            seasonStartDTimes.Clear();
            switch (predefinedSeasons)
            {
                case PredefinedSeasons.FourSeasons:
                    name = "�l�G";
                    seasonNames.Add("�~");
                    seasonNames.Add("�t");
                    seasonNames.Add("��");
                    seasonNames.Add("�H");
                    seasonNames.Add("�~");
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 3, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 6, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 9, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 12, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR + 1, 1, 1));
                    break;
                case PredefinedSeasons.HeatLoadClassification:
                    name = "�M���וʋG��";
                    seasonNames.Add("�~�G");
                    seasonNames.Add("���ԋG");
                    seasonNames.Add("�ċG");
                    seasonNames.Add("���ԋG");
                    seasonNames.Add("�~�G");
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 3, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 6, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 9, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 12, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR + 1, 1, 1));
                    break;
                case PredefinedSeasons.AllYear:
                    name = "�N��";
                    seasonNames.Add("�N��");
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR + 1, 1, 1));
                    break;
                case PredefinedSeasons.Holiday:
                    name = "�����̏j��";
                    seasonNames.Add("�j��");
                    seasonNames.Add("��ʓ�");
                    seasonNames.Add("�j��");
                    seasonNames.Add("��ʓ�");
                    seasonNames.Add("�j��");
                    seasonNames.Add("��ʓ�");
                    seasonNames.Add("�j��");
                    seasonNames.Add("��ʓ�");
                    seasonNames.Add("�j��");
                    seasonNames.Add("��ʓ�");
                    seasonNames.Add("�j��");
                    seasonNames.Add("��ʓ�");
                    seasonNames.Add("�j��");
                    seasonNames.Add("��ʓ�");
                    seasonNames.Add("�j��");
                    seasonNames.Add("��ʓ�");
                    seasonNames.Add("�j��");
                    seasonNames.Add("��ʓ�");
                    seasonNames.Add("�j��");
                    seasonNames.Add("��ʓ�");
                    seasonNames.Add("�j��");
                    seasonNames.Add("��ʓ�");
                    seasonNames.Add("�j��");
                    seasonNames.Add("��ʓ�");
                    seasonNames.Add("�j��");
                    seasonNames.Add("��ʓ�");
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 2));
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 15));
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 16));
                    seasonStartDTimes.Add(new DateTime(YEAR, 2, 11));
                    seasonStartDTimes.Add(new DateTime(YEAR, 2, 12));
                    seasonStartDTimes.Add(new DateTime(YEAR, 3, 21));
                    seasonStartDTimes.Add(new DateTime(YEAR, 3, 22));
                    seasonStartDTimes.Add(new DateTime(YEAR, 4, 29));
                    seasonStartDTimes.Add(new DateTime(YEAR, 4, 30));
                    seasonStartDTimes.Add(new DateTime(YEAR, 5, 3));
                    seasonStartDTimes.Add(new DateTime(YEAR, 5, 6));
                    seasonStartDTimes.Add(new DateTime(YEAR, 7, 20));
                    seasonStartDTimes.Add(new DateTime(YEAR, 7, 21));
                    seasonStartDTimes.Add(new DateTime(YEAR, 9, 15));
                    seasonStartDTimes.Add(new DateTime(YEAR, 9, 16));
                    seasonStartDTimes.Add(new DateTime(YEAR, 9, 21));
                    seasonStartDTimes.Add(new DateTime(YEAR, 9, 22));
                    seasonStartDTimes.Add(new DateTime(YEAR, 10, 10));
                    seasonStartDTimes.Add(new DateTime(YEAR, 10, 11));
                    seasonStartDTimes.Add(new DateTime(YEAR, 11, 3));
                    seasonStartDTimes.Add(new DateTime(YEAR, 11, 4));
                    seasonStartDTimes.Add(new DateTime(YEAR, 11, 23));
                    seasonStartDTimes.Add(new DateTime(YEAR, 11, 24));
                    seasonStartDTimes.Add(new DateTime(YEAR, 12, 23));
                    seasonStartDTimes.Add(new DateTime(YEAR, 12, 24));
                    seasonStartDTimes.Add(new DateTime(YEAR + 1, 1, 1));
                    break;
            }
        }

        #endregion

        #region<public���\�b�h>

        /// <summary>�G�߂�ǉ�����</summary>
        /// <param name="seasonName">�G�ߖ���</param>
        /// <param name="seasonStartDTime">�G�ߊJ�n����</param>
        /// <returns>�ǉ������̐^�U�i�w�茎���Ɋ��ɋG�߂���`����Ă���ꍇ�͎��s�j</returns>
        public bool AddSeason(string seasonName, DateTime seasonStartDTime)
        {
            DateTime dTime = new DateTime(YEAR, seasonStartDTime.Month, seasonStartDTime.Day);
            int sIndex = 0;
            //�K�؂Ȉʒu�ɑ}������
            for (int i = 1; i < seasonStartDTimes.Count; i++)
            {
                int instPoint = dTime.CompareTo(seasonStartDTimes[i]);
                if (instPoint < 0)
                {
                    sIndex = i;
                    seasonStartDTimes.Insert(i, dTime);
                    seasonNames.Insert(i, seasonName);
                    break;
                }
                else if (instPoint == 0) return false;
            }
            //�C�x���g�ʒm
            if (SeasonAddEvent != null) SeasonAddEvent(this, new SeasonsEventArgs(sIndex, seasonName, seasonStartDTimes[sIndex], seasonStartDTimes[sIndex + 1]));
            return true;
        }

        /// <summary>�G�ߖ��̂��擾����</summary>
        /// <param name="seasonIndex">�G�ߔԍ�</param>
        /// <returns>�G�ߖ���</returns>
        public string GetSeasonName(int seasonIndex)
        {
            return seasonNames[seasonIndex];
        }

        /// <summary>�G�ߏ����擾����</summary>
        /// <param name="seasonIndex">�G�ߔԍ�</param>
        /// <param name="seasonName">�G�ߖ���</param>
        /// <param name="seasonStartDTime">�G�ߊJ�n����</param>
        /// <param name="seasonEndDTime">�G�ߏI������</param>
        public void GetSeason(int seasonIndex, out string seasonName, out DateTime seasonStartDTime, out DateTime seasonEndDTime)
        {
            seasonName = seasonNames[seasonIndex];
            seasonStartDTime = seasonStartDTimes[seasonIndex];
            seasonEndDTime = seasonStartDTimes[seasonIndex + 1].AddDays(-1);
        }

        /// <summary>�G�ߏ����擾����</summary>
        /// <param name="seasonName">�G�ߖ���</param>
        /// <param name="seasonStartDTimes">�G�ߊJ�n�������X�g</param>
        /// <param name="seasonEndDTimes">�G�ߏI���������X�g</param>
        public void GetSeasons(string seasonName, out DateTime[] seasonStartDTimes, out DateTime[] seasonEndDTimes)
        {
            //�Y������G�߂����݂��Ȃ��ꍇ
            if (!seasonNames.Contains(seasonName))
            {
                seasonStartDTimes = new DateTime[0];
                seasonEndDTimes = new DateTime[0];
                return;
            }
            //�G�߂����݂���ꍇ�͂��ׂĂ̊��Ԃ𒲂ׂ�
            List<DateTime> dtStart = new List<DateTime>();
            List<DateTime> dtEnd = new List<DateTime>();
            for (int i = 0; i < seasonNames.Count; i++)
            {
                if (seasonNames[i] == seasonName)
                {
                    dtStart.Add(this.seasonStartDTimes[i]);
                    dtEnd.Add(this.seasonStartDTimes[i + 1].AddDays(-1));
                }
            }
            //�z��
            seasonStartDTimes = dtStart.ToArray();
            seasonEndDTimes = dtEnd.ToArray();
        }

        /// <summary>�G�߂��폜����</summary>
        /// <param name="seasonIndex">�G�ߔԍ�</param>
        public bool RemoveSeason(int seasonIndex)
        {
            //�G�߂��������`����Ă��Ȃ��ꍇ�͎��s
            if (seasonNames.Count - 1 < seasonIndex) return false;
            else
            {
                DateTime dtStart, dtEnd;
                string sName = seasonNames[seasonIndex];
                //�G�ߖ��̂��폜
                seasonNames.RemoveAt(seasonIndex);
                //�G�ߊJ�n�I���������X�V****************
                //�擪�G�߂̏ꍇ
                if (seasonIndex == 0)
                {
                    dtStart = seasonStartDTimes[0];
                    dtEnd = seasonStartDTimes[1];
                    seasonStartDTimes.RemoveAt(1);
                }
                //�ŏI�G�߂̏ꍇ
                else if (seasonIndex == seasonNames.Count)
                {
                    dtStart = seasonStartDTimes[seasonIndex];
                    dtEnd = seasonStartDTimes[seasonIndex + 1];
                    seasonStartDTimes.RemoveAt(seasonIndex);
                }
                //���̑��̋G�߂̏ꍇ
                else
                {
                    //���ԓ����v�Z����
                    dtStart = seasonStartDTimes[seasonIndex];
                    dtEnd = seasonStartDTimes[seasonIndex + 1];
                    TimeSpan tSpan = dtEnd - dtStart;
                    DateTime dtMiddle = dtStart.AddDays(tSpan.Days / 2);
                    seasonStartDTimes.RemoveAt(seasonIndex + 1);
                    seasonStartDTimes[seasonIndex] = dtMiddle;
                }
                //�C�x���g�ʒm
                if (SeasonRemoveEvent != null) SeasonRemoveEvent(this, new SeasonsEventArgs(seasonIndex, sName, dtStart, dtEnd));
                //�폜����
                return true;
            }
        }

        /// <summary>�G�ߖ��̂�ύX����</summary>
        /// <param name="seasonIndex">�G�ߔԍ�</param>
        /// <param name="seasonName">�G�ߖ���</param>
        /// <returns>���̕ύX�����̐^�U</returns>
        public bool ChangeSeasonName(int seasonIndex, string seasonName)
        {
            //�G�ߔԍ��͈͊O�w��̏ꍇ�͏I��
            if (seasonNames.Count - 1 < seasonIndex) return false;
            //�G�ߖ��̂�ύX
            seasonNames[seasonIndex] = seasonName;
            //�C�x���g�ʒm
            if (SeasonChangeEvent != null) SeasonChangeEvent(this, new SeasonsEventArgs(seasonIndex, seasonName, seasonStartDTimes[seasonIndex], seasonStartDTimes[seasonIndex + 1]));
            return true;
        }

        /// <summary>�G�ߒ[��������ύX����</summary>
        /// <param name="seasonIndex">�G�ߔԍ�</param>
        /// <param name="seasonDateTime">�G�ߒ[������</param>
        /// <param name="isStartDateTime">�G�ߊJ�n�����̐ݒ肩�ۂ�</param>
        /// <returns>�G�ߒ[��������ύX�����̐^�U</returns>
        public bool ChangeSeasonDateTime(int seasonIndex, DateTime seasonDateTime, bool isStartDateTime)
        {
            //�G�ߊJ�n�����̏ꍇ
            if (isStartDateTime)
            {
                //�ŏ��̋G�ߋ�؂�DateTime�̏ꍇ�͏I��
                if (seasonNames.Count - 1 < seasonIndex || seasonIndex <= 0) return false;
                //�G�ߕύX�\�͈͊O�̏ꍇ�͏I��
                if (seasonDateTime <= seasonStartDTimes[seasonIndex - 1] || seasonStartDTimes[seasonIndex + 1] <= seasonDateTime) return false;
                //�G�ߊJ�n������ύX����
                seasonStartDTimes[seasonIndex] = seasonDateTime;
            }
            //�G�ߏI�������̏ꍇ
            else
            {
                //�Ō�̋G�ߋ�؂�DateTime�̏ꍇ�͏I��
                if (seasonNames.Count - 2 < seasonIndex || seasonIndex < 0) return false;
                //�G�ߕύX�\�͈͊O�̏ꍇ�͏I��
                seasonDateTime = seasonDateTime.AddDays(1);
                if (seasonDateTime <= seasonStartDTimes[seasonIndex] || seasonStartDTimes[seasonIndex + 2] <= seasonDateTime) return false;
                //�G�ߏI��������ύX����
                seasonStartDTimes[seasonIndex + 1] = seasonDateTime;
            }
            //�C�x���g�ʒm
            if (SeasonChangeEvent != null) SeasonChangeEvent(this, new SeasonsEventArgs(seasonIndex, seasonNames[seasonIndex], seasonStartDTimes[seasonIndex], seasonStartDTimes[seasonIndex + 1]));
            return true;
        }

        #endregion//public���\�b�h

        #region<ITerm�C���^�[�t�F�[�X����>

        /// <summary>�G�ߖ��̃��X�g���擾����</summary>
        /// <returns>�G�ߖ��̃��X�g</returns>
        public string[] GetTermNames()
        {
            //�G�ߖ��̃��X�g��ێ�
            List<string> sNames = new List<string>();
            foreach (string sName1 in seasonNames)
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

        /// <summary>���t���w�肵�ċG�ߖ��̂��擾����</summary>
        /// <param name="dateTime">���t</param>
        /// <returns>�G�ߖ���</returns>
        public string GetTermName(DateTime dateTime)
        {
            DateTime dt = new DateTime(YEAR, dateTime.Month, dateTime.Day);
            for (int i = 0; i < seasonStartDTimes.Count; i++)
            {
                if (dt < seasonStartDTimes[i]) return seasonNames[i - 1];
            }
            throw new Exception("�X�P�W���[����`�͈͊O");
        }

        #endregion//ITerm�C���^�[�t�F�[�X����

        #region<ICloneable�C���^�[�t�F�[�X����>

        /// <summary>Seasons�N���X�̕�����Ԃ�</summary>
        /// <returns>Seasons�N���X�̕���</returns>
        public object Clone()
        {
            Seasons seasons = (Seasons)this.MemberwiseClone();
            seasons.seasonNames = new List<string>();
            seasons.seasonStartDTimes = new List<DateTime>();
            foreach (string sName in seasonNames) seasons.seasonNames.Add(sName);
            foreach (DateTime dTime in seasonStartDTimes) seasons.seasonStartDTimes.Add(dTime);
            //�C�x���g������
            seasons.NameChangeEvent = null;
            seasons.SeasonAddEvent = null;
            seasons.SeasonChangeEvent = null;
            seasons.SeasonRemoveEvent = null;
            return seasons;
        }

        #endregion//ICloneable�C���^�[�t�F�[�X����

        #region<�V���A���C�Y�֘A�̏���>

        /// <summary>�f�V���A���C�Y�p�R���X�g���N�^</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected Seasons(SerializationInfo sInfo, StreamingContext context)
        {
            //�o�[�W�������
            double version = sInfo.GetDouble("S_Version");

            //ID
            if (1.0 < version) id = sInfo.GetInt32("ID");
            //����
            name = sInfo.GetString("Name");
            //�G�ߖ��̃��X�g
            seasonNames.AddRange((string[])sInfo.GetValue("SeasonNames", typeof(string[])));
            //�G�ߊJ�n�N�������X�g
            seasonStartDTimes.AddRange((DateTime[])sInfo.GetValue("SeasonStartDTimes", typeof(DateTime[])));    
        }

        /// <summary>Seasons�V���A��������</summary>
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
            //�G�ߖ��̃��X�g
            info.AddValue("SeasonNames", seasonNames.ToArray(), typeof(string[]));
            //�G�ߊJ�n�N�������X�g
            info.AddValue("SeasonStartDTimes", seasonStartDTimes.ToArray(), typeof(DateTime[]));
        }

        #endregion//�V���A���C�Y�֘A�̏���

    }

    /// <summary>�G�ߊ֘A��EventArgs</summary>
    public class SeasonsEventArgs : EventArgs
    {

        #region<�C���X�^���X�ϐ�>

        /// <summary>�G�ߔԍ�</summary>
        private int seasonIndex;

        /// <summary>�G�ߖ���</summary>
        private string seasonName;

        /// <summary>�G�ߊJ�n����</summary>
        private DateTime seasonStart;

        /// <summary>�G�ߏI������</summary>
        private DateTime seasonEnd;

        #endregion//�C���X�^���X�ϐ�

        #region<�v���p�e�B>

        /// <summary>�G�ߔԍ����擾����</summary>
        public int SeasonIndex
        {
            get
            {
                return seasonIndex;
            }
        }

        /// <summary>�G�ߖ��̂��擾����</summary>
        public string SeasonName
        {
            get
            {
                return seasonName;
            }
        }

        /// <summary>�G�ߊJ�n�������擾����</summary>
        public DateTime SeasonStart
        {
            get
            {
                return seasonStart;
            }
        }

        /// <summary>�G�ߏI���������擾����</summary>
        public DateTime SeasonEnd
        {
            get
            {
                return seasonEnd;
            }
        }

        #endregion//�v���p�e�B

        #region<�R���X�g���N�^>

        /// <summary>�R���X�g���N�^</summary>
        /// <param name="seasonIndex">�G�ߔԍ�</param>
        /// <param name="seasonName">�G�ߖ���</param>
        /// <param name="seasonStart">�G�ߊJ�n����</param>
        /// <param name="seasonEnd">�G�ߏI������</param>
        public SeasonsEventArgs(int seasonIndex, string seasonName, DateTime seasonStart, DateTime seasonEnd)
        {
            this.seasonIndex = seasonIndex;
            this.seasonName = seasonName;
            this.seasonStart = seasonStart;
            this.seasonEnd = seasonEnd;
        }

        #endregion//�R���X�g���N�^

    }

    /// <summary>�ǂݎ���pSeasons�C���^�[�t�F�[�X</summary>
    public interface ImmutableSeasons : ImmutableITermStructure
    {
        /// <summary>��`�������ԑѐ����擾����</summary>
        int Count
        {
            get;
        }

        /// <summary>�G�ߖ��̂��擾����</summary>
        /// <param name="seasonIndex">�G�ߔԍ�</param>
        /// <returns>�G�ߖ���</returns>
        string GetSeasonName(int seasonIndex);

        /// <summary>�G�ߏ����擾����</summary>
        /// <param name="seasonIndex">�G�ߔԍ�</param>
        /// <param name="seasonName">�G�ߖ���</param>
        /// <param name="seasonStartDTime">�G�ߊJ�n����</param>
        /// <param name="seasonEndDTime">�G�ߏI������</param>
        void GetSeason(int seasonIndex, out string seasonName, out DateTime seasonStartDTime, out DateTime seasonEndDTime);

        /// <summary>�G�ߏ����擾����</summary>
        /// <param name="seasonName">�G�ߖ���</param>
        /// <param name="seasonStartDTimes">�G�ߊJ�n�������X�g</param>
        /// <param name="seasonEndDTimes">�G�ߏI���������X�g</param>
        void GetSeasons(string seasonName, out DateTime[] seasonStartDTimes, out DateTime[] seasonEndDTimes);

    }

}
