import os
import sys

# Ensure sibling modules are importable when script dir is not in sys.path
_SCRIPTS_DIR = os.path.dirname(os.path.abspath(__file__))
if not any(os.path.normcase(_SCRIPTS_DIR) == os.path.normcase(p) for p in sys.path):
    sys.path.insert(0, _SCRIPTS_DIR)

import build_config
import slack_command
from bundle_to_apk import convert_aab_to_apk_build_folder
import utils


def main():
    unity_project = build_config.read(build_config.KEY.UNITY_PROJECT)
    pipeline = build_config.read(build_config.KEY.PIPELINE)
    build_folder = os.path.join(unity_project, "../build")
    message_ts = utils.require_env("SLACK_MESSAGE_TS")

    convert_aab_to_apk_build_folder()

    print(f"LOG::: Send artifacts of {pipeline} to Slack channel")

    artifact_files = []
    for dirpath, _, filenames in os.walk(build_folder):
        for file in filenames:
            if file.endswith((".apk", ".aab", ".csv")):
                artifact_files.append(os.path.join(dirpath, file))

    if artifact_files:
        slack_command.append_files_to_message(
            slack_command.get_slack_channel_id(), message_ts, artifact_files
        )


if __name__ == "__main__":
    main()
