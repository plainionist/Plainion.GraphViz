
# Packaging Spec

The packaging spec describes the architecture of the system to analyse.

A *package* is used to describe a sub-system, a bigger component or a micro-service - a part of the 
system which can be described quite well with assembly patterns, e.g.:

```Xml
<Package Name="Plainion.Foundations">
    <Include Pattern="Plainion.Core.dll" />
    <Include Pattern="Plainion.Windows.dll" />
    <Include Pattern="Plainion.Prism.dll" />
</Package>
```

A *cluster* is used to group parts of the sub-system, e.g.:

```Xml
<Package Name="Plaionion.GraphViz">
    <Package.Clusters>
        <Cluster Name="CodeInspection">
            <Include Pattern="*CodeInspection*" />
        </Cluster>
        <Cluster Name="Documents">
            <Include Pattern="*.Documents.*" />
        </Cluster>
    </Package.Clusters>
    <Include Pattern="Plainion.GraphViz.*dll" />
</Package>
```

Furthermore a *cluster* can also be "folded" in the viewer. To support folding of the
entire package use:

```Xml
<Package Name="Prism">
    <Package.Clusters>
        <Cluster Name="Prism">
            <Include Pattern="*"/>
        </Cluster>
    </Package.Clusters>
    <Include Pattern="Microsoft.Practices.Prism.*.dll" />
</Package>
```

