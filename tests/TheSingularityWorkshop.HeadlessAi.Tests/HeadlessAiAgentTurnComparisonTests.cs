using Xunit;

namespace TheSingularityWorkshop.HeadlessAi.Tests;

public sealed class HeadlessAiAgentTurnComparisonTests
{
    private const string Sha = "0123456789abcdef0123456789abcdef01234567";
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);

    private static HeadlessAiAgentTurnResult Result(string provider, string turnId, string fingerprint = "same-request")
        => new()
        {
            TurnId = turnId,
            TaskId = "task-1",
            CorrelationId = "correlation-1",
            Repository = "TrentBest/Example",
            AuthorizedBranch = "agent/fix",
            BaseCommitSha = Sha,
            ProviderId = provider,
            ModelId = provider + "-model",
            RequestFingerprint = fingerprint,
            StartedAt = Now,
            CompletedAt = Now.AddSeconds(1),
            RawResponse = "{}",
            Proposal = new HeadlessAiAgentTurnProposal { Diagnosis = "A provider-specific diagnosis." },
            FinishReason = "completed"
        };

    private static HeadlessAiAgentTurnComparison Comparison() => new()
    {
        TaskId = "task-1",
        CorrelationId = "correlation-1",
        Repository = "TrentBest/Example",
        RequestFingerprint = "same-request",
        ProviderResults = [Result("provider-a", "turn-a"), Result("provider-b", "turn-b")]
    };

    [Fact]
    public void Comparison_AcceptsIndependentResultsWithIdenticalCanonicalRequest()
    {
        var comparison = Comparison();

        comparison.Validate();

        Assert.Equal(2, comparison.ProviderResults.Count);
        Assert.Equal(comparison.ProviderResults[0].RequestFingerprint,
            comparison.ProviderResults[1].RequestFingerprint);
    }

    [Fact]
    public void Comparison_RejectsDifferentRequestFingerprints()
    {
        var comparison = Comparison() with
        {
            ProviderResults = [Result("provider-a", "turn-a"), Result("provider-b", "turn-b", "different-request")]
        };

        Assert.Throws<ArgumentException>(comparison.Validate);
    }

    [Fact]
    public void Comparison_PreservesPartialProviderFailureWithoutInventingAssessment()
    {
        var failed = Result("provider-b", "turn-b") with
        {
            ErrorCode = "http_429",
            FinishReason = "error",
            Proposal = new HeadlessAiAgentTurnProposal
            {
                Uncertainty = "The provider did not return a usable response.",
                StopOrEscalateReason = "The host must decide whether to retry."
            }
        };
        var comparison = Comparison() with
        {
            ProviderResults = [Result("provider-a", "turn-a"), failed],
            Assessments =
            [
                new HeadlessAiProviderAssessment
                {
                    ProviderId = "provider-a",
                    TurnId = "turn-a",
                    Findings =
                    [
                        new HeadlessAiProviderFinding
                        {
                            FindingId = "finding-a",
                            ProviderId = "provider-a",
                            Statement = "The test assertion is inconsistent with the new behavior."
                        }
                    ]
                }
            ]
        };

        comparison.Validate();

        Assert.Equal("http_429", comparison.ProviderResults[1].ErrorCode);
        Assert.Single(comparison.Assessments);
        Assert.Equal("provider-b", comparison.ProviderResults[1].ProviderId);
    }

    [Fact]
    public void Comparison_PreservesExplicitDisagreementAndMinorityFindingAttribution()
    {
        var comparison = Comparison() with
        {
            Assessments =
            [
                new HeadlessAiProviderAssessment
                {
                    ProviderId = "provider-a",
                    TurnId = "turn-a",
                    Confidence = 0.9,
                    Findings =
                    [
                        new HeadlessAiProviderFinding
                        {
                            FindingId = "finding-a",
                            ProviderId = "provider-a",
                            Statement = "The failure is caused by a regression in the implementation."
                        }
                    ],
                    Omissions = ["Did not inspect the package audit output."],
                    EvidenceQualityNotes = ["Based on the test job excerpt only."],
                    ProposedNextActions = ["Inspect the failing test and nearby implementation."]
                },
                new HeadlessAiProviderAssessment
                {
                    ProviderId = "provider-b",
                    TurnId = "turn-b",
                    Confidence = 0.62,
                    Findings =
                    [
                        new HeadlessAiProviderFinding
                        {
                            FindingId = "finding-b",
                            ProviderId = "provider-b",
                            Statement = "The failure is caused by an outdated test expectation."
                        }
                    ],
                    Uncertainty = "The complete test history was not available."
                }
            ],
            FindingRelationships =
            [
                new HeadlessAiAgentTurnFindingRelationship
                {
                    Kind = "disagreement",
                    FindingIds = ["finding-a", "finding-b"],
                    Explanation = "The providers identify different causes; neither finding is promoted to fact."
                }
            ]
        };

        comparison.Validate();

        Assert.Equal("disagreement", comparison.FindingRelationships[0].Kind);
        Assert.Equal("finding-b", comparison.Assessments[1].Findings[0].FindingId);
        Assert.Equal(0.62, comparison.Assessments[1].Confidence);
    }

    [Fact]
    public void Comparison_RejectsRelationshipThatDropsProviderAttribution()
    {
        var comparison = Comparison() with
        {
            Assessments =
            [
                new HeadlessAiProviderAssessment
                {
                    ProviderId = "provider-a",
                    TurnId = "turn-a",
                    Findings =
                    [
                        new HeadlessAiProviderFinding
                        {
                            FindingId = "finding-a",
                            ProviderId = "provider-a",
                            Statement = "Finding from provider A."
                        },
                        new HeadlessAiProviderFinding
                        {
                            FindingId = "finding-a2",
                            ProviderId = "provider-a",
                            Statement = "Another finding from provider A."
                        }
                    ]
                }
            ],
            FindingRelationships =
            [
                new HeadlessAiAgentTurnFindingRelationship
                {
                    Kind = "agreement",
                    FindingIds = ["finding-a", "finding-a2"],
                    Explanation = "Invalid: both findings came from the same provider."
                }
            ]
        };

        Assert.Throws<ArgumentException>(comparison.Validate);
    }

    [Fact]
    public void Comparison_RejectsAssessmentForUnknownTurn()
    {
        var comparison = Comparison() with
        {
            Assessments =
            [
                new HeadlessAiProviderAssessment
                {
                    ProviderId = "provider-a",
                    TurnId = "not-in-this-comparison"
                }
            ]
        };

        Assert.Throws<ArgumentException>(comparison.Validate);
    }

    [Fact]
    public void Comparison_RejectsDuplicateProviderResults()
    {
        var comparison = Comparison() with
        {
            ProviderResults = [Result("provider-a", "turn-a"), Result("provider-a", "turn-b")]
        };

        Assert.Throws<ArgumentException>(comparison.Validate);
    }

    [Fact]
    public void Comparison_RejectsRelationshipToUnknownFinding()
    {
        var comparison = Comparison() with
        {
            FindingRelationships =
            [
                new HeadlessAiAgentTurnFindingRelationship
                {
                    Kind = "agreement",
                    FindingIds = ["missing-a", "missing-b"],
                    Explanation = "Unknown findings cannot be compared."
                }
            ]
        };

        Assert.Throws<ArgumentException>(comparison.Validate);
    }
}
