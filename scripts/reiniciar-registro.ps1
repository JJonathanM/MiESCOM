<#
.SYNOPSIS
    Borra cuentas de la base de datos de desarrollo para poder repetir el registro (por ejemplo,
    en una demostración). Los alumnos de prueba se conservan; solo se borra su cuenta de usuario.

.DESCRIPTION
    Usa el MySQL de docker-compose y la contraseña del archivo .env. También vacía Mailpit, para
    que en la demostración solo aparezca el correo nuevo. Al borrar la cuenta también se quita
    un bloqueo por intentos fallidos.

.EXAMPLE
    .\scripts\reiniciar-registro.ps1
    Borra todas las cuentas.

.EXAMPLE
    .\scripts\reiniciar-registro.ps1 2099630001
    Borra solo la cuenta de esa boleta.

.EXAMPLE
    .\scripts\reiniciar-registro.ps1 -ConservarCorreos
    Borra las cuentas pero deja los correos en Mailpit.
#>
param(
    [ValidatePattern('^\d{10}$')]
    [string] $Boleta,

    [switch] $ConservarCorreos,

    # Solo para pruebas con otros contenedores o puertos.
    [string] $Contenedor = 'miescom-mysql',
    [string] $MailpitUrl = 'http://localhost:8025'
)

$ErrorActionPreference = 'Stop'

$envFile = Join-Path (Split-Path $PSScriptRoot -Parent) '.env'
if (-not (Test-Path $envFile)) {
    throw "No existe el archivo .env. Copia .env.example como .env (ver la guía rápida)."
}
$match = Select-String -Path $envFile -Pattern '^MYSQL_PASSWORD=(.*)$'
if (-not $match) {
    throw "El archivo .env no tiene MYSQL_PASSWORD."
}
$password = $match.Matches[0].Groups[1].Value

# La boleta ya se validó como 10 dígitos, así que puede ir en el SQL sin riesgo.
$filtro = if ($Boleta) { "WHERE boleta = '$Boleta'" } else { '' }
$sql = "DELETE FROM usuario $filtro; SELECT ROW_COUNT();"

$borradas = docker exec -e "MYSQL_PWD=$password" $Contenedor mysql -u miescom -N miescom -e $sql
if ($LASTEXITCODE -ne 0) {
    throw "No se pudo entrar a MySQL. ¿Está abierto Docker Desktop y corriendo la base de datos? (docker compose up -d)"
}

if ($Boleta) {
    if ([int]$borradas -gt 0) { "Se borró la cuenta de la boleta $Boleta." } else { "La boleta $Boleta no tenía cuenta." }
} else {
    "Cuentas borradas: $borradas."
}

if (-not $ConservarCorreos) {
    try {
        Invoke-RestMethod -Method Delete -Uri "$MailpitUrl/api/v1/messages" | Out-Null
        'Correos de Mailpit borrados.'
    } catch {
        'Mailpit no respondió; los correos anteriores siguen ahí.'
    }
}

'Listo: ya puedes repetir el registro.'
