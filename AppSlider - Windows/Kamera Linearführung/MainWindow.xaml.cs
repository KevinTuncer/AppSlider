using Slider;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Kamera_Linearführung
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public bool prüfeAktivierung()
        {
            if (!lizenz.isAktiviert())
            {
                this.IsEnabled = false;
                AktivierungsWindow actiWin = new AktivierungsWindow(this);
                actiWin.Show();
                actiWin.Focus();
                return false;
            }
            else
            {
                this.IsEnabled = true;
                return true;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            expandiereEditorBoxen(false);
            listBox_Abläufe.ItemsSource = abläufe;

            // Lade AblaufButtons
            ladeAbläufe();

            erweiterteAnsicht(false);
            ansichtVerbunden(false);

            com = new COM(ereignisMessage, ansichtVerbunden);
            aktualisiere_comboBox_com();

            // Lade Settings, wie selected COM
            ladeSettings();

            this.IsEnabled = false;
            if (prüfeAktivierung()) // Wenn die abgespeicherte Überprüfung stimmte wird nochmals versucht den Key online zu überprüfen
            {
                new Thread(lizenz.erneuteOnlineÜberprüfung).Start();
            }
        }

        public COM com;
        public EditorWindow editorWindow;


        public ObservableCollection<Ablauf> abläufe = new ObservableCollection<Ablauf>();

        public bool ereignisMessage(string Text)
        {
            listBox_ereignis.Dispatcher.Invoke(new Action(delegate ()
            {
                if (listBox_ereignis.Items.Count > 1000)
                    listBox_ereignis.Items.RemoveAt(0);
                listBox_ereignis.Items.Add(Text);
                ScrollViewer sv = new ScrollViewer();
                sv.ScrollToEnd();

                // Scoll down
                if (AutoscrollEreignisBox && listBox_ereignis.Visibility == System.Windows.Visibility.Visible)
                {
                    ScrollViewer scrollViewer = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(listBox_ereignis, 0), 0) as ScrollViewer;
                    scrollViewer.ScrollToBottom();
                }
            }));
            return true;
        }

        private void aktualisiere_comboBox_com()
        {
            // Zeige verfügbare COMs an
            string[] PortNames = SerialPort.GetPortNames();
            int selectedindex = comboBox_com.SelectedIndex;
            comboBox_com.Items.Clear();
            foreach (string s in PortNames)
            {
                comboBox_com.Items.Add(s);
            }
            if (comboBox_com.SelectedIndex < 0 && comboBox_com.Items.Count > 0)
                comboBox_com.SelectedIndex = 0;
        }

        private void comboBox_com_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            aktualisiere_comboBox_com();
        }

        public bool isAnsichtVerbunden = false;
        public bool ansichtVerbunden(bool verbunden)
        {
            Dispatcher.Invoke(new Action(delegate ()
            {
                isAnsichtVerbunden = verbunden;
                if (verbunden)
                {
                    nichtGenullt();
                    button_connect.Content = "Trennen";
                    grid_Steuerung.IsEnabled = true;
                    button_AutoConnect.IsEnabled = false;
                    button_connect.IsEnabled = true;

                    aktualisiere_comboBox_com();
                    comboBox_com.SelectedValue = com.getSelectedPortName();

                    if (com.wasSetSliderLength)
                    {
                        slider_position.Maximum = com.SliderLength;
                    }

                    // Speichere den ausgewählten Port
                    settings.lastCOM = com.getSelectedPortName();
                    saveSettings();
                }
                else
                {
                    // prüfe ob das ShootParameter-Fenster geschlossen wurde und schließe ihn wenn nicht
                    if (shootParameterWindow != null && shootParameterWindow.IsInitialized)
                        shootParameterWindow.Close();

                    button_connect.Content = "Verbinden";
                    grid_Steuerung.IsEnabled = false;
                    button_AutoConnect.IsEnabled = true;
                    button_connect.IsEnabled = true;
                }

                ablaufButtonsEnable(verbunden);
            }));
            return true;
        }

        private void button_connect_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_com.SelectedIndex >= 0 && !com.isCOMMopen())
            {
                button_AutoConnect.IsEnabled = false;
                button_connect.IsEnabled = false;

                int portNr;
                if (!int.TryParse(comboBox_com.SelectedItem.ToString().Replace("COM", ""), out portNr))
                    return;
                int baud = 115200;
                //if (!int.TryParse(textBox_baud.Text.Trim(), out baud))
                //    return;
                int geräteID = 0;
                //if (!int.TryParse(textBox_baud.Text.Trim(), out geräteID))
                //    return;

                if (!com.openCOMM(geräteID, portNr, baud))
                {
                    MessageBox.Show("Port wird von einem anderen Programm belegt!");
                    ansichtVerbunden(false);
                    return;
                }

                new Thread(waitforReady).Start();
            }
            else if (com.isCOMMopen())
            {
                com.closeCOMM();

                ansichtVerbunden(false);
            }
        }

        private void waitforReady()
        {
            bool resetAusgeführt = false;
            for (int i = 0; i < 8; ++i)
            {
                Thread.Sleep(1000);
                if (com.wasStart || com.wasWait)
                {
                    if (com.wasWait)
                    {
                        com.NummerZurücksetzen(false);

                        if (com.frageNachMaschinenType())
                        {
                            com.frageNachConfig();

                            ansichtVerbunden(true);
                            return;
                        }
                        else
                            break;
                    }
                }

                // Wenn bereits mehr als 3 Messages empfangen wurden wird ein Reset ausgeführt
                if (!resetAusgeführt && com.geleseneMessageCount > 3)
                {
                    com.setReset();
                    resetAusgeführt = true;
                }
            }

            MessageBox.Show("Es scheit als wäre der falsche COM-Port ausgewählt,\nversuchen Sie \"Auto Connect\" um den richtigen COM zu finden.", "Fehler");
            // Com wieder entfernen
            if (com.isCOMMopen())
                com.closeCOMM();
            ansichtVerbunden(false);

            //- vielleicht noch einen Verbindungs-Timer anzeigen
        }


        private void button_senden_gCode_Click(object sender, RoutedEventArgs e)
        {
            com.shortGcodeBefehl(textBox_senden_gCode.Text.Trim());
        }

        private void textBox_senden_gCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                com.sendString(textBox_senden_gCode.Text.Trim());
        }

        public bool isAblaufÄnderungen = false;
        public bool isMainWindowAlive = true;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // prüfe ob das Ablauf-Editor-Fenster geschlossen wurde und schließe ihn wenn nicht
            if (editorWindow != null && editorWindow.IsInitialized)
                editorWindow.Close();

            // prüfe ob das ShootParameter-Fenster geschlossen wurde und schließe ihn wenn nicht
            if (shootParameterWindow != null && shootParameterWindow.IsInitialized)
                shootParameterWindow.Close();

            // Speichere ab, sofern es änderungen gab, seit der letzten Speichern.
            if (isAblaufÄnderungen)
            {
                saveAbläufe();
            }

            // prüfe ob der COM geschlossen wurde und schließe ihn wenn nicht
            if (com.isCOMMopen())
            {
                com.closeCOMM();
            }

            // prüfe ob das AutoConnect-Fenster geschlossen wurde und schließe ihn wenn nicht
            if (aC != null && aC.isOpen())
                aC.Close();

            isMainWindowAlive = false;
        }

        public double Runden(double lf)
        {
            return Math.Round(lf, 2);
        }

        private void slider_speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = Runden(e.NewValue);
            textBox_speed.Text = value.ToString();
            if (shootParameterWindow != null && shootParameterWindow.IsVisible)
            {
                camParam.speed = value;
                shootParameterWindow.textBox_speed.Text = value.ToString();
            }
            ZeigeVorraussichtlicheVerfahrZeitAn();
        }

        private void slider_position_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = Runden(e.NewValue);
            textBox_position.Text = value.ToString();
            if (shootParameterWindow != null && shootParameterWindow.IsVisible)
            {
                camParam.position = value;
                shootParameterWindow.textBox_position.Text = value.ToString();
            }
            ZeigeVorraussichtlicheVerfahrZeitAn();
        }

        private void ZeigeVorraussichtlicheVerfahrZeitAn()
        {
            if (slider_position == null || slider_speed == null)
                return;

            if (label_benötigteZeit != null)
                label_benötigteZeit.Content = Status.timeToString(Status.getFahrZeit(slider_position.Value, slider_speed.Value), true);
            //label_benötigteZeit_Millisec.Content = "+ " + ts.ToString("ff") + " ms";
        }


        private void slider_position_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (menuCheckBox_FahreDirekt.IsChecked == true)
                verFahre();
        }

        bool wasNullen = false;
        private void Nullen()
        {
            button_nullen.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x75, 0xF0, 0x68));
            com.shortGcodeBefehl("G28 X0");
            wasNullen = true;
        }

        private void nichtGenullt()
        {
            shoudAskNullen = true;
            wasNullen = false;
            button_nullen.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0x44, 0x44));
        }

        private void button_nullen_Click(object sender, RoutedEventArgs e)
        {
            Nullen();
        }

        private void button_stop_Click(object sender, RoutedEventArgs e)
        {
            com.setReset();
            nichtGenullt();
        }

        private void button_MotorenPower_Click(object sender, RoutedEventArgs e)
        {
            com.shortGcodeBefehl("M84");
        }

        public bool AutoscrollEreignisBox = false;
        private void checkBox_autoscroll_Checked(object sender, RoutedEventArgs e)
        {
            AutoscrollEreignisBox = true;
        }

        private void checkBox_autoscroll_Unchecked(object sender, RoutedEventArgs e)
        {
            AutoscrollEreignisBox = false;
        }

        private void button_clear_Click(object sender, RoutedEventArgs e)
        {
            listBox_ereignis.Items.Clear();
        }

        /*
        private void button_starteSchleife_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(schleife)).Start();
        }
        //-
        private void schleife()
        {
            for (int i = 0; i < 30 && com.isCOMMopen(); i++)
            {
                ereignisMessage("Durchlauf Nummer " + i + ".");
                com.shortGcodeBefehl("G0 X100");
                com.shortGcodeBefehl("G0 X0");

                Thread.Sleep(0);
            }
        }
        */

        bool firstErweiterteAnsicht = false;
        private void erweiterteAnsicht(bool erweitert)
        {
            if (erweitert)
            {
                grid_GBefehle.Visibility = System.Windows.Visibility.Visible;
                grid_ListBoxSteuerelemente.Visibility = System.Windows.Visibility.Visible;
                listBox_ereignis.Visibility = System.Windows.Visibility.Visible;
                //label_baud.Visibility = System.Windows.Visibility.Visible;
                //textBox_baud.Visibility = System.Windows.Visibility.Visible;
                button_erweiterteAnsicht.Foreground = Brushes.Green;

                button_editorWindow.Visibility = System.Windows.Visibility.Visible;
                button_ablaufBearbeiten.Visibility = System.Windows.Visibility.Visible;
                button_ablaufUP.Visibility = System.Windows.Visibility.Visible;
                button_ablaufDOWN.Visibility = System.Windows.Visibility.Visible;
                button_ablaufDelete.Visibility = System.Windows.Visibility.Visible;
                listBox_Abläufe.Margin = new Thickness(0, 83, 0, 41);

                if (!firstErweiterteAnsicht)
                    expander_ablauf.IsExpanded = true;


                isErweiterteAnsicht = true;
                firstErweiterteAnsicht = true;
                this.MinHeight = 385;
            }
            else
            {
                grid_GBefehle.Visibility = System.Windows.Visibility.Collapsed;
                grid_ListBoxSteuerelemente.Visibility = System.Windows.Visibility.Collapsed;
                listBox_ereignis.Visibility = System.Windows.Visibility.Collapsed;
                //label_baud.Visibility = System.Windows.Visibility.Collapsed;
                //textBox_baud.Visibility = System.Windows.Visibility.Collapsed;
                button_erweiterteAnsicht.Foreground = Brushes.DarkRed;

                button_editorWindow.Visibility = System.Windows.Visibility.Collapsed;
                button_ablaufBearbeiten.Visibility = System.Windows.Visibility.Collapsed;
                button_ablaufUP.Visibility = System.Windows.Visibility.Collapsed;
                button_ablaufDOWN.Visibility = System.Windows.Visibility.Collapsed;
                button_ablaufDelete.Visibility = System.Windows.Visibility.Collapsed;
                listBox_Abläufe.Margin = new Thickness(0, 0, 0, 0);

                isErweiterteAnsicht = false;
                this.MinHeight = 285;
                this.Height = 285;
            }

            menuCheckBox_erweiterteAnsicht.IsChecked = isErweiterteAnsicht;
        }

        bool isErweiterteAnsicht = true;
        private void button_erweiterteAnsicht_Click(object sender, RoutedEventArgs e)
        {
            if (isErweiterteAnsicht)
                erweiterteAnsicht(false);
            else
                erweiterteAnsicht(true);
        }

        private void menuCheckBox_erweiterteAnsicht_Checked(object sender, RoutedEventArgs e)
        {
            if (!isErweiterteAnsicht)
                erweiterteAnsicht(true);
        }

        private void menuCheckBox_erweiterteAnsicht_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isErweiterteAnsicht)
                erweiterteAnsicht(false);
        }

        private void MenuItem_Beenden_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_zurWebSite_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.tuncertec.de/");
        }

        private void MenuItem_überSlider_Click(object sender, RoutedEventArgs e)
        {
            string überSlider = "Slider v. 1.26 developed by\nTuncer Technik\nBrahmsstraße 22\n52477 Alsdorf\nGermany\nhttps://www.tuncertec.de\n\nMail: tuncer@tuncertec.de\nTel: 49 (0)176 47908301";
            MessageBox.Show(überSlider, "Über Slider");
        }

        private void MenuItem_Updates(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://tuncertec.de/software");
            //-
        }

        AutoConnect aC;
        private void button_AutoConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!com.isCOMMopen())
            {
                if (aC == null || !aC.isOpen())
                    aC = new AutoConnect(this);
                aC.Show();
                aC.Focus();
            }
        }

        static public string replacePunktZuKomma(string Wert)
        {
            return Wert.Trim().Replace('.', ',');
        }

        private void textBox_speed_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double value;
                if (double.TryParse(replacePunktZuKomma(textBox_speed.Text), out value))
                    slider_speed.Value = value;
            }
        }

        private void textBox_position_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double value;
                if (double.TryParse(replacePunktZuKomma(textBox_position.Text), out value))
                    slider_position.Value = value;

                if (menuCheckBox_FahreDirekt.IsChecked == true)
                    verFahre();
            }
        }

        bool shoudAskNullen = true;
        private bool askNullen()
        {
            if (!wasNullen && shoudAskNullen)
            {
                string frage = "Sie haben noch nicht genullt, möchten Sie jetzt \"Nullen\"?\nAnsonsten besteht das Risiko die Grenzen zu überfahren!";
                MessageBoxResult r1 = MessageBox.Show(frage, "Achtung", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (r1)
                {
                    case MessageBoxResult.No: shoudAskNullen = false; return true;
                    case MessageBoxResult.Yes: Nullen(); return false;
                    default: return false;
                }
            }
            else
                return true;
        }

        private void verFahre()
        {
            if (!askNullen())
                return;

            double value1;
            if (double.TryParse(replacePunktZuKomma(textBox_position.Text), out value1))
                slider_position.Value = value1;

            double value2;
            if (double.TryParse(replacePunktZuKomma(textBox_speed.Text), out value2))
                slider_speed.Value = value2;

            com.shortGcodeBefehl("G1 X" + value1.ToString() + " F" + value2.ToString());
        }

        private void button_fahren_Click(object sender, RoutedEventArgs e)
        {
            verFahre();
        }



        private void button_editorWindow_Click(object sender, RoutedEventArgs e)
        {
            neuerAblauf();
        }

        private void MenuItem_Bearbeiten_Click(object sender, RoutedEventArgs e)
        {
            neuerAblauf();
        }

        private void neuerAblauf()
        {
            if (editorWindow == null)
                editorWindow = new Slider.EditorWindow(this);
            else if (!editorWindow.IsVisible)
            {
                editorWindow = new Slider.EditorWindow(this);
            }

            editorWindow.Show();
            editorWindow.Focus();
        }

        private void bearbeiteAblauf(Ablauf ablauf)
        {
            if (editorWindow == null)
                editorWindow = new Slider.EditorWindow(this, ablauf);
            else if (!editorWindow.IsVisible)
            {
                editorWindow = new Slider.EditorWindow(this, ablauf);
            }

            editorWindow.Show();
            editorWindow.Focus();
        }

        private void expandiereEditorBoxen(bool expandieren)
        {
            if (expandieren)
                expansionsColumnAbläufe.Width = new GridLength(expansionsColumnAbläufe.MaxWidth);
            else
                expansionsColumnAbläufe.Width = new GridLength(0);
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            expandiereEditorBoxen(true);
            menuCheckBox_AblaufButtonsAnsicht.IsChecked = true;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            expandiereEditorBoxen(false);
            menuCheckBox_AblaufButtonsAnsicht.IsChecked = false;
        }

        //bool ablaufBearbeiten = false;
        private void button_ablaufBearbeiten_Click(object sender, RoutedEventArgs e)
        {
            /*
            if(!ablaufBearbeiten)
            {
                ablaufBearbeiten = true;
                button_ablaufBearbeiten.Background = Brushes.Blue;
            }
            else
            {
                ablaufBearbeiten = false;
                button_ablaufBearbeiten.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
            }
             */
            int select = listBox_Abläufe.SelectedIndex;
            if (select >= 0)
                bearbeiteAblauf(abläufe[select]);
        }

        private void button_ablaufUP_Click(object sender, RoutedEventArgs e)
        {
            int select = listBox_Abläufe.SelectedIndex;
            if (select > 0)
                abläufe.Move(select, select - 1);

            isAblaufÄnderungen = true;
        }

        private void button_ablaufDOWN_Click(object sender, RoutedEventArgs e)
        {
            int select = listBox_Abläufe.SelectedIndex;
            if (select >= 0 && select < listBox_Abläufe.Items.Count - 1)
                abläufe.Move(select, select + 1);

            isAblaufÄnderungen = true;
        }

        private void listBox_Abläufe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isErweiterteAnsicht)
                listBox_Abläufe.SelectedIndex = -1;
        }

        private void button_ablaufDelete_Click(object sender, RoutedEventArgs e)
        {
            int select = listBox_Abläufe.SelectedIndex;
            if (select >= 0)
                abläufe.RemoveAt(select);

            isAblaufÄnderungen = true;
        }

        public void ablaufAusführen(Ablauf ablauf)
        {
            if (!askNullen())
                return;

            listBox_Abläufe.IsEnabled = false;
            button_ablaufDelete.IsEnabled = false;

            foreach (Ablauf.Befehl b in ablauf.Befehle)
            {
                string gcode = b.getGcode();
                if (b.GetType() == typeof(Ablauf.ShootMoveShoot))
                {
                    if (gcode.IndexOf('T') < 0 && gcode.IndexOf('t') < 0)
                    {
                        if (menuCheckBox_ShootMitFokus.IsChecked == false)
                            gcode += " T0";
                    }
                }
                com.shortGcodeBefehl(gcode);
                Thread.Sleep(10);
            }

            button_ablaufDelete.IsEnabled = true;
            listBox_Abläufe.IsEnabled = true;

        }


        public void ablaufButtonsEnable(bool enable)
        {
            //listBox_Abläufe
            foreach (Ablauf a in abläufe)
            {
                a.isEnabled = enable;
                //a.Background = Brushes.Blue;
            }

            //Aktualisiere die listBox_Abläufe
            listBox_Abläufe.ItemsSource = null;
            listBox_Abläufe.ItemsSource = abläufe;

        }

        private static string savePfad = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Tuncer Technik\\Slider";
        private static string abläufeFileName = "eigeneButtons.xml";
        private static string settingsFileName = "settings.xml";
        public void saveAbläufe()
        {
            if (!System.IO.Directory.Exists(savePfad))
            {
                System.IO.Directory.CreateDirectory(savePfad);
            }
            ObjectFileConverter.SerializeObject<ObservableCollection<Ablauf>>(abläufe, savePfad + "\\" + abläufeFileName);
            isAblaufÄnderungen = false;
        }

        private void ladeAbläufe()
        {
            if (System.IO.File.Exists(savePfad + "\\" + abläufeFileName))
            {
                abläufe = ObjectFileConverter.DeSerializeObject<ObservableCollection<Ablauf>>(savePfad + "\\" + abläufeFileName);
                if (abläufe == null)
                    abläufe = new ObservableCollection<Ablauf>();
            }
            else
                abläufe = new ObservableCollection<Ablauf>();

            foreach (Ablauf a in abläufe)
            {
                a.setButtonClickFunktion(ablaufAusführen);
            }
        }
        //- Speichere an den richtigen Stellen

        Settings settings;

        public void saveSettings()
        {
            ObjectFileConverter.SerializeObject<Settings>(settings, savePfad + "\\" + settingsFileName);
        }

        private void ladeSettings()
        {
            if (System.IO.File.Exists(savePfad + "\\" + settingsFileName))
            {
                settings = ObjectFileConverter.DeSerializeObject<Settings>(savePfad + "\\" + settingsFileName);
                if (settings == null)
                    settings = new Settings();
                else
                {
                    foreach (string s in comboBox_com.Items)
                    {
                        if (s == settings.lastCOM)
                        {
                            comboBox_com.SelectedValue = settings.lastCOM;
                            break;
                        }
                    }
                }
            }
            else
                settings = new Settings();
        }

        private void menuCheckBox_AblaufButtonsAnsicht_Checked(object sender, RoutedEventArgs e)
        {
            expander_ablauf.IsExpanded = true;
        }

        private void menuCheckBox_AblaufButtonsAnsicht_Unchecked(object sender, RoutedEventArgs e)
        {
            expander_ablauf.IsExpanded = false;
        }

        private bool shootAusführen()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("M750");
            if (camParam.isShoot)
                sb.Append(" S" + (camParam.shoot * 1000).ToString());
            if (camParam.isFokus)
                sb.Append(" R" + (camParam.fokus * 1000).ToString());
            if (camParam.isStart)
                sb.Append(" I" + (camParam.start * 1000).ToString());
            if (camParam.isEnd)
                sb.Append(" J" + (camParam.end * 1000).ToString());
            if (!camParam.isShoot && !camParam.isFokus && !camParam.isStart && !camParam.isEnd)
            {
                MessageBox.Show("Keine gültigen Kameraparameter gewählt");
                return false;
            }

            com.shortGcodeBefehl(sb.ToString());
            return true;
        }

        private bool shootMoveShootAusführen()
        {
            if (!askNullen())
                return false;

            StringBuilder sb = new StringBuilder();
            sb.Append("M750");
            if (camParam.isShoot)
                sb.Append(" S" + (camParam.shoot * 1000).ToString());
            if (camParam.isFokus)
                sb.Append(" R" + (camParam.fokus * 1000).ToString());
            if (camParam.isStart)
                sb.Append(" I" + (camParam.start * 1000).ToString());
            if (camParam.isEnd)
                sb.Append(" J" + (camParam.end * 1000).ToString());
            if (!camParam.isShoot && !camParam.isFokus && !camParam.isStart && !camParam.isEnd)
            {
                MessageBox.Show("Keine gültigen Kameraparameter gewählt");
                return false;
            }
            if (camParam.isSpeed)
                sb.Append(" F" + camParam.speed.ToString());
            if (camParam.isPosition)
                sb.Append(" X" + camParam.position.ToString());
            if (camParam.isDx)
                sb.Append(" E" + camParam.dx.ToString());
            if (camParam.isPics)
                sb.Append(" P" + camParam.pics.ToString());

            string gcode = sb.ToString();
            if (gcode.IndexOf('T') < 0 && gcode.IndexOf('t') < 0)
            {
                if (menuCheckBox_ShootMitFokus.IsChecked == false)
                    gcode += " T0";
            }

            com.shortGcodeBefehl(gcode);
            return true;
        }


        public ShootParameterWindow shootParameterWindow;
        KameraParameter camParam = new KameraParameter();
        private void kameraSteuerungFenster()
        {
            camParam.setAllIsFalse();

            if (shootParameterWindow == null)
                shootParameterWindow = new ShootParameterWindow(shootAusführen, shootMoveShootAusführen, changeKamerasteuerung, camParam, true);
            else if (!shootParameterWindow.IsVisible)
            {
                shootParameterWindow = new ShootParameterWindow(shootAusführen, shootMoveShootAusführen, changeKamerasteuerung, camParam, true);
            }

            shootParameterWindow.Show();
            shootParameterWindow.WindowState = WindowState.Normal;
            shootParameterWindow.Focus();
        }

        private void button_KameraSteuerung_Click(object sender, RoutedEventArgs e)
        {
            kameraSteuerungFenster();
        }

        private bool changeKamerasteuerung(double position, bool isposition, double speed, bool isspeed)
        {
            if (isposition)
                slider_position.Value = position;
            if (isspeed)
                slider_speed.Value = speed;
            return true;
        }
    }
}


// TODO: System.ArgumentOutOfRangeException behandeln, wenn keine Verbindung aufgebaut werden konnte erschien es zumindest einmal
// TODO: Shoot and Move nur sichtbar wenn die Version passt