using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using System.IO.Ports;

namespace nspNECWireless
{

    public class classMsgFormat
    {
        public byte[] bytStart = new byte[2]; //2 start bytes of command: must be 0x0F - 0x5A (NEC wireless regulation)
        public byte bytMsgID; //Message ID - depend on what purpose of command, this will be different. (NEC wireless regulation)
                              //    bytMsgID = 0x11: Normall command sending
                              //    bytMsgID = 0x77: Reset command for wireless module
        public byte bytMsgNo { get; set; } //Message Number - this must be increase each time sending command
        public byte bytLength; //length - decide how many byte will send to destination (another wireless module)
        public byte[] bytDstID = new byte[4]; //The ID of destination - must setting correctly
        public byte[] bytSrcID = new byte[4]; //The ID of Source module - No need to specify if communication between 1 to 1 module (Example: can be setting to 0xFFFFFFFF)

        public byte[] bytParameter = new byte[100]; //The parameter series send to destination


        public List<byte> lstbyteParameter; //List of all Parameter will send to Destination

       

        //Constructor
        public classMsgFormat()
        {
            this.bytLength = 0;
            this.bytMsgNo = 0;
            this.lstbyteParameter = new List<byte>();
        }
    }

    ///
    /// Module property variable
    /// </summary>
    public class ModuleProperty
    {
        public byte bytChannel; //現在のチャンネル番号
        public byte bytPower; //現在の送信出力
        public byte bytRspBackoffCount; //ブロードキャスト送信を受信時に、応答前にEnergyDetectを実行する最大回数。
        public byte bytRspBackoffMin; //ブロードキャスト送信を受信時に、応答前にEnergyDetectを実行するまでの待ち時間の初期値。
        public byte bytRspBackoffMax; //ブロードキャスト送信を受信時に、応答前にEnergyDetectを実行するまでの待ち時間の最大値。
        public byte bytRspEnable; //デバイス検索要求を受信時に、応答するかどうか選択。（0:応答しない/1:応答する）
        public byte bytRetryCount; //デバイス検索要求を受信時に、応答するかどうか選択。（0:応答しない/1:応答する）
        public byte bytRetryWait; //再送までの待ち時間。
        public byte bytBackoffCount; //無線送信前にEnergyDetectを実行する最大回数。(0のときはEnergyDetectを実行しない。)
        public byte bytBackoffMin; //無線送信前にEnergyDetectを実行するまでの待ち時間の初期値。
        public byte bytBackoffMax; //無線送信前にEnergyDetectを実行するまでの待ち時間の最大値。
        public byte bytBaud;
        public long lngRcvTime; //スリープモードにおけるRF受信状態の期間。単位は1ms。
        public byte bytSleepTime; //スリープモードにおける省電力状態の期間。単位は1024ms。
        public byte bytReserved1;
        public byte bytReserved2;
        public byte bytCmdEnable; //コマンド送信を受信時に、応答するかどうかを選択。（0:応答しない/1:応答する）
        public byte bytEDThreshold; //データ送信前に実行したEnergyDetectの送信閾値。設定値以上の競合電波を検出した時、送信しない。
        public long lngSystemID; //システム固有のID。
        public long lngProductID; //製品のID
    }

    public class clsWirelessCom
    {
        [DllImport("Kernel32.dll")]
        public static extern long GetTickCount();
        //
        public classMsgFormat clsMsgFormat = new classMsgFormat();
        public ModuleProperty clsModuleProperty = new ModuleProperty(); 
        //
        public windowWirelessSetting WindowSettingNEC = new windowWirelessSetting(); //Window form for changing setting of Wireless Module
        public Views.wdNecCommand wdCommandTest = new Views.wdNecCommand();
        public System.Windows.Controls.MenuItem menuSetting { get; set; } //This menu item should add to main menu

        //
        public System.IO.Ports.SerialPort COMPort = new System.IO.Ports.SerialPort();

        public string strModuleSerial { get; set; }
        public const byte byteStart0 = 0x0F; //Fix 1st byte Start
        public const byte byteStart1 = 0x5A; //Fix 2nd byte Start

        public byte bytMsgNoCount = 0; //This should be increase each time sending- unique number
        public int intSequenceNo = 0; // 0x000 ~0x9999 (only 4 digit "0000" => "9999")   Sequence Number of command - The purpose is record to log (inside FRAM?) for investigate if necessary

        public List<byte> lstByteSendingData = new List<byte>(); //Contain all data which will be send to destination (Binary format)
        public List<byte> lstByteReceiveData = new List<byte>(); //Contain all return data received (31 bytes normally)

        public int intCheckSum; //Save Check sum result

        //
        public void IniFormSetting()
        {
            //this.WindowSettingNEC.btnTest.Click += btnTest_Click;
            this.WindowSettingNEC.Loaded += WindowSettingNEC_Loaded;
            this.WindowSettingNEC.btnApplySetting.Click += btnApplySetting_Click;
            this.WindowSettingNEC.btnGetInfo.Click += btnGetInfo_Click;
        }

        void btnGetInfo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //Display all current setting
            ClearDisplayModuleInfo();
            //System.Threading.Thread.Sleep(1000);
            bool boolTemp = this.cmdGetModuleProperty();   //call Getting Module Property Func
            if (boolTemp == true) //Reading OK
            {
                DisplayModuleInfo();
            }
        }

        void btnApplySetting_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            byte byteTemp = 0;
            //this.WindowSettingNEC.tbChannel.Text = this.clsModuleProperty.bytChannel.ToString("X");
            if(byte.TryParse(this.WindowSettingNEC.tbChannel.Text.Trim(),System.Globalization.NumberStyles.HexNumber, null, out byteTemp)==false)
            {
                MessageBox.Show("Channel setting is not hex number!","Setting Fail!");
            }
            this.clsModuleProperty.bytChannel = byteTemp;

            //this.WindowSettingNEC.tbPower.Text = this.clsModuleProperty.bytPower.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbPower.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("Power setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytPower = byteTemp;

            //this.WindowSettingNEC.tbRspBackoffCount.Text = this.clsModuleProperty.bytRspBackoffCount.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbRspBackoffCount.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("RspBackoffCount setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytRspBackoffCount = byteTemp;

            //this.WindowSettingNEC.tbRspBackOffMin.Text = this.clsModuleProperty.bytRspBackoffMin.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbRspBackOffMin.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("RspBackoffMin setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytRspBackoffMin = byteTemp;

