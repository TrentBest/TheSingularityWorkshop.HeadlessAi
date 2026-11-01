<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# Provider adapters

IHeadlessAiAdapter maps normalized input to an endpoint-specific HTTP request and extracts normalized output from a successful response. The profile provides endpoint, method, default headers, timeout, response limit, and provider-specific non-secret settings.

## Built-in direct-HTTP adapters

These adapters use documented HTTP JSON contracts directly and add no provider SDK dependency. Configure the exact endpoint and credentials yourself; normal provider authentication, quotas, rate limits, and billing still apply.

### OpenAI Responses API

- Adapter: OpenAiResponsesAdapter
- Endpoint: https://api.openai.com/v1/responses
- Required profile setting: model
- Optional settings: instructions, max_output_tokens, temperature, top_p, store
- Optional per-input metadata: previous_response_id
- Authentication: provide Authorization: Bearer ... through a request-time header provider. Keep secret values out of source-controlled profile settings.
- Extracts output_text content and response/model/status/token-usage metadata.

Reference: [OpenAI Responses API](https://platform.openai.com/docs/api-reference/responses/create).

### Gemini generateContent

- Adapter: GeminiGenerateContentAdapter
- Endpoint: configure the full model-specific URL, such as https://generativelanguage.googleapis.com/v1beta/models/MODEL:generateContent
- Optional settings: system_instruction, temperature, top_p, max_output_tokens
- Authentication: provide x-goog-api-key through a request-time header provider. Keep secret values out of source-controlled profile settings.
- Extracts candidate text and usage metadata. If no candidate text is returned, it reports a blocked/no-text condition instead of inventing an empty success.

Reference: [Gemini generateContent REST API](https://ai.google.dev/api/generate-content).

### Anthropic Messages API

- Adapter: AnthropicMessagesAdapter
- Endpoint: https://api.anthropic.com/v1/messages
- Required profile settings: model, max_tokens
- Optional settings: system, temperature, top_p, top_k
- Headers: configure x-api-key and anthropic-version (for example, 2023-06-01); use a request-time header provider for the secret API key and keep it out of source-controlled profile settings.
- Extracts text blocks and message/model/stop-reason/token-usage metadata.

Some current Claude model families reject sampling parameters such as temperature or top_p. Leave them unset unless supported by the selected model.

Reference: [Anthropic Messages API](https://platform.claude.com/docs/en/api/http/messages/create).

## Custom adapter options

- RawTextHeadlessAiAdapter is a minimal UTF-8 text example, not a general LLM provider adapter.
- DelegateHeadlessAiAdapter lets a host provide full request construction and response extraction delegates.
- JsonHeadlessAiAdapter handles UTF-8 JSON serialization/parsing while leaving request shape, response extraction, and optional metadata extraction explicit.
- A dedicated adapter type is preferable when an endpoint protocol becomes stable, has multiple options, or needs extensive fixture tests.

The agent applies the profile endpoint and method after request construction, so the profile remains the authority for where the request is sent. These built-in adapters cover simple text requests only; multi-turn histories, images/audio/video, streaming, tools, and provider-specific structured-output features remain explicit future work or custom-adapter responsibilities.

## Design rule

Providers and model versions differ in message roles, system instructions, content blocks, model identifiers, generation options, tools, token accounting, and response envelopes. Do not pretend they share one schema. Validate current official provider documentation before extending an adapter.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>

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
