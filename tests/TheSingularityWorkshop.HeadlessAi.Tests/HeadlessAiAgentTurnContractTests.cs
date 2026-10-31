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
    public void Envelope_RejectsOversizedTaskUrl()
    {
        var envelope = ValidEnvelope() with
        {
            IssueUrl = "https://example.com/" + new string('a', 2_048)
        };

        Assert.Throws<ArgumentException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsConversationIdentityAboveBound()
    {
        var envelope = ValidEnvelope() with { ConversationId = new string('c', 129) };

        Assert.Throws<ArgumentException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsTaskVersionAboveBound()
    {
        var envelope = ValidEnvelope() with { TaskVersion = new string('v', 129) };

        Assert.Throws<ArgumentException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsBlankOptionalConversationAndTaskVersions()
    {
        Assert.Throws<ArgumentException>(() => (ValidEnvelope() with { ConversationId = " " }).Validate(Now));
        Assert.Throws<ArgumentException>(() => (ValidEnvelope() with { TaskVersion = string.Empty }).Validate(Now));
    }

    [Fact]
    public void Envelope_AcceptsTurnWithoutCandidateOrOptionalEvidenceDetails()
    {
        var envelope = ValidEnvelope() with
        {
            CandidateCommitSha = null,
            Evidence = []
        };

        envelope.Validate(Now);
    }

    [Fact]
    public void Envelope_AcceptsEvidenceWithoutOptionalJobStepOrSummary()
    {
        var evidence = ValidEnvelope().Evidence[0] with
        {
            JobName = null,
            StepName = null,
            BoundedSummary = null
        };

        (ValidEnvelope() with { Evidence = [evidence] }).Validate(Now);
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
    public void Envelope_RejectsCanonicalBranches()
    {
        var envelope = ValidEnvelope() with { AuthorizedBranch = "development" };

        Assert.Throws<ArgumentException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsInvalidResourceLimits()
    {
        var envelope = ValidEnvelope() with
        {
            Limits = new HeadlessAiAgentTurnLimits { Timeout = TimeSpan.Zero }
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsNullEvidenceCollection()
    {
        var envelope = ValidEnvelope() with { Evidence = null! };

        Assert.Throws<ArgumentNullException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsBearerCredentialInPrompt()
    {
        var envelope = ValidEnvelope() with { CanonicalPrompt = "Authorization: Bearer abcdefghijklmnop" };

        Assert.Throws<ArgumentException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsCredentialInEvidenceSummary()
    {
        var envelope = ValidEnvelope() with
        {
            Evidence =
            [
                ValidEnvelope().Evidence[0] with { BoundedSummary = "api_key=abcdefghijklmnop" }
            ]
        };

        Assert.Throws<ArgumentException>(() => envelope.Validate(Now));
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
    public void Envelope_RejectsEvidenceForUnrelatedCommit()
    {
        var envelope = ValidEnvelope() with
        {
            Evidence =
            [
                ValidEnvelope().Evidence[0] with
                {
                    CommitSha = "abcdefabcdefabcdefabcdefabcdefabcdefabcd"
                }
            ]
        };

        Assert.Throws<ArgumentException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Envelope_RequiresPriorTurnAndPriorCandidateTogether()
    {
        var envelope = ValidEnvelope() with { PriorTurnId = "turn-before" };

        Assert.Throws<ArgumentException>(() => envelope.Validate(Now));
    }

    [Fact]
    public void Envelope_AllowsPriorEvidenceWhenResumingWithNewCandidate()
    {
        var priorSha = "abcdefabcdefabcdefabcdefabcdefabcdefabcd";
        var envelope = ValidEnvelope() with
        {
            PriorTurnId = "turn-before",
            PriorCandidateCommitSha = priorSha,
            Evidence =
            [
                ValidEnvelope().Evidence[0] with { CommitSha = priorSha }
            ]
        };

        envelope.Validate(Now);
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
    public void Proposal_RejectsNaNConfidenceAndNullQuestions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HeadlessAiAgentTurnProposal
        {
            Confidence = double.NaN
        }.Validate());
        Assert.Throws<ArgumentNullException>(() => new HeadlessAiAgentTurnProposal
        {
            Questions = null!
        }.Validate());
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
            CurrentAction = "Waiting for the host to restore the CI secret.",
            Blocker = "The CI credential is unavailable to the host.",
            NextExpectedUpdate = "After the host confirms the secret has been restored.",
            StopOrEscalateReason = "Required authorization is unavailable.",
            HumanDecisionRequired = "Restore the CI secret through the approved host settings."
        };

        proposal.Validate();

        Assert.Equal("blocked", proposal.OperationalStatus);
        Assert.Equal("Waiting for the host to restore the CI secret.", proposal.CurrentAction);
        Assert.Equal("The CI credential is unavailable to the host.", proposal.Blocker);
        Assert.Equal("After the host confirms the secret has been restored.", proposal.NextExpectedUpdate);
        Assert.Equal("Restore the CI secret through the approved host settings.", proposal.HumanDecisionRequired);
    }

    [Fact]
    public void Proposal_RejectsOversizedOperationalStatusAndCurrentAction()
    {
        Assert.Throws<ArgumentException>(() => new HeadlessAiAgentTurnProposal
        {
            OperationalStatus = new string('x', 2_049)
        }.Validate());
        Assert.Throws<ArgumentException>(() => new HeadlessAiAgentTurnProposal
        {
            CurrentAction = new string('x', 2_049)
        }.Validate());
        Assert.Throws<ArgumentException>(() => new HeadlessAiAgentTurnProposal
        {
            Blocker = new string('x', 4_097)
        }.Validate());
        Assert.Throws<ArgumentException>(() => new HeadlessAiAgentTurnProposal
        {
            NextExpectedUpdate = new string('x', 2_049)
        }.Validate());
    }

    [Fact]
    public void Result_RequiresRawResponseAndKeepsProviderMetadataSeparate()
    {
        var result = new HeadlessAiAgentTurnResult
        {
            TurnId = "turn-1",
            TaskId = "task-42",
            CorrelationId = "corr-42",
            Repository = "TrentBest/Example",
            AuthorizedBranch = "agent/fix-ci",
            BaseCommitSha = Sha,
            ProviderId = "fixture",
            ModelId = "fixture-model",
            RequestFingerprint = new string('a', 64),
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


    [Fact]
    public void Envelope_RejectsMalformedRepositoryAndExpiryBeyondConfiguredLifetime()
    {
        Assert.Throws<ArgumentException>(() => (ValidEnvelope() with { Repository = "owner/name/extra" }).Validate(Now));
        Assert.Throws<ArgumentException>(() => (ValidEnvelope() with
        {
            ExpiresAt = Now.AddHours(3)
        }).Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsCredentialInResponseSchemaAndContextReference()
    {
        Assert.Throws<ArgumentException>(() => (ValidEnvelope() with
        {
            ResponseSchema = "api_key=abcdefghijklmnop"
        }).Validate(Now));
        Assert.Throws<ArgumentException>(() => (ValidEnvelope() with
        {
            ContextReferences = ["https://example.test/context?access_token=abcdefghijklmnop"]
        }).Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsOversizedContextAndEvidenceCollections()
    {
        var tooManyContext = Enumerable.Repeat("context://ref", 129).ToArray();
        Assert.Throws<ArgumentException>(() => (ValidEnvelope() with
        {
            ContextReferences = tooManyContext
        }).Validate(Now));

        var tooManyEvidence = Enumerable.Range(0, 65)
            .Select(index => ValidEnvelope().Evidence[0] with { WorkflowRunId = "run-" + index })
            .ToArray();
        Assert.Throws<ArgumentException>(() => (ValidEnvelope() with
        {
            Evidence = tooManyEvidence
        }).Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsInvalidIssueAndPullRequestUrls()
    {
        Assert.Throws<ArgumentException>(() => (ValidEnvelope() with { IssueUrl = "file:///tmp/issue" }).Validate(Now));
        Assert.Throws<ArgumentException>(() => (ValidEnvelope() with { PullRequestUrl = "http://example.test/pr/1" }).Validate(Now));
    }

    [Fact]
    public void Envelope_RejectsOversizedIdempotencyKeyAndContextReference()
    {
        Assert.Throws<ArgumentException>(() => (ValidEnvelope() with
        {
            IdempotencyKey = new string('k', 257)
        }).Validate(Now));
        Assert.Throws<ArgumentException>(() => (ValidEnvelope() with
        {
            ContextReferences = [new string('x', 2_049)]
        }).Validate(Now));
    }

    [Fact]
    public void Limits_RejectsNonPositiveBoundsAndAllowsDocumentedZeroCounts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => (ValidEnvelope() with
        {
            Limits = new HeadlessAiAgentTurnLimits { MaxPromptCharacters = 0 }
        }).Validate(Now));
        Assert.Throws<ArgumentOutOfRangeException>(() => (ValidEnvelope() with
        {
            Limits = new HeadlessAiAgentTurnLimits { MaxProviderAttempts = 0 }
        }).Validate(Now));
        (ValidEnvelope() with
        {
            Evidence = [],
            ContextReferences = [],
            Limits = new HeadlessAiAgentTurnLimits { MaxEvidenceItems = 0, MaxContextReferences = 0 }
        }).Validate(Now);
    }

    [Fact]
    public void Limits_RejectsNegativeCountsAndNonPositiveSizeOrLifetimeBounds()
    {
        HeadlessAiAgentTurnLimits[] invalidLimits =
        [
            new() { MaxEvidenceItems = -1 },
            new() { MaxContextReferences = -1 },
            new() { MaxReferenceCharacters = 0 },
            new() { MaxSerializedEnvelopeCharacters = 0 },
            new() { MaxLifetime = TimeSpan.Zero },
            new() { Timeout = TimeSpan.FromTicks(-1) },
            new() { MaxConcurrentProviders = 0 }
        ];

        foreach (var limits in invalidLimits)
            Assert.Throws<ArgumentOutOfRangeException>(() => (ValidEnvelope() with { Limits = limits }).Validate(Now));
    }

    [Fact]
    public void Evidence_RejectsInvalidAttemptAndOversizedSummary()
    {
        var invalidAttempt = ValidEnvelope().Evidence[0] with { Attempt = 0 };
        Assert.Throws<ArgumentOutOfRangeException>(() => (ValidEnvelope() with { Evidence = [invalidAttempt] }).Validate(Now));

        var oversized = ValidEnvelope().Evidence[0] with { BoundedSummary = new string('x', 16_385) };
        Assert.Throws<ArgumentException>(() => (ValidEnvelope() with { Evidence = [oversized] }).Validate(Now));
    }

    [Fact]
    public void Result_RejectsNullEvidenceMetadataAndProposalCollections()
    {
        Assert.Throws<ArgumentNullException>(() => (ValidResult() with { Evidence = null! }).Validate());
        Assert.Throws<ArgumentNullException>(() => (ValidResult() with { ProviderMetadata = null! }).Validate());
        Assert.Throws<ArgumentNullException>(() => (ValidResult() with { Proposal = null! }).Validate());
    }

    [Fact]
    public void Result_RejectsInvalidConversationTaskAndConfigurationIdentities()
    {
        Assert.Throws<ArgumentException>(() => (ValidResult() with { ConversationId = " " }).Validate());
        Assert.Throws<ArgumentException>(() => (ValidResult() with { TaskVersion = " " }).Validate());
        Assert.Throws<ArgumentException>(() => (ValidResult() with { ConfigurationId = " " }).Validate());
        Assert.Throws<ArgumentException>(() => (ValidResult() with
        {
            ConfigurationId = new string('c', 129)
        }).Validate());
    }

    [Fact]
    public void Result_RejectsMalformedRequestFingerprint()
    {
        Assert.Throws<ArgumentException>(() => (ValidResult() with
        {
            RequestFingerprint = "sha256:not-a-full-digest"
        }).Validate());
    }

    [Fact]
    public void Result_RejectsPriorProvenanceMismatchAndNegativeTokenUsage()
    {
        Assert.Throws<ArgumentException>(() => (ValidResult() with
        {
            PriorTurnId = "prior-turn"
        }).Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() => (ValidResult() with
        {
            InputTokens = -1
        }).Validate());
    }

    [Fact]
    public void Result_RejectsEmptyMetadataKeysAndNullMetadataValues()
    {
        Assert.Throws<ArgumentException>(() => (ValidResult() with
        {
            ProviderMetadata = new Dictionary<string, string> { [string.Empty] = "value" }
        }).Validate());
        Assert.Throws<ArgumentException>(() => (ValidResult() with
        {
            ProviderMetadata = new Dictionary<string, string> { ["null-value"] = null! }
        }).Validate());
    }

    [Fact]
    public void Result_RejectsOversizedRawResponseAndProviderMetadata()
    {
        Assert.Throws<ArgumentException>(() => (ValidResult() with
        {
            RawResponse = new string('x', 1_000_001)
        }).Validate());
        Assert.Throws<ArgumentException>(() => (ValidResult() with
        {
            ProviderMetadata = new Dictionary<string, string>
            {
                ["oversized"] = new string('x', 4_097)
            }
        }).Validate());
    }

    [Fact]
    public void Result_RejectsEvidenceNotBoundToCurrentOrPriorCandidate()
    {
        var result = ValidResult() with
        {
            CandidateCommitSha = Sha,
            Evidence =
            [
                ValidEnvelope().Evidence[0] with
                {
                    CommitSha = "abcdefabcdefabcdefabcdefabcdefabcdefabcd"
                }
            ]
        };

        Assert.Throws<ArgumentException>(result.Validate);
    }

    private static HeadlessAiAgentTurnResult ValidResult() => new()
    {
        TurnId = "turn-valid",
        TaskId = "task-42",
        CorrelationId = "corr-42",
        Repository = "TrentBest/Example",
        AuthorizedBranch = "agent/fix-ci",
        BaseCommitSha = Sha,
        ProviderId = "fixture",
        ModelId = "fixture-model",
        RequestFingerprint = new string('a', 64),
        StartedAt = Now,
        CompletedAt = Now.AddSeconds(1),
        RawResponse = "{}",
        Proposal = new HeadlessAiAgentTurnProposal { Diagnosis = "A bounded diagnosis." },
        FinishReason = "completed"
    };

}
