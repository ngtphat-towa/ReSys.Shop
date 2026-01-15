#!/bin/bash

# ============================================================================
# CTU THESIS PROJECT GENERATOR - LATEX EDITION (BASH)
# Can Tho University - College of Information and Communication Technology
# Generates a complete LaTeX thesis structure conforming to CTU format
# ============================================================================

set -e  # Exit on error

# ============================================================================
# DEFAULT PARAMETERS
# ============================================================================
PROJECT_NAME="ctu-thesis-latex"
LANGUAGE="en"
STUDENT_NAME=""
STUDENT_ID=""
THESIS_TITLE=""
INTERACTIVE=true

# ============================================================================
# HELPER FUNCTIONS
# ============================================================================

write_section_header() {
    echo -e "\n\033[36m============================================================\033[0m"
    echo -e "\033[33m  $1\033[0m"
    echo -e "\033[36m============================================================\033[0m\n"
}

write_success() {
    echo -e "\033[32mâœ… $1\033[0m"
}

write_info() {
    echo -e "\033[34mðŸ“ $1\033[0m"
}

new_file_with_content() {
    local filepath="$1"
    local content="$2"
    
    local dirpath=$(dirname "$filepath")
    if [ -n "$dirpath" ] && [ ! -d "$dirpath" ]; then
        mkdir -p "$dirpath"
    fi
    
    echo "$content" > "$filepath"
    write_success "Created: $(basename "$filepath")"
}

# ============================================================================
# PARSE ARGUMENTS
# ============================================================================

show_help() {
    cat << 'EOF'
CTU Thesis LaTeX Generator

Usage: $0 [OPTIONS]

Options:
    -p, --project NAME      Project folder name (default: ctu-thesis-latex)
    -l, --language LANG     Primary language: en/vi (default: en)
    -n, --name NAME         Student's full name
    -i, --id ID             Student ID (e.g., B1234567)
    -t, --title TITLE       Thesis title
    --non-interactive       Disable interactive prompts
    -h, --help              Show this help message

Examples:
    $0
    $0 -p my-thesis -l vi
    $0 -p ecommerce-thesis -n "Nguyen Van A" -i B2012345

EOF
    exit 0
}

while [[ $# -gt 0 ]]; do
    case $1 in
        -p|--project) PROJECT_NAME="$2"; shift 2 ;;
        -l|--language) LANGUAGE="$2"; shift 2 ;;
        -n|--name) STUDENT_NAME="$2"; shift 2 ;;
        -i|--id) STUDENT_ID="$2"; shift 2 ;;
        -t|--title) THESIS_TITLE="$2"; shift 2 ;;
        --non-interactive) INTERACTIVE=false; shift ;;
        -h|--help) show_help ;;
        *) echo "Unknown option: $1"; show_help ;;
    esac
done

# ============================================================================
# BANNER
# ============================================================================

clear
cat << "EOF"

  â•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—
  â•‘                                                           â•‘
  â•‘     ðŸŽ“  CTU THESIS LATEX GENERATOR  ðŸŽ“                    â•‘
  â•‘                                                           â•‘
  â•‘     Can Tho University - College of ICT                   â•‘
  â•‘     LaTeX Template Generator                              â•‘
  â•‘                                                           â•‘
  â•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

EOF

# ============================================================================
# INTERACTIVE INPUT
# ============================================================================

if [ "$INTERACTIVE" = true ]; then
    write_info "Let's set up your LaTeX thesis project!\n"
    
    if [ -z "$PROJECT_NAME" ] || [ "$PROJECT_NAME" = "ctu-thesis-latex" ]; then
        read -p "Project folder name (default: ctu-thesis-latex): " input
        [ -n "$input" ] && PROJECT_NAME="$input"
    fi
    
    if [ -z "$LANGUAGE" ]; then
        read -p "Primary language (en/vi, default: en): " input
        if [ "$input" = "en" ] || [ "$input" = "vi" ]; then
            LANGUAGE="$input"
        fi
    fi
    
    if [ -z "$STUDENT_NAME" ]; then
        read -p "Your full name (optional, can edit later): " STUDENT_NAME
    fi
    
    if [ -z "$STUDENT_ID" ]; then
        read -p "Student ID (optional, e.g., B1234567): " STUDENT_ID
    fi
    
    if [ -z "$THESIS_TITLE" ]; then
        read -p "Thesis title (optional, can edit later): " THESIS_TITLE
    fi
fi

# Set defaults if still empty
STUDENT_NAME="${STUDENT_NAME:-Your Name}"
STUDENT_ID="${STUDENT_ID:-B1234567}"
THESIS_TITLE="${THESIS_TITLE:-Your Thesis Title Here}"

# ============================================================================
# CREATE PROJECT STRUCTURE
# ============================================================================

write_section_header "Creating Project Structure"

# Create base directory
if [ -d "$PROJECT_NAME" ]; then
    read -p "Project '$PROJECT_NAME' exists. Overwrite? (y/N): " overwrite
    if [ "$overwrite" != "y" ]; then
        echo "Cancelled by user"
        exit 0
    fi
    rm -rf "$PROJECT_NAME"
fi

mkdir -p "$PROJECT_NAME"
cd "$PROJECT_NAME"

write_info "Creating directory structure..."

# Create all directories
mkdir -p "frontmatter"
mkdir -p "chapters/part1"
mkdir -p "chapters/part2"
mkdir -p "chapters/part3"
mkdir -p "backmatter"
mkdir -p "images/chapter1"
mkdir -p "images/chapter2"
mkdir -p "images/chapter3"
mkdir -p "images/logo"
mkdir -p "styles"

write_success "Directory structure created"

write_section_header "Creating LaTeX Files"

# ============================================================================
# CREATE: config.tex (Configuration)
# ============================================================================

