# Migraciones de Entity Framework

## ¿Necesito Migraciones?

**NO**, si ya ejecutaste el script SQL `CrearBaseDatosCompleta_Corregido.sql` y la base de datos está creada, **NO necesitas crear migraciones**.

Los modelos de C# que creamos están diseñados para trabajar directamente con la estructura de la base de datos que ya existe.

## Si Quieres Usar Migraciones (Opcional)

Si prefieres usar migraciones de Entity Framework en lugar del script SQL, sigue estos pasos:

### 1. Asegúrate de tener acceso a internet
Las migraciones requieren descargar paquetes NuGet.

### 2. Ejecuta desde el directorio del proyecto:
```powershell
cd SistemaParqueaderoWEB
dotnet ef migrations add InitialCreate
```

### 3. Aplica las migraciones:
```powershell
dotnet ef database update
```

**Nota**: Si usas migraciones, NO ejecutes el script SQL, ya que las migraciones crearán la base de datos automáticamente.

## Usar la Base de Datos Existente (Recomendado)

Como ya tienes la base de datos creada:

1. ✅ Configura la cadena de conexión en `appsettings.json`
2. ✅ Ejecuta la aplicación: `dotnet run`
3. ✅ La aplicación se conectará directamente a la base de datos existente

Los modelos están configurados para mapear correctamente a las tablas existentes.

## Verificar la Conexión

Para verificar que todo funciona correctamente:

1. Configura la cadena de conexión con tus credenciales
2. Ejecuta la aplicación
3. Si hay errores de conexión, revisa:
   - Que el servidor SQL esté ejecutándose
   - Que las credenciales sean correctas
   - Que el usuario tenga permisos en la base de datos

## Solución de Problemas

### Error: "No se puede cargar el índice de servicio"
- Necesitas acceso a internet para descargar paquetes NuGet
- O usa los paquetes que ya están instalados si los tienes en caché

### Error: "Cannot open database"
- Verifica que la base de datos existe
- Verifica la cadena de conexión
- Verifica permisos del usuario
