using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Data.Entity.Validation;
using System.Threading;
using System.Data.Entity.Infrastructure;
using System.ComponentModel;
using TestProject.Data.DataObjects;
using System.Data.Entity.Core.Objects;

namespace TestProject.Data.DataInterfaces.Common
{
    /// <summary>
    /// Interface for DB context
    /// </summary>
    internal interface ITestProjectContext:IDisposable
    {
        #region Builtin Functions & Properies

        /// <summary>
        /// Creates a Database instance for this context that allows for creation/deletion/existence checks
        /// for the underlying database.
        /// </summary>
        Database Database { get; }


        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.DbSet`1" /> instance for access to entities of the given type in the context
        /// and the underlying store.
        /// </summary>
        /// <remarks>
        /// Note that Entity Framework requires that this method return the same instance each time that it is called
        /// for a given context instance and entity type. Also, the non-generic <see cref="T:System.Data.Entity.DbSet" /> returned by the
        /// <see cref="M:System.Data.Entity.DbContext.Set(System.Type)" /> method must wrap the same underlying query and set of entities. These invariants must
        /// be maintained if this method is overridden for anything other than creating test doubles for unit testing.
        /// See the <see cref="T:System.Data.Entity.DbSet`1" /> class for more details.
        /// </remarks>
        /// <typeparam name="TEntity"> The type entity for which a set should be returned. </typeparam>
        /// <returns> A set for the given entity type. </returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set")]
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        /// <summary>
        /// Returns a non-generic <see cref="T:System.Data.Entity.DbSet" /> instance for access to entities of the given type in the context
        /// and the underlying store.
        /// </summary>
        /// <param name="entityType"> The type of entity for which a set should be returned. </param>
        /// <returns> A set for the given entity type. </returns>
        /// <remarks>
        /// Note that Entity Framework requires that this method return the same instance each time that it is called
        /// for a given context instance and entity type. Also, the generic <see cref="T:System.Data.Entity.DbSet`1" /> returned by the
        /// <see cref="M:System.Data.Entity.DbContext.Set(System.Type)" /> method must wrap the same underlying query and set of entities. These invariants must
        /// be maintained if this method is overridden for anything other than creating test doubles for unit testing.
        /// See the <see cref="T:System.Data.Entity.DbSet" /> class for more details.
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set")]
        DbSet Set(Type entityType);

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are created for
        /// many-to-many relationships and relationships where there is no foreign key property
        /// included in the entity class (often referred to as independent associations).
        /// </returns>
        /// <exception cref="T:System.Data.Entity.Infrastructure.DbUpdateException">An error occurred sending updates to the database.</exception>
        /// <exception cref="T:System.Data.Entity.Infrastructure.DbUpdateConcurrencyException">
        /// A database command did not affect the expected number of rows. This usually indicates an optimistic
        /// concurrency violation; that is, a row has been changed in the database since it was queried.
        /// </exception>
        /// <exception cref="T:System.Data.Entity.Validation.DbEntityValidationException">
        /// The save was aborted because validation of entity property values failed.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// An attempt was made to use unsupported behavior such as executing multiple asynchronous commands concurrently
        /// on the same context instance.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The context or connection have been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// Some error occurred attempting to process entities in the context either before or after sending commands
        /// to the database.
        /// </exception>
        int SaveChanges();

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        /// A task that represents the asynchronous save operation.
        /// The task result contains the number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are created for
        /// many-to-many relationships and relationships where there is no foreign key property
        /// included in the entity class (often referred to as independent associations).
        /// </returns>
        /// <exception cref="T:System.Data.Entity.Infrastructure.DbUpdateException">An error occurred sending updates to the database.</exception>
        /// <exception cref="T:System.Data.Entity.Infrastructure.DbUpdateConcurrencyException">
        /// A database command did not affect the expected number of rows. This usually indicates an optimistic
        /// concurrency violation; that is, a row has been changed in the database since it was queried.
        /// </exception>
        /// <exception cref="T:System.Data.Entity.Validation.DbEntityValidationException">
        /// The save was aborted because validation of entity property values failed.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// An attempt was made to use unsupported behavior such as executing multiple asynchronous commands concurrently
        /// on the same context instance.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The context or connection have been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// Some error occurred attempting to process entities in the context either before or after sending commands
        /// to the database.
        /// </exception>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        /// A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous save operation.
        /// The task result contains the number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are created for
        /// many-to-many relationships and relationships where there is no foreign key property
        /// included in the entity class (often referred to as independent associations).
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if the context has been disposed.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Validates tracked entities and returns a Collection of <see cref="T:System.Data.Entity.Validation.DbEntityValidationResult" /> containing validation results.
        /// </summary>
        /// <returns> Collection of validation results for invalid entities. The collection is never null and must not contain null values or results for valid entities. </returns>
        /// <remarks>
        /// 1. This method calls DetectChanges() to determine states of the tracked entities unless
        /// DbContextConfiguration.AutoDetectChangesEnabled is set to false.
        /// 2. By default only Added on Modified entities are validated. The user is able to change this behavior
        /// by overriding ShouldValidateEntity method.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        IEnumerable<DbEntityValidationResult> GetValidationErrors();

        /// <summary>
        /// Gets a <see cref="T:System.Data.Entity.Infrastructure.DbEntityEntry`1" /> object for the given entity providing access to
        /// information about the entity and the ability to perform actions on the entity.
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity. </param>
        /// <returns> An entry for the entity. </returns>
        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Gets a <see cref="T:System.Data.Entity.Infrastructure.DbEntityEntry" /> object for the given entity providing access to
        /// information about the entity and the ability to perform actions on the entity.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <returns> An entry for the entity. </returns>
        DbEntityEntry Entry(object entity);

        /// <summary>
        /// Provides access to features of the context that deal with change tracking of entities.
        /// </summary>
        /// <value> An object used to access features that deal with change tracking. </value>
        DbChangeTracker ChangeTracker { get; }

        /// <summary>
        /// Provides access to configuration options for the context.
        /// </summary>
        /// <value> An object used to access configuration options. </value>
        DbContextConfiguration Configuration { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        string ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();

        #endregion

        #region DbSets
        DbSet<LoginAttemptFailure> LoginAttemptFailures { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<UserAccountSession> UserAccountSessions { get; set; }
        DbSet<UserRole> UserRoles { get; set; }
        #endregion
        #region Database Procedure Imports
        ObjectResult<usp_Security_Login_Result> usp_Security_Login(string userName, string userPassword, string iPAddress);
        #endregion

    }
}
