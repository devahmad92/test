namespace LSE_BioBase4_CSharpSample
{
  partial class LightPanel
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
      this.label1 = new System.Windows.Forms.Label();
      this.led1 = new LSE_BioBase4_CSharpSample.LED();
      this.led2 = new LSE_BioBase4_CSharpSample.LED();
      this.led3 = new LSE_BioBase4_CSharpSample.LED();
      this.led4 = new LSE_BioBase4_CSharpSample.LED();
      this.led5 = new LSE_BioBase4_CSharpSample.LED();
      this.SuspendLayout();
      // 
      // led1
      // 
      this.led1.LedColor = LSE_BioBase4_CSharpSample.ActiveColor.gray;
      this.led1.Location = new System.Drawing.Point(8, 4);
      this.led1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
      this.led1.Name = "led1";
      this.led1.Size = new System.Drawing.Size(113, 69);
      this.led1.TabIndex = 0;
      // 
      // led2
      // 
      this.led2.LedColor = LSE_BioBase4_CSharpSample.ActiveColor.gray;
      this.led2.Location = new System.Drawing.Point(126, 4);
      this.led2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
      this.led2.Name = "led2";
      this.led2.Size = new System.Drawing.Size(125, 69);
      this.led2.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(0, 5);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(0, 17);
      this.label1.TabIndex = 2;
      // 
      // led3
      // 
      this.led3.LedColor = LSE_BioBase4_CSharpSample.ActiveColor.gray;
      this.led3.Location = new System.Drawing.Point(257, 4);
      this.led3.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
      this.led3.Name = "led3";
      this.led3.Size = new System.Drawing.Size(113, 69);
      this.led3.TabIndex = 3;
      // 
      // led4
      // 
      this.led4.LedColor = LSE_BioBase4_CSharpSample.ActiveColor.gray;
      this.led4.Location = new System.Drawing.Point(376, 4);
      this.led4.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
      this.led4.Name = "led4";
      this.led4.Size = new System.Drawing.Size(125, 69);
      this.led4.TabIndex = 4;
      // 
      // led5
      // 
      this.led5.LedColor = LSE_BioBase4_CSharpSample.ActiveColor.gray;
      this.led5.Location = new System.Drawing.Point(507, 5);
      this.led5.Margin = new System.Windows.Forms.Padding(5);
      this.led5.Name = "led5";
      this.led5.Size = new System.Drawing.Size(125, 69);
      this.led5.TabIndex = 5;
      // 
      // LightPanel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.led5);
      this.Controls.Add(this.led4);
      this.Controls.Add(this.led3);
      this.Controls.Add(this.led2);
      this.Controls.Add(this.led1);
      this.Controls.Add(this.label1);
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.Name = "LightPanel";
      this.Size = new System.Drawing.Size(641, 76);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private LED led1;
    private LED led2;
    private LED led3;
    private LED led4;
    private LED led5;
  }
}
