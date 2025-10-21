#!/usr/bin/env python3
"""
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Phase1 Step5.1 — Skyworks V5: AI Agent Training System
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

AI Agent Training Orchestrator for SORA/EASA Compliance Experts

This system trains TWO specialized AI agents:
1. SORA_Compliance_Agent: Operational Authorization Expert (SORA 2.0 AMC, 2.5, PDRA, STS)
2. Mission_Planning_Agent: Flight Operations & Airspace Expert

Training Schedule: 3x daily (08:00, 14:00, 20:00)
Knowledge Sources: Full EASA corpus + Context Packs
Persistence: Training logs + memory snapshots

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
"""

import json
import sys
import yaml
from pathlib import Path
from datetime import datetime, timezone
from typing import Dict, List, Any, Optional

# Ensure console encoding won't crash under non-UTF consoles (e.g., Task Scheduler)
try:
    sys.stdout.reconfigure(encoding='utf-8', errors='ignore')
    sys.stderr.reconfigure(encoding='utf-8', errors='ignore')
except Exception:
    pass


class AgentKnowledgeBase:
    """Manages full corpus access for agents"""
    
    def __init__(self, corpus_path: Path, context_packs_path: Path):
        self.corpus_path = corpus_path
        self.context_packs_path = context_packs_path
        self.knowledge_index = {}
        
    def load_all_documents(self) -> Dict[str, str]:
        """Load ALL documents from corpus (not just chunks)"""
        documents = {}
        
        # Load root corpus files (like JARUS SORA v2.0)
        for file_path in self.corpus_path.glob("EXTRACTED_*.txt"):
            try:
                content = file_path.read_text(encoding='utf-8', errors='ignore')
                documents[file_path.stem] = content
                print(f"✓ Loaded: {file_path.name} ({len(content)} chars)")
            except Exception as e:
                print(f"✗ Failed to load {file_path.name}: {e}")
        
        # Load processed chunks
        processed_chunks = self.corpus_path / "processed_chunks"
        if processed_chunks.exists():
            for subfolder in processed_chunks.iterdir():
                if subfolder.is_dir():
                    for chunk_file in subfolder.glob("*.txt"):
                        try:
                            content = chunk_file.read_text(encoding='utf-8', errors='ignore')
                            key = f"{subfolder.name}/{chunk_file.stem}"
                            documents[key] = content
                        except:
                            pass
        
        return documents
    
    def load_context_packs(self) -> Dict[str, str]:
        """Load all generated context packs"""
        packs = {}
        for pack_folder in self.context_packs_path.iterdir():
            if pack_folder.is_dir():
                pack_file = pack_folder / "pack.md"
                if pack_file.exists():
                    content = pack_file.read_text(encoding='utf-8')
                    packs[pack_folder.name] = content
                    print(f"✓ Loaded Context Pack: {pack_folder.name}")
        return packs
    
    def build_knowledge_index(self) -> Dict[str, Any]:
        """Build comprehensive knowledge index for agents"""
        print("\n━━━ Building Knowledge Index ━━━")
        
        documents = self.load_all_documents()
        context_packs = self.load_context_packs()
        
        # Build SORA-specific indices
        sora_docs = {k: v for k, v in documents.items() if 'sora' in k.lower()}
        pdra_docs = {k: v for k, v in documents.items() if 'pdra' in k.lower()}
        sts_docs = {k: v for k, v in documents.items() if 'sts' in k.lower()}
        
        index = {
            "total_documents": len(documents),
            "total_context_packs": len(context_packs),
            "sora_documents": len(sora_docs),
            "pdra_documents": len(pdra_docs),
            "sts_documents": len(sts_docs),
            "documents": documents,
            "context_packs": context_packs,
            "indices": {
                "SORA": sora_docs,
                "PDRA": pdra_docs,
                "STS": sts_docs
            }
        }
        
        print(f"✓ Indexed {len(documents)} documents")
        print(f"✓ Indexed {len(context_packs)} context packs")
        print(f"✓ SORA docs: {len(sora_docs)}")
        print(f"✓ PDRA docs: {len(pdra_docs)}")
        print(f"✓ STS docs: {len(sts_docs)}")
        
        return index


