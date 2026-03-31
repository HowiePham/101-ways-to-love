"""Print all current build configuration values. Runs on every lifecycle event."""

import build_config

HOOK_EVENTS = ["*"]
HOOK_ORDER = -100


def run():
    build_config.print_all_config()
