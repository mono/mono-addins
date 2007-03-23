ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
ASSEMBLY = ../bin/TextEditorLib.dll
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../bin/Mono.Addins.dll \
	../../bin/Mono.Addins.Gui.dll
BUILD_DIR = ../bin

	
if ENABLE_GUI
all: $(ASSEMBLY)
endif

FILES = \
	./AssemblyInfo.cs \
	./ICommand.cs \
	./gtk-gui/generated.cs \
	./MainWindow.cs \
	./gtk-gui/MainWindow.cs \
	./TextEditorApp.cs \
	./Commands/CopyCommand.cs \
	./Commands/CutCommand.cs \
	./Commands/NewCommand.cs \
	./Commands/OpenCommand.cs \
	./Commands/PasteCommand.cs \
	./Commands/SaveCommand.cs \
	./ExtensionNodes/MenuItemNode.cs \
	./ExtensionNodes/MenuSeparatorNode.cs \
	./ExtensionNodes/SubmenuNode.cs \
	./ExtensionNodes/ToolButtonNode.cs \
	./ExtensionNodes/ToolSeparatorNode.cs \
	./ExtensionNodes/MenuNode.cs \
	./ExtensionNodes/ToolbarNode.cs \
	./Commands/ExitCommand.cs \
	./OpenFileCondition.cs \
	./ExtensionNodes/TemplateCategoryNode.cs \
	./ExtensionNodes/FileTemplateNode.cs \
	./Commands/SetupCommand.cs 

DATA_FILES = 

RESOURCES = \
	./gtk-gui/gui.stetic \
	./Templates/ChangeLogTemplate.txt \
	./Templates/DotConfigTemplate.txt \
	./Templates/TextEditor.addin.xml \
	./Templates/WorkReport.txt 

EXTRAS = 

REFERENCES =  \
	-r:System \
	$(GTK_SHARP_20_LIBS) \
	-r:Mono.Posix

DLL_REFERENCES = 

$(ASSEMBLY) $(ASSEMBLY).mdb: $(build_sources) $(build_resources) $(build_datafiles) 
	mkdir -p $(dir $(ASSEMBLY))		

	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$@ -target:$(COMPILE_TARGET) $(build_sources) $(build_resources_embed) $(build_references_ref)

if ENABLE_GUI
include $(top_srcdir)/makefile-sample.include
endif
