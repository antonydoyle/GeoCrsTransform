# Tier B: Optional verification against PROJ (run when VERIFY_WITH_PROJ=1).
# Requires PROJ or proj.exe on PATH. Not run in CI by default.
if (-not $env:VERIFY_WITH_PROJ) {
    Write-Host "Set VERIFY_WITH_PROJ=1 to run PROJ verification. Skipping."
    exit 0
}
$proj = Get-Command proj -ErrorAction SilentlyContinue
if (-not $proj) {
    Write-Host "PROJ not found on PATH. Install PROJ or use Docker. Skipping verification."
    exit 0
}
Write-Host "PROJ verification would run here (compare transformer output to cs2cs)."
exit 0
