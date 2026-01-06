# Papercut SMTP Setup Guide

This guide explains how to run Papercut, a local SMTP server with a web UI, for testing email notifications in ReSys.Shop.

## Option 1: .NET Aspire (Recommended)

When running the project via .NET Aspire (`ReSys.AppHost`), Papercut is automatically started as a container using the `changemakerstudiosus/papercut-smtp` image.
- **SMTP Port**: Auto-configured (Container port `2525`) and injected into the API service.
- **Web UI**: Click the "web" endpoint link in the Aspire Dashboard next to the "papercut" resource (maps to container port `8080`, host port `37408`).

## Option 2: Manual Docker

If you are running services manually or want a persistent mail server outside of Aspire sessions.

1. Navigate to this directory:
   ```powershell
   cd infrastructure/mail
   ```

2. Start the container:
   ```powershell
   docker-compose -f docker-compose.mail.yml up -d
   ```

3. Access the Web UI:
   - URL: [http://localhost:37408](http://localhost:37408)

4. SMTP Configuration for API:
   - Host: `localhost`
   - Port: `2525`

## Option 3: Desktop App (Windows)

1. Download the latest release from [Papercut SMTP Releases](https://github.com/ChangemakerStudios/Papercut-SMTP/releases).
2. Install and run `Papercut.Service.exe` and `Papercut.UI.exe`.
3. It will listen on port `25` (SMTP) and `37408` (HTTP) by default.
4. Update your `appsettings.Development.json` if ports differ.