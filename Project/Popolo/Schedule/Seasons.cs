/* Seasons.cs
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
    /// <summary>季節クラス</summary>
    [Serializable]
    public class Seasons : ITermStructure, ISerializable, ImmutableSeasons
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

        /// <summary>季節追加イベントハンドラ</summary>
        public delegate void SeasonAddEventHandler(object sender, SeasonsEventArgs e);

        /// <summary>季節変更イベントハンドラ</summary>
        public delegate void SeasonChangeEventHandler(object sender, SeasonsEventArgs e);

        /// <summary>季節削除イベントハンドラ</summary>
        public delegate void SeasonRemoveEventHandler(object sender, SeasonsEventArgs e);

        #endregion//delegate定義

        #region<イベント定義>

        /// <summary>名称変更イベント</summary>
        public event NameChangeEventHandler NameChangeEvent;

        /// <summary>季節追加イベント</summary>
        public event SeasonAddEventHandler SeasonAddEvent;

        /// <summary>季節変更イベント</summary>
        public event SeasonChangeEventHandler SeasonChangeEvent;

        /// <summary>季節削除イベント</summary>
        public event SeasonRemoveEventHandler SeasonRemoveEvent;

        #endregion//イベント定義

        #region 列挙型定義

        /// <summary>定義済みの季節</summary>
        public enum PredefinedSeasons
        {
            /// <summary>年中</summary>
            AllYear = 0,
            /// <summary>四季</summary>
            FourSeasons = 1,
            /// <summary>熱負荷計算用</summary>
            HeatLoadClassification = 2,
            /// <summary>国民の祝日</summary>
            Holiday = 3
        }

        #endregion

        #region プロパティ

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

        /// <summary>定義した季節数を取得する</summary>
        public int Count
        {
            get
            {
                return seasonNames.Count;
            }
        }

        #endregion

        #region インスタンス変数

        /// <summary>ID</summary>
        private int id;

        /// <summary>名称</summary>
        private string name;

        /// <summary>季節名称リスト</summary>
        private List<string> seasonNames = new List<string>();

        /// <summary>季節開始月日リスト（名称リスト数+1のリストとなる）</summary>
        private List<DateTime> seasonStartDTimes = new List<DateTime>();

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        public Seasons()
        {
            //初期化
            name = "名称未設定の季節定義";
            seasonNames.Add("年間");
            seasonStartDTimes.Add(new DateTime(YEAR, 1, 1));
            seasonStartDTimes.Add(new DateTime(YEAR + 1, 1, 1));
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="predefinedSeasons">定義済みの季節</param>
        public Seasons(PredefinedSeasons predefinedSeasons)
        {
            Initialize(predefinedSeasons);
        }

        /// <summary>定義済の季節で初期化する</summary>
        /// <param name="predefinedSeasons">定義済みの季節</param>
        public void Initialize(PredefinedSeasons predefinedSeasons)
        {
            seasonNames.Clear();
            seasonStartDTimes.Clear();
            switch (predefinedSeasons)
            {
                case PredefinedSeasons.FourSeasons:
                    name = "四季";
                    seasonNames.Add("冬");
                    seasonNames.Add("春");
                    seasonNames.Add("夏");
                    seasonNames.Add("秋");
                    seasonNames.Add("冬");
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 3, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 6, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 9, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 12, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR + 1, 1, 1));
                    break;
                case PredefinedSeasons.HeatLoadClassification:
                    name = "熱負荷別季節";
                    seasonNames.Add("冬季");
                    seasonNames.Add("中間季");
                    seasonNames.Add("夏季");
                    seasonNames.Add("中間季");
                    seasonNames.Add("冬季");
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 3, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 6, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 9, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 12, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR + 1, 1, 1));
                    break;
                case PredefinedSeasons.AllYear:
                    name = "年間";
                    seasonNames.Add("年間");
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR + 1, 1, 1));
                    break;
                case PredefinedSeasons.Holiday:
                    name = "国民の祝日";
                    seasonNames.Add("祝日");
                    seasonNames.Add("一般日");
                    seasonNames.Add("祝日");
                    seasonNames.Add("一般日");
                    seasonNames.Add("祝日");
                    seasonNames.Add("一般日");
                    seasonNames.Add("祝日");
                    seasonNames.Add("一般日");
                    seasonNames.Add("祝日");
                    seasonNames.Add("一般日");
                    seasonNames.Add("祝日");
                    seasonNames.Add("一般日");
                    seasonNames.Add("祝日");
                    seasonNames.Add("一般日");
                    seasonNames.Add("祝日");
                    seasonNames.Add("一般日");
                    seasonNames.Add("祝日");
                    seasonNames.Add("一般日");
                    seasonNames.Add("祝日");
                    seasonNames.Add("一般日");
                    seasonNames.Add("祝日");
                    seasonNames.Add("一般日");
                    seasonNames.Add("祝日");
                    seasonNames.Add("一般日");
                    seasonNames.Add("祝日");
                    seasonNames.Add("一般日");
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 1));
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 2));
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 15));
                    seasonStartDTimes.Add(new DateTime(YEAR, 1, 16));
                    seasonStartDTimes.Add(new DateTime(YEAR, 2, 11));
                    seasonStartDTimes.Add(new DateTime(YEAR, 2, 12));
                    seasonStartDTimes.Add(new DateTime(YEAR, 3, 21));
                    seasonStartDTimes.Add(new DateTime(YEAR, 3, 22));
                    seasonStartDTimes.Add(new DateTime(YEAR, 4, 29));
                    seasonStartDTimes.Add(new DateTime(YEAR, 4, 30));
                    seasonStartDTimes.Add(new DateTime(YEAR, 5, 3));
                    seasonStartDTimes.Add(new DateTime(YEAR, 5, 6));
                    seasonStartDTimes.Add(new DateTime(YEAR, 7, 20));
                    seasonStartDTimes.Add(new DateTime(YEAR, 7, 21));
                    seasonStartDTimes.Add(new DateTime(YEAR, 9, 15));
                    seasonStartDTimes.Add(new DateTime(YEAR, 9, 16));
                    seasonStartDTimes.Add(new DateTime(YEAR, 9, 21));
                    seasonStartDTimes.Add(new DateTime(YEAR, 9, 22));
                    seasonStartDTimes.Add(new DateTime(YEAR, 10, 10));
                    seasonStartDTimes.Add(new DateTime(YEAR, 10, 11));
                    seasonStartDTimes.Add(new DateTime(YEAR, 11, 3));
                    seasonStartDTimes.Add(new DateTime(YEAR, 11, 4));
                    seasonStartDTimes.Add(new DateTime(YEAR, 11, 23));
                    seasonStartDTimes.Add(new DateTime(YEAR, 11, 24));
                    seasonStartDTimes.Add(new DateTime(YEAR, 12, 23));
                    seasonStartDTimes.Add(new DateTime(YEAR, 12, 24));
                    seasonStartDTimes.Add(new DateTime(YEAR + 1, 1, 1));
                    break;
            }
        }

        #endregion

        #region<publicメソッド>

        /// <summary>季節を追加する</summary>
        /// <param name="seasonName">季節名称</param>
        /// <param name="seasonStartDTime">季節開始月日</param>
        /// <returns>追加成功の真偽（指定月日に既に季節が定義されている場合は失敗）</returns>
        public bool AddSeason(string seasonName, DateTime seasonStartDTime)
        {
            DateTime dTime = new DateTime(YEAR, seasonStartDTime.Month, seasonStartDTime.Day);
            int sIndex = 0;
            //適切な位置に挿入する
            for (int i = 1; i < seasonStartDTimes.Count; i++)
            {
                int instPoint = dTime.CompareTo(seasonStartDTimes[i]);
                if (instPoint < 0)
                {
                    sIndex = i;
                    seasonStartDTimes.Insert(i, dTime);
                    seasonNames.Insert(i, seasonName);
                    break;
                }
                else if (instPoint == 0) return false;
            }
            //イベント通知
            if (SeasonAddEvent != null) SeasonAddEvent(this, new SeasonsEventArgs(sIndex, seasonName, seasonStartDTimes[sIndex], seasonStartDTimes[sIndex + 1]));
            return true;
        }

        /// <summary>季節名称を取得する</summary>
        /// <param name="seasonIndex">季節番号</param>
        /// <returns>季節名称</returns>
        public string GetSeasonName(int seasonIndex)
        {
            return seasonNames[seasonIndex];
        }

        /// <summary>季節情報を取得する</summary>
        /// <param name="seasonIndex">季節番号</param>
        /// <param name="seasonName">季節名称</param>
        /// <param name="seasonStartDTime">季節開始月日</param>
        /// <param name="seasonEndDTime">季節終了月日</param>
        public void GetSeason(int seasonIndex, out string seasonName, out DateTime seasonStartDTime, out DateTime seasonEndDTime)
        {
            seasonName = seasonNames[seasonIndex];
            seasonStartDTime = seasonStartDTimes[seasonIndex];
            seasonEndDTime = seasonStartDTimes[seasonIndex + 1].AddDays(-1);
        }

        /// <summary>季節情報を取得する</summary>
        /// <param name="seasonName">季節名称</param>
        /// <param name="seasonStartDTimes">季節開始月日リスト</param>
        /// <param name="seasonEndDTimes">季節終了月日リスト</param>
        public void GetSeasons(string seasonName, out DateTime[] seasonStartDTimes, out DateTime[] seasonEndDTimes)
        {
            //該当する季節が存在しない場合
            if (!seasonNames.Contains(seasonName))
            {
                seasonStartDTimes = new DateTime[0];
                seasonEndDTimes = new DateTime[0];
                return;
            }
            //季節が存在する場合はすべての期間を調べる
            List<DateTime> dtStart = new List<DateTime>();
            List<DateTime> dtEnd = new List<DateTime>();
            for (int i = 0; i < seasonNames.Count; i++)
            {
                if (seasonNames[i] == seasonName)
                {
                    dtStart.Add(this.seasonStartDTimes[i]);
                    dtEnd.Add(this.seasonStartDTimes[i + 1].AddDays(-1));
                }
            }
            //配列化
            seasonStartDTimes = dtStart.ToArray();
            seasonEndDTimes = dtEnd.ToArray();
        }

        /// <summary>季節を削除する</summary>
        /// <param name="seasonIndex">季節番号</param>
        public bool RemoveSeason(int seasonIndex)
        {
            //季節が一つしか定義されていない場合は失敗
            if (seasonNames.Count - 1 < seasonIndex) return false;
            else
            {
                DateTime dtStart, dtEnd;
                string sName = seasonNames[seasonIndex];
                //季節名称を削除
                seasonNames.RemoveAt(seasonIndex);
                //季節開始終了月日を更新****************
                //先頭季節の場合
                if (seasonIndex == 0)
                {
                    dtStart = seasonStartDTimes[0];
                    dtEnd = seasonStartDTimes[1];
                    seasonStartDTimes.RemoveAt(1);
                }
                //最終季節の場合
                else if (seasonIndex == seasonNames.Count)
                {
                    dtStart = seasonStartDTimes[seasonIndex];
                    dtEnd = seasonStartDTimes[seasonIndex + 1];
                    seasonStartDTimes.RemoveAt(seasonIndex);
                }
                //その他の季節の場合
                else
                {
                    //中間日を計算する
                    dtStart = seasonStartDTimes[seasonIndex];
                    dtEnd = seasonStartDTimes[seasonIndex + 1];
                    TimeSpan tSpan = dtEnd - dtStart;
                    DateTime dtMiddle = dtStart.AddDays(tSpan.Days / 2);
                    seasonStartDTimes.RemoveAt(seasonIndex + 1);
                    seasonStartDTimes[seasonIndex] = dtMiddle;
                }
                //イベント通知
                if (SeasonRemoveEvent != null) SeasonRemoveEvent(this, new SeasonsEventArgs(seasonIndex, sName, dtStart, dtEnd));
                //削除成功
                return true;
            }
        }

        /// <summary>季節名称を変更する</summary>
        /// <param name="seasonIndex">季節番号</param>
        /// <param name="seasonName">季節名称</param>
        /// <returns>名称変更成功の真偽</returns>
        public bool ChangeSeasonName(int seasonIndex, string seasonName)
        {
            //季節番号範囲外指定の場合は終了
            if (seasonNames.Count - 1 < seasonIndex) return false;
            //季節名称を変更
            seasonNames[seasonIndex] = seasonName;
            //イベント通知
            if (SeasonChangeEvent != null) SeasonChangeEvent(this, new SeasonsEventArgs(seasonIndex, seasonName, seasonStartDTimes[seasonIndex], seasonStartDTimes[seasonIndex + 1]));
            return true;
        }

        /// <summary>季節端部月日を変更する</summary>
        /// <param name="seasonIndex">季節番号</param>
        /// <param name="seasonDateTime">季節端部月日</param>
        /// <param name="isStartDateTime">季節開始月日の設定か否か</param>
        /// <returns>季節端部月日を変更成功の真偽</returns>
        public bool ChangeSeasonDateTime(int seasonIndex, DateTime seasonDateTime, bool isStartDateTime)
        {
            //季節開始月日の場合
            if (isStartDateTime)
            {
                //最初の季節区切りDateTimeの場合は終了
                if (seasonNames.Count - 1 < seasonIndex || seasonIndex <= 0) return false;
                //季節変更可能範囲外の場合は終了
                if (seasonDateTime <= seasonStartDTimes[seasonIndex - 1] || seasonStartDTimes[seasonIndex + 1] <= seasonDateTime) return false;
                //季節開始月日を変更する
                seasonStartDTimes[seasonIndex] = seasonDateTime;
            }
            //季節終了月日の場合
            else
            {
                //最後の季節区切りDateTimeの場合は終了
                if (seasonNames.Count - 2 < seasonIndex || seasonIndex < 0) return false;
                //季節変更可能範囲外の場合は終了
                seasonDateTime = seasonDateTime.AddDays(1);
                if (seasonDateTime <= seasonStartDTimes[seasonIndex] || seasonStartDTimes[seasonIndex + 2] <= seasonDateTime) return false;
                //季節終了月日を変更する
                seasonStartDTimes[seasonIndex + 1] = seasonDateTime;
            }
            //イベント通知
            if (SeasonChangeEvent != null) SeasonChangeEvent(this, new SeasonsEventArgs(seasonIndex, seasonNames[seasonIndex], seasonStartDTimes[seasonIndex], seasonStartDTimes[seasonIndex + 1]));
            return true;
        }

        #endregion//publicメソッド

        #region<ITermインターフェース実装>

        /// <summary>季節名称リストを取得する</summary>
        /// <returns>季節名称リスト</returns>
        public string[] GetTermNames()
        {
            //季節名称リストを保持
            List<string> sNames = new List<string>();
            foreach (string sName1 in seasonNames)
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

        /// <summary>日付を指定して季節名称を取得する</summary>
        /// <param name="dateTime">日付</param>
        /// <returns>季節名称</returns>
        public string GetTermName(DateTime dateTime)
        {
            DateTime dt = new DateTime(YEAR, dateTime.Month, dateTime.Day);
            for (int i = 0; i < seasonStartDTimes.Count; i++)
            {
                if (dt < seasonStartDTimes[i]) return seasonNames[i - 1];
            }
            throw new Exception("スケジュール定義範囲外");
        }

        #endregion//ITermインターフェース実装

        #region<ICloneableインターフェース実装>

        /// <summary>Seasonsクラスの複製を返す</summary>
        /// <returns>Seasonsクラスの複製</returns>
        public object Clone()
        {
            Seasons seasons = (Seasons)this.MemberwiseClone();
            seasons.seasonNames = new List<string>();
            seasons.seasonStartDTimes = new List<DateTime>();
            foreach (string sName in seasonNames) seasons.seasonNames.Add(sName);
            foreach (DateTime dTime in seasonStartDTimes) seasons.seasonStartDTimes.Add(dTime);
            //イベント初期化
            seasons.NameChangeEvent = null;
            seasons.SeasonAddEvent = null;
            seasons.SeasonChangeEvent = null;
            seasons.SeasonRemoveEvent = null;
            return seasons;
        }

        #endregion//ICloneableインターフェース実装

        #region<シリアライズ関連の処理>

        /// <summary>デシリアライズ用コンストラクタ</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected Seasons(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            double version = sInfo.GetDouble("S_Version");

            //ID
            if (1.0 < version) id = sInfo.GetInt32("ID");
            //名称
            name = sInfo.GetString("Name");
            //季節名称リスト
            seasonNames.AddRange((string[])sInfo.GetValue("SeasonNames", typeof(string[])));
            //季節開始年月日リスト
            seasonStartDTimes.AddRange((DateTime[])sInfo.GetValue("SeasonStartDTimes", typeof(DateTime[])));    
        }

        /// <summary>Seasonsシリアル化処理</summary>
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
            //季節名称リスト
            info.AddValue("SeasonNames", seasonNames.ToArray(), typeof(string[]));
            //季節開始年月日リスト
            info.AddValue("SeasonStartDTimes", seasonStartDTimes.ToArray(), typeof(DateTime[]));
        }

        #endregion//シリアライズ関連の処理

    }

    /// <summary>季節関連のEventArgs</summary>
    public class SeasonsEventArgs : EventArgs
    {

        #region<インスタンス変数>

        /// <summary>季節番号</summary>
        private int seasonIndex;

        /// <summary>季節名称</summary>
        private string seasonName;

        /// <summary>季節開始月日</summary>
        private DateTime seasonStart;

        /// <summary>季節終了月日</summary>
        private DateTime seasonEnd;

        #endregion//インスタンス変数

        #region<プロパティ>

        /// <summary>季節番号を取得する</summary>
        public int SeasonIndex
        {
            get
            {
                return seasonIndex;
            }
        }

        /// <summary>季節名称を取得する</summary>
        public string SeasonName
        {
            get
            {
                return seasonName;
            }
        }

        /// <summary>季節開始月日を取得する</summary>
        public DateTime SeasonStart
        {
            get
            {
                return seasonStart;
            }
        }

        /// <summary>季節終了月日を取得する</summary>
        public DateTime SeasonEnd
        {
            get
            {
                return seasonEnd;
            }
        }

        #endregion//プロパティ

        #region<コンストラクタ>

        /// <summary>コンストラクタ</summary>
        /// <param name="seasonIndex">季節番号</param>
        /// <param name="seasonName">季節名称</param>
        /// <param name="seasonStart">季節開始月日</param>
        /// <param name="seasonEnd">季節終了月日</param>
        public SeasonsEventArgs(int seasonIndex, string seasonName, DateTime seasonStart, DateTime seasonEnd)
        {
            this.seasonIndex = seasonIndex;
            this.seasonName = seasonName;
            this.seasonStart = seasonStart;
            this.seasonEnd = seasonEnd;
        }

        #endregion//コンストラクタ

    }

    /// <summary>読み取り専用Seasonsインターフェース</summary>
    public interface ImmutableSeasons : ImmutableITermStructure
    {
        /// <summary>定義した時間帯数を取得する</summary>
        int Count
        {
            get;
        }

        /// <summary>季節名称を取得する</summary>
        /// <param name="seasonIndex">季節番号</param>
        /// <returns>季節名称</returns>
        string GetSeasonName(int seasonIndex);

        /// <summary>季節情報を取得する</summary>
        /// <param name="seasonIndex">季節番号</param>
        /// <param name="seasonName">季節名称</param>
        /// <param name="seasonStartDTime">季節開始月日</param>
        /// <param name="seasonEndDTime">季節終了月日</param>
        void GetSeason(int seasonIndex, out string seasonName, out DateTime seasonStartDTime, out DateTime seasonEndDTime);

        /// <summary>季節情報を取得する</summary>
        /// <param name="seasonName">季節名称</param>
        /// <param name="seasonStartDTimes">季節開始月日リスト</param>
        /// <param name="seasonEndDTimes">季節終了月日リスト</param>
        void GetSeasons(string seasonName, out DateTime[] seasonStartDTimes, out DateTime[] seasonEndDTimes);

    }

}
