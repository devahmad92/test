namespace LSE_BioBase4_CSharpSample
{
  partial class UserControlDialog
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.okButton = new System.Windows.Forms.Button();
            this.groupBoxBeeper = new System.Windows.Forms.GroupBox();
            this.btnBeep = new System.Windows.Forms.Button();
            this.lblPattern = new System.Windows.Forms.Label();
            this.trackBarPattern = new System.Windows.Forms.TrackBar();
            this.LblVol = new System.Windows.Forms.Label();
            this.trackBarVolume = new System.Windows.Forms.TrackBar();
            this.groupBoxLedControls = new System.Windows.Forms.GroupBox();
            this.btnLED = new System.Windows.Forms.Button();
            this.lblNumber = new System.Windows.Forms.Label();
            this.trackBarLED = new System.Windows.Forms.TrackBar();
            this.groupBoxLEDType = new System.Windows.Forms.GroupBox();
            this.radioButtonStatus = new System.Windows.Forms.RadioButton();
            this.radioButtonIcon = new System.Windows.Forms.RadioButton();
            this.groupBoxLEDColor = new System.Windows.Forms.GroupBox();
            this.radioButtonYellow = new System.Windows.Forms.RadioButton();
            this.radioButtonGreen = new System.Windows.Forms.RadioButton();
            this.radioButtonRed = new System.Windows.Forms.RadioButton();
            this.groupBoxLedPattern = new System.Windows.Forms.GroupBox();
            this.radioButtonSteady = new System.Windows.Forms.RadioButton();
            this.radioButtonFlash = new System.Windows.Forms.RadioButton();
            this.radioButtonBlink = new System.Windows.Forms.RadioButton();
            this.groupBoxTouchScreen = new System.Windows.Forms.GroupBox();
            this.pictureBoxTouch = new System.Windows.Forms.PictureBox();
            this.btnTouchTest = new System.Windows.Forms.Button();
            this.groupBoxTFT = new System.Windows.Forms.GroupBox();
            this.btnEmulateLEDTest = new System.Windows.Forms.Button();
            this.btnTFTTest = new System.Windows.Forms.Button();
            this.groupBoxBeeper.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPattern)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).BeginInit();
            this.groupBoxLedControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarLED)).BeginInit();
            this.groupBoxLEDType.SuspendLayout();
            this.groupBoxLEDColor.SuspendLayout();
            this.groupBoxLedPattern.SuspendLayout();
            this.groupBoxTouchScreen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTouch)).BeginInit();
            this.groupBoxTFT.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Location = new System.Drawing.Point(29, 468);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(115, 60);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // groupBoxBeeper
            // 
            this.groupBoxBeeper.Controls.Add(this.btnBeep);
            this.groupBoxBeeper.Controls.Add(this.lblPattern);
            this.groupBoxBeeper.Controls.Add(this.trackBarPattern);
            this.groupBoxBeeper.Controls.Add(this.LblVol);
            this.groupBoxBeeper.Controls.Add(this.trackBarVolume);
            this.groupBoxBeeper.Location = new System.Drawing.Point(14, 24);
            this.groupBoxBeeper.Name = "groupBoxBeeper";
            this.groupBoxBeeper.Size = new System.Drawing.Size(150, 300);
            this.groupBoxBeeper.TabIndex = 0;
            this.groupBoxBeeper.TabStop = false;
            this.groupBoxBeeper.Text = "Beeper";
            // 
            // btnBeep
            // 
            this.btnBeep.Location = new System.Drawing.Point(15, 204);
            this.btnBeep.Name = "btnBeep";
            this.btnBeep.Size = new System.Drawing.Size(115, 60);
            this.btnBeep.TabIndex = 4;
            this.btnBeep.Text = "Beep";
            this.btnBeep.UseVisualStyleBackColor = true;
            this.btnBeep.Click += new System.EventHandler(this.btnBeep_Click);
            // 
            // lblPattern
            // 
            this.lblPattern.AutoSize = true;
            this.lblPattern.Location = new System.Drawing.Point(6, 28);
            this.lblPattern.Name = "lblPattern";
            this.lblPattern.Size = new System.Drawing.Size(52, 16);
            this.lblPattern.TabIndex = 0;
            this.lblPattern.Text = "Pattern:";
            // 
            // trackBarPattern
            // 
            this.trackBarPattern.LargeChange = 1;
            this.trackBarPattern.Location = new System.Drawing.Point(6, 51);
            this.trackBarPattern.Maximum = 7;
            this.trackBarPattern.Name = "trackBarPattern";
            this.trackBarPattern.Size = new System.Drawing.Size(135, 56);
            this.trackBarPattern.TabIndex = 1;
            // 
            // LblVol
            // 
            this.LblVol.AutoSize = true;
            this.LblVol.Location = new System.Drawing.Point(3, 111);
            this.LblVol.Name = "LblVol";
            this.LblVol.Size = new System.Drawing.Size(56, 16);
            this.LblVol.TabIndex = 2;
            this.LblVol.Text = "Volume:";
            // 
            // trackBarVolume
            // 
            this.trackBarVolume.Location = new System.Drawing.Point(6, 134);
            this.trackBarVolume.Maximum = 100;
            this.trackBarVolume.Name = "trackBarVolume";
            this.trackBarVolume.Size = new System.Drawing.Size(135, 56);
            this.trackBarVolume.TabIndex = 3;
            this.trackBarVolume.Value = 50;
            // 
            // groupBoxLedControls
            // 
            this.groupBoxLedControls.Controls.Add(this.btnLED);
            this.groupBoxLedControls.Controls.Add(this.lblNumber);
            this.groupBoxLedControls.Controls.Add(this.trackBarLED);
            this.groupBoxLedControls.Controls.Add(this.groupBoxLEDType);
            this.groupBoxLedControls.Controls.Add(this.groupBoxLEDColor);
            this.groupBoxLedControls.Controls.Add(this.groupBoxLedPattern);
            this.groupBoxLedControls.Location = new System.Drawing.Point(482, 24);
            this.groupBoxLedControls.Name = "groupBoxLedControls";
            this.groupBoxLedControls.Size = new System.Drawing.Size(150, 530);
            this.groupBoxLedControls.TabIndex = 1;
            this.groupBoxLedControls.TabStop = false;
            this.groupBoxLedControls.Text = "LED controls";
            // 
            // btnLED
            // 
            this.btnLED.Location = new System.Drawing.Point(21, 456);
            this.btnLED.Name = "btnLED";
            this.btnLED.Size = new System.Drawing.Size(115, 60);
            this.btnLED.TabIndex = 5;
            this.btnLED.Text = "Set LED";
            this.btnLED.UseVisualStyleBackColor = true;
            this.btnLED.Click += new System.EventHandler(this.btnLED_Click);
            // 
            // lblNumber
            // 
            this.lblNumber.AutoSize = true;
            this.lblNumber.Location = new System.Drawing.Point(6, 23);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(87, 16);
            this.lblNumber.TabIndex = 0;
            this.lblNumber.Text = "LED Number:";
            // 
            // trackBarLED
            // 
            this.trackBarLED.LargeChange = 1;
            this.trackBarLED.Location = new System.Drawing.Point(9, 40);
            this.trackBarLED.Maximum = 4;
            this.trackBarLED.Minimum = 1;
            this.trackBarLED.Name = "trackBarLED";
            this.trackBarLED.Size = new System.Drawing.Size(135, 56);
            this.trackBarLED.TabIndex = 1;
            this.trackBarLED.Value = 1;
            // 
            // groupBoxLEDType
            // 
            this.groupBoxLEDType.Controls.Add(this.radioButtonStatus);
            this.groupBoxLEDType.Controls.Add(this.radioButtonIcon);
            this.groupBoxLEDType.Location = new System.Drawing.Point(9, 99);
            this.groupBoxLEDType.Name = "groupBoxLEDType";
            this.groupBoxLEDType.Size = new System.Drawing.Size(135, 90);
            this.groupBoxLEDType.TabIndex = 2;
            this.groupBoxLEDType.TabStop = false;
            this.groupBoxLEDType.Text = "Type";
            // 
            // radioButtonStatus
            // 
            this.radioButtonStatus.AutoSize = true;
            this.radioButtonStatus.Location = new System.Drawing.Point(15, 54);
            this.radioButtonStatus.Name = "radioButtonStatus";
            this.radioButtonStatus.Size = new System.Drawing.Size(65, 20);
            this.radioButtonStatus.TabIndex = 1;
            this.radioButtonStatus.TabStop = true;
            this.radioButtonStatus.Text = "Status";
            this.radioButtonStatus.UseVisualStyleBackColor = true;
            // 
            // radioButtonIcon
            // 
            this.radioButtonIcon.AutoSize = true;
            this.radioButtonIcon.Location = new System.Drawing.Point(15, 28);
            this.radioButtonIcon.Name = "radioButtonIcon";
            this.radioButtonIcon.Size = new System.Drawing.Size(53, 20);
            this.radioButtonIcon.TabIndex = 0;
            this.radioButtonIcon.TabStop = true;
            this.radioButtonIcon.Text = "Icon";
            this.radioButtonIcon.UseVisualStyleBackColor = true;
            // 
            // groupBoxLEDColor
            // 
            this.groupBoxLEDColor.Controls.Add(this.radioButtonYellow);
            this.groupBoxLEDColor.Controls.Add(this.radioButtonGreen);
            this.groupBoxLEDColor.Controls.Add(this.radioButtonRed);
            this.groupBoxLEDColor.Location = new System.Drawing.Point(9, 192);
            this.groupBoxLEDColor.Name = "groupBoxLEDColor";
            this.groupBoxLEDColor.Size = new System.Drawing.Size(135, 125);
            this.groupBoxLEDColor.TabIndex = 3;
            this.groupBoxLEDColor.TabStop = false;
            this.groupBoxLEDColor.Text = "Color";
            // 
            // radioButtonYellow
            // 
            this.radioButtonYellow.AutoSize = true;
            this.radioButtonYellow.Location = new System.Drawing.Point(20, 81);
            this.radioButtonYellow.Name = "radioButtonYellow";
            this.radioButtonYellow.Size = new System.Drawing.Size(68, 20);
            this.radioButtonYellow.TabIndex = 2;
            this.radioButtonYellow.TabStop = true;
            this.radioButtonYellow.Text = "Yellow";
            this.radioButtonYellow.UseVisualStyleBackColor = true;
            // 
            // radioButtonGreen
            // 
            this.radioButtonGreen.AutoSize = true;
            this.radioButtonGreen.Location = new System.Drawing.Point(20, 55);
            this.radioButtonGreen.Name = "radioButtonGreen";
            this.radioButtonGreen.Size = new System.Drawing.Size(65, 20);
            this.radioButtonGreen.TabIndex = 1;
            this.radioButtonGreen.TabStop = true;
            this.radioButtonGreen.Text = "Green";
            this.radioButtonGreen.UseVisualStyleBackColor = true;
            // 
            // radioButtonRed
            // 
            this.radioButtonRed.AutoSize = true;
            this.radioButtonRed.Location = new System.Drawing.Point(20, 29);
            this.radioButtonRed.Name = "radioButtonRed";
            this.radioButtonRed.Size = new System.Drawing.Size(54, 20);
            this.radioButtonRed.TabIndex = 0;
            this.radioButtonRed.TabStop = true;
            this.radioButtonRed.Text = "Red";
            this.radioButtonRed.UseVisualStyleBackColor = true;
            // 
            // groupBoxLedPattern
            // 
            this.groupBoxLedPattern.Controls.Add(this.radioButtonSteady);
            this.groupBoxLedPattern.Controls.Add(this.radioButtonFlash);
            this.groupBoxLedPattern.Controls.Add(this.radioButtonBlink);
            this.groupBoxLedPattern.Location = new System.Drawing.Point(9, 320);
            this.groupBoxLedPattern.Name = "groupBoxLedPattern";
            this.groupBoxLedPattern.Size = new System.Drawing.Size(135, 125);
            this.groupBoxLedPattern.TabIndex = 4;
            this.groupBoxLedPattern.TabStop = false;
            this.groupBoxLedPattern.Text = "Pattern";
            // 
            // radioButtonSteady
            // 
            this.radioButtonSteady.AutoSize = true;
            this.radioButtonSteady.Location = new System.Drawing.Point(22, 86);
            this.radioButtonSteady.Name = "radioButtonSteady";
            this.radioButtonSteady.Size = new System.Drawing.Size(99, 20);
            this.radioButtonSteady.TabIndex = 2;
            this.radioButtonSteady.TabStop = true;
            this.radioButtonSteady.Text = "Nonblinking";
            this.radioButtonSteady.UseVisualStyleBackColor = true;
            // 
            // radioButtonFlash
            // 
            this.radioButtonFlash.AutoSize = true;
            this.radioButtonFlash.Location = new System.Drawing.Point(22, 59);
            this.radioButtonFlash.Name = "radioButtonFlash";
            this.radioButtonFlash.Size = new System.Drawing.Size(61, 20);
            this.radioButtonFlash.TabIndex = 1;
            this.radioButtonFlash.TabStop = true;
            this.radioButtonFlash.Text = "Flash";
            this.radioButtonFlash.UseVisualStyleBackColor = true;
            // 
            // radioButtonBlink
            // 
            this.radioButtonBlink.AutoSize = true;
            this.radioButtonBlink.Location = new System.Drawing.Point(22, 32);
            this.radioButtonBlink.Name = "radioButtonBlink";
            this.radioButtonBlink.Size = new System.Drawing.Size(57, 20);
            this.radioButtonBlink.TabIndex = 0;
            this.radioButtonBlink.TabStop = true;
            this.radioButtonBlink.Text = "Blink";
            this.radioButtonBlink.UseVisualStyleBackColor = true;
            // 
            // groupBoxTouchScreen
            // 
            this.groupBoxTouchScreen.Controls.Add(this.pictureBoxTouch);
            this.groupBoxTouchScreen.Controls.Add(this.btnTouchTest);
            this.groupBoxTouchScreen.Location = new System.Drawing.Point(326, 24);
            this.groupBoxTouchScreen.Name = "groupBoxTouchScreen";
            this.groupBoxTouchScreen.Size = new System.Drawing.Size(150, 201);
            this.groupBoxTouchScreen.TabIndex = 3;
            this.groupBoxTouchScreen.TabStop = false;
            this.groupBoxTouchScreen.Text = "Touch Screen";
            // 
            // pictureBoxTouch
            // 
            this.pictureBoxTouch.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxTouch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBoxTouch.Location = new System.Drawing.Point(7, 32);
            this.pictureBoxTouch.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBoxTouch.Name = "pictureBoxTouch";
            this.pictureBoxTouch.Size = new System.Drawing.Size(136, 64);
            this.pictureBoxTouch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxTouch.TabIndex = 31;
            this.pictureBoxTouch.TabStop = false;
            // 
            // btnTouchTest
            // 
            this.btnTouchTest.Location = new System.Drawing.Point(19, 107);
            this.btnTouchTest.Name = "btnTouchTest";
            this.btnTouchTest.Size = new System.Drawing.Size(115, 60);
            this.btnTouchTest.TabIndex = 6;
            this.btnTouchTest.Text = "Test";
            this.btnTouchTest.UseVisualStyleBackColor = true;
            this.btnTouchTest.Click += new System.EventHandler(this.btnTouchTest_Click);
            // 
            // groupBoxTFT
            // 
            this.groupBoxTFT.Controls.Add(this.btnEmulateLEDTest);
            this.groupBoxTFT.Controls.Add(this.btnTFTTest);
            this.groupBoxTFT.Location = new System.Drawing.Point(170, 24);
            this.groupBoxTFT.Name = "groupBoxTFT";
            this.groupBoxTFT.Size = new System.Drawing.Size(150, 201);
            this.groupBoxTFT.TabIndex = 4;
            this.groupBoxTFT.TabStop = false;
            this.groupBoxTFT.Text = "LScan TFT";
            // 
            // btnEmulateLEDTest
            // 
            this.btnEmulateLEDTest.Location = new System.Drawing.Point(16, 32);
            this.btnEmulateLEDTest.Name = "btnEmulateLEDTest";
            this.btnEmulateLEDTest.Size = new System.Drawing.Size(115, 60);
            this.btnEmulateLEDTest.TabIndex = 8;
            this.btnEmulateLEDTest.Text = "LED Test";
            this.btnEmulateLEDTest.UseVisualStyleBackColor = true;
            this.btnEmulateLEDTest.Click += new System.EventHandler(this.btnEmulateLEDTest_Click);
            // 
            // btnTFTTest
            // 
            this.btnTFTTest.Location = new System.Drawing.Point(16, 114);
            this.btnTFTTest.Name = "btnTFTTest";
            this.btnTFTTest.Size = new System.Drawing.Size(115, 60);
            this.btnTFTTest.TabIndex = 7;
            this.btnTFTTest.Text = "TFT Test";
            this.btnTFTTest.UseVisualStyleBackColor = true;
            this.btnTFTTest.Click += new System.EventHandler(this.btnTFTTest_Click);
            // 
            // UserControlDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(647, 569);
            this.Controls.Add(this.groupBoxTFT);
            this.Controls.Add(this.groupBoxTouchScreen);
            this.Controls.Add(this.groupBoxBeeper);
            this.Controls.Add(this.groupBoxLedControls);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserControlDialog";
            this.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "User Controls";
            this.groupBoxBeeper.ResumeLayout(false);
            this.groupBoxBeeper.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPattern)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).EndInit();
            this.groupBoxLedControls.ResumeLayout(false);
            this.groupBoxLedControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarLED)).EndInit();
            this.groupBoxLEDType.ResumeLayout(false);
            this.groupBoxLEDType.PerformLayout();
            this.groupBoxLEDColor.ResumeLayout(false);
            this.groupBoxLEDColor.PerformLayout();
            this.groupBoxLedPattern.ResumeLayout(false);
            this.groupBoxLedPattern.PerformLayout();
            this.groupBoxTouchScreen.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTouch)).EndInit();
            this.groupBoxTFT.ResumeLayout(false);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button okButton;

    private System.Windows.Forms.GroupBox groupBoxBeeper;
    private System.Windows.Forms.Button btnBeep;
    private System.Windows.Forms.Label lblPattern;
    private System.Windows.Forms.TrackBar trackBarPattern;
    private System.Windows.Forms.Label LblVol;
    private System.Windows.Forms.TrackBar trackBarVolume;

    private System.Windows.Forms.GroupBox groupBoxLedControls;
    private System.Windows.Forms.Button btnLED;
    private System.Windows.Forms.Label lblNumber;
    private System.Windows.Forms.TrackBar trackBarLED;
    private System.Windows.Forms.GroupBox groupBoxLEDType;
    private System.Windows.Forms.RadioButton radioButtonIcon;
    private System.Windows.Forms.RadioButton radioButtonStatus;
    private System.Windows.Forms.GroupBox groupBoxLEDColor;
    private System.Windows.Forms.RadioButton radioButtonRed;
    private System.Windows.Forms.RadioButton radioButtonGreen;
    private System.Windows.Forms.RadioButton radioButtonYellow;
    private System.Windows.Forms.GroupBox groupBoxLedPattern;
    private System.Windows.Forms.RadioButton radioButtonBlink;
    private System.Windows.Forms.RadioButton radioButtonFlash;
    private System.Windows.Forms.RadioButton radioButtonSteady;
    private System.Windows.Forms.GroupBox groupBoxTouchScreen;
    private System.Windows.Forms.Button btnTouchTest;
    private System.Windows.Forms.GroupBox groupBoxTFT;
    private System.Windows.Forms.Button btnTFTTest;
    private System.Windows.Forms.PictureBox pictureBoxTouch;
    private System.Windows.Forms.Button btnEmulateLEDTest;
  }
}