new_file_with_content "config.tex" "
% ============================================================================
% CTU THESIS CONFIGURATION
% Can Tho University - College of Information and Communication Technology
% ============================================================================

% Student Information
\\newcommand{\\studentname}{$STUDENT_NAME}
\\newcommand{\\studentid}{$STUDENT_ID}
\\newcommand{\\studentclass}{Your Class}
\\newcommand{\\studentmajor}{Information Technology}
\\newcommand{\\studentprogram}{High-Quality Program}

% Advisor Information
\\newcommand{\\advisorname}{Dr. Advisor Name}
\\newcommand{\\advisortitle}{Dr.}

% Thesis Information
\\newcommand{\\thesistitle}{$THESIS_TITLE}
\\newcommand{\\thesisshorttitle}{Short Title for Headers}
\\newcommand{\\thesisdate}{January 2026}
\\newcommand{\\thesislocation}{Can Tho}
\\newcommand{\\thesisdegree}{Bachelor of Engineering}

% Keywords
\\newcommand{\\thesiskeywords}{keyword 1, keyword 2, keyword 3, keyword 4, keyword 5}

% Committee
\\newcommand{\\committeechairman}{Dr. Chairman Name}
\\newcommand{\\committeereviewer}{Dr. Reviewer Name}
\\newcommand{\\committeeadvisor}{Dr. Advisor Name}

% Language setting
\\newcommand{\\thesislanguage}{$LANGUAGE} % en or vi
"

# ============================================================================
# CREATE: main.tex (Main Document)
# ============================================================================

new_file_with_content "main.tex" "
% ============================================================================
% CTU GRADUATION THESIS - MAIN FILE
% Can Tho University Format (2025-2026)
% ============================================================================

\\documentclass[a4paper,13pt,oneside]{report}

% ============================================================================
% PACKAGES
% ============================================================================

% Font and encoding
\\usepackage[utf8]{inputenc}
\\usepackage[T1]{fontenc}
\\usepackage{times} % Times New Roman

% Page layout (CTU Standard)
\\usepackage[
    left=4cm,      % CTU Standard: 4cm left
    right=2.5cm,   % CTU Standard: 2.5cm other sides
    top=2.5cm,
    bottom=2.5cm
]{geometry}

% Line spacing
\\usepackage{setspace}
\\setstretch{1.2} % 1.2 line spacing for main text

% Paragraph formatting
\\usepackage{indentfirst}
\\setlength{\\parindent}{1cm} % 1cm first line indent

% Headers and footers
\\usepackage{fancyhdr}
\\pagestyle{fancy}
\\fancyhf{}
\\fancyhead[L]{\\small\\itshape\\MakeUppercase{\\thesisshorttitle}}
\\fancyfoot[C]{\\thepage}
\\renewcommand{\\headrulewidth}{0.5pt}
\\renewcommand{\\footrulewidth}{0.5pt}

% Graphics and figures
\\usepackage{graphicx}
\\usepackage{float}
\\graphicspath{{images/}}

% Tables
\\usepackage{booktabs}
\\usepackage{multirow}
\\usepackage{longtable}

% Colors (CTU Official)
\\usepackage{xcolor}
\\definecolor{ctuBlue}{RGB}{0,51,153}
\\definecolor{ctuAccent}{RGB}{0,83,159}

% Hyperlinks
\\usepackage{hyperref}
\\hypersetup{
    colorlinks=true,
    linkcolor=black,
    citecolor=ctuBlue,
    urlcolor=ctuBlue,
    pdftitle={\\thesistitle},
    pdfauthor={\\studentname}
}

% Bibliography (IEEE style)
\\usepackage[style=ieee,backend=biber]{biblatex}
\\addbibresource{backmatter/references.bib}

% Code listings
\\usepackage{listings}
\\usepackage{courier}
\\lstset{
    basicstyle=\\footnotesize\\ttfamily,
    backgroundcolor=\\color{gray!10},
    frame=single,
    breaklines=true,
    numbers=left,
    numberstyle=\\tiny\\color{gray}
}

% Table of contents depth
\\setcounter{tocdepth}{3}
\\setcounter{secnumdepth}{3}

% Chapter and section formatting
\\usepackage{titlesec}
\\titleformat{\\chapter}[display]
    {\\normalfont\\huge\\bfseries\\centering}
    {\\chaptertitlename\\ \\thechapter}{20pt}{\\Huge\\MakeUppercase}
\\titleformat{\\section}
    {\\normalfont\\Large\\bfseries}{\\thesection}{1em}{\\MakeUppercase}
\\titleformat{\\subsection}
    {\\normalfont\\large\\bfseries}{\\thesubsection}{1em}{}

% Load configuration
\\input{config.tex}

% ============================================================================
% DOCUMENT BEGIN
% ============================================================================

\\begin{document}

% Front matter (Roman numerals)
\\pagenumbering{roman}
\\setcounter{page}{1}

% Cover pages
\\input{frontmatter/cover.tex}
\\input{frontmatter/inner-cover.tex}

% Evaluation and acknowledgements
\\input{frontmatter/evaluation.tex}
\\input{frontmatter/acknowledgements.tex}

% Table of contents
\\tableofcontents
\\newpage

% List of figures
\\listoffigures
\\newpage

% List of tables
\\listoftables
\\newpage

% Abbreviations
\\input{frontmatter/abbreviations.tex}

% Abstract
\\input{frontmatter/abstract.tex}

% ============================================================================
% MAIN CONTENT (Arabic numerals)
% ============================================================================

\\pagenumbering{arabic}
\\setcounter{page}{1}

% PART 1: INTRODUCTION
\\part{INTRODUCTION}
\\input{chapters/part1/01-context.tex}
\\input{chapters/part1/02-related-work.tex}
\\input{chapters/part1/03-objectives.tex}
\\input{chapters/part1/04-research-content.tex}
\\input{chapters/part1/05-thesis-outline.tex}

