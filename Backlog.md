
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


test setup:
- 2 assemblies depending on each other
- API usage as well as inheritance
- require nuget dependency


Once tests available consider using AssemblyLoader everywhere

# Unsorted

- ensure that actor host is closed on main app close
  - dispose of unity

- change design to make it easier to add new algorithms to the UI
  - consider splitting "model + presentation + algo" and "visualization"

- take over Rajeev's Inspector implementation
