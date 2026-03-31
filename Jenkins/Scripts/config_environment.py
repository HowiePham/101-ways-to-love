import os
import pathlib
import re
import sys

# Ensure sibling modules are importable when script dir is not in sys.path
_SCRIPTS_DIR = os.path.dirname(os.path.abspath(__file__))
if not any(os.path.normcase(_SCRIPTS_DIR) == os.path.normcase(p) for p in sys.path):
    sys.path.insert(0, _SCRIPTS_DIR)

import yaml

import build_config
import utils

LOG_FILE_NAME = "buildlog.txt"


def run_init():
    build_target_env()
    unity_settings_env()
    git_info_env()


def format_unity_yaml(filepath):
    """
    Load a Unity pseudo-YAML file and strip non-YAML-1.1 parts (!u! tags, & file IDs).
    Returns a YAML-compatible string for PyYAML.
    """
    result = ""
    with open(filepath, 'r') as f:
        for line in f:
            if line.startswith('--- !u!'):
                result += '--- ' + line.split(' ')[2] + '\n'
            else:
                result += line
    return result


def unity_settings_env():
    """
        Write project path and project version + project name for fastlane and pipeline to work with
    """
    dir_path = os.path.dirname(os.path.realpath(__file__))
    path = pathlib.Path(dir_path)
    project_dir = path.parent.parent
    build_config.write(build_config.KEY.UNITY_PROJECT, f'"{project_dir}"')
    log_file_path = os.path.join(project_dir.parent, LOG_FILE_NAME)
    build_config.write(build_config.KEY.BUILD_LOG_PATH, f'"{log_file_path}"')

    # Read Project setting yaml
    project_setting = format_unity_yaml(os.path.join(project_dir, "ProjectSettings", "ProjectSettings.asset"))
    doc = yaml.safe_load(project_setting)
    build_config.write(build_config.KEY.COMPANY_NAME, doc["PlayerSettings"]["companyName"])
    build_config.write(build_config.KEY.PROJECT_NAME, doc["PlayerSettings"]["productName"])
    build_config.write(build_config.KEY.APP_VERSION, doc["PlayerSettings"]["bundleVersion"])

    build_target = build_config.read(build_config.KEY.BUILD_TARGET)
    platform_name = "Android"
    if build_target == "iOS":
        platform_name = "iPhone"
    build_config.write(build_config.KEY.BUNDLE_ID, doc["PlayerSettings"]["applicationIdentifier"][platform_name])

    project_version = os.path.join(project_dir, "ProjectSettings", "ProjectVersion.txt")

    # Read unity version + changeset
    with open(project_version) as f:
        for line in f:
            if "m_EditorVersion:" in line:
                for m in re.finditer(r'(\d+)\.(\d+)\.(\w+)', line):
                    build_config.write(build_config.KEY.UNITY_VERSION, m.group())

            if "m_EditorVersionWithRevision:" in line:
                for m in re.finditer(r'(\d+)\.(\d+)\.(\w+)', line):
                    build_config.write(build_config.KEY.UNITY_VERSION, m.group())
                for m in re.finditer(r'(?<=\().+?(?=\))', line):
                    build_config.write(build_config.KEY.UNITY_CHANGESET, m.group())


def build_target_env():
    branch_name = os.environ.get("BRANCH_NAME", "")
    build_config.write(build_config.KEY.GIT_BRANCH, branch_name)
    lower_branch_name = branch_name.lower()

    if "-release" in lower_branch_name:
        build_config.write(build_config.KEY.PIPELINE, "release")
    elif "-dev" in lower_branch_name:
        build_config.write(build_config.KEY.PIPELINE, "dev")
    elif "-test" in lower_branch_name:
        build_config.write(build_config.KEY.PIPELINE, "test")
    else:
        build_config.write(build_config.KEY.PIPELINE, "internal")

    if not build_config.contains(build_config.KEY.BUILD_TARGET):
        if "ios" in lower_branch_name:
            build_config.write(build_config.KEY.BUILD_TARGET, "iOS")
        else:
            build_config.write(build_config.KEY.BUILD_TARGET, "Android")


def run_command(command):
    return utils.run_command(command)


def git_info_env():
    keys = [
        build_config.KEY.GIT_AUTHOR_EMAIL,
        build_config.KEY.GIT_AUTHOR_NAME,
        build_config.KEY.GIT_COMMIT_HASH,
        build_config.KEY.GIT_COMMIT_SHORT_HASH,
        build_config.KEY.GIT_TREE_HASH,
        build_config.KEY.GIT_AUTHOR_DATE,
        build_config.KEY.GIT_COMMITTER_NAME,
        build_config.KEY.GIT_COMMITTER_EMAIL,
        build_config.KEY.GIT_COMMIT_DATE,
        build_config.KEY.GIT_SUBJECT,
        build_config.KEY.GIT_BODY,
        build_config.KEY.GIT_RAW_BODY,
        build_config.KEY.GIT_COMMIT_MESSAGE,
    ]
    formats = ["%ae", "%an", "%H", "%h", "%T", "%ai", "%cn", "%ce", "%ci", "%s", "%b", "%B", "%B"]
    format_str = "%x00".join(formats)
    output = run_command(f'git log -1 --pretty=format:{format_str}')
    parts = output.split("\0")

    if len(parts) < len(keys):
        print(f"[config_environment] Warning: git log returned {len(parts)} fields, expected {len(keys)}")
        return

    for key, value in zip(keys, parts):
        build_config.write(key, value)


if __name__ == "__main__":
    run_init()