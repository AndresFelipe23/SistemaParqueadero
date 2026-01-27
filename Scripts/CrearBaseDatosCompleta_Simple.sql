-- =============================================
-- Script para crear la base de datos completa
-- Sistema de Parqueadero - Montería, Colombia
-- Versión simplificada (sin rutas específicas)
-- =============================================

USE master;
GO

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SistemaParqueadero')
BEGIN
    CREATE DATABASE SistemaParqueadero;
    PRINT 'Base de datos SistemaParqueadero creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La base de datos SistemaParqueadero ya existe.';
END
GO

USE SistemaParqueadero;
GO

-- =============================================
-- TABLA: Usuarios (Empleados del parqueadero)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Apellido NVARCHAR(100) NOT NULL,
        Documento NVARCHAR(20) NOT NULL UNIQUE,
        Email NVARCHAR(100) NULL,
        Telefono NVARCHAR(20) NULL,
        Usuario NVARCHAR(50) NOT NULL UNIQUE,
        Contrasena NVARCHAR(255) NOT NULL, -- Hash de la contraseña
        Rol NVARCHAR(50) NOT NULL DEFAULT 'Empleado', -- Empleado, Supervisor, Administrador
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaUltimoAcceso DATETIME NULL,
        Observaciones NVARCHAR(500) NULL
    );
    PRINT 'Tabla Usuarios creada exitosamente.';
END
GO

-- =============================================
-- TABLA: Vehiculos (Historial de vehículos)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehiculos')
BEGIN
    CREATE TABLE Vehiculos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Placa NVARCHAR(10) NOT NULL UNIQUE,
        TipoVehiculo INT NOT NULL, -- 1 = Carro, 2 = Moto
        Marca NVARCHAR(50) NULL,
        Modelo NVARCHAR(50) NULL,
        Color NVARCHAR(30) NULL,
        PropietarioNombre NVARCHAR(200) NULL,
        PropietarioDocumento NVARCHAR(20) NULL,
        PropietarioTelefono NVARCHAR(20) NULL,
        VehiculoFrecuente BIT NOT NULL DEFAULT 0,
        TotalVisitas INT NOT NULL DEFAULT 0,
        FechaPrimeraVisita DATETIME NULL,
        FechaUltimaVisita DATETIME NULL,
        Observaciones NVARCHAR(500) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL
    );
    PRINT 'Tabla Vehiculos creada exitosamente.';
END
GO

-- =============================================
-- TABLA: Tarifas (Configuración de precios)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tarifas')
BEGIN
    CREATE TABLE Tarifas (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TipoVehiculo INT NOT NULL, -- 1 = Carro, 2 = Moto
        Nombre NVARCHAR(100) NOT NULL, -- Ej: "Hora Carro", "Minuto Moto"
        TipoCobro NVARCHAR(20) NOT NULL, -- PorHora, PorMinuto, Fijo
        Monto DECIMAL(18,2) NOT NULL,
        TiempoMinimo INT NULL, -- Minutos mínimos (para cobro fijo)
        TiempoMaximo INT NULL, -- Minutos máximos (para descuentos)
        DescuentoPorcentaje DECIMAL(5,2) NULL DEFAULT 0, -- Descuento aplicable
        Activo BIT NOT NULL DEFAULT 1,
        FechaInicio DATETIME NOT NULL DEFAULT GETDATE(),
        FechaFin DATETIME NULL,
        Observaciones NVARCHAR(500) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        UsuarioCreacionId INT NULL,
        FOREIGN KEY (UsuarioCreacionId) REFERENCES Usuarios(Id)
    );
    PRINT 'Tabla Tarifas creada exitosamente.';
END
GO

-- =============================================
-- TABLA: RegistrosParqueo (Entradas y salidas)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RegistrosParqueo')
BEGIN
    CREATE TABLE RegistrosParqueo (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        VehiculoId INT NOT NULL,
        CodigoBarras NVARCHAR(50) NOT NULL UNIQUE,
        FechaEntrada DATETIME NOT NULL,
        FechaSalida DATETIME NULL,
        TiempoParqueadoMinutos INT NULL, -- Calculado al momento de salida
        MontoTotal DECIMAL(18,2) NULL,
        DescuentoAplicado DECIMAL(18,2) NULL DEFAULT 0,
        MontoFinal DECIMAL(18,2) NULL, -- MontoTotal - DescuentoAplicado
        Activo BIT NOT NULL DEFAULT 1, -- True = aún en el parqueadero
        UsuarioEntradaId INT NULL, -- Usuario que registró la entrada
        UsuarioSalidaId INT NULL, -- Usuario que registró la salida
        ObservacionesEntrada NVARCHAR(500) NULL,
        ObservacionesSalida NVARCHAR(500) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL,
        FOREIGN KEY (VehiculoId) REFERENCES Vehiculos(Id),
        FOREIGN KEY (UsuarioEntradaId) REFERENCES Usuarios(Id),
        FOREIGN KEY (UsuarioSalidaId) REFERENCES Usuarios(Id)
    );
    PRINT 'Tabla RegistrosParqueo creada exitosamente.';
