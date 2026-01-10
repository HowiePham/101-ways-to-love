# import os
# import Config
# import sys
# from zipfile import ZipFile
# from os.path import basename
# import subprocess
# import shutil
# from pathlib import Path
# sys.path.insert(1, os.path.join(sys.path[0], '..'))
# 
# def run_command(command):
#     return subprocess.run(command, stdout=subprocess.PIPE, shell=True).stdout.decode('utf-8')
# 
# # git info
# commit_time = run_command("git log -1 --pretty=format:%ci").split(' +')[0].replace(' ', '--').replace(':', '-')
# git_hash = run_command("git log -1 --pretty=format:%h")
# project = Config.read(Config.KEY.PROJECT_NAME)
# branch = os.environ["BRANCH_NAME"]
# build_id = os.environ["BUILD_NUMBER"]
# 
# build_target = Config.read(Config.KEY.BUILD_TARGET)
# unity_project = Config.read(Config.KEY.UNITY_PROJECT)
# version = Config.read(Config.KEY.APP_VERSION)
# archive_folder = os.path.join(unity_project, "../archives/{platform}".format(platform=build_target))
# build_folder = os.path.join(unity_project, "../build")
# archive_fname = "{id}--{project}--{version}--{branch}--{time}--{hash}.zip".format(id=build_id, project=project, version=version, branch=branch, time=commit_time, hash=git_hash)
# 
# Path(archive_folder).mkdir(parents=True, exist_ok=True)
# 
# for (dirpath, dirnames, filenames) in os.walk(build_folder):
#     for dirname in dirnames:
#         if dirname == build_target:
#             archive_dest = os.path.join(archive_folder, archive_fname)
#             build_folder_path = os.path.join(dirpath, dirname)
# 
#             for (builddirpath, builddirnames, buildfilenames) in os.walk(dirpath):
#                 for fname in buildfilenames:
#                     if fname.endswith(".symbols.zip"):
#                         symbols_src = os.path.join(build_folder_path, fname)
#                         symbols_dest = os.path.join(build_folder_path, fname)
#                         print('LOG::: Starting moving symbols {fname} from {src} to {dest}'.format(fname=fname, src=symbols_src, dest=symbols_dest))
#                         shutil.move(symbols_src, symbols_dest)
#                         print('LOG::: Complete moving symbols {fname}'.format(fname=fname))
# 
#             print('LOG::: Starting zip archive {archive_file}'.format(archive_file=archive_fname))
#             zip_path = os.path.join(build_folder_path, archive_fname)
#             with ZipFile(zip_path, 'w') as zipObj:
#                 for (builddirpath, builddirnames, buildfilenames) in os.walk(dirpath):
#                     for fname in buildfilenames:
#                         if fname.endswith(".zip"):
#                             continue
#                         if fname.endswith(".apk") or fname.endswith(".aab") or fname.endswith(".json") or fname.endswith(".csv"):
#                             print('LOG::: Zipping {fname}'.format(fname=fname))
#                             filepath = os.path.join(builddirpath, fname)
#                             zipObj.write(filepath, basename(filepath))
#             shutil.move(zip_path, archive_dest)
#             print('LOG::: Complete zip archive {archive_file} to {path}'.format(archive_file=archive_fname, path=archive_dest))