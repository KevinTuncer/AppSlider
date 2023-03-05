using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;


namespace Kamera_Linearführung
{
    class Message
    {
        public Message(byte[] bytesMSG)
        {
            setByBytes(bytesMSG);
        }

        public Message(string Text)
        {
            setByText(Text);
        }

        public void setByText(string Text)
        {
            string[] Befehle = Text.Trim().Split(' ');
            int pos = 0;

            float f = 0; UInt16 ui = 0; byte b = 0x00;
            if (Befehle[pos].Length == 0)
                return;
            switch (Befehle[pos][0])
            {
                case 'N':
                case 'n':
                    isN = readUInt16(Befehle[pos], out ui);
                    N = ui;
                    pos++;
                    break;
                default:
                    isN = false;
                    break;
            }

            switch (Befehle[pos][0])
            {
                case 'M':
                case 'm':
                    isM = readUInt16(Befehle[pos], out ui);
                    M = ui;
                    pos++;
                    isG = false;
                    break;
                case 'G':
                case 'g':
                    isG = readByte(Befehle[pos], out b);
                    G = b;
                    pos++;
                    isM = false;
                    break;
                default:
                    isM = false;
                    isG = false;
                    break;
            }

            isX = false; isY = false; isZ = false; isF = false;
            isI = false; isJ = false; isK = false; isL = false;// - neu hinzugefügt
            isE = false; isT = false; isS = false; isP = false;// - neu hinzugefügt
            isNewN = false; isA = false; isB = false; isC = false;// - neu hinzugefügt
            isD = false; isH = false; isR = false; isO = false;// - neu hinzugefügt

            for (; pos < Befehle.Length; ++pos)
            {
                if (Befehle[pos].Length == 0)
                    continue;
                switch (Befehle[pos][0])
                {
                    case 'X':
                    case 'x':
                        isX = readFloat(Befehle[pos], out f);
                        X = f;
                        break;
                    case 'Y':
                    case 'y':
                        isY = readFloat(Befehle[pos], out f);
                        Y = f;
                        break;
                    case 'Z':
                    case 'z':
                        isZ = readFloat(Befehle[pos], out f);
                        Z = f;
                        break;
                    case 'E':
                    case 'e':
                        isE = readFloat(Befehle[pos], out f);
                        E = f;
                        break;
                    case 'F':
                    case 'f':
                        isF = readFloat(Befehle[pos], out f);
                        F = f;
                        break;
                    case 'T':
                    case 't':
                        isT = readUInt16(Befehle[pos], out ui);
                        T = ui;
                        break;
                    case 'S':
                    case 's':
                        isS = readFloat(Befehle[pos], out f);   //- int32 statt float
                        S = f;
                        break;
                    case 'P':
                    case 'p':
                        isP = readFloat(Befehle[pos], out f);   //- int32 statt float
                        P = f;
                        break;
                    case 'N':
                        isNewN = readUInt16(Befehle[pos], out ui);
                        NewN = ui;
                        break;
                    // - neu hinzugefügt
                    case 'I':
                    case 'i':
                        isI = readFloat(Befehle[pos], out f);
                        I = f;
                        break;
                    case 'J':
                    case 'j':
                        isJ = readFloat(Befehle[pos], out f);
                        J = f;
                        break;
                    case 'K':
                    case 'k':
                        isK = readFloat(Befehle[pos], out f);
                        K = f;
                        break;
                    case 'L':
                    case 'l':
                        isL = readFloat(Befehle[pos], out f);
                        L = f;
                        break;
                    case 'A':
                    case 'a':
                        isA = readFloat(Befehle[pos], out f);
                        A = f;
                        break;
                    case 'B':
                    case 'b':
                        isB = readFloat(Befehle[pos], out f);
                        B = f;
                        break;
                    case 'C':
                    case 'c':
                        isC = readFloat(Befehle[pos], out f);
                        C = f;
                        break;
                    case 'D':
                    case 'd':
                        isD = readFloat(Befehle[pos], out f);
                        D = f;
                        break;
                    case 'H':
                    case 'h':
                        isH = readFloat(Befehle[pos], out f);
                        H = f;
                        break;
                    case 'R':
                    case 'r':
                        isR = readFloat(Befehle[pos], out f);
                        R = f;
                        break;
                    case 'O':
                    case 'o':
                        isO = readFloat(Befehle[pos], out f);
                        O = f;
                        break;
                }
            }
        }

