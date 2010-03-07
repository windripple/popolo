/* PsychrometricChartDrawer.cs
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
using System.Text;

using System.Drawing;

namespace Popolo.Utility.Property
{
    /// <summary>��C���}�`��N���X</summary>
    public static class PsychrometricChartDrawer
    {

        #region �萔�錾

        /// <summary>���y��</summary>
        private static readonly Pen BPEN = Pens.Black;

        /// <summary>�����}�[�W��</summary>
        private const int L_MARGIN = 20;

        /// <summary>�E���}�[�W��</summary>
        private const int R_MARGIN = 40;

        /// <summary>�㑤�}�[�W��</summary>
        private const int T_MARGIN = 20;

        /// <summary>�����}�[�W��</summary>
        private const int B_MARGIN = 30;

        #endregion

        #region �񋓌^��`

        /// <summary>�`����̎��</summary>
        public enum Lines
        {
            /// <summary>�O�a��</summary>
            SaturatedLine = 1,
            /// <summary>�������x</summary>
            WetBulbTemperatureLine = 2,
            /// <summary>�G���^���s�[</summary>
            EnthalpyLine = 4,
            /// <summary>��Ύ��x</summary>
            AbsoluteHumidityLine = 8,
            /// <summary>�������x</summary>
            DryBulbTemperatureLine = 16,
            /// <summary>���Ύ��x</summary>
            RelativeHumidityLine = 32,
            /// <summary>��e�ϐ�</summary>
            SpecificVoluemLine = 64,
            /// <summary>���ׂ�</summary>
            All = SaturatedLine | WetBulbTemperatureLine | EnthalpyLine | AbsoluteHumidityLine | DryBulbTemperatureLine | RelativeHumidityLine | SpecificVoluemLine
        }

        #endregion

        #region �N���X�ϐ�

        /// <summary>�C��[kPa]</summary>
        private static double barometricPressure = 101.325d;

        /// <summary>��C���}�`��T�C�Y</summary>
        private static Size canvasSize = new Size(800, 600);

        /// <summary>����Ԓl���̕`��͈�</summary>
        private static Dictionary<Lines, LineProperty> lineProperties = new Dictionary<Lines, LineProperty>();

        #endregion

        #region �v���p�e�B

        /// <summary>�C����ݒ�E�擾����</summary>
        public static double BarometricPressure
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

        /// <summary>�`��L�����o�X�T�C�Y��ݒ�</summary>
        public static Size CanvasSize
        {
            set
            {
                canvasSize = value;
            }
            get
            {
                return canvasSize;
            }
        }

        #endregion

        #region �ÓI�R���X�g���N�^

        /// <summary>�ÓI�R���X�g���N�^</summary>
        static PsychrometricChartDrawer()
        {
            //����Ԓl���̕`��͈͂�������
            //�������x[CDB]
            lineProperties.Add(Lines.DryBulbTemperatureLine, new LineProperty(-10.0, 50.0, 1.0, new Pen(Color.Black), true));
            //��Ύ��x[kg/kgDA]
            lineProperties.Add(Lines.AbsoluteHumidityLine, new LineProperty(0.0, 0.037, 0.001, new Pen(Color.Black), true));
            //�G���^���s�[[kJ/kg]
            lineProperties.Add(Lines.EnthalpyLine, new LineProperty(0.0, 125.0, 5.0, new Pen(Color.Black), true));
            //�������x[CWB]
            lineProperties.Add(Lines.WetBulbTemperatureLine, new LineProperty(-5.0, 35.0, 1.0, new Pen(Color.Gray), true));
            //��e��[m3/kg]
            lineProperties.Add(Lines.SpecificVoluemLine, new LineProperty(0.75, 0.96, 0.01, new Pen(Color.Black), true));
            //���Ύ��x[%]
            lineProperties.Add(Lines.RelativeHumidityLine, new LineProperty(5.0, 95.0, 5.0, new Pen(Color.Black), true));
            //�O�a���C��
            lineProperties.Add(Lines.SaturatedLine, new LineProperty(-10.0, 50.0, 1.0, new Pen(Color.Black), true));

            //���������x���`��y���𒲐�
            //lineProperties[Lines.WetBulbTemperatureLine].DrawingPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            //����e�ϐ��`��y���𒲐�
            //lineProperties[Lines.SpecificVoluemLine].DrawingPen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

            double hmax = MoistAir.GetAirStateFromDBAH(50, 0, MoistAir.Property.Enthalpy);
            double hmin = MoistAir.GetAirStateFromDBAH(-10, 0, MoistAir.Property.Enthalpy);
            ax = Math.Tan(58d / 180d * Math.PI) * (hmax - hmin) / 0.037;
        }

        private static double ax;

        #endregion

        #region public���\�b�h

        /// <summary>Image�ɋ�C���}��`�悷��</summary>
        /// <param name="image">�`��Ώۂ̃C���[�W</param>
        public static void DrawChart(Image image)
        {
            //�c�������擾
            int width = canvasSize.Width - R_MARGIN - L_MARGIN;
            int height = canvasSize.Height - T_MARGIN - B_MARGIN;

            //�`��Ώۂ�����������ꍇ�͏I��
            if (width <= 0 || height <= 0) return;

            //Bitmap����Graphic�I�u�W�F�N�g���쐬
            Graphics gr = Graphics.FromImage(image);
            //���^�t�@�C���łȂ���΃A���`�G�C���A�X�ݒ�
            if (!(image is System.Drawing.Imaging.Metafile)) gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            //�`��̈�g��`��
            gr.DrawRectangle(BPEN, 0, 0, canvasSize.Width, canvasSize.Height);
            gr.DrawLine(BPEN, L_MARGIN, T_MARGIN, width + L_MARGIN, T_MARGIN);
            gr.DrawLine(BPEN, L_MARGIN, T_MARGIN + height, width + L_MARGIN, T_MARGIN + height);
            gr.DrawLine(BPEN, L_MARGIN, T_MARGIN, L_MARGIN, T_MARGIN + height);
            //gr.DrawRectangle(BPEN, L_MARGIN, T_MARGIN, width, height);

            //�`���_�������ֈړ�
            gr.TranslateTransform(L_MARGIN, height + T_MARGIN);

            //�`��̂��߂̕ϊ����[�g���v�Z
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double yRate = height / (maxAH - minAH);
            double xRate = width / (maxDB - minDB);

            //�O�a���`�揈��
            if (lineProperties[Lines.SaturatedLine].DrawLine) drawSaturatedLine(gr, xRate, yRate);
            //���������x���`�揈��
            if (lineProperties[Lines.DryBulbTemperatureLine].DrawLine) drawDryBulbTemperatureLine(gr, xRate, yRate);
            //����Ύ��x���`�揈��
            if (lineProperties[Lines.AbsoluteHumidityLine].DrawLine) drawAbsoluteHumidityLine(gr, xRate, yRate);

            //���G���^���s�[���`�揈��
            if (lineProperties[Lines.EnthalpyLine].DrawLine) drawEnthalpyLine(gr, xRate, yRate);
            //���������x���`�揈��
            if (lineProperties[Lines.WetBulbTemperatureLine].DrawLine) drawWetBulbTemperatureLine(gr, xRate, yRate);
            //����e�ϐ��`�揈��
            if (lineProperties[Lines.SpecificVoluemLine].DrawLine) drawSpecificVolumeLine(gr, xRate, yRate);
            //�����Ύ��x���`�揈��
            if (lineProperties[Lines.RelativeHumidityLine].DrawLine) drawRelativeHumidityLine(gr, xRate, yRate);

            //���E�ڐ��肨��ѐ��l�`�揈��
            drawAxis(gr, xRate, yRate);

            //Graphics�I�u�W�F�N�g�����
            gr.Dispose();
        }

        /// <summary>Image�ɋ�C���}����я�ԓ_��`�悷��</summary>
        /// <param name="image">�`��Ώۂ̃C���[�W</param>
        /// <param name="plotInfos">��C��ԓ_</param>
        public static void DrawChart(Image image, PlotsInformation[] plotInfos)
        {
            //�c�������擾
            int width = canvasSize.Width - R_MARGIN - L_MARGIN;
            int height = canvasSize.Height - T_MARGIN - B_MARGIN;

            //�`��Ώۂ�����������ꍇ�͏I��
            if (width <= 0 || height <= 0) return;

            //Bitmap����Graphic�I�u�W�F�N�g���쐬
            Graphics gr = Graphics.FromImage(image);
            //���^�t�@�C���łȂ���΃A���`�G�C���A�X�ݒ�
            if (!(image is System.Drawing.Imaging.Metafile)) gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //�`��̈�g��`��           
            gr.DrawRectangle(BPEN, 0, 0, canvasSize.Width, canvasSize.Height);
            gr.DrawLine(BPEN, L_MARGIN, T_MARGIN, width + L_MARGIN, T_MARGIN);
            gr.DrawLine(BPEN, L_MARGIN, T_MARGIN + height, width + L_MARGIN, T_MARGIN + height);
            gr.DrawLine(BPEN, L_MARGIN, T_MARGIN, L_MARGIN, T_MARGIN + height);
            //gr.DrawRectangle(BPEN, L_MARGIN, T_MARGIN, width, height);
            
            //�`���_�������ֈړ�
            gr.TranslateTransform(L_MARGIN, height + T_MARGIN);

            //�`��̂��߂̕ϊ����[�g���v�Z
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double yRate = height / (maxAH - minAH);
            double xRate = width / (maxDB - minDB);

            //�O�a���`�揈��
            if (lineProperties[Lines.SaturatedLine].DrawLine) drawSaturatedLine(gr, xRate, yRate);
            //���������x���`�揈��
            if (lineProperties[Lines.DryBulbTemperatureLine].DrawLine) drawDryBulbTemperatureLine(gr, xRate, yRate);
            //����Ύ��x���`�揈��
            if (lineProperties[Lines.AbsoluteHumidityLine].DrawLine) drawAbsoluteHumidityLine(gr, xRate, yRate);

            //���G���^���s�[���`�揈��
            if (lineProperties[Lines.EnthalpyLine].DrawLine) drawEnthalpyLine(gr, xRate, yRate);
            //���������x���`�揈��
            if (lineProperties[Lines.WetBulbTemperatureLine].DrawLine) drawWetBulbTemperatureLine(gr, xRate, yRate);
            //����e�ϐ��`�揈��
            if (lineProperties[Lines.SpecificVoluemLine].DrawLine) drawSpecificVolumeLine(gr, xRate, yRate);
            //�����Ύ��x���`�揈��
            if (lineProperties[Lines.RelativeHumidityLine].DrawLine) drawRelativeHumidityLine(gr, xRate, yRate);

            //���E�ڐ��肨��ѐ��l�`�揈��
            drawAxis(gr, xRate, yRate);

            //**���������ԓ_�`�揈��
            foreach (PlotsInformation pi in plotInfos)
            {
                Brush brsh = new SolidBrush(pi.FillColor);
                Pen pen = new Pen(pi.LineColor);
                float plotHalfSize = pi.Diameter / 2f;
                for (int i = 0; i < pi.DrybulbTemperatures.Length; i++)
                {
                    PointF ptf = getPointFromDBandAH(pi.DrybulbTemperatures[i], pi.AbsoluteHumidities[i], xRate, yRate);
                    gr.FillEllipse(brsh, ptf.X - plotHalfSize, ptf.Y - plotHalfSize, pi.Diameter, pi.Diameter);
                    gr.DrawEllipse(pen, ptf.X - plotHalfSize, ptf.Y - plotHalfSize, pi.Diameter, pi.Diameter);
                }
            }
            
            //Graphics�I�u�W�F�N�g�����
            gr.Dispose();
        }

        /// <summary>�v���b�g��`�悷��</summary>
        /// <param name="image">�`��Ώۂ̃C���[�W</param>
        /// <param name="plotInfo">�v���b�g���</param>
        public static void DrawPlots(Image image, PlotsInformation plotInfo)
        {
            DrawPlots(image, plotInfo.FillColor, plotInfo.LineColor, plotInfo.Diameter, plotInfo.DrybulbTemperatures, plotInfo.AbsoluteHumidities);
        }

        /// <summary>�v���b�g��`�悷��</summary>
        /// <param name="image">�`��Ώۂ̃C���[�W</param>
        /// <param name="fillColor">�v���b�g�J���[</param>
        /// <param name="lineColor">���E���F</param>
        /// <param name="plotSize">�v���b�g�T�C�Y</param>
        /// <param name="dbTemps">�������x���X�g</param>
        /// <param name="aHumids">��Ύ��x���X�g</param>
        public static void DrawPlots(Image image, Color fillColor, Color lineColor, float plotSize, double[] dbTemps, double[] aHumids)
        {
            //�������x����ѐ�Ύ��x�z��̒������قȂ�ꍇ�͗�O
            if (dbTemps.Length != aHumids.Length) throw new Exception("�v���b�g���s����");

            //�c�������擾
            int width = canvasSize.Width - R_MARGIN - L_MARGIN;
            int height = canvasSize.Height - T_MARGIN - B_MARGIN;

            //�`��Ώۂ�����������ꍇ�͏I��
            if (width <= 0 || height <= 0) return;

            //Bitmap����Graphic�I�u�W�F�N�g���쐬
            Graphics gr = Graphics.FromImage(image);
            //���^�t�@�C���łȂ���΃A���`�G�C���A�X�ݒ�
            if (!(image is System.Drawing.Imaging.Metafile)) gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //�`���_�������ֈړ�
            gr.TranslateTransform(L_MARGIN, height + T_MARGIN);

            //�`��̂��߂̕ϊ����[�g
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double yRate = height / (maxAH - minAH);
            double xRate = width / (maxDB - minDB);

            Brush brsh = new SolidBrush(fillColor);
            Pen pen = new Pen(lineColor);
            float plotHalfSize = plotSize / 2f;
            for (int i = 0; i < dbTemps.Length; i++)
            {
                PointF ptf = getPointFromDBandAH(dbTemps[i], aHumids[i], xRate, yRate);
                gr.FillEllipse(brsh, ptf.X - plotHalfSize, ptf.Y - plotHalfSize, plotSize, plotSize);
                gr.DrawEllipse(pen, ptf.X - plotHalfSize, ptf.Y - plotHalfSize, plotSize, plotSize);
            }
            //Graphics�I�u�W�F�N�g�����
            gr.Dispose();
        }

        /// <summary>���p�`��`�悷��</summary>
        /// <param name="image">�`��Ώۂ̃C���[�W</param>
        /// <param name="plotInfo">�v���b�g���</param>
        public static void DrawLine(Image image, PlotsInformation plotInfo)
        {
            //�������x����ѐ�Ύ��x�z��̒������قȂ�ꍇ�͗�O
            if (plotInfo.DrybulbTemperatures.Length != plotInfo.AbsoluteHumidities.Length) throw new Exception("�v���b�g���s����");

            //�c�������擾
            int width = canvasSize.Width - R_MARGIN - L_MARGIN;
            int height = canvasSize.Height - T_MARGIN - B_MARGIN;

            //�`��Ώۂ�����������ꍇ�͏I��
            if (width <= 0 || height <= 0) return;

            //Bitmap����Graphic�I�u�W�F�N�g���쐬
            Graphics gr = Graphics.FromImage(image);
            //���^�t�@�C���łȂ���΃A���`�G�C���A�X�ݒ�
            if (!(image is System.Drawing.Imaging.Metafile)) gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //�`���_�������ֈړ�
            gr.TranslateTransform(L_MARGIN, height + T_MARGIN);

            //�`��̂��߂̕ϊ����[�g
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double yRate = height / (maxAH - minAH);
            double xRate = width / (maxDB - minDB);

            //�|�C���g���X�g���쐬
            PointF[] ptf = new PointF[plotInfo.DrybulbTemperatures.Length];
            for (int i = 0; i < ptf.Length; i++)
            {
                ptf[i] = getPointFromDBandAH(plotInfo.DrybulbTemperatures[i], plotInfo.AbsoluteHumidities[i], xRate, yRate);
            }
            Pen pen = new Pen(plotInfo.LineColor);
            pen.Width = plotInfo.Diameter;

            if (2 <= ptf.Length) gr.DrawPolygon(pen, ptf);

            //Graphics�I�u�W�F�N�g�����
            gr.Dispose();
        }

        /// <summary>���W�Ɋ�Â��ċ�C��Ԃ��擾����</summary>
        /// <param name="ptf">���W</param>
        /// <returns>��C���</returns>
        public static MoistAir GetAirStateFromPoint(PointF ptf)
        {
            //�}�[�W������������
            double x = ptf.X - L_MARGIN;
            double y = ptf.Y - T_MARGIN;

            //�㉺���l���擾
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;

            //�������x����ѐ�Ύ��x���v�Z
            double dbTemp = minDB + (maxDB - minDB) / (canvasSize.Width - L_MARGIN - R_MARGIN) * x;            
            double aHumid = maxAH - (maxAH - minAH) / (canvasSize.Height - T_MARGIN - B_MARGIN) * y;
            double enthalpy = dbTemp + ax * aHumid;

            //�㉺�͈͊O�̏ꍇ��null��Ԃ�
            if (dbTemp < minDB || maxDB < dbTemp || aHumid < minAH || maxAH < aHumid) return null;
            //�͈͓��Ȃ�΋�C��Ԃ��v�Z���ĕԂ�
            else return MoistAir.GetAirStateFromAHEN(aHumid, enthalpy, barometricPressure);
        }

        /// <summary>�������x�Ɛ�Ύ��x�Ɋ�Â��č��W���v�Z����</summary>
        /// <param name="dryBulbTemperature">�������x</param>
        /// <param name="absoluteHumidity">��Ύ��x</param>
        /// <returns>���W</returns>
        public static PointF GetPointFromDBandAH(double dryBulbTemperature, double absoluteHumidity)
        {
            //�㉺���l���擾
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double yRate = (canvasSize.Height - T_MARGIN - B_MARGIN) / (maxAH - minAH);
            double xRate = (canvasSize.Width - R_MARGIN - L_MARGIN) / (maxDB - minDB);

            /*return new PointF((float)(((dryBulbTemperature - minDB) * xRate) + L_MARGIN),
                (float)((maxAH - absoluteHumidity) * yRate + T_MARGIN));*/
            double en = MoistAir.GetAirStateFromDBAH(dryBulbTemperature, absoluteHumidity, MoistAir.Property.Enthalpy);
            double dbt = en - ax * absoluteHumidity;
            return new PointF((float)((dbt - minDB) * xRate + L_MARGIN),
                (float)((maxAH - absoluteHumidity) * yRate + T_MARGIN));
        }

        /// <summary>����Ԓl���̕`��͈͂�ݒ肷��</summary>
        /// <param name="line">����</param>
        /// <param name="lineProperty">����Ԓl���̕`��͈�</param>
        public static void SetLineProperty(Lines line, LineProperty lineProperty)
        {
            if (lineProperties.ContainsKey(line))
            {
                lineProperties[line] = lineProperty;
            }
        }

        /// <summary>����Ԓl���̕`��͈͂��擾����</summary>
        /// <param name="line">����</param>
        /// <returns>����Ԓl���̕`��͈�</returns>
        public static LineProperty GetLineProperty(Lines line)
        {
            if (lineProperties.ContainsKey(line))
            {
                return lineProperties[line];
            }
            throw new Exception("�w�肳�ꂽ��ނ̏�Ԑ��͂���܂���");
        }

        #endregion

        #region private���\�b�h

        /// <summary>�������x����ѐ�Ύ��x����`��ʒu���v�Z����</summary>
        /// <param name="dbTemp">�������x</param>
        /// <param name="absoluteHumidity">��Ύ��x</param>
        /// <param name="xRate">x�������[�g</param>
        /// <param name="yRate">y�������[�g</param>
        /// <returns>�`��ʒu</returns>
        private static PointF getPointFromDBandAH(double dbTemp, double absoluteHumidity, double xRate, double yRate)
        {
            //�Ό����W�ϊ�
            double en = MoistAir.GetAirStateFromDBAH(dbTemp, absoluteHumidity, MoistAir.Property.Enthalpy);
            double dbt = en - ax * absoluteHumidity;
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            return new PointF((float)((dbt - minDB) * xRate), (float)((minAH - absoluteHumidity) * yRate));

            /*double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            return new PointF((float)((dbTemp - minDB) * xRate), (float)((minAH - absoluteHumidity) * yRate));*/
        }

        /// <summary>�O�a����`�悷��</summary>
        /// <param name="gr">Graphics�I�u�W�F�N�g</param>
        /// <param name="xRate">x�������[�g</param>
        /// <param name="yRate">y�������[�g</param>
        private static void drawSaturatedLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.SaturatedLine].DrawingPen;

            //�`��͈͊m�F
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double currentDB = minDB;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double deltaDB = lineProperties[Lines.DryBulbTemperatureLine].Spacing;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;

            double ahmd = MoistAir.GetSaturatedAbsoluteHumidity(minDB, MoistAir.Property.DryBulbTemperature, barometricPressure);
            if (ahmd < minAH)
            {
                ahmd = minAH;
                currentDB = MoistAir.GetSaturatedDrybulbTemperature(minAH, MoistAir.Property.AbsoluteHumidity, barometricPressure);
            }
            else if (maxAH < ahmd) return;

            PointF prevPtf = getPointFromDBandAH(currentDB, ahmd, xRate, yRate);
            while (currentDB < maxDB)
            {
                ahmd = MoistAir.GetSaturatedAbsoluteHumidity(currentDB, MoistAir.Property.DryBulbTemperature, barometricPressure);
                
                //�`��͈͓��Ɋ܂܂�Ă���ꍇ
                if (ahmd <= maxAH)
                {
                    PointF newPtf = getPointFromDBandAH(currentDB, ahmd, xRate, yRate);
                    gr.DrawLine(pen, prevPtf, newPtf);
                    prevPtf = newPtf;
                }
                else
                {
                    ahmd = maxAH;
                    currentDB = MoistAir.GetSaturatedDrybulbTemperature(maxAH, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                    PointF newPtf = getPointFromDBandAH(currentDB, ahmd, xRate, yRate);
                    gr.DrawLine(pen, prevPtf, newPtf);
                    return;
                }

                currentDB += deltaDB;
            }

            ahmd = MoistAir.GetSaturatedAbsoluteHumidity(maxDB, MoistAir.Property.DryBulbTemperature, barometricPressure);
            gr.DrawLine(pen, prevPtf, getPointFromDBandAH(maxDB, ahmd, xRate, yRate));
        }

        /// <summary>���������x����`�悷��</summary>
        /// <param name="gr">Graphics�I�u�W�F�N�g</param>
        /// <param name="xRate">x���[�g</param>
        /// <param name="yRate">y���[�g</param>
        private static void drawDryBulbTemperatureLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.DryBulbTemperatureLine].DrawingPen;

            //�`��͈͊m�F
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double currentDB = minDB;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double deltaDB = lineProperties[Lines.DryBulbTemperatureLine].Spacing;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double satDB = MoistAir.GetSaturatedDrybulbTemperature(maxAH, MoistAir.Property.AbsoluteHumidity, barometricPressure);

            while (currentDB <= maxDB)
            {
                //�n�_���v�Z
                double ahmd;
                if (satDB < currentDB) ahmd = maxAH;
                else ahmd = MoistAir.GetSaturatedAbsoluteHumidity(currentDB, MoistAir.Property.DryBulbTemperature, barometricPressure);
                if (minAH <= ahmd)
                {
                    PointF startPtf = getPointFromDBandAH(currentDB, ahmd, xRate, yRate);
                    //�I�_
                    PointF endPtf = getPointFromDBandAH(currentDB, minAH, xRate, yRate);
                    //�I�_�Ɛڑ�
                    gr.DrawLine(pen, startPtf, endPtf);
                }

                currentDB += deltaDB;
            }
        }

        /// <summary>����Ύ��x����`�悷��</summary>
        /// <param name="gr">Graphics�I�u�W�F�N�g</param>
        /// <param name="xRate">x���[�g</param>
        /// <param name="yRate">y���[�g</param>
        private static void drawAbsoluteHumidityLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.AbsoluteHumidityLine].DrawingPen;

            //�`��͈͊m�F
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double currentAH = minAH;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double deltaAH = lineProperties[Lines.AbsoluteHumidityLine].Spacing;
            double satAH = MoistAir.GetSaturatedAbsoluteHumidity(minDB, MoistAir.Property.DryBulbTemperature, barometricPressure);

            //��Ύ��xdeltaAH���݂ŕ`��
            while (currentAH <= maxAH)
            {
                //�n�_���v�Z
                double dbt;
                if (currentAH < satAH) dbt = minDB;
                else dbt = MoistAir.GetSaturatedDrybulbTemperature(currentAH, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                if (dbt <= maxDB)
                {
                    PointF startPtf = getPointFromDBandAH(dbt, currentAH, xRate, yRate);
                    //�I�_
                    PointF endPtf = getPointFromDBandAH(maxDB, currentAH, xRate, yRate);
                    //�I�_�Ɛڑ�
                    gr.DrawLine(pen, startPtf, endPtf);
                }

                currentAH += deltaAH;
            }
        }

        /// <summary>���G���^���s�[����`�悷��</summary>
        /// <param name="gr">Graphics�I�u�W�F�N�g</param>
        /// <param name="xRate">x���[�g</param>
        /// <param name="yRate">y���[�g</param>
        private static void drawEnthalpyLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.EnthalpyLine].DrawingPen;

            //�`��͈͊m�F
            double minH = lineProperties[Lines.EnthalpyLine].MinimumValue;
            double currentH = minH;
            double maxH = lineProperties[Lines.EnthalpyLine].MaximumValue;
            double deltaH = lineProperties[Lines.EnthalpyLine].Spacing;
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double deltaDB = lineProperties[Lines.DryBulbTemperatureLine].Spacing;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;

            //�`�揈��
            while (currentH <= maxH)
            {
                bool outofRange = false;
                //�n�_���v�Z
                double dbt = MoistAir.GetSaturatedDrybulbTemperature(currentH, MoistAir.Property.Enthalpy, barometricPressure);
                double ahmd = MoistAir.GetSaturatedAbsoluteHumidity(dbt, MoistAir.Property.DryBulbTemperature, barometricPressure);
                //�n�_���`��̈���������̏ꍇ
                if (dbt < minDB)
                {
                    dbt = minDB;
                    ahmd = MoistAir.GetAirStateFromDBEN(minDB, currentH, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                }
                else if (maxDB < dbt) outofRange = true;
                //�n�_���`��̈��������̏ꍇ
                if (maxAH < ahmd)
                {
                    ahmd = maxAH;
                    dbt = MoistAir.GetAirStateFromAHEN(maxAH, currentH, MoistAir.Property.DryBulbTemperature, barometricPressure);
                    //�n�_���`��̈�����E���̏ꍇ
                    if (maxDB < dbt) outofRange = true;
                }
                else if (ahmd < minAH) outofRange = true;
                PointF prevPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);

                //�I�_���v�Z
                if (!outofRange)
                {
                    PointF finalPt;
                    double dbtEnd = MoistAir.GetAirStateFromAHEN(minAH, currentH, MoistAir.Property.DryBulbTemperature, barometricPressure);
                    double ahmdEnd = MoistAir.GetAirStateFromDBEN(maxDB, currentH, MoistAir.Property.AbsoluteHumidity, barometricPressure);

                    //�E�̈�O�֏o��ꍇ
                    if (maxDB <= dbtEnd)
                    {
                        finalPt = getPointFromDBandAH(maxDB, ahmdEnd, xRate, yRate);
                        //�������x��deltaDB���݂ŕ`��
                        while (true)
                        {
                            dbt += deltaDB;
                            if (maxDB < dbt) break;
                            ahmd = MoistAir.GetAirStateFromDBEN(dbt, currentH, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                            PointF newPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);
                            gr.DrawLine(pen, prevPtf, newPtf);
                            prevPtf = newPtf;
                        }
                    }
                    //���̈�O�֏o��ꍇ
                    else
                    {
                        finalPt = getPointFromDBandAH(dbtEnd, minAH, xRate, yRate);
                        //�������x��deltaDB���݂ŕ`��
                        while (true)
                        {
                            dbt += deltaDB;
                            ahmd = MoistAir.GetAirStateFromDBEN(dbt, currentH, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                            if (ahmd < minAH) break;
                            PointF newPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);
                            gr.DrawLine(pen, prevPtf, newPtf);
                            prevPtf = newPtf;
                        }
                    }
                    //�I�_�Ɛڑ�
                    gr.DrawLine(pen, prevPtf, finalPt);
                }

                //�G���^���s�[�l���X�V
                currentH += deltaH;
            }
        }

        /// <summary>���������x����`�悷��</summary>
        /// <param name="gr">Graphics�I�u�W�F�N�g</param>
        /// <param name="xRate">x���[�g</param>
        /// <param name="yRate">y���[�g</param>
        private static void drawWetBulbTemperatureLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.WetBulbTemperatureLine].DrawingPen;

            //�`��͈͊m�F
            double minWB = lineProperties[Lines.WetBulbTemperatureLine].MinimumValue;
            double currentWB = minWB;
            double maxWB = lineProperties[Lines.WetBulbTemperatureLine].MaximumValue;
            double deltaWB = lineProperties[Lines.WetBulbTemperatureLine].Spacing;
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double deltaDB = lineProperties[Lines.DryBulbTemperatureLine].Spacing;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;

            //�������xdeltaWB���݂ŕ`��
            while (currentWB <= maxWB)
            {
                bool outofRange = false;
                //�n�_���v�Z
                double dbt = MoistAir.GetSaturatedDrybulbTemperature(currentWB, MoistAir.Property.WetBulbTemperature, barometricPressure);
                double ahmd = MoistAir.GetSaturatedAbsoluteHumidity(dbt, MoistAir.Property.DryBulbTemperature, barometricPressure);

                //�n�_���`��̈���������̏ꍇ
                if (dbt < minDB)
                {
                    dbt = minDB;
                    ahmd = MoistAir.GetAirStateFromDBWB(minDB, currentWB, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                }
                else if (maxDB < dbt) outofRange = true;
                //�n�_���`��̈��������̏ꍇ
                if (maxAH < ahmd)
                {
                    ahmd = maxAH;
                    dbt = MoistAir.GetAirStateFromWBAH(currentWB, maxAH, MoistAir.Property.DryBulbTemperature, barometricPressure);
                    //�n�_���`��̈�����E���̏ꍇ
                    if (maxDB < dbt) outofRange = true;
                }
                else if (ahmd < minAH) outofRange = true;

                PointF prevPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);

                //�I�_���v�Z
                if (!outofRange)
                {
                    //�I�_���v�Z
                    PointF finalPt;
                    double dbtEnd = MoistAir.GetAirStateFromWBAH(currentWB, minAH, MoistAir.Property.DryBulbTemperature, barometricPressure);
                    double ahmdEnd = MoistAir.GetAirStateFromDBWB(maxDB, currentWB, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                    //�E�̈�O�֏o��ꍇ
                    if (maxDB <= dbtEnd)
                    {
                        finalPt = getPointFromDBandAH(maxDB, ahmdEnd, xRate, yRate);
                        //�������x��deltaDB���݂ŕ`��
                        while (true)
                        {
                            dbt += deltaDB;
                            if (maxDB < dbt) break;
                            ahmd = MoistAir.GetAirStateFromDBWB(dbt, currentWB, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                            PointF newPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);
                            gr.DrawLine(pen, prevPtf, newPtf);
                            prevPtf = newPtf;
                        }
                    }
                    //���̈�O�֏o��ꍇ
                    else
                    {
                        finalPt = getPointFromDBandAH(dbtEnd, minAH, xRate, yRate);
                        //�������x��deltaDB���݂ŕ`��
                        while (true)
                        {
                            dbt += deltaDB;
                            ahmd = MoistAir.GetAirStateFromDBWB(dbt, currentWB, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                            if (ahmd < minAH) break;
                            PointF newPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);
                            gr.DrawLine(pen, prevPtf, newPtf);
                            prevPtf = newPtf;
                        }
                    }
                    //�I�_�Ɛڑ�
                    gr.DrawLine(pen, prevPtf, finalPt);
                }

                currentWB += deltaWB;
            }
        }

        /// <summary>����e�ϐ���`�悷��</summary>
        /// <param name="gr">Graphics�I�u�W�F�N�g</param>
        /// <param name="xRate">x���[�g</param>
        /// <param name="yRate">y���[�g</param>
        private static void drawSpecificVolumeLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.SpecificVoluemLine].DrawingPen;

            //�`��͈͊m�F
            double minSV = lineProperties[Lines.SpecificVoluemLine].MinimumValue;
            double currentSV = minSV;
            double maxSV = lineProperties[Lines.SpecificVoluemLine].MaximumValue;
            double deltaSV = lineProperties[Lines.SpecificVoluemLine].Spacing;
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double deltaDB = lineProperties[Lines.DryBulbTemperatureLine].Spacing;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;

            //��e��deltaSV���݂ŕ`��
            while (currentSV <= maxSV)
            {
                bool outofRange = false;
                //�n�_���v�Z
                double dbt = MoistAir.GetSaturatedDrybulbTemperature(currentSV, MoistAir.Property.SpecificVolume, barometricPressure);
                double ahmd = MoistAir.GetSaturatedAbsoluteHumidity(dbt, MoistAir.Property.DryBulbTemperature, barometricPressure);
                //�n�_���`��̈���������̏ꍇ
                if (dbt < minDB)
                {
                    dbt = minDB;
                    ahmd = MoistAir.GetAirStateFromDBSV(minDB, currentSV, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                }
                else if (maxDB < dbt) outofRange = true;
                //�n�_���`��̈��������̏ꍇ
                if (maxAH < ahmd)
                {
                    ahmd = maxAH;
                    dbt = MoistAir.GetAirStateFromAHSV(maxAH, currentSV, MoistAir.Property.DryBulbTemperature, barometricPressure);
                    //�n�_���`��̈�����E���̏ꍇ
                    if (maxDB < dbt) outofRange = true;
                }
                else if (ahmd < minAH) outofRange = true;
                PointF prevPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);
                //�I�_���v�Z
                if (!outofRange)
                {
                    PointF finalPt;
                    double dbtEnd = MoistAir.GetAirStateFromAHSV(minAH, currentSV, MoistAir.Property.DryBulbTemperature, barometricPressure);
                    double ahmdEnd = MoistAir.GetAirStateFromDBSV(maxDB, currentSV, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                    //�E�̈�O�֏o��ꍇ
                    if (maxDB <= dbtEnd)
                    {
                        finalPt = getPointFromDBandAH(maxDB, ahmdEnd, xRate, yRate);
                        //�������x��deltaDB���݂ŕ`��
                        while (true)
                        {
                            dbt += deltaDB;
                            if (maxDB < dbt) break;
                            ahmd = MoistAir.GetAirStateFromDBSV(dbt, currentSV, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                            PointF newPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);
                            gr.DrawLine(pen, prevPtf, newPtf);
                            prevPtf = newPtf;
                        }
                    }
                    //���̈�O�֏o��ꍇ
                    else
                    {
                        finalPt = getPointFromDBandAH(dbtEnd, minAH, xRate, yRate);
                        //�������x��deltaDB���݂ŕ`��
                        while (true)
                        {
                            dbt += deltaDB;
                            ahmd = MoistAir.GetAirStateFromDBSV(dbt, currentSV, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                            if (ahmd < minAH) break;
                            PointF newPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);
                            gr.DrawLine(pen, prevPtf, newPtf);
                            prevPtf = newPtf;
                        }
                    }
                    //�I�_�Ɛڑ�
                    gr.DrawLine(pen, prevPtf, finalPt);
                }

                currentSV += deltaSV;
            }

        }

        /// <summary>�����Ύ��x����`�悷��</summary>
        /// <param name="gr">Graphics�I�u�W�F�N�g</param>
        /// <param name="xRate">x���[�g</param>
        /// <param name="yRate">y���[�g</param>
        private static void drawRelativeHumidityLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.RelativeHumidityLine].DrawingPen;

            //�`��͈͊m�F
            double minRH = lineProperties[Lines.RelativeHumidityLine].MinimumValue;
            double currentRH = minRH;
            double maxRH = lineProperties[Lines.RelativeHumidityLine].MaximumValue;
            double deltaRH = lineProperties[Lines.RelativeHumidityLine].Spacing;
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double deltaDB = lineProperties[Lines.DryBulbTemperatureLine].Spacing;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;

            //���Ύ��xdeltaRH���݂ŕ`��
            while (currentRH <= maxRH)
            {
                bool outofRange = false;
                //�n�_���v�Z
                double dbt = minDB;
                double ahmd = MoistAir.GetAirStateFromDBRH(dbt, currentRH, MoistAir.Property.AbsoluteHumidity, barometricPressure);

                //�`��̈�̉����Ɉʒu����ꍇ
                if (ahmd < minAH)
                {
                    ahmd = minAH;
                    dbt = MoistAir.GetAirStateFromAHRH(minAH, currentRH, MoistAir.Property.DryBulbTemperature, barometricPressure);
                    if (maxDB < dbt) outofRange = true;
                }
                else if (maxAH < ahmd) outofRange = true;

                PointF prevPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);

                if (!outofRange)
                {
                    //�I�_���v�Z
                    PointF finalPt;
                    double ahmdEnd = MoistAir.GetAirStateFromDBRH(maxDB, currentRH, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                    //�㕔�̈�O�֏o��ꍇ
                    if (maxAH < ahmdEnd)
                    {
                        double dbtEnd = MoistAir.GetAirStateFromAHRH(maxAH, currentRH, MoistAir.Property.DryBulbTemperature, barometricPressure);
                        finalPt = getPointFromDBandAH(dbtEnd, maxAH, xRate, yRate);
                        //�������x��deltaDB���݂ŕ`��
                        while (true)
                        {
                            dbt += deltaDB;
                            ahmd = MoistAir.GetAirStateFromDBRH(dbt, currentRH, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                            if (maxAH < ahmd) break;
                            PointF newPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);
                            gr.DrawLine(pen, prevPtf, newPtf);
                            prevPtf = newPtf;
                        }
                    }
                    //�E���̈�O�֏o��ꍇ
                    else
                    {
                        finalPt = getPointFromDBandAH(maxDB, ahmdEnd, xRate, yRate);
                        //�������x��deltaDB���݂ŕ`��
                        while (true)
                        {
                            dbt += deltaDB;
                            if (maxDB < dbt) break;
                            ahmd = MoistAir.GetAirStateFromDBRH(dbt, currentRH, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                            PointF newPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);
                            gr.DrawLine(pen, prevPtf, newPtf);
                            prevPtf = newPtf;
                        }
                    }
                    //�I�_�Ɛڑ�
                    gr.DrawLine(pen, prevPtf, finalPt);
                }

                currentRH += deltaRH;
            }

        }

        /// <summary>���E�ڐ��肨��ѐ��l��`�悷��</summary>
        /// <param name="gr">Graphics�I�u�W�F�N�g</param>
        /// <param name="xRate">x���[�g</param>
        /// <param name="yRate">y���[�g</param>
        private static void drawAxis(Graphics gr, double xRate, double yRate)
        {
            Font fnt = new Font("Times New Roman", 7);
            Pen pen = lineProperties[Lines.DryBulbTemperatureLine].DrawingPen;

            //�`��͈͊m�F
            double currentDB =lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double deltaDB = lineProperties[Lines.DryBulbTemperatureLine].Spacing;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double currentAH = minAH;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double deltaAH = lineProperties[Lines.AbsoluteHumidityLine].Spacing;

            //�������x
            while (currentDB <= maxDB + maxDB * 0.0000001)
            {
                //�ڐ����`��
                PointF pt = getPointFromDBandAH(currentDB, minAH, xRate, yRate);
                //gr.DrawLine(pen, pt, new PointF(pt.X, pt.Y + 2));
                //���l��`��
                gr.DrawString(currentDB.ToString("F0"), fnt, Brushes.Black, new PointF(pt.X - 6, pt.Y + 8));

                currentDB += deltaDB;
            }

            //��Ύ��x
            while (currentAH <= maxAH + maxAH * 0.0000001)
            {
                //�ڐ����`��
                PointF pt = getPointFromDBandAH(maxDB, currentAH, xRate, yRate);
                //gr.DrawLine(pen, pt, new PointF(pt.X + 2, pt.Y));
                //���l��`��
                gr.DrawString(currentAH.ToString("F3"), fnt, Brushes.Black, new PointF(pt.X + 5, pt.Y - 6));

                currentAH += deltaAH;
            }
        }

        #endregion

        #region �\���̒�`

        /// <summary>����Ԓl���`��͈͕ێ��\����</summary>
        public struct LineProperty
        {

            /// <summary>�R���X�g���N�^</summary>
            /// <param name="minValue">��Ԓl����</param>
            /// <param name="maxValue">��Ԓl���</param>
            /// <param name="spacing">��Ԓl�Ԋu</param>
            /// <param name="pen">�`�悷��y�����</param>
            /// <param name="drawLine">�`�悷�邩�ۂ�</param>
            public LineProperty(double minValue, double maxValue, double spacing, Pen pen, bool drawLine)
            {
                this.MinimumValue = minValue;
                this.MaximumValue = maxValue;
                this.Spacing = spacing;
                this.DrawingPen = pen;
                this.DrawLine = drawLine;
            }

            /// <summary>��Ԓl����</summary>
            public double MinimumValue;

            /// <summary>��Ԓl���</summary>
            public double MaximumValue;

            /// <summary>��Ԓl�Ԋu</summary>
            public double Spacing;

            /// <summary>�`��y��</summary>
            public Pen DrawingPen;

            /// <summary>�`�悷�邩�ۂ�</summary>
            public bool DrawLine;
        }

        #endregion

        #region �C���i�[�N���X��`

        /// <summary>�v���b�g���</summary>
        public class PlotsInformation
        {

            #region public�t�B�[���h

            /// <summary>�������x���X�g</summary>
            public double[] DrybulbTemperatures = new double[0];

            /// <summary>��Ύ��x���X�g</summary>
            public double[] AbsoluteHumidities = new double[0];

            /// <summary>�`��F</summary>
            public Color FillColor = Color.Red;

            /// <summary>���̐F</summary>
            public Color LineColor = Color.Black;

            /// <summary>���a</summary>
            public float Diameter = 4;

            #endregion

            #region �R���X�g���N�^

            /// <summary>�R���X�g���N�^</summary>
            /// <param name="dbTemps">�������x���X�g</param>
            /// <param name="aHumids">��Ύ��x���X�g</param>
            /// <param name="fillColor">�`��F</param>
            /// <param name="lineColor">���̐F</param>
            /// <param name="diameter">���a</param>
            public PlotsInformation(double[] dbTemps, double[] aHumids,
                Color fillColor, Color lineColor, float diameter)
            {
                this.FillColor = fillColor;
                this.LineColor = lineColor;
                this.Diameter = diameter;

                int minLength = Math.Min(dbTemps.Length, aHumids.Length);
                DrybulbTemperatures = new double[minLength];
                AbsoluteHumidities = new double[minLength];
                for (int i = 0; i < minLength; i++)
                {
                    DrybulbTemperatures[i] = dbTemps[i];
                    AbsoluteHumidities[i] = aHumids[i];
                }
            }

            #endregion

        }

        #endregion

    }
}
