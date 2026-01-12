import os
import json
import sys
import requests
from google.oauth2.service_account import Credentials
from google.auth.transport.requests import Request
import Config


def get_access_token():
    cred_file = os.environ.get("GCLOUD_SERVICE_ACCOUNT_CREDENTIALS")
    if not cred_file or not os.path.exists(cred_file):
        raise RuntimeError("GCLOUD_SERVICE_ACCOUNT_CREDENTIALS not set or file missing")

    scopes = ["https://www.googleapis.com/auth/firebase.remoteconfig"]
    creds = Credentials.from_service_account_file(cred_file, scopes=scopes)
    creds.refresh(Request())
    return creds.token


def load_project_id():
    gs_path = os.path.join(
        os.path.dirname(os.path.abspath(__file__)),
        "../../Assets/google-services.json"
    )

    if not os.path.exists(gs_path):
        raise FileNotFoundError(f"google-services.json not found: {gs_path}")

    with open(gs_path, "r", encoding="utf-8") as f:
        data = json.load(f)

    return data["project_info"]["project_id"]


def get_remote_config(project_id: str, access_token: str):
    url = f"https://firebaseremoteconfig.googleapis.com/v1/projects/{project_id}/remoteConfig"

    response = requests.get(url, headers={
        "Authorization": f"Bearer {access_token}",
        "Accept": "application/json"
    })

    if response.status_code != 200:
        print(f"Error fetching Remote Config: {response.status_code}")
        print(f"Response text: {response.text}")
        raise RuntimeError("Remote Config API request failed")

    return response.json()


def get_conditions(project_id: str, access_token: str):
    config = get_remote_config(project_id, access_token)
    return config.get("conditions", [])


def validate_version_in_conditions(conditions, version_code: str) -> bool:
    version_code = str(version_code).strip()
    found = False

    for c in conditions:
        if version_code in c.get("name", "") or version_code in c.get("expression", ""):
            print(f"Matched condition: {c.get('name')}")
            found = True

    if not found:
        print(f"Remote Config does NOT include version: {version_code}")
        return False

    print("Version validation PASSED")
    return True


if __name__ == "__main__":
    try:
        print("Running Remote Config Validator")

        token = get_access_token()
        project_id = load_project_id()

        conditions = get_conditions(project_id, token)
        print(f"Found {len(conditions)} conditions")

        for c in conditions:
            print(f"- {c.get('name')}")

        version = Config.read(Config.KEY.APP_VERSION)

        if not version:
            print("APP_VERSION not provided - skipping check")
            sys.exit(0)

        print(f"Validating version: {version}")
        result = validate_version_in_conditions(conditions, version)

        if not result:
            Config.write(Config.KEY.BUILD_ERROR_MESSAGE, f"Remote Config validation failed! Missing config for new version: {version}")
            sys.exit(1)

        print("Validation passed")

    except Exception as e:
        print(f"Fatal Error: {e}")
        sys.exit(1)