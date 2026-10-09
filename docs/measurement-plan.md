# Measurement plan: token use, accuracy, and drift

The ProtocolAi + GrammarAi + HeadlessAi pipeline is a testable hypothesis, not a guarantee. Compact identifiers can reduce repeated wording, but the model must understand their meaning and preserve the intended task.

## Experimental arms

1. **Natural-language baseline:** task and relevant definitions are expressed in ordinary language.
2. **ProtocolAi, full setup:** send the protocol vocabulary/identity definitions and compact task representation in the request.
3. **ProtocolAi, reused setup:** put the vocabulary in a stable system/developer context or provider-supported cache, then send compact task representations.
4. **ProtocolAi + GrammarAi:** add the grammar/structure definition and send a compact structured task.
5. **Protocol + Grammar + validation:** compact structure plus schema validation, explicit repair policy, and bounded retries.

Keep provider/model/version, task, sampling options, tool access, and output budget comparable. Where a compact representation needs a vocabulary or grammar declaration, include those tokens in the full-cost calculation.

## Amortization and break-even

Report both per-request and total cost over repeated tasks. If the compact representation needs an initial setup of S tokens, and saves D input tokens per request after setup, then the token-only break-even is approximately ceil(S / D) requests when D is positive. This is only a planning estimate: provider caching, output-token changes, retries, and cache read/write pricing can change the real break-even point.

Do not claim token savings from comparing only the compact payload against a baseline that includes its full explanation. Measure provider-reported input/output usage whenever available. The built-in OpenAI, Gemini, and Anthropic adapters expose normalized usage metadata when those providers return it.

## Metrics

- Input/output tokens, including cached input or cache-write/read usage where the provider reports it.
- Estimated cost and wall-clock latency.
- Task success against a predeclared rubric.
- Structured-output validity and required-field completion.
- Unsupported-claim / hallucination rate using independently reviewed criteria.
- Semantic drift from requested intent across repeated runs.
- Retry/repair frequency and human correction time.
- Setup cost and number of requests required to break even.

## Method

- Use a versioned dataset of representative tasks, not hand-picked demonstrations alone.
- Include easy, ambiguous, adversarial, and out-of-distribution cases.
- Run repeated trials and report sample size, variance, exclusions, and confidence intervals.
- Blind reviewers to experimental arm where practical.
- Decode compact outputs back into application-owned meaning and compare semantic equivalence, not merely string equality.
- Preserve raw results securely; redact prompts/responses that contain sensitive data.
- Report failures and regressions alongside average improvements.

## Interpretation

Token reduction does not imply fewer hallucinations. Structured-output validity does not imply semantic correctness. A grammar can constrain form while the content remains false. A protocol can stabilize identity while still being misunderstood or misapplied. Never claim hallucinations or drift are eliminated; report measured rates, confidence, task coverage, model/version, and known failure modes.