            //this.WindowSettingNEC.tbRspBackOffMax.Text = this.clsModuleProperty.bytRspBackoffMax.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbRspBackOffMax.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("RspBackoffMax setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytRspBackoffMax = byteTemp;

            //this.WindowSettingNEC.tbRspEnable.Text = this.clsModuleProperty.bytRspEnable.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbRspEnable.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("RspEnable setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytRspEnable = byteTemp;

            //this.WindowSettingNEC.tbRetryCount.Text = this.clsModuleProperty.bytRetryCount.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbRetryCount.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("RetryCount setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytRetryCount = byteTemp;

            //this.WindowSettingNEC.tbRetryWait.Text = this.clsModuleProperty.bytRetryWait.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbRetryWait.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("RetryWait setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytRetryWait = byteTemp;
            
            //this.WindowSettingNEC.tbBackOffCount.Text = this.clsModuleProperty.bytBackoffCount.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbBackOffCount.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("BackoffCount setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytBackoffCount = byteTemp;

            //this.WindowSettingNEC.tbBackOffMax.Text = this.clsModuleProperty.bytBackoffMax.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbBackOffMax.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("BackoffMax setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytBackoffMax = byteTemp;

            //this.WindowSettingNEC.tbBackOffMin.Text = this.clsModuleProperty.bytBackoffMin.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbBackOffMin.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("BackoffMin setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytBackoffMin = byteTemp;

