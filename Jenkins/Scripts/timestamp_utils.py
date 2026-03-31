"""Timestamp parsing and duration formatting for build metrics."""
import os
import time
import warnings

from dateutil import parser
from dateutil.parser import UnknownTimezoneWarning

# Ignore UnknownTimezoneWarning to keep logs clean
warnings.filterwarnings("ignore", category=UnknownTimezoneWarning)

# Map ICT timezone (UTC+7)
TZINFOS = {
    "ICT": 7 * 3600,
    "GMT+7": 7 * 3600,
}

def get_build_timestamp_ms() -> int:
    dt_string = os.environ.get("BUILD_TIMESTAMP")
    if not dt_string:
        return int(time.time() * 1000)

    try:
        dt = parser.parse(dt_string, tzinfos=TZINFOS)
        epoch_ms = int(dt.timestamp() * 1000)
        return epoch_ms
    except Exception:
        # Fallback to current time if parsing fails
        return int(time.time() * 1000)


def get_realtime_build_duration_ms() -> int:
    """
    Calculate the elapsed time since BUILD_TIMESTAMP (in milliseconds).
    Returns 0 if BUILD_TIMESTAMP is invalid or not set.
    """
    start_ms = get_build_timestamp_ms()
    now_ms = int(time.time() * 1000)
    return max(0, now_ms - start_ms)


def format_duration(ms: int) -> str:
    """
    Convert milliseconds into human-readable format like '1h 23m 45s'.
    """
    if ms <= 0:
        return "0s"

    seconds = ms // 1000
    minutes, seconds = divmod(seconds, 60)
    hours, minutes = divmod(minutes, 60)

    if hours > 0:
        return f"{hours}h {minutes}m {seconds}s"
    elif minutes > 0:
        return f"{minutes}m {seconds}s"
    else:
        return f"{seconds}s"