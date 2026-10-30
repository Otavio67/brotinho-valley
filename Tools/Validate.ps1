param(
    [ValidateSet('Generate','Verify','EditMode','PlayMode')][string]$Mode = 'Verify',
    [string]$Editor = 'C:\Program Files\Unity\Hub\Editor\6000.3.27f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
New-Item -ItemType Directory -Path (Join-Path $projectRoot 'Logs'),(Join-Path $projectRoot 'TestResults') -Force | Out-Null
$log = Join-Path $projectRoot ('Logs\' + $Mode + '.log')
$arguments = @('-batchmode','-projectPath',('"' + $projectRoot + '"'),'-logFile',('"' + $log + '"'))
if ($Mode -eq 'Generate' -or $Mode -eq 'Verify') {
    $method = if ($Mode -eq 'Generate') { 'Brotinho.Editor.ProjectBuilder.BuildAndExit' } else { 'Brotinho.Editor.ProjectBuilder.Verify' }
    $arguments += @('-nographics','-executeMethod',$method)
    if ($Mode -eq 'Verify') { $arguments += '-quit' }
} else {
    $arguments += @('-runTests','-testPlatform',$Mode,'-testResults',('"' + (Join-Path $projectRoot ('TestResults\' + $Mode + '.xml')) + '"'))
}
$process = Start-Process -FilePath $Editor -ArgumentList $arguments -PassThru -Wait -WindowStyle Hidden
if ($process.ExitCode -ne 0) { Get-Content -LiteralPath $log -Tail 60; throw "Unity exited with $($process.ExitCode); see $log" }
if (Select-String -LiteralPath $log -Pattern 'error CS\d+|Scripts have compiler errors|Aborting batchmode' -Quiet) { throw "Unity reported compilation errors; see $log" }
if ($Mode -in @('EditMode','PlayMode')) {
    [xml]$report = Get-Content -LiteralPath (Join-Path $projectRoot ('TestResults\' + $Mode + '.xml'))
    $run = $report.'test-run'
    Write-Output "$Mode`: $($run.passed) passed; $($run.failed) failed; $($run.skipped) skipped"
    if ([int]$run.failed -gt 0 -or [int]$run.passed -eq 0) { throw 'Test suite did not pass.' }
}
Write-Output "Unity $Mode passed. Log: $log"
