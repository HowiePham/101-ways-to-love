import os
import sys
import time
import argparse

import Config
import JenkinsBuildClient
import SlackCommand
import SlackMessageFormatter
from TimestampUtils import get_realtime_build_duration_ms


def wait_for_final_status(build_url, user, token, timeout=60, interval=5):
    """
    Poll Jenkins build until final result is available.
    """
    start_time = time.time()
    while time.time() - start_time < timeout:
        info = JenkinsBuildClient.get_build_info(build_url, user, token)
        if info is None:
            print("[WARN] Jenkins build info is None, retrying...", flush=True)
            time.sleep(interval)
            continue

        result = info.get("result")

        if result:
            print(f"[INFO] Final build result detected: {result}", flush=True)
            return info

        print(f"[INFO] Build in progress... duration={get_realtime_build_duration_ms()}ms", flush=True)
        time.sleep(interval)

    print(f"[WARN] Timeout waiting for final Jenkins status after {timeout}s", flush=True)
    return JenkinsBuildClient.get_build_info(build_url, user, token)


def parse_args():
    parser = argparse.ArgumentParser(description="Update final Slack message after Jenkins build finishes")
    parser.add_argument("--jenkins-user", required=True, help="Jenkins username")
    parser.add_argument("--jenkins-token", required=True, help="Jenkins API token")
    return parser.parse_args()


def main():
    args = parse_args()

    build_url = os.environ.get("BUILD_URL")
    slack_ts = os.environ.get("SLACK_MESSAGE_TS")
    channel_id = SlackCommand.get_slack_channel_id()

    if not build_url or not slack_ts:
        print("[ERROR] Missing BUILD_URL or SLACK_MESSAGE_TS", flush=True)
        sys.exit(1)

    print(f"[INFO] Waiting for Jenkins to finalize build: {build_url}", flush=True)
    build_info = wait_for_final_status(build_url, args.jenkins_user, args.jenkins_token)

    if not build_info:
        print("[ERROR] Could not fetch build info.", flush=True)
        sys.exit(1)

    # Format Slack message; format_build_message sẽ gọi TimestampUtils.get_realtime_build_duration_ms() nếu cần
    message = SlackMessageFormatter.format_build_message(build_info)

    # Safe update preserving files
    print(f"[INFO] Updating Slack message (ts={slack_ts}) safely...", flush=True)
    success = SlackCommand.safe_update_message(channel_id, slack_ts, message)

    if success:
        print("[INFO] Slack message updated with final build status.", flush=True)
    else:
        print("[WARN] Failed to update Slack message.", flush=True)


if __name__ == "__main__":
    sys.stdout.reconfigure(line_buffering=True)
    sys.stderr.reconfigure(line_buffering=True)
    main()