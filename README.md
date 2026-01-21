# SalvadoreX POS - Android

Aplicacion movil de Punto de Venta para Android desarrollada con .NET MAUI.

## Caracteristicas

- **Punto de Venta (POS)**: Interfaz tactil para ventas rapidas
- **Inventario**: Gestion completa de productos y stock
- **Clientes**: Base de datos de clientes
- **Historial de Ventas**: Registro detallado de transacciones
- **Sincronizacion**: Sincronizacion automatica con Supabase cuando hay internet
- **Modo Offline**: Funciona 100% sin conexion a internet

## Requisitos

- Visual Studio 2022 con workload ".NET MAUI"
- Android SDK 24.0 o superior
- .NET 8.0 SDK

## Instalacion

1. Clonar el repositorio:
```bash
git clone https://github.com/pruebavolte/SalvadoreX-Android.git
```

2. Abrir `SalvadoreXAndroid.sln` en Visual Studio 2022

3. Seleccionar un dispositivo Android o emulador

4. Presionar F5 para compilar y ejecutar

## Generar APK

### Desde Visual Studio:

1. Cambiar configuracion a **Release**
2. Click derecho en el proyecto > **Publish**
3. Seleccionar **Ad Hoc** o **Google Play**
4. El APK se genera en `bin/Release/net8.0-android/`

### Desde linea de comandos:

```bash
dotnet publish -f net8.0-android -c Release
```

El APK firmado estara en:
`bin/Release/net8.0-android/publish/com.salvadorex.pos-Signed.apk`

## Estructura del Proyecto

```
SalvadoreXAndroid/
├── Models/           # Modelos de datos (Product, Customer, Sale, etc.)
├── Data/             # Servicio de base de datos SQLite
├── Services/         # Servicios (Sync, etc.)
├── ViewModels/       # ViewModels para MVVM
├── Views/            # Paginas XAML
├── Converters/       # Convertidores de datos
├── Resources/        # Estilos, colores, imagenes
└── Platforms/        # Codigo especifico de plataforma
    └── Android/      # Configuracion Android
```

## Base de Datos

La aplicacion usa SQLite para almacenamiento local. La base de datos se crea automaticamente en el primer inicio con:

- Categorias predeterminadas
- Configuracion inicial del negocio

## Sincronizacion

La sincronizacion con Supabase se realiza:
- Automaticamente cada 30 segundos cuando hay conexion
- Manualmente desde la pantalla de Configuracion

## Tecnologias

- **.NET 8.0 MAUI** - Framework multiplataforma
- **SQLite** - Base de datos local
- **MVVM** - Patron de arquitectura
- **Newtonsoft.Json** - Serializacion JSON

## Licencia

Copyright 2025 SalvadoreX. Todos los derechos reservados.
