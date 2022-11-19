
# Code Perspective

Code Perspective is a tool for looking inside any .Net applications and watching it run in real-time. Getting started with the source code is easy. Clone the repo, open the solution, and start debug. The XBuilder should come right up.

### Directories

* Compiler - Code for XBuilder.exe, this is responsible for doing static analysis on .Net assemblies and recompiling them with hooks that output analysis information to XLibrary.dll.

* Library - Code for XLibrary.dll, this is what's attached to the recompiled assembly and contains code for making sense of the data the target assembly sends it during run-time. All the viewer and rendering code is here.

* XTestApp - A simple application for testing cases XBuilder needs to be able to recompile.

Checkout http://www.codeperspective.com for more resources.