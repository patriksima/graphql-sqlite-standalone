using SQLite;

namespace GraphQLTest
{
    public class DbItem
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public int Health { get; set; }
    }
}