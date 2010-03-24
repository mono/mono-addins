SUBDIRS = \
	HelloWorld \
	HelloWorldWithManifest \
	WriterService \
	TextEditor

all:
	for subdir in $(SUBDIRS); do \
	  cd $$subdir && make && cd ..; \
	done

clean:
	for subdir in $(SUBDIRS); do \
	  cd $$subdir && make clean && cd ..; \
	done

