dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([mono-addins], [0.1])
AM_INIT_AUTOMAKE([foreign])

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

AC_PROG_INSTALL

AC_PATH_PROG(MCS, mcs, no)
if test "x$MCS" = "xno"; then
        AC_MSG_ERROR([mcs Not found])
fi

PKG_CHECK_MODULES([GNOME_SHARP_20], [gnome-sharp-2.0])
PKG_CHECK_MODULES([GTK_SHARP_20], [gtk-sharp-2.0])

MONO_ADDINS_DEFAULT_LIB='Mono.Addins.dll'
MONO_ADDINS_SETUP_DEFAULT_LIB='Mono.Addins.Setup.dll'
MONO_ADDINS_GUI_DEFAULT_LIB='Mono.Addins.Gui.dll'
AC_SUBST(MONO_ADDINS_DEFAULT_LIB)
AC_SUBST(MONO_ADDINS_GUI_DEFAULT_LIB)

AC_CONFIG_FILES([
./Mono.Addins/mono-addins.pc
./Mono.Addins.Gui/mono-addins-gui.pc
./Mono.Addins.Setup/mono-addins-setup.pc
./Makefile
./Mono.Addins.Gui/Makefile
./Mono.Addins.Setup/Makefile
./Mono.Addins/Makefile
./mautil/Makefile
./mautil/mautil

])

AC_OUTPUT
