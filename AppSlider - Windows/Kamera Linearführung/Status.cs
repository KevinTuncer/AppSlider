using Kamera_Linearführung;
using System;
using System.Text;

namespace Slider
{
    static class Status
    {
        public static double PositionX = 0;                 // in mm
        public static double Speed = 1;                     // in mm / s
        public static double HomingSpeedX = 300;            // in mm / s
        public static double BeschleunigungX = 1000;        // in mm / s^2

        public static double getFahrZeit(double xPos, double speed)   // Postition in mm und Speed in mm/min, Ergebniss in Sekunden
        {
            if (speed == 0)
                return 0;

            // Beschleunigung bei 1000 mm/s²
            double a = Status.BeschleunigungX;
            double speedS = Math.Abs(speed) / 60;

            double aT = speedS / a; // Anfahr-/ bzw. Abfahrzeit
            double aS = (a * aT * aT) / 2; // Stecke bis nicht mehr beschleunigt wird

            double deltaWeg = Math.Abs(Status.PositionX - xPos);

            if (2 * aS >= deltaWeg)    // falls die Strecke so kurz ist dass es keine unbeschleunigte Phase gibt 
            {
                //MessageBox.Show(Math.Sqrt(deltaWeg / a) + " " + 2*aS + " " +deltaWeg);
                return Math.Sqrt(deltaWeg / a) * 2;
            }

            return ((deltaWeg - (2 * aS)) / speedS) + (2 * aT);

        }

        public static string timeToString(double time, bool mitEnter)
        {
            TimeSpan ts = TimeSpan.FromSeconds(time);
            StringBuilder sb = new StringBuilder();
            sb.Append(ts.ToString(@"hh\:mm\:ss"));
            if (mitEnter)
                sb.Append("\n");
            sb.Append(ts.ToString(@"\.fff"));
            return sb.ToString();
        }

        public static bool newPositionFromMessage(Message msg, out double positionX)
        {
            if (msg.isG)
                switch (msg.G)
                {
                    case 0:
                    case 1:
                        if (msg.isX)
                        {
                            positionX = msg.X;
                            return true;
                        }
                        break;
                    case 28:    // Nullen
                        if (msg.isX)
                        {
                            positionX = 0;
                            return true;
                        }
                        else if (!msg.isY && !msg.isZ)
                        {
                            positionX = 0;
                            return true;
                        }
                        break;
                }
            if (msg.isM && msg.M == 750) // ShootMoveShoot
            {
                if (msg.isX)
                {
                    positionX = msg.X;
                    return true;
                }
                else if (msg.isE && msg.isP)
                {
                    positionX = msg.E * (msg.P - 1);
                    return true;
                }
            }
            positionX = 0;
            return false;
        }

        public static bool newSpeedFromMessage(Message msg, out double Speed)
        {
            if (msg.isG)
                switch (msg.G)
                {
                    case 0:
                    case 1:
                        if (msg.isF)
                        {
                            Speed = msg.F;
                            return true;
                        }
                        break;
                    case 28:    // Nullen
                        Speed = HomingSpeedX;
                        break;
                }
            if (msg.isM && msg.M == 750) // ShootMoveShoot
            {
                if (msg.isF)
                {
                    Speed = msg.F;
                    return true;
                }
            }
            Speed = 0;
            return false;
        }

    }
}
