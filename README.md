# Kozer
A program that sets goat(koza) as desktop wallpaper


-Introduction
	This is the project (Kozer) that changes the desktop background every @SomeInteger minutes
	and checks if the user changed and changes. (More information in ExtractedProcess/@KozerProcessFolderName/read.me)
	
	Also there is an installer that extracts @KozerProcessFolderName in destination directory, 
	adds to startup, launches the @KozerProcessExeName and deletes itself
	Developer mode is availible (More information in Installers/read.me)

-Project structure
	-Kozer
		Project sources

		-KozerProcess
		-KozerInstaller
	
	-Kozer varlib
		Installed program's data
	-Installers
		The newest installer

You can find ready installers here https://drive.google.com/folderview?id=0Bw-6kboN3QJ0Y3c0eGxIeXFhLVk&usp=sharing

-Instructions to build an installer
	1) build KozerProcess
	2) transfer the KozerProcess.exe in Kozer varlib
	3) rename the KozerProcess.exe to @KozerProcessExeName
	4) build .zip file from @KozerProcessFolderName in Kozer varlib
	5) transfer .zip file to Kozer/KozerInstaller
	6) add .zip file as an embedded resource in KozerInstaller project
	7) check constants
	8) build kozerInstaller

-Constants
	@KozerProcessExeName = "svchost.exe"
	@KozerProcessFolderName = "SDK"

