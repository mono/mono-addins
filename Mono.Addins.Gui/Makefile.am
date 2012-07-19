
ASSEMBLY = ../bin/Mono.Addins.Gui.dll
ASSEMBLY_NAME=Mono.Addins.Gui
PC_FILES_IN = mono-addins-gui.pc.in

if ENABLE_GUI

install-data-local: gac-install
uninstall-local: gac-uninstall

include $(top_srcdir)/Makefile.include
include $(top_srcdir)/xbuild.include

CLEANFILES += $(POLICY_ASSEMBLIES) $(POLICY_CONFIGS)

endif

