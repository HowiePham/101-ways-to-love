import os
import sys
import time
import argparse

# Ensure sibling modules are importable when script dir is not in sys.path
_SCRIPTS_DIR = os.path.dirname(os.path.abspath(__file__))
if not any(os.path.normcase(_SCRIPTS_DIR) == os.path.normcase(p) for p in sys.path):
    sys.path.insert(0, _SCRIPTS_DIR)

import build_config
import jenkins_build_client
import slack_command
import slack_message_formatter
import utils


class JenkinsSlackBot:
    def __init__(self, jenkins_user, jenkins_token):
        self.jenkins_user = jenkins_user
        self.jenkins_token = jenkins_token
        self.stop_monitoring = False

    def monitor_build(self, build_url, build_number, update_interval=10):
        print(f"[INFO] Monitoring build {build_url} #{build_number}...", flush=True)
        message_ts = os.environ.get("SLACK_MESSAGE_TS")

        build_info = jenkins_build_client.get_build_info(build_url, self.jenkins_user, self.jenkins_token)
        if not build_info:
            print("[ERROR] Failed to get initial build info", flush=True)
            return

        initial_message = slack_message_formatter.format_build_message(build_info)
        if message_ts:
            slack_command.update_message(slack_command.get_slack_channel_id(), message_ts, initial_message)
        else:
            message_ts = slack_command.post_message(slack_command.get_slack_channel_id(), initial_message)

        if not message_ts:
            print("[ERROR] Failed to send initial Slack message", flush=True)
            return

        print(f"[INFO] Monitoring Slack message TS: {message_ts}", flush=True)

        while not self.stop_monitoring:
            time.sleep(update_interval)
            build_info = jenkins_build_client.get_build_info(build_url, self.jenkins_user, self.jenkins_token)
            if not build_info:
                print(f"[WARN] Failed to fetch build info, retrying...", flush=True)
                continue

            if build_info.get("result") is None:
                updated_message = slack_message_formatter.format_build_message(build_info)
                slack_command.safe_update_message(slack_command.get_slack_channel_id(), message_ts, updated_message)
            else:
                print(f"[INFO] Build finished with result: {build_info.get('result')}", flush=True)
                break

        if self.stop_monitoring:
            print(f"[INFO] Monitoring stopped by signal", flush=True)

        # Final Slack update
        build_info = jenkins_build_client.get_build_info(build_url, self.jenkins_user, self.jenkins_token)
        if build_info:
            final_message = slack_message_formatter.format_build_message(build_info)
            slack_command.safe_update_message(slack_command.get_slack_channel_id(), message_ts, final_message)
            print(f"[INFO] Final Slack message updated: {build_info.get('result')}", flush=True)
        else:
            print("[ERROR] Failed to fetch final build info", flush=True)


def parse_arguments():
    parser = argparse.ArgumentParser(description="Monitor Jenkins build progress and update Slack message")
    parser.add_argument('--job-url', required=True, help='Jenkins job URL')
    parser.add_argument('--build-number', required=True, help='Build number to monitor')
    parser.add_argument('--jenkins-user', default=os.getenv('JENKINS_USER'), help='Jenkins username')
    parser.add_argument('--jenkins-token', default=os.getenv('JENKINS_TOKEN'), help='Jenkins API token')
    parser.add_argument('--pipeline-name', default=os.getenv('PIPELINE', 'dev'), help='Pipeline name')
    parser.add_argument('--slack-token', default=os.getenv('SLACK_BOT_TOKEN'), help='Slack bot token')
    parser.add_argument('--interval', type=int, default=10, help='Update interval in seconds')
    parser.add_argument('--verbose', action='store_true', help='Enable verbose logging')

    args = parser.parse_args()
    missing = [k for k in ['job_url', 'jenkins_user', 'jenkins_token', 'slack_token'] if not getattr(args, k, None)]
    if missing:
        parser.error(f"Missing required credentials: {', '.join(missing)}")

    return args


if __name__ == "__main__":
    utils.configure_line_buffering()

    args = parse_arguments()

    if args.verbose:
        print(f"[INFO] Starting Jenkins Slack Bot", flush=True)
        print(f"Jenkins user: {args.jenkins_user}", flush=True)
        print(f"Job URL: {args.job_url}", flush=True)
        print(f"Build number: {args.build_number}", flush=True)
        print(f"Pipeline: {args.pipeline_name}", flush=True)
        print(f"Update interval: {args.interval}s", flush=True)
        print("-" * 50, flush=True)

    bot = JenkinsSlackBot(jenkins_user=args.jenkins_user, jenkins_token=args.jenkins_token)

    try:
        bot.monitor_build(build_url=args.job_url, build_number=args.build_number, update_interval=args.interval)
    except KeyboardInterrupt:
        print(f"[INFO] Monitoring interrupted by user", flush=True)
    except Exception as e:
        print(f"[ERROR] {e}", flush=True)
        if args.verbose:
            import traceback
            traceback.print_exc()
        sys.exit(1)

    print(f"[INFO] Monitoring completed successfully", flush=True)
    sys.exit(0)