namespace Popolo.Utility.Schedule.Control
{
    partial class SeasonsEditor
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
            this.components = new System.ComponentModel.Container();
            this.lbxSeasons = new System.Windows.Forms.ListBox();
            this.dtPickerStart = new System.Windows.Forms.DateTimePicker();
            this.dtPickerEnd = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.tbxSeasonsName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbxSeasonName = new System.Windows.Forms.TextBox();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbxSeasons
            // 
            this.lbxSeasons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbxSeasons.FormattingEnabled = true;
            this.lbxSeasons.IntegralHeight = false;
            this.lbxSeasons.ItemHeight = 12;
            this.lbxSeasons.Location = new System.Drawing.Point(0, 78);
            this.lbxSeasons.Name = "lbxSeasons";
            this.lbxSeasons.ScrollAlwaysVisible = true;
            this.lbxSeasons.Size = new System.Drawing.Size(230, 72);
            this.lbxSeasons.TabIndex = 0;
            this.lbxSeasons.SelectedIndexChanged += new System.EventHandler(this.lbxSeasons_SelectedIndexChanged);
            // 
            // dtPickerStart
            // 
            this.dtPickerStart.CustomFormat = "MM月dd日";
            this.dtPickerStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtPickerStart.Location = new System.Drawing.Point(3, 53);
            this.dtPickerStart.MaxDate = new System.DateTime(2001, 12, 31, 0, 0, 0, 0);
            this.dtPickerStart.MinDate = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.dtPickerStart.Name = "dtPickerStart";
            this.dtPickerStart.ShowUpDown = true;
            this.dtPickerStart.Size = new System.Drawing.Size(72, 19);
            this.dtPickerStart.TabIndex = 1;
            this.toolTip.SetToolTip(this.dtPickerStart, "季節開始月日を設定します");
            this.dtPickerStart.Value = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.dtPickerStart.ValueChanged += new System.EventHandler(this.dtPickerStart_ValueChanged);
            // 
            // dtPickerEnd
            // 
            this.dtPickerEnd.CustomFormat = "MM月dd日";
            this.dtPickerEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtPickerEnd.Location = new System.Drawing.Point(97, 53);
            this.dtPickerEnd.MaxDate = new System.DateTime(2001, 12, 31, 0, 0, 0, 0);
            this.dtPickerEnd.MinDate = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.dtPickerEnd.Name = "dtPickerEnd";
            this.dtPickerEnd.ShowUpDown = true;
            this.dtPickerEnd.Size = new System.Drawing.Size(72, 19);
            this.dtPickerEnd.TabIndex = 1;
            this.toolTip.SetToolTip(this.dtPickerEnd, "季節終了月日を設定します");
            this.dtPickerEnd.Value = new System.DateTime(2001, 12, 31, 0, 0, 0, 0);
            this.dtPickerEnd.ValueChanged += new System.EventHandler(this.dtPickerEnd_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(78, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "～";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.tbxSeasonsName);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.tbxSeasonName);
            this.panel1.Controls.Add(this.btnRemove);
            this.panel1.Controls.Add(this.btnAdd);
            this.panel1.Controls.Add(this.dtPickerStart);
            this.panel1.Controls.Add(this.dtPickerEnd);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.MinimumSize = new System.Drawing.Size(230, 78);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(230, 78);
            this.panel1.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "名称：";
            // 
            // tbxSeasonsName
            // 
            this.tbxSeasonsName.Location = new System.Drawing.Point(44, 5);
            this.tbxSeasonsName.Name = "tbxSeasonsName";
            this.tbxSeasonsName.Size = new System.Drawing.Size(180, 19);
            this.tbxSeasonsName.TabIndex = 4;
            this.tbxSeasonsName.TextChanged += new System.EventHandler(this.tbxSeasonsName_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "季節名称：";
            this.toolTip.SetToolTip(this.label2, "季節名称を変更します");
            // 
            // tbxSeasonName
            // 
            this.tbxSeasonName.Location = new System.Drawing.Point(68, 30);
            this.tbxSeasonName.Name = "tbxSeasonName";
            this.tbxSeasonName.Size = new System.Drawing.Size(156, 19);
            this.tbxSeasonName.TabIndex = 4;
            this.toolTip.SetToolTip(this.tbxSeasonName, "季節名称を変更します");
            this.tbxSeasonName.TextChanged += new System.EventHandler(this.tbxSeasonName_TextChanged);
            // 
            // btnRemove
            // 
            this.btnRemove.BackgroundImage = global::Popolo.Utility.Properties.Resources.DeleteIcon;
            this.btnRemove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnRemove.Location = new System.Drawing.Point(200, 51);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(24, 24);
            this.btnRemove.TabIndex = 3;
            this.toolTip.SetToolTip(this.btnRemove, "選択中の季節を削除します");
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.BackgroundImage = global::Popolo.Utility.Properties.Resources.NewIcon;
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnAdd.Location = new System.Drawing.Point(176, 51);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(24, 24);
            this.btnAdd.TabIndex = 3;
            this.toolTip.SetToolTip(this.btnAdd, "選択中の季節を分割して新規の季節を作成します");
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // SeasonsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbxSeasons);
            this.Controls.Add(this.panel1);
            this.MinimumSize = new System.Drawing.Size(230, 150);
            this.Name = "SeasonsEditor";
            this.Size = new System.Drawing.Size(230, 150);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbxSeasons;
        private System.Windows.Forms.DateTimePicker dtPickerStart;
        private System.Windows.Forms.DateTimePicker dtPickerEnd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbxSeasonName;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbxSeasonsName;

    }
}
