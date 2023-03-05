using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kamera_Linearführung
{

    public class COM : IDisposable
    {
        private Func<string, bool> ereignisMSG;
        private Func<bool, bool> ansichtVerbunden;

        //const bool forcestart = true;

        private SerialPort _serialPort;
        private Thread readThread;
        private bool _continue;
        bool isMSGThreadActivate = false;

        public COM(Func<string, bool> EreignisMSG, Func<bool, bool> AnsichtVerbunden)
        {
            ereignisMSG = EreignisMSG;
            wasSetSliderLength = false;

            ansichtVerbunden = AnsichtVerbunden;
        }

        public bool isCOMMopen()
        {
            if (_serialPort != null)
                return _serialPort.IsOpen;
            else
                return false;
        }

        public void setReset()
        {
            _serialPort.DtrEnable = true;
            _serialPort.DtrEnable = false;
            _serialPort.DtrEnable = true;

            if (isCOMMopen())
            {
                clearMSGLists();
                sendeReset();


                iniOpenCom();

                Nummer = 1;

            }
        }

        private void iniOpenCom()
        {
            wasStart = false;
            wasWait = false;
            geleseneMessageCount = 0;
            MaschinenType = "";
        }

        public string getSelectedPortName()
        {
            if (_serialPort != null && _serialPort.IsOpen)
                return _serialPort.PortName;
            else
                return "";
        }

        private SerialPort iniSerialPort(int DeviceID, int COMPORT, int BaudRate) //- DeviceID wird nicht verwendet
        {
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = "COM" + COMPORT;
            _serialPort.BaudRate = BaudRate;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            return _serialPort;
        }

        public bool openCOMM(int DeviceID, int COMPORT, int BaudRate)
        {
            iniOpenCom();


            _serialPort = iniSerialPort(DeviceID, COMPORT, BaudRate);

            readThread = new Thread(Read);

            setReset();

            try
            {
                _serialPort.Open();
            }
            catch
            {
                return false;
            }
            _continue = true;
            if (_serialPort.IsOpen)
            {
                readThread.Start();
                return true;
            }
            else
                return false;

        }

        private void clearMSGLists()
        {
            sendeMsgList.Clear();
            wasSendMsgList.Clear();
        }

        public void closeCOMM()
        {
            isMSGThreadActivate = false;
            if (_serialPort != null && _serialPort.IsOpen)
            {
                if (_continue)
                {
                    clearMSGLists();
                    _continue = false;
                    //Thread.Sleep(500);
                    readThread.Join(_serialPort.ReadTimeout + 50);

                }
                try
                {
                    _serialPort.Close();
                }
                catch
                {

                }
            }
            _continue = false;
        }

        private void Read()
        {
            while (_continue)
            {
                try
                {
                    int messageByte = _serialPort.ReadByte();
                    readVerarbeitung((byte)messageByte);
                }
                catch (TimeoutException) { }
                catch (System.IO.IOException)
                {
                    // Stecker wurde vorrausichtlich gezogen
                    closeCOMM();

                    ereignisMSG("<Verbindung unterbrochen>");
                    ansichtVerbunden(false);
                }
                catch (System.InvalidOperationException)
                {
                    // Stecker wurde vorrausichtlich gezogen
                    closeCOMM();

                    ereignisMSG("<Verbindung unterbrochen>");
                    ansichtVerbunden(false);
                }

            }
        }

        List<byte> container = new List<byte>();
        byte oldReadByte = 0x00;

        private void readVerarbeitung(byte messageByte)
        {

            if (oldReadByte == 0x0D && messageByte == 0x0A)
            {
                if (container.Count > 0)
                    container.RemoveAt(container.Count - 1);  // entferne oldReadByte

                if (container.Count > 0)    //-

                    readAuswertung(container.ToArray());

                container = new List<byte>();
            }
            else
            {
                container.Add(messageByte);
                oldReadByte = messageByte;
            }
        }

        private bool neuSenden(int Nummer)
        {
            int count = wasSendMsgList.Count;
            for (int i = 0; i < count; ++i)
            {
                if (wasSendMsgList[i].isN && wasSendMsgList[i].N == Nummer)
                {
                    //SendMSG msg = wasSendMsgList[i];
                    //wasSendMsgList.RemoveAt(i);
                    //sendeMsgList.Insert(0, msg);

                    int removeAnzahl = wasSendMsgList.Count - i;
                    List<SendMSG> msgs = wasSendMsgList.GetRange(i, removeAnzahl);
                    wasSendMsgList.RemoveRange(i, removeAnzahl);
                    sendeMsgList.InsertRange(0, msgs);

                    ereignisMSG("<Neu senden ab " + Nummer + ">");

                    if (!isMSGThreadActivate)
                    {
                        Thread SendeMsgThread = new Thread(sendeMessageThread);
                        SendeMsgThread.Start();
                    }
                    return true;
                }
            }
            return false;
        }

        public bool wasStart, wasWait;
        public int geleseneMessageCount { get; private set; }
        private void messageCounter()
        {
            geleseneMessageCount++;
            if (geleseneMessageCount > 30000)
                geleseneMessageCount = 0;
        }


        const byte r = (byte)'r';
        private void readAuswertung(byte[] readMessage)
        {
            if (readMessage.Length < 2)
                return;

            /*//- erstelle Hex-String
              StringBuilder sb = new StringBuilder();
             foreach (byte b in readMessage)
                 sb.Append("0x" + b.ToString("X2") + " ");*/

            // erhalte Message-String
            string stringMSG = ASCIIEncoding.ASCII.GetString(readMessage);

            //- gebe Message und Hex-String aus
            string ereignisString = stringMSG;
            //ereignisString  += "\n" + sb.ToString();
            ereignisMSG(ereignisString);

            messageCounter();

            switch (readMessage[0])
            {
                case 0x6F:  // ok
                    if (stringMSG.Length > 1 && readMessage[1] == 0x6B)
                    {
                        if (stringMSG.Length > 2 && readMessage.Length > 2 && readMessage[2] == 0x20)  // bestätige Nummer
                        {
                            int N;
                            if (int.TryParse(stringMSG.Substring(3), out N))
                            {
                                int sendIterator = isWasSend(N);
                                if (sendIterator == -1)
                                {
                                    // Neu senden
                                    neuSenden(N);
                                }
                                else
                                {
                                    // Entferne die bestätigte Message aus der wasSendList
                                    wasSendMsgList.RemoveAt(sendIterator);

                                    //ereignisMSG("okidoki " + N + " <sendList>" + sendeMsgList.Count + " <wasSendList>" + wasSendMsgList.Count);
                                }
                            }
                        }
                        else                        // bestätigung ohne Nummer
                        {

                        }
                    }
                    break;

                case 0x52:  // Resend
                    if (stringMSG.Length >= 6 && stringMSG.Substring(0, 6) == "Resend")
                    {
                        if (readMessage[6] == 0x3A)  // Falls mit Doppelpunk
                        {
                            int N;
                            if (int.TryParse(stringMSG.Substring(7), out N))
                            {
                                // neu senden
                                if (!neuSenden(N))
                                    NummerZurücksetzen(false);  // Falls das neuSenden fehl schlägt werden die Nummern neu synchronisiert (wieder auf 1 gesetzt)
                            }
                            else
                                ereignisMSG("Konnte Resend-Wert nicht auslesen!");
                        }
                    }
                    break;

                case r: // resume
                    break;

                case 0X45:  // Error: 45 72 72 6F 72 3A
                    if (stringMSG.Length >= 5 && stringMSG.Substring(0, 5) == "Error")
                    {
                        if (stringMSG.Length > 5 && readMessage[5] == 0x3A)  // Falls mit Doppelpunk
                        {
                            if (stringMSG.Substring(6) == "Wrong checksum")    //Error:Wrong checksum
                            {
                                //ereignisMSG("");
                            }
                        }
                    }


                    break;

                case 0x55:  // Unknown command: 55 6E 6B 6E 6F 77 6E 20 63 6F 6D 6D 61 6E 3A
                    break;

                case 0x73:
                    // start 73 74 61 72 74
                    if (stringMSG.Length > 4 && readMessage[1] == 0x74 && readMessage[2] == 0x61 && readMessage[3] == 0x72 && readMessage[4] == 0x74)
                    {
                        wasStart = true;
                    }

                    // skip  73 6B 69 70 // 20 // und dann eine Zahl
                    if (stringMSG.Length > 3 && readMessage[1] == 0x6B && readMessage[2] == 0x69 && readMessage[3] == 0x70)
                    {

                    }

                    break;

                case 0x77:
                    // wait 77 61 69 74
                    if (stringMSG.Length > 3 && readMessage[1] == 0x61 && readMessage[2] == 0x69 && readMessage[3] == 0x74)
                    {
                        wasWait = true;
                    }
                    break;

                case 0x43:

                    if (stringMSG.Length > 12 && stringMSG.Contains("Config:XMax"))    // Config
                    {
                        Config_xMaxString(stringMSG);
                        wasWait = true;
                    }
                    break;

                default:
                    if (stringMSG.Length > 12 && stringMSG.Contains("MACHINE_TYPE"))
                    {
                        //System.Windows.MessageBox.Show(stringMSG);
                        Machine_TypeString(stringMSG);
                    }
                    break;
            }
            //if(6F 6B)
        }

        public string MaschinenType { get; private set; }
        public float SliderVersion { get; private set; }
        public float SliderLength { get; private set; }
        public bool wasSetSliderLength { get; private set; }

        private void getSliderVersionFromMachine_TypeString()
        {

            int end = MaschinenType.IndexOf(' ');
            if (end == -1)
                end = MaschinenType.Length;

            if (end > 0)
            {
                int start = end - 1;
                for (; start >= 0 && "01223456789.".IndexOf(MaschinenType[start]) != -1; start--) { } //TODO: sollte eigentlich start > 0 heißen;

                if (start >= 0)
                {
                    start++;
                    //System.Windows.MessageBox.Show(">>" + MaschinenType.Substring(start, end-start) + "<<");
                    float sVersion = -1;
                    float.TryParse(MaschinenType.Substring(start, end - start).Replace('.', ','), out sVersion);
                    SliderVersion = sVersion;
                    //System.Windows.MessageBox.Show("v. = " + SliderVersion);
                }
            }
        }

        private void Machine_TypeString(string MessageCode)
        {
            string[] codes = MessageCode.Split(' ');

            foreach (string s in codes)
            {
                int start = s.IndexOf("MACHINE_TYPE:");
                if (start >= 0)
                {
                    MaschinenType = s.Substring(start + 13);

                    getSliderVersionFromMachine_TypeString();
                    break;
                }
            }

        }

        private void Config_xMaxString(String MessageCode)
        {
            int start = MessageCode.IndexOf("XMax") + 5;
            if (start >= 0)
            {
                String s = MessageCode.Substring(start);

                float fo;
                if (float.TryParse(s.Replace('.', ','), out fo))
                {
                    SliderLength = fo;
                    wasSetSliderLength = true;
                }
                else
                    wasSetSliderLength = false;
            }
        }

        public bool frageNachMaschinenType()
        {
            // Sende Befehl zum erhalt des Maschinen Types
            shortGcodeBefehl("M115");

            for (int k = 0; k < 50; ++k)
            {
                Thread.Sleep(100);

                if (MaschinenType != "")
                {
                    if (MaschinenType.Contains("Slider"))
                    {
                        return true;
                    }
                    else
                        break;
                }
            }

            return false;
        }

        public bool frageNachConfig()
        {
            // Sende Befehl zum erhalt des Maschinen Types
            shortGcodeBefehl("M360");

            for (int k = 0; k < 50; ++k)
            {
                Thread.Sleep(100);

                if (wasSetSliderLength)
                {
                    return true;
                }
            }
            return false;
        }

        // überprüft ob die Messagenummer in der wasSendList ist
        private int isWasSend(int N)
        {
            int count = wasSendMsgList.Count;
            for (int i = 0; i < count; ++i)
            {
                if (wasSendMsgList[i].isN && wasSendMsgList[i].N == N)
                {
                    return i;
                }
            }
            return -1;
        }

        class SendMSG
        {
            public SendMSG(byte[] MsgByteArray, bool is_N, int n)
            {
                msgByteArray = MsgByteArray;
                //Message m = new Message(msgByteArray);
                //if (m.isN)
                //    N = m.N;
                //else
                N = n;
                isN = is_N;
            }

            public byte[] msgByteArray;
            public int N;
            public bool isN;
        }

        // Liste der zu sendenden Nachrichten
        List<SendMSG> sendeMsgList = new List<SendMSG>();
        // Liste der gesendeten Nachichten
        List<SendMSG> wasSendMsgList = new List<SendMSG>();

        private void sendeMessage(Message msg)
        {
            byte[] sendBytes = Message.CombineByteArrays(msg.ToRepRapBytes(), new byte[2] { 0x0D, 0x0A });

            sendeMsgList.Add(new SendMSG(sendBytes, msg.isN, msg.N));

            //hinterlege zuletzt gesetzte Position 
            double positionX;
            if (Slider.Status.newPositionFromMessage(msg, out positionX))
                Slider.Status.PositionX = positionX;

            //hinterlege zuletzt gesetzte Geschwindigkeit 
            double Speed;
            if (Slider.Status.newSpeedFromMessage(msg, out Speed))
                Slider.Status.Speed = Speed;

            ereignisMSG(msg.ToString());

            if (!isMSGThreadActivate)
            {
                Thread SendeMsgThread = new Thread(sendeMessageThread);
                SendeMsgThread.Start();
            }
        }

        private bool überprüfeTextAufN(string Text, out int N)
        {
            int index = Text.IndexOf('N');
            if (index >= 0)
            {
                int leerZeichen = Text.IndexOf(' ', index);
                if (leerZeichen == -1)
                    leerZeichen = Text.Length;

                if (int.TryParse(Text.Substring(index, leerZeichen - index), out N))
                {
                    return true;
                }
            }

            N = -1;
            return false;
        }

        private void sendeMessage(string text)
        {
            int N;
            bool isN = überprüfeTextAufN(text, out N);
            byte[] sendBytes = Message.CombineByteArrays(ASCIIEncoding.ASCII.GetBytes(text), new byte[2] { 0x0D, 0x0A });

            sendeMsgList.Add(new SendMSG(sendBytes, isN, N));

            ereignisMSG(text);

            if (!isMSGThreadActivate)
            {
                Thread SendeMsgThread = new Thread(sendeMessageThread);
                SendeMsgThread.Start();
            }
        }

        const int Buffergröße = 10;
        private void sendeMessageThread()
        {
            isMSGThreadActivate = true;
            while (sendeMsgList.Count > 0 && isMSGThreadActivate)
            {
                if (wasSendMsgList.Count < Buffergröße) // Warte bis der Buffer wieder frei ist
                {
                    SendMSG sMSG = sendeMsgList.First<SendMSG>();
                    wasSendMsgList.Add(sMSG);

                    Write(sMSG.msgByteArray);

                    sendeMsgList.Remove(sMSG);
                }
                else
                {
                    Thread.Sleep(500);
                    //- //TODO  Zeige an dass auf die Akzeptierung der Befehle gewartet wird. Sollte zulange gewartet werden soll eine fehlermeldung erscheinen  
                }
            }
            isMSGThreadActivate = false;

            // Zur Sicherheit um einen Nachrichtenabriss zu verhindern
            Thread.Sleep(100);
            if (sendeMsgList.Count > 0 && !isMSGThreadActivate)
                sendeMessageThread();
        }

        private void Write(byte[] Bytes)
        {
            //*
            //StringBuilder sb = new StringBuilder();
            //foreach (byte b in Bytes)
            //    sb.Append(b.ToString("X2") + " ");

            //Message msg = new Message(Bytes);
            //ereignisMessage(msg.ToString());
            //ereignisMessage("write: " + sb.ToString());
            if (_serialPort != null && _serialPort.IsOpen)  //- Absicherung falls Diskonnect, ist auf Nutzen noch nicht überprüft
                _serialPort.Write(Bytes, 0, Bytes.Length);
        }

        public UInt16 Nummer = 1;

        public void NummerZurücksetzen(bool mitOldNummer)   //- prüfe ob bool nötig
        {
            Message msg = new Message("M110 N0");
            if (mitOldNummer)
                msg.setN(Nummer);
            sendeMessage(msg);
            Nummer = 1;
        }

        public void sendeReset()
        {
            Message msg = new Message("M112");
            sendeMessage(msg);
        }

        private void NummerPP() // Setze die Nummern nach 65520 auf 1 zurück
        {
            Nummer++;
            if (Nummer > 0xFFF0)
            {
                NummerZurücksetzen(true);
            }
        }

        public void shortGcodeBefehl(string GcodeZeile)
        {
            Message msg = new Message(GcodeZeile);
            if (!msg.isN)
            {
                msg.setN(Nummer);
                NummerPP();
            }
            else if (msg.N + 1 == Nummer)
                NummerPP();

            sendeMessage(msg);
        }

        public void sendString(string Text)
        {
            sendeMessage(Text);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                closeCOMM();
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
