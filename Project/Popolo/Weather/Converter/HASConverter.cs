/* HASConverter.cs
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
    /// <summary>EESLISM用気象データを変換する</summary>
    public static class HASConverter
    {

        #region Properties

        /// <summary>気象データ名称を取得する</summary>
        public static string Name
        {
            get
            {
                return "HASP形式気象データ";
            }
        }

        /// <summary>第5行のデータに雲量を利用するか否かを設定する</summary>
        public static bool UseCCRate
        {
            get;
            set;
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
                    string buff;
                    DateTime dTime;

                    //第1行目からデータがスタート、EES形式との違い
                    //sReader.ReadLine();

                    for (int i = 0; i < 365; i++)
                    {
                        WeatherRecord[] wRecords = new WeatherRecord[24];

                        //年月日特定
                        buff = sReader.ReadLine();
                        int year = int.Parse(buff.Substring(72, 2));
                        int month = int.Parse(buff.Substring(74, 2));
                        int day = int.Parse(buff.Substring(76, 2));
                        if (year < 50) year += 2000;
                        else year += 1900;
                        dTime = new DateTime(year, month, day, 0, 0, 0);
                        for (int j = 0; j < 24; j++)
                        {
                            wRecords[j] = new WeatherRecord();
                            wRecords[j].DataDTime = dTime;
                            dTime = dTime.AddHours(1);
                        }

                        //乾球温度
                        for (int j = 0; j < 24; j++)
                        {
                            string bf = buff.Substring(j * 3, 3);
                            WeatherData wData = new WeatherData();
                            wData.Source = WeatherData.DataSource.CalculatedValue;
                            wData.Value = (double.Parse(bf) - 500d) / 10d;
                            wRecords[j].SetData(WeatherRecord.RecordType.DryBulbTemperature, wData);
                        }

                        //絶対湿度
                        buff = sReader.ReadLine();
                        for (int j = 0; j < 24; j++)
                        {
                            string bf = buff.Substring(j * 3, 3);
                            WeatherData wData = new WeatherData();
                            wData.Source = WeatherData.DataSource.CalculatedValue;
                            wData.Value = double.Parse(bf) / 10000d;
                            wRecords[j].SetData(WeatherRecord.RecordType.HumidityRatio, wData);
                        }

                        //法線面直達日射[kcal/m2-h]
                        buff = sReader.ReadLine();
                        for (int j = 0; j < 24; j++)
                        {
                            string bf = buff.Substring(j * 3, 3);
                            WeatherData wData = new WeatherData();
                            wData.Source = WeatherData.DataSource.CalculatedValue;
                            wData.Value = double.Parse(bf) * 1.163; // [W/m2]に変換
                            wRecords[j].SetData(WeatherRecord.RecordType.DirectNormalRadiation, wData);
                        }

                        //水平面天空日射[kcal/m2-h]
                        buff = sReader.ReadLine();
                        for (int j = 0; j < 24; j++)
                        {
                            string bf = buff.Substring(j * 3, 3);
                            WeatherData wData = new WeatherData();
                            wData.Source = WeatherData.DataSource.CalculatedValue;
                            wData.Value = double.Parse(bf) * 1.163; // [W/m2]に変換
                            wRecords[j].SetData(WeatherRecord.RecordType.DiffuseHorizontalRadiation, wData);
                        }

                        //雲量[-] (10分比)
                        if (UseCCRate)
                        {
                            buff = sReader.ReadLine();
                            for (int j = 0; j < 24; j++)
                            {
                                string bf = buff.Substring(j * 3, 3);
                                WeatherData wData = new WeatherData();
                                wData.Source = WeatherData.DataSource.CalculatedValue;
                                wData.Value = int.Parse(bf);
                                wRecords[j].SetData(WeatherRecord.RecordType.TotalSkyCover, wData);
                            }
                        }
                        //夜間放射量[kcal/m2-h]
                        else
                        {
                            buff = sReader.ReadLine();
                            for (int j = 0; j < 24; j++)
                            {
                                string bf = buff.Substring(j * 3, 3);
                                WeatherData wData = new WeatherData();
                                wData.Source = WeatherData.DataSource.CalculatedValue;
                                wData.Value = double.Parse(bf) * 1.163; // [W/m2]に変換
                                wRecords[j].SetData(WeatherRecord.RecordType.NocturnalRadiation, wData);
                            }
                        }

                        //風向[-] (16分比)
                        buff = sReader.ReadLine();
                        for (int j = 0; j < 24; j++)
                        {
                            string bf = buff.Substring(j * 3, 3);
                            WeatherData wData = new WeatherData();
                            wData.Source = WeatherData.DataSource.CalculatedValue;
                            wData.Value = convertFromWindDirectionCode(int.Parse(bf));
                            wRecords[j].SetData(WeatherRecord.RecordType.WindDirection, wData);
                        }

                        //風速[m/s]
                        buff = sReader.ReadLine();
                        for (int j = 0; j < 24; j++)
                        {
                            string bf = buff.Substring(j * 3, 3);
                            WeatherData wData = new WeatherData();
                            wData.Source = WeatherData.DataSource.CalculatedValue;
                            wData.Value = double.Parse(bf) / 10d;
                            wRecords[j].SetData(WeatherRecord.RecordType.WindSpeed, wData);
                        }

                        wdTable.AddWeatherRecord(wRecords);
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
            //書き出しストリームを用意
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
            
            sWriter.Close();

            success = true;
        }

        #endregion

        #region staticメソッド

        /// <summary>supwデータのための文字列を作成する</summary>
        /// <param name="wdTable">気象データテーブル</param>
        /// <returns>supwデータのための文字列</returns>
        public static string MakeSupwData(WeatherDataTable wdTable)
        {
            StringBuilder sBuilder = new StringBuilder();
            //1時間間隔のデータに変更
            WeatherDataTable houlyWDTable = wdTable.ConvertToHoulyDataTable();
            
            //名称設定
            sBuilder.AppendLine(houlyWDTable.Location.EnglishName);

            double maxT, minT, dayAveT, monthAveT, yearAveT, maxDayAveT;
            int maxDayAveDay = 0;
            ImmutableWeatherRecord wr = houlyWDTable.GetWeatherRecord(0);
            DateTime cTime = wr.DataDTime;
            maxT = minT = dayAveT = wr.GetData(WeatherRecord.RecordType.DryBulbTemperature).Value;
            monthAveT = yearAveT = 0;

            int day = 0;
            maxDayAveT = -999;
            for (int i = 1; i < houlyWDTable.WeatherRecordNumber; i++)
            {
                wr = houlyWDTable.GetWeatherRecord(i);
                double temp = wr.GetData(WeatherRecord.RecordType.DryBulbTemperature).Value;
                maxT = Math.Max(temp, maxT);
                minT = Math.Min(temp, minT);

                //日付が変わった場合
                if (cTime.Day != wr.DataDTime.Day)
                {
                    monthAveT += dayAveT;
                    dayAveT = dayAveT / 24d;
                    if (maxDayAveT < dayAveT)
                    {
                        maxDayAveT = dayAveT;
                        maxDayAveDay = cTime.DayOfYear;
                    }
                    dayAveT = 0;
                }

                //月が変わった場合
                if (cTime.Month != wr.DataDTime.Month)
                {
                    yearAveT += monthAveT;
                    sBuilder.Append(" " + (monthAveT / day).ToString("F1"));
                    monthAveT = 0;
                    day = 0;
                }

                dayAveT += temp;
                cTime = wr.DataDTime;
                day++;
            }
            sBuilder.AppendLine();
            sBuilder.Append(" " + maxDayAveDay);
            sBuilder.Append(" " + (yearAveT / houlyWDTable.WeatherRecordNumber).ToString("F1"));
            sBuilder.Append(" " + (maxT - minT).ToString("F1"));

            return sBuilder.ToString();
        }

        /// <summary>実数を3桁空白右揃えで表現する</summary>
        /// <param name="value">実数</param>
        /// <returns>文字列</returns>
        private static string convertTo3WordString(double value)
        {
            if (1000 <= value) return "999";
            else if (value <= 0) return "  0";
            else return String.Format("{0, 3}", value.ToString("F0"));
        }

        /// <summary>曜日を曜日コードに変換する</summary>
        /// <param name="dw">曜日</param>
        /// <returns>曜日コード</returns>
        private static string convertToDayOfWeekCode(DayOfWeek dw)
        {
            switch (dw)
            {
                case DayOfWeek.Sunday:
                    return "1";
                case DayOfWeek.Monday:
                    return "2";
                case DayOfWeek.Tuesday:
                    return "3";
                case DayOfWeek.Wednesday:
                    return "4";
                case DayOfWeek.Thursday:
                    return "5";
                case DayOfWeek.Friday:
                    return "6";
                case DayOfWeek.Saturday:
                    return "7";
                default:
                    return "0";
            }
        }

        /// <summary>風向を風向コードに変換する</summary>
        /// <param name="windDirection">風向</param>
        /// <returns>風向コード（16方位）</returns>
        private static string convertToWindDirectionCode(double windDirection)
        {
            if (windDirection < -168.75) return " 16";
            else if (windDirection < -146.25) return "  1";
            else if (windDirection < -123.75) return "  2";
            else if (windDirection < -101.25) return "  3";
            else if (windDirection < -78.75) return "  4";
            else if (windDirection < -56.25) return "  5";
            else if (windDirection < -33.75) return "  6";
            else if (windDirection < -11.25) return "  7";
            else if (windDirection < 11.25) return "  8";
            else if (windDirection < 33.75) return "  9";
            else if (windDirection < 56.25) return " 11";
            else if (windDirection < 78.75) return " 12";
            else if (windDirection < 101.25) return " 13";
            else if (windDirection < 123.75) return " 14";
            else if (windDirection < 146.25) return " 15";
            else if (windDirection < 168.75) return " 16";
            else return "  0";
        }

        /// <summary>風向コードを風向[degree]に変換する</summary>
        /// <param name="windowDirectionCode">風向コード（16方位）</param>
        /// <returns>風向[degree]</returns>
        private static double convertFromWindDirectionCode(int windowDirectionCode)
        {
            //南
            if (windowDirectionCode == 8) return 0;
            //北
            else if(windowDirectionCode == 16) return -180;
            //その他
            else return 22.5 * (windowDirectionCode - 8);
        }

        /// <summary>24時間分のデータを書き出す</summary>
        /// <param name="sWriter">書き出しストリーム</param>
        /// <param name="wRecord">24時間分のデータ</param>
        private static bool output24Data(StreamWriter sWriter, ImmutableWeatherRecord[] wRecord)
        {
            if (wRecord.Length != 24) throw new Exception();

            //年月日文字列
            DateTime dt = wRecord[0].DataDTime;
            string dTime = dt.Year.ToString().Substring(2);
            dTime += String.Format("{0, 2}", dt.Month.ToString());
            dTime += String.Format("{0, 2}", dt.Day.ToString());
            dTime += convertToDayOfWeekCode(dt.DayOfWeek);

            //気温[CDB]書き出し
            for (int i = 0; i < 24; i++)
            {
                WeatherData wd = wRecord[i].GetData(WeatherRecord.RecordType.DryBulbTemperature);
                if (wd.Source == WeatherData.DataSource.MissingValue) return false;
                else sWriter.Write(convertTo3WordString(wd.Value * 10 + 500));
            }
            sWriter.WriteLine(dTime + "1");

            //絶対湿度[kg/kg(DA)]書き出し
            for (int i = 0; i < 24; i++)
            {
                WeatherData wd = wRecord[i].GetData(WeatherRecord.RecordType.HumidityRatio);
                if (wd.Source == WeatherData.DataSource.MissingValue) return false;
                else sWriter.Write(convertTo3WordString(wd.Value * 10000));
            }
            sWriter.WriteLine(dTime + "2");

            //法線面直達日射量[kcal/m2h]書き出し
            for (int i = 0; i < 24; i++)
            {
                WeatherData wd = wRecord[i].GetData(WeatherRecord.RecordType.DirectNormalRadiation);
                if (wd.Source == WeatherData.DataSource.MissingValue) return false;
                else sWriter.Write(convertTo3WordString(wd.Value * 0.859999));
            }
            sWriter.WriteLine(dTime + "3");

            //水平面天空日射量[kcal/m2h]書き出し
            for (int i = 0; i < 24; i++)
            {
                WeatherData wd = wRecord[i].GetData(WeatherRecord.RecordType.DiffuseHorizontalRadiation);
                if (wd.Source == WeatherData.DataSource.MissingValue) return false;
                else sWriter.Write(convertTo3WordString(wd.Value * 0.859999));
            }
            sWriter.WriteLine(dTime + "4");

            if (UseCCRate)
            {
                //雲量(10分比)書き出し
                for (int i = 0; i < 24; i++)
                {
                    WeatherData wd = wRecord[i].GetData(WeatherRecord.RecordType.TotalSkyCover);
                    if (wd.Source == WeatherData.DataSource.MissingValue) sWriter.Write("  0");
                    else sWriter.Write(convertTo3WordString(wd.Value * 10));
                }
            }
            //夜間放射量[kcal/m2h]
            else
            {
                for (int i = 0; i < 24; i++)
                {
                    WeatherData wd = wRecord[i].GetData(WeatherRecord.RecordType.NocturnalRadiation);
                    if (wd.Source == WeatherData.DataSource.MissingValue) return false;
                    else sWriter.Write(convertTo3WordString(wd.Value * 0.859999));
                }
            }
            sWriter.WriteLine(dTime + "5");

            //風向(16方位)書き出し
            for (int i = 0; i < 24; i++)
            {
                WeatherData wd = wRecord[i].GetData(WeatherRecord.RecordType.WindDirection);
                if (wd.Source == WeatherData.DataSource.MissingValue) sWriter.Write("  0");
                else sWriter.Write(convertToWindDirectionCode(wd.Value));
            }
            sWriter.WriteLine(dTime + "6");

            //風速[m/s]書き出し
            for (int i = 0; i < 24; i++)
            {
                WeatherData wd = wRecord[i].GetData(WeatherRecord.RecordType.WindSpeed);
                if (wd.Source == WeatherData.DataSource.MissingValue) sWriter.Write("  0");
                else sWriter.Write(convertTo3WordString(wd.Value * 10));
            }
            sWriter.WriteLine(dTime + "7");
            return true;
        }

        #endregion

    }
}