            //this.WindowSettingNEC.tbRcvTime.Text = this.clsModuleProperty.lngRcvTime.ToString("X");
            long lngTemp = 0;
            if (long.TryParse(this.WindowSettingNEC.tbRcvTime.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out lngTemp) == false)
            {
                MessageBox.Show("RcvTime setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.lngRcvTime = lngTemp;

            //this.WindowSettingNEC.tbSleepTime.Text = this.clsModuleProperty.bytSleepTime.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbSleepTime.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("SleepTime setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytSleepTime = byteTemp;

            //this.WindowSettingNEC.tbReserved1.Text = this.clsModuleProperty.bytReserved1.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbReserved1.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("Reserved1 setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytReserved1 = byteTemp;

            //this.WindowSettingNEC.tbReserved2.Text = this.clsModuleProperty.bytReserved2.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbReserved2.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("Reserved2 setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytReserved2 = byteTemp;

            //this.WindowSettingNEC.tbCmdEnable.Text = this.clsModuleProperty.bytCmdEnable.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbCmdEnable.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("CmdEnable setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytCmdEnable = byteTemp;

            //this.WindowSettingNEC.tbEDThreshold.Text = this.clsModuleProperty.bytEDThreshold.ToString("X");
            if (byte.TryParse(this.WindowSettingNEC.tbEDThreshold.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out byteTemp) == false)
            {
                MessageBox.Show("EDThreshold setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.bytEDThreshold = byteTemp;

            //this.WindowSettingNEC.tbSystemId.Text = this.clsModuleProperty.lngSystemID.ToString("X");
            if (long.TryParse(this.WindowSettingNEC.tbSystemId.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out lngTemp) == false)
            {
                MessageBox.Show("SystemID setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.lngSystemID = lngTemp;

            //this.WindowSettingNEC.tbProductId.Text = this.clsModuleProperty.lngProductID.ToString("X");
            if (long.TryParse(this.WindowSettingNEC.tbProductId.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out lngTemp) == false)
            {
                MessageBox.Show("ProductID setting is not hex number!", "Setting Fail!");
            }
            this.clsModuleProperty.lngProductID = lngTemp;

            //Apply Setting
            bool blTemp = this.w_SetModuleProperty();
            if(blTemp==true)
            {
                MessageBox.Show("New Setting Apply OK!", "btnApplySetting_Click");
            }
            else
            {
                MessageBox.Show("Cannot Apply New Setting!", "btnApplySetting_Click");
            }

        }

        void WindowSettingNEC_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //Display all current setting
            ClearDisplayModuleInfo();
            bool boolTemp = this.cmdGetModuleProperty();   //call Getting Module Property Func
            if(boolTemp==true) //Reading OK
            {
                DisplayModuleInfo();
            }
        }

        void DisplayModuleInfo()
        {
            this.WindowSettingNEC.tbChannel.Text = this.clsModuleProperty.bytChannel.ToString("X");
            this.WindowSettingNEC.tbPower.Text = this.clsModuleProperty.bytPower.ToString("X");
            this.WindowSettingNEC.tbRspBackoffCount.Text = this.clsModuleProperty.bytRspBackoffCount.ToString("X");
            this.WindowSettingNEC.tbRspBackOffMin.Text = this.clsModuleProperty.bytRspBackoffMin.ToString("X");
            this.WindowSettingNEC.tbRspBackOffMax.Text = this.clsModuleProperty.bytRspBackoffMax.ToString("X");
            this.WindowSettingNEC.tbRspEnable.Text = this.clsModuleProperty.bytRspEnable.ToString("X");
            this.WindowSettingNEC.tbRetryCount.Text = this.clsModuleProperty.bytRetryCount.ToString("X");
            this.WindowSettingNEC.tbRetryWait.Text = this.clsModuleProperty.bytRetryWait.ToString("X");
            this.WindowSettingNEC.tbBackOffCount.Text = this.clsModuleProperty.bytBackoffCount.ToString("X");
            this.WindowSettingNEC.tbBackOffMax.Text = this.clsModuleProperty.bytBackoffMax.ToString("X");
            this.WindowSettingNEC.tbBackOffMin.Text = this.clsModuleProperty.bytBackoffMin.ToString("X");
            this.WindowSettingNEC.tbRcvTime.Text = this.clsModuleProperty.lngRcvTime.ToString("X");
            this.WindowSettingNEC.tbSleepTime.Text = this.clsModuleProperty.bytSleepTime.ToString("X");
            this.WindowSettingNEC.tbReserved1.Text = this.clsModuleProperty.bytReserved1.ToString("X");
            this.WindowSettingNEC.tbReserved2.Text = this.clsModuleProperty.bytReserved2.ToString("X");
            this.WindowSettingNEC.tbCmdEnable.Text = this.clsModuleProperty.bytCmdEnable.ToString("X");
            this.WindowSettingNEC.tbEDThreshold.Text = this.clsModuleProperty.bytEDThreshold.ToString("X");
            this.WindowSettingNEC.tbSystemId.Text = this.clsModuleProperty.lngSystemID.ToString("X");
            this.WindowSettingNEC.tbProductId.Text = this.clsModuleProperty.lngProductID.ToString("X");
        }

        void ClearDisplayModuleInfo()
        {
            this.WindowSettingNEC.tbChannel.Text = "";
            this.WindowSettingNEC.tbPower.Text = "";
            this.WindowSettingNEC.tbRspBackoffCount.Text = "";
            this.WindowSettingNEC.tbRspBackOffMin.Text = "";
            this.WindowSettingNEC.tbRspBackOffMax.Text = "";
            this.WindowSettingNEC.tbRspEnable.Text = "";
            this.WindowSettingNEC.tbRetryCount.Text = "";
            this.WindowSettingNEC.tbRetryWait.Text = "";
            this.WindowSettingNEC.tbBackOffCount.Text = "";
            this.WindowSettingNEC.tbBackOffMax.Text = "";
            this.WindowSettingNEC.tbBackOffMin.Text = "";
            this.WindowSettingNEC.tbRcvTime.Text = "";
            this.WindowSettingNEC.tbSleepTime.Text = "";
            this.WindowSettingNEC.tbReserved1.Text = "";
            this.WindowSettingNEC.tbReserved2.Text = "";
            this.WindowSettingNEC.tbCmdEnable.Text = "";
            this.WindowSettingNEC.tbEDThreshold.Text = "";
            this.WindowSettingNEC.tbSystemId.Text = "";
            this.WindowSettingNEC.tbProductId.Text = "";
        }

        public Boolean cmdGetModuleProperty()
        {
            //Special command to get module properties
            clsMsgFormat.bytMsgID = 0x29;
            clsMsgFormat.bytMsgNo = 0x00;

            Array.Resize(ref clsMsgFormat.bytParameter, 0); //Need to reset here because this is special command => there is no parameter

            string strTemp = "";

            strTemp = this.NECcomTxBinary("FFFFFFFF"); //Special command - follow NEC wireless module regulation
            if (strTemp != "0") return false;

            strTemp = this.NECcomRxBirary(2000);

            if (strTemp == "0") //Receive OK
            {
                switch (this.lstByteReceiveData[3])
                {

                    case 0x00:
                        //Note that first 13 byte is contain message format 
                        this.clsModuleProperty.bytChannel = this.lstByteReceiveData[13];
                        this.clsModuleProperty.bytPower = this.lstByteReceiveData[14];
                        this.clsModuleProperty.bytRspBackoffCount = this.lstByteReceiveData[15];
                        this.clsModuleProperty.bytRspBackoffMin = this.lstByteReceiveData[16];
                        this.clsModuleProperty.bytRspBackoffMax = this.lstByteReceiveData[17];
                        this.clsModuleProperty.bytRspEnable = this.lstByteReceiveData[18];
                        this.clsModuleProperty.bytRetryCount = this.lstByteReceiveData[19];
                        this.clsModuleProperty.bytRetryWait = this.lstByteReceiveData[20];
                        this.clsModuleProperty.bytBackoffCount = this.lstByteReceiveData[21];
                        this.clsModuleProperty.bytBackoffMin = this.lstByteReceiveData[22];
                        this.clsModuleProperty.bytBackoffMax = this.lstByteReceiveData[23];
                        this.clsModuleProperty.lngRcvTime = Convert.ToInt64(this.lstByteReceiveData[24] * 256 + this.lstByteReceiveData[25]);
                        this.clsModuleProperty.bytSleepTime = this.lstByteReceiveData[26];
                        this.clsModuleProperty.bytReserved1 = this.lstByteReceiveData[27];
                        this.clsModuleProperty.bytReserved2 = this.lstByteReceiveData[28];
                        this.clsModuleProperty.bytCmdEnable = this.lstByteReceiveData[29];
                        this.clsModuleProperty.bytEDThreshold = this.lstByteReceiveData[30];
                        this.clsModuleProperty.lngSystemID = Convert.ToInt64(this.lstByteReceiveData[31] * 256 + this.lstByteReceiveData[32]);
                        this.clsModuleProperty.lngProductID = Convert.ToInt64(this.lstByteReceiveData[33] * 256 + this.lstByteReceiveData[34]);

                        return true;

                    case 0x01:
                        return false;
                }
            }
            return false;
        }

        public bool w_SetModuleProperty()
        {
            bool blRet = false;
            //if (MsgFormat.bytMsgID!=0x7E)
            //{
            clsMsgFormat.bytMsgID = 0x7E;
            clsMsgFormat.bytMsgNo = 0x00;
            //}
            Array.Resize(ref clsMsgFormat.bytParameter, 23);

            clsMsgFormat.bytParameter[0] = this.clsModuleProperty.bytChannel;
            clsMsgFormat.bytParameter[1] = this.clsModuleProperty.bytPower;
            clsMsgFormat.bytParameter[2] = this.clsModuleProperty.bytBackoffCount;
            clsMsgFormat.bytParameter[3] = this.clsModuleProperty.bytBackoffMin;
            clsMsgFormat.bytParameter[4] = this.clsModuleProperty.bytBackoffMax;
            clsMsgFormat.bytParameter[5] = this.clsModuleProperty.bytRspEnable;
            clsMsgFormat.bytParameter[6] = this.clsModuleProperty.bytRetryCount;
            clsMsgFormat.bytParameter[7] = this.clsModuleProperty.bytRetryWait;
            clsMsgFormat.bytParameter[8] = this.clsModuleProperty.bytBackoffCount;
            clsMsgFormat.bytParameter[9] = this.clsModuleProperty.bytBackoffMin;
            clsMsgFormat.bytParameter[10] = this.clsModuleProperty.bytBackoffMax;
            clsMsgFormat.bytParameter[11] = this.clsModuleProperty.bytBaud;
            clsMsgFormat.bytParameter[12] = (byte)(((Int32)this.clsModuleProperty.lngRcvTime & 0xFF00)/0x100);
            clsMsgFormat.bytParameter[13] = (byte)((Int32)this.clsModuleProperty.lngRcvTime & 0xFF);
            clsMsgFormat.bytParameter[14] = this.clsModuleProperty.bytSleepTime;
            clsMsgFormat.bytParameter[15] = this.clsModuleProperty.bytReserved1;
            clsMsgFormat.bytParameter[16] = this.clsModuleProperty.bytReserved2;
            clsMsgFormat.bytParameter[17] = this.clsModuleProperty.bytCmdEnable;
            clsMsgFormat.bytParameter[18] = this.clsModuleProperty.bytEDThreshold;
            clsMsgFormat.bytParameter[19] = (byte)(((Int32)this.clsModuleProperty.lngSystemID & 0xFF00) / 0x100);
            clsMsgFormat.bytParameter[20] = (byte)((Int32)this.clsModuleProperty.lngSystemID & 0xFF);
            clsMsgFormat.bytParameter[21] = (byte)(((Int32)this.clsModuleProperty.lngProductID & 0xFF00) / 0x100);
            clsMsgFormat.bytParameter[22] = (byte)((Int32)this.clsModuleProperty.lngProductID & 0xFF);

            string strTemp = "";
            strTemp = this.NECcomTxBinary("FFFFFFFF");
            if (strTemp != "0") return false;

            strTemp = this.NECcomRxBirary(2000);

            if (strTemp == "0")
            {
                switch (this.lstByteReceiveData[3])
                {
                    case 0x00:
                        blRet = true;
                        break;
                    case 0x01:
                        blRet = false;
                        break;
                    default:
                        blRet = false;
                        break;
                }
            }

            return blRet;
        }

        public string NECExecuteReset()
        {
            string strRet = "";
            
            //Special command to reset
            clsMsgFormat.bytMsgID = 0x77;
            clsMsgFormat.bytMsgNo = 0x00;

            Array.Resize(ref clsMsgFormat.bytParameter, 5);
            //NEC wireless module regulation
            clsMsgFormat.bytParameter[0] = 0x24;
            clsMsgFormat.bytParameter[1] = 0x72;
            clsMsgFormat.bytParameter[2] = 0x73;
            clsMsgFormat.bytParameter[3] = 0x74;
            clsMsgFormat.bytParameter[4] = 0x24;

            strRet = this.NECcomTxBinary("FFFFFFFF"); //Special command - follow NEC wireless module regulation
            if (strRet != "0") return strRet;

            strRet = this.NECcomRxBirary(2000);
            if (strRet != "0") return strRet;

            switch (this.lstByteReceiveData[3])
            {
                case 0x00:
                    break;
                case 0x01:
                    return "NECExecuteReset(): Reset fail";
                default:
                    return "NECExecuteReset(): Reset fail";
            }

            //
            return strRet;
        }

        //
        public string NECBCommandGetADhandle(int intDataSize, out List<byte> lstbyteReceive)
        {
            string strRet = "";
            //
            int i,j = 0;
            int intGettingTimes = 0;
            lstbyteReceive = new List<byte>();
            //List<byte> lstbyteFullData = new List<byte>();

            if((intDataSize%59)==0)
            {
                intGettingTimes = (int)(intDataSize / 59);
            }
            else
            {
                intGettingTimes = (int)(intDataSize / 59) + 1;
            }

            //Start Getting data
            for (i = 0; i < intGettingTimes;i++)
            {
                strRet = this.NECcomRxBirary(2000,false);
                if(strRet=="0") //OK
                {
                    for(j=13;j<this.lstByteReceiveData.Count-1;j++) //0-12: default byte of NEC command format. Last byte is check sum byte => need to reject!
                    {
                        lstbyteReceive.Add(this.lstByteReceiveData[j]);
                    }
                }
                else //NG
                {
                    //string strtemp = "";
                    //string strtemp = this.NECExecuteReset();
                    break; //Just exit for retry
                }

                //System.Threading.Thread.Sleep(20);

            }

            //Checking if Number received data is same as Data size input
            if (strRet=="0") //OK?
            {
                if (intDataSize != lstbyteReceive.Count)
                {
                    strRet = "NECBCommandGetADhandle() error: number of received bytes [" + lstbyteReceive.Count.ToString() + "] not same as requested [" + intDataSize.ToString() + "]";
                }
            }
            
            return strRet;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string NECcomTxBinary(string strModuleId)
        {
            //System.Threading.Thread.Sleep(50);
            //Increase Message byte count Number for next sending
            this.clsMsgFormat.bytMsgNo = this.bytMsgNoCount;
            if (this.bytMsgNoCount == 0xFF)
            {
                this.bytMsgNoCount = 0x00;
            }
            else
            {
                this.bytMsgNoCount = Convert.ToByte(this.bytMsgNoCount + 0x01);
            }

            //
            string strRet = "0";
            //////////////////////////////////////////////
            byte DtsID;

            clsMsgFormat.bytStart[0] = byteStart0;
            clsMsgFormat.bytStart[1] = byteStart1;

            int i = 0;
            for (int j = 1; j <= 7; j = j + 2)
            {
                byte.TryParse(Strings.Mid(strModuleId, j, 2), System.Globalization.NumberStyles.HexNumber, null, out DtsID);
                clsMsgFormat.bytDstID[i] = DtsID;
                i = i + 1;
            }

            clsMsgFormat.bytSrcID[0] = 0xff;
            clsMsgFormat.bytSrcID[1] = 0xff;
            clsMsgFormat.bytSrcID[2] = 0xff;
            clsMsgFormat.bytSrcID[3] = 0xff;

            clsMsgFormat.bytLength = (byte)(13 + (clsMsgFormat.bytParameter.Length)); //Adding more 13 Fisrt byte is Fix - Following NEC wireless module regulation

            this.lstByteSendingData = new List<byte>();
            //13 Fisrt byte is Fix - Following NEC wireless module regulation

            this.lstByteSendingData.Add(this.clsMsgFormat.bytStart[0]);
            this.lstByteSendingData.Add(this.clsMsgFormat.bytStart[1]);
            this.lstByteSendingData.Add(this.clsMsgFormat.bytLength);
            this.lstByteSendingData.Add(this.clsMsgFormat.bytMsgID);
            this.lstByteSendingData.Add(this.clsMsgFormat.bytMsgNo);
            this.lstByteSendingData.Add(this.clsMsgFormat.bytDstID[0]);
            this.lstByteSendingData.Add(this.clsMsgFormat.bytDstID[1]);
            this.lstByteSendingData.Add(this.clsMsgFormat.bytDstID[2]);
            this.lstByteSendingData.Add(this.clsMsgFormat.bytDstID[3]);
            this.lstByteSendingData.Add(this.clsMsgFormat.bytSrcID[0]);
            this.lstByteSendingData.Add(this.clsMsgFormat.bytSrcID[1]);
            this.lstByteSendingData.Add(this.clsMsgFormat.bytSrcID[2]);
            this.lstByteSendingData.Add(this.clsMsgFormat.bytSrcID[3]);

            for (i = 14; i <= clsMsgFormat.bytLength; i++) //13 Fisrt byte is Fix - Following NEC wireless module regulation
            {
                this.lstByteSendingData.Add(clsMsgFormat.bytParameter[i - 14]);
            }

            //Check Com port condition
            if (this.COMPort.IsOpen == false)
            {
                this.COMPort.Open();
            }

            this.COMPort.Write(this.lstByteSendingData.ToArray(), 0, (int)(clsMsgFormat.bytLength));

            //////////////////////////////////////////////
            return strRet;
        }

        //Reading data from Com port buffer
        public string NECcomRxBirary(long lgWaitTime = 2000, bool blConfirmDataRet = true)
        {
            //IMPORTANT NOTES:
            //  Yako Wireless communication with version 0.15 sequences:
            //      - PC send command for DH 
            //      - MPU firmware will return data with following format
            //          + Length = 31 bytes
            //          + 15 first byte: 0x0F-0x5A-confirm byte (0x00 if OK)-length (0x0F) - byte1-byte2-byte3-byte4-byte5-byte6-byte7-byte8-byte9-byte10-byte11
            //          + 16 next byte:  0x0F-0x5A-confirm byte (0x11 if OK)-length (0x10) - byte1-byte2-byte3-byte4-byte5-byte6-byte7-byte8-byte9-byte10-byte11-byte12

            string strRet = "0";
            byte byteTemp = 0;
            byte byteConfirm = 0;
            byte bytCheckSum = 0;
            byte byte1stLength = 0;
            byte byte2ndLength = 0;
            int i = 0;
            //
            //int i = 0;
            this.lstByteReceiveData = new List<byte>();

            ////Clear all buffer of Com port
            if (this.COMPort.IsOpen == false)
            {
                this.COMPort.Open();
            }

            int intTest = this.COMPort.BytesToRead;


            //Marking tick to check time out
            long lng1stTime = GetTickCount();

            //Polling until buffer have at less 4 bytes
            do
            {
                if (GetTickCount() - lng1stTime > lgWaitTime)
                {
                    return "NECcomRxBirary() error: Time out. There is not enough data comming.";
                }
            }
            while (this.COMPort.BytesToRead < 4);

            //Reading all 4 bytes
            for (i = 0; i < 4;i++)
            {
                if(this.COMPort.BytesToRead > 0)
                {
                    byteTemp = Convert.ToByte(this.COMPort.ReadByte());
                    this.lstByteReceiveData.Add(byteTemp);
                }
            }

            //
            if (this.lstByteReceiveData.Count < 4) return "NECcomRxBirary() error: not receive enough 4 bytes";

            //Check byte start
            if (this.lstByteReceiveData[0] != byteStart0) return "NECcomRxBirary() error: receive byteStart0 not correct";
            if (this.lstByteReceiveData[1] != byteStart1) return "NECcomRxBirary() error: receive byteStart1 not correct";

            //Check length
            byte1stLength = this.lstByteReceiveData[2];


            //Getting all data of 1st data
            do
            {
                if (GetTickCount() - lng1stTime > lgWaitTime)
                {
                    return "NECcomRxBirary() error: Time out. Cannot receive " + byte1stLength.ToString() + " bytes.";
                    //break;
                }
                //Reading from buffer
                if (this.COMPort.BytesToRead > 0)
                {
                    byteTemp = Convert.ToByte(this.COMPort.ReadByte());
                    this.lstByteReceiveData.Add(byteTemp);
                }
                //Check receive enough data from 1st data serial
                if (this.lstByteReceiveData.Count == byte1stLength) //Receive enough return bytes 2nd step
                {
                    break;
                }
            }
            while (true);

            //Check byte confirm
            byteConfirm = this.lstByteReceiveData[3];

            //Check byte confirm
            if(byteConfirm == 0x01) return "NECcomRxBirary() 1st error: ComRxBinary Send Format Mistake [3rd byte = 0x01]";
            if(byteConfirm == 0x12) return "NECcomRxBirary() 2nd error: Wireless Communication Error [3rd byte = 0x12]";


            if (blConfirmDataRet==true) //Some command & some case do not request to confirm further
            {
                //Checkinf in normal sending data
                if ((byteConfirm != 0x11) && (this.clsMsgFormat.bytMsgID == 0x11)) //Not yet Ok. We need reading more data
                {
                    //Polling until buffer have at less more 4 bytes
                    do
                    {
                        long lng2ndTime = GetTickCount();
                        if (lng2ndTime - lng1stTime > lgWaitTime)
                        {
                            return "NECcomRxBirary() 2nd error: Time out. There is not enough data comming.";
                        }
                    }
                    while (this.COMPort.BytesToRead < 4);

                    //Reading all 4 bytes
                    for (i = 0; i < 4; i++)
                    {
                        if (this.COMPort.BytesToRead > 0)
                        {
                            byteTemp = Convert.ToByte(this.COMPort.ReadByte());
                            this.lstByteReceiveData.Add(byteTemp);
                        }
                    }

                    //Check legal byte number
                    if (this.lstByteReceiveData.Count < (byte1stLength + 4)) return "NECcomRxBirary() error: 2nd receive bytes less than 4";
                    //Check byte start
                    if (this.lstByteReceiveData[byte1stLength + 0] != byteStart0) return "NECcomRxBirary() error: 2nd receive byteStart0 not correct";
                    if (this.lstByteReceiveData[byte1stLength + 1] != byteStart1) return "NECcomRxBirary() error: 2nd receive byteStart1 not correct";

                    //Check length of 2nd data series
                    byte2ndLength = this.lstByteReceiveData[byte1stLength + 2];

                    //Getting all data from 2nd series
                    do
                    {
                        long lng2ndTime = GetTickCount();
                        if (lng2ndTime - lng1stTime > lgWaitTime)
                        {
                            return "NECcomRxBirary() error: Time out. Cannot receive " + (byte1stLength + byte2ndLength).ToString() + " bytes.";
                            //break;
                        }
                        //Reading from buffer
                        if (this.COMPort.BytesToRead > 0)
                        {
                            byteTemp = Convert.ToByte(this.COMPort.ReadByte());
                            this.lstByteReceiveData.Add(byteTemp);
                        }
                        //Check receive enough data from 1st data serial
                        if (this.lstByteReceiveData.Count == (byte1stLength + byte2ndLength)) //Receive enough return bytes 2nd step
                        {
                            break;
                        }
                    }
                    while (true);
                }

                //Analyze data in normal data sending (Not get or set module properties)
                if (this.clsMsgFormat.bytMsgID == 0x11) //Normal command sending
                {
                    bool bl2ndDataExist = false;
                    if (byte1stLength < this.lstByteReceiveData.Count)
                    {
                        bl2ndDataExist = true;
                    }

                    if (bl2ndDataExist == true)
                    {
                        #region _Check2ndData

                        //Check length
                        byteConfirm = this.lstByteReceiveData[byte1stLength + 3];

                        if (byteConfirm != 0x11) //NG
                        {
                            switch (byteConfirm)
                            {
                                case 0x01: //Send Format mistake
                                    return "NECcomRxBirary() 2nd error: ComRxBinary Send Format Mistake [3rd byte = 0x01]";
                                case 0x12: //Wireless Communication Error
                                    return "NECcomRxBirary() 2nd error: Wireless Communication Error [3rd byte = 0x12]";
                                default:
                                    return "NECcomRxBirary() 2nd error: unkown. ByteConfirm = [" + byteConfirm.ToString() + "]";
                            }
                        }

                        //If Confirm byte is OK (0x11). Then check sum
                        intCheckSum = 0;

                        for (i = (13 + byte1stLength); i <= (byte1stLength + byte2ndLength - 1 - 1); i++)
                        {
                            if (i > (this.lstByteReceiveData.Count - 1))
                            {
                                return "NECcomRxBirary() 2nd error: Check sum NG. Byte check sum not received.";
                            }
                            //
                            intCheckSum = intCheckSum + (int)this.lstByteReceiveData[i];
                        }
                        bytCheckSum = (byte)((int)(0xFF) & intCheckSum);
                        if (bytCheckSum != this.lstByteReceiveData[byte1stLength + byte2ndLength - 1]) //last byte is for check sum?
                        {
                            //data Receive NG
                            return "NECcomRxBirary() 2nd error: Check sum NG. bytCheckSum =[" + bytCheckSum.ToString() + "]. Last byte = [" + this.lstByteReceiveData[byte1stLength + byte2ndLength - 1].ToString() + "]";
                        }
                        #endregion
                    }
                    else //Only exist 1st data return
                    {
                        #region _Check1stData
                        //Check byte confirm
                        byteConfirm = this.lstByteReceiveData[3];

                        if (byteConfirm != 0x11) //NG
                        {
                            switch (byteConfirm)
                            {
                                case 0x01: //Send Format mistake
                                    return "NECcomRxBirary() error: ComRxBinary Send Format Mistake [3rd byte = 0x01]";

                                case 0x12: //Wireless Communication Error
                                    return "NECcomRxBirary() error: Wireless Communication Error [3rd byte = 0x12]";
                                default:
                                    return "NECcomRxBirary() error: unkown. ByteConfirm = [" + byteConfirm.ToString() + "]";
                            }
                        }

                        //If Confirm byte is OK (0x11). Then check sum
                        intCheckSum = 0;

                        for (i = 13; i <= (byte1stLength - 1 - 1); i++)
                        {
                            if (i > (this.lstByteReceiveData.Count - 1))
                            {
                                return "NECcomRxBirary() error: Check sum NG. Byte check sum not received.";
                            }
                            //
                            intCheckSum = intCheckSum + (int)this.lstByteReceiveData[i];
                        }
                        bytCheckSum = (byte)((int)(0xFF) & intCheckSum);
                        if (bytCheckSum != this.lstByteReceiveData[byte1stLength - 1]) //last byte is for check sum?
                        {
                            //data Receive NG
                            return "NECcomRxBirary() error: Check sum NG. bytCheckSum =[" + bytCheckSum.ToString() + "]. Last byte = [" + this.lstByteReceiveData[byte1stLength - 1].ToString() + "]";
                        }
                        #endregion
                    }
                }
            }

            //Return OK code "0" if everything is OK
            return strRet;
        }

        //
        public string NECSendDataAndCheck(string strModuleId, string strCommand, long lngTimeOut = 1000,long lngRcvTimeOut = 1000, bool blConfirmDataRet = true, bool blAutoSequenceNo = true)
        {
            string strRet = "0";
            string strTemp = "";
            byte btCheckSum = 0;
            this.intCheckSum = 0;
            int i = 0;
            int intFailCount = 0;

            //Do retry polling 
            bool blResult = false;
            

            //
            this.clsMsgFormat.bytMsgID = 0x11; //This one will appear in return list data

            //Remove all space " " character if exist
            strCommand = strCommand.Replace(" ", "");

            //Auto adding Sequence number
            if(blAutoSequenceNo==true)
            {
                //We need add Sequence Number to Command string
                // Example "F01,1,2,3,4" => "F01,SequenceNo,1,2,3,4"
                // SequenceNo = "0000" ~ "9999" Note that this must be Deimal format, not Hexa format!!!

                if(this.intSequenceNo>9999) //need to reset?
                {
                    this.intSequenceNo = 0;
                }

                //Cal string Sequence
                string strSequenceNo = "";
                strSequenceNo = this.intSequenceNo.ToString();
                for(i=strSequenceNo.Length;i<4;i++)
                {
                    strSequenceNo = "0" + strSequenceNo;
                }

                //Finding first "," character
                int int1stCommaPos = strCommand.IndexOf(',');

                if(int1stCommaPos==-1) //Not contains any comma
                {
                    strCommand = strCommand + "," + strSequenceNo;
                }
                else
                {
                    strCommand = strCommand.Insert(int1stCommaPos + 1, strSequenceNo + ",");
                }

                //Auto increase sequence number for next sending
                this.intSequenceNo++;
            }

            //Cal Length
            int intLength = 0;
            intLength = strCommand.Length + 1; //Add 1 more byte at Ending for "Check Sum" byte
            if (intLength >= 60)
            {
                //MessageBox.Show("Error: String Size Over!");
                return "NECSendDataAndCheck() Error: Data length Over limit 60!";
            }

           

            //
            Array.Resize(ref clsMsgFormat.bytParameter, intLength);

            for (i = 0; i <= (intLength - 1 - 1); i++)
            {
                clsMsgFormat.bytParameter[i] = (byte)(Strings.Asc(Strings.Mid(strCommand, i + 1, 1)));
                this.intCheckSum = this.intCheckSum + clsMsgFormat.bytParameter[i];
            }
            btCheckSum = (byte)((int)(0xFF) & this.intCheckSum);
            clsMsgFormat.bytParameter[intLength - 1] = btCheckSum;


            long lngStartTick = GetTickCount();

            while (blResult==false)
            {
                if ((GetTickCount() - lngStartTick) > lngTimeOut) //Time out
                {
                    return "NECSendDataAndCheck() time out. Return value: " + strTemp;
                }

                //
                if (this.COMPort.IsOpen == false)
                {
                    this.COMPort.Open();
                }


                ////
                //if (intFailCount > 1000) intFailCount = 0; //Reset
                //if (intFailCount>3) //Try to reset wireless module
                //{
                //    //Delay a little
                //    System.Threading.Thread.Sleep(20);
                //    //
                //    strTemp = this.NECExecuteReset();
                //    //Delay a little
                //    System.Threading.Thread.Sleep(20);
                //}

                //Clear all comport buffer before new sending
                this.COMPort.DiscardOutBuffer();
                this.COMPort.DiscardInBuffer();

                //Sending command to Com Port
                strTemp = NECcomTxBinary(strModuleId);
                if (strTemp != "0") //Error happen
                {
                    blResult = false;
                    intFailCount++;
                }
                else //Sending OK
                {
                    //Receiving data from Com port
                    strTemp = NECcomRxBirary(lngRcvTimeOut, blConfirmDataRet);
                    if (strTemp != "0") //Error happen
                    {
                        blResult = false;
                        intFailCount++;
                    }
                    else //Result OK
                    {
                        blResult = true;
                    }
                }
            }
            //If everything is Ok. Return OK code: 0
            return strRet;
        }

        //Write command & receive data return
        public string NECWR(string strCommand, string strModuleSerial, long lngTimeOut = 2000, long lngRcvTimeOut = 1000, bool blConfirmDataRet = true, bool blAutoSequenceNo = true)
        {
            //Write command through RS232 COM port & Get return data
            string strRet = "0";

            //Cal Module ID
            string strModuleID = "";
            strRet = this.CalModuleId(strModuleSerial, out strModuleID);
            if (strRet != "0") return strRet; //If error return error message

            //Send & Receive
            strRet = this.NECSendDataAndCheck(strModuleID, strCommand, lngTimeOut, lngRcvTimeOut, blConfirmDataRet, blAutoSequenceNo);

            //
            return strRet;
        }

        public string CalModuleId(string strModuleSerial, out string strModuleID)
        {
            //Calculate Module ID from Module Serial
            //Module Serial: SNEC-0A-0050E
            //=> Module ID: EC4005E  (Note that "4" is inserted between "EC" & "0050E")
            string strRet = "0";
            strModuleID = "";
            //Check length of Module serial input: must greater than 13
            strModuleSerial = strModuleSerial.Trim();
            if (strModuleSerial.Length < 13) return "CalModuleId() Error: The length of Module serial [" + strModuleSerial + "] is less than 13";
            //Cal module ID
            string strLotNumber = strModuleSerial.Substring(2, 2);
            string strSerial = strModuleSerial.Substring(8, 5);
            strModuleID = strLotNumber + "4" + strSerial;

            //
            return strRet;
        }

        
        /// <summary>
        /// CalActualVolt - Functions to support VE Calibration
        /// </summary>
        /// <param name="objCommand">This command send to MPU to get AD value - F02?</param> 
        /// <param name="objHiBytePos">This is position of high byte return (count from 0)</param>
        /// <param name="objLoBytePos">This is position of Low byte return (count from 0)</param>
        /// <param name="objVref">This is V reference of sensor - 5V but in the future it can be 3.3V</param>
        /// <param name="objResolution">Resolution of AD conversion - normally is 10bits - 1024</param>
        /// <returns></returns>
        public object CalActualVolt(object objCommand, object objHiBytePos, object objLoBytePos, object objVref, object objResolution)
        {
            object objRet = 0;
            //
            string strCommand = objCommand.ToString().Trim();
            //
            int intHiBytePos = 0;
            if(int.TryParse(objHiBytePos.ToString().Trim(), out intHiBytePos)==false)
            {
                return "CalActualVolt error: The high byte input [" + objHiBytePos.ToString() + "] is not integer!";
            }
            //
            int intLowBytePos = 0;
            if (int.TryParse(objLoBytePos.ToString().Trim(), out intLowBytePos) == false)
            {
                return "CalActualVolt error: The Low byte input [" + objLoBytePos.ToString() + "] is not integer!";
            }
            //
            double dblVref = 0;
            if(double.TryParse(objVref.ToString().Trim(), out dblVref)==false)
            {
                return "CalActualVolt error: The Vref input [" + objVref.ToString() + "] is not numeric!";
            }
            //
            int intResolution = 0;
            if (int.TryParse(objResolution.ToString().Trim(), out intResolution) == false)
            {
                return "CalActualVolt error: The Resolution input [" + objResolution.ToString() + "] is not integer!";
            }

            if(intResolution<=0)
            {
                return "CalActualVolt error: The Resolution input [" + objResolution.ToString() + "] is not bigger than 0!";
            }

            //If everything is OK, then sending command to get AD value and return

            //Sending
            string strRet = "";
            strRet = this.NECWR(strCommand, this.strModuleSerial, 2000);

            if(strRet!="0")
            {
                return "CalActualVolt error: sending command fail! return value: [" + strRet + "]";
            }

            if((this.lstByteReceiveData.Count-1)<intHiBytePos)
            {
                return "CalActualVolt error: lstByteReceiveData has not enough data compare with High byte setting! Maximum position: [" + (this.lstByteReceiveData.Count - 1).ToString() + "]";
            }

            if ((this.lstByteReceiveData.Count - 1) < intLowBytePos)
            {
                return "CalActualVolt error: lstByteReceiveData has not enough data compare with Low byte setting! Maximum position: [" + (this.lstByteReceiveData.Count - 1).ToString() + "]";
            }

            double dblADValue = 256 * this.lstByteReceiveData[intHiBytePos] + this.lstByteReceiveData[intLowBytePos];
            double dblVoltValue = dblADValue * dblVref / (double)intResolution;
            objRet = dblVoltValue;
            //
            return objRet;
        }

        /// <summary>
        /// CalActualAD - Functions to support VE Calibration
        /// </summary>
        /// <param name="objCommand">This command send to MPU to get AD value - F02?</param> 
        /// <param name="objHiBytePos">This is position of high byte return (count from 0)</param>
        /// <param name="objLoBytePos">This is position of Low byte return (count from 0)</param>
        /// <param name="objVref">This is V reference of sensor - 5V but in the future it can be 3.3V</param>
        /// <param name="objResolution">Resolution of AD conversion - normally is 10bits - 1024</param>
        /// <returns></returns>
        public object CalActualAD(object objCommand, object objHiBytePos, object objLoBytePos, object objModuleSerial = null)
        {
            object objRet = 0;
            //
            string strCommand = objCommand.ToString().Trim();
            //
            int intHiBytePos = 0;
            if (int.TryParse(objHiBytePos.ToString().Trim(), out intHiBytePos) == false)
            {
                return "CalActualAD error: The high byte input [" + objHiBytePos.ToString() + "] is not integer!";
            }
            //
            int intLowBytePos = 0;
            if (int.TryParse(objLoBytePos.ToString().Trim(), out intLowBytePos) == false)
            {
                return "CalActualAD error: The Low byte input [" + objLoBytePos.ToString() + "] is not integer!";
            }
            
            //If everything is OK, then sending command to get AD value and return

            //Sending
            string strRet = "";

            string strModuleSerial = this.strModuleSerial;
            if (objModuleSerial != null)
            {
                if(objModuleSerial.ToString().Trim()!="")
                {
                    strModuleSerial = objModuleSerial.ToString();
                }
            } 

            strRet = this.NECWR(strCommand, strModuleSerial, 2000);

            if (strRet != "0")
            {
                return "CalActualVolt error: sending command fail! return value: [" + strRet + "]";
            }

            if ((this.lstByteReceiveData.Count - 1) < intHiBytePos)
            {
                return "CalActualVolt error: lstByteReceiveData has not enough data compare with High byte setting! Maximum position: [" + (this.lstByteReceiveData.Count - 1).ToString() + "]";
            }

            if ((this.lstByteReceiveData.Count - 1) < intLowBytePos)
            {
                return "CalActualVolt error: lstByteReceiveData has not enough data compare with Low byte setting! Maximum position: [" + (this.lstByteReceiveData.Count - 1).ToString() + "]";
            }

            double dblADValue = 256 * this.lstByteReceiveData[intHiBytePos] + this.lstByteReceiveData[intLowBytePos];
            objRet = dblADValue;
            //
            return objRet;
        }

        //**************************************************************************************************************
        /// <summary>
        /// ///////////////
        /// </summary>
        public void ShowWindowCommand()
        {
            //Create new window
            this.wdCommandTest = new Views.wdNecCommand();
            this.wdCommandTest.myTimer.Tick += myTimer_Tick;
            this.wdCommandTest.btnSend.Click += btnSend_Click;
            this.wdCommandTest.btnContinueSend.Click += btnContinueSend_Click;
            this.wdCommandTest.Show();
        }

        void myTimer_Tick(object sender, EventArgs e)
        {
            this.btnSendHandle();
        }

        void btnContinueSend_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(this.wdCommandTest.myTimer.IsEnabled==false)
            {
                this.wdCommandTest.myTimer.Start();
            }
            else
            {
                this.wdCommandTest.myTimer.Stop();
            }
            
        }

        private int intSendingTimes = 0;
        private int intSendingTimesOK = 0;
        private double dblTotalTime = 0;
        void btnSend_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.btnSendHandle();
        }

        //
        public void btnSendHandle()
        {
            int i = 0;
            string strRet = "";
            bool blResult = false;

            this.wdCommandTest.tbReceive.Text = "";
            //
            string strCommand = this.wdCommandTest.tbCommand.Text.Trim();

            long lngStartTick = GetTickCount();
            //Sending
            strRet = this.NECWR(strCommand, this.strModuleSerial, 2000);

            long lngTactTime = GetTickCount() - lngStartTick;

            //Display Actual Send
            string strTemp = "";
            for (i = 0; i < this.lstByteSendingData.Count; i++)
            {
                strTemp = strTemp + this.lstByteSendingData[i].ToString("X") + ",";
            }
            this.wdCommandTest.tbActualSend.Text = strTemp;

            if (strRet != "0") //NG
            {
                this.wdCommandTest.tbReceive.Text = strRet;
            }
            else //OK
            {
                //Sending OK
                blResult = true;
                strRet = "";
                for (i = 0; i < this.lstByteReceiveData.Count; i++)
                {
                    strRet = strRet + this.lstByteReceiveData[i].ToString("X") + ",";
                }
                this.wdCommandTest.tbReceive.Text = strRet;
            }

            //Summary Result
            string strSummaryResult = "";

            if (this.dblTotalTime > 1000000000) //Need to reset
            {
                this.dblTotalTime = 0;
                this.intSendingTimesOK = 0;
                this.intSendingTimes = 0;
            }

            if (this.intSendingTimes > 1000000000) //Need to reset
            {
                this.intSendingTimesOK = 0;
                this.intSendingTimes = 0;
                this.dblTotalTime = 0;
            }

            if (this.intSendingTimesOK > 1000000000) //Need to reset
            {
                this.intSendingTimesOK = 0;
            }

            if (blResult == true) this.intSendingTimesOK++;
            this.intSendingTimes++;
            this.dblTotalTime = this.dblTotalTime + lngTactTime;
            double dblAverageTime = Math.Round(this.dblTotalTime / (double)this.intSendingTimes, 2);

            double dblPassRate =Math.Round(100*(double)this.intSendingTimesOK / (double)this.intSendingTimes, 2);

            //
            strSummaryResult = "OK rate: " + dblPassRate.ToString() + "% (" + this.intSendingTimesOK.ToString() + "/" + this.intSendingTimes.ToString() + "). ";
            strSummaryResult = strSummaryResult + "Tact time: " + lngTactTime.ToString() + " ms. ";
            strSummaryResult = strSummaryResult + "Average time: " + dblAverageTime.ToString() + " ms.";

            this.wdCommandTest.tbSummaryResult.Text = strSummaryResult;
        }

        //
        void menuItemUserSetting_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ShowWindowCommand();
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Constructor
        public clsWirelessCom()
        {
            this.menuSetting = new System.Windows.Controls.MenuItem();
            this.menuSetting.Header = "NEC Wireless Command Tool";
            this.menuSetting.Click += menuItemUserSetting_Click;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}

