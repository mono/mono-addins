ASSEMBLY = ../bin/Mono.Addins.Setup.dll
ASSEMBLY_NAME=Mono.Addins.Setup
PC_FILES_IN = mono-addins-setup.pc.in

install-data-local: gac-install
uninstall-local: gac-uninstall

include $(top_srcdir)/Makefile.include
include $(top_srcdir)/xbuild.include

CLEANFILES += $(POLICY_ASSEMBLIES) $(POLICY_CONFIGS)


