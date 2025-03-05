#!/bin/bash

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Ensure that getopt starts from first option if ". <script.sh>" was used.
OPTIND=1

ret_val=0

# Ensure we don't end the user's terminal session if invoked from source (".").
if [[ $0 != "${BASH_SOURCE[0]}" ]]; then
    ret='return'
else
    ret='exit'
fi

# Output formatting.

# Log level: 0=debug, 1=info, 2=warning, 3=error, 4=none
log_level=1

warn() { echo -e "\033[1;33mWarning:\033[0m $*" >&2; }

error() { echo -e "\033[1;31mError:\033[0m $*" >&2; }

header() { echo -e "\e[4m\e[1m\e[1;32m$*\e[0m"; }

bullet() { echo -e "\e[1;34m*\e[0m $*"; }

warn "*************************************************"
warn "*                    WARNING                    *"
warn "*                                               *"
warn "* THIS FILE IS FOR DEMONSTRATION PURPOSES ONLY. *"
warn "* DO NOT USE THIS FOR YOUR REAL PRODUCT UPDATE! *"
warn "*                                               *"
warn "*************************************************"

# Log debug prefix - blue
log_debug_pref="\033[1;30m[D]\033[0m"

# Log info prefix - blue
log_info_pref="\033[1;34m[I]\033[0m"

# Log warning prefix - yellow
log_warn_pref="\033[1;33m[W]\033[0m"

# Log error prefix - red
log_error_pref="\033[1;31m[E]\033[0m"

#
# Files and Folders information
#
workfolder=
firmware_file=/var/lib/adu/helloworld/firmware.json
output_file=
log_file=
result_file=

#
# Device Update specific arguments
#
check_is_installed=
installed_criteria=
do_download_action=
do_install_action=
do_apply_action=
do_cancel_action=


#
# Remaining aguments and parameters
#
PARAMS=

#
# Output, Logs, and Result helper functions.
#
_timestamp=

update_timestamp() {
    # See https://man7.org/linux/man-pages/man1/date.1.html
    _timestamp="$(date +'%Y/%m/%d:%H%M%S')"
}

log_debug() {
    if [ $log_level -gt 0 ]; then
        return
    fi
    log "$log_debug_pref" "$@"
}

log_info() {
    if [ $log_level -gt 1 ]; then
        return
    fi
    log "$log_info_pref" "$@"
}

log_warn() {
    if [ $log_level -gt 2 ]; then
        return
    fi
    log "$log_warn_pref" "$@"
}

log_error() {
    if [ $log_level -gt 3 ]; then
        return
    fi
    log "$log_error_pref" "$@"
}

log() {
    update_timestamp
    if [ -z "$log_file" ]; then
        echo -e "[$_timestamp]" "$@" >&1
    else
        echo "[$_timestamp]" "$@" >> "$log_file"
    fi
}

output() {
    update_timestamp
    if [ -z "$output_file" ]; then
        echo "[$_timestamp]" "$@" >&1
    else
        echo "[$_timestamp]" "$@" >> "$output_file"
    fi
}

#
# Write result json string to result file.
#
result() {
    # NOTE: don't insert timestamp in result file.
    if [ -z "$result_file" ]; then
        echo "$@" >&1
    else
        echo "$@" > "$result_file"
    fi
}

log "Log begin:"
output "Output begin:"

#
# Parsing arguments
#
while [[ $1 != "" ]]; do
    case $1 in

    #
    # Device Update specific arguments.
    #
    --action-download)
        shift
        do_download_action=yes
        ;;

    --action-install)
        shift
        log_info "Will runscript as 'installer' script."
        do_install_action=yes
        ;;

    --action-apply)
        shift
        do_apply_action=yes
        ;;

    --action_cancel)
        shift
        do_cancel_action=yes
        ;;

    --action-is-installed)
        shift
        check_is_installed=yes
        ;;

    --installed-criteria)
        shift
        if [[ -z $1 || $1 == -* ]]; then
            error "--installed-criteria requires an installedCriteria parameter."
            $ret 1
        fi
        installed_criteria="$1"
        shift
        ;;

    #
    # Update artifacts
    #
    --firmware-file)
        shift
        if [[ -z $1 || $1 == -* ]]; then
            error "--firmware-file parameter is mandatory."
            $ret 1
        fi
        firmware_file="$1"
        echo "firmware file: $firmware_file"
        shift
        ;;

    --work-folder)
        shift
        if [[ -z $1 || $1 == -* ]]; then
            error "--work-folder parameter is mandatory."
            $ret 1
        fi
        workfolder="$1"
        echo "Workfolder: $workfolder"
        shift
        ;;

    #
    # Output-related arguments.
    #
    # --out-file <file_path>, --result-file <file_path>, --log-file <file_path>
    #
    --output-file)
        shift
        if [[ -z $1 || $1 == -* ]]; then
            error "--out-file parameter is mandatory."
            $ret 1
        fi
        output_file="$1"

        #
        #Create output file path.
        #
        # Delete existing log.
        rm -f -r "$output_file"
        # Create dir(s) recursively (include filename, well remove it in the following line...).
        mkdir -p "$output_file"
        # Delete leaf-dir (w)
        rm -f -r "$output_file"

        shift
        ;;

    --result-file)
        shift
        if [[ -z $1 || $1 == -* ]]; then
            error "--result-file parameter is mandatory."
            $ret 1
        fi
        result_file="$1"
        #
        #Create result file path.
        #
        # Delete existing log.
        rm -f -r "$result_file"
        # Create dir(s) recursively (include filename, well remove it in the following line...).
        mkdir -p "$result_file"
        # Delete leaf-dir (w)
        rm -f -r "$result_file"
        shift
        ;;

    --log-file)
        shift
        if [[ -z $1 || $1 == -* ]]; then
            error "--log-file parameter is mandatory."
            $ret 1
        fi
        log_file="$1"
        shift
        ;;
    --log-level)
        shift
        if [[ -z $1 || $1 == -* ]]; then
            error "--log-level parameter is mandatory."
            $ret 1
        fi
        log_level=$1
        shift
        ;;

    -h | --help)
        print_help
        $ret 0
        ;;


    *) # preserve positional arguments
        PARAMS="$PARAMS $1"
        shift
        ;;
    esac
