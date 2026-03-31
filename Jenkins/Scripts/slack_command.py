"""Slack API wrapper for Jenkins CI notifications."""
import os
import ssl
import sys
import traceback

# Ensure sibling modules are importable when script dir is not in sys.path
_SCRIPTS_DIR = os.path.dirname(os.path.abspath(__file__))
if not any(os.path.normcase(_SCRIPTS_DIR) == os.path.normcase(p) for p in sys.path):
    sys.path.insert(0, _SCRIPTS_DIR)

import certifi
from slack_sdk import WebClient
from slack_sdk.errors import SlackApiError

import build_config
import slack_message_formatter

ssl_context = ssl.create_default_context(cafile=certifi.where())

def _resolve_slack_token():
    """Resolve Slack bot token: env var first, then build_config.cfg fallback."""
    token = os.environ.get("SLACK_BOT_TOKEN", "").strip()
    if not token:
        token = (build_config.read(build_config.KEY.SLACK_BOT_TOKEN) or "").strip().replace('"', "")
    if not token:
        print("[Slack] WARNING: No SLACK_BOT_TOKEN found in environment or build_config.cfg")
    return token


slack_token = _resolve_slack_token()
slack_client = WebClient(token=slack_token, timeout=600, ssl=ssl_context)


def post_message(channel_id, text, thread_ts=None):
    try:
        response = slack_client.chat_postMessage(
            channel=channel_id,
            text=text,
            thread_ts=thread_ts
        )
        return response["ts"]
    except SlackApiError as e:
        print(f"[Slack] Error posting message: {e.response['error']}", flush=True)
        return None


def update_message(channel_id, ts, text):
    try:
        slack_client.chat_update(channel=channel_id, ts=ts, text=text)
        print(f"[Slack] Updated message: {ts}", flush=True)
    except SlackApiError as e:
        print(f"[Slack] Error updating message: {e.response['error']}", flush=True)


def delete_message(channel_id, ts):
    try:
        slack_client.chat_delete(channel=channel_id, ts=ts)
        print(f"[Slack] Deleted message: {ts}", flush=True)
        return True
    except SlackApiError as e:
        print(f"[Slack] Error deleting message: {e.response['error']}", flush=True)
        return False


def update_message_with_files(channel_id, ts, text, file_paths):
    """Upload file(s) and update the Slack message with a block containing the file list."""
    try:
        if not file_paths:
            update_message(channel_id, ts, text)
            return True

        file_infos = []
        for file_path in file_paths:
            if not os.path.exists(file_path):
                continue
            response = slack_client.files_upload_v2(
                channel=channel_id,
                file=file_path,
                title=os.path.basename(file_path),
            )
            if response.get("ok"):
                file_name = os.path.basename(file_path)
                file_infos.append({
                    "name": file_name,
                    "url": response["file"]["permalink"],
                    "size": os.path.getsize(file_path) / (1024 * 1024),
                    "emoji": slack_message_formatter.get_file_emoji(file_name)
                })
                print(f"[Slack] Uploaded file: {file_path}", flush=True)

        if not file_infos:
            update_message(channel_id, ts, text)
            return False

        # Build block
        files_text = "\n".join([
            f"{f['emoji']} *<{f['url']}|{f['name']}>* ({f['size']:.2f} MB)"
            for f in file_infos
        ])
        blocks = [
            {"type": "section", "text": {"type": "mrkdwn", "text": text}},
            {"type": "divider"},
            {"type": "section", "text": {"type": "mrkdwn",
                                         "text": f"*Build Artifacts ({len(file_infos)})*\n{files_text}"}}
        ]

        slack_client.chat_update(channel=channel_id, ts=ts, text=text, blocks=blocks)
        print(f"[Slack] Updated message with {len(file_infos)} files", flush=True)
        return True
    except SlackApiError as e:
        print(f"[Slack] Error updating message with files: {e.response['error']}", flush=True)
        return False


