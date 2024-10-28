using CsvHelper.Configuration;
using CoolMangoes.Models;

namespace CoolMangoes.Modules
{
    public class ClassDataMap : ClassMap<ClassData>
    {
        public ClassDataMap()
        {
            Map(m => m.AssetHierarchy).Optional();
            Map(m => m.HierarchyCode).Optional();
            Map(m => m.AssetType).Optional();
            Map(m => m.MaintenanceType).Optional();
            Map(m => m.Statutory).Optional();
            Map(m => m.EstimatedLife).TypeConverter<NullableFloatConverter>().Optional();
            Map(m => m.RefurbishmentFrequency).TypeConverter<NullableFloatConverter>().Optional();
            Map(m => m.RefurbishmentCostAsProportionOfReplacementCost).TypeConverter<NullableFloatConverter>().Optional();
            Map(m => m.MinCost).TypeConverter<NullableFloatConverter>().Optional();
            Map(m => m.MaxCost).TypeConverter<NullableFloatConverter>().Optional();
            Map(m => m.AvgReplacementCost).TypeConverter<NullableFloatConverter>().Optional();
        }
    }
}
