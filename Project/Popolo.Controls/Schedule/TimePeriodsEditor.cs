/* TimePeriodsEditor.cs
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
    /// <summary>TimePeriods編集コントロール</summary>
    public partial class TimePeriodsEditor : UserControl
    {

        #region<インスタンス変数>

        /// <summary>編集中のTimePeriodsオブジェクト</summary>
        private TimePeriods timePeriods;

        /// <summary>初期化中か否かのフラグ</summary>
        private bool initializing = false;

        /// <summary>編集可能か否かのフラグ</summary>
        private bool editable = true;

        #endregion//インスタンス変数

        #region<プロパティ>

        /// <summary>編集中のTimePeriodsオブジェクトを取得する</summary>
        public TimePeriods EdittingTimePeriods
        {
            get
            {
                return timePeriods;
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
        public TimePeriodsEditor()
        {
            InitializeComponent();
        }

        #endregion//コンストラクタ

        #region<TimePeriods設定メソッド>

        /// <summary>編集するTimePeriodsオブジェクトを設定する</summary>
        /// <param name="timePeriods">編集するTimePeriodsオブジェクト</param>
        public void SetTimePeriods(TimePeriods timePeriods)
        {
            //編集中のTimePeriodsオブジェクトがあればイベント通知を解除
            if (this.timePeriods != null)
            {
                this.timePeriods.NameChangeEvent -= new TimePeriods.NameChangeEventHandler(timePeriods_NameChangeEvent);
                this.timePeriods.TimePeriodAddEvent -= new TimePeriods.TimePeriodAddEventHandler(timePeriods_TimePeriodAddEvent);
                this.timePeriods.TimePeriodChangeEvent -= new TimePeriods.TimePeriodChangeEventHandler(timePeriods_TimePeriodChangeEvent);
                this.timePeriods.TimePeriodRemoveEvent -= new TimePeriods.TimePeriodRemoveEventHandler(timePeriods_TimePeriodRemoveEvent);
            }
            //編集中のTimePeriodsオブジェクトを更新
            this.timePeriods = timePeriods;

            //TimePeriodsオブジェクトのイベント通知を受ける
            timePeriods.NameChangeEvent += new TimePeriods.NameChangeEventHandler(timePeriods_NameChangeEvent);
            timePeriods.TimePeriodAddEvent += new TimePeriods.TimePeriodAddEventHandler(timePeriods_TimePeriodAddEvent);
            timePeriods.TimePeriodChangeEvent += new TimePeriods.TimePeriodChangeEventHandler(timePeriods_TimePeriodChangeEvent);
            timePeriods.TimePeriodRemoveEvent += new TimePeriods.TimePeriodRemoveEventHandler(timePeriods_TimePeriodRemoveEvent);

            //コントロール削除時にイベント通知を解除
            this.Disposed += delegate(object sender, EventArgs e) {
                timePeriods.NameChangeEvent -= new TimePeriods.NameChangeEventHandler(timePeriods_NameChangeEvent);
                timePeriods.TimePeriodAddEvent -= new TimePeriods.TimePeriodAddEventHandler(timePeriods_TimePeriodAddEvent);
                timePeriods.TimePeriodChangeEvent -= new TimePeriods.TimePeriodChangeEventHandler(timePeriods_TimePeriodChangeEvent);
                timePeriods.TimePeriodRemoveEvent -= new TimePeriods.TimePeriodRemoveEventHandler(timePeriods_TimePeriodRemoveEvent);
            };

            //リストボックスを初期化
            lbxTimePeriods.Items.Clear();
            for (int i = 0; i < timePeriods.Count; i++) lbxTimePeriods.Items.Add(timePeriods.GetTimePeriodName(i));
            //時間帯が一つの場合は削除ボタンを操作不能にする
            if (lbxTimePeriods.Items.Count <= 1) btnRemove.Enabled = false;
            //一つ目の時間帯を選択
            if (0 < lbxTimePeriods.Items.Count) lbxTimePeriods.SelectedIndex = 0;

            //Seasonsの名称を設定
            initializing = true;
            tbxTimePeriodsName.Text = timePeriods.Name;
            initializing = false;
        }

        /// <summary>名称変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timePeriods_NameChangeEvent(object sender, EventArgs e)
        {
            tbxTimePeriodsName.Text = ((TimePeriods)sender).Name;
        }

        /// <summary>時間帯削除イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timePeriods_TimePeriodRemoveEvent(object sender, TimePeriodsEventArgs e)
        {
            lbxTimePeriods.Items.RemoveAt(e.TimePeriodIndex);
            //最後の一つの時間帯の場合は削除ボタンを編集不可にする
            if (lbxTimePeriods.Items.Count == 1) btnRemove.Enabled = false;
            //選択アイテムを更新
            if (e.TimePeriodIndex == 0) lbxTimePeriods.SelectedIndex = 0;
            else lbxTimePeriods.SelectedIndex = e.TimePeriodIndex - 1;
        }

        /// <summary>時間帯編集イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timePeriods_TimePeriodChangeEvent(object sender, TimePeriodsEventArgs e)
        {
            //初期化フラグON
            bool init = initializing;
            initializing = true;

            //選択中のアイテムを一時保存
            int sIndex = lbxTimePeriods.SelectedIndex;
            //編集対象が選択中のアイテムか否か
            bool isSelectedItem = (sIndex == e.TimePeriodIndex);
            lbxTimePeriods.Items.RemoveAt(e.TimePeriodIndex);
            lbxTimePeriods.Items.Insert(e.TimePeriodIndex, e.TimePeriodName);
            if (isSelectedItem) lbxTimePeriods.SelectedIndex = sIndex;

            //初期化フラグを戻す
            initializing = init;
        }

        /// <summary>時間帯追加イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timePeriods_TimePeriodAddEvent(object sender, TimePeriodsEventArgs e)
        {
            lbxTimePeriods.Items.Insert(e.TimePeriodIndex, e.TimePeriodName);
            //追加した時間帯が選択中の時間帯に影響を与える場合はコントロールを更新
            int sIndex = lbxTimePeriods.SelectedIndex;
            if (sIndex == e.TimePeriodIndex - 1 || sIndex == e.TimePeriodIndex + 1) updateControl();
            //削除ボタンが編集不可の場合は編集可能にする
            if (!btnRemove.Enabled) btnRemove.Enabled = true;
        }

        #endregion//TimePeriods設定メソッド

        #region<イベント発生時の処理>

        /// <summary>時間帯定義オブジェクト名称テキストボックス変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxTimePeriodsName_TextChanged(object sender, EventArgs e)
        {
            //初期化中は無視
            if (initializing) return;
            timePeriods.Name = tbxTimePeriodsName.Text;
        }

        /// <summary>名称テキストボックス変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxTimePeriodName_TextChanged(object sender, EventArgs e)
        {
            //初期化中は無視
            if (initializing) return;
            int sIndex = lbxTimePeriods.SelectedIndex;
            if (0 <= sIndex) timePeriods.ChangeTimePeriodName(sIndex, tbxTimePeriodName.Text);
           
            tbxTimePeriodName.Focus();
            tbxTimePeriodName.SelectionStart = tbxTimePeriodName.Text.Length;
        }

        /// <summary>時間帯追加ボタンクリックイベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            //初期化中は無視
            if (initializing) return;
            //選択中のTimePeriodsオブジェクト番号を取得
            int sIndex = lbxTimePeriods.SelectedIndex;
            //非選択の場合は終了
            if (sIndex < 0) return;
            string sName;
            DateTime sStart, sEnd;
            timePeriods.GetTimePeriod(sIndex, out sName, out sStart, out sEnd);
            TimeSpan tSpan = sEnd - sStart;
            //選択中のTimePeriodsオブジェクトの時間範囲が0時間の場合は分割できないので終了
            if (tSpan.Hours < 1) return;
            sStart = sStart.AddHours((tSpan.Hours + 1) / 2);
            timePeriods.AddTimePeriod("新規時間帯", sStart);
        }

        /// <summary>時間帯削除ボタンクリックイベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemove_Click(object sender, EventArgs e)
        {
            //初期化中は無視
            if (initializing) return;
            //選択中のTimePeriodsオブジェクト番号を取得
            int sIndex = lbxTimePeriods.SelectedIndex;
            //非選択の場合は終了
            if (sIndex < 0) return;
            //残りアイテムが一つの場合は終了
            if (lbxTimePeriods.Items.Count == 1) return;
            timePeriods.RemoveTimePeriod(sIndex);
        }

        /// <summary>時間帯開始時刻変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtPickerStart_ValueChanged(object sender, EventArgs e)
        {
            //初期化中は無視
            if (initializing) return;
            //選択中のTimePeriodsオブジェクト番号を取得
            int sIndex = lbxTimePeriods.SelectedIndex;
            //非選択の場合は終了
            if (sIndex < 0) return;
            //時間変更失敗の場合は元に戻す
            if (!timePeriods.ChangeTimePeriodDateTime(sIndex, dtPickerStart.Value, true))
            {
                updateControl();
            }
        }

        /// <summary>時間帯終了時刻変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtPickerEnd_ValueChanged(object sender, EventArgs e)
        {
            //初期化中は無視
            if (initializing) return;
            //選択中のTimePeriodsオブジェクト番号を取得
            int sIndex = lbxTimePeriods.SelectedIndex;
            //非選択の場合は終了
            if (sIndex < 0) return;
            //時間変更失敗の場合は元に戻す
            if (!timePeriods.ChangeTimePeriodDateTime(sIndex, dtPickerEnd.Value, false))
            {
                updateControl();
            }
        }

        /// <summary>選択中のTimePeriodsオブジェクト変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbxTimePeriods_SelectedIndexChanged(object sender, EventArgs e)
        {
            //コントロールを更新する
            updateControl();
        }

        /// <summary>コントロールを更新する</summary>
        private void updateControl()
        {
            //選択中のTimePeriodsオブジェクト番号を取得
            int sIndex = lbxTimePeriods.SelectedIndex;

            //非選択の場合はリストボックス以外のコントロールを編集不可にして終了
            if (sIndex < 0)
            {
                tbxTimePeriodName.Enabled = false;
                tbxTimePeriodsName.Enabled = false;
                btnAdd.Enabled = false;
                btnRemove.Enabled = false;
                dtPickerEnd.Enabled = false;
                dtPickerStart.Enabled = false;
                return;
            }

            //選択中のTimePeriodsオブジェクトの情報をコントロールに反映
            bool isInit = initializing;
            initializing = true;
            string sName;
            DateTime dtStart, dtEnd;
            timePeriods.GetTimePeriod(sIndex, out sName, out dtStart, out dtEnd);
            dtPickerStart.Value = dtStart;
            dtPickerEnd.Value = dtEnd;
            tbxTimePeriodName.Text = sName;
            initializing = isInit;

            //閲覧モードの場合
            if (!editable)
            {
                tbxTimePeriodName.Enabled = false;
                tbxTimePeriodsName.Enabled = false;
                btnAdd.Enabled = false;
                btnRemove.Enabled = false;
                dtPickerStart.Enabled = false;
                dtPickerEnd.Enabled = false;
                return;
            }

            btnAdd.Enabled = true;
            btnRemove.Enabled = true;
            tbxTimePeriodName.Enabled = true;
            tbxTimePeriodsName.Enabled = true;
            //両端の時間帯の場合は開始終了時刻を編集不可にする
            if (!dtPickerStart.Enabled) dtPickerStart.Enabled = true;
            if (!dtPickerEnd.Enabled) dtPickerEnd.Enabled = true;
            if (sIndex == 0) dtPickerStart.Enabled = false;
            if (sIndex == lbxTimePeriods.Items.Count - 1) dtPickerEnd.Enabled = false;
            
        }

        #endregion//イベント発生時の処理

    }
}
