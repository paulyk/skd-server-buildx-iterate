# nullable enable

using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VT.Model;

namespace VT.Server {

   public class ComponentType : ObjectType<Component> {

       protected override void Configure(IObjectTypeDescriptor<Component> descriptor) {           
           descriptor.Field(t => t.Id).Type<NonNullType<IdType>>();
       }
   }
}