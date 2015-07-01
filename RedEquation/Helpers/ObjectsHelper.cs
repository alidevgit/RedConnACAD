using System.Collections.Generic;
using RedEquation.Classes.Enums;

namespace RedEquation.Helpers
{
    public static class ObjectsHelper
    {
        //Dictionary with defining allowable types for all Object Type (Key - ParentObjectType, Value - Types that can be stored in)
        private static readonly Dictionary<ObjectType, List<ObjectType>> AllowableObjectTypeDictionary = new Dictionary<ObjectType, List<ObjectType>>()
        {
            {ObjectType.Project, new List<ObjectType>(){ObjectType.Unit, ObjectType.Material, ObjectType.Alignment, ObjectType.Group, ObjectType.Point, ObjectType.Surface, ObjectType.Circle, 
                                                        ObjectType.Volume, ObjectType.Line, ObjectType.Section, ObjectType.RebarCircularLayout, ObjectType.RebarProfileDefinition, 
                                                        ObjectType.RebarLineLayout, ObjectType.DesignCode, ObjectType.DesignCheck, ObjectType.DesignRun}}, 
            {ObjectType.Unit, new List<ObjectType>(){ObjectType.Group}}, 
            {ObjectType.Material, new List<ObjectType>(){ObjectType.Section, ObjectType.Shape}}, 
            {ObjectType.Alignment, new List<ObjectType>(){ObjectType.Straight, ObjectType.Circular, ObjectType.Spiral, ObjectType.ElevationPoint, ObjectType.CrossSection, ObjectType.CrossSectionSegment}}, 
            {ObjectType.Group, new List<ObjectType>(){ObjectType.Unit, ObjectType.Material, ObjectType.Alignment, ObjectType.Group, ObjectType.Point, ObjectType.Surface, ObjectType.Circle, 
                                                      ObjectType.Volume, ObjectType.Line, ObjectType.Section, ObjectType.Shape, ObjectType.RebarCircularLayout, ObjectType.RebarProfileDefinition, 
                                                      ObjectType.RebarLineLayout, ObjectType.TendonLayout, ObjectType.Repeat, ObjectType.Circular, ObjectType.Spiral, ObjectType.Straight,
                                                      ObjectType.CrossSection, ObjectType.CrossSectionSegment, ObjectType.ElevationPoint}}, 
            {ObjectType.Point, null}, 
            {ObjectType.Surface, new List<ObjectType>(){ObjectType.Point, ObjectType.Surface, ObjectType.Circle, ObjectType.Group, ObjectType.Repeat}},
            {ObjectType.Volume, new List<ObjectType>(){ObjectType.Surface, ObjectType.Circle, ObjectType.Repeat}},
            {ObjectType.Line, new List<ObjectType>(){ObjectType.Point, ObjectType.Surface, ObjectType.Circle, ObjectType.Section, ObjectType.Shape, ObjectType.RebarLineLayout, 
                                                     ObjectType.RebarCircularLayout, ObjectType.TendonLayout, ObjectType.Group, ObjectType.Repeat}},
            {ObjectType.Circle, new List<ObjectType>(){ObjectType.Point, ObjectType.Surface, ObjectType.Circle, ObjectType.Shape, ObjectType.Group, ObjectType.Repeat}},
            {ObjectType.Section, new List<ObjectType>(){ObjectType.Shape, ObjectType.Circle, ObjectType.RebarLineLayout, ObjectType.RebarCircularLayout, ObjectType.TendonLayout, ObjectType.Group, ObjectType.Repeat}},
            {ObjectType.Shape, new List<ObjectType>(){ObjectType.Point, ObjectType.Circle, ObjectType.Shape, ObjectType.Group, ObjectType.Repeat}},
            {ObjectType.RebarProfileDefinition, null},
            {ObjectType.RebarLineLayout, new List<ObjectType>(){ObjectType.Point, ObjectType.Group, ObjectType.Repeat}},
            {ObjectType.RebarCircularLayout, new List<ObjectType>(){ObjectType.Point, ObjectType.Group, ObjectType.Repeat}},
            {ObjectType.TendonLayout, new List<ObjectType>(){ObjectType.Point, ObjectType.Group, ObjectType.Repeat}},
            {ObjectType.Repeat, new List<ObjectType>(){ObjectType.Surface, ObjectType.Circle, ObjectType.Point, ObjectType.Volume, ObjectType.Group, ObjectType.Repeat}},
            {ObjectType.DesignCode, new List<ObjectType>(){ObjectType.Group, ObjectType.DesignCheck}},
            {ObjectType.DesignCheck, null},
            {ObjectType.DesignRun, null}
        };

        internal static List<ObjectType> GetAllowedTypes(ObjectType parentObjectType)
        {
            return !AllowableObjectTypeDictionary.ContainsKey(parentObjectType) ? null : AllowableObjectTypeDictionary[parentObjectType];
        }
    }
}
