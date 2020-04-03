using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace GraphQLTest
{
    public class Database
    {
        private readonly SqliteConnection _connection;
        
        public Database(string connectionString)
        {
            _connection = new SqliteConnection(connectionString);
            _connection.Open();
        }

        public string Query(string query)
        {
            var cmd = new SqliteCommand(query, _connection);
            return cmd.ExecuteScalar().ToString();
        }
    }
}