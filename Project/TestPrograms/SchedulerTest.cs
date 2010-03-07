using System;
using System.Collections.Generic;
using System.Text;

using Popolo.Schedule;
using Popolo.ThermophysicalProperty;

namespace Popolo
{
    static class SchedulerTest
    {
        public static void MakeScheduler()
        {
            //スケジュールを作成
            //FC有り
            CTSchedule fc = new CTSchedule();
            fc.FCStartTemperature = 13;
            fc.OperatingMode = CTSchedule.Mode.SwitchWithWBTemp;
            fc.OutletWaterTemperatureFC = 16;
            fc.OutletWaterTemperature = 32;

            //FC無し
            CTSchedule noFc = new CTSchedule();
            fc.OperatingMode = CTSchedule.Mode.NoFreeCooling;
            fc.OutletWaterTemperature = 32;

            //期間構造（四季）を作成
            ITermStructure terms = new Seasons(Seasons.PredefinedSeasons.FourSeasons);
            //最上層のスケジューラを作成
            Scheduler<CTSchedule> ctScheduler = new Scheduler<CTSchedule>(terms);

            //期間構造（平日・週末）を作成
            terms = new Days(Days.PredefinedDays.WeekDayAndWeekEnd);
            //冬季用スケジューラを作成
            Scheduler<CTSchedule> winterSC = new Scheduler<CTSchedule>(terms);
            //平日のみFC有り
            winterSC.SetSchedule("週末", noFc);
            winterSC.SetSchedule("平日", fc);

            //冬季の平日を階層構造で表現
            ctScheduler.SetScheduler("冬", winterSC);

            //その他の季節は曜日を問わずFC無し
            ctScheduler.SetSchedule("春", noFc);
            ctScheduler.SetSchedule("夏", noFc);
            ctScheduler.SetSchedule("秋", noFc);
        }

        public static void GetScheduleTest(Scheduler<CTSchedule> scheduler)
        {
            DateTime dateTime = new DateTime(1999, 12, 20);
            CTSchedule ctSchedule;
            scheduler.GetSchedule(dateTime, out ctSchedule);
        }

    }

    /// <summary>冷却塔スケジュール</summary>
    public class CTSchedule : ICloneable
    {

        /// <summary>運転モード</summary>
        public enum Mode
        {
            /// <summary>FC無し</summary>
            NoFreeCooling = 0,
            /// <summary>湿球温度でFC起動判定</summary>
            SwitchWithWBTemp = 1,
            /// <summary>乾球温度でFC起動判定</summary>
            SwitchWithDBTemp = 2
        }

        /// <summary>運転モード</summary>
        public Mode OperatingMode = Mode.NoFreeCooling;

        /// <summary>冷却水出口温度[C]</summary>
        public double OutletWaterTemperature = 32;

        /// <summary>FC時の冷却水出口温度[C]</summary>
        public double OutletWaterTemperatureFC = 16;

        /// <summary>FC起動判定温度[C]</summary>
        public double FCStartTemperature = 15;

        /// <summary>冷却塔出口温度設定値を取得する</summary>
        /// <param name="airState">外気状態</param>
        /// <returns>冷却塔出口温度設定値</returns>
        public double GetCoolingWaterTemperature(MoistAir airState)
        {
            //乾球温度基準でフリークーリングを起動させる場合
            if (OperatingMode == Mode.SwitchWithDBTemp &&
                airState.DryBulbTemperature < FCStartTemperature)
                return OutletWaterTemperatureFC;

            //湿球温度基準でフリークーリングを起動させる場合
            if (OperatingMode == Mode.SwitchWithWBTemp &&
                airState.WetBulbTemperature < FCStartTemperature)
                return OutletWaterTemperatureFC;
            
            //その他の場合は通常の出口温度制御
            return OutletWaterTemperature;
        }

        /// <summary>CTScheduleの複製を返す</summary>
        /// <returns>CTScheduleの複製</returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }

}
