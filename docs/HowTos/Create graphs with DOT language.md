---
title: Create graphs with DOT language
section: How to ...
navigation_weight: 100
---

# Create graphs with DOT language

An easy way to create graphs is to use the DOT language as described [here](http://www.graphviz.org/doc/info/lang.html).

## Cheat Sheet

A basic graph in DOT language looks like this:

```
digraph {
  Builder -> POCO1 
  Builder -> POCO2 
  Component -> IComponent 
  Context -> IComponent 
}
```

You could just describe the edges if all nodes have at least one edge using the "->".
Nodes without edges are just mentioned in a separate line.

*Note:* If you open a DOT file in Plainion.GraphViz and then continue editing it the visualization in 
Plainion.GraphViz will adopt automatically whenever you save the file.

# Hands-on

<iframe width="560" height="315" src="https://www.youtube.com/embed/jMi1I6Pd_9M" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>
