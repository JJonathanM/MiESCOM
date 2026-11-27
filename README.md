# MiESCOM

Página web para el registro de alumnos en actividades culturales y deportivas, con planes de sumar otros servicios más adelante.

> **Estado: prueba de concepto.** Funcionan el registro de usuario con contraseña temporal por correo, el inicio de sesión, el cambio de contraseña y la consulta de "Mi perfil" con datos de MySQL. En desarrollo se usan alumnos inventados.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) LTS
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (MySQL y Mailpit para desarrollo)

## Cómo ejecutarlo

Desde la raíz del repo.

**1. Base de datos y correo de prueba** (la primera vez, copia `.env.example` como `.env` y define las contraseñas):

```powershell
Copy-Item .env.example .env
notepad .env
docker compose up -d
```

**2. Cadena de conexión** (una sola vez; toma la `MYSQL_PASSWORD` del `.env` y se guarda fuera del repo):

```powershell
$pw = (Select-String -Path .env -Pattern '^MYSQL_PASSWORD=(.*)$').Matches[0].Groups[1].Value
dotnet user-secrets set "ConnectionStrings:MiEscom" "server=127.0.0.1;port=3306;database=miescom;user=miescom;password=$pw" --project backend/MiESCOM.Api
dotnet tool restore
```

**3. Backend** (`http://localhost:5080`). Al arrancar en desarrollo crea las tablas y carga los alumnos de prueba:

```powershell
dotnet run --project backend/MiESCOM.Api
```

**4. Frontend** (`http://localhost:5173`), en otra terminal:

```powershell
cd frontend
npm install
npm run dev
```

**5. Probar:** abre `http://localhost:5173`, elige **Registrarme** y escribe la boleta `2099630001`. El correo con la contraseña temporal aparece en Mailpit, `http://localhost:8025`. Al entrar con ella, la aplicación pide cambiarla.

| Boleta de prueba | Caso |
|---|---|
| `2099630001` | Correo institucional |
| `2099630002` | Solo correo personal |
| `2099630003` | Sin correo: el registro lo rechaza |

Todos los datos de prueba son inventados (boletas del año 2099 y correos de dominios reservados).

**Repetir el registro:** `.\scripts\reiniciar-registro.ps1` borra todas las cuentas y los correos de Mailpit; con una boleta (`.\scripts\reiniciar-registro.ps1 2099630001`) borra solo esa. Los alumnos de prueba se conservan y no hace falta reiniciar el backend.

## Estructura

```
docker-compose.yml                   # MySQL 8.4 y Mailpit, solo en 127.0.0.1
scripts/reiniciar-registro.ps1       # Borra cuentas para repetir el registro (desarrollo)
database/schema.sql                  # SQL del esquema, generado de la migración (solo para leerlo)
backend/MiESCOM.Api/
├── Program.cs                       # Configuración: base de datos, cookie, límites de intentos
├── Auth/                            # Login, registro, cambio de contraseña, sesión
├── Alumnos/                         # /api/alumnos/me y lectura de la CURP
├── Data/                            # Tablas (Entity Framework Core), migraciones y datos de prueba
└── Email/                           # Envío de correos (MailKit)
frontend/src/
├── App.tsx                          # Rutas y sesión
├── api/                             # Llamadas a la API
├── components/                      # Encabezado, menú lateral, formularios
└── pages/                           # Login, Registro, Inicio, Mi perfil, Cambiar contraseña
```

## Cambiar el esquema de la base de datos

1. Modifica las clases de `backend/MiESCOM.Api/Data/`.
2. Crea la migración: `dotnet tool restore` y luego `dotnet ef migrations add NombreDelCambio --project backend/MiESCOM.Api --output-dir Data/Migrations`.
3. Al reiniciar el backend en desarrollo, la migración se aplica sola.
4. Actualiza el SQL de referencia: `dotnet ef migrations script --project backend/MiESCOM.Api -o database/schema.sql`.

## Cómo trabajamos

- `main` siempre debe funcionar y no recibe pushes directos.
- Cada cambio va en una rama corta (`feat/...`, `fix/...`, `docs/...`) y entra por pull request con la revisión de la otra persona.
- Nunca subas secretos ni datos reales de alumnos. Los datos de prueba deben ser inventados.
