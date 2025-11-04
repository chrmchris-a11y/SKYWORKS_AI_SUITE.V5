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
            } else if (command === 'plan-mission') {
                await handlePlanMission(stream, prompt, workspaceRoot);
            } else if (command === 'evidence-sync') {
                await handleEvidenceSync(stream, prompt, workspaceRoot);
            } else if (command === 'robustness-check') {
                await handleRobustnessCheck(stream, prompt, workspaceRoot);
            } else if (command === 'make-binder') {
                await handleMakeBinder(stream, prompt, workspaceRoot);
            } else if (command === 'cga-generate') {
                await handleCgaGenerate(stream, prompt, workspaceRoot);
            } else if (command === 'what-if') {
                await handleWhatIf(stream, prompt, workspaceRoot);
            } else if (command === 'permit-list') {
                await handlePermitList(stream, prompt, workspaceRoot);
            } else if (command === 'permit-add') {
                await handlePermitAdd(stream, prompt, workspaceRoot);
            } else if (command === 'off-peak') {
                await handleOffPeak(stream, prompt, workspaceRoot);
            } else if (command === 'risk-guard') {
                await handleRiskGuard(stream, prompt, workspaceRoot);
            } else if (command === 'email-bundle') {
                await handleEmailBundle(stream, prompt, workspaceRoot);
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
    { cmd: '/plan-mission key=value ...', desc: 'Δημιούργησε Mission Briefing (ERP/Crew/Approvals) και αρχεία στο Docs/Compliance/Missions (προαιρετικά: updateMatrix=true)' },
    { cmd: '/evidence-sync [scenario=Facade|Stadium|...]', desc: 'Σάρωση Evidence και αυτόματη ενημέρωση OSO matrix' },
    { cmd: '/robustness-check [sail=II|III] [scenario=...]', desc: 'Έλεγχος robustness OSO (αναφορά μόνο)' },
    { cmd: '/make-binder operation=facade time=06:00 [mission=Docs/Compliance/Missions/Facade_0600.md]', desc: 'Συνθέτει Compliance Binder (briefing + αποσπάσματα OSO + approvals) σε Docs/Compliance/Binder' },
    { cmd: '/cga-generate name=Facade_0600 coords="lon,lat;lon,lat;..."', desc: 'Παράγει GeoJSON/KML για CGA και τα αποθηκεύει στο Docs/Compliance/CGA' },
    { cmd: '/what-if current=III target=II scenario=Facade', desc: 'Προτάσεις για μείωση SAIL (αναφορά με OSO/mitigations)' },
    { cmd: '/permit-list [filter=active|expired|all] [entity=...] [type=...]', desc: 'Λίστα αδειών/γνωστοποιήσεων από permits.json' },
    { cmd: '/permit-add key=value ...', desc: 'Καταχώρηση νέας άδειας (type/entity/location/valid_from/valid_to/status/reference/notes)' },
    { cmd: '/off-peak scenario=Facade zone=urban [day=weekday|weekend]', desc: 'Προτεινόμενα off-peak χρονικά παράθυρα για μείωση GRC' },
    { cmd: '/risk-guard rssi=-55 wind=8 gust=14 flow=low', desc: 'Γρήγορη αξιολόγηση live ρίσκων (C2/καιρός/ροή πεζών)' },
    { cmd: '/email-bundle scenario=Facade time=06:00 to="ops@example.com,qa@example.com"', desc: 'Δημιουργεί ZIP (Binder+Reports) και το στέλνει email ή αποθηκεύει .eml draft' },
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

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Mission Planner Helper
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

async function handlePlanMission(stream, prompt, workspaceRoot) {
    stream.markdown(`## 🗺️ Mission Planner\n\n`);

    // Parse key=value pairs
    // Example: operation=facade time=06:00 zone=urban airspace=G sail=II entries=3 length=120 format=both
    const defaults = {
        operation: 'facade',
        time: '06:00',
        zone: 'urban',
        airspace: 'G',
        sail: 'II',
        entries: '2',
        length: '100',
        format: 'both',
        updateMatrix: 'false'
    };

    const args = Object.assign({}, defaults);
    if (prompt && prompt.trim().length > 0) {
        const tokens = prompt.split(/\s+/);
        tokens.forEach(tok => {
            const [k, v] = tok.split('=');
            if (!k || typeof v === 'undefined') return;
            const key = k.trim().toLowerCase();
            const val = v.trim();
            if (['operation','time','zone','airspace','sail','entries','length','format','out','updatematrix'].includes(key)) {
                args[key] = val;
            }
        });
    } else {
        stream.markdown(`Χρήση: \`/plan-mission operation=facade time=06:00 zone=urban airspace=G sail=II entries=3 length=120 format=both\`\n\n`);
    }

    const missionsDir = path.join(workspaceRoot, 'Docs', 'Compliance', 'Missions');
    try { if (!fs.existsSync(missionsDir)) fs.mkdirSync(missionsDir, { recursive: true }); } catch (_) {}

    const opName = String(args.operation || 'mission').toLowerCase();
    const timeSafe = String(args.time || 'time').replace(/[:]/g, '');
    const baseName = `${opName}_${timeSafe}`;
    const outPath = args.out ? path.isAbsolute(args.out) ? args.out : path.join(workspaceRoot, args.out) : path.join(missionsDir, `${baseName}.md`);
    const jsonOutPath = outPath.replace(/\.md$/i, '.json');

    // Build python command
    const script = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'mission_planner.py');
    const cmd = `python "${script}" --operation ${args.operation} --time ${args.time} --zone ${args.zone} --airspace ${args.airspace} --sail-target ${args.sail} --entries ${args.entries} --length-m ${args.length} --format ${args.format} --out "${outPath}" --json-out "${jsonOutPath}"`;

    stream.markdown(`⏳ Δημιουργία mission briefing…\n\n`);
    try {
        const output = execSync(cmd, { cwd: workspaceRoot, encoding: 'utf-8', timeout: 60000, maxBuffer: 10 * 1024 * 1024 });
        // Show generated markdown (script prints MD even when writing to file)
        stream.markdown(output);
        stream.markdown(`\n📁 Αποθηκεύτηκε: \`${path.relative(workspaceRoot, outPath)}\`\n`);
        stream.markdown(`📁 JSON: \`${path.relative(workspaceRoot, jsonOutPath)}\`\n`);

        // Quick tips to link with OSO matrix
        stream.markdown(`\n🔗 Tip: Πρόσθεσε στο OSO matrix ως evidence: \`${path.relative(workspaceRoot, outPath)}\`\n`);

        // Optional OSO matrix auto-update
        const doUpdate = String(args.updatematrix || args.updateMatrix || 'false').toLowerCase() === 'true';
        if (doUpdate) {
            const updated = updateOSOMatrix(workspaceRoot, args.operation, args.time, path.relative(workspaceRoot, outPath), args.sail);
            if (updated) {
                stream.markdown(`✅ OSO matrix ενημερώθηκε αυτόματα με evidence, Owner=chrmc και σημερινή ημερομηνία.\n`);
            } else {
                stream.markdown(`⚠️ Δεν βρέθηκε αντίστοιχο Scenario Pack για αυτόματη ενημέρωση OSO. Έγινε μόνο δημιουργία briefing.\n`);
            }
        }
    } catch (err) {
        stream.markdown(`❌ Mission planner failed: ${err.message}\n`);
        stream.markdown(`Χρήση: \`/plan-mission operation=facade time=06:00 zone=urban airspace=G sail=II entries=3 length=120 format=both\`\n`);
    }
}

