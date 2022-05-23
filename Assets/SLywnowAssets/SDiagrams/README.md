
# SLywnow Block Diagrams

This asset will allow you to create block diagrams that you can use in the game. They can be useful for a level editor, for puzzles, for a detective investigation (to connect photos together) or for decorations.



## Installation

For the asset to work , it is required:

[SLywnow Basic](https://github.com/SLywnow/slywnow_basic)\
[UI Extensions](https://bitbucket.org/UnityUIExtensions/unity-ui-extensions/downloads/)


## Features

- Create and Delete Blocks of diagrams with different types
- Create connections between blocks (note that input and output connections differ from each other)
- Report information about the block: its position, connections, block id and type


## How to use
### Setup scene

1) Add the Sdia_Main component to any object that will not be disabled
2) Setup SDia_main:
    - Line Object - an object for drawing links between blocks, put ANY object with UILineConnector here
    - Ghost Object - any empty object with a RectTransform
    - Main Parent - the parent object on which the blocks appear
    - Background Parent - The object on which the connecting lines appear, it should be on the layer below the Main Parent
    - Spawn On Center - a new block will appear in the center of the screen
    - Default Spawn Point - the point of appearance of a new block by default, only if Spawn On Center = false
    - Hide Blocks Outside Screen - automatically disables blocks behind the screen area, use for optimization if required
    - Types - types of block that you can spawn
        - Name - the name of the type that will be used to create new blocks
        - Block - the block object that will appear when creating a new block

3) Create blocks, just add Sdia_Block to any RectTransform with CanvasGroup
    - DragObject - RectTransform with the SDia_DragObject component, define a zone that dragging it will change position of the block 
    - Enters - points for connecting to a block from another block
    - Exits - exit points from the block through which the block can connect to other blocks (You cannot use one Joint as enter and exit)
    - Block Objects - the objects that are contained inside the block, specify them to be able to get them through the GetObject method
        - Name - the name of the GameObject for GetObject
        - Obj - GameObject
4) Add all blocks to types in Main and disable it or create presets with it
5) Spawn something by calling SpawnNew(type_name) in SDia_Main

Joints:
- Connection Count - how many connections can a given Joint contain (regardless of its type)
- onConnection - unity Event, which is passed parameters: int id of connected block, int id of connected joint; when you connected this joint somewhere
- onDisconnection - unity Event, which is passed parameters: int id of disconnected block, int id of disconnected joint; when you disconnected this joint

### Usage

To create a new block, you just need to call the Spawn function in Sdia_Main, you can, for example, create a button for this.\
To connect 2 blocks, click on Exit Joint on the block, then click on Enter Joint on the other block. After that they will be connected\
You can get connection data by reading connections in Sdia_Joint, they are written in the format:
```
"id of block" "list id of joint"
```
example
```
1232 1
```
thats mean this joint connected to block with 1232 id and to joint in position 1 in list of joints. You can understand what list you need to read by remember rule: enter joints connected only to exit joints, and vice versa\
Or you can simply call the getconnectionpoint(int id) function to immediately get the Joint with which the connection is established. To get a block from Joint, just read the main variable inside joint

To remove connection select 2 joints again

### Working with main
You can spawn block by 3 functions:
- SpawnNew(string type) - spawn new block and position by default settings
- SpawnNew(string type, Vector2 position) - spawn new block in some position
- Spawn(string type, Vector2 position, int id) - spawn block with some id and position, use it for loading

### Working with blocks
To create new connection inside your code call SetConnection(bool enter, int jointId, int connectionId, int connectionJointId), where 
- enter - is this joint enter joint?
- jointId - id of joint in it's list
- connectionId - id of block witch you want to connect this joint
- connectionJointId - id of joint in block witch you want to connect this joint

To delete block use Delete() function in Block, you can add it to Button inside block

To get some object in blockObjects list use getObject(string name) or getObjectComponent<Type>(string name) to get some component inside object

### Working with joints

You can remove connection by RemoveConnection(int id)\
Or you can remove all connections by RemoveAllConnections()

### Saving and loading

You can write the blocks SDia_Main component to JSON to save the id, also create a method to get the data you need, for example, the position of the block or the state of the block objects\
When loading, just call the Spawn method(string type, Vector2 position, int id) to recreate the blocks

