/* TMY3Converter.cs
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
using System.IO;

namespace Popolo.Weather.Converter
{
    /// <summary>TMY3気象データを変換する</summary>
    public static class TMY3Converter
    {

        #region Properties

        /// <summary>気象データ名称を取得する</summary>
        public static string Name
        {
            get
            {
                return "TMY3形式気象データ";
            }
        }

        #endregion

        #region public methods

        /// <summary>ファイルを元にWeatherDataTableを構成する</summary>
        /// <param name="filePath">読み取りファイルのパス</param>
        /// <param name="success">読み取り成功の真偽</param>
        /// <returns>構成されたPWeatherDataオブジェクト</returns>
        public static WeatherDataTable ToPWeatherData(string filePath, out bool success)
        {
            success = false;

            //読み出しファイルの存在確認
            if (File.Exists(filePath))
            {
                WeatherDataTable wdTable = new WeatherDataTable();
                using (StreamReader sReader = new StreamReader(filePath))
                {
                    string[] buff;
                    DateTime dTime = new DateTime();

                    //第1行：地点情報
                    buff = sReader.ReadLine().Split(',');
                    LocationInformation lInfo = new LocationInformation();
                    lInfo.ID = int.Parse(buff[0]);
                    lInfo.Name = buff[1];
                    lInfo.EnglishName = buff[2];
                    lInfo.Latitude = double.Parse(buff[4]);
                    lInfo.Longitude = double.Parse(buff[5]);
                    lInfo.Elevation = double.Parse(buff[6]);

                    for (int i = 0; i < 8760; i++)
                    {
                        WeatherRecord wRecord = new WeatherRecord();
                        WeatherData wData;

                        //年月日特定
                        if (i == 0)
                        {
                            buff = sReader.ReadLine().Split(',');
                            dTime = DateTime.ParseExact(buff[0], "MM/dd/yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        }
                        wRecord.DataDTime = dTime;

                        //日射関連**********************************
                        //大気圏外水平面日射
                        wData = new WeatherData(double.Parse(buff[2]) / 3600d, WeatherData.DataSource.CalculatedValue, -1);
                        wRecord.SetData(WeatherRecord.RecordType.ExtraterrestrialHorizontalRadiation, wData);
                        //大気圏外法線面日射
                        wData = new WeatherData(double.Parse(buff[3]) / 3600d, WeatherData.DataSource.CalculatedValue, -1);
                        wRecord.SetData(WeatherRecord.RecordType.ExtraterrestrialDirectNormalRadiation, wData);
                        //水平面全天日射
                        wData = new WeatherData(double.Parse(buff[4]) / 3600d, WeatherData.DataSource.Unknown, double.Parse(buff[6]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.GlobalHorizontalRadiation, wData);
                        //直達日射
                        wData = new WeatherData(double.Parse(buff[7]) / 3600d, WeatherData.DataSource.Unknown, double.Parse(buff[9]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.DirectNormalRadiation, wData);
                        //水平面天空日射
                        wData = new WeatherData(double.Parse(buff[10]) / 3600d, WeatherData.DataSource.Unknown, double.Parse(buff[12]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.DiffuseHorizontalRadiation, wData);

                        //日照関連**********************************
                        //水平面全天照度
                        wData = new WeatherData(double.Parse(buff[13]), WeatherData.DataSource.Unknown, double.Parse(buff[15]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.GlobalHorizontalIlluminance, wData);
                        //法線面直射日射照度
                        wData = new WeatherData(double.Parse(buff[16]), WeatherData.DataSource.Unknown, double.Parse(buff[18]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.DirectNormalIlluminance, wData);
                        //水平面天空照度
                        wData = new WeatherData(double.Parse(buff[19]), WeatherData.DataSource.Unknown, double.Parse(buff[21]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.DiffuseHorizontalIlluminance, wData);
                        //天頂輝度
                        wData = new WeatherData(double.Parse(buff[22]), WeatherData.DataSource.Unknown, double.Parse(buff[24]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.ZenithLuminance, wData);
                        //雲量
                        wData = new WeatherData(double.Parse(buff[25]), getDSource(buff[26]), double.Parse(buff[27]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.TotalSkyCover, wData);
                        //雲量2
                        wData = new WeatherData(double.Parse(buff[28]), getDSource(buff[29]), double.Parse(buff[30]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.OpaqueSkyCover, wData);

                        //空気状態関連**********************************
                        //乾球温度
                        wData = new WeatherData(double.Parse(buff[31]), getDSource(buff[32]), double.Parse(buff[33]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.DryBulbTemperature, wData);
                        //露点温度
                        wData = new WeatherData(double.Parse(buff[34]), getDSource(buff[35]), double.Parse(buff[36]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.DewPointTemperature, wData);
                        //相対湿度
                        wData = new WeatherData(double.Parse(buff[37]), getDSource(buff[38]), double.Parse(buff[39]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.RelativeHumidity, wData);
                        //気圧
                        wData = new WeatherData(double.Parse(buff[40]) / 10d, getDSource(buff[41]), double.Parse(buff[42]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.AtmosphericPressure, wData);

                        //その他**********************************
                        //風向
                        wData = new WeatherData(double.Parse(buff[43]) - 180d, getDSource(buff[44]), double.Parse(buff[45]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.WindDirection, wData);
                        //風速
                        wData = new WeatherData(double.Parse(buff[46]), getDSource(buff[47]), double.Parse(buff[48]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.WindSpeed, wData);
                        //視認距離
                        wData = new WeatherData(double.Parse(buff[49]), getDSource(buff[50]), double.Parse(buff[51]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.Visibility, wData);
                        //雲高さ
                        wData = new WeatherData(double.Parse(buff[52]), getDSource(buff[53]), double.Parse(buff[54]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.CeilingHeight, wData);
                        //可降水量
                        wData = new WeatherData(double.Parse(buff[55]), getDSource(buff[56]), double.Parse(buff[57]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.PrecipitableWater, wData);
                        //大気混濁度
                        wData = new WeatherData(double.Parse(buff[58]), getDSource(buff[59]), double.Parse(buff[60]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.AerosolOpticalDepth, wData);
                        //アルベド
                        wData = new WeatherData(double.Parse(buff[61]), getDSource(buff[62]), double.Parse(buff[63]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.Albedo, wData);
                        //降水量
                        wData = new WeatherData(double.Parse(buff[64]), getDSource(buff[66]), double.Parse(buff[67]) / 100d);
                        wRecord.SetData(WeatherRecord.RecordType.PrecipitationLevel, wData);
                        //降水量計測時間はとりあえず無視

                        //気象データ追加
                        wdTable.AddWeatherRecord(wRecord);

                        //時刻更新
                        dTime = dTime.AddHours(1);
                    }
                }

                success = true;
                return wdTable;
            }
            else return null;
        }

        /// <summary>PWeatherDataをファイルに書き出す</summary>
        /// <param name="wdTable">WeatherDataTableオブジェクト</param>
        /// <param name="filePath">書き出しファイルへのパス</param>
        /// <param name="success">書き出し成功の真偽</param>
        public static void FromPWeather(WeatherDataTable wdTable, string filePath, out bool success)
        {
            /*//書き出しストリームを用意
            StreamWriter sWriter = new StreamWriter(filePath);

            //1時間間隔のデータに変更
            WeatherDataTable houlyWDTable = wdTable.ConvertToHoulyDataTable();

            //第一行を記入
            LocationInformation lInfo = wdTable.Location;
            string ss = lInfo.EnglishName + " " +
                lInfo.Latitude.ToString("F2") + " " +
                lInfo.Longitude.ToString("F2") + " " +
                lInfo.LongitudeAtStandardTime.ToString("F2") + " 0 0 0 ";
            while (ss.Length < 80) ss += "-";
            sWriter.WriteLine(ss);

            int cHour = 0;
            ImmutableWeatherRecord[] wrs = new ImmutableWeatherRecord[24];
            for (int i = 0; i < houlyWDTable.WeatherRecordNumber; i++)
            {
                wrs[cHour] = houlyWDTable.GetWeatherRecord(i);
                //24時間分データがたまった場合は書き出し
                if (cHour == 23)
                {
                    if (!output24Data(sWriter, wrs))
                    {
                        //変換失敗の場合
                        sWriter.Close();
                        File.Delete(filePath);
                        success = false;
                        return;
                    }
                    cHour = 0;
                }
                else cHour++;
            }

            sWriter.Close();*/

            success = true;
        }

        #endregion

        #region private methods

        /// <summary>データソースを取得する</summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        private static WeatherData.DataSource getDSource(string flag)
        {
            switch (flag)
            {
                case "A":
                    return WeatherData.DataSource.MeasuredValue;
                case "B":
                    return WeatherData.DataSource.PredictedValue;
                case "C":
                    return WeatherData.DataSource.PredictedValue;
                case "E":
                    return WeatherData.DataSource.CalculatedValue;
                case "F":
                    return WeatherData.DataSource.CalculatedValue;
                default:
                    return WeatherData.DataSource.MissingValue;
            }
        }

        #endregion

    }
}
