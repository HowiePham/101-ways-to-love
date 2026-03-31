"""Shared utilities for Jenkins CI scripts."""
import os
import subprocess
import sys


def run_command(command: str) -> str:
    """Run a shell command and return stripped stdout. Returns empty string on failure."""
    try:
        result = subprocess.run(
            command, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
            shell=True, text=True
        )
        return result.stdout.strip()
    except Exception as e:
        print(f"[WARN] Command failed: {command} — {e}", flush=True)
        return ""


def git_log(fmt: str) -> str:
    """Shorthand for `git log -1 --pretty=format:<fmt>`."""
    return run_command(f'git log -1 --pretty=format:{fmt}')


def require_env(name: str) -> str:
    """Get required environment variable or exit with error."""
    value = os.environ.get(name, "").strip()
    if not value:
        print(f"[ERROR] Required environment variable not set: {name}", flush=True)
        sys.exit(1)
    return value


def get_env(name: str, default: str = "") -> str:
    """Get optional environment variable with default."""
    return os.environ.get(name, default).strip()


def configure_line_buffering():
    """Enable line-buffered stdout/stderr for real-time Jenkins log output."""
    sys.stdout = os.fdopen(sys.stdout.fileno(), 'w', buffering=1)
    sys.stderr = os.fdopen(sys.stderr.fileno(), 'w', buffering=1)
