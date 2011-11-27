/* Days.cs
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
    /// <summary>曜日クラス</summary>
    [Serializable]
    public class Days : ITermStructure, ISerializable, ImmutableDays
    {

        #region<定数定義>

        /// <summary>シリアライズ用バージョン情報</summary>
        private double S_VERSION = 1.1;

        /// <summary>スケジュール定義する年（閏年に関係）</summary>
        private const int YEAR = 2001;

        #endregion//定数定義

        #region<delegate定義>

        /// <summary>名称変更イベントハンドラ</summary>
        public delegate void NameChangeEventHandler(object sender, EventArgs e);

        /// <summary>曜日変更イベントハンドラ</summary>
        public delegate void DayChangeEventHandler(object sender, DaysEventArgs e);

        #endregion//delegate定義

        #region<イベント定義>

        /// <summary>名称変更イベント</summary>
        public event NameChangeEventHandler NameChangeEvent;

        /// <summary>曜日変更イベント</summary>
        public event DayChangeEventHandler DayChangeEvent;

        #endregion//イベント定義

        #region<enumerators>

        /// <summary>定義済みの曜日</summary>
        public enum PredefinedDays
        {
            /// <summary>全曜日</summary>
            AllWeek = 0,
            /// <summary>週末</summary>
            WeekDayAndWeekEnd = 1,
            /// <summary>曜日別</summary>
            OneWeek = 2,
        }

        #endregion//enumerators

        #region<Properties>

        /// <summary>IDを設定・取得する</summary>
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

        /// <summary>名称を設定・取得する</summary>
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

        #endregion

        #region Instance variables

        /// <summary>ID</summary>
        private int id;

        /// <summary>名称</summary>
        private string name;

        /// <summary>グループ名称</summary>
        private string[] termNames = new string[7];

        #endregion

        #region<Constructor>

        /// <summary>Constructor</summary>
        public Days()
        {
            //初期化
            Initialize(PredefinedDays.OneWeek);
        }

        /// <summary>Constructor</summary>
        /// <param name="predefinedDays">定義済みの曜日</param>
        public Days(PredefinedDays predefinedDays)
        {
            //初期化
            Initialize(predefinedDays);
        }

        /// <summary>定義済の季節で初期化する</summary>
        /// <param name="predefinedDays">定義済みの季節</param>
        public void Initialize(PredefinedDays predefinedDays)
        {
            switch (predefinedDays)
            {
                case PredefinedDays.WeekDayAndWeekEnd:
                    name = "平日および週末";
                    termNames[0] = "日曜";
                    for (int i = 1; i < 6; i++) termNames[i] = "平日";
                    termNames[6] = "土曜";
                    break;
                case PredefinedDays.OneWeek:
                    name = "曜日別";
                    termNames[0] = "日曜";
                    termNames[1] = "月曜";
                    termNames[2] = "火曜";
                    termNames[3] = "水曜";
                    termNames[4] = "木曜";
                    termNames[5] = "金曜";
                    termNames[6] = "土曜";
                    break;
                case PredefinedDays.AllWeek:
                    name = "全曜日";
                    for (int i = 0; i < 7; i++) termNames[i] = "全曜日";
                    break;
            }
        }

        #endregion//Constructor

        #region public methods

        /// <summary>曜日グループ名称を変更する</summary>
        /// <param name="dayOfWeek">曜日</param>
        /// <param name="termName">曜日グループ名称</param>
        /// <returns>名称変更成功の真偽</returns>
        public void SetTermName(DayOfWeek dayOfWeek, string termName)
        {
            termNames[(int)dayOfWeek] = termName;
            //イベント通知
            if (DayChangeEvent != null) DayChangeEvent(this, new DaysEventArgs(dayOfWeek, termName));
        }

        /// <summary>曜日グループ名称を取得する</summary>
        /// <param name="dayOfWeek">曜日</param>
        /// <returns>曜日グループ名称</returns>
        public string GetTermName(DayOfWeek dayOfWeek)
        {
            return termNames[(int)dayOfWeek];
        }

        /// <summary>指定グループに属する曜日リストを返す</summary>
        /// <param name="termName">グループ名称</param>
        /// <returns>指定グループに属する曜日リスト</returns>
        public DayOfWeek[] GetDays(string termName)
        {
            List<DayOfWeek> dList = new List<DayOfWeek>();
            if (termNames[0] == termName) dList.Add(DayOfWeek.Sunday);
            if (termNames[1] == termName) dList.Add(DayOfWeek.Monday);
            if (termNames[2] == termName) dList.Add(DayOfWeek.Tuesday);
            if (termNames[3] == termName) dList.Add(DayOfWeek.Wednesday);
            if (termNames[4] == termName) dList.Add(DayOfWeek.Thursday);
            if (termNames[5] == termName) dList.Add(DayOfWeek.Friday);
            if (termNames[6] == termName) dList.Add(DayOfWeek.Saturday);
            return dList.ToArray();
        }

        #endregion

        #region<ITermインターフェース実装>

        /// <summary>曜日グループ名称リストを取得する</summary>
        /// <returns>曜日グループ名称リスト</returns>
        public string[] GetTermNames()
        {
            //曜日グループ名称リストを保持
            List<string> gNames = new List<string>();
            foreach (string gName1 in termNames)
            {
                //重複確認
                bool hasName = false;
                foreach (string gName2 in gNames)
                {
                    //重複している場合はbreak;
                    if (gName2 == gName1)
                    {
                        hasName = true;
                        break;
                    }
                }
                //未登録の名称であれば登録
                if (!hasName) gNames.Add(gName1);
            }
            return gNames.ToArray();
        }

        /// <summary>曜日を指定して曜日グループ名称を取得する</summary>
        /// <param name="dateTime">曜日</param>
        /// <returns>曜日グループ名称</returns>
        public string GetTermName(DateTime dateTime)
        {
            return termNames[(int)dateTime.DayOfWeek];
        }

        #endregion//ITermインターフェース実装

        #region<ICloneableインターフェース実装>

        /// <summary>DaysOfTheWeekクラスの複製を返す</summary>
        /// <returns>DaysOfTheWeekクラスの複製</returns>
        public object Clone()
        {
            Days daysOfTheWeek = (Days)this.MemberwiseClone();
            daysOfTheWeek.termNames = new string[7];
            this.termNames.CopyTo(daysOfTheWeek.termNames, 0);
            //イベント初期化
            daysOfTheWeek.NameChangeEvent = null;
            daysOfTheWeek.DayChangeEvent = null;
            return daysOfTheWeek;
        }

        #endregion//ICloneableインターフェース実装

        #region<シリアライズ関連の処理>

        /// <summary>デシリアライズ用Constructor</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected Days(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            double version = sInfo.GetDouble("S_Version");

            //ID
            if (1.0 < version) id = sInfo.GetInt32("ID");
            //名称
            name = sInfo.GetString("Name");
            //曜日定義リスト
            termNames = (string[])sInfo.GetValue("GroupNames", typeof(string[]));
        }

        /// <summary>HvacSystemシリアル化処理</summary>
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
            //曜日グループ名称リスト
            info.AddValue("GroupNames", termNames);
        }

        #endregion//シリアライズ関連の処理

    }

    /// <summary>曜日関連のEventArgs</summary>
    public class DaysEventArgs : EventArgs
    {

        #region<Instance variables>

        /// <summary>編集した曜日</summary>
        private DayOfWeek dayOfWeek;

        /// <summary>グループ名称</summary>
        private string groupName;

        #endregion//Instance variables

        #region<Properties>

        /// <summary>編集した曜日を取得する</summary>
        public DayOfWeek DayOfWeek
        {
            get
            {
                return dayOfWeek;
            }
        }

        /// <summary>グループ名称を取得する</summary>
        public string GroupName
        {
            get
            {
                return groupName;
            }
        }

        #endregion//Properties

        #region<Constructor>

        /// <summary>Constructor</summary>
        /// <param name="dayOfWeek">編集した曜日</param>
        /// <param name="groupName">グループ名称</param>
        public DaysEventArgs(DayOfWeek dayOfWeek, string groupName)
        {
            this.dayOfWeek = dayOfWeek;
            this.groupName = groupName;
        }

        #endregion//Constructor

    }

    /// <summary>読み取り専用Daysインターフェース</summary>
    public interface ImmutableDays : ImmutableITermStructure
    {
        /// <summary>指定グループに属する曜日リストを取得する</summary>
        /// <param name="groupName">グループ名称</param>
        /// <returns>指定グループに属する曜日リスト</returns>
        DayOfWeek[] GetDays(string groupName);
    }

}
