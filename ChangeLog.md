## 8.0 - 2025-02-15

- Dot files: color attribute uses quotes to support parsing in nexus-odyssey
- Support for weighted edges added
- Package analysis interprets count of references between two types a weight for edges

## 7.1 - 2024-08-07

- loading of .NET 8 projects into "package analysis" fixed

## 7.0 - 2024-07-21

- upgraded to .Net 8
- NuGet dependencies updated

## 6.2 - 2024-03-08

- Algo RemoveNodesNotConnectedOutsideCluster fixed
  (respect folding of clusters)

## 6.1 - 2023-10-03

- Algo RemoveNodesNotConnectedOutsideCluster fixed
  (equality implementation of graph items)

## 6.0 - 2023-08-02

- New Plugin added to visualize MarkDown file dependencies
- VsProject dependencies creates cluster for top level folders
- F11 toggles full screen mode
- Export graph as PNG
- Algo RemoveNodesNotConnectedOutsideCluster fixed

## 5.2 - 2023-07-22

- Package dependencies
  - "Used types only" option moved from UI to packaging spec
  - "Create clusters for namespaces" replaced by "Package.AutoClusters" attribute (see docs)

## 5.1 - 2023-07-19

- loading assemblies made more tollerant

## 5.0 - 2023-04-22

- loading assemblies for "CodeInspection" completely reworked to support NetCoreApp and NetFramework in a clean way

## 4.5 - 2022-09-09

- renaming clusters in cluster editor fixed
- AssemblyLoader improve to explicitly resolve assemblies and not rely purely on .Net probing path
- CallTree: method can be "*" to trace call paths to all methods of a type
- AssemblyLoader performance improved

## 4.4 - 2022-07-09

- rename of clusters in the cluster editor will be immediately reflected in rendered graph
- Package Analysis: reporting skipped assemblies fixed

## 4.3 - 2022-06-27

- Application Icon fixed

## 4.2 - 2022-06-25

- CallTree: tooltip added to dialog
- CallTree: assembly loading improved

## 4.1 - 2022-06-22

- hot reload of graph documents made more robust

## 4.0 - 2022-06-20

- Packages dependency graph
  - loaded file name shown
  - editor starts with template
- Migrated to .Net 6
- Migrated to Prism 8
- other dependencies updated

## 3.6 - 2021-04-28

- Fix: Packaging: generating clusters from namespaces in case namespace is null take
  assembly name

## 3.5 - 2021-04-11

- Context menus added
  - "Select reaching sources"
  - "Select visible" of cluster

## 3.4 - 2019-07-18

- Package analysis: fixed generating clusters for namespaces
- Ensure process is killed when exception is throw during startup
- Dependencies updated
- Cleanup of APIs of modules
- "dot.exe" home no longer read from config (always use embedded distribution)
- Switched to Unity as MEF is no longer supported in Prism 7
- Migrated to Prism 7.2

## 3.3 - 2019-07-18

- Package analysis: Generating clusters from namespaces fixed
- Package analysis: handling of compiler generated types improved
- fixed exception when connecting with RDP

## 3.2 - 2019-05-18

- CreateClustersForAssemblies attribute added to Package
- Package analysis: "All Edges" check box removed and behavior made default
- Package analysis: create package spec from a ".dot" file modeling assembly dependencies by opening
  the ".dot" file in the packaging dialog

## 3.1 - 2019-02-19

- minor wording adjustments in graph context menu
- fixed extending nodes of unfolded cluster
- "Show" context menu reintroduced as a shortcut for "Hide all but selected node" and
  "Add sources/targets/siblings"

## 3.0 - 2019-02-18

- Complete redesign of graph context menu with focus on simplicity. Unused and complex
  entries removed, new and more intuitive entries added.
- Cluster folding respects visibility of underlaying graph.
- Filter dialog supports filtering on nodes within folded clusters
- Update to .Net FW 4.7.2
- Fixed case handling when loading documents by extension
- Fixed loading DGML document without nodes
- Fixed various selection update issues

## 2.12.0 - 2018-10-07

- CodeInspection
  - Infrastructure for actor implementation created
  - Inheritance analyzer migrated to actor system for maintainability 
  - PathFinder analysis added
  - CallTree analysis added

## 2.11.0 - 2018-07-13

- CodeInspection service: maximum-frame-size increased for akka to support big spec files
- performance improvement for rendering graphs with huge amount of clusters
- Added "Freeze" API to Graph and RelaxedGraphBuilder to support much faster setting huge graphs to presentation

## 2.10.0 - 2018-03-12

- CodeInspection.Inspector.GetHardcodedStrings() added
- Algorithm "Remove nodes without incomings" for clusters added
- Algorithm "Remove nodes without outgoings" for clusters added

## 2.9.0 - 2018-02-19

- size of the filter editor slightly increased to that all content fits without scrollbar
- Added option to copy caption/identifiers of all visible nodes to clipboard

## 2.8.0 - 2018-02-10

- Online help will be included in the released package as markdown. 
  Html based help is only available online on GitHub
- UI tooltips improved
- Zoom sliders added
- Zoom with cursor keys at mouse position supported
- Bookmarks added to enable jumping between different settings

## 2.7.0 - 2017-12-26

- CodeInspection.Inspector.GetCalledMethods() added
- CodeInspection.Inspector supports nested classes
- Added layout algorithm "flow" (renamed "dot" to "Trees" and "Sftp" to "Galaxies")

## 2.6.0 - 2017-12-29

- added support saving and loading complete graphs including presentation information (".pgv")
- Load/Save of NodeMasks removed (use PGV files instead)

## 2.5.0 - 2017-12-09

- tracing between nodes added
- fold/unfold selection
- "hide all but selection" added
- "Deselect all" fixed

## 2.4.0 - 2017-11-20

- Context menu entries added to select incomings, outgoings and siblings
- Context menu added to deselect all

## 2.3.0 - 2017-11-15

- Plainion.Prism updated to get windows activated which are already open

## 2.2.0 - 2017-10-27

- fixed exceptions when opening context menu and then clicking on other node elements
- fixed writing back DOT files when clusters got modified
- fixed loading plugins

## 2.1.0 - 2017-08-24

- updated all dependencies to latest
- removed signing of assemblies
- removed prism dependency from graphviz lib

## 2.0.0 - 2017-08-22

- http://www.graphviz.org/ is bundled together with the release package of Plainion.GraphViz
- updated to .NET 4.6.1
- updated all dependencies to latest

## 1.22.0 - 2017-07-22

- show/hide transitive hull of selected nodes

## 1.21.0 - 2017-05-15

- DotWriter sorts nodes and edges on demand for better diffing
- fixed performance issue with syncing back to dot file

## 1.20.0 - 2017-03-22

- CodeInspection: fixed handling of empty clusters
- show "transitive hull"
- "home zoom/pan" implemented

## 1.19.0 - 2017-03-02

- dynamic edge thickness handling improved
- help system improved
- "show this" and "show this and selected" algorithms added
- cluster changes synced back to .dot files
- fixed saving of masks with folded clusters
- fixed docments filewatcher for changing files in visual studio

## 1.18.0 - 2016-08-13

- several bug fixes
- edges adjust thickness with zoom factor
- "add visible notes outside clusters to cluster" algorithm added
- filter dialog supports folded nodes
- "unfold and hide" algorithm added
- cluster renames reflected in package spec
- sync to package spec fixed
- "Description" attribute added package spec
- context menu operations can be applied to selected nodes
- save button now saves visible graph instead of full graph
- tool tips and other help improved
- "show all edges" added to packaging spec
