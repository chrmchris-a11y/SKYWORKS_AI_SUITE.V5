import os
import json
import time
import argparse
import base64
from urllib import request, parse, error

"""
Embedding generator for processed chunks.

- Supports OpenAI and Azure OpenAI via environment variables.
- Falls back gracefully to writing entries with embedding=null if not configured.

Usage examples:
  py -3 Tools/TrainingCenter/embedding_generator.py \
    --input "KnowledgeBase/EASA DOCS SPLIT CHUNKS/processed_chunks/EXTRACTED_EAR_UAS_2024" \
    --output "Tools/TrainingCenter/agent_memory/embeddings/EXTRACTED_EAR_UAS_2024.jsonl"

Env (OpenAI):
  OPENAI_API_KEY=...  EMBEDDING_MODEL=text-embedding-3-small

Env (Azure OpenAI):
  AZURE_OPENAI_API_KEY=...
  AZURE_OPENAI_ENDPOINT=https://<your>.openai.azure.com
  AZURE_OPENAI_EMBEDDING_DEPLOYMENT=<deployment-name>
"""


def list_chunk_files(input_dir: str):
    for root, _, files in os.walk(input_dir):
        for f in sorted(files):
            if f.lower().endswith('.txt'):
                yield os.path.join(root, f)


def load_text(path: str) -> str:
    with open(path, 'r', encoding='utf-8') as fh:
        return fh.read()


def write_jsonl(records, out_path: str):
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    with open(out_path, 'w', encoding='utf-8') as fh:
        for r in records:
            fh.write(json.dumps(r, ensure_ascii=False) + "\n")


# --- Providers ---

def openai_embed(api_key: str, model: str, inputs: list[str]) -> list:
    url = "https://api.openai.com/v1/embeddings"
    data = json.dumps({"model": model, "input": inputs}).encode('utf-8')
    req = request.Request(url, data=data, method='POST')
    req.add_header('Content-Type', 'application/json')
    req.add_header('Authorization', f"Bearer {api_key}")
    try:
        with request.urlopen(req, timeout=60) as resp:
            payload = json.loads(resp.read().decode('utf-8'))
            return [item['embedding'] for item in payload.get('data', [])]
    except error.HTTPError as e:
        msg = e.read().decode('utf-8', errors='ignore')
        print(f"OpenAI HTTPError: {e.code} {msg}")
    except Exception as ex:
        print(f"OpenAI error: {ex}")
    return [None] * len(inputs)


def azure_openai_embed(api_key: str, endpoint: str, deployment: str, inputs: list[str]) -> list:
    url = f"{endpoint}/openai/deployments/{deployment}/embeddings?api-version=2024-02-15-preview"
    data = json.dumps({"input": inputs}).encode('utf-8')
    req = request.Request(url, data=data, method='POST')
    req.add_header('Content-Type', 'application/json')
    req.add_header('api-key', api_key)
    try:
        with request.urlopen(req, timeout=60) as resp:
            payload = json.loads(resp.read().decode('utf-8'))
            return [item['embedding'] for item in payload.get('data', [])]
    except error.HTTPError as e:
        msg = e.read().decode('utf-8', errors='ignore')
        print(f"Azure OpenAI HTTPError: {e.code} {msg}")
    except Exception as ex:
        print(f"Azure OpenAI error: {ex}")
    return [None] * len(inputs)


# --- Runner ---

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--input', required=True, help='Directory containing chunk .txt files')
    ap.add_argument('--output', required=True, help='Output JSONL path for embeddings index')
    ap.add_argument('--batch', type=int, default=8, help='Batch size for embedding calls')
    args = ap.parse_args()

    # Detect provider
    provider = None
    openai_key = os.getenv('OPENAI_API_KEY')
    openai_model = os.getenv('EMBEDDING_MODEL', 'text-embedding-3-small')
    az_key = os.getenv('AZURE_OPENAI_API_KEY')
    az_endpoint = os.getenv('AZURE_OPENAI_ENDPOINT')
    az_deploy = os.getenv('AZURE_OPENAI_EMBEDDING_DEPLOYMENT')

    if openai_key:
        provider = ('openai', openai_key, openai_model)
    elif az_key and az_endpoint and az_deploy:
        provider = ('azure', az_key, az_endpoint, az_deploy)

    records = []
    chunk_paths = list(list_chunk_files(args.input))
    print(f"Found {len(chunk_paths)} chunk files under {args.input}")

    def embed_batch(texts: list[str]):
        if not provider:
            return [None] * len(texts)
        if provider[0] == 'openai':
            return openai_embed(provider[1], provider[2], texts)
        elif provider[0] == 'azure':
            return azure_openai_embed(provider[1], provider[2], provider[3], texts)
        return [None] * len(texts)

    # Process in batches
    for i in range(0, len(chunk_paths), args.batch):
        batch_paths = chunk_paths[i:i+args.batch]
        texts = [load_text(p) for p in batch_paths]
        vectors = embed_batch(texts)
        for p, t, v in zip(batch_paths, texts, vectors):
            rel = os.path.relpath(p, args.input)
            rec = {
                'source': rel,
                'text': t,
                'embedding': v
            }
            records.append(rec)
        # Be gentle on rate limits
        time.sleep(0.5)

    write_jsonl(records, args.output)
    print(f"Wrote {len(records)} embedding records to {args.output}")
    if not provider:
        print("No embedding provider configured. Records contain embedding=null. Set env vars to enable embedding generation.")

if __name__ == '__main__':
    main()
