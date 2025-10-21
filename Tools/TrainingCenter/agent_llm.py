#!/usr/bin/env python3
"""
LLM Service για AI Agent reasoning με Azure OpenAI
Καλείται από το VS Code chat extension για πλήρη expert-level απαντήσεις
"""

import json
import os
import sys
import re
from pathlib import Path
from datetime import datetime
from typing import List, Dict, Any, Optional


class AgentLLMService:
    def __init__(self, workspace_root: str):
        self.workspace_root = Path(workspace_root)
        self.memory_dir = self.workspace_root / "Tools" / "TrainingCenter" / "agent_memory"
        
        # Load Azure OpenAI config (with mock fallback)
        self.mock_mode = False
        self.client = self._init_azure_client()
        self.deployment = os.getenv("AZURE_OPENAI_DEPLOYMENT", "gpt-4o") if not self.mock_mode else "mock"
        self.max_tokens = int(os.getenv("AZURE_OPENAI_MAX_TOKENS", "4096"))
        self.temperature = float(os.getenv("AZURE_OPENAI_TEMPERATURE", "0.7"))
        
    def _init_azure_client(self):
        """Initialize Azure OpenAI client from env vars, else enable mock mode."""
        endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
        api_key = os.getenv("AZURE_OPENAI_API_KEY")

        if not endpoint or not api_key:
            # Enable mock mode when credentials are missing
            self.mock_mode = True
            return None

        try:
            from openai import AzureOpenAI  # Lazy import
        except ImportError:
            # If openai not installed, fallback to mock mode
            self.mock_mode = True
            return None

        return AzureOpenAI(
            azure_endpoint=endpoint,
            api_key=api_key,
            api_version="2024-08-01-preview"
        )
    
    def ask_agent(self, agent_name: str, question: str) -> Dict[str, Any]:
        """Ρωτά έναν agent με πλήρη reasoning και citations"""
        try:
            # Load agent memory
            memory = self._load_agent_memory(agent_name)
            if not memory:
                return {
                    "success": False,
                    "error": f"Agent memory not found for {agent_name}"
                }
            
            # Retrieve relevant context (RAG)
            relevant_sources = self._retrieve_relevant_context(question, memory)
            
            # Build prompts
            system_prompt = self._build_system_prompt(agent_name, memory, relevant_sources)
            user_prompt = self._build_user_prompt(question, relevant_sources)
            
            if self.mock_mode:
                # Generate a structured mock answer without calling Azure
                answer = self._build_mock_answer(agent_name, question, memory, relevant_sources)
                return {
                    "success": True,
                    "agent_name": agent_name,
                    "question": question,
                    "answer": answer,
                    "sources": [s["source"] for s in relevant_sources],
                    "tokens_used": 0,
                    "model": "mock"
                }
            else:
                # Call Azure OpenAI
                response = self.client.chat.completions.create(
                    model=self.deployment,
                    messages=[
                        {"role": "system", "content": system_prompt},
                        {"role": "user", "content": user_prompt}
                    ],
                    max_tokens=self.max_tokens,
                    temperature=self.temperature,
                    top_p=0.95
                )

                answer = response.choices[0].message.content

                return {
                    "success": True,
                    "agent_name": agent_name,
                    "question": question,
                    "answer": answer,
                    "sources": [s["source"] for s in relevant_sources],
                    "tokens_used": getattr(response.usage, 'total_tokens', 0),
                    "model": self.deployment
                }
            
        except Exception as e:
            return {
                "success": False,
                "error": f"LLM call failed: {str(e)}"
            }
    
    def _load_agent_memory(self, agent_name: str) -> Optional[Dict]:
        """Load agent memory JSON"""
        memory_file = self.memory_dir / f"{agent_name}_memory.json"
        if not memory_file.exists():
            return None
        
        with open(memory_file, 'r', encoding='utf-8') as f:
            return json.load(f)
    
    def _retrieve_relevant_context(self, question: str, memory: Dict) -> List[Dict]:
        """RAG: Retrieve relevant sources βάσει keyword matching"""
        # Extract keywords
        keywords = set(
            word.lower() for word in re.findall(r'\b\w{4,}\b', question)
        )
        
        # Score memory entries
        scored_entries = []
        for entry in memory.get("memory", [])[:200]:  # First 200 for performance
            score = self._calculate_relevance_score(entry, keywords)
            if score > 0:
                scored_entries.append((score, entry))
        
        # Top 10 most relevant
        scored_entries.sort(reverse=True, key=lambda x: x[0])
        return [entry for _, entry in scored_entries[:10]]
    
    def _calculate_relevance_score(self, entry: Dict, keywords: set) -> int:
        """Calculate relevance score βάσει keyword overlap"""
        score = 0
        
        # Score από source name
        source_lower = entry["source"].lower()
        score += sum(3 for kw in keywords if kw in source_lower)
        
        # Score από key terms
        if entry.get("key_terms"):
            terms_lower = [t.lower() for t in entry["key_terms"]]
            for kw in keywords:
                for term in terms_lower:
                    if kw in term or term in kw:
                        score += 5
                        break
        
        return score
    
    def _build_system_prompt(self, agent_name: str, memory: Dict, relevant_sources: List[Dict]) -> str:
        """Build comprehensive system prompt για expert reasoning"""
        expertise_list = "\n- ".join(memory.get("expertise", []))
        sources_list = "\n- ".join([s["source"] for s in relevant_sources])
        
        if agent_name == "SORA_Compliance_Agent":
            base_prompt = f"""You are a world-class expert in SORA (Specific Operations Risk Assessment) methodology, EASA UAS regulations, and JARUS SORA 2.5.

Your expertise covers:
- {expertise_list}

Your role:
1. **Provide comprehensive, well-reasoned answers** based on regulatory documents
2. **Cite specific sources** (document names, sections, annexes) — be precise
3. **Explain methodology step-by-step** when calculating risk levels or demonstrating compliance
4. **Offer practical advice** for operators seeking authorization
5. **Highlight edge cases, exceptions, and regulatory nuances** that experienced compliance officers know
6. **Use examples and scenarios** when helpful for understanding
7. **Structure your response clearly** with headings, bullet points, and numbered steps

Tone: Professional, authoritative, but approachable. Think like a senior SORA consultant with 10+ years experience.

Critical: Never give short, mechanical, or single-sentence answers. Always provide context, reasoning, and actionable guidance.

Response structure template:
## Direct Answer
[Clear, concise answer to the question]

## Regulatory Basis
[Cite SORA 2.0 AMC, JARUS 2.5, PDRA, or relevant annexes with specific sections]

## Methodology / Step-by-Step
[If applicable: walk through calculation, assessment, or compliance steps]

## Practical Recommendations
[Actionable advice for operators]

## Related Considerations
[Edge cases, alternatives, or related requirements]

## Sources
[List specific documents/sections cited]"""
        
        else:  # Mission_Planning_Agent
            base_prompt = f"""You are an expert UAS mission planner and flight operations specialist with complete mastery of STS-01/02 and operational procedures.

Your expertise covers:
- {expertise_list}

Your role:
1. **Guide users through mission planning** with detailed, practical steps
2. **Cite STS requirements** and operational procedures precisely
3. **Explain flight authorization workflows** step-by-step
4. **Provide checklists, templates, and operational tools** in your responses
5. **Address safety, coordination, and contingency considerations** proactively
6. **Use real-world operational scenarios** to illustrate procedures
7. **Structure responses for immediate operational use**

Tone: Practical, detail-oriented, safety-focused. Think like a chief pilot or operations manager.

Critical: Never give vague or incomplete guidance. Provide full operational procedures, checklists, and decision trees.

Response structure template:
## Executive Summary
[What the user needs to do]

## Regulatory Requirements
[STS-01/02 citations, operational rules]

## Step-by-Step Procedures
[Detailed operational steps with numbering]

## Required Documentation
[Checklists, forms, templates needed]

## Safety & Contingencies
[Risk mitigations, emergency procedures]

## Sources
[STS documents, operation manual sections cited]"""
        
        sources_section = ""
        if relevant_sources:
            sources_section = f"\n\nYou have access to these knowledge sources for this question:\n{sources_list}\n\nUse these to support your answer with specific citations."
        else:
            sources_section = "\n\nNote: No specific sources were retrieved for this question. Provide answer based on general SORA/EASA knowledge from your training."
        
        return base_prompt + sources_section
    
    def _build_user_prompt(self, question: str, relevant_sources: List[Dict]) -> str:
        """Build user prompt με relevant context"""
        prompt = f"User Question: {question}\n\n"
        
        if relevant_sources:
            prompt += "Relevant context from training data:\n\n"
            for source in relevant_sources[:5]:
                prompt += f"Source: {source['source']}\n"
                if source.get("key_terms"):
                    prompt += f"Key Terms: {', '.join(source['key_terms'][:8])}\n"
                prompt += f"Content Length: {source['content_length']} characters\n\n"
        
        prompt += "Please provide a comprehensive, expert-level response following the structure template in your system prompt."
        return prompt

    def _build_mock_answer(self, agent_name: str, question: str, memory: Dict, relevant_sources: List[Dict]) -> str:
        """Produce a realistic structured answer when running without Azure credentials."""
        sources_md = "\n".join(f"- {s['source']}" for s in relevant_sources[:8]) or "- (no specific sources retrieved)"
        expertise = "\n".join(f"- {e}" for e in memory.get("expertise", [])[:6]) or "- General SORA/STS knowledge"

        if agent_name == "SORA_Compliance_Agent":
            return (
                f"## Direct Answer\n"
                f"This is a mock expert response (Azure OpenAI not configured). Based on the retrieved SORA materials and your question, here's how a SORA expert would approach it.\n\n"
                f"## Regulatory Basis\n"
                f"Citations would normally reference JARUS SORA v2.5 (Main Body, Annexes A–D), SORA 2.0 AMC/GM, and related PDRA/OSO guidance.\n\n"
                f"## Methodology / Step-by-Step\n"
                f"1. Identify GRC/ARC and determine SAIL using the SORA tables.\n"
                f"2. Map applicable OSOs for the resulting SAIL level and evaluate robustness.\n"
                f"3. Address strategic and tactical mitigations; document evidences.\n\n"
                f"## Practical Recommendations\n"
                f"- Prepare a compliance matrix mapping OSOs to evidences.\n"
                f"- Include contingency procedures and communication plans.\n\n"
                f"## Related Considerations\n"
                f"- Airspace class, geocaging, and urban density may affect mitigations.\n\n"
                f"## Sources\n{sources_md}\n\n"
                f"---\nExpertise:\n{expertise}\n"
            )
        else:
            return (
                f"## Executive Summary\n"
                f"Mock response without Azure LLM: here is an operational outline aligned to STS procedures.\n\n"
                f"## Regulatory Requirements\n"
                f"Reference STS-01/02, operator documentation, and local coordination requirements.\n\n"
                f"## Step-by-Step Procedures\n"
                f"1. Preflight planning (site, NOTAMs, weather, coordination).\n"
                f"2. On-site checks (risk area, equipment, team briefing).\n"
                f"3. Flight execution and logging.\n\n"
                f"## Required Documentation\n"
                f"Checklist, briefing notes, maintenance logs, and incident reporting forms.\n\n"
                f"## Safety & Contingencies\n"
                f"Loss of C2, GNSS degradation, emergency landing sites.\n\n"
                f"## Sources\n{sources_md}\n\n"
                f"---\nExpertise:\n{expertise}\n"
            )


def main():
    """CLI entry point"""
    if len(sys.argv) < 3:
        print("Usage: python agent_llm.py <agent_name> <question>", file=sys.stderr)
        print("Example: python agent_llm.py SORA_Compliance_Agent \"What is SAIL level for GRC=3?\"", file=sys.stderr)
        sys.exit(1)
    
    agent_name = sys.argv[1]
    question = " ".join(sys.argv[2:])
    
    # Detect workspace root (3 levels up from this script)
    script_path = Path(__file__).resolve()
    workspace_root = script_path.parent.parent.parent
    
    service = AgentLLMService(str(workspace_root))
    result = service.ask_agent(agent_name, question)
    
    # Output JSON για parsing από VS Code extension
    print(json.dumps(result, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
