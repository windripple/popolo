using System;
using System.Collections.Generic;
using System.Text;

namespace Popolo.FluidNetwork
{
    /// <summary>節点接続イベント情報格納クラス</summary>
    public class NodeConnectionEventArgs : EventArgs
    {

        #region インスタンス変数

        /// <summary>節点1</summary>
        private ImmutableNode node1;

        /// <summary>節点2</summary>
        private ImmutableNode node2;

        /// <summary>流路</summary>
        private ImmutableChannel channel;

        #endregion

        #region プロパティ

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

        /// <summary>流路を取得する</summary>
        public ImmutableChannel Connector
        {
            get
            {
                return channel;
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="node1">節点1</param>
        /// <param name="node2">節点2</param>
        /// <param name="channel">流路</param>
        public NodeConnectionEventArgs(ImmutableNode node1, ImmutableNode node2, ImmutableChannel channel)
        {
            this.node1 = node1;
            this.node2 = node2;
            this.channel = channel;
        }

        #endregion

    }

    /// <summary>節点関連のイベント情報格納クラス</summary>
    public class NodeEventArgs : EventArgs
    {

        #region インスタンス変数

        /// <summary>節点1</summary>
        private ImmutableNode node;

        #endregion

        #region プロパティ

        /// <summary>節点を取得する</summary>
        public ImmutableNode Node
        {
            get
            {
                return node;
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="node">節点</param>
        public NodeEventArgs(ImmutableNode node)
        {
            this.node = node;
        }

        #endregion

    }

}
