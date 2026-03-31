"""
Declarative hook runner for Jenkins build lifecycle events.

Usage:
    python run_hooks.py <event_name> [event_name2 ...]

Events:
    after_unity_setup, before_build, after_build_failure,
    after_publish_success, after_publish_failure,
    after_test_success, after_test_failure

Each hook script in the hooks/ directory declares:
    HOOK_EVENTS = ["before_build"]   # or ["*"] for all events
    HOOK_ORDER  = 0                  # lower runs first
    def run(): ...                   # entry point

Backward compatibility:
    Old directory names (e.g. "BeforeBuild") are mapped to new event names
    so the Jenkinsfile can migrate incrementally.
"""

import importlib.util
import os
import sys
import traceback

dir_path = os.path.dirname(os.path.realpath(__file__))

# Map legacy directory names to new event names
LEGACY_EVENT_MAP = {
    "AfterUnitySetup": "after_unity_setup",
    "BeforeBuild": "before_build",
    "AfterBuild": "after_build",
    "AfterBuildFailure": "after_build_failure",
    "AfterPublishSuccess": "after_publish_success",
    "AfterPublishFailure": "after_publish_failure",
    "AfterTestSuccess": "after_test_success",
    "AfterTestFailure": "after_test_failure",
}


def resolve_event_name(name):
    """Resolve legacy directory names to canonical event names."""
    return LEGACY_EVENT_MAP.get(name, name)


def load_module(filepath):
    """Load a Python module from file path without executing module-level code via exec."""
    module_name = os.path.splitext(os.path.basename(filepath))[0]
    spec = importlib.util.spec_from_file_location(module_name, filepath)
    module = importlib.util.module_from_spec(spec)

    # Ensure parent Scripts/ directory is importable (for Config, SlackCommand, etc.)
    if dir_path not in sys.path:
        sys.path.insert(0, dir_path)

    spec.loader.exec_module(module)
    return module


def discover_hooks(hooks_dir, event_name):
    """Find and load all hook modules matching the given event."""
    matched = []

    if not os.path.isdir(hooks_dir):
        print(f"WARNING: hooks directory not found: {hooks_dir}")
        return matched

    for filename in sorted(os.listdir(hooks_dir)):
        if not filename.endswith(".py") or filename.startswith("_"):
            continue

        filepath = os.path.join(hooks_dir, filename)
        try:
            module = load_module(filepath)
        except Exception as e:
            print(f"\033[1;31m[Load Error]\033[0m {filename}: {e}")
            traceback.print_exc()
            continue

        events = getattr(module, "HOOK_EVENTS", [])
        order = getattr(module, "HOOK_ORDER", 0)
        run_fn = getattr(module, "run", None)

        if run_fn is None:
            print(f"\033[1;33m[Skip]\033[0m {filename}: no run() function defined")
            continue

        if "*" in events or event_name in events:
            matched.append((order, filename, run_fn))

    matched.sort(key=lambda x: (x[0], x[1]))
    return matched


def run_hooks(event_name):
    """Execute all hooks matching the given event, in order."""
    hooks_dir = os.path.join(dir_path, "hooks")
    matched = discover_hooks(hooks_dir, event_name)

    if not matched:
        print(f"No hooks registered for event: {event_name}")
        return True

    print(f"\n{'='*60}")
    print(f"Running hooks for event: {event_name}")
    print(f"{'='*60}\n")

    all_success = True
    for order, filename, run_fn in matched:
        print(f"\033[1;33m[Hook]\033[0m {filename} (order={order})")
        try:
            run_fn()
            print(f"\033[1;32m[Success]\033[0m {filename}\n")
        except Exception as e:
            print(f"\033[1;31m[Error]\033[0m {filename}: {e}")
            traceback.print_exc()
            all_success = False
            print()

    return all_success


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: run_hooks.py <event_name> [event_name2 ...]")
        print("Events: after_unity_setup, before_build, after_build_failure, etc.")
        sys.exit(1)

    success = True
    for arg in sys.argv[1:]:
        event = resolve_event_name(arg)
        if event != arg:
            print(f"[Compat] Mapped legacy name '{arg}' -> '{event}'")
        result = run_hooks(event)
        if not result:
            success = False

    sys.exit(0 if success else 1)
