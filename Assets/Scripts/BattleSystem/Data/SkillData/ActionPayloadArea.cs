using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPayloadArea : ActionPayload
{
    public class Area
    {
        public enum eShape
        {
            Cone,
            Rectangle
        }

        public eShape Shape;
        public Vector2 Dimensions;              // Rectangle length/width or cone radius/width (can be a circle)
        public Vector2 InnerDimensions;         // Can make a smaller shape to create a cutout (for example a donut)

        public PositionData AreaPosition;
        public float Rotation;

        // To do: add a way to define falloff ratios
    }

    public List<Area> AreasAffected;

    public override bool NeedsTarget()
    {
        if (Target == eTarget.EnemyEntities)
        {
            foreach (var area in AreasAffected)
            {
                if (area.AreaPosition.PositionOrigin == PositionData.ePositionOrigin.SelectedTargetPosition)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
