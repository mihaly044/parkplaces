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

        public static City FromString(string name, int id = 0)
        {
            return new City(id){Name = name};
        }
    }
}
