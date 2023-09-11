using Crossmatch.BioBaseApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;       // for Sleep
using System.Windows.Forms;
using System.Runtime.InteropServices; // for Marshal
using System.Xml;   // needed to parse XML in application
using System.IO;


namespace LSE_BioBase4_CSharpSample
{
  public partial class UserControlDialog : Form //UserControl
  {
    IBioBaseDevice _biobaseDevice;
    public UserControlDialog(IBioBaseDevice biobaseDevice)
    {
      _biobaseDevice = biobaseDevice;
      InitializeComponent();

      // Disable beeper if there is not valid open device
      if (_biobaseDevice == null)
        this.groupBoxBeeper.Enabled = false;
      // Disable LSE check if there is not valid open device or not the Guardian FireWire, Guardian USB, Guardian R, Patrol, etc.
      if ((_biobaseDevice == null) || (_biobaseDevice.deviceGuidanceType != guidanceType.guidanceTypeStatusLED))
        this.groupBoxLedControls.Enabled = false;
      // Disable touch screen test if there is not valid open device or not the Guardian 300, Guardian 200 or Guardian 100
      if ((_biobaseDevice == null) || (_biobaseDevice.deviceGuidanceType != guidanceType.guidanceTypeTouchDisplay))
        this.groupBoxTouchScreen.Enabled = false;
      // Disable TFT screen test if there is not valid open device or not the LScan 1000, LScan 1000PX, LScan 500P, LScan 500
      if ((_biobaseDevice == null) || ((_biobaseDevice.deviceGuidanceType != guidanceType.guidanceTypeTFT) &&
                                       (_biobaseDevice.deviceGuidanceType != guidanceType.guidanceTypeTFT_1000)))
        this.groupBoxTFT.Enabled = false;
      if (_biobaseDevice != null) {
        BioBaseDeviceInfo _deviceInfo = _biobaseDevice.DeviceInfo;
        string _deviceType = _deviceInfo.ModelName;
        if (_deviceType == LSEConst.DEVICE_TYPE_GUARDIAN_45)
            this.groupBoxBeeper.Enabled = false;
      }
    }

    /*!
     * \fn private void btnBeep_Click()
     * \brief Read UI Beeper pattern and volume setting before calling _Beep to create XML for SetOutputData
     * 
     */
    private void btnBeep_Click(object sender, EventArgs e)
    {
      try
      {
        _Beep(trackBarPattern.Value.ToString(), trackBarVolume.Value.ToString());
      }
      catch (BioBaseException ex)
      {
        string msg = string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_BEEPER, ex.Message);
        DialogResult result = MessageBox.Show(msg, "BioBase error", MessageBoxButtons.OK);
      }
      catch (Exception ex)
      {
        string msg = string.Format("{0} with {1} exception {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_BEEPER, ex.Message);
        DialogResult result = MessageBox.Show(msg, "Exception", MessageBoxButtons.OK);
      }
    }

    /*!
     * \fn private void btnLED_Click()
     * \brief Read UI LED settings before creating XML for SetOutputData
     * 
     * Based on slider and radio buttons, the various XML Values for the KeyValuePair are created. The 
     * values are defined in the PropertyConstants class but are in the format "I1_GREEN_B1", "S2_RED_B2", "S4_GREEN_B2", etc.
     * 
     * These tests are valid for Guardian FireWire, Guardian USB, Guardian R, Guardian R2, Patrol, Patrol ID, etc.
     * 
     */
    private void btnLED_Click(object sender, EventArgs e)
    {
      try
      {
        bool bTurnOff = false;    // If no LED options are selected, all LEDs will be turn doff
        string ledoutput1 = "", ledoutput1A = "", ledoutput2 = "", ledoutput2A = "";

        // Format "LED string" for the LED being turned on...
        // All "Icon" LEDs start with the letter I. All "Status" LEDs start with the letter S.
        if (radioButtonIcon.Checked == true)
          ledoutput1 = "I";
        else if (radioButtonStatus.Checked == true)
          ledoutput1 = "S";
        else bTurnOff = true;

        // Add LED number to "LED string"
        ledoutput1 += trackBarLED.Value.ToString();

        // Add color to "LED string"
        // Yellow requires two "LED strings". When red and green LED is enabled, we will get yellow
        if (radioButtonRed.Checked == true)
          ledoutput1 += "_RED";
        else if (radioButtonGreen.Checked == true)
          ledoutput1 += "_GREEN";
        else if (radioButtonYellow.Checked == true)
        {
          ledoutput1A = ledoutput1;
          ledoutput1 += "_RED";
          ledoutput1A += "_GREEN";
        }
        else bTurnOff = true;

        // Add blink (phase 0) and/or flash (phase 1) rate to "LED string"
        // Steady on requires two "LED strings". When B1 and B2 are enabled, we will get steady 
        if (radioButtonBlink.Checked == true)
        {
          ledoutput1 += "_B1";
          if (ledoutput1A != "") ledoutput1A += "_B1";  //Needed for Yellow
        }
        else if (radioButtonFlash.Checked == true)
        {
          ledoutput1 += "_B2";
          if (ledoutput1A != "") ledoutput1A += "_B2";  //Needed for Yellow
        }
        else if (radioButtonSteady.Checked == true)
        {
          ledoutput2 = ledoutput1;
          ledoutput2A = ledoutput1A;
          ledoutput1 += "_B1";
          if (ledoutput1A != "") ledoutput1A += "_B1";  //Needed for Yellow
          if (ledoutput2 != "") ledoutput2 += "_B2";
          if (ledoutput2A != "") ledoutput2A += "_B2";  //Needed for Yellow
        }
        else bTurnOff = true;

        XmlDocument xmlDoc = new XmlDocument();
        _SetupXMLOutputHeader(xmlDoc);

        XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
        xmlDoc.DocumentElement.AppendChild(outputDataElem);

        XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_STATUSLEDS);
        outputDataElem.AppendChild(statusDataElem);

        // turn off all status LEDs
        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_NONE);

