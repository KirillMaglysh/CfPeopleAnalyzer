using CfPeopleAnalyzer.db.@abstract;
using CfPeopleAnalyzer.db.models;

namespace CfPeopleAnalyzer.db.rawsql;

public class RawUserDao : IUserDao
{
    public const string TableCreateConfig = $"CREATE TABLE if not exists public.{User.TableName}" +
                                            $"(" +
                                            $"handle varchar(50) PRIMARY KEY, " +
                                            $"country VARCHAR (50), " +
                                            $"ratings int, " +
                                            $");";

    public void AddUser(User user)
    {
        var commandText =
            $"INSERT INTO {User.TableName}" +
            $" (handle, country, rating)" +
            $" VALUES" +
            $" ('{user.Handle}', '{user.Country}','{user.Ratings}')";
        var command = DbManagerFactory.DbManager.PrepareCommand(commandText);
        command.ExecuteReader().Close();
    }

    public bool Exist(string parsedUserHandle)
    {
        var commandText =
            $"Select Count(*) from users WHERE handle = '{parsedUserHandle}'";
        var command = DbManagerFactory.DbManager.PrepareCommand(commandText);
        var reader = command.ExecuteReader();
        var cnt = 0;
        while (reader.Read())
        {
            cnt += reader.GetInt32(0);
        }
        reader.Close();
        return cnt > 0;
    }
}