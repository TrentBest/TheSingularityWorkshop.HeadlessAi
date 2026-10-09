# Measurement plan: token use, accuracy, and drift

The ProtocolAi + GrammarAi + HeadlessAi pipeline is a testable hypothesis.

## Experimental arms

1. Natural-language baseline.
2. ProtocolAi semantic identifiers.
3. ProtocolAi plus GrammarAi structure.
4. Compact structure plus schema validation and bounded repair.

Keep provider/model/version, task, sampling options, tool access, and output budget comparable.

## Metrics

- Input/output tokens and provider-reported usage.
- Estimated cost and wall-clock latency.
- Task success against a predeclared rubric.
- Structured-output validity and required-field completion.
- Unsupported-claim rate using independently reviewed criteria.
- Semantic drift across repeated runs.
- Retry/repair frequency and human correction time.

Use a versioned dataset with easy, ambiguous, adversarial, and out-of-distribution cases. Repeat trials, disclose sample size and variance, blind reviewers where practical, and report regressions alongside improvements. Token reduction does not imply fewer hallucinations; valid structure does not imply correct content.
