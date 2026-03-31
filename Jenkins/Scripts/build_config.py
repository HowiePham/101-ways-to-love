"""Configuration reader/writer for Jenkins CI pipeline.

Create default cfg file to use between normal python code and shell script code.
Shell can use source function to source .cfg file to environment variable

Jenkins do not support nor advise people to share config in environment variable.
This .cfg file is how to communicate between stages.
"""

import os
import pathlib
import sys


class KEY:
    # Manual set config
    BUILD_METHOD_NAME = "BUILD_METHOD_NAME"
    UNITY_BUILD_PARAMS = "UNITY_BUILD_PARAMS"
    UNITY_BUILD_FAILURE = "UNITY_BUILD_FAILURE"
    UNITY_BUILD_LOG = "UNITY_BUILD_LOG"
    BASE_BUNDLE_VERSION_CODE = "BASE_BUNDLE_VERSION_CODE"
    GOOGLE_INTERNAL_TEST_URL = "GOOGLE_INTERNAL_TEST_URL"
    ANDROID_MIN_SDK = "ANDROID_MIN_SDK"
    ANDROID_TARGET_SDK = "ANDROID_TARGET_SDK"
    DEVELOPMENT_BUILD = "DEVELOPMENT_BUILD"
    ALLOW_DEBUGGING = "ALLOW_DEBUGGING"

    # Slack API
    SLACK_BOT_TOKEN = "SLACK_BOT_TOKEN"
    SLACK_DEFAULT_CHANNEL = "SLACK_DEFAULT_CHANNEL"
    SLACK_RELEASE_CHANNEL = "SLACK_RELEASE_CHANNEL"
    SLACK_DEV_CHANNEL = "SLACK_DEV_CHANNEL"
    SLACK_DEV_CHANNEL_ID = "SLACK_DEV_CHANNEL_ID"
    SLACK_RELEASE_CHANNEL_ID = "SLACK_RELEASE_CHANNEL_ID"
    SLACK_MESSAGE_TS = "SLACK_MESSAGE_TS"

    # Generate config env during CI
    BUNDLE_ID = "BUNDLE_ID"
    COMPANY_NAME = "COMPANY_NAME"
    PROJECT_NAME = "PROJECT_NAME"
    UNITY_VERSION = "UNITY_VERSION"
    UNITY_CHANGESET = "UNITY_CHANGESET"
    UNITY_MODULE = "UNITY_MODULE"
    UNITY_LICENSE = "UNITY_LICENSE"
    UNITY_PATH = "UNITY_PATH"
    UNITY_PROJECT = "UNITY_PROJECT"
    UNITY_BUILD_COMMAND = "UNITY_BUILD_COMMAND"
    BUILD_TARGET = "BUILD_TARGET"
    BUILD_LOG_PATH = "BUILD_LOG_PATH"
    PIPELINE = "PIPELINE"
    KEYSTORE_CREDENTIAL_ID = "KEYSTORE_CREDENTIAL_ID"
    KEYALIAS_CREDENTIAL_ID = "KEYALIAS_CREDENTIAL_ID"
    LATEST_GOOGLE_CONSOLE_VERSION_CODE = "LATEST_GOOGLE_CONSOLE_VERSION_CODE"
    BUILD_ERROR_MESSAGE = "BUILD_ERROR_MESSAGE"

    # After Unity build config
    APP_VERSION = "APP_VERSION"
    # Git commit info
    GIT_BRANCH = "GIT_BRANCH"
    GIT_AUTHOR_EMAIL = "GIT_AUTHOR_EMAIL"  # format:%ae
    GIT_AUTHOR_NAME = "GIT_AUTHOR_NAME"  # format:%an
    GIT_COMMIT_HASH = "GIT_COMMIT_HASH"  # format:%H
    GIT_COMMIT_SHORT_HASH = "GIT_COMMIT_SHORT_HASH"  # format:%h
    GIT_TREE_HASH = "GIT_TREE_HASH"  # format:%T
    GIT_AUTHOR_DATE = "GIT_AUTHOR_DATE"  # format:%aD
    GIT_COMMITTER_NAME = "GIT_COMMITTER_NAME"  # format:%cn
    GIT_COMMITTER_EMAIL = "GIT_COMMITTER_EMAIL"  # format:%ce
    GIT_COMMIT_DATE = "GIT_COMMIT_DATE"  # format:%ci
    GIT_SUBJECT = "GIT_SUBJECT"  # format:%s
    GIT_BODY = "GIT_BODY"  # format:%b
    GIT_RAW_BODY = "GIT_RAW_BODY"  # format:%B
    GIT_COMMIT_MESSAGE = "GIT_COMMIT_MESSAGE"

    # Firebase
    FIREBASE_APP_ID = "FIREBASE_APP_ID"

    # iOS / Fastlane
    IOS_TEAM_ID = "IOS_TEAM_ID"
    IOS_FASTLANE_LANE = "IOS_FASTLANE_LANE"

