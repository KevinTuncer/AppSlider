using Kamera_Linearführung;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Slider
{
    /// <summary>
    /// Interaktionslogik für EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        Ablauf ablauf;
        Ablauf oldAblauf;
        MainWindow m;
        bool isBearbeiten;
        KameraParameter camParam = new KameraParameter();

        public EditorWindow(MainWindow M)
        {
            m = M;
            isBearbeiten = false;
            ablauf = new Ablauf(m.ablaufAusführen);
            InitializeComponent();
            listBox_Editor.ItemsSource = ablauf.Befehle;
        }

        public EditorWindow(MainWindow M, Ablauf _ablauf)
        {
            m = M;
            isBearbeiten = true;
            InitializeComponent();

            oldAblauf = _ablauf;
            ablauf = new Ablauf(_ablauf, m.ablaufAusführen);

            textBox_name.Text = ablauf.Name;

            listBox_Editor.ItemsSource = ablauf.Befehle;
        }


        private void textBox_speed_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double value;
                if (double.TryParse(MainWindow.replacePunktZuKomma(textBox_speed.Text), out value))
                    slider_speed.Value = value;
            }
        }

        private void textBox_position_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double value;
                if (double.TryParse(MainWindow.replacePunktZuKomma(textBox_position.Text), out value))
                    slider_position.Value = value;

                //if (checkBox_FahreDirekt.IsChecked == true)
                //    verFahre();
            }
        }

        private void textBox_pause_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void slider_speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBox_speed.Text = m.Runden(e.NewValue).ToString();
            //ZeigeVorraussichtlicheVerfahrZeitAn();
        }

        private void slider_position_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBox_position.Text = m.Runden(e.NewValue).ToString();
            //ZeigeVorraussichtlicheVerfahrZeitAn();
        }

        private void slider_position_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void button_nullen_Click(object sender, RoutedEventArgs e)
        {
            ablauf.addNullen(); ;
        }

        private void button_MotorenPower_Click(object sender, RoutedEventArgs e)
        {
            ablauf.addMotorenAus();
        }

        private void Button_PauseAdd_Click(object sender, RoutedEventArgs e)
        {
            double value1;
            if (double.TryParse(MainWindow.replacePunktZuKomma(textBox_pause.Text), out value1))
                ablauf.addPause(Math.Round(value1, 4));
        }

        private void Button_PosSpeedAdd_Click(object sender, RoutedEventArgs e)
        {
            double value1;
            if (double.TryParse(MainWindow.replacePunktZuKomma(textBox_position.Text), out value1))
                slider_position.Value = value1;

            double value2;
            if (double.TryParse(MainWindow.replacePunktZuKomma(textBox_speed.Text), out value2))
                slider_speed.Value = value2;

            ablauf.addPosition(value1, value2);
        }

        private void button_delete_Click(object sender, RoutedEventArgs e)
        {
            int select = listBox_Editor.SelectedIndex;
            if (select >= 0)
                ablauf.RemoveAt(select);
        }

        private void button_up_Click(object sender, RoutedEventArgs e)
        {
            int select = listBox_Editor.SelectedIndex;
            if (select > 0)
                ablauf.Move(select, select - 1);
        }

        private void button_down_Click(object sender, RoutedEventArgs e)
        {
            int select = listBox_Editor.SelectedIndex;
            if (select >= 0 && select < listBox_Editor.Items.Count - 1)
                ablauf.Move(select, select + 1);
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            ablauf.isEnabled = m.button_fahren.IsEnabled;   // ob der hinzugefügte Button Enabled erstellt hinzugefügt werden soll oder nicht wird am button_fahren abgelesen 

            ablauf.Name = textBox_name.Text.Trim();
            if (!isBearbeiten && ablauf.Befehle.Count > 0) // wenn keine Befehle definiert sind wird kein Ablauf erstellt
            {
                m.abläufe.Add(ablauf);
                m.isAblaufÄnderungen = true;
            }
            else
            {
                int index = m.abläufe.IndexOf(oldAblauf);
                m.abläufe.Remove(oldAblauf);
                if (ablauf.Befehle.Count > 0) // wenn keine Befehle definiert sind wird der Ablauf auch nicht neu angelegt sondern nur entfernt 
                    m.abläufe.Insert(index, ablauf);

                m.isAblaufÄnderungen = true;
            }

            //- starte einen thread der nach einer gewissen Zeit abspeichert solange keine neue änderung vorgenommen wurde 

            this.Close();
        }


        public bool shootAdd()
        {
            if (!camParam.isShoot) camParam.shoot = 0;
            if (!camParam.isFokus) camParam.fokus = 0;
            if (!camParam.isStart) camParam.start = 0;
            if (!camParam.isEnd) camParam.end = 0;
            if (camParam.shoot == 0 && camParam.fokus == 0 && camParam.start == 0 && camParam.end == 0)
                return false;
            ablauf.addShoot(camParam.shoot * 1000, camParam.fokus * 1000, camParam.start * 1000, camParam.end * 1000);

            return true;
        }

        public bool shootMoveShootAdd()
        {
            if (!camParam.isShoot) camParam.shoot = 0;
            if (!camParam.isFokus) camParam.fokus = 0;
            if (!camParam.isStart) camParam.start = 0;
            if (!camParam.isEnd) camParam.end = 0;

            if (!camParam.isSpeed) camParam.speed = 0;
            if (!camParam.isPosition) camParam.position = -1;
            if (!camParam.isDx) camParam.dx = 0;
            if (!camParam.isPics) camParam.pics = 0;

            if (camParam.shoot == 0 && camParam.fokus == 0 && camParam.start == 0 && camParam.end == 0)
                return false;

            if (camParam.speed == 0 && camParam.position == -1 && camParam.dx == 0 && camParam.pics == 0)
                ablauf.addShoot(camParam.shoot * 1000, camParam.fokus * 1000, camParam.start * 1000, camParam.end * 1000);
            ablauf.addShootMoveShoot(camParam.shoot * 1000, camParam.fokus * 1000, camParam.start * 1000, camParam.end * 1000, camParam.speed, camParam.position, camParam.dx, camParam.pics);
            return true;
        }

        public ShootParameterWindow shootParameterWindow;

        private void button_KameraSteuerung_Click(object sender, RoutedEventArgs e)
        {
            camParam.setAllIsFalse();

            if (shootParameterWindow == null)
                shootParameterWindow = new ShootParameterWindow(shootAdd, shootMoveShootAdd, changeKamerasteuerung, camParam, false);
            else if (!shootParameterWindow.IsVisible)
            {
                shootParameterWindow = new ShootParameterWindow(shootAdd, shootMoveShootAdd, changeKamerasteuerung, camParam, false);
            }

            shootParameterWindow.Show();
            shootParameterWindow.Focus();
        }

        private bool changeKamerasteuerung(double position, bool isposition, double speed, bool isspeed)
        {
            /*if (isposition)
                slider_position.Value = position;
            if (isspeed)
                slider_position.Value = speed;*/
            return true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // prüfe ob das ShootParameter-Fenster geschlossen wurde und schließe es wenn nicht
            if (shootParameterWindow != null && shootParameterWindow.IsInitialized)
                shootParameterWindow.Close();
        }

        private void listBoxEditorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Übergebe Anfangsparameter
            if (listBox_Editor.SelectedIndex >= 0)   // entweder aus selektierter Listbox
            {
                Ablauf.Befehl befehl = ((Ablauf.Befehl)listBox_Editor.SelectedItem);

                if (befehl.GetType() == typeof(Ablauf.Shoot) || befehl.GetType() == typeof(Ablauf.ShootMoveShoot))
                {
                    if (befehl.var[0] > 0)
                        camParam.shoot = befehl.var[0] / 1000;
                    if (befehl.var[1] > 0)
                        camParam.fokus = befehl.var[1] / 1000;
                    if (befehl.var[2] > 0)
                        camParam.start = befehl.var[2] / 1000;
                    if (befehl.var[3] > 0)
                        camParam.end = befehl.var[3] / 1000;
                    if (befehl.GetType() == typeof(Ablauf.ShootMoveShoot))
                    {
                        if (befehl.var[4] > 0)
                        {
                            camParam.speed = befehl.var[4];
                        }
                        if (befehl.var[5] >= 0)
                        {
                            camParam.position = befehl.var[5];
                        }
                        if (befehl.var[6] != 0)
                        {
                            camParam.dx = befehl.var[6];
                        }
                        if (befehl.var[7] > 0)
                        {
                            camParam.pics = befehl.var[7];
                        }
                    }
                }
            }
        }


    }
}
