# mts2
Manufacturing Tracking System ver2

## Run Razor Pages app with Docker

### Build and run with Docker Compose

```bash
docker compose up --build -d
```

Open: <http://localhost:8080>

Stop:

```bash
docker compose down
```

### Build and run with Docker only

```bash
docker build -t mts2-web:local .
docker run --rm -p 8080:8080 --name mts2-web mts2-web:local
```

## Project layout

- `src/Mts2.Web`: ASP.NET Core Razor Pages app (`net8.0`)
- `Dockerfile`: multi-stage build and runtime image
- `docker-compose.yml`: local container orchestration
