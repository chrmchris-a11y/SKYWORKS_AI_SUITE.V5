const vscode = require('vscode');
const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');
const { handleAskAgentWithLLM } = require('./llm_handler');

/**
 * @param {vscode.ExtensionContext} context
 */
function activate(context) {
    const workspaceRoot = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;
    if (!workspaceRoot) {
        console.warn('[Skyworks Chat] No workspace folder detected');
        return;
    }

    const logsDir = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'logs');
    const memoryDir = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'agent_memory');
    const masterPromptPath = path.join(workspaceRoot, 'Docs', 'Orchestration', 'MASTER_PROMPT.md');

    // Register chat participant
    const participant = vscode.chat.createChatParticipant('skyworks.chat', async (request, context, stream, token) => {
        const command = request.command;
        const prompt = request.prompt;

        try {
            if (command === 'training-status') {
                await handleTrainingStatus(stream, logsDir, memoryDir);
            } else if (command === 'training-logs') {
                const count = parseInt(prompt) || 5;
                await handleTrainingLogs(stream, logsDir, count);
            } else if (command === 'training-report') {
                await handleTrainingReport(stream, memoryDir);
            } else if (command === 'agents') {
                await handleAgents(stream, memoryDir);
            } else if (command === 'ask-sora') {
                await handleAskAgentWithLLM(stream, 'SORA_Compliance_Agent', prompt, memoryDir, workspaceRoot);
            } else if (command === 'ask-mission') {
                await handleAskAgentWithLLM(stream, 'Mission_Planning_Agent', prompt, memoryDir, workspaceRoot);
            } else if (command === 'project-status') {
                await handleProjectStatus(stream, masterPromptPath, workspaceRoot);
            } else if (command === 'next-steps') {
                await handleNextSteps(stream, masterPromptPath);
            } else if (command === 'run-training') {
                const session = prompt.trim() || 'now';
                await handleRunTraining(stream, workspaceRoot, session);
            } else if (command === 'view-log') {
                await handleViewLog(stream, logsDir, prompt);
            } else if (command === 'scheduled-tasks') {
                await handleScheduledTasks(stream);
            } else if (command === 'help') {
                await handleHelp(stream);
            } else {
                // Fallback: general query
                stream.markdown(`Γεια σου! Χρησιμοποίησε \`/help\` για να δεις όλες τις εντολές.\n\n`);
                stream.markdown(`Ή γράψε:\n`);
                stream.markdown(`- \`/training-status\` για training info\n`);
                stream.markdown(`- \`/agents\` για agent list\n`);
                stream.markdown(`- \`/project-status\` για project overview\n`);
            }
        } catch (err) {
            stream.markdown(`❌ Error: ${err.message}\n`);
        }
    });

    participant.iconPath = vscode.Uri.file(path.join(context.extensionPath, 'icon.png'));
    context.subscriptions.push(participant);

    console.log('[Skyworks Chat] Participant registered: @skyworks');
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Command Handlers
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

async function handleTrainingStatus(stream, logsDir, memoryDir) {
    stream.markdown(`## 📊 Training Status\n\n`);

    // Get latest log file
    const logFiles = fs.readdirSync(logsDir)
        .filter(f => f.startsWith('training_') && f.endsWith('.log'))
        .map(f => ({ name: f, time: fs.statSync(path.join(logsDir, f)).mtime }))
        .sort((a, b) => b.time - a.time);

    if (logFiles.length === 0) {
        stream.markdown(`⚠️ No training logs found.\n`);
        return;
    }

    const latest = logFiles[0];
    const logPath = path.join(logsDir, latest.name);
    const logContent = fs.readFileSync(logPath, 'utf-8');

    // Parse log for key info
    const lines = logContent.split('\n');
    const loadedDocs = lines.filter(l => l.includes('✓ Loaded:')).length;
    const indexedMatch = logContent.match(/✓ Indexed (\d+) documents/);
    const indexed = indexedMatch ? indexedMatch[1] : 'N/A';

    // Get memory files
    const soraMemPath = path.join(memoryDir, 'SORA_Compliance_Agent_memory.json');
    const missionMemPath = path.join(memoryDir, 'Mission_Planning_Agent_memory.json');

    let soraEntries = 'N/A', missionEntries = 'N/A';
    if (fs.existsSync(soraMemPath)) {
        const soraMem = JSON.parse(fs.readFileSync(soraMemPath, 'utf-8'));
        soraEntries = soraMem.total_memory_entries || 0;
    }
    if (fs.existsSync(missionMemPath)) {
        const missionMem = JSON.parse(fs.readFileSync(missionMemPath, 'utf-8'));
        missionEntries = missionMem.total_memory_entries || 0;
    }

    stream.markdown(`✅ **Last Training**: ${latest.time.toLocaleString('el-GR')}\n\n`);
    stream.markdown(`📋 **Metrics**:\n`);
    stream.markdown(`- Documents Loaded: ${loadedDocs} root files\n`);
    stream.markdown(`- Total Indexed: ${indexed} documents\n`);
    stream.markdown(`- SORA_Compliance_Agent: **${soraEntries}** memory entries\n`);
    stream.markdown(`- Mission_Planning_Agent: **${missionEntries}** memory entries\n\n`);
    stream.markdown(`📁 Log: \`${latest.name}\`\n`);
}

