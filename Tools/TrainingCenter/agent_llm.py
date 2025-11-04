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
        
        # Load Azure/OpenAI config (with local reasoning fallback)
        self.mock_mode = False
        self.azure_mode = False
        self.openai_mode = False
        self.client = self._init_llm_client()
        # Model/deployment names per provider
        self.deployment = os.getenv("AZURE_OPENAI_DEPLOYMENT", "gpt-4o")
        self.openai_model = os.getenv("OPENAI_MODEL", "gpt-4o-mini")
        self.max_tokens = int(os.getenv("AZURE_OPENAI_MAX_TOKENS", "4096"))
        self.temperature = float(os.getenv("AZURE_OPENAI_TEMPERATURE", "0.7"))
        # Synonym map to enrich keyword matching for local reasoner
        self._synonyms = {
            "grc": ["ground risk class", "population", "tmpr", "mitigation"],
            "arc": ["air risk class", "airspace", "strategic", "tactical"],
            "sail": ["specific assurance", "integrity level", "oso", "operational safety objectives"],
            "oso": ["operational safety objective", "robustness", "sail mapping"],
            "pdra": ["pre-defined risk assessment", "pdra-s01", "pdra-s02"],
            "sts": ["standard scenario", "sts-01", "sts-02"],
            "annex": ["annex a", "annex b", "annex c", "annex d"],
            "easa": ["eu 2019/947", "eu 2019/945", "regulation"],
            "jarus": ["sora 2.5", "jar document", "jarus sora"],
        }
        
    def _init_llm_client(self):
        """Initialize LLM client: prefer Azure, else OpenAI, else local mode."""
        endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
        api_key = os.getenv("AZURE_OPENAI_API_KEY")

        try:
            # Attempt imports lazily
            from openai import AzureOpenAI, OpenAI  # type: ignore
        except Exception:
            AzureOpenAI = None  # type: ignore
            OpenAI = None  # type: ignore

        # Prefer Azure when creds present and SDK available
        if endpoint and api_key and AzureOpenAI is not None:
            self.azure_mode = True
            return AzureOpenAI(
                azure_endpoint=endpoint,
                api_key=api_key,
                api_version="2024-08-01-preview"
            )

        # Else try standard OpenAI when API key present
        openai_key = os.getenv("OPENAI_API_KEY")
        if openai_key and OpenAI is not None:
            self.openai_mode = True
            return OpenAI(api_key=openai_key)

        # Fallback to local
        self.mock_mode = True
        return None
    
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
                # Generate a structured local answer without calling Azure
                answer = self._build_local_answer(agent_name, question, memory, relevant_sources)
                return {
                    "success": True,
                    "agent_name": agent_name,
                    "question": question,
                    "answer": answer,
                    "sources": [s["source"] for s in relevant_sources],
                    "tokens_used": 0,
                    "model": "local-reasoner",
                    "mode": "local"
                }
            elif self.azure_mode:
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
                    "model": self.deployment,
                    "mode": "azure"
                }
            elif self.openai_mode:
                # Call OpenAI (non-Azure)
                response = self.client.chat.completions.create(
                    model=self.openai_model,
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
                    "model": self.openai_model,
                    "mode": "openai"
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
        """RAG: Retrieve relevant sources with lightweight enrichment and diversification"""
        # Extract base keywords (>=4 letters)
        base_keywords = [w.lower() for w in re.findall(r'\b\w{4,}\b', question)]
        keywords = set(base_keywords)
        # Enrich with synonyms for domain terms
        for kw in list(base_keywords):
            if kw in self._synonyms:
                for syn in self._synonyms[kw]:
                    if len(syn) >= 4:
                        keywords.update([t for t in re.findall(r'\b\w{3,}\b', syn.lower())])

        # Score memory entries
        scored_entries = []
        for entry in memory.get("memory", [])[:600]:  # look deeper but keep perf
            score = self._calculate_relevance_score(entry, keywords)
            if score > 0:
                # Add small boost for shorter content (more focused) and recentness if available
                length = max(1, int(entry.get("content_length", 1000)))
                score += max(0, 8 - (length // 1000))  # prefer <=8k chars
                scored_entries.append((score, entry))

        scored_entries.sort(reverse=True, key=lambda x: x[0])

        # Diversify by source prefix (e.g., pack name)
        top = []
        seen_prefix = set()
        for score, entry in scored_entries:
            source = entry.get("source", "")
            prefix = source.split("/")[0] if "/" in source else source.split("_")[0]
            if prefix not in seen_prefix or len(top) < 5:
                top.append(entry)
                seen_prefix.add(prefix)
            if len(top) >= 10:
                break

        return top
    
    def _calculate_relevance_score(self, entry: Dict, keywords: set) -> int:
        """Calculate relevance score βάσει keyword overlap with light heuristics"""
        score = 0

        # Score από source name
        source_lower = entry.get("source", "").lower()
        score += sum(3 for kw in keywords if kw in source_lower)

        # Score από key terms
        terms_lower = [t.lower() for t in entry.get("key_terms", [])]
        for kw in keywords:
            for term in terms_lower:
                if kw in term or term in kw:
                    score += 5
                    break

        # Boost for explicit Annex/STS/PDRA mentions in source
        if any(token in source_lower for token in ["annex", "sts", "pdra", "sora", "jarus"]):
            score += 3

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

    def _build_local_answer(self, agent_name: str, question: str, memory: Dict, relevant_sources: List[Dict]) -> str:
        """Produce a comprehensive structured answer using training metadata and best-practice templates when Azure LLM is not configured."""
        sources_md = "\n".join(f"- {s['source']}" for s in relevant_sources[:10]) or "- (no specific sources retrieved)"
        expertise = "\n".join(f"- {e}" for e in memory.get("expertise", [])[:8]) or "- General SORA/STS knowledge"

        # Basic confidence heuristic
        overlap = len(relevant_sources)
        confidence = "high" if overlap >= 7 else ("medium" if overlap >= 3 else "low")

        if agent_name == "SORA_Compliance_Agent":
            return (
                f"## Direct Answer\n"
                f"Here's a compliance-oriented answer aligned with SORA v2.0 AMC/GM and JARUS SORA v2.5.\n\n"
                f"## Regulatory Basis\n"
                f"Use JARUS SORA v2.5 (Main Body, Annexes A–D) and EASA AMC/GM to 2019/947 as primary references. Map OSOs to SAIL with stated robustness.\n\n"
                f"## Methodology / Step-by-Step\n"
                f"1. Determine GRC and ARC from the operational context.\n"
                f"2. Derive SAIL using SORA tables; validate assumptions.\n"
                f"3. Identify applicable OSOs by SAIL; evaluate robustness (Low/Med/High).\n"
                f"4. Address strategic/tactical mitigations; record evidences.\n"
                f"5. Compile compliance matrix and residual risk statement.\n\n"
                f"## Practical Recommendations\n"
                f"- Maintain a traceable OSO-to-evidence matrix with version control.\n"
                f"- Pre-coordinate with ANSP/local authorities when ARC or geo-fencing constraints apply.\n"
                f"- Include contingency and communication plans in the operating procedures.\n\n"
                f"## Related Considerations\n"
                f"- Urban density, airspace class, payload hazard, and overflight of assemblies affect mitigations.\n"
                f"- Where PDRA applies, reference PDRA Sx templates for expedited authorization.\n\n"
                f"## Sources\n{sources_md}\n\n"
                f"---\nConfidence: {confidence}\n\nExpertise:\n{expertise}\n"
            )
        else:
            return (
                f"## Executive Summary\n"
                f"Operational outline aligned to STS procedures and mission planning best practices.\n\n"
                f"## Regulatory Requirements\n"
                f"Reference STS-01/02, operator documentation, and local coordination requirements.\n\n"
                f"## Step-by-Step Procedures\n"
                f"1. Preflight planning (site, NOTAMs, weather, coordination).\n"
                f"2. On-site checks (risk area, equipment, team briefing).\n"
                f"3. Flight execution and logging.\n"
                f"4. Post-flight review and incident reporting.\n\n"
                f"## Required Documentation\n"
                f"Operation Manual, crew competency records, maintenance logs, mission briefings, NOTAM evidence.\n\n"
                f"## Safety & Contingencies\n"
                f"Loss of C2, GNSS degradation, weather deterioration, airspace incursion — define triggers and actions.\n\n"
                f"## Sources\n{sources_md}\n\n"
                f"---\nConfidence: {confidence}\n\nExpertise:\n{expertise}\n"
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