async function handleEvidenceSync(stream, prompt, workspaceRoot) {
    stream.markdown(`## 📎 Evidence Sync\n\n`);
    let scenario = null;
    if (prompt && prompt.trim()) {
        const tokens = prompt.split(/\s+/);
        tokens.forEach(tok => {
            const [k, v] = tok.split('=');
            if (k && v && k.toLowerCase() === 'scenario') scenario = v;
        });
    }
    const script = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'evidence_sync.py');
    const args = scenario ? ` --scenario "${scenario}"` : '';
    try {
        const out = execSync(`python "${script}"${args}`, { cwd: workspaceRoot, encoding: 'utf-8', timeout: 60000 });
        stream.markdown(out ? out : 'Done.');
        stream.markdown(`\n✅ Αν βρέθηκαν artifacts, ενημερώθηκε το \`Docs/Compliance/OSO_to_Evidence_Matrix.md\`.\n`);
    } catch (err) {
        stream.markdown(`❌ Evidence sync failed: ${err.message}\n`);
    }
}

async function handleRobustnessCheck(stream, prompt, workspaceRoot) {
    stream.markdown(`## 🧪 OSO Robustness Check\n\n`);
    let sail = null, scenario = null;
    if (prompt && prompt.trim()) {
        const tokens = prompt.split(/\s+/);
        tokens.forEach(tok => {
            const [k, v] = tok.split('=');
            if (k && v) {
                if (k.toLowerCase() === 'sail') sail = v;
                if (k.toLowerCase() === 'scenario') scenario = v;
            }
        });
    }
    const script = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'robustness_check.py');
    const args = `${sail ? ` --sail ${sail}` : ''}${scenario ? ` --scenario "${scenario}"` : ''}`;
    try {
        const out = execSync(`python "${script}"${args}`, { cwd: workspaceRoot, encoding: 'utf-8', timeout: 60000 });
        stream.markdown(out ? out : 'Done.');
    } catch (err) {
        stream.markdown(`❌ Robustness check failed: ${err.message}\n`);
    }
}

