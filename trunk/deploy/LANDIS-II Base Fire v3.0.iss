#define PackageName      "Base Fire"
#define PackageNameLong  "Base Fire Extension"
#define Version          "3.0.3"
#define ReleaseType      "official"

#define CoreVersion      "6.0"
#define CoreReleaseAbbr  ""

#include "J:\Scheller\LANDIS-II\deploy\package (Setup section) v6.0.iss"
#define ExtDir "C:\Program Files\LANDIS-II\v6\bin\extensions"
#define AppDir "C:\Program Files\LANDIS-II\v6"

[Files]

Source: ..\src\bin\debug\Landis.Extension.BaseFire.dll; DestDir: {#ExtDir}; Flags: replacesameversion

; Base Fire
Source: docs\LANDIS-II Base Fire v3.0 User Guide.pdf; DestDir: {#AppDir}\docs
Source: examples\*.txt; DestDir: {#AppDir}\examples\base-fire
Source: examples\*.bat; DestDir: {#AppDir}\examples\base-fire
Source: examples\ecoregions.gis; DestDir: {#AppDir}\examples\base-fire
Source: examples\initial-communities.gis; DestDir: {#AppDir}\examples\base-fire

#define BaseFire "Base Fire 3.0.2.txt"
Source: {#BaseFire}; DestDir: {#LandisPlugInDir}

[InstallDelete]
Type: files; Name: "{#AppDir}\Landis.Extension.BaseFire.dll"

[Run]
;; Run plug-in admin tool to add the entry for the plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""Base Fire"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#BaseFire}"" "; WorkingDir: {#LandisPlugInDir}


[Code]
{ Check for other prerequisites during the setup initialization }
#include "J:\Scheller\LANDIS-II\deploy\package (Code section) v3.iss"


//-----------------------------------------------------------------------------

function CurrentVersion_PostUninstall(currentVersion: TInstalledVersion): Integer;
begin
    Result := 0;
end;

//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  CurrVers_PostUninstall := @CurrentVersion_PostUninstall
  Result := True
end;
