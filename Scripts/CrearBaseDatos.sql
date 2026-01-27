-- Script para crear la base de datos del Sistema de Parqueadero
-- Ejecutar este script en SQL Server Management Studio

USE master;
GO

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SistemaParqueadero')
BEGIN
    CREATE DATABASE SistemaParqueadero;
END
GO

USE SistemaParqueadero;
GO

-- Crear la tabla RegistrosParqueo
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RegistrosParqueo')
BEGIN
    CREATE TABLE RegistrosParqueo (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Placa NVARCHAR(10) NOT NULL,
        TipoVehiculo INT NOT NULL, -- 1 = Carro, 2 = Moto
        FechaEntrada DATETIME NOT NULL,
        FechaSalida DATETIME NULL,
        MontoTotal DECIMAL(18,2) NULL,
        Activo BIT NOT NULL DEFAULT 1,
        CodigoBarras NVARCHAR(50) NULL,
        Observaciones NVARCHAR(500) NULL
    );

    -- Crear índices para mejorar el rendimiento
    CREATE INDEX IX_RegistrosParqueo_Placa ON RegistrosParqueo(Placa);
    CREATE INDEX IX_RegistrosParqueo_Activo ON RegistrosParqueo(Activo);
    CREATE INDEX IX_RegistrosParqueo_CodigoBarras ON RegistrosParqueo(CodigoBarras);
    CREATE INDEX IX_RegistrosParqueo_FechaEntrada ON RegistrosParqueo(FechaEntrada);
END
GO

PRINT 'Base de datos creada exitosamente!';
GO
