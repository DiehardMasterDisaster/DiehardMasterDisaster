using RWCustom;
using UnityEngine;

namespace DiehardMasterDisaster.GunStuff;

public static class PhysicsExtension
{
    public static SharedPhysics.TerrainCollisionData HorizontalCollision(this SharedPhysics.TerrainCollisionData self, Room room) => SharedPhysics.HorizontalCollision(room, self);
 
    public static SharedPhysics.TerrainCollisionData VerticalCollision(this SharedPhysics.TerrainCollisionData self, Room room) => SharedPhysics.VerticalCollision(room, self);

    public static SharedPhysics.TerrainCollisionData SlopesVertically(this SharedPhysics.TerrainCollisionData self, Room room) => SharedPhysics.SlopesVertically(room, self);
    
    public static SharedPhysics.TerrainCollisionData TerrainCollision(this SharedPhysics.TerrainCollisionData self, Room room) => self.VerticalCollision(room).HorizontalCollision(room).SlopesVertically(room);
    
    public static SharedPhysics.TerrainCollisionData TerrainCollision(this SharedPhysics.TerrainCollisionData self, Room room, Vector2 pos, Vector2 lastPos, Vector2 vel, float rad, bool goThroughFloors = false) => self.Set(pos, lastPos, vel, rad, new IntVector2(0, 0), goThroughFloors).VerticalCollision(room).HorizontalCollision(room).SlopesVertically(room);
}