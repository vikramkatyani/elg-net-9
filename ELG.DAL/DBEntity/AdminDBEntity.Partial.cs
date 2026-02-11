using System;
using System.Data.Entity;

namespace ELG.DAL.DBEntity
{
    /// <summary>
    /// Partial class extension for lmsdbEntities (AdminDBEntity context).
    /// Adds support for runtime connection string configuration for Azure deployment.
    /// </summary>
    public partial class lmsdbEntities : DbContext
    {
        /// <summary>
        /// Constructor that accepts a connection string parameter.
        /// Used by the factory pattern for dependency injection in ASP.NET Core.
        /// </summary>
        /// <param name="nameOrConnectionString">
        /// Connection string or the name of the connection string in web.config or appsettings.json
        /// </param>
        public lmsdbEntities(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            // Ensure we're using the provided connection string
            // This overrides the default "name=lmsdbEntities" from App.Config
        }
    }
}

