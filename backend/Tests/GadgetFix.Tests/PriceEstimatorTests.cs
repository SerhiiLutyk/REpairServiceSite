using GadgetFix.AI.Api;
using Xunit;

namespace GadgetFix.Tests;

public class PriceEstimatorTests
{
    private readonly PriceEstimator _estimator = new();

    [Fact]
    public void ScreenRepair_IsMoreExpensive_ThanBattery()
    {
        var screen = _estimator.Estimate(new EstimateRequest("смартфон", "iPhone 12", "розбитий екран"));
        var battery = _estimator.Estimate(new EstimateRequest("смартфон", "iPhone 12", "швидко сідає акумулятор"));

        Assert.True(screen.Max > battery.Max);
    }

    [Fact]
    public void Estimate_ReturnsValidRange()
    {
        var result = _estimator.Estimate(new EstimateRequest("ноутбук", null, "не вмикається"));

        Assert.True(result.Min > 0);
        Assert.True(result.Max >= result.Min);
        Assert.Equal("грн", result.Currency);
    }

    [Fact]
    public void UnknownProblem_HasLowerConfidence()
    {
        var vague = _estimator.Estimate(new EstimateRequest("планшет", null, "щось не так"));
        var clear = _estimator.Estimate(new EstimateRequest("планшет", null, "тріснуло скло дисплея"));

        Assert.True(clear.Confidence > vague.Confidence);
    }

    [Theory]
    [InlineData("смартфон")]
    [InlineData("ноутбук")]
    [InlineData("планшет")]
    [InlineData("годинник")]
    public void AllDeviceTypes_ProduceEstimate(string device)
    {
        var result = _estimator.Estimate(new EstimateRequest(device, null, "заміна екрана"));
        Assert.True(result.Max > 0);
    }
}
