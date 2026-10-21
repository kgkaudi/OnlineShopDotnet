# OnlineShop frontend

The storefront and admin UI for OnlineShopDotnet, built with **Next.js 16** (App Router), **React**, **TypeScript** and **Tailwind CSS**. It talks to the ASP.NET Core API in `../backend`.

## Requirements

- Node.js 20.9 or newer
- The API running on `http://localhost:5000` (see the root [README](../README.md))

## Setup

Create `.env.local`:

```
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

```bash
npm install
npm run dev
```

Open http://localhost:3000. `NEXT_PUBLIC_*` values are read when the dev server starts, so restart it after editing `.env.local`.

## Scripts

| Command | What it does |
|---------|--------------|
| `npm run dev` | development server |
| `npm run build` | production build (fails on TypeScript errors) |
| `npm start` | serve the production build |
| `npx tsc --noEmit` | quick type check, much faster than a full build |

## Pages

| Route | Description |
|-------|-------------|
| `/` | home |
| `/products`, `/products/[id]` | catalog and product details |
| `/cart`, `/wishlist` | cart and wishlist |
| `/orders` | your orders, with status filter, reorder and cancel |
| `/profile` | your profile |
| `/auth/login`, `/auth/register`, `/auth/logout` | authentication |
| `/admin/products`, `/admin/categories`, `/admin/users`, `/admin/orders` | admin pages (Admin role required) |
| `/admin/orders/[id]` | admin order details |

## Code layout

- `app/` : pages and shared components
- `src/lib/api.ts` : typed API client and token helpers
- `src/context/` : snackbar notifications
- `src/store/` : client-side auth state

## Docker

From the repository root, `docker compose up --build` builds and starts this app together with the API. The API URL is passed to the build as the `NEXT_PUBLIC_API_URL` build argument, so rebuild after changing it.
