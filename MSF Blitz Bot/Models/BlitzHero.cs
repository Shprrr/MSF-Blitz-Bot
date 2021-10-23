using System.Diagnostics.CodeAnalysis;

namespace MSFBlitzBot
{
    public struct BlitzHero
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public int? Power { get; private set; }
        public string PowerString { get; private set; }
        public float Accuracy { get; }

        public BlitzHero(string id, string power) : this()
        {
            Id = id;
            Name = HeroManager.GetName(id);
            if (int.TryParse(power, out var powerInt))
                Power = powerInt;
            PowerString = power;
            Accuracy = 1;
        }

        [Newtonsoft.Json.JsonConstructor]
        [SuppressMessage("Style", "IDE0060:Supprimer le paramètre inutilisé", Justification = "Calculated values")]
        public BlitzHero(string id, string name, int? power, string powerString, float accuracy) : this(id, powerString, accuracy)
        {
        }

        public BlitzHero(string id, string power, float accuracy) : this(id, power) => Accuracy = accuracy;
    }
}