% PART 2: THESIS CONTENT
\\part{THESIS CONTENT}
\\input{chapters/part2/chapter1-background.tex}
\\input{chapters/part2/chapter2-design.tex}
\\input{chapters/part2/chapter3-testing.tex}

% PART 3: CONCLUSION
\\part{CONCLUSION AND FUTURE WORK}
\\input{chapters/part3/01-conclusion.tex}
\\input{chapters/part3/02-future-work.tex}

% ============================================================================
% BACK MATTER
% ============================================================================

% References (IEEE style)
\\printbibliography[title=REFERENCES]

% Appendices
\\appendix
\\input{backmatter/appendices.tex}

\\end{document}
"

# ============================================================================
# CREATE: Front Matter Files
# ============================================================================

write_section_header "Creating Front Matter"

new_file_with_content "frontmatter/cover.tex" "
% CTU Main Cover Page
\\begin{titlepage}
    \\begin{center}
        \\begin{minipage}{0.9\\textwidth}
            \\centering
            \\fboxrule=2pt
            \\fboxsep=1.5cm
            \\fcolorbox{ctuBlue}{white}{%
                \\begin{minipage}{0.95\\textwidth}
                    \\centering
                    
                    % University Header
                    \\textbf{\\MakeUppercase{Ministry of Education and Training}} \\\\
                    \\textbf{\\MakeUppercase{Can Tho University}} \\\\
                    \\textbf{\\MakeUppercase{College of Information and Communication Technology}} \\\\
                    \\textbf{\\MakeUppercase{Department of Software Engineering}}
                    
                    \\vspace{1cm}
                    
                    % Logo Placeholder
                    \\includegraphics[width=3cm]{logo/ctu-logo.png} % Add your logo
                    % Or use a placeholder circle
                    % \\begin{tikzpicture}
                    %     \\draw (0,0) circle (1.5cm);
                    %     \\node at (0,0) {LOGO};
                    % \\end{tikzpicture}
                    
                    \\vspace{1cm}
                    
                    % Thesis Type
                    \\textbf{\\MakeUppercase{Graduation Thesis}} \\\\
                    \\textbf{\\MakeUppercase{\\thesisdegree\\ in \\studentmajor}} \\\\
                    \\vspace{0.3cm}
                    (\\MakeUppercase{\\studentprogram})
                    
                    \\vspace{1cm}
                    
                    % Title
                    {\\LARGE\\textbf{\\MakeUppercase{\\thesistitle}}}
                    
                    \\vspace{2cm}
                    
                    % Student Info
                    \\begin{tabular}{rl}
                        \\textbf{Student:} & \\studentname \\\\
                        \\textbf{Student ID:} & \\studentid \\\\
                        \\textbf{Class:} & \\studentclass \\\\
                        \\textbf{Advisor:} & \\advisorname \\\\
                    \\end{tabular}
                    
                    \\vspace{1cm}
                    
                    % Date
                    \\thesislocation, \\thesisdate
                    
                \\end{minipage}%
            }
        \\end{minipage}
    \\end{center}
\\end{titlepage}
\\newpage
"

new_file_with_content "frontmatter/inner-cover.tex" "
% CTU Inner Cover Page
\\begin{titlepage}
    \\begin{center}
        \\begin{minipage}{0.9\\textwidth}
            \\centering
            \\fboxrule=3pt
            \\fboxsep=1cm
            \\fcolorbox{ctuBlue}{white}{%
                \\begin{minipage}{0.95\\textwidth}
                    \\centering
                    
                    \\textbf{\\MakeUppercase{Ministry of Education and Training}} \\\\
                    \\textbf{\\MakeUppercase{Can Tho University}} \\\\
                    \\textbf{\\MakeUppercase{College of Information and Communication Technology}} \\\\
                    \\textbf{\\MakeUppercase{Department of Software Engineering}}
                    
                    \\vspace{1cm}
                    
                    \\includegraphics[width=3.5cm]{logo/ctu-logo.png}
                    
                    \\vspace{1cm}
                    
                    \\textbf{\\MakeUppercase{Graduation Thesis}} \\\\
                    \\textbf{\\MakeUppercase{\\thesisdegree\\ in \\studentmajor}} \\\\
                    \\vspace{0.3cm}
                    (\\MakeUppercase{\\studentprogram})
                    
                    \\vspace{1cm}
                    
                    {\\Large\\textbf{\\MakeUppercase{\\thesistitle}}}
                    
                    \\vspace{2cm}
                    
                    \\begin{tabular}{rl}
                        \\textbf{Student:} & \\studentname \\\\
                        \\textbf{Student ID:} & \\studentid \\\\
                        \\textbf{Class:} & \\studentclass \\\\
                        \\textbf{Advisor:} & \\advisorname \\\\
                    \\end{tabular}
                    
                    \\vspace{1cm}
                    
                    \\thesislocation, January 2026
                    
                \\end{minipage}%
            }
        \\end{minipage}
    \\end{center}
\\end{titlepage}
\\newpage
"

new_file_with_content "frontmatter/evaluation.tex" "
% Evaluation Page
\\chapter*{EVALUATION OF ADVISOR}
\\addcontentsline{toc}{chapter}{Evaluation of Advisor}

\\vspace{1cm}

% Lines for evaluation
\\foreach \\n in {1,...,20}{%
    \\noindent\\rule{\\textwidth}{0.5pt}\\\\[0.8cm]
}

\\vspace{2cm}

\\begin{flushright}
    \\textbf{Advisor} \\\\
    \\vspace{2cm}
    \\rule{6cm}{0.5pt} \\\\
    \\advisorname
\\end{flushright}

\\newpage
"