def get_message_content(channel_id, ts):
    """
    Return tuple (text, blocks) for the message at channel_id / ts.
    If cannot read, return (None, None).
    """
    try:
        # conversations.replies returns the message thread; the first message is the original
        resp = slack_client.conversations_replies(channel=channel_id, ts=ts, limit=1)
        if not resp.get("ok"):
            print(f"[Slack] get_message_content: ok==False resp={resp}", flush=True)
            return None, None

        messages = resp.get("messages", [])
        if not messages:
            print("[Slack] get_message_content: no messages found", flush=True)
            return None, None

        msg = messages[0]
        text = msg.get("text", "")
        blocks = msg.get("blocks", [])
        # normalize to list
        if blocks is None:
            blocks = []
        return text, blocks
    except SlackApiError as e:
        print(f"[Slack] Error getting message: {e.response.get('error')}", flush=True)
        return None, None
    except Exception as ex:
        print(f"[Slack] Exception in get_message_content: {ex}", flush=True)
        traceback.print_exc()
        return None, None


def safe_update_message(channel_id, ts, new_text):
    """Update Slack message text while preserving file blocks."""
    try:
        old_text, old_blocks = get_message_content(channel_id, ts)

        if not old_blocks:
            slack_client.chat_update(channel=channel_id, ts=ts, text=new_text)
            print("[Slack] Safe update: no blocks", flush=True)
            return True

        # Build new blocks: new text + non-text blocks from old message
        new_blocks = [
            {
                "type": "section",
                "text": {"type": "mrkdwn", "text": new_text}
            }
        ]

        # Keep only non-text blocks (divider, files, etc.)
        for block in old_blocks:
            block_type = block.get("type")

            # Skip first text section (already added above)
            if block_type == "section" and "text" in block and len(new_blocks) == 1:
                continue

            # Keep dividers and other special blocks
            if block_type in ["divider", "context", "actions", "image"]:
                new_blocks.append(block)

            # Keep file/artifact sections
            elif block_type == "section" and "text" in block:
                # This might be a file section - check if it has URLs
                text_content = block["text"].get("text", "")
                if "http" in text_content or "permalink" in text_content:
                    new_blocks.append(block)

        slack_client.chat_update(
            channel=channel_id,
            ts=ts,
            text=new_text,
            blocks=new_blocks
        )

        print(f"[Slack] Updated with {len(new_blocks)} blocks", flush=True)
        return True

    except SlackApiError as e:
        print(f"[Slack] Error: {e.response['error']}", flush=True)
        return False


def append_files_to_message(channel_id, ts, file_paths):
    """Append files to an existing message without losing previously attached files."""
    old_text, old_blocks = get_message_content(channel_id, ts)

    existing_file_lines = []
    if old_blocks and len(old_blocks) >= 3:
        existing_file_lines = old_blocks[-1]["text"]["text"].splitlines()[1:]  # Skip header line

    new_file_infos = []
    for file_path in file_paths:
        if not os.path.exists(file_path):
            continue
        try:
            response = slack_client.files_upload_v2(
                channel=channel_id,
                file=file_path,
                title=os.path.basename(file_path),
            )
            if response.get("ok"):
                file_name = os.path.basename(file_path)
                new_file_infos.append({
                    "name": file_name,
                    "url": response["file"]["permalink"],
                    "size": os.path.getsize(file_path) / (1024 * 1024),
                    "emoji": slack_message_formatter.get_file_emoji(file_name)
                })
                print(f"[Slack] Uploaded new file: {file_path}", flush=True)
        except SlackApiError as e:
            print(f"[Slack] Error uploading {file_path}: {e.response['error']}", flush=True)

    if not new_file_infos:
        return False

    new_files_text = "\n".join([
        f"{f['emoji']} *<{f['url']}|{f['name']}>* ({f['size']:.2f} MB)"
        for f in new_file_infos
    ])
    merged_file_text = "\n".join(existing_file_lines + new_files_text.splitlines())

    updated_blocks = [
        old_blocks[0],
        {"type": "divider"},
        {"type": "section", "text": {"type": "mrkdwn",
                                     "text": f"*Build Artifacts ({len(existing_file_lines) + len(new_file_infos)})*\n{merged_file_text}"}}
    ]

    try:
        slack_client.chat_update(
            channel=channel_id,
            ts=ts,
            text=old_text,
            blocks=updated_blocks
        )
        print(f"[Slack] Appended {len(new_file_infos)} files successfully", flush=True)
        return True
    except SlackApiError as e:
        print(f"[Slack] Error appending files: {e.response['error']}", flush=True)
        return False


def get_slack_channel_id():
    pipeline_name = build_config.read(build_config.KEY.PIPELINE)
    if pipeline_name == "release":
        return build_config.read(build_config.KEY.SLACK_RELEASE_CHANNEL_ID)
    return build_config.read(build_config.KEY.SLACK_DEV_CHANNEL_ID)