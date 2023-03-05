using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;

namespace Kamera_Linearführung
{
    static class lizenz
    {
        [DllImport("tkl.dll")]
        public static extern char genKey(char[] k, int nr);

        [DllImport("tkl.dll")]
        public static extern bool Test(char[] k);

        [DllImport("tkl.dll")]
        public static extern int genResendKey(char[] k);

        private static bool prüfeAufMöglichkeit(char[] key)
        {
            return Test(key);
        }

        private static char[] verschlüssele1(char[] key)
        {
            char[] newKey = new char[key.Length];
            for (int i = 0; i < key.Length; ++i)
            {
                newKey[i] = genKey(key, i);
            }
            return newKey;
        }

        private static string CharaArrayToHexZahlString(char[] key)
        {
            return BitConverter.ToString(ASCIIEncoding.ASCII.GetBytes(key)).Replace("-", "");
        }

        private static Rückmeldung OnlineMessage(string URI, string POST, out string reKey)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string HtmlResult = "";
                try
                {
                    HtmlResult = wc.UploadString(URI, POST);
                }
                catch
                {
                    reKey = "";
                    return Rückmeldung.OnlineFehlgeschlagen;
                }

                reKey = HtmlResult.Trim();
                return Rückmeldung.AllesOK;
            }
        }

        private static Rückmeldung prüfeOnline(char[] key)
        {
            // generiere einen Verschlüsselten Key
            char[] newKey = verschlüssele1(key);

            // Verschicke den verschlüsselten Key als HexZahlenString und erhalte einen verschlüsselten Bestätigungskey
            string sNewKey = CharaArrayToHexZahlString(newKey);
            string reKey = "";
            //Rückmeldung reMeldung = OnlineMessage("http://localhost/check.php", "Soft=LF&K=" + sNewKey, out reKey);
            Rückmeldung reMeldung = OnlineMessage("http://tuncer-technik.de/check.php", "Soft=LF&K=" + sNewKey, out reKey);

            if (reMeldung == Rückmeldung.OnlineFehlgeschlagen || reMeldung == Rückmeldung.FalscherKey)
                return reMeldung;

            // Vergleiche den Bestätigungskey mit dem eigenen in der verschlüsselten Form
            if (reKey == genResendKey(key).ToString())
                return Rückmeldung.AllesOK;
            else
                return Rückmeldung.FalscherKey;

        }

        private static string pfad = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Tuncer Technik\\Slider";
        private static string Filename = "liz";

        private class saveDaten
        {
            public static bool isOrdnerSchreibrecht(string OrdnerPfad)
            {
                try
                {
                    // Attempt to get a list of security permissions from the folder. 
                    // This will raise an exception if the path is read only or do not have access to view the permissions. 
                    System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(OrdnerPfad);
                    return true;
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }

            public bool auslesen()
            {
                string standDrin = DateiVerschlüsselung.lesen(pfad + "\\" + Filename);
                //MessageBox.Show(pfad + "\\" + Filename);
                if (standDrin != null && standDrin.Trim() != "")
                {
                    string[] daten = standDrin.Trim().Split('|');

                    if (daten.Length < 5)
                        return false;

                    // überprüfe den alten Schlüssel
                    char[] ckey = daten[0].ToCharArray();
                    if (!Test(ckey))
                        return false;

                    cKey = ckey;

                    // überprüfe die abgespeicherte Zeit
                    DateTime oldDate;
                    if (!DateTime.TryParse(daten[1], out oldDate))
                        return false;

                    if (oldDate > DateTime.Now) // ist die abgespeicherte Zeit später als die aktuelle wird der Code nicht mehr akzeptiert
                        return false;

                    date = oldDate;

                    // überprüfe on diese Version schon online aktiviert wurde
                    bool wasActivated;
                    if (!bool.TryParse(daten[2], out wasActivated))
                        return false;

                    activated = wasActivated;

                    // überprüfe ob die mac-Adresse des Internetcontrollers und die BIOS-Daten immernoch überein stimmen
                    string NowMac = Identifizierung.macId();
                    string NowBiosID = replaceSpecialChar(Identifizierung.BIOS_ID());

                    if (NowMac != daten[3] && daten[3].Trim() != null)
                    {
                        //- wenn man kein Internet hat, kann hier durchaus etwas anderes stehen
                    }

                    if (daten[4] != NowBiosID)
                        return false;

                    return wasActivated;
                }

                return false;
            }

            public void schreiben()
            {
                if (!Directory.Exists(pfad))
                    Directory.CreateDirectory(pfad);

                WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

                if (!isOrdnerSchreibrecht(pfad))
                {
                    if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                        MessageBox.Show("Slider besitzt kein Schreibrecht zur überprüfung der Lizenz,\nbitte versuchen Sie das Programm \"als Administrator\" auszuführen");
                    else
                        MessageBox.Show("Slider konnte die Lizenz nicht ordnungsgemäß überprüfen, Schreibrecht verwehrt");

                    if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsActive)
                        Application.Current.MainWindow.Close();
                }

                string mac = Identifizierung.macId();
                string biosID = replaceSpecialChar(Identifizierung.BIOS_ID());

                DateiVerschlüsselung.schreiben(pfad + "\\" + Filename, new string(cKey) + trennChar + date.ToString() + trennChar + activated.ToString() + trennChar + mac + trennChar + biosID);
            }

            const char trennChar = '|';

            private string replaceSpecialChar(string s)
            {
                return s.Replace(trennChar, '/');
            }

            public void setDaten(char[] c_Key, DateTime Date, bool Activated)
            {
                cKey = c_Key;
                date = Date;
                activated = Activated;
            }

            public void setKey(char[] c_Key)
            {
                cKey = c_Key;
            }

            public void setDate(DateTime Date)
            {
                date = Date;
            }

            public void setActivated(bool Activated)
            {
                activated = Activated;
            }

            public char[] cKey { get; private set; }
            public DateTime date { get; private set; }
            public bool activated { get; private set; }
        }

        public static bool isAktiviert()
        {
            return true; // Make it free
            //return (new saveDaten()).auslesen();
        }

        public static void erneuteOnlineÜberprüfung()
        {
            return; // make it free

            //saveDaten sv = new saveDaten();
            //sv.auslesen();

            //Rückmeldung rMeldung = prüfeOnline(sv.cKey);
            //if (rMeldung != Rückmeldung.AllesOK && rMeldung != Rückmeldung.OnlineFehlgeschlagen)    // Falls der Key nun Falsch sein sollte wird nochmal überprüft aber diesmal das Ergebnis abgespeichert und beim nächsten Neustart ist eine erneute Schlüsseleingabe erforderlich
            //    rMeldung = prüfeKey(sv.cKey);

            //return rMeldung;
        }

        public enum Rückmeldung { AllesOK, FalscherKey, OnlineFehlgeschlagen }

        public static Rückmeldung prüfeKey(char[] key)
        {
            bool isMöglich = prüfeAufMöglichkeit(key);
            if (!isMöglich)
                return Rückmeldung.FalscherKey;

            Rückmeldung onlineMeldung = prüfeOnline(key);

            //MessageBox.Show("Möglich: " + isMöglich + "\nOnline OK: " + isOK);

            // Speichere Werte ab:
            saveDaten sd = new saveDaten();
            sd.setKey(key);                         // den Schlüssel
            sd.setDate(DateTime.Now);    // die aktuelle Uhrzeit

            // ob die aktivierung erfolgreich war
            if (onlineMeldung == Rückmeldung.AllesOK)
                sd.setActivated(true);
            else
                sd.setActivated(false);

            sd.schreiben();

            return onlineMeldung;
        }
    }
}
