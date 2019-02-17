# Design decisions

- Algorithms should not modify the presentation directly
- "Add" and "Remove" algorithms generate delta masks which contain the delta from currently
  visible graph to the desired result.
- "Add" algorithms only consider nodes not being visible.
- "Remove" algorithms only consider nodes being visible.
- "Show" algorithms generate complete masks which contain all desired nodes of the currently
  visible graph. Hide all masks are needed to see effect on the graph.
