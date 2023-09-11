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


namespace LSE_BioBase4_CSharpSample
{
  public partial class Main
  {
    // Register the UI methods for each of the BioBase device callback events.
    // Check InvokeRequired because method is called from open device thread
    public void RegisterEventHandlers()
    {
      if (this.InvokeRequired)
      {
        // Invoke when called from events outside of GUI thread
        Invoke((Action)RegisterEventHandlers);
      }
      else
      {
        AddMessage("registering event 'IBioBaseDevice.Preview'");
        _biobaseDevice.Preview += new EventHandler<BioBasePreviewEventArgs>(_biobaseDevice_Preview);

        AddMessage("registering event 'IBioBaseDevice.ObjectQuality'");
        _biobaseDevice.ObjectQuality += new EventHandler<BioBaseObjectQualityEventArgs>(_biobaseDevice_ObjectQuality);

        AddMessage("registering event 'IBioBaseDevice.ObjectCount'");
        _biobaseDevice.ObjectCount += new EventHandler<BioBaseObjectCountEventArgs>(_biobaseDevice_ObjectCount);

        AddMessage("registering event 'IBioBaseDevice.ScannerUserInput'");
        _biobaseDevice.ScannerUserInput += new EventHandler<BioBaseUserInputEventArgs>(_biobaseDevice_ScannerUserInput);

        //TODO: Add support for ScannerUserOutput once we know why there is an error when unregistering this event.
//        AddMessage("registering event 'IBioBaseDevice.ScannerUserOutput'");
//        _biobaseDevice.ScannerUserOutput += new EventHandler<BioBaseUserOutputEventArgs>(_biobaseDevice_ScannerUserOutput);

        AddMessage("registering event 'IBioBaseDevice.AcquisitionStart'");
        _biobaseDevice.AcquisitionStart += new EventHandler<BioBaseAcquisitionStartEventArgs>(_biobaseDevice_AcquisitionStart);

        AddMessage("registering event 'IBioBaseDevice.AcquisitionComplete'");
        _biobaseDevice.AcquisitionComplete += new EventHandler<BioBaseAcquisitionCompleteEventArgs>(_biobaseDevice_AcquisitionComplete);

        AddMessage("registering event 'IBioBaseDevice.DataAvailable'");
        _biobaseDevice.DataAvailable += new EventHandler<BioBaseDataAvailableEventArgs>(_biobaseDevice_DataAvailable);

        AddMessage("registering event 'IBioBaseDevice.DetectedObject'");
        _biobaseDevice.DetectedObject += new EventHandler<BioBaseDetectObjectEventArgs>(_biobaseDevice_DetectedObject);
      }
    }

    #region BioB API level Events

    /*!
     * \fn void _biobase_DeviceCount()
     * \brief New event triggered when a device is attached or removed
     * Note that all updates to the UI this event must be done via Invoke calls.
     * This event is called from a thread created in the low level SDK to notify the application that 
     * the number of attached capture devices has changed. 
     * This event does not known if a new device was attached to the computer or an old device was removed. 
     * This event will get information about all the attached devices and update the UI accordingly.
     * This event updates the UI with list (FillDeviceListBox) of devices currently attached. The 
     * FillDeviceListBox() method will make sure that the prevously selected device is still selected.
     * Catch BioBaseException and Exception and log errors
     */
    void _biobase_DeviceCount(object sender, BioBaseDeviceCountEventArgs e)
    {
      try
      {
        AddMessage("event BIOB_DEVICE_COUNT_CHANGED: Checking number of attached devices");

        if (e.DeviceCount > 0)
        {
          if (m_scannerOpen == false)
            SetDeviceState(DeviceState.device_connected_and_not_opened);
        }
        else
        {
          //TODO: If device was open and now disconnected, we need to call CloseDevice()
          SetDeviceState(DeviceState.device_not_connected);
        }

        // Update list of connected devices
        _biobaseDevices = _biobase.ConnectedDevices;
        FillDeviceListBox();
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBaseDeviceCountEventArgs thread BioBase error {0}", ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("BioBaseDeviceCountEventArgs thread error {0}", ex.Message));
      }
    }
    #endregion



    #region BioB Device Events

    /*!
     * \fn void _biobaseDevice_Init()
     * \brief New event triggered during device open to give status of initialization process
     * Note that all updates to the UI from this event must be done via Invoke calls.
     * This event is called from a thread created in the low level SDK to notify the application 
     * of the initialization process.
     * This event updates the UI with message box but could also control a progress bar.
     */
    void _biobaseDevice_Init(object sender, BioBaseInitProgressEventArgs e)
    {
      float progress = e.ProgressValue;
      AddMessage(string.Format("event: Initializing device... {0}%", progress));
    }

