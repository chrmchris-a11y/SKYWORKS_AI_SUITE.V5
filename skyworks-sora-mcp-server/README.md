# SKYWORKS SORA MCP Server

**Expert JARUS SORA 2.0/2.5 knowledge for VS Code AI agents**

## What is this?

A Model Context Protocol (MCP) server that provides **instant access** to:

✅ SORA 2.0 & 2.5 tables (GRC, SAIL, OSO)  
✅ M1/M2/M3 mitigation formulas  
✅ Floor rule validation  
✅ 23 EASA/JARUS regulatory documents  
✅ Operations Manual structure (Part A-T)  

**No more re-reading files!** All knowledge pre-loaded in memory.

## Quick Start

### 1. Install

```powershell
# Run installation script (ONE TIME)
.\install.ps1
```

This will:
- Check Node.js
- Install dependencies
- Compile TypeScript
- Test the server
- Generate VS Code config

### 2. Configure VS Code

Add to `settings.json`:

```json
{
  "mcp.servers": {
    "skyworks-sora": {
      "command": "node",
      "args": ["C:/Users/chrmc/Desktop/SKYWORKS_AI_SUITE.V5/skyworks-sora-mcp-server/build/index.js"]
    }
  }
}
```

### 3. Reload VS Code

```
Ctrl+Shift+P → "Developer: Reload Window"
```

### 4. Test

Ask AI agent:
```
Use MCP tool: get_grc_table with version 2.5
```

Should return SORA 2.5 GRC table instantly.

## Available Tools (7)

```typescript
get_grc_table({ version, scenario, ua_size_column })
calculate_sail({ final_grc, residual_arc })
apply_mitigation({ version, intrinsic_grc, m1_level, m2_level, ... })
get_oso_requirements({ sail })
validate_floor_rule({ version, final_grc, scenario, ua_size_column })
search_sora_docs({ query, document? })
get_operations_manual_structure({ part? })
```

## Available Resources (4)

```
skyworks://knowledge/sora-2.0-tables
skyworks://knowledge/sora-2.5-tables
skyworks://knowledge/operations-manual
skyworks://knowledge/air-risk-arc-tmpr
```

## Documentation

- **Full Guide:** `../MCP_SERVER_GUIDE.md`
- **Project Overview:** `../PROJECT_ONBOARDING.md`
- **New Chat Template:** `../README_NEW_CHAT.md`

## Development

```powershell
# Install dependencies
npm install

# Build
npm run build

# Watch mode (auto-rebuild)
npm run watch

# Test
node build/index.js
# Should print: "SKYWORKS SORA MCP Server running on stdio"
```

## Architecture

```
src/
└── index.ts          # Main server (pre-loaded SORA tables, 7 tools, 4 resources)

build/
└── index.js          # Compiled output (run this in VS Code)
```

## Pre-loaded Knowledge

**SORA Tables:**
- GRC Table 2.0 (7 scenarios × 4 sizes)
- iGRC Table 2.5 (7 density levels × 5 sizes)
- M1/M2/M3 mitigation rules (2.0 & 2.5)
- SAIL matrix (Table 5/7)
- OSO requirements (17-24 OSOs)

**Documents:**
- 4× PERMANENT MEMORY files
- 23× EASA/JARUS documents (indexed for search)

## Performance

- **Startup:** <1 second
- **Tool calls:** <100ms
- **Memory:** ~50 MB
- **Speedup:** 50-100× vs manual file reading

## Troubleshooting

**Server not showing in VS Code:**
1. Check `settings.json` path (forward slashes!)
2. Reload window (`Ctrl+Shift+P`)
3. Check VS Code Output → "MCP Servers"

**Tools return errors:**
1. Run `npm install`
2. Run `npm run build`
3. Check `Docs/Knowledge/*.md` files exist

**TypeScript errors:**
```powershell
npm install --save-dev @types/node typescript
npm run build
```

## Version

**1.0.0** - Initial release (October 2025)

## License

MIT - Part of SKYWORKS AI Suite

---

**For full documentation, see:** `../MCP_SERVER_GUIDE.md`
