using CfPeopleAnalyzer.db.@abstract;
using Npgsql;

namespace CfPeopleAnalyzer.db.rawsql;

public class RawPostgresManager : IDbManager
{
    private const string ConnectionConfig = "Host=localhost;Username=postgres;Password=agile789;Database=CodeforcesDb";
    private NpgsqlConnection? _connection;


    public void OpenConnectionAndSetup()
    {
        OpenConnection();
        // SetUpTables();
    }

    public IUserDao GetUserDao()
    {
        return new RawUserDao();
    }

    public NpgsqlCommand PrepareCommand(string query)
    {
        return new NpgsqlCommand(query, _connection);
    }

    private void OpenConnection()
    {
        _connection = new NpgsqlConnection(ConnectionConfig);
        _connection.Open();
    }

    private void SetUpTables()
    {
        using var cmd = new NpgsqlCommand(RawUserDao.TableCreateConfig, _connection);
        cmd.ExecuteNonQueryAsync();
    }
}