#/bin/bash

# This script is used to install the iot dependencies on a compulab device.
curl https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb > ./packages-microsoft-prod.deb
sudo apt install ./packages-microsoft-prod.deb

# Install dependencies
sudo apt update
sudo apt install aziot-edge


# TODO: Change this when to real ADU agent exist in the debian 12 packages
curl https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb > ./packages-microsoft-prod.deb
sudo apt install ./packages-microsoft-prod.deb

# Install libssl1.1
# https://packages.debian.org/bullseye/arm64/libssl1.1/download
wget http://security.debian.org/debian-security/pool/updates/main/o/openssl/libssl1.1_1.1.1w-0+deb11u2_arm64.deb
sudo apt install ./libssl1.1_1.1.1w-0+deb11u2_arm64.deb


# Install dependencies
sudo apt update
sudo apt install deviceupdate-agent
