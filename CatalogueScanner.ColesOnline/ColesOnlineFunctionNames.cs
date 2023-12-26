namespace CatalogueScanner.ColesOnline;

public static class ColesOnlineFunctionNames
{
    /// <summary>
    /// The name of the <see cref="Functions.ScanColesOnlineSpecials"/> orchestrator function.
    /// </summary>
    public const string ScanColesOnlineSpecials = "ScanColesOnlineSpecials";

    /// <summary>
    /// The name of the <see cref="Functions.ScanColesOnlineSpecials"/> orchestration trigger function.
    /// </summary>
    public const string ScanColesOnlineSpecialsTimerStart = "ScanColesOnlineSpecials_TimerStart";

    /// <summary>
    /// The name of the <see cref="Functions.GetColesOnlineSpecialsDates"/> activity function.
    /// </summary>
    public const string GetColesOnlineSpecialsDates = "GetColesOnlineSpecialsDates";

    /// <summary>
    /// The name of the <see cref="Functions.GetColesOnlineBuildId"/> activity function.
    /// </summary>
    public const string GetColesOnlineBuildId = "GetColesOnlineBuildId";

    /// <summary>
    /// The name of the <see cref="Functions.GetColesOnlineSpecialsPageCount"/> activity function.
    /// </summary>
    public const string GetColesOnlineSpecialsPageCount = "GetColesOnlineSpecialsPageCount";

    /// <summary>
    /// The name of the <see cref="Functions.DownloadColesOnlineSpecialsPage"/> activity function.
    /// </summary>
    public const string DownloadColesOnlineSpecialsPage = "DownloadColesOnlineSpecialsPage";
}
