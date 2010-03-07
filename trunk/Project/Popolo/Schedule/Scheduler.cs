/* Scheduler.cs
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
    /// <summary>季節や時間帯などの期間のスケジュールを管理するクラス</summary>
    /// <typeparam name="SCHEDULE">スケジュール設定する状態値</typeparam>
    public class Scheduler<SCHEDULE> : ICloneable
        where SCHEDULE : ICloneable
    {

        #region<インスタンス変数>

        /// <summary>デフォルト値</summary>
        private SCHEDULE defaultValue = default(SCHEDULE);

        /// <summary>スケジュール名称</summary>
        private string name = "名称未設定のスケジュール";

        /// <summary>スケジュール内容保持配列</summary>
        private Dictionary<string, SCHEDULE> schedules = new Dictionary<string,SCHEDULE>();

        /// <summary>スケジューラ保持配列</summary>
        private Dictionary<string, Scheduler<SCHEDULE>> schedulers = new Dictionary<string, Scheduler<SCHEDULE>>();

        /// <summary>季節や時間帯などの期間名称リスト</summary>
        private string[] termNames = new string[0];

        /// <summary>季節や時間帯などの期間構造を持つオブジェクト</summary>
        private ImmutableITermStructure terms;

        #endregion//インスタンス変数

        #region<delegate定義>

        /// <summary>スケジューラ初期化イベントハンドラ</summary>
        public delegate void SchedulerInitializeEventHandler(object sender, EventArgs e);

        /// <summary>名称変更イベントハンドラ</summary>
        public delegate void SchedulerNameChangeEventHandler(object sender, EventArgs e);

        /// <summary>スケジューラ設定イベントハンドラ</summary>
        public delegate void SchedulerSetEventHandler(object sender, SchedulerEventArgs<SCHEDULE> e);

        /// <summary>スケジューラ削除イベントハンドラ</summary>
        public delegate void SchedulerRemoveEventHandler(object sender, SchedulerEventArgs<SCHEDULE> e);

        #endregion//delegate定義

        #region<イベント定義>

        /// <summary>スケジューラ初期化イベント</summary>
        public event SchedulerInitializeEventHandler SchedulerInitializeEvent;

        /// <summary>名称変更イベント</summary>
        public event SchedulerNameChangeEventHandler SchedulerNameChangeEvent;

        /// <summary>スケジューラ設定イベント</summary>
        public event SchedulerSetEventHandler SchedulerSetEvent;

        /// <summary>スケジューラ削除イベント</summary>
        public event SchedulerRemoveEventHandler SchedulerRemoveEvent;

        #endregion//イベント定義

        #region<プロパティ>

        /// <summary>デフォルト値を設定・取得する</summary>
        public SCHEDULE DefaultValue
        {
            get
            {
                return defaultValue;
            }
            set
            {
                defaultValue = value;
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
                if (SchedulerNameChangeEvent != null) SchedulerNameChangeEvent(this, new EventArgs());
            }
        }

        /// <summary>Schedulerが管理する期間構造オブジェクトを取得する</summary>
        public ImmutableITermStructure Terms
        {
            get
            {
                return this.terms;
            }
        }

        #endregion//プロパティ

        #region<コンストラクタ>

        /// <summary>デフォルトコンストラクタ</summary>
        public Scheduler() { }

        /// <summary>デフォルトコンストラクタ</summary>
        /// <param name="scheduleName">スケジューラ名称</param>
        public Scheduler(string scheduleName)
        {
            this.name = scheduleName;
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="terms">季節や時間帯などの期間構造を持つオブジェクト</param>
        public Scheduler(ImmutableITermStructure terms)
        {
            Initialize(terms);
        }

        /// <summary>デフォルトコンストラクタ</summary>
        /// <param name="scheduleName">スケジューラ名称</param>
        /// <param name="terms">季節や時間帯などの期間構造を持つオブジェクト</param>
        public Scheduler(string scheduleName, ImmutableITermStructure terms)
        {
            this.name = scheduleName;
            Initialize(terms);
        }

        /// <summary>初期化する</summary>
        public void Initialize()
        {
            this.terms = null;
            termNames = new string[0];
            //各期間にデフォルト値を設定する
            schedules.Clear();
            schedulers.Clear();
            if (SchedulerInitializeEvent != null) SchedulerInitializeEvent(this, new EventArgs());
        }

        /// <summary>初期化する</summary>
        /// <param name="terms">季節や時間帯などの期間構造を持つオブジェクト</param>
        public void Initialize(ImmutableITermStructure terms)
        {
            this.terms = terms;
            termNames = terms.GetTermNames();
            //各期間にデフォルト値を設定する
            schedules.Clear();
            schedulers.Clear();
            if (SchedulerInitializeEvent != null) SchedulerInitializeEvent(this, new EventArgs());
        }

        #endregion//コンストラクタ

        #region<publicメソッド>

        /// <summary>季節や時間帯などの期間名称リストを取得する</summary>
        /// <returns>季節や時間帯などの期間名称リスト</returns>
        public string[] GetTermNames()
        {
            return termNames;
        }

        /// <summary>全期間についてスケジュール内容を設定する</summary>
        /// <param name="schedule">スケジュール内容</param>
        public void SetSchedule(SCHEDULE schedule)
        {
            foreach (string termName in termNames)
            {
                //スケジューラが存在していればスケジューラ内のスケジュール内容を設定
                if (schedulers.ContainsKey(termName))
                {
                    schedulers[termName].SetSchedule(schedule);
                }
                //スケジューラがなければ直接に設定
                SetSchedule(termName, schedule);
            }
        }

        /// <summary>スケジュール内容を設定する</summary>
        /// <param name="dateTime">スケジュール内容を設定する月日</param>
        /// <param name="schedule">スケジュール内容</param>
        public void SetSchedule(DateTime dateTime, SCHEDULE schedule)
        {
            string sName = getTermName(dateTime);
            //スケジューラが存在していればスケジューラに委譲する
            if (schedulers.ContainsKey(sName)) schedulers[sName].SetSchedule(dateTime, schedule);
            //スケジューラがなければスケジュール内容を直接に設定する
            if (schedules.ContainsKey(sName)) schedules[sName] = schedule;
            else schedules.Add(sName, schedule);
        }

        /// <summary>スケジュール内容を設定する</summary>
        /// <param name="termName">スケジュール内容を設定する月日</param>
        /// <param name="schedule">スケジュール内容</param>
        /// <returns>設定成功の真偽（スケジューラが設定されている場合は設定できない）</returns>
        public bool SetSchedule(string termName, SCHEDULE schedule)
        {
            //スケジューラが存在していれば設定失敗
            if (schedulers.ContainsKey(termName)) return false;
            //スケジューラがなければスケジュール内容を直接に設定する
            if (schedules.ContainsKey(termName)) schedules[termName] = schedule;
            else schedules.Add(termName, schedule);
            return true;
        }

        /// <summary>スケジュール内容を取得する</summary>
        /// <param name="dateTime">スケジュール内容を取得する月日</param>
        /// <param name="schedule">スケジュール内容</param>
        /// <returns>スケジュール内容取得成功の真偽（スケジューラが設定されている場合は取得できない）</returns>
        public bool GetSchedule(DateTime dateTime, out SCHEDULE schedule)
        {
            string sName = getTermName(dateTime);
            //スケジューラが存在する場合には再帰的に呼び出す
            if (schedulers.ContainsKey(sName)) return schedulers[sName].GetSchedule(dateTime, out schedule);
            else return GetSchedule(sName, out schedule);
        }

        /// <summary>スケジュール内容を取得する</summary>
        /// <param name="termName">スケジュール内容を取得する期間</param>
        /// <param name="schedule">取得されたスケジュール内容</param>
        /// <returns>スケジュール内容取得成功の真偽</returns>
        public bool GetSchedule(string termName, out SCHEDULE schedule)
        {
            schedule = defaultValue;
            //スケジューラが存在する場合は取得失敗
            if (schedulers.ContainsKey(termName)) return false;
            //スケジューラがなければスケジュール内容を直接に返す
            else
            {
                if (schedules.ContainsKey(termName))
                {
                    schedule = schedules[termName];
                    return true;
                }
                else
                {
                    //期間名称が存在する場合はデフォルト値を返す
                    for (int i = 0; i < termNames.Length; i++)
                    {
                        if (termNames[i] == termName)
                        {
                            schedule = defaultValue;
                        }
                    }
                    return false;
                }
            }
        }

        /// <summary>スケジューラを設定する</summary>
        /// <param name="dateTime">スケジューラを設定する日時データ</param>
        /// <param name="scheduler">設定するスケジューラ</param>
        public void SetScheduler(DateTime dateTime, Scheduler<SCHEDULE> scheduler)
        {
            string sName = getTermName(dateTime);
            SetScheduler(sName, scheduler);
        }

        /// <summary>スケジューラを設定する</summary>
        /// <param name="termName">スケジューラを設定する期間名称</param>
        /// <param name="scheduler">設定するスケジューラ</param>
        public void SetScheduler(string termName, Scheduler<SCHEDULE> scheduler)
        {
            if (schedulers.ContainsKey(termName)) schedulers[termName] = scheduler;
            else schedulers.Add(termName, scheduler);
            //イベント通知
            if (SchedulerSetEvent != null) SchedulerSetEvent(this, new SchedulerEventArgs<SCHEDULE>(scheduler, termName));
        }

        /// <summary>指定日時を管理するスケジューラを削除する</summary>
        /// <param name="dateTime">日時</param>
        public void RemoveScheduler(DateTime dateTime)
        {
            string sName = getTermName(dateTime);
            RemoveScheduler(sName);
        }

        /// <summary>指定名称の期間を管理するスケジューラを削除する</summary>
        /// <param name="termName">期間名称</param>
        public void RemoveScheduler(string termName)
        {
            if (schedulers.ContainsKey(termName))
            {
                Scheduler<SCHEDULE> scheduler = schedulers[termName];
                schedulers.Remove(termName);
                //イベント通知
                if (SchedulerRemoveEvent != null) SchedulerRemoveEvent(this, new SchedulerEventArgs<SCHEDULE>(scheduler, termName));
            }
        }

        /// <summary>指定月日を管理するスケジューラを取得する</summary>
        /// <param name="dateTime">スケジューラを取得する月日</param>
        /// <param name="scheduler">指定月日を管理するスケジューラ</param>
        /// <returns>取得成功の真偽</returns>
        public bool GetScheduler(DateTime dateTime, out Scheduler<SCHEDULE> scheduler)
        {
            return GetScheduler(getTermName(dateTime), out scheduler);
        }

        /// <summary>指定名称の期間を管理するスケジューラを取得する</summary>
        /// <param name="termName">期間名称</param>
        /// <param name="scheduler">指定名称の期間を管理するスケジューラ</param>
        /// <returns>取得成功の真偽</returns>
        public bool GetScheduler(string termName, out Scheduler<SCHEDULE> scheduler)
        {
            if (schedulers.ContainsKey(termName))
            {
                scheduler = schedulers[termName];
                if (scheduler == null) return false;
                else return true;
            }
            else
            {
                scheduler = null;
                return false;
            }
        }

        /// <summary>指定月日を管理するスケジューラの有無を返す</summary>
        /// <param name="dateTime">スケジューラが管理する月日</param>
        /// <returns>存在する場合は真</returns>
        public bool HasScheduler(DateTime dateTime)
        {
            return HasScheduler(getTermName(dateTime));
        }

        /// <summary>指定名称の期間を管理するスケジューラの有無を返す</summary>
        /// <param name="termName">期間名称</param>
        /// <returns>存在する場合は真</returns>
        public bool HasScheduler(string termName)
        {
            return schedulers.ContainsKey(termName);
        }

        /// <summary>同一構造のスケジューラを作成する</summary>
        /// <typeparam name="T">スケジュール内容の型</typeparam>
        /// <returns>同一構造のスケジューラ</returns>
        public Scheduler<T> CopyStructure<T>()
            where T : ICloneable
        {
            Scheduler<T> scheduler = new Scheduler<T>(terms);
            foreach (string key in schedulers.Keys)
            {
                scheduler.schedulers.Add(key, schedulers[key].CopyStructure<T>());
            }
            return scheduler;
        }

        /// <summary>指定されたスケジューラへのパスを返す（\区切り）</summary>
        /// <param name="scheduler">スケジューラオブジェクト</param>
        /// <returns>指定されたスケジューラへのパス</returns>
        public string GetSchedulerPath(Scheduler<SCHEDULE> scheduler)
        {
            foreach (string key in schedulers.Keys)
            {
                Scheduler<SCHEDULE> sc = schedulers[key];
                if (ReferenceEquals(sc, schedulers))
                {
                    return this.name + "\\" + key;
                }
                string sPath = sc.GetSchedulerPath(scheduler);
                if (sPath != null) return this.name + "\\" + key + "\\";
            }
            return null;
        }

        /// <summary>指定IDの期間構造を使用しているか否かを返す</summary>
        /// <param name="termID">期間構造ID</param>
        /// <returns>使用している場合は真</returns>
        public bool UsingTerm(int termID)
        {
            if (terms == null) return false;
            if (terms.ID == termID) return true;
            else
            {
                foreach (string key in schedulers.Keys)
                {
                    if (schedulers[key].UsingTerm(termID)) return true;
                }
            }
            return false;
        }

        /// <summary>Term名称を取得する</summary>
        /// <param name="dateTime">日付</param>
        /// <returns>Term名称</returns>
        private string getTermName(DateTime dateTime)
        {
            if (terms == null) return "";
            return terms.GetTermName(dateTime);
        }

        #endregion//publicメソッド

        #region<ICloneableメソッド実装>

        /// <summary>Schedulerの複製を返す</summary>
        /// <returns>Schedulerの複製</returns>
        public object Clone()
        {
            Scheduler<SCHEDULE> scheduler = new Scheduler<SCHEDULE>();
            if (terms != null) scheduler.Initialize(terms);
            scheduler.name = this.name;
            scheduler.defaultValue = this.defaultValue;
            foreach (string key in schedules.Keys)
            {
                SCHEDULE target = (SCHEDULE)schedules[key].Clone();
                if (target != null) scheduler.schedules.Add(key, target);
            }
            foreach (string key in schedulers.Keys)
            {
                Scheduler<SCHEDULE> targetSC = (Scheduler<SCHEDULE>)schedulers[key].Clone();
                if(targetSC != null) scheduler.schedulers.Add(key, targetSC);
            }
            return scheduler;
        }

        #endregion//ICloneableメソッド実装

    }


    #region<シリアル化用クラス>

    /// <summary>Schedulerシリアル化用クラス</summary>
    [Serializable]
    public class SerializableScheduler : ISerializable
    {

        #region<定数宣言>

        /// <summary>シリアライズ用バージョン情報</summary>
        private double S_VERSION = 1.0;

        #endregion//定数宣言

        #region インスタンス変数

        /// <summary>スケジュール名称</summary>
        private string name = "名称未設定のスケジュール";

        /// <summary>スケジュール内容保持配列</summary>
        private Dictionary<string, object> schedules = new Dictionary<string, object>();

        /// <summary>スケジューラ保持配列</summary>
        private Dictionary<string, SerializableScheduler> schedulers = new Dictionary<string, SerializableScheduler>();

        /// <summary>季節や時間帯などの期間構造を持つオブジェクト</summary>
        private ImmutableITermStructure terms;

        /// <summary>デフォルト値</summary>
        private object defaultValue;

        #endregion

        #region<コンストラクタ>

        /// <summary>デフォルトコンストラクタ</summary>
        private SerializableScheduler() { }

        #endregion//コンストラクタ

        #region<スケジューラ変換処理>

        /// <summary>スケジューラに変換する</summary>
        /// <typeparam name="T">スケジューラのタイプ</typeparam>
        /// <returns>スケジューラ</returns>
        public Scheduler<T> ToScheduler<T>()
            where T : ICloneable
        {
            Scheduler<T> rtnSC = new Scheduler<T>();
            if (terms != null) rtnSC.Initialize(terms);
            rtnSC.Name = this.name;
            foreach (string key in schedules.Keys)
            {
                rtnSC.SetSchedule(key, (T)schedules[key]);
            }
            foreach (string key in schedulers.Keys)
            {
                if(schedulers[key] != null) rtnSC.SetScheduler(key, schedulers[key].ToScheduler<T>());
            }
            //デフォルト値
            rtnSC.DefaultValue = (T)defaultValue;

            return rtnSC;
        }

        /// <summary>シリアル化可能なスケジューラに変換する</summary>
        /// <typeparam name="T">スケジューラのタイプ</typeparam>
        /// <param name="scheduler">スケジューラ</param>
        /// <returns>シリアル化可能なスケジューラ</returns>
        public static SerializableScheduler FromScheduler<T>(Scheduler<T> scheduler)
            where T : ICloneable
        {
            SerializableScheduler sSC = new SerializableScheduler();
            sSC.name = scheduler.Name;
            sSC.terms = scheduler.Terms;

            foreach (string key in scheduler.GetTermNames())
            {
                T sc;
                if (scheduler.GetSchedule(key, out sc))
                {
                    sSC.schedules.Add(key, sc);
                }
            }
            foreach (string key in scheduler.GetTermNames())
            {
                Scheduler<T> sch;
                if (scheduler.GetScheduler(key, out sch))
                {
                    sSC.schedulers.Add(key, SerializableScheduler.FromScheduler<T>(sch));
                }
            }
            //デフォルト値
            sSC.defaultValue = scheduler.DefaultValue;

            return sSC;
        }

        #endregion//スケジューラ変換処理

        #region<シリアライズ関連の処理>

        /// <summary>デシリアライズ用コンストラクタ</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected SerializableScheduler(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            double version = sInfo.GetDouble("S_Version");

            //名称
            name = sInfo.GetString("Name");
            //期間構造を持つオブジェクト
            terms = (ITermStructure)sInfo.GetValue("ITermStructure", typeof(ITermStructure));
            //スケジュール内容リスト
            string[] sKeys;
            object[] sValues1;
            sKeys = (string[])sInfo.GetValue("ScheduleKeys", typeof(string[]));
            sValues1 = (object[])sInfo.GetValue("ScheduleValues", typeof(object[]));
            for (int i = 0; i < sKeys.Length; i++) schedules.Add(sKeys[i], sValues1[i]);
            //スケジューラリスト
            sKeys = (string[])sInfo.GetValue("SchedulerKeys", typeof(string[]));
            int sValues2Number = sInfo.GetInt32("SchedulerValuesNumber");
            for (int i = 0; i < sValues2Number; i++)
            {
                SerializableScheduler ss = (SerializableScheduler)sInfo.GetValue("SchedulerValues" + i.ToString(), typeof(SerializableScheduler));
                schedulers.Add(sKeys[i], ss);
            }
            //デフォルト値
            defaultValue = sInfo.GetValue("DefaultValue", typeof(object));
        }

        /// <summary>Schedulerシリアル化処理</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //バージョン情報
            info.AddValue("S_Version", S_VERSION);

            //名称
            info.AddValue("Name", name);
            //期間構造を持つオブジェクト
            info.AddValue("ITermStructure", terms);
            //スケジュール内容リスト
            List<string> sKeys = new List<string>();
            List<object> sValues1 = new List<object>();
            foreach (string key in schedules.Keys)
            {
                sKeys.Add(key);
                sValues1.Add(schedules[key]);
            }
            info.AddValue("ScheduleKeys", sKeys.ToArray());
            info.AddValue("ScheduleValues", sValues1.ToArray());
            //スケジューラリスト
            List<SerializableScheduler> sValues2 = new List<SerializableScheduler>();
            sKeys.Clear();
            foreach (string key in schedulers.Keys)
            {
                sKeys.Add(key);
                sValues2.Add(schedulers[key]);
            }
            info.AddValue("SchedulerKeys", sKeys.ToArray());
            info.AddValue("SchedulerValuesNumber", sValues2.Count);
            for (int i = 0; i < sValues2.Count; i++) info.AddValue("SchedulerValues" + i.ToString(), sValues2[i]);
            //デフォルト値
            info.AddValue("DefaultValue", defaultValue);
        }

        #endregion//シリアライズ関連の処理

    }

    #endregion

    /// <summary>スケジューラ関連のEventArgs</summary>
    public class SchedulerEventArgs<TYPE> : EventArgs
        where TYPE : ICloneable
    {

        #region<インスタンス変数>

        /// <summary>スケジューラ</summary>
        private Scheduler<TYPE> scheduler;

        /// <summary>スケジューラに対応する期間名称</summary>
        private string termName;

        #endregion//インスタンス変数

        #region<プロパティ>

        /// <summary>スケジューラを取得する</summary>
        public Scheduler<TYPE> TargetScheduler
        {
            get
            {
                return scheduler;
            }
        }

        /// <summary>スケジューラに対応する期間名称を取得する</summary>
        public string TermName
        {
            get
            {
                return termName;
            }
        }

        #endregion//プロパティ

        #region<コンストラクタ>

        /// <summary>コンストラクタ</summary>
        /// <param name="scheduler">スケジューラオブジェクト</param>
        /// <param name="termName">スケジューラに対応する期間名称</param>
        public SchedulerEventArgs(Scheduler<TYPE> scheduler, string termName)
        {
            this.scheduler = scheduler;
            this.termName = termName;
        }

        #endregion//コンストラクタ

    }

}
