# Travel App: Development Environment Setup

This guide takes you from an empty laptop to a running copy of the travel app: an Angular web app, a .NET API and a PostgreSQL/PostGIS database, all started with one command. Follow it top to bottom the first time. It takes about 30–45 minutes, most of which is waiting for downloads.

**Who reads what**

- **Part A** is done **once, by the repository owner**. It creates the GitHub repository and the first database migration.
- **Part B** is done **by every developer**, including the owner, on their own machine.
- **Parts C–E** are the daily workflow, troubleshooting, and a command cheat sheet. Keep them open while you work.

## How the environment works

Docker runs three containers on your machine, defined in `docker-compose.yml`:

```
  Your browser
       |
       |  http://localhost:4200
       v
+----------------+   /api/* calls   +----------------+   SQL    +--------------------+
|  web           | ---------------> |  api           | -------> |  db                |
|  Angular       |   (dev proxy)    |  .NET 10       |          |  PostgreSQL 17     |
|  ng serve      |                  |  dotnet watch  |          |  PostGIS 3.5       |
|  port 4200     |                  |  port 8080     |          |  port 5432         |
+----------------+                  +----------------+          +--------------------+
   ./frontend is                       ./backend is               data is kept in the
   mounted inside                      mounted inside             "pgdata" Docker volume
```

Three facts explain almost everything else in this guide:

1. **Your source code stays on your laptop.** The `frontend` and `backend` folders are mounted into the containers. When you save a file in your editor, the container sees the change immediately, and Angular and .NET reload automatically.
2. **Everyone has their own database.** Nobody shares a database during development. You can break, reset and experiment freely. Changes to the database *structure* reach other developers through migration files committed to git, never by editing someone else's database.
3. **The only things you install are Docker and Git.** The .NET SDK, Node.js, Angular CLI and PostgreSQL all run inside containers, in exactly the same versions for everyone.

**Repository layout**

```
travel-app/
├── docker-compose.yml        the three containers and how they connect
├── .env.example              settings template; you copy it to .env
├── db/init/                  SQL that runs once when your database is first created
├── scripts/                  helper scripts: migrations, database reset, psql
├── backend/
│   ├── TravelApp.slnx        solution file (open this in Rider / Visual Studio)
│   └── TravelApp.Api/
│       ├── Program.cs        API startup and configuration
│       ├── Domain/           entities (Place)
│       ├── Data/             DbContext, migrations, seed data
│       └── Endpoints/        HTTP endpoints (/api/places, /api/health)
├── frontend/                 Angular workspace (src/app is where you work)
└── .github/workflows/ci.yml  automatic build on every pull request
```

# Part A: One-time setup (repository owner only)

Complete Part B, steps B1–B3, on your own machine first, then come back here. Everyone else skips to Part B once the owner shares the repository.

## A1. Put the starter into a GitHub repository

1. On GitHub, create a new **private** repository named `travel-app`. Do not add a README, .gitignore or license; the starter already has them.
2. Unzip the starter. On Windows, unzip it **inside WSL** (for example to `~/code/travel-app`), for the reason explained in step B1.
3. In a terminal inside the `travel-app` folder, run:

```bash
git init -b main
git add .
git commit -m "Initial project setup: Angular, .NET 10, PostGIS, Docker Compose"
git remote add origin https://github.com/<your-account>/travel-app.git
git push -u origin main
```

