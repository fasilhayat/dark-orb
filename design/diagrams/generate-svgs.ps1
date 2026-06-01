function New-BellCurveSvg {
    param(
        [string]$Title,
        [string]$Scenario,
        [int]$N,
        [string]$HitRate,
        [double]$Mu,
        [double]$Sigma,
        [int]$RangeLower,
        [int]$RangeUpper,
        [string]$OutFile
    )

    $W = 800; $H = 480
    $MarginL = 80; $MarginR = 40; $MarginT = 60; $MarginB = 70
    $PlotW = $W - $MarginL - $MarginR
    $PlotH = $H - $MarginT - $MarginB
    $X0 = $MarginL; $Y0 = $H - $MarginB
    $maxPdf = 1.0 / ($Sigma * [Math]::Sqrt(2 * [Math]::PI))

    $points = @()
    $dataRows = @()
    for ($hit = $RangeLower; $hit -le $RangeUpper; $hit++) {
        $z = ($hit - $Mu) / $Sigma
        $pdf = [Math]::Exp(-0.5 * $z * $z) * $maxPdf
        $px = $X0 + ($hit - $RangeLower) * $PlotW / ($RangeUpper - $RangeLower)
        $py = $Y0 - $pdf * $PlotH / $maxPdf
        $points += @{px=$px; py=$py; hit=$hit; z=$z; pdf=$pdf}
        $dataRows += "$hit,$($z.ToString('F3')),$($pdf.ToString('E4'))"
    }

    $sb = New-Object System.Text.StringBuilder

    $sb.AppendLine('<?xml version="1.0" encoding="UTF-8"?>') | Out-Null
    $sb.AppendLine('<!--') | Out-Null
    $sb.AppendLine('  COMBAT OUTCOME DISTRIBUTION -- Machine-readable dataset') | Out-Null
    $sb.AppendLine("  Scenario: $Scenario") | Out-Null
    $sb.AppendLine("  N=$N  P(hit)=$HitRate  mu=$Mu  sigma=$Sigma") | Out-Null
    $sb.AppendLine('  Formula: pdf(x) = exp(-((x-mu)/sigma)^2/2) / (sigma*sqrt(2*pi))') | Out-Null
    $sb.AppendLine('  Data format: hit,z,pdf') | Out-Null
    foreach ($row in $dataRows) {
        $sb.AppendLine("  $row") | Out-Null
    }
    $sb.AppendLine('-->') | Out-Null

    $sb.AppendLine('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 480" font-family="Consolas, monospace" font-size="12">') | Out-Null
    $sb.AppendLine("  <title>$Title</title>") | Out-Null
    $sb.AppendLine("  <desc>$Scenario -- N=$N P(hit)=$HitRate mu=$Mu sigma=$Sigma</desc>") | Out-Null
    $sb.AppendLine('  <rect width="800" height="480" fill="#fafafa"/>') | Out-Null

    # Grid lines
    $sb.AppendLine('  <g stroke="#ddd" stroke-width="0.5">') | Out-Null
    foreach ($g in 0..4) { 
        $gy = $Y0 - $g * $PlotH / 4
        $sb.AppendLine("    <line x1='$X0' y1='$gy' x2='$($X0+$PlotW)' y2='$gy'/>") | Out-Null
    }
    $sb.AppendLine('  </g>') | Out-Null

    # CI shaded areas
    @(
        @{z=3; c='rgba(65,105,225,0.08)'},
        @{z=2; c='rgba(65,105,225,0.12)'},
        @{z=1; c='rgba(65,105,225,0.18)'}
    ) | ForEach-Object {
        $zVal = $_.z
        $cL = $Mu - $zVal * $Sigma; $cR = $Mu + $zVal * $Sigma
        $cLx = $X0 + ($cL - $RangeLower) * $PlotW / ($RangeUpper - $RangeLower)
        $cRx = $X0 + ($cR - $RangeLower) * $PlotW / ($RangeUpper - $RangeLower)
        $fillPts = @()
        foreach ($pt in $points) {
            if ($pt.hit -ge [Math]::Floor($cL) -and $pt.hit -le [Math]::Ceiling($cR)) {
                $fillPts += "$($pt.px.ToString('F1')),$($pt.py.ToString('F1'))"
            }
        }
        if ($fillPts.Count -gt 1) {
            $d = "M $($fillPts[0])"
            foreach ($fp in $fillPts[1..($fillPts.Count-1)]) { $d += " L $fp" }
            $d += " L $cRx,$Y0 L $cLx,$Y0 Z"
            $sb.AppendLine("  <path d='$d' fill='$($_.c)' stroke='none'/>") | Out-Null
        }
    }

    # Bell curve path
    $d = "M $($points[0].px.ToString('F1')),$($points[0].py.ToString('F1'))"
    for ($i = 1; $i -lt $points.Count; $i++) {
        $d += " L $($points[$i].px.ToString('F1')),$($points[$i].py.ToString('F1'))"
    }
    $sb.AppendLine("  <path d='$d' fill='none' stroke='#2c3e50' stroke-width='2.5' stroke-linejoin='round' stroke-linecap='round'/>") | Out-Null

    # Mean vertical line
    $muPx = $X0 + ($Mu - $RangeLower) * $PlotW / ($RangeUpper - $RangeLower)
    $sb.AppendLine("  <line x1='$muPx' y1='$Y0' x2='$muPx' y2='$($Y0 - $PlotH)' stroke='#e74c3c' stroke-width='2' stroke-dasharray='6,4'/>") | Out-Null
    $sb.AppendLine("  <text x='$muPx' y='$($Y0 - $PlotH - 8)' text-anchor='middle' fill='#e74c3c' font-size='13' font-weight='bold'>mu=$Mu</text>") | Out-Null

    # Sigma markers
    $sb.AppendLine('  <g font-size="11" fill="#555" text-anchor="middle">') | Out-Null
    @(-3,-2,-1,0,1,2,3) | ForEach-Object {
        $sm = $_
        $val = $Mu + $sm * $Sigma
        $vPx = $X0 + ($val - $RangeLower) * $PlotW / ($RangeUpper - $RangeLower)
        $label = if ($sm -eq 0) { "mu" } elseif ($sm -gt 0) { "+${sm}s" } else { "${sm}s" }
        $sb.AppendLine("    <line x1='$vPx' y1='$Y0' x2='$vPx' y2='$($Y0+6)' stroke='#999' stroke-width='1'/>") | Out-Null
        $sb.AppendLine("    <text x='$vPx' y='$($Y0+20)'>$label</text>") | Out-Null
    }
    $sb.AppendLine('  </g>') | Out-Null

    # Hit count labels
    $sb.AppendLine('  <g font-size="10" fill="#888" text-anchor="middle">') | Out-Null
    @(-3,-2,-1,0,1,2,3) | ForEach-Object {
        $z = $_
        $val = [Math]::Round($Mu + $z * $Sigma)
        $vPx = $X0 + ($val - $RangeLower) * $PlotW / ($RangeUpper - $RangeLower)
        $sb.AppendLine("    <text x='$vPx' y='$($Y0+36)' fill='#666'>$val</text>") | Out-Null
    }
    $sb.AppendLine('  </g>') | Out-Null

    # Axis titles
    $sb.AppendLine("  <text x='$($X0 + $PlotW/2)' y='$($Y0 + 52)' text-anchor='middle' fill='#333' font-size='13'>Hit count out of $N attacks</text>") | Out-Null
    $sb.AppendLine("  <text x='16' y='$($MarginT + $PlotH/2)' text-anchor='middle' fill='#333' font-size='13' transform='rotate(-90,16,$($MarginT+$PlotH/2))'>Probability density</text>") | Out-Null

    # Legend
    $ciLegendY = 20
    $sb.AppendLine('  <g font-size="11">') | Out-Null
    @(
        @{l='68% CI (+-1s)'; c='rgba(65,105,225,0.18)'},
        @{l='95% CI (+-2s)'; c='rgba(65,105,225,0.12)'},
        @{l='99.7% CI (+-3s)'; c='rgba(65,105,225,0.08)'},
        @{l='Mean (mu)'; c='#e74c3c'}
    ) | ForEach-Object {
        $lx = $W - 180
        $sb.AppendLine("    <rect x='$lx' y='$ciLegendY' width='14' height='14' fill='$($_.c)' stroke='#999' stroke-width='0.5'/>") | Out-Null
        $sb.AppendLine("    <text x='$($lx+20)' y='$($ciLegendY+11)' fill='#333'>$($_.l)</text>") | Out-Null
        $ciLegendY += 18
    }
    $sb.AppendLine('  </g>') | Out-Null

    # Title
    $sb.AppendLine("  <text x='$($W/2)' y='22' text-anchor='middle' fill='#1a1a1a' font-size='15' font-weight='bold'>$Title</text>") | Out-Null
    $sb.AppendLine("  <text x='$($W/2)' y='40' text-anchor='middle' fill='#555' font-size='12'>$Scenario -- P(hit)=$HitRate  mu=$Mu  sigma=$($Sigma.ToString('F1'))  N=$N</text>") | Out-Null

    $sb.AppendLine('</svg>') | Out-Null
    $sb.ToString() | Out-File -Encoding utf8 $OutFile
    Write-Host "  $OutFile"
}

