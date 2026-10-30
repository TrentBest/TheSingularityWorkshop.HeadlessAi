using Xunit;

namespace TheSingularityWorkshop.HeadlessAi.Tests;

public sealed class HeadlessAiAgentTurnContractTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);
    private const string Sha = "0123456789abcdef0123456789abcdef01234567";

    private static HeadlessAiAgentTurnEnvelope ValidEnvelope() => new()
    {
        TaskId = "task-42",
        CorrelationId = "corr-42",
        Repository = "TrentBest/Example",
        AuthorizedBranch = "agent/fix-ci",
        BaseCommitSha = Sha,
        CandidateCommitSha = Sha,
        IssueUrl = "https://github.com/TrentBest/Example/issues/42",
        Role = "implementation-agent",
        Instructions = "Inspect the CI evidence and propose a bounded repair.",
        CanonicalPrompt = "The test job failed. Diagnose the failure.",
        ExpiresAt = Now.AddMinutes(30),
        Evidence =
        [
            new HeadlessAiEvidenceReference
            {
                Repository = "TrentBest/Example",
                CommitSha = Sha,
                WorkflowRunId = "123456",
                Attempt = 2,
                WorkflowName = "Build and Test",
                JobName = "test",
                Conclusion = "failure",
                SourceUrl = "https://github.com/TrentBest/Example/actions/runs/123456",
                ObservedAt = Now,
                BoundedSummary = "One assertion failed."
            }
        ]
    };

    [Fact]
    public void Envelope_AcceptsVersionedBoundedEvidenceWithMatchingProvenance()
    {
        var envelope = ValidEnvelope();

        envelope.Validate(Now);

        Assert.Equal(HeadlessAiAgentTurnEnvelope.CurrentVersion, envelope.Version);
        Assert.Equal("123456", envelope.Evidence[0].WorkflowRunId);
    }

    [Fact]
    public void Envelope_RejectsExpiredTurn()
    {
        var envelope = ValidEnvelope() with { ExpiresAt = Now };

        Assert.Throws<ArgumentException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsUnsupportedVersion()
    {
        var envelope = ValidEnvelope() with { Version = 99 };

        Assert.Throws<ArgumentOutOfRangeException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsShortCommitSha()
    {
        var envelope = ValidEnvelope() with { BaseCommitSha = "abc123" };

        Assert.Throws<ArgumentException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsEvidenceFromDifferentRepository()
    {
        var envelope = ValidEnvelope() with
        {
            Evidence =
            [
                new HeadlessAiEvidenceReference
                {
                    Repository = "attacker/Other",
                    CommitSha = Sha,
                    WorkflowRunId = "123456",
                    Attempt = 1,
                    WorkflowName = "Build",
                    Conclusion = "success",
                    SourceUrl = "https://example.test/evidence",
                    ObservedAt = Now
                }
            ]
        };

        Assert.Throws<ArgumentException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsNonHttpsEvidenceReference()
    {
        var envelope = ValidEnvelope() with
        {
            Evidence =
            [
                new HeadlessAiEvidenceReference
                {
                    Repository = "TrentBest/Example",
                    CommitSha = Sha,
                    WorkflowRunId = "123456",
                    Attempt = 1,
                    WorkflowName = "Build",
                    Conclusion = "failure",
                    SourceUrl = "http://example.test/evidence",
                    ObservedAt = Now
                }
            ]
        };

        Assert.Throws<ArgumentException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Proposal_RejectsConfidenceOutsideZeroToOne()
    {
        var proposal = new HeadlessAiAgentTurnProposal { Confidence = 1.1 };

        Assert.Throws<ArgumentOutOfRangeException>(proposal.Validate);
    }

    [Fact]
    public void Proposal_AllowsUncertaintyAndEscalationWithoutExecutableAction()
    {
        var proposal = new HeadlessAiAgentTurnProposal
        {
            Diagnosis = "The workflow failed because a required secret is unavailable.",
            Confidence = 0.82,
            Uncertainty = "The repository host configuration is not visible.",
            OperationalStatus = "blocked",
            StopOrEscalateReason = "Required authorization is unavailable.",
            HumanDecisionRequired = "Restore the CI secret through the approved host settings."
        };

        proposal.Validate();

        Assert.Equal("blocked", proposal.OperationalStatus);
        Assert.Equal("Restore the CI secret through the approved host settings.", proposal.HumanDecisionRequired);
    }

    [Fact]
    public void Result_RequiresRawResponseAndKeepsProviderMetadataSeparate()
    {
        var result = new HeadlessAiAgentTurnResult
        {
            TurnId = "turn-1",
            TaskId = "task-42",
            CorrelationId = "corr-42",
            ProviderId = "fixture",
            ModelId = "fixture-model",
            RequestFingerprint = "sha256:request",
            StartedAt = Now,
            CompletedAt = Now.AddSeconds(1),
            RawResponse = "{\"diagnosis\":\"test failure\"}",
            Proposal = new HeadlessAiAgentTurnProposal { Diagnosis = "test failure" },
            ProviderMetadata = new Dictionary<string, string> { ["response_id"] = "resp-1" },
            FinishReason = "completed",
            InputTokens = 10,
            OutputTokens = 5
        };

        result.Validate();

        Assert.Equal("{\"diagnosis\":\"test failure\"}", result.RawResponse);
        Assert.Equal("resp-1", result.ProviderMetadata["response_id"]);
    }
}
