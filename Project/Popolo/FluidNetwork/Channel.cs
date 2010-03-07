/* Channel.cs
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
using System.Runtime.Serialization;

namespace Popolo.FluidNetwork
{
    /// <summary>流路クラス</summary>
    public class Channel : ImmutableChannel
    {

        #region 定数宣言

        /// <summary>シリアライズ用バージョン情報</summary>
        private double S_VERSION = 1.0;

        #endregion

        #region インスタンス変数

        /// <summary>節点1</summary>
        private Node node1;

        /// <summary>節点2</summary>
        private Node node2;

        /// <summary>変更フラグ</summary>
        private bool hasChanged = true;

        /// <summary>抵抗値</summary>
        private double resistance = 1d;

        /// <summary>抵抗指数</summary>
        private double eta = 1d;

        /// <summary>Node1からNode2への流量</summary>
        private double flow;

        #endregion

        #region プロパティ

        /// <summary>IDを設定・取得する</summary>
        public int ID { get; set; }

        /// <summary>名称を設定・取得する</summary>
        public string Name { get; set; }

        /// <summary>節点1を取得する</summary>
        public ImmutableNode Node1
        {
            get
            {
                return node1;
            }
        }

        /// <summary>節点2を取得する</summary>
        public ImmutableNode Node2
        {
            get
            {
                return node2;
            }
        }

        /// <summary>抵抗値を設定・取得する</summary>
        public double Resistance
        {
            get
            {
                return resistance;
            }
            set
            {
                if (0 < value)
                {
                    resistance = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>抵抗指数を設定・取得する</summary>
        public double Eta
        {
            get
            {
                return eta;
            }
            set
            {
                eta = Math.Max(1, Math.Min(2, value));
                hasChanged = true;
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        public Channel() { }

        /// <summary>コンストラクタ</summary>
        /// <param name="name">名称</param>
        public Channel(string name)
        {
            this.Name = name;
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="name">名称</param>
        /// <param name="resistance">抵抗値</param>
        /// <param name="eta">抵抗指数</param>
        public Channel(string name, double resistance, double eta)
        {
            this.Name = name;
            this.Resistance = resistance;
            this.Eta = eta;
        }

        #endregion

        #region 接続・切断処理

        /// <summary>接点を接続する</summary>
        /// <param name="node1">接点1</param>
        /// <param name="node2">接点2</param>
        public void Connect(Node node1, Node node2)
        {
            //接続を解除
            if (this.node1 != null) Disconnect();

            this.node1 = node1;
            this.node2 = node2;

            //接点エネルギー変更イベントを登録
            node1.PotentialChangeEvent += nodeFnc;
            node2.PotentialChangeEvent += nodeFnc;

            node1.AddChannel(this);
            node2.AddChannel(this);
        }

        /// <summary>切断する</summary>
        public void Disconnect()
        {
            node1.PotentialChangeEvent -= nodeFnc;
            node2.PotentialChangeEvent -= nodeFnc;

            node1.RemoveChannel(this);
            node2.RemoveChannel(this);

            node1 = null;
            node2 = null;
        }

        /// <summary>接点エネルギー変更イベントに対する処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nodeFnc(object sender, EventArgs e)
        {
            hasChanged = true;
        }

        #endregion

        #region 流量計算処理

        /// <summary>Node1からNode2への流量を取得する</summary>
        /// <returns>Node1からNode2への流量</returns>
        public double GetFlow()
        {
            if (hasChanged)
            {
                flow = calculateFlow();
                hasChanged = false;
            }
            return flow;
        }

        /// <summary>接点への流量を取得する</summary>
        /// <param name="node">接点</param>
        /// <returns>接点への流量</returns>
        public double GetFlow(ImmutableNode node)
        {
            if (node == node1) return -GetFlow();
            if (node == node2) return GetFlow();
            else return 0;
        }

        /// <summary>Node1からNode2への流量を計算する</summary>
        /// <returns>流量</returns>
        protected virtual double calculateFlow()
        {
            double deltaP = this.Node1.Potential - this.Node2.Potential;
            double flowDir = Math.Sign(deltaP);
            return flowDir * Math.Pow(1d / this.Resistance * Math.Abs(deltaP), 1d / this.Eta);
        }

        #endregion

        #region ISerializable実装

        /// <summary>デシリアライズ用コンストラクタ</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected Channel(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            double version = sInfo.GetDouble("S_Version");

            //ID
            ID = sInfo.GetInt32("id");
            //名称
            Name = sInfo.GetString("name");
            //接点1
            node1 = (Node)sInfo.GetValue("node1", typeof(Node));
            //接点2
            node2 = (Node)sInfo.GetValue("node2", typeof(Node));
            //抵抗値
            resistance = sInfo.GetDouble("resistance");
            //抵抗指数
            eta = sInfo.GetDouble("eta");
        }

        /// <summary>シリアル化処理</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //バージョン情報
            info.AddValue("S_Version", S_VERSION);

            //ID
            info.AddValue("id", ID);
            //名称
            info.AddValue("name", Name);
            //接点1
            info.AddValue("node1", node1);
            //接点2
            info.AddValue("node2", node2);
            //抵抗値
            info.AddValue("resistance", resistance);
            //抵抗指数
            info.AddValue("eta", eta);
        }

        #endregion

        #region IClineable実装

        /// <summary>複製を返す</summary>
        /// <returns>複製</returns>
        public object Clone()
        {
            Channel clonedChannel = (Channel)this.MemberwiseClone();
            clonedChannel.node1 = null;
            clonedChannel.node2 = null;
            return clonedChannel;
        }

        #endregion

    }

    #region 読み取り専用流路

    /// <summary>読み取り専用流路</summary>
    public interface ImmutableChannel: ISerializable, ICloneable
    {

        /// <summary>IDを取得する</summary>
        int ID { get; }

        /// <summary>節点1を取得する</summary>
        ImmutableNode Node1 { get; }

        /// <summary>節点2を取得する</summary>
        ImmutableNode Node2 { get; }

        /// <summary>抵抗値を取得する</summary>
        double Resistance { get; }

        /// <summary>抵抗指数を取得する</summary>
        double Eta { get; }

        /// <summary>Node1からNode2への流量を取得する</summary>
        /// <returns>Node1からNode2への流量</returns>
        double GetFlow();

        /// <summary>接点への流量を取得する</summary>
        /// <param name="node">接点</param>
        /// <returns>接点への流量</returns>
        double GetFlow(ImmutableNode node);

    }

    #endregion

}