class SORAComplianceAgent:
    """Agent 1: SORA Operational Authorization Expert"""
    
    def __init__(self, knowledge_base: AgentKnowledgeBase):
        self.name = "SORA_Compliance_Agent"
        self.kb = knowledge_base
        self.expertise = [
            "SORA 2.0 AMC",
            "JARUS SORA 2.5",
            "PDRA-01 (UAS operations over controlled ground area)",
            "PDRA-02 (UAS operations close to people)",
            "GRC (Ground Risk Class) calculation",
            "ARC (Air Risk Class) determination",
            "SAIL (Specific Assurance & Integrity Levels)",
            "OSO (Operational Safety Objectives)",
            "Operational Authorization procedures"
        ]
        self.memory = []
        self.training_log = []
        
    def train(self, knowledge_index: Dict[str, Any]) -> Dict[str, Any]:
        """Execute training session"""
        training_session = {
            "agent": self.name,
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "knowledge_accessed": [],
            "training_summary": {}
        }
        
        print(f"\n━━━ Training {self.name} ━━━")
        
        # Train on SORA documents
        sora_content = knowledge_index["indices"]["SORA"]
        for doc_name, content in sora_content.items():
            self._process_document(doc_name, content)
            training_session["knowledge_accessed"].append(doc_name)
        
        # Train on PDRA
        pdra_content = knowledge_index["indices"]["PDRA"]
        for doc_name, content in pdra_content.items():
            self._process_document(doc_name, content)
            training_session["knowledge_accessed"].append(doc_name)
        
        # Train on Context Packs
        for pack_name, pack_content in knowledge_index["context_packs"].items():
            if pack_name in ["GRC", "ARC", "SAIL", "OSO", "PDRA", "SORA_25_MainBody", "SORA_25_AnnexA", "SORA_25_AnnexB", "SORA_25_AnnexC", "SORA_25_AnnexD"]:
                self._process_document(f"ContextPack_{pack_name}", pack_content)
                training_session["knowledge_accessed"].append(pack_name)
        
        training_session["training_summary"] = {
            "documents_processed": len(training_session["knowledge_accessed"]),
            "memory_entries": len(self.memory),
            "expertise_areas": len(self.expertise)
        }
        
        self.training_log.append(training_session)
        
        print(f"✓ Processed {len(training_session['knowledge_accessed'])} knowledge sources")
        print(f"✓ Memory entries: {len(self.memory)}")
        
        return training_session
    
    def _process_document(self, doc_name: str, content: str):
        """Process and memorize document content"""
        # Extract key concepts (simplified - real implementation would use NLP)
        memory_entry = {
            "source": doc_name,
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "content_length": len(content),
            "key_terms": self._extract_key_terms(content)
        }
        self.memory.append(memory_entry)
    
    def _extract_key_terms(self, content: str) -> List[str]:
        """Extract key regulatory terms"""
        terms = []
        keywords = [
            "SORA", "GRC", "ARC", "SAIL", "OSO", "PDRA", "TMPR",
            "operational authorization", "risk assessment", "mitigation",
            "ground risk", "air risk", "integrity level", "robustness"
        ]
        for keyword in keywords:
            if keyword.lower() in content.lower():
                terms.append(keyword)
        return terms
    
    def save_memory(self, output_path: Path):
        """Persist agent memory"""
        memory_file = output_path / f"{self.name}_memory.json"
        memory_data = {
            "agent": self.name,
            "last_updated": datetime.now(timezone.utc).isoformat(),
            "total_memory_entries": len(self.memory),
            "expertise": self.expertise,
            "memory": self.memory[-100:],  # Keep last 100 entries
            "training_log": self.training_log
        }
        
        memory_file.write_text(json.dumps(memory_data, indent=2))
        print(f"✓ Saved memory: {memory_file}")


class MissionPlanningAgent:
    """Agent 2: Flight Operations & Mission Planning Expert"""
    
    def __init__(self, knowledge_base: AgentKnowledgeBase):
        self.name = "Mission_Planning_Agent"
        self.kb = knowledge_base
        self.expertise = [
            "STS-01 (VLOS operations)",
            "STS-02 (BVLOS operations with airspace observers)",
            "Operation Manual creation",
            "Mission planning procedures",
            "Airspace coordination",
            "Risk mitigation strategies",
            "Operational procedures",
            "Flight authorization workflows"
        ]
        self.memory = []
        self.training_log = []
        
    def train(self, knowledge_index: Dict[str, Any]) -> Dict[str, Any]:
        """Execute training session"""
        training_session = {
            "agent": self.name,
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "knowledge_accessed": [],
            "training_summary": {}
        }
        
        print(f"\n━━━ Training {self.name} ━━━")
        
        # Train on STS documents
        sts_content = knowledge_index["indices"]["STS"]
        for doc_name, content in sts_content.items():
            self._process_document(doc_name, content)
            training_session["knowledge_accessed"].append(doc_name)
        
        # Train on operational documents
        for doc_name, content in knowledge_index["documents"].items():
            if any(term in doc_name.lower() for term in ["operation", "manual", "procedure", "flight"]):
                self._process_document(doc_name, content)
                training_session["knowledge_accessed"].append(doc_name)
        
        # Train on relevant Context Packs
        for pack_name, pack_content in knowledge_index["context_packs"].items():
            if pack_name in ["STS", "PDRA"]:
                self._process_document(f"ContextPack_{pack_name}", pack_content)
                training_session["knowledge_accessed"].append(pack_name)
        
        training_session["training_summary"] = {
            "documents_processed": len(training_session["knowledge_accessed"]),
            "memory_entries": len(self.memory),
            "expertise_areas": len(self.expertise)
        }
        
        self.training_log.append(training_session)
        
        print(f"✓ Processed {len(training_session['knowledge_accessed'])} knowledge sources")
        print(f"✓ Memory entries: {len(self.memory)}")
        
        return training_session
    
    def _process_document(self, doc_name: str, content: str):
        """Process and memorize document content"""
        memory_entry = {
            "source": doc_name,
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "content_length": len(content),
            "key_operations": self._extract_operational_terms(content)
        }
        self.memory.append(memory_entry)
    
    def _extract_operational_terms(self, content: str) -> List[str]:
        """Extract operational terms"""
        terms = []
        keywords = [
            "STS-01", "STS-02", "VLOS", "BVLOS", "operational procedures",
            "mission planning", "airspace", "flight authorization",
            "operation manual", "risk mitigation"
        ]
        for keyword in keywords:
            if keyword.lower() in content.lower():
                terms.append(keyword)
        return terms
    
    def save_memory(self, output_path: Path):
        """Persist agent memory"""
        memory_file = output_path / f"{self.name}_memory.json"
        memory_data = {
            "agent": self.name,
            "last_updated": datetime.now(timezone.utc).isoformat(),
            "total_memory_entries": len(self.memory),
            "expertise": self.expertise,
            "memory": self.memory[-100:],
            "training_log": self.training_log
        }
        
        memory_file.write_text(json.dumps(memory_data, indent=2))
        print(f"✓ Saved memory: {memory_file}")


