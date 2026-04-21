# UI Bootstrap-Like Review

Scope reviewed:
- `MTS.RazorStarter` Razor Pages UI (active themed app)
- `src/Mts2.Web` starter UI shell

## Findings

### Looks like default Bootstrap conventions

1. **Generic utility/component naming in legacy pages**
   - `Engineering/Index.cshtml` and `Planning/Index.cshtml` still use `.card` containers, and `Engineering/Index.cshtml` uses `.btn`.
   - These names are strongly associated with Bootstrap defaults even though they are currently custom-styled. 

2. **Global selectors map generic HTML controls to framework-like defaults**
   - In `MTS.RazorStarter/wwwroot/css/mts-theme.css`, the selectors `input, select, textarea` apply shared control styles globally.
   - This creates a baseline look similar to framework defaults where all controls receive the same treatment.

3. **Legacy site stylesheet keeps Bootstrap-like tokens**
   - In `MTS.RazorStarter/wwwroot/css/site.css`, classes like `.card`, `.table`, `.badge`, `.alert`, and `.form-row` remain.
   - Even if unused in newer pages, these class names read as Bootstrap-ish and can make UI output feel framework-default.

4. **`src/Mts2.Web` shell uses Bootstrap-adjacent naming**
   - Layout still uses `.navbar` and `.container` class names.
   - These class names are canonical Bootstrap patterns and can be interpreted as default Bootstrap output.

## Suggested minimal cleanup (if desired)

- Replace remaining `.btn` in `Engineering/Index.cshtml` with `.mts-btn .mts-btn-primary`.
- Replace `.card` usage on older pages with `.part-card` + `.mts-panel`.
- Keep semantic wrappers but rename `.navbar`/`.container` in `src/Mts2.Web` to app-specific names (e.g., `.app-nav`, `.app-container`).
- Optionally deprecate/remove legacy `site.css` selectors in `MTS.RazorStarter` after confirming no references.
