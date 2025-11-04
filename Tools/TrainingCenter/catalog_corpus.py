import os
import json
from datetime import datetime

BASE_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', 'KnowledgeBase', 'EASA DOCS SPLIT CHUNKS'))
PROCESSED_DIR = os.path.join(BASE_DIR, 'processed_chunks')
OUT_JSON = os.path.abspath(os.path.join(os.path.dirname(__file__), 'agent_memory', 'corpus_index.json'))
OUT_MD = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', 'Docs', 'Knowledge', 'CORPUS_INDEX.md'))


def read_header_snippet(path, lines=10):
    try:
        with open(path, 'r', encoding='utf-8') as fh:
            content_lines = []
            for i, line in enumerate(fh):
                if i >= lines:
                    break
                content_lines.append(line.rstrip('\n'))
            return content_lines
    except Exception as e:
        return [f"<error reading header: {e}>"]


def count_lines(path):
    try:
        with open(path, 'r', encoding='utf-8') as fh:
            return sum(1 for _ in fh)
    except Exception:
        return None


def count_chunks_for(base_name):
    folder = os.path.join(PROCESSED_DIR, base_name)
    if not os.path.isdir(folder):
        return 0
    total = 0
    for root, _, files in os.walk(folder):
        total += sum(1 for f in files if f.lower().endswith('.txt'))
    return total


def main():
    entries = []
    txt_files = [f for f in os.listdir(BASE_DIR) if f.lower().endswith('.txt')]
    txt_files.sort()

    for fname in txt_files:
        full_path = os.path.join(BASE_DIR, fname)
        base_name = os.path.splitext(fname)[0]
        line_count = count_lines(full_path)
        chunks = count_chunks_for(base_name)
        header = read_header_snippet(full_path, 10)
        entry = {
            'file': fname,
            'base_name': base_name,
            'path': full_path,
            'line_count': line_count,
            'chunk_count': chunks,
            'header_snippet': header,
            'official_source': None  # to be filled with authoritative URL/code when available
        }
        entries.append(entry)

    summary = {
        'generated_at': datetime.utcnow().isoformat() + 'Z',
        'base_dir': BASE_DIR,
        'processed_dir': PROCESSED_DIR,
        'document_count': len(entries),
        'total_lines': sum(e['line_count'] for e in entries if e['line_count'] is not None),
        'total_chunks': sum(e['chunk_count'] for e in entries),
        'entries': entries
    }

    # Write JSON
    os.makedirs(os.path.dirname(OUT_JSON), exist_ok=True)
    with open(OUT_JSON, 'w', encoding='utf-8') as fh:
        json.dump(summary, fh, ensure_ascii=False, indent=2)

    # Write Markdown
    os.makedirs(os.path.dirname(OUT_MD), exist_ok=True)
    with open(OUT_MD, 'w', encoding='utf-8') as fh:
        fh.write('# EASA/JARUS Corpus Index\n\n')
        fh.write(f"Generated: {summary['generated_at']}\n\n")
        fh.write(f"Base: {BASE_DIR}\n\n")
        fh.write(f"Processed: {PROCESSED_DIR}\n\n")
        fh.write(f"Documents: {summary['document_count']} — Total lines: {summary['total_lines']} — Total chunks: {summary['total_chunks']}\n\n")
        for e in entries:
            fh.write(f"## {e['file']}\n\n")
            fh.write(f"- Lines: {e['line_count']}\n")
            fh.write(f"- Chunks: {e['chunk_count']}\n")
            fh.write(f"- Base name: {e['base_name']}\n")
            fh.write(f"- Official source: {e['official_source'] or 'N/A'}\n\n")
            if e['header_snippet']:
                fh.write('> Header snippet:\n')
                for line in e['header_snippet']:
                    safe = line.replace('\t', '    ')
                    fh.write(f"> {safe}\n")
                fh.write('\n')

if __name__ == '__main__':
    main()
