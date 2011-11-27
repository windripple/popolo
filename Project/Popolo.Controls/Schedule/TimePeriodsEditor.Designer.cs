namespace Popolo.Schedule
{
    partial class TimePeriodsEditor
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
            this.lbxTimePeriods = new System.Windows.Forms.ListBox();
            this.dtPickerStart = new System.Windows.Forms.DateTimePicker();
            this.dtPickerEnd = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.tbxTimePeriodsName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbxTimePeriodName = new System.Windows.Forms.TextBox();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbxTimePeriods
            // 
            this.lbxTimePeriods.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbxTimePeriods.FormattingEnabled = true;
            this.lbxTimePeriods.IntegralHeight = false;
            this.lbxTimePeriods.ItemHeight = 12;
            this.lbxTimePeriods.Location = new System.Drawing.Point(0, 78);
            this.lbxTimePeriods.Name = "lbxTimePeriods";
            this.lbxTimePeriods.ScrollAlwaysVisible = true;
            this.lbxTimePeriods.Size = new System.Drawing.Size(230, 72);
            this.lbxTimePeriods.TabIndex = 0;
            this.lbxTimePeriods.SelectedIndexChanged += new System.EventHandler(this.lbxTimePeriods_SelectedIndexChanged);
            // 
            // dtPickerStart
            // 
            this.dtPickerStart.CustomFormat = "HH時mm分";
            this.dtPickerStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtPickerStart.Location = new System.Drawing.Point(3, 53);
            this.dtPickerStart.MaxDate = new System.DateTime(2001, 1, 1, 23, 59, 59, 0);
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
            this.dtPickerEnd.CustomFormat = "HH時mm分";
            this.dtPickerEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtPickerEnd.Location = new System.Drawing.Point(97, 53);
            this.dtPickerEnd.MaxDate = new System.DateTime(2001, 1, 1, 23, 59, 0, 0);
            this.dtPickerEnd.MinDate = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.dtPickerEnd.Name = "dtPickerEnd";
            this.dtPickerEnd.ShowUpDown = true;
            this.dtPickerEnd.Size = new System.Drawing.Size(72, 19);
            this.dtPickerEnd.TabIndex = 1;
            this.toolTip.SetToolTip(this.dtPickerEnd, "季節終了月日を設定します");
            this.dtPickerEnd.Value = new System.DateTime(2001, 1, 1, 23, 59, 0, 0);
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
            this.panel1.Controls.Add(this.tbxTimePeriodsName);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.tbxTimePeriodName);
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
            this.label3.TabIndex = 7;
            this.label3.Text = "名称：";
            // 
            // tbxTimePeriodsName
            // 
            this.tbxTimePeriodsName.Location = new System.Drawing.Point(44, 5);
            this.tbxTimePeriodsName.Name = "tbxTimePeriodsName";
            this.tbxTimePeriodsName.Size = new System.Drawing.Size(180, 19);
            this.tbxTimePeriodsName.TabIndex = 6;
            this.tbxTimePeriodsName.TextChanged += new System.EventHandler(this.tbxTimePeriodsName_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "時間帯名称：";
            this.toolTip.SetToolTip(this.label2, "季節名称を変更します");
            // 
            // tbxTimePeriodName
            // 
            this.tbxTimePeriodName.Location = new System.Drawing.Point(80, 30);
            this.tbxTimePeriodName.Name = "tbxTimePeriodName";
            this.tbxTimePeriodName.Size = new System.Drawing.Size(144, 19);
            this.tbxTimePeriodName.TabIndex = 4;
            this.toolTip.SetToolTip(this.tbxTimePeriodName, "季節名称を変更します");
            this.tbxTimePeriodName.TextChanged += new System.EventHandler(this.tbxTimePeriodName_TextChanged);
            // 
            // btnRemove
            // 
            this.btnRemove.BackgroundImage = global::Popolo.Controls.Properties.Resources.DeleteIcon;
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
            this.btnAdd.BackgroundImage = global::Popolo.Controls.Properties.Resources.NewIcon;
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnAdd.Location = new System.Drawing.Point(176, 51);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(24, 24);
            this.btnAdd.TabIndex = 3;
            this.toolTip.SetToolTip(this.btnAdd, "選択中の季節を分割して新規の季節を作成します");
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // TimePeriodsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbxTimePeriods);
            this.Controls.Add(this.panel1);
            this.MinimumSize = new System.Drawing.Size(230, 150);
            this.Name = "TimePeriodsEditor";
            this.Size = new System.Drawing.Size(230, 150);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbxTimePeriods;
        private System.Windows.Forms.DateTimePicker dtPickerStart;
        private System.Windows.Forms.DateTimePicker dtPickerEnd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbxTimePeriodName;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbxTimePeriodsName;

    }
}
