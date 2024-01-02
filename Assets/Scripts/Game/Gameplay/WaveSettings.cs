public static class WaveSettings
{
    private static float _WaveSpawnDuration;
    private static float _WaveSpawnLineDelay;
    private static float _WaveMovementDuration;
    private static float _WaveTurnDelay;
    public static bool FastForward;
    public static void InitializeValues(float waveSpawnLineDelay, float waveSpawnDuration, float waveMovementDuration, float waveTurnDelay)
    {
        _WaveSpawnLineDelay = waveSpawnLineDelay;
        _WaveSpawnDuration = waveSpawnDuration;
        _WaveMovementDuration = waveMovementDuration;
        _WaveTurnDelay = waveTurnDelay;
    }

    public static float TurnDuration
    {
        get
        {
            float factor = FastForward ? 2f : 1f;
            return _WaveMovementDuration / factor;
        }
    }

    public static float DelayBetweenTurns
    {
        get
        {
            return FastForward ? .05f : _WaveTurnDelay;
        }
    }

    public static float WaveSpawnDuration => _WaveSpawnDuration;
    public static float WaveSpawnLineDelay => _WaveSpawnLineDelay;
}
