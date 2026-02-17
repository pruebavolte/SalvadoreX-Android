package com.salvadorex.pos

import android.annotation.SuppressLint
import android.provider.Settings
import android.webkit.JavascriptInterface
import org.json.JSONObject

class WebAppInterface(private val activity: MainActivity) {

    private var pendingAction: (() -> Unit)? = null

    @get:JavascriptInterface
    val isAndroidApp: Boolean = true

    fun onPermissionsDenied() {
        val message = JSONObject().apply {
            put("type", "bluetoothPermissionDenied")
        }
        postMessageToWebView(message.toString())
    }

    @JavascriptInterface
    fun scanBluetoothPrinters() {
        if (!activity.hasBluetoothPermissions()) {
            pendingAction = { scanBluetoothPrinters() }
            activity.requestBluetoothPermissions()
            return
        }

        activity.bluetoothManager.scanDevices { devices ->
            val message = JSONObject().apply {
                put("type", "bluetoothDevices")
                put("devices", devices)
            }
            postMessageToWebView(message.toString())
        }
    }

    @JavascriptInterface
    fun connectBluetoothPrinter(deviceId: String) {
        if (!activity.hasBluetoothPermissions()) {
            pendingAction = { connectBluetoothPrinter(deviceId) }
            activity.requestBluetoothPermissions()
            return
        }

        Thread {
            val success = activity.bluetoothManager.connectToDevice(deviceId)
            val message = JSONObject().apply {
                put("type", "bluetoothConnectResult")
                put("deviceId", deviceId)
                put("success", success)
            }
            postMessageToWebView(message.toString())
        }.start()
    }

    @JavascriptInterface
    fun printBluetooth(deviceId: String, data: String) {
        if (!activity.hasBluetoothPermissions()) {
            pendingAction = { printBluetooth(deviceId, data) }
            activity.requestBluetoothPermissions()
            return
        }

        Thread {
            val success = activity.bluetoothManager.printData(deviceId, data)
            val message = JSONObject().apply {
                put("type", "bluetoothPrintResult")
                put("deviceId", deviceId)
                put("success", success)
            }
            postMessageToWebView(message.toString())
        }.start()
    }

    @SuppressLint("HardwareIds")
    @JavascriptInterface
    fun getDeviceId(): String {
        return try {
            Settings.Secure.getString(
                activity.contentResolver,
                Settings.Secure.ANDROID_ID
            ) ?: "unknown"
        } catch (e: Exception) {
            "unknown"
        }
    }

    fun onPermissionsGranted() {
        pendingAction?.invoke()
        pendingAction = null
    }

    private fun postMessageToWebView(jsonString: String) {
        val escaped = jsonString
            .replace("\\", "\\\\")
            .replace("'", "\\'")
            .replace("\n", "\\n")
            .replace("\r", "\\r")

        activity.webView.post {
            activity.webView.evaluateJavascript(
                "window.postMessage('$escaped', '*');",
                null
            )
        }
    }
}
