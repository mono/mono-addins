
ASSEMBLY = ../bin/Mono.Addins.GuiGtk3.dll
ASSEMBLY_NAME=Mono.Addins.GuiGtk3
PC_FILES_IN = mono-addins-gui-gtk3.pc.in

if ENABLE_GUI_GTK3

install-data-local: gac-install
uninstall-local: gac-uninstall

include $(top_srcdir)/Makefile.include
include $(top_srcdir)/xbuild.include

CLEANFILES += $(POLICY_ASSEMBLIES) $(POLICY_CONFIGS)

endif

