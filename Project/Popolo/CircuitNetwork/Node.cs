/* Node.cs
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
using System.Runtime.Serialization;

namespace Popolo.CircuitNetwork
{
    /// <summary>節点クラス</summary>
    [Serializable]
    public class Node : ImmutableNode
    {

        #region 定数宣言

        /// <summary>シリアライズ用バージョン情報</summary>
        private double S_VERSION = 1.0;

        #endregion

        #region delegate定義

        /// <summary>エネルギー変更イベントハンドラ</summary>
        public delegate void PotentialChangeEventHandler(object sender, EventArgs e); 

        #endregion

        #region Event定義

        /// <summary>エネルギー変更イベント</summary>
        public event PotentialChangeEventHandler PotentialChangeEvent;

        #endregion

        #region インスタンス変数

        /// <summary>容量</summary>
        private double capacity;

        /// <summary>エネルギー</summary>
        private double potential;

        /// <summary>節点に接続されている流路リスト</summary>
        private List<ImmutableChannel> channels = new List<ImmutableChannel>();

        #endregion

        #region プロパティ

        /// <summary>IDを設定・取得する</summary>
        public int ID { get; set; }

        /// <summary>名称を設定・取得する</summary>
        public string Name { get; set; }

        /// <summary>外部の系への流出流量を設定・取得する</summary>
        public double ExternalFlow { get; set; }

        /// <summary>境界条件節点か否かの情報を設定・取得する</summary>
        public bool IsBoundaryNode { get; set; }

        /// <summary>エネルギーを取得する</summary>
        public double Potential
        {
            get
            {
                return potential;
            }
        }

        /// <summary>接続されている流路の数を取得する</summary>
        public int ChannelNumber
        {
            get
            {
                return channels.Count;
            }
        }

        /// <summary>容量を設定・取得する</summary>
        public virtual double Capacity
        {
            get
            {
                return capacity;
            }
            set
            {
                capacity = Math.Max(0, value);
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        public Node() { }

        /// <summary>コンストラクタ</summary>
        /// <param name="name">名称</param>
        public Node(string name)
        {
            this.Name = name;
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="name">名称</param>
        /// <param name="capacity">容量</param>
        public Node(string name, double capacity)
        {
            this.Name = name;
            this.Capacity = capacity;
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="name">名称</param>
        /// <param name="capacity">容量</param>
        /// <param name="initialPotential">エネルギー初期値</param>
        public Node(string name, double capacity, double initialPotential)
        {
            this.Name = name;
            this.Capacity = capacity;
            this.potential = initialPotential;
        }

        #endregion

        #region 流路設定・取得処理

        /// <summary>流路を追加する</summary>
        /// <param name="channel">流路</param>
        internal void AddChannel(ImmutableChannel channel)
        {
            channels.Add(channel);
        }

        /// <summary>流路を削除する</summary>
        /// <param name="channel">流路</param>
        internal void RemoveChannel(ImmutableChannel channel)
        {
            channels.Remove(channel);
        }

        /// <summary>指定した流路を含むか否かを返す</summary>
        /// <param name="channel">流路</param>
        /// <returns>指定した流路を含む場合は真</returns>
        public bool ContainsChannel(ImmutableChannel channel)
        {
            return channels.Contains(channel);
        }

        /// <summary>接続されている流路リストを取得する</summary>
        /// <returns>接続されている流路リスト</returns>
        public ImmutableChannel[] GetChannels()
        {
            return channels.ToArray();
        }

        #endregion

        #region publicメソッド

        /// <summary>エネルギーを設定する</summary>
        /// <param name="potential">エネルギー</param>
        public void SetPotential(double potential)
        {
            this.potential = potential;

            //イベント通知
            if (PotentialChangeEvent != null) PotentialChangeEvent(this, EventArgs.Empty);
        }

        /// <summary>接点に流れ込む流量合算値を計算する</summary>
        /// <returns>接点に流れ込む流量合算値</returns>
        public double GetTotalFlow()
        {
            double sum = 0;
            foreach (Channel channel in channels)
            {
                sum += channel.GetFlow(this);
            }
            return sum - ExternalFlow;
        }

        #endregion

        #region ISerializable実装

        /// <summary>デシリアライズ用コンストラクタ</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected Node(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            double version = sInfo.GetDouble("S_Version");

            //ID
            ID = sInfo.GetInt32("id");
            //名称
            Name = sInfo.GetString("name");
            //エネルギー
            potential = sInfo.GetDouble("potential");
            //容量
            capacity = sInfo.GetDouble("capacity");
            //境界ノードか否か
            IsBoundaryNode = sInfo.GetBoolean("isBoundaryNode");
            //外部の系への流出流量
            ExternalFlow = sInfo.GetDouble("externalFlow");
            //節点に接続されている流路リスト
            int channelsNumber = sInfo.GetInt32("channelsNumber");
            for (int i = 0; i < channelsNumber; i++) channels.Add((Channel)sInfo.GetValue("channels" + i.ToString(), typeof(Channel)));
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
            //エネルギー
            info.AddValue("potential", potential);
            //容量
            info.AddValue("capacity", capacity);
            //境界ノードか否か
            info.AddValue("isBoundaryNode", IsBoundaryNode);
            //外部の系への流出流量
            info.AddValue("externalFlow", ExternalFlow);
            //節点に接続されている流路リスト
            info.AddValue("channelsNumber", channels.Count);
            for (int i = 0; i < channels.Count; i++) info.AddValue("channels" + i.ToString(), channels[i]);
        }

        #endregion

        #region ICloneable実装

        /// <summary>複製を返す</summary>
        /// <returns>複製</returns>
        public object Clone()
        {
            Node clonedNode = (Node)this.MemberwiseClone();
            clonedNode.channels = new List<ImmutableChannel>();
            return clonedNode;
        }

        #endregion

    }

    #region 読み取り専用接点

    /// <summary>読み取り専用接点</summary>
    public interface ImmutableNode : ISerializable, ICloneable
    {

        /// <summary>IDを取得する</summary>
        int ID { get; }

        /// <summary>名称を取得する</summary>
        string Name { get; }

        /// <summary>エネルギーを取得する</summary>
        double Potential { get; }

        /// <summary>容量を取得する</summary>
        double Capacity { get; }

        /// <summary>接続されている流路の数を取得する</summary>
        int ChannelNumber { get; }

        /// <summary>外部の系への流出流量を取得する</summary>
        double ExternalFlow { get; }

        /// <summary>境界条件節点か否かの情報を取得する</summary>
        bool IsBoundaryNode { get; }

        /// <summary>指定した流路を含むか否かを返す</summary>
        /// <param name="channel">流路</param>
        /// <returns>指定した流路を含む場合は真</returns>
        bool ContainsChannel(ImmutableChannel channel);

        /// <summary>接点に流れ込む流量合算値を計算する</summary>
        /// <returns>接点に流れ込む流量合算値</returns>
        double GetTotalFlow();

    }

    #endregion

}
