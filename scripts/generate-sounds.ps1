# Generate placeholder WAV files for combat sound effects

$baseDir = Join-Path (Split-Path $PSScriptRoot -Parent) "src\BattleArena.Gui\Assets\Sounds"
$sampleRate = 22050
$bitsPerSample = 16
$channels = 1

# Sound definitions: [soundId, frequency(Hz), duration(s), amplitude(0-1)]
$sounds = @(
    @("BurnTick",       440,   0.15, 0.3),
    @("PoisonTick",     330,   0.15, 0.3),
    @("FrostTick",      880,   0.15, 0.3),
    @("ShockTick",      1200,  0.10, 0.4),
    @("BleedTick",      220,   0.20, 0.3),
    @("CriticalHit",    660,   0.50, 0.8),
    @("Fumble",         150,   0.30, 0.5),
    @("PerfectParry",   1000,  0.25, 0.7),
    @("PerfectDodge",   900,   0.20, 0.4),
    @("CounterAttack",  770,   0.30, 0.6),
    @("KillingBlow",    550,   0.80, 0.9),
    @("Resurrection",   440,   1.00, 0.6)
)

function New-WavFile {
    param([string]$Path, [int]$Frequency, [double]$Duration, [double]$Amplitude, [int]$SampleRate, [int]$Channels)

    $numSamples = [int]($SampleRate * $Duration)
    $blockAlign = $Channels * ($bitsPerSample / 8)
    $byteRate = $SampleRate * $blockAlign
    $dataSize = $numSamples * $blockAlign
    $fileSize = 36 + $dataSize

    $dir = Split-Path $Path -Parent
    if (!(Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }

    $fs = [System.IO.File]::Create($Path)
    $writer = New-Object System.IO.BinaryWriter($fs)

    try {
        # RIFF header
        $writer.Write([char[]]'RIFF')
        $writer.Write([int]$fileSize)
        $writer.Write([char[]]'WAVE')

        # fmt chunk
        $writer.Write([char[]]'fmt ')
        $writer.Write([int]16)           # chunk size
        $writer.Write([short]1)          # PCM format
        $writer.Write([short]$Channels)
        $writer.Write([int]$SampleRate)
        $writer.Write([int]$byteRate)
        $writer.Write([short]$blockAlign)
        $writer.Write([short]$bitsPerSample)

        # data chunk
        $writer.Write([char[]]'data')
        $writer.Write([int]$dataSize)

        $envelope = 0
        $envelopeSteps = [int]($SampleRate * 0.02)  # 20ms fade in/out

        for ($i = 0; $i -lt $numSamples; $i++) {
            $t = $i / $SampleRate
            $value = [Math]::Sin(2 * [Math]::PI * $Frequency * $t)

            # Simple amplitude envelope (fade in/out)
            if ($i -lt $envelopeSteps) {
                $envelope = ($i) / $envelopeSteps
            } elseif ($i -gt ($numSamples - $envelopeSteps)) {
                $envelope = ($numSamples - $i) / $envelopeSteps
            } else {
                $envelope = 1.0
            }

            $sample = [int]($value * $Amplitude * $envelope * 32767)
            $sample = [Math]::Max(-32768, [Math]::Min(32767, $sample))

            if ($Channels -eq 1) {
                $writer.Write([short]$sample)
            } else {
                $writer.Write([short]$sample)
                $writer.Write([short]$sample)
            }
        }
    }
    finally {
        $writer.Close()
        $fs.Dispose()
    }
}

Write-Host "Generating WAV sound files in: $baseDir"

foreach ($s in $sounds) {
    $id = $s[0]
    $freq = $s[1]
    $dur = $s[2]
    $amp = $s[3]
    $path = Join-Path $baseDir "$id.wav"
    New-WavFile -Path $path -Frequency $freq -Duration $dur -Amplitude $amp -SampleRate $sampleRate -Channels $channels
    Write-Host "  Created: $id.wav ($freq Hz, $dur`s)"
}

Write-Host "Done — $($sounds.Count) WAV files generated."
