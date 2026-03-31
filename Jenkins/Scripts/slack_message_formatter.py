"""Slack message formatting utilities for build notifications."""
import os
import sys

# Ensure sibling modules are importable when script dir is not in sys.path
_SCRIPTS_DIR = os.path.dirname(os.path.abspath(__file__))
if not any(os.path.normcase(_SCRIPTS_DIR) == os.path.normcase(p) for p in sys.path):
    sys.path.insert(0, _SCRIPTS_DIR)

import build_config
import timestamp_utils
import utils

# Lazy-loaded git info cache — avoids running git commands on import
_git_cache = {}


def _get_git_info(key, fmt):
    if key not in _git_cache:
        _git_cache[key] = utils.git_log(fmt)
    return _git_cache[key]


def developer_name():
    return _get_git_info('developer', '%cn')


def branch_name():
    return utils.get_env('BRANCH_NAME')


def commit_message():
    return _get_git_info('commit_msg', '%B')


def commit_short_hash():
    return _get_git_info('short_hash', '%h')


def project_name():
    return build_config.read(build_config.KEY.PROJECT_NAME) or ""


def get_build_status_emoji(result, building=False):
    emoji_map = {
        'SUCCESS': '\u2705',
        'FAILURE': '\u274c',
        'UNSTABLE': '\u26a0\ufe0f',
        'ABORTED': '\U0001f6d1',
        'NOT_BUILT': '\u23f8\ufe0f',
        'BUILDING': ':loading-jump-bar:'
    }
    if building:
        return emoji_map['BUILDING']
    return emoji_map.get(result, '\u2753')


def get_file_emoji(filename):
    """Get emoji for file type."""
    ext = os.path.splitext(filename)[1].lower()
    emoji_map = {
        '.apk': ':apk:',
        '.aab': ':aab:',
        '.ipa': '\U0001f34e',
        '.xapk': '\U0001f4e6',
        '.zip': '\U0001f5dc\ufe0f', '.rar': '\U0001f5dc\ufe0f', '.7z': '\U0001f5dc\ufe0f',
        '.tar': '\U0001f5dc\ufe0f', '.gz': '\U0001f5dc\ufe0f',
        '.pdf': '\U0001f4c4', '.doc': '\U0001f4dd', '.docx': '\U0001f4dd',
        '.txt': '\U0001f4dd', '.md': '\U0001f4dd',
        '.csv': '\U0001f4ca', '.xlsx': '\U0001f4ca', '.xls': '\U0001f4ca',
        '.png': '\U0001f5bc\ufe0f', '.jpg': '\U0001f5bc\ufe0f', '.jpeg': '\U0001f5bc\ufe0f',
        '.gif': '\U0001f5bc\ufe0f', '.svg': '\U0001f5bc\ufe0f',
        '.json': '\u2699\ufe0f', '.xml': '\u2699\ufe0f', '.yaml': '\u2699\ufe0f',
        '.yml': '\u2699\ufe0f', '.log': '\U0001f4cb',
        '.unitypackage': '\U0001f3ae', '.asset': '\U0001f3ae', '.prefab': '\U0001f3ae',
    }
    return emoji_map.get(ext, '\U0001f4ce')


def create_progress_bar(percentage):
    filled = int(percentage / 10)
    empty = 10 - filled
    bar = '\u2588' * filled + '\u2591' * empty
    return f"[{bar}] {percentage}%"


def format_build_message(build_info):
    """Format build information as Slack message using realtime duration."""
    result = build_info.get('result')
    building = result is None
    current_step_name = os.environ.get('BUILD_CURRENT_STEP', "")

    realtime_ms = timestamp_utils.get_realtime_build_duration_ms()
    duration_str = timestamp_utils.format_duration(realtime_ms)

    estimated_ms = build_info.get('estimatedDuration', 0)
    if building and estimated_ms > 0:
        percentage = min(int((realtime_ms / estimated_ms) * 100), 99)
        progress_bar = create_progress_bar(percentage)
    else:
        percentage = 100 if result == 'SUCCESS' else 0
        progress_bar = create_progress_bar(percentage)

    status = current_step_name if building else result
    emoji = get_build_status_emoji(result, building=building)

    message = f"{emoji} *{project_name()} #{build_info.get('number', '')}* - {status}\n"

    if result == 'FAILURE':
        err_msg = build_config.read(build_config.KEY.BUILD_ERROR_MESSAGE)
        if err_msg:
            message += f"*Reason:* `{err_msg}`\n"

    message += f"Branch: `{branch_name()}`\n"
    message += f"Developer: `{developer_name()}`\n"
    message += f"Commit SHA: `{commit_short_hash()}`\n"
    message += f"Changelog: ```{commit_message()}```\n"

    if building and estimated_ms > 0:
        message += f"Duration: {duration_str} / {timestamp_utils.format_duration(estimated_ms)} (est.)\n"
        message += f"{progress_bar}\n"
    else:
        message += f"Duration: {duration_str}\n"

    message += f"<{build_info.get('url', '')}|View Build>"

    if result == 'SUCCESS':
        build_target = build_config.read(build_config.KEY.BUILD_TARGET)
        if 'release' in branch_name().lower() and build_target and build_target.lower() == 'android':
            internal_test_link = build_config.read(build_config.KEY.GOOGLE_INTERNAL_TEST_URL)
            if internal_test_link:
                message += f"\n<{internal_test_link}|Internal Test>"

    return message
