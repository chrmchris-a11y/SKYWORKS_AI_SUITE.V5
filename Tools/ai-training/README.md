# AI Training Center – SORA Prompt Runner

This folder contains starter assets to train/evaluate prompts for SORA 2.0/2.5 logic.

Contents:
- dataset.sora.samples.json – a small set of curated cases covering common paths and edge cases
- prompt-sora-trainer-el.txt – placeholder for the Greek training prompt (paste your latest)
- prompt-sora-trainer-en.txt – placeholder for the English translation
- prompt-runner.ps1 – a PowerShell runner scaffold; previews messages and shows how to wire an API

## Usage

1. Place your prompt text in the prompt files (EL and EN). The runner defaults to EN.
2. Optionally set environment variables:
   - AI_ENDPOINT – Your model endpoint (OpenAI-compatible /chat/completions base)
   - AI_API_KEY – API key for the endpoint
3. Run the preview:

   pwsh -File ./prompt-runner.ps1 -DatasetPath ./dataset.sora.samples.json -PromptPath ./prompt-sora-trainer-en.txt

4. To execute real calls, uncomment the Invoke-RestMethod section and adapt to your provider.

## Notes
- No secrets are committed. The runner uses environment variables.
- The dataset format is intentionally simple and mirrors your app’s SORA inputs.
- Extend the dataset with golden cases from Tests/SORAAuthoritative_TestCases.json for broader coverage.