END
GO

-- =============================================
-- TABLA: Pagos (Movimientos de pagos)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pagos')
BEGIN
    CREATE TABLE Pagos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RegistroParqueoId INT NOT NULL,
        Monto DECIMAL(18,2) NOT NULL,
        MetodoPago NVARCHAR(50) NOT NULL, -- Efectivo, Tarjeta, Transferencia, Nequi, Daviplata
        ReferenciaPago NVARCHAR(100) NULL, -- Número de referencia, transacción, etc.
        EstadoPago NVARCHAR(20) NOT NULL DEFAULT 'Completado', -- Completado, Pendiente, Cancelado, Reembolsado
        FechaPago DATETIME NOT NULL DEFAULT GETDATE(),
        UsuarioPagoId INT NULL, -- Usuario que procesó el pago
        Observaciones NVARCHAR(500) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (RegistroParqueoId) REFERENCES RegistrosParqueo(Id),
        FOREIGN KEY (UsuarioPagoId) REFERENCES Usuarios(Id)
    );
    PRINT 'Tabla Pagos creada exitosamente.';
END
GO

-- =============================================
-- TABLA: MovimientosCaja (Control de caja)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MovimientosCaja')
BEGIN
    CREATE TABLE MovimientosCaja (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TipoMovimiento NVARCHAR(20) NOT NULL, -- Ingreso, Egreso, Apertura, Cierre
        Concepto NVARCHAR(200) NOT NULL,
        Monto DECIMAL(18,2) NOT NULL,
        MetodoPago NVARCHAR(50) NULL, -- Efectivo, Tarjeta, etc.
        PagoId INT NULL, -- Si está relacionado con un pago
        UsuarioId INT NOT NULL,
        Observaciones NVARCHAR(500) NULL,
        FechaMovimiento DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (PagoId) REFERENCES Pagos(Id),
        FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
    );
    PRINT 'Tabla MovimientosCaja creada exitosamente.';
END
GO

-- =============================================
-- TABLA: CierresCaja (Cierres diarios)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CierresCaja')
BEGIN
    CREATE TABLE CierresCaja (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FechaCierre DATE NOT NULL,
        FechaApertura DATETIME NOT NULL,
        FechaCierreReal DATETIME NOT NULL DEFAULT GETDATE(),
        MontoInicial DECIMAL(18,2) NOT NULL DEFAULT 0,
        MontoEfectivo DECIMAL(18,2) NOT NULL DEFAULT 0,
        MontoTarjeta DECIMAL(18,2) NOT NULL DEFAULT 0,
        MontoTransferencia DECIMAL(18,2) NOT NULL DEFAULT 0,
        MontoTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
        MontoEsperado DECIMAL(18,2) NULL, -- Monto calculado según registros
        Diferencia DECIMAL(18,2) NULL, -- Diferencia entre esperado y real
        TotalRegistros INT NOT NULL DEFAULT 0,
        TotalCarros INT NOT NULL DEFAULT 0,
        TotalMotos INT NOT NULL DEFAULT 0,
        UsuarioId INT NOT NULL,
        Observaciones NVARCHAR(500) NULL,
        Estado NVARCHAR(20) NOT NULL DEFAULT 'Abierto', -- Abierto, Cerrado, Revisado
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
    );
    PRINT 'Tabla CierresCaja creada exitosamente.';
END
GO

-- =============================================
-- TABLA: Configuracion (Configuración general)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Configuracion')
BEGIN
    CREATE TABLE Configuracion (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Clave NVARCHAR(100) NOT NULL UNIQUE,
        Valor NVARCHAR(500) NOT NULL,
        Tipo NVARCHAR(50) NULL, -- String, Number, Boolean, Date
        Descripcion NVARCHAR(500) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL,
        UsuarioActualizacionId INT NULL,
        FOREIGN KEY (UsuarioActualizacionId) REFERENCES Usuarios(Id)
    );
    PRINT 'Tabla Configuracion creada exitosamente.';
