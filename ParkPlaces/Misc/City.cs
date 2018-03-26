namespace ParkPlaces.Misc
{
    public class City
    {
        public int Id { get; }
        public string Name { get; set; }

        public City(int id = 0)
        {
            Id = id;
        }

        public override string ToString()
        {
            return Name;
        }

        public static City FromString(string name)
        {
            return new City(){Name = name};
        }
    }
}
