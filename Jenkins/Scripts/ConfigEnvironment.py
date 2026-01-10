import os
import pathlib
import re
import subprocess

import yaml

import Config

LogFileName = "buildlog.txt"


def run_init():
    build_target_env()
    unity_settings_env()
    git_info_env()


def format_unity_yaml(filepath):
    """
    Description:        Loads a file object from a Unity textual scene file, which is in a pseudo YAML style, and strips the
                        parts that are not YAML 1.1 compliant. Then returns a string as a stream, which can be passed to PyYAML.
                        Essentially removes the "!u!" tag directive, class type and the "&" file ID directive. PyYAML seems to handle
                        rest just fine after that.
    Returns:                String (YAML stream as string)
    """
    result = str()
    sourceFile = open(filepath, 'r')

    for lineNumber, line in enumerate(sourceFile.readlines()):
        if line.startswith('--- !u!'):
            result += '--- ' + line.split(' ')[2] + '\n'  # remove the tag, but keep file ID
        else:
            result += line

    sourceFile.close()

    return result


def unity_settings_env():
    """
        Write project path and project version + project name for fastlane and pipeline to work with
    """
    dir_path = os.path.dirname(os.path.realpath(__file__))
    path = pathlib.Path(dir_path)
    project_dir = path.parent.parent
    Config.write(Config.KEY.UNITY_PROJECT, f'"{project_dir}"')
    log_file_path = os.path.join(project_dir.parent, LogFileName)
    Config.write(Config.KEY.BUILD_LOG_PATH, f'"{log_file_path}"')

    # Read Project setting yaml
    project_setting = format_unity_yaml(os.path.join(project_dir, "ProjectSettings", "ProjectSettings.asset"))
    doc = yaml.safe_load(project_setting)
    Config.write(Config.KEY.COMPANY_NAME, doc["PlayerSettings"]["companyName"])
    Config.write(Config.KEY.PROJECT_NAME, doc["PlayerSettings"]["productName"])
    Config.write(Config.KEY.APP_VERSION, doc["PlayerSettings"]["bundleVersion"])

    build_target = Config.read(Config.KEY.BUILD_TARGET)
    platformName = "Android"
    if build_target == "iOS":
        platformName = "iPhone"
    Config.write(Config.KEY.BUNDLE_ID, doc["PlayerSettings"]["applicationIdentifier"][platformName])

    project_version = os.path.join(project_dir, "ProjectSettings", "ProjectVersion.txt")

    # Read unity version + changelist
    for i, line in enumerate(open(project_version)):
        for match in re.finditer("m_EditorVersion:", line):
            for m in re.finditer(r'(\d+)\.(\d+)\.(\w+)', line):
                Config.write(Config.KEY.UNITY_VERSION, m.group())

        for match in re.finditer("m_EditorVersionWithRevision:", line):
            for m in re.finditer(r'(\d+)\.(\d+)\.(\w+)', line):
                Config.write(Config.KEY.UNITY_VERSION, m.group())

            for m in re.finditer(r'(?<=\().+?(?=\))', line):
                Config.write(Config.KEY.UNITY_CHANGESET, m.group())


def build_target_env():
    branch_name = os.environ.get("BRANCH_NAME", "")
    Config.write(Config.KEY.GIT_BRANCH, branch_name)
    lower_branch_name = branch_name.lower()

    if "-release" in lower_branch_name:
        Config.write(Config.KEY.PIPELINE, "release")
    elif "-dev" in lower_branch_name:
        Config.write(Config.KEY.PIPELINE, "dev")
    elif "-test" in lower_branch_name:
        Config.write(Config.KEY.PIPELINE, "test")
    else:
        Config.write(Config.KEY.PIPELINE, "internal")

    if not Config.contain(Config.KEY.BUILD_TARGET):
        if "ios" in lower_branch_name:
            Config.write(Config.KEY.BUILD_TARGET, "iOS")
        else:
            Config.write(Config.KEY.BUILD_TARGET, "Android")


def run_command(command):
    return subprocess.run(command, stdout=subprocess.PIPE, shell=True).stdout.decode('utf-8')


def git_info_env():
    Config.write(Config.KEY.GIT_AUTHOR_EMAIL, run_command(f'git log -1 --pretty=format:%ae'))
    Config.write(Config.KEY.GIT_AUTHOR_NAME, run_command(f'git log -1 --pretty=format:%an'))
    Config.write(Config.KEY.GIT_AUTHOR, run_command(f'git log -1 --pretty=format:%an'))
    Config.write(Config.KEY.GIT_COMMIT_HASH, run_command(f'git log -1 --pretty=format:%H'))
    Config.write(Config.KEY.GIT_COMMIT_SHORT_HASH, run_command(f'git log -1 --pretty=format:%h'))
    Config.write(Config.KEY.GIT_TREE_HASH, run_command(f'git log -1 --pretty=format:%T'))
    Config.write(Config.KEY.GIT_AUTHOR_DATE, run_command(f'git log -1 --pretty=format:%ai'))
    Config.write(Config.KEY.GIT_COMMITTER_NAME, run_command(f'git log -1 --pretty=format:%cn'))
    Config.write(Config.KEY.GIT_COMMITER, run_command(f'git log -1 --pretty=format:%cn'))
    Config.write(Config.KEY.GIT_COMMITER_EMAIL, run_command(f'git log -1 --pretty=format:%ce'))
    Config.write(Config.KEY.GIT_COMMIT_DATE, run_command(f'git log -1 --pretty=format:%ci'))
    Config.write(Config.KEY.GIT_SUBJECT, run_command(f'git log -1 --pretty=format:%s'))
    Config.write(Config.KEY.GIT_BODY, run_command(f'git log -1 --pretty=format:%b'))
    Config.write(Config.KEY.GIT_RAW_BODY, run_command(f'git log -1 --pretty=format:%B'))
    Config.write(Config.KEY.GIT_COMMIT_MESSAGE, run_command("git log -1 --pretty=format:%B"))


run_init()