# Music Database Manager

A cross-platform .NET Core application that synchronizes multiple local music databases with a central, infallible server. This system ensures that local databases stay in sync with the server, automatically downloading missing files to clients (e.g., laptops, smartphones) on the fly.

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

**Music Database Manager** will allow you to synchronize music across devices (laptops, smartphones, or anything that can run a .NET application). It currently only supports a /Music_Library/Artist/Album folder format and is intended for power users that don't necessarily need a graphical interface (one may be implemented in the future). Client devices can query the master server to see if its library is out of date and immediately begin synchronization through HTTPS.

---

## Features

- **Cross-Platform Support**: Works on Windows, macOS, and Linux.
- **Automatic Sync**: Ensures that missing music files are downloaded from the central server.
- **File Integrity**: Employs SHA256 to ensure that files are not duplicated.
- **API Integration**: Exposes a RESTful API for triggering sync operations.

---

## Installation

### Prerequisites

Before running the project, ensure you have the following installed:

- [.NET 6 SDK or later](https://dotnet.microsoft.com/download)

### Steps

1. Clone the repository:

    ```bash
    git clone https://github.com/yourusername/music-database-manager.git
    cd music-database-manager
    ```

2. Restore the dependencies:

    ```bash
    dotnet restore
    ```

3. Build the application:

    ```bash
    dotnet build
    ```

4. Run the application:

    ```bash
    dotnet run
    ```

---

## Usage

After running the application, the Web API will be available at `http://localhost:5000` (or another port, depending on configuration). You can interact with it through the following API endpoints:

-- under construction --
