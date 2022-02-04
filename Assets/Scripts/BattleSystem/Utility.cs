using UnityEngine;

public static class Utility
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

    public static float Angle(Vector2 direction)
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

    public static Vector3 ApplyDirection(Vector3 point, Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.001f)
        {
            return Quaternion.LookRotation(direction) * point;
        }

        return point;
    }
}
