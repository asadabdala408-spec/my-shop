# Jewelry Shop — Deploy (Render + Vercel)

Mashruucan waa **laba qaybood** oo kala go'an:

| Qayb | Folder | Deploy |
|------|--------|--------|
| **Backend** (.NET API) | `Jewelryshop.Api/` | [Render](https://render.com) |
| **Frontend** (React + Vite) | `Jewelryshop.Client/` | [Vercel](https://vercel.com) |

> `JewelryShop_API_Complete/` waa nuqul hore — isticmaal **`Jewelryshop.Api`** kaliya.

Tilmaamaha Railway: [DEPLOY.md](./DEPLOY.md)

---

## 0. GitHub

Hubi in code-ka la push gareeyay:

```powershell
cd "c:\Users\hp\OneDrive\Desktop\jewelry shop"
git add .
git commit -m "Deploy setup"
git push
```

---

## 1. Neon — PostgreSQL (database-ka aad isticmaalayso)

Ma aha inaad PostgreSQL Render abuurto. Database-ka waa **Neon**; API-ga waa **Render**.

### 1.1 Abuur project Neon

1. Gal [neon.tech](https://neon.tech) → soo gal (GitHub ama email).
2. **New Project**.
3. Magac: tusaale `jewelryshop`.
4. Region: dooro mid kuugu dhow.
5. **Create Project**.

### 1.2 Connection string

1. Dashboard → project-kaaga → **Connect**.
2. Dooro **.NET** ama **Connection string**.
3. Nuqul string-ka. Waxay u ekaan kartaa:

```text
postgresql://USER:PASSWORD@ep-xxxx.region.aws.neon.tech/neondb?sslmode=require
```

4. U beddel **Npgsql** format (Render / .NET API):

```text
Host=ep-xxxx.region.aws.neon.tech;Database=neondb;Username=USER;Password=PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

| Neon URL qayb | Npgsql |
|---------------|--------|
| `ep-xxxx...neon.tech` | `Host=ep-xxxx...neon.tech` |
| `neondb` (ka dib `/`) | `Database=neondb` |
| user ka hor `@` | `Username=USER` |
| password | `Password=PASSWORD` |

> **Pooled vs Direct:** EF migrations / app-ka API — isticmaal **Direct** connection Neon (ma aha pooler) haddii aad aragto qalado migration. Neon Connect → toggle **Connection pooling** OFF si aad u hesho direct host.

### 1.3 Render-ka geli (hal variable)

Marka Web Service abuurto (tallaabada 2), geli:

```env
ConnectionStrings__DefaultConnection=Host=ep-xxxx.region.aws.neon.tech;Database=neondb;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

API-ga wuxuu **automatic** migrations ku sameynayaa marka uu kaco (`MigrateAsync`).

---

### Ikhtiyaar: PostgreSQL Render (haddii aadan Neon isticmaalin)

1. Render → **+ New** → **PostgreSQL** → **Create**.
2. **Connections** → nuqul URL → u beddel Npgsql format-ka kor ku qoran.

---

## 2. Render — Web Service (API)

1. **New +** → **Web Service**.
2. Dooro GitHub repo-ga jewelry shop.
3. Dejinta:

| Goob | Qiime |
|------|--------|
| **Name** | `jewelryshop-api` (ama magac aad doorato) |
| **Root Directory** | `Jewelryshop.Api` |
| **Environment** | `Docker` |
| **Region** | Isla region database-ka (ugu fiican) |
| **Branch** | `main` (ama branch-kaaga) |
| **Instance Type** | Free ama Paid |

4. **Advanced** (ikhtiyaar):
   - **Health Check Path**: `/health`

5. **Environment Variables** — ku dar (copy):

```env
ConnectionStrings__DefaultConnection=Host=BEDDEL_HOST;Port=5432;Database=BEDDEL_DB;Username=BEDDEL_USER;Password=BEDDEL_PASSWORD;SSL Mode=Require;Trust Server Certificate=true

Jwt__Issuer=Jewelryshop.Api
Jwt__Audience=Jewelryshop.Client
Jwt__Key=BEDDEL_JWT_SECRET_32_CHARACTERS_OR_MORE

Cors__AllowedOrigins=http://localhost:5173

ASPNETCORE_ENVIRONMENT=Production
```

**Cloudinary** (haddii sawir upload loo baahdo):

```env
Cloudinary__CloudName=BEDDEL_CLOUD_NAME
Cloudinary__ApiKey=BEDDEL_API_KEY
Cloudinary__ApiSecret=BEDDEL_API_SECRET
```

6. **Create Web Service** → sug deploy-ka.

### Domain API

Marka deploy dhammaado, URL waa noqon karaa:

```text
https://jewelryshop-api.onrender.com
```

Hubi browser-ka:

```text
https://YOUR-SERVICE.onrender.com/health
```

Waa inuu soo celiyo: `{"status":"ok"}`

### Logs

Haddii qalad jiro: service → **Logs** → eeg database connection, JWT, iwm.

---

## 3. Vercel — Frontend

1. Gal [vercel.com](https://vercel.com) → **Add New Project**.
2. Import isla GitHub repo.
3. **Root Directory**: `Jewelryshop.Client`
4. Framework: **Vite** (auto-detect)
5. **Environment Variables** — ka hor deploy:

```env
VITE_API_BASE_URL=https://YOUR-SERVICE.onrender.com
```

| Name | Value |
|------|--------|
| `VITE_API_BASE_URL` | `https://jewelryshop-api.onrender.com` (beddel magacaaga) |

**Ha ku darin** `/` dhamaadka URL-ka.

Dooro: **Production**, **Preview**, **Development**.

6. **Deploy**.

Hel Vercel URL, tusaale:

```text
https://jewelry-shop.vercel.app
```

---

## 4. CORS — isku xir Render + Vercel

Marka Vercel URL-kaaga hesho:

1. Render → **Web Service** → **Environment**.
2. Beddel `Cors__AllowedOrigins`:

```env
Cors__AllowedOrigins=https://jewelry-shop.vercel.app,http://localhost:5173
```

(Ku beddel `jewelry-shop.vercel.app` URL-kaaga dhabta ah.)

3. Service wuxuu si toos ah u redeploy gareeyaa (ama **Manual Deploy**).

4. Vercel: haddii aad hore u deploy gareysay, **Redeploy** ma aha loo baahan haddii `VITE_API_BASE_URL` sax yahay.

---

## 5. Hubi website-ka

1. Fur Vercel URL.
2. Login:
   - Email: `admin@jewelryshop.com`
   - Password: `Admin12345!`
3. Production-ka ka hor **beddel password-ka admin**.

---

## 6. Local development

**Backend:**

```powershell
cd Jewelryshop.Api
dotnet run
```

API: http://localhost:5286

**Frontend:**

```powershell
cd Jewelryshop.Client
npm install
npm run dev
```

App: http://localhost:5173 (Vite proxy `/api` → backend marka `VITE_API_BASE_URL` madhan yahay).

---

## Frontend Render (beddelka Vercel)

Haddii aad frontend sidoo kale Render ku rabto:

1. **New +** → **Static Site**.
2. Repo + **Root Directory**: `Jewelryshop.Client`
3. **Build Command**: `npm install && npm run build`
4. **Publish Directory**: `dist`
5. **Environment**:

```env
VITE_API_BASE_URL=https://YOUR-SERVICE.onrender.com
```

6. CORS: ku dar static site URL `Cors__AllowedOrigins` API-ga Render.

---

## Variables — copy paste (buuxa)

### Render (Web Service)

```env
ConnectionStrings__DefaultConnection=Host=BEDDEL_HOST;Port=5432;Database=BEDDEL_DB;Username=BEDDEL_USER;Password=BEDDEL_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
Jwt__Issuer=Jewelryshop.Api
Jwt__Audience=Jewelryshop.Client
Jwt__Key=BEDDEL_JWT_SECRET_32_CHARACTERS_OR_MORE
Cors__AllowedOrigins=https://BEDDEL-APP.vercel.app,http://localhost:5173
ASPNETCORE_ENVIRONMENT=Production
```

### Vercel

```env
VITE_API_BASE_URL=https://BEDDEL-API.onrender.com
```

---

## Taxane (Neon + Render + Vercel)

```
GitHub push
    ↓
Neon PostgreSQL (connection string)
    ↓
Render Web Service (Docker, Jewelryshop.Api) + Variables
    ↓
/health OK?
    ↓
Vercel (Jewelryshop.Client) + VITE_API_BASE_URL
    ↓
Cors__AllowedOrigins = Vercel URL → Render redeploy
    ↓
Website shaqeynaya
```

---

## Qalado caadi ah

| Dhibaato | Xalka |
|---------|--------|
| Free tier API hurdaa | Render free web service wuu seexdaa ~15 daqiiqo kadib — request-ka koowaad wuu qaadan karaa 30–60 ilbiriqsi |
| Failed to fetch (frontend) | Hubi `VITE_API_BASE_URL` + Redeploy Vercel |
| CORS error | Ku dar Vercel URL `Cors__AllowedOrigins` Render |
| 500 / database | Hubi `ConnectionStrings__DefaultConnection` format Npgsql |
| Build failed | Hubi **Root Directory** = `Jewelryshop.Api`, **Environment** = Docker |

---

## Render vs Railway

| | Render | Railway |
|---|--------|---------|
| Backend | ✅ Web Service + Docker | ✅ Docker |
| Postgres | ✅ Render PostgreSQL | ✅ Railway PostgreSQL |
| Free tier | Haa (cold start) | Haa |
| Guide | DEPLOY-RENDER.md (kan) | [DEPLOY.md](./DEPLOY.md) |

Labaduba way ku habboon yihiin mashruucan.

---

## Checklist

- [ ] Code GitHub ku jira
- [ ] Neon project created + connection string in Render
- [ ] Render Web Service deployed (`/health` OK)
- [ ] `ConnectionStrings__DefaultConnection` sax
- [ ] `Jwt__Key` secret cusub (ma aha default)
- [ ] Vercel `VITE_API_BASE_URL` = Render API URL
- [ ] `Cors__AllowedOrigins` includes Vercel domain
- [ ] Admin password changed
