import os
import subprocess
import glob
import sys
from zipfile import ZipFile
from typing import Optional

# Ensure sibling modules are importable when script dir is not in sys.path
_SCRIPTS_DIR = os.path.dirname(os.path.abspath(__file__))
if not any(os.path.normcase(_SCRIPTS_DIR) == os.path.normcase(p) for p in sys.path):
    sys.path.insert(0, _SCRIPTS_DIR)

import build_config


def find_bundletool_path(working_dir: str) -> Optional[str]:
    """Find bundletool-all-*.jar in the working directory. Returns highest version."""
    pattern = os.path.join(working_dir, "bundletool-all-*.jar")
    matches = glob.glob(pattern)
    if matches:
        matches.sort(reverse=True)
        print(f"LOG::: Found bundletool at {matches[0]}")
        return matches[0]
    else:
        print(f"[WARN] No bundletool found in {working_dir}")
        return None


def convert_aab_to_apk(aab_path: str, bundletool_path: str) -> Optional[str]:
    """Convert a single .aab file to .apk using bundletool. Returns the .apk path or None on failure."""
    if not os.path.exists(aab_path):
        print(f"[ERROR] File not found: {aab_path}")
        return None

    apks_path = aab_path.replace(".aab", ".apks")
    apk_path = aab_path.replace(".aab", ".apk")
    extract_dir = os.path.dirname(aab_path)

    print(f"\nLOG::: Start convert AAB to APK: {aab_path}")
    cmd = (
        f'java -jar "{bundletool_path}" build-apks '
        f'--mode=universal --bundle="{aab_path}" --output="{apks_path}"'
    )

    print(f"LOG::: Running command:\n  {cmd}")
    result = subprocess.run(cmd, shell=True)
    if result.returncode != 0:
        print(f"[ERROR] Failed to execute bundletool for {aab_path}")
        return None

    # Rename .apks to .zip for extraction
    apks_zip_path = apks_path.replace(".apks", ".zip")
    print(f"LOG::: Rename {apks_path} to {apks_zip_path}")
    os.rename(apks_path, apks_zip_path)

    # Extract universal.apk to the same directory as the source aab
    print("LOG::: Extracting APK from ZIP...")
    with ZipFile(apks_zip_path, "r") as zip_obj:
        zip_obj.extractall(path=extract_dir)

    universal_path = os.path.join(extract_dir, "universal.apk")
    if not os.path.exists(universal_path):
        print("[ERROR] universal.apk not found after extraction!")
        return None

    os.rename(universal_path, apk_path)
    os.remove(apks_zip_path)

    print(f"LOG::: Conversion complete to {apk_path}\n")
    return apk_path


def convert_aab_to_apk_build_folder():
    """Find all .aab files in the Unity build folder and convert them to .apk."""
    pipeline = build_config.read(build_config.KEY.PIPELINE)
    if pipeline != "release":
        print(f"Pipeline = '{pipeline}', skip conversion.")
        return

    unity_project = build_config.read(build_config.KEY.UNITY_PROJECT)
    build_folder = os.path.join(unity_project, "../build")
    working_dir = os.path.join(unity_project, "Jenkins/Scripts/Libs~/")

    print(f"Working directory: {working_dir}")

    bundletool_path = find_bundletool_path(working_dir)
    if not bundletool_path:
        print("[ERROR] Could not locate bundletool-all-*.jar. Aborting.")
        return

    for dirpath, _, filenames in os.walk(build_folder):
        for file in filenames:
            if file.endswith(".aab"):
                aab_path = os.path.join(dirpath, file)
                convert_aab_to_apk(aab_path, bundletool_path)
