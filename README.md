# MiESCOM

Página web para el registro de alumnos en actividades culturales y deportivas, con planes de sumar otros servicios más adelante.

> **Estado: prueba de concepto.** El login usa un usuario de demostración y no hay base de datos. La aplicación no arranca fuera del entorno de desarrollo.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) LTS

## Cómo ejecutarlo

Usa dos terminales, desde la raíz del repo.

**Backend** (`http://localhost:5080`):

```powershell
dotnet run --project backend/MiESCOM.Api
```

**Frontend** (`http://localhost:5173`):

```powershell
cd frontend
npm install
npm run dev
```

Abre `http://localhost:5173` y entra con el usuario de demostración:

| Usuario | Contraseña |
|---|---|
| `alumno.demo` | `Demo1234!` |

Estas credenciales son ficticias y públicas. No las reutilices en ningún otro lado.

## Estructura

```
backend/MiESCOM.Api/
├── Program.cs                     # Configuración de la API y de la cookie de sesión
└── Auth/
    ├── AuthEndpoints.cs           # /api/auth/login, /logout y /me
    ├── ICredentialValidator.cs    # Contrato para validar usuarios
    └── DemoCredentialValidator.cs # Usuario de demostración; se reemplaza más adelante
frontend/src/
├── App.tsx                        # Decide qué página mostrar según la sesión
├── api/auth.ts                    # Llamadas a la API
└── pages/                         # LoginPage y HomePage
```

## Cómo trabajamos

- `main` siempre debe funcionar y no recibe pushes directos.
- Cada cambio va en una rama corta (`feat/...`, `fix/...`, `docs/...`) y entra por pull request con la revisión de la otra persona.
- Nunca subas secretos ni datos reales de alumnos. Los datos de prueba deben ser inventados.
