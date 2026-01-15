<#
.SYNOPSIS
    CTU Thesis Project Generator - PowerShell Edition
.DESCRIPTION
    Creates a complete Typst thesis structure conforming to Can Tho University (CTU) format standards.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$ProjectName = "ctu-thesis",
    
    [Parameter()]
    [ValidateSet("en", "vi")]
    [string]$Language = "en",
    
    [Parameter()]
    [string]$StudentName,
    
    [Parameter()]
    [string]$StudentId,
    
    [Parameter()]
    [string]$ThesisTitle,
    
    [Parameter()]
    [switch]$NonInteractive
)

$ErrorActionPreference = "Stop"
$Interactive = -not $NonInteractive

# ============================================================================
# HELPER FUNCTIONS (Simplified for Parser Stability)
# ============================================================================

function Write-SectionHeader {
    param([string]$Title)
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Yellow
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function New-FileWithContent {
    param(
        [string]$Path,
        [string]$Content
    )
    $dir = Split-Path -Parent $Path
    if ($dir -and -not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    Set-Content -Path $Path -Value $Content -Encoding UTF8
    Write-Success "Created: $(Split-Path -Leaf $Path)"
}

# ============================================================================
# BANNER
# ============================================================================

Clear-Host
Write-Host "------------------------------------------------------------" -ForegroundColor Cyan
Write-Host "      CTU THESIS PROJECT GENERATOR (Typst v2.1)            " -ForegroundColor Yellow
Write-Host "------------------------------------------------------------" -ForegroundColor Cyan

# ============================================================================
# INTERACTIVE INPUT
# ============================================================================

if ($Interactive) {
    if ($ProjectName -eq "ctu-thesis") {
        $inputName = Read-Host "Project folder name (default: ctu-thesis)"
        if ($inputName) { $ProjectName = $inputName }
    }
    $langInput = Read-Host "Primary language (en/vi, default: en)"
    if ($langInput -and $langInput -in @("en", "vi")) { $Language = $langInput }
    if (-not $StudentName) { $StudentName = Read-Host "Your full name" }
    if (-not $StudentId) { $StudentId = Read-Host "Student ID" }
    if (-not $ThesisTitle) { $ThesisTitle = Read-Host "Thesis title" }
}

$StudentName = if ($StudentName) { $StudentName } else { "Your Name" }
$StudentId = if ($StudentId) { $StudentId } else { "B1234567" }
$ThesisTitle = if ($ThesisTitle) { $ThesisTitle } else { "Your Thesis Title Here" }

# ============================================================================
# CREATE PROJECT STRUCTURE
# ============================================================================

Write-SectionHeader "Creating Project Structure"

if (Test-Path $ProjectName) {
    if ($Interactive) {
        $overwrite = Read-Host "Project '$ProjectName' exists. Overwrite? (y/N)"
        if ($overwrite -ne "y") {
            Write-Host "Cancelled." -ForegroundColor Yellow
            return
        }
    }
    Remove-Item -Recurse -Force $ProjectName
}

New-Item -ItemType Directory -Path $ProjectName -Force | Out-Null
Push-Location $ProjectName

$directories = @(
    "template", "frontmatter", "backmatter", 
    "chapters/part1", "chapters/part2/chapter1", 
    "chapters/part2/chapter2", "chapters/part2/chapter3", 
    "chapters/part3", "images/logo"
)

foreach ($dir in $directories) {
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
}
Write-Success "Directory structure created"

# ============================================================================
# CREATE: info.typ
# ============================================================================

$infoTyp = @"
#let info = (
  en: (
    student: (name: "$StudentName", id: "$StudentId", class: "Your Class", major: "IT", program: "HQ"),
    advisor: (name: "Dr. Advisor", title: "Dr."),
    thesis: (title: "$ThesisTitle", short_title: "SHORT TITLE", date: "January 2026", location: "Can Tho", degree: "BACHELOR"),
    keywords: ("keyword1", "keyword2"),
    committee: (chairman: "Dr. A", reviewer: "Dr. B", advisor: "Dr. C"),
    abbreviations: (("API", "Application Programming Interface"), ("CTU", "Can Tho University")),
  ),
  vi: (
    student: (name: "$StudentName", id: "$StudentId", class: "Lớp", major: "CNTT", program: "CLC"),
    advisor: (name: "TS. GVHD", title: "TS."),
    thesis: (title: "$ThesisTitle", short_title: "TIÊU ĐỀ", date: "Tháng 01/2026", location: "Cần Thơ", degree: "KỸ SƯ"),
    keywords: ("từ khóa 1", "từ khóa 2"),
    committee: (chairman: "TS. A", reviewer: "TS. B", advisor: "TS. C"),
    abbreviations: (("API", "Giao diện API"), ("CTU", "ĐH Cần Thơ")),
  ),
)

#let settings = (
  primary_lang: "$Language",
  border_color: rgb(0, 51, 153),
  accent_color: rgb(0, 83, 159),
  format: (
    font: "Times New Roman",
    font_size: 13pt,
    line_spacing: 1.2,
    margins: (left: 4cm, right: 2.5cm, top: 2.5cm, bottom: 2.5cm),
    paragraph_indent: 1cm,
  ),
)
"@

New-FileWithContent -Path "info.typ" -Content $infoTyp

