# Fluent endpoint configuration: design direction

> **Status: proposed API direction, not a shipped API.** The current source configures a `HeadlessAiProfile` and `HeadlessAiAgentTemplate` through constructors. This document records the intended usability direction without pretending those fluent methods already exist.

## The simple promise

A host should be able to say, in its own application language:

1. where the request should go;
2. which wire protocol describes that endpoint;
3. which non-secret options are needed;
4. how credentials are supplied securely;
5. what limits apply to the request; and
6. then send input and receive output.

The core interaction remains **input in, response out**. Configuration should be easy to read and reuse; the request/response protocol should be replaceable; and the host retains control over interpretation, validation, permissions, memory, budgets, and actions.

## Keep the model out of the core

HeadlessAi should not require an LLM brand, model catalogue, model SDK, or model-specific object graph to exist in its core abstraction. A model identifier, when a remote protocol requires one, is endpoint configuration data—not a HeadlessAi type or an implicit promise that the model exists.

A provider protocol is a separate concern from a model identity:

- **Endpoint** says where the HTTP request is sent.
- **Protocol adapter** says how that endpoint expects a request and represents its response.
- **Settings** carry endpoint-specific, non-secret values without forcing them into the core domain.
- **Credential source** supplies headers at invocation time and remains host-owned.
- **Transport policy** covers HTTP method, timeout, cancellation, response bounds, and host-level outbound restrictions.

A URL alone cannot reliably select a protocol. A fluent API must make the protocol explicit, or use a registered protocol definition with an unambiguous identity. It must not silently infer that different providers share the same request body.

## Illustrative shape—not compilable current API

The following is a design sketch to communicate the intended reading experience. The method names are illustrative and are **not currently implemented**:

```csharp
var caller = HeadlessAi.Configure()
    .Endpoint(endpointUri)
    .UsingProtocol(protocolAdapter)
    .WithSetting("model", configuredModelId)
    .WithCredentials(hostCredentialSource)
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithMaxResponseBytes(1_000_000)
    .Build(httpClient);

var response = await caller.SendAsync(input, cancellationToken);
```

The final API should be evaluated against the actual public contracts before names or signatures are committed. It should reduce setup ceremony without hiding important choices or duplicating the existing transport and adapter machinery.

## Relationship to the current implementation

Today, callers compose the same concepts explicitly:

- `HeadlessAiProfile` contains the absolute endpoint URI, HTTP method, default headers, timeout, response-size bound, and non-secret settings.
- `IHeadlessAiAdapter` maps normalized input to the endpoint's protocol and extracts the response.
- `HeadlessAiAgentTemplate` reuses the profile, adapter, and optional request-time header provider.
- `CreateAgent(HttpClient)` creates a lightweight caller using transport owned by the host.

A future fluent facade should compose these contracts rather than replace or fork their behavior. The current constructor-based API remains the truthful usage surface until the fluent facade is implemented and tested.

## FSM_REST and authoring-time configuration

FSM_REST may be useful to an editor or authoring tool that helps people describe, validate, or compose endpoint request recipes. That is an authoring-time opportunity, not a reason to make HeadlessAi runtime depend on FSM_REST. A saved recipe can describe configuration; credentials, active transport, request execution, and response data remain runtime concerns owned by their respective components.

HeadlessAi should remain usable as a small standalone .NET library. Neither FSM_REST, FSM_COS, FSM_API, ProtocolAi, nor GrammarAi should be required just to configure an endpoint and send a request.

## Acceptance criteria before this becomes a public API

- The fluent path is backed by tests and produces the same validated configuration as the existing profile/template contracts.
- Endpoint URI, method, timeout, and response bounds retain current validation guarantees.
- Protocol selection is explicit; endpoint URL changes alone never imply protocol compatibility.
- Secret material is not stored in ordinary non-secret settings or exposed in diagnostics.
- A host can inject and reuse its `HttpClient`; the fluent API does not create a hidden pool per logical caller.
- The core API does not require a provider SDK or a type for any particular LLM/model.
- Provider-specific adapters remain replaceable and can be supplied by a separate optional assembly/package in a future design if that separation proves useful.
- The public usage example is built and tested before it is described as runnable.
- The API remains small: no orchestration loop, memory store, permission system, GUI, or automatic tool execution is smuggled into configuration.

---

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong></p>