    /*!
     * \fn void _biobaseDevice_Preview()
     * \brief New event triggered during capture with each new image
     * Note that all updates to the UI from this event must be done via Invoke calls.
     * This event is called from a thread created in the low level SDK with decimated preview
     * iamges at a rate between 10 and 30 per second.
     * This event updates the UI with UI ImageBox.
     * NOTE: When using multiple devices it is a good idea to add check for valid e.DeviceID
     * NOTE: Use this event to flag any additional overlay information on top of new preview image.
     * Other options to this event is the have the LSE SDK draw to the ImageBox. While not shown
     * in this sample, the BioB_SetVisualizationWindow() method supports LSE drawing the preview image.
     */
    void _biobaseDevice_Preview(object sender, BioBasePreviewEventArgs e)
    {
      if (checkBoxVisualization.Checked == false)
      {
        // Visualization is not being used so we must display image data in preview event 
        Bitmap ImageData = e.ImageData;
        ImageBox.Image = ImageData;
      }
    }

    /*!
     * \fn void _biobaseDevice_ObjectQuality()
     * \brief New event triggered during capture when the quality of each detected finger objects change.
     * During a 4 finger flat capture the image quality array will have 4 entries while during 
     * a fingerprint roll, the quality array will only have one entry.
     * Note that all updates to the UI from this event must be done via Invoke calls.
     * This event is called saves the image quality array to the global _biobaseDevice.mostRecentqualities variable.
     * The _biobaseDevice.mostRecentqualities.Length is then used to update the UI LED light panel and the capture status on each device.
     * Catch BioBaseException and Exception and log errors
     */
    void _biobaseDevice_ObjectQuality(object sender, BioBaseObjectQualityEventArgs e)
    {
      try
      {
        AddMessage("event BIOB_OBJECT_QUALITY: object[] quality");
        if (e is BioBaseObjectQualityEventArgs)
        {
          _biobaseDevice.mostRecentqualities = ((BioBaseObjectQualityEventArgs)e).QualStateArray;
          switch(_biobaseDevice.mostRecentqualities.Length)
          {
            case 1:
              SetUILedColors(ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[0]), 
                ActiveColor.gray, ActiveColor.gray, ActiveColor.gray, ActiveColor.gray);
              break;
            case 2:
              SetUILedColors(ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[0]),
                ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[1]),
                ActiveColor.gray, ActiveColor.gray, ActiveColor.gray);
              break;
            case 3:
              SetUILedColors(ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[0]),
                ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[1]),
                ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[2]),
                ActiveColor.gray, ActiveColor.gray);
              break;
            case 4:
              SetUILedColors(ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[0]),
                ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[1]),
                ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[2]),
                ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[3]), ActiveColor.gray);
              break;
            case 5:
              // support 5th (upper palm) status "LED" for LScan palm scanners
              SetUILedColors(ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[0]),
                ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[1]),
                ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[2]),
                ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[3]),
                ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[4]));
              break;
          }

          _SetStatusElements();  // Update status on device with mostRecentqualities and mostResentKey
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("event: BIOB_OBJECT_QUALITY error {0}%", ex.Message));
      }
    }

    /*!
     * \fn void _biobaseDevice_ObjectCount()
     * \brief New event triggered during capture when a new finger objects is detected.
     * During a 4 finger flat capture the e.ObjectCountState will return BIOB_TOO_FEW_OBJECTS 
     * when less than 4 figners are detectd. BIOB_OBJECT_COUNT_OK is returned when
     * the expected number of finger objects is detected.
     * Note that all updates to the UI from this event must be done via Invoke calls.
     * NOTE: Use this event to flag display hints and prompt operator when the number of fingers on the platen in not correct
     */
    void _biobaseDevice_ObjectCount(object sender, BioBaseObjectCountEventArgs e)
    {
      BioBObjectCountState state = e.ObjectCountState;
      switch (state)
      {
        case BioBObjectCountState.BIOB_OBJECT_COUNT_OK:
          AddMessage("event BIOB_OBJECT_COUNT: Object count (OK)");
          break;
        case BioBObjectCountState.BIOB_TOO_FEW_OBJECTS:
          AddMessage("event BIOB_OBJECT_COUNT: Object count (Too few objects)");
          break;
        case BioBObjectCountState.BIOB_TOO_MANY_OBJECTS:
          AddMessage("event BIOB_OBJECT_COUNT: Object count (Too many objects)");
          break;
      }
    }

    /*!
     * \fn void _biobaseDevice_ScannerUserInput()
     * \brief New event triggered during capture when a key is pressed on the device.
     * Note that all updates to the UI from this event must be done via Invoke calls.
     * This event can be expaned to suppport more key types. This event only demonstrates some 
     * of the key types availble from the devices.
     * NOTE: "IDButtonConfirmActive" and "IDButtonRetryActive" defined in Templates\imgs\*.svg files
     * \param  global input m_bAskRecapture used to determine capture state.  m_bAskRecapture is false 
     * during capture. The _biobaseDevice_DataAvailable event will set m_bAskRecapture to true.
     * During capture, the keys can cancel or adjust the catpure process.
     * After catpure, the keys can accept an image or request a rescan.
     */
    void _biobaseDevice_ScannerUserInput(object sender, BioBaseUserInputEventArgs e)
    {
      AddMessage("event BIOB_SCANNER_USERINPUT: Pressed key...");

      switch (e.PressedKeys)
      {
        case (PropertyConstants.OUT_DATA_KEY_FOOTSWITCH):
        case (PropertyConstants.OUT_DATA_KEY_RIGHT):
        case (PropertyConstants.OUT_DATA_KEY_OK):
        case ("IDButtonConfirmActive"):
          if (m_bAskRecapture == false)
            _biobaseDevice.AdjustAcquisitionProcess(PropertyConstants.PROC_ADJUST_TYPE_OPTIMIZE_CONTRAST, null);
          else
          {
            string msg = string.Format("Capture warning...");
            //TODO: Add prompt here to accept image with warning to replace prompt in _biobaseDevice_DataAvailable dlg
            // Current applcation does not properly process this device key because of MessageBox in the _biobaseDevice_DataAvailable event
            _BeepError();
          }
          break;

        case (PropertyConstants.OUT_DATA_KEY_CANCEL):
        case (PropertyConstants.OUT_DATA_KEY_LEFT):
        case ("IDButtonRetryActive"):
          if (m_bAskRecapture == false)
          { // capture in progress
            _biobaseDevice.CancelAcquisition();
            SetDeviceState(DeviceState.device_opened_and_capture_cancelled);

            //Reset device's LEDs, TFT display or Touch display here
            _biobaseDevice.mostResentKey = ActiveKeys.KEYS_NONE;
            _ResetStatusElements();
            _ResetGuidanceElements();
          }
          else
          { 
            string msg = string.Format("capture warning...");
            //TODO: Add prompt here to recapture image with warning to replace prompt in _biobaseDevice_DataAvailable dlg
            // Current applcation logic will not properly process RescanImage call because of MessageBox in the _biobaseDevice_DataAvailable event
            // RescanImage();
            _BeepError();
          }
          break;
      }
    }

    /*!
     * \fn void _biobaseDevice_ScannerUserOutput()
     * \brief New event triggered when data is sent to the scanner devices.
     * Note that all updates to the UI from this event must be done via Invoke calls.
     *
     * This event is not enabled.
     *
     */
    void _biobaseDevice_ScannerUserOutput(object sender, BioBaseUserOutputEventArgs e)
    {
      AddMessage(string.Format("event BIOB_SCANNER_USEROUTPUT: User Output to scanner acknowledged transactionID:{0}", e.TransactionID));

      // Option to confirm XML data sent to device.
      //if (e.FormatType == BioBOutputDataFormat.BIOB_OUT_XML)
      //{

      //}

      //Display a copy of Guardian 300 User Guidance image in UI
      // When e.FormatType is an image, then e.SetOutputData can be displayed in the UI.
      // This will only come from the Guardian, Guardian 300, Guardian 200, Guardian 100 and Guardian Module
      if (e.FormatType == BioBOutputDataFormat.BIOB_OUT_BMP)
      {
        MemoryStream bytes = new MemoryStream(e.SetOutputData);
        Bitmap bmp = new Bitmap(bytes);
        pictureBoxTouch.Image = bmp;
      }
    }

    /*!
     * \fn void _biobaseDevice_AcquisitionStart()
     * \brief New event triggered during capture when the auto capture starts.
     * Note that all updates to the UI from this event must be done via Invoke calls.
     * 
     * When performing capture, this is triggered when all the autocapture threashold have been met.
     * With flat capture, the _biobaseDevice_AcquisitionComplete will be triggered next.
     * With roll capture, the rolled image is still being stitched together and the 
     * _biobaseDevice_AcquisitionComplete event won't be triggered until the finger is lifted off the platen.
     * 
     * Optionally update screen message that all the autocapture threashold have been met.
     */
    void _biobaseDevice_AcquisitionStart(object sender, BioBaseAcquisitionStartEventArgs e)
    {
      AddMessage("event BIOB_ACQUISITION_STARTED: acquisition start");
    }

    /*!
     * \fn void _biobaseDevice_AcquisitionComplete()
     * \brief New event triggered during capture when the capture is complete
     * There can be a delay between this event and _biobaseDevice_DataAvailable event that contains 
     * the final image. This delay varies based on how long it takes to transfer the final image
     * from the device to the PC. This is typically longer on the LScan palm devices so this event
     * is used to display an hour glass on the device during this delay.
     */
    void _biobaseDevice_AcquisitionComplete(object sender, BioBaseAcquisitionCompleteEventArgs e)
    {
      AddMessage("event BIOB_ACQUISITION_COMPLETED: acquisition complete");
      // nothing more to do right now. Wait for Data Available event...

      switch(_biobaseDevice.deviceGuidanceType)
      {
        case guidanceType.guidanceTypeTFT:
        case guidanceType.guidanceTypeTFT_1000:
          TFT_ObjectCaptureDictionary obj = new TFT_ObjectCaptureDictionary();
          obj[PropertyConstants.OUT_DATA_TFT_STAT_TOP] = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ERASE;
          obj[PropertyConstants.OUT_DATA_TFT_STAT_BOTTOM] = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_HOURGLASS_ANIMATED;
          _TftShowFingerCaptureScreen(false, obj);

          break;
      }
    }


    /*!
     * \fn void _biobaseDevice_DataAvailable()
     * \brief New event triggered when capture is complete and final image is ready.
     * Note that all updates to the UI from this event must be done via Invoke calls.
     * global input _biobaseDevice use to send OK beep to device when capture is complete
     * global input _biobaseDevice.mostRecentqualities and _biobaseDevice.mostResentKey to control keys on status elements on device
     * global input _biobaseDevice.mostResentKey set to disable keys on device if capture is successful
     * \param global m_bAskRecapture set to true to flag accept or re-capture status because of warning
     * \param global m_bAskRecapture set to true to flag accept or re-capture status because of warning
     */
    void _biobaseDevice_DataAvailable(object sender, BioBaseDataAvailableEventArgs e)
    {
      AddMessage("event BIOB_DATA_AVAILABLE: final image ready");
      string devID = e.DeviceID;
      if ((e.IsFinal == true) && (e.DataStatus >= (int)BioBReturnCode.BIOB_SUCCESS))
      {
        // Display final full resolution image data!!!!!!!!!!!!!!!!!
        Bitmap ImageData = e.ImageData;
        ImageBox.Image = ImageData;

        AddMessage("event: Final image displayed");

        if (devID == _biobaseDevice.DeviceInfo.DeviceId)
          _BeepOK();
        else
          System.Media.SystemSounds.Beep.Play();

        if (e.DataStatus == (int)BioBReturnCode.BIOB_SUCCESS)
          _biobaseDevice.mostResentKey = ActiveKeys.KEYS_NONE;
        else
        {
          m_bAskRecapture = true;     // Used to OK button for accept captured image
          _biobaseDevice.mostResentKey = ActiveKeys.KEYS_ACCEPT_RECAPTURE;
        }
        _SetFinalStatusElements((BioBReturnCode)e.DataStatus);  // Update status on device with _e.DataStatus and _biobaseDevice.mostResentKey

        if(e.IsPADScoresValid == true)
        {
          AddMessage("PAD data for captured fingerprints");
          string msg = string.Format("   PAD invalid score value is {0:0.0000}", e.PADScoreInvalid);
          AddMessage(msg);
          msg = string.Format("   PAD minimum score value is {0:0.0000}", e.PADScoreMinimum);
          AddMessage(msg);
          msg = string.Format("   PAD maximum score value is {0:0.0000}", e.PADScoreMaximum);
          AddMessage(msg);
          msg = string.Format("   PAD threshold value is {0:0.0000}", e.PADThresold);
          AddMessage(msg);
          foreach (double score in e.PADScore)
          {
            msg = string.Format("   PAD Score is {0:0.0000}", score);
            AddMessage(msg);
          }
        }


        // Capture complete, now check for warnings on rolled fingers and PAD (spoof).
        // Display roll warnings (may have multiple warnings) and prompt operator to accept or re-capture.
        bool promptRescan = false;
        bool promptRescan2 = false;
        string strRescan = "";
        //check capture status for roll messages
        if ((e.DataStatus & (int)BioBReturnCode.BIOB_ROLL_SHIFTED_HORIZONTALLY) == (int)BioBReturnCode.BIOB_ROLL_SHIFTED_HORIZONTALLY)
        {
          promptRescan = true;
          strRescan += " SHIFTED HORIZONTALLY";
        }
        if ((e.DataStatus & (int)BioBReturnCode.BIOB_ROLL_SHIFTED_VERTICALLY) == (int)BioBReturnCode.BIOB_ROLL_SHIFTED_VERTICALLY)
        {
          promptRescan = true;
          strRescan += " SHIFTED VERTICALLY";
        }
        if ((e.DataStatus & (int)BioBReturnCode.BIOB_ROLL_LIFTED_TIP) == (int)BioBReturnCode.BIOB_ROLL_LIFTED_TIP)
        {
          promptRescan = true;
          strRescan += " LIFTED_TIP";
        }
        if ((e.DataStatus & (int)BioBReturnCode.BIOB_ROLL_ON_BORDER) == (int)BioBReturnCode.BIOB_ROLL_ON_BORDER)
        {
          promptRescan = true;
          strRescan += " ROLL_ON_BORDER";
        }
        if ((e.DataStatus & (int)BioBReturnCode.BIOB_ROLL_PAUSED) == (int)BioBReturnCode.BIOB_ROLL_PAUSED)
        {
          promptRescan = true;
          strRescan += " ROLL_PAUSED";
        }
        if ((e.DataStatus & (int)BioBReturnCode.BIOB_ROLL_FINGER_TOO_NARROW) == (int)BioBReturnCode.BIOB_ROLL_FINGER_TOO_NARROW)
        {
          promptRescan = true;
          strRescan += " ROLL_TOO_NARROW";
        }

        if ((e.DataStatus & (int)BioBReturnCode.BIOB_SPOOF_DETECTED) == (int)BioBReturnCode.BIOB_SPOOF_DETECTED)
        {
          promptRescan2 = true;
          strRescan += " SPOOF_DETECTED";
        }
        if ((e.DataStatus & (int)BioBReturnCode.BIOB_SPOOF_DETECTOR_FAIL) == (int)BioBReturnCode.BIOB_SPOOF_DETECTOR_FAIL)
        {
          promptRescan2 = true;
          strRescan += " SPOOF_DETECTOR_FAIL";
        }



        //If option to not use MessageBox, exit this event and wait for keys in _biobaseDevice_ScannerUserInput event to accept warning or rescan
        if (promptRescan)
        {
          string strRollMessage = "Detected the following roll warnings:" + strRescan + ". Do you want to accept the image?";
          if (MessageBox.Show(strRollMessage, "Roll Warning", MessageBoxButtons.YesNo) == DialogResult.No)
          {
            RescanImage();
            return;
          }
        }
        if (promptRescan2)
        {
          string strRollMessage = "Detected PAD warnings:" + strRescan + ". Do you want to accept the image?";
          if (MessageBox.Show(strRollMessage, "PAD Warning", MessageBoxButtons.YesNo) == DialogResult.No)
          {
            RescanImage();
            return;
          }
        }
      }
      else
      {
        string msg = string.Format("event: Final image not available; error {0}", e.DataStatus);
        AddMessage(msg);

        if (devID == _biobaseDevice.DeviceInfo.DeviceId)
          _BeepError();
        else
          System.Media.SystemSounds.Exclamation.Play();
        _biobaseDevice.mostResentKey = ActiveKeys.KEYS_NONE;
        _SetFinalStatusElements((BioBReturnCode)e.DataStatus);  // Update status on device with _e.DataStatus and _biobaseDevice.mostResentKey
      }


      SetDeviceState(DeviceState.device_opened_and_image_captured);


      //May want to reset device's LEDs, TFT display or Touch display here
      //      or wait until operator closes device and or starts new acquisition.
      //_ResetStatusElements();
      //_ResetGuidanceElements();
    }

    /*!
     * \fn void _biobaseDevice_DetectedObject()
     * \brief New event triggered if object was UNEXPECTEDLY detected on the platen
     * Note that all updates to the UI from this event must be done via Invoke calls.
     * NOTE: Use this event to display hints and prompt operator when something is on the platen
     */
    void _biobaseDevice_DetectedObject(object sender, BioBaseDetectObjectEventArgs e)
    {
      AddMessage("event BIOB_OBJECT_DETECTED: object on platen");
      BioBDeviceDectionAreaState state = e.DetectionAreaState;
      switch (state)
      {
        case BioBDeviceDectionAreaState.BIOB_CLEAR_OBJECT_FROM_DETECTION_AREA:
          AddMessage("Remove object from platen before continuing.");
          break;
        case BioBDeviceDectionAreaState.BIOB_DETECTION_AREA_CLEAR:
          AddMessage("Detected object has been remvoed from platen.");
          break;
      }
    }
    #endregion

  }
}