END
GO

-- =============================================
-- CREAR ÍNDICES PARA MEJORAR RENDIMIENTO
-- =============================================

-- Índices para Vehiculos
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculos_Placa')
    CREATE UNIQUE INDEX IX_Vehiculos_Placa ON Vehiculos(Placa);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculos_TipoVehiculo')
    CREATE INDEX IX_Vehiculos_TipoVehiculo ON Vehiculos(TipoVehiculo);
GO

-- Índices para RegistrosParqueo
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RegistrosParqueo_VehiculoId')
    CREATE INDEX IX_RegistrosParqueo_VehiculoId ON RegistrosParqueo(VehiculoId);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RegistrosParqueo_Activo')
    CREATE INDEX IX_RegistrosParqueo_Activo ON RegistrosParqueo(Activo);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RegistrosParqueo_CodigoBarras')
    CREATE UNIQUE INDEX IX_RegistrosParqueo_CodigoBarras ON RegistrosParqueo(CodigoBarras);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RegistrosParqueo_FechaEntrada')
    CREATE INDEX IX_RegistrosParqueo_FechaEntrada ON RegistrosParqueo(FechaEntrada);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RegistrosParqueo_FechaSalida')
    CREATE INDEX IX_RegistrosParqueo_FechaSalida ON RegistrosParqueo(FechaSalida);
GO

-- Índices para Pagos
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Pagos_RegistroParqueoId')
    CREATE INDEX IX_Pagos_RegistroParqueoId ON Pagos(RegistroParqueoId);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Pagos_FechaPago')
    CREATE INDEX IX_Pagos_FechaPago ON Pagos(FechaPago);
GO

-- Índices para MovimientosCaja
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MovimientosCaja_FechaMovimiento')
    CREATE INDEX IX_MovimientosCaja_FechaMovimiento ON MovimientosCaja(FechaMovimiento);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MovimientosCaja_UsuarioId')
    CREATE INDEX IX_MovimientosCaja_UsuarioId ON MovimientosCaja(UsuarioId);
GO

-- Índices para CierresCaja
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CierresCaja_FechaCierre')
    CREATE INDEX IX_CierresCaja_FechaCierre ON CierresCaja(FechaCierre);
GO

-- =============================================
-- INSERTAR DATOS INICIALES
-- =============================================

-- Insertar usuario administrador por defecto
IF NOT EXISTS (SELECT * FROM Usuarios WHERE Usuario = 'admin')
BEGIN
    INSERT INTO Usuarios (Nombre, Apellido, Documento, Usuario, Contrasena, Rol, Activo)
    VALUES ('Administrador', 'Sistema', '123456789', 'admin', 
            '$2a$11$KZ8Z8Z8Z8Z8Z8Z8Z8Z8Z8O', -- Hash de ejemplo, cambiar en producción
            'Administrador', 1);
    PRINT 'Usuario administrador creado (usuario: admin, contraseña: cambiar en producción).';
END
GO

-- Insertar tarifas por defecto
IF NOT EXISTS (SELECT * FROM Tarifas WHERE TipoVehiculo = 1 AND TipoCobro = 'PorHora')
BEGIN
    INSERT INTO Tarifas (TipoVehiculo, Nombre, TipoCobro, Monto, Activo)
    VALUES 
        (1, 'Tarifa por Hora - Carro', 'PorHora', 2000.00, 1),
        (2, 'Tarifa por Hora - Moto', 'PorHora', 1000.00, 1),
        (1, 'Tarifa por Minuto - Carro', 'PorMinuto', 35.00, 1),
        (2, 'Tarifa por Minuto - Moto', 'PorMinuto', 18.00, 1);
    PRINT 'Tarifas por defecto creadas.';
END
GO

-- Insertar configuración inicial
IF NOT EXISTS (SELECT * FROM Configuracion WHERE Clave = 'NombreParqueadero')
BEGIN
    INSERT INTO Configuracion (Clave, Valor, Tipo, Descripcion)
    VALUES 
        ('NombreParqueadero', 'Parqueadero Montería', 'String', 'Nombre del parqueadero'),
        ('Direccion', 'Montería, Colombia', 'String', 'Dirección del parqueadero'),
        ('Telefono', '', 'String', 'Teléfono de contacto'),
        ('Email', '', 'String', 'Email de contacto'),
        ('Moneda', 'COP', 'String', 'Código de moneda'),
        ('SimboloMoneda', '$', 'String', 'Símbolo de moneda'),
        ('RedondearMinutos', 'true', 'Boolean', 'Redondear minutos hacia arriba'),
        ('TiempoMinimoCobro', '15', 'Number', 'Tiempo mínimo en minutos para cobrar');
    PRINT 'Configuración inicial creada.';
