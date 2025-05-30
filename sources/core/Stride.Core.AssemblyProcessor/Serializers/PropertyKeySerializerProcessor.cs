// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Stride.Core.AssemblyProcessor.Serializers;

class PropertyKeySerializerProcessor : ICecilSerializerProcessor
{
    public void ProcessSerializers(CecilSerializerContext context)
    {
        // Iterate over each static member of type PropertyKey<> or ParameterKey<>
        foreach (var type in context.Assembly.MainModule.GetAllTypes())
        {
            foreach (var member in type.Fields)
            {
                if (!member.IsStatic || member.IsPrivate)
                    continue;

                if (ComplexSerializerRegistry.IsMemberIgnored(member.CustomAttributes, ComplexTypeSerializerFlags.SerializePublicFields, DataMemberMode.Default))
                    continue;

                if (member.FieldType.Name == "PropertyKey`1"
                    || member.FieldType.Name == "ParameterKey`1"
                    || member.FieldType.Name == "ValueParameterKey`1"
                    || member.FieldType.Name == "ObjectParameterKey`1"
                    || member.FieldType.Name == "PermutationParameterKey`1")
                {
                    context.GenerateSerializer(member.FieldType);

                    var genericType = (GenericInstanceType)member.FieldType;

                    // Also generate serializer for embedded type
                    context.GenerateSerializer(genericType.GenericArguments[0]);
                }
            }
        }
    }
}
