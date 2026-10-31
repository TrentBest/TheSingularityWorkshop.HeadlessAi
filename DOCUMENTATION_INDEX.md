<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# HeadlessAi — Documentation

HeadlessAi gives a .NET application a direct HTTP path to an AI-capable endpoint. The host supplies input and owns credentials, policy, and any action taken from the response.

## Start here

| Your goal | Read |
|---|---|
| Understand the problem and the boundary | [README](README.md) · [What is HeadlessAi?](docs/WHAT_IS_HEADLESSAI.md) |
| Make a request with the endpoint API | [Usage guide](docs/usage.md) |
| Create, resume, compare, and persist versioned agent turns | [Agent turns and resumability](docs/agent-turns-and-resumability.md) |
| Choose a supported endpoint protocol | [Provider adapters](docs/provider-adapters.md) |
| Understand response extraction and failures | [Provider failure contract](docs/provider-failure-contract.md) |
| Handle credentials, endpoint trust, and untrusted output | [Security model](docs/security.md) |
| Understand what the host owns | [Architecture boundaries](docs/architecture-boundaries.md) |
| See concrete examples across the Workshop | [Ecosystem integration](docs/ecosystem-integration.md) · [CiAi integration boundary](docs/ci-ai-integration.md) |
| Explore the system visually | [Gallery](docs/GALLERY.md) |
| Understand measured performance and its limits | [Performance](docs/performance.md) · [Benchmark methodology](docs/benchmark-methodology.md) |
| Learn the reasoning behind the API boundary | [Theory](docs/THEORY.md) · [Problem domain](docs/problem-domain.md) |


## Source-level type guides and UML

These source-checked pages explain the public core and provider-adapter contracts, with host-owned use cases. Diagrams are authored static neighborhoods, not generated repository-wide inventories or runtime traces.

| Type | Contract and purpose | UML and use cases |
|---|---|---|
| HeadlessAiAgent | [Type guide](docs/types/HeadlessAiAgent.md) | [UML](docs/UML/HeadlessAiAgentUML.md) |
| HeadlessAiAgentTemplate | [Type guide](docs/types/HeadlessAiAgentTemplate.md) | [UML](docs/UML/HeadlessAiAgentTemplateUML.md) |
| HeadlessAiProfile | [Type guide](docs/types/HeadlessAiProfile.md) | [UML](docs/UML/HeadlessAiProfileUML.md) |
| HeadlessAiInput | [Type guide](docs/types/HeadlessAiInput.md) | [UML](docs/UML/HeadlessAiInputUML.md) |
| HeadlessAiOutput | [Type guide](docs/types/HeadlessAiOutput.md) | [UML](docs/UML/HeadlessAiOutputUML.md) |
| HeadlessAiHttpException | [Type guide](docs/types/HeadlessAiHttpException.md) | [UML](docs/UML/HeadlessAiHttpExceptionUML.md) |
| IHeadlessAiAdapter | [Type guide](docs/types/IHeadlessAiAdapter.md) | [UML](docs/UML/IHeadlessAiAdapterUML.md) |
| IHeadlessAiRequestHeaderProvider | [Type guide](docs/types/IHeadlessAiRequestHeaderProvider.md) | [UML](docs/UML/IHeadlessAiRequestHeaderProviderUML.md) |
| RawTextHeadlessAiAdapter | [Type guide](docs/types/RawTextHeadlessAiAdapter.md) | [UML](docs/UML/RawTextHeadlessAiAdapterUML.md) |
| DelegateHeadlessAiAdapter | [Type guide](docs/types/DelegateHeadlessAiAdapter.md) | [UML](docs/UML/DelegateHeadlessAiAdapterUML.md) |
| JsonHeadlessAiAdapter | [Type guide](docs/types/JsonHeadlessAiAdapter.md) | [UML](docs/UML/JsonHeadlessAiAdapterUML.md) |
| OpenAiResponsesAdapter | [Type guide](docs/types/OpenAiResponsesAdapter.md) | [UML](docs/UML/OpenAiResponsesAdapterUML.md) |
| GeminiGenerateContentAdapter | [Type guide](docs/types/GeminiGenerateContentAdapter.md) | [UML](docs/UML/GeminiGenerateContentAdapterUML.md) |
| AnthropicMessagesAdapter | [Type guide](docs/types/AnthropicMessagesAdapter.md) | [UML](docs/UML/AnthropicMessagesAdapterUML.md) |

## Related Workshop tools

- [ProtocolAi](https://github.com/TrentBest/TheSingularityWorkshop.ProtocolAi) gives application-owned vocabulary values stable identities.
- [GrammarAi](https://github.com/TrentBest/TheSingularityWorkshop.GrammarAi) defines how externally owned identities can be arranged into grammar structure.
- [FSM_COS](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS) composes requested MicroBundles into a runtime assembly.

These packages solve different problems. HeadlessAi remains useful on its own; none of the related packages is required for a basic HTTP invocation.

---

<p align="center">
  <a href="https://github.com/TrentBest">GitHub</a> ·
  <a href="https://coderlegion.com/">Coder Legion</a> ·
  <a href="https://www.patreon.com/c/TheSingularityWorkshop">Patreon</a> ·
  <a href="https://www.paypal.com/donate/?hosted_button_id=3Z7263LCQMV9J">PayPal</a>
</p>

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
