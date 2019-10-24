using System.Data;
using async.Internal;
using async.OpenAndCloseConnections;
using Microsoft.Data.SqlClient;

namespace async
{
    public static class DatabaseUtility
    {
        // Shared connection in the application
        private static readonly SqlConnection _connection;


        static DatabaseUtility()
        {
            _connection = new SqlConnection("some connection string");
            _connection.Open();
        }


        public static User FindUser(string userName)
        {
            // Try to detect if the connection is closed or broken
            // and reopen if necessary
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
            
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "select * from users where UserName = @name";
            cmd.Parameters.Add("name", SqlDbType.VarChar).Value = userName;

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return readUser(reader);
                }
                else
                {
                    return null;
                }
            }
        }

        private static User readUser(SqlDataReader reader)
        {
            throw new System.NotImplementedException();
        }
    }
}