function updateOSOMatrix(workspaceRoot, operation, time, briefingRelPath, sailTarget) {
    try {
        const filePath = path.join(workspaceRoot, 'Docs', 'Compliance', 'OSO_to_Evidence_Matrix.md');
        if (!fs.existsSync(filePath)) return false;
        const raw = fs.readFileSync(filePath, 'utf-8');
        const lines = raw.split('\n');

        const op = String(operation || '').toLowerCase();
        const titleMap = {
            'facade': 'Facade Cleaning',
            'windows': 'Windows Cleaning',
            'roof': 'Roof Cleaning',
            'pv': 'PV Park Cleaning', 'pvpark': 'PV Park Cleaning', 'pv_park': 'PV Park Cleaning', 'solar': 'PV Park Cleaning', 'solarpark': 'PV Park Cleaning',
            'wind': 'Wind Turbine Cleaning', 'wind_turbine': 'Wind Turbine Cleaning', 'turbine': 'Wind Turbine Cleaning',
            'stadium': 'Stadium Cleaning'
        };
        const scenarioTitle = titleMap[op];
        if (!scenarioTitle) return false;
        const headerPrefix = `## Scenario Pack: ${scenarioTitle}`;

        const timeNorm = String(time || '').trim(); // Expect HH:MM
        const headerIndex = lines.findIndex(l => l.startsWith(headerPrefix) && l.includes(timeNorm));
        const today = new Date();
        const yyyy = today.getFullYear();
        const mm = String(today.getMonth() + 1).padStart(2, '0');
        const dd = String(today.getDate()).padStart(2, '0');
        const dateStr = `${yyyy}-${mm}-${dd}`;
        const evidenceNote = `Mission briefing: \`${briefingRelPath.replace(/\\/g, '/')}\``;

        if (headerIndex === -1) {
            // Append a new Scenario Pack at the end
            const sailStr = sailTarget ? ` (Target SAIL ${String(sailTarget).toUpperCase()})` : '';
            const newBlock = [
                '',
                '---',
                '',
                `${headerPrefix} — ${timeNorm}${sailStr}`,
                '',
                `Auto-generated via /plan-mission on ${dateStr}. Adjust robustness and details as required by the authority.`,
                '',
                '| OSO (Code/Title) | Required Robustness | Implementation — Technical | Implementation — Operational | Implementation — Organizational | Evidence (Docs/Tests/Records) | Status | Owner | Last Update |',
                '|---|:--:|---|---|---|---|:--:|---|---|',
                `| CGA enforcement & signage | M | CGA geofence aligned; barriers/signage | Marshals at entries; diversion plan | OM CGA SOP; marshal training | ${evidenceNote} | In progress | chrmc | ${dateStr} |`,
                `| Operation mode & LOS assurance | M | VLOS/EVLOS placement; corner coverage | Observer handovers; stand-off | Training on LOS coverage; role cards | ${evidenceNote} | In progress | chrmc | ${dateStr} |`,
                `| C2 reliability & EMI handling | M | RSSI/latency baseline; antenna orientation | Abort triggers; comms protocol | Comms plan; anomalies training | ${evidenceNote} | In progress | chrmc | ${dateStr} |`,
                `| Tooling leak/fail-safe | M | Auto shutoff; non-conductive lines/booms | Dry-run per shift; spill kit | Maintenance & tooling SOP | ${evidenceNote} | In progress | chrmc | ${dateStr} |`,
                `| ELS mapping & egress | M | Pre-marked ELS; RTH path check | Brief ELS; clear taxi corridors | Emergency response plan | ${evidenceNote} | Gap | chrmc | ${dateStr} |`,
                ''
            ].join('\n');
            fs.writeFileSync(filePath, raw + '\n' + newBlock, 'utf-8');
            return true;
        }

        // Find table start (line with table header)| next lines until a blank line or next '---' or next '## '
        let i = headerIndex + 1;
        // Skip description lines until we hit the table header row starting with '| OSO'
        while (i < lines.length && !(/^\|\s*OSO/.test(lines[i]))) i++;
        if (i >= lines.length) return false;
        const headerRowIndex = i;
        // Next line is the separator line of the table (---)
        i += 2; // Move to first data row (skip header and separator)

        // Update rows until a blank line or a line starting with '---' or '## '
        while (i < lines.length) {
            const line = lines[i];
            if (!line.trim()) break;
            if (line.startsWith('---') || line.startsWith('## ')) break;
            if (!line.startsWith('|')) { i++; continue; }
            // Skip the separator row (with --- in columns)
            if (line.includes('|---')) { i++; continue; }

            const parts = line.split('|');
            if (parts.length < 11) { i++; continue; }
            // parts[0] empty before first pipe, columns 1..9 meaningful, parts[last] empty
            // indices: 1:OSO,2:Req,3:Tech,4:Oper,5:Org,6:Evidence,7:Status,8:Owner,9:LastUpdate
            const evidence = parts[6].trim();
            const ownerIdx = 8;
            const dateIdx = 9;
            let newEvidence = evidence;
            if (!evidence.includes(briefingRelPath)) {
                newEvidence = evidence ? `${evidence}; ${evidenceNote}` : evidenceNote;
            }
            parts[6] = ` ${newEvidence} `;
            parts[ownerIdx] = ' chrmc ';
            parts[dateIdx] = ` ${dateStr} `;
            lines[i] = parts.join('|');
            i++;
        }

        fs.writeFileSync(filePath, lines.join('\n'), 'utf-8');
        return true;
    } catch (e) {
        return false;
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Compliance Binder & CGA Handlers
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

async function handleMakeBinder(stream, prompt, workspaceRoot) {
    stream.markdown(`## 📚 Compliance Binder\n\n`);
    // Defaults
    const defaults = { operation: 'facade', time: '06:00', mission: null, out: null };
    const args = { ...defaults };
    if (prompt && prompt.trim()) {
        const tokens = prompt.split(/\s+/);
        tokens.forEach(tok => {
            const [k, v] = tok.split('=');
            if (!k || typeof v === 'undefined') return;
            const key = k.trim().toLowerCase();
            const val = v.trim();
            if (['operation','time','mission','out'].includes(key)) args[key] = val;
        });
    } else {
        stream.markdown(`Χρήση: \`/make-binder operation=facade time=06:00 mission=Docs/Compliance/Missions/Facade_0600.md\`\n\n`);
    }

    const script = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'compliance_binder.py');
    const cmd = `python "${script}" --operation ${args.operation} --time ${args.time}`
        + (args.mission ? ` --mission "${path.isAbsolute(args.mission) ? args.mission : path.join(workspaceRoot, args.mission)}"` : '')
        + (args.out ? ` --out "${path.isAbsolute(args.out) ? args.out : path.join(workspaceRoot, args.out)}"` : '');
    try {
        const output = execSync(cmd, { cwd: workspaceRoot, encoding: 'utf-8', timeout: 60000 });
        stream.markdown(output || 'Done.');
    } catch (err) {
        stream.markdown(`❌ Binder failed: ${err.message}\n`);
    }
}

async function handleCgaGenerate(stream, prompt, workspaceRoot) {
    stream.markdown(`## 🗺️ CGA Generator\n\n`);
    // Expected: name=Facade_0600 coords="lon,lat;lon,lat;..." OR center=lon,lat width=30 height=20
    const args = { name: 'CGA_Area', coords: null, center: null, width: null, height: null };
    if (prompt && prompt.trim()) {
        const tokens = prompt.split(/\s+/);
        tokens.forEach(tok => {
            const [k, v] = tok.split('=');
            if (!k || typeof v === 'undefined') return;
            const key = k.trim().toLowerCase();
            const val = v.trim();
            if (['name','coords','center','width','height'].includes(key)) args[key] = val;
        });
    } else {
        stream.markdown(`Χρήση: \`/cga-generate name=Facade_0600 coords="23.7275,37.9838;23.7277,37.9839;..."\`\n\n`);
    }

    const script = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'cga_generator.py');
    const cmd = `python "${script}" --name "${args.name}"`
        + (args.coords ? ` --coords "${args.coords}"` : '')
        + (args.center ? ` --center "${args.center}"` : '')
        + (args.width ? ` --width ${args.width}` : '')
        + (args.height ? ` --height ${args.height}` : '');
    try {
        const output = execSync(cmd, { cwd: workspaceRoot, encoding: 'utf-8', timeout: 60000 });
        stream.markdown(output || 'Done.');
        stream.markdown(`\n✅ Τα αρχεία σώθηκαν στον φάκελο \`Docs/Compliance/CGA\`.\n`);
    } catch (err) {
        stream.markdown(`❌ CGA generation failed: ${err.message}\n`);
    }
}

