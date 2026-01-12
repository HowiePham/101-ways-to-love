import time
import requests


def get_build_info(job_url, jenkins_user, jenkins_token):
    """
    Fetch build info from Jenkins REST API.

    Args:
        job_url (str): Jenkins job URL (e.g. http://jenkins/job/MyApp/job/release/5/)
        jenkins_user (str): Jenkins username
        jenkins_token (str): Jenkins API token

    Returns:
        dict: JSON from Jenkins API, or None if error
    """
    clean_url = job_url.rstrip('/').replace("'", "")
    url = f"{clean_url}/api/json"

    try:
        response = requests.get(url, auth=(jenkins_user, jenkins_token), timeout=10)
        response.raise_for_status()
        build_info = response.json()

        # building flag from Jenkins API
        if 'result' not in build_info or build_info['result'] is None:
            build_info['building'] = True
        else:
            build_info['building'] = False

        return build_info

    except requests.exceptions.RequestException as e:
        print(f"[ERROR] Error fetching build info: {e}", flush=True)
        return None


def get_console_output(job_url, build_number, jenkins_user, jenkins_token, start=0):
    """
    Fetch Jenkins console output progressively.

    Returns:
        (text, more_data, next_start)
    """
    url = f"{job_url.rstrip('/')}/{build_number}/logText/progressiveText"
    params = {'start': start}

    try:
        response = requests.get(url, auth=(jenkins_user, jenkins_token), params=params, timeout=10)
        more_data = response.headers.get('X-More-Data', 'false') == 'true'
        text_size = int(response.headers.get('X-Text-Size', 0))
        return response.text, more_data, text_size
    except requests.exceptions.RequestException as e:
        print(f"[ERROR] Error fetching console output: {e}", flush=True)
        return "", False, start