using WheelWizard.Services;

namespace WheelWizard.Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Assert.Equal(Endpoints.RRUrl, "http://update.zplwii.xyz:8000/");
    }
}
