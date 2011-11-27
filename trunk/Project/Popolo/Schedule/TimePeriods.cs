/* TimePeriods.cs
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

namespace Popolo.Schedule
{
    /// <summary>時間帯クラス</summary>
    [Serializable]
    public class TimePeriods : ITermStructure, ISerializable, ImmutableTimePeriods
    {

        #region<定数定義>

        /// <summary>シリアライズ用バージョン情報</summary>
        private double S_VERSION = 1.1;

        /// <summary>スケジュール定義する年（閏年に関係）</summary>
        private const int YEAR = 2001;

        /// <summary>スケジュール定義する月</summary>
        private const int MONTH = 1;

        /// <summary>スケジュール定義する日</summary>
        private const int DAY = 1;

        #endregion//定数定義

        #region<delegate定義>

        /// <summary>名称変更イベントハンドラ</summary>
        public delegate void NameChangeEventHandler(object sender, EventArgs e);

        /// <summary>時刻帯追加イベントハンドラ</summary>
        public delegate void TimePeriodAddEventHandler(object sender, TimePeriodsEventArgs e);

        /// <summary>時刻帯変更イベントハンドラ</summary>
        public delegate void TimePeriodChangeEventHandler(object sender, TimePeriodsEventArgs e);

        /// <summary>時刻帯削除イベントハンドラ</summary>
        public delegate void TimePeriodRemoveEventHandler(object sender, TimePeriodsEventArgs e);

        #endregion//delegate定義

        #region<イベント定義>

        /// <summary>名称変更イベント</summary>
        public event NameChangeEventHandler NameChangeEvent;

        /// <summary>時刻帯追加イベント</summary>
        public event TimePeriodAddEventHandler TimePeriodAddEvent;

        /// <summary>時刻帯変更イベント</summary>
        public event TimePeriodChangeEventHandler TimePeriodChangeEvent;

        /// <summary>時刻帯削除イベント</summary>
        public event TimePeriodRemoveEventHandler TimePeriodRemoveEvent;

        #endregion//イベント定義

        #region<enumerators>

        /// <summary>定義済みの時間帯</summary>
        public enum PredefinedTimePeriods
        {
            /// <summary>終日</summary>
            AllDay = 0,
            /// <summary>時間別</summary>
            Hourly = 1,
            /// <summary>営業時間</summary>
            BusinessHours = 2,
            /// <summary>昼夜</summary>
            DayAndNight = 3
        }

        #endregion//enumerators

        #region<Properties>

        /// <summary>時間帯IDを設定・取得する</summary>
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        /// <summary>時間帯名称を設定・取得する</summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                if (NameChangeEvent != null) NameChangeEvent(this, new EventArgs());
            }
        }

        /// <summary>定義した時間帯数を取得する</summary>
        public int Count
        {
            get
            {
                return timePeriodNames.Count;
            }
        }

        #endregion

        #region<Instance variables>

        /// <summary>ID</summary>
        private int id;

        /// <summary>名称</summary>
        private string name;

        /// <summary>時間帯名称</summary>
        private List<string> timePeriodNames = new List<string>();

        /// <summary>時間帯開始時刻（名称リスト数+1のリストとなる）</summary>
        private List<DateTime> timePeriodStartTimes = new List<DateTime>();

        #endregion//Instance variables

        #region<Constructor>

        /// <summary>Constructor</summary>
        public TimePeriods()
        {
            //初期化
            timePeriodNames.Add("終日");
            timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 0, 0, 0));
            timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY + 1, 0, 0, 0));
        }

        /// <summary>Constructor</summary>
        /// <param name="predefinedTimePeriods">定義済の時間帯</param>
        public TimePeriods(PredefinedTimePeriods predefinedTimePeriods)
        {
            Initialize(predefinedTimePeriods);
        }

        /// <summary>定義済の時間帯で初期化する</summary>
        /// <param name="predefinedTimePeriods">定義済の時間帯</param>
        public void Initialize(PredefinedTimePeriods predefinedTimePeriods)
        {
            timePeriodNames.Clear();
            timePeriodStartTimes.Clear();
            switch (predefinedTimePeriods)
            {
                case PredefinedTimePeriods.AllDay:
                    name = "終日";
                    timePeriodNames.Add("終日");
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 0, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY + 1, 0, 0, 0));
                    break;
                case PredefinedTimePeriods.Hourly:
                    name = "時間別";
                    timePeriodNames.Add("0時");
                    timePeriodNames.Add("1時");
                    timePeriodNames.Add("2時");
                    timePeriodNames.Add("3時");
                    timePeriodNames.Add("4時");
                    timePeriodNames.Add("5時");
                    timePeriodNames.Add("6時");
                    timePeriodNames.Add("7時");
                    timePeriodNames.Add("8時");
                    timePeriodNames.Add("9時");
                    timePeriodNames.Add("10時");
                    timePeriodNames.Add("11時");
                    timePeriodNames.Add("12時");
                    timePeriodNames.Add("13時");
                    timePeriodNames.Add("14時");
                    timePeriodNames.Add("15時");
                    timePeriodNames.Add("16時");
                    timePeriodNames.Add("17時");
                    timePeriodNames.Add("18時");
                    timePeriodNames.Add("19時");
                    timePeriodNames.Add("20時");
                    timePeriodNames.Add("21時");
                    timePeriodNames.Add("22時");
                    timePeriodNames.Add("23時");
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 0, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 1, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 2, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 3, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 4, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 5, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 6, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 7, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 8, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 9, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 10, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 11, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 12, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 13, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 14, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 15, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 16, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 17, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 18, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 19, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 20, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 21, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 22, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 23, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY + 1, 0, 0, 0));
                    break;
                case PredefinedTimePeriods.BusinessHours:
                    name = "営業時間帯";
                    timePeriodNames.Add("非営業時間");
                    timePeriodNames.Add("営業時間");
                    timePeriodNames.Add("非営業時間");
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 0, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 8, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 19, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY + 1, 0, 0, 0));
                    break;
                case PredefinedTimePeriods.DayAndNight:
                    name = "昼夜";
                    timePeriodNames.Add("夜間");
                    timePeriodNames.Add("昼間");
                    timePeriodNames.Add("夜間");
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 0, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 7, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY, 23, 0, 0));
                    timePeriodStartTimes.Add(new DateTime(YEAR, MONTH, DAY + 1, 0, 0, 0));
                    break;
            }
        }

        #endregion//Constructor

        #region public methods

        /// <summary>時間帯を追加する</summary>
        /// <param name="timePeriodName">時間帯名称</param>
        /// <param name="timePeriodStartTime">時間帯開始時刻</param>
        /// <returns>追加成功の真偽（指定時刻に既に時間帯が定義されている場合は失敗）</returns>
        public bool AddTimePeriod(string timePeriodName, DateTime timePeriodStartTime)
        {
            DateTime dTime = new DateTime(YEAR, MONTH, DAY, timePeriodStartTime.Hour, timePeriodStartTime.Minute, timePeriodStartTime.Second);
            int sIndex = 0;
            //適切な位置に挿入する
            for (int i = 1; i < timePeriodStartTimes.Count; i++)
            {
                int instPoint = dTime.CompareTo(timePeriodStartTimes[i]);
                if (instPoint < 0)
                {
                    sIndex = i;
                    timePeriodStartTimes.Insert(i, dTime);
                    timePeriodNames.Insert(i, timePeriodName);
                    break;
                }
                else if (instPoint == 0) return false;
            }
            //イベント通知
            if (TimePeriodAddEvent != null) TimePeriodAddEvent(this, new TimePeriodsEventArgs(sIndex, timePeriodName, timePeriodStartTimes[sIndex], timePeriodStartTimes[sIndex + 1]));
            return true;
        }

        /// <summary>時間帯名称を取得する</summary>
        /// <param name="timePeriodIndex">時間帯番号</param>
        /// <returns>時間帯名称</returns>
        public string GetTimePeriodName(int timePeriodIndex)
        {
            return timePeriodNames[timePeriodIndex];
        }

        /// <summary>時間帯情報を取得する</summary>
        /// <param name="timePeriodIndex">時間帯番号</param>
        /// <param name="timePeriodName">時間帯名称</param>
        /// <param name="timePeriodStartTime">時間帯開始時刻</param>
        /// <param name="timePeriodEndTime">時間帯終了時刻</param>
        public void GetTimePeriod(int timePeriodIndex, out string timePeriodName, out DateTime timePeriodStartTime, out DateTime timePeriodEndTime)
        {
            timePeriodName = timePeriodNames[timePeriodIndex];
            timePeriodStartTime = timePeriodStartTimes[timePeriodIndex];
            timePeriodEndTime = timePeriodStartTimes[timePeriodIndex + 1].AddMinutes(-1);
        }

        /// <summary>時間帯情報を取得する</summary>
        /// <param name="timePeriodName">時間帯名称</param>
        /// <param name="timePeriodStartDTimes">時間帯開始時刻リスト</param>
        /// <param name="timePeriodEndDTimes">時間帯終了リスト</param>
        public void GetTimePeriods(string timePeriodName, out DateTime[] timePeriodStartDTimes, out DateTime[] timePeriodEndDTimes)
        {
            //該当する時間帯が存在しない場合
            if (!timePeriodNames.Contains(timePeriodName))
            {
                timePeriodStartDTimes = new DateTime[0];
                timePeriodEndDTimes = new DateTime[0];
                return;
            }
            //時間帯が存在する場合はすべての期間を調べる
            List<DateTime> dtStart = new List<DateTime>();
            List<DateTime> dtEnd = new List<DateTime>();
            for (int i = 0; i < timePeriodNames.Count; i++)
            {
                if (timePeriodNames[i] == timePeriodName)
                {
                    dtStart.Add(this.timePeriodStartTimes[i]);
                    dtEnd.Add(this.timePeriodStartTimes[i + 1].AddDays(-1));
                }
            }
            //配列化
            timePeriodStartDTimes = dtStart.ToArray();
            timePeriodEndDTimes = dtEnd.ToArray();
        }

        /// <summary>時間帯を削除する</summary>
        /// <param name="timePeriodIndex">時間帯番号</param>
        public bool RemoveTimePeriod(int timePeriodIndex)
        {
            //時間帯が一つしか定義されていない場合は失敗
            if (timePeriodNames.Count - 1 < timePeriodIndex) return false;
            else
            {
                DateTime dtStart, dtEnd;
                string sName = timePeriodNames[timePeriodIndex];
                //時間帯名称を削除
                timePeriodNames.RemoveAt(timePeriodIndex);
                //時間帯開始終了時刻を更新****************
                //先頭時間帯の場合
                if (timePeriodIndex == 0)
                {
                    dtStart = timePeriodStartTimes[0];
                    dtEnd = timePeriodStartTimes[1];
                    timePeriodStartTimes.RemoveAt(1);
                }
                //最終時間帯の場合
                else if (timePeriodIndex == timePeriodNames.Count)
                {
                    dtStart = timePeriodStartTimes[timePeriodIndex];
                    dtEnd = timePeriodStartTimes[timePeriodIndex + 1];
                    timePeriodStartTimes.RemoveAt(timePeriodIndex);
                }
                //その他の時刻帯の場合
                else
                {
                    //中間時刻を計算する
                    dtStart = timePeriodStartTimes[timePeriodIndex];
                    dtEnd = timePeriodStartTimes[timePeriodIndex + 1];
                    TimeSpan tSpan = dtEnd - dtStart;
                    DateTime dtMiddle = dtStart.AddSeconds(tSpan.Seconds / 2);
                    timePeriodStartTimes.RemoveAt(timePeriodIndex + 1);
                    timePeriodStartTimes[timePeriodIndex] = dtMiddle;
                }
                //イベント通知
                if (TimePeriodRemoveEvent != null) TimePeriodRemoveEvent(this, new TimePeriodsEventArgs(timePeriodIndex, sName, dtStart, dtEnd));
                //削除成功
                return true;
            }
        }

        /// <summary>時間帯名称を変更する</summary>
        /// <param name="timePeriodIndex">時間帯番号</param>
        /// <param name="timePeriodName">時間帯名称</param>
        /// <returns>名称変更成功の真偽</returns>
        public bool ChangeTimePeriodName(int timePeriodIndex, string timePeriodName)
        {
            //時間帯番号範囲外指定の場合は終了
            if (timePeriodNames.Count - 1 < timePeriodIndex) return false;
            //時間帯名称を変更
            timePeriodNames[timePeriodIndex] = timePeriodName;
            //イベント通知
            if (TimePeriodChangeEvent != null) TimePeriodChangeEvent(this, new TimePeriodsEventArgs(timePeriodIndex, timePeriodName, timePeriodStartTimes[timePeriodIndex], timePeriodStartTimes[timePeriodIndex + 1]));
            return true;
        }

        /// <summary>時間帯端部時刻を変更する</summary>
        /// <param name="timePeriodIndex">時間帯番号</param>
        /// <param name="timePeriodDateTime">時間帯端部時刻</param>
        /// <param name="isStartDateTime">時間帯開始時刻の設定か否か</param>
        /// <returns>時間帯端部時刻を変更成功の真偽</returns>
        public bool ChangeTimePeriodDateTime(int timePeriodIndex, DateTime timePeriodDateTime, bool isStartDateTime)
        {
            //時間帯開始時刻の場合
            if (isStartDateTime)
            {
                //最初の時間帯区切りDateTimeの場合は終了
                if (timePeriodNames.Count - 1 < timePeriodIndex || timePeriodIndex <= 0) return false;
                //時間帯変更可能範囲外の場合は終了
                if (timePeriodDateTime <= timePeriodStartTimes[timePeriodIndex - 1] || timePeriodStartTimes[timePeriodIndex + 1] <= timePeriodDateTime) return false;
                //時間帯開始時刻を変更する
                timePeriodStartTimes[timePeriodIndex] = timePeriodDateTime;
            }
            //時間帯終了時刻の場合
            else
            {
                //最後の時間帯区切りDateTimeの場合は終了
                if (timePeriodNames.Count - 2 < timePeriodIndex || timePeriodIndex < 0) return false;
                //時間帯変更可能範囲外の場合は終了
                timePeriodDateTime = timePeriodDateTime.AddMinutes(1);
                if (timePeriodDateTime <= timePeriodStartTimes[timePeriodIndex] || timePeriodStartTimes[timePeriodIndex + 2] <= timePeriodDateTime) return false;
                //時間帯終了時刻を変更する
                timePeriodStartTimes[timePeriodIndex + 1] = timePeriodDateTime;
            }
            
            //イベント通知
            if (TimePeriodChangeEvent != null) TimePeriodChangeEvent(this, new TimePeriodsEventArgs(timePeriodIndex, timePeriodNames[timePeriodIndex], timePeriodStartTimes[timePeriodIndex], timePeriodStartTimes[timePeriodIndex + 1]));
            return true;
        }

        #endregion

        #region<ITermインターフェース実装>

        /// <summary>時間帯名称リストを取得する</summary>
        /// <returns>時間帯名称リスト</returns>
        public string[] GetTermNames()
        {
            //時間帯名称リストを保持
            List<string> sNames = new List<string>();
            foreach (string sName1 in timePeriodNames)
            {
                //重複確認
                bool hasName = false;
                foreach (string sName2 in sNames)
                {
                    //重複している場合はbreak;
                    if (sName2 == sName1)
                    {
                        hasName = true;
                        break;
                    }
                }
                //未登録の名称であれば登録
                if (!hasName) sNames.Add(sName1);
            }
            return sNames.ToArray();
        }

        /// <summary>時刻を指定して時間帯名称を取得する</summary>
        /// <param name="dateTime">時刻</param>
        /// <returns>時間帯名称</returns>
        public string GetTermName(DateTime dateTime)
        {
            DateTime dt = new DateTime(YEAR, MONTH, DAY, dateTime.Hour, dateTime.Minute, dateTime.Second);
            for (int i = 0; i < timePeriodStartTimes.Count; i++)
            {
                if (dt < timePeriodStartTimes[i]) return timePeriodNames[i - 1];
            }
            throw new Exception("スケジュール定義範囲外");
        }

        #endregion//ITermインターフェース実装

        #region<ICloneableインターフェース実装>

        /// <summary>TimePeriodsクラスの複製を返す</summary>
        /// <returns>TimePeriodsクラスの複製</returns>
        public object Clone()
        {
            TimePeriods timePeriods = (TimePeriods)this.MemberwiseClone();
            timePeriods.timePeriodNames = new List<string>();
            timePeriods.timePeriodStartTimes = new List<DateTime>();
            foreach (string sName in timePeriodNames) timePeriods.timePeriodNames.Add(sName);
            foreach (DateTime dTime in timePeriodStartTimes) timePeriods.timePeriodStartTimes.Add(dTime);
            //イベントを初期化
            timePeriods.NameChangeEvent = null;
            timePeriods.TimePeriodAddEvent = null;
            timePeriods.TimePeriodChangeEvent = null;
            timePeriods.TimePeriodRemoveEvent = null;
            return timePeriods;
        }

        #endregion//ICloneableインターフェース実装

        #region<シリアライズ関連の処理>

        /// <summary>デシリアライズ用Constructor</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected TimePeriods(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            double version = sInfo.GetDouble("S_Version");

            //ID
            if (1.0 < version) id = sInfo.GetInt32("ID");
            //名称
            name = sInfo.GetString("Name");
            //時間帯名称リスト
            timePeriodNames.AddRange((string[])sInfo.GetValue("TimePeriodNames", typeof(string[])));
            //時間帯開始時刻リスト
            timePeriodStartTimes.AddRange((DateTime[])sInfo.GetValue("TimePeriodStartTimes", typeof(DateTime[])));    
        }

        /// <summary>TimePeriodsシリアル化処理</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //バージョン情報
            info.AddValue("S_Version", S_VERSION);

            //ID
            info.AddValue("ID", id);
            //名称
            info.AddValue("Name", name);
            //時間帯名称リスト
            info.AddValue("TimePeriodNames", timePeriodNames.ToArray());
            //時間帯開始時刻リスト
            info.AddValue("TimePeriodStartTimes", timePeriodStartTimes.ToArray());
        }

        #endregion//シリアライズ関連の処理

    }

    /// <summary>時間帯関連のEventArgs</summary>
    public class TimePeriodsEventArgs : EventArgs
    {

        #region<Instance variables>

        /// <summary>時間帯番号</summary>
        private int timePeriodIndex;

        /// <summary>時間帯名称</summary>
        private string timePeriodName;

        /// <summary>時間帯開始時刻</summary>
        private DateTime timePeriodStart;

        /// <summary>時間帯終了時刻</summary>
        private DateTime timePeriodEnd;

        #endregion//Instance variables

        #region<Properties>

        /// <summary>時間帯番号を取得する</summary>
        public int TimePeriodIndex
        {
            get
            {
                return timePeriodIndex;
            }
        }

        /// <summary>時間帯名称を取得する</summary>
        public string TimePeriodName
        {
            get
            {
                return timePeriodName;
            }
        }

        /// <summary>時間帯開始時刻を取得する</summary>
        public DateTime TimePeriodStart
        {
            get
            {
                return timePeriodStart;
            }
        }

        /// <summary>時間帯終了時刻を取得する</summary>
        public DateTime TimePeriodEnd
        {
            get
            {
                return timePeriodEnd;
            }
        }

        #endregion//Properties

        #region<Constructor>

        /// <summary>Constructor</summary>
        /// <param name="timePeriodIndex">時間帯番号</param>
        /// <param name="timePeriodName">時間帯名称</param>
        /// <param name="timePeriodStart">時間帯開始時刻</param>
        /// <param name="timePeriodEnd">時間帯終了時刻</param>
        public TimePeriodsEventArgs(int timePeriodIndex, string timePeriodName, DateTime timePeriodStart, DateTime timePeriodEnd)
        {
            this.timePeriodIndex = timePeriodIndex;
            this.timePeriodName = timePeriodName;
            this.timePeriodStart = timePeriodStart;
            this.timePeriodEnd = timePeriodEnd;
        }

        #endregion//Constructor

    }

    /// <summary>読み取り専用TimePeriodsインターフェース</summary>
    public interface ImmutableTimePeriods : ImmutableITermStructure
    {
        /// <summary>定義した時間帯数を取得する</summary>
        int Count
        {
            get;
        }

        /// <summary>時間帯名称を取得する</summary>
        /// <param name="timePeriodIndex">時間帯番号</param>
        /// <returns>時間帯名称</returns>
        string GetTimePeriodName(int timePeriodIndex);

        /// <summary>時間帯情報を取得する</summary>
        /// <param name="timePeriodIndex">時間帯番号</param>
        /// <param name="timePeriodName">時間帯名称</param>
        /// <param name="timePeriodStartTime">時間帯開始時刻</param>
        /// <param name="timePeriodEndTime">時間帯終了時刻</param>
        void GetTimePeriod(int timePeriodIndex, out string timePeriodName, out DateTime timePeriodStartTime, out DateTime timePeriodEndTime);

        /// <summary>時間帯情報を取得する</summary>
        /// <param name="timePeriodName">時間帯名称</param>
        /// <param name="timePeriodStartDTimes">時間帯開始時刻リスト</param>
        /// <param name="timePeriodEndDTimes">時間帯終了リスト</param>
        void GetTimePeriods(string timePeriodName, out DateTime[] timePeriodStartDTimes,
            out DateTime[] timePeriodEndDTimes);

    }

}