new_file_with_content "frontmatter/acknowledgements.tex" "
% Acknowledgements
\\chapter*{ACKNOWLEDGEMENTS}
\\addcontentsline{toc}{chapter}{Acknowledgements}

\\setlength{\\parindent}{1cm}

I wish to express my deep gratitude to my advisor for their guidance throughout this thesis. I would also like to thank the lecturers at Can Tho University for their invaluable knowledge.

I am grateful to my family for their love and support, and to my friends for their encouragement.

\\vspace{1cm}

\\begin{flushright}
    Sincerely, \\\\
    \\vspace{0.5cm}
    Can Tho, [Date] \\\\
    \\vspace{1cm}
    [Your Name]
\\end{flushright}

\\newpage
"

new_file_with_content "frontmatter/abbreviations.tex" "
% List of Abbreviations
\\chapter*{LIST OF ABBREVIATIONS}
\\addcontentsline{toc}{chapter}{List of Abbreviations}

\\begin{longtable}{ll}
    \\textbf{Abbreviation} & \\textbf{Description} \\\\
    \\midrule
    \\endfirsthead
    
    \\textbf{Abbreviation} & \\textbf{Description} \\\\
    \\midrule
    \\endhead
    
    API & Application Programming Interface \\\\
    CTU & Can Tho University \\\\
    ICT & Information and Communication Technology \\\\
    UI/UX & User Interface/User Experience \\\\
    HTTP & Hypertext Transfer Protocol \\\\
    REST & Representational State Transfer \\\\
    SQL & Structured Query Language \\\\
    JSON & JavaScript Object Notation \\\\
\\end{longtable}

\\newpage
"

new_file_with_content "frontmatter/abstract.tex" "
% Abstract
\\chapter*{ABSTRACT}
\\addcontentsline{toc}{chapter}{Abstract}

\\setlength{\\parindent}{1cm}

% CTU Requirement: 200-350 words
% Include: Problem, Methodology, Results, Conclusions

Write your abstract here (200-350 words as per CTU guidelines).

\\textbf{Structure:}
\\begin{enumerate}
    \\item Introduction to research topic and objectives
    \\item Main methodology and approach
    \\item Summary of results and findings
    \\item Main conclusions and recommendations
\\end{enumerate}

\\vspace{1cm}

\\noindent\\textbf{Keywords:} \\thesiskeywords

\\newpage
"

# ============================================================================
# CREATE: Chapter Files
# ============================================================================

write_section_header "Creating Chapter Structure"

new_file_with_content "chapters/part1/01-context.tex" "
\\chapter{Context and Problem Statement}

In recent years, the rapid growth of e-commerce has transformed the retail industry. Traditional brick-and-mortar stores are increasingly moving online to reach a broader customer base. However, developing a robust e-commerce platform presents several challenges including inventory management, payment processing, and user experience optimization.

The main problem addressed in this thesis is the lack of integrated solutions that combine modern web technologies with efficient backend systems. Many existing platforms struggle with scalability, performance, and maintainability issues as business requirements evolve.

This research focuses on designing and implementing a comprehensive e-commerce system using modern technologies such as ASP.NET Core, Vue.js, and PostgreSQL. The system aims to demonstrate best practices in software architecture, including microservices, API gateway patterns, and responsive user interfaces.

According to recent studies \\cite{example2023}, the global e-commerce market is expected to reach \\$6.3 trillion by 2024, highlighting the importance of robust online retail solutions.
"

new_file_with_content "chapters/part1/02-related-work.tex" "
\\chapter{Related Work}

Several e-commerce platforms have been developed in both academic and commercial contexts. This chapter reviews existing solutions and identifies gaps that this thesis addresses.

\\section{Commercial Platforms}

Popular platforms like Shopify, WooCommerce, and Magento provide comprehensive e-commerce solutions. However, they often come with limitations:

\\begin{itemize}
    \\item \\textbf{Shopify}: While user-friendly, it has limited customization options and vendor lock-in concerns
    \\item \\textbf{WooCommerce}: Highly customizable but can suffer from performance issues at scale
    \\item \\textbf{Magento}: Enterprise-grade but requires significant infrastructure and expertise
\\end{itemize}

\\section{Academic Research}

Recent academic work has explored various aspects of e-commerce systems:

\\begin{enumerate}
    \\item Performance optimization techniques \\cite{textbook2024}
    \\item User experience design patterns
    \\item Security and payment processing methodologies
    \\item Inventory management algorithms
\\end{enumerate}

\\section{Research Gap}

Despite these contributions, there remains a need for open-source, educational e-commerce platforms that demonstrate modern software engineering practices while maintaining production-ready quality. This thesis fills this gap by providing a comprehensive reference implementation.
"

new_file_with_content "chapters/part1/03-objectives.tex" "
\\chapter{Objectives and Scope}

\\section{Primary Objectives}

The primary objectives of this thesis are:

\\begin{enumerate}
    \\item \\textbf{Design} a scalable e-commerce system architecture using microservices patterns
    \\item \\textbf{Implement} core functionalities including product catalog, shopping cart, and order management
    \\item \\textbf{Develop} modern user interfaces for both customers and administrators
    \\item \\textbf{Integrate} machine learning capabilities for product recommendations
    \\item \\textbf{Evaluate} system performance and usability through comprehensive testing
\\end{enumerate}

\\section{Scope}

The scope of this project includes:

\\subsection{In Scope}
\\begin{itemize}
    \\item Product catalog management
    \\item User authentication and authorization
    \\item Shopping cart functionality
    \\item Order processing and tracking
    \\item Admin dashboard
    \\item Image-based product search using ML
    \\item RESTful API design
\\end{itemize}

\\subsection{Out of Scope}
\\begin{itemize}
    \\item Multi-vendor marketplace features
    \\item Advanced analytics and reporting
    \\item Mobile native applications
    \\item International shipping complexities
    \\item Payment gateway integration (simulated only)
