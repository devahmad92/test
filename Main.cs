/**
 * \file Main.cs
 * \author HID Global
 * \date 1 July 2020
 * \brief File containing C# example for LScan Essentials SDK.
 *
 * This C# sample works with the BiobaseNet.LSE.2013 DotNet library.
 * to capture fingerprint and palm images from HID biometric products.
 * \see https://www.hidglobal.com/products/biometric-readers-modules
 * \see https://www.hidglobal.com/products/readers/tenprint-readers/guardian-patrol
 * \see https://www.hidglobal.com/products/readers/palm-scanners/l-scan
 */
using Crossmatch.BioBaseApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;   // needed to parse XML in application

//Optional for debug logging
using Microsoft.Win32;      // Computer registry used to get OS details.
using System.Management;    // Computer Info, ManagementClass, Memory, CPU, System Name, BIOS, Controllers, MAC


namespace LSE_BioBase4_CSharpSample
{
  /*!
   * \enum public enum DeviceState
   * \brief Describes the current state of the selected biometric device.
   */
  public enum DeviceState
  {
    device_not_connected,
    device_connected_and_not_opened,
    device_opened_and_not_live,
    device_opened_and_live,
    device_opened_and_image_captured,
    device_opened_and_capture_cancelled
  };
  public static class LSEConst
  {
        public const string DEVICE_TYPE_LSCAN_1000         = "L Scan 1000";
        public const string DEVICE_TYPE_LSCAN_1000P        = "L SCAN 1000P";
        public const string DEVICE_TYPE_LSCAN_1000PX       = "L SCAN 1000PX";
        public const string DEVICE_TYPE_LSCAN_1000T        = "L SCAN 1000T";
        public const string DEVICE_TYPE_LSCAN_500          = "L Scan 500";
        public const string DEVICE_TYPE_LSCAN_500P         = "L SCAN 500P";
        public const string DEVICE_TYPE_LSCAN_500PJ        = "L SCAN 500PJ";
        public const string DEVICE_TYPE_LSCAN_GUARDIAN_FW  = "L SCAN GUARDIAN";
        public const string DEVICE_TYPE_LSCAN_GUARDIAN_USB = "L SCAN GUARDIAN USB";
        public const string DEVICE_TYPE_LSCAN_GUARDIAN_F   = "L SCAN GUARDIAN F";
        public const string DEVICE_TYPE_LSCAN_GUARDIAN_R2  = "L SCAN GUARDIAN R2";
        public const string DEVICE_TYPE_LSCAN_GUARDIAN_T   = "L SCAN GUARDIAN T";
        public const string DEVICE_TYPE_LSCAN_GUARDIAN_L   = "L SCAN GUARDIAN L";
        public const string DEVICE_TYPE_LSCAN_PATROL       = "L SCAN PATROL";
        public const string DEVICE_TYPE_PATROL             = "PATROL";
        public const string DEVICE_TYPE_PATROL_ID          = "Patrol ID";
        public const string DEVICE_TYPE_GUARDIAN           = "GUARDIAN";
        public const string DEVICE_TYPE_GUARDIAN_MODULE    = "GUARDIAN Module";
        public const string DEVICE_TYPE_GUARDIAN_100       = "GUARDIAN 100";
        public const string DEVICE_TYPE_GUARDIAN_200       = "GUARDIAN 200";
        public const string DEVICE_TYPE_GUARDIAN_300       = "GUARDIAN 300";
        public const string DEVICE_TYPE_GUARDIAN_45        = "GUARDIAN 45";
        public const string DEVICE_TYPE_VERIFIER_320LC     = "VERIFIER 320LC";
        public const string DEVICE_TYPE_VERIFIER_320S      = "VERIFIER 320S";
  }


  public partial class Main : Form
  {
    LseBioBase _biobase = null;
    IBioBaseDevice _biobaseDevice = null;   // Object for the one device that this application will have open at a given time.
    BioBaseDeviceInfo[] _biobaseDevices;

    DeviceState _deviceState = DeviceState.device_not_connected;
    string _deviceType = null;
    private bool _imageCaptured = false;    // Final image available when flag is true
    bool m_scannerOpen = false;
    bool m_bAskRecapture = false;     // Used to confirm if Scanner OK button is for contrast adjustment or accept captured image

    bool m_bMsgBoxOpened = false;

    bool m_ImpressionModeRoll = false;  //no adjust on the fly for roll

    string m_CurrentTFTstatus = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ERASE; // quality status prevously sent to TFT display
    ActiveKeys m_CurrentTFTKey = ActiveKeys.KEYS_NONE;                                // keys prevously sent to TFT display


    string m_TouchDisplayTemplatePath;  //< Touch display template root path


    public Main()
    {
      InitializeComponent();
      //InitializeBioBase();    //Could be done here but this app executes the API initialization via btnOpen click

      m_TouchDisplayTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "Templates\\";
      m_TouchDisplayTemplatePath = m_TouchDisplayTemplatePath.Replace("\\", "/");
      m_TouchDisplayTemplatePath = "file:///" + m_TouchDisplayTemplatePath;

      textBoxLog.Clear();
      SetDeviceState(DeviceState.device_not_connected); // disable buttons 

      FillDeviceListBox();
    }


