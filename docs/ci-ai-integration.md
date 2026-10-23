# CiAi integration boundary

CiAi is the application-facing interface to CI facts and permitted operations. HeadlessAi is the transport seam to a configured LLM endpoint. These are complementary responsibilities, not competing abstractions.

## Request and response boundary

1. CiAi obtains run facts from the CI provider and validates the repository, run identity, commit, status, conclusion, and evidence URL.
2. The host prepares a bounded request using application-owned meanings. ProtocolAi can encode recognized terms as integer identities.
3. HeadlessAi sends the request through the selected endpoint adapter and returns normalized response content.
4. CiAi validates the response against the known vocabulary and the current host permission policy.
5. Only validated host code can perform an operation. A model response is never authorization.

HeadlessAi intentionally does not interpret protocol IDs, decide whether a CI run succeeded, or execute a command. It also does not eliminate the configured endpoint or HTTP transport; it avoids requiring one provider SDK in every consumer.

## Offline transport proof

The test suite includes a fake HTTP endpoint that verifies a finite, integer-oriented request crosses the HeadlessAi boundary unchanged and that an out-of-vocabulary response is returned as untrusted text rather than being accepted as an operation. The test needs no credentials or external endpoint.

## First CiAi milestone

CiAi currently provides a validated read-only workflow-run snapshot and a GitHub Actions REST client. Live ingestion is kept separate from LLM invocation; permissioned operations and the complete ProtocolAi/HeadlessAi composition are later milestones. See [CiAi](https://github.com/TrentBest/TheSingularityWorkshop.CiAi/tree/development) for implementation status.
