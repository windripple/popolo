/* PMDConverter.cs
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

using System.IO;

using Popolo.ThermophysicalProperty;

namespace Popolo.Weather.Converter
{
    /// <summary>PMDデータを変換する</summary>
    public static class PMDConverter
    {

        #region クラス変数

        /// <summary>地点番号-地名対応リスト</summary>
        static readonly Dictionary<int, LocationInformation> locationInfos = new Dictionary<int, LocationInformation>();

        #endregion

        #region 静的Constructor

        /// <summary>静的Constructor</summary>
        static PMDConverter()
        {
            locationInfos.Add(47401, new LocationInformation(47401, "稚内", "WAKKANAI", 45, 141, 135, 2.8));
            locationInfos.Add(47402, new LocationInformation(47402, "北見枝幸", "KITAMIESASHI", 44, 142, 135, 6.7));
            locationInfos.Add(47404, new LocationInformation(47404, "羽幌", "HABORO", 44, 141, 135, 7.9));
            locationInfos.Add(47405, new LocationInformation(47405, "雄武", "OMU", 44, 142, 135, 13.8));
            locationInfos.Add(47406, new LocationInformation(47406, "留萌", "RUMOI", 43, 141, 135, 23.6));
            locationInfos.Add(47407, new LocationInformation(47407, "旭川", "ASAHIKAWA", 43, 142, 135, 120));
            locationInfos.Add(47409, new LocationInformation(47409, "網走", "ABASHIRI", 44, 144, 135, 37.6));
            locationInfos.Add(47411, new LocationInformation(47411, "小樽", "OTARU", 43, 141, 135, 24.9));
            locationInfos.Add(47412, new LocationInformation(47412, "札幌", "SAPPORO", 43, 141, 135, 17.2));
            locationInfos.Add(47417, new LocationInformation(47417, "帯広", "OBIHIRO", 42, 143, 135, 38.4));
            locationInfos.Add(47418, new LocationInformation(47418, "釧路", "KUSHIRO", 42, 144, 135, 4.5));
            locationInfos.Add(47420, new LocationInformation(47420, "根室", "NEMURO", 43, 145, 135, 25.2));
            locationInfos.Add(47421, new LocationInformation(47421, "寿都", "SUTTSU", 42, 140, 135, 33.4));
            locationInfos.Add(47423, new LocationInformation(47423, "室蘭", "MURORAN", 42, 140, 135, 39.9));
            locationInfos.Add(47424, new LocationInformation(47424, "苫小牧", "TOMAKOMAI", 42, 141, 135, 6.3));
            locationInfos.Add(47426, new LocationInformation(47426, "浦河", "URAKAWA", 42, 142, 135, 32.5));
            locationInfos.Add(47428, new LocationInformation(47428, "江差", "ESASHI", 41, 140, 135, 3.7));
            locationInfos.Add(47430, new LocationInformation(47430, "函館", "HAKODATE", 41, 140, 135, 35));
            locationInfos.Add(47435, new LocationInformation(47435, "紋別", "MOMBETSU", 44, 143, 135, 15.8));
            locationInfos.Add(47440, new LocationInformation(47440, "広尾", "HIROO", 42, 143, 135, 32.4));
            locationInfos.Add(47520, new LocationInformation(47520, "新庄", "SHINJO", 38, 140, 135, 105.1));
            locationInfos.Add(47570, new LocationInformation(47570, "若松", "WAKAMATSU", 37, 139, 135, 212.1));
            locationInfos.Add(47574, new LocationInformation(47574, "深浦", "FUKAURA", 40, 139, 135, 66.1));
            locationInfos.Add(47575, new LocationInformation(47575, "青森", "AOMORI", 40, 140, 135, 2.8));
            locationInfos.Add(47576, new LocationInformation(47576, "むつ", "MUTSU", 41, 141, 135, 2.9));
            locationInfos.Add(47581, new LocationInformation(47581, "八戸", "HACHINOHE", 40, 141, 135, 27.1));
            locationInfos.Add(47582, new LocationInformation(47582, "秋田", "AKITA", 39, 140, 135, 6.3));
            locationInfos.Add(47584, new LocationInformation(47584, "盛岡", "MORIOKA", 39, 141, 135, 155.2));
            locationInfos.Add(47585, new LocationInformation(47585, "宮古", "MIYAKO", 39, 141, 135, 42.5));
            locationInfos.Add(47587, new LocationInformation(47587, "酒田", "SAKATA", 38, 139, 135, 3.1));
            locationInfos.Add(47588, new LocationInformation(47588, "山形", "YAMAGATA", 38, 140, 135, 152.5));
            locationInfos.Add(47590, new LocationInformation(47590, "仙台", "SENDAI", 38, 140, 135, 38.9));
            locationInfos.Add(47592, new LocationInformation(47592, "石巻", "ISHINOMAKI", 38, 141, 135, 42.5));
            locationInfos.Add(47595, new LocationInformation(47595, "福島", "FUKUSHIMA", 37, 140, 135, 67.4));
            locationInfos.Add(47597, new LocationInformation(47597, "白河", "SHIRAKAWA", 37, 140, 135, 355));
            locationInfos.Add(47598, new LocationInformation(47598, "小名浜", "ONAHAMA", 36, 140, 135, 3.3));
            locationInfos.Add(47600, new LocationInformation(47600, "輪島", "WAJIMA", 37, 136, 135, 5.2));
            locationInfos.Add(47602, new LocationInformation(47602, "相川", "AIKAWA", 38, 138, 135, 5.5));
            locationInfos.Add(47604, new LocationInformation(47604, "新潟", "NIIGATA", 37, 139, 135, 1.9));
            locationInfos.Add(47605, new LocationInformation(47605, "金沢", "KANAZAWA", 36, 136, 135, 5.7));
            locationInfos.Add(47606, new LocationInformation(47606, "伏木", "FUSHIKI", 36, 137, 135, 11.6));
            locationInfos.Add(47607, new LocationInformation(47607, "富山", "TOYAMA", 36, 137, 135, 8.6));
            locationInfos.Add(47610, new LocationInformation(47610, "長野", "NAGANO", 36, 138, 135, 418.2));
            locationInfos.Add(47612, new LocationInformation(47612, "高田", "TAKADA", 37, 138, 135, 12.9));
            locationInfos.Add(47615, new LocationInformation(47615, "宇都宮", "UTSUNOMIYA", 36, 139, 135, 119.4));
            locationInfos.Add(47616, new LocationInformation(47616, "福井", "FUKUI", 36, 136, 135, 8.8));
            locationInfos.Add(47617, new LocationInformation(47617, "高山", "TAKAYAMA", 36, 137, 135, 560.1));
            locationInfos.Add(47618, new LocationInformation(47618, "松本", "MATSUMOTO", 36, 137, 135, 610));
            locationInfos.Add(47620, new LocationInformation(47620, "諏訪", "SUWA", 36, 138, 135, 760.1));
            locationInfos.Add(47622, new LocationInformation(47622, "軽井沢", "KARUIZAWA", 36, 138, 135, 999.1));
            locationInfos.Add(47624, new LocationInformation(47624, "前橋", "MAEBASHI", 36, 139, 135, 112.1));
            locationInfos.Add(47626, new LocationInformation(47626, "熊谷", "KUMAGAYA", 36, 139, 135, 30));
            locationInfos.Add(47629, new LocationInformation(47629, "水戸", "MITO", 36, 140, 135, 29.3));
            locationInfos.Add(47631, new LocationInformation(47631, "敦賀", "TSURUGA", 35, 136, 135, 1.6));
            locationInfos.Add(47632, new LocationInformation(47632, "岐阜", "GIFU", 35, 136, 135, 12.7));
            locationInfos.Add(47636, new LocationInformation(47636, "名古屋", "NAGOYA", 35, 136, 135, 51.1));
            locationInfos.Add(47638, new LocationInformation(47638, "甲府", "KOFU", 35, 138, 135, 272.8));
            locationInfos.Add(47639, new LocationInformation(47639, "富士山", "FUJISAN", 35, 138, 135, 775.1));
            locationInfos.Add(47640, new LocationInformation(47640, "河口湖", "KAWAGUCHIKO", 35, 138, 135, 859.6));
            locationInfos.Add(47641, new LocationInformation(47641, "秩父", "CHICHIBU", 35, 139, 135, 232.1));
            locationInfos.Add(47646, new LocationInformation(47646, "館野", "TATENO", 36, 140, 135, 25.2));
            locationInfos.Add(47648, new LocationInformation(47648, "銚子", "CHOSHI", 35, 140, 135, 20.1));
            locationInfos.Add(47649, new LocationInformation(47649, "上野", "UENO", 34, 136, 135, 159.2));
            locationInfos.Add(47651, new LocationInformation(47651, "津", "TSU", 34, 136, 135, 2.7));
            locationInfos.Add(47653, new LocationInformation(47653, "伊良湖", "IRAKO", 34, 137, 135, 6.2));
            locationInfos.Add(47654, new LocationInformation(47654, "浜松", "HAMAMATSU", 34, 137, 135, 31.7));
            locationInfos.Add(47655, new LocationInformation(47655, "御前崎", "OMAEZAKI", 34, 138, 135, 44.7));
            locationInfos.Add(47656, new LocationInformation(47656, "静岡", "SHIZUOKA", 34, 138, 135, 14.1));
            locationInfos.Add(47657, new LocationInformation(47657, "三島", "MISHIMA", 35, 138, 135, 20.5));
            locationInfos.Add(47662, new LocationInformation(47662, "東京", "TOKYO", 35, 139, 135, 6.1));
            locationInfos.Add(47663, new LocationInformation(47663, "尾鷲", "OWASE", 34, 136, 135, 15.3));
            locationInfos.Add(47666, new LocationInformation(47666, "石廊崎", "IROZAKI", 34, 138, 135, 54.7));
            locationInfos.Add(47668, new LocationInformation(47668, "網代", "AJIRO", 35, 139, 135, 66.9));
            locationInfos.Add(47670, new LocationInformation(47670, "横浜", "YOKOHAMA", 35, 139, 135, 39.1));
            locationInfos.Add(47674, new LocationInformation(47674, "勝浦", "KATSUURA", 35, 140, 135, 11.9));
            locationInfos.Add(47675, new LocationInformation(47675, "大島", "OSHIMA", 34, 139, 135, 74));
            locationInfos.Add(47677, new LocationInformation(47677, "三宅島", "MIYAKEJIMA", 34, 139, 135, 36.4));
            locationInfos.Add(47678, new LocationInformation(47678, "八丈島", "HACHIJOJIMA", 33, 139, 135, 151.4));
            locationInfos.Add(47682, new LocationInformation(47682, "千葉", "CHIBA", 35, 140, 135, 3.5));
            locationInfos.Add(47684, new LocationInformation(47684, "四日市", "YOKKAICHI", 34, 136, 135, 55.1));
            locationInfos.Add(47690, new LocationInformation(47690, "日光", "NIKKO", 36, 139, 135, 291.9));
            locationInfos.Add(47740, new LocationInformation(47740, "西郷", "SAIGO", 36, 133, 135, 26.5));
            locationInfos.Add(47741, new LocationInformation(47741, "松江", "MATSUE", 35, 133, 135, 16.9));
            locationInfos.Add(47742, new LocationInformation(47742, "境", "SAKAI", 35, 133, 135, 2));
            locationInfos.Add(47744, new LocationInformation(47744, "米子", "YONAGO", 35, 133, 135, 6.4));
            locationInfos.Add(47746, new LocationInformation(47746, "鳥取", "TOTTORI", 35, 134, 135, 7.1));
            locationInfos.Add(47747, new LocationInformation(47747, "豊岡", "TOYOOKA", 35, 134, 135, 3.4));
            locationInfos.Add(47750, new LocationInformation(47750, "舞鶴", "MAIZURU", 35, 135, 135, 2.4));
            locationInfos.Add(47755, new LocationInformation(47755, "浜田", "HAMADA", 34, 132, 135, 19));
            locationInfos.Add(47756, new LocationInformation(47756, "津山", "TSUYAMA", 35, 134, 135, 145.7));
            locationInfos.Add(47759, new LocationInformation(47759, "京都", "KYOTO", 35, 135, 135, 41.4));
            locationInfos.Add(47761, new LocationInformation(47761, "彦根", "HIKONE", 35, 136, 135, 87.3));
            locationInfos.Add(47762, new LocationInformation(47762, "下関", "SHIMONOSEKI", 33, 130, 135, 3.3));
            locationInfos.Add(47765, new LocationInformation(47765, "広島", "HIROSHIMA", 34, 132, 135, 3.6));
            locationInfos.Add(47766, new LocationInformation(47766, "呉", "KURE", 34, 132, 135, 3.5));
            locationInfos.Add(47767, new LocationInformation(47767, "福山", "FUKUYAMA", 34, 133, 135, 1.9));
            locationInfos.Add(47768, new LocationInformation(47768, "岡山", "OKAYAMA", 34, 133, 135, 2.8));
            locationInfos.Add(47769, new LocationInformation(47769, "姫路", "HIMEJI", 34, 134, 135, 38.2));
            locationInfos.Add(47770, new LocationInformation(47770, "神戸", "KOBE", 34, 135, 135, 5.3));
            locationInfos.Add(47772, new LocationInformation(47772, "大阪", "OSAKA", 34, 135, 135, 23));
            locationInfos.Add(47776, new LocationInformation(47776, "洲本", "SUMOTO", 34, 134, 135, 109.3));
            locationInfos.Add(47777, new LocationInformation(47777, "和歌山", "WAKAYAMA", 34, 135, 135, 13.9));
            locationInfos.Add(47778, new LocationInformation(47778, "潮岬", "SHIONOMISAKI", 33, 135, 135, 73));
            locationInfos.Add(47780, new LocationInformation(47780, "奈良", "NARA", 34, 135, 135, 104.4));
            locationInfos.Add(47784, new LocationInformation(47784, "山口", "YAMAGUCHI", 34, 131, 135, 16.7));
            locationInfos.Add(47800, new LocationInformation(47800, "厳原", "IZUHARA", 34, 129, 135, 3.7));
            locationInfos.Add(47805, new LocationInformation(47805, "平戸", "HIRADO", 33, 129, 135, 57.8));
            locationInfos.Add(47807, new LocationInformation(47807, "福岡", "FUKUOKA", 33, 130, 135, 2.5));
            locationInfos.Add(47809, new LocationInformation(47809, "飯塚", "IIZUKA", 33, 130, 135, 37.1));
            locationInfos.Add(47812, new LocationInformation(47812, "佐世保", "SASEBO", 33, 129, 135, 3.9));
            locationInfos.Add(47813, new LocationInformation(47813, "佐賀", "SAGA", 33, 130, 135, 5.5));
            locationInfos.Add(47814, new LocationInformation(47814, "日田", "HITA", 33, 130, 135, 82.9));
            locationInfos.Add(47815, new LocationInformation(47815, "大分", "OITA", 33, 131, 135, 4.6));
            locationInfos.Add(47817, new LocationInformation(47817, "長崎", "NAGASAKI", 32, 129, 135, 26.9));
            locationInfos.Add(47818, new LocationInformation(47818, "雲仙岳", "UNZENDAKE", 32, 130, 135, 677.5));
            locationInfos.Add(47819, new LocationInformation(47819, "熊本", "KUMAMOTO", 32, 130, 135, 37.7));
            locationInfos.Add(47821, new LocationInformation(47821, "阿蘇山", "ASOSAN", 32, 131, 135, 142.3));
            locationInfos.Add(47822, new LocationInformation(47822, "延岡", "NOBEOKA", 32, 131, 135, 19.2));
            locationInfos.Add(47823, new LocationInformation(47823, "阿久根", "AKUNE", 32, 130, 135, 40.1));
            locationInfos.Add(47824, new LocationInformation(47824, "人吉", "HITOYOSHI", 32, 130, 135, 145.8));
            locationInfos.Add(47827, new LocationInformation(47827, "鹿児島", "KAGOSHIMA", 31, 130, 135, 3.9));
            locationInfos.Add(47829, new LocationInformation(47829, "都城", "MIYAKONOJO", 31, 131, 135, 153.8));
            locationInfos.Add(47830, new LocationInformation(47830, "宮崎", "MIYAZAKI", 31, 131, 135, 9.2));
            locationInfos.Add(47831, new LocationInformation(47831, "枕崎", "MAKURAZAKI", 31, 130, 135, 29.5));
            locationInfos.Add(47835, new LocationInformation(47835, "油津", "ABURATSU", 31, 131, 135, 2.9));
            locationInfos.Add(47837, new LocationInformation(47837, "種子島", "TANEGASHIMA", 30, 130, 135, 24.9));
            locationInfos.Add(47838, new LocationInformation(47838, "牛深", "USHIBUKA", 32, 130, 135, 3));
            locationInfos.Add(47843, new LocationInformation(47843, "福江", "FUKUE", 32, 128, 135, 25.1));
            locationInfos.Add(47890, new LocationInformation(47890, "多度津", "TADOTSU", 34, 133, 135, 3.7));
            locationInfos.Add(47891, new LocationInformation(47891, "高松", "TAKAMATSU", 34, 134, 135, 8.7));
            locationInfos.Add(47892, new LocationInformation(47892, "宇和島", "UWAJIMA", 33, 132, 135, 2.4));
            locationInfos.Add(47893, new LocationInformation(47893, "高知", "KOCHI", 33, 133, 135, 0.5));
            locationInfos.Add(47895, new LocationInformation(47895, "徳島", "TOKUSHIMA", 34, 134, 135, 1.6));
            locationInfos.Add(47897, new LocationInformation(47897, "宿毛", "SUKUMO", 32, 132, 135, 2.2));
            locationInfos.Add(47898, new LocationInformation(47898, "清水", "SHIMIZU", 32, 133, 135, 31));
            locationInfos.Add(47909, new LocationInformation(47909, "名瀬", "NAZE", 28, 129, 135, 2.8));
            locationInfos.Add(47912, new LocationInformation(47912, "与那国島", "YONAGUNIJIMA", 24, 123, 135, 30));
            locationInfos.Add(47917, new LocationInformation(47917, "西表島", "IRIOMOTEJIMA", 24, 123, 135, 9.9));
            locationInfos.Add(47918, new LocationInformation(47918, "石垣島", "ISHIGAKIJIMA", 24, 124, 135, 5.7));
            locationInfos.Add(47927, new LocationInformation(47927, "宮古島", "MIYAKOJIMA", 24, 125, 135, 39.9));
            locationInfos.Add(47929, new LocationInformation(47929, "久米島", "KUMEJIMA", 26, 126, 135, 4));
            locationInfos.Add(47936, new LocationInformation(47936, "那覇", "NAHA", 26, 127, 135, 28.1));
            locationInfos.Add(47940, new LocationInformation(47940, "名護", "NAGO", 26, 127, 135, 6.1));
            locationInfos.Add(47945, new LocationInformation(47945, "南大東島", "MINAMIDAITO", 25, 131, 135, 15.3));
            locationInfos.Add(47971, new LocationInformation(47971, "父島", "CHICHIJIMA", 27, 142, 135, 2.7));
            locationInfos.Add(47991, new LocationInformation(47991, "南鳥島", "MINAMITORI.I", 24, 153, 135, 7.1));
            locationInfos.Add(89532, new LocationInformation(89532, "昭和", "SYOUWA", 69, 39, 135, 18.4));

        }

        #endregion

        #region Properties

        /// <summary>気象データ名称を取得する</summary>
        public static string Name
        {
            get
            {
                return "気象庁地上気象観測PMDデータ";
            }
        }

        #endregion

        #region public methods

        /// <summary>ファイルを元にWeatherDataTableを構成する</summary>
        /// <param name="filePath">読み取りファイルのパス</param>
        /// <param name="success">読み取り成功の真偽</param>
        /// <returns>構成されたPWeatherDataオブジェクト</returns>
        public static WeatherDataTable ToPWeatherData(string filePath, out bool success)
        {
            success = false;

            //読み出しファイルの存在確認
            if (!File.Exists(filePath)) return null;

            WeatherDataTable wdTable = new WeatherDataTable();
            Stream strm = File.OpenRead(filePath);
            BufferedStream bStrm = new BufferedStream(strm);

            //地点番号-名称特定
            LocationInformation lInfo;
            byte[] buffer = new byte[3];
            bStrm.Read(buffer, 0, 3);
            string ln = Encoding.GetEncoding(932).GetString(buffer);
            int lNumber;
            if (int.TryParse(ln, out lNumber))
            {
                if (!GetLocationInformation(lNumber, out lInfo))
                {
                    return null;
                }
            }
            else return null;

            //地点情報を設定
            wdTable.Location = lInfo;

            //年月日データまでシーク
            bStrm.Seek(14, SeekOrigin.Begin);

            //1時間データ読み込み処理
            buffer = new byte[14];
            while (true)
            {
                getHourlyData(ref wdTable, bStrm);
                //次の日にちまでシーク
                bStrm.Seek(184, SeekOrigin.Current);
                //年月日データまでシーク
                if (bStrm.Read(buffer, 0, 14) == 0) break;
            }

            success = true;
            return wdTable;
            
        }

        /// <summary>PWeatherDataをファイルに書き出す</summary>
        /// <param name="wdTable">WeatherDataTableオブジェクト</param>
        /// <param name="filePath">書き出しファイルへのパス</param>
        /// <param name="success">書き出し成功の真偽</param>
        public static void FromPWeather(WeatherDataTable wdTable, string filePath, out bool success)
        {
            success = false;
        }

        #endregion

        #region private methods

        /// <summary>地点情報読み込み処理</summary>
        /// <param name="pwData">PWeatherDataオブジェクト</param>
        /// <param name="bStrm">読み取りStream</param>
        private static void getHourlyData(ref WeatherDataTable pwData, BufferedStream bStrm)
        {
            LocationInformation locationInfo = pwData.Location;
            byte[] buffer;
            //年月日情報
            buffer = new byte[8];
            //最終行の場合は終了
            bStrm.Read(buffer, 0, 8);
            string dTime = System.Text.Encoding.GetEncoding(932).GetString(buffer);
            int year = int.Parse(dTime.Substring(0, 4));
            int month = int.Parse(dTime.Substring(4, 2));
            int day = int.Parse(dTime.Substring(6, 2));
            DateTime cTime = new DateTime(year, month, day, 1, 0, 0);

            //1時間データまでシーク
            bStrm.Seek(2, SeekOrigin.Current);

            //24時間データを取得
            buffer = new byte[56];
            WeatherData wd = new WeatherData();
            wd.Source = WeatherData.DataSource.CalculatedValue;
            bool sunRise = false;
            bool hasDR = false;
            for (int i = 0; i < 24; i++)
            {
                WeatherRecord wr = new WeatherRecord();
                wr.DataDTime = cTime;

                //データ取得
                bStrm.Read(buffer, 0, 56);
                string data = System.Text.Encoding.GetEncoding(932).GetString(buffer);

                //気圧[Pa]
                double atm = wd.Value = double.Parse(data.Substring(0, 5)) * 0.01d;
                WeatherData.DataSource atmSource = getDataSource(data.Substring(5, 1));
                wd.Source = atmSource;
                wr.SetData(WeatherRecord.RecordType.AtmosphericPressure, wd);

                //乾球温度[C]
                double dbt = wd.Value = double.Parse(data.Substring(12, 4)) * 0.1;
                WeatherData.DataSource dbtSource = getDataSource(data.Substring(16, 1));
                wd.Source = dbtSource;
                wr.SetData(WeatherRecord.RecordType.DryBulbTemperature, wd);

                //相対湿度[%]
                double rhd = wd.Value = double.Parse(data.Substring(21, 3));
                WeatherData.DataSource rhdSource = getDataSource(data.Substring(24, 1));
                wd.Source = rhdSource;
                wr.SetData(WeatherRecord.RecordType.RelativeHumidity, wd);

                //風向[degree]
                wd.Value = getWindowDirection(int.Parse(data.Substring(25, 2)));
                wd.Source = getDataSource(data.Substring(27, 1));
                wr.SetData(WeatherRecord.RecordType.WindDirection, wd);

                //風速[m/s]
                wd.Value = double.Parse(data.Substring(28, 3)) * 0.1;
                wd.Source = getDataSource(data.Substring(31, 1));
                wr.SetData(WeatherRecord.RecordType.WindSpeed, wd);

                //雲量10分比[-]
                wd.Value = double.Parse(data.Substring(32, 2)) * 0.1;
                wd.Source = getDataSource(data.Substring(34, 1));
                wr.SetData(WeatherRecord.RecordType.TotalSkyCover, wd);

                //天気記号
                wd.Value = double.Parse(data.Substring(35, 2));
                wd.Source = getDataSource(data.Substring(37, 1));
                wr.SetData(WeatherRecord.RecordType.WeatherCode, wd);

                //露点温度[C]
                wd.Value = double.Parse(data.Substring(38, 4)) * 0.1;
                wd.Source = getDataSource(data.Substring(42, 1));
                wr.SetData(WeatherRecord.RecordType.DewPointTemperature, wd);

                //全天日射量[W/m2]
                double ghRad = double.Parse(data.Substring(47, 3)) * 277.7777778 * 0.01;
                wd.Value = ghRad;
                WeatherData.DataSource ghRadSource = getDataSource(data.Substring(50, 1));
                wd.Source = ghRadSource;
                wr.SetData(WeatherRecord.RecordType.GlobalHorizontalRadiation, wd);

                //降水量[mm]
                wd.Value = double.Parse(data.Substring(51, 4)) * 0.1;
                wd.Source = getDataSource(data.Substring(55, 1));
                wr.SetData(WeatherRecord.RecordType.PrecipitationLevel, wd);

                //推定可能なデータを計算して埋める********************************************************
                //絶対湿度[kg/kg(DA)]
                if (dbtSource != WeatherData.DataSource.MissingValue && rhdSource != WeatherData.DataSource.MissingValue)
                {
                    wd.Value = MoistAir.GetAirStateFromDBRH(dbt, rhd, MoistAir.Property.HumidityRatio, atm);
                    wd.Source = WeatherData.DataSource.CalculatedValue;
                    wr.SetData(WeatherRecord.RecordType.HumidityRatio, wd);
                }

                //直散分離
                //太陽の存在確認
                bool sr = (0 < Sun.GetSunAltitude(locationInfo.Latitude, locationInfo.Longitude, 135d, cTime));
                
                //直散分離
                double dsRad, dhRad;
                //日出・日没調整
                if (!sunRise && sr) Sun.EstimateDiffuseAndDirectNormalRadiation(ghRad, locationInfo.Latitude, locationInfo.Longitude, 135d, cTime, out dsRad, out dhRad);
                else if (sunRise && !sr) Sun.EstimateDiffuseAndDirectNormalRadiation(ghRad, locationInfo.Latitude, locationInfo.Longitude, 135d, cTime.AddHours(-1), out dsRad, out dhRad);
                else Sun.EstimateDiffuseAndDirectNormalRadiation(ghRad, locationInfo.Latitude, locationInfo.Longitude, 135d, cTime.AddHours(-0.5), out dsRad, out dhRad);
                sunRise = sr;

                //24h「観測しない」が続いた場合は欠測扱い
                hasDR = (ghRadSource != WeatherData.DataSource.MissingValue || hasDR);
                if (i != 23 || hasDR)
                {
                    //直達日射量[W/m2]
                    wd.Value = dsRad;
                    wd.Source = WeatherData.DataSource.PredictedValue;
                    wr.SetData(WeatherRecord.RecordType.DirectNormalRadiation, wd);
                    //天空日射量[W/m2]
                    wd.Value = dhRad;
                    wd.Source = WeatherData.DataSource.PredictedValue;
                    wr.SetData(WeatherRecord.RecordType.DiffuseHorizontalRadiation, wd);
                }

                //空白データを欠測データとして埋める******************************************************
                wr.FillMissingData();

                //1時間進める
                cTime = cTime.AddHours(1);

                //気象レコード追加
                pwData.AddWeatherRecord(wr);
            }
        }

        /// <summary>RMKに従ってデータ種別を返す</summary>
        /// <param name="rmk">RMK</param>
        /// <returns>データ種別</returns>
        private static WeatherData.DataSource getDataSource(string rmk)
        {
            int iRmk = int.Parse(rmk);

            if (iRmk <= 2) return WeatherData.DataSource.MissingValue;
            else if (iRmk <= 4) return WeatherData.DataSource.PredictedValue;
            else return WeatherData.DataSource.MeasuredValue;
        }

        /// <summary>風向番号をdegreeに変換する</summary>
        /// <param name="wIndex">風向番号</param>
        /// <returns>degree</returns>
        private static double getWindowDirection(int wIndex)
        {
            switch (wIndex) { 
                case 0:
                case 21:
                case 99:
                    return 0;
                case 1:
                    return -157.5;
                case 2:
                case 17:
                    return -135.0;
                case 3:
                    return -112.5;
                case 4:
                    return -90.0;
                case 5:
                    return -67.5;
                case 6:
                case 18:
                    return -45.0;
                case 7:
                    return -22.5;
                case 8:
                    return 0;
                case 9:
                    return 22.5;
                case 10:
                case 19:
                    return 45.0;
                case 11:
                    return 67.5;
                case 12:
                    return 90.0;
                case 13:
                    return 112.5;
                case 14:
                case 20:
                    return 135.0;
                case 15:
                    return 157.5;
                case 16:
                    return 180.0;
                default:
                    throw new Exception("風向エラー");
            }
        }

        /// <summary>ID下3桁をもとに地点情報を取得する</summary>
        /// <param name="id3">ID下3桁</param>
        /// <param name="lInfo">地点情報</param>
        /// <returns>地点情報を取得成功の真偽</returns>
        public static bool GetLocationInformation(int id3, out LocationInformation lInfo)
        {
            int id = id3 + 47000;   //←いかにも問題のある処理。直せ!
            return locationInfos.TryGetValue(id, out lInfo);
        }

        #endregion

    }
}
