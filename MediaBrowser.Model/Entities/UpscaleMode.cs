namespace MediaBrowser.Model.Entities;

/// <summary>
/// Defines the target resolution for video upscaling.
/// </summary>
public enum UpscaleMode
{
    /// <summary>
    /// No upscaling.
    /// </summary>
    None = 0,

    /// <summary>
    /// Upscale to Full HD (1080p).
    /// </summary>
    FullHD1080p = 1,

    /// <summary>
    /// Upscale to Ultra HD (4K).
    /// </summary>
    UHD4K = 2
}
