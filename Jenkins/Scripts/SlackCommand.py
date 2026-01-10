from slack_sdk import WebClient
from slack_sdk.errors import SlackApiError
import Config
import certifi
import ssl
# add this line to point to your certificate path
ssl_context = ssl.create_default_context(cafile=certifi.where())

# Read doc https://api.slack.com/methods/files.upload

slack_token = Config.read(Config.KEY.SLACK_BOT_TOKEN).strip().replace('"', "")
slack_client = WebClient(token=slack_token, timeout=600, ssl=ssl_context)
slack_default_channel = Config.read(Config.KEY.SLACK_DEFAULT_CHANNEL).replace('"', "")


def send_message(channel, msg):
    try:
        response = slack_client.chat_postMessage(channel=channel, text=msg)
        # print(response)
    except SlackApiError as e:
        # print(f"response: {e.response}")
        print(f'Send message to channel {channel} error!')


def send_direct_message(email, msg):
    try:
        response = slack_client.chat_postMessage(channel=find_user_id(email), text=msg)
        # print(response)
    except SlackApiError as e:
        # print(f"response: {e.response}")
        print(f'Send direct message to email {email} error!')


def send_file(channel, file_path, file_title, comment):
    try:
        response = slack_client.files_upload(channels=channel, file=file_path, title=file_title,
                                             initial_comment=comment)
        # print(response)
    except SlackApiError as e:
        # print(f"response: {e.response}")
        print(f'Send file to channel {channel} error!')


def send_apk(channel, file_path):
    try:
        response = slack_client.files_upload(channels=channel, file=file_path, filetype="apk")
        # print(response)
    except SlackApiError as e:
        # print(f"response: {e.response}")
        print(f'Send apk to channel {channel} error!')


def get_channel(channel_text):
    if not channel_text:
        return "#" + channel_text.strip().replace("#", "")
    else:
        return "#" + slack_default_channel.strip().replace("#", "")


def find_user_id(email):
    response = slack_client.users_list(include_locale=True)
    for page in response:
        users = page['members']
        for user in users:
            profiles = user['profile']
            for k, v in profiles.items():
                if k == "email" and v == email:
                    user_id = user["id"]
                    return user_id


def get_user_mention(email):
    response = slack_client.users_list(include_locale=True)
    for page in response:
        users = page['members']
        for user in users:
            profiles = user['profile']
            for k, v in profiles.items():
                if k == "email" and v == email:
                    user_id = user["id"]
                    return f"<@{user_id}>"


def get_mention_list(input: str):
    def get_mention(p):
        for k, v in p.items():
            if k == "email" or k == "real_name_normalized" or k == "display_name_normalized":
                if input in str(v).lower():
                    user_id = user["id"]
                    print(f"found: " + v)
                    return f"<@{user_id}>"
        return None

    users_found = []
    response = slack_client.users_list(include_locale=True)
    # print(response)
    for page in response:
        users = page['members']
        for user in users:
            profiles = user['profile']
            found = get_mention(profiles)
            if found is not None:
                users_found.append(found)
    print(" ".join(users_found))