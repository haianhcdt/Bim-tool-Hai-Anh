using Autodesk.Revit.DB;
namespace DSCons.Revit.Starter.Infrastructure;
public static class ElementIdCompatibility
{
    public static long AsLong(this ElementId id){
#if REVIT2019 || REVIT2020 || REVIT2021 || REVIT2022 || REVIT2023
        return id.IntegerValue;
#else
        return id.Value;
#endif
    }
    public static ElementId Create(long value){
#if REVIT2019 || REVIT2020 || REVIT2021 || REVIT2022 || REVIT2023
        return new ElementId(checked((int)value));
#else
        return new ElementId(value);
#endif
    }
}
