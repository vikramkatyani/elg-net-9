using System;
using System.Data.Entity;
using ELG.DAL.DBEntity;
using ELG.DAL.DbEntityLearner;
using ELG.DAL.DBEntitySA;

namespace ELG.DAL
{
    /// <summary>
    /// Factory class for creating DbContext instances with runtime connection strings.
    /// Used for dependency injection and configuration management in ASP.NET Core.
    /// </summary>
    public static class DbContextFactory
    {
        /// <summary>
        /// Creates an lmsdbEntities context with the specified connection string.
        /// This is used for the Admin portal database operations.
        /// </summary>
        public static lmsdbEntities CreateAdminContext(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

            return new lmsdbEntities(connectionString);
        }

        /// <summary>
        /// Creates a learnerDBEntities context with the specified connection string.
        /// This is used for the Learner portal database operations.
        /// </summary>
        public static learnerDBEntities CreateLearnerContext(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

            return new learnerDBEntities(connectionString);
        }

        /// <summary>
        /// Creates a superadmindbEntities context with the specified connection string.
        /// This is used for the SuperAdmin portal database operations.
        /// </summary>
        public static superadmindbEntities CreateSuperAdminContext(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

            return new superadmindbEntities(connectionString);
        }
    }
}
