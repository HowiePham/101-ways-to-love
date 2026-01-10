import os
import sys
import Config
import SlackCommand
import subprocess

sys.path.insert(1, os.path.join(sys.path[0], '..'))

def run_command(command):
    return subprocess.run(command, stdout=subprocess.PIPE, shell=True).stdout.decode('utf-8')

# def get_slack_channel(pipeline_name):
#     if pipeline_name == "release":
#         return SlackCommand.get_channel(Config.read(Config.KEY.SLACK_RELEASE_CHANNEL))
#     elif pipeline_name == "dev":
#         return SlackCommand.get_channel(Config.read(Config.KEY.SLACK_DEV_CHANNEL))
#     else:
#         return SlackCommand.get_channel(Config.read(Config.KEY.SLACK_DEFAULT_CHANNEL))

def get_slack_channel(pipeline_name):
    if pipeline_name == "release":
        return "#ci-release"
    elif pipeline_name == "dev" or pipeline_name == "test":
        return "#ci-dev"
    else:
        return "#ci-dev"

# git info
commit_time = run_command("git log -1 --pretty=format:%ci")
email = run_command("git log -1 --pretty=format:%ae")
committer = run_command("git log -1 --pretty=format:%cn")
git_full_message = run_command("git log -1 --pretty=format:%B")
git_hash = run_command("git log -1 --pretty=format:%h")

# config
project = Config.read(Config.KEY.PROJECT_NAME)
company = Config.read(Config.KEY.COMPANY_NAME)
unity_project = Config.read(Config.KEY.UNITY_PROJECT)
pipeline = Config.read(Config.KEY.PIPELINE)

# jenkins
branch = os.environ["BRANCH_NAME"]
build_id = os.environ["BUILD_NUMBER"]
pipeline_url = os.environ["RUN_DISPLAY_URL"]
build_folder = os.path.join(unity_project, "../build")
build_url = os.environ["BUILD_URL"]
workspace = os.environ["WORKSPACE"]
artifact_url = build_url + "artifact"

# Not use function def due to function not get local variable run in jenkins
msg = f'''\
*{company}|{project}*
{commit_time}
{build_id} - {committer} | {branch}-{git_hash}
```{git_full_message}```
Unity build *SUCCESS*
<{pipeline_url}|Console Log>
'''

print("LOG::: Send artifacts of {p} to channel {c}\n".format(p=pipeline, c=get_slack_channel(pipeline)))
SlackCommand.send_message(get_slack_channel(pipeline), msg)

for (dirpath, dirnames, filenames) in os.walk(build_folder):
    for file in filenames:
        if file.endswith(".apk") or file.endswith(".aab") or file.endswith(".csv"):
            artifact_file = os.sep.join([dirpath, file])
            SlackCommand.send_file(get_slack_channel(pipeline), artifact_file, f"{file}", '')