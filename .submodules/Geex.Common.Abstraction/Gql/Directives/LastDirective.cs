﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HotChocolate.Types;

namespace Geex.Common.Abstraction.Gql.Directives
{
    public class LastDirectiveType : DirectiveType
    {
        protected override void Configure(
            IDirectiveTypeDescriptor descriptor)
        {
            descriptor.Name("last");
            descriptor.Location(DirectiveLocation.Field);
            descriptor.Argument("count").Type<IntType>().DefaultValue(1);
            descriptor.Use(next => context =>
            {
                var result = next.Invoke(context);
                result.AsTask().Wait();
                var collection = (context.Result as IEnumerable);
                context.Result = collection?.Cast<object>().TakeLast(1);
                return result;
            });
        }
    }

    public class FirstDirectiveType : DirectiveType
    {
        protected override void Configure(
            IDirectiveTypeDescriptor descriptor)
        {
            descriptor.Name("first");
            descriptor.Location(DirectiveLocation.Field);
            descriptor.Argument("count").Type<IntType>().DefaultValue(1);
            descriptor.Use(next => context =>
            {
                var result = next.Invoke(context);
                result.AsTask().Wait();
                var collection = (context.Result as IEnumerable);
                context.Result = collection?.Cast<object>().Take(1);
                return result;
            });
        }
    }
}
