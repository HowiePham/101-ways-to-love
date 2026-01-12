import os
import Config
import sys

pipeline = Config.read(Config.KEY.PIPELINE)

if pipeline != "release":
    sys.exit(0)

firebase_app_id = Config.read(Config.KEY.FIREBASE_APP_ID)
unity_project = Config.read(Config.KEY.UNITY_PROJECT)
build_folder = os.path.join(unity_project, "../build")
workingDir = os.path.join(unity_project, "Jenkins/Scripts/Libs/")
os.chdir(workingDir)

for (dirpath, dirnames, filenames) in os.walk(build_folder):
    for file in filenames:
        if file.endswith(".symbols.zip"):
            symbol_path = os.sep.join([dirpath, file])
            print('LOG::: Start uploading symbols to Firebase {path}\n'.format(path=symbol_path))
            cmd = 'firebase crashlytics:symbols:upload --app={firebase_app_id} {file_path}'.format(
                firebase_app_id=firebase_app_id, file_path=symbol_path)
            os.system(cmd)
            print('LOG::: Complete uploading symbols to Firebase {path}\n'.format(path=symbol_path))
            break
