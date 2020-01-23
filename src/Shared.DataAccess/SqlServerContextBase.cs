using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Shared.DataAccess
{
    [ExcludeFromCodeCoverage]
    public abstract class SqlServerContextBase : DbContext
    {
        protected string DbSchema { get; }

        protected SqlServerContextBase(DbContextOptions options) : base(options)
        {
            var schemaExt = options.FindExtension<DbContextOptionsSchemaExtension>();
            if (schemaExt != null)
                DbSchema = schemaExt.DbSchema;
        }

    }
}