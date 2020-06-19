# nullable enable
#nullable enable

using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using SKD.Model;

namespace SKD.Server {
    public class QueryType: ObjectType<Query> {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor) {
        }
    }
}
