# Design decisions

- Algorithms do not modify the presentation directly
- "Add" and "Remove" algorithms generate delta masks which contain the delta from currently
  visible graph to the desired result.
- "Add" algorithms only consider nodes not being visible.
- "Remove" algorithms only consider nodes being visible.
- In order to generate "Show" masks visualizing a certain sub graph an algorithm has 
  two options:
  - generate two masks: one "hide mask" containing all visible nodes and
    a "show mask" containing the desired nodes
  - generate an inverted "hide mask" containing the desired nodes
