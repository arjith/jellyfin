# Planned Features and Implementation Tasks

This document proposes several enhancements discovered while reviewing the Jellyfin codebase. Each feature includes context references, implementation tasks, testing steps, and fallback guidance.

## 1. Poster Collage Generation
Lines [620-632 of `SkiaEncoder.cs`](src/Jellyfin.Drawing.Skia/SkiaEncoder.cs) show a TODO for creating a poster collage when images have a tall aspect ratio.

**Tasks**
1. Add `BuildPosterCollage` to `StripCollageBuilder` to arrange images vertically.
2. Update `SkiaEncoder.CreateImageCollage` to call this new method when the collage ratio is below 0.9.
3. Write unit tests in `tests` verifying a collage file is created for poster layouts.
4. Document the new API in `README.md` or other relevant docs.

**Testing**
- Run `dotnet test` to ensure all tests pass.
- Manually generate a poster collage with sample images and verify output dimensions.

**Contingency**
- If tests fail on Skia availability, follow `SkiaEncoder.IsNativeLibAvailable` logic and verify the platform has the required native library.

## 2. Dynamic WebSocket Listener Discovery
The registration block in `CoreAppHost.cs` (lines 91-95) manually adds every `IWebSocketListener` type and has a TODO to search assemblies.

**Tasks**
1. Implement reflection-based scanning for `IWebSocketListener` implementations in loaded assemblies.
2. Replace the manual `AddSingleton<IWebSocketListener, ...>` entries with a loop that registers discovered types.
3. Add unit tests ensuring that listeners from test assemblies are picked up automatically.
4. Update documentation describing how new listeners are automatically loaded.

**Testing**
- Execute `dotnet test` after changes.
- Launch the server and confirm all existing listeners still operate.

**Contingency**
- If a listener fails to resolve, log the assembly names being scanned to diagnose missing references.

## 3. API Error Messages for Saving Lyrics and Subtitles
`LyricManager.cs` and `SubtitleManager.cs` log invalid path errors but do not return feedback to API callers (see lines 400-409 of `LyricManager.cs` and lines 258-264 of `SubtitleManager.cs`).

**Tasks**
1. Validate the computed paths before saving. When invalid, return an HTTP 400 error with a descriptive message.
2. Add tests exercising the failure cases for lyric and subtitle uploads.
3. Update API documentation to mention the new error responses.

**Testing**
- `dotnet test` should remain green.
- Use the web client to upload lyrics or subtitles to an invalid path and verify the API returns 400.

**Contingency**
- If existing clients rely on the previous silent failure, add a configuration toggle to maintain backward compatibility.

## 4. Image Language Preference Handling
Several TMDB image providers include a TODO about respecting image language settings when "All Languages" is disabled (example at lines 84-85 of `TmdbMovieImageProvider.cs`).

**Tasks**
1. Expose the "All Languages" flag and preferred image languages via configuration to the TMDB providers.
2. Update TMDB client calls to include the selected languages when fetching images.
3. Add unit tests verifying language-specific image retrieval.
4. Document how to configure preferred image languages.

**Testing**
- Run `dotnet test` and ensure providers still fetch images.
- Perform metadata refresh with different language settings and confirm the correct images appear.

**Contingency**
- If TMDB does not return language-filtered images, log the API responses and consider falling back to the previous behaviour.

