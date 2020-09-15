# Biofeedback XR

Biofeedback XR is part of the studies developed by [Juscelino Tanaka](mailto:juscelino.tanaka@gmail.com) 
during his Master classes in Computer Science at Federal University of Brazil - UFAM.

## What is it?
Biofeedback XR is the visual part of a biofeedback sensor system that tracks vital signals
(e.g. muscle activities, heartbeat rate, etc). This project uses Bluetooth Low Energy (BLE) to 
communicate the BLE sensor device with the visual part, which is the 3D application present
in this repository.

You can find more about the BLE device on [its repository](#).

### About this repository
This repository hosts the 3D application which display some visual representation of the data
collected by the sensor. The application was developed Unity3D and it was meant to run on
Oculus Quest, which is XR device used during this experiment.

## How to Setup

### Unity3D
This project was built using the Unity3D Engine.

Download the Unity Hub and install the 2020.1.3f1 editor together with the Android packages (make
sure to install Android SDK and NDK in case you do not have it set yet). Theoretically, any
2020.1.X editor should run the project properly, but in case you have any trouble, try that
specific version. Open this project using the proper version. Make sure you open the project
for Android platform either by switching platform later or while opening the project.

After you open the project you should see some compiling errors. That's because you need to import
a 3rd party library called [Bluetooth LE for iOS, tvOS and Android](https://assetstore.unity.com/packages/tools/network/bluetooth-le-for-ios-tvos-and-android-26661).
Unfortunately, since it's a paid library I cannot make it available publicly in here.

### 3rd Party Dependency

This project depends on [Bluetooth LE for iOS, tvOS and Android](https://assetstore.unity.com/packages/tools/network/bluetooth-le-for-ios-tvos-and-android-26661)
library which is available at [Unity Asset Store](https://assetstore.unity.com/).

You just need to import the two .cs files that are inside the Plugins folder:
- BluetoothDeviceScript
- BluetoothHardwareInterface

 You can also import the other files, but they are mostly example. If this maybe you should
 import the whole Plugins folder, if the plugin developer change something internally, then
 you would need to import the whole Plugins folder.
 
 ## Authors
 This project was developed by Juscelino Tanaka, with the support of a lot of professors that
 will be listed here later.
 
 ## Sponsors
 This study was financed in part by the Coordenação de Aperfeiçoamento de Pessoal de Nível
 Superior - Brasil (CAPES) - Finance Code 001.