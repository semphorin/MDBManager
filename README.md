# Music Database Manager

A (hopefully) cross-platform .NET Core application that synchronizes multiple local music databases with a master server. Docker will be implemented for easy deployment. This system ensures that slave databases stay in sync with the server by automatically downloading missing files to clients (e.g., laptops, smartphones) on the fly. Utilizes JWT+TOTP for stateless authentication.

*This is the server portion of the application and must be queried by an MDBM Client.*
*MDBM Client side is being developed and is not yet available. A link to its repo will be posted on this page when it is ready to be seen.*

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)

---

## Overview


---

## Features


---

## Installation

### Prerequisites

Before running the project, ensure you have the following installed:

- [.NET 8 SDK or later](https://dotnet.microsoft.com/download)

### Steps

1. Clone the repository:

    ```bash
    git clone https://github.com/semphorin/mdbmanager.git
    cd mdbmanager
    ```

2. Restore the dependencies:

    ```bash
    dotnet restore
    ```

3. Set a JWT secret:

    Environment variable method (less secure):
    ```bash
    setx JWT_MDB_SECRET "your_key_here"
    ```

    _____ method (more secure):
    ```bash
    uhhhh lol idk yet
    ```

4. Set your music path in Config/musicpath.yaml:

    ```bash
    musicPath: 'your_music_path_here'
    ```

5. Build and run the application:

    ```bash
    dotnet build
    dotnet run
    ```

6. Set up OTP by visiting this endpoint and scanning the QR code on a device with Google Auth or equivalent:

    ```bash
    /api/auth/generate-qr
    ```

---

## Usage

After port forwarding and setting up all of the necessary configuration including OTP, your MDB server should be ready to interact with the client (W.I.P).

-- under construction --