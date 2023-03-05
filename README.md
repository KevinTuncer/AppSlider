# AppSlider

The AppSlider is an open source computer controlled camera slider.

# AppSlider - Firmware

The open source firmware for motorized camera sliders. The default configuration regards to an Arduino Mega 2560.

## Control your camera on a slider with Shoot-Move-Shoot

This firmware is a modified version of the Repetier-Firmware 0.92.9 https://github.com/repetier/Repetier-Firmware.
A M750 command was added to make it capable of Shoot-Move-Shoot.
This version is configured for the USB and Bluetooth controlled AppSlider by TuncerTec.
Here you can find the building instructions: https://tuncertec.de/appslider-2/appslider-2-0/instructions/

For other sliders you can change the parameters in the configuration.h file.

### M750 command for Shoot-Move-Shoot

```cpp
R[long]     // Fokus-Time | default: 0
S[long]     // Shoot-Time  | default: 0
T[Boolean]  // true to press focus and shoot button at the same time to release the shutter | default: true
I[float]    // Upstream delay
J[float]    // Downstream delay

/* For a Shoot-Move-Shoot movement only two of the following three parameters need to set. */
X[float]    // Final position in X direction
E[float]    // Delta movement, difference of movement between each shot
P[long];    // Total shot images + 1
```

### For controlling

Use the manual input of the Software Repetier-Host or the camera slider spezific software by TuncerTec for Android and Windows: https://tuncertec.de/software/

# AppSlider - Windows

The open source Windows software to control the AppSlider. The software was designt in 2015 and this version is only avalable in German.

## Usage

1. If the device drivers are installed you will find the COM port of the AppSlider device. Click "Verbinden" to connect your device. Alternatively, you can click "Automatisch" to find the right COM automatically.
2. Click "Nullen" to drive the slider to the initial position.
3. Use the input of "Geschwindigkeit" and "Position" to define the speed and position of a movement.
4. Click on "Fahren" to execute the movement.
5. "Erweiterete Ansicht" gives you an extended interface. You can send G-Code directly, see the log and are able to create special buttens at "Ablauf erstellen". These buttons execute a manually defined list of orders.

### Shoot move shoot

The shoot move shoot function is used to create a time-lapse while moving the camera. The camera takes a picture, moves a bit forward and takes a new picture. This repeats a defined number of times. By combining all pictures to a video you get the time-lapse effect.

1. To execute a shoot move shoot function you have to create a button in the advanced settings by clicking on "Erweiterte Ansicht" and "Ablauf erstellen".
2. It is recommended to drive to the initial position first. You can add this order by clicking on "Nullen".
3. Then you can click on "Shoot Move Shoot" to add this order and a new window appears.
4. The "Kamera-Parameter" defines how long the focus and shot button of the camera should be pressed and how much time should be waited before and after these two actions.
5. The "Bewegungs-Parameter" defines the movement. It can be defined by setting two of three parameters. The absolut end position (Position), the moved distance after each shot (dx) and the number of total shots (Anzahl). The speed between the shots is defined at the input of "Geschwindigkeit".
6. The resulting duration for a shoot move shoot movement will be calculated at "Errechnete Gesamtdauer". After clicking "Add" the window closes and by clicking on "OK" you confirm the settings.
7. The created button appears on the right of the main window. On each click the defined list of orders will be executed.

# AppSlider - Android

The Android version is in English and not open source, yet.

# Contact

Feel free to contact me if you want to build on that project.
