# What is HeadlessAi?

**HeadlessAi is a small .NET library for calling AI services over HTTP through explicit, replaceable protocol adapters.** It handles reusable endpoint configuration, provider-specific request and response mapping, and bounded HTTP invocation—without requiring a provider SDK.

HeadlessAi is the connection to an AI service, not the model and not an autonomous agent. The host application supplies the task and credentials, then decides how to validate, interpret, and use the response. HeadlessAi does not own application memory, permissions, budgets, or actions.

## Four pieces, four jobs

- **Profile — where and how to call.** It stores the endpoint, HTTP method, non-secret settings, timeout, and response limit.
- **Adapter — how to speak that endpoint's language.** Different providers expect different request bodies and return different response shapes. An adapter translates explicitly rather than pretending those differences do not exist.
- **Template — a reusable recipe.** It combines the profile, adapter, and optional request-time header provider.
- **Agent instance — one calling surface.** The host can create many lightweight instances while reusing an injected HttpClient.

A template is not a running AI, and creating a hundred instances does not automatically create a safe hundred-request concurrency policy. The host must set budgets, queues, permissions, and cancellation rules.

## What happens when a request is sent?

1. The host decides what work is appropriate and supplies the input.
2. The selected adapter constructs the endpoint-specific request.
3. HeadlessAi applies the configured endpoint, obtains any request-time headers, and sends through the injected HTTP transport.
4. The response is bounded and passed to the adapter for extraction.
5. The host decides whether the returned content is useful, valid, and safe to act on.

A successful HTTP response means the endpoint replied; it does not mean the model told the truth. A structured answer can still be wrong. Model output is data, not authority to run tools or change an application.

## Why not just use a provider SDK?

A provider SDK can be the right choice when an application needs its full feature set. HeadlessAi targets a different need: a small, replaceable HTTP invocation boundary with explicit adapters and no provider SDK dependency in the built-in text-focused path. This keeps the host's architecture from being welded to one vendor's object model.

The trade-off is deliberate: built-in adapters do not automatically expose every feature of each provider. Streaming, multimodal content, provider-specific tools, and advanced structured outputs require additional adapter work or a custom adapter.

## What about ProtocolAi and GrammarAi?

They are optional neighbors, not prerequisites. A stable vocabulary and a reusable grammar may reduce repeated explanations or improve structural consistency across many similar tasks. They may also cost more tokens than they save, or produce valid-looking answers that are semantically wrong. The [measurement plan](measurement-plan.md) describes how to compare them fairly instead of promising gains in advance.

## Where to go next

- [Usage guide](usage.md) — configure and invoke an endpoint.
- [Provider adapters](provider-adapters.md) — settings for the built-in protocols.
- [Problem domain](problem-domain.md) — the precise vocabulary and non-goals.
- [Theory](THEORY.md) — why these boundaries exist.
