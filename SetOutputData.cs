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
    #region DeviceSounds


    /*!
     * \fn private void _BeepOK()
     * \brief Send beep pattern #3 to the device at full volume
     */
    private void _BeepOK()
    {
      _Beep("3", "100");
    }

    /*!
     * \fn private void _BeepError()
     * \brief Send beep pattern #1 to the device  at full volume
     * NOTE: if the developer wishes to view the XML forma structure, the xmlDoc.OuterXml string can be passed to the AddMessage method 
     * Catch BioBaseException and Exception and log errors
     */
    private void _BeepError()
    {
      _Beep("1", "100");
    }

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

        AddMessage(xmlDoc.OuterXml);  //option to log the formated XML string that is sent to the devices.

        _PerformUserOutput(xmlDoc.OuterXml);        //Send XML to device

      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_BEEPER, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_BEEPER, ex.Message));
      }
    }

    #endregion



    /*!
     * \fn private void _ResetStatusElements()
     * \brief Reset any status elements on the fingerprint device
     * Determine the type of device status elements and call the appropriate method.
     */
    private void _ResetStatusElements()
    {
      switch (_biobaseDevice.deviceGuidanceType)
      {
        case guidanceType.guidanceTypeNone:
          break;
        case guidanceType.guidanceTypeLScan:
          _ResetLScanLEDs();
          break;
        case guidanceType.guidanceTypeStatusLED:
          _ResetStatusLEDs();
          break;
        case guidanceType.guidanceTypeTFT:
        case guidanceType.guidanceTypeTFT_1000:
          _TftShowCompanyLogo();
          break;
        case guidanceType.guidanceTypeTouchDisplay:
          _ResetTouchDisplay();
          break;
      }
    }

    /*!
     * \fn private void _ResetGuidanceElements()
     * \brief Reset any guidance/icon elements on the fingerprint device
     * Determine the type of device guidance/icon elements and call the appropriate method.
     */
    private void _ResetGuidanceElements()
    {
      switch (_biobaseDevice.deviceGuidanceType)
      {
        case guidanceType.guidanceTypeNone:
          break;
        case guidanceType.guidanceTypeLScan:
          _ResetLScanLEDs();
          break;
        case guidanceType.guidanceTypeStatusLED:
          // Not required. Both status and icon LEDs are reset by _ResetStatusLEDs();   //a.k.a. _ResetIconLEDs();
          break;
        case guidanceType.guidanceTypeTFT:
        case guidanceType.guidanceTypeTFT_1000:
          // Not required. Done in _ResetStatusElements by _TftShowCompanyLogo();
          break;
        case guidanceType.guidanceTypeTouchDisplay:
          // Not required. Done in _ResetStatusElements by _ResetTouchDisplay();
          break;
      }
    }

    /*!
     * \fn private void _SetFinalStatusElements()
     * \brief Set any status on device with _e.DataStatus and _biobaseDevice.mostResentKey
     * \param DataStatus final image status from _biobaseDevice_DataAvailable
     * gloabal input _biobaseDevice.mostResentKey
     * Called from the _biobaseDevice_DataAvailable event handler
     * Determine the type of device status elements and call the appropriate method.
     */
    private void _SetFinalStatusElements(BioBReturnCode DataStatus)
    {
        switch (_biobaseDevice.deviceGuidanceType)
        {
          case guidanceType.guidanceTypeNone:
            break;
          case guidanceType.guidanceTypeLScan:
            _SetLScanStatusLEDs(DataStatus);
            break;
          case guidanceType.guidanceTypeStatusLED:
            _SetStatusLEDs(DataStatus);
            break;
          case guidanceType.guidanceTypeTFT:
          case guidanceType.guidanceTypeTFT_1000:
            _SetTftStatus(DataStatus);
            break;
          case guidanceType.guidanceTypeTouchDisplay:
            _SetTouchStatus(DataStatus);
            break;
        }
    }


    /*!
     * \fn private void _SetStatusElements()
     * \brief Set any status elements on the fingerprint device with mostRecentqualities and mostResentKey
     * Called from the _biobaseDevice_ObjectQuality event handler
     * Determine the type of device status elements and call the appropriate method.
     */
    private void _SetStatusElements()
    {
        switch (_biobaseDevice.deviceGuidanceType)
        {
          case guidanceType.guidanceTypeNone:
            break;
          case guidanceType.guidanceTypeLScan:
            _SetLScanStatusLEDs();
            break;
          case guidanceType.guidanceTypeStatusLED:
            _SetStatusLEDs();
            break;
          case guidanceType.guidanceTypeTFT:
          case guidanceType.guidanceTypeTFT_1000:
            _SetTftStatus();
            break;
          case guidanceType.guidanceTypeTouchDisplay:
            _SetTouchStatus();
            break;
        }
    }

    /*!
     * \fn private void _SetGuidanceElements()
     * \brief Set any guidance/icon elements on the fingerprint device 
     * Typically called from Begin Acquisition Process with position and impression being captured.
     * Determine the type of device guidance/icon elements and call the appropriate method.
     */
    private void _SetGuidanceElements()
    {
      switch (_biobaseDevice.deviceGuidanceType)
      {
        case guidanceType.guidanceTypeNone:
          break;
        case guidanceType.guidanceTypeLScan:
          _SetLScanLEDs();
          break;
        case guidanceType.guidanceTypeStatusLED:
          _SetIconLEDs();
          break;
        case guidanceType.guidanceTypeTFT:
        case guidanceType.guidanceTypeTFT_1000:
          _SetTftGuidance();
          break;
        case guidanceType.guidanceTypeTouchDisplay:
          _SetTouchGuidance();
          break;
      }
    }


    #region LScanLED
    /*!
     * \fn private void _ResetLScanLEDs()
     * \brief place holder to turn off the status and LED on legacy LScan 1000P and LScan 1000T
     * The end of support for the LScan 1000T was 31, March 2018
     * The end of support for the LScan 1000P was 31, December 2016
     */
    private void _ResetLScanLEDs()
    {
      //TODO: turn off the status and LED on legacy LScan 1000P and LScan 1000T
    }

    /*!
     * \fn private void _SetLScanStatusLEDs()
     * \brief place holder to control status on legacy LScan 1000P and LScan 1000T based on _biobaseDevice.mostRecentqualities
     * The end of support for the LScan 1000T was 31, March 2018
     * The end of support for the LScan 1000P was 31, December 2016
     */
    private void _SetLScanStatusLEDs()
    {
      //TODO:control status on legacy LScan 1000P and LScan 1000T 
    }
    /*!
     * \fn private void _SetLScanStatusLEDs()
     * \brief place holder to control FINAL status on legacy LScan 1000P and LScan 1000T based on e.DataStatus
     * The end of support for the LScan 1000T was 31, March 2018
     * The end of support for the LScan 1000P was 31, December 2016
     */
    private void _SetLScanStatusLEDs(BioBReturnCode DataStatus)
    {
      //TODO: control FINAL status on legacy LScan 1000P and LScan 1000T
    }

    /*!
     * \fn private void _SetIconLEDs()
     * \brief place holder to contorl LED based on position in legacy LScan 1000P and LScan 1000T.
     * Settings based on _biobaseDevicemostRecentPosition and _biobaseDevice.mostRecentImpression.
     * The end of support for the LScan 1000T was 31, March 2018
     * The end of support for the LScan 1000P was 31, December 2016
     */
    private void _SetLScanLEDs()
    {
      //TOD: contorl LED based on position in legacy LScan 1000P and LScan 1000T.
    }
    #endregion


    #region StatusAndIconLEDs
    // StatusAndIconLEDs - Status and Icon LED on LScan Guardian USB, Guardian F, Guardian L, Guardian R2, Patrol, Patrol ID, etc.

    /*!
     * \fn private void _SetStatusLEDs()
     * \brief Turn on the Device's status LEDs based on changes to the BioBObjectQualityState object
     * Then, the BioBase API requires that we reset Icon LEDs based on position and impression.
     * This method will each LED to use  non blinking LED colors!
     * global input _biobaseDevice.mostRecentqualities[] used to update status LEDs on device. 
     * global input _biobaseDevice.mostRecentPosition, _biobaseDevice.mostRecentImpression used to set icon LEDs on device.
     * These devices don't have keys to program with _mostResentKey
     * NOTE:  setting PropertyConstants.OUT_DATA_LED_S1_RED_B1 will give red slow blinking,
     *        setting PropertyConstants.OUT_DATA_LED_S1_RED_B2 will give red flash blinking, and
     *        setting both PropertyConstants.OUT_DATA_LED_S1_RED_B1 and PropertyConstants.OUT_DATA_LED_S1_RED_B2 will give red non-blinking
     *        
     * Typical XML format to Right thumb roll capture with solid green LEDs and red S1 status LED
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     *  -<OutputData>
     *   -<StatusLeds>
     *    <Led>NONE</Led>
     *
     *    <Led>S1_RED_B1</Led>
     *    <Led>S1_RED_B2</Led>
     *
     *    <Led>I4_GREEN_B1</Led>
     *    <Led>I4_GREEN_B2</Led>
     *   </StatusLeds>
     *  </OutputData>
     * </BioBase>
     *
     * Catch BioBaseException and Exception and log errors
     */
    private void _SetStatusLEDs()
    {
      try
      {
        // create xml tree based on _mostRecentqualities 
        XmlDocument xmlDoc = new XmlDocument();
        _SetupXMLOutputHeader(xmlDoc);

        XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
        xmlDoc.DocumentElement.AppendChild(outputDataElem);

        XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_STATUSLEDS);
        outputDataElem.AppendChild(statusDataElem);

        {
          // turn off all status LEDs
          _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_NONE);

          // Turn on first status LED bases on BioBObjectQualityState qualities[0]
          if (_biobaseDevice.mostRecentqualities.Length > 0)
          { // Set first status led
            _AddUserOutputStatusLed(xmlDoc, statusDataElem, _biobaseDevice.mostRecentqualities[0],
                                    null, null, //unknown
                                    PropertyConstants.OUT_DATA_LED_S1_RED_B1, PropertyConstants.OUT_DATA_LED_S1_RED_B2,  //Error color - non blinking Red - too dark, too light, bad shape, bad position, etc.
                                    PropertyConstants.OUT_DATA_LED_S1_RED_B1, PropertyConstants.OUT_DATA_LED_S1_RED_B2, PropertyConstants.OUT_DATA_LED_S1_GREEN_B1, PropertyConstants.OUT_DATA_LED_S1_GREEN_B2, //Not OK - non blinking Yellow - Not tracking
                                    PropertyConstants.OUT_DATA_LED_S1_GREEN_B1, PropertyConstants.OUT_DATA_LED_S1_GREEN_B2  //OK - non blinking Green - object good / correct pressure
                                    );
          }
          if (_biobaseDevice.mostRecentqualities.Length > 1)
          { // set second status led
            _AddUserOutputStatusLed(xmlDoc, statusDataElem, _biobaseDevice.mostRecentqualities[1],
                                    null, null, //unknown
                                    PropertyConstants.OUT_DATA_LED_S2_RED_B1, PropertyConstants.OUT_DATA_LED_S2_RED_B2,
                                    PropertyConstants.OUT_DATA_LED_S2_RED_B1, PropertyConstants.OUT_DATA_LED_S2_RED_B2, PropertyConstants.OUT_DATA_LED_S2_GREEN_B1, PropertyConstants.OUT_DATA_LED_S2_GREEN_B2,
                                    PropertyConstants.OUT_DATA_LED_S2_GREEN_B1, PropertyConstants.OUT_DATA_LED_S2_GREEN_B2);
          }
          if (_biobaseDevice.mostRecentqualities.Length > 2)
          { // set third status led
            _AddUserOutputStatusLed(xmlDoc, statusDataElem, _biobaseDevice.mostRecentqualities[2],
                                    null, null, //unknown
                                    PropertyConstants.OUT_DATA_LED_S3_RED_B1, PropertyConstants.OUT_DATA_LED_S3_RED_B2,
                                    PropertyConstants.OUT_DATA_LED_S3_RED_B1, PropertyConstants.OUT_DATA_LED_S3_RED_B2, PropertyConstants.OUT_DATA_LED_S3_GREEN_B1, PropertyConstants.OUT_DATA_LED_S3_GREEN_B2,
                                    PropertyConstants.OUT_DATA_LED_S3_GREEN_B1, PropertyConstants.OUT_DATA_LED_S3_GREEN_B2);
          }
          if (_biobaseDevice.mostRecentqualities.Length > 3)
          { // set fourth status led
            _AddUserOutputStatusLed(xmlDoc, statusDataElem, _biobaseDevice.mostRecentqualities[3],
                                    null, null, //unknown
                                    PropertyConstants.OUT_DATA_LED_S4_RED_B1, PropertyConstants.OUT_DATA_LED_S4_RED_B2,
                                    PropertyConstants.OUT_DATA_LED_S4_RED_B1, PropertyConstants.OUT_DATA_LED_S4_RED_B2, PropertyConstants.OUT_DATA_LED_S4_GREEN_B1, PropertyConstants.OUT_DATA_LED_S4_GREEN_B2,
                                    PropertyConstants.OUT_DATA_LED_S4_GREEN_B1, PropertyConstants.OUT_DATA_LED_S4_GREEN_B2);
          }

          // Must also set icon LEDs again!!!!!
          _SetIcon(xmlDoc, statusDataElem, _biobaseDevice.mostRecentPosition, _biobaseDevice.mostRecentImpression);
        }

        _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
      }
    }

    /*!
     * \fn private void _SetStatusLEDs()
     * \brief Turn on the Device's status LEDs based on changes to the e.DataStatus
     * Then, the BioBase API requires that we reset Icon LEDs based on position and impression.
     * This method will each LED to use  non blinking LED colors!
     * \param DataStatus final image status from _biobaseDevice_DataAvailable
     * global input _biobaseDevice.mostRecentPosition and _biobaseDevice.mostRecentImpression used to set icon LEDs on device.
     * These devices don't have keys to program with _biobaseDevice.mostResentKey
     * NOTE:  setting PropertyConstants.OUT_DATA_LED_S1_RED_B1 will give red slow blinking,
     *        setting PropertyConstants.OUT_DATA_LED_S1_RED_B2 will give red flash blinking, and
     *        setting both PropertyConstants.OUT_DATA_LED_S1_RED_B1 and PropertyConstants.OUT_DATA_LED_S1_RED_B2 will give red non-blinking
     *
     * Typical XML output for right thumb roll captured print roll warning flashing all status LEDs
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     *  -<OutputData>
     *   -<StatusLeds>
     *    <Led>NONE</Led>
     *    <Led>S1_RED_B2</Led>
     *    <Led>S1_GREEN_B2</Led>
     *    <Led>S2_RED_B2</Led>
     *    <Led>S2_GREEN_B2</Led>
     *    <Led>S3_RED_B2</Led>
     *    <Led>S3_GREEN_B2</Led>
     *    <Led>S4_RED_B2</Led>
     *    <Led>S4_GREEN_B2</Led>
     *
     *    <Led>I4_GREEN_B1</Led>
     *    <Led>I4_GREEN_B2</Led>
     *   </StatusLeds>
     *  </OutputData>
     * </BioBase>
     *
     * Typical XML output for right thumb roll captured print roll succeful with all status LEDs green
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     *  -<OutputData>
     *   -<StatusLeds>
     *    <Led>NONE</Led>
     *    <Led>S1_GREEN_B1</Led>
     *    <Led>S2_GREEN_B1</Led>
     *    <Led>S3_GREEN_B1</Led>
     *    <Led>S4_GREEN_B1</Led>
     *
     *    <Led>I4_GREEN_B1</Led>
     *    <Led>I4_GREEN_B2</Led>
     *    </StatusLeds>
     *    </OutputData>
     *    </BioBase>
     *    
     * Catch BioBaseException and Exception and log errors
     */
    private void _SetStatusLEDs(BioBReturnCode DataStatus)
    {
      try
      {
        // create xml tree based on mostRecentqualities 
        XmlDocument xmlDoc = new XmlDocument();
        _SetupXMLOutputHeader(xmlDoc);

        XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
        xmlDoc.DocumentElement.AppendChild(outputDataElem);

        XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_STATUSLEDS);
        outputDataElem.AppendChild(statusDataElem);

        {
          // turn off all status LEDs
          _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_NONE);

          if (DataStatus > 0)
          { // warning flash yellow
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S1_RED_B2);  // flash first LED
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S1_GREEN_B2);  // flash first LED
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S2_RED_B2);  // set second status led
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S2_GREEN_B2);  // set second status led
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S3_RED_B2);  // set third status led
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S3_GREEN_B2);  // set third status led
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S4_RED_B2);  // set fourth status led
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S4_GREEN_B2);  // set fourth status led
          }
          else if (DataStatus == BioBReturnCode.BIOB_SUCCESS)
          { // successfull - blink green
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S1_GREEN_B1);  // Set first status led
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S2_GREEN_B1);  // set second status led
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S3_GREEN_B1);  // set third status led
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S4_GREEN_B1);  // set fourth status led
          }
          if (DataStatus < 0)
          { // warning flash red error
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S1_RED_B2);  // flash first LED
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S2_RED_B2);  // set second status led
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S3_RED_B2);  // set third status led
            _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S4_RED_B2);  // set fourth status led
          }


          // Must also set icon LEDs again!!!!!
          _SetIcon(xmlDoc, statusDataElem, _biobaseDevice.mostRecentPosition, _biobaseDevice.mostRecentImpression);
        }

        _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
      }
    }

    /*!
     * \fn private void _ResetStatusLEDs()
     * \brief Turn off the status and Icon LEDs.
     * 
     * Typical XML format to reset LEDs on device
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     *  -<OutputData>
     *   -<StatusLeds>
     *    <Led>NONE</Led>
     *   </StatusLeds>
     *  </OutputData>
     *  </BioBase>
     * 
     * Catch BioBaseException and Exception and log errors
     */
    private void _ResetStatusLEDs()
    {
      try
      {
        // create xml tree
        XmlDocument xmlDoc = new XmlDocument();
        _SetupXMLOutputHeader(xmlDoc);

        XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
        xmlDoc.DocumentElement.AppendChild(outputDataElem);

        XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_STATUSLEDS);
        outputDataElem.AppendChild(statusDataElem);

        // turn off ALL status AND icon LEDs
        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_NONE);

        _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
      }
    }

    /*!
     * \fn private void _AddUserOutputStatusLed()
     * \brief use LED inputs strings to set approprate status based on quality input
     * Matches the quality value with the LED string to send to the device.
     * Method called once for each status LED on device
     */
    private void _AddUserOutputStatusLed(
                              XmlDocument xmlDoc,  ///< [in]  top level XML document for elements
                              XmlElement Parent,   ///< [in]  XML element to add element to
                              BioBObjectQualityState quality,
                              string ledUnknown1, string ledUnknown2,
                              string ledFingerError1, string ledFingerError2,    //Error color - non blinking Red - too dark, too light, bad shape, bad position, etc.
                              string ledTrackingNotOk1, string ledTrackingNotOk2, string ledTrackingNotOk3, string ledTrackingNotOk4,   //Not OK - non blinking Yellow - Not tracking
                              string ledOk1, string ledOk2 //OK - non blinking Green - object good / correct pressure
                              )
    {
      string[] pLed = { null, null, null, null };  // each led can have upto 4 different color and flashing rate

      switch (quality)
      {
        case BioBObjectQualityState.BIOB_OBJECT_TOO_DARK:
        case BioBObjectQualityState.BIOB_OBJECT_TOO_LIGHT:
        case BioBObjectQualityState.BIOB_OBJECT_BAD_SHAPE:
        case BioBObjectQualityState.BIOB_OBJECT_POSITION_NOT_OK:
        case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_HIGH:
        case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_LEFT:
        case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_RIGHT:
        case BioBObjectQualityState.BIOB_OBJECT_CORE_NOT_PRESENT:
        case BioBObjectQualityState.BIOB_OBJECT_CONFUSION:
        case BioBObjectQualityState.BIOB_OBJECT_ROTATED_CLOCKWISE:
        case BioBObjectQualityState.BIOB_OBJECT_ROTATED_COUNTERCLOCKWISE:
          // Set ERROR LED color and phase
          pLed[0] = ledFingerError1;
          pLed[1] = ledFingerError2;
          break;
        case BioBObjectQualityState.BIOB_OBJECT_TRACKING_NOT_OK:
          // Set NOT OK LED color and phase
          pLed[0] = ledTrackingNotOk1;
          pLed[1] = ledTrackingNotOk2;
          pLed[2] = ledTrackingNotOk3;
          pLed[3] = ledTrackingNotOk4;
          break;
        case BioBObjectQualityState.BIOB_OBJECT_GOOD:
          // Set OK LED color and phase
          pLed[0] = ledOk1;
          pLed[1] = ledOk2;
          break;
        case BioBObjectQualityState.BIOB_OBJECT_NOT_PRESENT:
          // Set unknown LED color and phase (often none/LED off)
          pLed[0] = ledUnknown1;
          pLed[1] = ledUnknown2;
          break;
        default:
          break;
      }

      for (int i = 0; i < 4; i++)
      {
        if (pLed[i] != null)
          _AddUserOutputElement(xmlDoc, Parent, PropertyConstants.OUT_DATA_LED, pLed[i]);
      }
    }

    /*!
     * \fn private void _SetIconLEDs()
     * \brief Turn on Icon LED based on position.
     * Icon LEDs will be non blinking Green
     * global input _biobaseDevice.mostRecentPosition and _biobaseDevice.mostRecentImpression used to set icon LEDs on device.
     * 
     * Typical XML format to Right thumb capture with solid green LEDs
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     *  -<OutputData>
     *   -<StatusLeds>
     *    <Led>NONE</Led>
     *    <Led>I4_GREEN_B1</Led>
     *    <Led>I4_GREEN_B2</Led>
     *   </StatusLeds>
     *  </OutputData>
     * </BioBase>
     * 
     * Catch BioBaseException and Exception and log errors
     */
    private void _SetIconLEDs()
    {
      try
      {
        // create xml tree
        XmlDocument xmlDoc = new XmlDocument();
        _SetupXMLOutputHeader(xmlDoc);

        XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
        xmlDoc.DocumentElement.AppendChild(outputDataElem);

        XmlElement DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_STATUSLEDS);
        outputDataElem.AppendChild(DataElem);
        
        // turn off ALL Status and Icon LEDs
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_NONE);

        _SetIcon(xmlDoc, DataElem, _biobaseDevice.mostRecentPosition, _biobaseDevice.mostRecentImpression);

        _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
      }
    }

    /*!
     * \fn private void _SetIcon()
     * \brief set individual XML for icon LED based on position.
     * This method does not include the XML header so it can be used to set initial icon LED and also with status LEDs.
     * Icon LEDs will be non blinking Green
     * 
     * Exceptions passed to calling method
     */
    private void _SetIcon(XmlDocument xmlDoc, XmlElement DataElem, string postion, string impression)
    {
      // Turn on first status LED bases on BioBObjectQualityState qualities[0]
      if (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_THUMB)
      {
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I4_GREEN_B1);
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I4_GREEN_B2);
      }
      else if ((postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX) ||
          (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_MIDDLE) ||
          (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING) ||
          (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LITTLE) ||
          (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FOUR_FINGERS) ||
          (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX_AND_MIDDLE) ||
          (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING_AND_LITTLE) ||
          (postion == PropertyConstants.DEV_PROP_POS_TYPE_TWO_FINGERS)   // Generic two fingers so in this application it will assume right and left index 
        )
      {
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I3_GREEN_B1);
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I3_GREEN_B2);
      }

      else if (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_THUMB)
      {
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I2_GREEN_B1);
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I2_GREEN_B2);
      }
      else if ((postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX) ||
          (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_MIDDLE) ||
          (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING) ||
          (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LITTLE) ||
          (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FOUR_FINGERS) ||
          (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX_AND_MIDDLE) ||
          (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING_AND_LITTLE))
      {
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I1_GREEN_B1);
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I1_GREEN_B2);
      }

      else if (postion == PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS)
      { //PreTriggerMessage = "Place 2 thumbs!";
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I2_GREEN_B1);
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I2_GREEN_B2);
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I4_GREEN_B1);
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I4_GREEN_B2);
      }
      else if (postion == PropertyConstants.DEV_PROP_POS_TYPE_BOTH_INDEXES_AND_MIDDLES)
      { //PreTriggerMessage = "Place Flat 4: Left Middle + Left Index + Right Index + Right Middle!";
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I1_GREEN_B1);
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I1_GREEN_B2);
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I3_GREEN_B1);
        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I3_GREEN_B2);
      }
    }
    #endregion


    #region TFTDisplay
    //TFTDisplay - control display on LScan 500P, LScan 500, and LScan 1000PX, LScan 1000, LScan 500, etc

    /// <summary>
    /// Class TFT_ObjectDictionary is the L SCAN palm scanner TFT segment object definitions.
    /// Each segment of the TFT display is controlled independently.
    /// This class defines each of the segments that are available in the FingerSelectionScreen formated screen.
    ///
    /// Base TFT_ObjectDictionary class used for the FingerSelectionScreen formated screen.
    /// NOTE: This sample application only uses the CaptureProgressScreen. Minor changes need to be done to use the FingerSelectionScreen formated screen.
    ///
    /// ***NOTE***: Each key is initialized to PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE but most other values could be used.
    ///             However, you must not use keys set to PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED when first switching to these screens.
    ///             ***BUT for the sake of speed, this application, will default to OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED and the in 
    ///             the initialization function, set all the keys to OUT_DATA_DISPLAY_OBJECT_INACTIVE.***
    /// ***NOTE***: Errors in XML formated data for the TFT are often related to using OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED without 
    ///             first setting the element to a valid value.
    /// </summary>
    protected class TFT_ObjectDictionary : Dictionary<string, string>
    {
      public TFT_ObjectDictionary()
      {
        this.Add(PropertyConstants.OUT_DATA_TFT_CTRL_LEFT, PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_ERASE);
        this.Add(PropertyConstants.OUT_DATA_TFT_CTRL_RIGHT, PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_ERASE);

        this.Add(PropertyConstants.OUT_DATA_TFT_L_PALM, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_THENAR, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_LOWERT, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_INTER, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_THUMB, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_INDEX, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_MIDDLE, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_RING, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_L_SMALL, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);

        this.Add(PropertyConstants.OUT_DATA_TFT_R_PALM, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_THENAR, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_LOWERT, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_INTER, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_THUMB, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_INDEX, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_MIDDLE, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_RING, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
        this.Add(PropertyConstants.OUT_DATA_TFT_R_SMALL, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
      }
      ~TFT_ObjectDictionary()
      {
        this.Clear();
      }
    }

    /// <summary>
    /// Class TFT_ObjectCaptureDictionary is the L SCAN palm scanner TFT segment object definitions.
    /// Derived from the TFT_ObjectDictionary to add additional segments for the CaptureProgressScreen
    /// Each segment of the TFT display is controlled independently.
    /// This class defines each of the segments that are available in the CaptureProgressScreen formated screen.
    ///
    /// The TFT_ObjectCaptureDictionary class used for the CaptureProgressScreen formated screen.
    /// NOTE: This sample application only uses the CaptureProgressScreen. Minor changes need to be done to use the FingerSelectionScreen formated screen.
    ///
    /// ***NOTE***: Each key is initialized to PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE but most other values could be used.
    ///             However, you must not use keys set to PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED when first switching to these screens.
    ///             ***BUT for the sake of speed, this application, will default to OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED and the in 
    ///             the initialization function, set all the keys to OUT_DATA_DISPLAY_OBJECT_INACTIVE.***
    /// </summary>
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
     * \fn private void _SetTftStatus()
     * \brief Display positioning status on TFT display based on BioBObjectQualityState object
     * global inputs: _biobaseDevice.mostRecentqualities[] used to update status LEDs on device
     * global inputs: _biobaseDevice.mostResentKey program with active keys
     * 
     * NOTE: This function assumes that the FingerSelectionScreen or CaptureProgressScreen formated 
     * screen has been initialized with the guidance elements via the _SetTftGuidance() method.
     * 
     * Catch BioBaseException and Exception and log errors
     */
    private void _SetTftStatus()
    {
      try
      {
        bool tooHigh = false;
        bool tooLeft = false;
        bool tooRight = false;
        bool flexToo = false;

        // Consolidate status of each individual finger to create one status
        for (int i = 0; i < _biobaseDevice.mostRecentqualities.Length; i++)
        {
          switch (_biobaseDevice.mostRecentqualities[i])
          {
            case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_HIGH: tooHigh = true; break;
            case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_LEFT: tooLeft = true; break;
            case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_RIGHT: tooRight = true; break;

            case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_HIGH:
            case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_LEFT:
            case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_RIGHT:
            case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_LOW: flexToo = true; break;
          }
        }

        string status;
        if ((tooHigh && tooLeft && tooRight) || flexToo)
          status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_DOWN_LEFT_RIGHT_UP;
        else if (tooHigh && tooLeft)
          status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_DOWN_RIGHT;
        else if (tooHigh && tooRight)
          status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_DOWN_LEFT;
        else if (tooRight && tooLeft)
          status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_LEFT_RIGHT;
        else if (tooRight)
          status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_LEFT;
        else if (tooLeft)
          status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_RIGHT;
        else if (tooHigh)
          status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_DOWN;
        else
          status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ERASE;


        string ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED;
        string ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED;
        switch (_biobaseDevice.mostResentKey)
        {
          case (ActiveKeys.KEYS_NONE):
            ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_ERASE;
            ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_ERASE;
            break;
          case (ActiveKeys.KEYS_OK_CONTRAST):
            // update buttons on first status change after start of catpure
            ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_CONTRAST;
            ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_OK;
            break;
          case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
            // update buttons and status when DataAvailable has error
            ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_REPEAT;
            ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_OK;
            status = (_biobaseDevice.mostRecentImpression == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL) ?
                    PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ROLL_ERROR : PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_CAPTURE_ERROR;
            break;
        }


        if ((status == m_CurrentTFTstatus) && (_biobaseDevice.mostResentKey == m_CurrentTFTKey))
          return; // nothing to update

        m_CurrentTFTstatus = status;
        m_CurrentTFTKey = _biobaseDevice.mostResentKey;
        TFT_ObjectCaptureDictionary obj = new TFT_ObjectCaptureDictionary();

        obj[PropertyConstants.OUT_DATA_TFT_CTRL_LEFT] = ctrlLeft;
        obj[PropertyConstants.OUT_DATA_TFT_CTRL_RIGHT] = ctrlRight;
        obj[PropertyConstants.OUT_DATA_TFT_STAT_TOP] = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_LEAVE_UNCHANGED;
        obj[PropertyConstants.OUT_DATA_TFT_STAT_BOTTOM] = m_CurrentTFTstatus;
        // NOTE: TFT_ObjectCaptureDictionary constructor sets values to OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED
        _TftShowFingerCaptureScreen(false, obj);


        //////////////////////////////////////////////////////
        //Outputdata to notify LSE to monitor inputs from keys
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement outputDataElem;
        XmlElement DataElem;
        switch (_biobaseDevice.mostResentKey)
        {
          case (ActiveKeys.KEYS_NONE):
            _SetupXMLOutputHeader(xmlDoc);

            outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
            xmlDoc.DocumentElement.AppendChild(outputDataElem);

            DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_ACTIVEBUTTONS);
            outputDataElem.AppendChild(DataElem);
            _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_NONE);
            _PerformUserOutput(xmlDoc.OuterXml);
            break;
          case (ActiveKeys.KEYS_OK_CONTRAST):
          case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
            _SetupXMLOutputHeader(xmlDoc);

            outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
            xmlDoc.DocumentElement.AppendChild(outputDataElem);

            DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_ACTIVEBUTTONS);
            outputDataElem.AppendChild(DataElem);
            _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_OK);
            _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_CANCEL);
            _PerformUserOutput(xmlDoc.OuterXml);
            break;
        }
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT, ex.Message));
      }
    }

    /*!
     * \fn private void _SetTftStatus()
     * \brief Display positioning status on TFT display based on e.DataStatus
     * \param DataStatus final image status from _biobaseDevice_DataAvailable
     * global inputs: _biobaseDevice.mostResentKey program with active keys
     * 
     * NOTE: This function assumes that the FingerSelectionScreen or CaptureProgressScreen formated 
     * screen has been initialized with the guidance elements via the _SetTftGuidance() method.
     * 
     * Catch BioBaseException and Exception and log errors
     */
    private void _SetTftStatus(BioBReturnCode DataStatus)
    {
      try
      {
        string status;

        // check status 
        switch ((BioBReturnCode)DataStatus)
        {
          case BioBReturnCode.BIOB_SUCCESS:
            status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_OK;
            break;

          case BioBReturnCode.BIOB_OPTICS_SURFACE_DIRTY:
            status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_SURFACE_IS_DIRTY;
            break;

          case BioBReturnCode.BIOB_REPLACE_PAD:
            status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_SURFACE_IS_DIRTY_ALT_1;
            break;

          case BioBReturnCode.BIOB_BAD_SCAN:
          case BioBReturnCode.BIOB_NO_CAPTURE_ACTIVE:
            status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_CAPTURE_ERROR;
            break;

          case BioBReturnCode.BIOB_AUTOCAPTURE_SEGMENTATION:
            status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_QUALITY_CHECK_ERROR;
            break;

          case BioBReturnCode.BIOB_NO_OBJECT:
          case BioBReturnCode.BIOB_SPOOF_DETECTED:
          case BioBReturnCode.BIOB_SPOOF_DETECTOR_FAIL:
            status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_SEQUENCE_CHECK_ERROR_ALT_1;
            break;

          case BioBReturnCode.BIOB_ROLL_SHIFTED_HORIZONTALLY:
          case BioBReturnCode.BIOB_ROLL_SHIFTED_VERTICALLY:
          case BioBReturnCode.BIOB_ROLL_LIFTED_TIP:
          case BioBReturnCode.BIOB_ROLL_ON_BORDER:
          case BioBReturnCode.BIOB_ROLL_PAUSED:
          case BioBReturnCode.BIOB_ROLL_FINGER_TOO_NARROW:
            status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ROLL_ERROR;
            break;

          default:
            status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_COMMON_ERROR;
            break;
        }


        string ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED;
        string ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED;
        switch (_biobaseDevice.mostResentKey)
        {
          case (ActiveKeys.KEYS_NONE):
            ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_ERASE;
            ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_ERASE;
            break;
          case (ActiveKeys.KEYS_OK_CONTRAST):
            // update buttons for OK to accept image captured with warning 
            ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_CONTRAST;
            ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_OK;
            break;
          case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
            // update buttons to retry capture when DataAvailable has warngin 
            ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_REPEAT;
            ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_OK;
            break;
        }

        if ((status == m_CurrentTFTstatus) && (_biobaseDevice.mostResentKey == m_CurrentTFTKey))
          return; // nothing to update

        m_CurrentTFTKey = _biobaseDevice.mostResentKey;

        TFT_ObjectCaptureDictionary obj = new TFT_ObjectCaptureDictionary();
        obj[PropertyConstants.OUT_DATA_TFT_CTRL_LEFT] = ctrlLeft;
        obj[PropertyConstants.OUT_DATA_TFT_CTRL_RIGHT] = ctrlRight;
        obj[PropertyConstants.OUT_DATA_TFT_STAT_TOP] = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_LEAVE_UNCHANGED;
        obj[PropertyConstants.OUT_DATA_TFT_STAT_BOTTOM] = m_CurrentTFTstatus = status;
        // NOTE: TFT_ObjectCaptureDictionary constructor sets values to OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED
        _TftShowFingerCaptureScreen(false, obj);

        //////////////////////////////////////////////////////
        //Outputdata to notify LSE to monitor inputs from keys
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement outputDataElem;
        XmlElement DataElem;
        switch (_biobaseDevice.mostResentKey)
        {
          case (ActiveKeys.KEYS_NONE):
            _SetupXMLOutputHeader(xmlDoc);

            outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
            xmlDoc.DocumentElement.AppendChild(outputDataElem);

            DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_ACTIVEBUTTONS);
            outputDataElem.AppendChild(DataElem);
            _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_NONE);
            _PerformUserOutput(xmlDoc.OuterXml);
            break;
          case (ActiveKeys.KEYS_OK_CONTRAST):
          case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
            _SetupXMLOutputHeader(xmlDoc);

            outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
            xmlDoc.DocumentElement.AppendChild(outputDataElem);

            DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_ACTIVEBUTTONS);
            outputDataElem.AppendChild(DataElem);
            _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_OK);
            _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_CANCEL);
            _PerformUserOutput(xmlDoc.OuterXml);
            break;
        }

      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT, ex.Message));
      }
    }

    /*!
     * \fn private void _TftShowCompanyLogo()
     * \brief Display company logo to reset TFT screen to default.
     * 
     * Typical XML format to display logo on TFT display
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     *  -<OutputData>
     *   -<Tft>
     *    -<LogoScreen>
     *     <Option>SHOW_FW_VERSION</Option>
     *     <ProgressBarPercent>0</ProgressBarPercent>
     *    </LogoScreen>
     *   </Tft>
     *  </OutputData>
     * </BioBase>
     * 
     * Catch BioBaseException and Exception and log errors
     */
    private void _TftShowCompanyLogo()
    {
      try
      {
        // create xml tree
        XmlDocument xmlDoc = new XmlDocument();
        _SetupXMLOutputHeader(xmlDoc);

        XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
        xmlDoc.DocumentElement.AppendChild(outputDataElem);

        XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT);
        outputDataElem.AppendChild(statusDataElem);

        XmlElement LogElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT_LOG_SCREEN);
        statusDataElem.AppendChild(LogElem);

        _AddUserOutputElement(xmlDoc, LogElem, PropertyConstants.OUT_DATA_TFT_LOG_SCR_OPTION, PropertyConstants.OUT_DATA_TFT_LOG_SCR_SHOW_FW_VERSION);
        _AddUserOutputElement(xmlDoc, LogElem, PropertyConstants.OUT_DATA_TFT_LOG_SCR_PROGRESS, "0");

        _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT_LOG_SCREEN, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT_LOG_SCREEN, ex.Message));
      }
    }

    /*!
     * \fn private void _SetTftGuidance()
     * \brief Display guidance based on position and impression to the TFT screen.
     * global inputs: _biobaseDevice.mostRecentPosition and _biobaseDevice.mostRecentImpression
     * global inputs: _biobaseDevice.mostResentKey
     * 
     * NOTE: When application adds support for finger annotation, this methods logic must add support for annotated fingers.
     * 
     * NOTE: Because this is the function that will change to the FingerSelectionScreen or CaptureProgressScreen, all
     *       ALL TFT_ObjectCaptureDictionary key values will be changed to OUT_DATA_DISPLAY_OBJECT_INACTIVE.
     * NOTE: When changing to the FingerSelectionScreen or CaptureProgressScreen formated screen, none of the 
     *       and TFT_ObjectDictionary key value can be set to OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED!
     *
     * Catch BioBaseException and Exception and log errors
     */
    private void _SetTftGuidance()
    {
      try
      {
        TFT_ObjectCaptureDictionary obj = new TFT_ObjectCaptureDictionary();

        // Initialize array to inactive. Must make sure no values are set to OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED when switching screens.
        var keys = new List<string>(obj.Keys);
        foreach (string key in keys)
        {
          obj[key] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE;
        }


        switch (_biobaseDevice.mostRecentPosition)
        {
          case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_THUMB): //PreTriggerMessage = "Impression?(Place):(roll) right thumb!";
            obj[PropertyConstants.OUT_DATA_TFT_R_THUMB] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX):  //PreTriggerMessage = "Impression?(Place):(roll) right index finger!";
            obj[PropertyConstants.OUT_DATA_TFT_R_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_MIDDLE):
            obj[PropertyConstants.OUT_DATA_TFT_R_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING):
            obj[PropertyConstants.OUT_DATA_TFT_R_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LITTLE):
            obj[PropertyConstants.OUT_DATA_TFT_R_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_THUMB): //"Place left thumb!";
            obj[PropertyConstants.OUT_DATA_TFT_R_THUMB] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX):
            obj[PropertyConstants.OUT_DATA_TFT_L_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_MIDDLE):
            obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING):
            obj[PropertyConstants.OUT_DATA_TFT_L_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LITTLE):
            obj[PropertyConstants.OUT_DATA_TFT_L_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FOUR_FINGERS):
            // PreTriggerMessage = "Place right 4 flat fingers!";
            obj[PropertyConstants.OUT_DATA_TFT_R_INDEX]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_RING]   = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_SMALL]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;

          case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FOUR_FINGERS):
            //PreTriggerMessage = "Place left 4 flat fingers!";
            obj[PropertyConstants.OUT_DATA_TFT_L_INDEX]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_RING]   = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_SMALL]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS):
            //PreTriggerMessage = "Place 2 thumbs!";
            obj[PropertyConstants.OUT_DATA_TFT_R_THUMB] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_THUMB] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_INDEXES_AND_MIDDLES):
            //PreTriggerMessage = "Place Flat 4: Left Middle + Left Index + Right Index + Right Middle!";
            // case is not supported directly by LScan palm device but software can be setup as an option.
            obj[PropertyConstants.OUT_DATA_TFT_R_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_INDEX]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_TWO_FINGERS):
            // Generic two fingers so in this application it will assume right and left index on TFT display
            obj[PropertyConstants.OUT_DATA_TFT_R_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX_AND_MIDDLE):
            // case is not supported directly by LScan palm device but software can be setup as an option.
            obj[PropertyConstants.OUT_DATA_TFT_R_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING_AND_LITTLE):
            // case is not supported directly by LScan palm device but software can be setup as an option.
            obj[PropertyConstants.OUT_DATA_TFT_R_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX_AND_MIDDLE):
            // case is not supported directly by LScan palm device but software can be setup as an option.
            obj[PropertyConstants.OUT_DATA_TFT_L_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING_AND_LITTLE):
            // case is not supported directly by LScan palm device but software can be setup as an option.
            obj[PropertyConstants.OUT_DATA_TFT_L_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;

          case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FULL_PALM):
            //PreTriggerMessage = "Place right upper palm!";
            // case is not supported directly by LScan palm device but software can be setup as an option.
            obj[PropertyConstants.OUT_DATA_TFT_L_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_RING]   = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_SMALL]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_PALM]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_INTER] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            SetUILedColors(5); // Display five UI status LEDs for fingers and palm.
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_WRITERS_PALM):
            //PreTriggerMessage = "Place right writers palm!";
            obj[PropertyConstants.OUT_DATA_TFT_R_THENAR] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LOWER_PALM):
            //PreTriggerMessage = "Place right lower palm!";
            // One part larger lower palm
            obj[PropertyConstants.OUT_DATA_TFT_R_PALM] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_LOWERT] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_MISSING;

            /*OR two part lower palm...
            obj[PropertyConstants.OUT_DATA_TFT_R_PALM]   = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_LOWERT] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            */
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_UPPER_PALM):
            //PreTriggerMessage = "Place right upper palm!";
            obj[PropertyConstants.OUT_DATA_TFT_R_INDEX]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_RING]   = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_SMALL]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_R_INTER]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            SetUILedColors(5); // Display five UI status LEDs for fingers and palm.
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FULL_PALM):
            //PreTriggerMessage = "Place left upper palm!";
            // case is not supported directly by LScan palm device but software can be setup as an option.
            obj[PropertyConstants.OUT_DATA_TFT_L_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_RING]   = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_SMALL]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_PALM]   = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_INTER]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            SetUILedColors(5); // Display five UI status LEDs for fingers and palm.
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_WRITERS_PALM):
            //PreTriggerMessage = "Place left writers palm!";
            obj[PropertyConstants.OUT_DATA_TFT_L_THENAR] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LOWER_PALM):
            //PreTriggerMessage = "Place left lower palm!";
            // One part larger lower palm
            obj[PropertyConstants.OUT_DATA_TFT_L_PALM]   = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_LOWERT] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_MISSING;
            
            /*OR two part lower palm...
            obj[PropertyConstants.OUT_DATA_TFT_L_PALM]   = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_LOWERT] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            */
            break;
          case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_UPPER_PALM):
            //PreTriggerMessage = "Place left upper palm!";
            obj[PropertyConstants.OUT_DATA_TFT_L_INDEX]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_RING]   = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_SMALL]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            obj[PropertyConstants.OUT_DATA_TFT_L_INTER]  = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
            SetUILedColors(5); // Display five UI status LEDs for fingers and palm.
            break;
        }

        string statusTop = "";
        switch (_biobaseDevice.mostRecentImpression)
        {
          case (PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL):
            switch (_biobaseDevice.mostRecentPosition)
            {
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_THUMB):
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX):
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_MIDDLE):
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING):
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LITTLE):
                statusTop = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_HORIZONTAL_RIGHT;
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_THUMB):
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX):
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_MIDDLE):
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING):
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LITTLE):
                statusTop = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_HORIZONTAL_LEFT;
                break;
              default:
                statusTop = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_HORIZONTAL;
                break;
            }
            break;
          case (PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL_VERTICAL):
            statusTop = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_VERTICAL;
            break;
          default:
            statusTop = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_CAPTURE_FLAT;
            break;
        }


        string statusBottom = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ERASE;

        string ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED;
        string ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_LEAVE_UNCHANGED;
        switch (_biobaseDevice.mostResentKey)
        {
          case (ActiveKeys.KEYS_NONE):
            ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_ERASE;
            ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ERASE;
            break;
          case (ActiveKeys.KEYS_OK_CONTRAST):
            // update buttons on first status change after start of catpure
            ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_CONTRAST;
            ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_OK;
            break;
          case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
            // update buttons and status when DataAvailable has error
            ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_REPEAT;
            ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_OK;
            statusBottom = (_biobaseDevice.mostRecentImpression == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL) ?
                    PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ROLL_ERROR : PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_CAPTURE_ERROR;
            break;
        }


        obj[PropertyConstants.OUT_DATA_TFT_CTRL_LEFT] = ctrlLeft;
        obj[PropertyConstants.OUT_DATA_TFT_CTRL_RIGHT] = ctrlRight;
        obj[PropertyConstants.OUT_DATA_TFT_STAT_TOP] = statusTop;
        obj[PropertyConstants.OUT_DATA_TFT_STAT_BOTTOM] = statusBottom;
        _TftShowFingerCaptureScreen(false, obj);


        //////////////////////////////////////////////////////
        /*Outputdata to notify LSE to monitor inputs from keys
         * 
         * Typical XML output for Active keys:
         * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
         * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
         *  -<OutputData>
         *   -<ActiveDeviceButtons>
         *    <Key>OK</Key>
         *    <Key>CANCEL</Key>
         *   </ActiveDeviceButtons>
         *  </OutputData>
         * </BioBase>
         */
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement outputDataElem;
        XmlElement DataElem;
        switch (_biobaseDevice.mostResentKey)
        {
          case (ActiveKeys.KEYS_NONE):
            _SetupXMLOutputHeader(xmlDoc);

            outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
            xmlDoc.DocumentElement.AppendChild(outputDataElem);

            DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_ACTIVEBUTTONS);
            outputDataElem.AppendChild(DataElem);
            _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_NONE);
            _PerformUserOutput(xmlDoc.OuterXml);
            break;
          case (ActiveKeys.KEYS_OK_CONTRAST):
          case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
            _SetupXMLOutputHeader(xmlDoc);

            outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
            xmlDoc.DocumentElement.AppendChild(outputDataElem);

            DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_ACTIVEBUTTONS);
            outputDataElem.AppendChild(DataElem);
            _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_OK);
            _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_CANCEL);
            _PerformUserOutput(xmlDoc.OuterXml);
            break;
        }
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT_CAP_SCREEN, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT_CAP_SCREEN, ex.Message));
      }
    }


    /*!
     * \fn private void _TftShowCaptureProgress()
     * \brief Format XML and send output for status and guidance to the TFT OUT_DATA_TFT_FIN_SCREEN and OUT_DATA_TFT_CAP_SCREEN
     * input: finger true for OUT_DATA_TFT_FIN_SCREEN and false for OUT_DATA_TFT_CAP_SCREEN
     * input: TFT_ObjectDictionary defines how each segment on the TFT is displayed.
     * 
     * No try/catch as the BioBaseException and Exception are passed up to the calling function
     * 
     * Typical XML format for TFT display for the CaptureProgressScreen formated screen:
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     *  -<OutputData>
     *   -<Tft>
     *    -<CaptureProgressScreen>
     *      <LeftButton>YELLOW_CONTRAST</LeftButton>
     *      <RightButton>GREEN_OK</RightButton>
     *      <StatTop>CAPTURE_FLAT</StatTop>
     *      <StatBottom>ERASE</StatBottom>
     *      <ColorLeftPalm>INACTIVE</ColorLeftPalm>
     *      <ColorLeftThenar>INACTIVE</ColorLeftThenar>
     *      <ColorLeftLowerThenar>INACTIVE</ColorLeftLowerThenar>
     *      <ColorLeftInterDigital>INACTIVE</ColorLeftInterDigital>
     *      <ColorLeftThumb>INACTIVE</ColorLeftThumb>
     *      <ColorLeftIndex>INACTIVE</ColorLeftIndex>
     *      <ColorLeftMiddle>INACTIVE</ColorLeftMiddle>
     *      <ColorLeftRing>INACTIVE</ColorLeftRing>
     *      <ColorLeftSmall>INACTIVE</ColorLeftSmall>
     *      <ColorRightPalm>INACTIVE</ColorRightPalm>
     *      <ColorRightThenar>INACTIVE</ColorRightThenar>
     *      <ColorRightLowerThenar>INACTIVE</ColorRightLowerThenar>
     *      <ColorRightInterDigital>INACTIVE</ColorRightInterDigital>
     *      <ColorRightThumb>INACTIVE</ColorRightThumb>
     *      <ColorRightIndex>CURRENT_SELECTION</ColorRightIndex>
     *      <ColorRightMiddle>CURRENT_SELECTION</ColorRightMiddle>
     *      <ColorRightRing>CURRENT_SELECTION</ColorRightRing>
     *      <ColorRightSmall>CURRENT_SELECTION</ColorRightSmall>
     *     </CaptureProgressScreen>
     *    </Tft>
     *   </OutputData>
     *  </BioBase>
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
      if (finger)
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


    #region TouchDisplay
    // TouchDisplay - Control Touch screen and LED display on Guardian, Guardian 300, Guardain 200 and Guardian 100


    /*!
     * \fn private void _ResetTouchDisplay()
     * \brief Display the comany log on the Touch Screen or turn off all the LEDs on the LED display
     * 
     * Typical XML format to display default touch screen:
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     *  -<OutputData>
     *   -<TouchDisplay>
     *    -<DesignTemplate>
     *     <URI>file:///E:/Templates/index_standby.html</URI>
     *    </DesignTemplate>
     *   </TouchDisplay>
     *  </OutputData>
     * </BioBase>
     * Catch BioBaseException and Exception and log errors
     */
    private void _ResetTouchDisplay()
    {
      try
      {
        // Dispaly index_standby.html on Guardian 300
        string template = m_TouchDisplayTemplatePath + "index_standby.html";

        // create xml tree
        XmlDocument xmlDoc = new XmlDocument();
        _SetupXMLOutputHeader(xmlDoc);

        XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
        xmlDoc.DocumentElement.AppendChild(outputDataElem);

        XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY);
        outputDataElem.AppendChild(statusDataElem);

        XmlElement LogElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY_DESIGNTEMPLATE);
        statusDataElem.AppendChild(LogElem);

        _AddUserOutputElement(xmlDoc, LogElem, PropertyConstants.OUT_DATA_TOUCHDISPLAY_URI, template);

        _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
      }
    }

    /*!
     * \fn private void _SetTouchStatus()
     * \brief The TouchDisplayStandardTemplate template and ExternalParameters are already set in _SetTouchGuidance!
     * This method doeesn't need _biobaseDevice.mostRecentqualities because this is automatically process by the template
     * global inputs: _biobaseDevice.mostResentKey 
     * 
     * Typical XML output for status update of flat capture of right thumb:
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     * -<OutputData>
     * -<TouchDisplay>
     * -<DesignTemplate>
     * <URI>file:///E:/temp/Templates/index_standard_thumbs.html</URI>
     * <ExternalParameter Value="2" Key="ButtonRetry"/>
     * <ExternalParameter Value="1" Key="ButtonConfirm"/>
     * <ExternalParameter Value="1" Key="FP1"/>
     * <ExternalParameter Value="0" Key="FP6"/>
     * </DesignTemplate>
     * </TouchDisplay>
     * </OutputData>
     * </BioBase>
     * 
     * Catch BioBaseException and Exception and log errors
     */
    private void _SetTouchStatus()
    {
      try
      {
        // Update TouchDisplayStandardExternalParameters with new _biobaseDevice.mostResentKey
        string Retry = "2"; //hidden
        string Confirm = "2"; //hidden
        switch (_biobaseDevice.mostResentKey)
        {
          case (ActiveKeys.KEYS_NONE):
            Retry = "2"; //hidden
            Confirm = "2"; //hidden
            break;
          case (ActiveKeys.KEYS_OK_CONTRAST):
            Retry = "2"; //hidden
            Confirm = "1"; //active  - enable AdjustAcquisitionProcess
            break;
          case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
            Retry = "1";  // button active  - enable RescanImage
            Confirm = "1";  // button active - enable accept image captured with warning
            break;
        }
        _biobaseDevice.TouchDisplayStandardExternalParameters["ButtonRetry"] = Retry;  // update button 
        _biobaseDevice.TouchDisplayStandardExternalParameters["ButtonConfirm"] = Confirm;  // update button 

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
        _AddUserOutputElement(xmlDoc, TemplateElem, PropertyConstants.OUT_DATA_TOUCHDISPLAY_URI, _biobaseDevice.TouchDisplayStandardTemplate);


        foreach (KeyValuePair<string, string> Ex in _biobaseDevice.TouchDisplayStandardExternalParameters)
        {
          XmlElement EpElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER);
          EpElem.SetAttribute(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER_KEY, Ex.Key);
          EpElem.SetAttribute(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER_VALUE, Ex.Value);
          TemplateElem.AppendChild(EpElem);
        }

        _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
      }
    }

    /*!
     * \fn private void _SetTouchStatus()
     * \brief The TouchDisplayStandardTemplate template and ExternalParameters are already set in _SetTouchGuidance!
     * \param DataStatus final image status from _biobaseDevice_DataAvailable
     * global inputs: _biobaseDevice.mostResentKey 
     * 
     * Typical XML output for successful flat capture of right thumb:
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     *  -<OutputData>
     *   -<TouchDisplay>
     *    -<DesignTemplate>
     *     <URI>file:///E:/temp/Templates/index_standard_thumbs.html</URI>
     *     <ExternalParameter Value="2" Key="ButtonRetry"/>
     *     <ExternalParameter Value="2" Key="ButtonConfirm"/>
     *     <ExternalParameter Value="1" Key="FP1"/>
     *     <ExternalParameter Value="0" Key="FP6"/>
     *     <ExternalParameter Value="1" Key="Result"/>
     *    </DesignTemplate>
     *   </TouchDisplay>
     *  </OutputData>
     * </BioBase>
     * 
     * Catch BioBaseException and Exception and log errors
     */
    private void _SetTouchStatus(BioBReturnCode DataStatus)
    {
      try
      {
        // Update TouchDisplayStandardExternalParameters with new _biobaseDevice.mostResentKey
        string Retry = "2"; //hidden
        string Confirm = "2"; //hidden
        switch (_biobaseDevice.mostResentKey)
        {
          case (ActiveKeys.KEYS_NONE):
            Retry = "2"; //hidden
            Confirm = "2"; //hidden
            break;
          case (ActiveKeys.KEYS_OK_CONTRAST):
            Retry = "2"; //hidden
            Confirm = "1"; //active  - enable AdjustAcquisitionProcess
            break;
          case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
            Retry = "1";  // button active  - enable RescanImage
            Confirm = "1";  // button active - enable accept image captured with warning
            break;
        }
        _biobaseDevice.TouchDisplayStandardExternalParameters["ButtonRetry"] = Retry;  // update button 
        _biobaseDevice.TouchDisplayStandardExternalParameters["ButtonConfirm"] = Confirm;  // update button 


        string status = "1";

        // check status 
        switch ((BioBReturnCode)DataStatus)
        {
          case BioBReturnCode.BIOB_SUCCESS:
            status = "1";
            break;

          case BioBReturnCode.BIOB_OPTICS_SURFACE_DIRTY:
            status = "3";
            break;

          case BioBReturnCode.BIOB_REPLACE_PAD:
            status = "9";
            break;

          case BioBReturnCode.BIOB_BAD_SCAN:
          case BioBReturnCode.BIOB_NO_CAPTURE_ACTIVE:
            status = "4";
            break;

          case BioBReturnCode.BIOB_AUTOCAPTURE_SEGMENTATION:
            status = "6";
            break;

          case BioBReturnCode.BIOB_NO_OBJECT:
          case BioBReturnCode.BIOB_SPOOF_DETECTED:
          case BioBReturnCode.BIOB_SPOOF_DETECTOR_FAIL:
            status = "2";
            break;

          case BioBReturnCode.BIOB_ROLL_SHIFTED_HORIZONTALLY:
          case BioBReturnCode.BIOB_ROLL_SHIFTED_VERTICALLY:
          case BioBReturnCode.BIOB_ROLL_LIFTED_TIP:
          case BioBReturnCode.BIOB_ROLL_ON_BORDER:
          case BioBReturnCode.BIOB_ROLL_PAUSED:
          case BioBReturnCode.BIOB_ROLL_FINGER_TOO_NARROW:
            status = "5";
            break;

          default:
            if( ( DataStatus & BioBReturnCode.BIOB_ROLL_SHIFTED_HORIZONTALLY ) == BioBReturnCode.BIOB_ROLL_SHIFTED_HORIZONTALLY ||
            ( DataStatus & BioBReturnCode.BIOB_ROLL_SHIFTED_VERTICALLY ) == BioBReturnCode.BIOB_ROLL_SHIFTED_VERTICALLY     ||
            ( DataStatus & BioBReturnCode.BIOB_ROLL_LIFTED_TIP ) == BioBReturnCode.BIOB_ROLL_LIFTED_TIP                     ||
            ( DataStatus & BioBReturnCode.BIOB_ROLL_ON_BORDER ) == BioBReturnCode.BIOB_ROLL_ON_BORDER                       ||
            ( DataStatus & BioBReturnCode.BIOB_ROLL_FINGER_TOO_NARROW ) == BioBReturnCode.BIOB_ROLL_FINGER_TOO_NARROW       ||
            (DataStatus & BioBReturnCode.BIOB_ROLL_PAUSED ) == BioBReturnCode.BIOB_ROLL_PAUSED                               )
            {
              status = "5";
            }
            else
            {
              status = "2";
            }
            break;
        }
        _biobaseDevice.TouchDisplayStandardExternalParameters["Result"] = status;   // Update status


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
        _AddUserOutputElement(xmlDoc, TemplateElem, PropertyConstants.OUT_DATA_TOUCHDISPLAY_URI, _biobaseDevice.TouchDisplayStandardTemplate);


        foreach (KeyValuePair<string, string> Ex in _biobaseDevice.TouchDisplayStandardExternalParameters)
        {
          XmlElement EpElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER);
          EpElem.SetAttribute(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER_KEY, Ex.Key);
          EpElem.SetAttribute(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER_VALUE, Ex.Value);
          TemplateElem.AppendChild(EpElem);
        }

        _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
      }
    }

    /*!
     * \fn private void _SetTouchGuidance()
     * \brief use position, impression and keys to format output for device's touch and LED display
     * global output:TouchDisplayInitialTemplate HTML template for initial prompt
     * global output:TouchDisplayInitialExternalParameters HTML template parameters for initial prompt
     * global output:TouchDisplayStandardTemplate HTML template for prompt after figners detected on the platen
     * global output:TouchDisplayStandardExternalParameters HTML template parameters for prompt after figners detected on the platen
     * global inputs: _biobaseDevice.mostRecentPosition and _biobaseDevice.mostRecentImpression
     * global inputs: _biobaseDevice.mostResentKey 
     * 
     * Note: "ButtonRetry", "ButtonConfirm", "Result" and "FPx" are defined in the Templates files
     * NOTE: When application adds support for finger annotation, this methods logic must add support for annotated fingers.
     * 
     * Typical XML output for flat capture of right thumb:
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     *  -<OutputData>
     *   -<TouchDisplay>
     *    -<DesignTemplate>
     *     <URI>file:///E:/temp/Templates/index_initial_thumbs.html</URI>
     *     <ExternalParameter Value="2" Key="ButtonRetry"/>
     *     <ExternalParameter Value="1" Key="ButtonConfirm"/>
     *     <ExternalParameter Value="1" Key="FP1"/>
     *     <ExternalParameter Value="0" Key="FP6"/>
     *    </DesignTemplate>
     *   </TouchDisplay>
     *  </OutputData>
     * </BioBase>
     * 
     * Catch BioBaseException and Exception and log errors
     */
    private void _SetTouchGuidance()
    {
      try
      {
        _biobaseDevice.TouchDisplayStandardTemplate = _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_standby.html";
        _biobaseDevice.TouchDisplayInitialExternalParameters.Clear();
        _biobaseDevice.TouchDisplayStandardExternalParameters.Clear();

        string Retry = "2"; //hidden
        string Confirm = "2"; //hidden
        switch (_biobaseDevice.mostResentKey)
        {
          case (ActiveKeys.KEYS_NONE):
            Retry = "2"; //hidden
            Confirm = "2"; //hidden
            break;
          case (ActiveKeys.KEYS_OK_CONTRAST):
            Retry = "2"; //hidden
            Confirm = "1"; //active  - enable AdjustAcquisitionProcess
            break;
          case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
            Retry = "1";  // button active  - enable RescanImage
            Confirm = "1";  // button active - enable accept image captured with warning
            break;
        }
        _biobaseDevice.TouchDisplayInitialExternalParameters.Add("ButtonRetry", Retry);  // button 
        _biobaseDevice.TouchDisplayInitialExternalParameters.Add("ButtonConfirm", Confirm);  // button 

        switch (_biobaseDevice.mostRecentImpression)
        {
          case (PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_FLAT):
            switch (_biobaseDevice.mostRecentPosition)
            {
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_THUMB):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_thumbs.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP1", "1");  // show right thumb
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP6", "0");  // hide left thumb

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_thumbs.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "1");  // show right index finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "0");  // hide right middle finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "0");  // hide right ring finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "0");  // hide right little finger

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_MIDDLE):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "0");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "1");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "0");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "0");

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "0");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "0");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "1");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "0");

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LITTLE):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "0");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "0");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "0");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "1");

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FOUR_FINGERS):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "1");  //show index
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "1");  //show middle
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "1");  //show ring
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "1");  //show little

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX_AND_MIDDLE):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "1");  //show index
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "1");  //show middle
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "0");  //hide ring
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "0");  //hide little

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING_AND_LITTLE):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "0");  //hide index
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "0");  //hide middle
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "1");  //show ring
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "1");  //show little

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_TWO_FINGERS):
                // Generic two fingers so in this application it will assume right and left index 
                //TODO: Fix  html for two index fingers. index_initial_four.html currently always displays 4 fingers.
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_four.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "1");  //show right index
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "0");  //hide right middle
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "1");  //show left index
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "0");  //hide left middle

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_four.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;


              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_THUMB):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_thumbs.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP1", "0");  // hide right thumb
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP6", "1");  // show left thumb

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_thumbs.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "1");  // show left index finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "0");  // hide left middle finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "0");  // hide left ring finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "0"); // hide left little finger

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_MIDDLE):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "0");  // hide left index finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "1");  // show left middle finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "0");  // hide left ring finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "0"); // hide left little finger

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "0");  // hide left index finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "0");  // hide left middle finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "1");  // show left ring finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "0"); // hide left little finger

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LITTLE):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "0");  // hide left index finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "0");  // hide left middle finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "0");  // hide left ring finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "1"); // show left little finger

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FOUR_FINGERS):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "1");  // show left index finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "1");  // show left middle finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "1");  // show left ring finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "1"); // show left little finger

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX_AND_MIDDLE):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "1");  // show left index finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "1");  // show left middle finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "0");  // hide left ring finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "0"); // hide left little finger

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING_AND_LITTLE):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "0");  // hide left index finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "0");  // hide left middle finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "1");  // show left ring finger
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "1"); // show left little finger

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS):
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_thumbs.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP1", "1");  // show right thumb
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP6", "1");  // show left thumb

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_thumbs.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_INDEXES_AND_MIDDLES):
                //PreTriggerMessage = "Place Flat 4: Left Middle + Left Index + Right Index + Right Middle!";
                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_four.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "1");  // show right thumb
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "1");  // show left thumb
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "1");  // show right thumb
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "1");  // show left thumb

                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_four.html";
                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                break;
            }
            break;


          case (PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL):
            _biobaseDevice.TouchDisplayInitialExternalParameters.Clear();
            _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_roll.html";
            _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_roll.html";
            switch (_biobaseDevice.mostRecentPosition)
            {
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_THUMB):
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP1", "1");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP1", "2");   // Prompt to Roll right thumb.... Green

                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                _biobaseDevice.TouchDisplayStandardExternalParameters["FP1"] = "2";     // Capturing Roll right thumb.... Yellow
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX):
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP1", "1");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "2");   // Prompt to Roll right index....

                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                _biobaseDevice.TouchDisplayStandardExternalParameters["FP2"] = "2";     // Capturing Roll index.... Yellow
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_MIDDLE):
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP1", "1");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "2");

                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                _biobaseDevice.TouchDisplayStandardExternalParameters["FP3"] = "2";
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING):
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP1", "1");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "2");

                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                _biobaseDevice.TouchDisplayStandardExternalParameters["FP4"] = "2";     // Capturing Roll - Yellow
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LITTLE):
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP1", "1");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "2");

                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                _biobaseDevice.TouchDisplayStandardExternalParameters["FP5"] = "2";     // Capturing Roll - Yellow
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_THUMB):
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP2", "1");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP6", "2");

                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                _biobaseDevice.TouchDisplayStandardExternalParameters["FP6"] = "2";     // Capturing Roll - Yellow
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX):
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP2", "1");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "2");

                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                _biobaseDevice.TouchDisplayStandardExternalParameters["FP7"] = "2";     // Capturing Roll - Yellow
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_MIDDLE):
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP2", "1");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "2");

                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                _biobaseDevice.TouchDisplayStandardExternalParameters["FP8"] = "2";     // Capturing Roll - Yellow
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING):
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP2", "1");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "2");

                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                _biobaseDevice.TouchDisplayStandardExternalParameters["FP9"] = "2";     // Capturing Roll - Yellow
                break;
              case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LITTLE):
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP2", "1");
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "2");

                //Optional example of how to update of the Touch Display to show other finger rolls are complete...
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP1", "3");  // mark red - error on right thumb
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "4");  // mark light blue - successful capture on right index
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "4");  // mark light blue - successful capture
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "4");  // mark light blue - successful capture
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "4");  // mark light blue - successful capture
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP6", "4");  // mark light blue - successful capture
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "4");  // mark light blue - successful capture
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "4");  // mark light blue - successful capture
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "0");  // mark light blue - successful capture

                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string,string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                _biobaseDevice.TouchDisplayStandardExternalParameters["FP10"] = "2";     // Capturing Roll - Yellow
                break;
            }
            break;
          case (PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL_VERTICAL):
            //TODO: Create and setup animated html templates for vertical roll
            break;
        }
  

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
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
      }
    }

    /*!
     * \fn private void _StopTouchUpdates()
     * \brief stops alll touch screen updated by sending null url
     * Catch BioBaseException and Exception and log errors
     */
    private void _StopTouchUpdates()
    {
      try
      {
        // Stop display on Guardian 300 - UserOutput with no URL....
        XmlDocument xmlDoc = new XmlDocument();
        _SetupXMLOutputHeader(xmlDoc);
        XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
        xmlDoc.DocumentElement.AppendChild(outputDataElem);
        XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY);
        outputDataElem.AppendChild(statusDataElem);
        _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
      }
    }

      
    #endregion

    /*!
     * \fn private void _SetImageText()
     * \brief place holder for applications that would use the Visualization Window() that has LSE draw to the image box.
     *  Becasue this application uses the preview event to draw to the image box, this is not implemented.
     *  this would send text to the Visualization Window() that is draw to the image box by LSE
     *  
     * //Typical visualization text overlay format:
     * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
     * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
     *  -<OutputData>
     *   -<VisualizationOverlay>
     *     <Text Value="Place fingers on platen." BelongsToImage="FALSE" FontSize="10" FontName="Arial" Color="0 0 255" PosX="10" PosY="10
     *    </VisualizationOverlay>
     *   </OutputData>
     *  </BioBase>
     *
     * Catch BioBaseException and Exception and log errors
     */
    private void _SetImageText(string msg)
    {
      try
      {
        // Display text on visualization window
        XmlDocument xmlDoc = new XmlDocument();
        _SetupXMLOutputHeader(xmlDoc);

        XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
        xmlDoc.DocumentElement.AppendChild(outputDataElem);

        XmlElement OverlayElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OVERLAY);
        outputDataElem.AppendChild(OverlayElem);

        XmlElement TextElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OVERLAY_TEXT);
        TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_POSY, "10");
        TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_POSX, "10");
        TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_COLOR, "0 0 255");
        TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_FONT_NAME, "Arial");
        TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_FONT_SIZE, "10");
        TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_BELONGS_TO_IMAGE, PropertyConstants.DEV_PROP_FALSE);
        TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_VALUE, msg);
        OverlayElem.AppendChild(TextElem);

        _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
      }
      catch (BioBaseException ex)
      {
        AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_OVERLAY_TEXT, ex.Message));
      }
      catch (Exception ex)
      {
        AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_OVERLAY_TEXT, ex.Message));
      }

    }

    /*!
     * \fn private void _SetupXMLOutputHeader()
     * \brief Format generic XML header for all SetOutputData 
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
     * \brief Add XML element for SetOutputData
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

  }
}