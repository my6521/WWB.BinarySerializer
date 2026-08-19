using System.Linq;
using Microsoft.CodeAnalysis;

namespace WWB.BinarySerializer.Generator.Emission;

internal static class TypeShape
{
    private const string FieldAttribute = "WWB.BinarySerializer.Attributes.BinaryFieldAttribute";

    public static bool IsScalar(ITypeSymbol type) =>
        type.SpecialType is not SpecialType.None and not SpecialType.System_Object
        || type.ToDisplayString() is "System.DateTime" or "System.TimeSpan"
        || IsByteArray(type);

    public static bool IsCollection(ITypeSymbol type, out ITypeSymbol element)
    {
        if (type is IArrayTypeSymbol { Rank: 1 } array)
        {
            element = array.ElementType;
            return true;
        }

        if (type is INamedTypeSymbol { IsGenericType: true, Name: "List" } named
            && named.TypeArguments.Length == 1
            && named.ContainingNamespace.ToDisplayString() == "System.Collections.Generic")
        {
            element = named.TypeArguments[0];
            return true;
        }

        element = null!;
        return false;
    }

    public static bool IsByteArray(ITypeSymbol type) =>
        type is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Byte };

    public static bool IsObject(ITypeSymbol type) =>
        type is INamedTypeSymbol named
        && named.TypeKind == TypeKind.Class
        && named.SpecialType == SpecialType.None
        && !named.IsGenericType
        && named.InstanceConstructors.Any(c => c.DeclaredAccessibility == Accessibility.Public && c.Parameters.Length == 0)
        && named.GetMembers().OfType<IPropertySymbol>()
            .Any(p => p.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == FieldAttribute));
}