        if (bTurnOff == false)
        {
          // Add KeyValuePair element for each valid "LED string"
          // a red or green LED with single blinking phase will only have one valid string to be added
          // a yellow LED with single blinking phase will require two valid string to be added
          // a yellow LED with steady on will require alll four valid string to be added
          if (ledoutput1 != "")
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, ledoutput1);
          if (ledoutput1A != "")
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, ledoutput1A);
          if (ledoutput2 != "")
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, ledoutput2);
          if (ledoutput2A != "")
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, ledoutput2A);
        }
        _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      }
      catch (BioBaseException ex)
      {
        string msg = string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message);
        DialogResult result = MessageBox.Show(msg, "BioBase error", MessageBoxButtons.OK);
      }
      catch (Exception ex)
      {
        string msg = string.Format("{0} with {1} exception {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message);
        DialogResult result = MessageBox.Show(msg, "Exception", MessageBoxButtons.OK);
      }
    }

    /*!
     * \fn private void btnTouchTest_Click()
     * \brief Read all the template files and creat XML for each file before calling SendTouchTest to create XML for SetOutputData
     * 
     * These tests are valid for Guardian, Guardian 300 displays and well as the Guardian 200 and Guardian 100 LED emulator displays.
     * 
     */
    private void btnTouchTest_Click(object sender, EventArgs e)
    {
      Cursor Saved = this.Cursor;
      try
      {
        this.Cursor = Cursors.WaitCursor;
        _biobaseDevice.ScannerUserOutput += new EventHandler<BioBaseUserOutputEventArgs>(_biobaseDevice_ScannerUserOutput);

        // Assume templates folder is in same location as the application binary
        string TouchDisplayTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "Templates\\";

        // Note: using _biobaseDevice TouchDisplayInitialTemplate and TouchDisplayInitialExternalParameter variable to setup 
        // string that are used by the SendTouchTest() method.
        // Could have used local string and Dictionary and pass them as parameters to the SendTouchTest() method.
        _biobaseDevice.TouchDisplayInitialExternalParameters.Clear();

        //Loop through each HTML file in Templates folder
        DirectoryInfo d = new DirectoryInfo(@TouchDisplayTemplatePath);
        FileInfo[] Files = d.GetFiles("*.html"); //Getting html files only
        foreach (FileInfo file in Files)
        {
          // Setup string with proper file name!
          _biobaseDevice.TouchDisplayInitialTemplate = "file:///" + TouchDisplayTemplatePath + file.Name;

          _biobaseDevice.TouchDisplayInitialExternalParameters.Clear(); // clear out all external html parameters

          if ((_biobaseDevice.TouchDisplayInitialTemplate.Contains("index_initial_right.html")) ||
              (_biobaseDevice.TouchDisplayInitialTemplate.Contains("index_standard_right.html")))
            {
            // If right hand, we setup the external parameters to not display each of the fingers on the right hand
            _biobaseDevice.TouchDisplayInitialExternalParameters.Clear();
            _biobaseDevice.TouchDisplayInitialExternalParameters["FP2"] = "0";  // remove right index finger (Right Thumb is FP1)
            _biobaseDevice.TouchDisplayInitialExternalParameters["FP3"] = "0";  // remove right middle finger
            _biobaseDevice.TouchDisplayInitialExternalParameters["FP4"] = "0";  // remove right ring finger
            _biobaseDevice.TouchDisplayInitialExternalParameters["FP5"] = "0";  // remove right litel finger
            // Loop through each finger on the right hand and add it to the animated image
            for (int i = 2; i < 6; i++)
            {
              string finger = string.Format("FP{0}", i);
              _biobaseDevice.TouchDisplayInitialExternalParameters[finger] = "1";  // show an additional finger
              SendTouchTest();
              Thread.Sleep(2000);
            }
          }
          else if ((_biobaseDevice.TouchDisplayInitialTemplate.Contains("index_initial_left.html")) ||
              (_biobaseDevice.TouchDisplayInitialTemplate.Contains("index_standard_left.html")))
          {
            _biobaseDevice.TouchDisplayInitialExternalParameters.Clear();
            _biobaseDevice.TouchDisplayInitialExternalParameters["FP7"] = "0";  // remove finger
            _biobaseDevice.TouchDisplayInitialExternalParameters["FP8"] = "0";  // remove finger
            _biobaseDevice.TouchDisplayInitialExternalParameters["FP9"] = "0";  // remove finger
            _biobaseDevice.TouchDisplayInitialExternalParameters["FP10"] = "0";  // remove finger
            for (int i = 7; i < 11; i++)
            {
              string finger = string.Format("FP{0}", i);
              _biobaseDevice.TouchDisplayInitialExternalParameters[finger] = "1";  // show an additional finger
              SendTouchTest();
              Thread.Sleep(2000);
            }
          }
          else if ((_biobaseDevice.TouchDisplayInitialTemplate.Contains("index_initial_thumbs.html")) ||
              (_biobaseDevice.TouchDisplayInitialTemplate.Contains("index_standard_thumbs.html")))
          {
            _biobaseDevice.TouchDisplayInitialExternalParameters.Clear();
            _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP1", "1");  // show right thumb
            _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP6", "1");  // show left thumb
            SendTouchTest();
            Thread.Sleep(2000);
          }
          else if (_biobaseDevice.TouchDisplayInitialTemplate.Contains("index_roll.html"))
          {
            _biobaseDevice.TouchDisplayInitialExternalParameters.Clear();
            _biobaseDevice.TouchDisplayInitialExternalParameters["HP1"] = "1";
            for (int i = 1; i < 6; i++)
            {
              string finger = string.Format("FP{0}", i);
              _biobaseDevice.TouchDisplayInitialExternalParameters[finger] = "1";  // show an additional finger
              SendTouchTest();
              Thread.Sleep(500);
            }

            _biobaseDevice.TouchDisplayInitialExternalParameters["HP1"] = "0";
            _biobaseDevice.TouchDisplayInitialExternalParameters["HP2"] = "1";
            for (int i = 6; i < 11; i++)
            {
              string finger = string.Format("FP{0}", i);
              _biobaseDevice.TouchDisplayInitialExternalParameters[finger] = "1";  // show an additional finger
              SendTouchTest();
              Thread.Sleep(500);
            }
          }
          else
          {
            SendTouchTest();
            Thread.Sleep(1500);
          }
        }

        // End test with index_final "thank you!" html file.
        _biobaseDevice.TouchDisplayInitialTemplate = "file:///" + TouchDisplayTemplatePath + "index_final.html";
        _biobaseDevice.TouchDisplayInitialExternalParameters.Clear();
        SendTouchTest();

      }
      catch (BioBaseException ex)
      {
        string msg = string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message);
        DialogResult result = MessageBox.Show(msg, "BioBase error", MessageBoxButtons.OK);
      }
      catch (Exception ex)
      {
        string msg = string.Format("{0} with {1} exception {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message);
        DialogResult result = MessageBox.Show(msg, "Exception", MessageBoxButtons.OK);
      }
      finally
      {
        _biobaseDevice.ScannerUserOutput -= _biobaseDevice_ScannerUserOutput;
        this.Cursor = Saved;
      }
    }

    /*!
     * \fn private void btnEmulateLEDTest_Click()
     * \brief Test each of the emulated LEDs by calling OutputLED to create XML for SetOutputData
     * 
     * This test is valid for LScan 1000, LScan 1000PX, LScan 500P, LScan 500, and legacy LScan 1000P, LScan 1000T
     * 
     */
    private void btnEmulateLEDTest_Click(object sender, EventArgs e)
    {
      try
      {
        // Emulation LEDs for the LScan 1000P, LScan 1000T, LScan 500P, LScan 1000PX, LScan 1000, LScan 500
        string led = PropertyConstants.OUT_DATA_LED_OK_GREEN_B1;  // LScan LED Green slow blink
        OutputLED(led);

        led = PropertyConstants.OUT_DATA_LED_OK_GREEN_B2;   // LScan LED Green fast flash 
        OutputLED(led);
        led = PropertyConstants.OUT_DATA_LED_OK_GREEN;      // LScan LED Green on
        OutputLED(led);
        led = PropertyConstants.OUT_DATA_LED_OK_YELLOW_B1;  // LScan LED Yellow slow blink
        OutputLED(led);
        led = PropertyConstants.OUT_DATA_LED_OK_YELLOW_B2;  // LScan LED Yellow fast flash 
        OutputLED(led);
        led = PropertyConstants.OUT_DATA_LED_OK_YELLOW;     // LScan LED Yellow on
        OutputLED(led);
        led = PropertyConstants.OUT_DATA_LED_CANCEL_B1;     // LScan LED Red slow blink
        OutputLED(led);
        led = PropertyConstants.OUT_DATA_LED_CANCEL_B2;     // LScan LED Red fast flash 
        OutputLED(led);
        led = PropertyConstants.OUT_DATA_LED_CANCEL;        // LScan LED Red on
        OutputLED(led);
        led = PropertyConstants.OUT_DATA_LED_NONE;
        OutputLED(led);

      }
      catch (BioBaseException ex)
      {
        string msg = string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message);
        DialogResult result = MessageBox.Show(msg, "BioBase error", MessageBoxButtons.OK);
      }
      catch (Exception ex)
      {
        string msg = string.Format("{0} with {1} exception {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message);
        DialogResult result = MessageBox.Show(msg, "Exception", MessageBoxButtons.OK);
      }
    }

    /*!
     * \fn private void btnTFTTest_Click()
     * \brief calls vaious fincutions to test the LScan TFT displays
     * 
     * This test is valid for LScan 1000, LScan 1000PX, LScan 500P, LScan 500, and legacy LScan 1000P, LScan 1000T
     */
    private void btnTFTTest_Click(object sender, EventArgs e)
    {
      Cursor Saved = this.Cursor;
      try
      {
        this.Cursor = Cursors.WaitCursor;
        TFTInitTest();        // Test intialization screens
        TFTCaptureTest();     // Test the OUT_DATA_TFT_CAP_SCREEN format screens
        TFTFingerTest();      // Test the OUT_DATA_TFT_FIN_SCREEN format screens
        TFTShowProgress(0);   // End test with logo screen
      }
      catch (BioBaseException ex)
      {
        string msg = string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT_CAP_SCREEN, ex.Message);
        DialogResult result = MessageBox.Show(msg, "BioBase error", MessageBoxButtons.OK);
      }
      catch (Exception ex)
      {
        string msg = string.Format("{0} with {1} exception {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT_CAP_SCREEN, ex.Message);
        DialogResult result = MessageBox.Show(msg, "Exception", MessageBoxButtons.OK);
      }
      finally
      {
        this.Cursor = Saved;
      }
    }

    #region BEEPER

    /*!
     * \fn private void _Beep()
     * \brief Send beep pattern to the device
     * NOTE: if the developer wishes to view the XML forma structure, the xmlDoc.OuterXml string can be passed to the AddMessage method 
     * 
     * Typical XML for beep command:
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     * -<OutputData>
     *   <Beeper Volume="100" Pattern="3"/>
     * </OutputData>
     * </BioBase>
     * 
     * Catch BioBaseException and Exception and log errors
     */
    private void _Beep(string pattern, string volume)
    {
      try
      {
        // create xml tree
        XmlDocument xmlDoc = new XmlDocument();
        _SetupXMLOutputHeader(xmlDoc);

        XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
        XmlElement beeperElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_BEEPER);
        beeperElem.SetAttribute(PropertyConstants.OUT_DATA_BEEP_PATTERN, pattern);
        beeperElem.SetAttribute(PropertyConstants.OUT_DATA_BEEP_VOLUME, volume);

        xmlDoc.DocumentElement.AppendChild(outputDataElem);
        outputDataElem.AppendChild(beeperElem);

        _PerformUserOutput(xmlDoc.OuterXml);        //Send XML to device
      }
      catch (BioBaseException ex)
      {
        string msg = string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_BEEPER, ex.Message);
        DialogResult result = MessageBox.Show(msg, "BioBase error", MessageBoxButtons.OK);
      }
      catch (Exception ex)
      {
        string msg = string.Format("{0} with {1} exception {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_BEEPER, ex.Message);
        DialogResult result = MessageBox.Show(msg, "Exception", MessageBoxButtons.OK);
      }
    }
    #endregion

    #region TOUCHSCREEN

    /*!
     * \fn private void SendTouchTest()
     * \brief Create XML for html files and parameters for SetOutputData
     * NOTE: if the developer wishes to view the XML structure, the xmlDoc.OuterXml string can be passed to the AddMessage method 
     * 
     * Typical XML for Touch Screen command:
     * 
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     * -<OutputData>
     *   -<TouchDisplay>
     *     -<DesignTemplate>
     *       <URI>file:///E:\Development\DP\LSE7\Applications\LSE_BioBase4_CSharpSample\bin\Debug\Templates\index_initial_left.html</URI>
     *       <ExternalParameter Value="1" Key="FP7"/>
     *       <ExternalParameter Value="1" Key="FP8"/>
     *       <ExternalParameter Value="1" Key="FP9"/>
     *       <ExternalParameter Value="0" Key="FP10"/>
     *     </DesignTemplate>
     *   </TouchDisplay>
     * </OutputData>
     * </BioBase>
     * 
     * Catch BioBaseException and Exception and log errors
     */
    private void SendTouchTest()
    {
      // create xml tree
      XmlDocument xmlDoc = new XmlDocument();
      _SetupXMLOutputHeader(xmlDoc);

      XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
      xmlDoc.DocumentElement.AppendChild(outputDataElem);

      XmlElement TDElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY);
      outputDataElem.AppendChild(TDElem);

      XmlElement TemplateElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY_DESIGNTEMPLATE);
      TDElem.AppendChild(TemplateElem);

      // Set template to initial animated html template.
      _AddUserOutputElement(xmlDoc, TemplateElem, PropertyConstants.OUT_DATA_TOUCHDISPLAY_URI, _biobaseDevice.TouchDisplayInitialTemplate);

      foreach (KeyValuePair<string, string> Ex in _biobaseDevice.TouchDisplayInitialExternalParameters)
      {
        XmlElement EpElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER);
        EpElem.SetAttribute(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER_KEY, Ex.Key);
        EpElem.SetAttribute(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER_VALUE, Ex.Value);
        TemplateElem.AppendChild(EpElem);
      }

      _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
    }

    /*!
     * \fn void _biobaseDevice_ScannerUserOutput()
     * \brief This event is triggered when data is sent to the scanner devices.
     * Note that all updates to the UI from this event must be done via Invoke calls.
     *
     * This event is enabled but images can't be updated from this thread
     *
     */
    void _biobaseDevice_ScannerUserOutput(object sender, BioBaseUserOutputEventArgs e)
    {

      //Display a copy of Guardian 300 User Guidance image in UI
      // When e.FormatType is an image, then e.SetOutputData can be displayed in the UI.
      // This will only come from the Guardian, Guardian 300, Guardian 200, Guardian 100 and Guardian Module
      if (e.FormatType == BioBOutputDataFormat.BIOB_OUT_BMP)
      {
        MemoryStream bytes = new MemoryStream(e.SetOutputData);
        Bitmap bmp = new Bitmap(bytes);
        UpdateImage(bmp);
      }
    }
    void UpdateImage(Bitmap bmp)
    {
        if(pictureBoxTouch.InvokeRequired)
          Invoke((Action<Bitmap>)UpdateImage, bmp);
        else
          pictureBoxTouch.Image = bmp;
    }
    #endregion

    #region LED
    /*!
     * \fn private void OutputLED()
     * \brief format emulated LED XML for SetOutputData
     * 
     * This test is valid for LScan 1000, LScan 1000PX, LScan 500P, LScan 500, and legacy LScan 1000P, LScan 1000T because it only 
     *  supports/controls one LED at a time.
     * 
     * Typical XML for beep command:
     * <?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>
     * <BioBase Version=\"4.0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"BioBase.xsd\">
     * <OutputData>
     * <StatusLeds>
     * <Led>NONE</Led>
     * <Led>OK_GREEN</Led>
     * </StatusLeds>
     * </OutputData>
     * </BioBase>
     * 
     * No try/catch as the BioBaseException and Exception are passed to calling functions
     * 
     */
    private void OutputLED(string led)
    {
      XmlDocument xmlDoc = new XmlDocument();
      _SetupXMLOutputHeader(xmlDoc);

      XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
      xmlDoc.DocumentElement.AppendChild(outputDataElem);

      XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_STATUSLEDS);
      outputDataElem.AppendChild(statusDataElem);

      // turn off all status LEDs
      _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_NONE);
      _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, led);
      _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      Thread.Sleep(750);
    }

    #endregion

    #region TFT_DISPLAY

    // L SCAN palm scanner TFT object definitions. Each segment of the TFT display is controled independently.
    // Base TFT_ObjectDictionary class used by the TFTFingerTest method.
    // Base TFT_ObjectDictionary class used for the FingerSelectionScreen formated screen.
    protected class TFT_ObjectDictionary : Dictionary<string, string>
    {
      public TFT_ObjectDictionary()
      {
        this.Add(PropertyConstants.OUT_DATA_TFT_CTRL_LEFT, PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_ERASE);
        this.Add(PropertyConstants.OUT_DATA_TFT_CTRL_RIGHT, PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_ERASE);

        this.Add(PropertyConstants.OUT_DATA_TFT_L_PALM, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_THENAR, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_LOWERT, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_INTER, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_THUMB, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_INDEX, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_MIDDLE, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_RING, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_SMALL, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);

        this.Add(PropertyConstants.OUT_DATA_TFT_R_PALM, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_THENAR, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_LOWERT, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_INTER, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_THUMB, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_INDEX, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_MIDDLE, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_RING, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_SMALL, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE);
      }
      ~TFT_ObjectDictionary()
      {
        this.Clear();
      }
    }

    // L SCAN palm scanner TFT object definitions. Each segment of the TFT display is controled independently.
    // drived TFT_ObjectCaptureDictionary class used by the TFTCaptureTest method.
    // drived TFT_ObjectCaptureDictionary class used for the CaptureProgressScreen formated screen.
    protected class TFT_ObjectCaptureDictionary : TFT_ObjectDictionary
    {

      public TFT_ObjectCaptureDictionary()
      {
        this[PropertyConstants.OUT_DATA_TFT_CTRL_LEFT] = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_ERASE;

        this.Add(PropertyConstants.OUT_DATA_TFT_STAT_TOP, PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ERASE);
        this.Add(PropertyConstants.OUT_DATA_TFT_STAT_BOTTOM, PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ERASE);

      }
      ~TFT_ObjectCaptureDictionary()
      {
        this.Clear();

      }
    }

    /*!
     * \fn void TFTInitTest()
     * \brief Display optional initial screens on TFT display
     * 
     * The following screens ar displayed:
     * OUT_DATA_TFT_LOG_SCR_PROGRESS
     * OUT_DATA_TFT_MOD_SCREEN
     * OUT_DATA_TFT_RES_SCREEN
     *
     * Catch BioBaseException and Exception and log errors
     */
    private void TFTInitTest()
    {
      try
      {
        for (int i = 1; i < 5; i++)
        {
          TFTShowProgress(i);
        }

        TFTShowModeScreen();        // shows mode selection screen.
        TFTShowResolutionScreen();  // shows resolution selection screen.
      }
      catch (BioBaseException ex)
      {
        string msg = string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT_LOG_SCR_OPTION, ex.Message);
        DialogResult result = MessageBox.Show(msg, "BioBase error", MessageBoxButtons.OK);
      }
      catch (Exception ex)
      {
        string msg = string.Format("{0} with {1} exception {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT_LOG_SCR_OPTION, ex.Message);
        DialogResult result = MessageBox.Show(msg, "Exception", MessageBoxButtons.OK);
      }
    }


    /*!
     * \fn private void TFTShowProgress()
     * \brief Setup XML element for OUT_DATA_TFT_LOG_SCR_PROGRESS and send output to device
     * 
     * No try/catch as the BioBaseException and Exception are passed to calling functions
     */
    private void TFTShowProgress(int percent)
    {
      XmlDocument xmlDoc = new XmlDocument();
      _SetupXMLOutputHeader(xmlDoc);

      XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
      xmlDoc.DocumentElement.AppendChild(outputDataElem);

      XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT);
      outputDataElem.AppendChild(statusDataElem);
      XmlElement LogElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT_LOG_SCREEN);
      statusDataElem.AppendChild(LogElem);

      _AddUserOutputElement(xmlDoc, LogElem, PropertyConstants.OUT_DATA_TFT_LOG_SCR_OPTION, PropertyConstants.OUT_DATA_TFT_LOG_SCR_SHOW_FW_VERSION);
      if (percent > 0 && percent <= 100)
      {
        string percentage = string.Format("{0}", percent * 25);
        _AddUserOutputElement(xmlDoc, LogElem, PropertyConstants.OUT_DATA_TFT_LOG_SCR_PROGRESS, percentage);
      }
      _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      Thread.Sleep(400);
    }

    /*!
     * \fn private void TFTShowModeScreen()
     * \brief Setup XML element for OUT_DATA_TFT_MOD_SCREEN and send output to device
     * 
     * TODO: read in key selection
     * 
     * No try/catch as the BioBaseException and Exception are passed to calling functions
     */
    private void TFTShowModeScreen()
    {
      // create xml tree for OUT_DATA_TFT_MOD_SCREEN option
      XmlDocument xmlDoc = new XmlDocument();
      _SetupXMLOutputHeader(xmlDoc);

      XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
      xmlDoc.DocumentElement.AppendChild(outputDataElem);

      XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT);
      outputDataElem.AppendChild(statusDataElem);
      XmlElement LogElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT_MOD_SCREEN);
      statusDataElem.AppendChild(LogElem);
      _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      Thread.Sleep(400);
    }

    /*!
     * \fn private void TFTShowResolutionScreen()
     * \brief Setup XML element for OUT_DATA_TFT_RES_SCREEN and send output to device
     * 
     * TODO: read in key selection
     * 
     * No try/catch as the BioBaseException and Exception are passed to calling functions
     */
    private void TFTShowResolutionScreen()
    {
      // create xml tree for OUT_DATA_TFT_RES_SCREEN option
      XmlDocument xmlDoc = new XmlDocument();
      _SetupXMLOutputHeader(xmlDoc);

      XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
      xmlDoc.DocumentElement.AppendChild(outputDataElem);

      XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT);
      outputDataElem.AppendChild(statusDataElem);
      XmlElement LogElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT_RES_SCREEN);
      statusDataElem.AppendChild(LogElem);
      _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      Thread.Sleep(400);
    }

    /*!
     * \fn private void TFTCaptureTest()
     * \brief Format XML elements for OUT_DATA_TFT_CAP_SCREEN and send output to device
     * 
     * The *CaptureProgressScreen* XML should have have 1 and 22 elements.
     * 
     * The *CaptureProgressScreen* XML can have <StatTop> and <StatBottom> elements.
     * 
     * It is required to have all 22 elements in the XML when making the first SetOutputData call
     * requiring the display to change to the *CaptureProgressScreen*. If the XML for the first call 
     * to does not contain all 22 KeyValuePair elements, there will be a BIOB_DEVICEINVALIDPARAM exception!
     * 
     * It is not required to have all 22 elements in the XML on subsequent SetOutputData calls. When the 
     * element is excluded it is equivelant to setting it's key value set to "LEAVE_UNCHANGED".
     * 
     * No try/catch as the BioBaseException and Exception are passed up to _SetTftGuidance and _SetTftStatus
     * 
     * Some examples of XML format for TFT display:
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     *  -<OutputData>
     *   -<Tft>
     *    -<CaptureProgressScreen>
     *      <LeftButton>YELLOW_CONTRAST</LeftButton>
     *      <RightButton>GREEN_OK</RightButton>
     *     </CaptureProgressScreen>
     *    </Tft>
     *   </OutputData>
     *  </BioBase>
     *  
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     * -<OutputData>
     * -<Tft>
     * -<CaptureProgressScreen>
     * 
     *   <StatTop>CAPTURE_FLAT</StatTop>
     *   <StatBottom>ERASE</StatBottom>
     *
     *   <LeftButton>YELLOW_CONTRAST</LeftButton>
     *   <RightButton>GREEN_OK</RightButton>
     *   <ColorLeftPalm>INACTIVE</ColorLeftPalm>
     *   <ColorLeftThenar>INACTIVE</ColorLeftThenar>
     *   <ColorLeftLowerThenar>INACTIVE</ColorLeftLowerThenar>
     *   <ColorLeftInterDigital>INACTIVE</ColorLeftInterDigital>
     *   <ColorLeftThumb>INACTIVE</ColorLeftThumb>
     *   <ColorLeftIndex>INACTIVE</ColorLeftIndex>
     *   <ColorLeftMiddle>INACTIVE</ColorLeftMiddle>
     *   <ColorLeftRing>INACTIVE</ColorLeftRing>
     *   <ColorLeftSmall>INACTIVE</ColorLeftSmall>
     *   <ColorRightPalm>INACTIVE</ColorRightPalm>
     *   <ColorRightThenar>INACTIVE</ColorRightThenar>
     *   <ColorRightLowerThenar>INACTIVE</ColorRightLowerThenar>
     *   <ColorRightInterDigital>INACTIVE</ColorRightInterDigital>
     *   <ColorRightThumb>AUTOCAPTURE_OK</ColorRightThumb>
     *   <ColorRightIndex>INACTIVE</ColorRightIndex>
     *   <ColorRightMiddle>INACTIVE</ColorRightMiddle>
     *   <ColorRightRing>INACTIVE</ColorRightRing>
     *   <ColorRightSmall>INACTIVE</ColorRightSmall>
     * </CaptureProgressScreen>
     * </Tft>
     * </OutputData>
     * </BioBase>
     * 
     */
    private void TFTCaptureTest()
    {
      TFT_ObjectCaptureDictionary obj = new TFT_ObjectCaptureDictionary();

      // Array of available "operation icons" ions for the TFT display's left and right button on the LScan palm scanner.
      string[] ctrlBtn = new string[] {
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_OK,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_OVERRIDE,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_CONTRAST,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_REPEAT,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_OK,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_OVERRIDE,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_ERASE,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_CONTRAST,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_REPEAT};
      // Loop through and display each Common Control icon option for the left button
      foreach (string text in ctrlBtn)
      {
        obj[PropertyConstants.OUT_DATA_TFT_CTRL_LEFT] = text;
        _TftShowFingerCaptureScreen(false, obj);
        Thread.Sleep(200);
      }
      // Loop through and display each Common Control icon option for the right button
      foreach (string text in ctrlBtn)
      {
        obj[PropertyConstants.OUT_DATA_TFT_CTRL_RIGHT] = text;
        _TftShowFingerCaptureScreen(false, obj);
        Thread.Sleep(200);
      }

      // Array of available "capture mode" ions for the TFT display's top status icon on the LScan palm scanner.
      string[] statusTop = new string[] {
        PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_HORIZONTAL,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_HORIZONTAL_LEFT,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_HORIZONTAL_RIGHT,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_LEAVE_UNCHANGED,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_VERTICAL,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_VERTICAL_UP,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ERASE,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_VERTICAL_DOWN,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_CAPTURE_FLAT};
      // Loop through and display each status top icon option located between the left and right hand
      foreach (string text in statusTop)
      {
        obj[PropertyConstants.OUT_DATA_TFT_STAT_TOP] = text;
        _TftShowFingerCaptureScreen(false, obj);
        Thread.Sleep(200);
      }

      // Array of available "status" ions for the TFT display's top status icon on the LScan palm scanner.
      string[] statusBottom = new string[] {
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_OK,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_OK_ALT_1,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ERASE,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_CLEAN_SURFACE,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_CLEAN_SURFACE_ALT_1,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_SURFACE_IS_DIRTY,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_SURFACE_IS_DIRTY_ALT_1,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_COMMON_ERROR,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_CAPTURE_ERROR,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_CAPTURE_ERROR_ALT_1,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_QUALITY_CHECK_ERROR,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ROLL_ERROR,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ROLL_ERROR_ALT_1,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ROLL_ERROR_ALT_2,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ROLL_ERROR_ALT_3,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_SEQUENCE_CHECK_ERROR,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_SEQUENCE_CHECK_ERROR_ALT_1,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_UP,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_UP_RIGHT,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_RIGHT,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_DOWN_RIGHT,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_DOWN,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_DOWN_LEFT,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_LEFT,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_UP_LEFT,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_DOWN_LEFT_RIGHT_UP,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_LEFT_RIGHT,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_HOURGLASS_STATIC,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_HOURGLASS_ANIMATED, 
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_CAPTURING_ANIMATED, 
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ROLLING_LEFT_ANIMATED,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ROLLING_RIGHT_ANIMATED,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_LEAVE_UNCHANGED,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_COMPRESSION_ERROR,
        PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_SEGMENTATION_ERROR};
      // Loop through and display each status bottom icon option located between the left and right hand
      foreach (string text in statusBottom)
      {
        obj[PropertyConstants.OUT_DATA_TFT_STAT_BOTTOM] = text;
        _TftShowFingerCaptureScreen(false, obj);
        Thread.Sleep(200);
      }

      // Array of available finger/hand segments for the TFT display on the LScan palm scanner.
      string[] segment = new string[] {
        PropertyConstants.OUT_DATA_TFT_L_PALM,
        PropertyConstants.OUT_DATA_TFT_L_THENAR,
        PropertyConstants.OUT_DATA_TFT_L_LOWERT,
        PropertyConstants.OUT_DATA_TFT_L_INTER,
        PropertyConstants.OUT_DATA_TFT_L_THUMB,
        PropertyConstants.OUT_DATA_TFT_L_INDEX,
        PropertyConstants.OUT_DATA_TFT_L_MIDDLE,
        PropertyConstants.OUT_DATA_TFT_L_RING,
        PropertyConstants.OUT_DATA_TFT_L_SMALL,
        PropertyConstants.OUT_DATA_TFT_R_PALM,
        PropertyConstants.OUT_DATA_TFT_R_THENAR,
        PropertyConstants.OUT_DATA_TFT_R_LOWERT,
        PropertyConstants.OUT_DATA_TFT_R_INTER,
        PropertyConstants.OUT_DATA_TFT_R_THUMB,
        PropertyConstants.OUT_DATA_TFT_R_INDEX,
        PropertyConstants.OUT_DATA_TFT_R_MIDDLE,
        PropertyConstants.OUT_DATA_TFT_R_RING,
        PropertyConstants.OUT_DATA_TFT_R_SMALL };

      // Array of available color options for finger/hand segments for the TFT display on the LScan palm scanner.
      string[] segmentText = new string[] {
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_MISSING,
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_CURRENT_SELECTION,
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_ACTIVE,
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_RESTRICTED,
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE,
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK,
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED};
      // Loop through each finger/hand segments 
      foreach (string seg in segment)
      {
        // Loop through and display each color option
        foreach (string text in segmentText)
        {
          obj[seg] = text;
          _TftShowFingerCaptureScreen(false, obj);
          Thread.Sleep(50);
        }
      }
    }

    /*!
     * \fn private void TFTFingerTest()
     * \brief Format XML elements for OUT_DATA_TFT_FIN_SCREEN and send output to device
     * 
     * The *FingerSelectionScreen* XML should have have 1 and 20 elements.
     * 
     * The *FingerSelectionScreen* XML can NOT have <StatTop> and <StatBottom> elements.
     * 
     * It is required to have all 20 elements in the XML when making the first SetOutputData call
     * requiring the display to change to the *FingerSelectionScreen*.  If the XML for the first call 
     * to does not contain all 20 KeyValuePair elements, there will be a BIOB_DEVICEINVALIDPARAM exception!
     * 
     * It is not required to have all 20 elements in the XML on subsequent SetOutputData calls. When the 
     * element is excluded it is equivelant to setting it's key value set to "LEAVE_UNCHANGED".
     * 
     * No try/catch as the BioBaseException and Exception are passed up to _SetTftGuidance and _SetTftStatus
     * 
     * Some examples of XML format for TFT display:
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     *  -<OutputData>
     *   -<Tft>
     *    -<FingerSelectionScreen>
     *      <LeftButton>YELLOW_CONTRAST</LeftButton>
     *      <RightButton>GREEN_OK</RightButton>
     *     </FingerSelectionScreen>
     *    </Tft>
     *   </OutputData>
     *  </BioBase>
     *  
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     * -<OutputData>
     * -<Tft>
     * -<FingerSelectionScreen>
     *   <LeftButton>YELLOW_CONTRAST</LeftButton>
     *   <RightButton>GREEN_OK</RightButton>
     *   <ColorLeftPalm>INACTIVE</ColorLeftPalm>
     *   <ColorLeftThenar>INACTIVE</ColorLeftThenar>
     *   <ColorLeftLowerThenar>INACTIVE</ColorLeftLowerThenar>
     *   <ColorLeftInterDigital>INACTIVE</ColorLeftInterDigital>
     *   <ColorLeftThumb>INACTIVE</ColorLeftThumb>
     *   <ColorLeftIndex>INACTIVE</ColorLeftIndex>
     *   <ColorLeftMiddle>INACTIVE</ColorLeftMiddle>
     *   <ColorLeftRing>INACTIVE</ColorLeftRing>
     *   <ColorLeftSmall>INACTIVE</ColorLeftSmall>
     *   <ColorRightPalm>INACTIVE</ColorRightPalm>
     *   <ColorRightThenar>INACTIVE</ColorRightThenar>
     *   <ColorRightLowerThenar>INACTIVE</ColorRightLowerThenar>
     *   <ColorRightInterDigital>INACTIVE</ColorRightInterDigital>
     *   <ColorRightThumb>AUTOCAPTURE_OK</ColorRightThumb>
     *   <ColorRightIndex>INACTIVE</ColorRightIndex>
     *   <ColorRightMiddle>INACTIVE</ColorRightMiddle>
     *   <ColorRightRing>INACTIVE</ColorRightRing>
     *   <ColorRightSmall>INACTIVE</ColorRightSmall>
     * </FingerSelectionScreen>
     * </Tft>
     * </OutputData>
     * </BioBase>
     * 
     */
    private void TFTFingerTest()
    {
      TFT_ObjectDictionary obj = new TFT_ObjectDictionary();

      // Array of available "Finger annotation" icons for the TFT display's left button on the LScan palm scanner.
      string[] ctrlLeftBtn = new string[] {
        PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_OBJECT_OK,
        PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_OBJECT_BANDAGED,
        PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_ERASE,
        PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_OBJECT_MISSING,
        PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_OBJECT_RESTRICTED,
        PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_REPEAT_YELLOW,
        PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_LEAVE_UNCHANGED};
      // Loop through and display each Selection Control icon option for the left button
      foreach (string text in ctrlLeftBtn)
      {
        obj[PropertyConstants.OUT_DATA_TFT_CTRL_LEFT] = text;
        _TftShowFingerCaptureScreen(true, obj);
        Thread.Sleep(200);
      }

      // Array of available "operation icons" ions for the TFT display's right button on the LScan palm scanner.
      string[] ctrlRightBtn = new string[] {
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_OK,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_ERASE,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_OVERRIDE,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_CONTRAST,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_REPEAT,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_OK,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_OVERRIDE,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_CONTRAST,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_REPEAT,
        PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED};
      // Loop through and display each Common Control icon option for the right button
      foreach (string text in ctrlRightBtn)
      {
        obj[PropertyConstants.OUT_DATA_TFT_CTRL_RIGHT] = text;
        _TftShowFingerCaptureScreen(true, obj);
        Thread.Sleep(200);
      }

      // Array of available finger/hand segments for the TFT display on the LScan palm scanner.
      string[] segment = new string[] {
        PropertyConstants.OUT_DATA_TFT_L_PALM,
        PropertyConstants.OUT_DATA_TFT_L_THENAR,
        PropertyConstants.OUT_DATA_TFT_L_LOWERT,
        PropertyConstants.OUT_DATA_TFT_L_INTER,
        PropertyConstants.OUT_DATA_TFT_L_THUMB,
        PropertyConstants.OUT_DATA_TFT_L_INDEX,
        PropertyConstants.OUT_DATA_TFT_L_MIDDLE,
        PropertyConstants.OUT_DATA_TFT_L_RING,
        PropertyConstants.OUT_DATA_TFT_L_SMALL,
        PropertyConstants.OUT_DATA_TFT_R_PALM,
        PropertyConstants.OUT_DATA_TFT_R_THENAR,
        PropertyConstants.OUT_DATA_TFT_R_LOWERT,
        PropertyConstants.OUT_DATA_TFT_R_INTER,
        PropertyConstants.OUT_DATA_TFT_R_THUMB,
        PropertyConstants.OUT_DATA_TFT_R_INDEX,
        PropertyConstants.OUT_DATA_TFT_R_MIDDLE,
        PropertyConstants.OUT_DATA_TFT_R_RING,
        PropertyConstants.OUT_DATA_TFT_R_SMALL };

      // Array of available color options for finger/hand segments for the TFT display on the LScan palm scanner.
      string[] segmentText = new string[] {
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_MISSING,
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_CURRENT_SELECTION,
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_ACTIVE,
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_RESTRICTED,
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE,
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK,
        PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED};
      // Loop through each finger/hand segments 
      foreach (string seg in segment)
      {
        // Loop through and display each color option
        foreach (string text in segmentText)
        {
          obj[seg] = text;
          _TftShowFingerCaptureScreen(true, obj);
          Thread.Sleep(50);
        }
      }
    }

    /*!
     * \fn private void _TftShowFingerSelection()
     * \brief Format XML and send output for status and guidance to the TFT OUT_DATA_TFT_FIN_SCREEN and OUT_DATA_TFT_CAP_SCREEN
     * input: finger true for OUT_DATA_TFT_FIN_SCREEN and false for OUT_DATA_TFT_CAP_SCREEN
     * 
     * No try/catch as the BioBaseException and Exception are passed up to the calling function
     * 
     */
    private void _TftShowFingerCaptureScreen(bool finger, TFT_ObjectDictionary obj)
    {
      // create xml tree
      XmlDocument xmlDoc = new XmlDocument();
      _SetupXMLOutputHeader(xmlDoc);

      XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
      xmlDoc.DocumentElement.AppendChild(outputDataElem);

      XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT);
      outputDataElem.AppendChild(statusDataElem);

      XmlElement Elem;
      if(finger)
        Elem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT_FIN_SCREEN);
      else
        Elem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT_CAP_SCREEN);

      statusDataElem.AppendChild(Elem);

      foreach (KeyValuePair<string, string> property in obj)
      {
        _AddUserOutputElement(xmlDoc, Elem, property.Key, property.Value);
      }
      _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
    }
    #endregion

    #region OUTPUT_UTILITIES

    /*!
     * \fn private void _SetupXMLOutputHeader()
     * \brief Format generic XML header for all SetOutputData 
     * 
     * No try/catch as the BioBaseException and Exception are passed to calling functions
     */
    private void _SetupXMLOutputHeader(XmlDocument xmlDoc)
    {
      XmlDeclaration declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.AppendChild(declaration);

      XmlElement rootBiobaseElem = xmlDoc.CreateElement(PropertyConstants.XML_ROOT);
      rootBiobaseElem.SetAttribute(PropertyConstants.XML_ROOT_INTERFACE_VERSION, PropertyConstants.XML_ROOT_INTERFACE_VERSION_NUMBER);
      rootBiobaseElem.SetAttribute(PropertyConstants.XMLNS, PropertyConstants.XMLNS_URL);

      XmlAttribute attributeNode = xmlDoc.CreateAttribute("xsi", "noNamespaceSchemaLocation", PropertyConstants.XMLNS_URL);
      attributeNode.Value = PropertyConstants.XML_SCHEMA_NAME;
      rootBiobaseElem.SetAttributeNode(attributeNode);
      xmlDoc.AppendChild(rootBiobaseElem);    // add root
    }

    /*!
     * \fn private void _AddUserOutputElement()
     * \brief Add XML KeyValuePair element for SetOutputData
     * 
     * No try/catch as the BioBaseException and Exception are passed to calling functions
     */
    private void _AddUserOutputElement(
                             XmlDocument xmlDoc,  ///< [in]  top level XML document for elements
                             XmlElement Parent,   ///< [in]  XML element to add element to
                             string element,      ///< [in]  new XML element
                             string text)         ///< [in]  text for XML element
    {
      XmlNode StatusElement = xmlDoc.CreateElement(element);
      StatusElement.InnerText = text;
      Parent.AppendChild(StatusElement);
    }

    /*!
     * \fn private void _PerformUserOutput()
     * \brief Format generic XML header for all SetOutputData 
     * 
     * No try/catch as the BioBaseException and Exception are passed to calling functions
     */
    private void _PerformUserOutput(string xml)
    {
      // Get IntPtr to unmanaged copy of xml string
      System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
      byte[] outData = encoding.GetBytes(xml);
      int size = Marshal.SizeOf(outData[0]) * outData.Length;
      IntPtr imagePtr = Marshal.AllocHGlobal(size);
      Marshal.Copy(outData, 0, imagePtr, outData.Length);

      // Format unmanaged structure for SetOutputData
      BioBSetOutputData outputData;
      outputData.Buffer = imagePtr;
      outputData.BufferSize = size;
      outputData.FormatType = BioBOutputDataFormat.BIOB_OUT_XML;
      outputData.pExtStruct = IntPtr.Zero;
      outputData.pStructName = null;
      outputData.TransactionID = 0;

      // Output sent to the open device referenced in the _biobaseDevice object
      _biobaseDevice.SetOutputData(outputData);
      Marshal.FreeHGlobal(imagePtr);
    }
    #endregion
  }
}
