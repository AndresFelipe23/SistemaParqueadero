# Sistema de Parqueadero - Montería, Colombia

Sistema web para gestión de parqueadero de motos y carros con registro de entrada/salida, cálculo automático de tarifas e impresión de recibos.

## Características

- ✅ Registro de entrada de vehículos (carros y motos) por placa
- ✅ Escaneo de código de barras/QR para entrada y salida
- ✅ Cálculo automático de tiempo de parqueo
- ✅ Cálculo automático de tarifas según tipo de vehículo y tiempo
- ✅ Impresión de recibos de entrada y salida
- ✅ Consulta de vehículos activos en tiempo real
- ✅ Base de datos SQL Server
- ✅ Interfaz web moderna y responsive

## Requisitos

- .NET 8.0 SDK
- SQL Server (LocalDB, Express o Full)
- Visual Studio 2022 o Visual Studio Code

## Instalación

### 1. Configurar la Base de Datos

Ejecute el script SQL incluido en la carpeta `Scripts`:

```sql
-- Ejecutar Scripts/CrearBaseDatos.sql en SQL Server Management Studio
```

O configure la conexión en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=SistemaParqueadero;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 2. Restaurar Paquetes NuGet

```bash
dotnet restore
```

### 3. Crear la Base de Datos con Entity Framework (Opcional)

Si prefiere usar migraciones de Entity Framework:

```bash
cd SistemaParqueaderoWEB
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Ejecutar la Aplicación

```bash
dotnet run
```

O desde Visual Studio, presione F5.

La aplicación estará disponible en: `https://localhost:5001` o `http://localhost:5000`

## Configuración de Tarifas

Las tarifas se configuran en `appsettings.json`:

```json
{
  "Tarifas": {
    "CarroPorHora": 2000,
    "MotoPorHora": 1000,
    "CarroPorMinuto": 35,
    "MotoPorMinuto": 18
  }
}
```

**Nota:** Los valores están en pesos colombianos (COP).

## Uso del Sistema

### Registrar Entrada

1. Navegue a **Entrada** en el menú
2. Ingrese la placa del vehículo
3. Seleccione el tipo de vehículo (Carro o Moto)
4. Opcionalmente, escanee o ingrese un código de barras/QR
5. Haga clic en **Registrar Entrada**
6. Se generará un recibo con código QR que debe imprimirse

### Registrar Salida

1. Navegue a **Salida** en el menú
2. Ingrese la placa o escanee el código QR del recibo de entrada
3. El sistema mostrará la información del vehículo y el monto estimado
4. Haga clic en **Registrar Salida e Imprimir Recibo**
5. Se generará un recibo de salida con el monto total a pagar

### Ver Vehículos Activos

1. Navegue a **Vehículos Activos** en el menú
2. Verá una lista de todos los vehículos actualmente en el parqueadero
3. La lista se actualiza automáticamente cada 30 segundos

## Estructura del Proyecto

```
SistemaParqueadero/
├── SistemaParqueaderoWEB/
│   ├── Controllers/
│   │   └── ParqueaderoController.cs
│   ├── Data/
│   │   └── ParqueaderoDbContext.cs
│   ├── Models/
│   │   ├── RegistroParqueo.cs
│   │   ├── TipoVehiculo.cs
│   │   ├── Tarifa.cs
│   │   └── ReciboViewModel.cs
│   ├── Views/
│   │   ├── Parqueadero/
│   │   │   ├── Index.cshtml
│   │   │   ├── Entrada.cshtml
│   │   │   ├── Salida.cshtml
│   │   │   ├── ReciboEntrada.cshtml
│   │   │   ├── ReciboSalida.cshtml
│   │   │   └── VehiculosActivos.cshtml
│   │   └── Shared/
│   │       └── _Layout.cshtml
│   └── appsettings.json
└── Scripts/
    └── CrearBaseDatos.sql
```

## Tecnologías Utilizadas

- ASP.NET Core MVC 8.0
- Entity Framework Core 8.0
- SQL Server
- Bootstrap 5
- HTML5 QR Code Scanner
- QRCode.js (para generar códigos QR)

## Funcionalidades de Escaneo

El sistema utiliza la biblioteca HTML5 QR Code Scanner que permite:
- Escaneo desde cámara web
- Escaneo desde cámara del dispositivo móvil
- Soporte para códigos QR y códigos de barras

**Nota:** El escaneo requiere permisos de cámara en el navegador.

## Impresión de Recibos

Los recibos están optimizados para impresión:
- Presione el botón **Imprimir Recibo** en cualquier recibo
- O use Ctrl+P (Cmd+P en Mac)
- Los recibos se formatean automáticamente para impresoras térmicas

## Soporte

Para problemas o preguntas, contacte al equipo de desarrollo.

---

**Desarrollado para Montería, Colombia** 🇨🇴
