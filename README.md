!!! THIS IS A WORK IN PROGRESS AND IT IS NOT CURRENTLY FUNCTIONAL !!!

# UnityPrefabWizard [![License](https://img.shields.io/badge/License-MIT-lightgrey.svg?style=flat)](http://mit-license.org)
A set of Unity utility functions for automatically creating prefabs for 3D assets, nicely wrapped in a friendly GUI.

## Setup
##### Option A) Clone or download the repository and drop it in your Unity project.
##### Option B) Add the repository to the package manifest (go in YourProject/Packages/ and open the "manifest.json" file and add "com..." line in the depenencies section). If you don't have Git installed, Unity will require you to install it.
```
{
  "dependencies": {
      ...
      "com.razluta.unityprefabwizard": "https://github.com/razluta/UnityPrefabWizard.git"
      ...
  }
}
```
*  *  *  *  *
## Architecture
Below is a high level explanation of how to tools are architected.

The primary components of the tools are:
- test
