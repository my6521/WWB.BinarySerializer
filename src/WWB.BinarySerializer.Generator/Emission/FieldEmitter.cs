using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace WWB.BinarySerializer.Generator.Emission;

internal static class FieldEmitter
{
    public static void Write(StringBuilder source, IPropertySymbol property, AttributeData attribute)
    {
        var valueCodec = ValueCodecName(attribute);
        if (valueCodec != null && !TypeShape.IsCollection(property.Type, out _))
        {
            source.AppendLine($"context.GetValueCodec<{TypeName(property.Type)}>({StringLiteral(valueCodec)}).Encode(writer, {Member(property)}, context);");
            return;
        }
        if (property.Type.SpecialType == SpecialType.System_String)
        {
            source.AppendLine($"context.ValidateStringLength(global::System.Text.Encoding.UTF8.GetByteCount({Member(property)}), typeof({TypeName(property.ContainingType)})); writer.WriteUtf8({Member(property)}, {NamedInt(attribute, "LengthPrefixSize", 1)});");
            return;
        }
        if (TypeShape.IsObject(property.Type))
        {
            WriteObject(source, property.Type, Member(property));
            return;
        }
        if (TypeShape.IsByteArray(property.Type))
        {
            WriteCollectionPrefix(source, property, attribute, $"{Member(property)}.Length");
            source.AppendLine($"writer.Write({Member(property)});");
            return;
        }
        if (TypeShape.IsCollection(property.Type, out var element))
        {
            WriteCollectionPrefix(source, property, attribute, property.Type is IArrayTypeSymbol ? $"{Member(property)}.Length" : $"{Member(property)}.Count");
            source.AppendLine($"foreach (var item in {Member(property)}) {{");
            if (valueCodec != null) source.AppendLine($"context.GetValueCodec<{TypeName(element)}>({StringLiteral(valueCodec)}).Encode(writer, item, context);");
            else if (TypeShape.IsObject(element)) WriteObject(source, element, "item");
            else WriteValue(source, element, "item");
            source.AppendLine("}");
            return;
        }
        WriteValue(source, property.Type, Member(property));
    }

    public static void Read(StringBuilder source, IPropertySymbol property, AttributeData attribute)
    {
        var valueCodec = ValueCodecName(attribute);
        if (valueCodec != null && !TypeShape.IsCollection(property.Type, out _))
        {
            source.AppendLine($"{Member(property)} = context.GetValueCodec<{TypeName(property.Type)}>({StringLiteral(valueCodec)}).Decode(ref reader, context);");
            return;
        }
        if (property.Type.SpecialType == SpecialType.System_String)
        {
            source.AppendLine($"{Member(property)} = reader.ReadUtf8({NamedInt(attribute, "LengthPrefixSize", 1)}, context, typeof({TypeName(property.ContainingType)}));");
            return;
        }
        if (TypeShape.IsObject(property.Type))
        {
            var typeName = TypeName(property.Type);
            source.AppendLine($"var codec_{property.Name} = context.GetCodec<{typeName}>(); using (context.Enter(typeof({typeName}))) {Member(property)} = codec_{property.Name}.Decode(ref reader, context);");
            return;
        }
        if (TypeShape.IsByteArray(property.Type))
        {
            var count = CountExpression(attribute);
            source.AppendLine($"var count_{property.Name} = {count}; context.ValidateCollectionLength(count_{property.Name}, typeof({TypeName(property.ContainingType)})); {Member(property)} = reader.ReadSpan(count_{property.Name}).ToArray();");
            return;
        }
        if (TypeShape.IsCollection(property.Type, out var element))
        {
            ReadCollection(source, property, attribute, element, valueCodec);
            return;
        }
        source.AppendLine($"{Member(property)} = {ReadValue(property.Type)};");
    }

