import os
import subprocess

import Config
import TimestampUtils


def run_command(command):
    return subprocess.run(command, stdout=subprocess.PIPE, shell=True).stdout.decode('utf-8')


DEVELOPER_NAME = run_command("git log -1 --pretty=format:%cn")
BRANCH_NAME = os.environ["BRANCH_NAME"]
COMMIT_MESSAGE = run_command("git log -1 --pretty=format:%B")
COMMIT_SHORT_HASH = run_command(f'git log -1 --pretty=format:%h')
PROJECT_NAME = Config.read(Config.KEY.PROJECT_NAME)


def get_build_status_emoji(result, building=False):
    emoji_map = {
        'SUCCESS': '✅',
        'FAILURE': '❌',
        'UNSTABLE': '⚠️',
        'ABORTED': '🛑',
        'NOT_BUILT': '⏸️',
        'BUILDING': ':loading-jump-bar:'
    }
    if building:
        return emoji_map['BUILDING']
    return emoji_map.get(result, '❓')


def get_file_emoji(filename):
    """Get emoji for file type"""
    ext = os.path.splitext(filename)[1].lower()
    emoji_map = {
        '.apk': ':apk:',
        '.aab': ':aab:',
        '.ipa': '🍎',
        '.xapk': '📦',
        '.zip': '🗜️', '.rar': '🗜️', '.7z': '🗜️', '.tar': '🗜️', '.gz': '🗜️',
        '.pdf': '📄', '.doc': '📝', '.docx': '📝', '.txt': '📝', '.md': '📝',
        '.csv': '📊', '.xlsx': '📊', '.xls': '📊',
        '.png': '🖼️', '.jpg': '🖼️', '.jpeg': '🖼️', '.gif': '🖼️', '.svg': '🖼️',
        '.json': '⚙️', '.xml': '⚙️', '.yaml': '⚙️', '.yml': '⚙️', '.log': '📋',
        '.unitypackage': '🎮', '.asset': '🎮', '.prefab': '🎮',
    }
    return emoji_map.get(ext, '📎')


def create_progress_bar(percentage):
    filled = int(percentage / 10)
    empty = 10 - filled
    return f"[{'█' * filled}{'░' * empty}] {percentage}%"


def format_build_message(build_info):
    """
    Format build information as Slack message using realtime duration.
    """
    result = build_info.get('result')
    building = result is None
    current_step_name = os.environ.get('BUILD_CURRENT_STEP', "")

    realtime_ms = TimestampUtils.get_realtime_build_duration_ms()
    duration_str = TimestampUtils.format_duration(realtime_ms)

    estimated_ms = build_info.get('estimatedDuration', 0)
    if building and estimated_ms > 0:
        percentage = min(int((realtime_ms / estimated_ms) * 100), 99)
        progress_bar = create_progress_bar(percentage)
    else:
        percentage = 100 if result == 'SUCCESS' else 0
        progress_bar = create_progress_bar(percentage)

    status = current_step_name if building else result
    emoji = get_build_status_emoji(result, building=building)

    message = f"{emoji} *{PROJECT_NAME} #{build_info.get('number', '')}* - {status}\n"

    if result == 'FAILURE':
        err_msg = Config.read(Config.KEY.BUILD_ERROR_MESSAGE)
        if err_msg:
            message += f"*Reason:* `{err_msg}`\n"

    message += f"Branch: `{BRANCH_NAME}`\n"
    message += f"Developer: `{DEVELOPER_NAME}`\n"
    message += f"Commit SHA: `{COMMIT_SHORT_HASH}`\n"
    message += f"Changelog: ```{COMMIT_MESSAGE}```\n"

    if building and estimated_ms > 0:
        message += f"Duration: {duration_str} / {TimestampUtils.format_duration(estimated_ms)} (est.)\n"
        message += f"{progress_bar}\n"
    else:
        message += f"Duration: {duration_str}\n"

    message += f"<{build_info.get('url', '')}|View Build>"

    if result == 'SUCCESS':
        build_target = Config.read(Config.KEY.BUILD_TARGET)
        if 'release' in BRANCH_NAME.lower() and build_target.lower() == 'android':
            internal_test_link = Config.read(Config.KEY.GOOGLE_INTERNAL_TEST_URL)
            if internal_test_link:
                message += f"\n<{internal_test_link}|Internal Test>"

    return message