        private static bool readFloat(string Befehl, out float f)
        {
            if (!float.TryParse(Befehl.Substring(1), out f))
            {
                MessageBox.Show("Fehler, konnte " + Befehl[0] + "-Wert nicht einlesen");
                return false;
            }
            return true;
        }

        private static bool readUInt16(string Befehl, out UInt16 i)
        {
            if (!UInt16.TryParse(Befehl.Substring(1), out i))
            {
                MessageBox.Show("Fehler, konnte " + Befehl[0] + "-Wert nicht einlesen");
                return false;
            }
            return true;
        }

        private static bool readByte(string Befehl, out byte b)
        {
            if (!byte.TryParse(Befehl.Substring(1), out b))
            {
                MessageBox.Show("Fehler, konnte " + Befehl[0] + "-Wert nicht einlesen");
                return false;
            }
            return true;
        }

        public void setByBytes(byte[] bytesMSG)
        {
            UInt16 def = BitConverter.ToUInt16(bytesMSG, 0);

            int pos = 2;

            float f;
            UInt16 ui;
            byte b;

            ui = 0;
            isN = leseUInt16WertAusBytesMSG(bytesMSG, def, 0, ref ui, ref pos);
            N = ui;

            b = 0;
            isM = leseByteWertAusBytesMSG(bytesMSG, def, 1, ref b, ref pos);
            M = b;

            b = 0;
            isG = leseByteWertAusBytesMSG(bytesMSG, def, 2, ref b, ref pos);
            G = b;

            f = 0;
            isX = leseFloatWertAusBytesMSG(bytesMSG, def, 3, ref f, ref pos);
            X = f;

            f = 0;
            isY = leseFloatWertAusBytesMSG(bytesMSG, def, 4, ref f, ref pos);
            Y = f;

            f = 0;
            isZ = leseFloatWertAusBytesMSG(bytesMSG, def, 5, ref f, ref pos);
            Z = f;

            f = 0;
            isE = leseFloatWertAusBytesMSG(bytesMSG, def, 6, ref f, ref pos);
            E = f;

            f = 0;
            isF = leseFloatWertAusBytesMSG(bytesMSG, def, 8, ref f, ref pos);
            F = f;

            b = 0;
            isT = leseByteWertAusBytesMSG(bytesMSG, def, 9, ref b, ref pos);    //- müss überprüft werden, T ist jeden Falls ein short
            T = b;

            f = 0;
            isS = leseFloatWertAusBytesMSG(bytesMSG, def, 10, ref f, ref pos);
            S = f;

            f = 0;
            isP = leseFloatWertAusBytesMSG(bytesMSG, def, 11, ref f, ref pos);
            P = f;

            // - neu hinzugefügt
            // Müsse dann wahrscheinlich auf das Protokoll V2 also das Repetierprotokoll umsteigen, aber kp genau
            /*f = 0;
            isI = leseFloatWertAusBytesMSG(bytesMSG, def, 12, ref f, ref pos);
            I = f;
            f = 0;
            isI = leseFloatWertAusBytesMSG(bytesMSG, def, 12, ref f, ref pos);
            I = f;
            f = 0;
            isI = leseFloatWertAusBytesMSG(bytesMSG, def, 12, ref f, ref pos);
            I = f;
            f = 0;
            isI = leseFloatWertAusBytesMSG(bytesMSG, def, 12, ref f, ref pos);
            I = f;
            ...*/

            //- für NewN bei einem M Befehl noch kp
        }

        private static bool leseFloatWertAusBytesMSG(byte[] bytesMSG, UInt16 def, int bitVerschiebung, ref float Variabel, ref int pos)
        {
            if ((def >> bitVerschiebung & 1) == 1)
            {
                Variabel = BitConverter.ToSingle(bytesMSG, pos);
                pos += 4;
                return true;
            }
            else
                return false;
        }

        private static bool leseByteWertAusBytesMSG(byte[] bytesMSG, UInt16 def, int bitVerschiebung, ref byte Variabel, ref int pos)
        {
            if ((def >> bitVerschiebung & 1) == 1)
            {
                Variabel = bytesMSG[pos];
                pos++;
                return true;
            }
            else
                return false;
        }

