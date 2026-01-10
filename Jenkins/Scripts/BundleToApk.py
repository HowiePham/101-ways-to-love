import os
import sys
import glob
from zipfile import ZipFile
from typing import Optional
import Config


def find_bundletool_path(working_dir: str) -> Optional[str]:
    """
    Tự động tìm file bundletool-all-*.jar trong working_dir.
    Trả về đường dẫn nếu tìm thấy, ngược lại trả None.
    """
    pattern = os.path.join(working_dir, "bundletool-all-*.jar")
    matches = glob.glob(pattern)
    if matches:
        # Ưu tiên version cao nhất (theo tên)
        matches.sort(reverse=True)
        print(f"LOG::: Found bundletool to {matches[0]}")
        return matches[0]
    else:
        print(f"[WARN] No bundletool found in {working_dir}")
        return None


def convert_aab_to_apk(aab_path: str, bundletool_path: str) -> Optional[str]:
    """
    Convert a single .aab file to .apk using bundletool.
    Trả về đường dẫn file .apk đã tạo, hoặc None nếu thất bại.
    """
    if not os.path.exists(aab_path):
        print(f"[ERROR] File not found: {aab_path}")
        return None

    apks_path = aab_path.replace(".aab", ".apks")
    apk_path = aab_path.replace(".aab", ".apk")

    print(f"\nLOG::: Start convert AAB to APK: {aab_path}")
    cmd = (
        f'java -jar "{bundletool_path}" build-apks '
        f'--mode=universal --bundle="{aab_path}" --output="{apks_path}"'
    )

    print(f"LOG::: Running command:\n  {cmd}")
    result = os.system(cmd)
    if result != 0:
        print(f"[ERROR] Failed to execute bundletool for {aab_path}")
        return None

    # Rename .apks to .zip
    apks_zip_path = apks_path.replace(".apks", ".zip")
    print(f"LOG::: Rename {apks_path} to {apks_zip_path}")
    os.rename(apks_path, apks_zip_path)

    # Extract universal.apk
    print("LOG::: Extracting APK from ZIP...")
    with ZipFile(apks_zip_path, "r") as zip_obj:
        zip_obj.extractall()

    if not os.path.exists("universal.apk"):
        print("[ERROR] universal.apk not found after extraction!")
        return None

    os.rename("universal.apk", apk_path)
    os.remove(apks_zip_path)

    print(f"LOG::: Conversion complete to {apk_path}\n")
    return apk_path


def convert_aab_to_apk_build_folder():
    """
    Dò tìm tất cả .aab trong thư mục build của project Unity,
    rồi tự động convert sang .apk.
    """
    pipeline = Config.read(Config.KEY.PIPELINE)
    if pipeline != "release":
        print(f"Pipeline = '{pipeline}', skip conversion.")
        return

    unity_project = Config.read(Config.KEY.UNITY_PROJECT)
    build_folder = os.path.join(unity_project, "../build")
    working_dir = os.path.join(unity_project, "Jenkins/Scripts/Libs~/")

    print(f"Working directory: {working_dir}")
    os.chdir(working_dir)

    bundletool_path = find_bundletool_path(working_dir)
    if not bundletool_path:
        print("[ERROR] Could not locate bundletool-all-*.jar. Aborting.")
        return 

    for dirpath, _, filenames in os.walk(build_folder):
        for file in filenames:
            if file.endswith(".aab"):
                aab_path = os.path.join(dirpath, file)
                convert_aab_to_apk(aab_path, bundletool_path)
