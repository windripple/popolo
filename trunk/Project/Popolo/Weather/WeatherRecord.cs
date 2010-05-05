/* WeatherRecord.cs
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

using System.Runtime.Serialization;

namespace Popolo.Weather
{
    /// <summary>気象レコード（気象データの集合体）</summary>
    [Serializable]
    public class WeatherRecord : ISerializable, IComparable, ImmutableWeatherRecord, ICloneable
    {

        #region 定数宣言

        /// <summary>シリアライズバージョン</summary>
        const double S_VERSION = 1.0;

        #endregion

        #region 列挙型

        /// <summary>気象データの種類</summary>
        public enum RecordType
        {
            /// <summary>乾球温度[C(DB)]</summary>
            DryBulbTemperature,
            /// <summary>絶対湿度[kg/kg(DA)]</summary>
            HumidityRatio,
            /// <summary>露点温度[C(DB)]</summary>
            DewPointTemperature,
            /// <summary>相対湿度[%]</summary>
            RelativeHumidity,
            /// <summary>大気圧[kPa]</summary>
            AtmosphericPressure,
            /// <summary>大気圏外水平面日射量[W/m2]*</summary>
            ExtraterrestrialHorizontalRadiation,
            /// <summary>大気圏外法線面日射量[W/m2]*</summary>
            ExtraterrestrialDirectNormalRadiation,
            /// <summary>水平面赤外線放射量[W/m2]*</summary>
            HorizontalInfraredRadiationFromSky,
            /// <summary>水平面全天日射量[W/m2]</summary>
            GlobalHorizontalRadiation,
            /// <summary>法線面直達日射量[W/m2]</summary>
            DirectNormalRadiation,
            /// <summary>水平面天空日射量[W/m2]</summary>
            DiffuseHorizontalRadiation,
            /// <summary>水平面全天空照度[lux] = 直射 + 天空</summary>
            GlobalHorizontalIlluminance,
            /// <summary>法線面直射日光照度[lux]</summary>
            DirectNormalIlluminance,
            /// <summary>水平面天空照度[lux]</summary>
            DiffuseHorizontalIlluminance,
            /// <summary>天頂輝度[Cd/m2]</summary>
            ZenithLuminance,
            /// <summary>風向[degree]（南を0度、北を180度、西を90度、東を-90度とする）</summary>
            WindDirection,
            /// <summary>風速[m/s]</summary>
            WindSpeed,
            /// <summary>雲量（10分比）[-]</summary>
            TotalSkyCover,
            /// <summary>完全に日射を遮る雲量[-]</summary>
            OpaqueSkyCover,
            /// <summary>視認距離[km]*</summary>
            Visibility,
            /// <summary>全天の5/8以上を覆う雲層であり、20,000ＦＴ未満のもののうち地表または水面から最も低い雲層の雲底までの高さ[m]*</summary>
            CeilingHeight,
            /// <summary>天気コード[-]*</summary>
            WeatherCode,
            /// <summary>可降水量[mm]</summary>
            PrecipitableWater,
            /// <summary>大気混濁度[1/1000]</summary>
            AerosolOpticalDepth,
            /// <summary>降雪量[cm]</summary>
            SnowDepth,
            /// <summary>最終降雪日からの経過日数[日]</summary>
            DaysSinceLastSnowfall,
            /// <summary>降水量[mm]</summary>
            PrecipitationLevel,
            /// <summary>アルベド[-]</summary>
            Albedo,
            /// <summary>夜間放射量[W/m2]</summary>
            NocturnalRadiation
        }

        #endregion

        #region インスタンス変数

        /// <summary>日時データ</summary>
        private DateTime dateTime;

        /// <summary>気象データ</summary>
        private Dictionary<RecordType, WeatherData> data = new Dictionary<RecordType, WeatherData>();

        #endregion

        #region プロパティ

        /// <summary>気象レコードの日時を取得・設定する</summary>
        public DateTime DataDTime
        {
            get
            {
                return dateTime;
            }
            set
            {
                dateTime = value;
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        public WeatherRecord() { }

        #endregion

        #region publicメソッド

        /// <summary>空のWeatherRecordを返す</summary>
        /// <returns>空のWeatherRecord</returns>
        public static WeatherRecord GetEmptyWeatherRecord()
        {
            WeatherRecord wr = new WeatherRecord();
            wr.FillMissingData();
            return wr;
        }

        /// <summary>気象データを設定する</summary>
        /// <param name="rType">気象データの種類</param>
        /// <param name="wData">気象データ</param>
        public void SetData(RecordType rType, WeatherData wData)
        {
            if (data.ContainsKey(rType)) data[rType] = (WeatherData)wData.Clone();
            else
            {
                data.Add(rType, (WeatherData)wData.Clone());
            }
        }

        /// <summary>気象データを取得する</summary>
        /// <param name="rType">気象データの種類</param>
        /// <returns>気象データ</returns>
        public WeatherData GetData(RecordType rType)
        {
            WeatherData wData;
            if(data.TryGetValue(rType, out wData)) return wData;
            else return null;
        }

        /// <summary>存在しないデータを欠測データとして埋める</summary>
        public void FillMissingData()
        {
            foreach (RecordType rt in Enum.GetValues(typeof(RecordType))) {
                if (!data.ContainsKey(rt))
                {
                    data.Add(rt, new WeatherData());
                }
            }
        }

        #endregion

        #region IComparableインターフェース実装

        /// <summary>WeatherDataオブジェクトと自身を比較する</summary>
        /// <param name="target">WeatherDataオブジェクト</param>
        /// <returns>自身が小さい場合は負、大きい場合は正、等値の場合は0を返す</returns>
        public int CompareTo(object target)
        {
            WeatherRecord wData = (WeatherRecord)target;
            if (this.dateTime < wData.dateTime) return -1;
            else if (wData.dateTime < this.dateTime) return 1;
            else return 0;
        }

        #endregion

        #region ISerializableインターフェース実装

        /// <summary>デシリアライズ用コンストラクタ</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected WeatherRecord(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            double version = sInfo.GetDouble("S_Version");

            //地点名称
            int keyNumber = sInfo.GetInt32("KeyNumber");
            for (int i = 0; i < keyNumber; i++)
            {
                RecordType rt = (RecordType)sInfo.GetValue("DataKey", typeof(RecordType));
                WeatherData wd = (WeatherData)sInfo.GetValue("DataValue", typeof(WeatherData));
                data.Add(rt, wd);
            }
        }

        /// <summary>シリアル化処理</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //バージョン情報
            info.AddValue("S_Version", S_VERSION);

            //地点名称
            info.AddValue("DateTime", dateTime);
            //気象データ
            int index = 0;
            info.AddValue("KeyNumber", data.Keys.Count);
            foreach (RecordType rt in data.Keys)
            {
                info.AddValue("DataKey" + index.ToString(), rt);
                info.AddValue("DataValue" + index.ToString(), rt);
            }
        }

        #endregion

        #region ICloneableインターフェース実装

        /// <summary>WeatherRecordの複製を返す</summary>
        /// <returns>WeatherRecordの複製</returns>
        public object Clone()
        {
            WeatherRecord wr = (WeatherRecord)this.MemberwiseClone();

            wr.data = new Dictionary<RecordType, WeatherData>();
            foreach (RecordType rt in data.Keys)
            {
                wr.data.Add(rt, (WeatherData)data[rt].Clone());
            }
            return wr;
        }

        #endregion

    }

    #region 読み取り専用気象レコード

    /// <summary>読み取り専用気象レコード</summary>
    public interface ImmutableWeatherRecord
    {

        /// <summary>気象レコードの日時を取得する</summary>
        DateTime DataDTime
        {
            get;
        }

        /// <summary>気象データを取得する</summary>
        /// <param name="rType">気象データの種類</param>
        /// <returns>気象データ</returns>
        WeatherData GetData(WeatherRecord.RecordType rType);

    }

    #endregion

}
