/* SchedulerEditor.cs
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Popolo.Utility.Schedule.Control
{
    /// <summary>スケジューラエディタ</summary>
    [Serializable]
    public partial class SchedulerEditor<TYPE> : UserControl
        where TYPE : ICloneable
    {

        #region<インスタンス変数>

        /// <summary>デフォルト値</summary>
        private TYPE defaultValue = default(TYPE);

        /// <summary>設定可能な期間定義オブジェクトリスト</summary>
        private ITermStructure[] iTermStructures;

        /// <summary>編集するスケジューラ</summary>
        private Scheduler<TYPE> scheduler;

        #endregion//インスタンス変数

        #region<delegate定義>

        /// <summary>スケジューライベントハンドラ</summary>
        public delegate void SchedulerEditorEventHandler(object sender, EventArgs e);

        #endregion//delegate定義

        #region<イベント定義>

        /// <summary>スケジューラ追加ボタンリスト表示イベント</summary>
        public event SchedulerEditorEventHandler SchedulerAddButtonOpeningEvent;

        /// <summary>スケジューラ追加イベント</summary>
        public event SchedulerEditorEventHandler SchedulerSetEvent;

        /// <summary>スケジューラ選択イベント</summary>
        public event TreeViewEventHandler TreeViewAfterSelectEvent;

        #endregion//イベント定義

        #region<プロパティ>

        /// <summary>デフォルト値を設定・取得する</summary>
        public TYPE DefaultValue
        {
            get
            {
                return defaultValue;
            }
            set
            {
                defaultValue = value;
            }
        }

        /// <summary>選択中のノードの階層を取得する</summary>
        public int SelectedNodeLevel
        {
            get
            {
                TreeNode tNode = scheduleTree.SelectedNode;
                if (tNode == null) return -1;
                else return tNode.Level;
            }
        }

        #endregion//プロパティ

        #region<コンストラクタ>

        /// <summary>コンストラクタ</summary>
        public SchedulerEditor()
        {
            InitializeComponent();
        }

        #endregion//コンストラクタ

        #region<publicメソッド>

        /// <summary>設定可能な期間定義オブジェクトリストを設定する</summary>
        /// <param name="iTermStructures">設定可能な期間定義オブジェクトリスト</param>
        public void SetITermStructures(ITermStructure[] iTermStructures)
        {
            this.iTermStructures = iTermStructures;
            //登録されているアイテムをクリア
            tsbtnAdd.DropDown.Items.Clear();
            foreach (ITermStructure it in iTermStructures)
            {
                tsbtnAdd.DropDown.Items.Add(it.Name);
            }
        }

        /// <summary>スケジューラを設定する</summary>
        /// <param name="iTermStructure">スケジューラで管理する期間定義オブジェクト</param>
        public void SetScheduler(ITermStructure iTermStructure)
        {
            this.scheduler = new Scheduler<TYPE>(iTermStructure);
            SetScheduler(scheduler);
        }

        /// <summary>スケジューラを設定する</summary>
        /// <param name="scheduler">スケジューラ</param>
        public void SetScheduler(Scheduler<TYPE> scheduler)
        {
            //デフォルト値を設定
            scheduler.DefaultValue = this.defaultValue;

            //編集中のスケジューラがある場合はイベント登録を解除
            if (this.scheduler != null)
            {
                this.scheduler.SchedulerInitializeEvent -= new Scheduler<TYPE>.SchedulerInitializeEventHandler(scheduler_SchedulerInitializeEvent);
                this.scheduler.SchedulerNameChangeEvent -= new Scheduler<TYPE>.SchedulerNameChangeEventHandler(scheduler_SchedulerNameChangeEvent);
                this.scheduler.SchedulerSetEvent -= new Scheduler<TYPE>.SchedulerSetEventHandler(scheduler_SchedulerSetEvent);
                this.scheduler.SchedulerRemoveEvent -= new Scheduler<TYPE>.SchedulerRemoveEventHandler(scheduler_SchedulerRemoveEvent);
            }

            //編集中のスケジューラを変更
            this.scheduler = scheduler;

            //Schedulerのイベント登録
            scheduler.SchedulerInitializeEvent += new Scheduler<TYPE>.SchedulerInitializeEventHandler(scheduler_SchedulerInitializeEvent);
            scheduler.SchedulerNameChangeEvent += new Scheduler<TYPE>.SchedulerNameChangeEventHandler(scheduler_SchedulerNameChangeEvent);
            scheduler.SchedulerSetEvent += new Scheduler<TYPE>.SchedulerSetEventHandler(scheduler_SchedulerSetEvent);
            scheduler.SchedulerRemoveEvent += new Scheduler<TYPE>.SchedulerRemoveEventHandler(scheduler_SchedulerRemoveEvent);

            //コントロール破棄時にイベント登録を解除
            this.Disposed += delegate(object sender, EventArgs e)
            {
                scheduler.SchedulerInitializeEvent -= new Scheduler<TYPE>.SchedulerInitializeEventHandler(scheduler_SchedulerInitializeEvent);
                scheduler.SchedulerNameChangeEvent -= new Scheduler<TYPE>.SchedulerNameChangeEventHandler(scheduler_SchedulerNameChangeEvent);
                scheduler.SchedulerSetEvent -= new Scheduler<TYPE>.SchedulerSetEventHandler(scheduler_SchedulerSetEvent);
                scheduler.SchedulerRemoveEvent -= new Scheduler<TYPE>.SchedulerRemoveEventHandler(scheduler_SchedulerRemoveEvent);
            };

            //TreeView初期化
            scheduleTree.Nodes.Clear();
            //先頭ノードを作成
            PhasedTreeNode tNode;
            tNode = new PhasedTreeNode(scheduler.Name);
            scheduleTree.Nodes.Add(tNode);
            //子ノードを追加する
            string[] terms = scheduler.GetTermNames();
            foreach (string term in terms)
            {
                PhasedTreeNode tn = new PhasedTreeNode(term);
                tNode.Nodes.Add(tn);
                //ノードとスケジューラを対応付け
                tn.Tag = scheduler;
            }
            tNode.SubFoldersAdded = true;
        }

        /// <summary>編集中のスケジューラを取得する</summary>
        /// <returns>編集中のスケジューラ</returns>
        public Scheduler<TYPE> GetScheduler()
        {
            return scheduler;
        }

        /// <summary>選択中のスケジューラと期間にスケジュールを設定する</summary>
        /// <param name="schedule">スケジュール内容</param>
        /// <returns>設定成功の真偽</returns>
        public bool SetSchedule(TYPE schedule)
        {
            //選択中のツリーノードを取得
            TreeNode tNode = scheduleTree.SelectedNode;

            //最上層または非選択の場合はfalseを返す
            if (tNode == null) return false;
            if (tNode.Level == 0) return false;

            //スケジューラを取得
            Scheduler<TYPE> scheduler = (Scheduler<TYPE>)tNode.Tag;
            return scheduler.SetSchedule(tNode.Text, (TYPE)schedule.Clone());
        }

        /// <summary>選択中のスケジューラと期間のスケジュールを取得する</summary>
        /// <param name="schedule">スケジュール内容</param>
        /// <returns>取得成功の真偽</returns>
        public bool GetSchedule(out TYPE schedule)
        {
            schedule = defaultValue;

            //選択中のツリーノードを取得
            TreeNode tNode = scheduleTree.SelectedNode;

            //最上層または非選択の場合はfalseを返す
            if (tNode == null) return false;
            if (tNode.Level == 0) return false;
            
            //スケジューラを取得
            Scheduler<TYPE> scheduler = (Scheduler<TYPE>)tNode.Tag;
            TYPE sc;
            bool result = scheduler.GetSchedule(tNode.Text, out sc);
            if (sc != null) schedule = (TYPE)sc.Clone();
            return result;
        }

        /// <summary>スケジューラノードが選択されているか否かを取得する</summary>
        /// <returns>スケジューラノードが選択されている場合は真</returns>
        public bool SchedulerNodeSelected()
        {
            //選択中のツリーノードを取得
            TreeNode tNode = scheduleTree.SelectedNode;

            //最上層または非選択の場合はfalseを返す
            if (tNode == null) return false;
            if (tNode.Level == 0) return false;

            return true;
        }

        #endregion//publicメソッド

        #region<privateメソッド>

        /// <summary>TreeNodeCollectionを検索してSchedulerと期間名称に対応するTreeNodeを特定する</summary>
        /// <param name="treeNodes">検索するTreeNodeCollection</param>
        /// <param name="scheduler">TreeNodeのタグに関連付けられたSchedulerオブジェクト</param>
        /// <param name="termName">ツリーノードに表示されているテキストの名称</param>
        /// <returns>Schedulerと期間名称に対応するTreeNode</returns>
        private TreeNode getTreeNodeFromSchedulerAndTermName(TreeNodeCollection treeNodes, Scheduler<TYPE> scheduler, string termName)
        {
            foreach (TreeNode tn in treeNodes)
            {
                if ((Scheduler<TYPE>)tn.Tag == scheduler && termName == tn.Text) return tn;
                else
                {
                    TreeNode tNode = getTreeNodeFromSchedulerAndTermName(tn.Nodes, scheduler, termName);
                    if (tNode != null) return tNode;
                }
            }
            return null;
        }

        /// <summary>TreeNodeCollectionを検索してSchedulerに対応するTreeNodeを特定する</summary>
        /// <param name="treeNodes">検索するTreeNodeCollection</param>
        /// <param name="scheduler">TreeNodeのタグに関連付けられたSchedulerオブジェクト</param>
        /// <returns>Schedulerに対応するTreeNode</returns>
        private TreeNode getTreeNodeFromScheduler(TreeNodeCollection treeNodes, Scheduler<TYPE> scheduler)
        {
            foreach (TreeNode tn in treeNodes)
            {
                if ((Scheduler<TYPE>)tn.Tag == scheduler) return tn;
                else
                {
                    TreeNode tNode = getTreeNodeFromScheduler(tn.Nodes, scheduler);
                    if (tNode != null) return tNode;
                }
            }
            return null;
        }

        #endregion//privateメソッド

        #region<イベント発生時の処理>

        /// <summary>選択ノード変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scheduleTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            updateTSBtn();

            if (TreeViewAfterSelectEvent != null) TreeViewAfterSelectEvent(this, e);
        }

        /// <summary>ToolStripButtonの状態を更新する</summary>
        private void updateTSBtn()
        {
            //選択中のノードを選択
            TreeNode tNode = scheduleTree.SelectedNode;
            if (tNode == null) return;

            //下層にスケジューラを持つか否かでスケジューラ追加削除ボタンの操作可能性を変更
            bool hasChild = 0 < tNode.Nodes.Count;
            //削除されるべきスケジューラが設定されていれば操作可能
            //tsbtnRemove.Enabled = hasChild && tNode.Level != 0;
            tsbtnRemove.Enabled = hasChild;
        }

        /// <summary>ノードを開く前の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scheduleTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            //各子ノードに対して孫ノードの有無を調べて初期化
            foreach (PhasedTreeNode childTNode in e.Node.Nodes)
            {
                //孫ノードの初期化が行われていない場合は実行
                if (!childTNode.SubFoldersAdded)
                {
                    //子ノードにスケジューラが設定されているか否かを確認
                    Scheduler<TYPE> scheduler = (Scheduler<TYPE>)childTNode.Tag;
                    Scheduler<TYPE> childSC;
                    if (scheduler.GetScheduler(childTNode.Text, out childSC))
                    {
                        //スケジューラが設定されている場合にはスケジューラの期間名称リストを孫ノードとして追加
                        string[] termNames = childSC.GetTermNames();
                        //期間名称リストに基づいてノード作成
                        foreach (string tName in termNames)
                        {
                            PhasedTreeNode tn = new PhasedTreeNode(tName);
                            childTNode.Nodes.Add(tn);
                            tn.Tag = childSC;
                        }

                        //Schedulerのイベント登録
                        childSC.SchedulerSetEvent += new Scheduler<TYPE>.SchedulerSetEventHandler(scheduler_SchedulerSetEvent);
                        childSC.SchedulerRemoveEvent += new Scheduler<TYPE>.SchedulerRemoveEventHandler(scheduler_SchedulerRemoveEvent);
                        //コントロール破棄時にイベント登録を解除
                        this.Disposed += delegate(object sender2, EventArgs e2)
                        {
                            childSC.SchedulerSetEvent -= new Scheduler<TYPE>.SchedulerSetEventHandler(scheduler_SchedulerSetEvent);
                            childSC.SchedulerRemoveEvent -= new Scheduler<TYPE>.SchedulerRemoveEventHandler(scheduler_SchedulerRemoveEvent);
                        };
                    }
                    //フォルダ開閉フラグを更新
                    childTNode.SubFoldersAdded = true;
                }
            }
        }

        /// <summary>スケジューラ追加ボタンクリック時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbtnAdd_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //スケジューラに設定する期間構造を取得して設定する
            foreach (ITermStructure it in iTermStructures)
            {
                if (it.Name == e.ClickedItem.Text)
                {
                    //選択されているTreeNodeを取得する
                    TreeNode tNode = scheduleTree.SelectedNode;
                    //選択されていなければ終了
                    if (tNode == null) return;
                    //最上層の場合は初期化
                    if (tNode.Level == 0)
                    {
                        this.scheduler.Initialize(it);
                    }
                    else
                    {
                        Scheduler<TYPE> scheduler = (Scheduler<TYPE>)tNode.Tag;
                        Scheduler<TYPE> newScheduler = new Scheduler<TYPE>(it);
                        newScheduler.DefaultValue = scheduler.DefaultValue;
                        scheduler.SetScheduler(tNode.Text, newScheduler);
                        //イベント通知
                        if (SchedulerSetEvent != null) SchedulerSetEvent(this, new EventArgs());
                    }
                    break;
                }
            }
        }

        /// <summary>スケジューラ削除ボタンクリック時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbtnRemove_Click(object sender, EventArgs e)
        {
            //選択されているTreeNodeとスケジューラを取得する
            TreeNode tNode = scheduleTree.SelectedNode;
            //選択されていなければ終了
            if (tNode == null) return;

            //トップレベルの場合は編集スケジューラを削除
            if (scheduleTree.SelectedNode.Level == 0)
            {
                scheduler.Initialize();
                return;
            }
           
            Scheduler<TYPE> sch = (Scheduler<TYPE>)tNode.Tag;
            //スケジューラ削除処理
            sch.RemoveScheduler(tNode.Text);
        }

        /// <summary>スケジューラ追加ボタンドロップダウンリスト表示前の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbtnAdd_DropDownOpening(object sender, EventArgs e)
        {
            if (SchedulerAddButtonOpeningEvent != null) SchedulerAddButtonOpeningEvent(this, new EventArgs());
        }

        /// <summary>プロパティボタンクリックイベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbtnProperty_Click(object sender, EventArgs e)
        {
            TreeNode tNode = scheduleTree.SelectedNode;
            //選択されていなければ終了
            if (tNode == null) return;
            //最上層のノードの場合は終了
            if (tNode.Level == 0) return;
            //タグに関連付けられたスケジューラを取得
            Scheduler<TYPE> scheduler = (Scheduler<TYPE>)tNode.Tag;
            ImmutableITermStructure terms = scheduler.Terms;

            //Editorを持つか確認
            if (!(terms is Seasons || terms is Days || terms is TimePeriods)) return;

            Form fm = new Form();
            fm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            fm.MaximizeBox = false;
            fm.MinimizeBox = false;
            fm.Text = "期間定義の詳細";
            //ノードが季節定義の場合
            if (terms is Seasons)
            {
                SeasonsEditor sEditor = new SeasonsEditor();
                sEditor.SetSeasons((Seasons)terms);
                sEditor.Editable = false;
                fm.Controls.Add(sEditor);
                fm.ClientSize = sEditor.Size;
                fm.MinimumSize = fm.Size;
                sEditor.Dock = DockStyle.Fill;
            }
            //ノードが曜日定義の場合
            else if (terms is Days)
            {
                DaysEditor dEditor = new DaysEditor();
                dEditor.SetDays((Days)terms);
                dEditor.Editable = false;
                fm.Controls.Add(dEditor);
                fm.ClientSize = dEditor.Size;
                fm.MinimumSize = fm.Size;
                dEditor.Dock = DockStyle.Fill;
            }
            //ノードが時間帯定義の場合
            else if (terms is TimePeriods)
            {
                TimePeriodsEditor tEditor = new TimePeriodsEditor();
                tEditor.SetTimePeriods((TimePeriods)terms);
                tEditor.Editable = false;
                fm.Controls.Add(tEditor);
                fm.ClientSize = tEditor.Size;
                fm.MinimumSize = fm.Size;
                tEditor.Dock = DockStyle.Fill;
            }
            //モーダルダイアログとして表示
            fm.ShowDialog(this);
        }

        #endregion//イベント発生時の処理

        #region<スケジューラのイベントに対する処理>

        /// <summary>スケジューラ名称変更イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scheduler_SchedulerNameChangeEvent(object sender, EventArgs e)
        {
            scheduleTree.Nodes[0].Text = scheduler.Name;
        }

        /// <summary>スケジューラ初期化イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scheduler_SchedulerInitializeEvent(object sender, EventArgs e)
        {
            //スケジューラオブジェクトを取得
            Scheduler<TYPE> sc = (Scheduler<TYPE>)sender;

            //TreeNodeを特定
            TreeNode tNode;
            //編集中のスケジューラそのものの場合
            if (ReferenceEquals(sc, scheduler))
            {
                tNode = scheduleTree.Nodes[0];
            }
            else
            {
                //対応するTreeNodeを特定する
                tNode = getTreeNodeFromScheduler(scheduleTree.Nodes, sc).Parent;
            }
            //該当するTreeNodeが未作成の場合はイベントを無視
            if (tNode == null) return;

            //子ノードを削除する
            tNode.Nodes.Clear();
            //子ノードを追加する
            string[] termNames = sc.GetTermNames();
            foreach (string tName in termNames)
            {
                PhasedTreeNode tn = new PhasedTreeNode(tName);
                //ノードとスケジューラを対応付け
                tn.Tag = sc;
                tNode.Nodes.Add(tn);
            }
        }

        /// <summary>スケジューラ削除イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scheduler_SchedulerRemoveEvent(object sender, SchedulerEventArgs<TYPE> e)
        {
            //スケジューラオブジェクトを取得
            Scheduler<TYPE> sc = (Scheduler<TYPE>)sender;
            //対応するTreeNodeを特定する
            TreeNode tNode = getTreeNodeFromSchedulerAndTermName(scheduleTree.Nodes, sc, e.TermName);
            //該当するTreeNodeが未作成の場合はイベントを無視
            if (tNode == null) return;

            //子ノードを削除する
            tNode.Nodes.Clear();

            //ボタンの状態を更新
            updateTSBtn();
        }

        /// <summary>スケジューラ設定イベント発生時の処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scheduler_SchedulerSetEvent(object sender, SchedulerEventArgs<TYPE> e)
        {
            //スケジューラオブジェクトを取得
            Scheduler<TYPE> sc = (Scheduler<TYPE>)sender;
            //対応するTreeNodeを特定する
            TreeNode tNode = getTreeNodeFromSchedulerAndTermName(scheduleTree.Nodes, sc, e.TermName);
            //該当するTreeNodeが未作成の場合はイベントを無視
            if (tNode == null) return;

            //子ノードを削除する
            tNode.Nodes.Clear();
            //子ノードを追加する
            string[] termNames = e.TargetScheduler.GetTermNames();
            foreach (string tName in termNames)
            {
                PhasedTreeNode tn = new PhasedTreeNode(tName);
                //ノードとスケジューラを対応付け
                tn.Tag = e.TargetScheduler;
                tNode.Nodes.Add(tn);
            }

            //Schedulerのイベント登録
            e.TargetScheduler.SchedulerInitializeEvent += new Scheduler<TYPE>.SchedulerInitializeEventHandler(scheduler_SchedulerInitializeEvent);
            e.TargetScheduler.SchedulerSetEvent += new Scheduler<TYPE>.SchedulerSetEventHandler(scheduler_SchedulerSetEvent);
            e.TargetScheduler.SchedulerRemoveEvent += new Scheduler<TYPE>.SchedulerRemoveEventHandler(scheduler_SchedulerRemoveEvent);
            //コントロール破棄時にイベント登録を解除
            this.Disposed += delegate(object sender2, EventArgs e2)
            {
                e.TargetScheduler.SchedulerInitializeEvent -= new Scheduler<TYPE>.SchedulerInitializeEventHandler(scheduler_SchedulerInitializeEvent);
                e.TargetScheduler.SchedulerSetEvent -= new Scheduler<TYPE>.SchedulerSetEventHandler(scheduler_SchedulerSetEvent);
                e.TargetScheduler.SchedulerRemoveEvent -= new Scheduler<TYPE>.SchedulerRemoveEventHandler(scheduler_SchedulerRemoveEvent);
            };

            //ボタンの状態を更新
            updateTSBtn();
        }

        #endregion//スケジューラのイベントに対する処理
        
    }
}