_dir_path = os.path.dirname(os.path.realpath(__file__))

_base_path = pathlib.Path(_dir_path)

_config_path = os.path.join(_base_path.parent, "config.cfg")


def _is_comment_or_empty(line):
    stripped = line.strip()
    return not stripped or stripped.startswith('#')


def _parse_config():
    """Parse config.cfg into a dict. Returns {key: value}."""
    result = {}
    try:
        with open(_config_path, 'r', encoding='utf-8') as f:
            for line in f:
                if _is_comment_or_empty(line):
                    continue
                parts = line.split("=", 1)
                if len(parts) < 2:
                    continue
                key = parts[0].strip()
                val = parts[1].rstrip()
                if val.startswith('"') and val.endswith('"'):
                    val = val[1:-1]
                result[key] = val
    except FileNotFoundError:
        print(f"[WARN] Config file not found: {_config_path}", flush=True)
    return result


def contains(name):
    return name in _parse_config()


def read(name):
    return _parse_config().get(name)


def write(key: str, new_value: str):
    """Write a key=value pair to config.cfg (shell-compatible format)."""
    if new_value is None or new_value == "":
        return
    if "\n" in new_value:
        new_value = new_value.replace('\n', '\\n')
    # if have space then wrap it around quote
    if ' ' in new_value:
        new_value = new_value.strip()
        if not new_value.startswith('"') and not new_value.endswith('"'):
            new_value = '"' + new_value + '"'

    # Read with utf-8 encoding to handle Vietnamese file names in git commit messages, etc.
    with open(_config_path, 'r', encoding="utf-8") as file:
        lines = file.readlines()

    # Build a clean list — filter out empty/malformed lines without mutating during iteration
    cleaned = []
    found_line = False
    for orig_line in lines:
        if not orig_line.strip():
            continue

        # Preserve comment lines as-is
        if orig_line.strip().startswith('#'):
            cleaned.append(orig_line)
            continue

        splits = orig_line.split("=", 1)
        if len(splits) < 2:
            print(f"Line with wrong format. Remove: {orig_line.strip()}", flush=True)
            continue

        if splits[0] == key:
            old_value = splits[1].strip()
            if old_value == new_value:
                print(f"no change {key}={old_value}", flush=True)
                return
            print(f"Replace {key} from {old_value} to {new_value}", flush=True)
            cleaned.append(f"{key}={new_value}")
            found_line = True
        else:
            cleaned.append(orig_line)

    if not found_line:
        print(f"Add new value {key}={new_value}", flush=True)
        cleaned.append(f"{key}={new_value}")

    with open(_config_path, 'w', encoding="utf-8") as f:
        for item in cleaned:
            f.write(item.rstrip() + "\n")


def print_all_config():
    print("------CONFIG------", flush=True)
    with open(_config_path, 'r', encoding='utf-8') as file:
        filedata = file.readlines()
    for x in filedata:
        print(x.strip(), flush=True)
    print("------CONFIG------", flush=True)


if __name__ == "__main__":
    import argparse

    parser = argparse.ArgumentParser(description="Write/Read Config in parent folder")
    parser.add_argument('--set', nargs='?', help="set a config value with --value")
    parser.add_argument('--get', nargs='?', help="return a value from config. Use with jenkins shell")
    parser.add_argument('--value', nargs='?', default=None, help="value to set")

    options = parser.parse_args()

    if options.get is not None:
        print(read(options.get), flush=True)
        sys.stdout.flush()

    if options.set is not None:
        write(options.set, options.value)
