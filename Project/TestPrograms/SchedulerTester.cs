using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization;

using Popolo.Utility.Schedule;

namespace Popolo.Utility
{
    internal static class SchedulerTester
    {

        public static void Test()
        {
            //スケジュールする期間構造を四季とする
            Seasons fourSeasons = new Seasons(Seasons.PredefinedSeasons.FourSeasons);

            //TestScheduleを管理するスケジューラとする
            Scheduler<TestSchedule> testScheduler = new Scheduler<TestSchedule>(fourSeasons);

            testScheduler.SetSchedule(new TestSchedule(10d, "SC1", 5));

        }

    }

    internal class TestSchedule : ICloneable, ISerializable
    {
        /// <summary>実数値型スケジュール内容</summary>
        public double DoubleValue { get; set; }

        /// <summary>文字列型スケジュール内容</summary>
        public string StringValue { get; set; }

        /// <summary>整数型スケジュール内容</summary>
        public int IntValue { get; set; }

        /// <summary>コンストラクタ</summary>
        /// <param name="dValue">実数値型スケジュール内容</param>
        /// <param name="sValue">文字列型スケジュール内容</param>
        /// <param name="iValue">整数型スケジュール内容</param>
        public TestSchedule(double dValue, string sValue, int iValue)
        {
            DoubleValue = dValue;
            StringValue = sValue;
            IntValue = iValue;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>デシリアライズ用コンストラクタ</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected TestSchedule(SerializationInfo sInfo, StreamingContext context)
        {
            
        }

        /// <summary>シリアル化処理</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            
        }
    }

}
