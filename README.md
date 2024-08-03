# SA3D.Archival
SA3D Archive reader and writer.

Supports the following formats:

| Format          	| Description                                                            	|
|-----------------	|------------------------------------------------------------------------	|
| PRS             	| Compression algorithm used by numerous retro Sega games.               	|
| PAK             	| Generic file archive used by Sonic Adventure 2.                        	|
| PVM / PVR / PVP 	| Texture encoding and archive used by dreamcast games and their ports.  	|
| GVM / GVR / GVP 	| Texture encoding and archive used by gamecube games and their ports.   	|
| PVMX / PVRX     	| Custom DirectX compliant texture encoding and archive based on PVM/PVR. 	|

## Releasing
!! Requires authorization via the X-Hax organisation

1. Edit the version number in src/SA3D.Archival/SA3D.Archival.csproj; Example: `<Version>1.0.0</Version>` -> `<Version>2.0.0</Version>`
2. Commit the change but dont yet push.
3. Tag the commit: `git tag -a [version number] HEAD -m "Release version [version number]"`
4. Push with tags: `git push --follow-tags`

This will automatically start the Github `Build and Publish` workflow