using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class DecimalTests
{
    [Theory]
    [InlineData("-79228162514264337593543950335")]
    [InlineData("79228162514264337593543950335")]
    public void RoundTrip(string value)
    {
        var source = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(source, BinarySerializer.DeserializeObject<DecimalContract>(BinarySerializer.SerializeObject(new DecimalContract { Value = source })).Value);
    }
}

[BinaryContract]
public class DecimalContract { [BinaryField(1)] public decimal Value { get; set; } }