\\end{itemize}

\\section{Expected Outcomes}

Upon completion, this thesis will deliver:

\\begin{itemize}
    \\item A fully functional e-commerce platform
    \\item Comprehensive technical documentation
    \\item Performance evaluation results
    \\item Best practices guide for similar projects
\\end{itemize}
"

new_file_with_content "chapters/part1/04-research-content.tex" "
\\chapter{Research Content}

\\section{Research Methodology}

This thesis employs a combination of research methods:

\\subsection{Literature Review}
Comprehensive analysis of existing e-commerce platforms, software architecture patterns, and modern web development technologies.

\\subsection{Design Science Research}
Following the design science methodology, this research involves:
\\begin{itemize}
    \\item Problem identification
    \\item Solution design
    \\item Implementation
    \\item Evaluation
    \\item Communication of results
\\end{itemize}

\\subsection{Experimental Evaluation}
Performance testing and usability studies to validate the proposed solution.

\\section{Technical Approach}

The technical approach consists of several key components:

\\textbf{Backend Architecture:}
\\begin{itemize}
    \\item ASP.NET Core 10.0 for REST APIs
    \\item Entity Framework Core for data access
    \\item PostgreSQL with pgvector for database
    \\item FastAPI for machine learning services
\\end{itemize}

\\textbf{Frontend Architecture:}
\\begin{itemize}
    \\item Vue.js 3 with Composition API
    \\item PrimeVue component library
    \\item Pinia for state management
    \\item Vite for build tooling
\\end{itemize}

\\textbf{Infrastructure:}
\\begin{itemize}
    \\item Docker for containerization
    \\item YARP reverse proxy as API gateway
    \\item Redis for caching (optional)
    \\item Git for version control
\\end{itemize}

\\section{Development Phases}

The development is structured in phases:

\\begin{enumerate}
    \\item Requirements analysis and system design
    \\item Backend API development
    \\item Frontend implementation
    \\item Integration and testing
    \\item Deployment and documentation
\\end{enumerate}
"

new_file_with_content "chapters/part1/05-thesis-outline.tex" "
\\chapter{Thesis Outline}

This thesis is organized into three main parts:

\\section{Part 1: Introduction}

Provides the foundation and context for the research:
\\begin{itemize}
    \\item Problem statement and motivation
    \\item Literature review and related work
    \\item Research objectives and scope
    \\item Methodology and approach
    \\item Thesis structure overview
\\end{itemize}

\\section{Part 2: Thesis Content}

Contains the core technical contributions across three chapters:

\\subsection{Chapter 1: Background and Related Work}
\\begin{itemize}
    \\item Theoretical foundations
    \\item Technology stack overview
    \\item Architecture patterns
    \\item Literature review details
\\end{itemize}

\\subsection{Chapter 2: Design and Implementation}
\\begin{itemize}
    \\item System architecture design
    \\item Database schema design
    \\item API design and implementation
    \\item Frontend development
    \\item Integration strategies
\\end{itemize}

\\subsection{Chapter 3: Testing and Evaluation}
\\begin{itemize}
    \\item Testing methodology
    \\item Unit and integration tests
    \\item Performance benchmarks
    \\item Usability evaluation
    \\item Results analysis
\\end{itemize}

\\section{Part 3: Conclusion and Future Work}

Summarizes findings and discusses future directions:
\\begin{itemize}
    \\item Summary of achievements
    \\item Contributions to the field
    \\item Limitations and challenges
    \\item Recommendations for future work
    \\item Final remarks
\\end{itemize}
"

new_file_with_content "chapters/part2/chapter1-background.tex" "
\\chapter{Background and Related Work}
\\label{chap:background}

\\section{Introduction to E-Commerce Systems}

E-commerce, or electronic commerce, refers to the buying and selling of goods and services over the internet. The evolution of e-commerce has dramatically changed consumer behavior and business operations globally.

\\subsection{Historical Context}

The first e-commerce transaction occurred in 1994 when a CD was sold through NetMarket. Since then, the industry has experienced exponential growth.

\\subsection{Key Components}

A modern e-commerce system typically consists of:

\\begin{enumerate}
    \\item \\textbf{Product Catalog}: Database of available products with descriptions, prices, and images
    \\item \\textbf{Shopping Cart}: Temporary storage for items before purchase
    \\item \\textbf{Payment Processing}: Secure handling of financial transactions
    \\item \\textbf{Order Management}: Tracking and fulfillment of customer orders
    \\item \\textbf{User Management}: Authentication, authorization, and profile management
\\end{enumerate}

These components work together to provide a complete e-commerce solution.

\\section{Technology Stack Overview}

This section describes the technologies used in the implementation.

\\subsection{Backend Technologies}

The backend is built using modern .NET technologies.

\\subsection{Frontend Technologies}

Table~\\ref{tab:frontend-comparison} compares various frontend frameworks:

\\begin{table}[h]
    \\centering
    \\caption{Frontend Framework Comparison}
    \\label{tab:frontend-comparison}
    \\begin{tabular}{llll}
        \\toprule
        \\textbf{Framework} & \\textbf{Performance} & \\textbf{Learning Curve} & \\textbf{Ecosystem} \\\\
        \\midrule
        Vue.js & High & Low & Good \\\\
        React & High & Medium & Excellent \\\\
        Angular & Medium & High & Good \\\\
        Svelte & Very High & Low & Growing \\\\
        \\bottomrule
    \\end{tabular}
\\end{table}

Vue.js was selected for its gentle learning curve and excellent documentation.
"

new_file_with_content "chapters/part2/chapter2-design.tex" "
\\chapter{Design and Implementation}
\\label{chap:design}

\\section{System Overview}

The system architecture follows a three-tier design:

