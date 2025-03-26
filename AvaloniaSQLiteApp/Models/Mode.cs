namespace AvaloniaSQLiteApp.Models
{
    public class Mode
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public int? MaxBottleNumber { get; set; }
        public int? MaxUsedTips { get; set; }

        public Mode(int id, string name, int maxBottleNumber, int maxUsedTips)
        {
            ID = id;
            Name = name;
            MaxBottleNumber = maxBottleNumber;
            MaxUsedTips = maxUsedTips;
        }

        public Mode() { }
    }
}
