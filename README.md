# AppSlider - Firmware 
The open source firmware for motorized camera sliders

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
