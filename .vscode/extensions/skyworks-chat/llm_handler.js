const fs = require('fs');
const path = require('path');

async function handleAskAgentWithLLM(stream, agentName, question, memoryDir, workspaceRoot) {
    stream.markdown(`## 🤖 ${agentName}\n\n`);

    if (!question || question.trim() === '') {
        stream.markdown(`❓ Παρακαλώ δώσε μια ερώτηση.\n\n`);
        stream.markdown(`Παράδειγμα: \`/ask-sora What is SAIL level for GRC=3?\`\n`);
        return;
    }

    stream.markdown(`**Ερώτηση**: ${question}\n\n`);
    stream.markdown(`⏳ Calling Azure OpenAI for expert reasoning...\n\n`);

    // Check if Azure OpenAI is configured
    const endpoint = process.env.AZURE_OPENAI_ENDPOINT;
    const apiKey = process.env.AZURE_OPENAI_API_KEY;

    if (!endpoint || !apiKey) {
        stream.markdown(`⚠️ **Azure OpenAI not configured**\n\n`);
        stream.markdown(`To enable full LLM reasoning, set environment variables:\n`);
        stream.markdown(`\`\`\`pwsh\n`);
        stream.markdown(`$env:AZURE_OPENAI_ENDPOINT = "https://YOUR-RESOURCE.openai.azure.com/"\n`);
        stream.markdown(`$env:AZURE_OPENAI_API_KEY = "your-key-here"\n`);
        stream.markdown(`\`\`\`\n\n`);
        stream.markdown(`📖 See: \`Tools/TrainingCenter/LLM_SETUP.md\`\n\n`);
        
        // Fallback to keyword search
        stream.markdown(`**Fallback mode**: Using keyword search from agent memory...\n\n`);
        await handleAskAgentKeywordSearch(stream, agentName, question, memoryDir);
        return;
    }

    // Call Python LLM service
    const llmScript = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'agent_llm.py');
    
    try {
        const { execSync } = require('child_process');
        const result = execSync(`python "${llmScript}" "${agentName}" "${question}"`, {
            encoding: 'utf-8',
            timeout: 60000, // 60s timeout
            maxBuffer: 10 * 1024 * 1024 // 10MB buffer
        });

        const response = JSON.parse(result);

        if (!response.success) {
            stream.markdown(`❌ **Error**: ${response.error}\n`);
            return;
        }

        // Display LLM response
        stream.markdown(`---\n\n`);
        stream.markdown(response.answer);
        stream.markdown(`\n\n---\n\n`);

        // Metadata
        stream.markdown(`📊 **Metadata**:\n`);
        stream.markdown(`- Model: ${response.model}\n`);
        stream.markdown(`- Tokens Used: ${response.tokens_used}\n`);
        stream.markdown(`- Sources Retrieved: ${response.sources.length}\n\n`);

        if (response.sources.length > 0) {
            stream.markdown(`📎 **Knowledge Sources Used**:\n`);
            response.sources.slice(0, 5).forEach(src => {
                stream.markdown(`- \`${src}\`\n`);
            });
        }

    } catch (error) {
        stream.markdown(`❌ **LLM call failed**: ${error.message}\n\n`);
        stream.markdown(`**Troubleshooting**:\n`);
        stream.markdown(`1. Check Python is installed: \`python --version\`\n`);
        stream.markdown(`2. Install openai package: \`pip install openai\`\n`);
        stream.markdown(`3. Verify env vars: \`$env:AZURE_OPENAI_ENDPOINT\`\n\n`);
        stream.markdown(`📖 Full guide: \`Tools/TrainingCenter/LLM_SETUP.md\`\n`);
    }
}

async function handleAskAgentKeywordSearch(stream, agentName, question, memoryDir) {
    // Original keyword-based search (fallback)
    const memFile = agentName === 'SORA_Compliance_Agent' 
        ? 'SORA_Compliance_Agent_memory.json'
        : 'Mission_Planning_Agent_memory.json';
    const memPath = path.join(memoryDir, memFile);

    if (!fs.existsSync(memPath)) {
        stream.markdown(`⚠️ Agent memory not found. Run training first.\n`);
        return;
    }

    const memory = JSON.parse(fs.readFileSync(memPath, 'utf-8'));

    // Simple keyword search in memory
    const keywords = question.toLowerCase().split(' ').filter(w => w.length > 3);
    const relevantEntries = memory.memory.slice(0, 100).filter(entry => {
        const source = entry.source.toLowerCase();
        const terms = entry.key_terms.map(t => t.toLowerCase());
        return keywords.some(kw => source.includes(kw) || terms.some(t => t.includes(kw)));
    });

    if (relevantEntries.length === 0) {
        stream.markdown(`⚠️ Δεν βρέθηκαν σχετικές πληροφορίες στη μνήμη του agent.\n\n`);
        stream.markdown(`Δοκίμασε:\n`);
        stream.markdown(`- Πιο γενική ερώτηση\n`);
        stream.markdown(`- Λέξεις-κλειδιά από SORA/PDRA/STS\n`);
        return;
    }

    stream.markdown(`✅ Βρέθηκαν **${relevantEntries.length}** σχετικές πηγές στη μνήμη.\n\n`);
    stream.markdown(`📎 **Σχετικές πηγές**:\n`);
    relevantEntries.slice(0, 5).forEach(entry => {
        stream.markdown(`- \`${entry.source}\` (${entry.content_length} chars)\n`);
        if (entry.key_terms.length > 0) {
            stream.markdown(`  - Terms: ${entry.key_terms.slice(0, 5).join(', ')}\n`);
        }
    });

    stream.markdown(`\n💡 **Note**: For full expert reasoning, configure Azure OpenAI (see LLM_SETUP.md).\n`);
}

// Export για χρήση στο extension.js
module.exports = { handleAskAgentWithLLM };
