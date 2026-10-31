using Xunit;

namespace TheSingularityWorkshop.HeadlessAi.Tests;

public sealed class HeadlessAiAgentTurnComparisonTests
{
    private const string Sha = "0123456789abcdef0123456789abcdef01234567";
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);

    private static HeadlessAiAgentTurnResult Result(string provider, string turnId, string fingerprint = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")
        => new()
        {
            TurnId = turnId,
            TaskId = "task-1",
            ConversationId = "conversation-1",
            TaskVersion = "task-v1",
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
        ConversationId = "conversation-1",
        TaskVersion = "task-v1",
        CorrelationId = "correlation-1",
        Repository = "TrentBest/Example",
        RequestFingerprint = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
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
    public void Comparison_RejectsBlankOptionalConversationAndTaskVersions()
    {
        Assert.Throws<ArgumentException>(() => (Comparison() with { ConversationId = " " }).Validate());
        Assert.Throws<ArgumentException>(() => (Comparison() with { TaskVersion = string.Empty }).Validate());
    }

    [Fact]
    public void Comparison_RejectsDifferentConversationIdentities()
    {
        var comparison = Comparison() with
        {
            ProviderResults =
            [
                Result("provider-a", "turn-a"),
                Result("provider-b", "turn-b") with { ConversationId = "conversation-2" }
            ]
        };

        Assert.Throws<ArgumentException>(comparison.Validate);
    }

    [Fact]
    public void Comparison_RejectsDifferentTaskVersions()
    {
        var comparison = Comparison() with
        {
            ProviderResults =
            [
                Result("provider-a", "turn-a"),
                Result("provider-b", "turn-b") with { TaskVersion = "task-v2" }
            ]
        };

        Assert.Throws<ArgumentException>(comparison.Validate);
    }

    [Fact]
    public void Comparison_RejectsMalformedRequestFingerprint()
    {
        Assert.Throws<ArgumentException>(() => (Comparison() with
        {
            RequestFingerprint = "not-a-sha256-digest"
        }).Validate());
    }

    [Fact]
    public void Comparison_RejectsDifferentRequestFingerprints()
    {
        var comparison = Comparison() with
        {
            ProviderResults = [Result("provider-a", "turn-a"), Result("provider-b", "turn-b", "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb")]
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


    [Fact]
    public void Comparison_RejectsUnsupportedVersionAndInvalidCandidateSha()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => (Comparison() with { Version = 2 }).Validate());
        Assert.Throws<ArgumentException>(() => (Comparison() with { CandidateCommitSha = "not-a-sha" }).Validate());
    }

    [Fact]
    public void Comparison_RejectsEmptyAndOversizedProviderSets()
    {
        Assert.Throws<ArgumentException>(() => (Comparison() with { ProviderResults = [] }).Validate());
        var tooMany = Enumerable.Range(0, 33)
            .Select(index => Result("provider-" + index, "turn-" + index))
            .ToArray();
        Assert.Throws<ArgumentException>(() => (Comparison() with { ProviderResults = tooMany }).Validate());
    }

    [Theory]
    [InlineData("task")]
    [InlineData("correlation")]
    [InlineData("repository")]
    [InlineData("candidate")]
    [InlineData("authorized-branch")]
    [InlineData("base-commit")]
    [InlineData("prior-turn")]
    [InlineData("idempotency-key")]
    [InlineData("issue-url")]
    [InlineData("pull-request-url")]
    [InlineData("evidence")]
    [InlineData("evidence-observed-offset")]
    public void Comparison_RejectsProviderResultWithMismatchedProvenance(string mismatch)
    {
        var first = Result("provider-a", "turn-a");
        var second = Result("provider-b", "turn-b");
        if (mismatch is "evidence" or "evidence-observed-offset")
        {
            first = first with
            {
                CandidateCommitSha = Sha,
                Evidence = [Evidence("first-provider evidence")]
            };
            second = second with
            {
                CandidateCommitSha = Sha,
                Evidence =
                [
                    mismatch == "evidence"
                        ? Evidence("different provider evidence")
                        : Evidence("first-provider evidence") with
                        {
                            ObservedAt = Now.ToOffset(TimeSpan.FromHours(-7))
                        }
                ]
            };
        }

        second = mismatch switch
        {
            "task" => second with { TaskId = "other-task" },
            "correlation" => second with { CorrelationId = "other-correlation" },
            "repository" => second with { Repository = "Other/Repository" },
            "candidate" => second with { CandidateCommitSha = Sha },
            "authorized-branch" => second with { AuthorizedBranch = "agent/other" },
            "base-commit" => second with { BaseCommitSha = new string('b', 40) },
            "prior-turn" => second with { PriorTurnId = "prior-turn", PriorCandidateCommitSha = Sha },
            "idempotency-key" => second with { IdempotencyKey = "different-idempotency-key" },
            "issue-url" => second with { IssueUrl = "https://github.com/TrentBest/Example/issues/2" },
            "pull-request-url" => second with { PullRequestUrl = "https://github.com/TrentBest/Example/pull/2" },
            _ => second
        };

        Assert.Throws<ArgumentException>(() => (Comparison() with
        {
            ProviderResults = [first, second]
        }).Validate());
    }

    [Fact]
    public void Comparison_RejectsDuplicateTurnIds()
    {
        var comparison = Comparison() with
        {
            ProviderResults = [Result("provider-a", "same-turn"), Result("provider-b", "same-turn")]
        };

        Assert.Throws<ArgumentException>(comparison.Validate);
    }

    [Fact]
    public void Comparison_RejectsAssessmentWhoseProviderDoesNotOwnTheReferencedTurn()
    {
        var wrongProvider = new HeadlessAiProviderAssessment { ProviderId = "provider-b", TurnId = "turn-a" };

        Assert.Throws<ArgumentException>(() => (Comparison() with { Assessments = [wrongProvider] }).Validate());
    }

    [Fact]
    public void Comparison_RejectsDuplicateFindingIdsAcrossAssessments()
    {
        var first = Assessment("provider-a", "turn-a", Finding("shared", "provider-a"));
        var second = Assessment("provider-b", "turn-b", Finding("shared", "provider-b"));

        Assert.Throws<ArgumentException>(() => (Comparison() with { Assessments = [first, second] }).Validate());
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Assessment_RejectsConfidenceOutsideFiniteUnitInterval(double confidence)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => (Assessment("provider-a", "turn-a") with
        {
            Confidence = confidence
        }).Validate());
    }

    [Fact]
    public void Assessment_RejectsInvalidFindingAttributionAndOversizedText()
    {
        Assert.Throws<ArgumentException>(() => Assessment("provider-a", "turn-a",
            Finding("finding", "provider-b")).Validate());
        Assert.Throws<ArgumentException>(() => Assessment("provider-a", "turn-a",
            new HeadlessAiProviderFinding
            {
                FindingId = "finding",
                ProviderId = "provider-a",
                Statement = new string('x', 8_193)
            }).Validate());
        Assert.Throws<ArgumentException>(() => (Assessment("provider-a", "turn-a") with
        {
            Uncertainty = new string('x', 4_097)
        }).Validate());
    }

    [Fact]
    public void Assessment_RejectsEmptyAndOversizedTextEntries()
    {
        Assert.Throws<ArgumentException>(() => (Assessment("provider-a", "turn-a") with
        {
            Omissions = [" "]
        }).Validate());
        Assert.Throws<ArgumentException>(() => (Assessment("provider-a", "turn-a") with
        {
            EvidenceQualityNotes = [new string('x', 4_097)]
        }).Validate());
        Assert.Throws<ArgumentException>(() => (Assessment("provider-a", "turn-a") with
        {
            ProposedNextActions = [new string('x', 2_049)]
        }).Validate());
        var tooMany = Enumerable.Repeat("note", 129).ToArray();
        Assert.Throws<ArgumentException>(() => (Assessment("provider-a", "turn-a") with
        {
            Omissions = tooMany
        }).Validate());
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("")]
    public void Relationship_RejectsUnsupportedKind(string kind)
    {
        var relationship = Relationship(kind, ["finding-a", "finding-b"]);
        var comparison = WithTwoFindings(relationship);
        Assert.Throws<ArgumentException>(comparison.Validate);
    }

    [Fact]
    public void Relationship_RejectsDuplicateOrOutOfRangeFindingIds()
    {
        Assert.Throws<ArgumentException>(WithTwoFindings(
            Relationship("agreement", ["finding-a", "finding-a"])).Validate);
        var tooMany = Enumerable.Range(0, 33).Select(index => "finding-" + index).ToArray();
        Assert.Throws<ArgumentException>(WithTwoFindings(
            Relationship("agreement", tooMany)).Validate);
    }

    [Fact]
    public void Relationship_RejectsMissingOrOversizedExplanation()
    {
        Assert.Throws<ArgumentException>(WithTwoFindings(
            Relationship("agreement", ["finding-a", "finding-b"]) with { Explanation = " " }).Validate);
        Assert.Throws<ArgumentException>(WithTwoFindings(
            Relationship("agreement", ["finding-a", "finding-b"]) with
            {
                Explanation = new string('x', 4_097)
            }).Validate);
    }

    [Fact]
    public void Comparison_RejectsNullCollectionsAndNullEntries()
    {
        Assert.Throws<ArgumentNullException>(() => (Comparison() with
        {
            ProviderResults = null!
        }).Validate());
        Assert.Throws<ArgumentNullException>(() => (Comparison() with
        {
            Assessments = [null!]
        }).Validate());
        Assert.Throws<ArgumentNullException>(() => (Comparison() with
        {
            FindingRelationships = [null!]
        }).Validate());
        Assert.Throws<ArgumentNullException>(() => (Comparison() with
        {
            ProviderResults = [null!]
        }).Validate());
    }


    [Fact]
    public void ComparisonSnapshot_IsUnaffectedByMutationsToCallerOwnedNestedCollections()
    {
        var questions = new List<string> { "Original provider question." };
        var metadata = new Dictionary<string, string> { ["finish_reason"] = "stop" };
        var firstResult = Result("provider-a", "turn-a") with
        {
            Proposal = new HeadlessAiAgentTurnProposal { Questions = questions },
            ProviderMetadata = metadata
        };
        var providerResults = new List<HeadlessAiAgentTurnResult>
        {
            firstResult,
            Result("provider-b", "turn-b")
        };
        var findingsA = new List<HeadlessAiProviderFinding>
        {
            Finding("finding-a", "provider-a")
        };
        var omissions = new List<string> { "Original omission." };
        var assessmentA = new HeadlessAiProviderAssessment
        {
            ProviderId = "provider-a",
            TurnId = "turn-a",
            Findings = findingsA,
            Omissions = omissions
        };
        var findingIds = new List<string> { "finding-a", "finding-b" };
        var relationship = Relationship("disagreement", findingIds);
        var assessments = new List<HeadlessAiProviderAssessment>
        {
            assessmentA,
            Assessment("provider-b", "turn-b", Finding("finding-b", "provider-b"))
        };
        var relationships = new List<HeadlessAiAgentTurnFindingRelationship> { relationship };
        var comparison = Comparison() with
        {
            ProviderResults = providerResults,
            Assessments = assessments,
            FindingRelationships = relationships
        };

        var snapshot = HeadlessAiAgentTurnComparisonSnapshot.Create(comparison);
        var originalPayload = snapshot.CanonicalPayload;
        var originalFingerprint = snapshot.Fingerprint;

        questions[0] = "Mutated question.";
        questions.Add("Injected question.");
        metadata["finish_reason"] = "mutated";
        metadata["injected"] = "value";
        providerResults.Clear();
        findingsA.Clear();
        omissions[0] = "Mutated omission.";
        omissions.Add("Injected omission.");
        findingIds[0] = "unknown-finding";
        findingIds.Add("injected-finding");
        assessments.Clear();
        relationships.Clear();

        Assert.Equal(originalPayload, snapshot.CanonicalPayload);
        Assert.Equal(originalFingerprint, snapshot.Fingerprint);
        Assert.Contains("Original provider question.", snapshot.CanonicalPayload);
        Assert.Contains("Original omission.", snapshot.CanonicalPayload);
        Assert.Contains("finding-a", snapshot.CanonicalPayload);
        Assert.DoesNotContain("Mutated question.", snapshot.CanonicalPayload);
        Assert.DoesNotContain("Injected omission.", snapshot.CanonicalPayload);
        Assert.DoesNotContain("unknown-finding", snapshot.CanonicalPayload);
        Assert.Equal(64, snapshot.Fingerprint.Length);
    }

    [Fact]
    public void ComparisonSnapshot_RejectsInvalidComparisonBeforeSnapshotting()
    {
        var invalid = Comparison() with
        {
            RequestFingerprint = "not-a-sha256-digest"
        };

        Assert.Throws<ArgumentException>(() => HeadlessAiAgentTurnComparisonSnapshot.Create(invalid));
    }

    private static HeadlessAiEvidenceReference Evidence(string summary)
        => new()
        {
            Repository = "TrentBest/Example",
            CommitSha = Sha,
            WorkflowRunId = "run-123",
            Attempt = 1,
            WorkflowName = "CI",
            Conclusion = "success",
            SourceUrl = "https://github.com/TrentBest/Example/actions/runs/123",
            ObservedAt = Now,
            BoundedSummary = summary
        };

    private static HeadlessAiProviderAssessment Assessment(
        string provider, string turn, params HeadlessAiProviderFinding[] findings)
        => new() { ProviderId = provider, TurnId = turn, Findings = findings };

    private static HeadlessAiProviderFinding Finding(string id, string provider)
        => new() { FindingId = id, ProviderId = provider, Statement = "A bounded provider finding." };

    private static HeadlessAiAgentTurnFindingRelationship Relationship(string kind, IReadOnlyList<string> ids)
        => new() { Kind = kind, FindingIds = ids, Explanation = "A recorded cross-provider relationship." };

    private static HeadlessAiAgentTurnComparison WithTwoFindings(HeadlessAiAgentTurnFindingRelationship relationship)
        => Comparison() with
        {
            Assessments =
            [
                Assessment("provider-a", "turn-a", Finding("finding-a", "provider-a")),
                Assessment("provider-b", "turn-b", Finding("finding-b", "provider-b"))
            ],
            FindingRelationships = [relationship]
        };

}
