using System.Collections.Generic;
using CfPeopleAnalyzer.db.models;

namespace CfPeopleAnalyzer.db.@abstract;

public interface IUserDao
{
    public void AddUser(User user);
    bool Exist(string parsedUserHandle);
}