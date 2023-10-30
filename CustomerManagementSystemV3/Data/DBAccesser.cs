using Microsoft.Data.SqlClient;

namespace CustomerManagementSystemV3.Data
{
    public class DBAccesser
    {
        private readonly string _connection_string;

        public DBAccesser(string connection)
        {
            _connection_string = connection;
        }

        public SqlConnection CreateDBConnection()
        {
            return new SqlConnection(_connection_string);
        }
    }
}
