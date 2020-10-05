#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

using System;
using System.Linq;

Item ("https://xamjenkinsartifact.azureedge.net/build-package-osx-mono/2020-02/99/620cf538206fe0f8cd63d76c502149b331f56f51/MonoFramework-MDK-6.12.0.93.macos10.xamarin.universal.pkg", kind: ItemDependencyKind.AtLeast);
