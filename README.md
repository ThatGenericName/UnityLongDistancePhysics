# UnityLongDistancePhysics
Creating workarounds for floating point precision problems in Unity's Physics Engine.

# Motivation

## Floating Point Precision

Unity's base physics engine uses PhysX, which is limited to single floating point precision values (floats). Similarly, the rest of Unity also uses floats.

In it's base form, this means that practically, any object is limited to at most 10km from the origin (0, 0, 0), even less if an object comprises of multiple interacting component as opposed to behaving as a single rigidbody.

When approaching this limit, objects will begin to jitter. Move further and the verticies that represent the 3d model itself will begin to jitter too. At the same time, physics calculations begins to have some more weird issues related to this jittering.

# Possible Existing Solution(s)

Below are some possible existing solutions to this problem.

## Floating Origin

One solution to this issue is a floating origin. Objects in the scene is continually shifted such that the player object, or the Camera itself will be always within a certain distance from the origin itself. 

The same can be performed with the velocities within a certain distance from the origin such that the velocity of any one object has a lower magnitude to reduce floating point precision problems in the physics.

However this doesn't solve the problem for objects located at a distance from the object that the origin is being centered to. Games like KSP and SimpleRockets use this method, and objects beyond the 10km limit is instead simulated using orbital mechanics models and cannot have things like actively firing thrusters, etc. 

However, what if the game is some kind of combat sim? Modern warefare can often take place across great distances. Common long ranged air to air missiles are often employed in excess of 20km and can have a maximum range of over 100km. These objects are not entirely predictable 

## Clustered Physics.

Here's another solution, Clustered Physics. Objects are grouped into clusters where the dimensions of the cluster is kept within the physical size limit.

For example, in Space Engineers ([Old source code](https://github.com/KeenSoftwareHouse/SpaceEngineers/tree/master), [Old Blog Post](https://blog.marekrosa.org/2014/12/space-engineers-super-large-worlds_17.html)), objects are placed into dynamically sized clusters. These clusters resize to ensure objects are never less than 2km from the edge of the cluster. That being said from my understanding, these clusters are static and the game has a base velocity limit of 100m/s. Mods can increase that limit but it's there for a reason.

Star Citizen has a similar approach, a part of their "Synchronized Local Physics Grids", however these clusters are to my knowledge centered on a ship. For a client this does not matter, afterall the simulations that a client will be concerned with will only be what the player can see. The backend implementation, what is being done on their server is what is the main part.

# My Planned Solution.

I will be taking a clustered approach similar to star citizen. I can use Unity's PhysicsScene to act as a cluster, and then implementing floating origin within those clusters.

## Challenges

### Larger Value Transforms

A custom numerical type needs to be created to encompass the distance that need to be modelled,

### Floating Origin

Each cluster will have a floating origin with a focus object.

### Clustering Objects

Objects are clustered in an Object Aligned manner. If 2 clusters overlap, objects sitting between the centerpoints of the 2 clusters will be duplicated.

### Synchronization Between Clusters.

As mentioned under Clustering Objects, objects will be duplicated. If an event causes an object to change directions, such as a collision imparting a force, the objects must be synchronized.

### Physics Wrapping and Handling
Common physics tools in Unity needs to have custom solutions, or additional processing to work.

For example, performing a raycast which may need to span across multiple clusters. 