END
GO

-- =============================================
-- CREAR VISTAS ÚTILES
-- =============================================

-- Vista: Resumen de vehículos activos
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_VehiculosActivos')
    DROP VIEW VW_VehiculosActivos;
GO

CREATE VIEW VW_VehiculosActivos AS
SELECT 
    r.Id AS RegistroId,
    r.CodigoBarras,
    v.Placa,
    v.TipoVehiculo,
    CASE v.TipoVehiculo 
        WHEN 1 THEN 'Carro' 
        WHEN 2 THEN 'Moto' 
    END AS TipoVehiculoTexto,
    v.Marca,
    v.Modelo,
    v.Color,
    r.FechaEntrada,
    DATEDIFF(MINUTE, r.FechaEntrada, GETDATE()) AS MinutosParqueado,
    ue.Nombre + ' ' + ue.Apellido AS UsuarioEntrada,
    r.ObservacionesEntrada
FROM RegistrosParqueo r
INNER JOIN Vehiculos v ON r.VehiculoId = v.Id
LEFT JOIN Usuarios ue ON r.UsuarioEntradaId = ue.Id
WHERE r.Activo = 1;
GO

-- Vista: Resumen de pagos del día
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_ResumenPagosDia')
    DROP VIEW VW_ResumenPagosDia;
GO

CREATE VIEW VW_ResumenPagosDia AS
SELECT 
    CAST(p.FechaPago AS DATE) AS Fecha,
    COUNT(*) AS TotalPagos,
    SUM(p.Monto) AS MontoTotal,
    SUM(CASE WHEN p.MetodoPago = 'Efectivo' THEN p.Monto ELSE 0 END) AS MontoEfectivo,
    SUM(CASE WHEN p.MetodoPago = 'Tarjeta' THEN p.Monto ELSE 0 END) AS MontoTarjeta,
    SUM(CASE WHEN p.MetodoPago = 'Transferencia' THEN p.Monto ELSE 0 END) AS MontoTransferencia,
    SUM(CASE WHEN p.MetodoPago IN ('Nequi', 'Daviplata') THEN p.Monto ELSE 0 END) AS MontoDigital
FROM Pagos p
WHERE p.EstadoPago = 'Completado'
GROUP BY CAST(p.FechaPago AS DATE);
GO

-- Vista: Historial completo de vehículos
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_HistorialVehiculos')
    DROP VIEW VW_HistorialVehiculos;
GO

CREATE VIEW VW_HistorialVehiculos AS
SELECT 
    v.Id AS VehiculoId,
    v.Placa,
    v.TipoVehiculo,
    CASE v.TipoVehiculo 
        WHEN 1 THEN 'Carro' 
        WHEN 2 THEN 'Moto' 
    END AS TipoVehiculoTexto,
    v.Marca,
    v.Modelo,
    v.Color,
    v.PropietarioNombre,
    v.TotalVisitas,
    v.FechaPrimeraVisita,
    v.FechaUltimaVisita,
    COUNT(r.Id) AS TotalRegistros,
    SUM(CASE WHEN r.Activo = 1 THEN 1 ELSE 0 END) AS RegistrosActivos,
    SUM(r.MontoFinal) AS MontoTotalPagado
FROM Vehiculos v
LEFT JOIN RegistrosParqueo r ON v.Id = r.VehiculoId
GROUP BY v.Id, v.Placa, v.TipoVehiculo, v.Marca, v.Modelo, v.Color, 
         v.PropietarioNombre, v.TotalVisitas, v.FechaPrimeraVisita, v.FechaUltimaVisita;
GO

PRINT 'Vistas creadas exitosamente.';
GO

-- =============================================
-- CREAR PROCEDIMIENTOS ALMACENADOS ÚTILES
-- =============================================

-- Procedimiento: Obtener vehículos activos
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_ObtenerVehiculosActivos')
    DROP PROCEDURE SP_ObtenerVehiculosActivos;
GO

CREATE PROCEDURE SP_ObtenerVehiculosActivos
AS
BEGIN
    SELECT * FROM VW_VehiculosActivos
    ORDER BY FechaEntrada DESC;
END
GO

-- Procedimiento: Registrar entrada de vehículo
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_RegistrarEntrada')
    DROP PROCEDURE SP_RegistrarEntrada;
