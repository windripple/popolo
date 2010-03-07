namespace Popolo.Utility.Schedule.Control
{
    partial class SchedulerEditor<TYPE>
    {
        /// <summary> 
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナで生成されたコード

        /// <summary> 
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.scheduleTree = new System.Windows.Forms.TreeView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbtnAdd = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsbtnRemove = new System.Windows.Forms.ToolStripButton();
            this.tsbtnProperty = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // scheduleTree
            // 
            this.scheduleTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scheduleTree.HideSelection = false;
            this.scheduleTree.Location = new System.Drawing.Point(0, 25);
            this.scheduleTree.Name = "scheduleTree";
            this.scheduleTree.Size = new System.Drawing.Size(200, 155);
            this.scheduleTree.TabIndex = 0;
            this.scheduleTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.scheduleTree_BeforeExpand);
            this.scheduleTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.scheduleTree_AfterSelect);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbtnAdd,
            this.tsbtnRemove,
            this.tsbtnProperty});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(200, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbtnAdd
            // 
            this.tsbtnAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnAdd.Image = global::Popolo.Controls.Properties.Resources.NewIcon;
            this.tsbtnAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnAdd.Name = "tsbtnAdd";
            this.tsbtnAdd.Size = new System.Drawing.Size(29, 22);
            this.tsbtnAdd.Text = "toolStripDropDownButton1";
            this.tsbtnAdd.ToolTipText = "季節や時間帯などの期間構造をもつスケジュールを設定します";
            this.tsbtnAdd.DropDownOpening += new System.EventHandler(this.tsbtnAdd_DropDownOpening);
            this.tsbtnAdd.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.tsbtnAdd_DropDownItemClicked);
            // 
            // tsbtnRemove
            // 
            this.tsbtnRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnRemove.Enabled = false;
            this.tsbtnRemove.Image = global::Popolo.Controls.Properties.Resources.DeleteIcon;
            this.tsbtnRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnRemove.Name = "tsbtnRemove";
            this.tsbtnRemove.Size = new System.Drawing.Size(23, 22);
            this.tsbtnRemove.Text = "toolStripButton1";
            this.tsbtnRemove.ToolTipText = "スケジュールを削除します";
            this.tsbtnRemove.Click += new System.EventHandler(this.tsbtnRemove_Click);
            // 
            // tsbtnProperty
            // 
            this.tsbtnProperty.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnProperty.Image = global::Popolo.Controls.Properties.Resources.PropertyIcon;
            this.tsbtnProperty.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnProperty.Name = "tsbtnProperty";
            this.tsbtnProperty.Size = new System.Drawing.Size(23, 22);
            this.tsbtnProperty.Text = "季節や時間帯などの期間定義を確認します";
            this.tsbtnProperty.Click += new System.EventHandler(this.tsbtnProperty_Click);
            // 
            // SchedulerEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.scheduleTree);
            this.Controls.Add(this.toolStrip1);
            this.MinimumSize = new System.Drawing.Size(120, 80);
            this.Name = "SchedulerEditor";
            this.Size = new System.Drawing.Size(200, 180);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView scheduleTree;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton tsbtnAdd;
        private System.Windows.Forms.ToolStripButton tsbtnRemove;
        private System.Windows.Forms.ToolStripButton tsbtnProperty;
    }
}
