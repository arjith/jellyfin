# Development Notes

This repository uses .NET and includes a suite of unit tests. When modifying any code you should run the tests from the repository root with:

```bash
dotnet test
```

If the `dotnet` CLI is missing you can install it with:

```bash
curl -L https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 9.0
export PATH="$HOME/.dotnet:$PATH"
```

## Tasks to Address Known Issues
The `ISSUE_SUMMARY.md` document enumerates several logical issues found during analysis. Each item below summarizes the fix and suggested validation steps.

1. **LibraryMonitor async void**
   - File: `Emby.Server.Implementations/IO/LibraryMonitor.cs`
   - Change `ReportFileSystemChangeComplete` to return `Task`.
   - Update `ILibraryMonitor` and all call sites to await the returned task.
   - **Test:** build and run `dotnet test` to ensure compilation succeeds and existing tests pass.

2. **User rename casing**
   - File: `Jellyfin.Server.Implementations/Users/UserManager.cs`
   - Replace `ToUpper()` calls with `ToUpperInvariant()` to avoid culture issues.
   - **Test:** run unit tests and verify a rename works by running the server and creating/renaming a user.

3. **WebSocket keep-alive exceptions**
   - File: `Emby.Server.Implementations/Session/SessionWebSocketListener.cs`
   - Convert `KeepAliveSockets` from `async void` to `async Task` and update the timer logic to await it.
   - **Test:** execute unit tests and observe server logs during WebSocket activity for unhandled exceptions.

4. **Potential null dereference in session inactivity check**
   - File: `Emby.Server.Implementations/Session/SessionManager.cs`
   - Guard against `LastPausedDate` being `null` before accessing `.Value`.
   - **Test:** run unit tests; optionally simulate a paused session with no timestamp and confirm no crash.

5. **Blocking network change handler**
   - File: `src/Jellyfin.Networking/Manager/NetworkManager.cs`
   - Replace `Thread.Sleep(2000)` in `OnNetworkChange` with an asynchronous delay (`await Task.Delay` inside an async method) or restructure to avoid blocking.
   - **Test:** run tests and monitor network change events to verify the handler no longer blocks.

6. **Unobserved scheduled task triggers**
   - File: `Emby.Server.Implementations/ScheduledTasks/ScheduledTaskWorker.cs`
   - Change `OnTriggerTriggered` to return `Task` and ensure callers await the execution.
   - **Test:** run tests and manually trigger a scheduled task to confirm error handling.

7. **Startup trigger asynchronous void**
   - File: `Emby.Server.Implementations/ScheduledTasks/Triggers/StartupTrigger.cs`
   - Change `Start` to return `Task` and handle the delay using `await Task.Delay`.
   - **Test:** build and run the server to ensure tasks queue at startup without unobserved failures.

8. **WebSocket cleanup callback ignores errors**
   - File: `Emby.Server.Implementations/Session/WebSocketController.cs`
   - Change `OnConnectionClosed` to return `Task` and await the session cleanup call.
   - **Test:** close WebSocket connections while the server runs and check logs for errors.

9. **Blocking read loop in ProgressiveFileStream**
   - File: `MediaBrowser.Controller/Streaming/ProgressiveFileStream.cs`
   - Remove `Thread.Sleep(50)` and rely on asynchronous reads (`await Task.Delay` if throttling is required).
   - **Test:** run performance tests or stream media to verify no unnecessary blocking occurs.

10. **Local time in WeeklyTrigger**
    - File: `Emby.Server.Implementations/ScheduledTasks/Triggers/WeeklyTrigger.cs`
    - Use `DateTime.UtcNow` and `DateTime.UtcNow.Date` instead of `DateTime.Now` for trigger calculations.
    - **Test:** run unit tests and verify trigger times across DST changes.

11. **Ollama web UI blank on Windows/WSL**
    - Investigate server binding and ensure the browser points to `http://localhost:11434` when launching with a local Ollama backend.
    - Document any configuration steps in the README if necessary.
    - **Test:** start the server with Ollama enabled on Windows/WSL and confirm the UI loads.

Follow these guidelines to resolve outstanding issues and keep tests green.
