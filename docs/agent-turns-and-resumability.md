<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# Versioned agent turns and resumability

HeadlessAi's agent-turn contracts provide a provider-neutral, versioned record for one bounded model exchange. They preserve task identity, repository and commit provenance, CI evidence references, request fingerprints, raw provider output, and normalized proposals.

They do **not** create an autonomous agent loop. The host remains responsible for conversation storage, scheduling, retries, provider selection, authorization, repository operations, workflow state, and every consequential action.

## The boundary

| Contract | Responsibility |
|---|---|
| `HeadlessAiAgentTurnEnvelope` | Host-supplied task, conversation, repository, branch, commit, prompt, expiry, limits, and evidence references |
| `HeadlessAiAgentTurnRequest` | Validated immutable canonical request shared across provider calls, with a SHA-256 request fingerprint |
| `HeadlessAiAgentTurnExecutor` | Exactly one provider invocation through a configured `HeadlessAiAgentTemplate` and host-owned `HttpClient` |
| `HeadlessAiAgentTurnResult` | Provider-attributed result, raw response, normalized untrusted proposal, timestamps, and provenance |
| `HeadlessAiAgentTurnComparison` | Validated collection of independent provider results and explicitly attributed assessments/relationships |
| `HeadlessAiAgentTurnComparisonSnapshot` | Immutable canonical comparison snapshot and fingerprint |
| `HeadlessAiAgentTurnBinaryCodec` | Versioned binary framing for request and comparison snapshots over `FSM_Serialization.IBinaryStream` |

The envelope is data, not authority. Validation requires an isolated feature branch and rejects `master` and `development` as authorized targets. CI evidence is treated as untrusted, provenance-bearing input; HeadlessAi does not independently verify a CI provider's verdict.

## One provider turn

Create the envelope in the host, validate and snapshot it once, then pass that same request snapshot to each provider executor. Sharing the request guarantees that every provider receives identical canonical input and the same request fingerprint.

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using TheSingularityWorkshop.HeadlessAi;

var now = DateTimeOffset.UtcNow;
var envelope = new HeadlessAiAgentTurnEnvelope
{
    TaskId = "task-42",
    ConversationId = "conversation-7",
    TaskVersion = "implementation-v1",
    CorrelationId = "correlation-42",
    Repository = "owner/repository",
    AuthorizedBranch = "agent/task-42",
    BaseCommitSha = "0123456789abcdef0123456789abcdef01234567",
    Role = "implementation",
    Instructions = "Inspect the supplied task and return a bounded proposal.",
    CanonicalPrompt = "Describe the requested change and any uncertainty.",
    ExpiresAt = now.AddMinutes(30)
};

var request = HeadlessAiAgentTurnRequest.Create(envelope, now);

using var httpClient = new HttpClient();
var profile = new HeadlessAiProfile(
    id: "provider-profile",
    endpoint: new Uri("https://api.openai.com/v1/responses"),
    settings: new Dictionary<string, string> { ["model"] = "configured-model" });

var template = new HeadlessAiAgentTemplate(profile, new OpenAiResponsesAdapter());
var executor = new HeadlessAiAgentTurnExecutor(template, httpClient);
var result = await executor.ExecuteAsync(request);
```

Configure authentication through the host's request-time header provider before running this example. The adapter and endpoint shown target the OpenAI Responses API; the sample omits the credential provider for brevity.

The executor performs one invocation. It does not retry automatically. Host cancellation propagates; the envelope's timeout bounds the invocation. Transport and provider failures are normalized into result data where defined. A malformed or invalid model proposal remains non-actionable; consumers must validate all output independently.

## Resuming a conversation

The host persists the turn result and decides when a later turn is appropriate. To continue a prior exchange, supply both `PriorTurnId` and `PriorCandidateCommitSha` on the next envelope. The pair is validated together; evidence must be bound to the current candidate SHA or the prior candidate SHA.

The host should also supply a new expiry, a host-generated idempotency key where duplicate delivery is possible, and the exact task/conversation version it intends to continue. HeadlessAi does not load history, infer the next task, decide whether a previous result is still acceptable, or prevent duplicate external actions. Those remain host responsibilities.

A request snapshot is immutable after creation. Its canonical JSON payload and fingerprint are the shared input identity; they are not a provider-specific conversation store. Persist the host's conversation history and the returned turn records in the host's chosen storage system.

## Comparing providers

A comparison records independent provider results for one shared task and request fingerprint. It checks that repository, branch, commit, resume, idempotency, issue/PR, and CI evidence provenance agree across results. Provider assessments and finding relationships must identify the included provider turns and findings.

A comparison does not calculate a winner, infer consensus, or convert a model statement into a verified fact. Failed providers may have no assessment. The host interprets disagreement and decides whether to ask another provider, gather evidence, stop, or request human review.

Use `HeadlessAiAgentTurnComparisonSnapshot.Create(comparison)` to validate and capture an immutable canonical snapshot. The snapshot bounds serialized output at 40,000,000 bytes and computes a SHA-256 fingerprint over its canonical payload.

## Binary persistence boundary

The binary codec uses the stable `TheSingularityWorkshop.FSM_Serialization` package and its `IBinaryStream` abstraction. HeadlessAi does not define a competing stream abstraction or take ownership of the host's storage system.

Each frame contains:

| Field | Size |
|---|---:|
| Magic (`HAIB`) | 4 bytes |
| Frame kind (request or comparison) | 1 byte |
| Frame version (little-endian) | 2 bytes |
| Payload length (signed little-endian) | 4 bytes |
| SHA-256 payload digest | 32 bytes |
| Canonical JSON payload | bounded |

The fixed header is 43 bytes. Request payloads are limited to 1,024,000 bytes; comparison payloads are limited to 40,000,000 bytes. Readers validate the magic, kind, version, length, digest, strict UTF-8, schema, provenance, and canonical representation. Partial stream reads are supported.

`PackRequest` / `UnpackRequest` and `PackComparison` / `UnpackComparison` operate on caller-supplied streams. The host owns stream lifetime, storage location, encryption at rest, access control, retention, and concurrency. The SHA-256 digest detects accidental corruption; it is **not a signature or authentication mechanism**. Do not treat a frame from an untrusted source as authentic merely because its digest matches.

Request unpacking requires the caller to supply the current time and revalidates expiry. An expired request cannot be resumed by simply deserializing its bytes. Comparison unpacking reconstructs and validates the snapshot before returning it.

## Limits and security responsibilities

- Keep API keys and other credentials in the host's secret provider, never in prompts, evidence summaries, context references, or persisted turn payloads.
- Treat evidence references, raw provider responses, provider metadata, assessments, and proposals as untrusted data.
- Never execute model-generated commands, patches, URLs, or tool instructions directly.
- Enforce repository permissions and branch authorization in the host; a branch name inside an envelope is a validated claim, not a GitHub permission grant.
- Apply storage encryption, retention, and authorization outside HeadlessAi.
- Configure provider-specific HTTP behavior, endpoint allowlists, and redirect policy on the host-owned transport.
- Do not infer a successful CI or workflow outcome from a model assessment.

For the existing endpoint/profile API, credentials, provider adapters, and transport guidance, see the [Usage Guide](usage.md) and [Security Model](security.md).

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