done

#
# Device Update related functions.
#

# usage: write_applied_firmware_json $firmwareVersion
write_applied_firmware_json() {
    
    if [[ ! -f $firmware_file ]]; then
        log_info "Firmware file does not exist. Creating it."
        mkdir -p "$(dirname $firmware_file)" && touch $firmware_file
    fi
    
    echo "{\"firmwareVersion\":$1}" > $firmware_file
}

# usage: read_applied_firmware_json <out param>
read_applied_firmware_json() {
    local -n res=$1 
    if [[ -f $firmware_file ]]; then
        res=$(jq -r '.firmwareVersion' $firmware_file)
    else
        res=""
    fi
}


#
# A helper function that evaluates whether an update is currently installed on the target,
# based on 'installedCriteria'.
#
# Usage: is_installed $installedCriteria <out resultCode> <out extendedResultCode> <out resultDetails>
#
# shellcheck disable=SC2034
function is_installed() {
    local -n rc=$2  # name reference for resultCode
    local -n erc=$3 # name reference for extendedResultCode
    local -n rd=$4  # name reference for resultDetails

    version=""
    read_applied_firmware_json version

    if [[ $version == "" ]]; then
        log_info "No version found."
        rc=901
        erc=0
        rd="No version found."
    elif [[ $version == $1 ]]; then
        log_info "Version $version is installed."
        rc=900
        erc=0
        rd=""
    else
        log_info "Version $version is not installed."
        rc=901
        erc=0
        rd=""
    fi
}


#
# Example implementation of 'IsInstalled' function, for contoso-motor component.
#
# Design Goal:
#   Determine whether the specified 'installedCriteria' (parameter $1) is met.
#
#   'installedCriteria' is a version number of a motor in a mock component's data file
#   located at "$component_props['path']/firmware.json".
#
#   The version number is in the following format:
#
#       {
#          ...
#          "version":"1.0"
#          ...
#       }
#
#
#   For demo purposes, to determine whether the motor has a desired version installed, we'll simply check
#   an output from command grep -F "\"version\":\"$installed_criteria\"" <component file>
#   If matched, grep command will return '0' exit code. Otherwise, '1'.
#
# Expected resultCode:
#     ADUC_Result_Failure = 0,
#     ADUC_Result_IsInstalled_Installed = 900,     /**< Succeeded and content is installed. */
#     ADUC_Result_IsInstalled_NotInstalled = 901,  /**< Succeeded and content is not installed */
#
IsInstalled() {
    resultCode=0
    extendedResultCode=0
    resultDetails=""
    ret_val=0

    is_installed "$installed_criteria" resultCode extendedResultCode resultDetails

    # Prepare ADUC_Result json.
    aduc_result_json="{\"resultCode\":$resultCode, \"extendedResultCode\":$extendedResultCode,\"resultDetails\":\"$resultDetails\"}"

    # Show output.
    output "Result:" "$aduc_result_json"

    # Write ADUC_Result to result file.
    result "$aduc_result_json"

    $ret $ret_val

}

#
# Perfomr download-related tasks
# Return codes:
#   500 - ADUC_Result_Download_Success
#   501 - ADUC_Result_Download_InProgress
#   502 - ADUC_Result_Download_Skipped_FileExists
#   503 - ADUC_Result_Download_Skipped_UpdateAlreadyInstalled
#   504 - ADUC_Result_Download_Skipped_NoMatchingComponents
#   520 - ADUC_Result_Download_Handler_SuccessSkipDownload
#   521 - ADUC_Result_Download_Handler_RequiredFullDownload
#
DownloadUpdateArtifacts() {
    resultCode=500
    extendedResultCode=0
    resultDetails=""
    ret_val=0

    #
    # PLACEHOLDER : Perform download-related tasks here.
    #
    #               Based on overall result..
    #
    #               # Set result code and details
    #               resultCode=<Result Code>
    #               extendedResultCode=<Extended Result Code, in case of error>
    #               resultDetails="<Additional result details>
    #               $ret_value=<Script exit code>

    # Prepare ADUC_Result json.
    aduc_result_json="{\"resultCode\":$resultCode, \"extendedResultCode\":$extendedResultCode,\"resultDetails\":\"$resultDetails\"}"

    # Show output.
    output "Result:" "$aduc_result_json"

    # Write ADUC_Result to result file.
    result "$aduc_result_json"

    $ret $ret_val
}

