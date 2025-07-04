﻿namespace CommonUtilities.Models;

/// <summary>
/// Model of application user.
/// </summary>
/// <param name="Identifier">
/// Unique user identifier.
/// </param>
/// <param name="Name">
/// Name of a user. Uniqueness is not necessary.
/// </param>
public sealed record User(int Identifier, string Name);