Check that `.env` is **not** in the list of committed files (it shouldn't exist yet, and it is ignored by `.gitignore` anyway), and that `frontend/package-lock.json` **is** committed. The lock file guarantees everyone installs identical npm packages.

## A2. Start the environment for the first time

```bash
cp .env.example .env
docker compose up --build
```

The first run downloads the base images and all packages, which takes 5–10 minutes. Keep the terminal open and watch the logs. At the end you will see a warning from the API:

```
No EF Core migrations exist yet, so the database has no tables.
Create the first one with: ./scripts/add-migration.sh InitialCreate
```

This is expected. The code defines a `Place` entity, but the database doesn't have a table for it yet. Creating that table is the next step.

## A3. Create the first database migration

A **migration** is a C# file that describes a change to the database structure, such as "create the Places table with these columns". EF Core generates it by comparing your C# entities with the previous migration. Migrations are committed to git, so every developer's database ends up with the same structure.

Open a **second** terminal in the same folder (leave the first one running) and run:

```bash
./scripts/add-migration.sh InitialCreate
docker compose restart api
```

The script runs the EF Core tool inside a temporary API container, so you don't need .NET installed locally. It creates three files in `backend/TravelApp.Api/Data/Migrations/`. The restart makes the API apply the migration and insert 12 sample places across Serbia.

Verify it worked:

1. Open http://localhost:8080/api/health. You should see `{"api":"ok","database":"ok","postgis":"3.5..."}`.
2. Open http://localhost:4200. The page should list places near Novi Sad, with distances.

Commit the migration:

```bash
git add backend/TravelApp.Api/Data/Migrations
git commit -m "Add initial database migration"
git push
```

## A4. Protect the main branch and invite the team

1. **Invite the team:** in the repository go to *Settings → Collaborators* and add the other four developers.
2. **Protect `main`:** go to *Settings → Branches → Add branch ruleset* (or *Add rule*), select `main`, and enable:
   - *Require a pull request before merging*, with 1 approval.
   - *Require status checks to pass*, then select **Build API** and **Build web app**. These checks appear in the list after the CI workflow has run at least once.
3. **Share the repository URL** and this tutorial with the team.

From now on nobody pushes directly to `main`. Every change goes through a pull request, which is built automatically by GitHub Actions and reviewed by a teammate.

# Part B: Setup for every developer

## B1. Install the prerequisites

**Windows 10/11 (most of the team)**

1. **Enable WSL2.** Open PowerShell *as Administrator*, run `wsl --install`, and restart the computer when asked. This installs Ubuntu. On first start Ubuntu asks you to create a Linux username and password; choose anything and remember the password.
2. **Install Docker Desktop** from docker.com. During installation keep *Use WSL 2 instead of Hyper-V* selected. After installation open Docker Desktop, go to *Settings → Resources → WSL integration*, and enable integration for **Ubuntu**.
3. **Give Docker enough memory.** 8 GB RAM for Docker is comfortable; 4 GB is the minimum. With WSL2 this is controlled by Windows automatically, which is fine for most laptops.
4. **Install Git inside Ubuntu.** Open the *Ubuntu* app and run:

```bash
sudo apt update && sudo apt install -y git
git config --global user.name "Your Name"
git config --global user.email "you@example.com"
```

**Why WSL matters:** keep the project inside the Linux file system (for example `~/code/travel-app`), **not** under `C:\Users\...` or `/mnt/c/...`. On the Windows file system, Docker is many times slower and automatic reloading often stops working. This single choice avoids most Windows problems.

**macOS:** install Docker Desktop (choose the Apple Silicon or Intel build to match your Mac) and Git (`xcode-select --install`). Use any folder you like.

**Linux:** install Docker Engine with the Compose plugin and add yourself to the `docker` group (`sudo usermod -aG docker $USER`, then log out and back in).

**Check the installation** (on Windows, run these in the Ubuntu terminal):

```bash
docker --version          # Docker version 27 or newer
docker compose version    # Docker Compose version v2.x
git --version
```

If `docker` says it can't connect, start Docker Desktop and wait until it shows *Engine running*.

## B2. Get access to GitHub and clone the repository

1. Accept the collaborator invitation from the email GitHub sent you.
2. Set up authentication. The simplest option is the GitHub CLI: `sudo apt install -y gh` (macOS: `brew install gh`), then `gh auth login`, and choose *GitHub.com → HTTPS → Login with a web browser*.
3. Clone:

```bash
mkdir -p ~/code && cd ~/code
git clone https://github.com/<owner-account>/travel-app.git
cd travel-app
```

## B3. Create your local settings file

```bash
cp .env.example .env
```

`.env` holds your local database name, user, password and ports. It is ignored by git, so your personal changes never reach the repository. The default values work as they are. Change a port only if something else on your machine already uses it (for example, a locally installed PostgreSQL on 5432: set `DB_PORT=5433`).

## B4. Start the environment

```bash
docker compose up --build
```

The first start takes 5–10 minutes; later starts take seconds. You will see logs from all three containers, each line prefixed with `db-1`, `api-1` or `web-1`. It is ready when you see both of these:

```
api-1  | Now listening on: http://[::]:8080
web-1  |   ➜  Local:   http://localhost:4200/
```

The terminal stays busy while the environment runs. Press `Ctrl+C` to stop it. To run it in the background instead, use `docker compose up -d` and view logs with `docker compose logs -f`.

## B5. Check that everything works

| What | Where | What you should see |
|---|---|---|
| Web app | http://localhost:4200 | Places near Novi Sad with distances, and a green status line with the PostGIS version |
| API health | http://localhost:8080/api/health | `{"api":"ok","database":"ok","postgis":"3.5..."}` |
| API data | http://localhost:8080/api/places | JSON list of 12 places |
| API description | http://localhost:8080/openapi/v1.json | OpenAPI (Swagger) description of all endpoints |
| Database shell | `./scripts/psql.sh` | A `travel=#` prompt. Try `SELECT "Name" FROM "Places";` then `\q` to exit |

If the web app says the search failed or there are no tables, the first migration hasn't been pulled or applied yet. Run `git pull`, then `docker compose restart api`.

**Connecting a database tool (DBeaver, DataGrip, pgAdmin):** host `localhost`, port `5432`, database `travel`, user `travel`, password `changeme` (the values from your `.env`). If you prefer a browser-based tool, run `docker compose --profile tools up -d pgadmin` and open http://localhost:5050.

## B6. Open the code in your editor

- **VS Code (recommended on Windows):** install the *WSL* extension, then run `code .` in the Ubuntu terminal inside the project folder. VS Code opens connected to WSL. Install the *C# Dev Kit* and *Angular Language Service* extensions when prompted.
- **JetBrains Rider:** use *Remote Development → WSL* on Windows, or open the folder directly on macOS/Linux. Open `backend/TravelApp.slnx` for the backend. Rider also handles the Angular frontend.
- **Visual Studio 2022/2026:** it works best with files on the Windows file system, which conflicts with the WSL advice above. Prefer VS Code or Rider for this project.

For code completion in the editor you can also install the **.NET 10 SDK** and **Node.js 24** on your machine (inside WSL on Windows). This is optional; the containers do the actual building and running.

## B7. Make your first change

This confirms that live reloading works on your machine.

1. **Frontend:** open `frontend/src/app/app.html` and change the text `Find places near` to `Explore around`. Save. The browser at http://localhost:4200 updates within a few seconds.
2. **Backend:** open `backend/TravelApp.Api/Data/SeedData.cs` and look at how places are defined. Then open `Endpoints/PlacesEndpoints.cs` and read the `/nearby` endpoint. That's the core of a location-based travel app: EF Core turns `IsWithinDistance` and `Distance` into the PostGIS functions `ST_DWithin` and `ST_Distance`. In the API logs you can see the exact SQL for every request.
3. Undo your change: `git checkout -- .`

**Setup is done.** From here, Part C describes how the team works together day to day.

# Part C: Daily workflow

## C1. Start and stop

```bash
docker compose up -d        # start in the background
docker compose logs -f api  # follow logs of one service (Ctrl+C stops following, not the app)
docker compose ps           # what is running
docker compose down         # stop everything (your database data is kept)
```

## C2. Working on a feature

```bash
git checkout main
git pull                                  # get everyone's latest changes
docker compose restart api                # applies any new migrations from teammates
git checkout -b feature/place-reviews     # one branch per feature
# ... write code, test at localhost:4200 ...
git add .
git commit -m "Add reviews to places"
git push -u origin feature/place-reviews
```

Then open a pull request on GitHub. GitHub Actions builds the API and the web app automatically. When the checks are green and a teammate approves, merge it.

**Branch naming:** `feature/...` for new functionality, `fix/...` for bug fixes, `chore/...` for tooling and dependencies.

## C3. Changing the database structure

Example: adding a rating to places.

1. Change the entity in `Domain/Place.cs`, for example by adding `public decimal? Rating { get; set; }`.
2. Create a migration with a descriptive name, then apply it:

```bash
./scripts/add-migration.sh AddPlaceRating
docker compose restart api
```

3. Open the generated file in `Data/Migrations/` and check that it does what you expect. **Always review generated migrations before committing.**
4. Commit the entity change and the migration files **in the same commit**.

**Rules that keep five people from colliding:**

- Never change the database by hand (in psql, DBeaver or pgAdmin) and expect others to have that change. If it isn't in a migration, it doesn't exist for anyone else.
- Never edit or delete a migration that has already been merged into `main`. Write a new migration that corrects it.
- If two people merge migrations at the same time, the second one pulls `main`, deletes their own *unmerged* migration, and runs `add-migration.sh` again so it builds on top of the other person's.
- If you forget to create a migration after changing an entity, the API refuses to start and reports *pending model changes*. The fix is to create the migration.

## C4. When packages change

| Change | What to run |
|---|---|
| Someone changed `frontend/package.json` | `docker compose up -d --build -V web` (`-V` refreshes the container's `node_modules`) |
| Someone changed a `.csproj` file | `docker compose restart api` (packages are restored automatically) |
| You want to add an npm package | `docker compose exec web npm install <package>`, then commit `package.json` and `package-lock.json` |
| You want to add a NuGet package | Add a `<PackageReference>` with an exact version to `TravelApp.Api.csproj`, then `docker compose restart api` |

## C5. Resetting your database

If your local data gets into a strange state:

```bash
./scripts/reset-db.sh
```

This deletes **only your local database** and recreates it from the migrations and seed data. Nobody else is affected.

## C6. Running the API from your IDE (optional, for debugging)

Setting breakpoints is easiest when the API runs directly in Rider, VS Code or Visual Studio. This requires the .NET 10 SDK and Node.js 24 installed locally.

```bash
docker compose stop api web     # keep only the database running in Docker
```

Then start the API from your IDE with the **http** launch profile; it listens on http://localhost:8080 and uses the connection string from `appsettings.Development.json`. Run the frontend locally with:

```bash
cd frontend
npm ci
npm run start:local             # proxies /api to localhost:8080 instead of the api container
```

When you're done, go back to the normal setup with `docker compose up -d`.

# Part D: Troubleshooting

| Symptom | Cause and fix |
|---|---|
| `port is already allocated` when starting | Another program uses that port (often a local PostgreSQL on 5432). Change `DB_PORT`, `API_PORT` or `WEB_PORT` in `.env` and start again. |
| Warnings that `DB_NAME` or other variables are not set | You're missing the `.env` file. Run `cp .env.example .env`. |
| `Cannot connect to the Docker daemon` | Docker Desktop is not running. Start it and wait for *Engine running*. On Windows, also check *Settings → Resources → WSL integration*. |
| Saving a file doesn't reload the browser or API | On Windows the project is probably on the Windows file system. Move it into WSL (`~/code`). Otherwise run `docker compose restart web` or `docker compose restart api`. |
| Web app shows "The API is not reachable" | Run `docker compose ps`. If `api` is not running, look at `docker compose logs api`; usually it's a compile error in C# code you just changed. |
| Web app shows "cannot reach the database" | Run `docker compose logs db`. If the database won't start after an image change, run `./scripts/reset-db.sh`. |
| API logs say *pending model changes* | You changed an entity without creating a migration. Run `./scripts/add-migration.sh <Name>`. |
| Web container fails with errors about a missing package after `git pull` | Run `docker compose up -d --build -V web`. |
| `permission denied` running a script | Run `chmod +x scripts/*.sh`. |
| Everything is slow or behaves strangely | As a last resort, run `docker compose down`, then `docker compose up --build`. Your data is kept. To also delete the database, run `docker compose down -v`. |

# Part E: Cheat sheet

| Task | Command |
|---|---|
| Start everything | `docker compose up -d` |
| Start and rebuild images | `docker compose up -d --build` |
| Stop everything | `docker compose down` |
| Follow API logs | `docker compose logs -f api` |
| Restart the API (applies new migrations) | `docker compose restart api` |
| New migration | `./scripts/add-migration.sh <Name>` |
| Database shell | `./scripts/psql.sh` |
| Reset local database | `./scripts/reset-db.sh` |
| Start pgAdmin | `docker compose --profile tools up -d pgadmin` |
| Web app | http://localhost:4200 |
| API | http://localhost:8080/api/health |

# What comes next

This setup is for local development. When the team is ready for a shared environment to test features together and show them to others:

- **Docker images already get built.** After every merge to `main`, the CI workflow publishes production images of the API and web app to GitHub Container Registry (`ghcr.io/<owner>/travel-app/api` and `.../web`). The `prod` stages in the two Dockerfiles define these images.
- **A staging server runs those images.** A small cloud VM (Hetzner or DigitalOcean, roughly €10–20 per month) can run them with Docker Compose, together with its own PostgreSQL/PostGIS and a reverse proxy for HTTPS. The web image already forwards `/api` to the API container.
- **Migrations need a production path.** In development the API applies migrations at startup. On staging and production, apply them as an explicit deployment step (for example with an EF Core migration bundle), so a deployment never changes the database unexpectedly.
