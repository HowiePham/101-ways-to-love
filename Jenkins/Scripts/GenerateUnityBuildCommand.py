import os, sys
import pathlib
import Config  # custom class

"""
This class generate command code for UnityBuild.ps1 and not run powershell directly. 

Also create shell command call directly with -shell argument
"""

if len(sys.argv) > 1:
    argument = sys.argv[1]
else:
    print("Missing arguments. Exit")
    exit(1)


BuildScriptName = "UnityBuild.ps1"

LogFileName = "unity_build_log.txt"
Logfile = os.path.join(os.getcwd(), LogFileName)  # use shell pwd not Unity project folder. So jenkins can detect it

UnityBuildMethodName = "FAIL_BUILD_METHOD"
BuildTarget = Config.read(Config.KEY.BUILD_TARGET)
BuildParams = ""

# create path to script and file
dir_path = os.path.dirname(os.path.realpath(__file__))
BuildScript = os.path.join(dir_path, BuildScriptName)
# assume this is Unity project folder
ProjectFolder = pathlib.Path(dir_path).parent

# save log
sys.stdout = open(os.devnull, 'w')
Config.write(Config.KEY.UNITY_BUILD_LOG, Logfile)
sys.stdout = sys.__stdout__

# Get unity path
unityPath = Config.read("UNITY_PATH")  # this one already have two quote around

pipeline = Config.read(Config.KEY.PIPELINE)

BuildMethod = Config.KEY.BUILD_METHOD_NAME

build_method = Config.read(BuildMethod)
if build_method is not None:
    UnityBuildMethodName = build_method

build_target = Config.read(Config.KEY.BUILD_TARGET)
if build_target is not None:
    BuildTarget = build_target

build_params = Config.read(Config.KEY.UNITY_BUILD_PARAMS)
if build_params is not None:
    BuildParams = build_params

# check -powershell
if "-powershell" in argument:
    command = f'{BuildScript} -projectPath "{ProjectFolder}" -buildTarget {BuildTarget} -unityPath "{unityPath}" -logFile {Logfile} -executeMethod {UnityBuildMethodName}'
    if BuildParams is not None and BuildParams != "":
        command += f' -params "{BuildParams}"'  # parse command that work with Window powershell
elif "-shell" in argument:
    command = f'{unityPath} -quit -batchmode -projectPath "{ProjectFolder}" -buildTarget {BuildTarget} -logFile {Logfile} -executeMethod {UnityBuildMethodName}'
    if BuildParams is not None and BuildParams != "":
        command += f' "{BuildParams}"'  # parse command that work with linux shell
else:
    print("Wrong input argument")
    exit(1)

# out put command to Jenkins
print(command)
sys.stdout.flush()  # Return string value to jenkins