        private static bool leseUInt16WertAusBytesMSG(byte[] bytesMSG, UInt16 def, int bitVerschiebung, ref UInt16 Variabel, ref int pos)
        {
            if ((def >> bitVerschiebung & 1) == 1)
            {
                Variabel = BitConverter.ToUInt16(bytesMSG, pos);
                pos += 2;
                return true;
            }
            else
                return false;
        }

        private string FloatToString(float Wert)
        {
            return Wert.ToString().Replace(',', '.');
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (isN)
                sb.Append("N" + N + ' ');
            if (isM)
                sb.Append("M" + M + ' ');
            if (isG)
                sb.Append("G" + G + ' ');
            if (isX)
                sb.Append("X" + FloatToString(X) + ' ');
            if (isY)
                sb.Append("Y" + FloatToString(Y) + ' ');
            if (isZ)
                sb.Append("Z" + FloatToString(Z) + ' ');
            if (isE)
                sb.Append("E" + FloatToString(E) + ' ');
            if (isF)
                sb.Append("F" + FloatToString(F) + ' ');
            if (isT)
                sb.Append("T" + T + ' ');
            if (isS)
                sb.Append("S" + FloatToString(S) + ' ');
            if (isP)
                sb.Append("P" + FloatToString(P) + ' ');
            if (isNewN)
                sb.Append("N" + NewN + ' ');

            //- Neu hinzugefügt
            if (isI)
                sb.Append("I" + FloatToString(I) + ' ');
            if (isJ)
                sb.Append("J" + FloatToString(J) + ' ');
            if (isK)
                sb.Append("K" + FloatToString(K) + ' ');
            if (isL)
                sb.Append("L" + FloatToString(L) + ' ');
            if (isA)
                sb.Append("A" + FloatToString(A) + ' ');
            if (isB)
                sb.Append("B" + FloatToString(B) + ' ');
            if (isC)
                sb.Append("C" + FloatToString(C) + ' ');
            if (isD)
                sb.Append("D" + FloatToString(D) + ' ');
            if (isH)
                sb.Append("H" + FloatToString(H) + ' ');
            if (isO)
                sb.Append("O" + FloatToString(O) + ' ');
            if (isR)
                sb.Append("R" + FloatToString(R) + ' ');

            return sb.ToString().TrimEnd();
        }

        public byte[] ToRepRapBytes()
        {
            string Gcode = this.ToString() + " *";
            byte[] GBytes = Encoding.ASCII.GetBytes(Gcode);
            int prüfsumme = CheckSum_RepRap(GBytes);

            return CombineByteArrays(GBytes, Encoding.ASCII.GetBytes(prüfsumme.ToString()));
        }

        public byte[] ToRepetierBytes()
        {
            List<byte> bytes = new List<byte>();
            byte first = 0x80, second = 0x00;
            if (isN)
            {
                first |= 1;
                byte[] byteArray = BitConverter.GetBytes(N);
                bytes.AddRange(byteArray);
            }
            if (isM)
            {
                first |= 1 << 1;
                if (M < 256)
                    bytes.Add((byte)M);
                else
                {
                    byte[] byteArray = BitConverter.GetBytes(M);
                    bytes.AddRange(byteArray);
                    MessageBox.Show("Achtung", "ToRepetierBytes() ist noch nicht auf M-Codes größer 255 getestet!");
                }
            }
            if (isG)
            {
                first |= 1 << 2;
                bytes.Add(G);
            }
            if (isX)
            {
                first |= 1 << 3;
                byte[] byteArray = BitConverter.GetBytes(X);
                bytes.AddRange(byteArray);
            }
            if (isY)
            {
                first |= 1 << 4;
                byte[] byteArray = BitConverter.GetBytes(Y);
                bytes.AddRange(byteArray);
            }
            if (isZ)
            {
                first |= 1 << 5;
                byte[] byteArray = BitConverter.GetBytes(Z);
                bytes.AddRange(byteArray);
            }
            if (isE)
            {
                first |= 1 << 6;
                byte[] byteArray = BitConverter.GetBytes(E);
                bytes.AddRange(byteArray);
            }

            if (isF)
            {
                second |= 1;
                byte[] byteArray = BitConverter.GetBytes(F);
                bytes.AddRange(byteArray);
            }
            if (isT)
            {
                second |= 1 << 1;
                if (T < 256)
                    bytes.Add((byte)T);
                else
                {
                    byte[] byteArray = BitConverter.GetBytes(T);
                    bytes.AddRange(byteArray);
                    MessageBox.Show("Achtung", "ToRepetierBytes() ist noch nicht auf T größer 255 getestet!");
                }
            }
            if (isS)
            {
                second |= 1 << 2;
                byte[] byteArray = BitConverter.GetBytes(S);
                bytes.AddRange(byteArray);
            }
            if (isP)
            {
                second |= 1 << 3;
                byte[] byteArray = BitConverter.GetBytes(P);
                bytes.AddRange(byteArray);
            }

            bytes.Insert(0, second);
            bytes.Insert(0, first);

            //- für NewN bei einem M Befehl, noch kp wie das in Repetier-Code definiert ist

            byte[] aBytes = bytes.ToArray();

            UInt16 prüfsumme = CheckSum_Fletcher(aBytes, aBytes.Length);
            bytes.AddRange(BitConverter.GetBytes(prüfsumme));
            return bytes.ToArray();
        }


