using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Slider
{
    public class Ablauf
    {
        public delegate void ButtonClickFunktion(Ablauf ablauf);
        private ButtonClickFunktion buttonClickFunktion;


        public Ablauf()
        {
            buttonClickFunktion = null;
            Befehle = new ObservableCollection<Befehl>();

        }

        public void setButtonClickFunktion(ButtonClickFunktion funktion)
        {
            buttonClickFunktion = funktion;
        }

        public Ablauf(ButtonClickFunktion funktion)
        {
            buttonClickFunktion = funktion;
            Befehle = new ObservableCollection<Befehl>();
        }

        public Ablauf(Ablauf ablauf, ButtonClickFunktion funktion)
        {
            buttonClickFunktion = funktion;
            Befehle = new ObservableCollection<Befehl>(ablauf.Befehle);
            Name = ablauf.Name;
        }

        public ObservableCollection<Befehl> Befehle = new ObservableCollection<Befehl>();


        [XmlInclude(typeof(Position))]
        [XmlInclude(typeof(Pause))]
        [XmlInclude(typeof(Shoot))]
        [XmlInclude(typeof(ShootMoveShoot))]
        [XmlInclude(typeof(Nullen))]
        [XmlInclude(typeof(MotorenAus))]
        [XmlInclude(typeof(Brush))]
        [XmlInclude(typeof(SolidColorBrush))]
        public class Befehl
        {
            public void setVar(double var_, int index)
            {
                var[index] = var_;
            }

            public string anzeige
            {
                get
                {
                    _details = getDetails();

                    if (_name.Length == 0)
                        return getGcode();
                    else if (_details.Length == 0)
                        return _name;
                    else
                        return _name + ": " + _details;
                }
            }

            public virtual string getDetails()
            {
                return _details;
            }

            public virtual string getGcode()
            {
                return "virtual";
            }

            [XmlIgnore()]
            public Brush color
            {
                get { return new SolidColorBrush(FillColor); }
                set { FillColor = (value as SolidColorBrush).Color; }
            }

            public Color FillColor = Colors.Black;
            protected string _name = "";
            public string getName()
            {
                return _name;
            }

            private string _details = "";

            public double[] var;
        }


        public class Pause : Befehl
        {
            public Pause()
            {
                FillColor = Colors.Blue;
                _name = "Pause";
                var = new double[1];
            }

            public override string getDetails()
            {
                return var[0].ToString() + "s";
            }

            public override string getGcode()
            {
                int S = (int)var[0]; int P = (int)((var[0] - (double)S) * 1000);
                String p = "", s = "";
                if (P > 0)
                    p = " P" + P.ToString();    // zusätzliche Millisekunden
                if (S > 0)
                    s = " S" + S.ToString();

                return "G4" + p + s;
            }
        }

        public class Position : Befehl
        {
            public Position()
            {
                FillColor = Colors.Black;
                _name = "Fahre auf";
                var = new double[2];
            }

            public override string getDetails()
            {
                return var[0].ToString() + "mm mit " + var[1].ToString() + "mm/min";
            }

            public override string getGcode()
            {
                return "G1 X" + var[0].ToString() + " F" + var[1].ToString();
            }
        }

        public class Shoot : Befehl
        {
            public Shoot()
            {
                FillColor = Colors.Red;
                _name = "Shoot";
                var = new double[4];
            }

            public override string getDetails()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(var[0].ToString() + "ms ");
                if (var[1] > 0)
                    sb.Append("Fokus: " + var[1].ToString() + "ms ");
                if (var[2] > 0 || var[3] > 0)
                    sb.Append("| ");
                if (var[2] > 0)
                    sb.Append("Vor: " + var[2].ToString() + "ms ");
                if (var[3] > 0)
                    sb.Append("Nach: " + var[3].ToString() + "ms ");
                return sb.ToString();
            }

            public override string getGcode()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("M750");
                if (var[0] > 0)
                    sb.Append(" S" + var[0].ToString());
                if (var[1] > 0)
                    sb.Append(" R" + var[1].ToString());
                if (var[2] > 0)
                    sb.Append(" I" + var[2].ToString());
                if (var[3] > 0)
                    sb.Append(" J" + var[3].ToString());

                return sb.ToString();
            }
        }

        public class ShootMoveShoot : Befehl
        {
            public ShootMoveShoot()
            {
                FillColor = Colors.BlueViolet;
                _name = "ShootMoveShoot";
                var = new double[8];
            }

            public override string getDetails()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\n Shoot: " + var[0].ToString() + "ms ");
                if (var[1] > 0)
                    sb.Append("Fokus: " + var[1].ToString() + "ms ");
                if (var[2] > 0 || var[3] > 0)
                    sb.Append("| ");
                if (var[2] > 0)
                    sb.Append("Vor: " + var[2].ToString() + "ms ");
                if (var[3] > 0)
                    sb.Append("Nach: " + var[3].ToString() + "ms ");

                if (var[4] > 0 || var[5] >= 0 || var[6] != 0 || var[7] > 0)
                    sb.Append("\n Move: ");
                if (var[7] > 0)
                    sb.Append("Anzahl: " + var[7].ToString() + "Auslöser ");
                if (var[6] != 0)
                    sb.Append("Wegdifferenz: " + var[6].ToString() + "mm ");
                if (var[4] > 0)
                    sb.Append("Speed: " + var[4].ToString() + "mm/min ");
                if (var[5] >= 0)
                    sb.Append("Zielposition: " + var[5].ToString() + "mm ");


                return sb.ToString();
            }

            public override string getGcode()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("M750");
                if (var[0] > 0)
                    sb.Append(" S" + var[0].ToString());
                if (var[1] > 0)
                    sb.Append(" R" + var[1].ToString());
                if (var[2] > 0)
                    sb.Append(" I" + var[2].ToString());
                if (var[3] > 0)
                    sb.Append(" J" + var[3].ToString());
                if (var[4] > 0)
                    sb.Append(" F" + var[4].ToString());
                if (var[5] >= 0)
                    sb.Append(" X" + var[5].ToString());
                if (var[6] != 0)
                    sb.Append(" E" + var[6].ToString());
                if (var[7] > 0)
                    sb.Append(" P" + var[7].ToString());

                return sb.ToString();
            }
        }

        public class Nullen : Befehl
        {
            public Nullen()
            {
                FillColor = Colors.Green;
                _name = "Nullen";
            }

            public override string getGcode()
            {
                return "G28 X0";
            }
        }

        public class MotorenAus : Befehl
        {
            public MotorenAus()
            {
                FillColor = Colors.DarkSlateGray;
                _name = "Motoren ausschalten";
            }

            public override string getGcode()
            {
                return "M84";
            }
        }

        public void addPause(double Time)
        {
            Pause p = new Pause();
            p.setVar(Time, 0);
            Befehle.Add(p);
        }

        public void addShoot(double shoot, double fokus, double start, double end)
        {
            Shoot p = new Shoot();
            p.setVar(shoot, 0);
            p.setVar(fokus, 1);
            p.setVar(start, 2);
            p.setVar(end, 3);

            Befehle.Add(p);
        }

        public void addShootMoveShoot(double shoot, double fokus, double start, double end, double speed, double position, double dx, double pics)
        {
            ShootMoveShoot p = new ShootMoveShoot();
            p.setVar(shoot, 0);
            p.setVar(fokus, 1);
            p.setVar(start, 2);
            p.setVar(end, 3);
            p.setVar(speed, 4);
            p.setVar(position, 5);
            p.setVar(dx, 6);
            p.setVar(pics, 7);
            Befehle.Add(p);
        }

        public void addMotorenAus()
        {
            Befehle.Add(new MotorenAus());
        }

        public void addNullen()
        {
            Befehle.Add(new Nullen());
        }

        public void addPosition(double posi, double speed)
        {
            Position p = new Position();
            p.setVar(posi, 0);
            p.setVar(speed, 1);
            Befehle.Add(p);
        }

        public void RemoveAt(int index)
        {
            Befehle.RemoveAt(index);
        }

        public void Move(int oldIndex, int newIndex)
        {
            Befehle.Move(oldIndex, newIndex);
        }

        private string _name;
        public string Name { get { return _name; } set { _name = value; } }

        private class ActionCommand : ICommand
        {
            private readonly Action _action;

            public ActionCommand(Action action)
            {
                _action = action;
            }

            public void Execute(object parameter)
            {
                _action();
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

#pragma warning disable
            public event EventHandler CanExecuteChanged;
#pragma warning restore
        }

        //  Wenn der jeweilige Button gedrückt wird:
        private ICommand someCommand;
        public ICommand SomeCommand
        {
            get
            {
                return someCommand ?? (someCommand = new ActionCommand(() =>
                    {
                        if (buttonClickFunktion != null)
                            buttonClickFunktion(this);
                    }));
            }
        }

        [XmlIgnore()]
        public bool isEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        private bool _isEnabled = true;

        /*
        public Brush Background
        {
            get { return _Background; }
            set { _Background = value; }
        }
        private Brush _Background = Brushes.Red;*/

    }
}
