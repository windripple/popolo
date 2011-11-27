namespace Popolo.ThermophysicalProperty
{
    partial class MoistAirTable
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MoistAirTable));
            this.dgAirTable = new System.Windows.Forms.DataGridView();
            this.colDBTemp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWBTemp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAHumid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRHumid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEnthalpy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSVolume = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDewPoint = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPressure = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgAirTable)).BeginInit();
            this.SuspendLayout();
            // 
            // dgAirTable
            // 
            this.dgAirTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgAirTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgAirTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDBTemp,
            this.colWBTemp,
            this.colAHumid,
            this.colRHumid,
            this.colEnthalpy,
            this.colSVolume,
            this.colDewPoint,
            this.colPressure});
            resources.ApplyResources(this.dgAirTable, "dgAirTable");
            this.dgAirTable.Name = "dgAirTable";
            this.dgAirTable.RowTemplate.Height = 21;
            this.dgAirTable.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgAirTable_CellValueChanged);
            this.dgAirTable.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.dgAirTable_RowsAdded);
            this.dgAirTable.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgAirTable_KeyDown);
            this.dgAirTable.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.dgAirTable_RowsRemoved);
            // 
            // colDBTemp
            // 
            resources.ApplyResources(this.colDBTemp, "colDBTemp");
            this.colDBTemp.Name = "colDBTemp";
            this.colDBTemp.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colWBTemp
            // 
            resources.ApplyResources(this.colWBTemp, "colWBTemp");
            this.colWBTemp.Name = "colWBTemp";
            this.colWBTemp.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colAHumid
            // 
            resources.ApplyResources(this.colAHumid, "colAHumid");
            this.colAHumid.Name = "colAHumid";
            this.colAHumid.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colRHumid
            // 
            resources.ApplyResources(this.colRHumid, "colRHumid");
            this.colRHumid.Name = "colRHumid";
            this.colRHumid.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colEnthalpy
            // 
            resources.ApplyResources(this.colEnthalpy, "colEnthalpy");
            this.colEnthalpy.Name = "colEnthalpy";
            this.colEnthalpy.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colSVolume
            // 
            resources.ApplyResources(this.colSVolume, "colSVolume");
            this.colSVolume.Name = "colSVolume";
            this.colSVolume.ReadOnly = true;
            this.colSVolume.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colDewPoint
            // 
            resources.ApplyResources(this.colDewPoint, "colDewPoint");
            this.colDewPoint.Name = "colDewPoint";
            this.colDewPoint.ReadOnly = true;
            this.colDewPoint.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colPressure
            // 
            resources.ApplyResources(this.colPressure, "colPressure");
            this.colPressure.Name = "colPressure";
            this.colPressure.ReadOnly = true;
            // 
            // MoistAirTable
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgAirTable);
            this.Name = "MoistAirTable";
            ((System.ComponentModel.ISupportInitialize)(this.dgAirTable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgAirTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDBTemp;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWBTemp;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAHumid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRHumid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEnthalpy;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSVolume;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDewPoint;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPressure;
    }
}
