# Technical Documentation

We plan for our game to use Unity Editor and its included technologies. In particular:

We intend to use the component-based paradigm of Unity scripts to create complex objects by attaching separate scripts of which each allows simple behaviour, like:
* Player-controlled movement
* Camera movement
* Possession mechanic in ghost form
* Action for each possessable object

Additionally, we have determined that the levels will be created within Unity (as opposed to, say, an entire shelf being modeled in Maya, which was our first approach). This will require basic components, like a single wall or floor or shelf board, to exist as reusable models which can be assembled into levels within the Editor.

We do intend to use Visual Studio for our coding. As such, we will take advantage of comments with triple slashes to create documentation of that sort which Visual Studio displays during code input. This will apply not only to methods, but also to attributes with non-obvious names (not `Vector3 velocity`, but perhaps `bool selectingObject`).

## Script architecture

We have come up with the following script classes:
* A Player script which handles inputs (dependent on the following).
* An abstract Movement class, which includes movement code, including translating camera-space vectors into world-space vectors. The following subclasses exist for it.
  * A GhostMovement script which handles 3-D movement of the ghost character.
  * At present, a DefaultObjectMovement script, which moves possessed objects using Rigidbodies.
* An IPossessable interface. This is currently implemented by a single behavior, ColliderBasedPossession, which handles changes in possession state.
* An IActionable interface. This is currently implemented by a single behavior, DefaultAction, which causes a possessed object to jump.
