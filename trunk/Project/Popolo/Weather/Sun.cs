/* Sun.cs
 * 
 * Copyright (C) 2008 E.Togashi
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

namespace Popolo.Weather
{
    /// <summary>太陽クラス</summary>
    /// <remarks>
    /// 数値計算で学ぶ光と熱の建築環境学, pp.20, 丸善, 宿谷昌則, 1993
    /// パソコンによる空気調和計算法, 宇田川光弘, 1986
    /// </remarks>
    public class Sun : ImmutableSun
    {

        #region 定数宣言

        /// <summary>DegreeをRadianに変換する係数</summary>
        const double DEG_TO_RAD = 2d * Math.PI / 360d;

        /// <summary>太陽定数[W/m2]</summary>
        const double SOLAR_CONSTANT = 1370d;

        /// <summary>黒体の放射定数[W/m2-K4]</summary>
        const double BLACK_RADIATION = 5.67e-8;

        #endregion

        #region 列挙型定義

        /// <summary>直散分離の手法</summary>
        public enum DiffuseAndDirectNormalRadiationEstimatingMethod
        {
            /// <summary>Berlageの式</summary>
            /// <remarks>Berlage,Von H.P.:Zur Theorie der Beleuchtung einer horizontalen Flache durch Tageslicht,Meteorologische Zeitschrift, May 1928,pp.174-180</remarks>
            Berlage,
            /// <summary>松尾の式</summary>
            /// <remarks>松尾陽:日本建築学会論文報告集,快晴時の日射について 日射量に関する研究2,pp.21-24,1960</remarks>
            Matsuo,
            /// <summary>永田の式</summary>
            /// <remarks>永田忠彦:晴天空による水平面散乱の日射の式の試案,日本建築学会学術講演梗概集,1978</remarks>
            Nagata,
            /// <summary>Liu-Jordanの式</summary>
            /// <remarks>Liu,B.Y.H-Jordan,R.C:The interrelationship and characteristic distribution of direct, diffuse and total solar radiation, solar energy, Vol.4, No.3, 1960</remarks>
            LiuAndJordan,
            /// <summary>宇田川の式</summary>
            /// <remarks>宇田川光弘,木村建一:水平面全天日射量観測値よりの直達日射量の推定,日本建築学会論文報告集,No.267,pp.83-90,19780530</remarks>
            Udatgawa,
            /// <summary>渡辺の式</summary>
            /// <remarks>渡辺俊行:水平面全天日射量の直散分離と傾斜面日射量の推定,日本建築学会論文報告集,No.330,pp.96-108,19830830</remarks>
            Watanabe,
            /// <summary>赤坂の式</summary>
            /// <remarks>H.Akasaka:Model of circumsolar radiation and diffuse sky radiation including cloudy sky, ISES, Solar World Congress, 1991</remarks>
            Akasaka,
            /// <summary>三木の式</summary>
            /// <remarks>三木信博:標準気象データの日射直散分離に関する研究 その6 日射直散分離法の提案,日本建築学会学術講演梗概集,pp.857-858,1991</remarks>
            Miki
        }

        #endregion

        #region クラス変数

        /// <summary>都市-位置対応リスト</summary>
        private static Dictionary<City, double[]> cities = new Dictionary<City,double[]>();

        #endregion

        #region インスタンス変数

        /// <summary>太陽高度[radian]</summary>
        private double altitude;

        /// <summary>太陽方位角[radian]</summary>
        private double orientation;

        /// <summary>法線面直達日射量[W/m2]</summary>
        private double directNormalRadiation;

        /// <summary>水平面天空日射量[W/m2]</summary>
        private double diffuseHorizontalRadiation;

        /// <summary>水平面全天日射量[W/m2]</summary>
        private double globalHorizontalRadiation;

        /// <summary>編集番号</summary>
        private uint revision = 0;

        /// <summary>計算地点の緯度[degree]</summary>
        private double latitude;

        /// <summary>計算地点の経度[degree]</summary>
        private double xLongitude;

        /// <summary>標準時を規定する地点の経度（東経で正）[degree]</summary>
        private double sLongitude;

        #endregion

        #region プロパティ

        /// <summary>太陽定数[W/m2]を取得する</summary>
        public static double SolarConstant
        {
            get
            {
                return SOLAR_CONSTANT;
            }
        }

        /// <summary>太陽高度[radian]を取得する</summary>
        public double Altitude
        {
            get
            {
                return altitude;
            }
        }

        /// <summary>太陽方位角[radian]を取得する</summary>
        public double Orientation
        {
            get
            {
                return orientation;
            }
        }

        /// <summary>法線面直達日射量[W/m2]を設定・取得する</summary>
        public double DirectNormalRadiation
        {
            get
            {
                return directNormalRadiation;
            }
            set
            {
                directNormalRadiation = value;
                revision++;
            }
        }

        /// <summary>水平面天空（散乱）日射量[W/m2]を設定・取得する</summary>
        public double DiffuseHorizontalRadiation
        {
            get
            {
                return diffuseHorizontalRadiation;
            }
            set
            {
                diffuseHorizontalRadiation = value;
                revision++;
            }
        }

        /// <summary>水平面全天日射量[W/m2]を設定・取得する</summary>
        public double GlobalHorizontalRadiation
        {
            get
            {
                return globalHorizontalRadiation;
            }
            set
            {
                globalHorizontalRadiation = value;
                revision++;
            }
        }

        /// <summary>計算地点の緯度（北が正）[degree]を取得する</summary>
        public double Latitude
        {
            get
            {
                return latitude;
            }
        }

        /// <summary>計算地点の経度（東が正）[degree]を取得する</summary>
        public double Longitude
        {
            get
            {
                return xLongitude;
            }
        }

        /// <summary>標準時を規定する地点の経度（東が正）[degree]を取得する</summary>
        public double StandardLongitude
        {
            get
            {
                return sLongitude;
            }
        }

        /// <summary>編集番号を取得する</summary>
        public uint Revision
        {
            get
            {
                return revision;
            }
        }

        /// <summary>現在の日時を取得する</summary>
        public DateTime CurrentDateTime
        {
            get;
            private set;
        }

        /// <summary>日没の時刻を取得する</summary>
        public DateTime SunSetTime
        {
            get;
            private set;
        }

        /// <summary>日の出の時刻を取得する</summary>
        public DateTime SunRiseTime
        {
            get;
            private set;
        }

        #endregion

        #region 列挙体定義

        /// <summary>都市</summary>
        public enum City
        {
            /// <summary></summary>
            Aberdeen,
            /// <summary></summary>
            Algiers,
            /// <summary>アムステルダム</summary>
            Amsterdam,
            /// <summary>アンカラ</summary>
            Ankara,
            /// <summary></summary>
            Asuncion,
            /// <summary></summary>
            Athens,
            /// <summary></summary>
            Auckland,
            /// <summary>バンコク</summary>
            Bangkok,
            /// <summary></summary>
            Barcelona,
            /// <summary>北京</summary>
            Beijing,
            /// <summary></summary>
            Belem,
            /// <summary></summary>
            Belfast,
            /// <summary></summary>
            Belgrade,
            /// <summary></summary>
            Berlin,
            /// <summary></summary>
            Birmingham,
            /// <summary></summary>
            Bogota,
            /// <summary></summary>
            Bordeaux,
            /// <summary></summary>
            Bremen,
            /// <summary></summary>
            Brisbane,
            /// <summary></summary>
            Bristol,
            /// <summary></summary>
            Brussels,
            /// <summary></summary>
            Bucharest,
            /// <summary></summary>
            Budapest,
            /// <summary></summary>
            BuenosAires,
            /// <summary></summary>
            Cairo,
            /// <summary></summary>
            Canton,
            /// <summary></summary>
            CapeTown,
            /// <summary></summary>
            Caracas,
            /// <summary></summary>
            Cayenne,
            /// <summary></summary>
            Chihuahua,
            /// <summary></summary>
            Chongqing,
            /// <summary></summary>
            Copenhagen,
            /// <summary></summary>
            Cordoba,
            /// <summary></summary>
            Dakar,
            /// <summary></summary>
            Djibouti,
            /// <summary>ダブリン</summary>
            Dublin,
            /// <summary></summary>
            Durban,
            /// <summary></summary>
            Edinburgh,
            /// <summary></summary>
            Frankfurt,
            /// <summary></summary>
            Georgetown,
            /// <summary>グラスゴー</summary>
            Glasgow,
            /// <summary></summary>
            GuatemalaCity,
            /// <summary></summary>
            Guayaquil,
            /// <summary></summary>
            Hamburg,
            /// <summary></summary>
            Hammerfest,
            /// <summary>ハバナ</summary>
            Havana,
            /// <summary>ヘルシンキ</summary>
            Helsinki,
            /// <summary></summary>
            Hobart,
            /// <summary>香港</summary>
            HongKong,
            /// <summary></summary>
            Iquique,
            /// <summary>イルクーツク</summary>
            Irkutsk,
            /// <summary></summary>
            Jakarta,
            /// <summary></summary>
            Johannesburg,
            /// <summary></summary>
            Kingston,
            /// <summary></summary>
            Kinshasa,
            /// <summary></summary>
            KualaLumpur,
            /// <summary></summary>
            LaPaz,
            /// <summary></summary>
            Leeds,
            /// <summary>リマ</summary>
            Lima,
            /// <summary>リスボン</summary>
            Lisbon,
            /// <summary></summary>
            Liverpool,
            /// <summary>ロンドン</summary>
            London,
            /// <summary></summary>
            Lyons,
            /// <summary>マドリッド</summary>
            Madrid,
            /// <summary>マンチェスター</summary>
            Manchester,
            /// <summary></summary>
            Manila,
            /// <summary></summary>
            Marseilles,
            /// <summary></summary>
            Mazatlan,
            /// <summary>メッカ</summary>
            Mecca,
            /// <summary>メルボルン</summary>
            Melbourne,
            /// <summary>メキシコシティ</summary>
            MexicoCity,
            /// <summary></summary>
            Milan,
            /// <summary></summary>
            Montevideo,
            /// <summary></summary>
            Moscow,
            /// <summary></summary>
            Munich,
            /// <summary>長崎</summary>
            Nagasaki,
            /// <summary>名古屋</summary>
            Nagoya,
            /// <summary>ナイロビ</summary>
            Nairobi,
            /// <summary>南京</summary>
            Nanjing,
            /// <summary></summary>
            Naples,
            /// <summary></summary>
            NewcastleOnTyne,
            /// <summary></summary>
            Odessa,
            /// <summary>大阪</summary>
            Osaka,
            /// <summary></summary>
            Oslo,
            /// <summary></summary>
            PanamaCity,
            /// <summary></summary>
            Paramaribo,
            /// <summary>パリ</summary>
            Paris,
            /// <summary></summary>
            Perth,
            /// <summary></summary>
            Plymouth,
            /// <summary></summary>
            PortMoresby,
            /// <summary></summary>
            Prague,
            /// <summary></summary>
            Reykjavík,
            /// <summary></summary>
            RioDeJaneiro,
            /// <summary>ローマ</summary>
            Rome,
            /// <summary></summary>
            Salvador,
            /// <summary></summary>
            Santiago,
            /// <summary></summary>
            StPetersburg,
            /// <summary></summary>
            SaoPaulo,
            /// <summary></summary>
            Shanghai,
            /// <summary>シンガポール</summary>
            Singapore,
            /// <summary></summary>
            Sofia,
            /// <summary>ストックホルム</summary>
            Stockholm,
            /// <summary>シドニー</summary>
            Sydney,
            /// <summary></summary>
            Tananarive,
            /// <summary>東京</summary>
            Tokyo,
            /// <summary></summary>
            Tripoli,
            /// <summary>ヴェネチア</summary>
            Venice,
            /// <summary></summary>
            Veracruz,
            /// <summary></summary>
            Vienna,
            /// <summary></summary>
            Vladivostok,
            /// <summary></summary>
            Warsaw,
            /// <summary></summary>
            Wellington,
            /// <summary>チューリッヒ</summary>
            Zurich
        }

        #endregion

        #region コンストラクタ

        /// <summary>静的コンストラクタ</summary>
        static Sun()
        {
            cities.Add(City.Aberdeen, new double[] { 57.15, -2.15, 0 });
            cities.Add(City.Algiers, new double[] { 36.83, 3, 15 });
            cities.Add(City.Amsterdam, new double[] { 52.37, 4.88, 15 });
            cities.Add(City.Ankara, new double[] { 39.92, 32.92, 30 });
            cities.Add(City.Asuncion, new double[] { -25.25, -57.67, -60 });
            cities.Add(City.Athens, new double[] { 37.97, 23.72, 30 });
            cities.Add(City.Auckland, new double[] { -36.87, 174.75, 180 });
            cities.Add(City.Bangkok, new double[] { 13.75, 100.5, -75 });
            cities.Add(City.Barcelona, new double[] { 41.38, 2.15, 15 });
            cities.Add(City.Beijing, new double[] { 39.92, 116.42, 120 });
            cities.Add(City.Belem, new double[] { -1.47, -48.48, -45 });
            cities.Add(City.Belfast, new double[] { 54.62, -5.93, 0 });
            cities.Add(City.Belgrade, new double[] { 44.87, 20.53, 15 });
            cities.Add(City.Berlin, new double[] { 52.5, 13.42, 15 });
            cities.Add(City.Birmingham, new double[] { 52.42, -1.92, 0 });
            cities.Add(City.Bogota, new double[] { 4.53, -74.25, -75 });
            cities.Add(City.Bordeaux, new double[] { 44.83, -0.52, 15 });
            cities.Add(City.Bremen, new double[] { 53.08, 8.82, 15 });
            cities.Add(City.Brisbane, new double[] { -27.48, 153.13, 150 });
            cities.Add(City.Bristol, new double[] { 51.47, -2.58, 0 });
            cities.Add(City.Brussels, new double[] { 50.87, 4.37, 15 });
            cities.Add(City.Bucharest, new double[] { 44.42, 26.12, 30 });
            cities.Add(City.Budapest, new double[] { 47.5, 19.08, 15 });
            cities.Add(City.BuenosAires, new double[] { -34.58, -58.37, -45 });
            cities.Add(City.Cairo, new double[] { 30.03, 31.35, 30 });
            cities.Add(City.Canton, new double[] { 23.12, 113.25, 120 });
            cities.Add(City.CapeTown, new double[] { -33.92, 18.37, 30 });
            cities.Add(City.Caracas, new double[] { 10.47, -67.03, -60 });
            cities.Add(City.Cayenne, new double[] { 4.82, -52.3, -45 });
            cities.Add(City.Chihuahua, new double[] { 28.62, -106.08, -105 });
            cities.Add(City.Chongqing, new double[] { 29.77, 106.57, 120 });
            cities.Add(City.Copenhagen, new double[] { 55.67, 12.57, 15 });
            cities.Add(City.Cordoba, new double[] { -31.47, -64.17, -45 });
            cities.Add(City.Dakar, new double[] { 14.67, -17.47, 0 });
            cities.Add(City.Djibouti, new double[] { 11.5, 43.05, 45 });
            cities.Add(City.Dublin, new double[] { 53.33, -6.25, 0 });
            cities.Add(City.Durban, new double[] { -29.88, 30.88, 30 });
            cities.Add(City.Edinburgh, new double[] { 55.92, -3.17, 0 });
            cities.Add(City.Frankfurt, new double[] { 50.12, 8.68, 15 });
            cities.Add(City.Georgetown, new double[] { 6.75, -58.25, -60 });
            cities.Add(City.Glasgow, new double[] { 55.83, -4.25, 0 });
            cities.Add(City.GuatemalaCity, new double[] { 14.62, -90.52, -90 });
            cities.Add(City.Guayaquil, new double[] { -2.17, -79.93, -75 });
            cities.Add(City.Hamburg, new double[] { 53.55, 10.03, 15 });
            cities.Add(City.Hammerfest, new double[] { 70.63, 23.63, 15 });
            cities.Add(City.Havana, new double[] { 23.13, -82.38, -75 });
            cities.Add(City.Helsinki, new double[] { 60.17, 25, 30 });
            cities.Add(City.Hobart, new double[] { -42.87, 147.32, 150 });
            cities.Add(City.HongKong, new double[] { 22.33, 114.18, 120 });
            cities.Add(City.Iquique, new double[] { -20.17, -70.12, -60 });
            cities.Add(City.Irkutsk, new double[] { 52.5, 104.33, 120 });
            cities.Add(City.Jakarta, new double[] { -6.27, 106.8, 105 });
            cities.Add(City.Johannesburg, new double[] { -26.2, 28.07, 30 });
            cities.Add(City.Kingston, new double[] { 17.98, -76.82, -75 });
            cities.Add(City.Kinshasa, new double[] { -4.3, 15.28, 15 });
            cities.Add(City.KualaLumpur, new double[] { 3.13, 101.7, 120 });
            cities.Add(City.LaPaz, new double[] { -16.45, -68.37, -60 });
            cities.Add(City.Leeds, new double[] { 53.75, -1.5, 0 });
            cities.Add(City.Lima, new double[] { -12, -77.03, -75 });
            cities.Add(City.Lisbon, new double[] { 38.73, -9.15, 0 });
            cities.Add(City.Liverpool, new double[] { 53.42, -3, 0 });
            cities.Add(City.London, new double[] { 51.53, -0.08, 0 });
            cities.Add(City.Lyons, new double[] { 45.75, 4.83, 15 });
            cities.Add(City.Madrid, new double[] { 40.43, -3.7, 15 });
            cities.Add(City.Manchester, new double[] { 53.5, -2.25, 0 });
            cities.Add(City.Manila, new double[] { 14.58, 120.95, 120 });
            cities.Add(City.Marseilles, new double[] { 43.33, 5.33, 15 });
            cities.Add(City.Mazatlan, new double[] { 23.2, -106.42, -105 });
            cities.Add(City.Mecca, new double[] { 21.48, 39.75, 45 });
            cities.Add(City.Melbourne, new double[] { -37.78, 144.97, 150 });
            cities.Add(City.MexicoCity, new double[] { 19.43, -99.12, -90 });
            cities.Add(City.Milan, new double[] { 45.45, 9.17, 15 });
            cities.Add(City.Montevideo, new double[] { -34.88, -56.17, -45 });
            cities.Add(City.Moscow, new double[] { 55.75, 37.6, 45 });
            cities.Add(City.Munich, new double[] { 48.13, 11.58, 15 });
            cities.Add(City.Nagasaki, new double[] { 32.8, 129.95, 135 });
            cities.Add(City.Nagoya, new double[] { 35.12, 136.93, 135 });
            cities.Add(City.Nairobi, new double[] { -1.42, 36.92, 45 });
            cities.Add(City.Nanjing, new double[] { 32.05, 118.88, 120 });
            cities.Add(City.Naples, new double[] { 40.83, 14.25, 15 });
            cities.Add(City.NewcastleOnTyne, new double[] { 54.97, -1.62, 0 });
            cities.Add(City.Odessa, new double[] { 46.45, 30.8, 30 });
            cities.Add(City.Osaka, new double[] { 34.53, 135.5, 135 });
            cities.Add(City.Oslo, new double[] { 59.95, 10.7, 15 });
            cities.Add(City.PanamaCity, new double[] { 8.97, -79.53, -75 });
            cities.Add(City.Paramaribo, new double[] { 5.75, -55.25, -45 });
            cities.Add(City.Paris, new double[] { 48.8, 2.33, 15 });
            cities.Add(City.Perth, new double[] { -31.95, 115.87, 120 });
            cities.Add(City.Plymouth, new double[] { 50.42, -4.08, 0 });
            cities.Add(City.PortMoresby, new double[] { -9.42, 147.13, 150 });
            cities.Add(City.Prague, new double[] { 50.08, 14.43, 15 });
            cities.Add(City.Reykjavík, new double[] { 64.07, -21.97, 0 });
            cities.Add(City.RioDeJaneiro, new double[] { -22.95, -43.2, -45 });
            cities.Add(City.Rome, new double[] { 41.9, 12.45, 15 });
            cities.Add(City.Salvador, new double[] { -12.93, -38.45, -45 });
            cities.Add(City.Santiago, new double[] { -33.47, -70.75, -60 });
            cities.Add(City.StPetersburg, new double[] { 59.93, 30.3, 45 });
            cities.Add(City.SaoPaulo, new double[] { -23.52, -46.52, -45 });
            cities.Add(City.Shanghai, new double[] { 31.17, 121.47, 120 });
            cities.Add(City.Singapore, new double[] { 1.23, 103.92, 120 });
            cities.Add(City.Sofia, new double[] { 42.67, 23.33, 30 });
            cities.Add(City.Stockholm, new double[] { 59.28, 18.05, 15 });
            cities.Add(City.Sydney, new double[] { -34, 151, 150 });
            cities.Add(City.Tananarive, new double[] { -18.83, 47.55, 45 });
            cities.Add(City.Tokyo, new double[] { 35.67, 139.75, 135 });
            cities.Add(City.Tripoli, new double[] { 32.95, 13.2, 30 });
            cities.Add(City.Venice, new double[] { 45.43, 12.33, 15 });
            cities.Add(City.Veracruz, new double[] { 19.17, -96.17, -90 });
            cities.Add(City.Vienna, new double[] { 48.23, 16.33, 15 });
            cities.Add(City.Vladivostok, new double[] { 43.17, 132, 150 });
            cities.Add(City.Warsaw, new double[] { 52.23, 21, 15 });
            cities.Add(City.Wellington, new double[] { -41.28, 174.78, 180 });
            cities.Add(City.Zurich, new double[] { 47.35, 8.52, 15 });
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="latitude">計算地点の緯度（北が正）[degree]</param>
        /// <param name="xLongitude">計算地点の経度（東が正）[degree]</param>
        /// <param name="sLongitude">標準時を規定する地点の経度（東が正）[degree]</param>
        public Sun(double latitude, double xLongitude, double sLongitude)
        {
            this.latitude = latitude;
            this.xLongitude = xLongitude;
            this.sLongitude = sLongitude;
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="city">都市</param>
        public Sun(City city)
        {
            double[] loc = cities[city];
            this.latitude = loc[0];
            this.xLongitude = loc[1];
            this.sLongitude = loc[2];
        }

        /// <summary>コピーコンストラクタ</summary>
        /// <param name="sun">コピーする太陽オブジェクト</param>
        public Sun(ImmutableSun sun)
        {
            this.latitude = sun.Latitude;
            this.xLongitude = sun.Longitude;
            this.sLongitude = sun.StandardLongitude;
        }

        #endregion

        #region publicメソッド

        /// <summary>太陽位置等を更新する</summary>
        /// <param name="dateTime">日時</param>
        public void Update(DateTime dateTime)
        {
            //日付変更の場合には日の出と日没時刻を更新
            if (CurrentDateTime.Month != dateTime.Month || CurrentDateTime.Day != dateTime.Day)
            {
                SunSetTime = GetSunSetTime(latitude, xLongitude, sLongitude, dateTime);
                SunRiseTime = GetSunRiseTime(latitude, xLongitude, sLongitude, dateTime);
            }

            CurrentDateTime = dateTime;
            GetSunPosition(latitude, xLongitude, sLongitude, dateTime, out altitude, out orientation);
            revision++;
        }

        /// <summary>直散分離を行い、法線面直達日射量と水平面全天日射量を推定する</summary>
        /// <param name="globalHorizontalRadiation">水平面全天日射量[W/m2]</param>
        public void EstimateDiffuseAndDirectNormalRadiation(double globalHorizontalRadiation)
        {
            this.GlobalHorizontalRadiation = globalHorizontalRadiation;
            EstimateDiffuseAndDirectNormalRadiation(globalHorizontalRadiation,
                this.Latitude, this.Longitude, this.StandardLongitude,
                this.CurrentDateTime,  out this.directNormalRadiation, out this.diffuseHorizontalRadiation);
        }

        /// <summary>直散分離を行い、法線面直達日射量と水平面全天日射量を推定する</summary>
        /// <param name="method">直散分離の手法</param>
        /// <param name="globalHorizontalRadiation">水平面全天日射量[W/m2]</param>
        public void EstimateDiffuseAndDirectNormalRadiation(double globalHorizontalRadiation, 
            DiffuseAndDirectNormalRadiationEstimatingMethod method)
        {
            this.GlobalHorizontalRadiation = globalHorizontalRadiation;
            EstimateDiffuseAndDirectNormalRadiation(globalHorizontalRadiation,
                this.Latitude, this.Longitude, this.StandardLongitude,
                this.CurrentDateTime, method, out this.directNormalRadiation, out this.diffuseHorizontalRadiation);
        }

        /// <summary>全天日射[W/m2]と天空日射[W/m2]から直達日射[W/m2]を計算して設定する</summary>
        /// <param name="globalHorizontalRadiation">全天日射[W/m2]</param>
        /// <param name="diffuseHorizontalRadiation">天空日射[W/m2]</param>
        public void SetDirectNormalRadiation(double globalHorizontalRadiation, double diffuseHorizontalRadiation)
        {
            GlobalHorizontalRadiation = globalHorizontalRadiation;
            DiffuseHorizontalRadiation = diffuseHorizontalRadiation;
            DirectNormalRadiation = GetDirectNormalRadiation(GlobalHorizontalRadiation, DiffuseHorizontalRadiation, Altitude);
        }

        /// <summary>直達日射[W/m2]と全天日射[W/m2]から天空日射[W/m2]を計算して設定する</summary>
        /// <param name="directNormalRadiation">直達日射[W/m2]</param>
        /// <param name="globalHorizontalRadiation">全天日射[W/m2]</param>
        public void SetDiffuseHorizontalRadiation(double directNormalRadiation, double globalHorizontalRadiation)
        {
            GlobalHorizontalRadiation = globalHorizontalRadiation;
            DirectNormalRadiation = directNormalRadiation;
            DiffuseHorizontalRadiation = GetDiffuseHorizontalRadiation(DirectNormalRadiation, GlobalHorizontalRadiation, Altitude);
        }

        /// <summary>天空日射[W/m2]と直達日射[W/m2]から全天日射[W/m2]を計算して設定する</summary>
        /// <param name="diffuseHorizontalRadiation">天空日射[W/m2]</param>
        /// <param name="directNormalRadiation">直達日射[W/m2]</param>
        public void SetGlobalHorizontalRadiation(double diffuseHorizontalRadiation, double directNormalRadiation)
        {
            DirectNormalRadiation = directNormalRadiation;
            DiffuseHorizontalRadiation = diffuseHorizontalRadiation;
            GlobalHorizontalRadiation = GetGlobalHorizontalRadiation(DiffuseHorizontalRadiation, DirectNormalRadiation, Altitude);
        }

        #endregion

        #region 太陽位置関連のstaticメソッド

        /// <summary>太陽位置を返す</summary>
        /// <param name="latitude">計算地点の緯度[degree]</param>
        /// <param name="xlongitude">計算地点の経度[degree]</param>
        /// <param name="sLongitude">標準時を規定する地点の経度（東経で正）[degree]</param>
        /// <param name="dTime">日時</param>
        /// <param name="altitude">太陽高度[rad]</param>
        /// <param name="orientation">太陽方位角[rad]</param>
        /// <remarks>緯度および経度は北側および東側を+として設定する</remarks>
        /// <example>
        /// 東京における太陽高度および方位の計算法を示す。
        /// ただし、東京は北緯35.68°、東経139.77°
        /// 日本標準時を決定する明石市は東経135°に位置する
        /// <code>
        /// double altitude, azimuth;
        /// Sun.GetPosition(35.68, 139.77, 135, new DateTime(2004, 6, 22, 12, 0, 0, 0), out altitude, out azimuth);
        /// </code>
        /// </example>
        public static void GetSunPosition(double latitude, double xlongitude, double sLongitude,
            DateTime dTime, out double altitude, out double orientation)
        {
            //緯度をRadianに変換
            double phi = DEG_TO_RAD * latitude;

            double b = (360d * (dTime.DayOfYear - 81) / 365d) * DEG_TO_RAD;
            double sd = 0.397949 * Math.Sin(b);
            double cd = Math.Sqrt(1 - sd * sd);
            double e = 1d / 60d * (9.87 * Math.Sin(2 * b) - 7.53 * Math.Cos(b) - 1.5 * Math.Sin(b));
            double tas = dTime.Hour + dTime.Minute / 60d + dTime.Second / 3600d + e + (xlongitude - sLongitude) / 15d;
            double omega = ((tas - 12) * 15) * DEG_TO_RAD;

            //Sin太陽高度を計算
            double sp = Math.Sin(phi);
            double cp = Math.Cos(phi);
            double sh = sp * sd + cp * cd * Math.Cos(omega);

            if (sh < 0)
            {
                altitude = 0;
                orientation = 0;
            }
            else
            {
                altitude = Math.Asin(sh);
                double ch = Math.Sqrt(1.0 - sh * sh);
                double ca = (sh * sp - sd) / (ch * cp);
                orientation = Math.Acos(ca);
                if (omega < 0) orientation *= -1;
            }
        }

        /// <summary>日没の時刻を求める</summary>
        /// <param name="latitude">計算地点の緯度[degree]</param>
        /// <param name="xlongitude">計算地点の経度[degree]</param>
        /// <param name="sLongitude">標準時を規定する地点の経度（東経で正）[degree]</param>
        /// <param name="dTime">日時</param>
        /// <returns>日没の時刻</returns>
        public static DateTime GetSunSetTime(double latitude, double xlongitude, double sLongitude, DateTime dTime)
        {
            //緯度をRadianに変換
            double phi = DEG_TO_RAD * latitude;

            double b = (360d * (dTime.DayOfYear - 81) / 365d) * DEG_TO_RAD;
            double sd = 0.397949 * Math.Sin(b);
            double delta = Math.Asin(sd);
            double cd = Math.Cos(delta);
            double e = 1d / 60d * (9.87 * Math.Sin(2 * b) - 7.53 * Math.Cos(b) - 1.5 * Math.Sin(b));
            double td = Math.Tan(delta);
            double tp = Math.Tan(phi);
            double cOmega = -tp * td;
            double omega = Math.Acos(cOmega);
            double tas = omega / 15d / DEG_TO_RAD + 12;
            double t = tas - e - (xlongitude - sLongitude) / 15d;
            double tt = t * 3600;
            int sec = (int)(tt % 60);
            double ttt = (tt - sec) / 60;
            int minute = (int)(ttt % 60);
            int hour = (int)((ttt - minute) / 60);
            return new DateTime(dTime.Year, dTime.Month, dTime.Day, hour, minute, sec);
        }

        /// <summary>日の出の時刻を求める</summary>
        /// <param name="latitude">計算地点の緯度[degree]</param>
        /// <param name="xlongitude">計算地点の経度[degree]</param>
        /// <param name="sLongitude">標準時を規定する地点の経度（東経で正）[degree]</param>
        /// <param name="dTime">日時</param>
        /// <returns>日の出の時刻</returns>
        public static DateTime GetSunRiseTime(double latitude, double xlongitude, double sLongitude, DateTime dTime)
        {
            //緯度をRadianに変換
            double phi = DEG_TO_RAD * latitude;

            double b = (360d * (dTime.DayOfYear - 81) / 365d) * DEG_TO_RAD;
            double sd = 0.397949 * Math.Sin(b);
            double delta = Math.Asin(sd);
            double cd = Math.Cos(delta);
            double e = 1d / 60d * (9.87 * Math.Sin(2 * b) - 7.53 * Math.Cos(b) - 1.5 * Math.Sin(b));
            double td = Math.Tan(delta);
            double tp = Math.Tan(phi);
            double cOmega = -tp * td;
            double omega = -Math.Acos(cOmega);
            double tas = omega / 15d / DEG_TO_RAD + 12;
            double t = tas - e - (xlongitude - sLongitude) / 15d;
            double tt = t * 3600;
            int sec = (int)(tt % 60);
            double ttt = (tt - sec) / 60;
            int minute = (int)(ttt % 60);
            int hour = (int)((ttt - minute) / 60);
            return new DateTime(dTime.Year, dTime.Month, dTime.Day, hour, minute, sec);
        }

        /// <summary>太陽高度[rad]を返す</summary>
        /// <param name="latitude">緯度[degree]</param>
        /// <param name="xlongitude">計算地点の経度[degree]</param>
        /// <param name="sLongitude">標準時を規定する地点の経度（東経で正）[degree]</param>
        /// <param name="dTime">日時</param>
        /// <returns>太陽高度[rad]</returns>
        public static double GetSunAltitude(double latitude, double xlongitude, double sLongitude, DateTime dTime)
        {
            double altitude, azimuth;
            GetSunPosition(latitude, xlongitude, sLongitude, dTime, out altitude, out azimuth);
            return altitude;
        }

        /// <summary>太陽方位角[rad]を返す</summary>
        /// <param name="latitude">緯度[degree]</param>
        /// <param name="xlongitude">計算地点の経度[degree]</param>
        /// <param name="sLongitude">標準時を規定する地点の経度（東経で正）[degree]</param>
        /// <param name="dTime">日時</param>
        /// <returns>太陽方位角[rad]</returns>
        public static double GetSunAzimuth(double latitude, double xlongitude, double sLongitude, DateTime dTime)
        {
            double altitude, azimuth;
            GetSunPosition(latitude, xlongitude, sLongitude, dTime, out altitude, out azimuth);
            return azimuth;
        }

        /// <summary>太陽赤緯[degree]を返す</summary>
        /// <param name="dTime">日時</param>
        /// <returns>太陽赤緯[degree]</returns>
        public static double GetSunDeclination(DateTime dTime)
        {
            double dDeg = 2d * Math.PI * dTime.DayOfYear / 365d;
            return DEG_TO_RAD * (0.3622133 - 23.24763 * Math.Cos(dDeg + 0.153231) - 0.3368908 *
                Math.Cos(2.0 * dDeg + 0.2070988) - 0.1852646 * Math.Cos(3.0 * dDeg + 0.6201293));
        }

        /// <summary>均時差を返す</summary>
        /// <param name="dTime">日時</param>
        /// <returns>均時差</returns>
        public static double GetEquationOfTime(DateTime dTime)
        {
            double dDeg = 2d * Math.PI * dTime.DayOfYear / 365d;
            return 60.0 * (-0.0002786409 + 0.1227715 * Math.Cos(dDeg + 1.498311) - 0.1654575 *
                Math.Cos(2.0 * dDeg - 1.261546) - 0.00535383 * Math.Cos(3.0 * dDeg - 1.1571));
        }

        /// <summary>時角[°]を返す</summary>
        /// <param name="equationOfTime">均時差</param>
        /// <param name="xlongitude">計算地点の経度[degree]</param>
        /// <param name="slongitude">標準時を規定する地点の経度（東経で正）[degree]</param>
        /// <param name="dTime">日時</param>
        /// <returns>時角[°]</returns>
        /// <remarks>宿谷：数値計算で学ぶ光と熱の建築環境学 p.20</remarks>
        public static double GetHourAngle(double equationOfTime, double xlongitude, double slongitude, DateTime dTime)
        {
            double ts = (double)dTime.Hour + (double)dTime.Minute / 60.0d;
            return (15.0 * (ts - 12.0) + xlongitude - slongitude + 0.25 * equationOfTime) * DEG_TO_RAD;
        }

        #endregion

        #region 日射量関連のstaticメソッド

        /// <summary>大気圏外日射量[W/m2]を計算する</summary>
        /// <param name="daysOfYear">通日(1月1日=1, 12月31日=365)</param>
        /// <returns>大気圏外日射量[W/m2]</returns>
        public static double GetExtraterrestrialRadiation(int daysOfYear)
        {
            return SOLAR_CONSTANT * (1d + 0.033 * Math.Cos(2d * Math.PI * daysOfYear / 365d));
        }

        /// <summary>水平面全天日射量[W/m2]をもとに直散分離を行う</summary>
        /// <param name="globalHorizontalRadiation">水平面全天日射量[W/m2]</param>
        /// <param name="latitude">緯度[degree]</param>
        /// <param name="xlongitude">計算地点の経度[degree]</param>
        /// <param name="sLongitude">標準時を規定する地点の経度（東経で正）[degree]</param>
        /// <param name="dTime">日時</param>
        /// <param name="directSolarRadiation">法線面直達日射量[W/m2]</param>
        /// <param name="diffuseHorizontalRadiation">天空日射[W/m2]</param>
        public static void EstimateDiffuseAndDirectNormalRadiation(
            double globalHorizontalRadiation,
            double latitude, double xlongitude, double sLongitude, DateTime dTime,
            out double directSolarRadiation, out double diffuseHorizontalRadiation)
        {
            //デフォルトは宇田川の手法
            estimateDiffuseAndDirectNormalRadiation(globalHorizontalRadiation, latitude, xlongitude, sLongitude, dTime,
                DiffuseAndDirectNormalRadiationEstimatingMethod.Udatgawa, out directSolarRadiation, out diffuseHorizontalRadiation);
        }

        /// <summary>水平面全天日射量[W/m2]をもとに直散分離を行う</summary>
        /// <param name="globalHorizontalRadiation">水平面全天日射量[W/m2]</param>
        /// <param name="latitude">緯度[degree]</param>
        /// <param name="xlongitude">計算地点の経度[degree]</param>
        /// <param name="sLongitude">標準時を規定する地点の経度（東経で正）[degree]</param>
        /// <param name="dTime">日時</param>
        /// <param name="method">直散分離の手法</param>
        /// <param name="directSolarRadiation">法線面直達日射量[W/m2]</param>
        /// <param name="diffuseHorizontalRadiation">天空日射[W/m2]</param>
        public static void EstimateDiffuseAndDirectNormalRadiation(
            double globalHorizontalRadiation,
            double latitude, double xlongitude, double sLongitude, DateTime dTime,
            DiffuseAndDirectNormalRadiationEstimatingMethod method,
            out double directSolarRadiation, out double diffuseHorizontalRadiation)
        {
            estimateDiffuseAndDirectNormalRadiation(globalHorizontalRadiation, latitude, xlongitude, sLongitude, dTime,
                method, out directSolarRadiation, out diffuseHorizontalRadiation);
        }

        /// <summary>全天日射[W/m2]と天空日射[W/m2]から法線面直達日射[W/m2]を求める</summary>
        /// <param name="globalHorizontalRadiation">全天日射[W/m2]</param>
        /// <param name="diffuseHorizontalRadiation">天空日射[W/m2]</param>
        /// <param name="altitude">太陽高度[radian]</param>
        /// <returns>法線面直達日射[W/m2]</returns>
        public static double GetDirectNormalRadiation(double globalHorizontalRadiation,
            double diffuseHorizontalRadiation, double altitude)
        {
            return (globalHorizontalRadiation - diffuseHorizontalRadiation) / Math.Asin(altitude);
        }

        /// <summary>法線面直達日射[W/m2]と全天日射[W/m2]から天空日射[W/m2]を求める</summary>
        /// <param name="directNormalRadiation">法線面直達日射[W/m2]</param>
        /// <param name="globalHorizontalRadiation">全天日射[W/m2]</param>
        /// <param name="altitude">太陽高度[radian]</param>
        /// <returns>天空日射[W/m2]</returns>
        public static double GetDiffuseHorizontalRadiation(double directNormalRadiation,
            double globalHorizontalRadiation, double altitude)
        {
            return globalHorizontalRadiation - Math.Sin(altitude) * directNormalRadiation;
        }

        /// <summary>天空日射[W/m2]と法線面直達日射[W/m2]から全天日射[W/m2]を求める</summary>
        /// <param name="diffuseHorizontalRadiation">天空日射[W/m2]</param>
        /// <param name="directNormalRadiation">法線面直達日射[W/m2]</param>
        /// <param name="altitude">太陽高度[radian]</param>
        /// <returns>全天日射[W/m2]</returns>
        public static double GetGlobalHorizontalRadiation(double diffuseHorizontalRadiation,
            double directNormalRadiation, double altitude)
        {
            return directNormalRadiation * Math.Sin(altitude) + diffuseHorizontalRadiation;
        }

        #endregion

        #region privateメソッド

        /// <summary>水平面天空日射[W/m2]に基づいて大気透過率[-]を推定する</summary>
        /// <param name="sinH">太陽高度の正弦</param>
        /// <param name="extraterrestrialRadiation">大気圏外日射[W/m2]</param>
        /// <param name="globalHorizontalRadiation">水平面天空日射[W/m2]</param>
        /// <param name="method">推定手法</param>
        /// <returns>大気透過率[-]</returns>
        private static double estimateAtmosphericTransmissivity(double sinH, double extraterrestrialRadiation,
            double globalHorizontalRadiation, DiffuseAndDirectNormalRadiationEstimatingMethod method)
        {
            //入力異常判定
            if (sinH <= 0) return 0;
            if (extraterrestrialRadiation < globalHorizontalRadiation) return 0;
            if (globalHorizontalRadiation <= 0) return 0;

            //初期値
            double atmTrans = 1;

            //収束計算実行;
            int iterNum = 0;
            const int MAX_ITER = 20;
            const double DELTA = 0.0001;
            const double ERR_TOL = 0.001;
            double dn, dff;
            double err1, err2;
            estimateRadiationFromAtmosphericTransmissivity(atmTrans, sinH, extraterrestrialRadiation, method, out dn, out dff);
            err1 = globalHorizontalRadiation - (dn * sinH + dff);
            while (true)
            {
                estimateRadiationFromAtmosphericTransmissivity(atmTrans - DELTA, sinH, extraterrestrialRadiation, method, out dn, out dff);
                err2 = globalHorizontalRadiation - (dn * sinH + dff);
                atmTrans -= err2 * DELTA / (err1 - err2);
                estimateRadiationFromAtmosphericTransmissivity(atmTrans, sinH, extraterrestrialRadiation, method, out dn, out dff);
                err1 = globalHorizontalRadiation - (dn * sinH + dff);

                if (Math.Abs(err2) < ERR_TOL) break;
                iterNum++;
                if (MAX_ITER < iterNum) break;
            }

            return atmTrans;
        }

        /// <summary>水平面全天日射量[W/m2]をもとに直散分離を行う</summary>
        /// <param name="globalHorizontalRadiation">水平面全天日射量[W/m2]</param>
        /// <param name="latitude">緯度[degree]</param>
        /// <param name="xlongitude">計算地点の経度[degree]</param>
        /// <param name="sLongitude">標準時を規定する地点の経度（東経で正）[degree]</param>
        /// <param name="dTime">日時</param>
        /// <param name="directSolarRadiation">法線面直達日射量[W/m2]</param>
        /// <param name="method">直散分離の手法</param>
        /// <param name="diffuseHorizontalRadiation">天空日射[W/m2]</param>
        private static void estimateDiffuseAndDirectNormalRadiation(
            double globalHorizontalRadiation,
            double latitude, double xlongitude, double sLongitude, DateTime dTime,
            DiffuseAndDirectNormalRadiationEstimatingMethod method,
            out double directSolarRadiation, out double diffuseHorizontalRadiation)
        {
            directSolarRadiation = diffuseHorizontalRadiation = 0;
            double io = GetExtraterrestrialRadiation(dTime.DayOfYear);
            double sinH = Math.Sin(Sun.GetSunAltitude(latitude, xlongitude, sLongitude, dTime));
            if (sinH <= 0.001) return;

            //宇田川の手法
            if (method == DiffuseAndDirectNormalRadiationEstimatingMethod.Udatgawa)
            {
                double ktc = 0.5163 + 0.333 * sinH + 0.00803 * sinH * sinH;
                double ktt = Math.Min(1, globalHorizontalRadiation / (io * sinH));
                if (ktc <= ktt) directSolarRadiation = (-0.43 + 1.43 * ktt) * io;
                else directSolarRadiation = (2.277 - 1.258 * sinH + 0.2396 * sinH * sinH) * Math.Pow(ktt, 3) * io;
                directSolarRadiation = Math.Min(directSolarRadiation, globalHorizontalRadiation / sinH);
                diffuseHorizontalRadiation = globalHorizontalRadiation - directSolarRadiation * sinH;
            }
            //三木の手法
            else if (method == DiffuseAndDirectNormalRadiationEstimatingMethod.Miki)
            {
                double lkt = Math.Min(1, globalHorizontalRadiation / (io * sinH));
                double skt = (lkt - 0.15 - 0.2 * sinH) / 0.6;
                double skd;
                if (skt <= 0) skd = 0;
                else skd = 3 * skt * skt - 2 * skt * skt * skt;
                double lkd = Math.Min(skd * lkt, Math.Pow(0.8, (7 + sinH) / (1 + 7 * sinH)));
                double lks = Math.Max(lkt - lkd, 0.005);
                directSolarRadiation = lkd * io;
                directSolarRadiation = Math.Min(directSolarRadiation, globalHorizontalRadiation / sinH);
                diffuseHorizontalRadiation = globalHorizontalRadiation - directSolarRadiation * sinH;
            }
            //数値計算による方法
            else
            {
                double atmTrans = estimateAtmosphericTransmissivity(sinH, io, globalHorizontalRadiation, method);
                estimateRadiationFromAtmosphericTransmissivity(atmTrans, sinH, io, method, out directSolarRadiation, out diffuseHorizontalRadiation);
            }
            
            //0以上とする
            directSolarRadiation = Math.Max(0, directSolarRadiation);
            diffuseHorizontalRadiation = Math.Max(0, diffuseHorizontalRadiation);
        }

        /// <summary>日射に関する大気透過率[-]に基づいて法線面直達日射[W/m2]と水平面天空日射[W/m2]を推定する</summary>
        /// <param name="atmosphericTransmissivity">日射に関する大気透過率[-]</param>
        /// <param name="sinH">太陽高度の正弦</param>
        /// <param name="extraterrestrialRadiation">大気圏外日射[W/m2]</param>
        /// <param name="method">推定手法</param>
        /// <param name="directNormalRadiation">法線面直達日射[W/m2]</param>
        /// <param name="diffuseHorizontalRadiation">水平面天空日射[W/m2]</param>
        private static void estimateRadiationFromAtmosphericTransmissivity(double atmosphericTransmissivity, double sinH,
            double extraterrestrialRadiation, DiffuseAndDirectNormalRadiationEstimatingMethod method,
            out double directNormalRadiation, out double diffuseHorizontalRadiation)
        {
            double pcosech = Math.Pow(atmosphericTransmissivity, 1 / sinH);
            directNormalRadiation = extraterrestrialRadiation * Math.Pow(atmosphericTransmissivity, 1 / sinH);
            diffuseHorizontalRadiation = 0;

            switch (method)
            {
                case DiffuseAndDirectNormalRadiationEstimatingMethod.Akasaka:
                    //赤坂の手法
                    diffuseHorizontalRadiation = sinH * extraterrestrialRadiation * (1 - pcosech) * 0.95 *
                        Math.Pow(atmosphericTransmissivity, 1 / (0.5 + 2.5 * sinH)) * Math.Pow(1 - atmosphericTransmissivity, 2d / 3d);
                    break;
                case DiffuseAndDirectNormalRadiationEstimatingMethod.Berlage:
                    //Berlageの手法
                    diffuseHorizontalRadiation = sinH * extraterrestrialRadiation * (1 - pcosech) *
                        0.5 / (1 - 1.4 * Math.Log(atmosphericTransmissivity));
                    break;
                case DiffuseAndDirectNormalRadiationEstimatingMethod.LiuAndJordan:
                    //Liu&Jordanの方法
                    diffuseHorizontalRadiation = sinH * extraterrestrialRadiation * (0.271 - 0.2939 * pcosech);
                    break;
                case DiffuseAndDirectNormalRadiationEstimatingMethod.Matsuo:
                    //松尾の手法
                    diffuseHorizontalRadiation = sinH * extraterrestrialRadiation * (1 - pcosech) * (1 - atmosphericTransmissivity) *
                       1.2 / (1 - 1.4 * Math.Log(atmosphericTransmissivity));
                    break;
                case DiffuseAndDirectNormalRadiationEstimatingMethod.Nagata:
                    //永田の手法
                    diffuseHorizontalRadiation = sinH * extraterrestrialRadiation * (1 - pcosech) *
                        (0.66 - 0.32 * sinH) * (0.5 + (0.4 - 0.3 * atmosphericTransmissivity) * sinH);
                    break;
                case DiffuseAndDirectNormalRadiationEstimatingMethod.Watanabe:
                    //渡辺の手法                    
                    double qq = (0.9013 + 1.123 * sinH) * Math.Pow(atmosphericTransmissivity, 0.489 / sinH) * Math.Pow(1 - pcosech, 2.525);
                    diffuseHorizontalRadiation = sinH * extraterrestrialRadiation * qq / (1 + qq);
                    break;
            }
        }

        #endregion

    }

    #region 読み取り専用の太陽interface

    /// <summary>読み取り専用の太陽</summary>
    public interface ImmutableSun
    {
        /// <summary>太陽高度[radian]を取得する</summary>
        double Altitude
        {
            get;
        }

        /// <summary>太陽方位角[radian]を取得する</summary>
        double Orientation
        {
            get;
        }

        /// <summary>法線面直達日射量[W/m2]を取得する</summary>
        double DirectNormalRadiation
        {
            get;
        }

        /// <summary>水平面天空（散乱）日射量[W/m2]を取得する</summary>
        double DiffuseHorizontalRadiation
        {
            get;
        }

        /// <summary>水平面全天日射量[W/m2]を取得する</summary>
        double GlobalHorizontalRadiation
        {
            get;
        }

        /// <summary>編集番号を取得する</summary>
        uint Revision
        {
            get;
        }

        /// <summary>計算地点の緯度（北が正）[degree]を取得する</summary>
        double Latitude
        {
            get;
        }

        /// <summary>計算地点の経度（東が正）[degree]を取得する</summary>
        double Longitude
        {
            get;
        }

        /// <summary>標準時を規定する地点の経度（東が正）[degree]を取得する</summary>
        double StandardLongitude
        {
            get;
        }

        /// <summary>現在の日時を取得する</summary>
        DateTime CurrentDateTime
        {
            get;
        }
    }

    #endregion

}
