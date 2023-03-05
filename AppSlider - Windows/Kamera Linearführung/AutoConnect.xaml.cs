using System;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Kamera_Linearführung
{
    /// <summary>
    /// Die COM-vebindungen werden nacheinander getestet bis das richtige Gerät gefunden wurde
    /// </summary>
    public partial class AutoConnect : Window
    {
        MainWindow MW;

        public AutoConnect(MainWindow mw)
        {
            MW = mw;
            MW.button_connect.IsEnabled = false;

            InitializeComponent();

            MeldungEntfernen();
            sucheCOMstart();
        }

        private void Meldung(string Meldung, Brush color)
        {
            Dispatcher.Invoke(new Action(delegate ()
            {
                textBlock_Meldung.Foreground = color;
                textBlock_Meldung.Text = Meldung;
            }));
        }

        private void MeldungEntfernen()
        {
            Meldung("", Brushes.Black);
        }

        private void NormaleMeldung(string meldung)
        {
            Meldung(meldung, Brushes.Black);
        }

        private void FehlerMeldung(string errorMeldung)
        {
            Meldung(errorMeldung, Brushes.Red);
        }

        private void beschäftigt(bool isBeschäftigt)
        {
            Dispatcher.Invoke(new Action(delegate ()
            {
                if (isBeschäftigt)
                {
                    button_erneuteSucheCOM.IsEnabled = false;
                    progressBar_COMsuche.Value = 0;
                }
                else
                {
                    button_erneuteSucheCOM.IsEnabled = true;
                    progressBar_COMsuche.Value = progressBar_COMsuche.Maximum;
                }
            }));
        }

        private void progressBarChange(int value, int maxvalue)
        {
            Dispatcher.Invoke(new Action(delegate ()
            {
                progressBar_COMsuche.Maximum = maxvalue;
                progressBar_COMsuche.Value = value;
            }));
        }

        private void verbindungErfolgreich()
        {
            progressBarChange(100, 100);    // Progressbar auf 100% von 100% setzen
            MW.ansichtVerbunden(true);

            Thread.Sleep(1000);  // warte kurz um den User nicht zu verwirren

            Dispatcher.Invoke(new Action(delegate ()
            {
                this.Close();
            }));
        }

        private void sucheCOM()
        {
            MeldungEntfernen();

            beschäftigt(true);

            // erhalte COM-Namen
            string[] PortNames = SerialPort.GetPortNames();
            if (PortNames.Length < 0)
            {
                FehlerMeldung("Kein COM angeschlossen");
                return;
            }

            // erhalte baud
            int baud = 115200;
            Dispatcher.Invoke(new Action(delegate ()
            {
                if (!int.TryParse(textBox_baud.Text.Trim(), out baud))
                {
                    FehlerMeldung("Konnte Baud nicht auslesen");
                    return;
                }
            }));

            NormaleMeldung("Gefundene COM-Ports: " + PortNames.Length);

            if (MW.com.isCOMMopen())
                MW.com.closeCOMM();

            for (int i = 0; i < PortNames.Length && thisIsOpen; ++i)
            {
                progressBarChange(i, PortNames.Length);

                int portNr;
                if (!int.TryParse(PortNames[i].Replace("COM", ""), out portNr))
                    continue;

                if (!MW.com.openCOMM(0, portNr, baud))
                {
                    NormaleMeldung("COM" + portNr + " wird von einem anderen Programm belegt!");
                    continue;
                }
                else
                {
                    NormaleMeldung("Verbindungsaufbau mit COM" + portNr);
                    int warteZeit = 8; bool resetAusgeführt = false; bool wasWronComMeldung = false;
                    for (int j = 0; j < warteZeit; ++j)
                    {
                        Thread.Sleep(1000);

                        NormaleMeldung("Warte auf Startsignal von COM" + portNr + " - " + (warteZeit - j) + "s verbleiben");
                        if (MW.com.wasStart)
                        {
                            NormaleMeldung("Startsignal von COM" + portNr + " erhalten, überprüfe Gerät");

                            if (MW.com.frageNachMaschinenType())
                            {
                                MW.com.frageNachConfig();

                                Meldung("Verbindung mit COM" + portNr + " erfolgreich", Brushes.Green);
                                verbindungErfolgreich();
                                return;
                            }

                            wasWronComMeldung = true;
                            NormaleMeldung("Falscher Maschinentyp an COM" + portNr + " angeschlossen");
                            break;
                        }
                        else if (resetAusgeführt && MW.com.wasWait)
                        {
                            NormaleMeldung("Forcestart an COM" + portNr + " ausführen.");

                            MW.com.NummerZurücksetzen(false);

                            if (MW.com.frageNachMaschinenType())
                            {
                                Meldung("Verbindung mit COM" + portNr + " erfolgreich", Brushes.Green);
                                verbindungErfolgreich();
                                return;
                            }

                            wasWronComMeldung = true;
                            NormaleMeldung("Falscher Maschinentyp an COM" + portNr + " angeschlossen");
                            break;
                        }

                        // Wenn mehr als 3 Messages schon empfangen wurden wird ein Reset ausgeführt
                        if (!resetAusgeführt && MW.com.geleseneMessageCount > 3)
                        {
                            MW.com.setReset();
                            resetAusgeführt = true;
                        }

                    }

                    if (!wasWronComMeldung)
                        NormaleMeldung("Es wurde kein Startsignal von COM" + portNr + " empfangen");
                }

                if (MW.com.isCOMMopen())
                    MW.com.closeCOMM();
                MW.ansichtVerbunden(false);
            }

            FehlerMeldung("Es wurde kein passender COM gefunden");

            beschäftigt(false);
        }

        Thread comSuchethread;
        private void sucheCOMstart()
        {

            if (comSuchethread == null || !comSuchethread.IsAlive)
            {
                comSuchethread = new Thread(sucheCOM);
                comSuchethread.Start();
            }
        }

        public bool isOpen()
        {
            return thisIsOpen;
        }

        bool thisIsOpen = true;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            thisIsOpen = false;
            if (MW != null && MW.isMainWindowAlive)
                MW.button_connect.IsEnabled = true;
        }

        private void button_erneuteSucheCOM_Click(object sender, RoutedEventArgs e)
        {
            sucheCOMstart();
        }
    }
}
