using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public sealed class RobustnessTests
{
    [Fact]
    public void GeneratedCodec_RandomizedRoundTripsAreStable()
    {
        var runtime = SerializerRuntime.CreateDefault();
        var random = new Random(0x5EED);

        for (var iteration = 0; iteration < 1_000; iteration++)
        {
            var value = new RobustnessContract
            {
                Text = RandomText(random, random.Next(0, 40)),
                Values = Enumerable.Range(0, random.Next(0, 40)).Select(_ => random.Next()).ToArray()
            };

            var payload = runtime.Serialize(value);
            var decoded = runtime.Deserialize<RobustnessContract>(payload);

            Assert.Equal(value.Text, decoded.Text);
            Assert.Equal(value.Values, decoded.Values);
        }
    }

    [Fact]
    public void GeneratedCodec_DeterministicMalformedPayloadFuzzOnlyReturnsDomainErrors()
    {
        var runtime = new SerializerBuilder()
            .Configure(new SerializerOptions
            {
                MaxPayloadLength = 256,
                MaxStringLength = 64,
                MaxCollectionLength = 64
            })
            .Build();
        var random = new Random(0xBAD5EED);

        for (var iteration = 0; iteration < 2_000; iteration++)
        {
            var payload = new byte[random.Next(1, 128)];
            random.NextBytes(payload);

            try
            {
                runtime.Deserialize<RobustnessContract>(payload);
            }
            catch (SerializationException exception)
            {
                Assert.Equal(typeof(RobustnessContract), exception.ContractType);
            }
        }
    }

    [Fact]
    public void ImmutableRuntime_IsSafeForConcurrentRoundTrips()
    {
        var runtime = SerializerRuntime.CreateDefault();

        Parallel.For(0, 5_000, index =>
        {
            var source = new RobustnessContract { Text = $"item-{index}", Values = new[] { index, -index } };
            var result = runtime.Deserialize<RobustnessContract>(runtime.Serialize(source));
            Assert.Equal(source.Text, result.Text);
            Assert.Equal(source.Values, result.Values);
        });
    }

    private static string RandomText(Random random, int length)
    {
        var chars = new char[length];
        for (var i = 0; i < chars.Length; i++) chars[i] = (char)random.Next(0x20, 0xD7FF);
        return new string(chars);
    }
}

[BinaryContract]
public sealed class RobustnessContract
{
    [BinaryField(1, LengthPrefixSize = 2)]
    public string Text { get; set; } = string.Empty;

    [BinaryField(2, LengthPrefixSize = 2)]
    public int[] Values { get; set; } = Array.Empty<int>();
}
