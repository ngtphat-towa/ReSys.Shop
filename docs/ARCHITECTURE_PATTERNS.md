# ReSys.Shop: ML Integration Architecture Patterns

This document serves as a reference for the architectural decisions and future scaling strategies for integrating the .NET API with the Python ML Service.

## 1. Executive Summary

The current architecture follows the **API Orchestrator (BFF)** pattern. This was chosen for its maintainability, security, and traceability, which are critical for a thesis and production-ready microservices.

---

## 2. Patterns Overview

### A. API Orchestrator (Current)
*   **Flow:** Frontend → Gateway → API → ML Service → API → DB.
*   **Role:** .NET API acts as the "Brain"; Python is a stateless tool.
*   **Best For:** Real-time searches, high traceability, and security.

### B. Shared Database
*   **Flow:** Python ↔ PostgreSQL ↔ .NET.
*   **Role:** Both services share a data layer.
*   **Best For:** Simple background indexing where performance outweighs security concerns.

### C. Event-Driven (Async)
*   **Flow:** API → Message Broker (RabbitMQ) → Python → API/DB.
*   **Role:** Fully decoupled services communicating via events.
*   **Best For:** Bulk processing, high resilience, and non-blocking tasks.

### D. Vector DB Specialist
*   **Flow:** .NET/Python → Qdrant/Milvus.
*   **Role:** Moving vector math to a dedicated engine.
*   **Best For:** Extreme scale (millions of vectors) and complex similarity filtering.

### E. gRPC Sidecar
*   **Flow:** API (gRPC Client) → Python (gRPC Server).
*   **Role:** Binary-based, high-performance communication.
*   **Best For:** Ultra-low latency requirements and strictly typed contracts.

---

## 3. Comparison Matrix

| Feature | Orchestrator | Shared DB | Event-Driven | Vector DB | gRPC |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **Latency** | Medium | Low | High | Very Low | Very Low |
| **Coupling** | Loose | High | Very Loose | Moderate | Moderate |
| **Scalability** | Moderate | Hard | Very High | Extreme | High |
| **Complexity** | Simple | Simple | Complex | High | Moderate |
| **Security** | Centralized | Fragmented | Distributed | Moderate | Centralized |

---

## 4. Why API Orchestrator is the Choice?

1.  **Security:** .NET manages all Identity, JWT, and DB access. Python remains isolated.
2.  **Observability:** Aspire/OpenTelemetry provides a single trace from Frontend to DB.
3.  **Maintainability:** Changes in the PostgreSQL schema only affect the .NET layer.
4.  **Stateless Python:** The ML service only cares about math (Embeddings), making it easy to replace or scale.

---

## 5. Future Roadmap (Hybrid Approach)

To scale the system for a professional level:
1.  **Real-Time Search:** Continue using **Orchestrator** with **gRPC** for better speed.
2.  **Product Indexing:** Implement **Event-Driven** logic. When a product is updated, the system emits an event for Python to process in the background.
3.  **Massive Scale:** Migrate from `pgvector` to a **Vector DB** (like Qdrant) once the catalog exceeds 1 million items.

---
*Documented on: December 31, 2025*
