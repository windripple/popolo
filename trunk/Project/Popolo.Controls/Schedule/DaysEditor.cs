/* DaysEditor.cs
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Popolo.Schedule
{
    /// <summary>�j����`�ҏW�R���g���[��</summary>
    public partial class DaysEditor : UserControl
    {

        #region<�C���X�^���X�ϐ�>

        /// <summary>�ҏW����Seasons�I�u�W�F�N�g</summary>
        private Days days;

        /// <summary>�����������ۂ��̃t���O</summary>
        private bool initializing = false;

        /// <summary>�ҏW�\���ۂ��̃t���O</summary>
        private bool editable = true;

        #endregion//�C���X�^���X�ϐ�

        #region<�v���p�e�B>

        /// <summary>�ҏW����Days�I�u�W�F�N�g���擾����</summary>
        public Days EdittingDays
        {
            get
            {
                return days;
            }
        }

        /// <summary>�ҏW�\���ۂ����擾�E�ݒ肷��</summary>
        public bool Editable
        {
            get
            {
                return editable;
            }
            set
            {
                editable = value;
                updateControl();
            }
        }

        #endregion//�v���p�e�B

        #region<�R���X�g���N�^>

        /// <summary>�R���X�g���N�^</summary>
        public DaysEditor()
        {
            InitializeComponent();
        }

        #endregion//�R���X�g���N�^

        #region<Days�ݒ胁�\�b�h>

        /// <summary>Days�I�u�W�F�N�g��ݒ肷��</summary>
        /// <param name="days">Days�I�u�W�F�N�g</param>
        public void SetDays(Days days)
        {
            //���ݕҏW����Days�I�u�W�F�N�g������ꍇ�̓C�x���g�ʒm������
            if (this.days != null)
            {
                this.days.NameChangeEvent -= new Days.NameChangeEventHandler(days_NameChangeEvent);
                this.days.DayChangeEvent -= new Days.DayChangeEventHandler(days_DayChangeEvent);
            }

            this.days = days;

            //�C�x���g�o�^
            days.NameChangeEvent += new Days.NameChangeEventHandler(days_NameChangeEvent);
            days.DayChangeEvent += new Days.DayChangeEventHandler(days_DayChangeEvent);

            //�R���g���[���j�����ɃC�x���g�ʒm������
            this.Disposed += delegate(object sender, EventArgs e)
            {
                days.NameChangeEvent -= new Days.NameChangeEventHandler(days_NameChangeEvent);
                days.DayChangeEvent -= new Days.DayChangeEventHandler(days_DayChangeEvent);
            };

            //�������������s��
            bool initFlag = initializing;
            initializing = true;

            //���̂�ݒ�
            tbxDaysName.Text = days.Name;

            //TextBox�ɗj���O���[�v��ݒ�
            tbxSunday.Text = days.GetTermName(DayOfWeek.Sunday);
            tbxMonday.Text = days.GetTermName(DayOfWeek.Monday);
            tbxTuesday.Text = days.GetTermName(DayOfWeek.Tuesday);
            tbxWednesday.Text = days.GetTermName(DayOfWeek.Wednesday);
            tbxThursday.Text = days.GetTermName(DayOfWeek.Thursday);
            tbxFriday.Text = days.GetTermName(DayOfWeek.Friday);
            tbxSaturday.Text = days.GetTermName(DayOfWeek.Saturday);

            initializing = initFlag;
        }

        /// <summary>���̕ύX�C�x���g�������̏���</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void days_NameChangeEvent(object sender, EventArgs e)
        {
            tbxDaysName.Text = days.Name;
        }

        /// <summary>�j���O���[�v�ύX�C�x���g�������̏���</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void days_DayChangeEvent(object sender, DaysEventArgs e)
        {
            switch (e.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    tbxSunday.Text = e.GroupName;
                    break;
                case DayOfWeek.Monday:
                    tbxMonday.Text = e.GroupName;
                    break;
                case DayOfWeek.Tuesday:
                    tbxTuesday.Text = e.GroupName;
                    break;
                case DayOfWeek.Wednesday:
                    tbxWednesday.Text = e.GroupName;
                    break;
                case DayOfWeek.Thursday:
                    tbxThursday.Text = e.GroupName;
                    break;
                case DayOfWeek.Friday:
                    tbxFriday.Text = e.GroupName;
                    break;
                case DayOfWeek.Saturday:
                    tbxSaturday.Text = e.GroupName;
                    break;
            }
        }

        #endregion//Days�ݒ胁�\�b�h

        #region<�C�x���g�������̏���>

        /// <summary>Days���̃e�L�X�g�{�b�N�X�ύX�C�x���g�������̏���</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxDaysName_TextChanged(object sender, EventArgs e)
        {
            days.Name = tbxDaysName.Text;
        }

        /// <summary>�O���[�v���̃e�L�X�g�{�b�N�X�ύX�C�x���g�������̏���</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxGroupName_TextChanged(object sender, EventArgs e)
        {
            TextBox tbx = (TextBox)sender;
            if (ReferenceEquals(tbx, tbxSunday))
            {
                days.SetTermName(DayOfWeek.Sunday, tbx.Text);
            }
            else if (ReferenceEquals(tbx, tbxMonday))
            {
                days.SetTermName(DayOfWeek.Monday, tbx.Text);
            }
            else if (ReferenceEquals(tbx, tbxTuesday))
            {
                days.SetTermName(DayOfWeek.Tuesday, tbx.Text);
            }
            else if (ReferenceEquals(tbx, tbxWednesday))
            {
                days.SetTermName(DayOfWeek.Wednesday, tbx.Text);
            }
            else if (ReferenceEquals(tbx, tbxThursday))
            {
                days.SetTermName(DayOfWeek.Thursday, tbx.Text);
            }
            else if (ReferenceEquals(tbx, tbxFriday))
            {
                days.SetTermName(DayOfWeek.Friday, tbx.Text);
            }
            else if (ReferenceEquals(tbx, tbxSaturday))
            {
                days.SetTermName(DayOfWeek.Saturday, tbx.Text);
            }
        }

        #endregion//�C�x���g�������̏���

        /// <summary>�R���g���[�����X�V����</summary>
        private void updateControl()
        {
            if (editable)
            {
                tbxDaysName.ReadOnly = false;
                tbxSunday.ReadOnly = false;
                tbxMonday.ReadOnly = false;
                tbxTuesday.ReadOnly = false;
                tbxWednesday.ReadOnly = false;
                tbxThursday.ReadOnly = false;
                tbxFriday.ReadOnly = false;
                tbxSaturday.ReadOnly = false;
            }
            else
            {
                tbxDaysName.ReadOnly = true;
                tbxSunday.ReadOnly = true;
                tbxMonday.ReadOnly = true;
                tbxTuesday.ReadOnly = true;
                tbxWednesday.ReadOnly = true;
                tbxThursday.ReadOnly = true;
                tbxFriday.ReadOnly = true;
                tbxSaturday.ReadOnly = true;
            }
        }

    }
}
