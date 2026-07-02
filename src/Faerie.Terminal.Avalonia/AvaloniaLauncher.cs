using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using Faerie.Runtime;
using Faerie.Terminal.Headless;

namespace Faerie.Terminal.Avalonia;

/// <summary>
/// High-level host launcher that can run a game in Avalonia or headless mode with fluent
/// configuration and minimal <c>Program.cs</c> boilerplate.
/// </summary>
public static class AvaloniaLauncher
{
    /// <summary>
    /// Creates a launcher for a game factory. The factory is evaluated only when a mode starts.
    /// </summary>
    public static AvaloniaLaunchBuilder For(Func<Game> gameFactory)
    {
        ArgumentNullException.ThrowIfNull(gameFactory);
        return new AvaloniaLaunchBuilder(gameFactory);
    }
}

/// <summary>
/// Fluent launcher for choosing between Avalonia and headless execution.
/// </summary>
public sealed class AvaloniaLaunchBuilder
{
    private readonly Func<Game> _gameFactory;
    private readonly AppBuilder _appBuilder;
    private Action<AvaloniaDisplayBuilder>? _displayConfigure;
    private Func<string[], HeadlessLaunchDecision>? _headlessDecision;
    private bool _useFluentTheme = true;

    internal AvaloniaLaunchBuilder(Func<Game> gameFactory)
    {
        _gameFactory = gameFactory;
        _appBuilder = AppBuilder.Configure<LauncherApplication>().UsePlatformDetect().LogToTrace();
    }

    /// <summary>
    /// Configures the underlying Avalonia app builder.
    /// </summary>
    public AvaloniaLaunchBuilder ConfigureAppBuilder(Func<AppBuilder, AppBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _ = configure(_appBuilder);
        return this;
    }

    /// <summary>
    /// Enables or disables the default Fluent theme for the hosted Avalonia app.
    /// </summary>
    public AvaloniaLaunchBuilder UseFluentTheme(bool enabled = true)
    {
        _useFluentTheme = enabled;
        return this;
    }

    /// <summary>
    /// Configures the underlying <see cref="AvaloniaDisplayBuilder"/> used for desktop mode.
    /// </summary>
    public AvaloniaLaunchBuilder ConfigureDisplay(Action<AvaloniaDisplayBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _displayConfigure += configure;
        return this;
    }

    /// <summary>
    /// Configures a local app-data save catalog using defaults from the game title.
    /// </summary>
    public AvaloniaLaunchBuilder WithDefaultAppDataSaveCatalog(
        string? appDataFolder = null,
        string? baseName = null,
        string extension = ".json",
        SaveSlotNaming naming = SaveSlotNaming.SuffixDash,
        int maxLabelLength = 3) =>
        ConfigureDisplay(display => display.WithDefaultAppDataSaveCatalog(
            appDataFolder, baseName, extension, naming, maxLabelLength));

    /// <summary>
    /// Enables built-in headless command-line parsing (<see cref="HeadlessArgs"/>).
    /// </summary>
    public AvaloniaLaunchBuilder WithHeadlessFromArgs()
    {
        _headlessDecision = args =>
        {
            if (!HeadlessArgs.TryParse(args, out HeadlessOptions? options, out string? error))
                return HeadlessLaunchDecision.Skip;

            if (options is not null)
                return HeadlessLaunchDecision.Run(options);

            int exitCode = error == HeadlessArgs.FormatHelp() ? 0 : 1;
            return HeadlessLaunchDecision.Error(error ?? "Invalid headless options.", exitCode);
        };
        return this;
    }

    /// <summary>
    /// Enables headless mode when <paramref name="shouldRunHeadless"/> returns true, using
    /// <paramref name="optionsFactory"/> to provide runner options.
    /// </summary>
    public AvaloniaLaunchBuilder WithHeadlessRunnerWhen(
        Func<string[], bool> shouldRunHeadless,
        Func<string[], HeadlessOptions> optionsFactory)
    {
        ArgumentNullException.ThrowIfNull(shouldRunHeadless);
        ArgumentNullException.ThrowIfNull(optionsFactory);

        _headlessDecision = args =>
            shouldRunHeadless(args)
                ? HeadlessLaunchDecision.Run(optionsFactory(args))
                : HeadlessLaunchDecision.Skip;
        return this;
    }

    /// <summary>
    /// Enables fully custom headless decision logic.
    /// </summary>
    public AvaloniaLaunchBuilder WithHeadlessDecision(Func<string[], HeadlessLaunchDecision> decision)
    {
        _headlessDecision = decision ?? throw new ArgumentNullException(nameof(decision));
        return this;
    }

    /// <summary>
    /// Runs headless or Avalonia mode. Returns a process exit code.
    /// </summary>
    public int Run(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (_headlessDecision is not null)
        {
            HeadlessLaunchDecision decision = _headlessDecision(args);
            if (decision.ShouldRunHeadless)
            {
                if (decision.Options is null)
                {
                    if (!string.IsNullOrWhiteSpace(decision.Message))
                        Console.WriteLine(decision.Message);
                    return decision.ExitCode;
                }

                return HeadlessRunner.Run(_gameFactory(), decision.Options);
            }
        }

        LauncherApplication.CurrentLauncher = this;
        _appBuilder.StartWithClassicDesktopLifetime(args);
        return 0;
    }

    internal void InitializeApplication(Application app)
    {
        if (_useFluentTheme)
            app.Styles.Add(new FluentTheme());
    }

    internal void AttachMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
    {
        AvaloniaDisplayBuilder display = AvaloniaDisplay.For(_gameFactory());
        _displayConfigure?.Invoke(display);
        display.AttachToDesktop(desktop);
    }
}

/// <summary>
/// Result of deciding whether to run in headless mode.
/// </summary>
public sealed record HeadlessLaunchDecision(
    bool ShouldRunHeadless,
    HeadlessOptions? Options = null,
    string? Message = null,
    int ExitCode = 0)
{
    public static HeadlessLaunchDecision Skip { get; } = new(false);
    public static HeadlessLaunchDecision Run(HeadlessOptions options) => new(true, options);
    public static HeadlessLaunchDecision Error(string message, int exitCode = 1) =>
        new(true, null, message, exitCode);
}

internal sealed class LauncherApplication : Application
{
    internal static AvaloniaLaunchBuilder? CurrentLauncher { get; set; }

    public override void Initialize()
    {
        CurrentLauncher?.InitializeApplication(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (CurrentLauncher is { } launcher &&
            ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            launcher.AttachMainWindow(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
