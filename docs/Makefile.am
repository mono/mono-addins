
DOCFILES = \
	$(top_builddir)/bin/Mono.Addins.dll \
	$(top_builddir)/bin/Mono.Addins.Setup.dll \
	$(top_builddir)/bin/Mono.Addins.Gui.dll

DOC_ASSEMBLED_FILES = \
	mono-addins-docs.tree \
	mono-addins-docs.zip

if ENABLE_DOCS

all: doc-update doc-assemble

doc-update:
	mdoc update --delete $(MDOC_OPTIONS) -o . $(DOCFILES)

doc-assemble: mono-addins-docs.zip

mono-addins-docs.zip : mono-addins-docs.tree
mono-addins-docs.tree:
	mdoc assemble -o mono-addins-docs .

install: install-docs

install-docs: mono-addins-docs.source $(DOC_ASSEMBLED_FILES)
	cp $^ `pkg-config monodoc --variable sourcesdir`

uninstall: uninstall-docs

uninstall-docs:
	-rm `pkg-config monodoc --variable sourcesdir`/mono-addins-docs{.source,.tree,.zip}

clean: clean-docs

clean-docs:
	-rm -rf $(DOC_ASSEMBLED_FILES)

EXTRA_DIST = mono-addins-docs.source

endif

