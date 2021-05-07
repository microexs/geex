﻿using Geex.Shared._ShouldMigrateToLib.Abstractions;

namespace Geex.Core.Authorization
{
    public class AuthorizeTargetType : Enumeration<AuthorizeTargetType, string>
    {
        public static AuthorizeTargetType Role { get; } = new AuthorizeTargetType(nameof(Role));
        public static AuthorizeTargetType User { get; } = new AuthorizeTargetType(nameof(User));

        public AuthorizeTargetType(string value) : base(value)
        {
        }
    }
}