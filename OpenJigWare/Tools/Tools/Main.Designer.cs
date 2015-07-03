namespace Tools
{
    partial class Main
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
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn3D = new System.Windows.Forms.Button();
            this.btn3D_Full = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtRatio = new System.Windows.Forms.TextBox();
            this.txtTest = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btn3D
            // 
            this.btn3D.Location = new System.Drawing.Point(60, 24);
            this.btn3D.Name = "btn3D";
            this.btn3D.Size = new System.Drawing.Size(155, 77);
            this.btn3D.TabIndex = 0;
            this.btn3D.Text = "3D Modeling Tool";
            this.btn3D.UseVisualStyleBackColor = true;
            this.btn3D.Click += new System.EventHandler(this.btn3D_Click);
            // 
            // btn3D_Full
            // 
            this.btn3D_Full.Location = new System.Drawing.Point(60, 166);
            this.btn3D_Full.Name = "btn3D_Full";
            this.btn3D_Full.Size = new System.Drawing.Size(155, 77);
            this.btn3D_Full.TabIndex = 0;
            this.btn3D_Full.Text = "3D Modeling Tool(Ratio)";
            this.btn3D_Full.UseVisualStyleBackColor = true;
            this.btn3D_Full.Click += new System.EventHandler(this.btn3D_Full_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 145);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Ratio";
            // 
            // txtRatio
            // 
            this.txtRatio.Location = new System.Drawing.Point(70, 135);
            this.txtRatio.Name = "txtRatio";
            this.txtRatio.Size = new System.Drawing.Size(97, 25);
            this.txtRatio.TabIndex = 2;
            this.txtRatio.Text = "1.0";
            this.txtRatio.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtRatio_MouseClick);
            // 
            // txtTest
            // 
            this.txtTest.Location = new System.Drawing.Point(173, 135);
            this.txtTest.Name = "txtTest";
            this.txtTest.Size = new System.Drawing.Size(97, 25);
            this.txtTest.TabIndex = 2;
            this.txtTest.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtTest_MouseClick);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 255);
            this.Controls.Add(this.txtTest);
            this.Controls.Add(this.txtRatio);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn3D_Full);
            this.Controls.Add(this.btn3D);
            this.Name = "Main";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn3D;
        private System.Windows.Forms.Button btn3D_Full;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRatio;
        private System.Windows.Forms.TextBox txtTest;
    }
}

