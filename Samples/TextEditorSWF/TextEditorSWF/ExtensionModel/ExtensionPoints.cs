using System;
using Mono.Addins;
using TextEditorSWF.ExtensionModel;

// This file defines some data extension points.

// Toolbar extension point

[assembly: ExtensionPoint ("/TextEditor/Toolbar", NodeName = "Button", NodeType = typeof (InterfaceItemExtensionNode))]
[assembly: ExtensionPoint ("/TextEditor/Toolbar", NodeName = "Separator", NodeType = typeof (SeparatorExtensionNode))]

// Main menu extension point

[assembly: ExtensionPoint ("/TextEditor/MainMenu", NodeName="Menu", NodeType = typeof(MenuExtensionNode))]

