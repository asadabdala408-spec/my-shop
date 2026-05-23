# Jewelry Shop

E-commerce jewelry store: **React frontend** + **ASP.NET Core API** + **PostgreSQL**.

## Project structure

```
jewelry shop/
├── Jewelryshop.Api/      # Backend API (.NET) → deploy to Railway
├── Jewelryshop.Client/   # Frontend (React + Vite) → deploy to Vercel
└── DEPLOY.md             # Step-by-step Railway + Vercel guide
```

## Local run

```powershell
# Backend
cd Jewelryshop.Api
dotnet run

# Frontend (new terminal)
cd Jewelryshop.Client
npm install
npm run dev
```

- API: http://localhost:5286  
- App: http://localhost:5173  

## Deploy

- **[DEPLOY.md](./DEPLOY.md)** — Railway (backend) + Vercel (frontend)
- **[DEPLOY-RENDER.md](./DEPLOY-RENDER.md)** — Render (backend) + Neon (database) + Vercel (frontend)
