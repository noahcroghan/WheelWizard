using System;
using System.Collections.Generic;
using System.Linq;
using WheelWizard.Models.Enums;
using WheelWizard.Views.Components.WhWzLibrary;

namespace WheelWizard.Services;

public class BadgeManager
{
    public static BadgeManager Instance { get; } = new();
    private BadgeManager() { }
    public void LoadBadges()
    {
   
    }

    // private static readonly string[] _firstPlaces = new[]
    // {
    //     "4343-3434-3434",
    //     "2277-7727-2227"
    // };

    // private static readonly string[] _secondPlaces = new[]
    // {
    //     "1251-5622-1012",
    //     "0000-0202-1121"
    // };

    // private static readonly string[] _thirdPlaces = new[]
    // {
    //     "3955-9063-2091",
    //     "4988-1656-7319"
    // };
    public BadgeVariant[] GetRandomBadgeVariants(int? seed = null)
    {
        var random = seed == null ? new Random() : new Random(seed.Value);
        var allVariants = Enum.GetValues(typeof(BadgeVariant)) as BadgeVariant[];
        var numberOfBadges = random.Next(0, 5);

        var selectedVariants = new List<BadgeVariant>();
        for (var i = 0; i < numberOfBadges; i++)
        {
            if (allVariants == null) continue;
            var randomIndex = random.Next(allVariants.Length);
            selectedVariants.Add(allVariants[randomIndex]);
        }

        return selectedVariants.ToArray();
    }
    
    public BadgeVariant[] GetBadgeVariants(string friendCode)
    {
        return GetRandomBadgeVariants();
    }

    public IEnumerable<Badge> GetBadges(string friendCode) => GetBadges(GetBadgeVariants(friendCode));
    public IEnumerable<Badge> GetBadges(BadgeVariant[] badges) => badges.Select(variant => new Badge { Variant = variant });
}
