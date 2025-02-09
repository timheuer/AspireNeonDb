﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// A resource that represents a PostgreSQL database. This is a child resource of a <see cref="PostgresServerResource"/>.
/// </summary>
/// <param name="name">The name of the resource.</param>
/// <param name="databaseName">The database name.</param>
/// <param name="postgresParentResource">The PostgreSQL parent resource associated with this database.</param>
public class NeonDatabaseResource(string name, string databaseName, NeonServerResource postgresParentResource) : Resource(ThrowIfNull(name)), IResourceWithParent<NeonServerResource>, IResourceWithConnectionString
{
    /// <summary>
    /// Gets the parent PostgresSQL container resource.
    /// </summary>
    public NeonServerResource Parent { get; } = ThrowIfNull(postgresParentResource);

    /// <summary>
    /// Gets the connection string expression for the Postgres database.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
       ReferenceExpression.Create($"{Parent};Database={DatabaseName}");

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string DatabaseName { get; } = ThrowIfNull(databaseName);

    private static T ThrowIfNull<T>([NotNull] T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        => argument ?? throw new ArgumentNullException(paramName);
}