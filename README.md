# SalvadoreX POS - Android

Aplicación móvil para Android con WebView.

## Requisitos

- Android 7.0 (API 24) o superior
- 50 MB de espacio libre

## Compilación

```bash
# Compilar APK Release
dotnet publish -f net8.0-android -c Release

# El APK estará en:
# bin/Release/net8.0-android/publish/com.salvadorex.pos-Signed.apk
```

## Estructura

```
├── WebApp/           # Interfaz web embebida
│   ├── index.html
│   ├── app.js
│   └── version.txt
├── Assets/
│   └── webapp/       # Copia de WebApp para Android
├── Services/         # Servicios nativos C#
│   ├── DatabaseService.cs
│   ├── SyncService.cs
│   ├── UpdateService.cs
│   ├── LicensingService.cs
│   └── NativeBridge.cs
├── MainActivity.cs   # Actividad con WebView
├── AndroidManifest.xml
└── SalvadoreXPOS.csproj
```

## Actualizar WebApp

Para actualizar la interfaz desde el repositorio web principal:

1. Descargar `index.html` y `app.js` actualizados
2. Reemplazar en `WebApp/` Y en `Assets/webapp/`
3. Recompilar

## Características

- Offline-first con SQLite local
- Sincronización automática con Supabase
- Licenciamiento por hardware (Android ID)
- Interfaz idéntica a Windows y Web

## Instalación

1. Habilitar "Orígenes desconocidos" en Configuración > Seguridad
2. Instalar el APK
3. Conceder permisos solicitados

## Licencia

Copyright © 2025 SalvadoreX