    /*!
     * \fn private void InitializeBioBase()
     * \brief Initialize BioBase API library with DeviceCount Change event and Open API
     * Application will wait for DeviceCount Change event before enabling UI to select and open a device
     * Catch BioBaseException and Exception and log errors
     */
    private void InitializeBioBase()
    {
      try
      {
        // Optional logging of OS and hardware.
        SystemInformation();


        AddMessage("BioBase object.");
        _biobase = new LseBioBase();

        AddMessage("BioBase API Register DeviceCount Change callback event.");
        _biobase.DeviceCountChanged += new EventHandler<BioBaseDeviceCountEventArgs>(_biobase_DeviceCount);

        AddMessage("BioBase API Open");
        _biobase.Open();

        // Optional logging of BioBase API version.
        BioBaseInterfaceVersion ver = _biobase.InterfaceVersion;
        AddMessage(string.Format("BioBase API version {0}.{1}", ver.Major, ver.Minor));

        // Optional logging of this API version.
        BioBaseApiProperties prop = _biobase.ApiProperties;
        AddMessage(string.Format("{0} SDK version {1}.", prop.Api, prop.Product));

      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("InitializeBioBase BioBase object error {0}", ex.Message));
      }
      catch (DllNotFoundException ex)
      {
        string msg = string.Format("LSEBioBase Open method failed - {0}.  \n\n Copy native dlls to development folder?", ex.Message);
        DialogResult result = MessageBox.Show(msg, "Missing native dlls", MessageBoxButtons.OK);
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("InitializeBioBase object error {0}", ex.Message));
      }
    }


    /*!
     * \fn private void FillDeviceListBox()
     * \brief Fill in list box with list of attached devices.
     * This method is called each time DeviceCount change event is called.
     * FillDeviceListBox uses BioBaseDeviceInfo[] _biobaseDevices object to fill the UI List Box
     * after the _biobase_DeviceCount event creates/updates _biobaseDevices with attached devices.
     * Catch BioBaseException and Exception and log errors
     */
    private void FillDeviceListBox()
    {
      try
      {
        if (DeviceInfoBox.InvokeRequired)
        {
          Invoke((Action)FillDeviceListBox);
        }
        else
        {
          bool foundDev = false;
          string SelectedindexDevID = "";

          // Remember Device ID so we can re-select device when re-populate DeviceInfoBox
          if (DeviceInfoBox.Items.Count > 0)
            SelectedindexDevID = DeviceInfoBox.SelectedItem.ToString(); 

          // If device was open and now disconnected, we need to call CloseDevice()
          // This logic can be reduced if you want to assume you will only have one device.
          // Get name of open device and assume it is no longer attached until proven otherwise
          string openDevice = "";
          bool bOpenDeviceAttached = false;
          if (_biobaseDevice != null)
            openDevice = _biobaseDevice.DeviceInfo.DeviceId;

          // clear out old device information
          while (DeviceInfoBox.Items.Count != 0)
          {
            DeviceInfoBox.Items.Clear();
            DeviceInfoBox.ClearSelected();
          }

          if ((_biobaseDevices == null) || (_biobaseDevices.Length == 0))
          {
            return; // Return if no devices are attached....
          }

          if (_biobase != null)
          {
            // loop through attached devices and add all of the DeviceID to the DeviceInfoBox list box
            AddMessage(" Filling list box with list of attached devices");
            foreach (BioBaseDeviceInfo device in _biobaseDevices)
            {
              if (device.DeviceId.Length > 0)
              {
                DeviceInfoBox.Items.Add(device.DeviceId);

                if (device.DeviceId == openDevice)
                  bOpenDeviceAttached = true; // We found open device is still attached. So, we won't call CloseDevice after this foreach loop

                // First, always select first item in list by default
                if (DeviceInfoBox.Items.Count == 1)
                  DeviceInfoBox.SelectedIndex = DeviceInfoBox.TopIndex;
                // Second, always select item in list if it was prevously selected.
                if (SelectedindexDevID == device.DeviceId)
                {
                  // If previously selelected item is found, make sure it is still selected
                  // Previously selelected item may still be open...
                  foundDev = true;
                  DeviceInfoBox.SelectedIndex = DeviceInfoBox.Items.Count - 1;
                }
              }
            }

            //If a device was open and is now disconnected, it must be closed
            if ((_biobaseDevice != null) && (bOpenDeviceAttached == false))
              CloseDevice();

            // If not able to re-select any prevously selected device, enable DeviceInfoBox
            if (foundDev == false)
            {
              // if new device is selected, it will not be open yet
              SetDeviceState(DeviceState.device_connected_and_not_opened);
              DeviceInfoBox.Enabled = true;
            }
          }
        }
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("FillDeviceListBox BioBase error {0}", ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("FillDeviceListBox error {0}", ex.Message));
      }
    }

    /*!
     * \fn private void btnOpen_Click()
     * \brief Open API button click
     * If API already open, close devices and API and then re-intitalize API
     * Catch BioBaseException and Exception and log errors
     */
    private void btnOpen_Click(object sender, EventArgs e)
    {
      try
      {
        if (_biobase != null) _biobase.Close(); // Close before re-open
        
        InitializeBioBase();
        AddMessage("Open BioBase API");

        // Now, it is best to wait for OnDeviceCountChanged event before continuing.
        // OPTION: Could check _biobase.NumberOfDevices in a loop unit a device is attached.
        // Another bad option would be to just *hope* all the devices are ready and get list of device here.
        //_biobaseDevices = _biobase.ConnectedDevices;
        //FillDeviceListBox();
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioB_Open BioBase error {0}", ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("BioB_Open error {0}", ex.Message));
      }
    }

    /*!
     * \fn private void btnClose_Click()
     * \brief Close API button click
     * If open, close devices and then close API
     * Catch BioBaseException and Exception and log errors
     */
    private void btnClose_Click(object sender, EventArgs e)
    {
      try
      {
        if(_biobaseDevice != null)
          CloseDevice();
        m_scannerOpen = false;

        _biobaseDevices = new BioBaseDeviceInfo[0];
        FillDeviceListBox();
        SetDeviceState(DeviceState.device_not_connected);

        _biobase.Close();
        AddMessage("Close BioBase API. Must call Open BioBase API before continuing.");
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioB_Close BioBase error {0}", ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("BioB_Close error {0}", ex.Message));
      }
    }

    /*!
     * \fn private void btnExit_Click()
     * \brief dispose of objects and exit the application.
     * Will close devices (will abort acquisition) before close API and exit.
     */
    private void btnExit_Click(object sender, EventArgs e)
    {
      if (_biobaseDevice != null)
        CloseDevice();

      if (_biobase != null)
      {
        _biobase.Close();
        _biobase.Dispose();
        _biobase = null;
      }
      this.Close();
    }

    /*!
     * \fn private void btnOpenDevice_Click()
     * \brief Open button click for the device selected in the DeviceInfoBox
     * Creates the IBioBaseDevice object, register events and opendevice, fill Position and Impression combo boxes
     * Catch BioBaseException and Exception and log errors
     */
    private void btnOpenDevice_Click(object sender, EventArgs e)
    {
      try
      {
        if (_biobase == null)
        {
          AddMessage("Error, BioBase BioBase API not opened!");
        }
        else
        {
          if (_biobaseDevice != null)
          {
            // Close device before it is Re-opened
            AddMessage("calling method 'IBioBaseDevice.Dispose'");
            _biobaseDevice.Dispose();
            _biobaseDevice = null;

          }

          btnOpenDevice.Enabled = false;  // Disable Open device button so only one CreateBioBaseDevice thread is created

          // Get the selected device that will be opened.
          // Start a new thread to call to open device!
          // Don't wait for it to finish because it can take up to 30 seconds for LScan 1000
          // The thread will enable UI if OpenDevice is successful.
          DeviceInfoBox.Enabled = false;  // Don't allow change to Device ID when device is open.
          string selectedDevice = DeviceInfoBox.SelectedItem.ToString();
          Thread myThread = new Thread(delegate()
          {
            CreateBioBaseDevice(selectedDevice);
          });
          myThread.Start();
          //        myThread.Join();  // Don't wait for thread to exit. Thread will enable UI so we know when to continue.
          // The _biobaseDevice_Init event will also be called with 100% when device is open and ready
          // Another less than ideal option would be to call _biobaseDevice.IsDeviceOpen() in a loop.
        }
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioB_OpenDevice BioBase error {0}", ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("BioB_OpenDevice error {0}", ex.Message));
      }
    }


    /*!
     * \fn private void btnCloseDevice_Click()
     * \brief Close button click for the device referenced in the _biobaseDevice object 
     * dispose reset UI, IBioBaseDevice object, un-register events and closedevice
     */
    private void btnCloseDevice_Click(object sender, EventArgs e)
    {
      // Reset LEDs in GUI on clsoedevice
      SetUILedColors(ActiveColor.gray, ActiveColor.gray, ActiveColor.gray, ActiveColor.gray, ActiveColor.gray);

      CloseDevice();
    }

    /*!
     * \fn private void btnProperties_Click()
     * \brief Properties button click for the device referenced in the _biobaseDevice object
     * This method demonstrates how to get the device properties that are supported by this scanner.
     * This logic is optional but helps so that application does not need to hard code the features of each device.
     * This will use two ways to query the device for device specific information.
     */
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // ***NOTE: The properties int the BioBaseDevicePropertyDictionary and returned in the GetBioBaseProperties 
    //          XML data are the only properties supported by this SDK. While the PropertyConstants class lists 
    //          all the BioBase properties, only a subset are supported by this SDK. If this application tries 
    //          to set properties that are not supported by this SDK, the BioBaseInterop class will throw an 
    //          exception that the application must catch.
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void btnProperties_Click(object sender, EventArgs e)
    {
      //OPTIONAL:
      // Option #1 get all of the device properties in the BioBaseDevicePropertyDictionary format
      BioBaseDevicePropertyDictionary DevPropDict = _biobaseDevice.Properties;
      foreach (KeyValuePair<string, string> property in DevPropDict)
      {
        // Log each of the device properties
        AddMessage(string.Format("{0} = {1}", property.Key, property.Value));

        //NOTE: This is a good place to add optional check of specific device property the application may need.
      }

      //Option #2 get all of the device properties in XML format and parse XML in application
      string DevProps = _biobaseDevice.GetBioBaseProperties();
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.LoadXml(DevProps);
      XmlNodeList elem = xmlDoc.GetElementsByTagName("DeviceProperties");
      if (elem == null)
      {
        AddMessage("Unalble to parse DeviceProperties XML");
      }
      else
      {
        XmlNodeList propertyNodes = elem[0].ChildNodes;
        foreach (XmlNode node in propertyNodes)
        {
          if (node.NodeType == XmlNodeType.Element)
          {
            // Log each of the device properties
            AddMessage(string.Format("{0} = {1}", node.Name, node.InnerText));
          }
        }
      }
    }

    /*!
     * \fn private void btnSave_Click()
     * \brief Save button click for the fingerprint image in the ImageBox picturebox.
     * Save final image with prompt for file name and save image.
     * Catch Exception and display MessageBox with any errors
     */
    private void btnSave_Click(object sender, EventArgs e)
    {
      // Save final captured image
      try
      {
        if (_imageCaptured == false)
        {
          AddMessage("There is no image to save.");
          return;
        }
        SaveFileDialog dlg = new SaveFileDialog();
        dlg.Filter = "*.bmp|*.bmp";
        dlg.RestoreDirectory = true;
        if (dlg.ShowDialog() == DialogResult.OK)
        {
          ImageBox.Image.Save(dlg.FileName);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Problem saving image to file. Error:{0}", ex.Message));
      }
    }

    /*!
     * \fn private void btnAcquire_Click()
     * \brief Acquire configures the properties for capture and begins the acquire process.
     * This method will set any required and some options properties
     * The position and impression will be saved for the case when re-capture is required
     * Calls the StartAcquire method with the position and impression
     * StartAcquire is setup so it can also be called if a re-capture is required
     * Catch BioBaseException and Exception with any errors
     */
    private void btnAcquire_Click(object sender, EventArgs e)
    {
      try
      {
        if (_biobaseDevice.IsDeviceReady() == false)
        {
          AddMessage("Device is not ready to capture.");
          return;
        }

        // Remove any _biobaseDevice_DataAvailable image. 1. won't conflict with visualization image. 2. ensure security of personal data (GDPR)!!!!
        ImageBox.Image = null;
        ImageBox.Update();

        bool bAutoCaptureSupported = false;

        bool bFlexRollSupported = false;
        bool bFlexFlatSupported = false;

        if (_deviceType == null)
        {
           bFlexRollSupported = false;
           bFlexFlatSupported = false;
        }
        else if( (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_500P)
               || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_500PJ)
               || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_500)
               || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_1000PX)
               || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_1000P)
               || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_1000)
               || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_1000T))
        {
           bFlexFlatSupported = true;
           bFlexRollSupported = false;
           AddMessage(string.Format("Opening device type {0}, FlexFlat = true, FlexRoll=false", _deviceType));
        }
        else if ((_deviceType == LSEConst.DEVICE_TYPE_GUARDIAN_MODULE)
               || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_GUARDIAN_FW)
               || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_GUARDIAN_USB)
               || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_GUARDIAN_T)
               || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_GUARDIAN_R2)
               || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_GUARDIAN_L)
               || (_deviceType == LSEConst.DEVICE_TYPE_GUARDIAN)
               || (_deviceType == LSEConst.DEVICE_TYPE_PATROL)
               || (_deviceType == LSEConst.DEVICE_TYPE_GUARDIAN_200)
               || (_deviceType == LSEConst.DEVICE_TYPE_GUARDIAN_300))
        {
           bFlexFlatSupported = true;
           bFlexRollSupported = true;
           AddMessage(string.Format("Opening device type {0}, FlexFlat = true, FlexRoll=true", _deviceType));
        }
        else if (_deviceType == LSEConst.DEVICE_TYPE_GUARDIAN_45)
        {
           bFlexFlatSupported = false;
           bFlexRollSupported = false;
           AddMessage(string.Format("Opening device type {0}, FlexFlat = false, FlexRoll=false", _deviceType));
        }
        
        // Enable auto capture.
        // Code should first check if auto capture is supported else SetProperty will throw exception
        string test = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_DEVICE_AUTOCAPTURE_SUPPORTED);
        if (_biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_DEVICE_AUTOCAPTURE_SUPPORTED) == PropertyConstants.DEV_PROP_TRUE)
        {
           bAutoCaptureSupported = true;
           _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_ON, PropertyConstants.DEV_PROP_TRUE);
           AddMessage(string.Format("InFunc [btnAcquire_Click] DEV_PROP_DEVICE_AUTOCAPTURE_SUPPORTED  = true, set DEV_PROP_AUTOCAPTURE_ON =true"));
        }
        else
        {
           string strTF = "false";
           _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_ON, PropertyConstants.DEV_PROP_FALSE);
           if (_biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_ON) == PropertyConstants.DEV_PROP_TRUE)
           {
              strTF = "true";
           }
           _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_ON, PropertyConstants.DEV_PROP_FALSE);
           AddMessage(string.Format("InFunc [btnAcquire_Click] DEV_PROP_DEVICE_AUTOCAPTURE_SUPPORTED  = false,  DEV_PROP_AUTOCAPTURE_ON ={0}", strTF));

        }

        //Allow Autocontrast for flat if checkbox is selected but no Autocontrast for rolls
        if ((comboBox_Impression.Text == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_FLAT) && (checkBoxAutocontrast.Checked))
          _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCONTRAST_ON, PropertyConstants.DEV_PROP_TRUE);
        else
        {
          checkBoxAutocontrast.Checked = false; // Autocontrast should not be used for rolls - Rolled image can have uneven contrast
          _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCONTRAST_ON, PropertyConstants.DEV_PROP_FALSE);
        }

        //Check UI option for capture override options
        if(checkBoxAltTrigger.Checked && bAutoCaptureSupported)
        {
          _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_OVERRIDE_ON, PropertyConstants.DEV_PROP_TRUE);
          _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_OVERRIDE_TIME, "4000");
          if(radioButtonInsufficientObjectCount.Checked)
            _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_OVERRIDE_MODE, PropertyConstants.DEV_PROP_ON_INSUFFICIENT_COUNT);
          else
            _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_OVERRIDE_MODE, PropertyConstants.DEV_PROP_ON_INSUFFICIENT_QUALITY);
        }
        else
        {
           _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_OVERRIDE_ON, PropertyConstants.DEV_PROP_FALSE);
           checkBoxVisualization.Checked = true;
        }


        //Set option to check for spoof detection AKA presentation attack detection (PAD) 
        //Only set option if supported by device else BeginAcquire will return an error
        _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_SPOOF_DETECTION_ON, (checkBoxPAD.Checked)? PropertyConstants.DEV_PROP_TRUE : PropertyConstants.DEV_PROP_FALSE);

        // Set number of fingers (objects) being captured based on Position
        // This allows the application to annotate a finger (i.e. acquire "RightFour" with only 3 fingers
        _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_NUM_RQD_OBJECTS, comboBox_NumObjCapture.Text);
        //Int16 fingerCount = Int16.Parse(comboBox_NumObjCapture.Text);
        SetUILedColors(4); // Display all four UI status LEDs for LSE because they correspond to location finger is detected on platen.

        //Set option to allow the image resolution to be changed
        // Checked if device supports setting else the SetProperty will throw exception
        // Must be done before checking Flex Capture so it knows the resolution
        _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_IMAGE_RESOLUTION, (checkBox1000dpi.Checked)? PropertyConstants.DEV_PROP_RESOLUTION_1000 : PropertyConstants.DEV_PROP_RESOLUTION_500);


        //Check option for Flex Roll Area capture
        // Flex roll capture supported on Guardian, Guardian 300, Guardian 200, Guardian Module
        if (comboBox_Impression.Text == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL)
        {
          m_ImpressionModeRoll = true;
          if (checkBoxFlexRollCapture.Checked)
          {
            if ((_biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_ROLL_FLEXIBLE) != PropertyConstants.DEV_PROP_NOT_SET) && bFlexRollSupported)
            { // Flex roll property is settable...
              _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ROLL_FLEXIBLE, PropertyConstants.DEV_PROP_TRUE);
              _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, "-1 -1 800 748");
            }
            else
            {
              // checkBoxFlexRollCapture checked but flex rolls is NOT VALID for this device.
              checkBoxFlexRollCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
              // traditional (non-flex) capture area.
              _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, "0 0 0 0");
            }
          }
          else
          {
            // DEV_PROP_ROLL_FLEXIBLE property is persistant so it must be turned off if settable
            if (_biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_ROLL_FLEXIBLE) != PropertyConstants.DEV_PROP_NOT_SET)
                _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ROLL_FLEXIBLE, PropertyConstants.DEV_PROP_FALSE);
            // traditional (non-flex) capture area
            _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, "0 0 0 0");
          }

          // Must use use visualization window for flex roll capture
          if (checkBoxFlexRollCapture.Checked == true)
            checkBoxVisualization.Checked = true;
        }


        //Check option for Flex Flast Area capture
        // Flex flat capture supported on LScan 500P, LScan 500PJ, LScan 500, and LScan 1000PX, LScan 1000, Guardian, Guardian 300, Guardian 200, Guardian 100, Guardian Module
        else if (comboBox_Impression.Text == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_FLAT)
        {
          m_ImpressionModeRoll = false;
          if (checkBoxFlexFlatCapture.Checked)
          {
            string FlexArea = "-1 -1 800 748";

            if (_biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_IMAGE_RESOLUTION) == PropertyConstants.DEV_PROP_RESOLUTION_1000)
            { // 1000 dpi palm scanner
              switch (comboBox_Position.Text)
              {
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FULL_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_WRITERS_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LOWER_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_UPPER_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FULL_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_WRITERS_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LOWER_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_UPPER_PALM):
                  // checkBoxFlexFlatCapture checked but flex flat is NOT VALID for this device.
                  checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                  // traditional (non-flex) capture area.
                  FlexArea = "0 0 0 0";
                  break;

                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FOUR_FINGERS):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FOUR_FINGERS):
                case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS):
                case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_INDEXES_AND_MIDDLES):
                  FlexArea = "-1 -1 3200 3000";   //flex flat capture area for 4 fingers or 2 thumbs at 1000dpi
                  if (!bFlexFlatSupported) {
                    FlexArea = "0 0 0 0";
                    checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                  }
                  break;

                case (PropertyConstants.DEV_PROP_POS_TYPE_TWO_FINGERS):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX_AND_MIDDLE):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING_AND_LITTLE):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX_AND_MIDDLE):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING_AND_LITTLE):
                    checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option
                    FlexArea = "0 0 0 0";     // traditional (non-flex) capture area
                    break;
                default:  // And all the single finger positions
                  FlexArea = "-1 -1 3200 3000";   //flex flat capture area for 4 fingers or 2 thumbs at 1000dpi
                  if (!bFlexFlatSupported) {
                    FlexArea = "0 0 0 0";
                    checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                  }
                  //checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.

                  /// The single finger postion must be changed to DEV_PROP_POS_TYPE_BOTH_THUMBS for flex flat work!
                  /// BUT this hack will cause the FIR record to return DEV_PROP_POS_TYPE_BOTH_THUMBS instead of the requested position!!!
                  //FlexArea = "-1 -1 1600 1496";  //flex flat capture area for 2 fingers (and 1 finger) at 1000dpi
                  //comboBox_Position.Text = PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS;
                  break;
              }
            }
            else
            {   // 500dpi - Guardian and LScan palm
              switch (comboBox_Position.Text)
              {
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FULL_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_WRITERS_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LOWER_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_UPPER_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FULL_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_WRITERS_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LOWER_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_UPPER_PALM):
                  // checkBoxFlexFlatCapture checked but flex flat is NOT VALID for this postion.
                  checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                  // traditional (non-flex) capture area.
                  FlexArea = "0 0 0 0";
                  break;

                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FOUR_FINGERS):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FOUR_FINGERS):
                case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS):
                case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_INDEXES_AND_MIDDLES):
                  FlexArea = "-1 -1 1600 1496";   //flex flat capture area for 4 fingers or 2 thumbs at 500dpi
                  if (!bFlexFlatSupported) {
                    FlexArea = "0 0 0 0";
                    checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                  }
                  break;

                case (PropertyConstants.DEV_PROP_POS_TYPE_TWO_FINGERS):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX_AND_MIDDLE):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING_AND_LITTLE):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX_AND_MIDDLE):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING_AND_LITTLE):
                  FlexArea = "0 0 0 0";     // traditional (non-flex) capture area.
                  checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                  break;
               default:  // And all the single finger positions
                  FlexArea = "-1 -1 1600 1496";   //flex flat capture area for 4 fingers or 2 thumbs at 500dpi
                  if (!bFlexFlatSupported) {
                    FlexArea = "0 0 0 0";  // traditional (non-flex) capture area.
                                        checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                  }
                  //checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.

                  /// The single finger postion must be changed to DEV_PROP_POS_TYPE_BOTH_THUMBS for flex flat work!
                  /// BUT this hack will cause the FIR record to return DEV_PROP_POS_TYPE_BOTH_THUMBS instead of the requested position!!!
                  //FlexArea = "-1 -1 800 748";   //flex flat capture area for 2 fingers (and 1 finger) at 500dpi
                  //comboBox_Position.Text = PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS;
                  break;
              }
            }
            _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, FlexArea);

            // Must use use visualization window for flex flat capture
            if (checkBoxFlexFlatCapture.Checked == true)
              checkBoxVisualization.Checked = true;
          }// checkBoxFlexFlatCapture.Checked
          else
          {
            // traditional (non-flex) flat capture area
            _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, "0 0 0 0");
          }
        }//DEV_PROP_IMPR_TYPE_FINGERPRINT_FLAT
        else if (comboBox_Impression.Text == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL_VERTICAL)
        {// DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL_VERTICAL, DEV_PROP_IMPR_TYPE_FINGERPRINT_UNKNOWN, etc.
          _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, "0 0 0 0");
          _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_ON, PropertyConstants.DEV_PROP_FALSE);
        }
        else
        {  //DEV_PROP_IMPR_TYPE_FINGERPRINT_UNKNOWN, etc.
           _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, "0 0 0 0");
        }


        if (checkBoxVisualization.Checked == true)
        {
          // Setup visualization window...
          _biobaseDevice.SetVisualizationWindow(ImageBox.Handle, PropertyConstants.DEV_ID_VIS_FINGER_WND, BioBOsType.BIOB_WIN32OS);

          _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_VISUALIZATION_MODE, PropertyConstants.DEV_PROP_VISMODE_PREVIEW_ONLY);
          _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_VISUALIZATION_FULLIMAGE_ON, PropertyConstants.DEV_PROP_FALSE);
          _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_VISUALIZATION_BK_COLOR, PropertyConstants.DEV_PROP_DEFAULT_BK_COLOR);
        }


        // Reset LEDs in GUI on start of capture
        SetUILedColors(ActiveColor.gray, ActiveColor.gray, ActiveColor.gray, ActiveColor.gray, ActiveColor.gray);

        _biobaseDevice.mostRecentPosition = comboBox_Position.Text;        //Save position for StartAcquire and in case we need to re-capture. also used in SetOutputData
        _biobaseDevice.mostRecentImpression = comboBox_Impression.Text;    //Save impression for StartAcquire and in case we need to re-capture. also used in SetOutputData
        StartAcquire();

      }
      catch (BioBaseException ex)
      {
        MessageBox.Show(string.Format("Setting up device for Acquire BioBase error {0}", ex.Message));
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Setting up device for Acquire error {0}", ex.Message));
      }
    }

    /*!
     * \fn private void StartAcquire()
     * \brief Begins the acquire process. 
     * This method is called to start or re-start the BeginAcquisitionProcess
     * This method is setup so it can also be called if a re-capture is required
     * This method will set UI and device with prompts on what is being captured
     * When the BeginAcquisitionProcess method is called it may throw exception warnings that 
     * require opererator response.
     * \param  _biobaseDevice selected device object
     * \param  _biobaseDevice.mostRecentPosition in a input string defining the position being captureed
     * \param  _biobaseDevice.mostRecentImpression in a input string defining the impression being captureed
     * BeginAcquisitionProcess exception warning is:
     *  1. BIOB_REPLACE_PAD that can be ignored or prompt to cancel acquisition.
     * Catch BioBaseException and Exception with any errors or warnings
     */
    private void StartAcquire()
    {

      if (_biobaseDevice == null)
      {
        MessageBox.Show("Open device first");
        return;
      }
      try
      {
        if (_biobaseDevice.IsDeviceReady() == false)
        {
          MessageBox.Show("Device is not Opened.");
          return;
        }
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("Start Acquire BioBase error {0}", ex.Message));
        return;
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("Start Acquire error {0}", ex.Message));
        return;
      }

      try
      {
        // Must have try/catch to pick up warning returned by low level BioB_BeginAcquisitionProcess
        _biobaseDevice.BeginAcquisitionProcess();
      }
      catch (BioBaseException ex)
      {
        if (ex.ReturnCode == BioBReturnCode.BIOB_REPLACE_PAD)
        {
          // Optional check of Replace silicone membrane here. Code could be modified to always ignore warning.
          // with BIOB_REPLACE_PAD return code, there are two options. We won't prompt to replace silicone membrane here.
          // Device should be closed and re-opened when silicone membrane is replaced.
          // 1. Ignore the warning to replace silicone membrane; continue with Acquire (break)
          // 2. Cancel Acquire without changing device state (return)
          string emsg = string.Format("BioB_BeginAcquisitionProcess warning {0}.   Ignore replace silicone membrane warning?", ex.Message);
          DialogResult result = MessageBox.Show(emsg, "BioB_OpenDevice", MessageBoxButtons.YesNo);
          if (result == System.Windows.Forms.DialogResult.No)
          {
            _biobaseDevice.CancelAcquisition();
            return; // 2. Cancel Acquire without changing UI device state (return)
          }
        }
        else
        {
          throw new BioBaseException(ex.ReturnCode);
        }
      }

      SetDeviceState(DeviceState.device_opened_and_live);
      m_bAskRecapture = false;     // Used to confirm button is for contrast adjustment
      _biobaseDevice.mostResentKey = ActiveKeys.KEYS_OK_CONTRAST;
      _SetGuidanceElements();

      //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      //NOTE: This would be a good spot to add an UI Prompt for user on place the correct position and impression on the platen
      // For application using the visualizer logic this can be done by calling the _SetImageText() method.
      //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      if (checkBoxVisualization.Checked == true)
      {
        if (_biobaseDevice.mostRecentImpression == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL)
          _SetImageText("Roll finger horizontally!");
        else if (_biobaseDevice.mostRecentImpression == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL_VERTICAL)
          _SetImageText("Roll finger vertically!");
        else if (_biobaseDevice.mostRecentImpression == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_FLAT)
        {  //"Place position!"
          string msg = "Place " + _biobaseDevice.mostRecentPosition + "!";
          _SetImageText(msg);
        }
        else
          _SetImageText("Place fingers on platen.");
      }
    }

    /*!
     * \fn private void RescanImage()
     * \brief Calls StartAcquire to restart because of operator request
     * Called when previous capture had warning and opertor requests capture re-try
     */
    void RescanImage()
    {
      ImageBox.Image = null;
      ImageBox.Update();
      if(this.InvokeRequired)
        this.Invoke(new Action(() => RescanImage()));
      else
        StartAcquire();
    }


    /*!
     * \fn private void btnForce_Click()
     * \brief Force button click to manually override fingerprint auto capture force the fingerprint capture
     * Catch Exception and display MessageBox with any errors
     */
    private void btnForce_Click(object sender, EventArgs e)
    {
      try
      {
        AddMessage("Force capture with acquisition Override.");
        _biobaseDevice.RequestAcquisitionOverride();

        // Note: SetDeviceStatus is updated in _biobaseDevice_DataAvailable event
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Force capture error {0}", ex.Message));
      }
    }

    /*!
     * \fn private void btnCancelAcquire_Click()
     * \brief Cancel button click to abort the fingerprint capture
     * Catch Exception and display MessageBox with any errors
     */
    private void btnCancelAcquire_Click(object sender, EventArgs e)
    {
      try
      {
        AddMessage("Cancel capture.");
        _biobaseDevice.CancelAcquisition();
        SetDeviceState(DeviceState.device_opened_and_capture_cancelled);

        //Reset device's LEDs, TFT display or Touch display here
        _biobaseDevice.mostResentKey = ActiveKeys.KEYS_NONE;
        _ResetStatusElements();
        _ResetGuidanceElements();
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Cancel capture error {0}", ex.Message));
      }
    }

    /*!
     * \fn private void btnAdjust_Click()
     * \brief Adjust button click will adjust the contrast optimization during the fingerprint capture process
     * Catch Exception and display MessageBox with any errors
     */
    private void btnAdjust_Click(object sender, EventArgs e)
    {
      try
      {
        AddMessage("Manually make capture adjustments.");
        // Manually make adjustments optimize contrast 
        _biobaseDevice.AdjustAcquisitionProcess(PropertyConstants.PROC_ADJUST_TYPE_OPTIMIZE_CONTRAST, null);
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Adjust capture error {0}", ex.Message));
      }
    }

    /*!
     * \fn private void checkBoxFlexFlatCapture_CheckedChanged()
     * \brief Flex flat and roll capture require visualization
     */
    private void checkBoxFlexFlatCapture_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxFlexFlatCapture.Checked == true)
      {
        checkBoxVisualization.Enabled = false;
        checkBoxVisualization.Checked = true;
      }

      if ((checkBoxFlexFlatCapture.Checked == false) && (checkBoxFlexRollCapture.Checked == false))
      {
        checkBoxVisualization.Enabled = true;
      }
    }

    /*!
     * \fn private void checkBoxFlexRollCapture_CheckedChanged()
     * \brief Flex flat and roll capture require visualization
     */
    private void checkBoxFlexRollCapture_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxFlexRollCapture.Checked == true)
      {
        checkBoxVisualization.Enabled = false;
        checkBoxVisualization.Checked = true;
      }

      if ((checkBoxFlexFlatCapture.Checked == false) && (checkBoxFlexRollCapture.Checked == false))
      {
        checkBoxVisualization.Enabled = true;
      }

    }

    /*!
     * \fn private void comboBox_Position_SelectedIndexChanged()
     * \brief adjust the number of objects (fingers) being captured based on position.
     * This method does not account for annotation of fingers.
     * 
     * Catch Exception and display MessageBox with any errors
     */
    private void comboBox_Position_SelectedIndexChanged(object sender, EventArgs e)
    {
      switch(comboBox_Position.Text)
      {
        case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_THUMB):
        case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX):
        case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_MIDDLE):
        case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING):
        case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LITTLE):
        case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_THUMB):
        case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX):
        case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_MIDDLE):
        case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING):
        case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LITTLE):

        case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_WRITERS_PALM):
        case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LOWER_PALM):
        case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_WRITERS_PALM):
        case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LOWER_PALM):
          comboBox_NumObjCapture.Text = "1";
          // Default to capture 1 object.
          break;
        case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS):
        case (PropertyConstants.DEV_PROP_POS_TYPE_TWO_FINGERS):
        case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX_AND_MIDDLE):
        case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING_AND_LITTLE):
        case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX_AND_MIDDLE):
        case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING_AND_LITTLE):
          // Default to capture 2 objects.
          comboBox_NumObjCapture.Text = "2";
          break;
        case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FOUR_FINGERS):
        case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FOUR_FINGERS):
        case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_INDEXES_AND_MIDDLES):
          // Default to capture 4  objects. In case of annotation, this will need to edit
          comboBox_NumObjCapture.Text = "4";
          break;

        case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FULL_PALM):
        case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_UPPER_PALM):
        case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FULL_PALM):
        case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_UPPER_PALM):
          // Default to capture 5 objects. In case of annotation, this will need to edit
          comboBox_NumObjCapture.Text = "5";
          break;
      }
    }


    /*!
     * \fn private void SetDeviceState()
     * \brief Update the UI with device state with status of selected device and adjust button enable/disable based on that state.
     * Catch Exception and display MessageBox with any errors
     */
    public void SetDeviceState(DeviceState deviceState)
    {
      try
      {
        if (this.InvokeRequired)
        {
          // Invoke when called from events outside of UI thread
          Invoke((Action<DeviceState>)SetDeviceState, deviceState);
        }
        else
        {
          _deviceState = deviceState;
          switch (_deviceState)
          {
            case DeviceState.device_not_connected:
              DeviceStatus.Text = "Device Not Connected";
              DeviceStatus.BackColor = Color.Red;
              DeviceStatus.ForeColor = Color.White;

              comboBox_NumObjCapture.Enabled = false;
              comboBox_Position.Enabled = false;
              comboBox_Impression.Enabled = false;
              btnOpenDevice.Enabled = false;
              btnCloseDevice.Enabled = false;
              btnProperties.Enabled = false;
              btnSaveImage.Enabled = false;
              btnAcquire.Enabled = false;
              btnForce.Enabled = false;
              btnCancelAcquire.Enabled = false;
              btnAdjust.Enabled = false;
              checkBoxAltTrigger.Enabled = false;
              radioButtonInsufficientQuality.Enabled = false;
              radioButtonInsufficientObjectCount.Enabled = false;
              checkBoxAutocontrast.Enabled = false;
              checkBoxPAD.Enabled = false;
              checkBox1000dpi.Enabled = false;
              checkBoxFlexRollCapture.Enabled = false;
              checkBoxFlexFlatCapture.Enabled = false;
              checkBoxVisualization.Enabled = false;

              btnUserControls.Enabled = false;
              _imageCaptured = false;
              break;

            case DeviceState.device_connected_and_not_opened:
              DeviceStatus.Text = "Device Connected";
              DeviceStatus.BackColor = Color.Orange;
              DeviceStatus.ForeColor = Color.White;

              comboBox_NumObjCapture.Enabled = false;
              comboBox_Position.Enabled = false;
              comboBox_Impression.Enabled = false;
              btnOpenDevice.Enabled = true;
              btnCloseDevice.Enabled = false;
              btnProperties.Enabled = false;
              btnSaveImage.Enabled = false;
              btnAcquire.Enabled = false;
              btnForce.Enabled = false;
              btnAdjust.Enabled = false;
              btnCancelAcquire.Enabled = false;
              checkBoxAltTrigger.Enabled = false;
              radioButtonInsufficientQuality.Enabled = false;
              radioButtonInsufficientObjectCount.Enabled = false;
              checkBoxAutocontrast.Enabled = false;
              checkBoxPAD.Enabled = false;
              checkBox1000dpi.Enabled = false;
              checkBoxFlexRollCapture.Enabled = false;
              checkBoxFlexFlatCapture.Enabled = false;
              checkBoxVisualization.Enabled = false;

              btnUserControls.Enabled = false;
              _imageCaptured = false;
              break;

            case DeviceState.device_opened_and_not_live:
              DeviceStatus.Text = "Device Open";
              DeviceStatus.BackColor = Color.Green;
              DeviceStatus.ForeColor = Color.White;

              comboBox_NumObjCapture.Enabled = true;
              comboBox_Position.Enabled = true;
              comboBox_Impression.Enabled = true;
              btnOpenDevice.Enabled = false;
              btnCloseDevice.Enabled = true;
              btnProperties.Enabled = true;
              btnSaveImage.Enabled = false;
              btnAcquire.Enabled = true;
              btnForce.Enabled = false;
              btnAdjust.Enabled = false;
              btnCancelAcquire.Enabled = false;
              checkBoxAltTrigger.Enabled = true;
              radioButtonInsufficientQuality.Enabled = true;
              radioButtonInsufficientObjectCount.Enabled = true;
              checkBoxAutocontrast.Enabled = true;
              checkBoxPAD.Enabled = true;
              checkBox1000dpi.Enabled = true;
              checkBoxFlexRollCapture.Enabled = true;
              checkBoxFlexFlatCapture.Enabled = true;
              checkBoxVisualization.Enabled = true;

              btnUserControls.Enabled = true;
              _imageCaptured = false;
              break;

            case DeviceState.device_opened_and_live:
              DeviceStatus.Text = "Acquiring";
              DeviceStatus.BackColor = Color.LightPink;
              DeviceStatus.ForeColor = Color.Black;

              comboBox_Position.Enabled = false;
              comboBox_Impression.Enabled = false;
              btnOpenDevice.Enabled = false;
              btnCloseDevice.Enabled = true;
              btnProperties.Enabled = false;
              btnSaveImage.Enabled = false;
              btnAcquire.Enabled = false;
              btnForce.Enabled = true;
              btnAdjust.Enabled = !m_ImpressionModeRoll; // true;
              btnCancelAcquire.Enabled = true;

              btnUserControls.Enabled = false;
              _imageCaptured = false;
              break;

            case DeviceState.device_opened_and_image_captured:
              DeviceStatus.Text = "Fingerprint Captured";
              DeviceStatus.BackColor = Color.Green;
              DeviceStatus.ForeColor = Color.White;

              comboBox_NumObjCapture.Enabled = true;
              comboBox_Position.Enabled = true;
              comboBox_Impression.Enabled = true;
              btnOpenDevice.Enabled = false;
              btnCloseDevice.Enabled = true;
              btnProperties.Enabled = true;
              btnSaveImage.Enabled = true;
              btnAcquire.Enabled = true;
              btnForce.Enabled = false;
              btnAdjust.Enabled = false;
              btnCancelAcquire.Enabled = false;
              checkBoxAltTrigger.Enabled = true;
              radioButtonInsufficientQuality.Enabled = true;
              radioButtonInsufficientObjectCount.Enabled = true;
              checkBoxAutocontrast.Enabled = true;
              checkBoxPAD.Enabled = true;
              checkBox1000dpi.Enabled = true;
              checkBoxFlexRollCapture.Enabled = true;
              checkBoxFlexFlatCapture.Enabled = true;
              checkBoxVisualization.Enabled = true;

              btnUserControls.Enabled = true;
              _imageCaptured = true;    // allow image to be saved

              _ResetStatusElements();
              _ResetGuidanceElements();
              break;

            case DeviceState.device_opened_and_capture_cancelled:
              DeviceStatus.Text = "Cancelled";
              DeviceStatus.BackColor = Color.Green;
              DeviceStatus.ForeColor = Color.White;

              comboBox_NumObjCapture.Enabled = true;
              comboBox_Position.Enabled = true;
              comboBox_Impression.Enabled = true;
              btnOpenDevice.Enabled = false;
              btnCloseDevice.Enabled = true;
              btnProperties.Enabled = true;
              btnSaveImage.Enabled = false;
              btnAcquire.Enabled = true;
              btnForce.Enabled = false;
              btnAdjust.Enabled = false;
              btnCancelAcquire.Enabled = false;
              checkBoxAltTrigger.Enabled = true;
              radioButtonInsufficientQuality.Enabled = true;
              radioButtonInsufficientObjectCount.Enabled = true;
              checkBoxAutocontrast.Enabled = true;
              checkBoxPAD.Enabled = true;
              checkBox1000dpi.Enabled = true;
              checkBoxFlexRollCapture.Enabled = true;
              checkBoxFlexFlatCapture.Enabled = true;
              checkBoxVisualization.Enabled = true;

              btnUserControls.Enabled = true;
              _imageCaptured = false;
              break;
          }
        }
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("SetDeviceState update of UI error {0}", ex.Message));
        return;
      }
    }

    /////////////////////////////////////////////////////////////////////////////////////
    // Delegate for updating UI controls that setup that needs to be done after openDevice
    // UI is updated in CreateBioBaseDevice worker thread after successful open device.
    // CreateBioBaseDevice is not in UI thread because it can take up to 30 seconds to open a device
    private delegate void EnableControlDelegate(object data, bool val);
    private void EnableControl(object data, bool val)
    {
      System.Windows.Forms.Control ctrl = (System.Windows.Forms.Control)data;
      ctrl.Enabled = val;
    }
    // Delegate for Checked controls that setup that needs to be done after openDevice
    private delegate void CheckedControlDelegate(object data, bool val);
    private void CheckedControl(object data, bool val)
    {
      System.Windows.Forms.CheckBox ctrl = (System.Windows.Forms.CheckBox)data;
      ctrl.Checked = val;
    }

    /*!
     * \enum public enum comboBoxMethod
     * \brief Describes function to be done on the conboBox during device initialization.
     */
    public enum comboBoxMethod
    {
      Clear = 0,
      ItemsAdd,
      SelectedIndex
    };
    // Delegate for comboBox setup that needs to be done after openDevice
    // Code uses getproperty to add positions and impressions supported by the newly opened device
    private delegate void UpdatecomboBoxDelegate(object data, comboBoxMethod method, string val);
    private void UpdatecomboBox(object data, comboBoxMethod method, string val)
    {
      System.Windows.Forms.ComboBox cb = (System.Windows.Forms.ComboBox)data;
      switch (method)
      {
        case comboBoxMethod.Clear:
          cb.Items.Clear();
          break;
        case comboBoxMethod.ItemsAdd:
          cb.Items.Add(val);
          break;
        case comboBoxMethod.SelectedIndex:
          int nIndex = Int32.Parse(val);
          cb.SelectedIndex = nIndex;
          break;
      }
    }

    /*!
     * \fn public void CreateBioBaseDevice()
     * \brief CreateBioBaseDevice method is run in a thread because the NativeOpenDevice call can take 10 to 30 seconds to complete
     * Method run in thread to not block UI for long initialization time.
     * Becase care must be taken to not access device properties or start acquire until after device is Open, the
     * UI controls are also updated within this thread after the opendevice is successful.
     * INPUT: selectDevice - Device ID of device to be openned.
     * Catch BioBaseException and Exception and log errors
     */
    public void CreateBioBaseDevice(string selectDevice)
    {
      try
      {
        // Check if there is a device already opened
        if (_biobaseDevice == null)
        {

          if (selectDevice != null)
          {
            AddMessage(string.Format("Opening device {0}", selectDevice));

            // Find selected device in our devices list
            // Then open the device
            for (int cnt = 0; cnt < _biobaseDevices.Length; cnt++)
            {
              if (_biobaseDevices[cnt].DeviceId == selectDevice)
              {
                // InitProgress event is defined in the iBioBase class because the event is triggered before the IBioBaseDevice object is created
                AddMessage("registering event 'IBioBaseDevice.InitProgress'");
                _biobase.InitProgress += new EventHandler<BioBaseInitProgressEventArgs>(_biobaseDevice_Init);

                _deviceType = _biobaseDevices[cnt].ModelName;
                AddMessage(string.Format("Opening device type {0}", _deviceType));

                while (true)
                {
                  try
                  {
                    // Must have try/catch to pick up warning and errors returned by low level BioB_OpenDevice
                    _biobase.OpenDevice(_biobaseDevices[cnt], out _biobaseDevice);
                    //NOTE: _biobase.OpenDevice can return with exception because 
                    //      devImpl.Initialize can return with exception because 
                    //      Interop.OpenDevice will throw a exception on warnings or errors.
                    //      catch exception here so UI can notify operator and prompted for action.
                    break;
                  }
                  catch (BioBaseException ex)
                  {
                    if (ex.ReturnCode > BioBReturnCode.BIOB_SUCCESS)
                    {
                      // device is OPENED but with warning.
                      // positive return code is a warning, prompt to continue or fix
                      AddMessage(string.Format("BioB_OpenDevice warning {0}", ex.Message));


                      if (ex.ReturnCode == BioBReturnCode.BIOB_OPTICS_SURFACE_DIRTY)
                      {
                        // with BIOB_OPTICS_SURFACE_DIRTY return code, there are three options.
                        // 1. Ignore the dirty platen; continue with open (break)
                        // 2. abort open device (return)
                        // 3. Have operator clean the device platen and re-open device to re-check cleanliness (continue)
                        m_bMsgBoxOpened = true;
                        string msg = string.Format("BioB_OpenDevice warning {0}.   Clean Device and retry?", ex.Message);
                        DialogResult result = MessageBox.Show(msg, "BioB_OpenDevice", MessageBoxButtons.AbortRetryIgnore);
                        m_bMsgBoxOpened = false;
                        if (result == System.Windows.Forms.DialogResult.Ignore)
                          break;  // 1. Ignore the dirty platen; continue with open (break)
                        else if (result == System.Windows.Forms.DialogResult.Abort)
                        { // 2. abort open device (return)
                          CloseDevice();
                          return;     //Close device and return without initializing UI
                        }
                        else
                        { // 3. Have operator clean the device platen and re-open device to re-check cleanliness (continue)
                          CloseDevice();
                          continue;     // Close device and retry openning device after the platen was cleaned
                        }
                      }
                      else if (ex.ReturnCode == BioBReturnCode.BIOB_REPLACE_PAD)
                      {
                        // with BIOB_REPLACE_PAD return code, there are three options.
                        // 1. Ignore the warning to replace silicone membrane; continue with open (break)
                        // 2. abort open device (return)
                        // 3. Have operator replace the silicone membrane, reset the membrane usage and re-open device to check cleanliness of new membrane (continue)
                        string msg = string.Format("BioB_OpenDevice warning {0}.   Replace silicone membrane and retry?", ex.Message);
                        DialogResult result = MessageBox.Show(msg, "BioB_OpenDevice", MessageBoxButtons.YesNoCancel);
                        if (result == System.Windows.Forms.DialogResult.No)
                          break;  // 1. Ignore the warning to replace silicone membrane; continue with open (break)
                        else if (result == System.Windows.Forms.DialogResult.Cancel)
                        { // 2. abort open device (return)
                          CloseDevice();
                          return; //Close device and return without initializing UI
                        }
                        else
                        {
                          // 3. Have operator replace the silicone membrane, reset the membrane usage and re-open device to check cleanliness of new membrane (continue)

                          //OPTIONAL get properties of silicone membrane life expectancy with four GetProperty calls.
                          string usageMax = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_SILICONE_MEMBRANE_MAX_USAGE_COUNT);
                          string usageCurrent = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_SILICONE_MEMBRANE_CURRENT_USAGE_COUNT);
                          string daysMax = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_SILICONE_MEMBRANE_MAX_LIFE_DAYS);
                          string daysCurrent = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_SILICONE_MEMBRANE_REPLACEMENT_DATE);

                          // Prompt the operator to replace the silicone memberane and then clicked OK button
                          // IF the OK button is pressed, the silicone membrane counter will be reset for another 8000 captures
                          // IF the OK button was not pressed, the silicone membrane counter warning message will be displayed again on the next attempt to start capture.
                          if (MessageBox.Show("Replace silicone membrane then click OK,", "Replace silicone membrane", MessageBoxButtons.OK) == DialogResult.OK)
                            _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_SILICONE_MEMBRANE_REPLACE, PropertyConstants.DEV_PROP_TRUE);
                          CloseDevice();
                          continue;     // Close device and retry openning device after the platen was cleaned
                          // break;     // we don't want to continue with open because we need check cleanliness of new silicone membrane
                        }
                      }
                      else 
                      {
                        // with unknown warning return code, there are three options.
                        // 1. Ignore the warning (break)
                        // 2. abort open device (return)
                        string msg = string.Format("BioB_OpenDevice warning {0} - Continue?", ex.Message);
                        DialogResult result = MessageBox.Show(msg, "BioB_OpenDevice", MessageBoxButtons.YesNo);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                          break;  // 1. Ignore the warning; continue with open (break)
                        if (result == System.Windows.Forms.DialogResult.No)
                        {
                          CloseDevice();
                          return; // 2. abort open device (return)
                        }
                      }
                    }


                    else if (ex.ReturnCode < BioBReturnCode.BIOB_SUCCESS)
                    {
                        if (ex.ReturnCode == BioBReturnCode.BIOB_OBJECT_ON_PLATEN_DURING_CALIBRATION)
                        {
                            // device open failed with BIOB_OBJECT_ON_PLATEN_DURING_CALIBRATION error.
                            AddMessage(string.Format("BioB_OpenDevice Error {0}. Object or excessive dirt detected on the platen during the collection of background images.", ex.Message));
                            // with BIOB_OBJECT_ON_PLATEN_DURING_CALIBRATION return code, there are two options.
                            // 1. Have operator clean the device platen and re-open device to re-check cleanliness (continue
                            // 2. abort open device (return)
                            string msg = string.Format("BioB_OpenDevice Error {0}. Retry by removing object from platen, cleaning and clicking Yes. Click No to quit", ex.Message);
                            DialogResult result = MessageBox.Show(msg, "BioB_OpenDevice", MessageBoxButtons.YesNo);
                            if (result == System.Windows.Forms.DialogResult.Yes)
                            { 
                                //1. Have operator clean the device platen and re-open device to re-check cleanliness (continue)
                                continue;
                            }
                        }
                        {
                            // device is NOT OPENED because of error.
                            // If negative error, Close device to clean up and return without initializing UI
                            CloseDevice();
                            if (ex.ReturnCode != BioBReturnCode.BIOB_OBJECT_ON_PLATEN_DURING_CALIBRATION)
                            {
                                string msg = string.Format("Device closed. Fix BioB_OpenDevice error {0} and try again", ex.Message);
                                AddMessage(msg);
                                MessageBox.Show(msg, "BioBase4 Open Device", MessageBoxButtons.OK);
                            }
                            return;
                        }
                    }
                  }
                } //while(true)
                break;
              }
            }

            if (_biobaseDevice != null)
            {
              //OpenDevice was successful, now update the UI
              RegisterEventHandlers();

              if (DeviceInfoBox.InvokeRequired)
                DeviceInfoBox.Invoke(new EnableControlDelegate(EnableControl), new object[] { DeviceInfoBox, false });
              else
                EnableControl(DeviceInfoBox, false);

              //////////////////////////////////////////
              // Fill Position combo box for this device
              string supportedPositions = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_DEVICE_AVAILABLE_POSITION_TYPES);
              string[] positionArray = supportedPositions.Split(' ');

              //comboBox_Position.Items.Clear();
              if (comboBox_Position.InvokeRequired)
                comboBox_Position.Invoke(new UpdatecomboBoxDelegate(UpdatecomboBox), new object[] { comboBox_Position, comboBoxMethod.Clear, null });
              else
                UpdatecomboBox(comboBox_Position, comboBoxMethod.Clear, null);

              foreach (string pos in positionArray)
              {
                //comboBox_Position.Items.Add(pos);
                if (comboBox_Position.InvokeRequired)
                  comboBox_Position.Invoke(new UpdatecomboBoxDelegate(UpdatecomboBox), new object[] { comboBox_Position, comboBoxMethod.ItemsAdd, pos });
                else
                  UpdatecomboBox(comboBox_Position, comboBoxMethod.ItemsAdd, pos);
              }

              ////////////////////////////////////////////
              // Fill Impression combo box for this device
              string supportedImpressions = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_DEVICE_AVAILABLE_IMPRESSION_TYPES);
              string[] impressionsArray = supportedImpressions.Split(' ');

              //comboBox_Impression.Items.Clear();
              if (comboBox_Impression.InvokeRequired)
                comboBox_Impression.Invoke(new UpdatecomboBoxDelegate(UpdatecomboBox), new object[] { comboBox_Impression, comboBoxMethod.Clear, null });
              else
                UpdatecomboBox(comboBox_Impression, comboBoxMethod.Clear, null);

              foreach (string impress in impressionsArray)
              {
                //comboBox_Impression.Items.Add(impress);
                if (comboBox_Impression.InvokeRequired)
                  comboBox_Impression.Invoke(new UpdatecomboBoxDelegate(UpdatecomboBox), new object[] { comboBox_Impression, comboBoxMethod.ItemsAdd, impress });
                else
                  UpdatecomboBox(comboBox_Impression, comboBoxMethod.ItemsAdd, impress);
              }

              // Option to select the first valid entry in each of the combo boxes so we can save time and quickly click Acquire
              //select first valid position in list
              //comboBox_Position.SelectedIndex = 1;
              if (comboBox_Position.InvokeRequired)
                comboBox_Position.Invoke(new UpdatecomboBoxDelegate(UpdatecomboBox), new object[] { comboBox_Position, comboBoxMethod.SelectedIndex, "1" });
              else
                UpdatecomboBox(comboBox_Position, comboBoxMethod.SelectedIndex, "1");

              //comboBox_Impression.SelectedIndex = 1;
              if (comboBox_Impression.InvokeRequired)
                comboBox_Impression.Invoke(new UpdatecomboBoxDelegate(UpdatecomboBox), new object[] { comboBox_Impression, comboBoxMethod.SelectedIndex, "1" });
              else
                UpdatecomboBox(comboBox_Impression, comboBoxMethod.SelectedIndex, "1");

              //Set option to check for spoof detection AKA presentation attack detection (PAD) 
              //Only allo option to be set if supported by device else BeginAcquire will return an error
              bool bspoof;
              if(_biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_DEVICE_SPOOF_DETECTION_SUPPORTED) == PropertyConstants.DEV_PROP_TRUE)
                bspoof = true;
              else
                bspoof = false;
              if (checkBoxPAD.InvokeRequired)
              {
                checkBoxPAD.Invoke(new EnableControlDelegate(EnableControl), new object[] { checkBoxPAD, bspoof });
                checkBoxPAD.Invoke(new CheckedControlDelegate(CheckedControl), new object[] { checkBoxPAD, bspoof });
              }
              else
              {
                EnableControl(checkBoxPAD, bspoof);
                CheckedControl(checkBoxPAD, bspoof);
              }

              //Set option to allow the image resolution to be changed
              // Checked if device supports setting else the SetProperty will throw exception
              string resolution = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_DEVICE_AVAILABLE_IMAGE_RESOLUTIONS);
              bool bdpi;
              if (resolution.Contains(PropertyConstants.DEV_PROP_RESOLUTION_1000))
                bdpi = true;
              else
                bdpi = false;
              if (checkBox1000dpi.InvokeRequired)
              {
                checkBox1000dpi.Invoke(new EnableControlDelegate(EnableControl), new object[] { checkBox1000dpi, bdpi });
                checkBox1000dpi.Invoke(new CheckedControlDelegate(CheckedControl), new object[] { checkBox1000dpi, bdpi });
              }
              else
              {
                EnableControl(checkBox1000dpi, bdpi);
                CheckedControl(checkBox1000dpi, bdpi);
              }

              //Reset device's LEDs, TFT display or Touch display here at start if previous cycle didn't turn everything off
              _biobaseDevice.mostResentKey = ActiveKeys.KEYS_NONE;
              _ResetStatusElements();
              _ResetGuidanceElements();

              m_scannerOpen = true;
              AddMessage("OpenDevice succeeded");
              SetDeviceState(DeviceState.device_opened_and_not_live);
            }
          }
        }
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("InitializeBioBase IBioBaseDevice error {0}", ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("InitializeBioBase IBioBaseDevice error {0}", ex.Message));
      }
      finally
      {
      }
    }

    /*!
     * \fn public void CloseDevice()
     * \brief CloseDevice unregisters device event handlers and disposes of the _biobaseDevice device object.
     * Catch Exception and log errors
     */
    void CloseDevice()
    {
      try
      {
        // Remove any _biobaseDevice_DataAvailable image. 1. won't conflict with visualization image. 2. ensure security of personal data (GDPR)!!!!
        if(_biobaseDevice.mostRecentImpression != "")
        {
            ImageBox.Image = null;
            ImageBox.Update();
        }

        if (_biobaseDevice != null)
        {
          //reset device's LEDs, TFT display or Touch display here
          _biobaseDevice.mostResentKey = ActiveKeys.KEYS_NONE;
          _ResetStatusElements();
          _ResetGuidanceElements();

          _biobaseDevice.DetectedObject -= _biobaseDevice_DetectedObject;
          _biobaseDevice.DataAvailable -= _biobaseDevice_DataAvailable;
          _biobaseDevice.AcquisitionComplete -= _biobaseDevice_AcquisitionComplete;
          _biobaseDevice.AcquisitionStart -= _biobaseDevice_AcquisitionStart;
          _biobaseDevice.ScannerUserOutput -= _biobaseDevice_ScannerUserOutput;
          _biobaseDevice.ScannerUserInput -= _biobaseDevice_ScannerUserInput;
          _biobaseDevice.ObjectCount -= _biobaseDevice_ObjectCount;
          _biobaseDevice.ObjectQuality -= _biobaseDevice_ObjectQuality;
          _biobaseDevice.Preview -= _biobaseDevice_Preview;

          _biobase.InitProgress -= _biobaseDevice_Init;

          AddMessage("calling method 'IBioBaseDevice.Dispose'");
          _biobaseDevice.Dispose();
          _biobaseDevice = null;
        }
      }
      catch (Exception ex)
      {
        // log but ignore errors on closing
        AddMessage(string.Format("CloseDevice error {0}", ex.Message));
      }
      finally
      {
        if (DeviceInfoBox.InvokeRequired)
            DeviceInfoBox.Invoke(new EnableControlDelegate(EnableControl), new object[] { DeviceInfoBox, true });
        else
            EnableControl(DeviceInfoBox, true);

        SetDeviceState(DeviceState.device_connected_and_not_opened);
        m_scannerOpen = false;
      }
    }

    /*!
     * \fn public void AddMessage()
     * \brief The texboxLog is a diagnostic tool to help the developer track the work flow a this sample application.
     * The max size in the textbox is defined at 50000 bytes in InitializeComponent
     * The method will limit the log to 10,000 bytes.
     */
    public void AddMessage(string eventName)
    {
      if (textBoxLog.InvokeRequired)
      {
        // Invoke when called from events outside of UI thread
        Invoke((Action<string>)AddMessage, eventName);
      }
      else
      {
        if (textBoxLog.TextLength + eventName.Length > textBoxLog.MaxLength)
        {
          const int truncatingSize = 10000;
          string textTB = textBoxLog.Text;
          textTB = textTB.Remove(0, truncatingSize);
          textBoxLog.Text = textTB;
        }
        this.textBoxLog.Text += eventName + "\r\n";
        this.textBoxLog.SelectionStart = textBoxLog.TextLength;
        this.textBoxLog.ScrollToCaret();
        this.Refresh();
      }
    }

    /*!
     * \fn public void btnSaveLog_Click()
     * \brief prompts to save the contents of the texboxLog to help the developer track the work flow a this sample application.
     * Catch Exception and log errors
     */
    private void btnSaveLog_Click(object sender, EventArgs e)
    {
      try
      {
        SaveFileDialog dlg = new SaveFileDialog();
        dlg.Filter = "*.txt|*.txt";
        dlg.RestoreDirectory = true;
        if (dlg.ShowDialog() == DialogResult.OK)
        {
          File.WriteAllText(dlg.FileName, textBoxLog.Text);
        }
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("btnSaveLog_Click error {0}", ex.Message));
      }
    }

    /*!
     * \fn public void SetUILedColors()
     * \brief Update status of status LEDs based on quality state of each of the four fingers.
     *  The 5th (upper palm) status "LED" enabled for palm capture
     */
    public void SetUILedColors(ActiveColor led1, ActiveColor led2, ActiveColor led3, ActiveColor led4, ActiveColor led5)
    {
      if (LEDlightPanel.InvokeRequired)
        // Invoke when called from events outside of GUI thread
        Invoke((Action<ActiveColor, ActiveColor, ActiveColor, ActiveColor, ActiveColor>)SetUILedColors, led1, led2, led3, led4, led5);
      else
      {
        LEDlightPanel.SetUILedColors(led1, led2, led3, led4, led5);
      }
    }

    /*!
     * \fn public void SetUILedColors()
     * \brief Set the number of status LEDs being displayed.
     * The number of LEDs can be between 0 and 5.
     */
    public void SetUILedColors(int count)
    {
      LEDlightPanel.LedCount = count;
    }
    /*!
     * \fn public void ConvertQualityToIndicatorColor()
     * \brief Determine the color of the UI status LEDs based on quality from _biobaseDevice_ObjectQuality event.
     */
    static public ActiveColor ConvertQualityToIndicatorColor(BioBObjectQualityState qualityCode)
    {
      ActiveColor retval = ActiveColor.gray;
      switch (qualityCode)
      {
        case BioBObjectQualityState.BIOB_OBJECT_NOT_PRESENT:
          retval = ActiveColor.gray;
          break;
        case BioBObjectQualityState.BIOB_OBJECT_TOO_LIGHT:
        case BioBObjectQualityState.BIOB_OBJECT_TOO_DARK:
        case BioBObjectQualityState.BIOB_OBJECT_BAD_SHAPE:
          retval = ActiveColor.red;
          break;
        case BioBObjectQualityState.BIOB_OBJECT_GOOD:
          retval = ActiveColor.green;
          break;
        case BioBObjectQualityState.BIOB_OBJECT_POSITION_NOT_OK:
        case BioBObjectQualityState.BIOB_OBJECT_CORE_NOT_PRESENT:
        case BioBObjectQualityState.BIOB_OBJECT_TRACKING_NOT_OK:
        case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_HIGH:
        case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_LEFT:
        case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_RIGHT:
        case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_LOW:
        case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_HIGH:
        case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_LEFT:
        case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_RIGHT:
        case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_LOW:
        case BioBObjectQualityState.BIOB_OBJECT_OCCLUSION:
        case BioBObjectQualityState.BIOB_OBJECT_CONFUSION:
        case BioBObjectQualityState.BIOB_OBJECT_ROTATED_CLOCKWISE:
        case BioBObjectQualityState.BIOB_OBJECT_ROTATED_COUNTERCLOCKWISE:
          retval = ActiveColor.yellow;
          break;
        default:
          retval = ActiveColor.red;
          break;
      }
      return retval;
    }

    /*!
     * \fn public void btnUserControls_Click()
     * \brief popup dialog to test various  parts of the fingerprint devices
     */
    private void btnUserControls_Click(object sender, EventArgs e)
    {
      UserControlDialog dlg = new UserControlDialog(_biobaseDevice);
      dlg.ShowDialog();
    }

    /*!
     * \fn public void SystemInformation()
     * \brief This functions collection OPTIONAL information to aid in debugging 
     *  Use .net ManagementClass classes to gather Computer system information
     * This is an optional function used to gather information to aid in debugging 
     */
    private void SystemInformation()
    {
      // SYSTEM information....
      try
      {
        AddMessage("System Information:");
        AddMessage(string.Format(" Application: {0} version:{1}", System.AppDomain.CurrentDomain.FriendlyName, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()));

        String OSVersion = GetOSVersion();
        AddMessage(string.Format(" OS = {0}", OSVersion));

        using (ManagementClass mc = new ManagementClass("Win32_PhysicalMemory"))
        {
          long MemorySize = 0;
          long memCap = 0;
          foreach (ManagementObject mo in mc.GetInstances())
          {
            memCap = Convert.ToInt64(mo["Capacity"]);
            MemorySize += memCap;    // Add installled memory stick size
          }
          String TotalMemory = String.Format("{0:0.00} GB", ((MemorySize / 1024) / 1024) / 1000.0);
          AddMessage(string.Format(" PhysicalMemory = {0}", TotalMemory));
        }

        using (ManagementClass mc = new ManagementClass("Win32_Processor"))
        {
          foreach (ManagementObject mo in mc.GetInstances())
          {
            String cpuModel = (string)mo["Name"] + ", " + (string)mo["Caption"];
            AddMessage(string.Format(" CPU = {0}", cpuModel));
          }
        }

        using (ManagementClass mc = new ManagementClass("Win32_ComputerSystem"))
        {
          foreach (ManagementObject mo in mc.GetInstances())
          {
            String Name = (string)mo["Name"];
            AddMessage(string.Format(" SystemName = {0}", Name));

            String model = (string)mo["Manufacturer"] + " " + (string)mo["Model"];
            AddMessage(string.Format(" SystemModel = {0}", model));
          }
        }

        using (ManagementClass mc = new ManagementClass("Win32_Bios"))
        {
          foreach (ManagementObject mo in mc.GetInstances())
          {
            String SerialNumber = (string)mo["Serialnumber"];
            AddMessage(string.Format(" ServiceTag = {0}", SerialNumber));
          }
        }

        using (ManagementClass mc = new ManagementClass("Win32_USBController"))
        {
          foreach (ManagementObject mo in mc.GetInstances())
          {
            String USBController = (string)mo["Caption"];
            AddMessage(string.Format(" UsbController = {0}", USBController));
          }
        }

        using (ManagementClass mc = new ManagementClass("Win32_1394Controller"))
        {
          foreach (ManagementObject mo in mc.GetInstances())
          {
            String Controller = (string)mo["Caption"];
            AddMessage(string.Format(" FirewireController = {0}", Controller));
          }
        }

        int index = 0;
        using (ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
        {
          foreach (ManagementObject mo in mc.GetInstances())
          {
            if ((bool)mo["IPEnabled"] == true)
            {
              index++;
              AddMessage(string.Format(" Network Controller {0}", index.ToString()));

              String Description = (string)mo["Description"];
              AddMessage(string.Format("  Controller = {0}", Description));

              String MAC = (string)mo["MACAddress"];
              AddMessage(string.Format("  MACAddress = {0}", MAC));

              String[] gateway = (string[])mo["DefaultIPGateway"];
              if (gateway != null)
                foreach (String address in gateway)
                {
                  AddMessage(string.Format("  DefaultIPGateway = {0}", address));
                }

              String[] IPAddress = (string[])mo["IPAddress"];
              if (IPAddress != null)
                foreach (String address in IPAddress)
                {
                  AddMessage(string.Format("  IPAddress = {0}", address));
                }
            }
          }
        }

      }
      catch (System.Exception e)
      {
        AddMessage(string.Format("System exception collecting System info in LogDeviceInfo = {0}", e.Message));
      }
    }

    /*!
     * \fn public void GetOSVersion()
     * \brief This functions uses Registry and .Net Environment class to determine OS version.
     * This is an optional function used to gather information to aid in debugging 
     */
    private String GetOSVersion()
    {
      //  Per Microsoft - Applications not manifested for Windows 8.1 or Windows 10 will return the Windows 8 OS version value (6.2). This is for support of legacy apps.
      // i.e. GetVersionEx does not set dwMajorVersion & dwMinorVersion after Windows 8.0.
      // Also, RtlGetVersion() does not set wProductType which is properly set by the GetVersionEx() function.
      // Thus for C++ call GetVersionEx() to get wProductType and then RtlGetVersion() for dwMajorVersion & dwMinorVersion. Must be in order...
      // For C#, we can read the registry...

      //For NT Platform...
      Int32 OSMajorVersion = (Int32)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentMajorVersionNumber", -1);
      Int32 OSMinorVersion = (Int32)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentMinorVersionNumber", -1);
      String OSCurrentVersion = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion", string.Empty).ToString();
      String OSProductName = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", string.Empty).ToString();
      String OSBuildVersion = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber", string.Empty).ToString();
      String OSVersion;

      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32S:
          return "Win 3.1";
        case PlatformID.Win32Windows:
          switch (Environment.OSVersion.Version.Minor)
          {
            case 0: return "Windows 95";
            case 10: return "Windows 98";
            case 90: return "Windows ME";
            default: return "Widnows 9x";
          }

        case PlatformID.Win32NT:
          switch (OSMajorVersion)
          {
            case 3: return "Widnows NT 3.51";
            case 4: return "Widnows NT 4.0";
            case 5: 
              switch (OSMinorVersion)
              {
                case 0: return "Widnows 2000";
                case 1: return "Widnows XP";
                case 2: return "Widnows 2003";
              }
              break;

            case 6:
              switch (OSMinorVersion)
              {
                case 0: return "Widnows Vista";
                case 1: return "Widnows 7";
                case 2: return "Widnows 8";
                case 3: return "Widnows 8.1";
              }
              break;
            case 10:
              OSVersion = String.Format("{0} ({1}.{2}.{3})", OSProductName, OSMajorVersion, OSMinorVersion, OSBuildVersion);
              return OSVersion;
            case -1:
              // no OSMajorVersion or OSMinorVersion entry on some Windows 7 systems.
              OSVersion = String.Format("{0} {1}.{2}", OSProductName, OSCurrentVersion, OSBuildVersion);
              return OSVersion;
            default:
              OSVersion = String.Format("Unknown OS ({0} {1} {2}.{3}.{4})", Environment.OSVersion.Platform, Environment.OSVersion.Version.Minor, OSMajorVersion, OSMinorVersion, OSBuildVersion);
              return OSVersion;
          }
          break;

        case PlatformID.WinCE:
          return "Windows CE";
      }

      return "Unknown";
    }


    private void Main_Load(object sender, EventArgs e)
    {

    }

    private void Main_Closing(object sender, FormClosingEventArgs e) //EventArgs e)
    {
            bool bClose = checkCanClose();
            e.Cancel = !bClose;
    }

    private bool checkCanClose()
    {
            return !m_bMsgBoxOpened;
    }



    private void InitializeComponent()
    {
      this.btnExit = new System.Windows.Forms.Button();
      this.btnProperties = new System.Windows.Forms.Button();
      this.btnOpen = new System.Windows.Forms.Button();
      this.btnClose = new System.Windows.Forms.Button();
      this.btnOpenDevice = new System.Windows.Forms.Button();
      this.btnCloseDevice = new System.Windows.Forms.Button();
      this.btnAcquire = new System.Windows.Forms.Button();
      this.btnForce = new System.Windows.Forms.Button();
      this.btnSaveImage = new System.Windows.Forms.Button();
      this.btnAdjust = new System.Windows.Forms.Button();
      this.comboBox_Position = new System.Windows.Forms.ComboBox();
      this.comboBox_Impression = new System.Windows.Forms.ComboBox();
      this.textBoxLog = new System.Windows.Forms.TextBox();
      this.ImageBox = new System.Windows.Forms.PictureBox();
      this.DeviceStatus = new System.Windows.Forms.TextBox();
      this.DeviceListGrpBx = new System.Windows.Forms.GroupBox();
      this.DeviceInfoBox = new System.Windows.Forms.ListBox();
      this.btnCancelAcquire = new System.Windows.Forms.Button();
      this.LabelPosition = new System.Windows.Forms.Label();
      this.LabelImpression = new System.Windows.Forms.Label();
      this.labelNum = new System.Windows.Forms.Label();
      this.comboBox_NumObjCapture = new System.Windows.Forms.ComboBox();
      this.checkBox1000dpi = new System.Windows.Forms.CheckBox();
      this.checkBoxPAD = new System.Windows.Forms.CheckBox();
      this.btnSaveLog = new System.Windows.Forms.Button();
      this.checkBoxAutocontrast = new System.Windows.Forms.CheckBox();
      this.checkBoxAltTrigger = new System.Windows.Forms.CheckBox();
      this.radioButtonInsufficientQuality = new System.Windows.Forms.RadioButton();
      this.radioButtonInsufficientObjectCount = new System.Windows.Forms.RadioButton();
      this.checkBoxFlexRollCapture = new System.Windows.Forms.CheckBox();
      this.checkBoxFlexFlatCapture = new System.Windows.Forms.CheckBox();
      this.pictureBoxTouch = new System.Windows.Forms.PictureBox();
      this.checkBoxVisualization = new System.Windows.Forms.CheckBox();
      this.btnUserControls = new System.Windows.Forms.Button();
      this.LEDlightPanel = new LSE_BioBase4_CSharpSample.LightPanel();
      ((System.ComponentModel.ISupportInitialize)(this.ImageBox)).BeginInit();
      this.DeviceListGrpBx.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTouch)).BeginInit();
      this.SuspendLayout();
      // 
      // btnExit
      // 
      this.btnExit.Location = new System.Drawing.Point(189, 70);
      this.btnExit.Margin = new System.Windows.Forms.Padding(4);
      this.btnExit.Name = "btnExit";
      this.btnExit.Size = new System.Drawing.Size(147, 62);
      this.btnExit.TabIndex = 27;
      this.btnExit.Text = "Exit App";
      this.btnExit.UseVisualStyleBackColor = true;
      this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
      // 
      // btnProperties
      // 
      this.btnProperties.Location = new System.Drawing.Point(15, 389);
      this.btnProperties.Margin = new System.Windows.Forms.Padding(4);
      this.btnProperties.Name = "btnProperties";
      this.btnProperties.Size = new System.Drawing.Size(147, 62);
      this.btnProperties.TabIndex = 4;
      this.btnProperties.Text = "Device Properties";
      this.btnProperties.UseVisualStyleBackColor = true;
      this.btnProperties.Click += new System.EventHandler(this.btnProperties_Click);
      // 
      // btnOpen
      // 
      this.btnOpen.Location = new System.Drawing.Point(15, 2);
      this.btnOpen.Margin = new System.Windows.Forms.Padding(4);
      this.btnOpen.Name = "btnOpen";
      this.btnOpen.Size = new System.Drawing.Size(147, 62);
      this.btnOpen.TabIndex = 0;
      this.btnOpen.Text = "Open API";
      this.btnOpen.UseVisualStyleBackColor = true;
      this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
      // 
      // btnClose
      // 
      this.btnClose.Location = new System.Drawing.Point(188, 1);
      this.btnClose.Margin = new System.Windows.Forms.Padding(4);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(147, 62);
      this.btnClose.TabIndex = 26;
      this.btnClose.Text = "Close API";
      this.btnClose.UseVisualStyleBackColor = true;
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      // 
      // btnOpenDevice
      // 
      this.btnOpenDevice.Location = new System.Drawing.Point(15, 320);
      this.btnOpenDevice.Margin = new System.Windows.Forms.Padding(4);
      this.btnOpenDevice.Name = "btnOpenDevice";
      this.btnOpenDevice.Size = new System.Drawing.Size(147, 62);
      this.btnOpenDevice.TabIndex = 3;
      this.btnOpenDevice.Text = "Open Device";
      this.btnOpenDevice.UseVisualStyleBackColor = true;
      this.btnOpenDevice.Click += new System.EventHandler(this.btnOpenDevice_Click);
      // 
      // btnCloseDevice
      // 
      this.btnCloseDevice.Location = new System.Drawing.Point(188, 320);
      this.btnCloseDevice.Margin = new System.Windows.Forms.Padding(4);
      this.btnCloseDevice.Name = "btnCloseDevice";
      this.btnCloseDevice.Size = new System.Drawing.Size(147, 62);
      this.btnCloseDevice.TabIndex = 25;
      this.btnCloseDevice.Text = "Close Device";
      this.btnCloseDevice.UseVisualStyleBackColor = true;
      this.btnCloseDevice.Click += new System.EventHandler(this.btnCloseDevice_Click);
      // 
      // btnAcquire
      // 
      this.btnAcquire.Location = new System.Drawing.Point(349, 642);
      this.btnAcquire.Margin = new System.Windows.Forms.Padding(4);
      this.btnAcquire.Name = "btnAcquire";
      this.btnAcquire.Size = new System.Drawing.Size(143, 62);
      this.btnAcquire.TabIndex = 20;
      this.btnAcquire.Text = "Acquire";
      this.btnAcquire.UseVisualStyleBackColor = true;
      this.btnAcquire.Click += new System.EventHandler(this.btnAcquire_Click);
      // 
      // btnForce
      // 
      this.btnForce.Location = new System.Drawing.Point(497, 642);
      this.btnForce.Margin = new System.Windows.Forms.Padding(4);
      this.btnForce.Name = "btnForce";
      this.btnForce.Size = new System.Drawing.Size(143, 62);
      this.btnForce.TabIndex = 21;
      this.btnForce.Text = "Force Capture";
      this.btnForce.UseVisualStyleBackColor = true;
      this.btnForce.Click += new System.EventHandler(this.btnForce_Click);
      // 
      // btnSaveImage
      // 
      this.btnSaveImage.Location = new System.Drawing.Point(188, 389);
      this.btnSaveImage.Margin = new System.Windows.Forms.Padding(4);
      this.btnSaveImage.Name = "btnSaveImage";
      this.btnSaveImage.Size = new System.Drawing.Size(147, 62);
      this.btnSaveImage.TabIndex = 24;
      this.btnSaveImage.Text = "Save Image";
      this.btnSaveImage.UseVisualStyleBackColor = true;
      this.btnSaveImage.Click += new System.EventHandler(this.btnSave_Click);
      // 
      // btnAdjust
      // 
      this.btnAdjust.Location = new System.Drawing.Point(645, 642);
      this.btnAdjust.Margin = new System.Windows.Forms.Padding(4);
      this.btnAdjust.Name = "btnAdjust";
      this.btnAdjust.Size = new System.Drawing.Size(143, 62);
      this.btnAdjust.TabIndex = 22;
      this.btnAdjust.Text = "Adjust";
      this.btnAdjust.UseVisualStyleBackColor = true;
      this.btnAdjust.Click += new System.EventHandler(this.btnAdjust_Click);
      // 
      // comboBox_Position
      // 
      this.comboBox_Position.FormattingEnabled = true;
      this.comboBox_Position.Location = new System.Drawing.Point(16, 631);
      this.comboBox_Position.Margin = new System.Windows.Forms.Padding(4);
      this.comboBox_Position.Name = "comboBox_Position";
      this.comboBox_Position.Size = new System.Drawing.Size(320, 24);
      this.comboBox_Position.TabIndex = 17;
      this.comboBox_Position.SelectedIndexChanged += new System.EventHandler(this.comboBox_Position_SelectedIndexChanged);
      // 
      // comboBox_Impression
      // 
      this.comboBox_Impression.FormattingEnabled = true;
      this.comboBox_Impression.Location = new System.Drawing.Point(16, 679);
      this.comboBox_Impression.Margin = new System.Windows.Forms.Padding(4);
      this.comboBox_Impression.Name = "comboBox_Impression";
      this.comboBox_Impression.Size = new System.Drawing.Size(320, 24);
      this.comboBox_Impression.TabIndex = 19;
      // 
      // textBoxLog
      // 
      this.textBoxLog.Location = new System.Drawing.Point(350, 491);
      this.textBoxLog.Margin = new System.Windows.Forms.Padding(4);
      this.textBoxLog.MaxLength = 50000;
      this.textBoxLog.Multiline = true;
      this.textBoxLog.Name = "textBoxLog";
      this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.textBoxLog.Size = new System.Drawing.Size(586, 143);
      this.textBoxLog.TabIndex = 29;
      // 
      // ImageBox
      // 
      this.ImageBox.BackColor = System.Drawing.SystemColors.Window;
      this.ImageBox.Location = new System.Drawing.Point(350, 50);
      this.ImageBox.Margin = new System.Windows.Forms.Padding(4);
      this.ImageBox.Name = "ImageBox";
      this.ImageBox.Size = new System.Drawing.Size(465, 430);
      this.ImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.ImageBox.TabIndex = 22;
      this.ImageBox.TabStop = false;
      // 
      // DeviceStatus
      // 
      this.DeviceStatus.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.DeviceStatus.ForeColor = System.Drawing.Color.Red;
      this.DeviceStatus.Location = new System.Drawing.Point(13, 141);
      this.DeviceStatus.Margin = new System.Windows.Forms.Padding(4);
      this.DeviceStatus.Name = "DeviceStatus";
      this.DeviceStatus.Size = new System.Drawing.Size(323, 30);
      this.DeviceStatus.TabIndex = 1;
      this.DeviceStatus.TabStop = false;
      this.DeviceStatus.Text = "Device Status";
      // 
      // DeviceListGrpBx
      // 
      this.DeviceListGrpBx.Controls.Add(this.DeviceInfoBox);
      this.DeviceListGrpBx.Location = new System.Drawing.Point(7, 178);
      this.DeviceListGrpBx.Margin = new System.Windows.Forms.Padding(4);
      this.DeviceListGrpBx.Name = "DeviceListGrpBx";
      this.DeviceListGrpBx.Padding = new System.Windows.Forms.Padding(4);
      this.DeviceListGrpBx.Size = new System.Drawing.Size(329, 134);
      this.DeviceListGrpBx.TabIndex = 2;
      this.DeviceListGrpBx.TabStop = false;
      this.DeviceListGrpBx.Text = "Device List";
      // 
      // DeviceInfoBox
      // 
      this.DeviceInfoBox.FormattingEnabled = true;
      this.DeviceInfoBox.HorizontalScrollbar = true;
      this.DeviceInfoBox.ItemHeight = 16;
      this.DeviceInfoBox.Location = new System.Drawing.Point(11, 22);
      this.DeviceInfoBox.Margin = new System.Windows.Forms.Padding(4);
      this.DeviceInfoBox.Name = "DeviceInfoBox";
      this.DeviceInfoBox.Size = new System.Drawing.Size(310, 100);
      this.DeviceInfoBox.TabIndex = 0;
      // 
      // btnCancelAcquire
      // 
      this.btnCancelAcquire.Location = new System.Drawing.Point(794, 642);
      this.btnCancelAcquire.Margin = new System.Windows.Forms.Padding(4);
      this.btnCancelAcquire.Name = "btnCancelAcquire";
      this.btnCancelAcquire.Size = new System.Drawing.Size(143, 62);
      this.btnCancelAcquire.TabIndex = 23;
      this.btnCancelAcquire.Text = "Cancel Capture";
      this.btnCancelAcquire.UseVisualStyleBackColor = true;
      this.btnCancelAcquire.Click += new System.EventHandler(this.btnCancelAcquire_Click);
      // 
      // LabelPosition
      // 
      this.LabelPosition.AutoSize = true;
      this.LabelPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.LabelPosition.Location = new System.Drawing.Point(12, 609);
      this.LabelPosition.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.LabelPosition.Name = "LabelPosition";
      this.LabelPosition.Size = new System.Drawing.Size(74, 20);
      this.LabelPosition.TabIndex = 16;
      this.LabelPosition.Text = "Position:";
      // 
      // LabelImpression
      // 
      this.LabelImpression.AutoSize = true;
      this.LabelImpression.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.LabelImpression.Location = new System.Drawing.Point(12, 658);
      this.LabelImpression.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.LabelImpression.Name = "LabelImpression";
      this.LabelImpression.Size = new System.Drawing.Size(85, 18);
      this.LabelImpression.TabIndex = 18;
      this.LabelImpression.Text = "Impression:";
      // 
      // labelNum
      // 
      this.labelNum.AutoSize = true;
      this.labelNum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelNum.Location = new System.Drawing.Point(12, 561);
      this.labelNum.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.labelNum.Name = "labelNum";
      this.labelNum.Size = new System.Drawing.Size(152, 20);
      this.labelNum.TabIndex = 14;
      this.labelNum.Text = "Objects to capture:";
      // 
      // comboBox_NumObjCapture
      // 
      this.comboBox_NumObjCapture.FormattingEnabled = true;
      this.comboBox_NumObjCapture.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5"});
      this.comboBox_NumObjCapture.Location = new System.Drawing.Point(16, 583);
      this.comboBox_NumObjCapture.Margin = new System.Windows.Forms.Padding(4);
      this.comboBox_NumObjCapture.Name = "comboBox_NumObjCapture";
      this.comboBox_NumObjCapture.Size = new System.Drawing.Size(132, 24);
      this.comboBox_NumObjCapture.TabIndex = 15;
      this.comboBox_NumObjCapture.Text = "1";
      // 
      // checkBox1000dpi
      // 
      this.checkBox1000dpi.AutoSize = true;
      this.checkBox1000dpi.Location = new System.Drawing.Point(195, 487);
      this.checkBox1000dpi.Margin = new System.Windows.Forms.Padding(4);
      this.checkBox1000dpi.Name = "checkBox1000dpi";
      this.checkBox1000dpi.Size = new System.Drawing.Size(81, 21);
      this.checkBox1000dpi.TabIndex = 9;
      this.checkBox1000dpi.Text = "1000dpi";
      this.checkBox1000dpi.UseVisualStyleBackColor = true;
      // 
      // checkBoxPAD
      // 
      this.checkBoxPAD.AutoSize = true;
      this.checkBoxPAD.Location = new System.Drawing.Point(195, 460);
      this.checkBoxPAD.Margin = new System.Windows.Forms.Padding(4);
      this.checkBoxPAD.Name = "checkBoxPAD";
      this.checkBoxPAD.Size = new System.Drawing.Size(122, 21);
      this.checkBoxPAD.TabIndex = 8;
      this.checkBoxPAD.Text = "Check for PAD";
      this.checkBoxPAD.UseVisualStyleBackColor = true;
      // 
      // btnSaveLog
      // 
      this.btnSaveLog.Location = new System.Drawing.Point(822, 418);
      this.btnSaveLog.Margin = new System.Windows.Forms.Padding(4);
      this.btnSaveLog.Name = "btnSaveLog";
      this.btnSaveLog.Size = new System.Drawing.Size(113, 62);
      this.btnSaveLog.TabIndex = 30;
      this.btnSaveLog.Text = "Save Log";
      this.btnSaveLog.UseVisualStyleBackColor = true;
      this.btnSaveLog.Click += new System.EventHandler(this.btnSaveLog_Click);
      // 
      // checkBoxAutocontrast
      // 
      this.checkBoxAutocontrast.AutoSize = true;
      this.checkBoxAutocontrast.Location = new System.Drawing.Point(195, 514);
      this.checkBoxAutocontrast.Margin = new System.Windows.Forms.Padding(4);
      this.checkBoxAutocontrast.Name = "checkBoxAutocontrast";
      this.checkBoxAutocontrast.Size = new System.Drawing.Size(110, 21);
      this.checkBoxAutocontrast.TabIndex = 10;
      this.checkBoxAutocontrast.Text = "Autocontrast";
      this.checkBoxAutocontrast.UseVisualStyleBackColor = true;
      // 
      // checkBoxAltTrigger
      // 
      this.checkBoxAltTrigger.AutoSize = true;
      this.checkBoxAltTrigger.Location = new System.Drawing.Point(15, 460);
      this.checkBoxAltTrigger.Margin = new System.Windows.Forms.Padding(4);
      this.checkBoxAltTrigger.Name = "checkBoxAltTrigger";
      this.checkBoxAltTrigger.Size = new System.Drawing.Size(100, 21);
      this.checkBoxAltTrigger.TabIndex = 5;
      this.checkBoxAltTrigger.Text = "Alt. Trigger";
      this.checkBoxAltTrigger.UseVisualStyleBackColor = true;
      // 
      // radioButtonInsufficientQuality
      // 
      this.radioButtonInsufficientQuality.AutoSize = true;
      this.radioButtonInsufficientQuality.Location = new System.Drawing.Point(23, 508);
      this.radioButtonInsufficientQuality.Name = "radioButtonInsufficientQuality";
      this.radioButtonInsufficientQuality.Size = new System.Drawing.Size(144, 21);
      this.radioButtonInsufficientQuality.TabIndex = 7;
      this.radioButtonInsufficientQuality.TabStop = true;
      this.radioButtonInsufficientQuality.Text = "Insufficient Quality";
      this.radioButtonInsufficientQuality.UseVisualStyleBackColor = true;
      // 
      // radioButtonInsufficientObjectCount
      // 
      this.radioButtonInsufficientObjectCount.AutoSize = true;
      this.radioButtonInsufficientObjectCount.Checked = true;
      this.radioButtonInsufficientObjectCount.Location = new System.Drawing.Point(23, 484);
      this.radioButtonInsufficientObjectCount.Name = "radioButtonInsufficientObjectCount";
      this.radioButtonInsufficientObjectCount.Size = new System.Drawing.Size(165, 21);
      this.radioButtonInsufficientObjectCount.TabIndex = 6;
      this.radioButtonInsufficientObjectCount.TabStop = true;
      this.radioButtonInsufficientObjectCount.Text = "Insufficient Finger Cnt";
      this.radioButtonInsufficientObjectCount.UseVisualStyleBackColor = true;
      // 
      // checkBoxFlexRollCapture
      // 
      this.checkBoxFlexRollCapture.AutoSize = true;
      this.checkBoxFlexRollCapture.Location = new System.Drawing.Point(195, 541);
      this.checkBoxFlexRollCapture.Margin = new System.Windows.Forms.Padding(4);
      this.checkBoxFlexRollCapture.Name = "checkBoxFlexRollCapture";
      this.checkBoxFlexRollCapture.Size = new System.Drawing.Size(137, 21);
      this.checkBoxFlexRollCapture.TabIndex = 11;
      this.checkBoxFlexRollCapture.Text = "Flex Roll Capture";
      this.checkBoxFlexRollCapture.UseVisualStyleBackColor = true;
      this.checkBoxFlexRollCapture.CheckedChanged += new System.EventHandler(this.checkBoxFlexRollCapture_CheckedChanged);
      // 
      // checkBoxFlexFlatCapture
      // 
      this.checkBoxFlexFlatCapture.AutoSize = true;
      this.checkBoxFlexFlatCapture.Location = new System.Drawing.Point(195, 568);
      this.checkBoxFlexFlatCapture.Margin = new System.Windows.Forms.Padding(4);
      this.checkBoxFlexFlatCapture.Name = "checkBoxFlexFlatCapture";
      this.checkBoxFlexFlatCapture.Size = new System.Drawing.Size(136, 21);
      this.checkBoxFlexFlatCapture.TabIndex = 12;
      this.checkBoxFlexFlatCapture.Text = "Flex Flat Capture";
      this.checkBoxFlexFlatCapture.UseVisualStyleBackColor = true;
      this.checkBoxFlexFlatCapture.CheckedChanged += new System.EventHandler(this.checkBoxFlexFlatCapture_CheckedChanged);
      // 
      // pictureBoxTouch
      // 
      this.pictureBoxTouch.BackColor = System.Drawing.Color.Transparent;
      this.pictureBoxTouch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.pictureBoxTouch.Location = new System.Drawing.Point(18, 72);
      this.pictureBoxTouch.Margin = new System.Windows.Forms.Padding(4);
      this.pictureBoxTouch.Name = "pictureBoxTouch";
      this.pictureBoxTouch.Size = new System.Drawing.Size(144, 63);
      this.pictureBoxTouch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBoxTouch.TabIndex = 30;
      this.pictureBoxTouch.TabStop = false;
      // 
      // checkBoxVisualization
      // 
      this.checkBoxVisualization.AutoSize = true;
      this.checkBoxVisualization.Location = new System.Drawing.Point(195, 595);
      this.checkBoxVisualization.Margin = new System.Windows.Forms.Padding(4);
      this.checkBoxVisualization.Name = "checkBoxVisualization";
      this.checkBoxVisualization.Size = new System.Drawing.Size(109, 21);
      this.checkBoxVisualization.TabIndex = 13;
      this.checkBoxVisualization.Text = "Visualization";
      this.checkBoxVisualization.UseVisualStyleBackColor = true;
      // 
      // btnUserControls
      // 
      this.btnUserControls.Location = new System.Drawing.Point(822, 330);
      this.btnUserControls.Margin = new System.Windows.Forms.Padding(4);
      this.btnUserControls.Name = "btnUserControls";
      this.btnUserControls.Size = new System.Drawing.Size(113, 62);
      this.btnUserControls.TabIndex = 31;
      this.btnUserControls.Text = "User Controls";
      this.btnUserControls.UseVisualStyleBackColor = true;
      this.btnUserControls.Click += new System.EventHandler(this.btnUserControls_Click);
      // 
      // LEDlightPanel
      // 
      this.LEDlightPanel.Anchor = System.Windows.Forms.AnchorStyles.Top;
      this.LEDlightPanel.BackColor = System.Drawing.Color.LightGray;
      this.LEDlightPanel.LedCount = 0;
      this.LEDlightPanel.Location = new System.Drawing.Point(350, 2);
      this.LEDlightPanel.Margin = new System.Windows.Forms.Padding(0);
      this.LEDlightPanel.Name = "LEDlightPanel";
      this.LEDlightPanel.Size = new System.Drawing.Size(465, 44);
      this.LEDlightPanel.TabIndex = 28;
      this.LEDlightPanel.TabStop = false;
      // 
      // Main
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(948, 711);
      this.Controls.Add(this.btnUserControls);
      this.Controls.Add(this.checkBoxVisualization);
      this.Controls.Add(this.pictureBoxTouch);
      this.Controls.Add(this.checkBoxFlexFlatCapture);
      this.Controls.Add(this.checkBoxFlexRollCapture);
      this.Controls.Add(this.radioButtonInsufficientObjectCount);
      this.Controls.Add(this.radioButtonInsufficientQuality);
      this.Controls.Add(this.checkBoxAltTrigger);
      this.Controls.Add(this.checkBoxAutocontrast);
      this.Controls.Add(this.btnSaveLog);
      this.Controls.Add(this.checkBoxPAD);
      this.Controls.Add(this.checkBox1000dpi);
      this.Controls.Add(this.labelNum);
      this.Controls.Add(this.LabelImpression);
      this.Controls.Add(this.LabelPosition);
      this.Controls.Add(this.DeviceListGrpBx);
      this.Controls.Add(this.DeviceStatus);
      this.Controls.Add(this.ImageBox);
      this.Controls.Add(this.textBoxLog);
      this.Controls.Add(this.btnCancelAcquire);
      this.Controls.Add(this.btnAdjust);
      this.Controls.Add(this.btnForce);
      this.Controls.Add(this.btnAcquire);
      this.Controls.Add(this.btnSaveImage);
      this.Controls.Add(this.btnProperties);
      this.Controls.Add(this.btnCloseDevice);
      this.Controls.Add(this.btnOpenDevice);
      this.Controls.Add(this.comboBox_Impression);
      this.Controls.Add(this.comboBox_Position);
      this.Controls.Add(this.comboBox_NumObjCapture);
      this.Controls.Add(this.btnExit);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.btnOpen);
      this.Controls.Add(this.LEDlightPanel);
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "Main";
      this.Text = "LSE BioBase4 C# Sample";
      this.Load += new System.EventHandler(this.Main_Load);
      ((System.ComponentModel.ISupportInitialize)(this.ImageBox)).EndInit();
      this.DeviceListGrpBx.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTouch)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_Closing);

    }
  }
}
