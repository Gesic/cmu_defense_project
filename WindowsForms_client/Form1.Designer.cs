namespace WindowsForms_client
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.LPN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RE = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ON = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ODB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OSA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VMM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VModel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.LPN,
            this.Status,
            this.RE,
            this.ON,
            this.ODB,
            this.OSA,
            this.OC,
            this.VM,
            this.VMM,
            this.VModel,
            this.VC});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 24);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 27;
            this.dataGridView1.Size = new System.Drawing.Size(700, 336);
            this.dataGridView1.TabIndex = 0;
            // 
            // LPN
            // 
            this.LPN.HeaderText = "License Plate Numbeer";
            this.LPN.MinimumWidth = 6;
            this.LPN.Name = "LPN";
            this.LPN.Width = 125;
            // 
            // Status
            // 
            this.Status.HeaderText = "Status";
            this.Status.MinimumWidth = 6;
            this.Status.Name = "Status";
            this.Status.Width = 125;
            // 
            // RE
            // 
            this.RE.HeaderText = "Registeration Expiration";
            this.RE.MinimumWidth = 6;
            this.RE.Name = "RE";
            this.RE.Width = 125;
            // 
            // ON
            // 
            this.ON.HeaderText = "Owner Name";
            this.ON.MinimumWidth = 6;
            this.ON.Name = "ON";
            this.ON.Width = 125;
            // 
            // ODB
            // 
            this.ODB.HeaderText = "Owner Birth Date";
            this.ODB.MinimumWidth = 6;
            this.ODB.Name = "ODB";
            this.ODB.Width = 125;
            // 
            // OSA
            // 
            this.OSA.HeaderText = "Location";
            this.OSA.MinimumWidth = 6;
            this.OSA.Name = "OSA";
            this.OSA.Width = 125;
            // 
            // OC
            // 
            this.OC.HeaderText = "Owner City";
            this.OC.MinimumWidth = 6;
            this.OC.Name = "OC";
            this.OC.Width = 125;
            // 
            // VM
            // 
            this.VM.HeaderText = "Vehicle Manufacturer";
            this.VM.MinimumWidth = 6;
            this.VM.Name = "VM";
            this.VM.Width = 125;
            // 
            // VMM
            // 
            this.VMM.HeaderText = "Make";
            this.VMM.MinimumWidth = 6;
            this.VMM.Name = "VMM";
            this.VMM.Width = 125;
            // 
            // VModel
            // 
            this.VModel.HeaderText = "Model";
            this.VModel.MinimumWidth = 6;
            this.VModel.Name = "VModel";
            this.VModel.Width = 125;
            // 
            // VC
            // 
            this.VC.HeaderText = "Color";
            this.VC.MinimumWidth = 6;
            this.VC.Name = "VC";
            this.VC.Width = 125;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(700, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuToolStripMenuItem
            // 
            this.menuToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem});
            this.menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            this.menuToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.menuToolStripMenuItem.Text = "Menu";
            this.menuToolStripMenuItem.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuToolStripMenuItem_DropDownItemClicked);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 360);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Detection List";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn LPN;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn RE;
        private System.Windows.Forms.DataGridViewTextBoxColumn ON;
        private System.Windows.Forms.DataGridViewTextBoxColumn ODB;
        private System.Windows.Forms.DataGridViewTextBoxColumn OSA;
        private System.Windows.Forms.DataGridViewTextBoxColumn OC;
        private System.Windows.Forms.DataGridViewTextBoxColumn VM;
        private System.Windows.Forms.DataGridViewTextBoxColumn VMM;
        private System.Windows.Forms.DataGridViewTextBoxColumn VModel;
        private System.Windows.Forms.DataGridViewTextBoxColumn VC;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
    }
}

