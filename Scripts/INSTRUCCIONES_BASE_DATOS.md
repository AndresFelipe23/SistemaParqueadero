# Instrucciones para Crear la Base de Datos

## Requisitos Previos

- SQL Server 2016 o superior (Express, Standard, Enterprise)
- SQL Server Management Studio (SSMS) o Azure Data Studio
- Permisos de administrador en SQL Server

## Pasos para Ejecutar el Script

### Opción 1: Usando SQL Server Management Studio (SSMS)

1. Abra **SQL Server Management Studio**
2. Conéctese a su instancia de SQL Server (usualmente `localhost` o `.\SQLEXPRESS`)
3. Haga clic en **Nueva Consulta** (New Query)
4. Abra el archivo `CrearBaseDatosCompleta.sql`
5. Si su instalación de SQL Server está en una ruta diferente, modifique las líneas 11-20 del script:
   ```sql
   -- Cambiar estas rutas según su instalación:
   FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\SistemaParqueadero.mdf',
   ```
   Por ejemplo, para SQL Server Express:
   ```sql
   FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\SistemaParqueadero.mdf',
   ```
6. Ejecute el script completo (F5 o botón Ejecutar)
7. Verifique que todos los mensajes de "creado exitosamente" aparezcan

### Opción 2: Usando Azure Data Studio

1. Abra **Azure Data Studio**
2. Conéctese a su instancia de SQL Server
3. Abra el archivo `CrearBaseDatosCompleta.sql`
4. Modifique las rutas si es necesario (ver Opción 1)
5. Ejecute el script

### Opción 3: Usando sqlcmd (Línea de comandos)

```bash
sqlcmd -S localhost -E -i CrearBaseDatosCompleta.sql
```

O para SQL Server Express:
```bash
sqlcmd -S .\SQLEXPRESS -E -i CrearBaseDatosCompleta.sql
```

## Verificación

Después de ejecutar el script, verifique que todo se creó correctamente:

```sql
USE SistemaParqueadero;
GO

-- Verificar tablas
SELECT name FROM sys.tables ORDER BY name;

-- Verificar vistas
SELECT name FROM sys.views ORDER BY name;

-- Verificar procedimientos almacenados
SELECT name FROM sys.procedures ORDER BY name;

-- Verificar datos iniciales
SELECT * FROM Usuarios;
SELECT * FROM Tarifas;
SELECT * FROM Configuracion;
```

## Estructura de la Base de Datos

### Tablas Principales

1. **Usuarios**: Empleados y administradores del sistema
2. **Vehiculos**: Historial completo de vehículos que han usado el parqueadero
3. **Tarifas**: Configuración de precios por tipo de vehículo
4. **RegistrosParqueo**: Entradas y salidas de vehículos
5. **Pagos**: Movimientos de pagos realizados
6. **MovimientosCaja**: Control de ingresos y egresos de caja
7. **CierresCaja**: Cierres diarios de caja
8. **Configuracion**: Configuración general del sistema

### Vistas Útiles

- **VW_VehiculosActivos**: Lista de vehículos actualmente en el parqueadero
- **VW_ResumenPagosDia**: Resumen diario de pagos por método
- **VW_HistorialVehiculos**: Historial completo de cada vehículo

### Procedimientos Almacenados

- **SP_ObtenerVehiculosActivos**: Obtiene lista de vehículos activos
- **SP_RegistrarEntrada**: Registra entrada de vehículo (crea vehículo si no existe)
- **SP_RegistrarSalida**: Registra salida y calcula monto

## Datos Iniciales

El script crea automáticamente:

1. **Usuario Administrador**:
   - Usuario: `admin`
   - Contraseña: **DEBE CAMBIARSE EN PRODUCCIÓN**
   - Rol: Administrador

2. **Tarifas por Defecto**:
   - Carro por hora: $2,000 COP
   - Moto por hora: $1,000 COP
   - Carro por minuto: $35 COP
   - Moto por minuto: $18 COP

3. **Configuración Inicial**:
   - Nombre del parqueadero
   - Dirección
   - Configuración de moneda

## Configuración de la Cadena de Conexión

Después de crear la base de datos, actualice `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SistemaParqueadero;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Para SQL Server Express:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=SistemaParqueadero;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Solución de Problemas

### Error: "No se puede crear el archivo de base de datos"

**Solución**: Modifique las rutas en el script para que apunten a una carpeta donde tenga permisos de escritura, o elimine las líneas de FILEGROUP y deje que SQL Server use la configuración predeterminada.

### Error: "Ya existe la base de datos"

**Solución**: Si desea recrear la base de datos, primero elimínela:
```sql
USE master;
GO
DROP DATABASE IF EXISTS SistemaParqueadero;
GO
```

### Error de permisos

**Solución**: Asegúrese de estar conectado con una cuenta que tenga permisos de `sysadmin` o `dbcreator`.

## Próximos Pasos

Después de crear la base de datos:

1. Actualice la cadena de conexión en `appsettings.json`
2. Ejecute las migraciones de Entity Framework (si las usa)
3. Cambie la contraseña del usuario administrador
4. Configure las tarifas según sus necesidades
5. Agregue usuarios adicionales si es necesario
