# CTU Thesis Project Generators

This directory contains the automation suite for generating Typst and LaTeX thesis projects conforming to the Can Tho University (CTU) College of ICT guidelines for the 2025-2026 academic year.

---

## 🛠️ 1. Available Generators

### A. Typst Generator (PowerShell - Windows)
**Script**: `Generate-CtUThesis.ps1`
- **Focus**: Modern, fast compilation using the Typst typesetting system.
- **Features**: Includes a bilingual `info.typ` configuration and a pre-configured `build.ps1`.

### B. Typst Generator (Bash - Linux/macOS/WSL)
**Script**: `generate-ctu-thesis-typst.sh`
- **Focus**: Cross-platform parity with the PowerShell version.

### C. LaTeX Generator (Bash - Standard)
**Script**: `generate-ctu-thesis-latex.sh`
- **Focus**: Traditional academic workflow using `pdflatex` and `biblatex`.

---

## ⚙️ 2. Script Arguments (PowerShell Version)

The `Generate-CtUThesis.ps1` script supports several flags for automation:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `-ProjectName` | String | `ctu-thesis` | The name of the output directory. |
| `-Language` | `en/vi` | `en` | Sets the primary language for the cover and headings. |
| `-StudentName` | String | "Your Name" | Pre-fills the student metadata. |
| `-StudentId` | String | "B123..." | Pre-fills the student ID. |
| `-ThesisTitle` | String | "..." | Pre-fills the document title. |
| `-NonInteractive` | Switch | Off | Skips all prompts and uses defaults/arguments. |

**Example Command:**
```powershell
.\Generate-CtUThesis.ps1 -ProjectName "my-ecommerce-thesis" -Language "vi" -NonInteractive
```

---

## 🚦 3. Common Workflow

1. **Generation**: Run the script to create your project folder.
2. **Configuration**: Open `info.typ` (Typst) or `config.tex` (LaTeX) and update your personal details.
3. **Drafting**: Write your content in the `chapters/` sub-directories.
4. **Compilation**:
   - **Typst**: `.\build.ps1` or `typst compile main.typ`
   - **LaTeX**: Use `latexmk -pdf main.tex` or the provided `compile.sh`.

---

## 🕳️ 4. Pitfalls & Known Issues

### 1. PowerShell Parser "Phantom Braces"
**Issue**: The script would fail with `MissingEndCurlyBrace` even if braces were balanced.
**Cause**: High-Unicode characters (emojis like 🎓) in here-strings can sometimes confuse the PowerShell script parser depending on the host encoding.
**Fix**: The generator was rewritten to use standard ASCII markers for console feedback, ensuring stability across all Windows terminals.

### 2. Nested Here-String Boundaries
**Issue**: Typst code blocks (which use `#` and `@`) inside PowerShell strings can lead to incorrect termination.
**Fix**: Explicit boundaries and clean termination markers (`'@`) are used to prevent Typst syntax from "leaking" into the PowerShell logic.

### 3. Font Requirements
**Issue**: Compilation fails with "Font not found".
**Cause**: CTU standards strictly require **Times New Roman**.
**Fix**: Ensure Times New Roman is installed on your OS. For Linux/WSL, you may need to install the `ttf-mscorefonts-installer` package.

---

## 📋 5. Prerequisites

- **For Typst**: `winget install typst` (Windows) or `brew install typst` (macOS).
- **For LaTeX**: TeX Live or MiKTeX.
- **For PDF Viewing**: A PDF viewer that supports auto-refresh (like SumatraPDF or VS Code's extension).