    private static void WriteObject(StringBuilder source, ITypeSymbol type, string expression)
    {
        var typeName = TypeName(type);
        source.AppendLine($"global::System.ArgumentNullException.ThrowIfNull({expression}); var codec_{Sanitize(expression)} = context.GetCodec<{typeName}>(); using (context.Enter(typeof({typeName}))) codec_{Sanitize(expression)}.Encode(writer, {expression}, context);");
    }

    private static void WriteValue(StringBuilder source, ITypeSymbol sourceType, string member)
    {
        var isEnum = sourceType.TypeKind == TypeKind.Enum;
        var type = isEnum ? ((INamedTypeSymbol)sourceType).EnumUnderlyingType! : sourceType;
        var cast = isEnum ? $"({TypeName(type)})" : "";
        var expression = $"{cast}{member}";
        if (type.ToDisplayString() == "System.DateTime") { source.AppendLine($"writer.WriteInt64({member}.ToBinary());"); return; }
        if (type.ToDisplayString() == "System.TimeSpan") { source.AppendLine($"writer.WriteInt64({member}.Ticks);"); return; }
        if (type.SpecialType == SpecialType.System_Boolean) expression = $"(byte)({member} ? 1 : 0)";
        else if (type.SpecialType == SpecialType.System_SByte) expression = $"unchecked((byte){expression})";
        else if (type.SpecialType == SpecialType.System_Char) expression = $"(ushort){expression}";
        source.AppendLine($"writer.{NativeMethod(type, false)}({expression});");
    }

    private static void WriteCollectionPrefix(StringBuilder source, IPropertySymbol property, AttributeData attribute, string count)
    {
        source.AppendLine($"global::System.ArgumentNullException.ThrowIfNull({Member(property)});");
        var size = NamedInt(attribute, "FixedLength", 0);
        if (size == 0) source.AppendLine($"writer.WriteLength({count}, {NamedInt(attribute, "LengthPrefixSize", 1)});");
        else source.AppendLine($"if ({count} != {size}) throw new global::System.ArgumentException(\"Collection length mismatch.\");");
    }

    private static void ReadCollection(StringBuilder source, IPropertySymbol property, AttributeData attribute, ITypeSymbol element, string? valueCodec)
    {
        var elementName = TypeName(element);
        source.AppendLine($"var count_{property.Name} = {CountExpression(attribute)}; context.ValidateCollectionLength(count_{property.Name}, typeof({TypeName(property.ContainingType)}));");
        if (valueCodec != null)
        {
            var getCodec = $"context.GetValueCodec<{elementName}>({StringLiteral(valueCodec)})";
            if (property.Type is IArrayTypeSymbol)
                source.AppendLine($"var values_{property.Name} = new {elementName}[count_{property.Name}]; var valueCodec_{property.Name} = {getCodec}; for (var i = 0; i < count_{property.Name}; i++) values_{property.Name}[i] = valueCodec_{property.Name}.Decode(ref reader, context); {Member(property)} = values_{property.Name};");
            else
                source.AppendLine($"var values_{property.Name} = new global::System.Collections.Generic.List<{elementName}>(count_{property.Name}); var valueCodec_{property.Name} = {getCodec}; for (var i = 0; i < count_{property.Name}; i++) values_{property.Name}.Add(valueCodec_{property.Name}.Decode(ref reader, context)); {Member(property)} = values_{property.Name};");
        }
        else if (property.Type is IArrayTypeSymbol)
            source.AppendLine(TypeShape.IsObject(element)
                ? $"var values_{property.Name} = new {elementName}[count_{property.Name}]; var codec_{property.Name}Element = context.GetCodec<{elementName}>(); for (var i = 0; i < count_{property.Name}; i++) {{ using (context.Enter(typeof({elementName}))) values_{property.Name}[i] = codec_{property.Name}Element.Decode(ref reader, context); }} {Member(property)} = values_{property.Name};"
                : $"var values_{property.Name} = new {elementName}[count_{property.Name}]; for (var i = 0; i < count_{property.Name}; i++) values_{property.Name}[i] = {ReadValue(element)}; {Member(property)} = values_{property.Name};");
        else
            source.AppendLine(TypeShape.IsObject(element)
                ? $"var values_{property.Name} = new global::System.Collections.Generic.List<{elementName}>(count_{property.Name}); var codec_{property.Name}Element = context.GetCodec<{elementName}>(); for (var i = 0; i < count_{property.Name}; i++) {{ using (context.Enter(typeof({elementName}))) values_{property.Name}.Add(codec_{property.Name}Element.Decode(ref reader, context)); }} {Member(property)} = values_{property.Name};"
                : $"var values_{property.Name} = new global::System.Collections.Generic.List<{elementName}>(count_{property.Name}); for (var i = 0; i < count_{property.Name}; i++) values_{property.Name}.Add({ReadValue(element)}); {Member(property)} = values_{property.Name};");
    }

