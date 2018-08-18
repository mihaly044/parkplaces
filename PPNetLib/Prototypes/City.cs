using ProtoBuf;

namespace PPNetLib.Prototypes
{
    [ProtoContract]
    public class City
    {
        [ProtoMember(1)]
        public int Id { get; }

        [ProtoMember(2)]
        public string Name { get; set; }

        public City()
        {
            Id = 0;
        }

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