GO

CREATE PROCEDURE SP_RegistrarEntrada
    @Placa NVARCHAR(10),
    @TipoVehiculo INT,
    @CodigoBarras NVARCHAR(50),
    @UsuarioId INT,
    @Observaciones NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Buscar o crear vehículo
        DECLARE @VehiculoId INT;
        
        SELECT @VehiculoId = Id FROM Vehiculos WHERE Placa = @Placa;
        
        IF @VehiculoId IS NULL
        BEGIN
            INSERT INTO Vehiculos (Placa, TipoVehiculo, FechaPrimeraVisita, FechaUltimaVisita, TotalVisitas)
            VALUES (@Placa, @TipoVehiculo, GETDATE(), GETDATE(), 1);
            
            SET @VehiculoId = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE Vehiculos 
            SET TotalVisitas = TotalVisitas + 1,
                FechaUltimaVisita = GETDATE()
            WHERE Id = @VehiculoId;
        END
        
        -- Crear registro de parqueo
        INSERT INTO RegistrosParqueo (VehiculoId, CodigoBarras, FechaEntrada, Activo, UsuarioEntradaId, ObservacionesEntrada)
        VALUES (@VehiculoId, @CodigoBarras, GETDATE(), 1, @UsuarioId, @Observaciones);
        
        DECLARE @RegistroId INT = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        
        SELECT @RegistroId AS RegistroId, @VehiculoId AS VehiculoId;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Procedimiento: Registrar salida y calcular monto
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_RegistrarSalida')
    DROP PROCEDURE SP_RegistrarSalida;
GO

CREATE PROCEDURE SP_RegistrarSalida
    @RegistroId INT,
    @UsuarioId INT,
    @MontoTotal DECIMAL(18,2),
    @Descuento DECIMAL(18,2) = 0,
    @Observaciones NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @FechaEntrada DATETIME;
        DECLARE @FechaSalida DATETIME = GETDATE();
        DECLARE @Minutos INT;
        DECLARE @MontoFinal DECIMAL(18,2);
        
        SELECT @FechaEntrada = FechaEntrada 
        FROM RegistrosParqueo 
        WHERE Id = @RegistroId AND Activo = 1;
        
        IF @FechaEntrada IS NULL
        BEGIN
            THROW 50001, 'Registro no encontrado o ya cerrado', 1;
        END
        
        SET @Minutos = DATEDIFF(MINUTE, @FechaEntrada, @FechaSalida);
        SET @MontoFinal = @MontoTotal - @Descuento;
        
        UPDATE RegistrosParqueo
        SET FechaSalida = @FechaSalida,
            TiempoParqueadoMinutos = @Minutos,
            MontoTotal = @MontoTotal,
            DescuentoAplicado = @Descuento,
            MontoFinal = @MontoFinal,
            Activo = 0,
            UsuarioSalidaId = @UsuarioId,
            ObservacionesSalida = @Observaciones,
            FechaActualizacion = GETDATE()
        WHERE Id = @RegistroId;
        
        COMMIT TRANSACTION;
        
        SELECT @RegistroId AS RegistroId, @MontoFinal AS MontoFinal;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Procedimientos almacenados creados exitosamente.';
GO

-- =============================================
-- FIN DEL SCRIPT
-- =============================================
PRINT '========================================';
PRINT 'Base de datos creada exitosamente!';
PRINT '========================================';
PRINT '';
PRINT 'Tablas creadas:';
PRINT '  - Usuarios';
PRINT '  - Vehiculos';
PRINT '  - Tarifas';
PRINT '  - RegistrosParqueo';
PRINT '  - Pagos';
PRINT '  - MovimientosCaja';
PRINT '  - CierresCaja';
PRINT '  - Configuracion';
PRINT '';
PRINT 'Vistas creadas:';
PRINT '  - VW_VehiculosActivos';
PRINT '  - VW_ResumenPagosDia';
PRINT '  - VW_HistorialVehiculos';
PRINT '';
PRINT 'Procedimientos almacenados creados:';
PRINT '  - SP_ObtenerVehiculosActivos';
PRINT '  - SP_RegistrarEntrada';
PRINT '  - SP_RegistrarSalida';
PRINT '';
PRINT 'Datos iniciales insertados:';
PRINT '  - Usuario administrador (admin)';
PRINT '  - Tarifas por defecto';
PRINT '  - Configuración inicial';
PRINT '';
PRINT 'NOTA: Cambiar la contraseña del usuario administrador en producción!';
GO
