ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
ASSEMBLY = ./bin/Debug/UnitTests.dll
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../bin/Mono.Addins.dll
BUILD_DIR = ./bin/Debug
NOINSTALL=yes

if ENABLE_TESTS
all: $(ASSEMBLY)
else
all:
endif

FILES = \
	./TestExtensions.cs \
	./IWriter.cs \
	./TestBase.cs \
	./TestEvents.cs \
	./TestLoadUnload.cs \
	./GlobalInfoCondition.cs \
	./ParameterInfoCondition.cs \
	./TestConditions.cs \
	./TestSetup.cs \
	./ISampleExtender.cs \
	./ComplexNode.cs 

DATA_FILES = \
	./SimpleApp.addin.xml \
	./test.addins 

RESOURCES = 

EXTRAS = 

REFERENCES =  \
	-r:System \
	$(MONO_NUNIT_LIBS) \
	-r:System.Xml

DLL_REFERENCES = 

$(ASSEMBLY) $(ASSEMBLY).mdb: $(build_sources) $(build_resources) $(build_datafiles) 
	mkdir -p $(dir $(ASSEMBLY))		

	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$@ -target:$(COMPILE_TARGET) $(build_sources) $(build_resources_embed) $(build_references_ref)

if ENABLE_TESTS
include $(top_srcdir)/makefile-sample.include
endif
