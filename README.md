# SalvadoreX POS - Aplicación Android

Aplicación nativa Android que envuelve la aplicación web SalvadoreX POS en un WebView con soporte para impresoras Bluetooth.

## Requisitos

- Android Studio Arctic Fox o superior
- Android SDK 34
- Android 8.0+ (API 26) en el dispositivo

## Compilar el APK

### Opción 1: Android Studio
1. Abre este directorio en Android Studio
2. Espera a que Gradle sincronice las dependencias
3. Ve a **Build > Build Bundle / APK > Build APK**
4. El APK se genera en: `app/build/outputs/apk/debug/app-debug.apk`

### Opción 2: Línea de comandos
```bash
./gradlew assembleDebug
```
El APK se genera en: `app/build/outputs/apk/debug/app-debug.apk`

### APK de Release (firmado)
```bash
./gradlew assembleRelease
```
Necesitarás configurar un keystore de firma en `app/build.gradle.kts`.

## Cambiar la URL de la aplicación web

Edita `app/build.gradle.kts` y modifica el valor de `WEB_APP_URL`:

```kotlin
buildConfigField("String", "WEB_APP_URL", "\"https://tu-url-aqui.com\"")
```

## Funcionalidades nativas

- **Impresión Bluetooth**: Escanea, conecta e imprime en impresoras térmicas Bluetooth
- **Detección automática**: La app web detecta automáticamente que está en Android
- **Permisos en tiempo real**: Solicita permisos de Bluetooth cuando se necesitan

## Permisos requeridos

- `INTERNET` - Acceso a la aplicación web
- `BLUETOOTH` / `BLUETOOTH_ADMIN` - Gestión de Bluetooth (Android < 12)
- `BLUETOOTH_SCAN` / `BLUETOOTH_CONNECT` - Bluetooth (Android 12+)
- `ACCESS_FINE_LOCATION` - Requerido para escaneo Bluetooth
