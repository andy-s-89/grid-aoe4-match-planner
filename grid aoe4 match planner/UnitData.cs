using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public class IntOrDoubleConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return 0; // or use another default value if needed
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out int intValue))
                {
                    return intValue;
                }
                else if (reader.TryGetDouble(out double doubleValue))
                {
                    return (int)doubleValue;
                }
            }

            throw new JsonException($"Unexpected token {reader.TokenType} when parsing integer.");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class DoubleOrIntConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return 0.0; // or use another default value if needed
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetDouble(out double doubleValue))
                {
                    return doubleValue;
                }
                else if (reader.TryGetInt32(out int intValue))
                {
                    return intValue;
                }
            }

            throw new JsonException($"Unexpected token {reader.TokenType} when parsing double.");
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class UnitWeaponModifier
    {
        public string property { get; set; }
        public Target target { get; set; }
        public string effect { get; set; }
        public int value { get; set; }
        public string type { get; set; }
    }

    public class Target
    {
        public List<List<string>> @class { get; set; }
    }

    public class UnitWeaponRange
    {
        public double min { get; set; }
        public double max { get; set; }
    }

    public class UnitWeaponDurations
    {
        public double aim { get; set; }
        public double windup { get; set; }
        public double attack { get; set; }
        public double winddown { get; set; }
        public double reload { get; set; }
        public double setup { get; set; }
        public double teardown { get; set; }
        public double cooldown { get; set; }
    }

    public class UnitWeaponBurst
    {
        [JsonConverter(typeof(IntOrDoubleConverter))]
        public int count { get; set; }
    }

    public class UnitWeapon
    {
        public string name { get; set; }
        public string type { get; set; }
        public int damage { get; set; }
        public double speed { get; set; }
        public UnitWeaponRange range { get; set; }
        public List<UnitWeaponModifier> modifiers { get; set; }
        public UnitWeaponDurations durations { get; set; }
        public UnitWeaponBurst burst { get; set; }
        public string attribName { get; set; }
        public int pbgid { get; set; }
    }

    public class UnitArmor
    {
        public string type { get; set; }
        public int value { get; set; }
    }

    public class UnitSight
    {
        public int line { get; set; }
        public int height { get; set; }
    }

    public class UnitMovement
    {
        [JsonConverter(typeof(DoubleOrIntConverter))]
        public double speed { get; set; }
    }

    public class UnitData
    {
        public string id { get; set; }
        public string baseId { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public int pbgid { get; set; }
        public string attribName { get; set; }
        public int age { get; set; }
        public List<string> civs { get; set; }
        public string description { get; set; }
        public List<string> classes { get; set; }
        public List<string> displayClasses { get; set; }
        public bool unique { get; set; }
        public UnitCost costs { get; set; }
        public List<string> producedBy { get; set; }
        public string icon { get; set; }
        public int hitpoints { get; set; }
        public List<UnitWeapon> weapons { get; set; }
        public List<UnitArmor> armor { get; set; }
        public UnitSight sight { get; set; }
        public UnitMovement movement { get; set; }
    }

    public class UnitCost
    {
        public int food { get; set; }
        public int wood { get; set; }
        public int stone { get; set; }
        public int gold { get; set; }
        public int vizier { get; set; }
        public int oliveoil { get; set; }
        public int total { get; set; }
        public int popcap { get; set; }

        [JsonConverter(typeof(IntOrDoubleConverter))]
        public int time { get; set; }
    }

    public class CivUnitData
    {
        public List<UnitData> data { get; set; }
    }

}
