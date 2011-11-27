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
    /// <summary>曜日定義編集コントロール</summary>
    public partial class DaysEditor : UserControl
    {

        #region<インスタンス変数>

        /// <summary>編集中のSeasonsオブジェクト</summary>
        private Days days;

        /// <summary>初期化中か否かのフラグ</summary>
        private bool initializing = false;

        /// <summary>編集可能か否かのフラグ</summary>
        private bool editable = true;

        #endregion//インスタンス変数

        #region<プロパティ>

        /// <summary>編集中のDaysオブジェクトを取得する</summary>
        public Days EdittingDays
        {
            get
            {
                return days;
            }
        }

        /// <summary>編集可能か否かを取得・設定する</summary>
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

        #endregion//プロパティ

        #region<コンストラクタ>

        /// <summary>コンストラクタ</summary>
        public DaysEditor()
        {
            InitializeComponent();
        }

        #endregion//コンストラクタ

        #region<Days設定メソッド>

        /// <summary>Daysオブジェクトを設定する</summary>
        /// <param name="days">Daysオブジェクト</param>
        public void SetDays(Days days)
        {
            //現在編集中のDaysオブジェクトがある場合はイベント通知を解除
            if (this.days != null)
            {
                this.days.NameChangeEvent -= new Days.NameChangeEventHandler(days_NameChangeEvent);
                this.days.DayChangeEvent -= new Days.DayChangeEventHandler(days_DayChangeEvent);
            }

            this.days = days;

            //イベント登録
            days.NameChangeEvent += new Days.NameChangeEventHandler(days_NameChangeEvent);
            days.DayChangeEvent += new Days.DayChangeEventHandler(days_DayChangeEvent);

            //コントロール破棄時にイベント通知を解除
            this.Disposed += delegate(object sender, EventArgs e)
            {
                days.NameChangeEvent -= new Days.NameChangeEventHandler(days_NameChangeEvent);
                days.DayChangeEvent -= new Days.DayChangeEventHandler(days_DayChangeEvent);
            };

            //初期化処理を行う
            bool initFlag = initializing;
            initializing = true;

            //名称を設定
            tbxDaysName.Text = days.Name;

            //TextBoxに曜日グループを設定
            tbxSunday.Text = days.GetTermName(DayOfWeek.Sunday);
            tbxMonday.Text = days.GetTermName(DayOfWeek.Monday);
            tbxTuesday.Text = days.GetTermName(DayOfWeek.Tuesday);
            tbxWednesday.Text = days.GetTermName(DayOfWeek.Wednesday);
            tbxThursday.Text = days.GetTermName(DayOfWeek.Thursday);
            tbxFriday.Text = days.GetTermName(DayOfWeek.Friday);
            tbxSaturday.Text = days.GetTermName(DayOfWeek.Saturday);

            initializing = initFlag;
        }

        /// <summary>名称変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void days_NameChangeEvent(object sender, EventArgs e)
        {
            tbxDaysName.Text = days.Name;
        }

        /// <summary>曜日グループ変更イベント発生時の処理</summary>
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

        #endregion//Days設定メソッド

        #region<イベント発生時の処理>

        /// <summary>Days名称テキストボックス変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxDaysName_TextChanged(object sender, EventArgs e)
        {
            days.Name = tbxDaysName.Text;
        }

        /// <summary>グループ名称テキストボックス変更イベント発生時の処理</summary>
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

        #endregion//イベント発生時の処理

        /// <summary>コントロールを更新する</summary>
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
