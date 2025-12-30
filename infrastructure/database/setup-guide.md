# Manual PostgreSQL + pgvector Setup Guide

This guide covers how to install PostgreSQL and enable the `pgvector` extension from scratch on Windows and Linux.

## 1. Install PostgreSQL

### Windows
1. Download the installer from [postgresql.org/download/windows](https://www.postgresql.org/download/windows/).
2. Run the installer.
3. **Important**: Note down the password you set for the `postgres` user (e.g., `password`).
4. Keep the port as `5432`.
5. Finish the installation.

### Linux (Ubuntu/Debian)
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql.service
```

## 2. Install pgvector Extension

### Windows
1. You need the Microsoft Visual C++ Redistributable.
2. Go to the [pgvector release page](https://github.com/pgvector/pgvector/releases).
3. Download the zip file matching your Postgres version (e.g., `pgvector_0.5.1_pg16.zip`).
4. Extract the contents.
5. Copy files to your Postgres installation directory (usually `C:\Program Files\PostgreSQL\16`):
   - `lib/vector.dll` -> `C:\Program Files\PostgreSQL\16\lib\`
   - `share/extension/vector*` -> `C:\Program Files\PostgreSQL\16\share\extension\`

### Linux
You need to compile it or install from packages if available.
```bash
# Install build dependencies
sudo apt install postgresql-server-dev-all build-essential git

# Clone and build
git clone --branch v0.5.1 https://github.com/pgvector/pgvector.git
cd pgvector
make
sudo make install
```

## 3. Enable Extension in Database

1. Open a tool like `pgAdmin` or use the command line `psql`.
2. Connect to your database (`shopdb`).
   - If `shopdb` doesn't exist: `CREATE DATABASE shopdb;`
3. Run the following SQL command:

```sql
CREATE EXTENSION vector;
```

4. Verify installation:
```sql
SELECT * FROM pg_extension WHERE extname = 'vector';
```

## 4. Connection String

Update your `appsettings.Development.json` with your credentials:
```json
"ConnectionStrings": {
  "shopdb": "Host=localhost;Port=5432;Database=shopdb;Username=postgres;Password=your_password"
}
```
