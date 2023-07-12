@echo off

cd GPCModel
REM delete existing nuget packages
del *.nupkg



echo PACKING
nuget pack GPCModel.csproj -Version 1.0.1.3 -properties Configuration=Release


echo PUSHING
for /f %%l in ('dir /b /s *.nupkg') do (
	echo FILE %%l
	
	nuget add %%l -source \\studio\Software_Development\NuGetPackages\
		
	del %%l
)

cd ..