/* Circuit.cs
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
using System.Text;
using System.Runtime.Serialization;

namespace Popolo.CircuitNetwork
{
    /// <summary>回路網クラス</summary>
    public class Circuit : ImmutableCircuit
    {

        #region Constants

        /// <summary>シリアライズ用バージョン情報</summary>
        private double S_VERSION = 1.0;

        #endregion

        #region delegate定義

        /// <summary>節点接続イベントハンドラ</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void NodeConnectEventHandler(object sender, NodeConnectionEventArgs e);

        /// <summary>節点切断イベントハンドラ</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void NodeDisConnectEventHandler(object sender, NodeConnectionEventArgs e);

        /// <summary>節点追加イベントハンドラ</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void NodeAddEventHandler(object sender, NodeEventArgs e);

        /// <summary>節点削除イベントハンドラ</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void NodeRemoveEventHandler(object sender, NodeEventArgs e); 

        #endregion

        #region event定義

        /// <summary>節点接続イベント</summary>
        public event NodeConnectEventHandler NodeConnectEvent;

        /// <summary>節点切断イベント</summary>
        public event NodeDisConnectEventHandler NodeDisConnectEvent;

        /// <summary>節点追加イベント</summary>
        public event NodeAddEventHandler NodeAddEvent;

        /// <summary>節点削除イベント</summary>
        public event NodeRemoveEventHandler NodeRemoveEvent;

        #endregion

        #region Instance variables

        /// <summary>流路リスト</summary>
        private List<Channel> channels = new List<Channel>();

        /// <summary>節点リスト</summary>
        private List<Node> nodes = new List<Node>();

        #endregion

        #region Properties

        /// <summary>IDを設定・取得する</summary>
        public int ID { get; set; }

        /// <summary>名称を設定・取得する</summary>
        public string Name { get; set; }

        /// <summary>流路の数を取得する</summary>
        public int ChannelsNumber
        {
            get
            {
                return channels.Count;
            }
        }

        /// <summary>節点の数を取得する</summary>
        public int NodesNumber
        {
            get
            {
                return nodes.Count;
            }
        }

        #endregion

        #region Constructor

        /// <summary>Constructor</summary>
        public Circuit() { }

        /// <summary>Constructor</summary>
        /// <param name="name">名称</param>
        public Circuit(string name)
        {
            this.Name = name;
        }

        #endregion

        #region 流路に関する処理

        /// <summary>流路を取得する</summary>
        /// <param name="channelIndex">流路番号</param>
        /// <returns>流路</returns>
        public ImmutableChannel GetChannel(int channelIndex)
        {
            return channels[channelIndex];
        }

        /// <summary>流路リストを取得する</summary>
        /// <returns>流路リスト</returns>
        public ImmutableChannel[] GetChannels()
        {
            return channels.ToArray();
        }

        /// <summary>流路を返す</summary>
        /// <param name="channel">読み取り専用流路</param>
        /// <returns>流路</returns>
        private Channel getChannel(ImmutableChannel channel)
        {
            foreach (Channel cn in channels)
            {
                if (cn.Equals(channel)) return cn;
            }
            return null;
        }

        #endregion

        #region 節点に関する処理

        /// <summary>節点を取得する</summary>
        /// <param name="nodeIndex">節点番号</param>
        /// <returns>節点</returns>
        public ImmutableNode GetNode(int nodeIndex)
        {
            return nodes[nodeIndex];
        }

        /// <summary>節点リストを取得する</summary>
        /// <returns>節点リスト</returns>
        public ImmutableNode[] GetNodes()
        {
            return nodes.ToArray();
        }

        /// <summary>節点を追加する</summary>
        /// <param name="node">追加する節点</param>
        /// <returns>追加した節点（Clone処理を行うため<paramref name="node"/>とは異なる）</returns>
        public ImmutableNode AddNode(Node node)
        {
            //複製してIDを付与
            Node newNode = (Node)node.Clone();
            newNode.ID = nodes.Count;

            //追加
            this.nodes.Add(newNode);

            //イベント通知
            if (NodeAddEvent != null) NodeAddEvent(this, new NodeEventArgs(newNode));

            return newNode;
        }

        /// <summary>節点を削除する</summary>
        /// <param name="node">節点</param>
        public void RemoveNode(ImmutableNode node)
        {
            Node nd = getNode(node);

            //節点に接続されている流路を削除
            ImmutableChannel[] cnls = nd.GetChannels();
            foreach (ImmutableChannel cnl in cnls) DisconnectNodes(cnl);

            //リストから削除
            nodes.Remove(nd);

            //IDを詰める
            foreach (Node nod in nodes)
            {
                if (nod.ID == (nodes.Count - 1)) nod.ID = nd.ID;
            }

            //イベント通知
            if (NodeRemoveEvent != null) NodeRemoveEvent(this, new NodeEventArgs(nd));
        }

        /// <summary>節点を返す</summary>
        /// <param name="node">読み取り専用節点</param>
        /// <returns>節点</returns>
        private Node getNode(ImmutableNode node)
        {
            foreach (Node nd in nodes)
            {
                if (nd.Equals(node)) return nd;
            }
            return null;
        }

        /// <summary>節点にエネルギーを設定する</summary>
        /// <param name="potential">エネルギー</param>
        /// <param name="node">節点</param>
        public void SetPotential(double potential, ImmutableNode node)
        {
            int index = getNodeIndex(node);
            if (index != -1) SetPotential(potential, index);
        }

        /// <summary>節点にエネルギーを設定する</summary>
        /// <param name="potential">エネルギー</param>
        /// <param name="nodeIndex">節点番号</param>
        internal void SetPotential(double potential, int nodeIndex)
        {
            nodes[nodeIndex].SetPotential(potential);
        }

        /// <summary>外部の系への流出流量を節点に設定する</summary>
        /// <param name="externalFlow">外部の系への流出流量</param>
        /// <param name="node">節点</param>
        public void SetExternalFlow(double externalFlow, ImmutableNode node)
        {
            int index = getNodeIndex(node);
            if (index != -1) SetExternalFlow(externalFlow, index);
        }

        /// <summary>外部の系への流出流量を節点に設定する</summary>
        /// <param name="externalFlow">外部の系への流出流量</param>
        /// <param name="nodeIndex">節点番号</param>
        internal void SetExternalFlow(double externalFlow, int nodeIndex)
        {
            nodes[nodeIndex].ExternalFlow = externalFlow;
        }

        /// <summary>境界条件ノードか否かを設定する</summary>
        /// <param name="isBoundaryNode">境界条件ノードか否か</param>
        /// <param name="node">節点</param>
        public void SetBoundaryNode(bool isBoundaryNode, ImmutableNode node)
        {
            int index = getNodeIndex(node);
            if (index != -1) SetBoundaryNode(isBoundaryNode, index);
        }

        /// <summary>境界条件ノードか否かを設定する</summary>
        /// <param name="isBoundaryNode">境界条件ノードか否か</param>
        /// <param name="nodeIndex">節点番号</param>
        internal void SetBoundaryNode(bool isBoundaryNode, int nodeIndex)
        {
            nodes[nodeIndex].IsBoundaryNode = isBoundaryNode;
        }

        /// <summary>節点の番号を返す</summary>
        /// <param name="node">節点オブジェクト</param>
        /// <returns>節点の番号</returns>
        private int getNodeIndex(ImmutableNode node)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Equals(node)) return i;
            }
            return -1;
        }

        #endregion

        #region 接続・切断処理

        /// <summary>節点を接続する</summary>
        /// <param name="node1">節点1</param>
        /// <param name="node2">節点2</param>
        /// <param name="channel">接続に使用する流路</param>
        /// <returns>接続した流路（Clone処理を行うため<paramref name="channel"/>とは異なる）</returns>
        public ImmutableChannel ConnectNodes(ImmutableNode node1, ImmutableNode node2, Channel channel)
        {
            //節点存在確認
            Node nd1 = getNode(node1);
            Node nd2 = getNode(node2);
            if (nd1 == null || nd2 == null) return null;

            //流路を複製してIDを付与
            Channel newChannel = (Channel)channel.Clone();
            newChannel.ID = channels.Count;

            //接続処理
            newChannel.Connect(nd1, nd2);
            //リストに追加
            this.channels.Add(newChannel);

            //イベント通知
            if (NodeConnectEvent != null) NodeConnectEvent(this, new NodeConnectionEventArgs(nd1, nd2, newChannel));

            return newChannel;
        }

        /// <summary>節点を切断する</summary>
        /// <param name="channel">接点を接続している流路</param>
        /// <returns>切断成功の真偽</returns>
        public bool DisconnectNodes(ImmutableChannel channel)
        {
            Channel cn = getChannel(channel);
            if (cn == null) return false;

            ImmutableNode nd1 = cn.Node1;
            ImmutableNode nd2 = cn.Node2;

            //切断処理
            cn.Disconnect();
            channels.Remove(cn);
            //IDを詰める
            foreach (Channel cnl in channels)
            {
                if (cnl.ID == (channels.Count - 1)) cnl.ID = cn.ID;
            }

            //イベント通知
            if (NodeDisConnectEvent != null) NodeDisConnectEvent(this, new NodeConnectionEventArgs(nd1, nd2, cn));

            return true;
        }

        #endregion

        #region ISerializable実装

        /// <summary>デシリアライズ用Constructor</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected Circuit(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            double version = sInfo.GetDouble("S_Version");

            //ID
            ID = sInfo.GetInt32("id");
            //名称
            Name = sInfo.GetString("name");
            //節点リスト
            int nodesNumber = sInfo.GetInt32("nodesNumber");
            for (int i = 0; i < nodesNumber; i++) nodes.Add((Node)sInfo.GetValue("nodes" + i.ToString(), typeof(Node)));
            //流路リスト
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
            //節点リスト
            info.AddValue("nodesNumber", nodes.Count);
            for (int i = 0; i < nodes.Count; i++) info.AddValue("nodes" + i.ToString(), nodes[i]);
            //流路リスト
            info.AddValue("channelsNumber", channels.Count);
            for (int i = 0; i < channels.Count; i++) info.AddValue("channels" + i.ToString(), channels[i]);
        }

        #endregion

    }

    #region 読み取り専用回路網

    /// <summary>読み取り専用回路網</summary>
    public interface ImmutableCircuit : ISerializable
    {

        /// <summary>IDを取得する</summary>
        int ID { get; }

        /// <summary>流路の数を取得する</summary>
        int ChannelsNumber { get; }

        /// <summary>節点の数を取得する</summary>
        int NodesNumber { get; }

        /// <summary>流路を取得する</summary>
        /// <param name="channelIndex">流路番号</param>
        /// <returns>流路</returns>
        ImmutableChannel GetChannel(int channelIndex);

        /// <summary>流路リストを取得する</summary>
        /// <returns>流路リスト</returns>
        ImmutableChannel[] GetChannels();

        /// <summary>節点を取得する</summary>
        /// <param name="nodeIndex">節点番号</param>
        /// <returns>節点</returns>
        ImmutableNode GetNode(int nodeIndex);

        /// <summary>節点リストを取得する</summary>
        /// <returns>節点リスト</returns>
        ImmutableNode[] GetNodes();

    }

    #endregion

}
