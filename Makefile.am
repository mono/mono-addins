
SUBDIRS =  Mono.Addins Mono.Addins.CecilReflector Mono.Addins.Setup Mono.Addins.MSBuild Mono.Addins.Gui Mono.Addins.GuiGtk3 mautil Test docs

EXTRA_DIST = COPYING mono-addins.snk Mono.Addins.sln

SRC_PACKAGE_DIR=Mono.Addins-source-@VERSION@
BIN_PACKAGE_DIR=Mono.Addins-binary-@VERSION@

src-zip:
	rm -rf $(SRC_PACKAGE_DIR)
	rm -f $(SRC_PACKAGE_DIR).zip
	mkdir $(SRC_PACKAGE_DIR)
	mdtool project-export Mono.Addins.sln -f:"MSBuild (Visual Studio 2008)" -d:$(SRC_PACKAGE_DIR) -p:mautil -p:Mono.Addins -p:Mono.Addins.CecilReflector -p:Mono.Addins.Setup
	mdtool project-export Samples/HelloWorld/HelloWorld.sln -f:"MSBuild (Visual Studio 2008)" -d:$(SRC_PACKAGE_DIR)/Samples/HelloWorld
	mdtool project-export Samples/HelloWorldWithManifest/HelloWorldWithManifest.sln -f:"MSBuild (Visual Studio 2008)" -d:$(SRC_PACKAGE_DIR)/Samples/HelloWorldWithManifest
	mdtool project-export Samples/TextEditorSWF/TextEditorSWF.sln -f:"MSBuild (Visual Studio 2008)" -d:$(SRC_PACKAGE_DIR)/Samples/TextEditorSWF
	cp support/README-src $(SRC_PACKAGE_DIR)/README.txt
	cp AUTHORS $(SRC_PACKAGE_DIR)/AUTHORS.txt
	cp COPYING $(SRC_PACKAGE_DIR)/COPYING.txt
	cp mono-addins.snk $(SRC_PACKAGE_DIR)/mono-addins.snk
	unix2dos $(SRC_PACKAGE_DIR)/README.txt
	unix2dos $(SRC_PACKAGE_DIR)/AUTHORS.txt
	unix2dos $(SRC_PACKAGE_DIR)/COPYING.txt
	rm -rf $(SRC_PACKAGE_DIR)/Test
	rm -f $(SRC_PACKAGE_DIR)/configure.ac
	cd $(SRC_PACKAGE_DIR) && for fi in `find  . -name Makefile.am`; do rm $$fi; done
	cd $(SRC_PACKAGE_DIR) && zip -r ../$(SRC_PACKAGE_DIR).zip *
	
bin-zip:
	rm -rf $(BIN_PACKAGE_DIR)
	rm -f $(BIN_PACKAGE_DIR).zip
	mkdir $(BIN_PACKAGE_DIR)
	cp support/README-bin $(BIN_PACKAGE_DIR)/README.txt
	cp AUTHORS $(BIN_PACKAGE_DIR)/AUTHORS.txt
	cp COPYING $(BIN_PACKAGE_DIR)/COPYING.txt
	unix2dos $(BIN_PACKAGE_DIR)/README.txt
	unix2dos $(BIN_PACKAGE_DIR)/AUTHORS.txt
	unix2dos $(BIN_PACKAGE_DIR)/COPYING.txt
	cp bin/mautil.exe $(BIN_PACKAGE_DIR)
	cp bin/Mono.Addins.dll $(BIN_PACKAGE_DIR)
	cp bin/Mono.Addins.xml $(BIN_PACKAGE_DIR)
	cp bin/Mono.Addins.Setup.dll $(BIN_PACKAGE_DIR)
	cp bin/Mono.Addins.Setup.xml $(BIN_PACKAGE_DIR)
	cp bin/Mono.Addins.CecilReflector.dll $(BIN_PACKAGE_DIR)
	cp support/ICSharpCode.SharpZipLib.license.txt $(BIN_PACKAGE_DIR)
	cp support/ICSharpCode.SharpZipLib.dll $(BIN_PACKAGE_DIR)
	cd $(BIN_PACKAGE_DIR) && zip -r ../$(BIN_PACKAGE_DIR).zip *

nuget:
	nuget pack Mono.Addins.nuspec
	nuget pack Mono.Addins.Setup.nuspec