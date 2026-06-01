$files = Get-ChildItem "design/diagrams/*.svg"
foreach ($f in $files) {
    $bytes = [System.IO.File]::ReadAllBytes($f.FullName)
    if ($bytes.Length -gt 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        $clean = $bytes[3..($bytes.Length - 1)]
        [System.IO.File]::WriteAllBytes($f.FullName, $clean)
        Write-Host "  Fixed BOM: $($f.Name)"
    } else {
        Write-Host "  No BOM: $($f.Name)"
    }
}
