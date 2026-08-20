using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class BinarySerializerTests
{
    [Fact]
    public void SerializeAndDeserialize_Primitives_UsesConfiguredBigEndianOrder()
    {
        var source = new PrimitivePacket
        {
            Flag = true,
            Number = 0x01020304,
            Code = 0x0506,
            Amount = 12.5m
        };

        var bytes = BinarySerializer.SerializeObject(source);
        var result = BinarySerializer.DeserializeObject<PrimitivePacket>(bytes);

        Assert.Equal(new byte[] { 1, 1, 2, 3, 4, 5, 6 }, bytes.Take(7));
        Assert.Equal(source.Flag, result.Flag);
        Assert.Equal(source.Number, result.Number);
        Assert.Equal(source.Code, result.Code);
        Assert.Equal(source.Amount, result.Amount);
    }

    [Fact]
    public void RecordClass_RoundTripsThroughGeneratedCodec()
    {
        var value = new RecordPacket { Value = 123 };

        var bytes = BinarySerializer.SerializeObject(value);

        Assert.Equal(new byte[] { 123, 0, 0, 0 }, bytes);
        Assert.Equal(value, BinarySerializer.DeserializeObject<RecordPacket>(bytes));
    }

    [Fact]
    public void SerializeAndDeserialize_SByteAndChar_RoundTripsUsingConfiguredEndian()
    {
        var source = new CharacterPacket { SignedValue = -1, Character = '\u4E2D' };

        var bytes = BinarySerializer.SerializeObject(source);
        var result = BinarySerializer.DeserializeObject<CharacterPacket>(bytes);

        Assert.Equal(new byte[] { 0xFF, 0x4E, 0x2D }, bytes);
        Assert.Equal(source.SignedValue, result.SignedValue);
        Assert.Equal(source.Character, result.Character);
    }

    [Fact]
    public void Serialize_SameOrderProperties_UsesMetadataOrderDeterministically()
    {
        var bytes = BinarySerializer.SerializeObject(new SameOrderPacket { First = 1, Second = 0x0203 });

        Assert.Equal(new byte[] { 1, 3, 2 }, bytes);
    }

    [Fact]
    public void SerializeAndDeserialize_NestedObjectAndVariableList_RoundTrips()
    {
        var source = new ComplexPacket
        {
            Header = new PacketHeader { Version = 2, Sequence = 0x1234 },
            Values = new List<int> { 10, 20, 30 },
            Children = new List<PacketItem>
            {
                new() { Id = 1, Value = 100 },
                new() { Id = 2, Value = 200 }
            }
        };

        var result = BinarySerializer.DeserializeObject<ComplexPacket>(BinarySerializer.SerializeObject(source));

        Assert.Equal(source.Header.Version, result.Header.Version);
        Assert.Equal(source.Header.Sequence, result.Header.Sequence);
        Assert.Equal(source.Values, result.Values);
        Assert.Collection(result.Children,
            item => Assert.Equal((byte)1, item.Id),
            item => Assert.Equal((byte)2, item.Id));
        Assert.Equal(new[] { 100, 200 }, result.Children.Select(item => item.Value));
    }

    [Fact]
    public void SerializeAndDeserialize_VariablePayloadLargerThanInitialCapacity_RoundTrips()
    {
        var source = new LargePayload { Data = Enumerable.Range(0, 700).Select(i => (byte)i).ToArray() };

        var bytes = BinarySerializer.SerializeObject(source);
        var result = BinarySerializer.DeserializeObject<LargePayload>(bytes);

        Assert.Equal(702, bytes.Length);
        Assert.Equal(source.Data, result.Data);
    }

    [Fact]
    public void BinaryContract_SizeIsExposedAsGeneratedBufferCapacityHint()
    {
        var codec = Assert.IsAssignableFrom<IBufferCapacityHint>(GeneratedCodecRegistry<LargePayload>.Instance);

        Assert.Equal(1, codec.InitialCapacity);
    }

    [Fact]
    public void SerializeAndDeserialize_Utf8String_WritesByteLengthPrefix()
    {
        var source = new VariableUtf8 { Value = "ABCD" };

        var bytes = BinarySerializer.SerializeObject(source);
        var result = BinarySerializer.DeserializeObject<VariableUtf8>(bytes);

        Assert.Equal(new byte[] { 4, (byte)'A', (byte)'B', (byte)'C', (byte)'D' }, bytes);
        Assert.Equal(source.Value, result.Value);
    }

    [Fact]
    public void SerializeAndDeserialize_VariableAsciiString_RoundTrips()
    {
        var source = new VariableAscii { Value = "ABC" };

        var bytes = BinarySerializer.SerializeObject(source);
        var result = BinarySerializer.DeserializeObject<VariableAscii>(bytes);

        Assert.Equal(new byte[] { 3, (byte)'A', (byte)'B', (byte)'C' }, bytes);
        Assert.Equal(source.Value, result.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("00")]
    public void SerializeAndDeserialize_VariableUtf8String_PreservesEmptyAndSingleByteValues(string value)
    {
        var result = BinarySerializer.DeserializeObject<VariableUtf8>(BinarySerializer.SerializeObject(new VariableUtf8 { Value = value }));

        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Serialize_VariableUtf8StringExceedingOneBytePrefix_Throws()
    {
        var source = new VariableUtf8 { Value = string.Concat(Enumerable.Repeat("AB", 256)) };

        Assert.Throws<ArgumentOutOfRangeException>(() => BinarySerializer.SerializeObject(source));
    }

    [Fact]
    public void Serialize_VariableAsciiExceedingOneBytePrefix_Throws()
    {
        var source = new VariableAscii { Value = new string('A', 256) };

        Assert.Throws<ArgumentOutOfRangeException>(() => BinarySerializer.SerializeObject(source));
    }

    [Fact]
    public void Serialize_VariableByteArrayExceedingOneBytePrefix_Throws()
    {
        var source = new VariableByteArray { Value = new byte[256] };

        Assert.Throws<ArgumentOutOfRangeException>(() => BinarySerializer.SerializeObject(source));
    }

    [Fact]
    public void Serialize_FixedLengthCollectionWithWrongCount_Throws()
    {
        var source = new FixedCollection { Values = new List<byte> { 1 } };

        Assert.Throws<ArgumentException>(() => BinarySerializer.SerializeObject(source));
    }

    [Fact]
    public void Serialize_FixedLengthByteArrayWithWrongLength_Throws()
    {
        var source = new FixedByteArray { Value = new byte[] { 1, 2 } };

        Assert.Throws<ArgumentException>(() => BinarySerializer.SerializeObject(source));
    }

    [Fact]
    public void BinaryField_LengthPrefixSizeOutsideSupportedRange_Throws()
    {
        var property = new BinaryFieldAttribute();

        Assert.Throws<ArgumentOutOfRangeException>(() => property.LengthPrefixSize = 5);
    }

    [Fact]
    public void ValueCodecOptions_RejectInvalidLengths()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WWB.BinarySerializer.Runtime.ValueCodecOptions(-1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WWB.BinarySerializer.Runtime.ValueCodecOptions(0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WWB.BinarySerializer.Runtime.ValueCodecOptions(0, 5));
    }

    [Fact]
    public void BinaryField_FixedLengthMustBePositive()
    {
        var field = new BinaryFieldAttribute();

        Assert.Throws<ArgumentOutOfRangeException>(() => field.FixedLength = 0);
    }

    [Fact]
    public void BinaryContract_SizeMustBePositive()
    {
        var contract = new BinaryContractAttribute();

        Assert.Throws<ArgumentOutOfRangeException>(() => contract.Size = 0);
    }

    [Fact]
    public void BinaryField_ValueCodecNameCannotBeWhitespace()
    {
        var field = new BinaryFieldAttribute();

        Assert.Throws<ArgumentException>(() => field.ValueCodecName = " ");
    }

    [Fact]
    public void Serialize_IgnoredField_IsExcludedFromPayload()
    {
        var payload = BinarySerializer.SerializeObject(new IgnoredFieldPacket { Included = 7, Ignored = 99 });
        var result = BinarySerializer.DeserializeObject<IgnoredFieldPacket>(payload);

        Assert.Equal(new byte[] { 7 }, payload);
        Assert.Equal(7, result.Included);
        Assert.Equal(0, result.Ignored);
    }

    [Fact]
    public void Serialize_ReadOnlyAttributedProperty_ThrowsConfigurationException()
    {
        Assert.Throws<CodecNotFoundException>(() => BinarySerializer.SerializeObject(new ReadOnlyPropertyPacket()));
    }

    [Fact]
    public void SerializeAndDeserialize_Enum_UsesItsUnderlyingType()
    {
        var source = new EnumPacket { Value = PacketState.Ready };

        var bytes = BinarySerializer.SerializeObject(source);
        var result = BinarySerializer.DeserializeObject<EnumPacket>(bytes);

        Assert.Equal(new byte[] { 1, 0, 0, 0 }, bytes);
        Assert.Equal(source.Value, result.Value);
    }

    [Fact]
    public void Serialize_NestedTypeWithoutPublicParameterlessConstructor_ThrowsConfigurationException()
    {
        Assert.Throws<CodecNotFoundException>(() => BinarySerializer.SerializeObject(new NonCreatableChildPacket { Child = new NonCreatableChild(1) }));
    }

    [Fact]
    public void Serialize_ListLengthTooLargeForPrefix_Throws()
    {
        var source = new OneByteLengthList { Values = Enumerable.Repeat((byte)1, 256).ToList() };

        Assert.Throws<ArgumentOutOfRangeException>(() => BinarySerializer.SerializeObject(source));
    }

    [Fact]
    public void Serialize_UnsupportedNestedList_ThrowsNotSupportedException()
    {
        Assert.Throws<CodecNotFoundException>(() => BinarySerializer.SerializeObject(new UnsupportedNestedListPacket()));
    }

    [Fact]
    public void Serialize_CyclicObjectGraph_ThrowsNotSupportedException()
    {
        Assert.Throws<CodecNotFoundException>(() => BinarySerializer.SerializeObject(new CyclicPacket { Node = new CyclicNode() }));
    }

    [Fact]
    public void Serialize_NestedTypeWithUnsupportedProperty_ThrowsNotSupportedException()
    {
        Assert.Throws<CodecNotFoundException>(() => BinarySerializer.SerializeObject(new OuterPacketWithBadNested()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(new byte[0])]
    public void Deserialize_EmptyInput_ThrowsArgumentNullException(byte[]? bytes)
    {
        Assert.ThrowsAny<ArgumentException>(() => BinarySerializer.DeserializeObject<PrimitivePacket>(bytes!));
    }

    [Fact]
    public void Deserialize_TruncatedInput_Throws()
    {
        Assert.Throws<SerializationException>(() => BinarySerializer.DeserializeObject<PrimitivePacket>(new byte[] { 1 }));
    }

    [Fact]
    public void Deserialize_NegativeVariableStringLength_Throws()
    {
        Assert.Throws<SerializationException>(() => BinarySerializer.DeserializeObject<FourByteLengthUtf8>(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }));
    }
}

[BinaryContract(EndianType = EndianType.Big)]
public class PrimitivePacket
{
    [BinaryField(1)] public bool Flag { get; set; }
    [BinaryField(2)] public int Number { get; set; }
    [BinaryField(3)] public ushort Code { get; set; }
    [BinaryField(4)] public decimal Amount { get; set; }
}

[BinaryContract(EndianType = EndianType.Big)]
public class CharacterPacket
{
    [BinaryField(1)] public sbyte SignedValue { get; set; }
    [BinaryField(2)] public char Character { get; set; }
}

[BinaryContract]
public class SameOrderPacket
{
    [BinaryField] public byte First { get; set; }
    [BinaryField] public ushort Second { get; set; }
}

[BinaryContract]
public class ComplexPacket
{
    [BinaryField(1)] public PacketHeader Header { get; set; } = new();
    [BinaryField(2)] public List<int> Values { get; set; } = new();
    [BinaryField(3)] public List<PacketItem> Children { get; set; } = new();
}

public class PacketHeader
{
    [BinaryField(1)] public byte Version { get; set; }
    [BinaryField(2)] public ushort Sequence { get; set; }
}

public class PacketItem
{
    [BinaryField(1)] public byte Id { get; set; }
    [BinaryField(2)] public int Value { get; set; }
}

[BinaryContract]
public sealed record RecordPacket
{
    [BinaryField(1)]
    public int Value { get; set; }
}

[BinaryContract(Size = 1)]
public class LargePayload
{
    [BinaryField(1, LengthPrefixSize = 2)] public byte[] Data { get; set; } = Array.Empty<byte>();
}

[BinaryContract(Size = 1)]
public class VariableUtf8
{
    [BinaryField(1)] public string Value { get; set; } = string.Empty;
}

[BinaryContract(Size = 1)]
public class VariableAscii
{
    [BinaryField(1)] public string Value { get; set; } = string.Empty;
}

[BinaryContract]
public class FixedCollection
{
    [BinaryField(1, FixedLength = 2)] public List<byte> Values { get; set; } = new();
}

[BinaryContract]
public class OneByteLengthList
{
    [BinaryField(1)] public List<byte> Values { get; set; } = new();
}

[BinaryContract]
public class FixedByteArray
{
    [BinaryField(1, FixedLength = 3)] public byte[] Value { get; set; } = Array.Empty<byte>();
}

[BinaryContract]
public class VariableByteArray
{
    [BinaryField(1)] public byte[] Value { get; set; } = Array.Empty<byte>();
}

[BinaryContract]
public class UnsupportedNestedListPacket
{
    [BinaryField(1)]
    public List<List<int>> Values { get; set; } = new();
}

[BinaryContract]
public class CyclicPacket
{
    [BinaryField(1)] public CyclicNode Node { get; set; } = new();
}

public class CyclicNode
{
    [BinaryField(1)] public CyclicNode? Next { get; set; }
}

[BinaryContract]
public class OuterPacketWithBadNested
{
    [BinaryField(1)] public InnerPacketWithBadNested Inner { get; set; } = new();
}

public class InnerPacketWithBadNested
{
    [BinaryField(1)] public List<List<int>> Bad { get; set; } = new();
}

[BinaryContract]
public class FourByteLengthUtf8
{
    [BinaryField(1, LengthPrefixSize = 4)] public string Value { get; set; } = string.Empty;
}

[BinaryContract]
public class ReadOnlyPropertyPacket
{
    [BinaryField(1)] public byte Value => 1;
}

[BinaryContract]
public class IgnoredFieldPacket
{
    [BinaryField(1)] public byte Included { get; set; }
    [BinaryField(2, Ignore = true)] public int Ignored { get; set; }
}

[BinaryContract]
public class EnumPacket
{
    [BinaryField(1)] public PacketState Value { get; set; }
}

public enum PacketState
{
    Unknown,
    Ready
}

[BinaryContract]
public class NonCreatableChildPacket
{
    [BinaryField(1)] public NonCreatableChild Child { get; set; } = new(0);
}

public class NonCreatableChild
{
    public NonCreatableChild(int value) => Value = value;
    [BinaryField(1)] public int Value { get; set; }
}