class AgentTrainingOrchestrator:
    """Orchestrates daily training for both agents"""
    
    def __init__(self, corpus_path: str, context_packs_path: str, output_path: str):
        self.corpus_path = Path(corpus_path)
        self.context_packs_path = Path(context_packs_path)
        self.output_path = Path(output_path)
        self.output_path.mkdir(parents=True, exist_ok=True)
        
        # Initialize knowledge base
        self.kb = AgentKnowledgeBase(self.corpus_path, self.context_packs_path)
        
        # Initialize agents
        self.agent1 = SORAComplianceAgent(self.kb)
        self.agent2 = MissionPlanningAgent(self.kb)
        
    def run_training_session(self):
        """Execute complete training session for both agents"""
        print("╔═══════════════════════════════════════════════════════════╗")
        print("║   SKYWORKS AI AGENT TRAINING SYSTEM — Session Start      ║")
        print("╚═══════════════════════════════════════════════════════════╝")
        print(f"Timestamp: {datetime.now(timezone.utc).isoformat()}")
        
        # Build knowledge index
        knowledge_index = self.kb.build_knowledge_index()
        
        # Train Agent 1: SORA Compliance Expert
        session1 = self.agent1.train(knowledge_index)
        self.agent1.save_memory(self.output_path)
        
        # Train Agent 2: Mission Planning Expert
        session2 = self.agent2.train(knowledge_index)
        self.agent2.save_memory(self.output_path)
        
        # Save training report
        self._save_training_report(session1, session2, knowledge_index)
        
        print("\n╔═══════════════════════════════════════════════════════════╗")
        print("║   TRAINING SESSION COMPLETE                               ║")
        print("╚═══════════════════════════════════════════════════════════╝")
    
    def _save_training_report(self, session1: Dict, session2: Dict, knowledge_index: Dict):
        """Generate comprehensive training report"""
        report_file = self.output_path / f"training_report_{datetime.now(timezone.utc).strftime('%Y%m%d_%H%M%S')}.json"
        
        report = {
            "training_session": {
                "timestamp": datetime.now(timezone.utc).isoformat(),
                "knowledge_sources": {
                    "total_documents": knowledge_index["total_documents"],
                    "total_context_packs": knowledge_index["total_context_packs"],
                    "sora_documents": knowledge_index["sora_documents"],
                    "pdra_documents": knowledge_index["pdra_documents"],
                    "sts_documents": knowledge_index["sts_documents"]
                }
            },
            "agents": {
                "SORA_Compliance_Agent": session1,
                "Mission_Planning_Agent": session2
            }
        }
        
        report_file.write_text(json.dumps(report, indent=2))
        print(f"\n✓ Training report saved: {report_file}")


def main():
    """Main entry point"""
    import argparse
    
    # Calculate base path (two levels up from this script)
    script_path = Path(__file__).resolve()
    base_path = script_path.parent.parent.parent
    
    parser = argparse.ArgumentParser(description="Train SKYWORKS AI Agents")
    parser.add_argument("--corpus", 
                       default=str(base_path / "KnowledgeBase" / "EASA DOCS SPLIT CHUNKS"),
                       help="Path to EASA corpus")
    parser.add_argument("--packs", 
                       default=str(base_path / "ContextPacks"),
                       help="Path to Context Packs")
    parser.add_argument("--output", 
                       default=str(base_path / "Tools" / "TrainingCenter" / "agent_memory"),
                       help="Output path for agent memory")
    
    args = parser.parse_args()
    
    orchestrator = AgentTrainingOrchestrator(
        corpus_path=args.corpus,
        context_packs_path=args.packs,
        output_path=args.output
    )
    
    orchestrator.run_training_session()


if __name__ == "__main__":
    main()
