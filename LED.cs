using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace LSE_BioBase4_CSharpSample
{
  /*!
  * \partial class  LED
  * \brief map device's status LEDs in GUI
  */
  public partial class LED : UserControl
  {
    Color _red1;
    Color _red2;
    Color _green1;
    Color _green2;
    Color _yellow1;
    Color _yellow2;

    ActiveColor _activeColor = ActiveColor.gray;

    public LED()
    {
      InitializeComponent();
      _red1 = Color.FromArgb(255, 0xEB, 0, 0);
      _red2 = Color.FromArgb(255, 0x9E, 0, 0);
      _green1 = Color.FromArgb(255, 0, 0x73, 0);
      _green2 = Color.FromArgb(255, 0x80, 0xBF, 0);
      _yellow1 = Color.FromArgb(255, 0xFF, 0xFF, 0x00);
      _yellow2 = Color.FromArgb(255, 0xFF, 0xFF, 0xCC);

    }
    public ActiveColor LedColor
    {
      get { return _activeColor; }
      set
      {
        if (_activeColor != value)
        {
          _activeColor = value;
          this.Invalidate();
          this.Update();
        }
      }
    }
    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);
      LinearGradientBrush brush = null;
      // draw one oval
      GraphicsPath path = new GraphicsPath();
      path.AddEllipse(0, 0, this.Width, this.Height);
      switch (_activeColor)
      {
        case ActiveColor.gray:
          brush = new LinearGradientBrush(new Point(0, 0), new Point(0, this.Height), Color.LightGray, Color.Gray);
          break;
        case ActiveColor.red:
          brush = new LinearGradientBrush(new Point(0, 0), new Point(0, this.Height), _red1, _red2);
          break;
        case ActiveColor.green:
          brush = new LinearGradientBrush(new Point(0, 0), new Point(0, this.Height), _green2, _green1);
          break;
        case ActiveColor.yellow:
          brush = new LinearGradientBrush(new Point(0, 0), new Point(0, this.Height), _yellow2, _yellow1);
          break;
      }
      if (brush != null)
      {
        e.Graphics.FillPath(brush, path);
      }
    }
  }
  public enum ActiveColor
  {
    red,
    green,
    gray,
    yellow
  }
}
