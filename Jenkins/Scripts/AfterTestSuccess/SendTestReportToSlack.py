import os
import sys
import Config
import SlackCommand
import subprocess

sys.path.insert(1, os.path.join(sys.path[0], '..'))

def run_command(command):
    return subprocess.run(command, stdout=subprocess.PIPE, shell=True).stdout.decode('utf-8')

def get_slack_channel(pipeline_name):
    if pipeline_name == "release":
        return "#ci-release"
    elif pipeline_name == "dev" or pipeline_name == "test":
        return "#ci-dev"
    else:
        return "#ci-dev"

# git info
commit_time = run_command("git log -1 --pretty=format:%ci")
committer = run_command("git log -1 --pretty=format:%cn")
git_hash = run_command("git log -1 --pretty=format:%h")

# config
unity_project = Config.read(Config.KEY.UNITY_PROJECT)
pipeline = Config.read(Config.KEY.PIPELINE)

# jenkins
build_folder = os.path.join(unity_project, "../build")

for (dirpath, dirnames, filenames) in os.walk(build_folder):
    for file in filenames:
        if file.endswith(".html"):
            artifact_file = os.sep.join([dirpath, file])
            SlackCommand.send_file(get_slack_channel(pipeline), artifact_file, f"{file}", '')