        public static byte[] CombineByteArrays(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public static byte[] CombineByteArrays(byte[] first, byte[] second, byte[] third)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            return ret;
        }

        // Reprap CheckSum
        private int CheckSum_RepRap(byte[] bytes)
        {
            int c = 0;
            foreach (byte b in bytes)
            {
                if (b == '*') break;
                c ^= b;
            }
            c &= 0xFF;
            return c;
        }

        // Repetier CheckSum nach Fletcher
        public static UInt16 CheckSum_Fletcher(byte[] Bytes, int length)
        {
            uint C0, C1;
            uint data;
            uint i;
            UInt16 ck1, ck2;

            /* Initial value */
            C0 = 0;
            C1 = 0;

            /* memories - 32bits wide*/
            for (i = 0; i < length; i++)    /* nb_bytes has been verified */
            {

                data = Bytes[i];
                C0 = (C0 + data) % 255;
                C1 = (C1 + C0) % 255;

            }
            /* Calculate the intermediate ISO checksum value */
            ck1 = (byte)C0;//(byte)((255 - ((C0 + C1)) % 255));
            ck2 = (byte)(C1 % 255);
            if (ck1 == 0)
            {
                //ck1 = MASK_BYTE_LSB;
            }
            if (ck2 == 0)
            {
                //ck2 = MASK_BYTE_LSB;
            }

            return (UInt16)((((UInt16)ck2) << 8) | ((UInt16)ck1));
        }

        public void setN(UInt16 Nummer)
        {
            isN = true;
            N = Nummer;
        }

        public string Fehlermeldung { get; private set; }

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float F { get; private set; }
        public float E { get; private set; }
        public float P { get; private set; }
        public float S { get; private set; }
        public UInt16 N { get; private set; }
        public byte G { get; private set; }
        public UInt16 M { get; private set; }
        public UInt16 T { get; private set; }
        public UInt16 NewN { get; private set; }

        //- Neu hinzugefügt
        public float I { get; private set; }
        public float J { get; private set; }
        public float K { get; private set; }
        public float L { get; private set; }
        public float A { get; private set; }
        public float B { get; private set; }
        public float C { get; private set; }
        public float D { get; private set; }
        public float H { get; private set; }
        public float O { get; private set; }
        public float R { get; private set; }

        public bool isX { get; private set; }
        public bool isY { get; private set; }
        public bool isZ { get; private set; }
        public bool isF { get; private set; }
        public bool isE { get; private set; }
        public bool isP { get; private set; }
        public bool isS { get; private set; }
        public bool isN { get; private set; }
        public bool isG { get; private set; }
        public bool isM { get; private set; }
        public bool isT { get; private set; }
        public bool isNewN { get; private set; }

        //- Neu hinzugefügt
        public bool isI { get; private set; }
        public bool isJ { get; private set; }
        public bool isK { get; private set; }
        public bool isL { get; private set; }
        public bool isA { get; private set; }
        public bool isB { get; private set; }
        public bool isC { get; private set; }
        public bool isD { get; private set; }
        public bool isH { get; private set; }
        public bool isO { get; private set; }
        public bool isR { get; private set; }
    }
}