async function handleTrainingLogs(stream, logsDir, count) {
    stream.markdown(`## 📋 Recent Training Logs\n\n`);

    const logFiles = fs.readdirSync(logsDir)
        .filter(f => f.startsWith('training_') && f.endsWith('.log'))
        .map(f => {
            const stat = fs.statSync(path.join(logsDir, f));
            return { name: f, time: stat.mtime, size: stat.size };
        })
        .sort((a, b) => b.time - a.time)
        .slice(0, count);

    if (logFiles.length === 0) {
        stream.markdown(`⚠️ No logs found.\n`);
        return;
    }

    logFiles.forEach((log, i) => {
        const sizeKB = (log.size / 1024).toFixed(1);
        stream.markdown(`${i + 1}. **${log.name}**\n`);
        stream.markdown(`   - Time: ${log.time.toLocaleString('el-GR')}\n`);
        stream.markdown(`   - Size: ${sizeKB} KB\n\n`);
    });
}

async function handleTrainingReport(stream, memoryDir) {
    stream.markdown(`## 📊 Latest Training Report\n\n`);

    const reportFiles = fs.readdirSync(memoryDir)
        .filter(f => f.startsWith('training_report_') && f.endsWith('.json'))
        .map(f => ({ name: f, time: fs.statSync(path.join(memoryDir, f)).mtime }))
        .sort((a, b) => b.time - a.time);

    if (reportFiles.length === 0) {
        stream.markdown(`⚠️ No training reports found.\n`);
        return;
    }

    const latestReport = reportFiles[0];
    const reportPath = path.join(memoryDir, latestReport.name);
    const report = JSON.parse(fs.readFileSync(reportPath, 'utf-8'));

    stream.markdown(`**Timestamp**: ${new Date(report.training_session.timestamp).toLocaleString('el-GR')}\n\n`);
    stream.markdown(`### Knowledge Sources\n`);
    stream.markdown(`- Total Documents: **${report.training_session.knowledge_sources.total_documents}**\n`);
    stream.markdown(`- Context Packs: **${report.training_session.knowledge_sources.total_context_packs}**\n`);
    stream.markdown(`- SORA Docs: **${report.training_session.knowledge_sources.sora_documents}**\n`);
    stream.markdown(`- PDRA Docs: **${report.training_session.knowledge_sources.pdra_documents}**\n`);
    stream.markdown(`- STS Docs: **${report.training_session.knowledge_sources.sts_documents}**\n\n`);

    stream.markdown(`### Agent Metrics\n`);
    const soraAgent = report.agents.SORA_Compliance_Agent;
    const missionAgent = report.agents.Mission_Planning_Agent;

    stream.markdown(`**SORA_Compliance_Agent**:\n`);
    stream.markdown(`- Documents Processed: ${soraAgent.documents_processed}\n`);
    stream.markdown(`- Memory Entries: ${soraAgent.memory_entries}\n\n`);

    stream.markdown(`**Mission_Planning_Agent**:\n`);
    stream.markdown(`- Documents Processed: ${missionAgent.documents_processed}\n`);
    stream.markdown(`- Memory Entries: ${missionAgent.memory_entries}\n\n`);

    stream.markdown(`📁 Report: \`${latestReport.name}\`\n`);
}

