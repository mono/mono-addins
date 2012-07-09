dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([mono-addins], [1.0])
AM_INIT_AUTOMAKE([foreign tar-ustar])
AM_MAINTAINER_MODE

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

API_VERSION=1.0.0.0
AC_SUBST(API_VERSION)
POLICY_VERSIONS="0.2 0.3 0.4 0.5 0.6"
AC_SUBST(POLICY_VERSIONS)

AC_PROG_INSTALL

AC_PATH_PROG(MCS, gmcs, no)
if test "x$MCS" = "xno"; then
        AC_MSG_ERROR([mcs Not found])
fi

AC_PATH_PROG(GACUTIL, gacutil, no)
if test "x$GACUTIL" = "xno" ; then
	AC_MSG_ERROR([No gacutil tool found])
fi

AC_PATH_PROG(AL, al, no)
if test "x$AL" = "xno" ; then
	AC_MSG_ERROR([No al tool found. You need to install either the mono or .Net SDK.])
fi

GACUTIL_FLAGS='/package mono-addins /root $(DESTDIR)$(libdir)'
GACUTIL_POLICY_FLAGS='/root $(DESTDIR)$(libdir)'

AC_SUBST(GACUTIL)
AC_SUBST(GACUTIL_FLAGS)
AC_SUBST(GACUTIL_POLICY_FLAGS)

AC_ARG_ENABLE(gui,
	AC_HELP_STRING([--enable-gui],
		[enable support for GTK dependent libraries [default=yes]]),
		enable_gui=${enableval}, enable_gui=yes)

AM_CONDITIONAL(ENABLE_GUI, test x$enable_gui = xyes)

AC_ARG_ENABLE(tests,
	AC_HELP_STRING([--enable-tests],
		[build unit tests [default=no]]),
		enable_tests=${enableval}, enable_tests=no)

AM_CONDITIONAL(ENABLE_TESTS, test x$enable_tests = xyes)

AC_ARG_ENABLE(docs,
	AC_HELP_STRING([--enable-docs],
		[build documentation [default=no]]),
		enable_docs=${enableval}, enable_docs=no)

AM_CONDITIONAL(ENABLE_DOCS, test x$enable_docs = xyes)

if test "x$enable_gui" = "xyes"; then
	PKG_CHECK_MODULES([GTK_SHARP_20], [gtk-sharp-2.0])
fi

if test "x$enable_tests" = "xyes"; then
	PKG_CHECK_MODULES([MONO_NUNIT], [mono-nunit])
fi

if test "x$enable_docs" = "xyes"; then
	PKG_CHECK_MODULES([MONODOC], [monodoc])
fi


AC_SUBST(MONO_NUNIT_LIBS)

MONO_ADDINS_DEFAULT_LIB='Mono.Addins.dll'
MONO_ADDINS_SETUP_DEFAULT_LIB='Mono.Addins.Setup.dll'
MONO_ADDINS_GUI_DEFAULT_LIB='Mono.Addins.Gui.dll'
MONO_ADDINS_MSBUILD_DEFAULT_LIB='Mono.Addins.MSBuild.dll'
AC_SUBST(MONO_ADDINS_DEFAULT_LIB)
AC_SUBST(MONO_ADDINS_GUI_DEFAULT_LIB)
AC_SUBST(MONO_ADDINS_SETUP_DEFAULT_LIB)
AC_SUBST(MONO_ADDINS_MSBUILD_DEFAULT_LIB)

AC_CONFIG_FILES([
./mono-addins.spec
./Mono.Addins/mono-addins.pc
./Mono.Addins.Gui/mono-addins-gui.pc
./Mono.Addins.Setup/mono-addins-setup.pc
./Mono.Addins.MSBuild/mono-addins-msbuild.pc
./Makefile
./policy.config
./Mono.Addins.Gui/Makefile
./Mono.Addins.Setup/Makefile
./Mono.Addins/Makefile
./Mono.Addins.CecilReflector/Makefile
./Mono.Addins.MSBuild/Makefile
./mautil/Makefile
./mautil/mautil
./docs/Makefile
./Test/HelloWorldExtension/Makefile
./Test/FileContentExtension/Makefile
./Test/UnitTests/Makefile
./Test/CommandExtension/Makefile
./Test/SystemInfoExtension/Makefile
./Test/FileExtender/Makefile
./Test/Makefile

])

AC_OUTPUT

echo ""
echo "Configuration summary"
echo ""
echo "   * Installation prefix = $prefix"
echo "   * gui support: $enable_gui"
echo "   * unit tests: $enable_tests"
echo "   * documentation: $enable_docs"
echo ""


