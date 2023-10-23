using System;
using System.Collections.Generic;
using System.Data;
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

    public int GetParsedCount()
    {
        const string commandText = "Select Count(*) from users";
        var command = DbManagerFactory.DbManager.PrepareCommand(commandText);
        var reader = command.ExecuteReader();
        var count = ParseIntFromSqlReader(reader);
        reader.Close();
        return count;
    }

    public int GetUndefinedCountryCount()
    {
        const string commandText = "Select Count(*) from users WHERE country = 'Undefined'";
        var command = DbManagerFactory.DbManager.PrepareCommand(commandText);
        var reader = command.ExecuteReader();
        var count = ParseIntFromSqlReader(reader);
        reader.Close();
        return count;
    }

    public IEnumerable<CountryAndRating> GetCountryAndRatingsAsc()
    {
        var commandText = "a";
        return GetCountryAndRatings(commandText);
    }

    public IEnumerable<CountryAndRating> GetCountryAndRatingsDesk()
    {
        var commandText = "s";
        return GetCountryAndRatings(commandText);
    }

    private IEnumerable<CountryAndRating> GetCountryAndRatings(string commandText)
    {
        var command = DbManagerFactory.DbManager.PrepareCommand(commandText);
        var reader = command.ExecuteReader();
        var humans = ParseCountryAndRatingsFromSqlReader(reader);
        reader.Close();
        return humans;
    }

    public IEnumerable<CountryAndRating> GetCountryAndPopulationAsc()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<CountryAndRating> GetCountryAndPopulationDesk()
    {
        throw new NotImplementedException();
    }

    private static int ParseIntFromSqlReader(IDataReader reader)
    {
        reader.Read();
        return reader.GetInt32(0);
    }

    private static List<CountryAndRating> ParseCountryAndRatingsFromSqlReader(IDataReader reader)
    {
        List<CountryAndRating> countryAndRatings = new List<CountryAndRating>();
        while (reader.Read())
        {
            countryAndRatings.Add(new CountryAndRating(
                    reader.GetString(0),
                    reader.GetInt32(1)
                )
            );
        }

        return countryAndRatings;
    }

    private static List<CountryAndPopulation> ParseCountryAndPopulationFromSqlReader(IDataReader reader)
    {
        List<CountryAndPopulation> countryAndPopulations = new List<CountryAndPopulation>();
        while (reader.Read())
        {
            countryAndPopulations.Add(new CountryAndPopulation(
                    reader.GetString(0),
                    reader.GetInt32(1)
                )
            );
        }

        return countryAndPopulations;
    }
}