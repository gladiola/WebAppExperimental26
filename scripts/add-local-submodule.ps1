# Update these two variables
$source = "C:/Users/gladi/source/repos/red-rfid-aclj-project01SEP2025C"
$target = "libs/red-rfid-aclj-project01SEP2025C"

# Ensure source is a git repo
if (-not (git -C $source rev-parse --is-inside-work-tree 2>$null)) {
  Write-Error "Source path is not a git repository: $source"
  exit 1
}

# If target already exists, show status and exit
if (Test-Path $target) {
  Write-Output "Target path already exists: $target"
  git -C $target status --short
  exit 0
}

# Add submodule via file:// URI (avoids Windows path parsing issues)
git submodule add "file:///$source" $target

# Initialize and fetch submodule contents
git submodule update --init --recursive $target

# Confirm results
git submodule status --recursive
Write-Output "Contents of target (first level):"
Get-ChildItem -Force $target | Select-Object Name, Mode, Length
Write-Output "Show .gitmodules:"
Get-Content .gitmodules

