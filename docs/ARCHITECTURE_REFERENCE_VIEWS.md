# Architecture Reference & External Perspectives

This document contains detailed technical explanations and external validations of the ReSys.Shop architecture patterns to provide context for junior developers and academic review.

---

## 1. gRPC: Junior Developer Perspective

### What is gRPC?
gRPC is a high-performance remote procedure call (RPC) framework. Instead of sending JSON over HTTP like REST, a client calls a method on a remote service as if it were a local method.

**Key Idea:** “Call functions on another service over the network, strongly typed and fast.”

### Core Building Blocks
*   **Protocol Buffers (Protobuf):** Language-neutral contract definition. It compiles to C# classes and is significantly smaller and faster than JSON.
*   **HTTP/2:** Supports multiplexing (many calls over one connection) and binary framing.
*   **Service Definition:** You define services and methods in a `.proto` file, and code is generated for both server and client.

### REST vs. gRPC
| Feature | REST | gRPC |
| :--- | :--- | :--- |
| **Model** | Resources (URL) | Methods (Functions) |
| **Payload** | JSON (Text) | Protobuf (Binary) |
| **Contract** | Flexible | Strict (.proto) |
| **Speed** | Slower | Faster |

---

## 2. Architectural Pattern Validation (Expert View)

Below is a validation of our "API Orchestrator" decision vs other industry patterns.

### A. The "System of Record" Principle
*   **Claim:** ".NET Core layer owns data validation, persistence, and authorization."
*   **Expert Verdict:** Correct. Python should be a **Stateless Compute Engine**.
*   **Why:** This aligns with Domain-Driven Design (DDD). Python "computes" (generates embeddings), but .NET "decides" (saves to DB).

### B. Shared Database Risk Assessment
*   **Claim:** "Shared Database is a security nightmare."
*   **Refined View:** It is technically fast but **architecturally dangerous**.
*   **Risk:** You lose transactional boundaries and C# domain validation rules. If Python writes directly to Postgres, you risk data corruption and bypass audit trails.

### C. gRPC as an Optimization, Not a Requirement
*   **Claim:** "gRPC replaces REST."
*   **Refined View:** They are complementary. gRPC is a **transport optimization**.
*   **Verdict:** Start with REST for ease of debugging and OpenTelemetry tracing. Move to gRPC only when high-frequency internal calls create a performance bottleneck.

---

## 3. pgvector: Strategic Positioning

### Why it is the "Right" Choice for Mid-Scale
*   **Consistency:** Avoids the "Dual Write" problem where a SQL DB and a Vector DB get out of sync.
*   **Hybrid Queries:** Allows combining traditional business filters (e.g., `WHERE Category = 'Shoes'`) with vector search in one atomic operation.
*   **Scalability Path:** pgvector is effective up to ~3 million vectors. Transitioning to a dedicated Vector DB (like Qdrant) is a future migration step, not an initial requirement.

### Wording for Thesis Justification
> “The proposed architecture prioritizes correctness and domain ownership over premature optimization. Performance enhancements like gRPC and event-driven pipelines are treated as evolutionary steps rather than initial requirements.”

---
*Reference Context: Derived from architectural reviews on Dec 31, 2025*
