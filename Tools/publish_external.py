#!/usr/bin/env python3

import subprocess
import paramiko
import requests
import os
import io
import hashlib

##GITHUB_TOKEN = os.environ["GITHUB_TOKEN"]
#PUBLISH_TOKEN = os.environ["PUBLISH_TOKEN"]
##GITHUB_REPOSITORY = os.environ["GITHUB_REPOSITORY"]
#VERSION = os.environ['GITHUB_SHA']
#FORK_ID = os.environ['FORK_ID']

LOCALE_PUBLIC_SSH_KEY = os.environ['LOCALE_PUBLIC_SSH_KEY']
LOCALE_PUBLIC_SSH_PORT = os.environ['LOCALE_PUBLIC_SSH_PORT']
LOCALE_PUBLISH_TOKEN = os.environ['LOCALE_PUBLISH_TOKEN']
LOCALE_USER_IP = os.environ['LOCALE_USER_IP']
LOCALE_USER_NAME = os.environ['LOCALE_USER_NAME']
LOCALE_USER_PUBLISH_PATH = os.environ['LOCALE_USER_PUBLISH_PATH']
LOCALE_USER_LISTENER_PORT = os.environ['LOCALE_USER_LISTENER_PORT']

ROBUST_CDN_URL = "https://cdn.station14.ru/"

def main():
    random_bytes = os.urandom(64)
    token = hashlib.sha256(random_bytes).hexdigest()

    pkey = paramiko.RSAKey.from_private_key(io.StringIO(LOCALE_PUBLIC_SSH_KEY))

    print("Содержимое рабочего каталога пакуется в архив.")
    create_archive()
    print("Начинается выгрузка архива на удалённую машину.")
    send_archive(LOCALE_USER_IP, LOCALE_PUBLIC_SSH_PORT, LOCALE_USER_NAME, pkey, LOCALE_USER_PUBLISH_PATH)
    print("Выгрузка прошла успешно!")

    url = f"http://{LOCALE_USER_IP}:{LOCALE_USER_LISTENER_PORT}/publish/work/{token}"
    archive = "/zip"
    '''
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
    '''
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
    cur_dir = os.getcwd()
    parent_dir = os.path.dirname(cur_dir)

    subprocess.run(
        args=['zip', '-r', 'build.zip', '.'], 
        cwd=parent_dir, 
        check=True, 
        stderr=subprocess.PIPE, 
        stdin=subprocess.PIPE, 
        stdout=subprocess.PIPE)

def send_archive(host, port, username, pkey, remote_path):
    ssh = paramiko.SSHClient()
    ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())

    try:
        ssh.connect(
            host, 
            port, 
            username, 
            pkey=pkey,
            look_for_keys=False
            )
        
        sftp = ssh.open_sftp()
        sftp.mkdir(remote_path)
        sftp.put('build.zip', remote_path, print_locale_publish_progress)
    except Exception as e:
        sftp.close()
        ssh.close()
        raise e

    sftp.close()
    ssh.close()
        

def print_locale_publish_progress(uploaded_bytes: int, all_bytes: int):
    print(f"Выгружено {uploaded_bytes} из {all_bytes} байт.")

if __name__ == '__main__':
    main()
