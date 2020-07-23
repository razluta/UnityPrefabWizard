!!! THIS IS A WORK IN PROGRESS AND IT IS NOT CURRENTLY FUNCTIONAL !!!

# UnityPrefabWizard [![License](https://img.shields.io/badge/License-MIT-lightgrey.svg?style=flat)](http://mit-license.org)
This is a set of Unity utility functions for automatically creating prefabs for 3D assets (with materials and texture assignments), nicely wrapped in a friendly GUI.

Verified on the following versions of Unity:
- 2019.4
- 2019.3

![](https://github.com/razluta/UnityPrefabWizard/blob/master/Screenshots/UnityPrefabWizard.png)


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
Below is a high level explanation of how the tools are architected.

The primary components of the tools are:
- test