# ============================================================================
# CREATE: main.typ
# ============================================================================

$mainTyp = @'
#import "info.typ": *
#import "template/ctu-styles.typ": ctu-styles
#import "template/i18n.typ": term

#let lang = settings.primary_lang
#show: doc => ctu-styles(doc, lang: lang)

#set document(title: info.at(lang).thesis.title, author: info.at(lang).student.name)

#set page(numbering: "i")
#counter(page).update(1)

#import "frontmatter/cover.typ": cover-page
#cover-page(lang: lang)

#include "frontmatter/table-of-contents.typ"
#include "frontmatter/abstract.typ"

#set page(numbering: "1")
#counter(page).update(1)
#set heading(numbering: "1.1.1.1")

#let part-heading(body) = {
  pagebreak()
  v(2cm)
  heading(level: 1, numbering: none, outlined: true)[#body]
}

#part-heading[PART 1: INTRODUCTION]
#include "chapters/part1-introduction.typ"

#part-heading[PART 2: CONTENT]
#include "chapters/part2-content.typ"

#part-heading[PART 3: CONCLUSION]
#include "chapters/part3-conclusion.typ"

#pagebreak()
#bibliography("backmatter/bibliography.bib", style: "ieee")
'@

New-FileWithContent -Path "main.typ" -Content $mainTyp

# ============================================================================
# CREATE: Template Styles
# ============================================================================

$ctuStylesTyp = @'
#import "../info.typ": info, settings

#let ctu-styles(doc, lang: "en") = {
  set page(
    paper: "a4",
    margin: settings.format.margins,
    footer: context {
      if page.numbering != none {
        set align(center)
        text(size: 11pt)[#counter(page).display(page.numbering)]
      }
    }
  )
  set text(font: settings.format.font, size: settings.format.font_size, lang: lang)
  set par(justify: true, leading: 0.25cm, first-line-indent: settings.format.paragraph_indent)
  show heading.where(level: 1): it => {
    pagebreak(weak: true)
    set align(center)
    set text(size: 14pt, weight: "bold")
    upper(it)
    v(12pt)
  }
  doc
}
'@

New-FileWithContent -Path "template/ctu-styles.typ" -Content $ctuStylesTyp

$i18nTyp = @'
#let dict = (
  en: (toc: "TABLE OF CONTENTS", abstract: "ABSTRACT"),
  vi: (toc: "MỤC LỤC", abstract: "TÓM TẮT")
)
#let term(lang, key) = dict.at(lang).at(key)
'@

New-FileWithContent -Path "template/i18n.typ" -Content $i18nTyp

# ============================================================================
# CREATE: Frontmatter
# ============================================================================

$coverTyp = @'
#import "../info.typ": *
#let cover-page(lang: "en") = {
  let data = info.at(lang)
  page(numbering: none)[
    #set align(center)
    #rect(width: 100%, height: 100%, stroke: settings.border_color + 2pt, inset: 1cm)[
      #text(weight: "bold", size: 13pt)[CAN THO UNIVERSITY]
      #v(2fr)
      #text(weight: "bold", size: 16pt)[#upper(data.thesis.title)]
      #v(2fr)
      #grid(columns: (1fr, 1fr), align: (right, left), column-gutter: 1cm,
        [Student:], [#data.student.name],
        [ID:], [#data.student.id]
      )
      #v(1fr)
      #data.thesis.location, #data.thesis.date
    ]
  ]
}
'@

New-FileWithContent -Path "frontmatter/cover.typ" -Content $coverTyp

$tocTyp = @'
#heading(level: 1, numbering: none, outlined: false)[TABLE OF CONTENTS]
#outline(title: none, indent: auto, depth: 3)
'@

New-FileWithContent -Path "frontmatter/table-of-contents.typ" -Content $tocTyp

$abstractTyp = @'
#heading(level: 1, numbering: none)[ABSTRACT]
Write your abstract here...
'@

New-FileWithContent -Path "frontmatter/abstract.typ" -Content $abstractTyp

# ============================================================================
# CREATE: Chapters
# ============================================================================

$introTyp = @'
= Introduction
This is the introduction.
'@

New-FileWithContent -Path "chapters/part1-introduction.typ" -Content $introTyp

$contentTyp = @'
= Main Content
This is the main content.
'@

New-FileWithContent -Path "chapters/part2-content.typ" -Content $contentTyp

$conclusionTyp = @'
= Conclusion
This is the conclusion.
'@

New-FileWithContent -Path "chapters/part3-conclusion.typ" -Content $conclusionTyp

# ============================================================================
# CREATE: Backmatter
# ============================================================================

$bib = @'
@article{key, author={Author}, title={Title}, journal={Journal}, year={2026}}
'@

New-FileWithContent -Path "backmatter/bibliography.bib" -Content $bib

# ============================================================================
# CREATE: Build Script
# ============================================================================

$buildPs1 = @'
param([string]$Action = "build")
if ($Action -eq "build") {
    Write-Host "Building..."
    typst compile main.typ thesis.pdf
} elseif ($Action -eq "watch") {
    typst watch main.typ thesis.pdf
}
'@

New-FileWithContent -Path "build.ps1" -Content $buildPs1

Write-SectionHeader "Summary"
Write-Host "Project '$ProjectName' created successfully."
Pop-Location
