namespace OpenJigWare.Docking
{
    partial class frmDesigner
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDesigner));
            this.pnProp = new System.Windows.Forms.Panel();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.tmrDisp = new System.Windows.Forms.Timer(this.components);
            this.pnProp_Selected = new System.Windows.Forms.Panel();
            this.pnMotors = new System.Windows.Forms.Panel();
            this.mnstMenu = new System.Windows.Forms.MenuStrip();
            this.tsmnFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.mnClose = new System.Windows.Forms.ToolStripMenuItem();
            this.mnSave = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnOpenAndCopyStl = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnExit = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnView_DrawText = new System.Windows.Forms.ToolStripMenuItem();
            this.mnKinematics = new System.Windows.Forms.ToolStripMenuItem();
            this.mn3D = new System.Windows.Forms.ToolStripMenuItem();
            this.mnMotionTool = new System.Windows.Forms.ToolStripMenuItem();
            this.tcJson = new System.Windows.Forms.TabControl();
            this.tpProp = new System.Windows.Forms.TabPage();
            this.btnPos_Left = new System.Windows.Forms.Button();
            this.btnPos_Bottom = new System.Windows.Forms.Button();
            this.btnPos_Top = new System.Windows.Forms.Button();
            this.txtBack_Pan = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtBack_Tilt = new System.Windows.Forms.TextBox();
            this.txtBack_Swing = new System.Windows.Forms.TextBox();
            this.btnPos_Front = new System.Windows.Forms.Button();
            this.txtBack_X = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.txtBack_Y = new System.Windows.Forms.TextBox();
            this.txtBack_Z = new System.Windows.Forms.TextBox();
            this.btnPos_Right = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.chkEnMotor = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBaud = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtComPort = new System.Windows.Forms.TextBox();
            this.btnCheckComport = new System.Windows.Forms.Button();
            this.btnInit2 = new System.Windows.Forms.Button();
            this.btnInit1 = new System.Windows.Forms.Button();
            this.btnInit0 = new System.Windows.Forms.Button();
            this.tsTop = new System.Windows.Forms.ToolStrip();
            this.tsbtnModel0 = new System.Windows.Forms.ToolStripButton();
            this.tsbtnModel1 = new System.Windows.Forms.ToolStripButton();
            this.tsbtnModel2 = new System.Windows.Forms.ToolStripButton();
            this.tsbtnModel3 = new System.Windows.Forms.ToolStripButton();
            this.tsbtnModel4 = new System.Windows.Forms.ToolStripButton();
            this.tsbtnModel5 = new System.Windows.Forms.ToolStripButton();
            this.tsbtnModel6 = new System.Windows.Forms.ToolStripButton();
            this.tsbtnModel10 = new System.Windows.Forms.ToolStripButton();
            this.tsbtnModel11 = new System.Windows.Forms.ToolStripButton();
            this.tsbtnModel12 = new System.Windows.Forms.ToolStripButton();
            this.tsbtnModel13 = new System.Windows.Forms.ToolStripButton();
            this.tsbtnModel14 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbtnMouseMode = new System.Windows.Forms.ToolStripButton();
            this.tstxtAlpha = new System.Windows.Forms.ToolStripTextBox();
            this.mnstMenu.SuspendLayout();
            this.tcJson.SuspendLayout();
            this.tpProp.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tsTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnProp
            // 
            this.pnProp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pnProp.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.pnProp.Location = new System.Drawing.Point(6, 66);
            this.pnProp.Name = "pnProp";
            this.pnProp.Size = new System.Drawing.Size(314, 472);
            this.pnProp.TabIndex = 1;
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(7, 384);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMessage.Size = new System.Drawing.Size(630, 148);
            this.txtMessage.TabIndex = 2;
            // 
            // tmrDisp
            // 
            this.tmrDisp.Tick += new System.EventHandler(this.tmrDisp_Tick);
            // 
            // pnProp_Selected
            // 
            this.pnProp_Selected.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnProp_Selected.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.pnProp_Selected.Location = new System.Drawing.Point(324, 66);
            this.pnProp_Selected.Name = "pnProp_Selected";
            this.pnProp_Selected.Size = new System.Drawing.Size(314, 472);
            this.pnProp_Selected.TabIndex = 1;
            // 
            // pnMotors
            // 
            this.pnMotors.BackColor = System.Drawing.Color.Transparent;
            this.pnMotors.Location = new System.Drawing.Point(7, 7);
            this.pnMotors.Name = "pnMotors";
            this.pnMotors.Size = new System.Drawing.Size(630, 306);
            this.pnMotors.TabIndex = 3;
            // 
            // mnstMenu
            // 
            this.mnstMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmnFile,
            this.viewToolStripMenuItem});
            this.mnstMenu.Location = new System.Drawing.Point(0, 0);
            this.mnstMenu.Name = "mnstMenu";
            this.mnstMenu.Size = new System.Drawing.Size(671, 28);
            this.mnstMenu.TabIndex = 4;
            this.mnstMenu.Text = "menuStrip1";
            // 
            // tsmnFile
            // 
            this.tsmnFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnOpen,
            this.mnClose,
            this.mnSave,
            this.toolStripSeparator1,
            this.mnOpenAndCopyStl,
            this.toolStripMenuItem1,
            this.mnExit});
            this.tsmnFile.Name = "tsmnFile";
            this.tsmnFile.Size = new System.Drawing.Size(44, 24);
            this.tsmnFile.Text = "&File";
            // 
            // mnOpen
            // 
            this.mnOpen.Name = "mnOpen";
            this.mnOpen.Size = new System.Drawing.Size(231, 24);
            this.mnOpen.Text = "&Open";
            this.mnOpen.Click += new System.EventHandler(this.mnOpen_Click);
            // 
            // mnClose
            // 
            this.mnClose.Name = "mnClose";
            this.mnClose.Size = new System.Drawing.Size(231, 24);
            this.mnClose.Text = "&Close";
            this.mnClose.Click += new System.EventHandler(this.mnClose_Click);
            // 
            // mnSave
            // 
            this.mnSave.Name = "mnSave";
            this.mnSave.Size = new System.Drawing.Size(231, 24);
            this.mnSave.Text = "&Save";
            this.mnSave.Click += new System.EventHandler(this.mnSave_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(228, 6);
            // 
            // mnOpenAndCopyStl
            // 
            this.mnOpenAndCopyStl.Name = "mnOpenAndCopyStl";
            this.mnOpenAndCopyStl.Size = new System.Drawing.Size(231, 24);
            this.mnOpenAndCopyStl.Text = "Open And Extract files";
            this.mnOpenAndCopyStl.Click += new System.EventHandler(this.mnOpenAndCopyStl_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(228, 6);
            // 
            // mnExit
            // 
            this.mnExit.Name = "mnExit";
            this.mnExit.Size = new System.Drawing.Size(231, 24);
            this.mnExit.Text = "&Exit";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnView_DrawText,
            this.mnKinematics,
            this.mn3D,
            this.mnMotionTool});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(54, 24);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // mnView_DrawText
            // 
            this.mnView_DrawText.Name = "mnView_DrawText";
            this.mnView_DrawText.Size = new System.Drawing.Size(158, 24);
            this.mnView_DrawText.Text = "&DrawText";
            this.mnView_DrawText.Click += new System.EventHandler(this.mnView_DrawText_Click);
            // 
            // mnKinematics
            // 
            this.mnKinematics.Name = "mnKinematics";
            this.mnKinematics.Size = new System.Drawing.Size(158, 24);
            this.mnKinematics.Text = "Kinematics";
            this.mnKinematics.Click += new System.EventHandler(this.mnKinematics_Click);
            // 
            // mn3D
            // 
            this.mn3D.Name = "mn3D";
            this.mn3D.Size = new System.Drawing.Size(158, 24);
            this.mn3D.Text = "3D";
            this.mn3D.Click += new System.EventHandler(this.mn3D_Click);
            // 
            // mnMotionTool
            // 
            this.mnMotionTool.Name = "mnMotionTool";
            this.mnMotionTool.Size = new System.Drawing.Size(158, 24);
            this.mnMotionTool.Text = "MotionTool";
            this.mnMotionTool.Click += new System.EventHandler(this.mnMotionTool_Click);
            // 
            // tcJson
            // 
            this.tcJson.Controls.Add(this.tpProp);
            this.tcJson.Controls.Add(this.tabPage2);
            this.tcJson.Location = new System.Drawing.Point(12, 58);
            this.tcJson.Name = "tcJson";
            this.tcJson.SelectedIndex = 0;
            this.tcJson.Size = new System.Drawing.Size(652, 573);
            this.tcJson.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.tcJson.TabIndex = 5;
            // 
            // tpProp
            // 
            this.tpProp.Controls.Add(this.btnPos_Left);
            this.tpProp.Controls.Add(this.btnPos_Bottom);
            this.tpProp.Controls.Add(this.btnPos_Top);
            this.tpProp.Controls.Add(this.txtBack_Pan);
            this.tpProp.Controls.Add(this.label3);
            this.tpProp.Controls.Add(this.label4);
            this.tpProp.Controls.Add(this.label5);
            this.tpProp.Controls.Add(this.txtBack_Tilt);
            this.tpProp.Controls.Add(this.txtBack_Swing);
            this.tpProp.Controls.Add(this.btnPos_Front);
            this.tpProp.Controls.Add(this.txtBack_X);
            this.tpProp.Controls.Add(this.label7);
            this.tpProp.Controls.Add(this.label8);
            this.tpProp.Controls.Add(this.label13);
            this.tpProp.Controls.Add(this.txtBack_Y);
            this.tpProp.Controls.Add(this.txtBack_Z);
            this.tpProp.Controls.Add(this.btnPos_Right);
            this.tpProp.Controls.Add(this.pnProp);
            this.tpProp.Controls.Add(this.pnProp_Selected);
            this.tpProp.Location = new System.Drawing.Point(4, 25);
            this.tpProp.Name = "tpProp";
            this.tpProp.Padding = new System.Windows.Forms.Padding(3);
            this.tpProp.Size = new System.Drawing.Size(644, 544);
            this.tpProp.TabIndex = 0;
            this.tpProp.Text = "Property";
            this.tpProp.UseVisualStyleBackColor = true;
            this.tpProp.Click += new System.EventHandler(this.tpProp_Click);
            // 
            // btnPos_Left
            // 
            this.btnPos_Left.BackColor = System.Drawing.Color.Turquoise;
            this.btnPos_Left.Font = new System.Drawing.Font("Gulim", 8F);
            this.btnPos_Left.Location = new System.Drawing.Point(513, 14);
            this.btnPos_Left.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPos_Left.Name = "btnPos_Left";
            this.btnPos_Left.Size = new System.Drawing.Size(62, 43);
            this.btnPos_Left.TabIndex = 682;
            this.btnPos_Left.Text = "Left";
            this.btnPos_Left.UseVisualStyleBackColor = false;
            this.btnPos_Left.Click += new System.EventHandler(this.btnPos_Left_Click);
            // 
            // btnPos_Bottom
            // 
            this.btnPos_Bottom.BackColor = System.Drawing.Color.Turquoise;
            this.btnPos_Bottom.Font = new System.Drawing.Font("Gulim", 8F);
            this.btnPos_Bottom.Location = new System.Drawing.Point(450, 14);
            this.btnPos_Bottom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPos_Bottom.Name = "btnPos_Bottom";
            this.btnPos_Bottom.Size = new System.Drawing.Size(62, 43);
            this.btnPos_Bottom.TabIndex = 680;
            this.btnPos_Bottom.Text = "Bottom";
            this.btnPos_Bottom.UseVisualStyleBackColor = false;
            this.btnPos_Bottom.Click += new System.EventHandler(this.btnPos_Bottom_Click);
            // 
            // btnPos_Top
            // 
            this.btnPos_Top.BackColor = System.Drawing.Color.Turquoise;
            this.btnPos_Top.Font = new System.Drawing.Font("Gulim", 8F);
            this.btnPos_Top.Location = new System.Drawing.Point(576, 14);
            this.btnPos_Top.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPos_Top.Name = "btnPos_Top";
            this.btnPos_Top.Size = new System.Drawing.Size(62, 43);
            this.btnPos_Top.TabIndex = 681;
            this.btnPos_Top.Text = "Top";
            this.btnPos_Top.UseVisualStyleBackColor = false;
            this.btnPos_Top.Click += new System.EventHandler(this.btnPos_Top_Click);
            // 
            // txtBack_Pan
            // 
            this.txtBack_Pan.Location = new System.Drawing.Point(39, 38);
            this.txtBack_Pan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBack_Pan.Name = "txtBack_Pan";
            this.txtBack_Pan.Size = new System.Drawing.Size(37, 25);
            this.txtBack_Pan.TabIndex = 677;
            this.txtBack_Pan.Text = "0";
            this.txtBack_Pan.TextChanged += new System.EventHandler(this.txtBack_Pan_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("GulimChe", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(8, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 14);
            this.label3.TabIndex = 674;
            this.label3.Text = "Pan";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("GulimChe", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(82, 43);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 14);
            this.label4.TabIndex = 675;
            this.label4.Text = "Tilt";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("GulimChe", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(164, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 14);
            this.label5.TabIndex = 676;
            this.label5.Text = "Swing";
            // 
            // txtBack_Tilt
            // 
            this.txtBack_Tilt.Location = new System.Drawing.Point(120, 38);
            this.txtBack_Tilt.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBack_Tilt.Name = "txtBack_Tilt";
            this.txtBack_Tilt.Size = new System.Drawing.Size(37, 25);
            this.txtBack_Tilt.TabIndex = 678;
            this.txtBack_Tilt.Text = "0";
            this.txtBack_Tilt.TextChanged += new System.EventHandler(this.txtBack_Tilt_TextChanged);
            // 
            // txtBack_Swing
            // 
            this.txtBack_Swing.Location = new System.Drawing.Point(213, 38);
            this.txtBack_Swing.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBack_Swing.Name = "txtBack_Swing";
            this.txtBack_Swing.Size = new System.Drawing.Size(37, 25);
            this.txtBack_Swing.TabIndex = 679;
            this.txtBack_Swing.Text = "0";
            this.txtBack_Swing.TextChanged += new System.EventHandler(this.txtBack_Swing_TextChanged);
            // 
            // btnPos_Front
            // 
            this.btnPos_Front.BackColor = System.Drawing.Color.Turquoise;
            this.btnPos_Front.Font = new System.Drawing.Font("Gulim", 8F);
            this.btnPos_Front.Location = new System.Drawing.Point(324, 14);
            this.btnPos_Front.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPos_Front.Name = "btnPos_Front";
            this.btnPos_Front.Size = new System.Drawing.Size(62, 43);
            this.btnPos_Front.TabIndex = 670;
            this.btnPos_Front.Text = "Front";
            this.btnPos_Front.UseVisualStyleBackColor = false;
            this.btnPos_Front.Click += new System.EventHandler(this.btnPos_Front_Click);
            // 
            // txtBack_X
            // 
            this.txtBack_X.Location = new System.Drawing.Point(39, 10);
            this.txtBack_X.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBack_X.Name = "txtBack_X";
            this.txtBack_X.Size = new System.Drawing.Size(37, 25);
            this.txtBack_X.TabIndex = 666;
            this.txtBack_X.Text = "0";
            this.txtBack_X.TextChanged += new System.EventHandler(this.txtBack_X_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Font = new System.Drawing.Font("GulimChe", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label7.ForeColor = System.Drawing.Color.Black;
            this.label7.Location = new System.Drawing.Point(22, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(14, 14);
            this.label7.TabIndex = 663;
            this.label7.Text = "X";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Font = new System.Drawing.Font("GulimChe", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label8.ForeColor = System.Drawing.Color.Black;
            this.label8.Location = new System.Drawing.Point(103, 15);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(14, 14);
            this.label8.TabIndex = 664;
            this.label8.Text = "Y";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.BackColor = System.Drawing.Color.Transparent;
            this.label13.Font = new System.Drawing.Font("GulimChe", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label13.ForeColor = System.Drawing.Color.Black;
            this.label13.Location = new System.Drawing.Point(192, 15);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(14, 14);
            this.label13.TabIndex = 665;
            this.label13.Text = "Z";
            // 
            // txtBack_Y
            // 
            this.txtBack_Y.Location = new System.Drawing.Point(120, 10);
            this.txtBack_Y.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBack_Y.Name = "txtBack_Y";
            this.txtBack_Y.Size = new System.Drawing.Size(37, 25);
            this.txtBack_Y.TabIndex = 667;
            this.txtBack_Y.Text = "0";
            this.txtBack_Y.TextChanged += new System.EventHandler(this.txtBack_Y_TextChanged);
            // 
            // txtBack_Z
            // 
            this.txtBack_Z.Location = new System.Drawing.Point(213, 10);
            this.txtBack_Z.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBack_Z.Name = "txtBack_Z";
            this.txtBack_Z.Size = new System.Drawing.Size(37, 25);
            this.txtBack_Z.TabIndex = 668;
            this.txtBack_Z.Text = "0";
            this.txtBack_Z.TextChanged += new System.EventHandler(this.txtBack_Z_TextChanged);
            // 
            // btnPos_Right
            // 
            this.btnPos_Right.BackColor = System.Drawing.Color.Turquoise;
            this.btnPos_Right.Font = new System.Drawing.Font("Gulim", 8F);
            this.btnPos_Right.Location = new System.Drawing.Point(387, 14);
            this.btnPos_Right.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPos_Right.Name = "btnPos_Right";
            this.btnPos_Right.Size = new System.Drawing.Size(62, 43);
            this.btnPos_Right.TabIndex = 673;
            this.btnPos_Right.Text = "Right";
            this.btnPos_Right.UseVisualStyleBackColor = false;
            this.btnPos_Right.Click += new System.EventHandler(this.btnPos_Right_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.chkEnMotor);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.txtBaud);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.txtComPort);
            this.tabPage2.Controls.Add(this.btnCheckComport);
            this.tabPage2.Controls.Add(this.btnInit2);
            this.tabPage2.Controls.Add(this.btnInit1);
            this.tabPage2.Controls.Add(this.btnInit0);
            this.tabPage2.Controls.Add(this.txtMessage);
            this.tabPage2.Controls.Add(this.pnMotors);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(644, 544);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Control";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // chkEnMotor
            // 
            this.chkEnMotor.AutoSize = true;
            this.chkEnMotor.Checked = true;
            this.chkEnMotor.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnMotor.Location = new System.Drawing.Point(352, 318);
            this.chkEnMotor.Name = "chkEnMotor";
            this.chkEnMotor.Size = new System.Drawing.Size(116, 19);
            this.chkEnMotor.TabIndex = 7;
            this.chkEnMotor.Text = "Enable Motor";
            this.chkEnMotor.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(463, 357);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "BaudRate";
            // 
            // txtBaud
            // 
            this.txtBaud.Location = new System.Drawing.Point(537, 352);
            this.txtBaud.Name = "txtBaud";
            this.txtBaud.Size = new System.Drawing.Size(100, 25);
            this.txtBaud.TabIndex = 5;
            this.txtBaud.Text = "1000000";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(470, 325);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "ComPort";
            // 
            // txtComPort
            // 
            this.txtComPort.Location = new System.Drawing.Point(537, 320);
            this.txtComPort.Name = "txtComPort";
            this.txtComPort.Size = new System.Drawing.Size(78, 25);
            this.txtComPort.TabIndex = 5;
            this.txtComPort.Text = "1";
            // 
            // btnCheckComport
            // 
            this.btnCheckComport.Location = new System.Drawing.Point(352, 339);
            this.btnCheckComport.Name = "btnCheckComport";
            this.btnCheckComport.Size = new System.Drawing.Size(105, 40);
            this.btnCheckComport.TabIndex = 4;
            this.btnCheckComport.Text = "Check Comport";
            this.btnCheckComport.UseVisualStyleBackColor = true;
            this.btnCheckComport.Click += new System.EventHandler(this.btnCheckComport_Click);
            // 
            // btnInit2
            // 
            this.btnInit2.Location = new System.Drawing.Point(135, 351);
            this.btnInit2.Name = "btnInit2";
            this.btnInit2.Size = new System.Drawing.Size(122, 27);
            this.btnInit2.TabIndex = 4;
            this.btnInit2.Text = "Init2(Motor)";
            this.btnInit2.UseVisualStyleBackColor = true;
            this.btnInit2.Click += new System.EventHandler(this.btnInit2_Click);
            // 
            // btnInit1
            // 
            this.btnInit1.Location = new System.Drawing.Point(7, 351);
            this.btnInit1.Name = "btnInit1";
            this.btnInit1.Size = new System.Drawing.Size(122, 27);
            this.btnInit1.TabIndex = 4;
            this.btnInit1.Text = "Init1(Motor)";
            this.btnInit1.UseVisualStyleBackColor = true;
            this.btnInit1.Click += new System.EventHandler(this.btnInit1_Click);
            // 
            // btnInit0
            // 
            this.btnInit0.Location = new System.Drawing.Point(7, 319);
            this.btnInit0.Name = "btnInit0";
            this.btnInit0.Size = new System.Drawing.Size(122, 27);
            this.btnInit0.TabIndex = 4;
            this.btnInit0.Text = "Clear(Motor)";
            this.btnInit0.UseVisualStyleBackColor = true;
            this.btnInit0.Click += new System.EventHandler(this.btnInit0_Click);
            // 
            // tsTop
            // 
            this.tsTop.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbtnModel0,
            this.tsbtnModel1,
            this.tsbtnModel2,
            this.tsbtnModel3,
            this.tsbtnModel4,
            this.tsbtnModel5,
            this.tsbtnModel6,
            this.tsbtnModel10,
            this.tsbtnModel11,
            this.tsbtnModel12,
            this.tsbtnModel13,
            this.tsbtnModel14,
            this.toolStripSeparator2,
            this.tsbtnMouseMode,
            this.tstxtAlpha});
            this.tsTop.Location = new System.Drawing.Point(0, 28);
            this.tsTop.Name = "tsTop";
            this.tsTop.Size = new System.Drawing.Size(671, 27);
            this.tsTop.TabIndex = 117;
            this.tsTop.Text = "toolStrip1";
            this.tsTop.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.tsTop_ItemClicked);
            // 
            // tsbtnModel0
            // 
            this.tsbtnModel0.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnModel0.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnModel0.Image")));
            this.tsbtnModel0.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnModel0.Name = "tsbtnModel0";
            this.tsbtnModel0.Size = new System.Drawing.Size(23, 24);
            this.tsbtnModel0.Text = "toolStripButton1";
            this.tsbtnModel0.Click += new System.EventHandler(this.tsbtnModel0_Click);
            // 
            // tsbtnModel1
            // 
            this.tsbtnModel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnModel1.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnModel1.Image")));
            this.tsbtnModel1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnModel1.Name = "tsbtnModel1";
            this.tsbtnModel1.Size = new System.Drawing.Size(23, 24);
            this.tsbtnModel1.Text = "toolStripButton2";
            this.tsbtnModel1.Click += new System.EventHandler(this.tsbtnModel1_Click);
            // 
            // tsbtnModel2
            // 
            this.tsbtnModel2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnModel2.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnModel2.Image")));
            this.tsbtnModel2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnModel2.Name = "tsbtnModel2";
            this.tsbtnModel2.Size = new System.Drawing.Size(23, 24);
            this.tsbtnModel2.Text = "toolStripButton3";
            this.tsbtnModel2.Click += new System.EventHandler(this.tsbtnModel2_Click);
            // 
            // tsbtnModel3
            // 
            this.tsbtnModel3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnModel3.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnModel3.Image")));
            this.tsbtnModel3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnModel3.Name = "tsbtnModel3";
            this.tsbtnModel3.Size = new System.Drawing.Size(23, 24);
            this.tsbtnModel3.Text = "toolStripButton4";
            this.tsbtnModel3.Click += new System.EventHandler(this.tsbtnModel3_Click);
            // 
            // tsbtnModel4
            // 
            this.tsbtnModel4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnModel4.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnModel4.Image")));
            this.tsbtnModel4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnModel4.Name = "tsbtnModel4";
            this.tsbtnModel4.Size = new System.Drawing.Size(23, 24);
            this.tsbtnModel4.Text = "toolStripButton5";
            this.tsbtnModel4.Click += new System.EventHandler(this.tsbtnModel4_Click);
            // 
            // tsbtnModel5
            // 
            this.tsbtnModel5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnModel5.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnModel5.Image")));
            this.tsbtnModel5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnModel5.Name = "tsbtnModel5";
            this.tsbtnModel5.Size = new System.Drawing.Size(23, 24);
            this.tsbtnModel5.Text = "toolStripButton1";
            this.tsbtnModel5.Click += new System.EventHandler(this.tsbtnModel5_Click);
            // 
            // tsbtnModel6
            // 
            this.tsbtnModel6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnModel6.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnModel6.Image")));
            this.tsbtnModel6.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnModel6.Name = "tsbtnModel6";
            this.tsbtnModel6.Size = new System.Drawing.Size(23, 24);
            this.tsbtnModel6.Text = "toolStripButton2";
            this.tsbtnModel6.Click += new System.EventHandler(this.tsbtnModel6_Click);
            // 
            // tsbtnModel10
            // 
            this.tsbtnModel10.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnModel10.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnModel10.Image")));
            this.tsbtnModel10.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnModel10.Name = "tsbtnModel10";
            this.tsbtnModel10.Size = new System.Drawing.Size(23, 24);
            this.tsbtnModel10.Text = "toolStripButton6";
            this.tsbtnModel10.Click += new System.EventHandler(this.tsbtnModel10_Click);
            // 
            // tsbtnModel11
            // 
            this.tsbtnModel11.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnModel11.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnModel11.Image")));
            this.tsbtnModel11.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnModel11.Name = "tsbtnModel11";
            this.tsbtnModel11.Size = new System.Drawing.Size(23, 24);
            this.tsbtnModel11.Text = "toolStripButton7";
            this.tsbtnModel11.Click += new System.EventHandler(this.tsbtnModel11_Click);
            // 
            // tsbtnModel12
            // 
            this.tsbtnModel12.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnModel12.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnModel12.Image")));
            this.tsbtnModel12.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnModel12.Name = "tsbtnModel12";
            this.tsbtnModel12.Size = new System.Drawing.Size(23, 24);
            this.tsbtnModel12.Text = "toolStripButton8";
            this.tsbtnModel12.Click += new System.EventHandler(this.tsbtnModel12_Click);
            // 
            // tsbtnModel13
            // 
            this.tsbtnModel13.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnModel13.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnModel13.Image")));
            this.tsbtnModel13.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnModel13.Name = "tsbtnModel13";
            this.tsbtnModel13.Size = new System.Drawing.Size(23, 24);
            this.tsbtnModel13.Text = "toolStripButton9";
            this.tsbtnModel13.Click += new System.EventHandler(this.tsbtnModel13_Click);
            // 
            // tsbtnModel14
            // 
            this.tsbtnModel14.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnModel14.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnModel14.Image")));
            this.tsbtnModel14.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnModel14.Name = "tsbtnModel14";
            this.tsbtnModel14.Size = new System.Drawing.Size(23, 24);
            this.tsbtnModel14.Text = "toolStripButton10";
            this.tsbtnModel14.Click += new System.EventHandler(this.tsbtnModel14_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbtnMouseMode
            // 
            this.tsbtnMouseMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnMouseMode.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnMouseMode.Image")));
            this.tsbtnMouseMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnMouseMode.Name = "tsbtnMouseMode";
            this.tsbtnMouseMode.Size = new System.Drawing.Size(23, 24);
            this.tsbtnMouseMode.Text = "toolStripButton1";
            this.tsbtnMouseMode.Click += new System.EventHandler(this.tsbtnMouseMode_Click);
            // 
            // tstxtAlpha
            // 
            this.tstxtAlpha.Name = "tstxtAlpha";
            this.tstxtAlpha.Size = new System.Drawing.Size(100, 27);
            this.tstxtAlpha.Text = "1.0";
            this.tstxtAlpha.TextChanged += new System.EventHandler(this.tstxtAlpha_TextChanged);
            // 
            // frmDesigner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 634);
            this.Controls.Add(this.tsTop);
            this.Controls.Add(this.tcJson);
            this.Controls.Add(this.mnstMenu);
            this.MainMenuStrip = this.mnstMenu;
            this.Name = "frmDesigner";
            this.Text = "Designer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmDesigner_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmDesigner_FormClosed);
            this.Load += new System.EventHandler(this.frmDesigner_Load);
            this.Resize += new System.EventHandler(this.frmDesigner_Resize);
            this.mnstMenu.ResumeLayout(false);
            this.mnstMenu.PerformLayout();
            this.tcJson.ResumeLayout(false);
            this.tpProp.ResumeLayout(false);
            this.tpProp.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tsTop.ResumeLayout(false);
            this.tsTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnProp;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Timer tmrDisp;
        private System.Windows.Forms.Panel pnProp_Selected;
        private System.Windows.Forms.Panel pnMotors;
        private System.Windows.Forms.MenuStrip mnstMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmnFile;
        private System.Windows.Forms.ToolStripMenuItem mnOpen;
        private System.Windows.Forms.ToolStripMenuItem mnClose;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnExit;
        private System.Windows.Forms.TabControl tcJson;
        private System.Windows.Forms.TabPage tpProp;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ToolStripMenuItem mnSave;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnView_DrawText;
        private System.Windows.Forms.ToolStripMenuItem mnKinematics;
        private System.Windows.Forms.ToolStrip tsTop;
        private System.Windows.Forms.ToolStripButton tsbtnModel0;
        private System.Windows.Forms.ToolStripButton tsbtnModel1;
        private System.Windows.Forms.ToolStripButton tsbtnModel2;
        private System.Windows.Forms.ToolStripButton tsbtnModel3;
        private System.Windows.Forms.ToolStripButton tsbtnModel4;
        private System.Windows.Forms.ToolStripButton tsbtnModel5;
        private System.Windows.Forms.ToolStripButton tsbtnModel6;
        private System.Windows.Forms.ToolStripButton tsbtnModel10;
        private System.Windows.Forms.ToolStripButton tsbtnModel11;
        private System.Windows.Forms.ToolStripButton tsbtnModel12;
        private System.Windows.Forms.ToolStripButton tsbtnModel13;
        private System.Windows.Forms.ToolStripButton tsbtnModel14;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbtnMouseMode;
        private System.Windows.Forms.ToolStripTextBox tstxtAlpha;
        private System.Windows.Forms.ToolStripMenuItem mnOpenAndCopyStl;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Button btnInit2;
        private System.Windows.Forms.Button btnInit1;
        private System.Windows.Forms.Button btnInit0;
        private System.Windows.Forms.CheckBox chkEnMotor;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBaud;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtComPort;
        private System.Windows.Forms.TextBox txtBack_Pan;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtBack_Tilt;
        private System.Windows.Forms.TextBox txtBack_Swing;
        private System.Windows.Forms.Button btnPos_Front;
        private System.Windows.Forms.TextBox txtBack_X;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtBack_Y;
        private System.Windows.Forms.TextBox txtBack_Z;
        private System.Windows.Forms.Button btnPos_Right;
        private System.Windows.Forms.Button btnPos_Left;
        private System.Windows.Forms.Button btnPos_Bottom;
        private System.Windows.Forms.Button btnPos_Top;
        private System.Windows.Forms.Button btnCheckComport;
        private System.Windows.Forms.ToolStripMenuItem mn3D;
        private System.Windows.Forms.ToolStripMenuItem mnMotionTool;
    }
}