rem Before running this script, make sure that the following items are accessible to the script:

rem - pyinstaller (either set your PATH appropriately, or run this script in "Scripts")	
rem - browsers.json (copy from "Lib\site-packages\cloudscraper\user_agent\browsers.json")
rem - version_info.txt
rem - icon.ico

rem For portability, prefer a 32-bit Python install.

rem Download the latest version of cloudscraper.

pip install --no-cache-dir --upgrade cloudscraper 

rem Build the executable.

pyinstaller cloudscraper_cli.py ^
	--onefile ^
	--add-data "browsers.json;/cloudscraper/user_agent" ^
	--hidden-import "cloudscraper.interpreters.native" ^
	--hidden-import "cloudscraper.exceptions.cloudflare_exceptions" ^
	--version-file "version_info.txt" ^
	--icon "icon.ico"