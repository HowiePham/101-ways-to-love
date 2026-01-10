import os
import pathlib
import time
import Config
import sys

dir_path = os.path.dirname(os.path.realpath(__file__))
path = pathlib.Path(dir_path)

branch = sys.argv[1]
git_hash = sys.argv[2]
commit_time = sys.argv[3]
unity_project = Config.read(Config.KEY.UNITY_PROJECT)
bundle_id = Config.read(Config.KEY.BUNDLE_ID)
project_name = Config.read(Config.KEY.PROJECT_NAME)
project_version = Config.read(Config.KEY.APP_VERSION)
html_report_title = project_name + " " + project_version
html_report_filename = f"Test_Report_{project_name}_{project_version}_{branch}_{git_hash}_{commit_time}.html"
html_report_filename = html_report_filename.replace(" ", "")
build_folder = os.path.join(unity_project, "../build/Android")
test_folder = os.path.join(unity_project, "AutoTest/src/tests")
html_report_path = os.path.join(build_folder, html_report_filename)
print("LOG::: Html export at " + html_report_path)
apk_path = ""

for (dirpath, dirnames, filenames) in os.walk(build_folder):
    for file in filenames:
        if file.endswith(".apk"):
            apk_path = os.sep.join([dirpath, file])
            break

os.environ["BUILD_PATH"] = build_folder
os.environ["APPIUM_APPFILE"] = apk_path
os.environ["APPIUM_URL"] = "http://localhost:4723/wd/hub"
os.environ["APPIUM_DEVICE"] = "Local Device"
os.environ["APPIUM_PLATFORM"] = "android"
os.environ["APPIUM_AUTOMATION"] = "uiautomator2"

os.system("echo \"Starting Appium...\"")
# os.system("appium --log-no-colors --log-timestamp  --command-timeout 60")
# os.system("echo \"Installing app on device\"")
# os.system("adb uninstall " + bundle_id)
# os.system("adb install " + apk_path)
# os.system("echo \"Setup ADB port forwarding\"")
# os.system("adb forward --remove-all")
# os.system("adb forward tcp:13000 tcp:13000")
# os.system("echo \"Start the app\"")
# os.system("adb shell am start -n {package_id}/com.unity3d.player.UnityPlayerActivity".format(package_id=bundle_id))
time.sleep(10)
os.system("ps -ef|grep appium")
os.system("echo \"Run the tests\"")
os.system("python -m pytest {test_path} --html-report={report_path} --title=\"{report_title}\"".format(test_path=test_folder, report_path=html_report_path, report_title=html_report_title))
os.system("echo \"Stop the app\"")
os.system("adb shell am force-stop " + bundle_id)


