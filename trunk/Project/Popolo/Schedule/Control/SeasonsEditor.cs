/* SeasonsEditor.cs
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

namespace Popolo.Utility.Schedule.Control
{
    /// <summary>Seasons編集コントロール</summary>
    public partial class SeasonsEditor : UserControl
    {

        #region<インスタンス変数>

        /// <summary>編集中のSeasonsオブジェクト</summary>
        private Seasons seasons;

        /// <summary>初期化中か否かのフラグ</summary>
        private bool initializing = false;

        /// <summary>編集可能か否かのフラグ</summary>
        private bool editable = true;

        #endregion//インスタンス変数

        #region<プロパティ>

        /// <summary>編集中のSeasonsオブジェクトを取得する</summary>
        public Seasons EdittingSeasons
        {
            get
            {
                return seasons;
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
        public SeasonsEditor()
        {
            InitializeComponent();
        }

        #endregion//コンストラクタ

        #region<Seasons設定メソッド>

        /// <summary>編集するSeasonsオブジェクトを設定する</summary>
        /// <param name="seasons">編集するSeasonsオブジェクト</param>
        public void SetSeasons(Seasons seasons)
        {
            //編集中のseasonsオブジェクトがあればイベント通知を解除
            if (this.seasons != null)
            {
                this.seasons.NameChangeEvent -= new Seasons.NameChangeEventHandler(seasons_NameChangeEvent);
                this.seasons.SeasonAddEvent -= new Seasons.SeasonAddEventHandler(seasons_SeasonAddEvent);
                this.seasons.SeasonChangeEvent -= new Seasons.SeasonChangeEventHandler(seasons_SeasonChangeEvent);
                this.seasons.SeasonRemoveEvent -= new Seasons.SeasonRemoveEventHandler(seasons_SeasonRemoveEvent);
            }
            //編集中のseasonsオブジェクトを更新
            this.seasons = seasons;

            //Seasonsオブジェクトのイベント通知を受ける
            seasons.NameChangeEvent += new Seasons.NameChangeEventHandler(seasons_NameChangeEvent);
            seasons.SeasonAddEvent += new Seasons.SeasonAddEventHandler(seasons_SeasonAddEvent);
            seasons.SeasonChangeEvent += new Seasons.SeasonChangeEventHandler(seasons_SeasonChangeEvent);
            seasons.SeasonRemoveEvent += new Seasons.SeasonRemoveEventHandler(seasons_SeasonRemoveEvent);

            //コントロール削除時にイベント通知を解除
            this.Disposed += delegate(object sender, EventArgs e) {
                seasons.NameChangeEvent -= new Seasons.NameChangeEventHandler(seasons_NameChangeEvent);
                seasons.SeasonAddEvent -= new Seasons.SeasonAddEventHandler(seasons_SeasonAddEvent);
                seasons.SeasonChangeEvent -= new Seasons.SeasonChangeEventHandler(seasons_SeasonChangeEvent);
                seasons.SeasonRemoveEvent -= new Seasons.SeasonRemoveEventHandler(seasons_SeasonRemoveEvent);
            };

            //リストボックスを初期化
            lbxSeasons.Items.Clear();
            for (int i = 0; i < seasons.Count; i++) lbxSeasons.Items.Add(seasons.GetSeasonName(i));
            //季節が一つの場合は削除ボタンを操作不能にする
            if (lbxSeasons.Items.Count <= 1) btnRemove.Enabled = false;
            //一つ目の季節を選択
            if (0 < lbxSeasons.Items.Count) lbxSeasons.SelectedIndex = 0;

            //Seasonsの名称を設定
            initializing = true;
            tbxSeasonsName.Text = seasons.Name;
            initializing = false;
        }

        /// <summary>名称変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void seasons_NameChangeEvent(object sender, EventArgs e)
        {
            tbxSeasonsName.Text = ((Seasons)sender).Name;
        }

        /// <summary>季節削除イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void seasons_SeasonRemoveEvent(object sender, SeasonsEventArgs e)
        {
            lbxSeasons.Items.RemoveAt(e.SeasonIndex);
            //最後の一つの季節の場合は削除ボタンを編集不可にする
            if (lbxSeasons.Items.Count <= 1) btnRemove.Enabled = false;
            //選択アイテムを更新
            if (e.SeasonIndex == 0) lbxSeasons.SelectedIndex = 0;
            else lbxSeasons.SelectedIndex = e.SeasonIndex - 1;
        }

        /// <summary>季節編集イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void seasons_SeasonChangeEvent(object sender, SeasonsEventArgs e)
        {
            //選択中のアイテムを一時保存
            int sIndex = lbxSeasons.SelectedIndex;
            //編集対象が選択中のアイテムか否か
            bool isSelectedItem = (sIndex == e.SeasonIndex);
            lbxSeasons.Items.RemoveAt(e.SeasonIndex);
            lbxSeasons.Items.Insert(e.SeasonIndex, e.SeasonName);
            if (isSelectedItem) lbxSeasons.SelectedIndex = sIndex;
        }

        /// <summary>季節追加イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void seasons_SeasonAddEvent(object sender, SeasonsEventArgs e)
        {
            lbxSeasons.Items.Insert(e.SeasonIndex, e.SeasonName);
            //追加した季節が選択中の季節に影響を与える場合はコントロールを更新
            int sIndex = lbxSeasons.SelectedIndex;
            if (sIndex == e.SeasonIndex - 1 || sIndex == e.SeasonIndex + 1) updateControl();
            //削除ボタンが編集不可の場合は編集可能にする
            if (!btnRemove.Enabled) btnRemove.Enabled = true;
        }

        #endregion//Seasons設定メソッド

        #region<イベント発生時の処理>

        /// <summary>季節定義オブジェクト名称テキストボックス変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxSeasonsName_TextChanged(object sender, EventArgs e)
        {
            //初期化中は無視
            if (initializing) return;
            seasons.Name = tbxSeasonsName.Text;
        }

        /// <summary>季節名称テキストボックス変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxSeasonName_TextChanged(object sender, EventArgs e)
        {
            //初期化中は無視
            if (initializing) return;
            int sIndex = lbxSeasons.SelectedIndex;
            if (0 <= sIndex) seasons.ChangeSeasonName(sIndex, tbxSeasonName.Text);

            tbxSeasonName.Focus();
            tbxSeasonName.SelectionStart = tbxSeasonName.Text.Length;
        }

        /// <summary>季節追加ボタンクリックイベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            //初期化中は無視
            if (initializing) return;
            //選択中のSeasonsオブジェクト番号を取得
            int sIndex = lbxSeasons.SelectedIndex;
            //非選択の場合は終了
            if (sIndex < 0) return;
            string sName;
            DateTime sStart, sEnd;
            seasons.GetSeason(sIndex, out sName, out sStart, out sEnd);
            TimeSpan tSpan = sEnd - sStart;
            //選択中のSeasonsオブジェクトの日にち範囲が0日の場合は分割できないので終了
            if (tSpan.Days < 1) return;
            sStart = sStart.AddDays((tSpan.Days + 1) / 2);
            seasons.AddSeason("新規季節", sStart);
        }

        /// <summary>季節削除ボタンクリックイベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemove_Click(object sender, EventArgs e)
        {
            //初期化中は無視
            if (initializing) return;
            //選択中のSeasonsオブジェクト番号を取得
            int sIndex = lbxSeasons.SelectedIndex;
            //非選択の場合は終了
            if (sIndex < 0) return;
            //残りアイテムが一つの場合は終了
            if (lbxSeasons.Items.Count == 1) return;
            seasons.RemoveSeason(sIndex);
        }

        /// <summary>季節開始月日変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtPickerStart_ValueChanged(object sender, EventArgs e)
        {
            //初期化中は無視
            if (initializing) return;
            //選択中のSeasonsオブジェクト番号を取得
            int sIndex = lbxSeasons.SelectedIndex;
            //非選択の場合は終了
            if (sIndex < 0) return;
            //月日変更失敗の場合は元に戻す
            if (!seasons.ChangeSeasonDateTime(sIndex, dtPickerStart.Value, true))
            {
                updateControl();
            }
        }

        /// <summary>季節終了月日変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtPickerEnd_ValueChanged(object sender, EventArgs e)
        {
            //初期化中は無視
            if (initializing) return;
            //選択中のSeasonsオブジェクト番号を取得
            int sIndex = lbxSeasons.SelectedIndex;
            //非選択の場合は終了
            if (sIndex < 0) return;
            //月日変更失敗の場合は元に戻す
            if (!seasons.ChangeSeasonDateTime(sIndex, dtPickerEnd.Value, false))
            {
                updateControl();
            }
        }

        /// <summary>選択中のSeasonsオブジェクト変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbxSeasons_SelectedIndexChanged(object sender, EventArgs e)
        {
            //コントロールを更新する
            updateControl();
        }

        /// <summary>コントロールを更新する</summary>
        private void updateControl()
        {
            //選択中のSeasonsオブジェクト番号を取得
            int sIndex = lbxSeasons.SelectedIndex;
            
            //非選択の場合はリストボックス以外のコントロールを編集不可にして終了
            if (sIndex < 0)
            {
                tbxSeasonName.Enabled = false;
                tbxSeasonsName.Enabled = false;
                btnAdd.Enabled = false;
                btnRemove.Enabled = false;
                dtPickerEnd.Enabled = false;
                dtPickerStart.Enabled = false;
                return;
            }

            //選択中のSeasonsオブジェクトの情報をコントロールに反映
            bool isInit = initializing;
            initializing = true;
            string sName;
            DateTime dtStart, dtEnd;
            seasons.GetSeason(sIndex, out sName, out dtStart, out dtEnd);
            dtPickerStart.Value = dtStart;
            dtPickerEnd.Value = dtEnd;
            tbxSeasonName.Text = sName;
            initializing = isInit;

            //閲覧モードの場合
            if (!editable)
            {
                tbxSeasonName.Enabled = false;
                tbxSeasonsName.Enabled = false;
                btnAdd.Enabled = false;
                btnRemove.Enabled = false;
                dtPickerStart.Enabled = false;
                dtPickerEnd.Enabled = false;
                return;
            }

            btnAdd.Enabled = true;
            btnRemove.Enabled = true;
            tbxSeasonName.Enabled = true;
            tbxSeasonsName.Enabled = true;
            //両端の季節の場合は開始終了月日を編集不可にする
            if (!dtPickerStart.Enabled && editable) dtPickerStart.Enabled = true;
            if (!dtPickerEnd.Enabled && editable) dtPickerEnd.Enabled = true;
            if (sIndex == 0) dtPickerStart.Enabled = false;
            if (sIndex == lbxSeasons.Items.Count - 1) dtPickerEnd.Enabled = false;
        }

        #endregion//イベント発生時の処理

    }
}
