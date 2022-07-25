using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class VectorUtility
{
    public static Vector2 Get2DVector(Vector3 position)
    {
        return new Vector2(position.x, position.z);
    }

    public static Vector3 Rotate(Vector3 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float x = v.x;
        float z = v.z;
        v.x = (cos * x) - (sin * z);
        v.z = (sin * x) + (cos * z);
        return v;
    }

    public static float Angle2D(Vector2 direction)
    {
        if (direction.x < 0)
        {
            return 360.0f - (Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg * -1.0f);
        }
        else
        {
            return Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        }
    }

    public static float Angle3D(Vector3 direction)
    {
        if (direction.x < 0)
        {
            return 360.0f - (Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg * -1.0f);
        }
        else
        {
            return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        }
    }

    public static float Angle(Vector2 forward, Vector2 direction)
    {
        return Mathf.Acos(Vector2.Dot(direction, forward)) * 57.2958f;
    }

    public static float Distance2D(Entity entity1, Entity entity2)
    {
        if (entity1 == null || entity2 == null)
        {
            return 0.0f;
        }

        var radii = entity1.EntityData.Radius + entity2.EntityData.Radius;
        var distance = Vector3.SqrMagnitude(entity1.Position - entity2.Position) - radii * radii;

        return distance;
    }

    public static float Distance2D(Vector2 pos, Entity entity)
    {
        var radius = entity.EntityData.Radius;
        var entityPos = Get2DVector(entity.Origin);
        var distance = Vector2.SqrMagnitude(pos - entityPos) - radius * radius;

        return distance;
    }

    public static float Distance3D(Vector3 pos, Entity entity)
    {
        var radius = entity.EntityData.Radius;
        var entityPos = entity.Position;
        var distance = Vector3.SqrMagnitude(pos - entityPos) - radius * radius;

        return distance;
    }

    public static float DistanceXZ(Vector3 pos1, Vector3 pos2)
    {
        return Vector2.SqrMagnitude(Get2DVector(pos1) - Get2DVector(pos2));
    }

    public static bool IsInRange(Entity entity1, Entity entity2, float range)
    {
        var distance = Distance2D(entity1, entity2);

        return distance <= range * range;
    }

    public static Vector2 RotateAroundPosition(Vector2 point, float angle, Vector2 centerOfRotation)
    {
        var sin = Mathf.Sin(angle);
        var cos = Mathf.Cos(angle);
        var temp = new Vector2();

        point -= centerOfRotation;
        temp.x = point.x * cos - point.y * sin;
        temp.y = point.x * sin + point.y * cos;
        point = temp + centerOfRotation;

        return point;
    }

    public static Vector3 OrbitPosition(float yOffset, Vector3 anchorPos, float distance, float angle)
    {
        var x = distance * Mathf.Cos(angle * Mathf.Deg2Rad);
        var z = distance * Mathf.Sin(angle * Mathf.Deg2Rad);

        var position = anchorPos;
        position.x += x;
        position.y += yOffset;
        position.z += z;

        return position;
    }

    public static Vector3 ApplyDirection(Vector3 point, Vector3 direction)
    {
        if (direction.sqrMagnitude > Constants.Epsilon)
        {
            return Quaternion.LookRotation(direction) * point;
        }

        return point;
    }
}