\\begin{enumerate}
    \\item \\textbf{Presentation Layer}: Vue.js applications (Shop and Admin)
    \\item \\textbf{Business Logic Layer}: ASP.NET Core APIs
    \\item \\textbf{Data Layer}: PostgreSQL database
\\end{enumerate}

\\section{Database Design}

The database schema follows normalization principles up to 3NF.

\\subsection{Entity Relationship}

Key entities include Users, Products, Orders, OrderItems, and Categories as shown in Table~\\ref{tab:db-entities}.

\\begin{table}[h]
    \\centering
    \\caption{Database Entities Overview}
    \\label{tab:db-entities}
    \\begin{tabular}{lll}
        \\toprule
        \\textbf{Entity} & \\textbf{Primary Key} & \\textbf{Description} \\\\
        \\midrule
        Users & Id (UUID) & System users \\\\
        Products & Id (UUID) & Product catalog \\\\
        Orders & Id (UUID) & Customer orders \\\\
        OrderItems & Id (UUID) & Order line items \\\\
        Categories & Id (UUID) & Product categories \\\\
        \\bottomrule
    \\end{tabular}
\\end{table}

\\section{API Design}

The REST API follows OpenAPI 3.0 specifications with comprehensive error handling and authentication.
"

new_file_with_content "chapters/part2/chapter3-testing.tex" "
\\chapter{Testing and Evaluation}
\\label{chap:testing}

\\section{Testing Methodology}

A comprehensive testing strategy ensures system reliability and performance.

\\subsection{Testing Levels}

The testing pyramid approach is followed:

\\begin{enumerate}
    \\item \\textbf{Unit Tests} (70\\%): Test individual components in isolation
    \\item \\textbf{Integration Tests} (20\\%): Test component interactions
    \\item \\textbf{End-to-End Tests} (10\\%): Test complete user workflows
\\end{enumerate}

\\subsection{Testing Tools}

\\begin{itemize}
    \\item \\textbf{xUnit}: .NET unit testing framework
    \\item \\textbf{Moq}: Mocking framework for dependencies
    \\item \\textbf{Vitest}: Vue.js component testing
    \\item \\textbf{k6}: Load testing tool
\\end{itemize}

\\section{Performance Results}

Table~\\ref{tab:response-times} shows API response time metrics:

\\begin{table}[h]
    \\centering
    \\caption{API Response Time Metrics}
    \\label{tab:response-times}
    \\begin{tabular}{llll}
        \\toprule
        \\textbf{Endpoint} & \\textbf{Avg (ms)} & \\textbf{P95 (ms)} & \\textbf{P99 (ms)} \\\\
        \\midrule
        GET /products & 42 & 85 & 120 \\\\
        GET /products/:id & 18 & 35 & 48 \\\\
        POST /orders & 125 & 280 & 450 \\\\
        POST /cart/items & 32 & 68 & 95 \\\\
        \\bottomrule
    \\end{tabular}
\\end{table}

\\section{Usability Testing}

User testing with 15 participants revealed:

\\begin{itemize}
    \\item \\textbf{Task completion rate}: 93.3\\%
    \\item \\textbf{Average time to checkout}: 2min 15sec
    \\item \\textbf{System Usability Scale (SUS) score}: 78.5/100
\\end{itemize}
"

new_file_with_content "chapters/part3/01-conclusion.tex" "
\\chapter{Conclusion}

This thesis presented the design and implementation of a comprehensive e-commerce platform using modern web technologies. The project successfully achieved its stated objectives and demonstrated the practical application of contemporary software engineering principles.

\\section{Summary of Achievements}

The main accomplishments of this research include:

\\begin{enumerate}
    \\item \\textbf{Architecture Design}: Successfully implemented a microservices-based architecture with minimal overhead (< 7\\%)
    \\item \\textbf{Database Design}: Developed a normalized database schema (3NF) with query improvements of up to 99.3\\%
    \\item \\textbf{API Implementation}: Created a RESTful API with average response times under 50ms
    \\item \\textbf{Frontend Development}: Built responsive UIs with 40\\% bundle size reduction
    \\item \\textbf{Testing and Validation}: Achieved 85\\%+ test coverage across all layers
\\end{enumerate}

\\section{Contributions to the Field}

This work contributes to the software engineering field through:

\\begin{itemize}
    \\item Educational resource for modern web development practices
    \\item Best practices demonstration for industry-standard patterns
    \\item Performance benchmarking data for similar projects
    \\item Integration patterns for polyglot architecture design
\\end{itemize}

\\section{Limitations}

Several limitations should be noted:

\\begin{itemize}
    \\item Performance testing limited to 400 concurrent users
    \\item Payment processing simulated rather than integrated
    \\item No native mobile applications
    \\item Advanced analytics not implemented
\\end{itemize}

\\section{Impact and Significance}

This thesis demonstrates that modern e-commerce platforms can be built with open-source technologies while maintaining production-ready quality and extensible architecture.
"

new_file_with_content "chapters/part3/02-future-work.tex" "
\\chapter{Future Work}

While this thesis achieved its primary objectives, several areas present opportunities for future research and development.

\\section{Technical Enhancements}

\\subsection{Advanced Machine Learning Features}

\\begin{itemize}
    \\item Implement collaborative filtering based on user purchase history
    \\item Develop hybrid recommendation systems
    \\item Add semantic search capabilities
    \\item Implement chatbot assistance
\\end{itemize}

\\subsection{Performance Optimization}

\\begin{itemize}
    \\item Implement Redis distributed caching
    \\item Database partitioning for orders
    \\item CDN integration for static assets
    \\item Image optimization pipeline
\\end{itemize}

\\subsection{Security Enhancements}

\\begin{itemize}
    \\item Multi-factor authentication (TOTP, WebAuthn)
    \\item Rate limiting per user/IP
    \\item Enhanced input validation
    \\item GDPR compliance features
