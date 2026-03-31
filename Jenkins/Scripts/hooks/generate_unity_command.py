"""Generate the Unity CLI build command and write it to build_config."""

import contextlib
import io
import os
import platform as platform_mod
import sys

import build_config

HOOK_EVENTS = ["before_build"]
HOOK_ORDER = 10

LOG_FILE_NAME = "buildlog.txt"


def run():
    logfile = os.path.join(os.getcwd(), LOG_FILE_NAME)

    build_target = build_config.read(build_config.KEY.BUILD_TARGET)
    params = ""

    current_platform = str(platform_mod.system()).lower()

    # Save log path to config (suppress stdout during write to avoid noise)
    with contextlib.redirect_stdout(io.StringIO()):
        build_config.write(build_config.KEY.UNITY_BUILD_LOG, logfile)

    unity_path = build_config.read(build_config.KEY.UNITY_PATH)
    project_folder = build_config.read(build_config.KEY.UNITY_PROJECT)

    build_method = build_config.read(build_config.KEY.BUILD_METHOD_NAME)
    if build_method is None:
        build_method = "Builder.BuildDevelopment"

    value = build_config.read(build_config.KEY.BUILD_TARGET)
    if value is not None:
        build_target = value

    value = build_config.read(build_config.KEY.UNITY_BUILD_PARAMS)
    if value is not None:
        params = value

    unity_path = unity_path.replace('"', "").replace("'", "")

    command = (
        f'{unity_path} -quit -batchmode -disable-assembly-updater '
        f'-projectPath "{project_folder}" -buildTarget {build_target} '
        f'-logFile {logfile} -executeMethod {build_method}'
    )
    if params is not None and params != "":
        command += f' "{params}"'

    # Add -nographics for headless CI builds (eliminates GPU initialization overhead)
    if os.environ.get("CI") or os.environ.get("JENKINS_URL"):
        command += " -nographics"

    # Support linux headless builds
    if "linux" in current_platform:
        command = f"xvfb-run {command} -force-free"

    build_config.write(build_config.KEY.UNITY_BUILD_COMMAND, command)

    # Output command to Jenkins
    print(command)
    sys.stdout.flush()
