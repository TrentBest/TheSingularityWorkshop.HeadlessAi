<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# What is HeadlessAi?

**HeadlessAi gives a .NET application a headless input-to-response connection to a configured AI-capable HTTP endpoint.** The host supplies the input; HeadlessAi sends the request and returns extracted response content. No browser window, GUI automation, or provider SDK is required for the current direct-HTTP path.

Think of it as the connection cable, not the computer at the other end. HeadlessAi does not contain an LLM, choose what your application should ask, or decide what a response is allowed to do.

## Four pieces, four jobs

- **Profile — where and how to call.** The current profile records an absolute endpoint URI, HTTP method, default headers, timeout, response-size bound, and provider-specific non-secret settings.
- **Adapter — how to speak the endpoint's protocol.** Different endpoints can expect different request bodies and response shapes. The adapter maps the input and extracts returned content without pretending all services share one schema.
- **Template — a reusable recipe.** It combines the profile, adapter, and optional request-time header provider.
- **Agent instance — the calling surface.** It sends an input using an injected `HttpClient`, whose lifetime remains owned by the host.

The public API configures profiles and templates through constructors; the example in the [Usage guide](usage.md) shows the complete request path.

## What happens when a request is sent?

1. The host decides what work is appropriate and supplies the input.
2. The selected adapter constructs the endpoint-specific request.
3. HeadlessAi applies the configured endpoint and method, obtains any request-time headers, and sends through the host-provided HTTP transport.
4. The response is bounded and passed to the adapter for extraction.
5. The host decides whether the returned content is useful, valid, and safe to act on.

A successful HTTP response means the endpoint replied; it does not mean the model told the truth. Model output is untrusted data, never authority to run tools or change an application.

## Provider-neutral does not mean protocol-blind

The core contracts should not require a particular LLM identity, model class, or provider SDK. A model identifier may still be a setting required by a particular remote protocol; it is configuration data, not a HeadlessAi model abstraction.

The repository currently includes explicit direct-HTTP adapters for OpenAI Responses, Gemini generateContent, and Anthropic Messages, along with raw-text, JSON, and delegate extension options. Those are concrete adapters around the generic invocation seam. They do not imply that every endpoint shares one format or that HeadlessAi core should depend on those providers' SDKs.

A URL alone cannot make two protocols interchangeable. Selecting an endpoint and selecting the protocol used to communicate with it are distinct choices.

## Why not just use a provider SDK?

A provider SDK can be the right choice when an application needs its full feature set. HeadlessAi targets a different need: a small, replaceable HTTP invocation boundary that avoids tying the host to one provider SDK. The trade-off is that built-in adapters do not automatically expose every feature of each service. Streaming, multimodal content, provider-specific tools, and advanced structured outputs require additional adapter work or a custom adapter.

## What about FSM_REST, ProtocolAi, and GrammarAi?

FSM_REST may help an editor or authoring tool create and validate endpoint recipes. That is separate from runtime invocation: HeadlessAi should not require FSM_REST just to send an HTTP request.

ProtocolAi and GrammarAi are optional companions for applications that need known semantic identities or constrained structures. They are not prerequisites for basic endpoint communication and must not become core dependencies. A parseable or grammar-valid answer can still be false or unsafe.

## Where to go next

- [Usage guide](usage.md) — use the current constructor-based API.
- [Provider adapters](provider-adapters.md) — configure the currently supported protocols.
- [Problem domain](problem-domain.md) — the precise vocabulary and non-goals.
- [Theory](THEORY.md) — why these boundaries exist.

---

<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://github.com/TrentBest.png?size=200" alt="The Singularity Workshop on GitHub" width="200">
  </a>
</p>

<p align="center">
  <a href="https://github.com/TrentBest">GitHub</a> ·
  <a href="https://coderlegion.com/">Coder Legion</a> ·
  <a href="https://www.patreon.com/c/TheSingularityWorkshop">Patreon</a> ·
  <a href="https://www.paypal.com/donate/?hosted_button_id=3Z7263LCQMV9J">PayPal</a>
  <br>
  <a href="https://github.com/TrentBest/FSM_API">FSM_API</a> ·
  <a href="https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization">FSM_Serialization</a> ·
  <a href="https://github.com/TrentBest/WebPage">WebPage</a>
</p>

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong><br><em>And because static boundaries are invitations to cause trouble.</em></p>
