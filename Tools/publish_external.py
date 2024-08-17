#!/usr/bin/env python3

import subprocess
import paramiko
import requests
import os
import hashlib

GITHUB_TOKEN = os.environ["GITHUB_TOKEN"]
PUBLISH_TOKEN = os.environ["PUBLISH_TOKEN"]
GITHUB_REPOSITORY = os.environ["GITHUB_REPOSITORY"]
VERSION = os.environ['GITHUB_SHA']
FORK_ID = os.environ['FORK_ID']

LOCALE_PUBLIC_SSH_KEY = os.environ['LOCALE_PUBLIC_SSH_KEY']
LOCALE_PUBLIC_SSH_PORT = os.environ['LOCALE_PUBLIC_SSH_PORT']
LOCALE_PUBLISH_TOKEN = os.environ['LOCALE_PUBLISH_TOKEN']
LOCALE_USER_IP = os.environ['LOCALE_USER_IP']
LOCALE_USER_NAME = os.environ['LOCALE_USER_NAME']
LOCALE_USER_PUBLISH_PATH = os.environ['LOCALE_USER_PUBLISH_PATH']
LOCALE_USER_LISTENER_PORT = os.environ['LOCALE_USER_LISTENER_PORT']

#
# CONFIGURATION PARAMETERS
# Forks should change these to publish to their own infrastructure.
#

ROBUST_CDN_URL = "https://cdn.station14.ru/"

def main():
    random_bytes = os.urandom(64)
    token = hashlib.sha256(random_bytes).hexdigest()

    pkey = paramiko.RSAKey.from_private_key(LOCALE_PUBLIC_SSH_KEY)

    create_archive()
    send_archive(LOCALE_USER_IP, LOCALE_PUBLIC_SSH_PORT, LOCALE_USER_NAME, pkey, LOCALE_USER_PUBLISH_PATH)

    url = f"http://{LOCALE_USER_IP}:{LOCALE_USER_LISTENER_PORT}/publish/work/{token}"
    archive = "/zip"

    print("Начинается отправка на cdn.")
    publish_data = {
        "version": VERSION,
        "engineVersion": get_engine_version(),
        "archive": url + archive
    }
    publish_headers = {
        "Authorization": f"Bearer {PUBLISH_TOKEN}",
        "Content-Type": "application/json"
    }
    publish_resp = requests.post(f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish", json=publish_data, headers=publish_headers)
    publish_resp.raise_for_status()
    print("Отправка на cdn успешна!")

    print("Начинается удаление выгруженного архива.")
    delete_head = {
        "Authorization": f"Bearer {LOCALE_PUBLISH_TOKEN}",
    }
    delete_resp = requests.delete(url, headers=delete_head)
    delete_resp.raise_for_status()
    print("Удаление выгруженного архива прошло успешно!")


def get_engine_version() -> str:
    return "229.1.2"

def create_archive():
    subprocess.run(['zip', '-r', 'build.zip', '.'])

def send_archive(host, port, username, pkey, remote_path):
    ssh = paramiko.SSHClient()
    ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    
    try:
        ssh.connect(host, port, username, pkey=pkey)
        
        sftp = ssh.open_sftp()
        sftp.mkdir(remote_path)
        sftp.put('build.zip', remote_path)
    finally:
        sftp.close()
        ssh.close()

if __name__ == '__main__':
    main()