async function handleWhatIf(stream, prompt, workspaceRoot) {
    stream.markdown(`## 🤔 What-if SAIL Reduction\n\n`);
    let current = null, target = null, scenario = null;
    if (prompt && prompt.trim()) {
        const tokens = prompt.split(/\s+/);
        tokens.forEach(tok => {
            const [k, v] = tok.split('=');
            if (!k || typeof v === 'undefined') return;
            const key = k.trim().toLowerCase();
            const val = v.trim();
            if (key === 'current') current = val;
            if (key === 'target') target = val;
            if (key === 'scenario') scenario = val;
        });
    } else {
        stream.markdown(`Χρήση: \`/what-if current=III target=II scenario=Facade\`\n\n`);
    }
    const script = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'what_if_engine.py');
    const args = `${current ? ` --current ${current}` : ''}${target ? ` --target ${target}` : ''}${scenario ? ` --scenario "${scenario}"` : ''}`;
    try {
        const output = execSync(`python "${script}"${args}`, { cwd: workspaceRoot, encoding: 'utf-8', timeout: 60000 });
        stream.markdown(output || 'Done.');
    } catch (err) {
        stream.markdown(`❌ What-if failed: ${err.message}\n`);
    }
}

async function handlePermitList(stream, prompt, workspaceRoot) {
    stream.markdown(`## 🗂️ Permits — List\n\n`);
    let filter = null, entity = null, type = null;
    if (prompt && prompt.trim()) {
        const tokens = prompt.split(/\s+/);
        tokens.forEach(tok => {
            const [k, v] = tok.split('=');
            if (!k || typeof v === 'undefined') return;
            const key = k.trim().toLowerCase();
            const val = v.trim();
            if (key === 'filter') filter = val;
            if (key === 'entity') entity = val;
            if (key === 'type') type = val;
        });
    }
    const script = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'permit_tracker.py');
    const args = `${filter ? ` --filter ${filter}` : ''}${entity ? ` --entity "${entity}"` : ''}${type ? ` --type ${type}` : ''}`;
    try {
        const output = execSync(`python "${script}" --list${args}`, { cwd: workspaceRoot, encoding: 'utf-8', timeout: 60000 });
        stream.markdown(output || 'Done.');
    } catch (err) {
        stream.markdown(`❌ Permit list failed: ${err.message}\n`);
    }
}

