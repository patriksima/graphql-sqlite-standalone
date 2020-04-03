using SQLite;

namespace GraphQLTest
{
    public class DbUser
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        [Unique] public string Email { get; set; }
    }
}