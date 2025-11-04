"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.activate = activate;
exports.deactivate = deactivate;
const vscode = __importStar(require("vscode"));
const path_1 = require("path");
const fs_1 = require("fs");
let lastStatusGlobal;
function readStatus(root) {
    const statusPath = (0, path_1.join)(root, 'Docs', 'Knowledge', 'PROJECT_STATUS.json');
    try {
        const raw = (0, fs_1.readFileSync)(statusPath, 'utf-8');
        return JSON.parse(raw);
    }
    catch {
        return undefined;
    }
}
function ensureSessionLog(root) {
    const dir = (0, path_1.join)(root, 'Docs', 'Knowledge', 'SESSION_LOGS');
    if (!(0, fs_1.existsSync)(dir))
        (0, fs_1.mkdirSync)(dir, { recursive: true });
    return (0, path_1.join)(dir, `${new Date().toISOString().slice(0, 10)}.md`);
}
function writeStatus(root, data) {
    const statusPath = (0, path_1.join)(root, 'Docs', 'Knowledge', 'PROJECT_STATUS.json');
    const payload = {
        ...data,
        updatedAt: new Date().toISOString(),
    };
    (0, fs_1.writeFileSync)(statusPath, JSON.stringify(payload, null, 2), 'utf-8');
    return payload;
}
function activate(context) {
    const root = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;
    if (!root)
        return;
    const initialStatus = readStatus(root);
    const ch = vscode.window.createOutputChannel('SKYWORKS Assistant');
    let welcomePanel;
    const makePrompt = (s) => {
        const next = s ? (s.nextStep ?? s.currentStep + 1) : 1;
        const phase = s ? s.currentPhase : 1;
        return [
            '🚀 SKYWORKS PROJECT — Full Context Start',
            '',
            '📊 STEP 1: Read Project Structure',
            '- Open and read: Docs/Knowledge/PROJECT_PHASES_12.md (all 12 phases)',
            '- Understand: We are building a JARUS SORA 2.0/2.5 compliance platform',
            '',
            '🧠 STEP 2: Load Knowledge Base (via MCP)',
            '- MCP Server: skyworks-sora (already running)',
            '- DO NOT re-read the 23 EASA/JARUS files — they are pre-loaded in MCP memory',
            '- Available MCP tools: get_grc_table, calculate_sail, apply_mitigation, get_oso_requirements, validate_floor_rule, get_operations_manual_structure',
            '- Available MCP resources: sora-2.0-tables, sora-2.5-tables, operations-manual, air-risk-arc-tmpr, project-status, project-phases',
            '',
            '📍 STEP 3: Check Current Status',
            '- Call MCP tool: get_project_status',
            `- Current location: Phase ${phase}, Step ${s?.currentStep ?? '?'} → Next: ${next}`,
            `- Phase title: ${s?.phaseTitle ?? 'Unknown'}`,
            '',
            '✅ STEP 4: Start Working',
            `- Begin Step ${next} with full context`,
            '- Use MCP tools for all SORA calculations',
            '- Cite EASA/JARUS sources when making assertions',
            '',
            'Ready to proceed!'
        ].join('\n');
    };
    const makeGuardrailPrompt = (s) => {
        const next = s ? (s.nextStep ?? s.currentStep + 1) : 1;
        const phase = s ? s.currentPhase : 1;
        return [
            '🛡️ SKYWORKS PROJECT — Start with FULL GUARDRAILS',
            '',
            '📊 STEP 1: Read Project Structure',
            '- Read: Docs/Knowledge/PROJECT_PHASES_12.md (12-phase plan)',
            '- Read: PROJECT_ONBOARDING.md',
            '- Read: MCP_SERVER_GUIDE.md',
            '',
            '🧠 STEP 2: Verify Knowledge Readiness',
            '- MCP Server: skyworks-sora',
            '- Call MCP tool: knowledge_check',
            '- ⚠️ DO NOT PROCEED unless you get READY status',
            '- The 23 EASA/JARUS documents are pre-loaded in MCP memory',
            '',
            '📜 STEP 3: Read Evidence Policy',
            '- Read MCP resource: skyworks://policy/evidence-policy',
            '- Mandatory rules:',
            '  • Always cite EASA/JARUS (document + section/page)',
            '  • Use MCP tools for calculations (never re-implement)',
            '  • If citation missing → call search_sora_docs, ask for help, do NOT proceed',
            '  • Append all citations to: Docs/Knowledge/SESSION_EVIDENCE.md',
            '',
            '📍 STEP 4: Check Project Status',
            '- Call MCP tool: get_project_status',
            `- Current: Phase ${phase}, Step ${s?.currentStep ?? '?'} → Next: ${next}`,
            `- Title: ${s?.phaseTitle ?? 'Unknown'}`,
            '',
            '✅ STEP 5: Begin Work',
            `- Start Step ${next} following all guardrails above`,
            '- Every assertion = citation required',
            '- Log evidence as you work',
            '',
            'Proceed with full compliance!'
        ].join('\n');
    };
    const readStepMetadata = (root, step) => {
        const metadataPath = (0, path_1.join)(root, 'Docs', 'Knowledge', 'STEP_METADATA.json');
        if (!(0, fs_1.existsSync)(metadataPath))
            return undefined;
        try {
            const content = (0, fs_1.readFileSync)(metadataPath, 'utf8');
            const data = JSON.parse(content);
            return data.steps?.[step.toString()];
        }
        catch {
            return undefined;
        }
    };
    const makeClaudePrompt = (s, root) => {
        const currentStep = s?.currentStep ?? 1;
        const next = s ? (s.nextStep ?? s.currentStep + 1) : 1;
        const phase = s ? s.currentPhase : 1;
        const phaseTitle = s?.phaseTitle ?? 'Unknown';
        const metadata = readStepMetadata(root, next);
        if (metadata?.complexity === 'very_high') {
            const filesList = metadata.files_to_read.length > 0
                ? metadata.files_to_read.map(f => `- ${f}`).join('\n')
                : '- ContextPacks/OSO/pack.md';
            return [
                `Step ${next}: ${metadata.title}`,
                '',
                'READ:',
                filesList,
                '',
                'USE: @workspace /tools knowledge_check',
                '',
                'Greek comments. Ask questions first. Handoff to Copilot when done.'
            ].join('\n');
        }
        return [
            '🧠 SKYWORKS — Claude Sonnet 4',
            '',
            '🇬🇷 Μίλα ΜΟΝΟ στα Ελληνικά!',
            '',
            `� STATUS: Phase ${phase} (${phaseTitle}) | Step ${currentStep} → ${next}`,
            '',
            '📖 ΔΙΑΒΑΣΕ ΠΡΩΤΑ:',
            `1. Docs/Knowledge/PROJECT_PHASES_12.md → Βρες Step ${next}`,
            '2. PROJECT_ONBOARDING.md → Tech stack',
            '3. Docs/Knowledge/CLAUDE_WORKFLOW.md → Ποιος κάνει τι',
            '',
            '🎯 ΡΩΤΑ ΠΡΙΝ ΓΡΑΨΕΙΣ ΚΩΔΙΚΑ:',
            '1. Ποια αρχεία να διαβάσω;',
            '2. Από μηδέν ή συνεχίζω υπάρχοντα;',
            '3. Ειδικές απαιτήσεις;',
            '',
            `📤 ΟΤΑΝ ΤΕΛΕΙΩΣΕΙΣ → Πες: "✅ Step ${next} done! Δώσε στον Copilot για integration."`,
            '',
            '❌ ΜΗΝ: Διαβάσεις όλα τα 117 steps | Μιλάς Αγγλικά | Generic code',
            '✅ ΝΑΙ: Ρωτάς ερωτήσεις | Focused code | Clear handoff'
        ].join('\n');
    };
    const makeQuickPrompt = (s) => {
        const next = s ? (s.nextStep ?? s.currentStep + 1) : 1;
        const phase = s ? s.currentPhase : 1;
        return [
            '⚡ SKYWORKS PROJECT — Quick Continue',
            '',
            `📍 Current Status: Phase ${phase}, Step ${s?.currentStep ?? '?'} → Next: ${next}`,
            `Title: ${s?.phaseTitle ?? 'Unknown'}`,
            '',
            '🧠 MCP Server: skyworks-sora (active)',
            '- Tools: get_grc_table, calculate_sail, apply_mitigation, get_oso_requirements, validate_floor_rule',
            '- Resources: All SORA 2.0/2.5 tables, Operations Manual, Project Status',
            '',
            `✅ Ready to continue Step ${next}`,
            '- Use MCP tools for calculations',
            '- Cite EASA/JARUS when needed',
            '',
            'Let\'s go!'
        ].join('\n');
    };
    async function openWarmupDocs(rootDir) {
        const files = [
            'Docs/Knowledge/AI_WARMUP.md',
            'Docs/Knowledge/CLAUDE_WORKFLOW.md',
            'PROJECT_ONBOARDING.md',
            'MCP_SERVER_GUIDE.md'
        ];
        for (const rel of files) {
            const uri = vscode.Uri.file((0, path_1.join)(rootDir, rel));
            try {
                const doc = await vscode.workspace.openTextDocument(uri);
                await vscode.window.showTextDocument(doc, { preview: false, preserveFocus: true });
            }
            catch {
                // ignore if missing
            }
        }
    }
    async function logEvidenceFromClipboard(rootDir) {
        try {
            const text = await vscode.env.clipboard.readText();
            if (!text || !text.trim()) {
                vscode.window.showWarningMessage('Το clipboard είναι άδειο — αντιγράψτε citations πρώτα.');
                return;
            }
            const s = readStatus(rootDir);
            const logPath = (0, path_1.join)(rootDir, 'Docs', 'Knowledge', 'SESSION_EVIDENCE.md');
            const now = new Date().toISOString();
            const header = `\n\n## ${now} — Phase ${s?.currentPhase ?? '?'} Step ${s?.currentStep ?? '?'}`;
            if (!(0, fs_1.existsSync)(logPath))
                (0, fs_1.writeFileSync)(logPath, '# Session Evidence Log\n', 'utf-8');
            (0, fs_1.appendFileSync)(logPath, `${header}\n${text}\n`, 'utf-8');
            vscode.window.showInformationMessage('Καταγράφηκαν citations στο SESSION_EVIDENCE.md');
        }
        catch (e) {
            vscode.window.showErrorMessage(`Αποτυχία καταγραφής evidence: ${e}`);
        }
    }
    const showWelcome = (s) => {
        if (!welcomePanel) {
            welcomePanel = vscode.window.createWebviewPanel('skyworksWelcome', 'Skyworks — Welcome', vscode.ViewColumn.Active, { enableScripts: true });
            welcomePanel.onDidDispose(() => { welcomePanel = undefined; });
            welcomePanel.webview.onDidReceiveMessage(async (msg) => {
                if (msg?.command === 'start-full') {
                    const prompt = makePrompt(readStatus(root));
                    await vscode.env.clipboard.writeText(prompt);
                    await vscode.commands.executeCommand('workbench.action.chat.open');
                    vscode.window.showInformationMessage('✅ Full Context Start — Paste (Ctrl+V) στο chat!', { modal: false });
                }
                else if (msg?.command === 'start-guardrails') {
                    const prompt = makeGuardrailPrompt(readStatus(root));
                    await vscode.env.clipboard.writeText(prompt);
                    await vscode.commands.executeCommand('workbench.action.chat.open');
                    vscode.window.showInformationMessage('🛡️ Guardrails Active — Paste (Ctrl+V) στο chat!', { modal: false });
                }
                else if (msg?.command === 'start-claude') {
                    const prompt = makeClaudePrompt(readStatus(root), root);
                    // Create temporary file with the prompt
                    const doc = await vscode.workspace.openTextDocument({
                        content: prompt,
                        language: 'markdown'
                    });
                    await vscode.window.showTextDocument(doc, { preview: false, viewColumn: vscode.ViewColumn.Beside });
                    // Also copy to clipboard as backup
                    await vscode.env.clipboard.writeText(prompt);
                    // Show instructions
                    const response = await vscode.window.showInformationMessage('🧠 Claude Sonnet 4 Prompt Ready!\n\n1️⃣ Κάνε Ctrl+A (select all) στο tab που άνοιξε\n2️⃣ Ctrl+C (copy)\n3️⃣ Click το Continue icon αριστερά\n4️⃣ Ctrl+V στο Continue chat\n\nΉ πάτα OK για auto-copy!', { modal: true }, 'OK - Auto Copy');
                    if (response === 'OK - Auto Copy') {
                        // Select all text in the document
                        const editor = vscode.window.activeTextEditor;
                        if (editor) {
                            const firstLine = editor.document.lineAt(0);
                            const lastLine = editor.document.lineAt(editor.document.lineCount - 1);
                            editor.selection = new vscode.Selection(firstLine.range.start, lastLine.range.end);
                            // Copy to clipboard
                            await vscode.commands.executeCommand('editor.action.clipboardCopyAction');
                            // Open Continue sidebar
                            await vscode.commands.executeCommand('continue.continueGUIView.focus');
                            vscode.window.showInformationMessage('✅ Prompt copied! Πάτα Ctrl+V στο Continue chat αριστερά!');
                        }
                    }
                }
                else if (msg?.command === 'quick-continue') {
                    const prompt = makeQuickPrompt(readStatus(root));
                    await vscode.env.clipboard.writeText(prompt);
                    await vscode.commands.executeCommand('workbench.action.chat.open');
                    vscode.window.showInformationMessage('⚡ Quick Continue — Paste (Ctrl+V)!', { modal: false });
                }
                else if (msg?.command === 'warmup-ai') {
                    await openWarmupDocs(root);
                    vscode.window.showInformationMessage('📖 Warmup docs opened — Full context loaded!');
                }
                else if (msg?.command === 'log-evidence') {
                    await logEvidenceFromClipboard(root);
                }
            });
        }
        const sNow = s ?? readStatus(root);
        const next = sNow ? (sNow.nextStep ?? sNow.currentStep + 1) : 1;
        const html = `
      <html>
        <head>
          <style>
            body { font-family: sans-serif; margin: 1.2rem; background: #1e1e1e; color: #d4d4d4; }
            h2 { color: #4fc3f7; margin-bottom: 0.5rem; }
            .info-box { background: #2d2d30; padding: 12px; border-left: 4px solid #4fc3f7; margin: 16px 0; }
            .info-box ul { margin: 8px 0; padding-left: 20px; }
            .info-box li { margin: 4px 0; }
            .instructions { background: #3a3d41; padding: 12px; border-radius: 4px; margin: 16px 0; font-size: 0.9em; }
            .btn-group { display: flex; gap: 10px; flex-wrap: wrap; margin-top: 20px; }
            .btn { padding: 10px 16px; border: none; border-radius: 6px; cursor: pointer; font-size: 14px; font-weight: 600; transition: all 0.2s; }
            .btn:hover { transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.3); }
            .btn-primary { background: #007acc; color: white; }
            .btn-danger { background: #d32f2f; color: white; }
            .btn-success { background: #388e3c; color: white; }
            .btn-warning { background: #f57c00; color: white; }
            .btn-info { background: #0288d1; color: white; }
          </style>
        </head>
        <body>
          <h2>🚀 Skyworks — Καλώς ήρθες!</h2>
          
          <div class="info-box">
            <strong>📊 Project Status:</strong>
            <ul>
              <li><b>Φάση:</b> ${sNow?.currentPhase ?? '—'} — ${sNow?.phaseTitle ?? 'Unknown'}</li>
              <li><b>Τρέχον Βήμα:</b> ${sNow?.currentStep ?? '—'} → <b>Επόμενο:</b> ${next}</li>
              <li><b>Τελευταία ενημέρωση:</b> ${sNow?.updatedAt ?? '—'}</li>
            </ul>
            ${sNow?.pendingJobs && Object.keys(sNow.pendingJobs).length > 0 ? `
              <div style="margin-top: 12px; padding-top: 12px; border-top: 1px solid #4fc3f7;">
                <strong>📋 Pending Jobs:</strong>
                ${Object.entries(sNow.pendingJobs).map(([key, job]) => `
                  <div style="margin: 8px 0; padding: 8px; background: #1e1e1e; border-radius: 4px;">
                    <div style="font-weight: 600; color: #ffa726;">🔸 ${key.toUpperCase()}</div>
                    <div style="font-size: 0.85em; margin-top: 4px;">
                      <span style="color: #66bb6a;">Status: ${job.status}</span> | 
                      <span style="color: #42a5f5;">Priority: ${job.priority}</span> | 
                      <span style="color: #ffca28;">Progress: ${job.completedItems}/${job.totalItems} (${job.completionPercent}%)</span>
                    </div>
                    <div style="font-size: 0.8em; margin-top: 4px; color: #bdbdbd;">${job.description}</div>
                    ${job.remainingTasks && job.remainingTasks.length > 0 ? `
                      <div style="margin-top: 6px; font-size: 0.75em;">
                        <div style="color: #ff9800;">Remaining: ${job.remainingTasks.length} task(s)</div>
                      </div>
                    ` : ''}
                  </div>
                `).join('')}
              </div>
            ` : ''}
          </div>

          <div class="instructions">
            <strong>💡 Πώς λειτουργεί:</strong><br>
            1️⃣ Διάλεξε ένα κουμπί παρακάτω<br>
            2️⃣ Το Chat θα ανοίξει και το prompt θα είναι στο clipboard<br>
            3️⃣ Κάνε <strong>Ctrl+V</strong> στο chat για να ξεκινήσεις!<br><br>
            <strong>📍 Layout:</strong> Claude Sonnet 4 = Panel (πάνω από terminal) | Copilot = Sidebar (δεξιά)
          </div>

          <div class="btn-group">
            <button id="full" class="btn btn-primary">🚀 Full Context Start</button>
            <button id="guardrails" class="btn btn-danger">🛡️ Start with Guardrails</button>
            <button id="claude" class="btn btn-info">🧠 Start Claude Sonnet 4</button>
            <button id="quick" class="btn btn-info">⚡ Quick Continue</button>
          </div>

          <div class="btn-group" style="margin-top: 10px;">
            <button id="warmup" class="btn btn-success">📖 Open Warmup Docs</button>
            <button id="logEvidence" class="btn btn-warning">📝 Log Citations</button>
          </div>

          <div style="margin-top: 24px; padding: 12px; background: #2d2d30; border-radius: 4px; font-size: 0.85em;">
            <strong>🎯 Τι κάνει κάθε κουμπί:</strong><br><br>
            <strong>🚀 Full Context Start:</strong> Διαβάζει 12-φασικό πλάνο, φορτώνει MCP knowledge (23 docs), ελέγχει status, ξεκινάει δουλειά<br>
            <strong>🛡️ Guardrails:</strong> Όλα τα παραπάνω + mandatory citations, knowledge_check, evidence logging<br>
            <strong>🧠 Claude Sonnet 4:</strong> Focused prompt για πολύπλοκα tasks (algorithms, reports, manuals) — Άνοιγει Continue PANEL (πάνω από terminal)!<br>
            <strong>⚡ Quick Continue:</strong> Μόνο status check + MCP tools — για γρήγορη συνέχεια<br>
            <strong>📖 Warmup:</strong> Ανοίγει docs (AI_WARMUP, ONBOARDING, MCP_GUIDE, CLAUDE_WORKFLOW)<br>
            <strong>📝 Log Citations:</strong> Append clipboard → SESSION_EVIDENCE.md
          </div>

          <script>
            const vscodeApi = acquireVsCodeApi();
            document.getElementById('full').addEventListener('click', () => {
              vscodeApi.postMessage({ command: 'start-full' });
            });
            document.getElementById('guardrails').addEventListener('click', () => {
              vscodeApi.postMessage({ command: 'start-guardrails' });
            });
            document.getElementById('claude').addEventListener('click', () => {
              vscodeApi.postMessage({ command: 'start-claude' });
            });
            document.getElementById('quick').addEventListener('click', () => {
              vscodeApi.postMessage({ command: 'quick-continue' });
            });
            document.getElementById('warmup').addEventListener('click', () => {
              vscodeApi.postMessage({ command: 'warmup-ai' });
            });
            document.getElementById('logEvidence').addEventListener('click', () => {
              vscodeApi.postMessage({ command: 'log-evidence' });
            });
          </script>
        </body>
      </html>`;
        if (welcomePanel)
            welcomePanel.webview.html = html;
    };
    const readyMessage = () => {
        const s = initialStatus;
        if (s) {
            const next = s.nextStep ?? (s.currentStep + 1);
            ch.appendLine('=== SKYWORKS READY ===');
            ch.appendLine(`Φάση: ${s.currentPhase} — ${s.phaseTitle ?? ''}`);
            ch.appendLine(`Βήμα: ${s.currentStep} → Επόμενο: ${next}`);
            ch.appendLine(`Τελευταία ενημέρωση: ${s.updatedAt ?? 'n/a'}`);
            if (s.notes)
                ch.appendLine(`Σημειώσεις: ${s.notes}`);
            ch.show(true);
            vscode.window.showInformationMessage(`Έτοιμοι για Βήμα ${next} της Φάσης ${s.currentPhase}.`, 'Άνοιγμα PROJECT_ONBOARDING.md').then(choice => {
                if (choice) {
                    const p = vscode.Uri.file((0, path_1.join)(root, 'PROJECT_ONBOARDING.md'));
                    vscode.commands.executeCommand('vscode.open', p);
                }
            });
        }
        else {
            vscode.window.showInformationMessage('SKYWORKS: Δεν βρέθηκε PROJECT_STATUS.json — θα δημιουργηθεί όταν ενημερωθεί από MCP.');
        }
    };
    // Startup: show only welcome panel (no auto-open docs)
    showWelcome(initialStatus);
    // Command registrations
    context.subscriptions.push(vscode.commands.registerCommand('skyworks.assistant.readyMessage', readyMessage), vscode.commands.registerCommand('skyworks.assistant.openSummary', () => {
        const p = vscode.Uri.file((0, path_1.join)(root, 'Docs', 'Knowledge', 'PROJECT_PHASES_12.md'));
        vscode.commands.executeCommand('vscode.open', p);
    }));
    // Prepare for shutdown summary: keep last known status in memory
    lastStatusGlobal = initialStatus;
    // Watch for dynamic changes in PROJECT_STATUS.json and update UI/webview
    const watcher = vscode.workspace.createFileSystemWatcher(new vscode.RelativePattern((0, path_1.join)(root, 'Docs', 'Knowledge'), 'PROJECT_STATUS.json'));
    const onChange = () => {
        const s = readStatus(root);
        lastStatusGlobal = s;
        const next = s ? (s.nextStep ?? s.currentStep + 1) : undefined;
        if (s && next !== undefined) {
            vscode.window.setStatusBarMessage(`SKYWORKS: Φάση ${s.currentPhase}, Βήμα ${s.currentStep} → ${next}`, 5000);
        }
        showWelcome(s);
    };
    watcher.onDidChange(onChange, undefined, context.subscriptions);
    watcher.onDidCreate(onChange, undefined, context.subscriptions);
    watcher.onDidDelete(onChange, undefined, context.subscriptions);
    context.subscriptions.push(watcher);
}
function deactivate() {
    try {
        const root = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;
        if (!root)
            return;
        const s = lastStatusGlobal ?? readStatus(root);
        const log = ensureSessionLog(root);
        const now = new Date().toISOString();
        const line = s
            ? `\n- ${now}: Φάση ${s.currentPhase} (${s.phaseTitle ?? ''}), Βήμα ${s.currentStep} → Επόμενο ${s.nextStep ?? s.currentStep + 1}`
            : `\n- ${now}: Καμία διαθέσιμη κατάσταση (PROJECT_STATUS.json δεν βρέθηκε)`;
        if (!(0, fs_1.existsSync)(log))
            (0, fs_1.writeFileSync)(log, `# Session Log ${now.slice(0, 10)}\n`, 'utf-8');
        (0, fs_1.appendFileSync)(log, line, 'utf-8');
    }
    catch {
        // ignore
    }
}
//# sourceMappingURL=extension.js.map