    private static string ReadValue(ITypeSymbol sourceType)
    {
        var isEnum = sourceType.TypeKind == TypeKind.Enum;
        var type = isEnum ? ((INamedTypeSymbol)sourceType).EnumUnderlyingType! : sourceType;
        if (type.ToDisplayString() == "System.DateTime") return "global::System.DateTime.FromBinary(reader.ReadInt64())";
        if (type.ToDisplayString() == "System.TimeSpan") return "global::System.TimeSpan.FromTicks(reader.ReadInt64())";
        var value = $"reader.{NativeMethod(type, true)}()";
        if (type.SpecialType == SpecialType.System_Boolean) value += " == 1";
        if (isEnum) return $"({TypeName(sourceType)}){value}";
        if (type.SpecialType == SpecialType.System_SByte) return $"unchecked((sbyte){value})";
        if (type.SpecialType == SpecialType.System_Char) return $"(char){value}";
        return value;
    }

    private static string? NativeMethod(ITypeSymbol type, bool read) => type.SpecialType switch
    {
        SpecialType.System_Boolean or SpecialType.System_Byte or SpecialType.System_SByte => read ? "ReadByte" : "WriteByte",
        SpecialType.System_Char or SpecialType.System_UInt16 => read ? "ReadUInt16" : "WriteUInt16",
        SpecialType.System_Int16 => read ? "ReadInt16" : "WriteInt16",
        SpecialType.System_Int32 => read ? "ReadInt32" : "WriteInt32",
        SpecialType.System_UInt32 => read ? "ReadUInt32" : "WriteUInt32",
        SpecialType.System_Int64 => read ? "ReadInt64" : "WriteInt64",
        SpecialType.System_UInt64 => read ? "ReadUInt64" : "WriteUInt64",
        SpecialType.System_Single => read ? "ReadSingle" : "WriteSingle",
        SpecialType.System_Double => read ? "ReadDouble" : "WriteDouble",
        SpecialType.System_Decimal => read ? "ReadDecimal" : "WriteDecimal",
        _ => null
    };

    private static string CountExpression(AttributeData attribute) =>
        NamedInt(attribute, "FixedLength", 0) is var size && size > 0 ? size.ToString() : $"reader.ReadLength({NamedInt(attribute, "LengthPrefixSize", 1)})";

    private static string? ValueCodecName(AttributeData attribute) =>
        attribute.NamedArguments.FirstOrDefault(x => x.Key == "ValueCodecName").Value.Value as string;

    private static int NamedInt(AttributeData attribute, string name, int fallback) =>
        attribute.NamedArguments.FirstOrDefault(x => x.Key == name).Value.Value is int value ? value : fallback;

    private static string TypeName(ITypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    private static string StringLiteral(string value) => SymbolDisplay.FormatLiteral(value, true);
    private static string Sanitize(string value) => new(value.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
    private static string Member(IPropertySymbol property) => $"value.{EscapeIdentifier(property.Name)}";
    private static string EscapeIdentifier(string name) => SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None ? "@" + name : name;
}
