param (
    [Parameter(Position=0)]
    [string]$Version
)

$AppName = "SystemGameManager"
$ProjectDir = "."
$PublishDir = ".\bin\Release\net8.0-windows\win-x64\publish"
$OutputDir = ".\releases"
$RepoUrl = "https://github.com/Krassheiten/SystemGameManager"

if ([string]::IsNullOrWhiteSpace($Version)) {
    Write-Host "[X] Bitte Version angeben: .\publishnewversion.ps1 1.0.1"
    exit 1
}

Write-Host "[START] Release Start: Version $Version"

# 1. Build / Publish
Write-Host "[BUILD] Build läuft..."
dotnet publish $ProjectDir -c Release -r win-x64 --self-contained true

# 2. Velopack Pack
Write-Host "[PACK] Velopack Packaging..."
vpk pack -u $AppName -v $Version -p $PublishDir -o $OutputDir

# 3. Git Commit + Tag
Write-Host "[GIT] Git Commit + Tag..."
git add .
git commit -m "Release $Version"
git tag v$Version

# 4. Push Code + Tag
# git push
# git push origin "v$Version"

# 5. OPTIONAL: GitHub Release Upload
# vpk upload github --repoUrl $RepoUrl

Write-Host "[DONE] Release $Version komplett fertig!"