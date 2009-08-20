
KEY_FILE=$(top_srcdir)/mono-addins.snk
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS = -keyfile:$(KEY_FILE) -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
ASSEMBLY = ../bin/Mono.Addins.MSBuild.dll
ASSEMBLY_NAME=Mono.Addins.MSBuild
COMPILE_TARGET = library
PC_FILES_IN = mono-addins-msbuild.pc.in
PROJECT_REFERENCES =  \
	../bin/Mono.Addins.dll \
	../bin/Mono.Addins.Setup.dll
BUILD_DIR = ../bin

	
all: $(ASSEMBLY)

FILES =  \
	AssemblyInfo.cs \
	ResolveAddinReferences.cs

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	./Makefile.am 

REFERENCES =  \
	-r:Microsoft.Build.Framework \
	-r:Microsoft.Build.Utilities \
	-r:System

DLL_REFERENCES = 

$(ASSEMBLY).mdb: $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(POLICY_ASSEMBLIES)
	mkdir -p $(dir $(ASSEMBLY))		
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$@ -target:$(COMPILE_TARGET) $(build_sources) $(build_resources_embed) $(build_references_ref)

install-data-local: gac-install

uninstall-local: gac-uninstall

include $(top_srcdir)/Makefile.include

CLEANFILES += $(POLICY_ASSEMBLIES) $(POLICY_CONFIGS)
