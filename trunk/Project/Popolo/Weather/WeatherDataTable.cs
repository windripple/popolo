/* WeatherDataTable.cs
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

using System.Runtime.Serialization;

namespace Popolo.Weather
{
    /// <summary>気象データテーブル（気象レコードの集合体）</summary>
    [Serializable]
    public class WeatherDataTable : ICloneable, ISerializable
    {

        #region 定数宣言

        /// <summary>シリアライズバージョン</summary>
        const double S_VERSION = 1.0;

        #endregion

        #region インスタンス変数

        /// <summary>気象レコードリスト</summary>
        List<WeatherRecord> wdList = new List<WeatherRecord>();

        /// <summary>気象地点情報</summary>
        private LocationInformation location = new LocationInformation();

        #endregion

        #region プロパティ

        /// <summary>気象地点情報を設定・取得する</summary>
        public LocationInformation Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }

        /// <summary>気象レコードのデータ数を取得する</summary>
        public int WeatherRecordNumber
        {
            get
            {
                return wdList.Count;
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        public WeatherDataTable() { }

        /// <summary>コンストラクタ</summary>
        /// <param name="weatherRecord">気象レコード</param>
        public WeatherDataTable(WeatherRecord[] weatherRecord)
        {
            AddWeatherRecord(weatherRecord);
        }

        #endregion

        #region publicメソッド

        /// <summary>気象レコードを取得する</summary>
        /// <param name="recordIndex">気象レコード番号</param>
        /// <returns>気象レコード</returns>
        public ImmutableWeatherRecord GetWeatherRecord(int recordIndex)
        {
            return wdList[recordIndex];
        }

        /// <summary>気象レコードを追加する</summary>
        /// <param name="weatherRecord">気象レコード</param>
        /// <remarks>
        /// 一旦追加した気象レコードは変更不可
        /// 変更したい場合は削除した後に新規に追加する
        /// </remarks>
        public void AddWeatherRecord(WeatherRecord weatherRecord)
        {
            wdList.Add(weatherRecord);
        }

        /// <summary>気象レコードを追加する</summary>
        /// <param name="weatherRecord">気象レコード配列</param>
        public void AddWeatherRecord(WeatherRecord[] weatherRecord)
        {
            wdList.AddRange(weatherRecord);
        }

        /// <summary>気象レコードを削除する</summary>
        /// <param name="recordIndex">気象レコード番号</param>
        public void RemoveWeatherRecord(int recordIndex)
        {
            wdList.RemoveAt(recordIndex);
        }

        /// <summary>気象レコードを全削除する</summary>
        public void ClearWeatherRecord()
        {
            wdList.Clear();
        }

        /// <summary>特定年のデータテーブルを取得する</summary>
        /// <param name="year">年</param>
        /// <returns>特定年のデータテーブル</returns>
        public WeatherDataTable GetAnnualDataTable(int year)
        {
            WeatherDataTable wdTable = new WeatherDataTable();
            foreach (WeatherRecord wr in wdList)
            {
                if (wr.DataDTime.Year == year) wdTable.AddWeatherRecord(wr);
            }
            return wdTable;
        }

        /// <summary>1時間間隔のデータテーブルに変換する</summary>
        /// <returns>1時間間隔のデータテーブル</returns>
        /// <remarks>不足するデータは線形補間する</remarks>
        public WeatherDataTable ConvertToHoulyDataTable()
        {
            List<WeatherRecord> wdl = new List<WeatherRecord>();
            WeatherRecord wr1 = wdList[0];
            DateTime cDTime = new DateTime(wr1.DataDTime.Year, wr1.DataDTime.Month, wr1.DataDTime.Day,
                wr1.DataDTime.Hour, wr1.DataDTime.Minute, wr1.DataDTime.Second);
            for (int i = 1; i < wdList.Count; i++)
            {
                WeatherRecord wr2 = wdList[i];
                while (cDTime <= wr2.DataDTime)
                {
                    //補間および追加処理
                    wdl.Add(interpolateWRecord(wr1, wr2, cDTime));
                    cDTime = cDTime.AddHours(1);
                }
                wr1 = wr2;
            }

            WeatherDataTable wdTable = new WeatherDataTable(wdl.ToArray());
            wdTable.Location = (LocationInformation)this.location.Clone();
            return wdTable;
        }

        #endregion

        #region privateメソッド

        /// <summary>気象レコードを日付順にソートする</summary>
        public void SortRecord()
        {
            wdList.Sort();
        }

        /// <summary>気象レコードを線形補間する</summary>
        /// <param name="wRecord1">気象レコード1</param>
        /// <param name="wRecord2">気象レコード2</param>
        /// <param name="dTime">補間する時刻</param>
        /// <returns>補間した気象レコード</returns>
        private WeatherRecord interpolateWRecord(WeatherRecord wRecord1, WeatherRecord wRecord2, DateTime dTime)
        {
            //両端と一致する場合は複製を返す
            if (wRecord1.DataDTime == dTime) return (WeatherRecord)wRecord1.Clone();
            else if (wRecord2.DataDTime == dTime)return (WeatherRecord)wRecord2.Clone();

            //その他の場合は補間処理
            WeatherRecord wRecord = new WeatherRecord();
            double ts1 = (dTime - wRecord1.DataDTime).TotalHours;
            double ts2 = (wRecord2.DataDTime - dTime).TotalHours;
            ts1 = ts1 / (ts1 + ts2);
            ts2 = 1.0 - ts1;
            foreach (WeatherRecord.RecordType rt in Enum.GetValues(typeof(WeatherRecord.RecordType)))
            {
                WeatherData wd1 = wRecord1.GetData(rt);
                WeatherData wd2 = wRecord2.GetData(rt);
                WeatherData wd = new WeatherData();

                //いずれかが欠測の場合は欠測
                if (wd1.Source == WeatherData.DataSource.MissingValue ||
                    wd2.Source == WeatherData.DataSource.MissingValue) wd.Source = WeatherData.DataSource.MissingValue;
                //その他の場合は推定（補間）値 //単純な線形補間では問題がある要素もある。直せ!
                else wd.Source = WeatherData.DataSource.PredictedValue;
                wd.Value = wd1.Value * ts1 + wd2.Value * ts2;

                wRecord.SetData(rt, wd);
            }

            return wRecord;
        }

        #endregion

        #region ISerializableインターフェース実装

        /// <summary>デシリアライズ用コンストラクタ</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected WeatherDataTable(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            double version = sInfo.GetDouble("S_Version");

            //気象レコードリスト
            wdList.AddRange((WeatherRecord[])sInfo.GetValue("WdList", typeof(WeatherRecord[])));
            //地点情報
            location = (LocationInformation)sInfo.GetValue("Location", typeof(LocationInformation));
         }

        /// <summary>シリアル化処理</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //バージョン情報
            info.AddValue("S_Version", S_VERSION);

            //気象レコードリスト
            info.AddValue("WdList", wdList.ToArray());
            //地点情報
            info.AddValue("Location", location);
        }

        #endregion

        #region ICloneableインターフェース実装

        /// <summary>WeatherDataTableの複製を返す</summary>
        /// <returns>WeatherDataTableの複製</returns>
        public object Clone()
        {
            WeatherDataTable wt = (WeatherDataTable)this.MemberwiseClone();

            wt.wdList = new List<WeatherRecord>();
            foreach (WeatherRecord wr in wdList)
            {
                wt.wdList.Add((WeatherRecord)wr.Clone());
            }

            return wt;
        }

        #endregion

    }
}
