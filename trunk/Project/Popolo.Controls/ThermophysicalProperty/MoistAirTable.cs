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
    /// <summary>�����C��ԕ\���e�[�u��</summary>
    public partial class MoistAirTable : UserControl
    {

        #region �萔

        /// <summary>��C��Ԃ̎�ނƗ�ԍ��̑Ή�</summary>
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

        #region �C���X�^���X�ϐ�

        /// <summary>���������t���O</summary>
        private bool initializing = false;

        /// <summary>�ҏW���̗�ԍ����X�g�i�s�̐��������݁j</summary>
        private List<int> editColsList = new List<int>();

        /// <summary>�C��[kPa]</summary>
        private double barometricPressure = 101.325d;

        #endregion

        #region �v���p�e�B

        /// <summary>�C��[kPa]��ݒ�E�擾����</summary>
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

        #region �R���X�g���N�^

        /// <summary>�R���X�g���N�^</summary>
        public MoistAirTable()
        {
            InitializeComponent();

            editColsList.Add(-1);

            dgAirTable.Rows[0].HeaderCell.Value = "1";
        }

        #endregion

        #region �C�x���g����

        /// <summary>�Z���ҏW�C�x���g�������̏���</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgAirTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //���������̓C�x���g�𖳎�
            if (initializing) return;

            //���l���ۂ����m�F
            double value;
            if (e.ColumnIndex == -1 || e.RowIndex == -1) return;
            if (double.TryParse((string)dgAirTable[e.ColumnIndex, e.RowIndex].Value, out value))
            {
                updateRow(e.RowIndex, e.ColumnIndex);
            }
        }

        /// <summary>��ǉ��C�x���g�������̏���</summary>
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

        /// <summary>��폜�C�x���g�������̏���</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgAirTable_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            editColsList.RemoveAt(e.RowIndex);
        }

        /// <summary>�L�[�_�E���C�x���g�������̏���</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgAirTable_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode.ToString() == "V" && dgAirTable.Focused)
            {
                PasteClipboardData();
            } 
        }

        #endregion//�C�x���g����

        #region private���\�b�h

        /// <summary>�s���X�V����</summary>
        /// <param name="rowIndex">�X�V�ɗ��p����Z���̗�����ԍ�</param>
        /// <param name="colIndex">�X�V�ɗ��p����Z���̍s�����ԍ�</param>
        private void updateRow(int rowIndex, int colIndex)
        {
            int editCol = editColsList[rowIndex];
            //CellA���ҏW�ς݂̏ꍇ
            if (editCol != -1)
            {
                //�ҏW�ς݃Z���Ɨ�ԍ����قȂ�ꍇ
                if (editCol != colIndex)
                {
                    //��C��Ԃ��v�Z
                    double val1, val2;
                    val1 = double.Parse((string)dgAirTable[editCol, rowIndex].Value);
                    val2 = double.Parse((string)dgAirTable[colIndex, rowIndex].Value);
                    try
                    {
                        //��C��Ԃ��v�Z
                        MoistAir mAir = MoistAir.GetAirState(val1, val2, properties[editCol], properties[colIndex], barometricPressure);
                        //��C��Ԃ𔽉f
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
                        //�Z�����͂�����
                        initializing = true;
                        for (int i = 0; i < 8; i++)
                        {
                            if (i != colIndex && i != editCol) dgAirTable[i, rowIndex].Value = e.Message;
                        }
                        initializing = false;
                    }
                    //�ҏW�I��
                    editColsList[rowIndex] = -1;
                }
                else
                {
                    editColsList[rowIndex] = colIndex;
                }
            }
            //CellA���ҏW�O�̏ꍇ
            else
            {
                //���̃Z�����󔒂ɐݒ�
                initializing = true;
                for (int i = 0; i < 8; i++)
                {
                    if (colIndex != i)
                    {
                        dgAirTable[i, rowIndex].Value = "";
                    }
                }
                initializing = false;

                //����̃Z����ҏW�σZ���ɐݒ�
                editColsList[rowIndex] = colIndex;
            }
        }

        /// <summary>�N���b�v�{�[�h�̒l��\��t����</summary>
        public void PasteClipboardData()
        {
            //���݂̃Z���ȉ��Ƀy�[�X�g����
            if (dgAirTable.CurrentCell == null) return;
            int insertRowIndex = dgAirTable.CurrentCell.RowIndex;

            //�N���b�v�{�[�h�̓��e���擾
            string pasteText = Clipboard.GetText();
            if (string.IsNullOrEmpty(pasteText)) return;

            //�s�ŕ���
            pasteText = pasteText.Replace("\r\n", "\n");
            pasteText = pasteText.Replace('\r', '\n');
            pasteText = pasteText.TrimEnd(new char[] { '\n' });
            string[] lines = pasteText.Split('\n');

            //�s���ƂɃZ���L������
            //�ŏI��łȂ����2�񕪃y�[�X�g�\
            bool multiVals = dgAirTable.CurrentCell.ColumnIndex < 4;
            int c1 = dgAirTable.CurrentCell.ColumnIndex;
            int c2 = dgAirTable.CurrentCell.ColumnIndex + 1;

            int newRows = lines.Length - (dgAirTable.NewRowIndex - insertRowIndex);
            if (0 < newRows) dgAirTable.Rows.Add(newRows);
            foreach (string line in lines)
            {
                //�^�u�ŕ���
                string[] vals = line.Split('\t');

                //�s���s������ꍇ�ɂ͒ǉ�
                /*if (insertRowIndex == dgAirTable.NewRowIndex)
                {
                    dgAirTable.Rows.Add();
                    //dgAirTable.Rows.Insert(dgAirTable.NewRowIndex, 1);
                }*/
                DataGridViewRow row = dgAirTable.Rows[insertRowIndex];

                if (0 < vals.Length) row.Cells[c1].Value = vals[0];
                if (1 < vals.Length && multiVals) row.Cells[c2].Value = vals[1];

                //���̍s��
                insertRowIndex++;
            }
        }

        #endregion

        #region public���\�b�h

        /// <summary>�I�����ꂽ�e�[�u���̓��e���擾����</summary>
        /// <returns>�I�����ꂽ�e�[�u���̓��e</returns>
        public DataObject GetClipboardContent()
        {
            return dgAirTable.GetClipboardContent();
        }

        /// <summary>�f�[�^��CSV�t�@�C���ɏ����o��</summary>
        /// <param name="filePath">CSV�t�@�C���ւ̃p�X</param>
        public void OutputDataToCSVFile(string filePath)
        {
            StringBuilder sBuilder = new StringBuilder();
            
            int cols = dgAirTable.Columns.Count;
            int rows = dgAirTable.Rows.Count;
            //�w�b�_�������o��
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
