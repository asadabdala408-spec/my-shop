# Frontend deployment (Vercel)

This app is deployed separately from the .NET API.

## Vercel setup

1. Import the GitHub repo on [vercel.com](https://vercel.com).
2. Set **Root Directory** to `Jewelryshop.Client`.
3. Add environment variable:

```text
VITE_API_BASE_URL=https://your-railway-api.up.railway.app
```

(No trailing slash.)

4. Deploy. Build output is `dist/`.

## Backend

The API runs on **Railway** from `Jewelryshop.Api/`. Full instructions: [../DEPLOY.md](../DEPLOY.md).

## Build locally

```powershell
npm install
npm run build
```

## Local development

Backend:

```powershell
cd "c:\Users\hp\OneDrive\Desktop\jewelry shop\Jewelryshop.Api"
dotnet run
```

Frontend:

```powershell
cd "c:\Users\hp\OneDrive\Desktop\jewelry shop\Jewelryshop.Client"
npm.cmd run dev
```
