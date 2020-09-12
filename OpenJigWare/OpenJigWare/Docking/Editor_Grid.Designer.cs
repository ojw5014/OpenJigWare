namespace OpenJigWare.Docking
{
    partial class frmGridEditor
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
            this.btnInitpos = new System.Windows.Forms.Button();
            this.chkFreeze_Swing = new System.Windows.Forms.CheckBox();
            this.chkFreeze_Z = new System.Windows.Forms.CheckBox();
            this.chkFreeze_Y = new System.Windows.Forms.CheckBox();
            this.chkFreeze_Pan = new System.Windows.Forms.CheckBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnSimul = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnMotionEnd = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtMotionCounter = new System.Windows.Forms.TextBox();
            this.btnEms = new System.Windows.Forms.Button();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.label128 = new System.Windows.Forms.Label();
            this.lbMotion_Counter = new System.Windows.Forms.Label();
            this.lbMotion_Status = new System.Windows.Forms.Label();
            this.label129 = new System.Windows.Forms.Label();
            this.btnConnect_Serial = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.label127 = new System.Windows.Forms.Label();
            this.lbMotion_Message = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtChangeValue = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.btnCmd_Clear = new System.Windows.Forms.Button();
            this.btnCmd_Sync = new System.Windows.Forms.Button();
            this.btnCmd_Repeat = new System.Windows.Forms.Button();
            this.btnZ_Input = new System.Windows.Forms.Button();
            this.btnZ_Minus = new System.Windows.Forms.Button();
            this.btnY_Input = new System.Windows.Forms.Button();
            this.btnX_Input = new System.Windows.Forms.Button();
            this.btnY_Minus = new System.Windows.Forms.Button();
            this.btnX_Minus = new System.Windows.Forms.Button();
            this.btnZ_Plus = new System.Windows.Forms.Button();
            this.btnY_Plus = new System.Windows.Forms.Button();
            this.txtPercent = new System.Windows.Forms.TextBox();
            this.btnPercent = new System.Windows.Forms.Button();
            this.btnX_Plus = new System.Windows.Forms.Button();
            this.btnValueIncrement = new System.Windows.Forms.Button();
            this.btnValueDecrement = new System.Windows.Forms.Button();
            this.btnValueStackIncrement = new System.Windows.Forms.Button();
            this.btnValueStackDecrement = new System.Windows.Forms.Button();
            this.btnFlip = new System.Windows.Forms.Button();
            this.btnValueMul = new System.Windows.Forms.Button();
            this.btnValueDiv = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnValueFlip = new System.Windows.Forms.Button();
            this.btnValueChange = new System.Windows.Forms.Button();
            this.btnInterpolation = new System.Windows.Forms.Button();
            this.btnInterpolation2 = new System.Windows.Forms.Button();
            this.btnGroupDel = new System.Windows.Forms.Button();
            this.btnGroup1 = new System.Windows.Forms.Button();
            this.btnGroup2 = new System.Windows.Forms.Button();
            this.btnGroup3 = new System.Windows.Forms.Button();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.btnDisplay_RememberPos = new System.Windows.Forms.Button();
            this.chkFreeze_Tilt = new System.Windows.Forms.CheckBox();
            this.btnInitpos2 = new System.Windows.Forms.Button();
            this.chkFreeze_X = new System.Windows.Forms.CheckBox();
            this.btnPos_TurnBack = new System.Windows.Forms.Button();
            this.btnPos_Front = new System.Windows.Forms.Button();
            this.txtBackAngle_X = new System.Windows.Forms.TextBox();
            this.txtSocket_Port = new System.Windows.Forms.TextBox();
            this.btnPos_Go = new System.Windows.Forms.Button();
            this.btnDisplay_GetThePose = new System.Windows.Forms.Button();
            this.btnPos_Left = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btnMotionFileOpen = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.txtBackAngle_Y = new System.Windows.Forms.TextBox();
            this.btnPos_Top = new System.Windows.Forms.Button();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtComment = new System.Windows.Forms.TextBox();
            this.btnPos_Bottom = new System.Windows.Forms.Button();
            this.btnBinarySave = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtBackAngle_Z = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnPos_Right = new System.Windows.Forms.Button();
            this.btnTextSave = new System.Windows.Forms.Button();
            this.btnMotion_Download2 = new System.Windows.Forms.Button();
            this.btnMotion_GetList2 = new System.Windows.Forms.Button();
            this.cmbBaud = new System.Windows.Forms.ComboBox();
            this.dgGrid = new System.Windows.Forms.DataGridView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.tmrRun = new System.Windows.Forms.Timer(this.components);
            this.chkSmooth = new System.Windows.Forms.CheckBox();
            this.lbModify = new System.Windows.Forms.Label();
            this.tmrBack = new System.Windows.Forms.Timer(this.components);
            this.chkSaveArduino = new System.Windows.Forms.CheckBox();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgGrid)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnInitpos
            // 
            this.btnInitpos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnInitpos.Location = new System.Drawing.Point(1141, 28);
            this.btnInitpos.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnInitpos.Name = "btnInitpos";
            this.btnInitpos.Size = new System.Drawing.Size(59, 74);
            this.btnInitpos.TabIndex = 738;
            this.btnInitpos.Text = "InitPos 1";
            this.btnInitpos.UseVisualStyleBackColor = false;
            // 
            // chkFreeze_Swing
            // 
            this.chkFreeze_Swing.AutoSize = true;
            this.chkFreeze_Swing.BackColor = System.Drawing.Color.Transparent;
            this.chkFreeze_Swing.Location = new System.Drawing.Point(1049, 64);
            this.chkFreeze_Swing.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeze_Swing.Name = "chkFreeze_Swing";
            this.chkFreeze_Swing.Size = new System.Drawing.Size(95, 19);
            this.chkFreeze_Swing.TabIndex = 725;
            this.chkFreeze_Swing.Text = "Freeze(S)";
            this.chkFreeze_Swing.UseVisualStyleBackColor = false;
            // 
            // chkFreeze_Z
            // 
            this.chkFreeze_Z.AutoSize = true;
            this.chkFreeze_Z.BackColor = System.Drawing.Color.Transparent;
            this.chkFreeze_Z.Location = new System.Drawing.Point(956, 64);
            this.chkFreeze_Z.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeze_Z.Name = "chkFreeze_Z";
            this.chkFreeze_Z.Size = new System.Drawing.Size(94, 19);
            this.chkFreeze_Z.TabIndex = 726;
            this.chkFreeze_Z.Text = "Freeze(Z)";
            this.chkFreeze_Z.UseVisualStyleBackColor = false;
            // 
            // chkFreeze_Y
            // 
            this.chkFreeze_Y.AutoSize = true;
            this.chkFreeze_Y.BackColor = System.Drawing.Color.Transparent;
            this.chkFreeze_Y.Location = new System.Drawing.Point(956, 47);
            this.chkFreeze_Y.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeze_Y.Name = "chkFreeze_Y";
            this.chkFreeze_Y.Size = new System.Drawing.Size(93, 19);
            this.chkFreeze_Y.TabIndex = 722;
            this.chkFreeze_Y.Text = "Freeze(Y)";
            this.chkFreeze_Y.UseVisualStyleBackColor = false;
            // 
            // chkFreeze_Pan
            // 
            this.chkFreeze_Pan.AutoSize = true;
            this.chkFreeze_Pan.BackColor = System.Drawing.Color.Transparent;
            this.chkFreeze_Pan.Location = new System.Drawing.Point(1049, 47);
            this.chkFreeze_Pan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeze_Pan.Name = "chkFreeze_Pan";
            this.chkFreeze_Pan.Size = new System.Drawing.Size(95, 19);
            this.chkFreeze_Pan.TabIndex = 727;
            this.chkFreeze_Pan.Text = "Freeze(P)";
            this.chkFreeze_Pan.UseVisualStyleBackColor = false;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(669, 38);
            this.btnStop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(93, 25);
            this.btnStop.TabIndex = 681;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // btnSimul
            // 
            this.btnSimul.Location = new System.Drawing.Point(576, 38);
            this.btnSimul.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSimul.Name = "btnSimul";
            this.btnSimul.Size = new System.Drawing.Size(93, 25);
            this.btnSimul.TabIndex = 682;
            this.btnSimul.Text = "Simul";
            this.btnSimul.UseVisualStyleBackColor = true;
            this.btnSimul.Click += new System.EventHandler(this.btnSimul_Click);
            // 
            // btnRun
            // 
            this.btnRun.Font = new System.Drawing.Font("굴림", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnRun.ForeColor = System.Drawing.Color.Blue;
            this.btnRun.Location = new System.Drawing.Point(762, 13);
            this.btnRun.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(91, 78);
            this.btnRun.TabIndex = 680;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnMotionEnd
            // 
            this.btnMotionEnd.Location = new System.Drawing.Point(669, 66);
            this.btnMotionEnd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnMotionEnd.Name = "btnMotionEnd";
            this.btnMotionEnd.Size = new System.Drawing.Size(93, 25);
            this.btnMotionEnd.TabIndex = 688;
            this.btnMotionEnd.Text = "MotionEnd";
            this.btnMotionEnd.UseVisualStyleBackColor = true;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(576, 66);
            this.btnReset.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(93, 25);
            this.btnReset.TabIndex = 689;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(166, 4);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(99, 50);
            this.btnConnect.TabIndex = 703;
            this.btnConnect.Text = "Connect [Wifi]";
            this.btnConnect.UseVisualStyleBackColor = true;
            // 
            // txtMotionCounter
            // 
            this.txtMotionCounter.Font = new System.Drawing.Font("굴림", 8F);
            this.txtMotionCounter.Location = new System.Drawing.Point(725, 13);
            this.txtMotionCounter.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtMotionCounter.Name = "txtMotionCounter";
            this.txtMotionCounter.Size = new System.Drawing.Size(37, 23);
            this.txtMotionCounter.TabIndex = 691;
            this.txtMotionCounter.Text = "1";
            // 
            // btnEms
            // 
            this.btnEms.BackColor = System.Drawing.Color.Red;
            this.btnEms.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnEms.ForeColor = System.Drawing.Color.Yellow;
            this.btnEms.Location = new System.Drawing.Point(854, 13);
            this.btnEms.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnEms.Name = "btnEms";
            this.btnEms.Size = new System.Drawing.Size(91, 77);
            this.btnEms.TabIndex = 690;
            this.btnEms.Text = "Ems";
            this.btnEms.UseVisualStyleBackColor = false;
            // 
            // txtIp
            // 
            this.txtIp.Location = new System.Drawing.Point(71, 4);
            this.txtIp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(93, 25);
            this.txtIp.TabIndex = 704;
            this.txtIp.Text = "192.168.1.100";
            // 
            // label128
            // 
            this.label128.BackColor = System.Drawing.Color.Brown;
            this.label128.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label128.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label128.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label128.ForeColor = System.Drawing.Color.Cyan;
            this.label128.Location = new System.Drawing.Point(285, 754);
            this.label128.Name = "label128";
            this.label128.Size = new System.Drawing.Size(75, 20);
            this.label128.TabIndex = 696;
            this.label128.Text = "Message";
            this.label128.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMotion_Counter
            // 
            this.lbMotion_Counter.BackColor = System.Drawing.Color.Gainsboro;
            this.lbMotion_Counter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbMotion_Counter.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbMotion_Counter.Location = new System.Drawing.Point(228, 754);
            this.lbMotion_Counter.Name = "lbMotion_Counter";
            this.lbMotion_Counter.Size = new System.Drawing.Size(56, 20);
            this.lbMotion_Counter.TabIndex = 698;
            this.lbMotion_Counter.Text = "0";
            this.lbMotion_Counter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMotion_Status
            // 
            this.lbMotion_Status.BackColor = System.Drawing.Color.Gainsboro;
            this.lbMotion_Status.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbMotion_Status.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbMotion_Status.Location = new System.Drawing.Point(85, 754);
            this.lbMotion_Status.Name = "lbMotion_Status";
            this.lbMotion_Status.Size = new System.Drawing.Size(74, 20);
            this.lbMotion_Status.TabIndex = 702;
            this.lbMotion_Status.Text = "Ready";
            this.lbMotion_Status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label129
            // 
            this.label129.BackColor = System.Drawing.Color.Brown;
            this.label129.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label129.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label129.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label129.ForeColor = System.Drawing.Color.Cyan;
            this.label129.Location = new System.Drawing.Point(159, 754);
            this.label129.Name = "label129";
            this.label129.Size = new System.Drawing.Size(67, 20);
            this.label129.TabIndex = 695;
            this.label129.Text = "Counter";
            this.label129.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnConnect_Serial
            // 
            this.btnConnect_Serial.Location = new System.Drawing.Point(166, 4);
            this.btnConnect_Serial.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnConnect_Serial.Name = "btnConnect_Serial";
            this.btnConnect_Serial.Size = new System.Drawing.Size(99, 50);
            this.btnConnect_Serial.TabIndex = 697;
            this.btnConnect_Serial.Text = "Connect";
            this.btnConnect_Serial.UseVisualStyleBackColor = true;
            this.btnConnect_Serial.Click += new System.EventHandler(this.btnConnect_Serial_Click);
            // 
            // label12
            // 
            this.label12.BackColor = System.Drawing.Color.Brown;
            this.label12.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label12.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label12.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label12.ForeColor = System.Drawing.Color.Cyan;
            this.label12.Location = new System.Drawing.Point(576, 13);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(147, 24);
            this.label12.TabIndex = 692;
            this.label12.Text = "반복횟수(0-무한대)";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label127
            // 
            this.label127.BackColor = System.Drawing.Color.Brown;
            this.label127.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label127.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label127.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label127.ForeColor = System.Drawing.Color.Cyan;
            this.label127.Location = new System.Drawing.Point(8, 754);
            this.label127.Name = "label127";
            this.label127.Size = new System.Drawing.Size(75, 20);
            this.label127.TabIndex = 693;
            this.label127.Text = "Status";
            this.label127.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMotion_Message
            // 
            this.lbMotion_Message.BackColor = System.Drawing.Color.Gainsboro;
            this.lbMotion_Message.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbMotion_Message.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbMotion_Message.Location = new System.Drawing.Point(362, 754);
            this.lbMotion_Message.Name = "lbMotion_Message";
            this.lbMotion_Message.Size = new System.Drawing.Size(199, 20);
            this.lbMotion_Message.TabIndex = 694;
            this.lbMotion_Message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.DarkGray;
            this.panel2.Controls.Add(this.txtChangeValue);
            this.panel2.Controls.Add(this.label17);
            this.panel2.Controls.Add(this.btnCmd_Clear);
            this.panel2.Controls.Add(this.btnCmd_Sync);
            this.panel2.Controls.Add(this.btnCmd_Repeat);
            this.panel2.Controls.Add(this.btnZ_Input);
            this.panel2.Controls.Add(this.btnZ_Minus);
            this.panel2.Controls.Add(this.btnY_Input);
            this.panel2.Controls.Add(this.btnX_Input);
            this.panel2.Controls.Add(this.btnY_Minus);
            this.panel2.Controls.Add(this.btnX_Minus);
            this.panel2.Controls.Add(this.btnZ_Plus);
            this.panel2.Controls.Add(this.btnY_Plus);
            this.panel2.Controls.Add(this.txtPercent);
            this.panel2.Controls.Add(this.btnPercent);
            this.panel2.Controls.Add(this.btnX_Plus);
            this.panel2.Controls.Add(this.btnValueIncrement);
            this.panel2.Controls.Add(this.btnValueDecrement);
            this.panel2.Controls.Add(this.btnValueStackIncrement);
            this.panel2.Controls.Add(this.btnValueStackDecrement);
            this.panel2.Controls.Add(this.btnFlip);
            this.panel2.Controls.Add(this.btnValueMul);
            this.panel2.Controls.Add(this.btnValueDiv);
            this.panel2.Controls.Add(this.btnClear);
            this.panel2.Controls.Add(this.btnValueFlip);
            this.panel2.Controls.Add(this.btnValueChange);
            this.panel2.Controls.Add(this.btnInterpolation);
            this.panel2.Controls.Add(this.btnInterpolation2);
            this.panel2.Controls.Add(this.btnGroupDel);
            this.panel2.Controls.Add(this.btnGroup1);
            this.panel2.Controls.Add(this.btnGroup2);
            this.panel2.Controls.Add(this.btnGroup3);
            this.panel2.Location = new System.Drawing.Point(11, 94);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(670, 76);
            this.panel2.TabIndex = 687;
            // 
            // txtChangeValue
            // 
            this.txtChangeValue.BackColor = System.Drawing.Color.MistyRose;
            this.txtChangeValue.Font = new System.Drawing.Font("굴림체", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtChangeValue.Location = new System.Drawing.Point(24, 1);
            this.txtChangeValue.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtChangeValue.Name = "txtChangeValue";
            this.txtChangeValue.Size = new System.Drawing.Size(56, 23);
            this.txtChangeValue.TabIndex = 420;
            this.txtChangeValue.Text = "0";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.BackColor = System.Drawing.Color.Transparent;
            this.label17.Font = new System.Drawing.Font("굴림체", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label17.Location = new System.Drawing.Point(0, 8);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(28, 14);
            this.label17.TabIndex = 421;
            this.label17.Text = "Val";
            // 
            // btnCmd_Clear
            // 
            this.btnCmd_Clear.BackColor = System.Drawing.Color.Transparent;
            this.btnCmd_Clear.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCmd_Clear.Location = new System.Drawing.Point(595, 4);
            this.btnCmd_Clear.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnCmd_Clear.Name = "btnCmd_Clear";
            this.btnCmd_Clear.Size = new System.Drawing.Size(69, 48);
            this.btnCmd_Clear.TabIndex = 678;
            this.btnCmd_Clear.Text = "Remove Cmd";
            this.btnCmd_Clear.UseVisualStyleBackColor = false;
            // 
            // btnCmd_Sync
            // 
            this.btnCmd_Sync.BackColor = System.Drawing.Color.Turquoise;
            this.btnCmd_Sync.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCmd_Sync.Location = new System.Drawing.Point(528, 26);
            this.btnCmd_Sync.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnCmd_Sync.Name = "btnCmd_Sync";
            this.btnCmd_Sync.Size = new System.Drawing.Size(69, 25);
            this.btnCmd_Sync.TabIndex = 676;
            this.btnCmd_Sync.Text = "Sync";
            this.btnCmd_Sync.UseVisualStyleBackColor = false;
            // 
            // btnCmd_Repeat
            // 
            this.btnCmd_Repeat.BackColor = System.Drawing.Color.MediumPurple;
            this.btnCmd_Repeat.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCmd_Repeat.ForeColor = System.Drawing.Color.White;
            this.btnCmd_Repeat.Location = new System.Drawing.Point(528, 4);
            this.btnCmd_Repeat.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnCmd_Repeat.Name = "btnCmd_Repeat";
            this.btnCmd_Repeat.Size = new System.Drawing.Size(69, 25);
            this.btnCmd_Repeat.TabIndex = 677;
            this.btnCmd_Repeat.Text = "Repeaat";
            this.btnCmd_Repeat.UseVisualStyleBackColor = false;
            this.btnCmd_Repeat.Click += new System.EventHandler(this.btnCmd_Repeat_Click);
            // 
            // btnZ_Input
            // 
            this.btnZ_Input.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnZ_Input.Location = new System.Drawing.Point(351, 48);
            this.btnZ_Input.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnZ_Input.Name = "btnZ_Input";
            this.btnZ_Input.Size = new System.Drawing.Size(32, 24);
            this.btnZ_Input.TabIndex = 670;
            this.btnZ_Input.Text = "Z";
            this.btnZ_Input.UseVisualStyleBackColor = true;
            this.btnZ_Input.Click += new System.EventHandler(this.btnZ_Input_Click);
            // 
            // btnZ_Minus
            // 
            this.btnZ_Minus.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnZ_Minus.Location = new System.Drawing.Point(351, 24);
            this.btnZ_Minus.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnZ_Minus.Name = "btnZ_Minus";
            this.btnZ_Minus.Size = new System.Drawing.Size(32, 24);
            this.btnZ_Minus.TabIndex = 670;
            this.btnZ_Minus.Text = "Z-";
            this.btnZ_Minus.UseVisualStyleBackColor = true;
            this.btnZ_Minus.Click += new System.EventHandler(this.btnZ_Minus_Click);
            // 
            // btnY_Input
            // 
            this.btnY_Input.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnY_Input.Location = new System.Drawing.Point(322, 48);
            this.btnY_Input.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnY_Input.Name = "btnY_Input";
            this.btnY_Input.Size = new System.Drawing.Size(32, 24);
            this.btnY_Input.TabIndex = 669;
            this.btnY_Input.Text = "Y";
            this.btnY_Input.UseVisualStyleBackColor = true;
            this.btnY_Input.Click += new System.EventHandler(this.btnY_Input_Click);
            // 
            // btnX_Input
            // 
            this.btnX_Input.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnX_Input.Location = new System.Drawing.Point(292, 48);
            this.btnX_Input.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnX_Input.Name = "btnX_Input";
            this.btnX_Input.Size = new System.Drawing.Size(32, 24);
            this.btnX_Input.TabIndex = 668;
            this.btnX_Input.Text = "X";
            this.btnX_Input.UseVisualStyleBackColor = true;
            this.btnX_Input.Click += new System.EventHandler(this.btnX_Input_Click);
            // 
            // btnY_Minus
            // 
            this.btnY_Minus.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnY_Minus.Location = new System.Drawing.Point(322, 24);
            this.btnY_Minus.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnY_Minus.Name = "btnY_Minus";
            this.btnY_Minus.Size = new System.Drawing.Size(32, 24);
            this.btnY_Minus.TabIndex = 669;
            this.btnY_Minus.Text = "Y-";
            this.btnY_Minus.UseVisualStyleBackColor = true;
            this.btnY_Minus.Click += new System.EventHandler(this.btnY_Minus_Click);
            // 
            // btnX_Minus
            // 
            this.btnX_Minus.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnX_Minus.Location = new System.Drawing.Point(292, 24);
            this.btnX_Minus.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnX_Minus.Name = "btnX_Minus";
            this.btnX_Minus.Size = new System.Drawing.Size(32, 24);
            this.btnX_Minus.TabIndex = 668;
            this.btnX_Minus.Text = "X-";
            this.btnX_Minus.UseVisualStyleBackColor = true;
            this.btnX_Minus.Click += new System.EventHandler(this.btnX_Minus_Click);
            // 
            // btnZ_Plus
            // 
            this.btnZ_Plus.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnZ_Plus.Location = new System.Drawing.Point(351, 1);
            this.btnZ_Plus.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnZ_Plus.Name = "btnZ_Plus";
            this.btnZ_Plus.Size = new System.Drawing.Size(32, 23);
            this.btnZ_Plus.TabIndex = 667;
            this.btnZ_Plus.Text = "Z+";
            this.btnZ_Plus.UseVisualStyleBackColor = true;
            this.btnZ_Plus.Click += new System.EventHandler(this.btnZ_Plus_Click);
            // 
            // btnY_Plus
            // 
            this.btnY_Plus.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnY_Plus.Location = new System.Drawing.Point(322, 1);
            this.btnY_Plus.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnY_Plus.Name = "btnY_Plus";
            this.btnY_Plus.Size = new System.Drawing.Size(32, 23);
            this.btnY_Plus.TabIndex = 666;
            this.btnY_Plus.Text = "Y+";
            this.btnY_Plus.UseVisualStyleBackColor = true;
            this.btnY_Plus.Click += new System.EventHandler(this.btnY_Plus_Click);
            // 
            // txtPercent
            // 
            this.txtPercent.Font = new System.Drawing.Font("굴림체", 8.25F);
            this.txtPercent.Location = new System.Drawing.Point(82, 1);
            this.txtPercent.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtPercent.Name = "txtPercent";
            this.txtPercent.Size = new System.Drawing.Size(34, 23);
            this.txtPercent.TabIndex = 665;
            this.txtPercent.Text = "70";
            // 
            // btnPercent
            // 
            this.btnPercent.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnPercent.ForeColor = System.Drawing.Color.Green;
            this.btnPercent.Location = new System.Drawing.Point(117, 1);
            this.btnPercent.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnPercent.Name = "btnPercent";
            this.btnPercent.Size = new System.Drawing.Size(32, 23);
            this.btnPercent.TabIndex = 664;
            this.btnPercent.Text = "%";
            this.btnPercent.UseVisualStyleBackColor = true;
            this.btnPercent.Click += new System.EventHandler(this.btnPercent_Click);
            // 
            // btnX_Plus
            // 
            this.btnX_Plus.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnX_Plus.Location = new System.Drawing.Point(292, 1);
            this.btnX_Plus.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnX_Plus.Name = "btnX_Plus";
            this.btnX_Plus.Size = new System.Drawing.Size(32, 23);
            this.btnX_Plus.TabIndex = 423;
            this.btnX_Plus.Text = "X+";
            this.btnX_Plus.UseVisualStyleBackColor = true;
            this.btnX_Plus.Click += new System.EventHandler(this.btnX_Plus_Click);
            // 
            // btnValueIncrement
            // 
            this.btnValueIncrement.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnValueIncrement.Location = new System.Drawing.Point(2, 24);
            this.btnValueIncrement.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnValueIncrement.Name = "btnValueIncrement";
            this.btnValueIncrement.Size = new System.Drawing.Size(24, 24);
            this.btnValueIncrement.TabIndex = 423;
            this.btnValueIncrement.Text = "+";
            this.btnValueIncrement.UseVisualStyleBackColor = true;
            this.btnValueIncrement.Click += new System.EventHandler(this.btnValueIncrement_Click);
            // 
            // btnValueDecrement
            // 
            this.btnValueDecrement.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnValueDecrement.Location = new System.Drawing.Point(24, 24);
            this.btnValueDecrement.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnValueDecrement.Name = "btnValueDecrement";
            this.btnValueDecrement.Size = new System.Drawing.Size(24, 24);
            this.btnValueDecrement.TabIndex = 424;
            this.btnValueDecrement.Text = "-";
            this.btnValueDecrement.UseVisualStyleBackColor = true;
            this.btnValueDecrement.Click += new System.EventHandler(this.btnValueDecrement_Click);
            // 
            // btnValueStackIncrement
            // 
            this.btnValueStackIncrement.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnValueStackIncrement.Location = new System.Drawing.Point(150, 1);
            this.btnValueStackIncrement.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnValueStackIncrement.Name = "btnValueStackIncrement";
            this.btnValueStackIncrement.Size = new System.Drawing.Size(69, 23);
            this.btnValueStackIncrement.TabIndex = 427;
            this.btnValueStackIncrement.Text = "Inc";
            this.btnValueStackIncrement.UseVisualStyleBackColor = true;
            this.btnValueStackIncrement.Click += new System.EventHandler(this.btnValueStackIncrement_Click);
            // 
            // btnValueStackDecrement
            // 
            this.btnValueStackDecrement.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnValueStackDecrement.Location = new System.Drawing.Point(217, 1);
            this.btnValueStackDecrement.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnValueStackDecrement.Name = "btnValueStackDecrement";
            this.btnValueStackDecrement.Size = new System.Drawing.Size(69, 23);
            this.btnValueStackDecrement.TabIndex = 428;
            this.btnValueStackDecrement.Text = "Dec";
            this.btnValueStackDecrement.UseVisualStyleBackColor = true;
            this.btnValueStackDecrement.Click += new System.EventHandler(this.btnValueStackDecrement_Click);
            // 
            // btnFlip
            // 
            this.btnFlip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnFlip.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFlip.Location = new System.Drawing.Point(88, 24);
            this.btnFlip.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnFlip.Name = "btnFlip";
            this.btnFlip.Size = new System.Drawing.Size(61, 24);
            this.btnFlip.TabIndex = 431;
            this.btnFlip.Text = "Mirror";
            this.btnFlip.UseVisualStyleBackColor = false;
            this.btnFlip.Click += new System.EventHandler(this.btnFlip_Click);
            // 
            // btnValueMul
            // 
            this.btnValueMul.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnValueMul.Location = new System.Drawing.Point(46, 24);
            this.btnValueMul.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnValueMul.Name = "btnValueMul";
            this.btnValueMul.Size = new System.Drawing.Size(24, 24);
            this.btnValueMul.TabIndex = 432;
            this.btnValueMul.Text = "*";
            this.btnValueMul.UseVisualStyleBackColor = true;
            this.btnValueMul.Click += new System.EventHandler(this.btnValueMul_Click);
            // 
            // btnValueDiv
            // 
            this.btnValueDiv.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnValueDiv.Location = new System.Drawing.Point(67, 24);
            this.btnValueDiv.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnValueDiv.Name = "btnValueDiv";
            this.btnValueDiv.Size = new System.Drawing.Size(24, 24);
            this.btnValueDiv.TabIndex = 433;
            this.btnValueDiv.Text = "/";
            this.btnValueDiv.UseVisualStyleBackColor = true;
            this.btnValueDiv.Click += new System.EventHandler(this.btnValueDiv_Click);
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.Cyan;
            this.btnClear.Location = new System.Drawing.Point(2, 48);
            this.btnClear.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(147, 25);
            this.btnClear.TabIndex = 419;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnValueFlip
            // 
            this.btnValueFlip.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnValueFlip.Location = new System.Drawing.Point(217, 24);
            this.btnValueFlip.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnValueFlip.Name = "btnValueFlip";
            this.btnValueFlip.Size = new System.Drawing.Size(69, 24);
            this.btnValueFlip.TabIndex = 426;
            this.btnValueFlip.Text = "Flip";
            this.btnValueFlip.UseVisualStyleBackColor = true;
            this.btnValueFlip.Click += new System.EventHandler(this.btnValueFlip_Click);
            // 
            // btnValueChange
            // 
            this.btnValueChange.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnValueChange.Location = new System.Drawing.Point(150, 24);
            this.btnValueChange.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnValueChange.Name = "btnValueChange";
            this.btnValueChange.Size = new System.Drawing.Size(69, 24);
            this.btnValueChange.TabIndex = 422;
            this.btnValueChange.Text = "Change";
            this.btnValueChange.UseVisualStyleBackColor = true;
            this.btnValueChange.Click += new System.EventHandler(this.btnValueChange_Click);
            // 
            // btnInterpolation
            // 
            this.btnInterpolation.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInterpolation.Location = new System.Drawing.Point(150, 48);
            this.btnInterpolation.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnInterpolation.Name = "btnInterpolation";
            this.btnInterpolation.Size = new System.Drawing.Size(69, 24);
            this.btnInterpolation.TabIndex = 429;
            this.btnInterpolation.Text = "Curve";
            this.btnInterpolation.UseVisualStyleBackColor = true;
            this.btnInterpolation.Click += new System.EventHandler(this.btnInterpolation_Click);
            // 
            // btnInterpolation2
            // 
            this.btnInterpolation2.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInterpolation2.Location = new System.Drawing.Point(217, 48);
            this.btnInterpolation2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnInterpolation2.Name = "btnInterpolation2";
            this.btnInterpolation2.Size = new System.Drawing.Size(69, 24);
            this.btnInterpolation2.TabIndex = 430;
            this.btnInterpolation2.Text = "S-Curve";
            this.btnInterpolation2.UseVisualStyleBackColor = true;
            this.btnInterpolation2.Click += new System.EventHandler(this.btnInterpolation2_Click);
            // 
            // btnGroupDel
            // 
            this.btnGroupDel.BackColor = System.Drawing.Color.Transparent;
            this.btnGroupDel.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGroupDel.Location = new System.Drawing.Point(453, 3);
            this.btnGroupDel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnGroupDel.Name = "btnGroupDel";
            this.btnGroupDel.Size = new System.Drawing.Size(69, 70);
            this.btnGroupDel.TabIndex = 450;
            this.btnGroupDel.Text = "Remove Group";
            this.btnGroupDel.UseVisualStyleBackColor = false;
            this.btnGroupDel.Click += new System.EventHandler(this.btnGroupDel_Click);
            // 
            // btnGroup1
            // 
            this.btnGroup1.BackColor = System.Drawing.Color.Turquoise;
            this.btnGroup1.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGroup1.Location = new System.Drawing.Point(386, 3);
            this.btnGroup1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnGroup1.Name = "btnGroup1";
            this.btnGroup1.Size = new System.Drawing.Size(69, 25);
            this.btnGroup1.TabIndex = 448;
            this.btnGroup1.Text = "Group1";
            this.btnGroup1.UseVisualStyleBackColor = false;
            this.btnGroup1.Click += new System.EventHandler(this.btnGroup1_Click);
            // 
            // btnGroup2
            // 
            this.btnGroup2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.btnGroup2.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGroup2.Location = new System.Drawing.Point(386, 25);
            this.btnGroup2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnGroup2.Name = "btnGroup2";
            this.btnGroup2.Size = new System.Drawing.Size(69, 25);
            this.btnGroup2.TabIndex = 452;
            this.btnGroup2.Text = "Group2";
            this.btnGroup2.UseVisualStyleBackColor = false;
            this.btnGroup2.Click += new System.EventHandler(this.btnGroup2_Click);
            // 
            // btnGroup3
            // 
            this.btnGroup3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnGroup3.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGroup3.Location = new System.Drawing.Point(386, 48);
            this.btnGroup3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnGroup3.Name = "btnGroup3";
            this.btnGroup3.Size = new System.Drawing.Size(69, 25);
            this.btnGroup3.TabIndex = 451;
            this.btnGroup3.Text = "Group3";
            this.btnGroup3.UseVisualStyleBackColor = false;
            this.btnGroup3.Click += new System.EventHandler(this.btnGroup3_Click);
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(71, 4);
            this.txtPort.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(93, 25);
            this.txtPort.TabIndex = 699;
            this.txtPort.Text = "1";
            // 
            // btnDisplay_RememberPos
            // 
            this.btnDisplay_RememberPos.BackColor = System.Drawing.Color.Turquoise;
            this.btnDisplay_RememberPos.Font = new System.Drawing.Font("굴림", 8F);
            this.btnDisplay_RememberPos.Location = new System.Drawing.Point(1021, 83);
            this.btnDisplay_RememberPos.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnDisplay_RememberPos.Name = "btnDisplay_RememberPos";
            this.btnDisplay_RememberPos.Size = new System.Drawing.Size(58, 24);
            this.btnDisplay_RememberPos.TabIndex = 732;
            this.btnDisplay_RememberPos.Text = "Set";
            this.btnDisplay_RememberPos.UseVisualStyleBackColor = false;
            // 
            // chkFreeze_Tilt
            // 
            this.chkFreeze_Tilt.AutoSize = true;
            this.chkFreeze_Tilt.BackColor = System.Drawing.Color.Transparent;
            this.chkFreeze_Tilt.Location = new System.Drawing.Point(1049, 30);
            this.chkFreeze_Tilt.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeze_Tilt.Name = "chkFreeze_Tilt";
            this.chkFreeze_Tilt.Size = new System.Drawing.Size(93, 19);
            this.chkFreeze_Tilt.TabIndex = 723;
            this.chkFreeze_Tilt.Text = "Freeze(T)";
            this.chkFreeze_Tilt.UseVisualStyleBackColor = false;
            // 
            // btnInitpos2
            // 
            this.btnInitpos2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnInitpos2.Location = new System.Drawing.Point(1141, 101);
            this.btnInitpos2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnInitpos2.Name = "btnInitpos2";
            this.btnInitpos2.Size = new System.Drawing.Size(59, 74);
            this.btnInitpos2.TabIndex = 737;
            this.btnInitpos2.Text = "InitPos 2";
            this.btnInitpos2.UseVisualStyleBackColor = false;
            // 
            // chkFreeze_X
            // 
            this.chkFreeze_X.AutoSize = true;
            this.chkFreeze_X.BackColor = System.Drawing.Color.Transparent;
            this.chkFreeze_X.Location = new System.Drawing.Point(956, 30);
            this.chkFreeze_X.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeze_X.Name = "chkFreeze_X";
            this.chkFreeze_X.Size = new System.Drawing.Size(94, 19);
            this.chkFreeze_X.TabIndex = 724;
            this.chkFreeze_X.Text = "Freeze(X)";
            this.chkFreeze_X.UseVisualStyleBackColor = false;
            // 
            // btnPos_TurnBack
            // 
            this.btnPos_TurnBack.BackColor = System.Drawing.Color.Turquoise;
            this.btnPos_TurnBack.Font = new System.Drawing.Font("굴림", 8F);
            this.btnPos_TurnBack.Location = new System.Drawing.Point(1079, 151);
            this.btnPos_TurnBack.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPos_TurnBack.Name = "btnPos_TurnBack";
            this.btnPos_TurnBack.Size = new System.Drawing.Size(58, 24);
            this.btnPos_TurnBack.TabIndex = 728;
            this.btnPos_TurnBack.Text = "←";
            this.btnPos_TurnBack.UseVisualStyleBackColor = false;
            // 
            // btnPos_Front
            // 
            this.btnPos_Front.BackColor = System.Drawing.Color.Turquoise;
            this.btnPos_Front.Font = new System.Drawing.Font("굴림", 8F);
            this.btnPos_Front.Location = new System.Drawing.Point(1021, 106);
            this.btnPos_Front.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPos_Front.Name = "btnPos_Front";
            this.btnPos_Front.Size = new System.Drawing.Size(58, 24);
            this.btnPos_Front.TabIndex = 730;
            this.btnPos_Front.Text = "Front";
            this.btnPos_Front.UseVisualStyleBackColor = false;
            // 
            // txtBackAngle_X
            // 
            this.txtBackAngle_X.Location = new System.Drawing.Point(965, 84);
            this.txtBackAngle_X.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBackAngle_X.Name = "txtBackAngle_X";
            this.txtBackAngle_X.Size = new System.Drawing.Size(31, 25);
            this.txtBackAngle_X.TabIndex = 683;
            this.txtBackAngle_X.Text = "0";
            // 
            // txtSocket_Port
            // 
            this.txtSocket_Port.Location = new System.Drawing.Point(71, 29);
            this.txtSocket_Port.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtSocket_Port.Name = "txtSocket_Port";
            this.txtSocket_Port.Size = new System.Drawing.Size(93, 25);
            this.txtSocket_Port.TabIndex = 700;
            this.txtSocket_Port.Text = "5002";
            // 
            // btnPos_Go
            // 
            this.btnPos_Go.BackColor = System.Drawing.Color.Turquoise;
            this.btnPos_Go.Location = new System.Drawing.Point(999, 83);
            this.btnPos_Go.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPos_Go.Name = "btnPos_Go";
            this.btnPos_Go.Size = new System.Drawing.Size(24, 90);
            this.btnPos_Go.TabIndex = 686;
            this.btnPos_Go.Text = "Go";
            this.btnPos_Go.UseVisualStyleBackColor = false;
            // 
            // btnDisplay_GetThePose
            // 
            this.btnDisplay_GetThePose.BackColor = System.Drawing.Color.Turquoise;
            this.btnDisplay_GetThePose.Font = new System.Drawing.Font("굴림", 8F);
            this.btnDisplay_GetThePose.Location = new System.Drawing.Point(1079, 83);
            this.btnDisplay_GetThePose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnDisplay_GetThePose.Name = "btnDisplay_GetThePose";
            this.btnDisplay_GetThePose.Size = new System.Drawing.Size(58, 24);
            this.btnDisplay_GetThePose.TabIndex = 735;
            this.btnDisplay_GetThePose.Text = "Get";
            this.btnDisplay_GetThePose.UseVisualStyleBackColor = false;
            // 
            // btnPos_Left
            // 
            this.btnPos_Left.BackColor = System.Drawing.Color.Turquoise;
            this.btnPos_Left.Font = new System.Drawing.Font("굴림", 8F);
            this.btnPos_Left.Location = new System.Drawing.Point(1079, 128);
            this.btnPos_Left.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPos_Left.Name = "btnPos_Left";
            this.btnPos_Left.Size = new System.Drawing.Size(58, 24);
            this.btnPos_Left.TabIndex = 733;
            this.btnPos_Left.Text = "Left";
            this.btnPos_Left.UseVisualStyleBackColor = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Font = new System.Drawing.Font("굴림체", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label7.ForeColor = System.Drawing.Color.Black;
            this.label7.Location = new System.Drawing.Point(953, 91);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(14, 14);
            this.label7.TabIndex = 679;
            this.label7.Text = "X";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Location = new System.Drawing.Point(6, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 15);
            this.label4.TabIndex = 719;
            this.label4.Text = "Baudrate";
            // 
            // label10
            // 
            this.label10.BackColor = System.Drawing.Color.Gainsboro;
            this.label10.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label10.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label10.Location = new System.Drawing.Point(956, 6);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(67, 22);
            this.label10.TabIndex = 709;
            this.label10.Text = "Comment";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Font = new System.Drawing.Font("굴림체", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label8.ForeColor = System.Drawing.Color.Black;
            this.label8.Location = new System.Drawing.Point(953, 121);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(14, 14);
            this.label8.TabIndex = 678;
            this.label8.Text = "Y";
            // 
            // btnMotionFileOpen
            // 
            this.btnMotionFileOpen.Location = new System.Drawing.Point(477, 58);
            this.btnMotionFileOpen.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnMotionFileOpen.Name = "btnMotionFileOpen";
            this.btnMotionFileOpen.Size = new System.Drawing.Size(93, 25);
            this.btnMotionFileOpen.TabIndex = 714;
            this.btnMotionFileOpen.Text = "Open";
            this.btnMotionFileOpen.UseVisualStyleBackColor = true;
            this.btnMotionFileOpen.Click += new System.EventHandler(this.btnMotionFileOpen_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.BackColor = System.Drawing.Color.Transparent;
            this.label13.Font = new System.Drawing.Font("굴림체", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label13.ForeColor = System.Drawing.Color.Black;
            this.label13.Location = new System.Drawing.Point(953, 152);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(14, 14);
            this.label13.TabIndex = 677;
            this.label13.Text = "Z";
            // 
            // txtBackAngle_Y
            // 
            this.txtBackAngle_Y.Location = new System.Drawing.Point(965, 114);
            this.txtBackAngle_Y.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBackAngle_Y.Name = "txtBackAngle_Y";
            this.txtBackAngle_Y.Size = new System.Drawing.Size(31, 25);
            this.txtBackAngle_Y.TabIndex = 684;
            this.txtBackAngle_Y.Text = "0";
            // 
            // btnPos_Top
            // 
            this.btnPos_Top.BackColor = System.Drawing.Color.Turquoise;
            this.btnPos_Top.Font = new System.Drawing.Font("굴림", 8F);
            this.btnPos_Top.Location = new System.Drawing.Point(1021, 151);
            this.btnPos_Top.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPos_Top.Name = "btnPos_Top";
            this.btnPos_Top.Size = new System.Drawing.Size(58, 24);
            this.btnPos_Top.TabIndex = 731;
            this.btnPos_Top.Text = "Top";
            this.btnPos_Top.UseVisualStyleBackColor = false;
            // 
            // txtFileName
            // 
            this.txtFileName.Font = new System.Drawing.Font("굴림", 8F);
            this.txtFileName.Location = new System.Drawing.Point(284, 30);
            this.txtFileName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(286, 23);
            this.txtFileName.TabIndex = 717;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Enabled = false;
            this.label5.Location = new System.Drawing.Point(28, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(20, 15);
            this.label5.TabIndex = 721;
            this.label5.Text = "IP";
            // 
            // txtComment
            // 
            this.txtComment.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtComment.Location = new System.Drawing.Point(1025, 3);
            this.txtComment.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtComment.Multiline = true;
            this.txtComment.Name = "txtComment";
            this.txtComment.Size = new System.Drawing.Size(174, 25);
            this.txtComment.TabIndex = 711;
            // 
            // btnPos_Bottom
            // 
            this.btnPos_Bottom.BackColor = System.Drawing.Color.Turquoise;
            this.btnPos_Bottom.Font = new System.Drawing.Font("굴림", 8F);
            this.btnPos_Bottom.Location = new System.Drawing.Point(1021, 128);
            this.btnPos_Bottom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPos_Bottom.Name = "btnPos_Bottom";
            this.btnPos_Bottom.Size = new System.Drawing.Size(58, 24);
            this.btnPos_Bottom.TabIndex = 729;
            this.btnPos_Bottom.Text = "Bottom";
            this.btnPos_Bottom.UseVisualStyleBackColor = false;
            // 
            // btnBinarySave
            // 
            this.btnBinarySave.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnBinarySave.Location = new System.Drawing.Point(381, 58);
            this.btnBinarySave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnBinarySave.Name = "btnBinarySave";
            this.btnBinarySave.Size = new System.Drawing.Size(93, 25);
            this.btnBinarySave.TabIndex = 715;
            this.btnBinarySave.Text = "Save(압축)";
            this.btnBinarySave.UseVisualStyleBackColor = true;
            this.btnBinarySave.Click += new System.EventHandler(this.btnBinarySave_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(7, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 15);
            this.label3.TabIndex = 718;
            this.label3.Text = "Comport";
            // 
            // txtBackAngle_Z
            // 
            this.txtBackAngle_Z.Location = new System.Drawing.Point(965, 147);
            this.txtBackAngle_Z.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBackAngle_Z.Name = "txtBackAngle_Z";
            this.txtBackAngle_Z.Size = new System.Drawing.Size(31, 25);
            this.txtBackAngle_Z.TabIndex = 685;
            this.txtBackAngle_Z.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Enabled = false;
            this.label6.Location = new System.Drawing.Point(21, 34);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 15);
            this.label6.TabIndex = 720;
            this.label6.Text = "Port";
            // 
            // btnPos_Right
            // 
            this.btnPos_Right.BackColor = System.Drawing.Color.Turquoise;
            this.btnPos_Right.Font = new System.Drawing.Font("굴림", 8F);
            this.btnPos_Right.Location = new System.Drawing.Point(1079, 106);
            this.btnPos_Right.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPos_Right.Name = "btnPos_Right";
            this.btnPos_Right.Size = new System.Drawing.Size(58, 24);
            this.btnPos_Right.TabIndex = 734;
            this.btnPos_Right.Text = "Right";
            this.btnPos_Right.UseVisualStyleBackColor = false;
            // 
            // btnTextSave
            // 
            this.btnTextSave.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnTextSave.Location = new System.Drawing.Point(284, 58);
            this.btnTextSave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnTextSave.Name = "btnTextSave";
            this.btnTextSave.Size = new System.Drawing.Size(93, 25);
            this.btnTextSave.TabIndex = 716;
            this.btnTextSave.Text = "Save";
            this.btnTextSave.UseVisualStyleBackColor = true;
            this.btnTextSave.Click += new System.EventHandler(this.btnTextSave_Click);
            // 
            // btnMotion_Download2
            // 
            this.btnMotion_Download2.BackgroundImage = global::OpenJigWare.Properties.Resources.download;
            this.btnMotion_Download2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnMotion_Download2.Location = new System.Drawing.Point(875, 100);
            this.btnMotion_Download2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnMotion_Download2.Name = "btnMotion_Download2";
            this.btnMotion_Download2.Size = new System.Drawing.Size(38, 40);
            this.btnMotion_Download2.TabIndex = 676;
            this.btnMotion_Download2.UseVisualStyleBackColor = true;
            // 
            // btnMotion_GetList2
            // 
            this.btnMotion_GetList2.BackgroundImage = global::OpenJigWare.Properties.Resources.search;
            this.btnMotion_GetList2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnMotion_GetList2.Location = new System.Drawing.Point(831, 100);
            this.btnMotion_GetList2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnMotion_GetList2.Name = "btnMotion_GetList2";
            this.btnMotion_GetList2.Size = new System.Drawing.Size(38, 40);
            this.btnMotion_GetList2.TabIndex = 675;
            this.btnMotion_GetList2.UseVisualStyleBackColor = true;
            // 
            // cmbBaud
            // 
            this.cmbBaud.FormattingEnabled = true;
            this.cmbBaud.Items.AddRange(new object[] {
            "9600",
            "19200",
            "38400",
            "57600",
            "115200",
            "1000000",
            "2000000",
            "3000000",
            "4000000",
            "4500000"});
            this.cmbBaud.Location = new System.Drawing.Point(71, 30);
            this.cmbBaud.Name = "cmbBaud";
            this.cmbBaud.Size = new System.Drawing.Size(93, 23);
            this.cmbBaud.TabIndex = 741;
            // 
            // dgGrid
            // 
            this.dgGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgGrid.Location = new System.Drawing.Point(14, 175);
            this.dgGrid.Name = "dgGrid";
            this.dgGrid.RowTemplate.Height = 27;
            this.dgGrid.Size = new System.Drawing.Size(1188, 497);
            this.dgGrid.TabIndex = 742;
            this.dgGrid.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgGrid_CellEnter);
            this.dgGrid.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgGrid_CellMouseDoubleClick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(6, 5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(276, 86);
            this.tabControl1.TabIndex = 743;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnConnect_Serial);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.cmbBaud);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.txtPort);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(268, 57);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnConnect);
            this.tabPage2.Controls.Add(this.txtIp);
            this.tabPage2.Controls.Add(this.txtSocket_Port);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(268, 57);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("굴림체", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(283, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 14);
            this.label1.TabIndex = 677;
            this.label1.Text = "File";
            // 
            // tmrRun
            // 
            this.tmrRun.Interval = 10;
            this.tmrRun.Tick += new System.EventHandler(this.tmrRun_Tick);
            // 
            // chkSmooth
            // 
            this.chkSmooth.AutoSize = true;
            this.chkSmooth.BackColor = System.Drawing.Color.Transparent;
            this.chkSmooth.Checked = true;
            this.chkSmooth.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSmooth.Font = new System.Drawing.Font("굴림", 8F, System.Drawing.FontStyle.Bold);
            this.chkSmooth.Location = new System.Drawing.Point(190, 9);
            this.chkSmooth.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkSmooth.Name = "chkSmooth";
            this.chkSmooth.Size = new System.Drawing.Size(86, 18);
            this.chkSmooth.TabIndex = 744;
            this.chkSmooth.Text = "Smooth";
            this.chkSmooth.UseVisualStyleBackColor = false;
            // 
            // lbModify
            // 
            this.lbModify.AutoSize = true;
            this.lbModify.BackColor = System.Drawing.Color.Transparent;
            this.lbModify.Font = new System.Drawing.Font("굴림체", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbModify.ForeColor = System.Drawing.Color.Black;
            this.lbModify.Location = new System.Drawing.Point(712, 152);
            this.lbModify.Name = "lbModify";
            this.lbModify.Size = new System.Drawing.Size(35, 14);
            this.lbModify.TabIndex = 677;
            this.lbModify.Text = "File";
            // 
            // tmrBack
            // 
            this.tmrBack.Interval = 1000;
            this.tmrBack.Tick += new System.EventHandler(this.tmrBack_Tick);
            // 
            // chkSaveArduino
            // 
            this.chkSaveArduino.AutoSize = true;
            this.chkSaveArduino.Location = new System.Drawing.Point(687, 94);
            this.chkSaveArduino.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkSaveArduino.Name = "chkSaveArduino";
            this.chkSaveArduino.Size = new System.Drawing.Size(117, 19);
            this.chkSaveArduino.TabIndex = 745;
            this.chkSaveArduino.Text = "Save Arduino";
            this.chkSaveArduino.UseVisualStyleBackColor = true;
            // 
            // frmGridEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1209, 783);
            this.Controls.Add(this.chkSaveArduino);
            this.Controls.Add(this.chkSmooth);
            this.Controls.Add(this.txtBackAngle_X);
            this.Controls.Add(this.txtBackAngle_Y);
            this.Controls.Add(this.txtBackAngle_Z);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.dgGrid);
            this.Controls.Add(this.btnInitpos);
            this.Controls.Add(this.chkFreeze_Swing);
            this.Controls.Add(this.chkFreeze_Z);
            this.Controls.Add(this.chkFreeze_Y);
            this.Controls.Add(this.chkFreeze_Pan);
            this.Controls.Add(this.btnMotion_Download2);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnSimul);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnMotion_GetList2);
            this.Controls.Add(this.btnMotionEnd);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.txtMotionCounter);
            this.Controls.Add(this.btnEms);
            this.Controls.Add(this.label128);
            this.Controls.Add(this.lbMotion_Counter);
            this.Controls.Add(this.lbMotion_Status);
            this.Controls.Add(this.label129);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label127);
            this.Controls.Add(this.lbMotion_Message);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.btnDisplay_RememberPos);
            this.Controls.Add(this.chkFreeze_Tilt);
            this.Controls.Add(this.btnInitpos2);
            this.Controls.Add(this.chkFreeze_X);
            this.Controls.Add(this.btnPos_TurnBack);
            this.Controls.Add(this.btnPos_Front);
            this.Controls.Add(this.btnPos_Go);
            this.Controls.Add(this.btnDisplay_GetThePose);
            this.Controls.Add(this.btnPos_Left);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.btnMotionFileOpen);
            this.Controls.Add(this.lbModify);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.btnPos_Top);
            this.Controls.Add(this.txtFileName);
            this.Controls.Add(this.txtComment);
            this.Controls.Add(this.btnPos_Bottom);
            this.Controls.Add(this.btnBinarySave);
            this.Controls.Add(this.btnPos_Right);
            this.Controls.Add(this.btnTextSave);
            this.Name = "frmGridEditor";
            this.Text = "Editor_Grid";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmGridEditor_FormClosing);
            this.Load += new System.EventHandler(this.frmGridEditor_Load);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgGrid)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnInitpos;
        private System.Windows.Forms.CheckBox chkFreeze_Swing;
        private System.Windows.Forms.CheckBox chkFreeze_Z;
        private System.Windows.Forms.CheckBox chkFreeze_Y;
        private System.Windows.Forms.CheckBox chkFreeze_Pan;
        private System.Windows.Forms.Button btnMotion_Download2;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnSimul;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnMotion_GetList2;
        private System.Windows.Forms.Button btnMotionEnd;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtMotionCounter;
        private System.Windows.Forms.Button btnEms;
        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.Label label128;
        private System.Windows.Forms.Label lbMotion_Counter;
        private System.Windows.Forms.Label lbMotion_Status;
        private System.Windows.Forms.Label label129;
        private System.Windows.Forms.Button btnConnect_Serial;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label127;
        private System.Windows.Forms.Label lbMotion_Message;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox txtChangeValue;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Button btnCmd_Clear;
        private System.Windows.Forms.Button btnCmd_Sync;
        private System.Windows.Forms.Button btnCmd_Repeat;
        private System.Windows.Forms.Button btnZ_Input;
        private System.Windows.Forms.Button btnZ_Minus;
        private System.Windows.Forms.Button btnY_Input;
        private System.Windows.Forms.Button btnX_Input;
        private System.Windows.Forms.Button btnY_Minus;
        private System.Windows.Forms.Button btnX_Minus;
        private System.Windows.Forms.Button btnZ_Plus;
        private System.Windows.Forms.Button btnY_Plus;
        private System.Windows.Forms.TextBox txtPercent;
        private System.Windows.Forms.Button btnPercent;
        private System.Windows.Forms.Button btnX_Plus;
        private System.Windows.Forms.Button btnValueIncrement;
        private System.Windows.Forms.Button btnValueDecrement;
        private System.Windows.Forms.Button btnValueStackIncrement;
        private System.Windows.Forms.Button btnValueStackDecrement;
        private System.Windows.Forms.Button btnFlip;
        private System.Windows.Forms.Button btnValueMul;
        private System.Windows.Forms.Button btnValueDiv;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnValueFlip;
        private System.Windows.Forms.Button btnValueChange;
        private System.Windows.Forms.Button btnInterpolation;
        private System.Windows.Forms.Button btnInterpolation2;
        private System.Windows.Forms.Button btnGroupDel;
        private System.Windows.Forms.Button btnGroup1;
        private System.Windows.Forms.Button btnGroup2;
        private System.Windows.Forms.Button btnGroup3;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Button btnDisplay_RememberPos;
        private System.Windows.Forms.CheckBox chkFreeze_Tilt;
        private System.Windows.Forms.Button btnInitpos2;
        private System.Windows.Forms.CheckBox chkFreeze_X;
        private System.Windows.Forms.Button btnPos_TurnBack;
        private System.Windows.Forms.Button btnPos_Front;
        private System.Windows.Forms.TextBox txtBackAngle_X;
        private System.Windows.Forms.TextBox txtSocket_Port;
        private System.Windows.Forms.Button btnPos_Go;
        private System.Windows.Forms.Button btnDisplay_GetThePose;
        private System.Windows.Forms.Button btnPos_Left;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnMotionFileOpen;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtBackAngle_Y;
        private System.Windows.Forms.Button btnPos_Top;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.Button btnPos_Bottom;
        private System.Windows.Forms.Button btnBinarySave;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtBackAngle_Z;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnPos_Right;
        private System.Windows.Forms.Button btnTextSave;
        private System.Windows.Forms.ComboBox cmbBaud;
        private System.Windows.Forms.DataGridView dgGrid;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer tmrRun;
        private System.Windows.Forms.CheckBox chkSmooth;
        private System.Windows.Forms.Label lbModify;
        private System.Windows.Forms.Timer tmrBack;
        private System.Windows.Forms.CheckBox chkSaveArduino;
    }
}