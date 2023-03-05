namespace Slider
{
    public class KameraParameter
    {
        public KameraParameter()
        {

        }

        public bool isFokus = false;
        public bool isShoot = false;
        public bool isStart = false;
        public bool isEnd = false;

        public bool isSpeed = false;
        public bool isPosition = false;
        public bool isDx = false;
        public bool isPics = false;

        public double fokus = 3;
        public double shoot = 1;
        public double start = 0.25;
        public double end = 0.25;

        public double speed;
        public double position;
        public double dx;
        public double pics;

        public void setAllIsFalse()
        {
            isFokus = false;
            isShoot = false;
            isStart = false;
            isEnd = false;

            isSpeed = false;
            isPosition = false;
            isDx = false;
            isPics = false;
        }
    }
}
