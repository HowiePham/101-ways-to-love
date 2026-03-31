"""Clean the build output folder before starting a new build."""

import os
import shutil

import build_config

HOOK_EVENTS = ["before_build"]
HOOK_ORDER = 0


def run():
    print("-----------CLEAN BUILD FOLDER-----------", flush=True)

    project_dir = build_config.read(build_config.KEY.UNITY_PROJECT)
    build_folder = os.path.join(project_dir, "../build")

    print(f"Clean folder {build_folder}", flush=True)
    if os.path.exists(build_folder):
        for filename in os.listdir(build_folder):
            file_path = os.path.join(build_folder, filename)
            try:
                if os.path.isfile(file_path) or os.path.islink(file_path):
                    os.unlink(file_path)
                    print(f"Unlink: {file_path}", flush=True)
                elif os.path.isdir(file_path):
                    shutil.rmtree(file_path)
                    print(f"Remove: {file_path}", flush=True)
            except Exception as e:
                print(f"Failed to delete {file_path}. Reason: {e}", flush=True)

    print("-----------DONE CLEAN BUILD FOLDER-----------", flush=True)
