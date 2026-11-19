# Checks run by Deploy-Mod before deploying: the Harmony patch smoke test against the installed 1.7 game.
param([string]$RimWorldRoot = 'C:\Program Files (x86)\Steam\steamapps\common\RimWorld')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent

$harmony = Join-Path $RimWorldRoot '..\..\workshop\content\294100\2009463077\1.7\Assemblies\0Harmony.dll'
if (-not (Test-Path -LiteralPath $harmony)) { Write-Host "FAIL Harmony not found at $harmony"; exit 1 }
$harmony = (Resolve-Path -LiteralPath $harmony).Path

$project = Join-Path $root 'Tests\FoundingBonds.PatchSmoke\FoundingBonds.PatchSmoke.csproj'
& dotnet run --project $project -c Release -p:HarmonyDll="$harmony" -- $RimWorldRoot $harmony (Join-Path $root '1.7\Assemblies\FoundingBonds.dll')
if ($LASTEXITCODE -ne 0) { Write-Host 'FAIL Harmony patch smoke test'; exit 1 }
Write-Host 'Test-All passed'
