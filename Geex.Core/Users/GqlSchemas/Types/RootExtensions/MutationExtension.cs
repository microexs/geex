﻿using Geex.Core.Users.GqlSchemas.Types.Inputs;
using Geex.Shared.Roots;
using HotChocolate.Types;

namespace Geex.Core.Users.GqlSchemas.Types.RootExtensions
{
    public class MutationExtension : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(nameof(Mutation));
            descriptor.Field<UserResolver>(x => x.Register(default, default, default))
                .Argument("input", x => x.Type<RegisterUserInputType>());
            descriptor.Field<UserResolver>(x => x.AssignRoles(default, default))
                .Argument("input", x => x.Type<AssignRoleInputType>()).Authorize("Permission");
            descriptor.Field<UserResolver>(x => x.AssignRoles(default, default))
                .Argument("input", x => x.Type<AssignRoleInputType>()).Authorize("Permission");
            base.Configure(descriptor);
        }
    }
}