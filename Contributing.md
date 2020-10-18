# Setting up Dev Environment

1. Install Visual Studio Community Edition 2019
   1. Include .Net Desktop Development
2. Open the solution `XSDDiagram2012.sln`

# Common issues

## Error while trying to open project properties

While trying to open the properties of each project, the following error occurs:
```
An error occurred trying to load the project properties window.  Close the window and try again.
Value cannot be null.
Parameter name: val
```

The problem is described here:
https://developercommunity.visualstudio.com/content/problem/919366/an-error-occurred-trying-to-load-the-project-prope.html

The solution is to modify the Visual Studio Installation to include the .Net Desktop Development stuff.