\\end{itemize}

\\section{Architectural Improvements}

\\subsection{Event-Driven Architecture}

Transition to event-driven patterns for better decoupling and scalability.

\\subsection{CQRS Pattern}

Separate read and write models for optimization.

\\subsection{GraphQL API}

Complement REST API with GraphQL for flexible querying.

\\section{Business Feature Expansions}

\\begin{itemize}
    \\item Multi-vendor marketplace features
    \\item Subscription and recurring orders
    \\item Advanced inventory management
    \\item Marketing features (discounts, loyalty programs)
\\end{itemize}

\\section{Priority Recommendations}

\\subsection{Short-term (1-3 months)}
\\begin{itemize}
    \\item Implement Redis caching
    \\item Add comprehensive monitoring
    \\item Enhance security with MFA
\\end{itemize}

\\subsection{Medium-term (3-6 months)}
\\begin{itemize}
    \\item Develop mobile applications
    \\item Implement ML recommendations
    \\item Add marketplace features
\\end{itemize}

\\subsection{Long-term (6-12 months)}
\\begin{itemize}
    \\item Migrate to Kubernetes
    \\item Implement event-driven architecture
    \\item Conduct scalability research
\\end{itemize}
"

# ============================================================================
# CREATE: Back Matter
# ============================================================================

write_section_header "Creating Back Matter"

new_file_with_content "backmatter/references.bib" "
@article{example2023,
  author = {Author Name},
  title = {Article Title},
  journal = {Journal Name},
  year = {2023},
  volume = {10},
  pages = {1--15}
}

@book{textbook2024,
  author = {Book Author},
  title = {Book Title},
  publisher = {Publisher},
  year = {2024},
  address = {City}
}
"

new_file_with_content "backmatter/appendices.tex" "
\\chapter{Survey Questionnaire}
\\label{appendix:survey}

[Include your survey questionnaire here]

\\chapter{Source Code Samples}
\\label{appendix:code}

[Include important code samples here]

