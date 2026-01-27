# Configuración de Conexión a Base de Datos

## Configurar la Cadena de Conexión

Para conectar la aplicación a SQL Server con autenticación por usuario y contraseña, edite los archivos de configuración:

### 1. Editar `appsettings.json`

Abra el archivo `SistemaParqueaderoWEB/appsettings.json` y actualice la cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=SistemaParqueadero;User Id=TU_USUARIO;Password=TU_CONTRASEÑA;TrustServerCertificate=True;"
  }
}
```

### 2. Editar `appsettings.Development.json`

También actualice el archivo de desarrollo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=SistemaParqueadero;User Id=TU_USUARIO;Password=TU_CONTRASEÑA;TrustServerCertificate=True;"
  }
}
```

## Ejemplos de Cadenas de Conexión

### SQL Server Local (con instancia)
```
Server=localhost\\SQLEXPRESS;Database=SistemaParqueadero;User Id=sa;Password=TuPassword123;TrustServerCertificate=True;
```

### SQL Server Remoto
```
Server=192.168.1.100;Database=SistemaParqueadero;User Id=parqueadero_user;Password=TuPassword123;TrustServerCertificate=True;
```

### SQL Server con Puerto Específico
```
Server=servidor.dominio.com,1433;Database=SistemaParqueadero;User Id=usuario;Password=contraseña;TrustServerCertificate=True;
```

### SQL Server Azure
```
Server=tcp:servidor.database.windows.net,1433;Database=SistemaParqueadero;User Id=usuario@servidor;Password=contraseña;Encrypt=True;TrustServerCertificate=False;
```

## Parámetros de la Cadena de Conexión

- **Server**: Nombre del servidor o dirección IP
  - Para instancias: `servidor\\INSTANCIA` o `servidor\INSTANCIA`
  - Para puerto específico: `servidor,puerto`
  
- **Database**: Nombre de la base de datos (`SistemaParqueadero`)

- **User Id**: Nombre de usuario de SQL Server

- **Password**: Contraseña del usuario

- **TrustServerCertificate**: `True` para desarrollo, `False` para producción

## Verificar la Conexión

Después de configurar la cadena de conexión, puede verificar la conexión ejecutando la aplicación:

```bash
cd SistemaParqueaderoWEB
dotnet run
```

Si hay errores de conexión, verifique:

1. ✅ El servidor SQL Server está ejecutándose
2. ✅ El usuario y contraseña son correctos
3. ✅ El usuario tiene permisos en la base de datos `SistemaParqueadero`
4. ✅ El firewall permite conexiones al puerto de SQL Server (por defecto 1433)
5. ✅ La autenticación SQL Server está habilitada en el servidor

## Habilitar Autenticación SQL Server

Si está usando SQL Server local y solo tiene autenticación de Windows habilitada:

1. Abra **SQL Server Management Studio**
2. Conéctese al servidor
3. Haga clic derecho en el servidor → **Propiedades**
4. Vaya a **Seguridad**
5. Seleccione **Autenticación de SQL Server y autenticación de Windows**
6. Haga clic en **Aceptar**
7. Reinicie el servicio SQL Server

## Crear Usuario en SQL Server

Si necesita crear un usuario específico para la aplicación:

```sql
USE SistemaParqueadero;
GO

-- Crear usuario de inicio de sesión
CREATE LOGIN parqueadero_user WITH PASSWORD = 'TuPasswordSeguro123!';
GO

-- Crear usuario en la base de datos
CREATE USER parqueadero_user FOR LOGIN parqueadero_user;
GO

-- Otorgar permisos
ALTER ROLE db_owner ADD MEMBER parqueadero_user;
GO
```

## Seguridad

⚠️ **IMPORTANTE**: 

- **NUNCA** suba archivos `appsettings.json` con credenciales reales a repositorios públicos
- Use variables de entorno o Azure Key Vault en producción
- Use usuarios con permisos mínimos necesarios (no `sa`)

### Usar Variables de Entorno (Recomendado)

Puede configurar la cadena de conexión usando variables de entorno:

**Windows (PowerShell):**
```powershell
$env:ConnectionStrings__DefaultConnection = "Server=servidor;Database=SistemaParqueadero;User Id=usuario;Password=contraseña;TrustServerCertificate=True;"
```

**Linux/Mac:**
```bash
export ConnectionStrings__DefaultConnection="Server=servidor;Database=SistemaParqueadero;User Id=usuario;Password=contraseña;TrustServerCertificate=True;"
```

## Solución de Problemas

### Error: "Cannot open database"
- Verifique que la base de datos existe
- Verifique que el usuario tiene permisos

### Error: "Login failed for user"
- Verifique usuario y contraseña
- Verifique que la autenticación SQL está habilitada

### Error: "A network-related or instance-specific error"
- Verifique que SQL Server está ejecutándose
- Verifique el nombre del servidor
- Verifique el firewall

### Error: "The server was not found or was not accessible"
- Verifique la conectividad de red
- Verifique que SQL Server Browser está ejecutándose (para instancias nombradas)
