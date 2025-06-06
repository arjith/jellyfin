# Identified Logical Issues

The following code issues were found during analysis. Manual GitHub issues are recommended.

## 1. Unobserved asynchronous call in LibraryMonitor

`Emby.Server.Implementations/IO/LibraryMonitor.cs` declares `ReportFileSystemChangeComplete` as `async void`. Any exceptions are lost and callers cannot await completion, potentially creating multiple overlapping invocations and resource leaks.

## 2. Culture-sensitive casing in user rename

`Jellyfin.Server.Implementations/Users/UserManager.cs` compares usernames using `ToUpper()` without specifying an invariant culture when checking for duplicates. This may fail on cultures with special casing rules (e.g. Turkish).

## 3. Unhandled exceptions in WebSocket keep-alive

`Emby.Server.Implementations/Session/SessionWebSocketListener.cs` defines `KeepAliveSockets` as an `async void` method invoked by a timer. Any exceptions thrown during keep-alive processing are unobserved, risking silent failures and unexpected termination of the timer.

## 4. Potential null dereference in session inactivity check

`Emby.Server.Implementations/Session/SessionManager.cs` uses `(DateTime.UtcNow - i.LastPausedDate).Value` while filtering inactive sessions. If `LastPausedDate` is `null` due to race conditions, this will throw an `InvalidOperationException` and disrupt session cleanup.

## 5. Blocking network change event handler

`src/Jellyfin.Networking/Manager/NetworkManager.cs` calls `Thread.Sleep(2000)` inside the `OnNetworkChange` event handler. This blocks the network change thread for two seconds and may delay event processing under heavy churn.


## 6. Unobserved scheduled task triggers

`Emby.Server.Implementations/ScheduledTasks/ScheduledTaskWorker.cs` defines `OnTriggerTriggered` as `async void`. Exceptions in scheduled tasks are ignored and many triggers could overlap if execution is slow.

## 7. Startup trigger asynchronous void

`Emby.Server.Implementations/ScheduledTasks/Triggers/StartupTrigger.cs` exposes `Start` as an `async void` method. Any failure to queue tasks at startup will not surface and may halt subsequent operations.

## 8. WebSocket cleanup callback ignores errors

`Emby.Server.Implementations/Session/WebSocketController.cs` uses `async void` for `OnConnectionClosed`. Exceptions during session cleanup may go unnoticed and leave resources allocated.

## 9. Inefficient blocking read in file streaming

`MediaBrowser.Controller/Streaming/ProgressiveFileStream.cs` performs `Thread.Sleep(50)` in `Read` loops. This blocks worker threads and throttles throughput under heavy load.

## 10. Local time calculations in weekly trigger

`Emby.Server.Implementations/ScheduledTasks/Triggers/WeeklyTrigger.cs` computes trigger times using `DateTime.Now`. When daylight saving changes occur, tasks may fire at unexpected times.


## 11. Local Ollama web UI fails on Windows/WSL

When launching the server with a local Ollama backend on Windows or WSL, the web browser remains blank. This may be due to wrong host binding or cross-origin configuration. Investigate network listener configuration and ensure the browser points to `http://localhost:11434`.
