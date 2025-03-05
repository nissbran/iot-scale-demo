#/bin/bash

# This script is used to configure the partitions of a compulab device which have run the initial cl-deploy.
# Pre-requisites:
# - The device has been installed with the initial cl-deploy.
# - The device is booted with the live debian image and the filesystem is unmounted.


# This script is based on the fact that the device has a single partition with the following layout:
# /dev/mmcblk2p1 
# /dev/mmcblk2p2  
#
# parted print:
# Number  Start   End    Size   File system  Name     Flags
#  1      4194kB  128MB  124MB  ext4         primary
#  2      133MB   125GB  125GB  ext4

DEVICE="/dev/mmcblk2"
PARTITION="/dev/mmcblk2p2"
OS_COPY1_END_SIZE="10GiB"
OS_COPY2_END_SIZE="20GiB"
DATA_COPY_END_SIZE="110GiB"
FILE_SYSTEM="ext4"
RESIZE2FS_SIZE="10G"

# PART 1: Resize the root partition ----------------------------

# Check for errors
sudo e2fsck -f -y $PARTITION
 
# Resize the file system to fit the partition
sudo resize2fs $PARTITION $RESIZE2FS_SIZE

# Resize the partition
sudo parted $DEVICE resizepart 2 $OS_COPY1_END_SIZE

# Resize the file system to fill the partition
sudo resize2fs $PARTITION

# Check for errors
sudo e2fsck -f -y $PARTITION

# PART 2: Create the second copy of the root partition ---------

# Create a new partition for the second copy of the root partition
sudo parted $DEVICE mkpart primary $FILE_SYSTEM $OS_COPY1_END_SIZE $OS_COPY2_END_SIZE

# PART 3: Create the data partition ----------------------------

# Create the data partition
sudo parted $DEVICE mkpart primary $FILE_SYSTEM $OS_COPY2_END_SIZE $DATA_COPY_END_SIZE

# PART 4: Create the file system for the data partition --------
sudo mkfs.ext4 /dev/mmcblk2p4

sudo mkdir -p /data/storage

# add the following line to /etc/fstab
# /dev/mmcblk2p4       /data/storage        ext4       defaults              0   0


# Reboot the device
sudo reboot