function New-ComparisonSvg {
    param([string]$OutFile)
    
    $W = 700; $H = 400
    $MarginL = 180; $MarginR = 80; $RowH = 50; $RowGap = 30
    $PlotW = $W - $MarginL - $MarginR
    $BarY = 80

    $scenarios = @(
        @{Name='Balanced'; AP=10; DP=8; Rate='60.25%'; Mu=1205; Sigma=21.9; Lo=1139; Hi=1271; Crit='4.5%'; Fumble='4.75%'; PP='4.75%'},
        @{Name='Defensive'; AP=8; DP=14; Rate='27.50%'; Mu=550; Sigma=19.97; Lo=480; Hi=620; Crit='-'; Fumble='-'; PP='-'},
        @{Name='Attacker'; AP=12; DP=5; Rate='76.50%'; Mu=1530; Sigma=17.72; Lo=1460; Hi=1600; Crit='-'; Fumble='-'; PP='-'},
        @{Name='High Lvl'; AP=23; DP=10; Rate='87.75%'; Mu=1755; Sigma=9.97; Lo=1700; Hi=1810; Crit='-'; Fumble='-'; PP='-'}
    )

    $sb = New-Object System.Text.StringBuilder
    $sb.AppendLine('<?xml version="1.0" encoding="UTF-8"?>') | Out-Null
    $sb.AppendLine('<!--') | Out-Null
    $sb.AppendLine('  COMBAT DISTRIBUTION COMPARISON -- Machine-readable data') | Out-Null
    $sb.AppendLine('  Fields: scenario,ap,dp,hit_rate,mu,sigma,ci68_l,ci68_r,ci95_l,ci95_r,ci997_l,ci997_r,crit_rate,fumble_rate,pp_rate') | Out-Null
    foreach ($s in $scenarios) {
        $lo68 = [Math]::Round($s.Mu - $s.Sigma)
        $hi68 = [Math]::Round($s.Mu + $s.Sigma)
        $lo95 = [Math]::Round($s.Mu - 2*$s.Sigma)
        $hi95 = [Math]::Round($s.Mu + 2*$s.Sigma)
        $sb.AppendLine("  $($s.Name),$($s.AP),$($s.DP),$($s.Rate),$($s.Mu),$($s.Sigma.ToString('F2')),$lo68,$hi68,$lo95,$hi95,$($s.Lo),$($s.Hi),$($s.Crit),$($s.Fumble),$($s.PP)") | Out-Null
    }
    $sb.AppendLine('-->') | Out-Null

    $sb.AppendLine("<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 $W $H' font-family='Consolas, monospace' font-size='11'>") | Out-Null
    $sb.AppendLine('  <title>Combat distribution comparison</title>') | Out-Null
    $sb.AppendLine('  <rect width="700" height="400" fill="#fafafa"/>') | Out-Null

    # Legend
    $sb.AppendLine('  <g font-size="10">') | Out-Null
    $legY = 20
    @(
        @{l='68% CI (+-1s)'; c='rgba(65,105,225,0.25)'},
        @{l='95% CI (+-2s)'; c='rgba(65,105,225,0.13)'},
        @{l='99.7% CI (+-3s)'; c='rgba(65,105,225,0.06)'},
        @{l='Mean (mu)'; c='#e74c3c'}
    ) | ForEach-Object {
        $sb.AppendLine("    <rect x='$($MarginL+10)' y='$legY' width='12' height='10' fill='$($_.c)' stroke='#999' stroke-width='0.5'/>") | Out-Null
        $sb.AppendLine("    <text x='$($MarginL+28)' y='$($legY+9)' fill='#333'>$($_.l)</text>") | Out-Null
        $legY += 14
    }
    $sb.AppendLine('  </g>') | Out-Null

    for ($i = 0; $i -lt $scenarios.Count; $i++) {
        $s = $scenarios[$i]
        $y = $BarY + $i * ($RowH + $RowGap)
        $mu = $s.Mu; $sigma = $s.Sigma
        $l68 = $mu - $sigma; $r68 = $mu + $sigma
        $l95 = $mu - 2*$sigma; $r95 = $mu + 2*$sigma
        $range = $s.Hi - $s.Lo

        $barL = $MarginL; $barR = $MarginL + $PlotW
        $bar95L = $MarginL + ($l95 - $s.Lo) / $range * $PlotW
        $bar95R = $MarginL + ($r95 - $s.Lo) / $range * $PlotW
        $bar68L = $MarginL + ($l68 - $s.Lo) / $range * $PlotW
        $bar68R = $MarginL + ($r68 - $s.Lo) / $range * $PlotW
        $muX = $MarginL + ($mu - $s.Lo) / $range * $PlotW

        $sb.AppendLine("    <rect x='$barL' y='$y' width='$($barR-$barL)' height='12' fill='rgba(65,105,225,0.06)' rx='2'/>") | Out-Null
        $sb.AppendLine("    <rect x='$bar95L' y='$y' width='$($bar95R-$bar95L)' height='12' fill='rgba(65,105,225,0.13)' rx='2'/>") | Out-Null
        $sb.AppendLine("    <rect x='$bar68L' y='$y' width='$($bar68R-$bar68L)' height='12' fill='rgba(65,105,225,0.25)' rx='2'/>") | Out-Null
        $sb.AppendLine("    <line x1='$muX' y1='$($y-4)' x2='$muX' y2='$($y+16)' stroke='#e74c3c' stroke-width='2.5'/>") | Out-Null
        $sb.AppendLine("    <line x1='$barL' y1='$($y+12)' x2='$barL' y2='$($y+17)' stroke='#999' stroke-width='1'/>") | Out-Null
        $sb.AppendLine("    <line x1='$barR' y1='$($y+12)' x2='$barR' y2='$($y+17)' stroke='#999' stroke-width='1'/>") | Out-Null

        $sb.AppendLine("    <text x='$($MarginL-8)' y='$($y+10)' text-anchor='end' fill='#1a1a1a' font-weight='bold'>$($s.Name)</text>") | Out-Null
        $sb.AppendLine("    <text x='$barR' y='$($y-4)' text-anchor='start' fill='#666' font-size='10'>P(hit)=$($s.Rate)</text>") | Out-Null
        $sb.AppendLine("    <text x='$barR' y='$($y+6)' text-anchor='start' fill='#888' font-size='9'>AP=$($s.AP) DP=$($s.DP)</text>") | Out-Null
        $sb.AppendLine("    <text x='$muX' y='$($y+26)' text-anchor='middle' fill='#e74c3c' font-size='10' font-weight='bold'>mu=$mu</text>") | Out-Null
    }

    $sb.AppendLine('  <g font-size="9" fill="#888" text-anchor="middle">') | Out-Null
    $sb.AppendLine("    <text x='$($MarginL + $PlotW/2)' y='$($BarY + 4*($RowH+$RowGap) + 15)'>Each bar spans the full 99.7% range; darker = narrower CI</text>") | Out-Null
    $sb.AppendLine('  </g>') | Out-Null

    $sb.AppendLine('</svg>') | Out-Null
    $sb.ToString() | Out-File -Encoding utf8 $OutFile
    Write-Host "  $OutFile"
}

