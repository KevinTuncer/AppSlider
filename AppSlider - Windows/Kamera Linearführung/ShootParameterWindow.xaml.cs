using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Slider
{
    /// <summary>
    /// Interaction logic for ShootParameterWindow.xaml
    /// </summary>
    public partial class ShootParameterWindow : Window
    {
        KameraParameter kp;

        private Func<bool> buttonShootClick;
        private Func<bool> buttonShootMoveShootClick;
        Func<double, bool, double, bool, bool> changeValue;


        bool isAusführenAnsicht = false;
        public ShootParameterWindow(Func<bool> ButtonShootClick, Func<bool> ButtonShootMoveShootClick, Func<double, bool, double, bool, bool> changeKamerasteuerung, KameraParameter camParam, bool ausführenAnsicht)
        {
            buttonShootClick = ButtonShootClick;
            buttonShootMoveShootClick = ButtonShootMoveShootClick;
            InitializeComponent();
            kp = camParam;
            isAusführenAnsicht = ausführenAnsicht;
            changeValue = changeKamerasteuerung;

            // Übergebe Anfangsparameter aus letzter Zuweisung
            if (kp.shoot >= 0)
                textBox_shoot.Text = kp.shoot.ToString();
            if (kp.fokus >= 0)
                textBox_focus.Text = kp.fokus.ToString();
            if (kp.start >= 0)
                textBox_startVerzögerung.Text = kp.start.ToString();
            if (kp.end >= 0)
                textBox_nachVerzögerung.Text = kp.end.ToString();

            if (kp.speed > 0)
            {
                textBox_speed.Text = kp.speed.ToString();
                checkBox_speed.IsChecked = true;
            }
            else
                checkBox_speed.IsChecked = false;

            if (kp.position >= 0)
            {
                textBox_position.Text = kp.position.ToString();
                unSelectLastradiobutton(0);
            }
            if (kp.dx != 0)
            {
                textBox_dx.Text = kp.dx.ToString();
                unSelectLastradiobutton(1);
            }
            if (kp.pics > 0)
            {
                textBox_pics.Text = kp.pics.ToString();
                unSelectLastradiobutton(2);
            }

            if (ausführenAnsicht)
            {
                Button_Shoot_Add.Content = "Ausführen";
                Button_ShootAndMove_Add.Content = "Ausführen";
            }
            else
            {
                Button_Shoot_Add.Content = "Add";
                Button_ShootAndMove_Add.Content = "Add";
            }
        }


        private string replaceNoNumber(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                if ("0123456789,.+-".IndexOf(c) >= 0)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private bool textBox_getValue(TextBox tb, out double value)
        {
            if (double.TryParse(Kamera_Linearführung.MainWindow.replacePunktZuKomma(tb.Text), out value))
            {
                value = Math.Round(value, 4);
                tb.Text = value.ToString();
                return true;
            }

            return false;
        }

        private void berechneDrittenParameter()
        {
            // lese Bewegungsparameter aus und korigiere sie in der jeweiligen Textbox 
            double dx, position, pics;
            if (!textBox_getValue(textBox_position, out position))
                position = 0;
            if (!textBox_getValue(textBox_dx, out dx))
                dx = 0;
            if (!textBox_getValue(textBox_pics, out pics))
                pics = 0;

            // erhale letzte Position
            double oldX = 0;
            if (isAusführenAnsicht)
            {
                oldX = Status.PositionX;
            }
            else
            {
                oldX = 0;
            }

            // Berechne den nichtmarkierten Parameter
            if (radioButton_position.IsChecked == true)
                if (radioButton_pics.IsChecked == true)
                {
                    if (pics - 1 != 0)
                        textBox_dx.Text = Math.Round(Math.Abs(position - oldX) / (pics - 1), 4).ToString();
                    else
                        textBox_dx.Text = "0";
                }
                else
                {
                    if (dx != 0)
                        textBox_pics.Text = Math.Round(Math.Abs((position - oldX) / dx) + 1, 4).ToString();
                    else
                        textBox_pics.Text = "0";
                }
            else
                textBox_position.Text = Math.Round((dx * (pics - 1)) + oldX, 4).ToString();

            berechneGesamteFahrZeit();
        }

        private void berechneGesamteFahrZeit()
        {
            getShootParameter();
            getBewegungsParameter();

            double dx, speed, endPosition;
            bool isSpeed = false;
            if (!textBox_getValue(textBox_dx, out dx))
                dx = 0;
            if (!textBox_getValue(textBox_position, out endPosition))
                endPosition = 0;

            if (kp.isSpeed)
            {
                speed = kp.speed;
                isSpeed = true;
            }
            else
            {
                if (isAusführenAnsicht)
                {
                    speed = Slider.Status.Speed;
                    isSpeed = true;
                }
                else
                    speed = 0;  //- muss vorherige Geschwindigkeiten im Editormodus analisieren
            }

            if (isSpeed)
            {
                double auslöserZeit = 0;
                if (kp.isFokus) auslöserZeit += kp.fokus;
                if (kp.isShoot) auslöserZeit += kp.shoot;
                if (kp.isStart) auslöserZeit += kp.start;
                if (kp.isEnd) auslöserZeit += kp.end;

                double bewegungsTime;
                if (dx != 0)
                {
                    double count = Math.Abs((endPosition - Slider.Status.PositionX) / dx);
                    bewegungsTime = Status.getFahrZeit(Slider.Status.PositionX + dx, speed);
                    bewegungsTime *= count;
                    auslöserZeit *= count + 1;
                }
                else
                    bewegungsTime = Status.getFahrZeit(Slider.Status.PositionX + dx, speed);

                lable_gesamtDauer.Content = Status.timeToString(bewegungsTime + auslöserZeit, false);
            }
            else
                lable_gesamtDauer.Content = Status.timeToString(0, false);
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (e.Key == Key.Enter)
            {
                tb.Text = replaceNoNumber(tb.Text);

                berechneDrittenParameter();
            }
        }

        private void textBox_speed_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                textBox_speed.Text = replaceNoNumber(textBox_speed.Text);
                double value;
                if (textBox_getValue(textBox_speed, out value))
                    changeValue(0, false, value, true);

                berechneDrittenParameter();
            }
        }

        private void textBox_position_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                textBox_position.Text = replaceNoNumber(textBox_position.Text);
                double value;
                if (textBox_getValue(textBox_position, out value))
                    changeValue(value, true, 0, false);

                berechneDrittenParameter();
            }
        }

        private void getShootParameter()
        {
            if (textBox_getValue(textBox_focus, out kp.fokus) && kp.fokus > 0)
                kp.isFokus = true;
            else
                kp.isFokus = false;
            if (textBox_getValue(textBox_shoot, out kp.shoot) && kp.shoot > 0)
                kp.isShoot = true;
            else
                kp.isShoot = false;
            if (textBox_getValue(textBox_startVerzögerung, out kp.start) && kp.start > 0)
                kp.isStart = true;
            else
                kp.isStart = false;
            if (textBox_getValue(textBox_nachVerzögerung, out kp.end) && kp.end > 0)
                kp.isEnd = true;
            else
                kp.isEnd = false;
        }

        private void getBewegungsParameter()
        {
            kp.isSpeed = false;
            if (checkBox_speed.IsChecked == true)
                if (textBox_getValue(textBox_speed, out kp.speed) && kp.speed > 0)
                    kp.isSpeed = true;

            kp.isPosition = false;
            if (radioButton_position.IsChecked == true)
                if (textBox_getValue(textBox_position, out kp.position) && kp.position >= 0)
                    kp.isPosition = true;

            kp.isDx = false;
            if (radioButton_dx.IsChecked == true)
                if (textBox_getValue(textBox_dx, out kp.dx) && kp.dx > 0)
                    kp.isDx = true;

            kp.isPics = false;
            if (radioButton_pics.IsChecked == true)
                if (textBox_getValue(textBox_pics, out kp.pics) && kp.pics > 0)
                    kp.isPics = true;
        }

        private void Button_Shoot_Add_Click(object sender, RoutedEventArgs e)
        {
            getShootParameter();
            buttonShootClick();
            berechneDrittenParameter();
            if (!isAusführenAnsicht)
                this.Close();
        }

        private void Button_ShootAndMove_Add_Click(object sender, RoutedEventArgs e)
        {
            getShootParameter();
            getBewegungsParameter();
            berechneDrittenParameter();
            buttonShootMoveShootClick();
            if (!isAusführenAnsicht)
                this.Close();
        }

        private void select(int index)
        {
            switch (index)
            {
                case 0: radioButton_position.IsChecked = true; textBox_position.IsEnabled = true; break;
                case 1: radioButton_dx.IsChecked = true; textBox_dx.IsEnabled = true; break;
                case 2: radioButton_pics.IsChecked = true; textBox_pics.IsEnabled = true; break;
            }
        }

        private void unSelect(int index)
        {
            switch (index)
            {
                case 0: radioButton_position.IsChecked = false; textBox_position.IsEnabled = false; break;
                case 1: radioButton_dx.IsChecked = false; textBox_dx.IsEnabled = false; break;
                case 2: radioButton_pics.IsChecked = false; textBox_pics.IsEnabled = false; break;
            }
        }

        private int[] radioButtonIndex = new int[3] { 0, 0, 0 };
        private void unSelectLastradiobutton(int selected)
        {
            select(selected);
            radioButtonIndex[selected] = 0;
            int[] last = new int[2];
            int k = 0;
            for (int i = 0; i < radioButtonIndex.Length; ++i)
                if (i != selected)
                {
                    radioButtonIndex[i]++;
                    last[k] = i;
                    k++;
                }
            if (radioButton_position.IsChecked == true && radioButton_dx.IsChecked == true && radioButton_pics.IsChecked == true)
            {
                if (radioButtonIndex[last[0]] > radioButtonIndex[last[1]])
                    unSelect(last[0]);
                else
                    unSelect(last[1]);
            }
        }
        private void radioButton_position_Click(object sender, RoutedEventArgs e)
        {
            unSelectLastradiobutton(0);
        }

        private void radioButton_dx_Click(object sender, RoutedEventArgs e)
        {
            unSelectLastradiobutton(1);
        }

        private void radioButton_pics_Click(object sender, RoutedEventArgs e)
        {
            unSelectLastradiobutton(2);
        }

        private void checkBox_speed_Click(object sender, RoutedEventArgs e)
        {
            textBox_speed.IsEnabled = (bool)checkBox_speed.IsChecked;
        }
    }
}