async function handleAgents(stream, memoryDir) {
    stream.markdown(`## 🤖 Active Agents\n\n`);

    const agents = [
        { name: 'SORA_Compliance_Agent', file: 'SORA_Compliance_Agent_memory.json' },
        { name: 'Mission_Planning_Agent', file: 'Mission_Planning_Agent_memory.json' }
    ];

    agents.forEach(agent => {
        const memPath = path.join(memoryDir, agent.file);
        if (!fs.existsSync(memPath)) {
            stream.markdown(`⚠️ **${agent.name}**: Memory file not found\n\n`);
            return;
        }

        const memory = JSON.parse(fs.readFileSync(memPath, 'utf-8'));
        stream.markdown(`### ${agent.name}\n`);
        stream.markdown(`**Expertise**:\n`);
        memory.expertise.forEach(exp => stream.markdown(`- ${exp}\n`));
        stream.markdown(`\n**Memory**: ${memory.total_memory_entries} entries\n`);
        stream.markdown(`**Last Updated**: ${new Date(memory.last_updated).toLocaleString('el-GR')}\n\n`);
    });
}

// Deprecated: handled by handleAskAgentWithLLM in llm_handler.js
async function handleAskAgent(stream, agentName, question, memoryDir) {
    return handleAskAgentWithLLM(stream, agentName, question, memoryDir, vscode.workspace.workspaceFolders?.[0]?.uri.fsPath || '');
}

async function handleProjectStatus(stream, masterPromptPath, workspaceRoot) {
    stream.markdown(`## 📊 SKYWORKS AI SUITE — Project Status\n\n`);

    // Read training center status
    const memoryDir = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'agent_memory');
    const reportFiles = fs.existsSync(memoryDir) 
        ? fs.readdirSync(memoryDir).filter(f => f.startsWith('training_report_'))
        : [];

    stream.markdown(`### ✅ Phase 1: Foundation & Core Structure\n\n`);
    stream.markdown(`- ✅ Step 1: Solution structure (.NET 8)\n`);
    stream.markdown(`- ✅ Step 2: REST API (Skyworks.Api)\n`);
    stream.markdown(`- ✅ Step 3: Agent Communication System\n`);
    stream.markdown(`- ✅ Step 4: Basic endpoints\n`);
    stream.markdown(`- ✅ **Step 5.1: Training Center** (**COMPLETE**)\n`);
    stream.markdown(`  - Two AI agents: SORA_Compliance_Agent, Mission_Planning_Agent\n`);
    stream.markdown(`  - 3x daily automated training (08:00, 14:00, 20:00)\n`);
    stream.markdown(`  - Persistent memory (JSON) with ${reportFiles.length} training reports\n`);
    stream.markdown(`  - Logs and monitoring ready\n\n`);

    stream.markdown(`### ⏳ Pending (Phase 1)\n\n`);
    stream.markdown(`- ⏳ Step 6: Compliance Framework (NEXT)\n`);
    stream.markdown(`- ⏸️ Step 7: Security & Authentication\n`);
    stream.markdown(`- ⏸️ Step 8: Web Interface\n`);
    stream.markdown(`- ⏸️ Step 9: Documentation System\n`);
    stream.markdown(`- ⏸️ Step 10: Integration Testing\n\n`);

    stream.markdown(`**Current Focus**: Training Center operational, ready for Step 6\n`);
}

async function handleNextSteps(stream, masterPromptPath) {
    stream.markdown(`## 🎯 Next Steps\n\n`);

    stream.markdown(`### Immediate (Phase 1 continuation)\n\n`);
    stream.markdown(`1. **Step 6: Compliance Framework**\n`);
    stream.markdown(`   - Embed SORA/DCA rules into validation engine\n`);
    stream.markdown(`   - Create risk assessment pipelines (GRC/ARC/SAIL)\n`);
    stream.markdown(`   - Integrate with agents' knowledge base\n\n`);

    stream.markdown(`2. **Step 7: Security & Authentication**\n`);
    stream.markdown(`   - JWT/RBAC setup for API\n`);
    stream.markdown(`   - User roles: Admin, Operator, Auditor\n`);
    stream.markdown(`   - API key management\n\n`);

    stream.markdown(`3. **Step 8: Web Interface foundation**\n`);
    stream.markdown(`   - Chat UI for agent Q&A\n`);
    stream.markdown(`   - Training dashboard\n`);
    stream.markdown(`   - Compliance calculator\n\n`);

    stream.markdown(`### Quick wins\n\n`);
    stream.markdown(`- Add LLM provider (Azure OpenAI) for agent reasoning\n`);
    stream.markdown(`- Monitoring API endpoints (\`/api/agents/status\`, \`/api/agents/train\`)\n`);
    stream.markdown(`- Vector store for semantic search (FAISS/Chroma)\n\n`);

    stream.markdown(`💡 Type \`/project-status\` to see completed work.\n`);
}