Write-Host 'Generating SVGs...'
New-BellCurveSvg -Title 'Hit Distribution - Balanced Combat' -Scenario 'L2 STR12 SR8 vs AC8' -N 2000 -HitRate '60.25%' -Mu 1205 -Sigma 21.9 -RangeLower 1139 -RangeUpper 1271 -OutFile 'design/diagrams/combat-distribution-bellcurve.svg'
New-BellCurveSvg -Title 'Hit Distribution - Defensive Advantage' -Scenario 'L1 STR10 SR8 vs AC14' -N 2000 -HitRate '27.50%' -Mu 550 -Sigma 19.97 -RangeLower 480 -RangeUpper 620 -OutFile 'design/diagrams/combat-distribution-defensive.svg'
New-BellCurveSvg -Title 'Hit Distribution - Attacker Advantage' -Scenario 'L1 STR14 SR10 vs AC5' -N 2000 -HitRate '76.50%' -Mu 1530 -Sigma 17.72 -RangeLower 1460 -RangeUpper 1600 -OutFile 'design/diagrams/combat-distribution-attacker.svg'
New-BellCurveSvg -Title 'Hit Distribution - High Level Scaling' -Scenario 'L5 STR18 SR17 vs AC10' -N 2000 -HitRate '87.75%' -Mu 1755 -Sigma 9.97 -RangeLower 1700 -RangeUpper 1810 -OutFile 'design/diagrams/combat-distribution-highlevel.svg'
New-ComparisonSvg -OutFile 'design/diagrams/combat-distribution-comparison.svg'
Write-Host 'Done.'