async function handlePermitAdd(stream, prompt, workspaceRoot) {
    stream.markdown(`## ➕ Permits — Add\n\n`);
    if (!prompt || !prompt.trim()) {
        stream.markdown(`Χρήση: \`/permit-add type=venue entity="City of X" location="Main Square" valid_from=2025-10-22 valid_to=2025-10-23 status=approved reference=REF123 notes="Dawn ops"\`\n`);
        return;
    }
    const fields = {};
    const tokens = prompt.split(/\s+/);
    tokens.forEach(tok => {
        const eq = tok.indexOf('=');
        if (eq <= 0) return;
        const k = tok.slice(0, eq).toLowerCase();
        let v = tok.slice(eq + 1);
        // strip quotes if present
        v = v.replace(/^"|"$/g, '');
        fields[k] = v;
    });
    const script = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'permit_tracker.py');
    const args = Object.entries(fields).map(([k, v]) => ` --${k} "${v}"`).join('');
    try {
        const output = execSync(`python "${script}" --add${args}`, { cwd: workspaceRoot, encoding: 'utf-8', timeout: 60000 });
        stream.markdown(output || 'Done.');
    } catch (err) {
        stream.markdown(`❌ Permit add failed: ${err.message}\n`);
    }
}

async function handleOffPeak(stream, prompt, workspaceRoot) {
    stream.markdown(`## ⏰ Off-peak Optimizer\n\n`);
    let scenario = null, zone = null, day = null;
    if (prompt && prompt.trim()) {
        const tokens = prompt.split(/\s+/);
        tokens.forEach(tok => {
            const [k, v] = tok.split('=');
            if (!k || typeof v === 'undefined') return;
            const key = k.trim().toLowerCase();
            const val = v.trim();
            if (key === 'scenario') scenario = val;
            if (key === 'zone') zone = val;
            if (key === 'day') day = val;
        });
    }
    const script = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'off_peak_optimizer.py');
    const args = `${scenario ? ` --scenario ${scenario}` : ''}${zone ? ` --zone ${zone}` : ''}${day ? ` --day ${day}` : ''}`;
    try {
        const output = execSync(`python "${script}"${args}`, { cwd: workspaceRoot, encoding: 'utf-8', timeout: 60000 });
        stream.markdown(output || 'Done.');
    } catch (err) {
        stream.markdown(`❌ Off-peak failed: ${err.message}\n`);
    }
}

