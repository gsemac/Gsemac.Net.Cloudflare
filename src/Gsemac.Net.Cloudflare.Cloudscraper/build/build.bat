rem This script will build a standalone executable for Cloudscraper.
rem For portability, consider using a 32-bit Python installation.

rem The following files should be accessible to the script:
rem * browsers.json (copy from "Lib\site-packages\cloudscraper\user_agent\browsers.json")
rem * version_info.txt
rem * icon.ico

rem Download the latest version of cloudscraper.

pip install --no-cache-dir --upgrade cloudscraper 

rem Build the executable.

pyinstaller cloudscraper.py ^
	--onefile ^
	--add-data "browsers.json;/cloudscraper/user_agent" ^
	--hidden-import "cloudscraper.interpreters.native" ^
	--hidden-import "cloudscraper.exceptions.cloudflare_exceptions" ^
	--version-file "file_version_info.txt" ^
	--icon "icon.ico"