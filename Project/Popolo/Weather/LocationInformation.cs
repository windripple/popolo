/* Location.cs
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
    /// <summary>気象地点情報</summary>
    [Serializable]
    public class LocationInformation : ISerializable, ICloneable, ImmutableLocation
    {

        #region Constants

        /// <summary>シリアライズバージョン</summary>
        const double S_VERSION = 1.0;

        #endregion

        #region Instance variables

        /// <summary>ID</summary>
        private int id;

        /// <summary>地点名称</summary>
        private string name;

        /// <summary>英語名称</summary>
        private string eName;

        /// <summary>緯度[degree]（南が負、北が正）</summary>
        private double latitude = 0.0;

        /// <summary>経度[degree]（西が負、東が正）</summary>
        private double longitude = 0.0;

        /// <summary>標準時の経度[degree]（西が負、東が正）</summary>
        private double longitudeAtStandardTime;

        /// <summary>海抜[m]</summary>
        private double elevation;

        /// <summary>時間帯</summary>
        private double timeZone;

        #endregion

        #region Properties

        /// <summary>地点IDを設定・取得する</summary>
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

        /// <summary>地点名称を設定・取得する</summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>英語地点名称を設定・取得する</summary>
        public string EnglishName
        {
            get
            {
                return eName;
            }
            set
            {
                eName = value;
            }
        }

        /// <summary>緯度[deg]を設定・取得する</summary>
        /// <remarks>
        /// 南が負、北が正
        /// -90~90
        /// </remarks>
        public double Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                latitude = Math.Max(-90, Math.Min(90, value));
            }
        }

        /// <summary>経度[deg]を設定・取得する</summary>
        /// <remarks>
        /// 西が負、東が正
        /// -180~180
        /// </remarks>
        public double Longitude
        {
            get
            {
                return longitude;
            }
            set
            {
                longitude = Math.Max(-180, Math.Min(180, value));
            }
        }

        /// <summary>標準時での経度[deg]を設定・取得する</summary>
        /// <remarks>
        /// 西が負、東が正
        /// -180~180
        /// </remarks>
        public double LongitudeAtStandardTime
        {
            get
            {
                return longitudeAtStandardTime;
            }
            set
            {
                longitudeAtStandardTime = Math.Max(-180, Math.Min(180, value));
            }
        }

        /// <summary>海抜[m]を設定・取得する</summary>
        public double Elevation
        {
            get
            {
                return elevation;
            }
            set
            {
                elevation = Math.Max(-1000, Math.Min(9999, value));
            }
        }

        /// <summary>時間帯を設定・取得する</summary>
        public double TimeZone
        {
            get
            {
                return timeZone;
            }
            set
            {
                timeZone = Math.Max(-12.0, Math.Min(12.0, value));
            }
        }

        #endregion

        #region Constructor

        /// <summary>Constructor</summary>
        public LocationInformation() { }

        /// <summary>Constructor</summary>
        /// <param name="id">ID</param>
        /// <param name="name">名称</param>
        /// <param name="eName">英語名称</param>
        /// <param name="latitude">緯度[degree]</param>
        /// <param name="longitude">経度[degree]</param>
        /// <param name="longitudeAtStandardTime">標準時での経度[deg]</param>
        /// <param name="elevation">海抜[m]</param>
        public LocationInformation(int id, string name, string eName, double latitude,
            double longitude, double longitudeAtStandardTime, double elevation)
        {
            this.id = id;
            this.name = name;
            this.eName = eName;
            this.latitude = latitude;
            this.longitude = longitude;
            this.longitudeAtStandardTime = longitudeAtStandardTime;
            this.elevation = elevation;
        }

        #endregion

        #region ISerializableインターフェース実装

        /// <summary>デシリアライズ用Constructor</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected LocationInformation(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            double version = sInfo.GetDouble("S_Version");

            //地点名称
            name = sInfo.GetString("Name");
            //緯度[度]
            latitude = sInfo.GetDouble("Latitude");
            //経度[度]
            longitude = sInfo.GetDouble("Longitude");
            //海抜[m]
            elevation = sInfo.GetDouble("Elevation");
            //時間帯
            timeZone = sInfo.GetDouble("TimeZone");
         }

        /// <summary>シリアル化処理</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //バージョン情報
            info.AddValue("S_Version", S_VERSION);

            //地点名称
            info.AddValue("Name", name);
            //緯度[度]
            info.AddValue("Latitude", latitude);
            //経度[度]
            info.AddValue("Longitude", longitude);
            //海抜[m]
            info.AddValue("Elevation", elevation);
            //時間帯
            info.AddValue("TimeZone", timeZone);
        }

        #endregion

        #region ICloneableインターフェース実装

        /// <summary>Locationの複製を返す</summary>
        /// <returns>Locationの複製</returns>
        public object Clone()
        {
            LocationInformation lc = (LocationInformation)this.MemberwiseClone();

            return lc;
        }

        #endregion

    }

    #region 読み取り専用気象地点情報

    /// <summary>読み取り専用気象地点情報</summary>
    public interface ImmutableLocation
    {

        /// <summary>地点名称を取得する</summary>
        string Name
        {
            get;
        }

        /// <summary>緯度[deg]を取得する</summary>
        /// <remarks>
        /// 南が負、北が正
        /// -90~90
        /// </remarks>
        double Latitude
        {
            get;
        }

        /// <summary>経度[deg]を取得する</summary>
        /// <remarks>
        /// 西が負、東が正
        /// -180~180
        /// </remarks>
        double Longitude
        {
            get;
        }

        /// <summary>海抜[m]を取得する</summary>
        double Elevation
        {
            get;
        }

        /// <summary>時間帯を取得する</summary>
        double TimeZone
        {
            get;
        }

    }

    #endregion

}
