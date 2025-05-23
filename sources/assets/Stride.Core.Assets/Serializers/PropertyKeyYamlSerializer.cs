// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Reflection;
using Stride.Core.Yaml;
using Stride.Core.Yaml.Events;
using Stride.Core.Yaml.Serialization;

namespace Stride.Core.Assets.Serializers;

[YamlSerializerFactory(YamlSerializerFactoryAttribute.Default)]
internal class PropertyKeyYamlSerializer : AssetScalarSerializerBase
{
    public override bool CanVisit(Type type)
    {
        // Because a PropertyKey<> inherits directly from PropertyKey, we can directly check the base only
        // ParameterKey<> inherits from ParameterKey, so it won't conflict with the custom ParameterKeyYamlSerializer
        // defined in the Stride.Assets assembly

        if (type == typeof(PropertyKey))
        {
            return true;
        }

        for (Type? t = type; t != null; t = t.BaseType)
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(PropertyKey<>))
                return true;

        return false;
    }

    public override object? ConvertFrom(ref ObjectContext objectContext, Scalar fromScalar)
    {
        var lastDot = fromScalar.Value.LastIndexOf('.');
        if (lastDot == -1)
            return null;

        var className = fromScalar.Value[..lastDot];

        var containingClass = objectContext.SerializerContext.TypeFromTag("!" + className, out var typeAliased)
            ?? throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to find class from tag [{0}]".ToFormat(className)); // Readd initial '!'

        var propertyName = fromScalar.Value[(lastDot + 1)..];
        var propertyField = containingClass.GetField(propertyName, BindingFlags.Public | BindingFlags.Static)
            ?? throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to find property [{0}] in class [{1}]".ToFormat(propertyName, containingClass.Name));
        return propertyField.GetValue(null);
    }

    protected override void WriteScalar(ref ObjectContext objectContext, ScalarEventInfo scalar)
    {
        // TODO: if ParameterKey is written to an object, It will not serialized a tag
        scalar.Tag = null;
        scalar.IsPlainImplicit = true;
        base.WriteScalar(ref objectContext, scalar);
    }

    public override string ConvertTo(ref ObjectContext objectContext)
    {
        var propertyKey = (PropertyKey)objectContext.Instance;

        return PropertyKeyNameResolver.ComputePropertyKeyName(objectContext.SerializerContext, propertyKey);
    }

    
}
