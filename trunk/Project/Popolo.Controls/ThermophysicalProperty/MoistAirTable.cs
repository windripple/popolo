/* MoistAirTable.cs
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using System.IO;

namespace Popolo.ThermophysicalProperty
{
    /// <summary>湿り空気状態表示テーブル</summary>
    public partial class MoistAirTable : UserControl
    {

        #region 定数

        /// <summary>空気状態の種類と列番号の対応</summary>
        private readonly MoistAir.Property[] properties = { 
            MoistAir.Property.DryBulbTemperature,
            MoistAir.Property.WetBulbTemperature,
            MoistAir.Property.HumidityRatio,
            MoistAir.Property.RelativeHumidity,
            MoistAir.Property.Enthalpy,
            MoistAir.Property.SpecificVolume,
            MoistAir.Property.SaturatedTemperature
        };

        #endregion

        #region インスタンス変数

        /// <summary>初期化中フラグ</summary>
        private bool initializing = false;

        /// <summary>編集中の列番号リスト（行の数だけ存在）</summary>
        private List<int> editColsList = new List<int>();

        /// <summary>気圧[kPa]</summary>
        private double barometricPressure = 101.325d;

        #endregion

        #region プロパティ

        /// <summary>気圧[kPa]を設定・取得する</summary>
        public double BarometricPressure
        {
            set
            {
                if (0 < value)
                {
                    barometricPressure = value;
                }
            }
            get
            {
                return barometricPressure;
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        public MoistAirTable()
        {
            InitializeComponent();

            editColsList.Add(-1);

            dgAirTable.Rows[0].HeaderCell.Value = "1";
        }

        #endregion

        #region イベント処理

        /// <summary>セル編集イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgAirTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //初期化中はイベントを無視
            if (initializing) return;

            //数値か否かを確認
            double value;
            if (e.ColumnIndex == -1 || e.RowIndex == -1) return;
            if (double.TryParse((string)dgAirTable[e.ColumnIndex, e.RowIndex].Value, out value))
            {
                updateRow(e.RowIndex, e.ColumnIndex);
            }
        }

        /// <summary>列追加イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgAirTable_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            int[] newInts = new int[e.RowCount];
            for (int i = 0; i < newInts.Length; i++)
            {
                newInts[i] = -1;

                dgAirTable.Rows[e.RowIndex + i].HeaderCell.Value = (e.RowIndex + i + 1).ToString("F0");
            }
            editColsList.InsertRange(e.RowIndex, newInts);
        }

        /// <summary>列削除イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgAirTable_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            editColsList.RemoveAt(e.RowIndex);
        }

        /// <summary>キーダウンイベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgAirTable_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode.ToString() == "V" && dgAirTable.Focused)
            {
                PasteClipboardData();
            } 
        }

        #endregion//イベント処理

        #region privateメソッド

        /// <summary>行を更新する</summary>
        /// <param name="rowIndex">更新に利用するセルの列方向番号</param>
        /// <param name="colIndex">更新に利用するセルの行方向番号</param>
        private void updateRow(int rowIndex, int colIndex)
        {
            int editCol = editColsList[rowIndex];
            //CellAが編集済みの場合
            if (editCol != -1)
            {
                //編集済みセルと列番号が異なる場合
                if (editCol != colIndex)
                {
                    //空気状態を計算
                    double val1, val2;
                    val1 = double.Parse((string)dgAirTable[editCol, rowIndex].Value);
                    val2 = double.Parse((string)dgAirTable[colIndex, rowIndex].Value);
                    try
                    {
                        //空気状態を計算
                        MoistAir mAir = MoistAir.GetAirState(val1, val2, properties[editCol], properties[colIndex], barometricPressure);
                        //空気状態を反映
                        initializing = true;
                        dgAirTable[0, rowIndex].Value = mAir.DryBulbTemperature.ToString("F2");
                        dgAirTable[1, rowIndex].Value = mAir.WetBulbTemperature.ToString("F2");
                        dgAirTable[2, rowIndex].Value = mAir.HumidityRatio.ToString("F5");
                        dgAirTable[3, rowIndex].Value = mAir.RelativeHumidity.ToString("F2");
                        dgAirTable[4, rowIndex].Value = mAir.Enthalpy.ToString("F2");
                        dgAirTable[5, rowIndex].Value = MoistAir.GetAirStateFromDBHR(mAir.DryBulbTemperature, mAir.HumidityRatio, MoistAir.Property.SpecificVolume, barometricPressure).ToString("F4");
                        dgAirTable[6, rowIndex].Value = MoistAir.GetSaturatedDrybulbTemperature(mAir.HumidityRatio, MoistAir.Property.HumidityRatio, barometricPressure).ToString("F2");
                        dgAirTable[7, rowIndex].Value = barometricPressure.ToString("F3");
                        initializing = false;
                    }
                    catch (MoistAir.InputValueOutOfRangeException e)
                    {
                        //セル入力を消す
                        initializing = true;
                        for (int i = 0; i < 8; i++)
                        {
                            if (i != colIndex && i != editCol) dgAirTable[i, rowIndex].Value = e.Message;
                        }
                        initializing = false;
                    }
                    //編集終了
                    editColsList[rowIndex] = -1;
                }
                else
                {
                    editColsList[rowIndex] = colIndex;
                }
            }
            //CellAが編集前の場合
            else
            {
                //他のセルを空白に設定
                initializing = true;
                for (int i = 0; i < 8; i++)
                {
                    if (colIndex != i)
                    {
                        dgAirTable[i, rowIndex].Value = "";
                    }
                }
                initializing = false;

                //今回のセルを編集済セルに設定
                editColsList[rowIndex] = colIndex;
            }
        }

        /// <summary>クリップボードの値を貼り付ける</summary>
        public void PasteClipboardData()
        {
            //現在のセル以下にペーストする
            if (dgAirTable.CurrentCell == null) return;
            int insertRowIndex = dgAirTable.CurrentCell.RowIndex;

            //クリップボードの内容を取得
            string pasteText = Clipboard.GetText();
            if (string.IsNullOrEmpty(pasteText)) return;

            //行で分割
            pasteText = pasteText.Replace("\r\n", "\n");
            pasteText = pasteText.Replace('\r', '\n');
            pasteText = pasteText.TrimEnd(new char[] { '\n' });
            string[] lines = pasteText.Split('\n');

            //行ごとにセル記入処理
            //最終列でなければ2列分ペースト可能
            bool multiVals = dgAirTable.CurrentCell.ColumnIndex < 4;
            int c1 = dgAirTable.CurrentCell.ColumnIndex;
            int c2 = dgAirTable.CurrentCell.ColumnIndex + 1;

            int newRows = lines.Length - (dgAirTable.NewRowIndex - insertRowIndex);
            if (0 < newRows) dgAirTable.Rows.Add(newRows);
            foreach (string line in lines)
            {
                //タブで分割
                string[] vals = line.Split('\t');

                //行が不足する場合には追加
                /*if (insertRowIndex == dgAirTable.NewRowIndex)
                {
                    dgAirTable.Rows.Add();
                    //dgAirTable.Rows.Insert(dgAirTable.NewRowIndex, 1);
                }*/
                DataGridViewRow row = dgAirTable.Rows[insertRowIndex];

                if (0 < vals.Length) row.Cells[c1].Value = vals[0];
                if (1 < vals.Length && multiVals) row.Cells[c2].Value = vals[1];

                //次の行へ
                insertRowIndex++;
            }
        }

        #endregion

        #region publicメソッド

        /// <summary>選択されたテーブルの内容を取得する</summary>
        /// <returns>選択されたテーブルの内容</returns>
        public DataObject GetClipboardContent()
        {
            return dgAirTable.GetClipboardContent();
        }

        /// <summary>データをCSVファイルに書き出す</summary>
        /// <param name="filePath">CSVファイルへのパス</param>
        public void OutputDataToCSVFile(string filePath)
        {
            StringBuilder sBuilder = new StringBuilder();
            
            int cols = dgAirTable.Columns.Count;
            int rows = dgAirTable.Rows.Count;
            //ヘッダを書き出す
            for (int i = 0; i < cols; i++)
            {
                sBuilder.Append(dgAirTable.Columns[i].HeaderText + ",");
            }
            sBuilder.Remove(sBuilder.Length - 1, 1);
            sBuilder.AppendLine();
            
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    sBuilder.Append(dgAirTable[j, i].Value + ",");
                }
                sBuilder.Remove(sBuilder.Length - 1, 1);
                sBuilder.AppendLine();
            }

            StreamWriter sWriter = new StreamWriter(filePath, false, Encoding.GetEncoding("Shift_JIS"));
            sWriter.Write(sBuilder.ToString());
            sWriter.Close();
        }

        #endregion

    }
}
