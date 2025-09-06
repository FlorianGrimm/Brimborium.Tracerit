namespace Sample.WebApp.Services;

public class AngularFileServiceTests {
    [Test]
    public async Task GetPatternFromAngularPathTest() {
        await Assert.That(AngularFileService.GetPatternFromAngularPath("/a/b", true)).IsEqualTo("/a/{**rest}");
        await Assert.That(AngularFileService.GetPatternFromAngularPath("/a", true)).IsEqualTo("/a/{**rest}");
        await Assert.That(AngularFileService.GetPatternFromAngularPath("/", true)).IsEqualTo("/");
        await Assert.That(AngularFileService.GetPatternFromAngularPath("", true)).IsEqualTo("/");
    }
}
