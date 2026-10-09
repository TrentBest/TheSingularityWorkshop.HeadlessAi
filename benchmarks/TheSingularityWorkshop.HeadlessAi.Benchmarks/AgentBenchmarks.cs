using System.Net;
using BenchmarkDotNet.Attributes;
using TheSingularityWorkshop.HeadlessAi;

namespace TheSingularityWorkshop.HeadlessAi.Benchmarks;

/// <summary>Measures local agent construction and fake-transport overhead without live provider calls.</summary>
[MemoryDiagnoser]
public class AgentBenchmarks
{
    private HttpClient _client = null!;
    private HeadlessAiProfile _profile = null!;
    private IHeadlessAiAdapter _adapter = null!;
    private HeadlessAiAgentTemplate _template = null!;
    private HeadlessAiAgent _agent = null!;
    private HeadlessAiInput _input = null!;

    [GlobalSetup]
    public void Setup()
    {
        _client = new HttpClient(new FixtureHandler());
        _profile = new HeadlessAiProfile("benchmark", new Uri("https://benchmark.invalid/invoke"));
        _adapter = new RawTextHeadlessAiAdapter();
        _template = new HeadlessAiAgentTemplate(_profile, _adapter);
        _agent = _template.CreateAgent(_client);
        _input = new HeadlessAiInput("benchmark payload");
    }

    [GlobalCleanup]
    public void Cleanup() => _client.Dispose();

    [Benchmark]
    public HeadlessAiAgent ConstructAgentDirectly() => new(_client, _profile, _adapter);

    [Benchmark]
    public HeadlessAiAgent CreateAgentFromTemplate() => _template.CreateAgent(_client);

    [Benchmark]
    public Task<HeadlessAiOutput> FakeTransportRoundTrip() => _agent.SendAsync(_input);

    private sealed class FixtureHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("fixture response")
            });
    }
}
