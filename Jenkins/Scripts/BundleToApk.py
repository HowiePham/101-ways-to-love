import os
import Config
import sys
from zipfile import ZipFile

pipeline = Config.read(Config.KEY.PIPELINE)

if pipeline != "release":
    sys.exit(0)

unity_project = Config.read(Config.KEY.UNITY_PROJECT)
build_folder = os.path.join(unity_project, "../build")
workingDir = os.path.join(unity_project, "Jenkins/Scripts/Libs~/")
os.chdir(workingDir)

for (dirpath, dirnames, filenames) in os.walk(build_folder):
    for file in filenames:
        if file.endswith(".aab"):
            aab_path = os.sep.join([dirpath, file])
            apk_path = aab_path.replace('.aab', '.apk')
            apks_path = aab_path.replace('.aab', '.apks')
            print('LOG::: Start convert aab to apk {path}\n'.format(path=aab_path))
            cmd = 'java -jar bundletool-all-1.18.1.jar build-apks --mode=universal --bundle=\"{aab}\" --output=\"{apks}\"'.format(
                aab=aab_path, apks=apks_path)
            print('LOG::: Convert aab to apk {path}\n'.format(path=cmd))
            os.system(cmd)
            print('LOG::: Start rename extension apks to zip\n')
            apks_zip_path = apks_path.replace('.apks', '.zip')
            os.rename(apks_path, apks_zip_path)
            print('LOG::: Start unzip apks file\n')
            with ZipFile(apks_zip_path, 'r') as zipObj:
                zipObj.extractall()
            os.rename('universal.apk', apk_path)
            print('LOG::: Clean up {path}\n'.format(path=apks_zip_path))
            os.remove(apks_zip_path)
            print('LOG::: Complete convert {path}\n'.format(path=aab_path))
