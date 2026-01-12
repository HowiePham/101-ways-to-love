#!/usr/bin/env python3
import argparse
import os
import sys
import time

import Config
import JenkinsBuildClient
import SlackCommand
from UnityLogParser import UnityLogParser

sys.path.insert(1, os.path.join(sys.path[0], '..'))

import requests


def extract_first_error(console_text: str) -> str:
    """
    Extract first meaningful error line from Jenkins console log.
    Priority rules:
    - Lines containing script returned exit code
    - Lines containing ERROR, Exception, Traceback
    """
    keywords = ["script returned exit code", "ERROR", "Exception", "Traceback"]

    for line in console_text.splitlines():
        stripped = line.strip()
        if any(k in stripped for k in keywords):
            return stripped

    # fallback: return first non-empty line
    for line in console_text.splitlines():
        if line.strip():
            return line.strip()

    return ""


def post_console_error(jenkins_user, jenkins_token):
    """
    Post top-level Jenkins failure reason to Slack thread.
    Source priority:
    1. currentBuild.description (if exists)
    2. First error line parsed from consoleText
    """
    build_url = os.environ.get("BUILD_URL")
    slack_thread_ts = Config.read(Config.KEY.SLACK_MESSAGE_TS)

    if not build_url or not slack_thread_ts:
        print("Missing BUILD_URL or SLACK_MESSAGE_TS, skipping error post", flush=True)
        return

    build_info = JenkinsBuildClient.get_build_info(build_url, jenkins_user, jenkins_token)
    desc = build_info.get("description", "") if build_info else ""

    if desc:
        print(f"Posting Jenkins error description: {desc}", flush=True)
        SlackCommand.post_message(
            SlackCommand.get_slack_channel_id(),
            f":x: {desc}",
            thread_ts=slack_thread_ts
        )
        return

    # No description -> Try consoleText
    console_url = build_url.rstrip("/") + "/consoleText"
    print(f"Fetching console log: {console_url}", flush=True)

    try:
        resp = requests.get(console_url, auth=(jenkins_user, jenkins_token), timeout=10)
        if resp.status_code == 200:
            error_line = extract_first_error(resp.text)
            if error_line:
                slack_msg = f":x: {error_line}"
                print(f"Posting parsed console error: {error_line}", flush=True)
                SlackCommand.post_message(
                    SlackCommand.get_slack_channel_id(),
                    slack_msg,
                    thread_ts=slack_thread_ts
                )
        else:
            print(f"Failed to fetch console log: {resp.status_code}", flush=True)
    except Exception as e:
        print(f"Error fetching consoleText: {e}", flush=True)


def parse_and_post_build_errors():
    """Parse Unity build log and post errors to Slack thread."""
    try:
        build_log_path = Config.read(Config.KEY.BUILD_LOG_PATH)
        slack_thread_ts = Config.read(Config.KEY.SLACK_MESSAGE_TS)

        if not slack_thread_ts:
            print("Missing SLACK_MESSAGE_TS — cannot post to Slack thread.", flush=True)
            return

        if not build_log_path or not os.path.exists(build_log_path):
            msg = f"Build log not found at path: {build_log_path}"
            print(msg, flush=True)
            # SlackCommand.post_message(SlackCommand.get_slack_channel_id(), msg, thread_ts=slack_thread_ts)
            return

        print(f"[{time.strftime('%Y-%m-%d %H:%M:%S')}] Parsing build log: {build_log_path}", flush=True)

        parser = UnityLogParser()
        result = parser.parse_file(build_log_path)

        if not result:
            msg = "Failed to parse build log."
            print(msg, flush=True)
            SlackCommand.post_message(SlackCommand.get_slack_channel_id(), msg, thread_ts=slack_thread_ts)
            return

        if not result.get('success', False):
            detailed_errors = parser.generate_detail_report()
            SlackCommand.post_message(
                SlackCommand.get_slack_channel_id(),
                detailed_errors,
                thread_ts=slack_thread_ts
            )
            print(f"[{time.strftime('%Y-%m-%d %H:%M:%S')}] Posted detailed error report to Slack thread", flush=True)
        else:
            msg = "No build errors found in log. Please review manually."
            print(msg, flush=True)
            SlackCommand.post_message(SlackCommand.get_slack_channel_id(), msg, thread_ts=slack_thread_ts)

    except Exception as e:
        msg = f"Error analyzing build log: {e}"
        print(msg, flush=True)
        SlackCommand.post_message(SlackCommand.get_slack_channel_id(), msg,
                                  thread_ts=os.environ.get("SLACK_MESSAGE_TS", ""))


def parse_args():
    parser = argparse.ArgumentParser(description="Update final Slack message after Jenkins build finishes")
    parser.add_argument("--jenkins-user", required=True, help="Jenkins username")
    parser.add_argument("--jenkins-token", required=True, help="Jenkins API token")
    return parser.parse_args()


if __name__ == "__main__":
    sys.stdout = os.fdopen(sys.stdout.fileno(), 'w', buffering=1)
    sys.stderr = os.fdopen(sys.stderr.fileno(), 'w', buffering=1)

    print(f"[{time.strftime('%Y-%m-%d %H:%M:%S')}] Starting ParseAndPostErrors.py", flush=True)
    args = parse_args()
    post_console_error(args.jenkins_user, args.jenkins_token)
    parse_and_post_build_errors()
    print(f"[{time.strftime('%Y-%m-%d %H:%M:%S')}] Completed ParseAndPostErrors.py", flush=True)