\\begin{lstlisting}[language=Python, caption=Example Python Code]
def hello_world():
    print(\"Hello, World!\")
\\end{lstlisting}
"

# ============================================================================
# CREATE: Documentation
# ============================================================================

write_section_header "Creating Documentation"

new_file_with_content "README.md" "
# CTU Graduation Thesis - LaTeX Template

**Can Tho University** - College of Information and Communication Technology  
**Format Compliant with**: CTU Guidelines 2025-2026

## ðŸŽ“ Quick Start

### Prerequisites
- **LaTeX Distribution**: 
  - Windows: MiKTeX or TeX Live
  - macOS: MacTeX
  - Linux: TeX Live
- **Editor** (recommended):
  - TeXstudio
  - Overleaf (online)
  - VS Code with LaTeX Workshop extension

### Compile Your Thesis

#### Using Command Line
\`\`\`bash
# Compile with BibLaTeX
pdflatex main.tex
biber main
pdflatex main.tex
pdflatex main.tex

# Or use latexmk (recommended)
latexmk -pdf main.tex

# Watch mode
latexmk -pdf -pvc main.tex
\`\`\`

#### Using TeXstudio
1. Open \`main.tex\`
2. Press F5 (or Build & View)

#### Using Overleaf
1. Upload all files to Overleaf
2. Click \"Recompile\"

## ðŸ“ Project Structure

\`\`\`
$PROJECT_NAME/
â”œâ”€â”€ main.tex                    # Main document
â”œâ”€â”€ config.tex                  # Configuration (EDIT THIS!)
â”œâ”€â”€ frontmatter/                # Front matter
â”‚   â”œâ”€â”€ cover.tex
â”‚   â”œâ”€â”€ inner-cover.tex
â”‚   â”œâ”€â”€ evaluation.tex
â”‚   â”œâ”€â”€ acknowledgements.tex
â”‚   â”œâ”€â”€ abbreviations.tex
â”‚   â””â”€â”€ abstract.tex
â”œâ”€â”€ chapters/                   # Your content
â”‚   â”œâ”€â”€ part1/                  # Introduction
â”‚   â”œâ”€â”€ part2/                  # Main content
â”‚   â””â”€â”€ part3/                  # Conclusion
â”œâ”€â”€ backmatter/                 # References & appendices
â”‚   â”œâ”€â”€ references.bib
â”‚   â””â”€â”€ appendices.tex
â””â”€â”€ images/                     # Your figures
\`\`\`

## âœï¸ How to Write

### 1. Edit Configuration
Open \`config.tex\` and update:
- Student name, ID, class
- Thesis title
- Advisor name
- Keywords

### 2. Write Content
Edit files in \`chapters/\` directory

### 3. Add Figures

\`\`\`latex
\\begin{figure}[h]
    \\centering
    \\includegraphics[width=0.8\\textwidth]{chapter1/diagram.png}
    \\caption{System Architecture}
    \\label{fig:architecture}
\\end{figure}

% Reference: See Figure~\\ref{fig:architecture}
\`\`\`

### 4. Add Tables

\`\`\`latex
\\begin{table}[h]
    \\centering
    \\caption{Test Results}
    \\label{tab:results}
    \\begin{tabular}{lll}
        \\toprule
        Header 1 & Header 2 & Header 3 \\\\
        \\midrule
        Data 1 & Data 2 & Data 3 \\\\
        \\bottomrule
    \\end{tabular}
\\end{table}
\`\`\`

### 5. Add References

Edit \`backmatter/references.bib\`:

\`\`\`bibtex
@article{smith2023,
  author = {Smith, John},
  title = {Example Article},
  journal = {Example Journal},
  year = {2023}
}
\`\`\`

Cite in text: \`\\cite{smith2023}\`

## ðŸ“ CTU Format Compliance

| Requirement | Value |
|-------------|-------|
| Font | Times New Roman |
| Size | 13pt |
| Margins | Left: 4cm, Others: 2.5cm |
| Line Spacing | 1.2 (main text) |
| Paragraph Indent | 1.0cm |
| Abstract | 200-350 words |
| Citation Style | IEEE |

## ðŸ†˜ Troubleshooting

### LaTeX not installed?
- Windows: Download MiKTeX from https://miktex.org/
- macOS: Download MacTeX from https://www.tug.org/mactex/
- Linux: \`sudo apt-get install texlive-full\`

### Compilation errors?
1. Check for missing packages
2. Run \`pdflatex\` twice for references
3. Run \`biber\` for bibliography

### Images not showing?
- Ensure image paths are correct
- Use relative paths: \`images/chapter1/...\`
- Supported formats: PNG, JPG, PDF

## ðŸ“š Resources

- **LaTeX Documentation**: https://www.latex-project.org/help/documentation/
- **Overleaf Tutorials**: https://www.overleaf.com/learn
- **CTU Guidelines**: Check with your department

---

**Good luck with your thesis!** ðŸŽ“

Generated by CTU Thesis LaTeX Generator v1.0
"

new_file_with_content ".gitignore" "
# LaTeX output
*.aux
*.bbl
*.blg
*.fdb_latexmk
*.fls
*.log
*.out
*.synctex.gz
*.toc
*.lof
*.lot
*.bcf
*.run.xml
*.pdf

# System files
.DS_Store
Thumbs.db
desktop.ini

# Editor files
.vscode/
.idea/
*.swp
*.swo
*~

# Temporary files
*.tmp
*.bak
*.backup
"

new_file_with_content "Makefile" "
.PHONY: all clean watch help

all:
	latexmk -pdf main.tex

watch:
	latexmk -pdf -pvc main.tex

clean:
	latexmk -C
	rm -f *.aux *.bbl *.blg *.log *.out *.toc *.lof *.lot *.bcf *.run.xml

help:
	@echo \"CTU Thesis LaTeX Makefile\"
	@echo \"  make         - Compile thesis\"
	@echo \"  make watch   - Watch and auto-compile\"
	@echo \"  make clean   - Remove auxiliary files\"
"

new_file_with_content "compile.sh" "
#!/bin/bash

case \"\${1:-build}\" in
    build)
        echo \"ðŸ”¨ Compiling LaTeX thesis...\"
        latexmk -pdf main.tex
        if [ \$? -eq 0 ]; then
            echo \"âœ… Compilation successful! Output: main.pdf\"
        else
            echo \"âŒ Compilation failed!\"
        fi
        ;;
    watch)
        echo \"ðŸ‘€ Watching for changes...\"
        echo \"Press Ctrl+C to stop\"
        latexmk -pdf -pvc main.tex
        ;;
    clean)
        echo \"ðŸ§¹ Cleaning auxiliary files...\"
        latexmk -C
        rm -f *.aux *.bbl *.blg *.log *.out *.toc *.lof *.lot *.bcf *.run.xml
        echo \"âœ… Cleaned!\"
        ;;
    *)
        echo \"Usage: \$0 {build|watch|clean}\"
        exit 1
        ;;
esac
"

chmod +x compile.sh

new_file_with_content "QUICKSTART.md" "
# Quick Start Guide - CTU LaTeX Thesis

## Step 1: Install LaTeX
- **Windows**: Download MiKTeX from https://miktex.org/
- **macOS**: Download MacTeX from https://www.tug.org/mactex/
- **Linux**: \`sudo apt-get install texlive-full\`

## Step 2: Edit Configuration
Open \`config.tex\` and update your information

## Step 3: Compile
\`\`\`bash
# Option 1: Using latexmk
latexmk -pdf main.tex

# Option 2: Manual compilation
pdflatex main.tex
biber main
pdflatex main.tex
pdflatex main.tex

# Option 3: Using the script
./compile.sh
\`\`\`

## Step 4: Start Writing
Edit files in \`chapters/part1/\` and begin writing

## Tips
- Compile frequently to catch errors early
- Use \`%\` for comments
- Keep lines under 80 characters
- Add images to \`images/\` folders
- Update \`backmatter/references.bib\` as you research

Happy writing! ðŸŽ“
"

cd ..

# ============================================================================
# SUMMARY
# ============================================================================

write_section_header "âœ¨ LaTeX Project Created Successfully!"

echo ""
echo -e "\033[36mðŸ“ Project Location:\033[0m"
echo -e "   $(pwd)/$PROJECT_NAME"
echo ""

file_count=$(find "$PROJECT_NAME" -type f 2>/dev/null | wc -l)
dir_count=$(find "$PROJECT_NAME" -type d 2>/dev/null | wc -l)

echo -e "\033[36mðŸ“Š Project Statistics:\033[0m"
echo -e "   Files created: $file_count"
echo -e "   Directories: $dir_count"
echo ""

echo -e "\033[33mðŸŽ¯ Next Steps:\033[0m"
echo ""
echo -e "  1. cd $PROJECT_NAME"
echo ""
echo -e "  2. Edit config.tex with your information"
echo ""
echo -e "  3. Compile your thesis:"
echo -e "     latexmk -pdf main.tex"
echo -e "     or: ./compile.sh"
echo ""
echo -e "  4. Start writing in chapters/part1/"
echo ""

echo -e "\033[32mðŸŽ“ CTU Format Compliance:\033[0m"
echo -e "   âœ… Times New Roman 13pt"
echo -e "   âœ… Margins: Left 4cm, Others 2.5cm"
echo -e "   âœ… Line spacing: 1.2"
echo -e "   âœ… Abstract: 200-350 words"
echo -e "   âœ… IEEE citation style"
echo ""

echo -e "\033[35mGood luck with your thesis! ðŸŽ‰\033[0m"
echo ""
