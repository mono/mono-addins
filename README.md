Mono.Addins is a framework for creating extensible applications, and for creating libraries which extend those applications.

Mono.Addins has been designed to be easy to use and useful for a wide range of applications: from simple applications with small extensibility needs, to complex applications which need support for large add-in structures.

The main features of Mono.Addins are:

* Supports descriptions of add-ins using custom attributes (for simple and common extensions) or using an xml manifest (for more complex extensibility needs).
* Support for add-in hierarchies, where add-ins may depend on other add-ins.
* Lazy loading of add-ins.
* Provides an API for accessing to add-in descriptions, which will allow building development and documentation tools for handling add-ins.
* Dynamic activation / deactivation of add-ins at run time.
* Allows sharing add-in registries between applications, and defining arbitrary add-in locations.
* Allows implementing extensible libraries.
* Supports add-in localization.
* In addition to the basic add-in engine, it provides a Setup library to be used by applications which want to offer basic add-in management features to users, such as enabling/disabling add-ins, or installing add-ins from on-line repositories.

## Building

Open Mono.Addins.sln and build.

## Documentation

Extensive documentation is available in this project's [wiki](https://github.com/mono/mono-addins/wiki). 
