using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class DateTimeTests
{
    [Fact]
    public void RoundTrip_DefaultDateTimeBinaryEncoding()
    {
        var value = new DateTime(2024, 6, 1, 12, 30, 45, DateTimeKind.Local);
        Assert.Equal(value, BinarySerializer.DeserializeObject<DateTimeContract>(BinarySerializer.SerializeObject(new DateTimeContract { Value = value })).Value);
    }
}

[BinaryContract]
public class DateTimeContract { [BinaryField(1)] public DateTime Value { get; set; } }
