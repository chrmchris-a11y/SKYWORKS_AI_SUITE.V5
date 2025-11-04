# 🌍 Internationalization (i18n) Infrastructure

## Overview
SKYWORKS AI SUITE V5 supports **13 languages** with automatic culture detection from HTTP headers.

## Supported Languages

| Code | Language | Native Name |
|------|----------|-------------|
| `en` | English | English (default) |
| `el` | Greek | Ελληνικά |
| `de` | German | Deutsch |
| `fr` | French | Français |
| `es` | Spanish | Español |
| `it` | Italian | Italiano |
| `ru` | Russian | Русский |
| `zh` | Chinese | 中文 |
| `pl` | Polish | Polski |
| `hr` | Croatian | Hrvatski |
| `sl` | Slovenian | Slovenščina |
| `uk` | Ukrainian | Українська |
| `cs` | Czech | Čeština |

## Backend (.NET)

### Architecture
- **Resource Files**: `.resx` files in `Backend/src/Skyworks.Api/Resources/`
  - `SharedResources.resx` - Default (English)
  - `SharedResources.el.resx` - Greek translations
  - Additional language files follow pattern: `SharedResources.{culture}.resx`

- **IStringLocalizer**: Dependency-injected in controllers
  ```csharp
  private readonly IStringLocalizer<Resources.SharedResources> _localizer;
  
  // Usage
  var message = _localizer["RequestBodyRequired"].Value;
  ```

### Culture Detection
1. **Accept-Language Header** (primary)
   - Client sends: `Accept-Language: el`
   - API responds with Greek strings
   
2. **Default Fallback**: `en` (English)

### Adding New Translations
1. Create new `.resx` file: `SharedResources.{culture}.resx`
2. Copy structure from `SharedResources.resx`
3. Translate `<value>` elements (keep `<data name>` identical)
4. Rebuild project

## Frontend (HTML/JavaScript)

### Status
- **Batch 2**: Frontend JSON i18n loader (pending)
- **Batch 3**: Greek Mission Planner UI (pending)

### Planned Architecture
```javascript
// Load translations
const i18n = await fetch('/app/i18n/en.json').then(r => r.json());

// Usage
document.getElementById('title').textContent = i18n.missionPlanner.title;
```

## Testing Localization

### cURL Example (Greek)
```bash
curl -H "Accept-Language: el" http://localhost:5000/api/sora/info
```

**Expected Response** (Greek):
```json
{
  "name": "Ολοκληρωμένη Αξιολόγηση SORA",
  "description": "Ολοκληρωμένη Αξιολόγηση Κινδύνου Ειδικών Λειτουργιών σύμφωνα με JARUS SORA 2.0 και 2.5",
  ...
}
```

### cURL Example (Default - English)
```bash
curl http://localhost:5000/api/sora/info
```

**Expected Response** (English):
```json
{
  "name": "SORA Complete Assessment Pipeline",
  "description": "End-to-end Specific Operations Risk Assessment per JARUS SORA 2.0 and 2.5",
  ...
}
```

## Implementation Timeline

- [x] **Batch 1** (Backend): Resource files, middleware, controller integration (~15-20 min)
- [ ] **Batch 2** (Frontend): JSON files, loader, language switcher (~20-25 min)
- [ ] **Batch 3** (Sample): Full Greek translations for Mission Planner (~15-20 min)
- [ ] **Batch 4** (PDF): QuestPDF multi-language font support (~15-20 min)
- [ ] **Phase 8** (Future): Mass translation to all 13 languages

## Phase 8: Mass Translation Strategy

### Scope
- 1,000+ strings across:
  - API messages (200 strings)
  - Frontend UI (400 strings)
  - SORA domain terms (300 strings)
  - Validation messages (100 strings)

### Approach
1. **Professional Translation Service** (Recommended)
   - EASA/aviation domain expertise required
   - Quality assurance for regulatory terms

2. **AI-Assisted + Human Review** (Alternative)
   - GPT-4 for initial translation
   - Native speaker review for technical accuracy
   - SORA/JARUS term consistency check

### Effort Estimate
- Per language: ~4-6 hours (translation + review)
- 12 languages × 5 hours = **60 hours** (2 weeks with 1 translator)
- Parallel: 4 translators = **3-4 days**

## Notes
- **SORA Terms**: Keep official JARUS terminology in English (e.g., "SAIL", "OSO", "TMPR") with parenthetical translations
- **PDF Reports**: Font support for Cyrillic/CJK required (Noto Sans family)
- **Testing**: Add culture-specific integration tests per controller
