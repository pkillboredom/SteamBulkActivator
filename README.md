# SteamBulkActivatorCLI

[![License](https://img.shields.io/github/license/pkillboredom/SteamBulkActivatorCLI.svg?label=License&maxAge=86400)](./LICENSE)
[![GitHub Release](https://img.shields.io/github/release/pkillboredom/SteamBulkActivatorCLI.svg?label=Latest&maxAge=60)](https://github.com/pkillboredom/SteamBulkActivatorCLI/releases/latest)

Activate multiple Steam keys in bulk by passing a file on the command line.
Adapted from [SteamBulkActivator by Ezzpify](https://github.com/Ezzpify/SteamBulkActivator).

# Download

https://github.com/pkillboredom/SteamBulkActivatorCLI/releases/latest

# How to Use
Create a text file with one key per line, then feed it to the program like so. Steam must be running and you should probably make sure its logged into the correct account.

``` PS> .\SteamBulkActivatorCLI.exe --path .\path\to\keys.txt ```

# FAQ

* **What's the point of this CLI version?**
 * I manage a cluster of PCs with Ansible. I run an Ansible playbook that distributes new keys to each PC and runs this program against it. Saves a few manhours of standing around and clicking.

* **Is this program done?**
 * No, the logging is not done yet and it is only basically tested.

* **How can I trust this program?**
 * The program and all dependencies are Open Source. This means that the source code (the blueprints) of the program are open to the public to read and modify. This means that you can read through it and compile your own version to ensure that there is no malicious code present.
 
* **Can you get VAC banned?**
 * No.
