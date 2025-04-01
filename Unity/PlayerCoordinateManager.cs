namespace HeatBot
{
    public class PlayerCoordinateManager
    {
        public int CurrentX { get; private set; }
        public int CurrentY { get; private set; }

        public PlayerCoordinateManager(int startX, int startY)
        {
            CurrentX = startX;
            CurrentY = startY;
        }

        public void MoveUp()
        {
            CurrentY -= 1;
        }

        public void MoveDown()
        {
            CurrentY += 1;
        }

        public void MoveLeft()
        {
            CurrentX -= 1;
        }

        public void MoveRight()
        {
            CurrentX += 1;
        }

        public void SetCoordinates(int x, int y)
        {
            CurrentX = x;
            CurrentY = y;
        }
    }
}