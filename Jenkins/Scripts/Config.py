import os
import pathlib
import sys

"""
Create default cfg file to use between normal python code and shell script code.
Shell can use source function to source .cfg file to environment variable

Jenkins do not support nor advise people to share config in environment variable. This .cfg file is how to communicate between stages
"""


class KEY(object):
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
    SLACK_DEV_CHANNEL_ID="SLACK_DEV_CHANNEL_ID"
    SLACK_RELEASE_CHANNEL_ID="SLACK_RELEASE_CHANNEL_ID"
    SLACK_MESSAGE_TS ="SLACK_MESSAGE_TS"

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
    BUILD_TARGET = "BUILD_TARGET"
    BUILD_LOG_PATH = "BUILD_LOG_PATH"
    PIPELINE = "PIPELINE"
    KEYSTORE_CREDENTIAL_ID = "KEYSTORE_CREDENTIAL_ID"
    KEYALIAS_CREDENTIAL_ID = "KEYALIAS_CREDENTIAL_ID"
#     GCLOUD_SERVICE_ACCOUNT_CREDENTIALS = "GCLOUD_SERVICE_ACCOUNT_CREDENTIALS"
    LATEST_GOOGLE_CONSOLE_VERSION_CODE = "LATEST_GOOGLE_CONSOLE_VERSION_CODE"
    BUILD_ERROR_MESSAGE = "BUILD_ERROR_MESSAGE"

    # After Unity build config
    APP_VERSION = "APP_VERSION"
    # Git commit info
    GIT_BRANCH = "GIT_BRANCH"
    GIT_AUTHOR_EMAIL = "GIT_AUTHOR_EMAIL"  # format:%ae
    GIT_AUTHOR_NAME = "GIT_AUTHOR_NAME"  # format:%an
    GIT_AUTHOR = "GIT_AUTHOR"  # format:%an
    GIT_COMMIT_HASH = "GIT_COMMIT_HASH"  # format:%H
    GIT_COMMIT_SHORT_HASH = "GIT_COMMIT_SHORT_HASH"  # format:%h
    GIT_TREE_HASH = "GIT_TREE_HASH"  # format:%T
    GIT_AUTHOR_DATE = "GIT_AUTHOR_DATE"  # format:%aD
    GIT_COMMITTER_NAME = "GIT_COMMITTER_NAME"  # format:%cn
    GIT_COMMITER = "GIT_COMMITER"  # format:%cn
    GIT_COMMITER_EMAIL = "GIT_COMMITER_EMAIL"  # format:%ce
    GIT_COMMIT_DATE = "GIT_COMMIT_DATE"  # format:%ci
    GIT_SUBJECT = "GIT_SUBJECT"  # format:%s
    GIT_BODY = "GIT_BODY"  # format:%b
    GIT_RAW_BODY = "GIT_RAW_BODY"  # format:%B
    GIT_COMMIT_MESSAGE = "GIT_COMMIT_MESSAGE"
    
    # Firebase
    FIREBASE_APP_ID = "FIREBASE_APP_ID"

dir_path = os.path.dirname(os.path.realpath(__file__))

path = pathlib.Path(dir_path)

config = os.path.join(path.parent, "config.cfg")


def contain(name):
    with open(config, 'r') as file:
        filedata = file.readlines()
    for i, x in enumerate(filedata):
        splits = x.split("=")
        if splits[0] == name:
            return True
    return False


def read(name):
    with open(config, 'r') as file:
        filedata = file.readlines()
    for i, x in enumerate(filedata):
        splits = x.split("=")
        if splits[0] == name:
            var = splits[1].rstrip()
            if var.startswith('"') and var.endswith('"'):
                var = var[1: -1]
            return var
    return None


# Config file in format Shell Config for .sh script
def write(key: str, new_value: str):
    if new_value is None or new_value == "":
        return
    if "\n" in new_value:
        # print("multi-line String. replace with \\\\n")
        new_value = new_value.replace('\n', '\\n')
    # if have space then wrap it around quote
    if ' ' in new_value:
        new_value = new_value.strip()
        if not new_value.startswith('"') and not new_value.endswith('"'):
            new_value = '"' + new_value + '"'

    # Read in the file with utf-8 encoding to prevent error while reading git commit message that contained utf-8 characters (Vietnamese file name, etc...)
    with open(config, 'r', encoding="utf-8") as file:
        lines = file.readlines()

    found_line = False
    for i, orig_line in enumerate(lines):
        # remove empty line
        if not orig_line:
            lines.pop(i)
            continue

        splits = orig_line.split("=")

        if len(splits) != 2:
            print("Line with wrong format. Remove: " + orig_line)
            lines.pop(i)
            continue
        # line already exist. replace it
        if splits[0] == key:
            old_value = splits[1].strip()
            if old_value == new_value:
                print(f"no change {key}={old_value}")
                return
            print("Replace " + key + " from " + old_value + " to " + new_value)
            lines[i] = key + "=" + new_value
            found_line = True
    if not found_line:
        value = key + "=" + new_value
        print("Add new value " + value)
        lines.append(value)
    # Write the file out again
    with open(config, 'w', encoding="utf-8") as f:
        for item in lines:
            f.write(item.rstrip() + "\n")


def print_all_config():
    print("------CONFIG------")
    with open(config, 'r') as file:
        filedata = file.readlines()
    for i, x in enumerate(filedata):
        print(x.strip())
    print("------CONFIG------")


if __name__ == "__main__":
    import argparse

    parser = argparse.ArgumentParser(description="Write/Read Config in parent folder")
    parser.add_argument('--set', nargs='?', help="set a config value with --value")
    parser.add_argument('--get', nargs='?', help="return a value from config. Use with jenkins shell")
    parser.add_argument('--value', nargs='?', default=None, help="value to set")

    options = parser.parse_args()

    if options.get is not None:
        print(read(options.get))
        sys.stdout.flush()

    if options.set is not None:
        write(options.set, options.value)