async function handleRunTraining(stream, workspaceRoot, session) {
    stream.markdown(`## ▶️ Running Training Session: **${session}**\n\n`);

    const runnerPath = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'run_agent_training.cmd');
    if (!fs.existsSync(runnerPath)) {
        stream.markdown(`❌ Runner script not found: \`${runnerPath}\`\n`);
        return;
    }

    stream.markdown(`⏳ Starting training...\n\n`);

    try {
        const output = execSync(`cmd /c "${runnerPath}" ${session}`, {
            cwd: path.join(workspaceRoot, 'Tools', 'TrainingCenter'),
            encoding: 'utf-8',
            timeout: 120000 // 2 min timeout
        });

        stream.markdown(`✅ Training completed.\n\n`);
        stream.markdown(`**Output**:\n\`\`\`\n${output.slice(-500)}\n\`\`\`\n`);
    } catch (err) {
        stream.markdown(`❌ Training failed: ${err.message}\n`);
    }
}

async function handleViewLog(stream, logsDir, filename) {
    if (!filename || filename.trim() === '') {
        stream.markdown(`❓ Παρακαλώ δώσε το όνομα του log file.\n\n`);
        stream.markdown(`Παράδειγμα: \`/view-log training_morning_20251021_203130.log\`\n`);
        return;
    }

    const logPath = path.join(logsDir, filename.trim());
    if (!fs.existsSync(logPath)) {
        stream.markdown(`❌ Log file not found: \`${filename}\`\n`);
        return;
    }

    const content = fs.readFileSync(logPath, 'utf-8');
    const lines = content.split('\n').slice(-50).join('\n');

    stream.markdown(`## 📄 Log: ${filename}\n\n`);
    stream.markdown(`\`\`\`\n${lines}\n\`\`\`\n`);
}

async function handleScheduledTasks(stream) {
    stream.markdown(`## 📅 Scheduled Training Tasks\n\n`);

    try {
        const output = execSync('schtasks /query /tn "Skyworks_AgentTraining_*" /fo LIST', { encoding: 'utf-8' });
        stream.markdown(`\`\`\`\n${output}\n\`\`\`\n`);
    } catch (err) {
        stream.markdown(`⚠️ Could not query scheduled tasks. Make sure tasks are created via \`create_agent_schedules.cmd\`.\n`);
    }
}

async function handleHelp(stream) {
    stream.markdown(`## 💬 Skyworks Chat Commands\n\n`);

    const commands = [
        { cmd: '/training-status', desc: 'Κατάσταση τελευταίου training' },
        { cmd: '/training-logs [N]', desc: 'Πιο πρόσφατα N logs (default: 5)' },
        { cmd: '/training-report', desc: 'Αναλυτική αναφορά training' },
        { cmd: '/agents', desc: 'Λίστα agents με expertise' },
        { cmd: '/ask-sora <Q>', desc: 'Ερώτηση σε SORA agent' },
        { cmd: '/ask-mission <Q>', desc: 'Ερώτηση σε Mission agent' },
        { cmd: '/project-status', desc: 'Που βρίσκεται το project' },
        { cmd: '/next-steps', desc: 'Επόμενα βήματα workflow' },
        { cmd: '/run-training [S]', desc: 'Τρέξε training τώρα (S=morning/afternoon/evening/now)' },
        { cmd: '/view-log <file>', desc: 'Δείξε log file' },
        { cmd: '/scheduled-tasks', desc: 'Προγραμματισμένες εργασίες' },
        { cmd: '/help', desc: 'Αυτή η λίστα' }
    ];

    commands.forEach(c => {
        stream.markdown(`- **\`${c.cmd}\`**: ${c.desc}\n`);
    });

    stream.markdown(`\n💡 **Tip**: Όλες οι εντολές ξεκινούν με \`/\` μέσα στο \`@skyworks\` chat.\n\n`);
    stream.markdown(`📖 Για λεπτομέρειες: \`Tools/TrainingCenter/CHAT_COMMANDS.md\`\n`);
}

function deactivate() {}

module.exports = {
    activate,
    deactivate
};
