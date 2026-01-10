import os
import sys
import Config
import SlackCommand
import subprocess

sys.path.insert(1, os.path.join(sys.path[0], '..'))

google_internal_test_url = Config.read(Config.KEY.GOOGLE_INTERNAL_TEST_URL)
pipeline = Config.read(Config.KEY.PIPELINE)

def run_command(command):
    return subprocess.run(command, stdout=subprocess.PIPE, shell=True).stdout.decode('utf-8')

def get_slack_channel(pipeline_name):
    if pipeline_name == "release":
        return "#ci-release"
    elif pipeline_name == "dev":
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
Publish to Google Internal Test *SUCCESS*
<{pipeline_url}|Console Log>
<{google_internal_test_url}|Test Link>
'''

print("LOG::: Send Google internal test link {l} of {p} to channel {c}\n".format(l=google_internal_test_url, p=pipeline, c=get_slack_channel(pipeline)))

if google_internal_test_url != "" and pipeline == "release":
    SlackCommand.send_message(get_slack_channel(pipeline), msg)