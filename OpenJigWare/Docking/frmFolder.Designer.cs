namespace OpenJigWare.Docking
{
    partial class frmFolder
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblInfo = new System.Windows.Forms.Label();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.picInfo = new System.Windows.Forms.PictureBox();
            this.Label4 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.lblParentPath = new System.Windows.Forms.Label();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.picParent = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picParent)).BeginInit();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblInfo.Location = new System.Drawing.Point(62, 99);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(265, 34);
            this.lblInfo.TabIndex = 39;
            // 
            // GroupBox2
            // 
            this.GroupBox2.Location = new System.Drawing.Point(67, 84);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(260, 2);
            this.GroupBox2.TabIndex = 38;
            this.GroupBox2.TabStop = false;
            // 
            // picInfo
            // 
            this.picInfo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picInfo.Location = new System.Drawing.Point(22, 99);
            this.picInfo.Name = "picInfo";
            this.picInfo.Size = new System.Drawing.Size(34, 34);
            this.picInfo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picInfo.TabIndex = 37;
            this.picInfo.TabStop = false;
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(12, 79);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(53, 12);
            this.Label4.TabIndex = 36;
            this.Label4.Text = "발견정보";
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(12, 9);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(53, 12);
            this.Label1.TabIndex = 32;
            this.Label1.Text = "부모폴더";
            // 
            // lblParentPath
            // 
            this.lblParentPath.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblParentPath.Location = new System.Drawing.Point(62, 29);
            this.lblParentPath.Name = "lblParentPath";
            this.lblParentPath.Size = new System.Drawing.Size(265, 34);
            this.lblParentPath.TabIndex = 35;
            // 
            // GroupBox1
            // 
            this.GroupBox1.Location = new System.Drawing.Point(67, 14);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(260, 2);
            this.GroupBox1.TabIndex = 34;
            this.GroupBox1.TabStop = false;
            // 
            // picParent
            // 
            this.picParent.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picParent.Location = new System.Drawing.Point(22, 29);
            this.picParent.Name = "picParent";
            this.picParent.Size = new System.Drawing.Size(34, 34);
            this.picParent.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picParent.TabIndex = 33;
            this.picParent.TabStop = false;
            // 
            // frmFolder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(343, 147);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.picInfo);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.lblParentPath);
            this.Controls.Add(this.GroupBox1);
            this.Controls.Add(this.picParent);
            this.Name = "frmFolder";
            this.Text = "frmFolder";
            this.Load += new System.EventHandler(this.frmFolder_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picParent)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label lblInfo;
        internal System.Windows.Forms.GroupBox GroupBox2;
        internal System.Windows.Forms.PictureBox picInfo;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Label lblParentPath;
        internal System.Windows.Forms.GroupBox GroupBox1;
        internal System.Windows.Forms.PictureBox picParent;
    }
}