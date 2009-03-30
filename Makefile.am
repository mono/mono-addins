
SUBDIRS =  Mono.Addins Mono.Addins.CecilReflector Mono.Addins.Setup Mono.Addins.Gui mautil Test Samples

DOCFILES = \
	$(top_builddir)/bin/Mono.Addins.dll \
	$(top_builddir)/bin/Mono.Addins.Setup.dll \
	$(top_builddir)/bin/Mono.Addins.Gui.dll

DOC_ASSEMBLED_FILES = \
	mono-addins-docs.tree \
	mono-addins-docs.zip

apidocs: doc-update

doc-update:
	mdoc update --delete $(MDOC_OPTIONS) -o docs $(DOCFILES)

doc-assemble: mono-addins-docs.zip

mono-addins-docs.zip : mono-addins-docs.tree
mono-addins-docs.tree:
	mdoc assemble -o mono-addins-docs docs

install: install-docs

install-docs: mono-addins-docs.source $(DOC_ASSEMBLED_FILES)
	cp $^ `pkg-config monodoc --variable sourcesdir`

uninstall: uninstall-docs

uninstall-docs:
	-rm `pkg-config monodoc --variable sourcesdir`/mono-addins-docs{.source,.tree,.zip}

clean: clean-docs

clean-docs:
	-rm $(DOC_ASSEMBLED_FILES)

EXTRA_DIST = COPYING mono-addins.snk mono-addins-docs.source
