/* TMY1Converter.cs
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

using Popolo.ThermophysicalProperty;

namespace Popolo.Weather.Converter
{
    /// <summary>TMY1気象データを変換する</summary>
    public static class TMY1Converter
    {

        #region クラス変数

        /// <summary>データの読み取りに利用するバイト列</summary>
        static byte[] rByte = new byte[132];

        #endregion

        #region Properties

        /// <summary>気象データ名称を取得する</summary>
        public static string Name
        {
            get
            {
                return "TMY1形式気象データ";
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
            if (!File.Exists(filePath)) return null;

            WeatherDataTable wdTable = new WeatherDataTable();

            using(StreamReader sReader = new StreamReader(filePath))
            {
                string str;
                bool firstLine = true;
                while ((str = sReader.ReadLine()) != null)
                {
                    //初回は地点情報を設定
                    if (firstLine)
                    {
                        LocationInformation lInfo = new LocationInformation();
                        lInfo.ID = int.Parse(str.Substring(0, 5));
                        lInfo.Name = lInfo.EnglishName = "TMY1" + lInfo.ID.ToString("F0");
                        wdTable.Location = lInfo;

                        firstLine = false;
                    }

                    WeatherRecord wRecord = new WeatherRecord();
                    WeatherData wData;

                    //日時
                    int year = int.Parse(str.Substring(5,2));
                    if (year < 20) year += 2000;
                    else year += 1900;
                    int month = int.Parse(str.Substring(7, 2));
                    int day = int.Parse(str.Substring(9, 2));
                    int hour = int.Parse(str.Substring(11, 2)) - 1;
                    int minute = int.Parse(str.Substring(13, 2));

                    wRecord.DataDTime = new DateTime(year, month, day, hour, minute, 0);

                    //直達日射[W/m2]
                    if (str.Substring(24, 4) == "9999") wData = new WeatherData(0d, getDSource1(str.Substring(23, 1)), -1);
                    else wData = new WeatherData(double.Parse(str.Substring(24, 4)) / 3.6d, getDSource1(str.Substring(23, 1)), -1);
                    wRecord.SetData(WeatherRecord.RecordType.DirectNormalRadiation, wData);

                    //水平面天空日射[W/m2]
                    if (str.Substring(29, 4) == "9999") wData = new WeatherData(0d, getDSource1(str.Substring(28, 1)), -1);
                    else wData = new WeatherData(double.Parse(str.Substring(29, 4)) / 3.6d, getDSource1(str.Substring(28, 1)), -1);
                    wRecord.SetData(WeatherRecord.RecordType.DiffuseHorizontalRadiation, wData);

                    //水平面全天日射[W/m2]
                    if (str.Substring(54, 4) == "9999") wData = new WeatherData(0d, getDSource1(str.Substring(54, 1)), -1);
                    else wData = new WeatherData(double.Parse(str.Substring(54, 4)) / 3.6d, getDSource1(str.Substring(53, 1)), -1);
                    wRecord.SetData(WeatherRecord.RecordType.GlobalHorizontalRadiation, wData);

                    //雲高さ[m]
                    if (str.Substring(72, 4) == "7777") wData = new WeatherData(0d, WeatherData.DataSource.MissingValue, -1);
                    if (str.Substring(72, 4) == "8888") wData = new WeatherData(0d, WeatherData.DataSource.MissingValue, -1);
                    else wData = new WeatherData(double.Parse(str.Substring(72, 4)) * 10, WeatherData.DataSource.MeasuredValue, -1);
                    wRecord.SetData(WeatherRecord.RecordType.CeilingHeight, wData);

                    //視認距離[km]
                    if (str.Substring(81, 4) == "8888") wData = new WeatherData(160d, WeatherData.DataSource.MeasuredValue, -1);
                    else wData = new WeatherData(double.Parse(str.Substring(81, 4)) * 10, WeatherData.DataSource.MeasuredValue, -1);
                    wRecord.SetData(WeatherRecord.RecordType.Visibility, wData);

                    //気圧[kPa]
                    wData = new WeatherData(double.Parse(str.Substring(98, 5)) / 100, WeatherData.DataSource.MeasuredValue, -1);
                    double atm = wData.Value;
                    wRecord.SetData(WeatherRecord.RecordType.AtmosphericPressure, wData);

                    //外気乾球温度[C]
                    wData = new WeatherData(double.Parse(str.Substring(103, 4)) / 10, WeatherData.DataSource.MeasuredValue, -1);
                    double dbt = wData.Value;
                    wRecord.SetData(WeatherRecord.RecordType.DryBulbTemperature, wData);

                    //露点温度[C]
                    wData = new WeatherData(double.Parse(str.Substring(107, 4)) / 10, WeatherData.DataSource.MeasuredValue, -1);
                    double dpt = wData.Value;
                    wRecord.SetData(WeatherRecord.RecordType.DewPointTemperature, wData);

                    //その他の空気状態
                    double ahd = MoistAir.GetSaturatedHumidityRatio(dpt, MoistAir.Property.DryBulbTemperature, atm);
                    MoistAir mAir = MoistAir.GetAirStateFromDBHR(dbt, ahd, atm);

                    //相対湿度[%]
                    wRecord.SetData(WeatherRecord.RecordType.RelativeHumidity, new WeatherData(mAir.RelativeHumidity, WeatherData.DataSource.CalculatedValue, -1));
                    //絶対湿度[kg/kg(DA)]
                    wRecord.SetData(WeatherRecord.RecordType.HumidityRatio, new WeatherData(mAir.HumidityRatio, WeatherData.DataSource.CalculatedValue, -1));

                    //風向
                    wData = new WeatherData(double.Parse(str.Substring(111, 3)), WeatherData.DataSource.MeasuredValue, -1);
                    wRecord.SetData(WeatherRecord.RecordType.WindDirection, wData);
                    
                    //風速[m/s]
                    wData = new WeatherData(double.Parse(str.Substring(114, 4))  / 10d, WeatherData.DataSource.MeasuredValue, -1);
                    wRecord.SetData(WeatherRecord.RecordType.WindSpeed, wData);

                    //雲量
                    double dbl = double.Parse(str.Substring(118, 2));
                    if (dbl == 99) wData = new WeatherData(double.Parse(str.Substring(118, 2)), WeatherData.DataSource.MissingValue, -1);
                    else wData = new WeatherData(double.Parse(str.Substring(118, 2)), WeatherData.DataSource.MeasuredValue, -1);
                    wRecord.SetData(WeatherRecord.RecordType.TotalSkyCover, wData);

                    //雲量2
                    wData = new WeatherData(double.Parse(str.Substring(120, 2)), WeatherData.DataSource.MeasuredValue, -1);
                    wRecord.SetData(WeatherRecord.RecordType.OpaqueSkyCover, wData);

                    //欠測補充
                    wRecord.FillMissingData();

                    //気象レコード追加
                    wdTable.AddWeatherRecord(wRecord);
                }
            }

            success = true;
            return wdTable;            
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
        private static WeatherData.DataSource getDSource1(string flag)
        {
            switch (flag)
            {
                case "0":
                    return WeatherData.DataSource.MeasuredValue;
                case "1":
                case "2":
                case "3":
                case "4":
                case "7":
                    return WeatherData.DataSource.CalculatedValue;
                case "5":
                case "8":
                    return WeatherData.DataSource.PredictedValue;
                default:
                    return WeatherData.DataSource.MissingValue;
            }
        }

        #endregion

    }
}
