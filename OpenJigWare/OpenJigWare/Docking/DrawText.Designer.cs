namespace OpenJigWare.Docking
{
    partial class frmDrawText
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
            this.rtxtDraw = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // rtxtDraw
            // 
            this.rtxtDraw.Location = new System.Drawing.Point(12, 12);
            this.rtxtDraw.Name = "rtxtDraw";
            this.rtxtDraw.Size = new System.Drawing.Size(981, 458);
            this.rtxtDraw.TabIndex = 0;
            this.rtxtDraw.Text = "";
            this.rtxtDraw.KeyDown += new System.Windows.Forms.KeyEventHandler(this.rtxtDraw_KeyDown);
            this.rtxtDraw.MouseDown += new System.Windows.Forms.MouseEventHandler(this.rtxtDraw_MouseDown);
            // 
            // frmDrawText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1005, 482);
            this.Controls.Add(this.rtxtDraw);
            this.Name = "frmDrawText";
            this.Text = "DrawText";
            this.Load += new System.EventHandler(this.frmDrawText_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox rtxtDraw;

    }
}