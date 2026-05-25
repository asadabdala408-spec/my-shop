# Jewelry Shop — Deploy (Railway + Vercel)

Mashruucan waa **laba qaybood** oo kala go'an:

| Qayb | Folder | Deploy |
|------|--------|--------|
| **Backend** (.NET API) | `Jewelryshop.Api/` | [Railway](https://railway.app) |
| **Frontend** (React + Vite) | `Jewelryshop.Client/` | [Vercel](https://vercel.com) |

> `JewelryShop_API_Complete/` waa nuqul hore — isticmaal **`Jewelryshop.Api`** kaliya.

---

## 1. Backend — Railway

### Abuur mashruuc

1. Gal [railway.app](https://railway.app) → **New Project** → **Deploy from GitHub repo**.
2. Dooro repo-gaaga.
3. **Root Directory** (ama **Service path**): `Jewelryshop.Api`
4. Railway wuxuu isticmaali doonaa `Dockerfile` + `railway.toml`.

### Database — Neon (aad isticmaalayso) ama Railway PostgreSQL

#### Neon + Railway (tusaale aad)

Ma u baahanid PostgreSQL Railway. Database-ka waa **Neon**; API-ga waa **Railway**.

1. Gal [neon.tech](https://neon.tech) → project-kaaga → **Connect**.
2. Dooro **Connection string** → **Direct** (pooling OFF — wanaagsan migrations).
3. Nuqul URL-ka, tusaale:

```text
postgresql://USER:PASSWORD@ep-xxxx.region.aws.neon.tech/neondb?sslmode=require
```

4. Railway → **API service** → **Variables** → ku dar **mid** ka mid ah:

| Name | Value |
|------|--------|
| `DATABASE_URL` | paste Neon URL oo dhan (`postgresql://...`) |

**ama** Npgsql format:

| Name | Value |
|------|--------|
| `ConnectionStrings__DefaultConnection` | `Host=ep-xxxx.region.aws.neon.tech;Database=neondb;Username=USER;Password=PASSWORD;SSL Mode=Require` |

5. **Redeploy** API.

> **Ha isticmaalin** `${{Postgres.DATABASE_URL}}` haddii aadan PostgreSQL Railway lahayn — Neon waa external.

#### Railway PostgreSQL (haddii aad Neon ka tagto)

1. Project-ka → **+ New** → **Database** → **PostgreSQL**.
2. API → **Variables** → `DATABASE_URL` = `${{Postgres.DATABASE_URL}}` (Add Reference).

### Environment variables (API service)

**Ha ku darin** placeholder `your_user` / `your_password` — app-ku wuu iska diidi doonaa.

Ku dar sidoo kale:

```text
Jwt__Issuer=Jewelryshop.Api
Jwt__Audience=Jewelryshop.Client
Jwt__Key=<random secret, at least 32 characters>
Cors__AllowedOrigins=https://YOUR-APP.vercel.app,http://localhost:5173
ASPNETCORE_ENVIRONMENT=Production
```

Kadib **Redeploy** API service.

Cloudinary (haddii aad sawirro upload gareyso):

```text
Cloudinary__CloudName=...
Cloudinary__ApiKey=...
Cloudinary__ApiSecret=...
```

### Domain

1. API service → **Settings** → **Networking** → **Generate Domain**.
2. Nuqul URL-ka, tusaale: `https://jewelryshop-api-production.up.railway.app`

### Hubi

Browser ama curl:

```text
https://YOUR-RAILWAY-URL/health
```

Waa inuu soo celiyo: `{"status":"ok"}`

Database (waa inuu shaqeeyaa):

```text
https://YOUR-RAILWAY-URL/health/db
```

Waa inuu soo celiyo: `{"status":"ok","categories":5,"products":15}` (tirooyinka way kala duwanaan karaan).

Haddii Railway **Healthcheck failed** (`/health` service unavailable):

1. API → **Variables** → **Add Reference** → PostgreSQL → `DATABASE_URL` (waa lagama maarmaan).
2. Hubi `Jwt__Key` (ugu yaraan 32 xaraf) haddii aad appsettings default ka saartay.
3. Push code cusub (migration waa inaysan xannibin `/health`).
4. **Deploy Logs** → eeg qalad startup (tusaale `Database is not configured`).

Haddii `{"status":"error","message":"Cannot connect to database."}`:

1. API service → **Variables** → **Add Reference** → dooro PostgreSQL service → `DATABASE_URL` (ha isticmaalin `DATABASE_PUBLIC_URL` gudaha Railway).
2. Ama ku dar: `ConnectionStrings__DefaultConnection` = isla `DATABASE_URL` (postgres://…).
3. Hubi in PostgreSQL service uu **Running** yahay.
4. **Redeploy** API kadib beddelka.

---

## 2. Frontend — Vercel

### Abuur mashruuc

1. Gal [vercel.com](https://vercel.com) → **Add New Project** → import GitHub repo.
2. **Root Directory**: `Jewelryshop.Client`
3. Framework: **Vite** (auto-detect)

### Environment variable

**Settings** → **Environment Variables**:

| Name | Value |
|------|--------|
| `VITE_API_BASE_URL` | `https://YOUR-RAILWAY-URL` (aan trailing slash lahayn) |

Ku dar **Production**, **Preview**, iyo **Development** haddii loo baahdo.

### Deploy

Vercel wuxuu dhisaa `npm run build` → folder `dist`.

### CORS

Ku dar URL-ka Vercel Railway `Cors__AllowedOrigins`:

```text
https://your-app.vercel.app,http://localhost:5173
```

Kadib **Redeploy** API-ga Railway.

---

## 3. Local development

**Terminal 1 — Backend:**

```powershell
cd "Jewelryshop.Api"
dotnet run
```

API: `http://localhost:5286`

**Terminal 2 — Frontend:**

```powershell
cd "Jewelryshop.Client"
npm install
npm run dev
```

Frontend: `http://localhost:5173` — Vite proxy wuxuu `/api` u diraa backend-ka (`.env` madama `VITE_API_BASE_URL` madhan yahay).

---

## 4. Admin default (beddel production-ka)

```text
Email: admin@jewelryshop.com
Password: Admin12345!
```

Production-ka ka hor beddel password-ka admin.

---

## Quick checklist

- [ ] PostgreSQL Railway / Neon connected
- [ ] Railway API `/health` returns OK
- [ ] `VITE_API_BASE_URL` on Vercel = Railway URL
- [ ] `Cors__AllowedOrigins` includes Vercel domain
- [ ] `Jwt__Key` is a strong random secret (not default)
- [ ] Admin password changed
