using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace HaloUI.Tests.E2E;

public sealed class PlaywrightEnvironmentFixture : IAsyncLifetime
{
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan WaitForServerTimeout = TimeSpan.FromMinutes(3);

    private readonly HttpClient _httpClient = new() { Timeout = ProbeTimeout };
    private readonly ConcurrentQueue<string> _recentOutput = new();

    private Process? _demoHostProcess;
    private bool _startedByFixture;

    public string BaseUrl { get; } = ResolveBaseUrl();

    public bool AutoStartEnabled { get; } = ResolveAutoStartEnabled();

    public async Task InitializeAsync()
    {
        if (await IsServerReachableAsync())
        {
            return;
        }

        if (!AutoStartEnabled)
        {
            throw new InvalidOperationException(
                $"Playwright E2E target '{BaseUrl}' is not reachable. "
                + "Set HALOUI_E2E_AUTOSTART=true or start HaloUI.DemoHost manually.");
        }

        if (!IsLoopbackBaseUrl(BaseUrl))
        {
            throw new InvalidOperationException(
                $"HALOUI_E2E_BASE_URL '{BaseUrl}' is not reachable and cannot be auto-started because it is not a loopback address.");
        }

        StartDemoHostProcess();

        if (await WaitUntilServerReachableAsync(WaitForServerTimeout))
        {
            return;
        }

        var output = string.Join(Environment.NewLine, _recentOutput);
        throw new TimeoutException(
            $"DemoHost did not become reachable at '{BaseUrl}' within {WaitForServerTimeout}."
            + Environment.NewLine
            + output);
    }

    public Task DisposeAsync()
    {
        _httpClient.Dispose();

        if (_startedByFixture && _demoHostProcess is { HasExited: false })
        {
            try
            {
                _demoHostProcess.Kill(entireProcessTree: true);
            }
            catch
            {
                // Ignore shutdown failures to avoid hiding test outcomes.
            }
        }

        _demoHostProcess?.Dispose();
        _demoHostProcess = null;

        return Task.CompletedTask;
    }

    private void StartDemoHostProcess()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var demoHostProjectPath = Path.Combine(repositoryRoot, "HaloUI.DemoHost", "HaloUI.DemoHost.csproj");
        var arguments = $"run --project \"{demoHostProjectPath}\" --urls {BaseUrl}";

        _demoHostProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = repositoryRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        _demoHostProcess.OutputDataReceived += (_, eventArgs) => AppendOutput(eventArgs.Data);
        _demoHostProcess.ErrorDataReceived += (_, eventArgs) => AppendOutput(eventArgs.Data);

        if (!_demoHostProcess.Start())
        {
            throw new InvalidOperationException("Failed to start HaloUI.DemoHost process.");
        }

        _demoHostProcess.BeginOutputReadLine();
        _demoHostProcess.BeginErrorReadLine();
        _startedByFixture = true;
    }

    private async Task<bool> IsServerReachableAsync()
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/");
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            return response.StatusCode is >= HttpStatusCode.OK and < HttpStatusCode.InternalServerError;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    private async Task<bool> WaitUntilServerReachableAsync(TimeSpan timeout)
    {
        using var timeoutCts = new CancellationTokenSource(timeout);

        while (!timeoutCts.IsCancellationRequested)
        {
            if (_demoHostProcess is { HasExited: true })
            {
                var output = string.Join(Environment.NewLine, _recentOutput);
                throw new InvalidOperationException(
                    $"DemoHost process exited unexpectedly with code {_demoHostProcess.ExitCode}."
                    + Environment.NewLine
                    + output);
            }

            if (await IsServerReachableAsync())
            {
                return true;
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        return false;
    }

    private static string ResolveBaseUrl()
    {
        var configured = Environment.GetEnvironmentVariable("HALOUI_E2E_BASE_URL");
        var normalized = string.IsNullOrWhiteSpace(configured)
            ? "http://127.0.0.1:5210"
            : configured.Trim();

        return normalized.TrimEnd('/');
    }

    private static bool ResolveAutoStartEnabled()
    {
        var configured = Environment.GetEnvironmentVariable("HALOUI_E2E_AUTOSTART");
        if (string.IsNullOrWhiteSpace(configured))
        {
            return true;
        }

        return bool.TryParse(configured, out var value) && value;
    }

    private static string ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var solutionPath = Path.Combine(current.FullName, "HaloUI.slnx");
            if (File.Exists(solutionPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            "Unable to locate repository root. HaloUI.slnx was not found in parent directories.");
    }

    private static bool IsLoopbackBaseUrl(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return IPAddress.TryParse(uri.Host, out var ipAddress) && IPAddress.IsLoopback(ipAddress);
    }

    private void AppendOutput(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        _recentOutput.Enqueue(line);
        while (_recentOutput.Count > 200)
        {
            _recentOutput.TryDequeue(out _);
        }
    }
}
