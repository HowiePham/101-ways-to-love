import os
import pathlib
import re
import sys
import platform as platform_module

# Ensure sibling modules are importable when script dir is not in sys.path
_SCRIPTS_DIR = os.path.dirname(os.path.abspath(__file__))
if not any(os.path.normcase(_SCRIPTS_DIR) == os.path.normcase(p) for p in sys.path):
    sys.path.insert(0, _SCRIPTS_DIR)

import build_config

"""
Find Installed Unity and save it to config
"""


def check_unity_version_exist(location: str, version: str, current_platform: str):
    if "darwin" in current_platform:
        unity_path = os.path.join(location, version, "Unity.app", "Contents", "MacOS", "Unity")
    elif "linux" in current_platform:
        unity_path = os.path.join(location, version, "Editor", "Unity")
    else:
        unity_path = os.path.join(location, version, "Editor", "Unity.exe")

    print("Check Unity Installation: " + unity_path)
    unity_exe = pathlib.Path(unity_path)

    if unity_exe.is_file():
        build_config.write(build_config.KEY.UNITY_PATH, f'"{unity_path}"')
        return True

    print("Not found: " + unity_path)
    return False


def try_use_version(location: str, version: str, current_platform: str):
    """Try to find Unity at the given version. Returns True if found."""
    return check_unity_version_exist(location, version, current_platform)


def main():
    current_platform = platform_module.system().lower()
    print("Platform: " + current_platform)

    # Default Unity Hub install locations per platform
    if "darwin" in current_platform:
        default_unity_location = "/Applications/Unity/Hub/Editor"
    elif "linux" in current_platform:
        default_unity_location = "~/Unity/Hub/Editor"
    else:
        default_unity_location = 'C:\\UnityEditors'

    location = os.environ.get("UNITY_INSTALL_LOCATION", default_unity_location)
    unity_version = build_config.read(build_config.KEY.UNITY_VERSION)
    if not unity_version:
        print("ERROR: UNITY_VERSION not found in build_config.cfg")
        sys.exit(1)
    unity_version = unity_version.strip()

    print("Unity Install Location: " + location)

    # Try exact version first
    if try_use_version(location, unity_version, current_platform):
        print("Confirm unity.exe exist. Return 0")
        sys.exit(0)

    # Find alternative version (e.g. from 2023.2.22f1, try any 2023.2.* version)
    print("Finding alternative unity version now")
    for m in re.finditer(r'(\d+)\.(\d+)\.', unity_version):
        prefix = m.group()
        print(prefix)
        if not os.path.isdir(location):
            print(f"Unity install location does not exist: {location}")
            break
        possible_editors = sorted(os.listdir(location), reverse=True)
        for f in possible_editors:
            if prefix in f:
                print(f"Found alternative version: {f}")
                if try_use_version(location, f, current_platform):
                    print(f"Using alternative Unity version: {f}")
                    sys.exit(0)

    print("ERROR: No matching Unity installation found")
    sys.exit(1)


if __name__ == "__main__":
    main()
