using System.Data.Common;

namespace Shared.DataAccess.DbConnectionFactories
{
    public interface IDbConnectionFactory
    {
        DbConnection CreateDbConnection(string name);
    }
}