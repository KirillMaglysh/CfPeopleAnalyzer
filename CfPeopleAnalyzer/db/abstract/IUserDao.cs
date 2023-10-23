using System.Collections.Generic;
using CfPeopleAnalyzer.db.models;

namespace CfPeopleAnalyzer.db.@abstract;

public interface IUserDao
{
    public void AddUser(User user);
    bool Exist(string parsedUserHandle);

    public int GetParsedCount();

    public int GetUndefinedCountryCount();

    public IEnumerable<CountryAndRating> GetCountryAndRatingsAsc();

    public IEnumerable<CountryAndRating> GetCountryAndRatingsDesk();

    public IEnumerable<CountryAndRating> GetCountryAndPopulationAsc();

    public IEnumerable<CountryAndRating> GetCountryAndPopulationDesk();
}
