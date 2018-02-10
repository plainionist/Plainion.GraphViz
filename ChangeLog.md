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
