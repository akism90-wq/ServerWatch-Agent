# ServerWatch User Manual

## Overview

ServerWatch consists of two independent projects:

-   **ServerWatchAgent** -- ASP.NET Core backend running on the Linux
    server.
-   **ServerWatch Wear OS** -- Wear OS client for Samsung/Pebble
    (future).

The backend exposes a REST API consumed by the watch.

------------------------------------------------------------------------

# Architecture

## Development

Wear OS Debug Build → ASCOLTARE (`http://192.168.0.225:5189/status`)

## Production

Wear OS Release Build → grey-area (`http://192.168.0.126:5189/status`)

The debug and release URLs are configured through
`BuildConfig.SERVERWATCH_URL`.

------------------------------------------------------------------------

# ServerWatchAgent

## Development

Run locally:

``` bash
make run
```

Runs on:

    http://0.0.0.0:5189

## Deploy to Production

``` bash
make deploy
```

Performs:

1.  Publish self-contained Linux release.
2.  Package deployment.
3.  Upload to grey-area.
4.  Install.
5.  Restart `serverwatch-agent.service`.
6.  Verify `/status`.

## Deploy Production Configuration

``` bash
make deploy-config
```

Used only when production configuration changes (passwords, thresholds,
etc.).

The production configuration is stored locally as:

    deploy/appsettings.production.json

It is ignored by Git and copied to:

    /opt/serverwatch/app/appsettings.json

------------------------------------------------------------------------

# Production Service

Check status:

``` bash
sudo systemctl status serverwatch-agent.service
```

Logs:

``` bash
sudo journalctl -u serverwatch-agent.service -n 50 --no-pager
```

Health check:

``` bash
curl http://localhost:5189/status
```

------------------------------------------------------------------------

# Wear OS

## Debug Development

Android Studio:

-   Select Emulator or Watch
-   Press Run

Uses:

    ASCOLTARE

No manual APK generation required.

------------------------------------------------------------------------

## Release Deployment

One command:

``` powershell
.\scripts\deploy-watch-release.ps1
```

The script automatically:

1.  Configures JAVA_HOME.
2.  Builds signed release APK.
3.  Detects the connected watch.
4.  Removes previous installation.
5.  Installs release APK.

Release builds always target:

    grey-area

------------------------------------------------------------------------

# Signing

The release keystore is stored outside the repository.

    Documents/AndroidKeys/serverwatch-release.jks

Local signing configuration:

    release-signing.properties

Ignored by Git.

Never commit:

-   release-signing.properties
-   \*.jks
-   \*.keystore

------------------------------------------------------------------------

# Git

Commit source only.

Never commit:

-   appsettings.json
-   production configuration
-   signing keys
-   cookies
-   generated binaries

------------------------------------------------------------------------

# Daily Workflow

## Backend code

``` bash
make deploy
```

## Backend configuration

``` bash
make deploy-config
```

## Watch debug

Run from Android Studio.

## Watch release

``` powershell
.\scripts\deploy-watch-release.ps1
```

------------------------------------------------------------------------

# Future Improvements

-   `make doctor`
-   Automatic production validation
-   Pebble deployment
-   CI/CD pipeline
