REM Omnideck Emulator init script

REM Example to show key presses etc
REM ".\OmnideckEmulator.exe" --printkeypress 1 --printcoordinates 1

REM Example that DISABLES the XBox 360 gamepad as input (it is enabled by default) and does not print any keys/coordinates
REM also using a max movement speed of 1.0 m/s. If you press A on the XBox 360 controller the speed will double.
".\OmnideckEmulator.exe" --usexbox360gamepad 0 --printkeypress 0 --printcoordinates 0 --movespeed 1.0
