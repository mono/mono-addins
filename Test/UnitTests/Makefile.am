ASSEMBLY_COMPILER_COMMAND = @MCS@
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
ASSEMBLY = ./bin/Debug/UnitTests.dll
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../bin/Mono.Addins.CecilReflector.dll \
	../../bin/Mono.Addins.dll
BUILD_DIR = ./bin/Debug
NOINSTALL=yes

if ENABLE_TESTS
all: $(ASSEMBLY)
else
all:
endif

FILES =  \
	AddinInformationTests.cs \
	ExtensionModel/ComplexNode.cs \
	ExtensionModel/GlobalInfoCondition.cs \
	ExtensionModel/ISampleExtender.cs \
	ExtensionModel/IWriter.cs \
	ExtensionModel/IWriterWithMetadata.cs \
	ExtensionModel/NodeWithAttribute.cs \
	ExtensionModel/ParameterInfoCondition.cs \
	ExtensionModel/SimpleExtensionAttribute.cs \
	TestBase.cs \
	TestConditions.cs \
	TestEvents.cs \
	TestExtensions.cs \
	TestLoadUnload.cs \
	TestLocalization.cs \
	TestSetup.cs 

DATA_FILES =  \
	SimpleApp.addin.xml \
	test.addins 

RESOURCES = 

EXTRAS = 

REFERENCES =  \
	-pkg:mono-nunit \
	-pkg:nunit \
	-r:System \
	-r:System.Xml

DLL_REFERENCES = 

$(ASSEMBLY).mdb: $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) 
	mkdir -p $(dir $(ASSEMBLY))		

	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$@ -target:$(COMPILE_TARGET) $(build_sources) $(build_resources_embed) $(build_references_ref)

if ENABLE_TESTS
include $(top_srcdir)/makefile-sample.include
endif
