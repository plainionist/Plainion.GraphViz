
# wpf open dialog on same screen as main window

- or: remember screen?
- worked with .net FW but with .Net 6 dialogs are always popping up on primary screen

# How to resolve indirect dependencies?

- which are not in bin folder (yet, as "publish" was not called)
- how to know where to search?
- analyze deps.json?
- https://stackoverflow.com/questions/59284307/manually-loading-of-assemblies-and-dependencies-at-runtime-nuget-dependencies
- https://mikelimasierra.net/index.php/2020/07/23/resolving-cached-nuget-packages-at-runtime/

# Tests for CodeInspection 

we need tests to load
- .netfw
- .netcore
- .net6

with following analyzers
- packages
- inheritance
- calltree
- pathfinder

Once tests available consider using AssemblyLoader everywhere

# Unsorted

- ensure that actor host is closed on main app close
  - dispose of unity

- change design to make it easier to add new algorithms to the UI
  - consider splitting "model + presentation + algo" and "visualization"

- take over Rajeev's Inspector implementation