async function handleRiskGuard(stream, prompt, workspaceRoot) {
    stream.markdown(`## 🛡️ Real-time Risk Guard\n\n`);
    // params: rssi, wind, gust, flow
    let rssi = null, wind = null, gust = null, flow = null;
    if (prompt && prompt.trim()) {
        const tokens = prompt.split(/\s+/);
        tokens.forEach(tok => {
            const [k, v] = tok.split('=');
            if (!k || typeof v === 'undefined') return;
            const key = k.trim().toLowerCase();
            const val = v.trim();
            if (key === 'rssi') rssi = val;
            if (key === 'wind') wind = val;
            if (key === 'gust') gust = val;
            if (key === 'flow') flow = val;
        });
    }
    const script = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'risk_guard.py');
    const args = `${rssi ? ` --rssi ${rssi}` : ''}${wind ? ` --wind ${wind}` : ''}${gust ? ` --gust ${gust}` : ''}${flow ? ` --flow ${flow}` : ''}`;
    try {
        const output = execSync(`python "${script}"${args}`, { cwd: workspaceRoot, encoding: 'utf-8', timeout: 60000 });
        stream.markdown(output || 'Done.');
    } catch (err) {
        stream.markdown(`❌ Risk guard failed: ${err.message}\n`);
    }
}

async function handleEmailBundle(stream, prompt, workspaceRoot) {
    stream.markdown(`## ✉️ Email Deliverables\n\n`);
    // Parse: scenario, time, to
    let scenario = null, time = null, to = null;
    if (prompt && prompt.trim()) {
        const tokens = prompt.split(/\s+/);
        tokens.forEach(tok => {
            const [k, v] = tok.split('=');
            if (!k || typeof v === 'undefined') return;
            const key = k.trim().toLowerCase();
            let val = v.trim();
            val = val.replace(/^"|"$/g, '');
            if (key === 'scenario') scenario = val;
            if (key === 'time') time = val;
            if (key === 'to') to = val;
        });
    }
    if (!scenario || !time || !to) {
        stream.markdown(`Χρήση: \`/email-bundle scenario=Facade time=06:00 to="ops@example.com"\`\n`);
        return;
    }
    try {
        // 1) Bundle zip
        const bundler = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'bundle_deliverables.py');
        const zipRel = execSync(`python "${bundler}" --scenario ${scenario} --time ${time}`, { cwd: workspaceRoot, encoding: 'utf-8', timeout: 60000 }).trim();
        const zipAbs = path.join(workspaceRoot, zipRel);
        stream.markdown(`📦 Bundle: \`${zipRel}\``);

        // 2) Email (SMTP if env set; else EML draft)
        const sender = path.join(workspaceRoot, 'Tools', 'TrainingCenter', 'email_sender.py');
        const subject = `${scenario} ${time} — Deliverables`;
        const body = `Αυτόματο πακέτο παραδοτέων για ${scenario} ${time}. Περιλαμβάνει Binder και Reports.`;
        const res = execSync(`python "${sender}" --to "${to}" --subject "${subject}" --body "${body}" --attach "${zipAbs}"`, { cwd: workspaceRoot, encoding: 'utf-8', timeout: 60000 }).trim();
        if (res === 'SENT') {
            stream.markdown(`✅ Εστάλη email στους: ${to}`);
        } else {
            stream.markdown(`✍️ Δημιουργήθηκε draft: \`${res}\` (άνοιξέ το με email client)`);
        }
    } catch (err) {
        stream.markdown(`❌ Email bundle failed: ${err.message}\n`);
    }
}
