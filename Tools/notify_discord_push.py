#!/usr/bin/env python3

import os
import json
import notify_discord
import requests

COMMITS = json.loads(os.environ["COMMITS"])
PR_NOTIFY_WEBHOOK = os.environ["PR_NOTIFY_WEBHOOK"]
DEVELOPER_GITHUB_TOKEN = os.environ["DEVELOPER_GITHUB_TOKEN"]

# божи упаси(дебаг строка)
def main():
    organizations = [
        'corvax-nexus',
        'Wl-Developers',
    ]

    headers = {
        'Authorization': f'token {DEVELOPER_GITHUB_TOKEN}'
    }

    content = ""

    peoples = []

    # Получаем участников ВЛьских организаций, чтобы потом отсеивать 'не наши' коммиты. 
    # А зачем нам коммиты основы или... оффов?? Незачем!
    for org_name in organizations:
        members_api_url = f'https://api.github.com/orgs/{org_name}/members'

        response = requests.get(members_api_url, headers=headers)
        response.raise_for_status()

        members = response.json()
  
        for member in members:
            member_api_url = member['url']
            response = requests.get(member_api_url, headers=headers)
            response.raise_for_status()

            member_json = response.json()
            member_name = member_json["name"]

            if (member_name not in peoples):
                peoples.append(str(member_name))

    for commit in COMMITS:
        author = commit["author"]["name"]
        if (author not in peoples):
            print(f"{author} не был найден среди участников вайтлист организаций: {peoples}!")
            continue

        message = commit['message']
        content += f"{notify_discord.format_body(message)}\n"

    if content == "" or content.isspace():
        print("В тексте не найдено :cl: тега и элементов add/remove... и т.д.")
        print("Ошибка при отправке оповещения. Оно было пустым!")
        return

    notify_discord.send_discord(content, PR_NOTIFY_WEBHOOK)

if __name__ == '__main__':
    main()
