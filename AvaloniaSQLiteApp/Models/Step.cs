namespace AvaloniaSQLiteApp.Models
{
    public class Step
    {
        public int ID { get; set; }
        public int ModeId { get; set; }
        public int Timer { get; set; }
        public string? Destination { get; set; }
        public int Speed { get; set; }
        public string? Type { get; set; }
        public int Volume { get; set; }

        public Step(int id, int modeId, int timer, string? destination, int speed, string? type, int volume)
        {
            ID = id;
            ModeId = modeId;
            Timer = timer;
            Destination = destination;
            Speed = speed;
            Type = type;
            Volume = volume;
        }

        public Step() { }
    }
}