#
# Perform install-related tasks.
#   600 - ADUC_Result_Install_Success
#   601 - ADUC_Result_Install_InProgress
#   603 - ADUC_Result_Install_Skipped_UpdateAlreadyInstalled
#   604 - ADUC_Result_Install_Skipped_NoMatchingComponents
#   605 - ADUC_Result_Install_RequiredImmediateReboot
#   606 - ADUC_Result_Install_RequiredReboot
#   607 - ADUC_Result_Install_RequiredImmediateAgentRestart
#   608 - ADUC_Result_Install_RequiredAgentRestart
#
InstallUpdate() {
    resultCode=0
    extendedResultCode=0
    resultDetails=""
    ret_val=0

    log_info "Installing update."

    # Check whether the component is already installed the specified update...
    is_installed "$installed_criteria" resultCode extendedResultCode resultDetails

    is_installed_ret=$?

    if [[ $is_installed_ret -ne 0 ]]; then
        # is_installed functin failed to execute.
        resultCode=0
        resultDetails="Internal error in 'is_installed' function."
    elif [[ $resultCode == 0 ]]; then
        # Failed to determine whether the update has been installed or not.
        # Return current ADUC_Result
        echo "Failed to determine wehther the update has been installed or note."
    elif [[ $resultCode -eq 901 ]]; then
        # Not installed.

        # install an update.
        echo "Installing update." >> "${log_file}"

        write_applied_firmware_json "$installed_criteria"

        resultCode=600
        resultDetails="Update installed successfully with version $installed_criteria."
        ret_val=0
    else
        # Installed.
        log_info "It appears that this component already installed the specified update."
        resultCode=603
        extendedResultCode=0
        resultDetails="Already installed."
        ret_val=0
    fi

    #
    # PLACEHOLDER : Evaluate 'installedCriteria' to determine if there is anything to do.
    #               If not, return ADUC_Result_Install_Success (600). Otherwise, continue.
    #

    #
    # PLACEHOLDER : Perform installation tasks here.
    #
    #               Based on overall result..
    #
    #               # Set result code and details
    #               resultCode=<Result Code>
    #               extendedResultCode=<Extended Result Code, in case of error>
    #               resultDetails="<Additional result details>
    #               $ret_value=<Script exit code>

    # Prepare ADUC_Result json.
    aduc_result_json="{\"resultCode\":$resultCode, \"extendedResultCode\":$extendedResultCode,\"resultDetails\":\"$resultDetails\"}"

    # Show output.
    output "Result:" "$aduc_result_json"

    # Write ADUC_Result to result file.
    result "$aduc_result_json"

    $ret $ret_val
}

#
# Perform install-related tasks.
# Return codes:
#   700 - ADUC_Result_Apply_Success
#   701 - ADUC_Result_Apply_InProgress
#   705 - ADUC_Result_Apply_RequiredImmediateReboot
#   706 - ADUC_Result_Apply_RequiredReboot
#   707 - ADUC_Result_Apply_RequiredImmediateAgentRestart
#   708 - ADUC_Result_Apply_RequiredAgentRestart
#
ApplyUpdate() {
    resultCode=700
    extendedResultCode=0
    resultDetails=""
    ret_val=0
    #
    # PLACEHOLDER : Perform installation tasks here.
    #
    #               Based on overall result..
    #
    #               # Set result code and details
    #               resultCode=<Result Code>
    #               extendedResultCode=<Extended Result Code, in case of error>
    #               resultDetails="<Additional result details>
    #               $ret_value=<Script exit code>

    # Prepare ADUC_Result json.
    aduc_result_json="{\"resultCode\":$resultCode, \"extendedResultCode\":$extendedResultCode,\"resultDetails\":\"$resultDetails\"}"

    # Show output.
    output "Result:" "$aduc_result_json"

    # Write ADUC_Result to result file.
    result "$aduc_result_json"

    $ret $ret_val
}

# Perform cancel-related tasks.
# Return codes:
#   800 - ADUC_Result_Cancel_Success
#   801 - ADUC_Result_Cancel_Failure
CancelUpdate() {
    ret_val=0
    $ret $ret_val
}

#
# Main
#
if [ -n "$check_is_installed" ]; then
    IsInstalled "$installed_criteria"
    exit $ret_val
fi

if [ -n "$do_download_action" ]; then
    DownloadUpdateArtifacts
    exit $ret_val
fi

if [ -n "$do_install_action" ]; then
    InstallUpdate
    exit $ret_val
fi

if [ -n "$do_apply_action" ]; then
    ApplyUpdate
    exit $ret_val
fi

if [ -n "$do_cancel_action" ]; then
    CancelUpdate
    exit $ret_val
fi

$ret $ret_val
