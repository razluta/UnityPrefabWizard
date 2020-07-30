# UnityPrefabWizard [![License](https://img.shields.io/badge/License-MIT-lightgrey.svg?style=flat)](http://mit-license.org)
This is a set of Unity utility functions for automatically creating prefabs for 3D assets (with shader assignment, material creation and texture assignments), nicely wrapped in a friendly GUI.

Verified on the following versions of Unity:
- 2019.4
- 2019.3

![](/Screenshots/UnityPrefabWizard.png)


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
## Using the public API
With the tools provided in this repository ...

*  *  *  *  *

## Using the provided GUI
### Step 001
Launch the _**Prefab Wizard**_ from the top menu bar under **Art Tools > Prefab Wizard**.

### Step 002
#### Option A - Create some new reules
#### Option B - Load some previously created rules
Press the **Load Rules** button. /
![](/Screenshots/UnityPrefabWizard_LoadRules.png)
Navigate to the location of the previously saved rule file .JSON and load it. /
The rule area will get automatically populated with the valid rules from the provided file. /
![](/Screenshots/UnityPrefabWizard_LoadedRules.png)
At this point, you can still edit the rules: in the example below, the tool will create a material for the mesh, give it a ShaderGraph as a shader, rename the material to be the mesh name + the suffix **__Mat** and finally match up all the textures that match the names (mesh name + the listed texture suffixes) with the right shader inputs.
 ![](/Screenshots/UnityPrefabWizard_EditRules.png)

### Step 003
Edit the rules as necessary.
Example

### Step 004 
Run the prefab creation

*  *  *  *  *
## Architecture
Below is a high level explanation of how the tools are architected.

The primary components of the tools are:
- A class called **PrefabWizardEditor** which is the primary entry point into the tool.
- A class called 
