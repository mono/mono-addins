#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

using System;
using System.Linq;

Item ("https://xamjenkinsartifact.azureedge.net/build-package-osx-mono/2019-06/174/6b4b99e571b94331765170418d875416bf295a4e/MonoFramework-MDK-6.4.0.190.macos10.xamarin.universal.pkg", kind: ItemDependencyKind.AtLeast);
