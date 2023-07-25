@rem This script will build a standalone executable for FlareSolverr 3.0.0+.
@rem For portability, consider using a 32-bit Python installation.

@rem The following files should be accessible to the script:
@rem * version_info.txt
@rem * icon.ico

@rem TODO: The "get_flaresolverr_version" function in "utils.py" needs to be manually patched so FlareSolverr looks for
@rem "package.json" in its current directory instead of in the parent directory (which would be %temp%). Pyinstaller can
@rem place files directly in %temp%, but can't delete or overwrite them later. This causes the application to immediately
@rem exit on subsequent runs.

@rem Download the latest version of FlareSolverr.

git clone "https://github.com/FlareSolverr/FlareSolverr.git"
cd FlareSolverr
git pull origin master
cd ..

@rem Build the executable.

pyinstaller "FlareSolverr/src/flaresolverr.py" ^
	--onefile ^
	--add-data "FlareSolverr/package.json;." ^
	--version-file "file_version_info.txt" ^
	--icon "FlareSolverr/resources/flaresolverr_logo.ico"