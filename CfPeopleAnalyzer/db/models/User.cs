namespace CfPeopleAnalyzer.db.models;

public record User(string Handle, string Country, int Ratings)
{
    public const string TableName = "users";
}