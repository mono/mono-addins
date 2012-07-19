
ASSEMBLY = ../bin/mautil.exe
ASSEMBLY_WRAPPER = mautil
ASSEMBLY_WRAPPER_IN = mautil.in

$(ASSEMBLY): csproj_build

include $(top_srcdir)/Makefile.include
include $(top_srcdir)/xbuild.include

programfilesdir = $(libdir)/mono/mono-addins
programfiles_DATA = $(ASSEMBLY) $(build_datafiles)

man_MANS = mautil.1
EXTRA_DIST += $(man_MANS)

