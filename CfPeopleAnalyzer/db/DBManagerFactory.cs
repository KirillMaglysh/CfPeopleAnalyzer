
using CfPeopleAnalyzer.db.@abstract;
using CfPeopleAnalyzer.db.rawsql;

namespace CfPeopleAnalyzer.db;

public abstract class DbManagerFactory
{
    public static readonly IDbManager DbManager = new RawPostgresManager();
    
    private DbManagerFactory()
    {
    }
}