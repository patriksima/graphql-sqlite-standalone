using SQLite;

namespace GraphQLTest
{
    public class DbUserItem
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        [Indexed] public int DbUserId { get; set; }
        [Indexed] public int DbItemId { get; set; }
    }
}