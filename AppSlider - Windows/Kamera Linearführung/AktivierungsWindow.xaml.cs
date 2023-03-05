using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Kamera_Linearführung
{
    /// <summary>
    /// Interaktionslogik für AktivierungsWindow.xaml
    /// </summary>
    public partial class AktivierungsWindow : Window
    {
        MainWindow mw;
        public AktivierungsWindow(MainWindow mainWindow)
        {
            mw = mainWindow;
            InitializeComponent();

            FehlerMeldungEntfernen();

            thisIsEnabled(true);
            this.Topmost = true;
        }

        private void FehlerMeldungEntfernen()
        {
            textBlock_error.Text = "";
        }

        private void FehlerMeldung(string Fehler)
        {
            textBlock_error.Text = Fehler;
        }

        private string koregiereString(string s)
        {
            List<char> cl = new List<char>();
            foreach (char c in s)
            {
                if ("0123456789qwertzuiopasdfghjklyxcvbnmQWERTZUIOPASDFGHJKLYXCVBNM".IndexOf(c) >= 0)
                    cl.Add(c);
            }
            return new string(cl.ToArray());
        }

        private void thisIsEnabled(bool enabled)
        {
            if (enabled)
                label_wirdüberprüft.Visibility = System.Windows.Visibility.Hidden;
            else
                label_wirdüberprüft.Visibility = System.Windows.Visibility.Visible;
            button_aktivieren.IsEnabled = enabled;
            button_cancel.IsEnabled = enabled;
        }

        private lizenz.Rückmeldung rückmeldung = lizenz.Rückmeldung.FalscherKey;

        private void button_aktivieren_Click(object sender, RoutedEventArgs e)
        {
            if (rückmeldung == lizenz.Rückmeldung.AllesOK)
                this.Close();

            FehlerMeldungEntfernen();

            string s1 = koregiereString(textBox_aktivierung1.Text);
            string s2 = koregiereString(textBox_aktivierung2.Text);
            string s3 = koregiereString(textBox_aktivierung3.Text);

            textBox_aktivierung1.Text = s1;
            textBox_aktivierung2.Text = s2;
            textBox_aktivierung3.Text = s3;

            string skey = s1 + s2 + s3;
            if (skey.Length != 12)
            {
                FehlerMeldung("Die Eingabe war Fehlerhaft");
            }
            else
            {
                char[] ckey = skey.ToCharArray();

                thisIsEnabled(false);

                rückmeldung = lizenz.prüfeKey(ckey);
                switch (rückmeldung)
                {
                    case lizenz.Rückmeldung.AllesOK:
                        //-
                        textBlock_error.Foreground = Brushes.Green;
                        button_cancel.Visibility = System.Windows.Visibility.Hidden;
                        button_aktivieren.Content = "Weiter";
                        FehlerMeldung("Die Software wurde erfolgkreich aktiviert");

                        break;
                    case lizenz.Rückmeldung.FalscherKey:
                        FehlerMeldung("Geben Sie bitte einen gültigen Key ein");
                        break;
                    case lizenz.Rückmeldung.OnlineFehlgeschlagen:
                        FehlerMeldung("Aktivierung fehlgeschlagen!");
                        //-
                        break;
                }

                thisIsEnabled(true);
            }
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            switch (rückmeldung)
            {
                case lizenz.Rückmeldung.AllesOK: mw.prüfeAktivierung(); return;
                default: break;
            }
            if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsActive)
                Application.Current.MainWindow.Close();
        }
    }
}
