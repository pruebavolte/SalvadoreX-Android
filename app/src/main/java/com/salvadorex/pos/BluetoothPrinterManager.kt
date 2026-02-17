package com.salvadorex.pos

import android.annotation.SuppressLint
import android.bluetooth.BluetoothAdapter
import android.bluetooth.BluetoothDevice
import android.bluetooth.BluetoothManager
import android.bluetooth.BluetoothSocket
import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.os.Build
import org.json.JSONArray
import org.json.JSONObject
import java.io.IOException
import java.io.OutputStream
import java.util.UUID

class BluetoothPrinterManager(private val context: Context) {

    companion object {
        val SPP_UUID: UUID = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB")
    }

    private val bluetoothManager: BluetoothManager? =
        context.getSystemService(Context.BLUETOOTH_SERVICE) as? BluetoothManager
    private val bluetoothAdapter: BluetoothAdapter? = bluetoothManager?.adapter

    private var connectedSocket: BluetoothSocket? = null
    private var outputStream: OutputStream? = null
    private var connectedDeviceAddress: String? = null

    private var discoveryReceiver: BroadcastReceiver? = null
    private val discoveredDevices = mutableListOf<JSONObject>()

    @SuppressLint("MissingPermission")
    fun scanDevices(onComplete: (JSONArray) -> Unit) {
        val devices = JSONArray()

        if (bluetoothAdapter == null || !bluetoothAdapter.isEnabled) {
            onComplete(devices)
            return
        }

        try {
            val pairedDevices = bluetoothAdapter.bondedDevices
            pairedDevices?.forEach { device ->
                devices.put(deviceToJson(device, paired = true))
            }
        } catch (e: SecurityException) {
            onComplete(devices)
            return
        }

        discoveredDevices.clear()

        try {
            unregisterDiscoveryReceiver()

            discoveryReceiver = object : BroadcastReceiver() {
                override fun onReceive(ctx: Context?, intent: Intent?) {
                    when (intent?.action) {
                        BluetoothDevice.ACTION_FOUND -> {
                            val device: BluetoothDevice? = if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                                intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE, BluetoothDevice::class.java)
                            } else {
                                @Suppress("DEPRECATION")
                                intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE)
                            }
                            device?.let {
                                val json = deviceToJson(it, paired = false)
                                discoveredDevices.add(json)
                            }
                        }
                        BluetoothAdapter.ACTION_DISCOVERY_FINISHED -> {
                            discoveredDevices.forEach { devices.put(it) }
                            unregisterDiscoveryReceiver()
                            onComplete(devices)
                        }
                    }
                }
            }

            val filter = IntentFilter().apply {
                addAction(BluetoothDevice.ACTION_FOUND)
                addAction(BluetoothAdapter.ACTION_DISCOVERY_FINISHED)
            }
            context.registerReceiver(discoveryReceiver, filter)
            bluetoothAdapter.startDiscovery()

            android.os.Handler(android.os.Looper.getMainLooper()).postDelayed({
                try {
                    if (bluetoothAdapter.isDiscovering) {
                        bluetoothAdapter.cancelDiscovery()
                    }
                } catch (_: SecurityException) {}
            }, 8000)

        } catch (e: SecurityException) {
            onComplete(devices)
        }
    }

    @SuppressLint("MissingPermission")
    fun connectToDevice(address: String): Boolean {
        if (bluetoothAdapter == null) return false

        disconnect()

        try {
            if (bluetoothAdapter.isDiscovering) {
                bluetoothAdapter.cancelDiscovery()
            }
        } catch (_: SecurityException) {}

        val device: BluetoothDevice = try {
            bluetoothAdapter.getRemoteDevice(address)
        } catch (e: IllegalArgumentException) {
            return false
        }

        return try {
            val socket = device.createRfcommSocketToServiceRecord(SPP_UUID)
            socket.connect()
            connectedSocket = socket
            outputStream = socket.outputStream
            connectedDeviceAddress = address
            true
        } catch (e: IOException) {
            try {
                val fallbackSocket = device.javaClass
                    .getMethod("createRfcommSocket", Int::class.javaPrimitiveType)
                    .invoke(device, 1) as BluetoothSocket
                fallbackSocket.connect()
                connectedSocket = fallbackSocket
                outputStream = fallbackSocket.outputStream
                connectedDeviceAddress = address
                true
            } catch (e2: Exception) {
                disconnect()
                false
            }
        } catch (e: SecurityException) {
            false
        }
    }

    fun printData(address: String, data: String): Boolean {
        if (connectedDeviceAddress != address || outputStream == null) {
            if (!connectToDevice(address)) {
                return false
            }
        }

        return try {
            val bytes = data.toByteArray(Charsets.UTF_8)
            outputStream?.write(bytes)
            outputStream?.flush()
            true
        } catch (e: IOException) {
            disconnect()
            false
        }
    }

    fun disconnect() {
        try {
            outputStream?.close()
        } catch (_: IOException) {}
        try {
            connectedSocket?.close()
        } catch (_: IOException) {}
        outputStream = null
        connectedSocket = null
        connectedDeviceAddress = null
    }

    fun isConnected(): Boolean {
        return connectedSocket?.isConnected == true
    }

    @SuppressLint("MissingPermission")
    private fun deviceToJson(device: BluetoothDevice, paired: Boolean): JSONObject {
        val name = try {
            device.name ?: "Unknown"
        } catch (_: SecurityException) {
            "Unknown"
        }

        val type = try {
            when (device.bluetoothClass?.majorDeviceClass) {
                0x0600 -> "printer"
                else -> "unknown"
            }
        } catch (_: Exception) {
            "unknown"
        }

        return JSONObject().apply {
            put("name", name)
            put("address", device.address)
            put("paired", paired)
            put("type", type)
            put("connected", device.address == connectedDeviceAddress && isConnected())
        }
    }

    private fun unregisterDiscoveryReceiver() {
        discoveryReceiver?.let {
            try {
                context.unregisterReceiver(it)
            } catch (_: IllegalArgumentException) {}
            discoveryReceiver = null
        }
    }
}
