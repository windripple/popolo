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
    /// <summary>空気線図描画クラス</summary>
    public static class PsychrometricChartDrawer
    {

        #region 定数宣言

        /// <summary>黒ペン</summary>
        private static readonly Pen BPEN = Pens.Black;

        /// <summary>左側マージン</summary>
        private const int L_MARGIN = 20;

        /// <summary>右側マージン</summary>
        private const int R_MARGIN = 40;

        /// <summary>上側マージン</summary>
        private const int T_MARGIN = 20;

        /// <summary>下側マージン</summary>
        private const int B_MARGIN = 30;

        #endregion

        #region 列挙型定義

        /// <summary>描画線の種類</summary>
        public enum Lines
        {
            /// <summary>飽和線</summary>
            SaturatedLine = 1,
            /// <summary>湿球温度</summary>
            WetBulbTemperatureLine = 2,
            /// <summary>エンタルピー</summary>
            EnthalpyLine = 4,
            /// <summary>絶対湿度</summary>
            AbsoluteHumidityLine = 8,
            /// <summary>乾球温度</summary>
            DryBulbTemperatureLine = 16,
            /// <summary>相対湿度</summary>
            RelativeHumidityLine = 32,
            /// <summary>比容積線</summary>
            SpecificVoluemLine = 64,
            /// <summary>すべて</summary>
            All = SaturatedLine | WetBulbTemperatureLine | EnthalpyLine | AbsoluteHumidityLine | DryBulbTemperatureLine | RelativeHumidityLine | SpecificVoluemLine
        }

        #endregion

        #region クラス変数

        /// <summary>気圧[kPa]</summary>
        private static double barometricPressure = 101.325d;

        /// <summary>空気線図描画サイズ</summary>
        private static Size canvasSize = new Size(800, 600);

        /// <summary>等状態値線の描画範囲</summary>
        private static Dictionary<Lines, LineProperty> lineProperties = new Dictionary<Lines, LineProperty>();

        #endregion

        #region プロパティ

        /// <summary>気圧を設定・取得する</summary>
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

        /// <summary>描画キャンバスサイズを設定</summary>
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

        #region 静的コンストラクタ

        /// <summary>静的コンストラクタ</summary>
        static PsychrometricChartDrawer()
        {
            //等状態値線の描画範囲を初期化
            //乾球温度[CDB]
            lineProperties.Add(Lines.DryBulbTemperatureLine, new LineProperty(-10.0, 50.0, 1.0, new Pen(Color.Black), true));
            //絶対湿度[kg/kgDA]
            lineProperties.Add(Lines.AbsoluteHumidityLine, new LineProperty(0.0, 0.037, 0.001, new Pen(Color.Black), true));
            //エンタルピー[kJ/kg]
            lineProperties.Add(Lines.EnthalpyLine, new LineProperty(0.0, 125.0, 5.0, new Pen(Color.Black), true));
            //湿球温度[CWB]
            lineProperties.Add(Lines.WetBulbTemperatureLine, new LineProperty(-5.0, 35.0, 1.0, new Pen(Color.Gray), true));
            //比容積[m3/kg]
            lineProperties.Add(Lines.SpecificVoluemLine, new LineProperty(0.75, 0.96, 0.01, new Pen(Color.Black), true));
            //相対湿度[%]
            lineProperties.Add(Lines.RelativeHumidityLine, new LineProperty(5.0, 95.0, 5.0, new Pen(Color.Black), true));
            //飽和蒸気線
            lineProperties.Add(Lines.SaturatedLine, new LineProperty(-10.0, 50.0, 1.0, new Pen(Color.Black), true));

            //等湿球温度線描画ペンを調整
            //lineProperties[Lines.WetBulbTemperatureLine].DrawingPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            //等比容積線描画ペンを調整
            //lineProperties[Lines.SpecificVoluemLine].DrawingPen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

            double hmax = MoistAir.GetAirStateFromDBAH(50, 0, MoistAir.Property.Enthalpy);
            double hmin = MoistAir.GetAirStateFromDBAH(-10, 0, MoistAir.Property.Enthalpy);
            ax = Math.Tan(58d / 180d * Math.PI) * (hmax - hmin) / 0.037;
        }

        private static double ax;

        #endregion

        #region publicメソッド

        /// <summary>Imageに空気線図を描画する</summary>
        /// <param name="image">描画対象のイメージ</param>
        public static void DrawChart(Image image)
        {
            //縦横幅を取得
            int width = canvasSize.Width - R_MARGIN - L_MARGIN;
            int height = canvasSize.Height - T_MARGIN - B_MARGIN;

            //描画対象が小さすぎる場合は終了
            if (width <= 0 || height <= 0) return;

            //BitmapからGraphicオブジェクトを作成
            Graphics gr = Graphics.FromImage(image);
            //メタファイルでなければアンチエイリアス設定
            if (!(image is System.Drawing.Imaging.Metafile)) gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            //描画領域枠を描画
            gr.DrawRectangle(BPEN, 0, 0, canvasSize.Width, canvasSize.Height);
            gr.DrawLine(BPEN, L_MARGIN, T_MARGIN, width + L_MARGIN, T_MARGIN);
            gr.DrawLine(BPEN, L_MARGIN, T_MARGIN + height, width + L_MARGIN, T_MARGIN + height);
            gr.DrawLine(BPEN, L_MARGIN, T_MARGIN, L_MARGIN, T_MARGIN + height);
            //gr.DrawRectangle(BPEN, L_MARGIN, T_MARGIN, width, height);

            //描画基準点を左下へ移動
            gr.TranslateTransform(L_MARGIN, height + T_MARGIN);

            //描画のための変換レートを計算
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double yRate = height / (maxAH - minAH);
            double xRate = width / (maxDB - minDB);

            //飽和線描画処理
            if (lineProperties[Lines.SaturatedLine].DrawLine) drawSaturatedLine(gr, xRate, yRate);
            //等乾球温度線描画処理
            if (lineProperties[Lines.DryBulbTemperatureLine].DrawLine) drawDryBulbTemperatureLine(gr, xRate, yRate);
            //等絶対湿度線描画処理
            if (lineProperties[Lines.AbsoluteHumidityLine].DrawLine) drawAbsoluteHumidityLine(gr, xRate, yRate);

            //等エンタルピー線描画処理
            if (lineProperties[Lines.EnthalpyLine].DrawLine) drawEnthalpyLine(gr, xRate, yRate);
            //等湿球温度線描画処理
            if (lineProperties[Lines.WetBulbTemperatureLine].DrawLine) drawWetBulbTemperatureLine(gr, xRate, yRate);
            //等比容積線描画処理
            if (lineProperties[Lines.SpecificVoluemLine].DrawLine) drawSpecificVolumeLine(gr, xRate, yRate);
            //等相対湿度線描画処理
            if (lineProperties[Lines.RelativeHumidityLine].DrawLine) drawRelativeHumidityLine(gr, xRate, yRate);

            //軸・目盛りおよび数値描画処理
            drawAxis(gr, xRate, yRate);

            //Graphicsオブジェクトを閉じる
            gr.Dispose();
        }

        /// <summary>Imageに空気線図および状態点を描画する</summary>
        /// <param name="image">描画対象のイメージ</param>
        /// <param name="plotInfos">空気状態点</param>
        public static void DrawChart(Image image, PlotsInformation[] plotInfos)
        {
            //縦横幅を取得
            int width = canvasSize.Width - R_MARGIN - L_MARGIN;
            int height = canvasSize.Height - T_MARGIN - B_MARGIN;

            //描画対象が小さすぎる場合は終了
            if (width <= 0 || height <= 0) return;

            //BitmapからGraphicオブジェクトを作成
            Graphics gr = Graphics.FromImage(image);
            //メタファイルでなければアンチエイリアス設定
            if (!(image is System.Drawing.Imaging.Metafile)) gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //描画領域枠を描画           
            gr.DrawRectangle(BPEN, 0, 0, canvasSize.Width, canvasSize.Height);
            gr.DrawLine(BPEN, L_MARGIN, T_MARGIN, width + L_MARGIN, T_MARGIN);
            gr.DrawLine(BPEN, L_MARGIN, T_MARGIN + height, width + L_MARGIN, T_MARGIN + height);
            gr.DrawLine(BPEN, L_MARGIN, T_MARGIN, L_MARGIN, T_MARGIN + height);
            //gr.DrawRectangle(BPEN, L_MARGIN, T_MARGIN, width, height);
            
            //描画基準点を左下へ移動
            gr.TranslateTransform(L_MARGIN, height + T_MARGIN);

            //描画のための変換レートを計算
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double yRate = height / (maxAH - minAH);
            double xRate = width / (maxDB - minDB);

            //飽和線描画処理
            if (lineProperties[Lines.SaturatedLine].DrawLine) drawSaturatedLine(gr, xRate, yRate);
            //等乾球温度線描画処理
            if (lineProperties[Lines.DryBulbTemperatureLine].DrawLine) drawDryBulbTemperatureLine(gr, xRate, yRate);
            //等絶対湿度線描画処理
            if (lineProperties[Lines.AbsoluteHumidityLine].DrawLine) drawAbsoluteHumidityLine(gr, xRate, yRate);

            //等エンタルピー線描画処理
            if (lineProperties[Lines.EnthalpyLine].DrawLine) drawEnthalpyLine(gr, xRate, yRate);
            //等湿球温度線描画処理
            if (lineProperties[Lines.WetBulbTemperatureLine].DrawLine) drawWetBulbTemperatureLine(gr, xRate, yRate);
            //等比容積線描画処理
            if (lineProperties[Lines.SpecificVoluemLine].DrawLine) drawSpecificVolumeLine(gr, xRate, yRate);
            //等相対湿度線描画処理
            if (lineProperties[Lines.RelativeHumidityLine].DrawLine) drawRelativeHumidityLine(gr, xRate, yRate);

            //軸・目盛りおよび数値描画処理
            drawAxis(gr, xRate, yRate);

            //**ここから状態点描画処理
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
            
            //Graphicsオブジェクトを閉じる
            gr.Dispose();
        }

        /// <summary>プロットを描画する</summary>
        /// <param name="image">描画対象のイメージ</param>
        /// <param name="plotInfo">プロット情報</param>
        public static void DrawPlots(Image image, PlotsInformation plotInfo)
        {
            DrawPlots(image, plotInfo.FillColor, plotInfo.LineColor, plotInfo.Diameter, plotInfo.DrybulbTemperatures, plotInfo.AbsoluteHumidities);
        }

        /// <summary>プロットを描画する</summary>
        /// <param name="image">描画対象のイメージ</param>
        /// <param name="fillColor">プロットカラー</param>
        /// <param name="lineColor">境界線色</param>
        /// <param name="plotSize">プロットサイズ</param>
        /// <param name="dbTemps">乾球温度リスト</param>
        /// <param name="aHumids">絶対湿度リスト</param>
        public static void DrawPlots(Image image, Color fillColor, Color lineColor, float plotSize, double[] dbTemps, double[] aHumids)
        {
            //乾球温度および絶対湿度配列の長さが異なる場合は例外
            if (dbTemps.Length != aHumids.Length) throw new Exception("プロット数不整合");

            //縦横幅を取得
            int width = canvasSize.Width - R_MARGIN - L_MARGIN;
            int height = canvasSize.Height - T_MARGIN - B_MARGIN;

            //描画対象が小さすぎる場合は終了
            if (width <= 0 || height <= 0) return;

            //BitmapからGraphicオブジェクトを作成
            Graphics gr = Graphics.FromImage(image);
            //メタファイルでなければアンチエイリアス設定
            if (!(image is System.Drawing.Imaging.Metafile)) gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //描画基準点を左下へ移動
            gr.TranslateTransform(L_MARGIN, height + T_MARGIN);

            //描画のための変換レート
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
            //Graphicsオブジェクトを閉じる
            gr.Dispose();
        }

        /// <summary>多角形を描画する</summary>
        /// <param name="image">描画対象のイメージ</param>
        /// <param name="plotInfo">プロット情報</param>
        public static void DrawLine(Image image, PlotsInformation plotInfo)
        {
            //乾球温度および絶対湿度配列の長さが異なる場合は例外
            if (plotInfo.DrybulbTemperatures.Length != plotInfo.AbsoluteHumidities.Length) throw new Exception("プロット数不整合");

            //縦横幅を取得
            int width = canvasSize.Width - R_MARGIN - L_MARGIN;
            int height = canvasSize.Height - T_MARGIN - B_MARGIN;

            //描画対象が小さすぎる場合は終了
            if (width <= 0 || height <= 0) return;

            //BitmapからGraphicオブジェクトを作成
            Graphics gr = Graphics.FromImage(image);
            //メタファイルでなければアンチエイリアス設定
            if (!(image is System.Drawing.Imaging.Metafile)) gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //描画基準点を左下へ移動
            gr.TranslateTransform(L_MARGIN, height + T_MARGIN);

            //描画のための変換レート
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double yRate = height / (maxAH - minAH);
            double xRate = width / (maxDB - minDB);

            //ポイントリストを作成
            PointF[] ptf = new PointF[plotInfo.DrybulbTemperatures.Length];
            for (int i = 0; i < ptf.Length; i++)
            {
                ptf[i] = getPointFromDBandAH(plotInfo.DrybulbTemperatures[i], plotInfo.AbsoluteHumidities[i], xRate, yRate);
            }
            Pen pen = new Pen(plotInfo.LineColor);
            pen.Width = plotInfo.Diameter;

            if (2 <= ptf.Length) gr.DrawPolygon(pen, ptf);

            //Graphicsオブジェクトを閉じる
            gr.Dispose();
        }

        /// <summary>座標に基づいて空気状態を取得する</summary>
        /// <param name="ptf">座標</param>
        /// <returns>空気状態</returns>
        public static MoistAir GetAirStateFromPoint(PointF ptf)
        {
            //マージンを差し引く
            double x = ptf.X - L_MARGIN;
            double y = ptf.Y - T_MARGIN;

            //上下限値を取得
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;

            //乾球温度および絶対湿度を計算
            double dbTemp = minDB + (maxDB - minDB) / (canvasSize.Width - L_MARGIN - R_MARGIN) * x;            
            double aHumid = maxAH - (maxAH - minAH) / (canvasSize.Height - T_MARGIN - B_MARGIN) * y;
            double enthalpy = dbTemp + ax * aHumid;

            //上下範囲外の場合はnullを返す
            if (dbTemp < minDB || maxDB < dbTemp || aHumid < minAH || maxAH < aHumid) return null;
            //範囲内ならば空気状態を計算して返す
            else return MoistAir.GetAirStateFromAHEN(aHumid, enthalpy, barometricPressure);
        }

        /// <summary>乾球温度と絶対湿度に基づいて座標を計算する</summary>
        /// <param name="dryBulbTemperature">乾球温度</param>
        /// <param name="absoluteHumidity">絶対湿度</param>
        /// <returns>座標</returns>
        public static PointF GetPointFromDBandAH(double dryBulbTemperature, double absoluteHumidity)
        {
            //上下限値を取得
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

        /// <summary>等状態値線の描画範囲を設定する</summary>
        /// <param name="line">線種</param>
        /// <param name="lineProperty">等状態値線の描画範囲</param>
        public static void SetLineProperty(Lines line, LineProperty lineProperty)
        {
            if (lineProperties.ContainsKey(line))
            {
                lineProperties[line] = lineProperty;
            }
        }

        /// <summary>等状態値線の描画範囲を取得する</summary>
        /// <param name="line">線種</param>
        /// <returns>等状態値線の描画範囲</returns>
        public static LineProperty GetLineProperty(Lines line)
        {
            if (lineProperties.ContainsKey(line))
            {
                return lineProperties[line];
            }
            throw new Exception("指定された種類の状態線はありません");
        }

        #endregion

        #region privateメソッド

        /// <summary>乾球温度および絶対湿度から描画位置を計算する</summary>
        /// <param name="dbTemp">乾球温度</param>
        /// <param name="absoluteHumidity">絶対湿度</param>
        /// <param name="xRate">x方向レート</param>
        /// <param name="yRate">y方向レート</param>
        /// <returns>描画位置</returns>
        private static PointF getPointFromDBandAH(double dbTemp, double absoluteHumidity, double xRate, double yRate)
        {
            //斜交座標変換
            double en = MoistAir.GetAirStateFromDBAH(dbTemp, absoluteHumidity, MoistAir.Property.Enthalpy);
            double dbt = en - ax * absoluteHumidity;
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            return new PointF((float)((dbt - minDB) * xRate), (float)((minAH - absoluteHumidity) * yRate));

            /*double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            return new PointF((float)((dbTemp - minDB) * xRate), (float)((minAH - absoluteHumidity) * yRate));*/
        }

        /// <summary>飽和線を描画する</summary>
        /// <param name="gr">Graphicsオブジェクト</param>
        /// <param name="xRate">x方向レート</param>
        /// <param name="yRate">y方向レート</param>
        private static void drawSaturatedLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.SaturatedLine].DrawingPen;

            //描画範囲確認
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
                
                //描画範囲内に含まれている場合
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

        /// <summary>等乾球温度線を描画する</summary>
        /// <param name="gr">Graphicsオブジェクト</param>
        /// <param name="xRate">xレート</param>
        /// <param name="yRate">yレート</param>
        private static void drawDryBulbTemperatureLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.DryBulbTemperatureLine].DrawingPen;

            //描画範囲確認
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double currentDB = minDB;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double deltaDB = lineProperties[Lines.DryBulbTemperatureLine].Spacing;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double satDB = MoistAir.GetSaturatedDrybulbTemperature(maxAH, MoistAir.Property.AbsoluteHumidity, barometricPressure);

            while (currentDB <= maxDB)
            {
                //始点を計算
                double ahmd;
                if (satDB < currentDB) ahmd = maxAH;
                else ahmd = MoistAir.GetSaturatedAbsoluteHumidity(currentDB, MoistAir.Property.DryBulbTemperature, barometricPressure);
                if (minAH <= ahmd)
                {
                    PointF startPtf = getPointFromDBandAH(currentDB, ahmd, xRate, yRate);
                    //終点
                    PointF endPtf = getPointFromDBandAH(currentDB, minAH, xRate, yRate);
                    //終点と接続
                    gr.DrawLine(pen, startPtf, endPtf);
                }

                currentDB += deltaDB;
            }
        }

        /// <summary>等絶対湿度線を描画する</summary>
        /// <param name="gr">Graphicsオブジェクト</param>
        /// <param name="xRate">xレート</param>
        /// <param name="yRate">yレート</param>
        private static void drawAbsoluteHumidityLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.AbsoluteHumidityLine].DrawingPen;

            //描画範囲確認
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double currentAH = minAH;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double deltaAH = lineProperties[Lines.AbsoluteHumidityLine].Spacing;
            double satAH = MoistAir.GetSaturatedAbsoluteHumidity(minDB, MoistAir.Property.DryBulbTemperature, barometricPressure);

            //絶対湿度deltaAH刻みで描画
            while (currentAH <= maxAH)
            {
                //始点を計算
                double dbt;
                if (currentAH < satAH) dbt = minDB;
                else dbt = MoistAir.GetSaturatedDrybulbTemperature(currentAH, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                if (dbt <= maxDB)
                {
                    PointF startPtf = getPointFromDBandAH(dbt, currentAH, xRate, yRate);
                    //終点
                    PointF endPtf = getPointFromDBandAH(maxDB, currentAH, xRate, yRate);
                    //終点と接続
                    gr.DrawLine(pen, startPtf, endPtf);
                }

                currentAH += deltaAH;
            }
        }

        /// <summary>等エンタルピー線を描画する</summary>
        /// <param name="gr">Graphicsオブジェクト</param>
        /// <param name="xRate">xレート</param>
        /// <param name="yRate">yレート</param>
        private static void drawEnthalpyLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.EnthalpyLine].DrawingPen;

            //描画範囲確認
            double minH = lineProperties[Lines.EnthalpyLine].MinimumValue;
            double currentH = minH;
            double maxH = lineProperties[Lines.EnthalpyLine].MaximumValue;
            double deltaH = lineProperties[Lines.EnthalpyLine].Spacing;
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double deltaDB = lineProperties[Lines.DryBulbTemperatureLine].Spacing;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;

            //描画処理
            while (currentH <= maxH)
            {
                bool outofRange = false;
                //始点を計算
                double dbt = MoistAir.GetSaturatedDrybulbTemperature(currentH, MoistAir.Property.Enthalpy, barometricPressure);
                double ahmd = MoistAir.GetSaturatedAbsoluteHumidity(dbt, MoistAir.Property.DryBulbTemperature, barometricPressure);
                //始点が描画領域よりも左方の場合
                if (dbt < minDB)
                {
                    dbt = minDB;
                    ahmd = MoistAir.GetAirStateFromDBEN(minDB, currentH, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                }
                else if (maxDB < dbt) outofRange = true;
                //始点が描画領域よりも上方の場合
                if (maxAH < ahmd)
                {
                    ahmd = maxAH;
                    dbt = MoistAir.GetAirStateFromAHEN(maxAH, currentH, MoistAir.Property.DryBulbTemperature, barometricPressure);
                    //始点が描画領域よりも右方の場合
                    if (maxDB < dbt) outofRange = true;
                }
                else if (ahmd < minAH) outofRange = true;
                PointF prevPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);

                //終点を計算
                if (!outofRange)
                {
                    PointF finalPt;
                    double dbtEnd = MoistAir.GetAirStateFromAHEN(minAH, currentH, MoistAir.Property.DryBulbTemperature, barometricPressure);
                    double ahmdEnd = MoistAir.GetAirStateFromDBEN(maxDB, currentH, MoistAir.Property.AbsoluteHumidity, barometricPressure);

                    //右領域外へ出る場合
                    if (maxDB <= dbtEnd)
                    {
                        finalPt = getPointFromDBandAH(maxDB, ahmdEnd, xRate, yRate);
                        //乾球温度をdeltaDB刻みで描画
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
                    //下領域外へ出る場合
                    else
                    {
                        finalPt = getPointFromDBandAH(dbtEnd, minAH, xRate, yRate);
                        //乾球温度をdeltaDB刻みで描画
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
                    //終点と接続
                    gr.DrawLine(pen, prevPtf, finalPt);
                }

                //エンタルピー値を更新
                currentH += deltaH;
            }
        }

        /// <summary>等湿球温度線を描画する</summary>
        /// <param name="gr">Graphicsオブジェクト</param>
        /// <param name="xRate">xレート</param>
        /// <param name="yRate">yレート</param>
        private static void drawWetBulbTemperatureLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.WetBulbTemperatureLine].DrawingPen;

            //描画範囲確認
            double minWB = lineProperties[Lines.WetBulbTemperatureLine].MinimumValue;
            double currentWB = minWB;
            double maxWB = lineProperties[Lines.WetBulbTemperatureLine].MaximumValue;
            double deltaWB = lineProperties[Lines.WetBulbTemperatureLine].Spacing;
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double deltaDB = lineProperties[Lines.DryBulbTemperatureLine].Spacing;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;

            //湿球温度deltaWB刻みで描画
            while (currentWB <= maxWB)
            {
                bool outofRange = false;
                //始点を計算
                double dbt = MoistAir.GetSaturatedDrybulbTemperature(currentWB, MoistAir.Property.WetBulbTemperature, barometricPressure);
                double ahmd = MoistAir.GetSaturatedAbsoluteHumidity(dbt, MoistAir.Property.DryBulbTemperature, barometricPressure);

                //始点が描画領域よりも左方の場合
                if (dbt < minDB)
                {
                    dbt = minDB;
                    ahmd = MoistAir.GetAirStateFromDBWB(minDB, currentWB, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                }
                else if (maxDB < dbt) outofRange = true;
                //始点が描画領域よりも上方の場合
                if (maxAH < ahmd)
                {
                    ahmd = maxAH;
                    dbt = MoistAir.GetAirStateFromWBAH(currentWB, maxAH, MoistAir.Property.DryBulbTemperature, barometricPressure);
                    //始点が描画領域よりも右方の場合
                    if (maxDB < dbt) outofRange = true;
                }
                else if (ahmd < minAH) outofRange = true;

                PointF prevPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);

                //終点を計算
                if (!outofRange)
                {
                    //終点を計算
                    PointF finalPt;
                    double dbtEnd = MoistAir.GetAirStateFromWBAH(currentWB, minAH, MoistAir.Property.DryBulbTemperature, barometricPressure);
                    double ahmdEnd = MoistAir.GetAirStateFromDBWB(maxDB, currentWB, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                    //右領域外へ出る場合
                    if (maxDB <= dbtEnd)
                    {
                        finalPt = getPointFromDBandAH(maxDB, ahmdEnd, xRate, yRate);
                        //乾球温度をdeltaDB刻みで描画
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
                    //下領域外へ出る場合
                    else
                    {
                        finalPt = getPointFromDBandAH(dbtEnd, minAH, xRate, yRate);
                        //乾球温度をdeltaDB刻みで描画
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
                    //終点と接続
                    gr.DrawLine(pen, prevPtf, finalPt);
                }

                currentWB += deltaWB;
            }
        }

        /// <summary>等比容積線を描画する</summary>
        /// <param name="gr">Graphicsオブジェクト</param>
        /// <param name="xRate">xレート</param>
        /// <param name="yRate">yレート</param>
        private static void drawSpecificVolumeLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.SpecificVoluemLine].DrawingPen;

            //描画範囲確認
            double minSV = lineProperties[Lines.SpecificVoluemLine].MinimumValue;
            double currentSV = minSV;
            double maxSV = lineProperties[Lines.SpecificVoluemLine].MaximumValue;
            double deltaSV = lineProperties[Lines.SpecificVoluemLine].Spacing;
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double deltaDB = lineProperties[Lines.DryBulbTemperatureLine].Spacing;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;

            //比容積deltaSV刻みで描画
            while (currentSV <= maxSV)
            {
                bool outofRange = false;
                //始点を計算
                double dbt = MoistAir.GetSaturatedDrybulbTemperature(currentSV, MoistAir.Property.SpecificVolume, barometricPressure);
                double ahmd = MoistAir.GetSaturatedAbsoluteHumidity(dbt, MoistAir.Property.DryBulbTemperature, barometricPressure);
                //始点が描画領域よりも左方の場合
                if (dbt < minDB)
                {
                    dbt = minDB;
                    ahmd = MoistAir.GetAirStateFromDBSV(minDB, currentSV, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                }
                else if (maxDB < dbt) outofRange = true;
                //始点が描画領域よりも上方の場合
                if (maxAH < ahmd)
                {
                    ahmd = maxAH;
                    dbt = MoistAir.GetAirStateFromAHSV(maxAH, currentSV, MoistAir.Property.DryBulbTemperature, barometricPressure);
                    //始点が描画領域よりも右方の場合
                    if (maxDB < dbt) outofRange = true;
                }
                else if (ahmd < minAH) outofRange = true;
                PointF prevPtf = getPointFromDBandAH(dbt, ahmd, xRate, yRate);
                //終点を計算
                if (!outofRange)
                {
                    PointF finalPt;
                    double dbtEnd = MoistAir.GetAirStateFromAHSV(minAH, currentSV, MoistAir.Property.DryBulbTemperature, barometricPressure);
                    double ahmdEnd = MoistAir.GetAirStateFromDBSV(maxDB, currentSV, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                    //右領域外へ出る場合
                    if (maxDB <= dbtEnd)
                    {
                        finalPt = getPointFromDBandAH(maxDB, ahmdEnd, xRate, yRate);
                        //乾球温度をdeltaDB刻みで描画
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
                    //下領域外へ出る場合
                    else
                    {
                        finalPt = getPointFromDBandAH(dbtEnd, minAH, xRate, yRate);
                        //乾球温度をdeltaDB刻みで描画
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
                    //終点と接続
                    gr.DrawLine(pen, prevPtf, finalPt);
                }

                currentSV += deltaSV;
            }

        }

        /// <summary>等相対湿度線を描画する</summary>
        /// <param name="gr">Graphicsオブジェクト</param>
        /// <param name="xRate">xレート</param>
        /// <param name="yRate">yレート</param>
        private static void drawRelativeHumidityLine(Graphics gr, double xRate, double yRate)
        {
            Pen pen = lineProperties[Lines.RelativeHumidityLine].DrawingPen;

            //描画範囲確認
            double minRH = lineProperties[Lines.RelativeHumidityLine].MinimumValue;
            double currentRH = minRH;
            double maxRH = lineProperties[Lines.RelativeHumidityLine].MaximumValue;
            double deltaRH = lineProperties[Lines.RelativeHumidityLine].Spacing;
            double minDB = lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double deltaDB = lineProperties[Lines.DryBulbTemperatureLine].Spacing;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;

            //相対湿度deltaRH刻みで描画
            while (currentRH <= maxRH)
            {
                bool outofRange = false;
                //始点を計算
                double dbt = minDB;
                double ahmd = MoistAir.GetAirStateFromDBRH(dbt, currentRH, MoistAir.Property.AbsoluteHumidity, barometricPressure);

                //描画領域の下方に位置する場合
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
                    //終点を計算
                    PointF finalPt;
                    double ahmdEnd = MoistAir.GetAirStateFromDBRH(maxDB, currentRH, MoistAir.Property.AbsoluteHumidity, barometricPressure);
                    //上部領域外へ出る場合
                    if (maxAH < ahmdEnd)
                    {
                        double dbtEnd = MoistAir.GetAirStateFromAHRH(maxAH, currentRH, MoistAir.Property.DryBulbTemperature, barometricPressure);
                        finalPt = getPointFromDBandAH(dbtEnd, maxAH, xRate, yRate);
                        //乾球温度をdeltaDB刻みで描画
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
                    //右方領域外へ出る場合
                    else
                    {
                        finalPt = getPointFromDBandAH(maxDB, ahmdEnd, xRate, yRate);
                        //乾球温度をdeltaDB刻みで描画
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
                    //終点と接続
                    gr.DrawLine(pen, prevPtf, finalPt);
                }

                currentRH += deltaRH;
            }

        }

        /// <summary>軸・目盛りおよび数値を描画する</summary>
        /// <param name="gr">Graphicsオブジェクト</param>
        /// <param name="xRate">xレート</param>
        /// <param name="yRate">yレート</param>
        private static void drawAxis(Graphics gr, double xRate, double yRate)
        {
            Font fnt = new Font("Times New Roman", 7);
            Pen pen = lineProperties[Lines.DryBulbTemperatureLine].DrawingPen;

            //描画範囲確認
            double currentDB =lineProperties[Lines.DryBulbTemperatureLine].MinimumValue;
            double maxDB = lineProperties[Lines.DryBulbTemperatureLine].MaximumValue;
            double deltaDB = lineProperties[Lines.DryBulbTemperatureLine].Spacing;
            double minAH = lineProperties[Lines.AbsoluteHumidityLine].MinimumValue;
            double currentAH = minAH;
            double maxAH = lineProperties[Lines.AbsoluteHumidityLine].MaximumValue;
            double deltaAH = lineProperties[Lines.AbsoluteHumidityLine].Spacing;

            //乾球温度
            while (currentDB <= maxDB + maxDB * 0.0000001)
            {
                //目盛りを描画
                PointF pt = getPointFromDBandAH(currentDB, minAH, xRate, yRate);
                //gr.DrawLine(pen, pt, new PointF(pt.X, pt.Y + 2));
                //数値を描画
                gr.DrawString(currentDB.ToString("F0"), fnt, Brushes.Black, new PointF(pt.X - 6, pt.Y + 8));

                currentDB += deltaDB;
            }

            //絶対湿度
            while (currentAH <= maxAH + maxAH * 0.0000001)
            {
                //目盛りを描画
                PointF pt = getPointFromDBandAH(maxDB, currentAH, xRate, yRate);
                //gr.DrawLine(pen, pt, new PointF(pt.X + 2, pt.Y));
                //数値を描画
                gr.DrawString(currentAH.ToString("F3"), fnt, Brushes.Black, new PointF(pt.X + 5, pt.Y - 6));

                currentAH += deltaAH;
            }
        }

        #endregion

        #region 構造体定義

        /// <summary>等状態値線描画範囲保持構造体</summary>
        public struct LineProperty
        {

            /// <summary>コンストラクタ</summary>
            /// <param name="minValue">状態値下限</param>
            /// <param name="maxValue">状態値上限</param>
            /// <param name="spacing">状態値間隔</param>
            /// <param name="pen">描画するペン種類</param>
            /// <param name="drawLine">描画するか否か</param>
            public LineProperty(double minValue, double maxValue, double spacing, Pen pen, bool drawLine)
            {
                this.MinimumValue = minValue;
                this.MaximumValue = maxValue;
                this.Spacing = spacing;
                this.DrawingPen = pen;
                this.DrawLine = drawLine;
            }

            /// <summary>状態値下限</summary>
            public double MinimumValue;

            /// <summary>状態値上限</summary>
            public double MaximumValue;

            /// <summary>状態値間隔</summary>
            public double Spacing;

            /// <summary>描画ペン</summary>
            public Pen DrawingPen;

            /// <summary>描画するか否か</summary>
            public bool DrawLine;
        }

        #endregion

        #region インナークラス定義

        /// <summary>プロット情報</summary>
        public class PlotsInformation
        {

            #region publicフィールド

            /// <summary>乾球温度リスト</summary>
            public double[] DrybulbTemperatures = new double[0];

            /// <summary>絶対湿度リスト</summary>
            public double[] AbsoluteHumidities = new double[0];

            /// <summary>描画色</summary>
            public Color FillColor = Color.Red;

            /// <summary>線の色</summary>
            public Color LineColor = Color.Black;

            /// <summary>直径</summary>
            public float Diameter = 4;

            #endregion

            #region コンストラクタ

            /// <summary>コンストラクタ</summary>
            /// <param name="dbTemps">乾球温度リスト</param>
            /// <param name="aHumids">絶対湿度リスト</param>
            /// <param name="fillColor">描画色</param>
            /// <param name="lineColor">線の色</param>
            /// <param name="diameter">直径</param>
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
