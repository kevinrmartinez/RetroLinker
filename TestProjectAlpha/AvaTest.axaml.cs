using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;

[assembly: AvaloniaTestApplication(typeof(TestProjectAlpha.TestAppBuilder))]

namespace TestProjectAlpha;

public class AvaTest : Application
{
    public AvaTest()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<AvaTest>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}