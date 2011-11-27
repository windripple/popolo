/* WeatherData.cs
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
    /// <summary>気象データ</summary>
    [Serializable]
    public class WeatherData : ISerializable, ImmutableWeatherData, ICloneable
    {

        #region Constants

        /// <summary>シリアライズバージョン</summary>
        const double S_VERSION = 1.0;

        #endregion

        #region 列挙型

        /// <summary>データソースの種類</summary>
        public enum DataSource
        {
            /// <summary>実測値</summary>
            MeasuredValue,
            /// <summary>計算値（他の状態値に基づく理論的計算結果）</summary>
            CalculatedValue,
            /// <summary>予測値（統計値や補間値）</summary>
            PredictedValue,
            /// <summary>欠測値</summary>
            MissingValue,
            /// <summary>不明</summary>
            Unknown
        }

        #endregion

        #region Instance variables

        /// <summary>値</summary>
        private double value = 0;

        /// <summary>ソース</summary>
        private DataSource source = DataSource.MissingValue;

        /// <summary>誤差率[-]</summary>
        private double errorRate = -1;

        #endregion

        #region Properties

        /// <summary>値を設定・取得する</summary>
        public double Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        /// <summary>ソースを設定・取得する</summary>
        public DataSource Source
        {
            get
            {
                return source;
            }
            set
            {
                this.source = value;
            }
        }

        /// <summary>値を設定・取得する</summary>
        public double ErrorRate
        {
            get
            {
                return errorRate;
            }
            set
            {
                this.errorRate = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>Constructor</summary>
        public WeatherData() { }

        /// <summary>Constructor</summary>
        /// <param name="value">値</param>
        /// <param name="source">ソース</param>
        /// <param name="errorRate">誤差率[-]</param>
        public WeatherData(double value, DataSource source, double errorRate)
        {
            this.value = value;
            this.source = source;
            this.errorRate = errorRate;
        }

        #endregion

        #region ISerializableインターフェース実装

        /// <summary>デシリアライズ用Constructor</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected WeatherData(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            double version = sInfo.GetDouble("S_Version");

            //地点名称
            value = sInfo.GetDouble("Value");
            //緯度[度]
            source = (DataSource)sInfo.GetValue("Source", typeof(DataSource));
            //経度[度]
            errorRate = sInfo.GetDouble("ErrorRate");
         }

        /// <summary>シリアル化処理</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //バージョン情報
            info.AddValue("S_Version", S_VERSION);

            //値
            info.AddValue("Value", value);
            //ソース
            info.AddValue("Source", source);
            //誤差率[-]
            info.AddValue("ErrorRate", errorRate);
        }

        #endregion

        #region ICloneableインターフェース実装

        /// <summary>WeatherDataの複製を返す</summary>
        /// <returns>WeatherDataの複製</returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion

    }

    #region 読み取り専用気象データ

    /// <summary>読み取り専用気象データ</summary>
    public interface ImmutableWeatherData
    {
        #region Properties

        /// <summary>値を取得する</summary>
        double Value
        {
            get;
        }

        /// <summary>ソースを取得する</summary>
        WeatherData.DataSource Source
        {
            get;
        }

        /// <summary>値を取得する</summary>
        double ErrorRate
        {
            get;
        }

        #endregion
    }

    #endregion

}
