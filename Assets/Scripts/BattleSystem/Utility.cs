using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utility
{
    public static List<T> EnumValues<T>()
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }

    public static string[] EnumStrings<T>(int limit = -1)
    {
        var enumTypes = EnumValues<T>();
        var count = limit > 0 ? limit : enumTypes.Count();
        var types = new string[count];

        for (int i = 0; i < count; i++)
        {
            types[i] = enumTypes[i].ToString();
        }
        return types;
    }

    public static List<T> CopyList<T>(List<T> list)
    {
        var copy = new List<T>();
        foreach (var item in list)
        {
            copy.Add(item);
        }
        return copy;
    }

    public static List<T> CopyLists<T>(List<List<T>> lists)
    {
        var copy = new List<T>();
        foreach (var list in lists)
        {
            foreach (var item in list)
            {
                copy.Add(item);
            }
        }
        return copy;
    }

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
        var radii = entity1.EntityData.Radius + entity2.EntityData.Radius;
        var distance = Vector3.SqrMagnitude(entity1.transform.position - entity2.transform.position) - radii * radii;

        return distance;
    }

    public static float Distance2D(Vector2 pos, Entity entity)
    {
        var radius = entity.EntityData.Radius;
        var entityPos = Get2DVector(entity.transform.position);
        var distance = Vector2.SqrMagnitude(pos - entityPos) - radius * radius;

        return distance;
    }

    public static float Distance3D(Vector3 pos, Entity entity)
    {
        var radius = entity.EntityData.Radius;
        var entityPos = entity.transform.position;
        var distance = Vector3.SqrMagnitude(pos - entityPos) - radius * radius;

        return distance;
    }

    public static float DistanceXZ(Vector3 pos1, Vector3 pos2)
    {
        return Vector2.SqrMagnitude(Get2DVector(pos1) - Get2DVector(pos1));
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
