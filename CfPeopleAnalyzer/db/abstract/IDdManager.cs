using Npgsql;

namespace CfPeopleAnalyzer.db.@abstract;

public interface IDbManager
{
    public void OpenConnectionAndSetup();

    public IUserDao GetUserDao();

    public NpgsqlCommand PrepareCommand(string query);
}