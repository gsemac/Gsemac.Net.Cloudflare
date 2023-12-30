# This script will build a standalone executable for FlareSolverr 3.0.0+.
# For portability, consider using a 32-bit Python installation.

# The "get_flaresolverr_version" function in "utils.py" needs to be patched in some versions of FlareSolverr so that
# FlareSolver looks for "package.json" in its current directory instead of its parent directory (%temp%).
# PyInstaller places files directly in %temp%, but can't delete or overwrite them later, causing FlareSolverr to crash
# on subsequent runs.

# Download the latest version of FlareSolverr.

git clone "https://github.com/FlareSolverr/FlareSolverr.git"
cd FlareSolverr
git pull origin master
cd ..

# Reads the version string from package.json and writes it to file_version_info.txt.

$fileVersionInfoFilePath = 'file_version_info.txt'
$packageJsonFilePath = './FlareSolverr/package.json'

$packageJson = Get-Content $packageJsonFilePath -Raw | ConvertFrom-Json

$productNameStr = 'FlareSolverr'
$versionStr = $packageJson.version
$commaSeparatedVersionStr = ($versionStr.Split('.') -join ', ') + ', 0'
$descriptionStr = $packageJson.description
$authorStr = $packageJson.author

(Get-Content $fileVersionInfoFilePath -Raw) `
    -replace "((?:filevers|prodvers)=)\([^)]*\)", ('$1({0})' -f $commaSeparatedVersionStr) `
    -replace "\(u'(CompanyName|ProductName)', u'([^']*)'\)", ("(u'`$1', u'{0}')" -f $productNameStr) `
    -replace "\(u'(FileDescription)', u'([^']*)'\)", ("(u'`$1', u'{0}')" -f $descriptionStr) `
    -replace "\(u'(LegalCopyright)', u'([^']*)'\)", ("(u'`$1', u'{0}')" -f $authorStr) `
    -replace "\(u'(ProductVersion)', u'([^']*)'\)", ("(u'`$1', u'{0}')" -f $versionStr) |
    Out-File $fileVersionInfoFilePath -NoNewline

# Build the executable.

pyinstaller "FlareSolverr/src/flaresolverr.py" `
	--onefile `
	--add-data "FlareSolverr/package.json;." `
	--version-file "file_version_info.txt" `
	--icon "FlareSolverr/resources/flaresolverr_logo.ico"