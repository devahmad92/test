using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LSE_BioBase4_CSharpSample
{
  /*!
  * \partial class  LightPanel
  * \brief map device's status LEDs in simulated status LEDs on UI
   * These UI status "LEDs" are displayed above the fingerprint's ImageBox
   * This class controls the size, number and color of the UI LEDs based on the _biobaseDevice_ObjectQuality event
  */
  public partial class LightPanel : UserControl
  {
    int _ledCount = 0;
    float _ratio = 1.67f;

    public LightPanel()
    {
      InitializeComponent();

      if (_ledCount == 0)
      {
        led1.Visible = false;
        led2.Visible = false;
        led3.Visible = false;
        led4.Visible = false;
        led5.Visible = false;
      }
    }


    /*!
     * \fn int LedCount()
     * \brief get/set the number of status LEDs displayed in UI. 
     * The size and location of the status LEDs are adjusted based on the number being displayed.
     * The number of LEDs can be between 0 and 5.
     */
    public int LedCount
    {
      get { return _ledCount; }
      set
      {
        if (_ledCount != value)
        {
          _ledCount = value;  //to hide LED set count = 0;
          SetLedLocation();
        }
        led1.LedColor = ActiveColor.gray;
        led2.LedColor = ActiveColor.gray;
        led3.LedColor = ActiveColor.gray;
        led4.LedColor = ActiveColor.gray;
        led5.LedColor = ActiveColor.gray;
      }
    }

    /*!
     * \fn int SetUILedColors()
     * \brief set the color of status LEDs displayed in UI.
     */
    public void SetUILedColors(ActiveColor indicator1, ActiveColor indicator2, ActiveColor indicator3, ActiveColor indicator4, ActiveColor indicator5)
    {
      led1.LedColor = indicator1;
      led2.LedColor = indicator2;
      led3.LedColor = indicator3;
      led4.LedColor = indicator4;
      led5.LedColor = indicator5;
      this.Invalidate();
      this.Update();
    }

    protected override void OnResize(EventArgs e)
    {
      SetLedLocation();
      base.OnResize(e);
    }

    /*!
     * \fn int SetLedLocation()
     * \brief set the size, spacing and location of the status LEDs displayed in UI.
     */
    void SetLedLocation()
    {
      if (_ledCount == 0)
      {
        led1.Visible = false;
        led2.Visible = false;
        led3.Visible = false;
        led4.Visible = false;
        led5.Visible = false;
      }
      else if (_ledCount == 1)
      {
        // one center LED
        led1.Visible = true;
        led2.Visible = false;
        led3.Visible = false;
        led4.Visible = false;
        led5.Visible = false;

        led1.Height = this.Height;
        led1.Width = (int)(this.Height * _ratio);
        led1.Top = 0;
        led1.Left = (this.Width - led1.Width) / 2;
        led1.Invalidate();
        led1.Update();
      }
      else if (_ledCount == 2)
      {
        // two leds
        led1.Height = this.Height;
        led1.Width = (int)(this.Height * _ratio);
        led2.Height = this.Height;
        led2.Width = (int)(this.Height * _ratio);
        led1.Top = 0;
        led2.Top = 0;
        int gap = (this.Width - led1.Width - led1.Width) / 3;
        if (gap > 0)
        {
          led1.Left = gap;
          led2.Left = gap + gap + led1.Width;
        }
        else
        {
          led1.Left = 0;
          led2.Left = this.Width - led2.Width;
        }
        led1.Visible = true;
        led2.Visible = true;
        led3.Visible = false;
        led4.Visible = false;
        led5.Visible = false;

        led1.Invalidate();
        led1.Update();
        led2.Invalidate();
        led2.Update();
      }
      else if (_ledCount == 4)
      {
        led1.Height = this.Height;
        led1.Width = (int)(this.Height * _ratio);
        led2.Height = this.Height;
        led2.Width = (int)(this.Height * _ratio);
        led3.Height = this.Height;
        led3.Width = (int)(this.Height * _ratio);
        led4.Height = this.Height;
        led4.Width = (int)(this.Height * _ratio);

        led1.Top = 0;
        led2.Top = 0;
        led3.Top = 0;
        led4.Top = 0;

        int gap = (this.Width - (led1.Width * 4)) / 5;
        if (gap > 0)
        {
          led1.Left = gap;
          led2.Left = (gap * 2) + led1.Width;
          led3.Left = (gap * 3) + led1.Width + led2.Width;
          led4.Left = (gap * 4) + led1.Width + led2.Width + led3.Width;
        }
        else
        {
          led1.Left = 0;
          led2.Left = this.Width - led2.Width;
          led3.Left = this.Width - led2.Width - led3.Width;
          led4.Left = this.Width - led2.Width - led3.Width - led4.Width;
        }

        led1.Visible = true;
        led2.Visible = true;
        led3.Visible = true;
        led4.Visible = true;
        led5.Visible = false;


        led1.Invalidate();
        led1.Update();
        led2.Invalidate();
        led2.Update();
        led3.Invalidate();
        led3.Update();
        led4.Invalidate();
        led4.Update();
      }
      else if (_ledCount == 5)
      {
        // support for 5th (upper palm) status UI "LED"
        led1.Height = this.Height;
        led1.Width = (int)(this.Height * _ratio);
        led2.Height = this.Height;
        led2.Width = (int)(this.Height * _ratio);
        led3.Height = this.Height;
        led3.Width = (int)(this.Height * _ratio);
        led4.Height = this.Height;
        led4.Width = (int)(this.Height * _ratio);
        led5.Height = this.Height;
        led5.Width = (int)(this.Height * _ratio);

        led1.Top = 0;
        led2.Top = 0;
        led3.Top = 0;
        led4.Top = 0;
        led5.Top = 0;

        int gap = (this.Width - (led1.Width * 5)) /6;
        if (gap > 0)
        {
          led1.Left = gap;
          led2.Left = (gap * 2) + led1.Width;
          led3.Left = (gap * 3) + led1.Width + led2.Width;
          led4.Left = (gap * 4) + led1.Width + led2.Width + led3.Width;
          led5.Left = (gap * 5) + led1.Width + led2.Width + led3.Width + led4.Width;
        }
        else
        {
          led1.Left = 0;
          led2.Left = this.Width - led2.Width;
          led3.Left = this.Width - led2.Width - led3.Width;
          led4.Left = this.Width - led2.Width - led3.Width - led4.Width;
          led5.Left = this.Width - led2.Width - led3.Width - led4.Width - led5.Left;
        }

        led1.Visible = true;
        led2.Visible = true;
        led3.Visible = true;
        led4.Visible = true;
        led5.Visible = true;


        led1.Invalidate();
        led1.Update();
        led2.Invalidate();
        led2.Update();
        led3.Invalidate();
        led3.Update();
        led4.Invalidate();
        led4.Update();
        led5.Invalidate();
        led5.Update();
      }
      else
        throw new Exception("_ledCount must be between 0 and 5");
    }
  }
}
