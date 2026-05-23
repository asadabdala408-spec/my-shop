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

### PostgreSQL

1. Project-ka → **+ New** → **Database** → **PostgreSQL**.
2. Ku xir API service-ka: **Variables** → **Add Reference** → `DATABASE_URL` ama isticmaal connection string Neon/Postgres.

### Environment variables (API service)

Ku dar **Variables** tab-ka:

```text
ConnectionStrings__DefaultConnection=<postgres connection string>
Jwt__Issuer=Jewelryshop.Api
Jwt__Audience=Jewelryshop.Client
Jwt__Key=<random secret, at least 32 characters>
Cors__AllowedOrigins=https://YOUR-APP.vercel.app,http://localhost:5173
ASPNETCORE_ENVIRONMENT=Production
```

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
