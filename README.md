# 🛡️ KNZ User System (CS2)

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![Target: CounterStrikeSharp](https://img.shields.io/badge/Platform-CounterStrikeSharp-orange.svg)](https://github.com/rokk0/CounterStrikeSharp)
[![Database: MySQL](https://img.shields.io/badge/Database-MySQL-00758f.svg)]()

A lightweight and efficient user tracking system for Counter-Strike 2 servers using **CounterStrikeSharp**. It automatically logs player data (SteamID64, IP, Name) into a MySQL database and assigns unique internal User IDs.

---

## ✨ Features

* 📊 **Automatic Logging:** Saves every player's SteamID64, last known Name, and IP Address on join.
* 🆔 **Internal ID System:** Assigns a unique, incremental ID to every player for easier management.
* 🔍 **Admin Lookup:** Search for player information via Name, SteamID, or internal ID.
* 💾 **MySQL Integration:** Persistent storage with automatic table creation.
* 🎨 **Colored Chat:** Fully customizable chat prefix and color support.
* 📅 **Activity Tracking:** Records the `last_seen` timestamp for every player.

---

## 🛠️ Requirements

* [CounterStrikeSharp](https://github.com/rokk0/CounterStrikeSharp) (Latest Version)
* MySQL / MariaDB Server

---

## 🚀 Installation

1. Download the latest release.
2. Place the `KNZUserSystem` folder into your server's `addons/counterstrikesharp/plugins/` directory.
3. Restart the server or load the plugin manually.
4. A `config.json` file will be generated in `addons/counterstrikesharp/configs/plugins/KNZUserSystem/`.
5. Edit the config with your MySQL credentials and reload.

---

## ⚙️ Configuration

```json
{
  "Host": "",
  "Database": "",
  "User": "",
  "Password": "",
  "Port": 3306,
  "TableName": "user_system",
  "CommandFlag": "@knz/user",
  "ChatPrefix": "{purple}[KNZ] {default}"
}
```

---

## 🪓 Commands

* css_userid <name> - Gives you the info in the database about that player, this one is for the online player.
* css_steamid <userid> - Gives you the info in the database about a userid, this one is for the offline players.
* css_offid <steamid64> - Gives you the info in the database about a steamid64, this one is for offline players aswell.

## ⚡ BE AWARE !

- The lang folder is not generating itself, it has to be in there.
- And the most important note -> The plugin is made entirely with AI, i only know how to talk to AI, i don't have any idea how to do this BUT i gave a try and it worked.
- If you like the idea and are an actual developer you are more than welcome to use this code as a base but it has to remain public to everyone.
- If you would like to help me with this project, the one where i make multiple plugins strictly with AI, you are free to leave suggestions on discord - .kenzo1337
