# Phase1 Step5 — Skyworks V5
"""
Context Pack Generator
Builds markdown context packs from EASA/SORA corpus for AI agent training.
"""

import os
import sys
import json
import csv
import argparse
from pathlib import Path
from datetime import datetime, timezone

# --- Config Parser (with PyYAML fallback) ---
def load_config(config_path):
    """Load config.yaml with optional PyYAML or fallback parser."""
    try:
        import yaml
        with open(config_path, 'r', encoding='utf-8') as f:
            return yaml.safe_load(f)
    except ImportError:
        # Fallback: simple YAML parser (supports basic structure only)
        return parse_yaml_simple(config_path)

def parse_yaml_simple(config_path):
    """Simple YAML parser for config.yaml (no external deps)."""
    config = {
        'version': '1.0',
        'corpus_path': 'KnowledgeBase/EASA DOCS SPLIT CHUNKS',
        'output_path': 'ContextPacks',
        'topics': [],
        'supported_extensions': ['.md', '.txt', '.jsonl', '.csv']
    }
    
    with open(config_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    current_topic = None
    in_keywords = False
    
    for line in lines:
        line = line.rstrip()
        
        # Topic start
        if line.strip().startswith('- name:'):
            if current_topic:
                config['topics'].append(current_topic)
            current_topic = {'name': line.split('"')[1], 'keywords': [], 'max_chars': 50000}
            in_keywords = False
        
        # Keywords section
        elif 'keywords:' in line and current_topic:
            in_keywords = True
        
        # Keyword item
        elif in_keywords and line.strip().startswith('- "'):
            kw = line.strip()[3:-1]  # Remove '- "' and '"'
            current_topic['keywords'].append(kw)
        
        # Max chars
        elif 'max_chars:' in line and current_topic:
            current_topic['max_chars'] = int(line.split(':')[1].strip())
            in_keywords = False
    
    # Add last topic
    if current_topic:
        config['topics'].append(current_topic)
    
    return config

# --- Corpus Reader ---
def read_corpus(corpus_path, extensions):
    """Read all corpus files and return list of (text, source) tuples."""
    chunks = []
    corpus_dir = Path(corpus_path)
    
    if not corpus_dir.exists():
        print(f"ERROR: Corpus path not found: {corpus_path}")
        return chunks
    
    for ext in extensions:
        for file_path in corpus_dir.rglob(f"*{ext}"):
            try:
                if ext in ['.md', '.txt']:
                    with open(file_path, 'r', encoding='utf-8') as f:
                        text = f.read().strip()
                        if text:
                            chunks.append((text, str(file_path.relative_to(corpus_dir.parent))))
                
                elif ext == '.jsonl':
                    with open(file_path, 'r', encoding='utf-8') as f:
                        for line in f:
                            obj = json.loads(line)
                            text = obj.get('text', '').strip()
                            source = obj.get('source', str(file_path.relative_to(corpus_dir.parent)))
                            if text:
                                chunks.append((text, source))
                
                elif ext == '.csv':
                    with open(file_path, 'r', encoding='utf-8') as f:
                        reader = csv.DictReader(f)
                        for row in reader:
                            text = row.get('text', '').strip()
                            source = row.get('source', str(file_path.relative_to(corpus_dir.parent)))
                            if text:
                                chunks.append((text, source))
            
            except Exception as e:
                print(f"WARNING: Failed to read {file_path}: {e}")
    
    return chunks

# --- Pack Generator ---
def generate_pack(topic, chunks, output_path, max_chars):
    """Generate a context pack for a specific topic."""
    keywords = [kw.lower() for kw in topic['keywords']]
    filtered_chunks = []
    total_chars = 0
    
    # Filter by keywords
    for text, source in chunks:
        text_lower = text.lower()
        if any(kw in text_lower for kw in keywords):
            chunk_len = len(text)
            if total_chars + chunk_len <= max_chars:
                filtered_chunks.append((text, source))
                total_chars += chunk_len
            else:
                break  # Budget exceeded
    
    if not filtered_chunks:
        print(f"WARNING: No chunks found for topic '{topic['name']}'")
        return
    
    # Create output directory
    topic_dir = Path(output_path) / topic['name']
    topic_dir.mkdir(parents=True, exist_ok=True)
    
    # Write pack.md
    pack_path = topic_dir / 'pack.md'
    with open(pack_path, 'w', encoding='utf-8') as f:
        f.write(f"# Context Pack: {topic['name']}\n\n")
        f.write(f"**Generated:** {datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M:%S')} UTC\n")
        f.write(f"**Keywords:** {', '.join(topic['keywords'])}\n")
        f.write(f"**Chunks:** {len(filtered_chunks)}\n")
        f.write(f"**Total Characters:** {total_chars:,}\n\n")
        f.write("---\n\n")
        
        for idx, (text, source) in enumerate(filtered_chunks, 1):
            f.write(f"## Excerpt {idx}\n\n")
            f.write(f"{text}\n\n")
            f.write(f"**Source:** `{source}`\n\n")
            f.write("---\n\n")
    
    print(f"✓ Generated pack: {pack_path} ({len(filtered_chunks)} chunks, {total_chars:,} chars)")

# --- Main ---
def main():
    parser = argparse.ArgumentParser(description='Generate Context Packs for AI training')
    parser.add_argument('--topic', type=str, help='Generate pack for specific topic')
    parser.add_argument('--all', action='store_true', help='Generate packs for all topics')
    parser.add_argument('--config', type=str, default='Tools/TrainingCenter/config.yaml', 
                       help='Path to config.yaml')
    args = parser.parse_args()
    
    # Load config
    config = load_config(args.config)
    print(f"Loaded config version {config['version']}")
    
    # Read corpus
    print(f"Reading corpus from: {config['corpus_path']}")
    chunks = read_corpus(config['corpus_path'], config['supported_extensions'])
    print(f"Loaded {len(chunks)} chunks")
    
    if not chunks:
        print("ERROR: No chunks found. Check corpus path.")
        sys.exit(1)
    
    # Generate packs
    if args.all:
        for topic in config['topics']:
            generate_pack(topic, chunks, config['output_path'], topic['max_chars'])
    elif args.topic:
        topic_obj = next((t for t in config['topics'] if t['name'].lower() == args.topic.lower()), None)
        if not topic_obj:
            print(f"ERROR: Topic '{args.topic}' not found in config")
            sys.exit(1)
        generate_pack(topic_obj, chunks, config['output_path'], topic_obj['max_chars'])
    else:
        print("ERROR: Specify --topic or --all")
        sys.exit(1)
    
    print("\n✓ Context pack generation complete")

if __name__ == '__main__':
    main()
